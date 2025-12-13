using System.Text.RegularExpressions;

namespace OzshBot.Infrastructure;

public class PhoneParser
{
    public static List<string> ExtractAllPhones(string input)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(input))
            return result;
        
        var matches = Regex.Matches(input, @"[+\d][\d\-\(\)\s]{8,20}");

        foreach (Match m in matches)
        {
            try
            {
                var phone = NormalizePhone(m.Value);
                result.Add(phone);
            }
            catch (ArgumentException)
            {
                continue;
            }
        }
        return result;
    }
    
    public static string NormalizePhone(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) 
            throw new ArgumentException(nameof(input));
        
        string digits = Regex.Replace(input, @"\D", "");
        
        if (digits.Length == 11 && (digits[0] == '8' || digits[0] == '7'))
        {
            digits = digits.Substring(1);
            return "+7" + digits;
        }

        throw new ArgumentException("Invalid phone number");
    }
}