using System.Text.RegularExpressions;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Resolver
{
    public class ConditionalGroup
    {
        public Keyword Keyword { get; set; }

        public Conditional LeftGroup { get; set; }

        public Conditional RightGroup { get; set; }

        public void SetGroups(string left, string right, Keyword keyword)
        {
            Keyword = keyword;
            LeftGroup = Conditional.Create(left);
            if (!string.IsNullOrEmpty(right))
            {
                RightGroup = Conditional.Create(right);
            }
        }

        public void Parse(string input, int currentIndex = 0)
        {
            // 'F' == 'F' AND 'B' == 'B'
            // "{{context.dicom.tags[('0010','0040')]}} == 'F' AND {{context.executions.body_part_identifier.result.body_part}} == 'leg'"

            var findAnds = new Regex("(and|AND)");
            var foundAnds = findAnds.Matches(input);
            var findOrs = new Regex("(or|OR)");
            var foundOrs = findOrs.Matches(input);

            if (foundAnds.Count == 1 && foundOrs.Count == 0)
            {
                var splitByAnd = input.Split("AND");
                SetGroups(splitByAnd[0], splitByAnd[1], Keyword.AND);
            }
            if (foundAnds.Count == 0 && foundOrs.Count == 1)
            {
                var splitByOr = input.Split("OR");
                SetGroups(splitByOr[0], splitByOr[1], Keyword.OR);
            }
            if (foundAnds.Count == 0 && foundOrs.Count == 0)
            {
                SetGroups(input, string.Empty, Keyword.SINGULAR);
            }
        }

        public bool Evaluate()
        {
            if (Keyword == Keyword.AND)
            {
                return LeftGroup.Evaluate() && RightGroup.Evaluate();
            }
            if (Keyword == Keyword.OR)
            {
                return LeftGroup.Evaluate() || RightGroup.Evaluate();
            }
            return LeftGroup.Evaluate();
        }


        public static ConditionalGroup Create(string input, int currentIndex = 0)
        {
            var conditionalGroup = new ConditionalGroup();
            conditionalGroup.Parse(input.Trim());

            return conditionalGroup;
        }
    }
}
