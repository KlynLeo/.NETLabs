using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.Validators.Attributes
{
    public class OrderCategoryAttribute : ValidationAttribute
    {
        private readonly string[] _allowedCategories;

        public OrderCategoryAttribute(params string[] allowedCategories)
        {
            _allowedCategories = allowedCategories;
            ErrorMessage = $"Category must be one of the following: {string.Join(", ", allowedCategories)}.";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            return _allowedCategories.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase);
        }
    }
}