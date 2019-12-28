# Sample: Pipeline branching

This example project shows how to conditionally use middleware using the `UseWhen` extension method for a pipeline.

## PipelineBranching

This project contains two functions: `http://localhost:7071/api/Function1` and `http://localhost:7071/api/Function2`.

Each function will execute a different pipeline branch, which sets a response header `x-middleware-a` or `x-middleware-b`.

The pipeline is set up using the following code, which inspects the inbound request to decide which middleware is invoked:

```
pipeline.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/Function1"),
                 p => p.Use(middlewareA)
                       .Use(func));
        .Use(middlewareB))
        .Use(func);
```