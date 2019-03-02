namespace Hackathon.SDN.Foundation.TranslationService.Infrastructure.Pipelines.FieldShouldBeTranslated {

    public class ShouldBeTranslatedProperty {

        public void Process(FieldShouldBeTranslatedPipelineArgs args) {
            if (!args.Field.ShouldBeTranslated) {
                args.ShouldBeTranslated = false;
            }
        }

    }

}