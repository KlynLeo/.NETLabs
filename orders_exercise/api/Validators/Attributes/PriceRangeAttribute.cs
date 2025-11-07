using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace api.Validators.Attributes
{
    public class PriceRangeAttribute : ValidationAttribute
    {
        private readonly decimal _min;
        private readonly decimal _max;

        public PriceRangeAttribute(double min, double max)
        {
            _min = Convert.ToDecimal(min);
            _max = Convert.ToDecimal(max);
            ErrorMessage = $"Price must be between {_min.ToString("C", CultureInfo.InvariantCulture)} and {_max.ToString("C", CultureInfo.InvariantCulture)}.";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;

            if (decimal.TryParse(value.ToString(), out decimal price))
            {
                return price >= _min && price <= _max;
            }

            return false;
        }
    }
}