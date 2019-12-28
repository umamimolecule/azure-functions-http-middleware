# Sample: Conditional branching

This example project shows how to conditionally use middleware using the `UseWhen` extension method for a pipeline.

## ConditionalBranching

This project contains two functions: `http://localhost:7071/api/Function1` and `http://localhost:7071/api/Function2`.

Each function will execute different middleware, which sets a response header `x-middleware-a` or `x-middleware-b`.

The pipeline is set up using the following code, which inspects the inbound request to decide which middleware is invoked:

```
return pipeline.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/Function1"),
                        p => p.Use(middlewareA))
               .UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/Function2"),
                        p => p.Use(middlewareB))
               .Use(func);
```