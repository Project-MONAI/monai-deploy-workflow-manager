using System.Text.RegularExpressions;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Extensions;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Resolver
{
    public class ConditionalGroup
    {
        public Keyword Keyword { get; set; }

        public Conditional? LeftConditional { get; set; } = null;

        public Conditional? RightConditional { get; set; } = null;

        public ConditionalGroup? LeftGroup { get; set; } = null;

        public ConditionalGroup? RightGroup { get; set; } = null;

        public bool LeftIsSet => LeftGroup is not null || LeftConditional is not null;

        public bool RightIsSet => RightGroup is not null || RightConditional is not null;

        public int GroupedLogical { get; set; } = 1;

        public Regex FindAnds { get; } = new Regex(@"([\s]and[\s]|[\s]AND[\s]|[\s]And[\s])");

        public Regex FindOrs { get; } = new Regex(@"([\s]or[\s]|[\s]OR[\s]|[\s]Or[\s])");

        public Regex FindBrackets { get; } = new Regex(@"((?<!\[)\()");

        public Regex FindCloseBrackets { get; } = new Regex(@"((?<!\[)\))");

        private string[] ParseOrs(string input) => FindOrs.SplitOnce(input);

        private string[] ParseAnds(string input) => FindAnds.SplitOnce(input);

        public void Set(string left, string right, Keyword keyword)
        {
            Keyword = keyword;
            if (!string.IsNullOrEmpty(left))
            {
                if (FindAnds.Matches(left).Any() || FindOrs.Matches(left).Any())
                {
                    LeftGroup = ConditionalGroup.Create(left, GroupedLogical);
                }
                else
                {
                    LeftConditional = Conditional.Create(left);
                }
            }
            if (!string.IsNullOrEmpty(right))
            {
                if (FindAnds.Matches(right).Any() || FindOrs.Matches(right).Any() || FindBrackets.Matches(right).Any())
                {
                    RightGroup = ConditionalGroup.Create(right, GroupedLogical);
                }
                else
                {
                    RightConditional = Conditional.Create(right);
                }
            }
        }

        public void Parse(string input, int groupedLogicalParent = 0)
        {
            var foundOpenBrackets = FindBrackets.Matches(input);
            var foundClosingBrackets = FindCloseBrackets.Matches(input);
            if (GroupedLogical > 1 && foundOpenBrackets.Count != foundClosingBrackets.Count)
            {
                throw new ArgumentException("Matching brackets missing.");
            }

            var foundAnds = FindAnds.Matches(input);
            var foundOrs = FindOrs.Matches(input);

            if (foundOpenBrackets.Count >= 1)
            {
                var firstOpenBracketIndex = foundOpenBrackets.First().Index;
                if (firstOpenBracketIndex == 0)
                {
                    GroupedLogical = groupedLogicalParent + 1;
                    input = input.Trim('(');
                }
            }

            if (foundAnds.Count == 1 && foundOrs.Count == 0)
            {
                var splitByAnd = ParseAnds(input);
                Set(splitByAnd[0], splitByAnd[1].TrimStartExt("AND"), Keyword.AND);
                return;
            }
            if (foundAnds.Count == 0 && foundOrs.Count == 1)
            {
                var splitByOr = ParseOrs(input);
                Set(splitByOr[0], splitByOr[1].TrimStartExt("OR"), Keyword.OR);
                return;
            }
            if (foundAnds.Count == 0 && foundOrs.Count == 0)
            {
                Set(input, string.Empty, Keyword.SINGULAR);
                return;
            }

            ParseComplex(input);
        }


        private void ParseBrackets(string input)
        {
            var foundAnds = FindAnds.Matches(input);
            var foundOrs = FindOrs.Matches(input);

            var indexOfBracket = FindBrackets.Match(input).Index;
            var indexOfClosingBracket = FindCloseBrackets.Match(input).Index;
            if (indexOfBracket == -1)
            {
                throw new InvalidOperationException("Expected Bracket: Bracket not found");
            }
            var startingBracketHasBeenTrimmed = indexOfClosingBracket < indexOfBracket;
            // if first index of any ANDs or ORs are before the first bracket then left hand evaluation should be processed
            if (!startingBracketHasBeenTrimmed && (foundAnds.Any() && foundAnds.First().Index < indexOfBracket) || (foundOrs.Any() && foundOrs.First().Index < indexOfBracket))
            {
                if (foundAnds.Any() && foundAnds.First().Index < foundOrs.First().Index)
                {
                    var splitByAnds = ParseAnds(input);
                    var rightAnd = splitByAnds[1].TrimStartExt("AND");
                    Set(splitByAnds[0], rightAnd, Keyword.AND);
                }
                else if (!foundAnds.Any() && foundOrs.Any() || foundOrs.First().Index < foundAnds.First().Index)
                {
                    var splitByOrs = ParseOrs(input);
                    var rightOrs = splitByOrs[1].TrimStartExt("OR");
                    Set(splitByOrs[0], rightOrs, Keyword.OR);
                }
            }
            else
            {
                //handle left hand brackets
                var bracketedConditionalGroup = input.Substring(0, indexOfClosingBracket);
                LeftGroup = ConditionalGroup.Create(bracketedConditionalGroup, GroupedLogical);
                var rightSideConditionalGroup = input.Substring(indexOfClosingBracket + 1, input.Length - indexOfClosingBracket - 1);
                rightSideConditionalGroup = rightSideConditionalGroup.TrimStartExt("AND").TrimStartExt("OR");
                RightGroup = ConditionalGroup.Create(rightSideConditionalGroup, GroupedLogical);
            }
        }

        private void ParseComplex(string input)
        {
            var foundBrackets = FindBrackets.Matches(input);

            if (foundBrackets.Any())
            {
                ParseBrackets(input);
            }

            var getFirstIndexOf = (Regex find) => find.Match(input).Index;
            if (FindOrs.IsMatch(input) && getFirstIndexOf(FindAnds) > getFirstIndexOf(FindOrs) || !FindAnds.IsMatch(input)) // gets first index for any "AND" if its greater than parse left OR first
            {
                var splitByOr = ParseOrs(input);
                if (splitByOr[0] == string.Empty || splitByOr[1] == string.Empty)
                {
                    throw new ArgumentException($"Error parsing OR condition in: {input}");
                }
                Set(splitByOr[0], splitByOr[1].TrimStartExt("OR"), Keyword.OR);
            }
            else
            {
                var splitByAnd = ParseAnds(input);
                if (splitByAnd[0] == string.Empty || splitByAnd[1] == string.Empty)
                {
                    throw new ArgumentException($"Error parsing OR condition in: {input}");
                }
                Set(splitByAnd[0], splitByAnd[1].TrimStartExt("AND"), Keyword.AND);
            }
        }

        public bool Evaluate()
        {
            if (Keyword == Keyword.AND)
            {
                return EvaluateAnds();
            }
            if (Keyword == Keyword.OR)
            {
                return EvaluateOrs();
            }
            if (LeftConditional is not null && !RightIsSet)
            {
                return LeftConditional.Evaluate();
            }

            throw new InvalidOperationException("Evaluation Error");
        }

        private bool EvaluateOrs()
        {
            if (RightGroup is not null && RightGroup.GroupedLogical > GroupedLogical)
            {
                if (LeftConditional is not null)
                {
                    return RightGroup.Evaluate() || LeftConditional.Evaluate();
                }
                if (LeftGroup is not null)
                {
                    return RightGroup.Evaluate() || LeftGroup.Evaluate();
                }
            }

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
            if (LeftConditional is not null && RightGroup is not null)
            {
                return LeftConditional.Evaluate() || RightGroup.Evaluate();
            }

            throw new InvalidOperationException("Evaluation Error in EvaluateOrs");
        }

        private bool EvaluateAnds()
        {
            if (RightGroup is not null && RightGroup.GroupedLogical > GroupedLogical)
            {
                if (LeftConditional is not null)
                {
                    return RightGroup.Evaluate() && LeftConditional.Evaluate();
                }
                if (LeftGroup is not null)
                {
                    return RightGroup.Evaluate() && LeftGroup.Evaluate();
                }
            }

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
            if (LeftConditional is not null && RightGroup is not null)
            {
                return LeftConditional.Evaluate() && RightGroup.Evaluate();
            }

            throw new InvalidOperationException("Evaluation Error in EvaluateAnds");
        }

        public static ConditionalGroup Create(string input, int groupedLogicalParent = 0)
        {
            var conditionalGroup = new ConditionalGroup();
            if (groupedLogicalParent == 0)
            {
                input = TrimStartingBrackets(input, conditionalGroup);
            }
            conditionalGroup.Parse(input.Trim(), groupedLogicalParent);

            return conditionalGroup;

            string TrimStartingBrackets(string input, ConditionalGroup conditionalGroup)
            {
                var foundOpenBrackets = conditionalGroup.FindBrackets.Matches(input);
                var foundClosingBrackets = conditionalGroup.FindCloseBrackets.Matches(input);

                if (foundOpenBrackets.Count != foundClosingBrackets.Count)
                {
                    throw new ArgumentException("Matching brackets missing.");
                }

                if (foundOpenBrackets.Count == 1)
                {
                    var foundBracketsAreAtStartAndEnd = foundOpenBrackets.First().Index == 0
                        && foundClosingBrackets.First().Index == input.Length - 1;
                    if (foundBracketsAreAtStartAndEnd)
                    {
                        input = input.Trim('(');
                        input = input.Trim(')');
                    }
                }

                return input;
            }
        }
    }
}
