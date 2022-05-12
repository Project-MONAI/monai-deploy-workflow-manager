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

        public string LeftParameter { get; set; }

        public string RightParameter { get; set; }

        public ConditionalGroup LeftGroup { get; set; }

        public ConditionalGroup RightGroup { get; set; }

        public void Parse(string input, int currentIndex = 0)
        {
            //input = "'F' == {{context.dicom.tags[('0010','0040')]}}";
            //"AND {{context.dicom.tags[('0010','0040')]}} == 'F'"
            switch (input)
            {
                case "{":
                    var idxClosingBracket = input.IndexOf('}') + 1;
                    LeftParameter = input[currentIndex..idxClosingBracket];
                    break;
                case "'":
                    var idxClosingQuote = input.IndexOf('\'') + 1;
                    RightParameter = input[currentIndex..idxClosingQuote];
                    break;
                case "A":
                case "a":
                    Parse(input, currentIndex + 1);
                    break;
                case "N":
                case "n":
                    var npreviousChar = input[currentIndex - 1];
                    if (npreviousChar == 'a' || npreviousChar == 'A')
                    {
                        Parse(input, currentIndex + 1);
                    }
                    break;
                case "D":
                case "d":
                    var dpreviousChar = input[currentIndex - 1];
                    if (dpreviousChar == 'n' || dpreviousChar == 'N')
                    {
                        Keyword = Keywords.AND;
                    }
                    break;
                default:
                    break;
            }
        }

        public static ConditionalGroup Create(string input, int currentIndex = 0)
        {
            input = "{{context.dicom.tags[('0010','0040')]}} == 'F'";
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
