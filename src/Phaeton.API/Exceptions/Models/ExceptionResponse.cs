using System.Net;

namespace Phaeton.API.Exceptions.Models;

public sealed record ExceptionResponse(
    object Response,
    HttpStatusCode Code
);