using System.Globalization;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Resolver
{
    public class Conditional
    {
        public string LogicalOperator { get; set; } = string.Empty;

        public string LeftParameter { get; set; } = string.Empty;

        public string RightParameter { get; set; } = string.Empty;

        public bool RequiresResolving { get; set; } = false;

        public void SetNextParameter(string value)
        {
            if (string.IsNullOrEmpty(LeftParameter))
            {
                LeftParameter = value;
                return;
            }
            else if (string.IsNullOrEmpty(RightParameter))
            {
                RightParameter = value;
                return;
            }
            throw new ArgumentException("All parameters set");
        }


        public bool Parse(ReadOnlySpan<char> input, int currentIndex = 0)
        {
            if (currentIndex >= input.Length)
            {
                return true;
            }

            var isAnd = input.Slice(0, 3).ToString().ToUpper() == "AND";
            var isOr = input.Slice(0, 2).ToString().ToUpper() == "OR";
            if (isAnd || isOr)
            {
                throw new ArgumentException($"No left hand parameter at index: {currentIndex}");
            }
            if (input.Trim().ToString().ToUpper().EndsWith("AND") || input.Trim().ToString().ToUpper().EndsWith("OR"))
            {
                throw new ArgumentException($"No right hand parameter at index: {currentIndex}");
            }


            var currentChar = input[currentIndex];
            var nextIndex = currentIndex + 1;

            switch (currentChar)
            {
                case '{':
                    var idxClosingBracket = input.Slice(nextIndex).IndexOf('}') + 3;
                    SetNextParameter(input.Slice(currentIndex, idxClosingBracket).ToString());
                    RequiresResolving = true;
                    currentIndex = nextIndex + idxClosingBracket;
                    break;
                case '\'':
                    var lengthTillClosingQuote = input.Slice(nextIndex).IndexOf('\'');
                    SetNextParameter(input.Slice(nextIndex, lengthTillClosingQuote).ToString());
                    currentIndex = nextIndex + lengthTillClosingQuote;
                    break;
                case '!':
                case '=':
                    var nextChar = input[currentIndex + 1];

                    if (nextChar == '=' || nextChar == '>' || nextChar == '<')
                    {
                        var chars = new char[] { currentChar, nextChar };
                        LogicalOperator = new string(chars);
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

        public static Conditional Create(string input, int currentIndex = 0)
        {
            var conditionalGroup = new Conditional();
            conditionalGroup.Parse(input.Trim());

            return conditionalGroup;
        }

        public bool Evaluate(IFormatProvider? culture = null)
        {
            if (culture == null)
            {
                culture = CultureInfo.InvariantCulture;
            }
            if (RequiresResolving)
            {
                ResolveParameters();
            }
            switch (LogicalOperator)
            {
                case "==":
                    return LeftParameter == RightParameter;
                case "!=":
                    return LeftParameter != RightParameter;
                case ">":
                    return Convert.ToInt16(LeftParameter, culture) > Convert.ToInt16(RightParameter, culture);
                case "<":
                    return Convert.ToInt16(LeftParameter, culture) < Convert.ToInt16(RightParameter, culture);
                case "=>":
                    return Convert.ToInt16(LeftParameter, culture) >= Convert.ToInt16(RightParameter, culture);
                case "=<":
                    return Convert.ToInt16(LeftParameter, culture) >= Convert.ToInt16(RightParameter, culture);
                default:
                    throw new InvalidOperationException("Invalid logical operator between parameters {} and");
            }
        }

        private void ResolveParameters() => throw new NotImplementedException();
    }
}
