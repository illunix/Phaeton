using Phaeton.API.Exceptions.Models;

namespace Phaeton.API.Exceptions.Interfaces;

public interface IExceptionToResponseMapper
{
    ExceptionResponse Map(Exception ex);
}