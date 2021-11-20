using System.ComponentModel;

namespace Infra.Net.Extensions.Tests.TestEntities;

public enum TestEnum
{
    //TODO Checar o funcionamento do localized description
    //[LocalizedDescription("numberOne", typeof(TestResource))]
    [Description("Number One")]
    FirstItem,
    SecondItem,
    Third_Item,
    [Description("Fourth-Item")]
    FourthItem,
    [Description("Fifth_Item")]
    FifthItem,
    //[LocalizedDescription("CD8090", typeof(TestResource))]
    //SixthItem
}