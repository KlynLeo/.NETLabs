using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace api.Validators.Attributes
{
     public class ValidISBNAttribute : ValidationAttribute, IClientModelValidator
    {
        public ValidISBNAttribute()
        {
            ErrorMessage = "ISBN must be 10 or 13 digits (hyphens and spaces are ignored).";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true; 

            string isbn = value.ToString()!.Replace("-", "").Replace(" ", "");
            return Regex.IsMatch(isbn, @"^\d{10}(\d{3})?$");
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-validisbn", ErrorMessage!);
        }

        private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key)) return false;
            attributes.Add(key, value);
            return true;
        }
    }
}