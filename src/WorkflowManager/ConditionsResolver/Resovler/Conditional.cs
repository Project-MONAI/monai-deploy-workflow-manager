/*
 * Copyright 2021-2022 MONAI Consortium
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

using System.Globalization;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Resolver
{
    public sealed class Conditional
    {
        public const string CONTAINS = " IN ";
        public const string NOT_CONTAINS = " NOT IN ";

        public const char GT = '>';
        public const string GTstr = ">";

        public const char LT = '<';
        public const string LTstr = "<";

        public const char NOT = '!';
        public const char EQUAL = '=';
        public const string EQUAL_EQUALS = "==";
        public const string LE = "<=";
        public const string EL = "=<";
        public const string NOT_EQUAL = "!=";
        public const string GE = ">=";
        public const string EG = "=>";

        public const string NULL = "NULL ";
        public const string UNDEFINED = "UNDEFINED ";


        public const char OPEN_SQUIGGILY_BRACKET = '{';
        public const char OPEN_SQUARE_BRACKET = '[';

        public const char SINGLE_QUOTE = '\'';

        public const string AND = "AND";
        public const string OR = "OR";

        public string LogicalOperator { get; set; } = string.Empty;

        public string LeftParameter { get; set; } = string.Empty;

        public bool LeftParameterIsArray { get; set; } = false;

        public string RightParameter { get; set; } = string.Empty;

        public bool RightParameterIsArray { get; set; } = false;

        public void SetNextParameter(string value, bool isArrayValue = false)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                value = "NULL";
            }

            if (string.IsNullOrEmpty(LeftParameter))
            {
                LeftParameter = value;
                LeftParameterIsArray = isArrayValue;
                return;
            }

            else if (string.IsNullOrEmpty(RightParameter))
            {
                RightParameter = value;
                RightParameterIsArray = isArrayValue;
                return;
            }

            throw new ArgumentException("All parameters set");
        }

        public void Parse(ReadOnlySpan<char> input, int currentIndex = 0)
        {
            if (input.IsEmpty || input.IsWhiteSpace())
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (currentIndex >= input.Length)
            {
                return;
            }

            var isAnd = SliceInput(input, 0, 3) == AND;
            var isOr = SliceInput(input, 0, 2) == OR;

            if (isAnd || isOr)
            {
                throw new ArgumentException($"No left hand parameter at index: {currentIndex}");
            }

            if (input.Trim().ToString().ToUpper().EndsWith(AND) || input.Trim().ToString().ToUpper().EndsWith(OR))
            {
                throw new ArgumentException($"No right hand parameter at index: {currentIndex}");
            }

            var currentChar = input[currentIndex];
            var nextIndex = currentIndex + 1;

            // The bulk of the parsing is here in ParseChar
            currentIndex = ParseChar(input, currentIndex, currentChar, nextIndex);

            if (currentIndex + 1 >= input.Length)
            {
                return;
            }

            ParseSlices(input, currentIndex);

            if (string.IsNullOrWhiteSpace(LeftParameter)
                && string.IsNullOrWhiteSpace(RightParameter)
                && string.IsNullOrWhiteSpace(LogicalOperator))
            {
                throw new ArgumentException($"Unable to parse \"{input}\"");
            }
            Parse(input, currentIndex + 1);
        }

        private void ParseSlices(ReadOnlySpan<char> input, int currentIndex)
        {
            var isIn = false;
            if (input.Length - currentIndex > CONTAINS.Length && currentIndex != 0)
            {
                isIn = SliceInput(input, currentIndex - 1, CONTAINS.Length) == CONTAINS;
            }

            var isNotIn = false;
            if (input.Length - currentIndex > NOT_CONTAINS.Length && currentIndex != 0)
            {
                isNotIn = SliceInput(input, currentIndex - 1, NOT_CONTAINS.Length) == NOT_CONTAINS;
            }

            var isNull = false;
            if (input.Length - currentIndex > NULL.Length)
            {
                isNull = SliceInput(input, currentIndex, NULL.Length)
                    .StartsWith(NULL, StringComparison.InvariantCultureIgnoreCase);
            }

            var isUndefined = false;
            if (input.Length - currentIndex > UNDEFINED.Length)
            {
                isUndefined = SliceInput(input, currentIndex, UNDEFINED.Length)
                    .StartsWith(UNDEFINED, StringComparison.InvariantCultureIgnoreCase);
            }

            if (isNull || isUndefined || input.IsWhiteSpace() || input.IsEmpty)
            {
                SetNextParameter("");
            }

            if (isIn)
            {
                LogicalOperator = CONTAINS;
            }

            if (isNotIn)
            {
                LogicalOperator = NOT_CONTAINS;
            }
        }

        private static string SliceInput(ReadOnlySpan<char> input, int start, int length)
            => input.Slice(start, length).ToString().ToUpper();

        private int ParseChar(ReadOnlySpan<char> input, int currentIndex, char currentChar, int nextIndex)
        {
            switch (currentChar)
            {
                case OPEN_SQUARE_BRACKET:
                    var lengthTillClosingBracket = input.Slice(nextIndex).IndexOf(']');
                    if (lengthTillClosingBracket == -1)
                    {
                        throw new ArgumentException($"Matching closing bracket missing in \"{input}\"");
                    }
                    SetNextParameter(input.Slice(nextIndex, lengthTillClosingBracket).ToString(), true);
                    currentIndex = nextIndex + lengthTillClosingBracket + 1;
                    break;
                case OPEN_SQUIGGILY_BRACKET:
                    var idxClosingBracket = input.Slice(nextIndex).IndexOf('}') + 3;
                    if (idxClosingBracket == -1)
                    {
                        throw new ArgumentException($"Matching closing bracket missing in \"{input}\"");
                    }
                    SetNextParameter(input.Slice(currentIndex, idxClosingBracket).ToString());
                    currentIndex = nextIndex + idxClosingBracket;
                    break;
                case SINGLE_QUOTE:
                    var lengthTillClosingQuote = input.Slice(nextIndex).IndexOf('\'');
                    if (lengthTillClosingQuote == -1)
                    {
                        throw new ArgumentException($"Matching closing quotation mark missing in \"{input}\"");
                    }
                    SetNextParameter(input.Slice(nextIndex, lengthTillClosingQuote).ToString());
                    currentIndex = nextIndex + lengthTillClosingQuote;
                    break;
                case NOT:
                case EQUAL:
                    // this checks for  == or !=
                    var nextChar = input[currentIndex + 1];

                    if (nextChar == '=' || nextChar == '>' || nextChar == '<')
                    {
                        var chars = new char[] { currentChar, nextChar };
                        LogicalOperator = new string(chars);
                        currentIndex += 1;
                    }
                    break;
                case GT:
                case LT:
                    LogicalOperator = currentChar.ToString();
                    break;
                default:
                    break;
            }

            return currentIndex;
        }

        public static Conditional Create(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException(nameof(input));
            }
            var conditionalGroup = new Conditional();
            conditionalGroup.Parse(input.Trim());

            return conditionalGroup;
        }

        public bool Evaluate(IFormatProvider? culture = null)
        {
            culture ??= CultureInfo.InvariantCulture;

            return LogicalOperator switch
            {
                CONTAINS => ContainsEvaluate(),
                NOT_CONTAINS => !ContainsEvaluate(),
                EQUAL_EQUALS => LeftParameter == RightParameter,
                NOT_EQUAL => LeftParameter != RightParameter,
                GTstr => Convert.ToInt16(LeftParameter, culture) > Convert.ToInt16(RightParameter, culture),
                LTstr => Convert.ToInt16(LeftParameter, culture) < Convert.ToInt16(RightParameter, culture),
                GE => Convert.ToInt16(LeftParameter, culture) >= Convert.ToInt16(RightParameter, culture),
                EG => Convert.ToInt16(LeftParameter, culture) >= Convert.ToInt16(RightParameter, culture),
                LE => Convert.ToInt16(LeftParameter, culture) <= Convert.ToInt16(RightParameter, culture),
                EL => Convert.ToInt16(LeftParameter, culture) <= Convert.ToInt16(RightParameter, culture),
                _ => throw new InvalidOperationException("Invalid logical operator between parameters {} and"),
            };
        }

        private bool ContainsEvaluate()
        {
            string[] arr;
            var compare = string.Empty;
            if (LeftParameterIsArray)
            {
                arr = LeftParameter.Split(',');
                compare = RightParameter;
            }
            else
            {
                arr = RightParameter.Split(',');
                compare = LeftParameter;
            }

            if (arr.Any(p =>
                p.Trim().Equals("NULL", StringComparison.InvariantCultureIgnoreCase)
                || p.Trim().Equals("UNDEFINED", StringComparison.InvariantCultureIgnoreCase)))
            {
                for (var i = 0; i < arr.Length; i++)
                {
                    if (arr[i].Trim().Equals("NULL", StringComparison.InvariantCultureIgnoreCase) || arr[i].Equals("UNDEFINED", StringComparison.InvariantCultureIgnoreCase))
                    {
                        arr[i] = "NULL";
                    }
                }
            }

            return arr.Any(p => p.Trim().Trim('\"').Trim('\'').Equals(compare));
        }
    }
}
