using System.ComponentModel.DataAnnotations;

namespace FunctionAppMiddlewarePOC
{
    public class MyGetFunctionQueryParameters
    {
        [Required]
        public string Name { get; set; }
    }
}
