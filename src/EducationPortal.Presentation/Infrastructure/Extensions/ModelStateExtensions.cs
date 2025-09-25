using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EducationPortal.Presentation;

public static class ModelStateExtensions
{
    public static void ApplyValidationResult(this ModelStateDictionary modelState, ValidationResult result)
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}