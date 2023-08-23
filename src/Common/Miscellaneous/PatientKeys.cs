﻿/*
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

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous
{
    public static class PatientKeys
    {
        /// <summary>
        /// Key for the patient id.
        /// </summary>
        public static readonly string PatientId = "patient_id";

        /// <summary>
        /// Key for the patient name.
        /// </summary>
        public static readonly string PatientName = "patient_name";

        /// <summary>
        /// Key for the patient sex.
        /// </summary>
        public static readonly string PatientSex = "patient_sex";

        /// <summary>
        /// Key for the patient dob.
        /// </summary>
        public static readonly string PatientDob = "patient_dob";

        /// <summary>
        /// Key for the patient age.
        /// </summary>
        public static readonly string PatientAge = "patient_age";

        /// <summary>
        /// Key for the patient hospital id.
        /// </summary>
        public static readonly string PatientHospitalId = "patient_hospital_id";
    }
}
