using System;
using TranslationService.Interface;

namespace DeeplConnector.Service {

    public class DeeplTranslationService : ITranslationProviderService {

        private readonly DeeplClient _client;
        
        public DeeplTranslationService(DeeplClient client) {
            _client = client;
        }

        public string GetTranslatedContent(string sourceText, string sourceLanguageCode, string targetLanguageCode) {
            throw new NotImplementedException();
        }

    }

}
