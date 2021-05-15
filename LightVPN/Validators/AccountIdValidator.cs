using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LightVPN.Validators
{
    /// <summary>
    /// Validates an account ID
    /// </summary>
    public class AccountIdValidator : ValidationRule
    {
        /// <summary>
        /// Compiled regex that we match with the input string
        /// </summary>
        private readonly Regex _regex = new("^[a-zA-Z0-9-]+$", RegexOptions.Compiled);
        /// <summary>
        /// Validates the input object
        /// </summary>
        /// <param name="value">The object you wish to validate, it must be a string</param>
        /// <param name="cultureInfo"></param>
        /// <returns>A validation result depending on how the validation went</returns>
        /// <exception cref="ArgumentException"></exception>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is not string) throw new ArgumentException("Input must be a string");

            if (!_regex.IsMatch(value.ToString()))
            {
                return new ValidationResult(false, "Please enter a valid account ID");
            }
            if (value.ToString().Length < 3)
            {
                return new ValidationResult(false, "Account ID must be at least 3 characters");
            }
            return new ValidationResult(true, null);
        }
    }
}