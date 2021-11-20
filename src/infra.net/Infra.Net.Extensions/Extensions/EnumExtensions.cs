
namespace Infra.Net.Extensions.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        return EnumHelper.GetDescription(value);
    }
}