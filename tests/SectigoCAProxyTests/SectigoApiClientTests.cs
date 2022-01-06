using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Keyfactor.AnyGateway.Sectigo.Client;
using Keyfactor.AnyGateway.Sectigo.API;
using System.IO;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;

namespace SectigoCAProxyTests
{
    [TestClass]
    public class SectigoApiClientTests
    {
        [TestMethod]
        public async Task ApiClientReturnsPagedPersonList()
        {
            string personResponse = Mocks.GetFileFromResourceFolder("21PersonResponse.json");
            var handlerMock = Mocks.GetMessageHandler(personResponse);

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
            var apiClient = new SectigoApiClient(httpClient);

            var persons = await apiClient.ListPersons(12345);

            Assert.IsTrue(persons.Persons.Count>0);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }
        [TestMethod]
        public async Task ApiClientReturnsOrganizationList()
        {
            var handlerMock = Mocks.GetMessageHandler(Mocks.GetFileFromResourceFolder("OrgListResponse.json"));

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
            var apiClient = new SectigoApiClient(httpClient);

            var response = await apiClient.ListOrganizations();

            Assert.IsNotNull(response);
        }
        [TestMethod]
        public async Task ApiClientReturnsSuccessRevokeById()
        {
            var handlerMock = Mocks.GetMessageHandler("",
                System.Net.HttpStatusCode.NoContent);

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var apiClient = new SectigoApiClient(httpClient);

            var response = await apiClient.RevokeSslCertificateById(12345, "unspecified");

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());

        }
        [TestMethod]
        public void ApiClientReturnsSuccessSync()
        {
            var handlerMock = Mocks.GetMessageHandler("[{\"sslId\":139,\"commonName\":\"ccmqa.com\"}]", System.Net.HttpStatusCode.OK);

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
            var apiClient = new SectigoApiClient(httpClient);

            var certBuffer = new BlockingCollection<Certificate>(100);
            var cancelToken = new CancellationToken();

            var producerTask = apiClient.CertificateListProducer(certBuffer, cancelToken, 2,"");

            foreach (var c in certBuffer.GetConsumingEnumerable())
            {
                Assert.IsTrue(c.Id == 139);
            }

        }
        [TestMethod]
        public async Task ApiClientReturnsEnrollmentResult()
        {
            
            var httpHandler = Mocks.GetMessageHandler("{\"sslId\":136,\"renewId\":\"sWXl3EAqzn2qXLNP6pSW\"}", System.Net.HttpStatusCode.OK);

            var httpClient = new HttpClient(httpHandler.Object) {BaseAddress=new Uri("http://localhost") };
            var apiclient = new SectigoApiClient(httpClient);

            var response = await apiclient.Enroll(new EnrollRequest { });

            Assert.AreEqual(136, response);
            
        }
        [TestMethod]
        public async Task ApiClientReturnRenewalResult()
        {
            var httpHandler = Mocks.GetMessageHandler("{\"sslId\":136}", System.Net.HttpStatusCode.OK);
            var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
            var apiclient = new SectigoApiClient(httpClient);

            var response = await apiclient.Renew(136);

            Assert.AreEqual(136, response);
        }

        [TestMethod]
        public async Task ApiClientReturnReissueResult() 
        {

            var httpHandler = Mocks.GetMessageHandler("", System.Net.HttpStatusCode.NoContent);

            var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
            var apiclient = new SectigoApiClient(httpClient);
            var reissueRequest = new ReissueRequest();

            await apiclient.Reissue(reissueRequest, 136);

        }
        [TestMethod]
        public async Task ApliClientReturnsCertificateForPickup()
        {
            string pemChain = Mocks.GetFileFromResourceFolder("base64PEM.txt");
            var httpHandler = Mocks.GetMessageHandler(pemChain, System.Net.HttpStatusCode.OK);

            var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost")};
            var apiclient = new SectigoApiClient(httpClient);

            var response = await apiclient.PickupCertificate(12345, "CN=AddTrust External CA Root, OU=AddTrust External TTP Network, O=AddTrust AB, C=SE");

            Assert.IsTrue(response.Subject.Equals("CN=AddTrust External CA Root, OU=AddTrust External TTP Network, O=AddTrust AB, C=SE"));
        }

        [TestMethod]
        public async Task ApliClientReturnsCertificateById()
        {
            var httpHandler = Mocks.GetMessageHandler(Mocks.GetFileFromResourceFolder("CertificateResponse.json"));

            var httpClient = new HttpClient(httpHandler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var apiClient = new SectigoApiClient(httpClient);

            var response = await apiClient.GetCertificate(135);

            Assert.AreEqual(135, response.Id);
        }

        [TestMethod]
        public async Task ApiClientReturnsCustomFieldList()
        {
            var httpHandler = Mocks.GetMessageHandler("[{\"id\":81,\"name\":\"testName\",\"mandatory\":true},{\"id\":82,\"name\":\"testName2\",\"mandatory\":true}]");

            var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost/") };

            var apiClient = new SectigoApiClient(httpClient);

            var response = await apiClient.ListCustomFields();

            Assert.IsTrue(response.CustomFields.Count == 2);
        }

        [TestMethod]
        public async Task ApiClientReturnsSslProfileList()
        {
            var httpHandler = Mocks.GetMessageHandler("[{\"id\":2846,\"name\":\"SSL SASP 1113276892\",\"terms\":[365]},{\"id\":2847,\"name\":\"SSL SASP 1113276892\",\"terms\":[365]}]");
            var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
            var apiClient = new SectigoApiClient(httpClient);

            var response = await apiClient.ListSslProfiles();

            Assert.IsTrue(response.SslProfiles.Count == 2);
        }
        [TestMethod]
        public async Task ApiClientReturnsSslProfileListByOrg()
        {
            var httpHandler = Mocks.GetMessageHandler("[{\"id\":2846,\"name\":\"SSL SASP 1113276892\",\"terms\":[365]},{\"id\":2847,\"name\":\"SSL SASP 1113276892\",\"terms\":[365]}]");
            var httpClient = new HttpClient(httpHandler.Object) { BaseAddress = new Uri("http://localhost") };
            var apiClient = new SectigoApiClient(httpClient);

            var response = await apiClient.ListSslProfiles(123);


            httpHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => (r.Method == HttpMethod.Get)&&(r.RequestUri.Query.Contains("organizationId=123"))),
                ItExpr.IsAny<CancellationToken>());

            Assert.IsTrue(response.SslProfiles.Count == 2);
        }
    }
}
