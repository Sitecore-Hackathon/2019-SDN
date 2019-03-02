using System.Collections.Generic;
using Sitecore.Data;

namespace Hackathon.SDN.Foundation.TranslationService.Models {
    public class TranslationResult {

        public TranslationResult() {
            ItemsWithErrors = new List<ID>();
        }

        public int SuccessfullyTranslated { get; set; }

        public int Skipped { get; set; }

        public int OccuredErrors { get; set; }

        public List<ID> ItemsWithErrors { get; set; }
    }
}