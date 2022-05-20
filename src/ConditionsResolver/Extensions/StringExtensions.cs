// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Extensions
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
    }
}
