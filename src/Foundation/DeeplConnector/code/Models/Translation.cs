using Newtonsoft.Json;

namespace DeeplConnector.Models {

    public class Translation {

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

    }

}
