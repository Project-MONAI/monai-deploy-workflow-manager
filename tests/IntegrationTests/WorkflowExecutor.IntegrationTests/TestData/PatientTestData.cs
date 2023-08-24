/*
 * Copyright 2022 MONAI Consortium
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

using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.TestData
{
    public class PatientTestData
    {
        public string? Name { get; set; }

        public PatientDetails? Patient { get; set; }
    }

    public static class PatientsTestData
    {
        public static List<PatientTestData> TestData = new List<PatientTestData>()
        {
            new PatientTestData()
            {
                Name = "Full_Patient",
                Patient = new PatientDetails()
                {
                    PatientId = "1",
                    PatientName = "Patient_Full_Patient",
                    PatientSex = "male",
                    PatientDob = new DateTime(2000, 01, 01, 0, 0, 0, kind: DateTimeKind.Local),
                    PatientAge = "21",
                    PatientHospitalId = "123"
                }
            },
            new PatientTestData()
            {
                Name = "Partial_Patient",
                Patient = new PatientDetails()
                {
                    PatientId = "2",
                    PatientName = null,
                    PatientSex = "female",
                    PatientDob = null,
                    PatientAge = null,
                    PatientHospitalId = null
                }
            },
            new PatientTestData()
            {
                Name = "Null_Patient",
                Patient = new PatientDetails()
                {
                    PatientId = null,
                    PatientName = null,
                    PatientSex = null,
                    PatientDob = null,
                    PatientAge = null,
                    PatientHospitalId = null
                }
            },
        };
    }
}
