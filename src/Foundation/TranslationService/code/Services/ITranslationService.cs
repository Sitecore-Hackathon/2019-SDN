using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Hackathon.SDN.Foundation.TranslationService.Services {
    public interface ITranslationService {

        string TranslateItem(Item sourceItem, Language targetLanguage, bool includeSubItems);
    }
}