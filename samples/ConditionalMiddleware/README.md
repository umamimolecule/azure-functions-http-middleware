# Sample: Conditional middleware

This example project shows how to conditionally use middleware using the `UseWhen` extension method for a pipeline.

## ConditionalMiddleware

This project contains two functions: `http://localhost:7071/api/Function1` and `http://localhost:7071/api/Function2`.

Function1 will execute middleware A then B, whilst Function2 will only execute middlware B.

The pipeline is set up using the following code, which inspects the inbound request to decide which middleware is invoked:

```
return pipeline.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/Function1"),
                        p => p.Use(middlewareA))
               .Use(middlewareB)
               .Use(func);
```
