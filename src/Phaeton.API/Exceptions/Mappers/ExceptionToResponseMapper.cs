using Humanizer;
using Phaeton.Abstractions;
using Phaeton.API.Exceptions.Interfaces;
using Phaeton.API.Exceptions.Models;
using System.Collections.Concurrent;
using System.Net;

namespace Phaeton.API.Exceptions.Mappers;

internal sealed class ExceptionToResponseMapper : IExceptionToResponseMapper
{
    private readonly ConcurrentDictionary<Type, string> _codes = new();

    public ExceptionResponse Map(Exception ex)
        => ex switch
        {
            ExceptionBase exBase => new ExceptionResponse(new ErrorsResponse(
                new Error(
                    GetErrorCode(ex),
                    ex.Message
                )),
                HttpStatusCode.BadRequest
            ),
            ArgumentException _ => new(
                new ErrorsResponse(new Error(
                    GetErrorCode(ex),
                    ex.Message
                )),
                HttpStatusCode.BadRequest
            ),
            HttpRequestException _ => new(
                new ErrorsResponse(new Error(
                    "internal_service_http_communication",
                    "There was an internal HTTP service communication error.")
                ),
                HttpStatusCode.InternalServerError
            ),
            _ => new(new ErrorsResponse(new Error(
                    "error",
                    "There was an error.")
                ),
                HttpStatusCode.InternalServerError
            )
        };

    private sealed record Error(
        string Code,
        string Message
    );

    private sealed record ErrorsResponse(params Error[] Errors);

    private string GetErrorCode(object ex)
    {
        var type = ex.GetType();

        return _codes.GetOrAdd(
            type,
            type.Name
                .Underscore()
                .Replace(
                    "_exception",
                    string.Empty
                )
        );
    }
}