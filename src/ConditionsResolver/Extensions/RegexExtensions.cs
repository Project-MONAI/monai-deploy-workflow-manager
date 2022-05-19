// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Text.RegularExpressions;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Extensions
{
    public static class RegexExtensions
    {
        public static string[] SplitOnce(this Regex regex, string input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            var inputArr = regex.Split(input);
            return new string[] { inputArr.First(), string.Join(string.Empty, inputArr.Skip(1)) };
        }
    }
}
