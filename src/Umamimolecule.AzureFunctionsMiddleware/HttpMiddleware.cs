using System.Threading.Tasks;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public abstract class HttpMiddleware
    {
        public HttpMiddleware Next;

        protected HttpMiddleware(HttpMiddleware next)
        {
            this.Next = next;
        }

        protected HttpMiddleware()
        {
        }

        public abstract Task InvokeAsync(IHttpFunctionContext context);
    }
}
