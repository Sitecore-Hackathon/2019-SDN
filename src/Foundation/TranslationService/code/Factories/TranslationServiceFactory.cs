using Hackathon.SDN.Foundation.TranslationService.Services;
using Sitecore.Configuration;

namespace Hackathon.SDN.Foundation.TranslationService.Factories {

    public static class TranslationServiceFactory {

        public static ITranslationService Create() => Factory.CreateObject("contentTranslator/translationService", true) as ITranslationService;

    }

}