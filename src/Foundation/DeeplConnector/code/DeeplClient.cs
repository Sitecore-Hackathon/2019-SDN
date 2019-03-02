using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace DeeplConnector {

    public class DeeplClient {

        private readonly string _baseUrl;

        private readonly string _authKey;

        public DeeplClient(string baseUrl, string authKey) {
            _baseUrl = baseUrl;
            _authKey = authKey;
        }
        
        public string SendTranslationRequest(string text, string sourceLanguage, string targetLanguage) {
            var request = (HttpWebRequest)WebRequest.Create(_baseUrl);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version11;
            request.Method = "POST";

            var postBytes = Encoding.UTF8.GetBytes($"auth_key={_authKey}&text={HttpUtility.UrlEncode(text)}&source_lang={HttpUtility.UrlEncode(sourceLanguage)}&target_lang={HttpUtility.UrlEncode(targetLanguage)}");

            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.Accept = "application/json";
            request.ContentLength = postBytes.Length;

            var requestStream = request.GetRequestStream();
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            var response = (HttpWebResponse)request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream())) {
                return reader.ReadToEnd();
            }
        }

    }

}
