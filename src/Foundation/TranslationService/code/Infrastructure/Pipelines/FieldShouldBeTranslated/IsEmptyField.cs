namespace Hackathon.SDN.Foundation.TranslationService.Infrastructure.Pipelines.FieldShouldBeTranslated {

    public class IsEmptyField {

        public void Process(FieldShouldBeTranslatedPipelineArgs args) {
            if (string.IsNullOrWhiteSpace(args.Field.Value)) {
                args.ShouldBeTranslated = false;
            }
        }

    }

}