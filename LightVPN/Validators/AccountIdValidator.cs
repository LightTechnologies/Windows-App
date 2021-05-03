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
    public class AccountIdValidator : ValidationRule
    {
        private readonly Regex _regex = new("^[a-zA-Z0-9-]+$", RegexOptions.Compiled);
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
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
