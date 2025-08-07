using AbyssIrc.Core.Utils;

namespace AbyssIrc.Core.Extensions;

public static class StringMethodExtension
{
    public static string ToSnakeCase(this string text)
    {
        return StringUtils.ToSnakeCase(text);
    }

    public static string ToSnakeCaseUpper(this string text)
    {
        return StringUtils.ToSnakeCase(text).ToUpper();
    }

    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0]))
        {
            return str;
        }

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}
