using Monai.Deploy.WorkflowManager.ConditionsResolver.Resolver;
using Xunit;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Tests.Resolver
{
    public class ConditionalGroupTests
    {
        //TODO: Multiple AND/OR Keywords
        //TODO: Error Conditions AND/OR Keywords
        //TODO: Test Operators ('==') AND/OR Keywords
        //TODO: Parse Parameters
        //TODO: Test reversed parameters 'F' on left and {{}} on right

        [Theory]
        [InlineData("{{context.dicom.tags[('0010','0040')]}} == 'F' AND {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData("{{context.dicom.tags[('0010','0040')]}} == 'F' OR {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        public void ConditionalGroup_Creates_HasLeftAndRightGroups(string input)
        {
            var conditionalGroup = ConditionalGroup.Create(input);
            Assert.NotNull(conditionalGroup.LeftGroup);
            Assert.NotNull(conditionalGroup.RightGroup);
        }

        [Theory]
        [InlineData(
            "{{context.dicom.tags[('0010','0040')]}}",
            "F",
            "{{context.executions.body_part_identifier.result.body_part}}",
            "leg",
            "{{context.dicom.tags[('0010','0040')]}} == 'F' AND {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData(
            "{{context.dicom.tags[('0010','0040')]}}",
            "F",
            "{{context.executions.body_part_identifier.result.body_part}}",
            "leg",
            "{{context.dicom.tags[('0010','0040')]}} == 'F' OR {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        public void ConditionalGroup_Creates_HasLeftAndRightGroupsWithValues(string leftGroupLeftParam,
                                                                             string leftGroupRightParam,
                                                                             string rightGroupLeftParam,
                                                                             string rightGroupRightParam,
                                                                             string input)
        {
            var conditionalGroup = ConditionalGroup.Create(input);
            Assert.NotNull(conditionalGroup.LeftGroup);
            Assert.NotNull(conditionalGroup.RightGroup);

            Assert.Equal(leftGroupLeftParam, conditionalGroup.LeftGroup.LeftParameter);
            Assert.Equal(leftGroupRightParam, conditionalGroup.LeftGroup.RightParameter);
            Assert.Equal(rightGroupLeftParam, conditionalGroup.RightGroup.LeftParameter);
            Assert.Equal(rightGroupRightParam, conditionalGroup.RightGroup.RightParameter);
        }

        //[InlineData(false, "AND {{context.dicom.tags[('0010','0040')]}} == 'F'")]
        //[InlineData(true, "{{context.dicom.tags[('0010','0040')]}} == 'F'")]
        ////[InlineData(-4, -6, -10)]
        ////[InlineData(-2, 2, 0)]
        ////[InlineData(int.MinValue, -1, int.MaxValue)]
        //{
        //    var conditional = ConditionalGroup.Create(input);
        //    var result = conditional.Evaluate();
        //    Assert.Equal(expected, result);
        //}
    }
}
