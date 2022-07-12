namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class ClinicalReviewStepDefinitions
    {
        public ClinicalReviewStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            ClinicalReviewQueue = objectContainer.Resolve<RabbitConsumer>("ClincialReviewQueue");
            _outputHelper = outputHelper;
            DataHelper = objectContainer.Resolve<DataHelper>();
        }

        public RabbitConsumer ClinicalReviewQueue { get; }

        private readonly ISpecFlowOutputHelper _outputHelper;

        public DataHelper DataHelper { get; }

        [Then(@"A Clincial Review Request event is published")]
        public void ThenAClincialReviewRequestIsPublished()
        {
            throw new PendingStepException();
        }
    }
}
