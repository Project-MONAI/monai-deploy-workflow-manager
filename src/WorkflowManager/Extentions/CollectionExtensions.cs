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

// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;

namespace Monai.Deploy.WorkflowManager.Extentions
{
    /// <summary>
    /// Set of useful extensions for collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>Indicates whether the specified array is null or has a length of zero.</summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="array">The array to test.</param>
        /// <returns>true if the array parameter is null or has a length of zero; otherwise, false.</returns>
        /// <returns>true if collection is empty.</returns>
        public static bool IsNullOrEmpty<T>(this ICollection<T> array)
        {
            return array is null || array.Count == 0;
        }
    }
}
