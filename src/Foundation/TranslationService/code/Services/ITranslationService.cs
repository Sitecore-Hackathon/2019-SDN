﻿using Hackathon.SDN.Foundation.TranslationService.Models;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Jobs;

namespace Hackathon.SDN.Foundation.TranslationService.Services {
    public interface ITranslationService {

        TranslationResult TranslateItem(Item sourceItem, Language targetLanguage, bool includeSubItems, Job job);
    }
}