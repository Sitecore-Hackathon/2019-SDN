using Sitecore.Globalization;

namespace Hackathon.SDN.Foundation.TranslationService.Providers {

    public interface ITranslationProvider {
        
        string Translate(string text, Language sourceLanguage, Language targetLanguage);

        bool CanTranslate(Language sourceLanguage, Language targetLanguage);

    }

}
