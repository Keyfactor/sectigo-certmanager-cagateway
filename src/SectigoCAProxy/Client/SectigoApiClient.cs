// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.
using CSS.Common.Logging;

using Keyfactor.AnyGateway.Sectigo.API;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Keyfactor.AnyGateway.Sectigo.Client
{
	public class SectigoApiClient : LoggingClientBase
	{
		private HttpClient RestClient { get; }

		public SectigoApiClient(HttpClient httpClient)
		{
			RestClient = httpClient;
		}

		public async Task<Certificate> GetCertificate(int sslId)
		{
			var response = await RestClient.GetAsync($"api/ssl/v1/{sslId}");
			return await ProcessResponse<Certificate>(response);
		}

		public async Task CertificateListProducer(BlockingCollection<Certificate> certs,
						CancellationToken cancelToken, int pageSize = 25, string filter = "")
		{
			int batchCount;
			int blockedCount;
			int totalCount = 0;

			List<Certificate> certificatePageToProcess;
			try
			{
				//Paging loop will iterate though the certificates until all certificates have been returned
				do
				{
					if (cancelToken.IsCancellationRequested)
					{
						certs.CompleteAdding();
						break;
					}

					int certIndex = totalCount > 0 ? (totalCount - 1) : 0;
					Logger.Info($"Request Certificates at Position {certIndex} with Page Size {pageSize}");
					certificatePageToProcess = await PageCertificates(certIndex, pageSize, filter);
					Logger.Debug($"Found {certificatePageToProcess.Count} certificate to process");

					//Processing Loop will add and retry adding to queue until all certificates have been processed for a page
					batchCount = 0;
					blockedCount = 0;
					do
					{
						Certificate cert = certificatePageToProcess[batchCount];
						Logger.Debug($"Processing: {cert}");
						Certificate certDetails = null;
						try
						{
							if (certDetails == null)
								certDetails = await GetCertificate(cert.Id);
						}
						catch (SectigoApiException aEx)
						{
							Logger.Error($"Error requesting certificate details. Skipping certificate. {aEx.Message}");
							batchCount++;
							continue;
						}

						if (certs.TryAdd(certDetails, 50, cancelToken))
						{
							batchCount++;
							totalCount++;
						}
						else
						{
							Logger.Trace($"Adding {cert.Id} to queue was blocked. Retry");
							blockedCount++;//TODO: If blocked count is excessive, should we skip?
						}
						certIndex++;
					}
					while (batchCount < certificatePageToProcess.Count);

					Logger.Info($"Added {batchCount} certificates to queue for processing.");
				} while (certificatePageToProcess.Count == pageSize);//if the API returns less than we requested, we assume we have reached the end
			}
			catch (HttpRequestException hEx)
			{
				Logger.Error($"Sync interrupted by HTTP Exception. {hEx.InnerException.Message}");
				certs.CompleteAdding();//Stops the consuming enumerable and sync will continue until the queue is empty
			}
			catch (Exception ex)
			{
				//fail gracefully and stop syncing.
				Logger.Error($"Sync interrupted by General Exception. {ex.Message}");
				certs.CompleteAdding();//Stops the consuming enumerable and sync will continue until the queue is empty
			}
		}

		public async Task CertificateListProducer(BlockingCollection<Certificate> certs,
						CancellationToken cancelToken, int pageSize = 25, Dictionary<string, string[]> filter = null)
		{
			if (filter != null && filter.Count > 0)
			{
				//each kvp key = type, value each filter
				foreach (var s in filter)
				{
					foreach (var value in s.Value)
						await CertificateListProducer(certs, cancelToken, pageSize, $"{s.Key}={value}");
				}
			}
			else
			{
				// No filters
				await CertificateListProducer(certs, cancelToken, pageSize, "");
			}

			certs.CompleteAdding();
		}

		public async Task<List<Certificate>> PageCertificates(int position = 0, int size = 25, string filter = "")
		{
			string filterQueryString = String.IsNullOrEmpty(filter) ? string.Empty : $"&{filter}";
			Logger.Trace($"API Request: api/ssl/v1?position={position}&size={size}{filterQueryString}".TrimEnd());
			var response = await RestClient.GetAsync($"api/ssl/v1?position={position}&size={size}{filterQueryString}".TrimEnd());
			return await ProcessResponse<List<Certificate>>(response);
		}

		public async Task<bool> RevokeSslCertificateById(int sslId, string revreason)
		{
			JObject o = JObject.FromObject(new
			{
				reason = revreason
			});
			var response = await RestClient.PostAsJsonAsync($"api/ssl/v1/revoke/{sslId}", o);
			if (response.IsSuccessStatusCode)
			{
				return true;
			}
			var failedResp = ProcessResponse<RevocationResponse>(response).Result;
			return failedResp.IsSuccess;//Should throw an exception with error message from API
		}

		public async Task<ListOrganizationsResponse> ListOrganizations()
		{
			var response = await RestClient.GetAsync("api/organization/v1");
			if (response.IsSuccessStatusCode)
			{
				string responseContent = await response.Content.ReadAsStringAsync();
				Logger.Trace($"Raw Response: {responseContent}");
			}
			var orgsResponse = await ProcessResponse<List<Organization>>(response);

			return new ListOrganizationsResponse { Organizations = orgsResponse };
		}

		public async Task<OrganizationDetailsResponse> GetOrganizationDetails(int orgId)
		{
			var response = await RestClient.GetAsync($"api/organization/v1/{orgId}");
			if (response.IsSuccessStatusCode)
			{
				string responseContent = await response.Content.ReadAsStringAsync();
				Logger.Trace($"Raw Response: {responseContent}");
			}

			var orgDetailsResponse = await ProcessResponse<OrganizationDetailsResponse>(response);
			return orgDetailsResponse;
		}

		public async Task<ListPersonsResponse> ListPersons(int orgId)
		{
			int pageSize = 25;
			List<Person> responseList = new List<Person>();
			List<Person> partialList = new List<Person>();
			do
			{
				partialList = await PagePerons(orgId, responseList.Count - 1, pageSize);
				responseList.AddRange(partialList);
			}
			while (partialList.Count == pageSize);

			return new ListPersonsResponse() { Persons = responseList };
		}

		public async Task<ListCustomFieldsResponse> ListCustomFields()
		{
			var response = await RestClient.GetAsync("api/ssl/v1/customFields");
			return new ListCustomFieldsResponse { CustomFields = await ProcessResponse<List<CustomField>>(response) };
		}

		public async Task<ListSslProfilesResponse> ListSslProfiles(int? orgId = null)
		{
			string urlSuffix = string.Empty;
			if (orgId.HasValue)
			{
				urlSuffix = $"?organizationId={orgId}";
			}

			var response = await RestClient.GetAsync($"api/ssl/v1/types{urlSuffix}");
			return new ListSslProfilesResponse { SslProfiles = await ProcessResponse<List<Profile>>(response) };
		}

		public async Task<List<Person>> PagePerons(int orgId, int position = 0, int size = 25)
		{
			var response = await RestClient.GetAsync($"api/person/v1?position={position}&size={size}&organizationId={orgId}");
			return await ProcessResponse<List<Person>>(response);
		}

		public async Task<int> Enroll(EnrollRequest request)
		{
			try
			{
				var response = await RestClient.PostAsJsonAsync("api/ssl/v1/enroll", request);
				var enrollResponse = await ProcessResponse<EnrollResponse>(response);

				return enrollResponse.sslId;
			}
			catch (InvalidOperationException invalidOp)
			{
				throw new Exception($"Invalid Operation. {invalidOp.Message}|{invalidOp.StackTrace}", invalidOp);
			}
			catch (HttpRequestException httpEx)
			{
				throw new Exception($"HttpRequestException. {httpEx.Message}|{httpEx.StackTrace}", httpEx);
			}
			catch (SectigoApiException)
			{
				throw;
			}
		}

		public async Task<int> Renew(int sslId)
		{
			try
			{
				var response = await RestClient.PostAsJsonAsync($"api/ssl/v1/renewById/{sslId}", "");
				var renewResponse = await ProcessResponse<EnrollResponse>(response);

				return renewResponse.sslId;
			}
			catch (InvalidOperationException invalidOp)
			{
				throw new Exception($"Invalid Operation. {invalidOp.Message}|{invalidOp.StackTrace}");
			}
			catch (HttpRequestException httpEx)
			{
				throw new Exception($"HttpRequestException. {httpEx.Message}|{httpEx.StackTrace}");
			}
			catch (SectigoApiException)
			{
				throw;
			}
		}

		public async Task<X509Certificate2> PickupCertificate(int sslId, string subject)
		{
			var response = await RestClient.GetAsync($"api/ssl/v1/collect/{sslId}/x509CO");

			if (response.IsSuccessStatusCode && response.Content.Headers.ContentLength > 0)
			{
				string pemChain = await response.Content.ReadAsStringAsync();

				string[] splitChain = pemChain.Replace("\r\n", string.Empty).Split(new string[] { "-----" }, StringSplitOptions.RemoveEmptyEntries);

				return new X509Certificate2(Convert.FromBase64String(splitChain[1]));
			}
			return null;
			//return new X509Certificate2();
		}

		public async Task Reissue(ReissueRequest request, int sslId)
		{
			var response = await RestClient.PostAsJsonAsync($"api/ssl/v1/replace/{sslId}", request);
			response.EnsureSuccessStatusCode();
		}

		#region Static Methods

		private static Func<String, String> hexify = (ss => ss.Length <= 2 ? ss : ss.Substring(0, 2) + ":" + hexify(ss.Substring(2)));

		private static async Task<T> ProcessResponse<T>(HttpResponseMessage response)
		{
			if (response.IsSuccessStatusCode)
			{
				string responseContent = await response.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<T>(responseContent);
			}
			else
			{
				var error = JsonConvert.DeserializeObject<Error>(await response.Content.ReadAsStringAsync());
				throw new SectigoApiException($"{error.Code} | {error.Description}") { ErrorCode = error.Code, Description = error.Description };
			}
		}

		private static string GetCertificateType(CertificateType type)
		{
			return Enum.GetName(typeof(CertificateType), type)?.ToLower();
		}

		#endregion Static Methods
	}
}