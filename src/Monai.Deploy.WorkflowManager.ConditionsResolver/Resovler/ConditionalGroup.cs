using System.Text.RegularExpressions;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Resolver
{
    public class ConditionalGroup : IEvaluator
    {
        public Keyword Keyword { get; set; }

        public Conditional? LeftConditional { get; set; } = null;

        public Conditional? RightConditional { get; set; } = null;

        public ConditionalGroup? LeftGroup { get; set; } = null;

        public ConditionalGroup? RightGroup { get; set; } = null;

        public bool LeftIsSet => LeftGroup is not null || LeftConditional is not null;

        public bool RightIsSet => RightGroup is not null || RightConditional is not null;

        public Regex FindAnds { get; set; } = new Regex(@"([\s]and[\s]|[\s]AND[\s])");

        public Regex FindOrs { get; set; } = new Regex(@"([\s]or[\s]|[\s]OR[\s])");

        public void Set(string left, string right, Keyword keyword)
        {
            Keyword = keyword;
            if (FindAnds.Matches(left).Any() || FindOrs.Matches(left).Any())
            {
                LeftGroup = ConditionalGroup.Create(left);
            }
            else
            {
                LeftConditional = Conditional.Create(left);
            }
            if (!string.IsNullOrEmpty(right))
            {
                if (FindAnds.Matches(right).Any() || FindOrs.Matches(right).Any())
                {
                    RightGroup = ConditionalGroup.Create(right);
                }
                else
                {
                    RightConditional = Conditional.Create(right);
                }
            }
        }

        //TODO: this is not right place for this.
        private string[] RegexSplitConcat(string[] splitByArray)
        {
            return new string[] { splitByArray.First(), string.Join(" ", splitByArray.Skip(1)) };
        }

        //TODO: Move to extension string class and casing parameter
        public string TrimStartCaseInsensitive(string input, string suffixToRemove)
        {
            input = input.TrimStart();
            while (input != null && suffixToRemove != null && input.ToUpper().TrimStart().StartsWith(suffixToRemove.ToUpper()))
            {
                input = input.TrimStart().Substring(suffixToRemove.Length, input.Length - suffixToRemove.Length);
            }
            return input.TrimStart();
        }

        public void Parse(string input)
        {
            var foundAnds = FindAnds.Matches(input);
            var foundOrs = FindOrs.Matches(input);

            if (foundAnds.Count == 1 && foundOrs.Count == 0)
            {
                var splitByAnd = RegexSplitConcat(Regex.Split(input, @"([\s]and[\s]|[\s]AND[\s])", RegexOptions.IgnoreCase));
                Set(splitByAnd[0], TrimStartCaseInsensitive(splitByAnd[1], "AND"), Keyword.AND);
                return;
            }
            if (foundAnds.Count == 0 && foundOrs.Count == 1)
            {
                var splitByOr = RegexSplitConcat(Regex.Split(input, @"([\s]or[\s]|[\s]OR[\s])", RegexOptions.IgnoreCase));
                Set(splitByOr[0], TrimStartCaseInsensitive(splitByOr[1], "OR"), Keyword.OR);
                return;
            }
            if (foundAnds.Count == 0 && foundOrs.Count == 0)
            {
                Set(input, string.Empty, Keyword.SINGULAR);
                return;
            }

            ParseComplex(input);
        }

        private void ParseComplex(string input)
        {
            var getFirstIndexOf = (Regex find) => find.Match(input).Index;
            if (FindOrs.IsMatch(input) && getFirstIndexOf(FindAnds) > getFirstIndexOf(FindOrs) || !FindAnds.IsMatch(input)) // gets first index for any "AND" if its greater than parse left OR first
            {
                var splitByOr = RegexSplitConcat(Regex.Split(input, @"([\s]or[\s]|[\s]OR[\s])", RegexOptions.IgnoreCase));
                Set(splitByOr[0], TrimStartCaseInsensitive(splitByOr[1], "OR"), Keyword.OR);
            }
            else
            {
                var splitByAnd = RegexSplitConcat(Regex.Split(input, @"([\s]and[\s]|[\s]AND[\s])", RegexOptions.IgnoreCase));
                Set(splitByAnd[0], TrimStartCaseInsensitive(splitByAnd[1], "AND"), Keyword.AND);
            }
        }

        public bool Evaluate()
        {
            if (Keyword == Keyword.AND)
            {
                if (LeftConditional is not null && RightConditional is not null)
                {
                    return LeftConditional.Evaluate() && RightConditional.Evaluate();
                }
                if (LeftGroup is not null && RightGroup is not null)
                {
                    return LeftGroup.Evaluate() && RightGroup.Evaluate();
                }
                if (LeftGroup is not null && RightConditional is not null)
                {
                    return LeftGroup.Evaluate() && RightConditional.Evaluate();
                }
            }
            if (Keyword == Keyword.OR)
            {
                if (LeftConditional is not null && RightConditional is not null)
                {
                    return LeftConditional.Evaluate() || RightConditional.Evaluate();
                }
                if (LeftGroup is not null && RightGroup is not null)
                {
                    return LeftGroup.Evaluate() || RightGroup.Evaluate();
                }
                if (LeftGroup is not null && RightConditional is not null)
                {
                    return LeftGroup.Evaluate() || RightConditional.Evaluate();
                }
            }
            if (LeftConditional is not null && !RightIsSet)
            {
                return LeftConditional.Evaluate();
            }

            throw new InvalidOperationException("Evaluation Error");
        }

        public static ConditionalGroup Create(string input, int currentIndex = 0)
        {
            var conditionalGroup = new ConditionalGroup();
            conditionalGroup.Parse(input.Trim());

            return conditionalGroup;
        }
    }
}
