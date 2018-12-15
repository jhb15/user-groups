using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UserGroupsTest.TestUtils
{
    class ModelValidator
    {
        // Adapted from https://stackoverflow.com/questions/2167811/unit-testing-asp-net-dataannotations-validation
        public static IList<ValidationResult> GetValidation(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }
    }
}
