using System;
using Newtonsoft.Json;

namespace Hackathon.SDN.Foundation.DeeplConnector.Tests.Models {

    internal class HttpBinResponse {

        [JsonProperty("data")]
        public string Data { get; set; }
    
        [JsonProperty("form")]
        public Form Form { get; set; }

        [JsonProperty("headers")]
        public Headers Headers { get; set; }

        [JsonProperty("json")]
        public object Json { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("origin")]
        public string Origin { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

    }

    internal class Form {

        [JsonProperty("auth_key")]
        public string AuthKey { get; set; }

        [JsonProperty("source_lang")]
        public string SourceLang { get; set; }

        [JsonProperty("target_lang")]
        public string TargetLang { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

    }

    internal class Headers {

        [JsonProperty("Accept")]
        public string Accept { get; set; }

    }

}

