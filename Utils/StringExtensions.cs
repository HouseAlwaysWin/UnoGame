// Utils/StringExtensions.cs
namespace GodotHelper
{
    public static class StringExtensions
    {
        public static string ToLowerCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s) || char.IsLower(s[0]))
            {
                return s;
            }
            if (s.Length == 1)
            {
                return s.ToLowerInvariant();
            }
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }
    }
}
