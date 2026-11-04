using System.ComponentModel.DataAnnotations;

namespace BookHaven.API.Validation;

public class Isbn13Attribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is null || value is string isbnValue && string.IsNullOrEmpty(isbnValue))
        {
            return true; // Allow null or empty values, use [Required] for mandatory fields
        }
        if (value is not string isbn)
        {
            return false;
        }
        if (isbn.Length != 13)
        {
            return false;
        }
        if (!isbn.StartsWith("978") && !isbn.StartsWith("979"))
        {
            return false;
        }
        if (!isbn.All(char.IsAsciiDigit))
        {
            return false;
        }

        int sum = 0;

        for (int i = 0; i < 12; i++)
        {
            int digit = isbn[i] - '0';
            sum += digit * (i % 2 == 0 ? 1 : 3);
        }

        int checkDigit = (10 - (sum % 10)) % 10;

        return checkDigit == (isbn[12] - '0');
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be a valid ISBN-13 number starting with 978 or 979.";
    }
}
