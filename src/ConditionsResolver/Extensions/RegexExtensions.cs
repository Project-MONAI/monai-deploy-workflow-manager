using System.Text.RegularExpressions;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Extensions
{
    public static class RegexExtensions
    {
        public static string[] SplitOnce(this Regex regex, string input)
        {
            var inputArr = regex.Split(input);
            return new string[] { inputArr.First(), string.Join(" ", inputArr.Skip(1)) };
        }
    }
}
