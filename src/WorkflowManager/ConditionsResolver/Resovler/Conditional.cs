/*
 * Copyright 2022 MONAI Consortium
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
using System.Text.RegularExpressions;

// ReSharper disable InconsistentNaming

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Resovler
{
    public sealed class Conditional
    {
        public const string CONTAINS = "CONTAINS";
        public const string NOT_CONTAINS = "NOT_CONTAINS";

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

        public const string NULL = "NULL";
        public const string UNDEFINED = "UNDEFINED";

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
                value = NULL;
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

        public int Parse(ReadOnlySpan<char> input, int currentIndex = 0)
        {
            var originalInput = input;
            if (currentIndex == 0)
            {
                var pattern = @"(?i:\bnull\b|''|""""|\bundefined\b)";
                var replace = NULL;
                input = Regex.Replace(input.ToString(), pattern, replace, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
            }

            if (input.IsEmpty || input.IsWhiteSpace())
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (currentIndex >= input.Length)
            {
                return currentIndex;
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

            // bulk of complexity is here.
            currentIndex = ParseChars(input, currentIndex, currentChar, nextIndex);

            if (currentIndex + 1 >= input.Length)
            {
                return currentIndex;
            }

            var lengthTillEnd = input.Length - currentIndex;
            currentIndex = ParseExtendedOperators(
                SliceInput(input, currentIndex, lengthTillEnd), currentIndex);

            if (string.IsNullOrWhiteSpace(LeftParameter)
                && string.IsNullOrWhiteSpace(RightParameter)
                && string.IsNullOrWhiteSpace(LogicalOperator))
            {
                throw new ArgumentException($"Unable to parse \"{originalInput}\"");
            }
            return Parse(input, currentIndex + 1);
        }

        private int ParseChars(ReadOnlySpan<char> input, int currentIndex, char currentChar, int nextIndex)
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
                    currentIndex = Parse(input, nextIndex + lengthTillClosingBracket + 1);
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
                    if (lengthTillClosingQuote == 0)
                    {
                        SetNextParameter("");
                    }
                    else
                    {
                        SetNextParameter(input.Slice(nextIndex, lengthTillClosingQuote).ToString());
                    }
                    currentIndex = nextIndex + lengthTillClosingQuote;
                    break;

                case NOT:
                case EQUAL:
                case GT:
                case LT:
                    // this checks for  ==, !=, =>, =<, <=, >=
                    var nextChar = input[currentIndex + 1];

                    if (nextChar == '=' || nextChar == '>' || nextChar == '<')
                    {
                        var chars = new char[] { currentChar, nextChar };
                        LogicalOperator = new string(chars);
                        currentIndex += 1;
                    }
                    else if (currentChar == GT || currentChar == LT)
                    {
                        LogicalOperator = currentChar.ToString();
                    }
                    break;

                default:
                    break;
            }

            return currentIndex;
        }

        private int ParseExtendedOperators(ReadOnlySpan<char> input, int currentIndex)
        {
            var currentWord = Regex.Match(input.ToString(), @"\'\w+\'|^\w+", new RegexOptions(), TimeSpan.FromSeconds(1)).Value;

            if (currentWord.ToUpper() == CONTAINS && currentIndex != 0)
            {
                LogicalOperator = CONTAINS;
                return currentIndex + currentWord.Length - 1;
            }

            if (currentWord.ToUpper() == NOT_CONTAINS && currentIndex != 0)
            {
                LogicalOperator = NOT_CONTAINS;
                return currentIndex + currentWord.Length - 1;
            }

            if (currentWord.ToUpper() == NULL)
            {
                SetNextParameter("");
                return currentIndex + currentWord.Length - 1;
            }

            return currentIndex;
        }

        private static string SliceInput(ReadOnlySpan<char> input, int start, int length)
            => input.Slice(start, length).ToString().ToUpper();

        public static Conditional Create(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException(nameof(input));
            }
            var conditionalGroup = new Conditional();
            conditionalGroup.Parse(input.Trim());

            switch (conditionalGroup.LogicalOperator)
            {
                case GTstr:
                case LTstr:
                case GE:
                case LE:
                case EG:
                case EL:
                    if ((decimal.TryParse(conditionalGroup.LeftParameter, out _)
                        || decimal.TryParse(conditionalGroup.RightParameter, out _)) is false)
                    {
                        throw new ArgumentException($"Invalid numeric value in: {input}");
                    }
                    break;
            }

            return conditionalGroup;
        }

        public bool Evaluate(IFormatProvider? culture = null)
        {
            culture ??= CultureInfo.InvariantCulture;

            return LogicalOperator switch
            {
                CONTAINS => ContainsEvaluate(),
                NOT_CONTAINS => !ContainsEvaluate(),
                EQUAL_EQUALS => string.Equals(LeftParameter.Trim(), RightParameter.Trim(), StringComparison.InvariantCultureIgnoreCase),
                NOT_EQUAL => !string.Equals(LeftParameter.Trim(), RightParameter.Trim(), StringComparison.InvariantCultureIgnoreCase),
                GTstr => Convert.ToDecimal(LeftParameter, culture) > Convert.ToDecimal(RightParameter, culture),
                LTstr => Convert.ToDecimal(LeftParameter, culture) < Convert.ToDecimal(RightParameter, culture),
                GE => Convert.ToDecimal(LeftParameter, culture) >= Convert.ToDecimal(RightParameter, culture),
                EG => Convert.ToDecimal(LeftParameter, culture) >= Convert.ToDecimal(RightParameter, culture),
                LE => Convert.ToDecimal(LeftParameter, culture) <= Convert.ToDecimal(RightParameter, culture),
                EL => Convert.ToDecimal(LeftParameter, culture) <= Convert.ToDecimal(RightParameter, culture),
                _ => throw new InvalidOperationException("Invalid logical operator between parameters {} and"),
            };
        }

        private bool ContainsEvaluate()
        {
            if (LeftParameterIsArray && RightParameterIsArray)
            {
                var arr1 = LeftParameter.Split(',').Select(i => CleanString(i)).ToArray();
                var arr2 = RightParameter.Split(',').Select(i => CleanString(i)).ToArray();
                MakeNullsUpperCase(arr1);
                MakeNullsUpperCase(arr2);
                var result = arr1.Any(item => arr2.Any(p => p.Equals(item)));
                return result;
            }

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

            MakeNullsUpperCase(arr);

            return arr.Any(p => CleanString(p).Equals(compare));
        }

        private static string CleanString(string p) => p.Trim().Trim('\"').Trim('\'').Trim('�').Trim('�');

        private static bool EqualsNullOrDefined(string str) =>
            str.Trim().Equals(NULL, StringComparison.InvariantCultureIgnoreCase)
            || str.Trim().Equals(UNDEFINED, StringComparison.InvariantCultureIgnoreCase);

        private static void MakeNullsUpperCase(string[] arr)
        {
            if (arr.Any(p => EqualsNullOrDefined(p)))
            {
                for (var i = 0; i < arr.Length; i++)
                {
                    if (EqualsNullOrDefined(arr[i]))
                    {
                        arr[i] = NULL;
                    }
                }
            }
        }
    }
}
