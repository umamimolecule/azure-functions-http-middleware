# Sample: Model validation

This example project shows how to configure the built-in middleware to perform model validation for query parameters and body payload.

It shows the default response sent when validation is unsuccessful, and also how to customise the response.

### Table of contents
 - [Body validation](#bodyvalidation)
     - [Default validation failure response](#defaultbodyvalidationresponse)
     - [Custom validation failure response](#custombodyvalidationresponse)
 - [Query validation](#queryvalidation)
     - [Default validation failure response](#defaultqueryvalidationresponse)
     - [Custom validation failure response](#customqueryvalidationresponse)


<a name="bodyvalidation" />

## Body validation

<a name="defaultbodyvalidationresponse" />

### Default validation failure response

Submit a POST request with the following JSON payload to the endpoint `http://localhost:7071/api/BodyValidation`:

```
{
  "name": "A",
  "user": {
    "id": 1,
    "name": "Fred"
  }
}
```

This will return 200 OK and send back the received body:

```
{
  "correlationId": "...",
  "message": "OK",
  "body": {
    "name": "Test",
    "user": {
       "id": 1,
       "name": "Fred",
       "description": null
     }
  }
}
```

Now try the following payload:

```
{
  "name": null,
  "user": {
    "id": 1,
    "name": "Fred"
  }
}
```

and observe it returns 400 Bad Request with a description of the error:

```
{
  "correlationId": "...",
  "error": {
    "code": "INVALID_BODY",
    "message": "The Name field is required."
  }
}
```

This is the default validation failure response structure.

Note that body validation can handle a complex nested object.  Submit the following request:

```
{
  "name": "Test",
  "user": {
    "id": 1
  }
}
```

and the response indicates a nested child property is invalid:

```
{
  "correlationId": "...",
  "error": {
    "code": "INVALID_BODY",
    "message": "User.Name: The Name field is required."
  }
}
```

<a name="custombodyvalidationresponse" />

### Custom validation failure response
Custom responses can returned when body payload validation is unsuccessful by passing in a function to the `IMiddlewarePipeline.UseBodyValidation` extension method:

```
Func<HttpContext, ModelValidationResult, IActionResult> handleValidationFailure
```

This function takes in two arguments:
- `HttpContext` - The HTTP context for the current request
- `ModelValidationResult` - The result of the validation

and returns an `IActionResult` containing the response to be returned to the caller.

To see this in action in this sample, Submit a POST request with the following JSON payload to the endpoint `http://localhost:7071/api/BodyValidationWithCustomResponse`:


```
{
  "name": null,
  "user": {
    "id": 1,
    "name": "Fred"
  }
}
```

and observe it returns 400 Bad Request with a description of the error:

```
{
  "customResponse": {
    "errorMessage": "Name: The Name field is required."
  }
}
```

<a name="queryvalidation" />

## Query validation

<a name="defaultqueryvalidationresponse" />

##

Submit a GET request to `http://localhost:7071/api/QueryValidation?name=Fred`:

This will return 200 OK and send back the received body:

```
{
  "correlationId": "...",
  "message": "OK",
  "query": {
    "name": "Fred",
    "description": null
  }
}
```

Now try the following request `http://localhost:7071/api/QueryValidation?name2=Fred`

and observe it returns 400 Bad Request with a description of the error:

```
{
  "correlationId": "...",
  "error": {
    "code": "INVALID_QUERY_PARAMETERS",
    "message": "The Name field is required."
  }
}
```

<a name="customqueryvalidationresponse" />

### Custom validation failure response
Custom responses can returned when query parameter validation is unsuccessful by  by passing in a function to the `IMiddlewarePipeline.UseQueryValidation` extension method:

```
Func<HttpContext, ModelValidationResult, IActionResult> handleValidationFailure
```

This function takes in two arguments:
- `HttpContext` - The HTTP context for the current request
- `ModelValidationResult` - The result of the validation

and returns an `IActionResult` containing the response to be returned to the caller.

To see this in action in this sample, Submit a GET request with the following JSON payload to the endpoint `http://localhost:7071/api/QueryValidationWithCustomResponse?name2=Fred`


and observe it returns 400 Bad Request with a description of the error:

```
{
  "customResponse": {
    "errorMessage": "Name: The Name field is required."
  }
}
```