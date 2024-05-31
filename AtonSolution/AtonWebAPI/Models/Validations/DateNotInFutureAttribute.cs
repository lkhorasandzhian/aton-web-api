using System;
using System.ComponentModel.DataAnnotations;

namespace AtonWebAPI.Models.Validations
{
	public class DateNotInFutureAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is DateTime date)
			{
				if (date < DateTime.Now)
				{
					return ValidationResult.Success;
				}

				return new ValidationResult("Birthday cannot be in the future");
			}

			return ValidationResult.Success;
		}
	}
}
