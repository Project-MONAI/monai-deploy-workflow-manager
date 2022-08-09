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
using Ardalis.GuardClauses;

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
            Guard.Against.NullOrWhiteSpace(value, nameof(value));

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
            Parse(input, currentIndex + 1);
        }

        public static Conditional Create(string input, int currentIndex = 0)
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
