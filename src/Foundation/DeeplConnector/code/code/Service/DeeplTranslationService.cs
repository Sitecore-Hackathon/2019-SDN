using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Hackathon.SDN.Foundation.DeeplConnector.Models;
using Hackathon.SDN.Foundation.TranslationService.Interface;
using Newtonsoft.Json;

namespace Hackathon.SDN.Foundation.DeeplConnector.Service {

    public class DeeplTranslationService : ITranslationProviderService {

        private readonly string DeeplAuthKey;

        private readonly string _deeplBaseUrl;


        public DeeplTranslationService() {
            DeeplAuthKey = Sitecore.Configuration.Settings.GetSetting("DeeplAuthKey");
            _deeplBaseUrl = Sitecore.Configuration.Settings.GetSetting("DeeplBaseUrl");
        }

        public string GetTranslatedContent(string sourceText, string sourceLanguageCode, string targetLanguageCode) {

            var request =
                (HttpWebRequest)WebRequest.Create(_deeplBaseUrl + $"?auth_key={DeeplAuthKey}" + $"&text={sourceText}" +
                                                   $"&source_lang={sourceLanguageCode.ToUpper()}" +
                                                   $"&target_lang={targetLanguageCode.ToUpper()}" + "&ignore_tags=<a>");

            var responseJson = string.Empty;
            using (var response = request.GetResponse()) {
                using (var responseStream = response.GetResponseStream()) {
                    if (responseStream != null) {
                        using (var reader = new StreamReader(responseStream)) {
                            responseJson = reader.ReadToEnd();
                        }
                    }
                }
            }

            var listTranslations = JsonConvert.DeserializeObject<List<Translation>>(responseJson);
            if (listTranslations.Any() == false) {
                throw new NoTranslationFoundException();
            }

            if (listTranslations.Count > 1) {
                throw new MultipleTranslationFoundException();
            }

            return listTranslations.First().Text;
        }
    }
}
