// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class Evaluator
    {
        [JsonProperty(PropertyName = "correlation_id")]
        public string CorrelationId { get; set; }

        [JsonProperty(PropertyName = "input")]
        public Artifact Input { get; set; }

        [JsonProperty(PropertyName = "executions")]
        public ExecutionContext Executions { get; set; }

        [JsonProperty(PropertyName = "dicom")]
        public ExecutionContext Dicom { get; set; }

        public static Evaluator Parse(string input)
        {
            ReadOnlySpan<char> charSpan = input.Trim().ToCharArray();

            return new Evaluator();
        }
    }

    public class ConditionalGroup
    {
        private readonly List<string> _logicalOperators = new List<string> { "==", "!=", ">", "<", "=>", "=<" };

        Keywords Keyword { get; set; }

        public int LogicalOperator { get; set; }

        public string LeftParameter { get; set; }

        public string RightParameter { get; set; }

        public ConditionalGroup LeftGroup { get; set; }

        public ConditionalGroup RightGroup { get; set; }

        public bool Parse(string input, int currentIndex = 0)
        {
            //input = "'F' == {{context.dicom.tags[('0010','0040')]}}";
            //"AND {{context.dicom.tags[('0010','0040')]}} == 'F'"
            var currentChar = input[currentIndex];
            char? previousChar = null;
            char? nextChar = null;

            if (currentIndex != 0)
            {
                previousChar = input[currentIndex - 1];
            }
            if (currentIndex < input.Length - 1)
            {
                nextChar = input[currentIndex + 1];
            }

            switch (currentChar)
            {
                case '{':
                    var idxClosingBracket = input.IndexOf('}') + 2;
                    LeftParameter = input[currentIndex..idxClosingBracket];
                    currentIndex = idxClosingBracket;
                    break;
                case '\'':
                    var idxClosingQuote = input.IndexOf('\'', currentIndex) + 1;
                    if (RightParameter is null)
                    {
                        RightParameter = nextChar.ToString();
                    }
                    currentIndex = idxClosingQuote;
                    break;
                case '!':
                case '=':
                    if (nextChar == '=')
                    {
                        LogicalOperator = _logicalOperators.IndexOf($"{currentChar}{nextChar}");
                        currentIndex += 1;
                    }
                    break;
                case '<':
                case '>':
                    LogicalOperator = _logicalOperators.IndexOf(currentChar.ToString());
                    break;
                case 'A':
                case 'a':
                    return Parse(input, currentIndex + 1);
                    break;
                case 'N':
                case 'n':
                    if (previousChar == 'a' || previousChar == 'A')
                    {
                        return Parse(input, currentIndex + 1);
                    }
                    break;
                case 'D':
                case 'd':
                    if (previousChar == 'n' || previousChar == 'N')
                    {
                        Keyword = Keywords.AND;
                    }
                    break;
                default:
                    break;
            }
            if (currentIndex == input.Length)
            {
                return true;
            }
            return Parse(input, currentIndex + 1);
        }

        public static ConditionalGroup Create(string input, int currentIndex = 0)
        {
            //input = "{{context.dicom.tags[('0010','0040')]}} == 'F'";
            var conditionalGroup = new ConditionalGroup();
            conditionalGroup.Parse(input);

            return conditionalGroup;

        }

        public bool Evaluate()
        {
            return false;
        }
    }
    /// <summary>
    /// Group Keywords or Operators
    /// </summary>
    enum Keywords
    {
        AND,
        OR,
    }
}
