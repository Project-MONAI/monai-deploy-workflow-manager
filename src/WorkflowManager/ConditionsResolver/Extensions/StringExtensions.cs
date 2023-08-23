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

using System.Text;

namespace Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Trims entire strings from start of strings.
        /// by default is case insenstive.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="prefixToRemove"></param>
        /// <param name="stringComparison"></param>
        /// <returns></returns>
        public static string TrimStartExt(this string input, string prefixToRemove, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            switch (stringComparison)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.InvariantCulture:
                case StringComparison.Ordinal:
                    while (input != null && prefixToRemove != null && input.TrimStart().StartsWith(prefixToRemove))
                    {
                        // clean string before removing the prefix
                        input = input.TrimStart();
                        // clean string after removing the prefix
                        input = input.TrimStart().Substring(prefixToRemove.Length, input.Length - prefixToRemove.Length);
                    }
                    return input?.TrimStart() ?? string.Empty;

                case StringComparison.CurrentCultureIgnoreCase:
                case StringComparison.InvariantCultureIgnoreCase:
                case StringComparison.OrdinalIgnoreCase:
                    while (input != null && prefixToRemove != null && input.ToUpper().TrimStart().StartsWith(prefixToRemove.ToUpper()))
                    {
                        input = input.TrimStart();
                        input = input.TrimStart().Substring(prefixToRemove.Length, input.Length - prefixToRemove.Length);
                    }
                    return input?.TrimStart() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Adds brackets to multiple conditions.
        /// </summary>
        /// <param name="input">Array of conditions.</param>
        /// <returns></returns>
        public static string CombineConditionString(this string[] input)
        {
            var value = new StringBuilder();

            if (input.Length == 1)
            {
                return input.First();
            }

            for (var i = 0; i < input.Length; i++)
            {
                value.Append($"({input[i]})");

                if (i != input.Length - 1)
                {
                    value.Append(" AND ");
                }
            }

            return value.ToString();
        }
    }
}
