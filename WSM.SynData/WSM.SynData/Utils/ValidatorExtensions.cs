using System.Linq;
using System.Text.RegularExpressions;

namespace WSM.SynData.Utils
{
    public static class ValidatorExtensions
    {
        public static bool IsValidEmailAddress(this string value)
        {
            var regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(value);
        }

        public static bool IsNumeric(this string value)
        {
            var regex = new Regex(@"^[0-9]+$");
            return regex.IsMatch(value);
        }

        public static bool IsValidUrl(this string value)
        {
            var regex = new Regex(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$");
            return regex.IsMatch(value);
        }

        public static bool IsValidIPv4(this string ipString)
        {
            if (string.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }
    }
}
