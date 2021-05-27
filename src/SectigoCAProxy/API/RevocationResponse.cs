using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.AnyGateway.Sectigo.API
{
    public class RevocationResponse
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsSuccess { get; set; }
    }
}
