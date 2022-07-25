// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.PayloadListener.Extensions
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
