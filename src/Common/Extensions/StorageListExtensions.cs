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

using Ardalis.GuardClauses;

namespace Monai.Deploy.WorkflowManager.Common.Extensions
{
    public static class StorageListExtensions
    {
        public static Dictionary<string, string> ToArtifactDictionary(this List<Messaging.Common.Storage> storageList)
        {
            Guard.Against.Null(storageList, nameof(storageList));

            var artifactDict = new Dictionary<string, string>();

            foreach (var storage in storageList)
            {
                if (string.IsNullOrWhiteSpace(storage.Name) || string.IsNullOrWhiteSpace(storage.RelativeRootPath))
                {
                    continue;
                }

                artifactDict.Add(storage.Name, storage.RelativeRootPath);
            }

            return artifactDict;
        }
    }
}
