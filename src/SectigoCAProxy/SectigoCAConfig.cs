// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Keyfactor.AnyGateway.Sectigo
{
    public class SectigoCAConfig
    {
        public SectigoCAConfig()
        {
            SyncFilter = new Dictionary<string, string[]>();
        }
        [JsonProperty("ApiEndpoint")]
        public string ApiEndpoint { get; set; }
        [JsonProperty("AuthType")]
        public string AuthenticationType { get; set; }
        [JsonProperty("CustomerUri")]
        public string CustomerUri { get; set; }
        [JsonProperty("Username")]
        public string Username { get; set; }
        [JsonProperty("Password")]
        public string Password { get; set; }
        [JsonProperty("PickupRetries")]
        public int PickupRetries { get; set; }
        [JsonProperty("PickupDelay")]
        public int PickupDelayInSeconds { get; set; }
        [JsonProperty("PageSize")]
        public int PageSize { get; set; }
        [JsonProperty("ExternalRequestorFieldName")]
        public string ExternalRequestorFieldName { get; set; }
        [JsonProperty("SyncFilter")]
        public Dictionary<string, string[]> SyncFilter { get; set; }
        public string GetSyncFilterQueryString() 
        { 
            string filterQueryString = string.Empty;
            foreach (var f in this.SyncFilter)
            {
                filterQueryString += $"{f.Key}={String.Join($"&{f.Key}=", f.Value)}&";
            }

            return filterQueryString.TrimEnd('&');
        }
    }

}
