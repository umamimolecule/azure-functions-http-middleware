using System.ComponentModel.DataAnnotations;

namespace FunctionAppMiddlewarePOC
{
    public class MyPostFunctionBodyParameters
    {
        [Required]
        public string Name { get; set; }
    }
}
