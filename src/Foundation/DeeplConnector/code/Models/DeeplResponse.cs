using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeeplConnector.Models
{
    public class DeeplResponse {

        [JsonProperty(PropertyName = "translations")]
        public IEnumerable<Translation> Translations { get; set; }

    }
}