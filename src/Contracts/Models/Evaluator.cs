// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Text.RegularExpressions;
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
        public Keywords Keyword { get; set; }

        public Conditional LeftGroup { get; set; }

        public Conditional RightGroup { get; set; }

        public void SetGroups(string left, string right, Keywords keyword)
        {
            LeftGroup = Conditional.Create(left);
            RightGroup = Conditional.Create(right);
            Keyword = keyword;
        }

        public void Parse(string input, int currentIndex = 0)
        {
            // "{{context.dicom.tags[('0010','0040')]}} == 'F' AND {{context.executions.body_part_identifier.result.body_part}} == 'leg'"

            var findAnds = new Regex("(and|AND)");
            var foundAnds = findAnds.Matches(input);
            var findOrs = new Regex("(or|OR)");
            var foundOrs = findOrs.Matches(input);

            if (foundAnds.Count == 1 && foundOrs.Count == 0)
            {
                var splitByAnd = input.Split("AND");
                SetGroups(splitByAnd[0], splitByAnd[1], Keywords.AND);
            }
            if (foundAnds.Count == 0 && foundOrs.Count == 1)
            {
                var splitByOr = input.Split("OR");
                SetGroups(splitByOr[0], splitByOr[1], Keywords.OR);
            }
            if (foundAnds.Count == 0 && foundOrs.Count == 0)
            {
                var splitByOr = input.Split("OR");
                SetGroups(splitByOr[0], splitByOr[1], Keywords.SINGULAR);
            }

        }

        public static ConditionalGroup Create(string input, int currentIndex = 0)
        {
            var conditionalGroup = new ConditionalGroup();
            conditionalGroup.Parse(input.Trim());

            return conditionalGroup;
        }
    }


    public class Conditional
    {
        //private readonly List<string> _logicalOperators = new List<string> { "==", "!=", ">", "<", "=>", "=<" };


        public string LogicalOperator { get; set; }

        public string LeftParameter { get; set; }

        public string RightParameter { get; set; }


        public bool Parse(string input, int currentIndex = 0)
        {
            //input = "'F' == {{context.dicom.tags[('0010','0040')]}}";
            //"AND {{context.dicom.tags[('0010','0040')]}} == 'F'"
            if (currentIndex >= input.Length)
            {
                return true;
            }

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
                    var nextIndex = currentIndex + 1;
                    var idxClosingQuote = input.IndexOf('\'', nextIndex);
                    if (RightParameter is null)
                    {
                        RightParameter = input[nextIndex..idxClosingQuote];
                        currentIndex = idxClosingQuote;
                    }
                    break;
                case '!':
                case '=':
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
                //case 'A':
                //case 'a':
                //    if (string.IsNullOrEmpty(LeftParameter))
                //    {
                //        throw new ArgumentException($"No left hand parameter at index: {currentIndex}");
                //    }
                //    return Parse(input, currentIndex + 1);
                //    break;
                //case 'N':
                //case 'n':
                //    if (previousChar == 'a' || previousChar == 'A')
                //    {
                //        return Parse(input, currentIndex + 1);
                //    }
                //    break;
                //case 'D':
                //case 'd':
                //    if (previousChar == 'n' || previousChar == 'N')
                //    {
                //        Keyword = Keywords.AND;
                //    }
                //    break;
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
            return false;
        }
    }
    /// <summary>
    /// Group Keywords or Operators
    /// </summary>
    public enum Keywords
    {
        SINGULAR,
        AND,
        OR,
    }
}
