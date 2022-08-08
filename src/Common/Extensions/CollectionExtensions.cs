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

using Ardalis.GuardClauses;

namespace Monai.Deploy.WorkflowManager.Common.Extensions
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

        /// <summary>
        /// Appends dictionaries together.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="array"></param>
        /// <param name="otherArray"></param>
        public static void Append<TKey, TValue>(this Dictionary<TKey, TValue> array, Dictionary<TKey, TValue> otherArray) where TKey : notnull
        {
            Guard.Against.Null(array);
            if (otherArray.IsNullOrEmpty())
            {
                return;
            }
            foreach (var item in otherArray)
            {
                array.Add(item.Key, item.Value);
            }
            return;
        }
    }
}
