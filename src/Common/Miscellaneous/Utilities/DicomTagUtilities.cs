/*
 * Copyright 2023 MONAI Consortium
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

using System.Text.RegularExpressions;
using FellowOakDicom;

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous.Utilities
{
    public static class DicomTagUtilities
    {
        public static DicomTag GetDicomTagByName(string tag)
        {
            return DicomDictionary.Default[tag] ?? DicomDictionary.Default[Regex.Replace(tag, @"\s+", "", RegexOptions.None, TimeSpan.FromSeconds(1))];
        }

        public static (bool valid, IList<string> invalidTags) DicomTagsValid(IEnumerable<string> dicomTags)
        {
            var invalidTags = new List<string>();

            foreach (var t in dicomTags)
            {
                var tag = GetDicomTagByName(t);
                if (tag == null)
                {
                    invalidTags.Add(t);
                }
            }

            return (!invalidTags.Any(), invalidTags);
        }
    }
}
