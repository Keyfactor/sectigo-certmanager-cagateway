// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.AnyGateway.Sectigo.API
{
    
    public class EnrollRequest
    {
        public int orgId { get; set; }
        public string csr { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string subjAltNames { get; set; }
        public int certType { get; set; }
        public int numberServers { get; set; }
        public int serverType { get; set; }
        public int term { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string comments { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<CustomField> customFields { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string externalRequester { get; set; }
    }

    public class CustomField
    {
        public string name { get; set; }

        public string value { get; set; }

		[JsonIgnore]
        public bool mandatory { get; set; }
    }
}
