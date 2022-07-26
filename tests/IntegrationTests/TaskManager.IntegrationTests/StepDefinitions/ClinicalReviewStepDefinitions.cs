/*
 * Copyright 2021-2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
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
