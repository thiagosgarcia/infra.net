using System.Collections.Generic;
using Infra.Net.Extensions.Extensions;
using Infra.Net.Extensions.Helpers.Enum;
using Infra.Net.Extensions.Tests.TestEntities;
using Xunit;
using Xunit2.Should;

namespace Infra.Net.Extensions.Tests.Extensions
{
    public class EnumExtensionsTests
    {
        [Theory]
        [InlineData("Number One", TestEnum.FirstItem)]
        [InlineData("SecondItem", TestEnum.SecondItem)]
        [InlineData("Third_Item", TestEnum.Third_Item)]
        [InlineData("Fourth-Item", TestEnum.FourthItem)]
        [InlineData("Fifth_Item", TestEnum.FifthItem)]
        public void MustGetDescription(string expected, TestEnum value)
        {
            value.GetDescription().ShouldBe(expected);
        }

        [Fact]
        public void MustReturnListWithDescription()
        {
            EnumHelper.EnumToList<TestEnum>().ShouldBe(new List<string>()
            {
                "Number One",
                "SecondItem", 
                "Third_Item", 
                "Fourth-Item",
                "Fifth_Item"
            });
        }

        [Fact]
        public void MustReturnListWithToString()
        {
            EnumHelper.EnumToList<TestEnum>(false).ShouldBe(new List<string>()
            {
                "FirstItem",
                "SecondItem", 
                "Third_Item",
                "FourthItem",
                "FifthItem"
            });
        }

    }
}