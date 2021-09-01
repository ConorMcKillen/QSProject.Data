using System;
using System.Net;
using System.ComponentModel.DataAnnotations;

namespace QSProject.Data.Validators
{
    // Custom validator - can be used via [UrlResource] on model attribute

    public class UrlResource : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string _url = (string)value; // url property being validated should be a string

            if (UrlResourceExists(_url))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Url resource does not exist");

        }

        // verify that url points to a valid resource
        private bool UrlResourceExists(string url)
        {
            if (url == null)
            {
                return true;
            }
            // method HEAD verifies resource existence
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Method = "HEAD";
            try
            {
                webRequest.GetResponse();
                return true; // got here so valid
            }
            catch
            {
                return false; // exception thrown so invalid
            }

        }

    }
}
