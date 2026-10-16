using System.ComponentModel.DataAnnotations;

namespace Shopping_web.Repositories.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName);
                string[] extensions = { ".jpg", ".jpeg", ".png", ".gif" };
                bool result = extensions.Any(x => extension.EndsWith(x));
                if (!result)
                {
                    return new ValidationResult("Invalid file extension. Allowed extensions are: " + string.Join(", ", extensions));
                }
            }
            return ValidationResult.Success;
        }
    }
}
