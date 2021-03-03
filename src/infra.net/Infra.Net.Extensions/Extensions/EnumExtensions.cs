using System;
using Infra.Net.Extensions.Helpers.Enum;

namespace Infra.Net.Extensions.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            return EnumHelper.GetDescription(value);
        }
    }
}