
using System.Text.RegularExpressions;

namespace Crud.Application.Util
{
    public static class StringValidatorExtension
    {
        public static bool IsContainXss(this string target)
        {
            return Regex.IsMatch(target, @"<[^>]+>", RegexOptions.IgnoreCase) && // ensure no xss
                                    Regex.IsMatch(target, @"(script|onerror|onload)\s*=", RegexOptions.IgnoreCase);
        }

        public static bool IsValidEmail(this string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
