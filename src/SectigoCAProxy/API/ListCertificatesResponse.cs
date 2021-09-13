// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.AnyGateway.Sectigo.API
{
    public class ListCertificatesResponse
    { 
        public List<Certificate> Certificates { get; set; }
    }

    public class Certificate
    {
        public Certificate()
        {
            SubjectAlternativeNames = new List<string>();
        }

        [JsonProperty("sslId")]
        public int Id { get; set; }
        [JsonProperty("commonName")]
        public string CommonName { get; set; }
        [JsonProperty("subjectAlternativeNames")]
        public List<string> SubjectAlternativeNames { get; set; }
        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }
        [JsonProperty("certType")]
        public Profile CertType { get; set; }
        public DateTime? requested { get; set; }
        public DateTime? approved { get; set; }
        public DateTime? revoked { get; set; }
        public string status { get; set; }

        public override string ToString()
        {
            return $"sslId:{this.Id} | commonName:{this.CommonName} | serialNumber:{this.SerialNumber}";
        }
    }

    public class SubjectAltName
    { 
        public string Name { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CertificateType
    { 
        SSL=0,
        DEVICE=1,
        SMIME=2,
        CodeSign=4
    }
}
