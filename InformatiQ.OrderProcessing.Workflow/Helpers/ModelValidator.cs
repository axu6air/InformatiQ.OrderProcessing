using InformatiQ.OrderProcessing.Workflow.Models;
using System.ComponentModel.DataAnnotations;

namespace InformatiQ.OrderProcessing.Workflow.Helpers
{
    public static class ModelValidator
    {
        public static CustomValidators<T> BuildValidation<T>(T res) where T : BaseModel
        {
            var results = new List<ValidationResult>();
            Dictionary<string, List<string>> myErrors = new();

            var body = new CustomValidators<T>();
            body.Value = res;
            body.IsValid = Validator.TryValidateObject(body.Value, new ValidationContext(body.Value, null, null), results, true);

            foreach (var s in results)
            {
                foreach (var key in s.MemberNames)
                {
                    if (myErrors.TryGetValue(key, out List<string> valueList))
                        valueList.Add(s.ErrorMessage);
                    else
                        myErrors.Add(key, new List<string> { s.ErrorMessage });
                }
            }
            body.ValidationErrors = myErrors.Select(s => new ValidationError { PropertyName = s.Key, ErrorList = s.Value.ToArray() });
            return body;
        }

        public static CustomValidators<T> ValidateRequestAsync<T>(this T model) where T : BaseModel
        {
            return BuildValidation<T>(model);
        }

        public class CustomValidators<T>
        {
            public bool IsValid { get; set; }
            public T Value { get; set; }
            public IEnumerable<ValidationError> ValidationErrors { get; set; }
        }

        public class ValidationError
        {
            public string PropertyName { get; set; }
            public string[] ErrorList { get; set; }
        }
    }
}
