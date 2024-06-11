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

using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Extensions;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Resovler
{
    public class ConditionalGroup
    {
        public Keyword Keyword { get; set; } = Keyword.Singular;

        public Conditional? LeftConditional { get; set; } = null;

        public Conditional? RightConditional { get; set; } = null;

        public ConditionalGroup? LeftGroup { get; set; } = null;

        public ConditionalGroup? RightGroup { get; set; } = null;

        public bool LeftIsSet => LeftGroup is not null || LeftConditional is not null;

        public bool RightIsSet => RightGroup is not null || RightConditional is not null;

        public int GroupedLogical { get; set; } = 1;

        public Regex FindAnds { get; } = new(@"([\s]and[\s]|[\s]AND[\s]|[\s]And[\s])", RegexOptions.None, matchTimeout: TimeSpan.FromSeconds(2));

        public Regex FindOrs { get; } = new(@"([\s]or[\s]|[\s]OR[\s]|[\s]Or[\s])", RegexOptions.None, matchTimeout: TimeSpan.FromSeconds(2));

        public Regex FindBrackets { get; } = new(@"((?<!\[)\()", RegexOptions.None, matchTimeout: TimeSpan.FromSeconds(2));

        public Regex FindCloseBrackets { get; } = new(@"((?<!\[)\))", RegexOptions.None, matchTimeout: TimeSpan.FromSeconds(2));

        private string[] ParseOrs(string input) => FindOrs.SplitOnce(input);

        private string[] ParseAnds(string input) => FindAnds.SplitOnce(input);

        public void Set(string left, string right, Keyword? keyword)
        {
            if (keyword is not null)
            {
                Keyword = (Keyword)keyword;
            }
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
            ArgumentNullException.ThrowIfNullOrEmpty(input, nameof(input));

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
                Set(splitByAnd[0], splitByAnd[1].TrimStartExt("AND"), Keyword.And);
                return;
            }
            if (foundAnds.Count == 0 && foundOrs.Count == 1)
            {
                var splitByOr = ParseOrs(input);
                Set(splitByOr[0], splitByOr[1].TrimStartExt("OR"), Keyword.Or);
                return;
            }
            if (foundAnds.Count == 0 && foundOrs.Count == 0)
            {
                Set(input, string.Empty, Keyword.Singular);
                return;
            }

            ParseComplex(input);
        }

        public void ParseBrackets(string input)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(input, nameof(input));

            var foundAnds = FindAnds.Matches(input);
            var foundOrs = FindOrs.Matches(input);

            var foundBrackets = FindBrackets.Match(input);
            var indexOfClosingBracket = FindCloseBrackets.Match(input).Index;

            if (!foundBrackets.Success)
            {
                throw new InvalidOperationException("Expected Bracket: Bracket not found");
            }
            var startingBracketHasBeenTrimmed = indexOfClosingBracket < foundBrackets.Index;
            // if first index of any ANDs or ORs are before the first bracket then left hand evaluation should be processed
            if (!startingBracketHasBeenTrimmed && (foundAnds.Any() && foundAnds.First().Index < foundBrackets.Index) || (foundOrs.Any() && foundOrs.First().Index < foundBrackets.Index))
            {
                if (!foundOrs.Any() || foundAnds.Any() && foundAnds.First().Index < foundOrs.First().Index)
                {
                    var splitByAnds = ParseAnds(input);
                    var rightAnd = splitByAnds[1].TrimStartExt("AND");
                    Set(splitByAnds[0], rightAnd, Keyword.And);
                }
                else if (!foundAnds.Any() && foundOrs.Any() || foundOrs.First().Index < foundAnds.First().Index)
                {
                    var splitByOrs = ParseOrs(input);
                    var rightOrs = splitByOrs[1].TrimStartExt("OR");
                    Set(splitByOrs[0], rightOrs, Keyword.Or);
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
            ArgumentNullException.ThrowIfNullOrWhiteSpace(input, nameof(input));

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
                Set(splitByOr[0], splitByOr[1].TrimStartExt("OR"), Keyword.Or);
            }
            else
            {
                var splitByAnd = ParseAnds(input);
                if (splitByAnd[0] == string.Empty || splitByAnd[1] == string.Empty)
                {
                    throw new ArgumentException($"Error parsing OR condition in: {input}");
                }
                Set(splitByAnd[0], splitByAnd[1].TrimStartExt("AND"), Keyword.And);
            }
        }

        public bool Evaluate()
        {
            if (Keyword == Keyword.And)
            {
                return EvaluateAnds();
            }
            if (Keyword == Keyword.Or)
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
                return EvaluteOrsLogicalGroups();
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

        private bool EvaluteOrsLogicalGroups()
        {
            if (LeftConditional is not null && RightGroup is not null)
            {
                return RightGroup.Evaluate() || LeftConditional.Evaluate();
            }
            if (LeftGroup is not null && RightGroup is not null)
            {
                return RightGroup.Evaluate() || LeftGroup.Evaluate();
            }
            return false;
        }

        private bool EvaluateAnds()
        {
            if (RightGroup is not null && RightGroup.GroupedLogical > GroupedLogical)
            {
                return EvaluteAndsLogicalGroups();
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

        private bool EvaluteAndsLogicalGroups()
        {
            if (LeftConditional is not null && RightGroup is not null)
            {
                return RightGroup.Evaluate() && LeftConditional.Evaluate();
            }
            if (LeftGroup is not null && RightGroup is not null)
            {
                return RightGroup.Evaluate() && LeftGroup.Evaluate();
            }
            return false;
        }

        public static ConditionalGroup Create(string input, int groupedLogicalParent = 0)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(input, nameof(input));
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
