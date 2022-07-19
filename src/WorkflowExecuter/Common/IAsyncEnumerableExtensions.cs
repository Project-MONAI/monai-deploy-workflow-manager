// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monai.Deploy.WorkflowManager.Extentions
{
    /// <summary>
    /// IAsyncEnumerable Extensions.
    /// </summary>
    public static class IAsyncEnumerableExtensions
    {
        /// <summary>
        /// Creates IAsyncEnumerable from an IEnumerable type.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="enumerable">IEnumerable to make Async.</param>
        /// <returns>IAsyncEnumerable of enumerable</returns>
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                yield return await Task.FromResult(item);
            }
        }

        public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
        {
            foreach (var value in list)
            {
                await func(value);
            }
        }

    }
}
