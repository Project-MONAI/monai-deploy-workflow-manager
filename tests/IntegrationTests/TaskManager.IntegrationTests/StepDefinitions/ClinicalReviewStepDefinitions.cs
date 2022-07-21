// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class ClinicalReviewStepDefinitions
    {
        public ClinicalReviewStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
            DataHelper = objectContainer.Resolve<DataHelper>() ?? throw new ArgumentNullException(nameof(DataHelper));
            Assertions = new Assertions(_outputHelper);
        }

        private readonly ISpecFlowOutputHelper _outputHelper;
        public DataHelper DataHelper { get; }
        public Assertions Assertions { get; }

        [Then(@"A Clincial Review Request event is published")]
        public void ThenAClincialReviewRequestIsPublished()
        {
            var clinicalReviewRequestEvent = DataHelper.GetClinicalReviewRequestEvent();

            Assertions.AssertClinicalReviewEvent(clinicalReviewRequestEvent, DataHelper.TaskDispatchEvent);
        }
    }
}
