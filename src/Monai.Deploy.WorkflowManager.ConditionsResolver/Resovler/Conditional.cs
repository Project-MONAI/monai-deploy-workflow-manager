namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Resolver
{
    public class Conditional
    {
        public string LogicalOperator { get; set; }

        public string LeftParameter { get; set; }

        public string RightParameter { get; set; }


        public bool Parse(ReadOnlySpan<char> input, int currentIndex = 0)
        {
            //input = "'F' == {{context.dicom.tags[('0010','0040')]}}";
            //"AND {{context.dicom.tags[('0010','0040')]}} == 'F'"
            if (currentIndex >= input.Length)
            {
                return true;
            }

            var isAnd = input.Slice(0, 3).ToString().ToUpper() == "AND";
            var isOr = input.Slice(0, 2).ToString().ToUpper() == "OR";
            if (isAnd || isOr)
            {
                throw new ArgumentException($"No left hand parameter at index: {0}");
            }

            var currentChar = input[currentIndex];

            switch (currentChar)
            {
                case '{':
                    var idxClosingBracket = input.IndexOf('}') + 2;
                    LeftParameter = input.Slice(currentIndex, idxClosingBracket).ToString(); //[currentIndex..idxClosingBracket];
                    currentIndex = idxClosingBracket;
                    break;
                case '\'':
                    var nextIndex = currentIndex + 1;
                    var lengthTillClosingQuote = input.Slice(nextIndex).IndexOf('\'');
                    if (RightParameter is null)
                    {
                        RightParameter = input.Slice(nextIndex, lengthTillClosingQuote).ToString(); //[nextIndex..idxClosingQuote];
                        currentIndex = nextIndex + lengthTillClosingQuote;
                    }
                    break;
                case '!':
                case '=':
                    var nextChar = input[currentIndex + 1];

                    if (nextChar == '=')
                    {
                        LogicalOperator = $"{currentChar}{nextChar}";
                        currentIndex += 1;
                    }
                    break;
                case '<':
                case '>':
                    LogicalOperator = currentChar.ToString();
                    break;
                default:
                    break;
            }
            return Parse(input, currentIndex + 1);
        }

        // TODO: Probably only used by testing do we need to keep it? probably not
        public static Conditional Create(string input, int currentIndex = 0)
        {
            //input = "{{context.dicom.tags[('0010','0040')]}} == 'F'";
            var conditionalGroup = new Conditional();
            conditionalGroup.Parse(input.Trim());

            return conditionalGroup;
        }

        public bool Evaluate()
        {
            switch (LogicalOperator)
            {
                case "==":
                    return LeftParameter == RightParameter;
                case "!=":
                    return LeftParameter != RightParameter;
                case ">":
                    return Convert.ToInt16(LeftParameter) > Convert.ToInt16(RightParameter);
                case "<":
                    return Convert.ToInt16(LeftParameter) < Convert.ToInt16(RightParameter);
                case "=>":
                    return Convert.ToInt16(LeftParameter) >= Convert.ToInt16(RightParameter);
                case "=<":
                    return Convert.ToInt16(LeftParameter) >= Convert.ToInt16(RightParameter);
                default:
                    throw new InvalidOperationException("Invalid logical operator between parameters {} and");
            }
        }
    }
}
