namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Trims entire strings from start of strings.
        /// by default is case insenstive.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="suffixToRemove"></param>
        /// <param name="stringComparison"></param>
        /// <returns></returns>
        public static string TrimStartExt(this string input, string suffixToRemove, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            switch (stringComparison)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.InvariantCulture:
                case StringComparison.Ordinal:
                    input = input.TrimStart();
                    while (input != null && suffixToRemove != null && input.TrimStart().StartsWith(suffixToRemove))
                    {
                        input = input.TrimStart().Substring(suffixToRemove.Length, input.Length - suffixToRemove.Length);
                    }
                    return input?.TrimStart() ?? string.Empty;
                case StringComparison.CurrentCultureIgnoreCase:
                case StringComparison.InvariantCultureIgnoreCase:
                case StringComparison.OrdinalIgnoreCase:
                    input = input.TrimStart();
                    while (input != null && suffixToRemove != null && input.ToUpper().TrimStart().StartsWith(suffixToRemove.ToUpper()))
                    {
                        input = input.TrimStart().Substring(suffixToRemove.Length, input.Length - suffixToRemove.Length);
                    }
                    return input?.TrimStart() ?? string.Empty;
            }
            return string.Empty;
        }
    }
}
