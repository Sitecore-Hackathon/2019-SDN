using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace TranslationService.Interface {
    public interface ITranslationService {

        string TranslateItem(Item sourceItem, Language targetLanguage, bool includeRelatedItems, bool includeSubItems);
    }
}
