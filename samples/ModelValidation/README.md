# Sample: Model validation

This example project shows how to configure the built-in middleware to perform model validation for query parameters and body payload.

## BodyValidation

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

Note that body validation can handle a complex nested object.  Submit the following request:

```
{
  "name": "Test",
  "user": {
    "id": 1
  }
}
```

and the resposne indicates a nested child property is invalid:

```
{
  "correlationId": "...",
  "error": {
    "code": "INVALID_BODY",
    "message": "User.Name: The Name field is required."
  }
}
```

## QueryValidation

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

Now try the following request `http://localhost:7071/api/QueryValidation?name2=Fred`:

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
