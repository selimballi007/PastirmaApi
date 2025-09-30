namespace PastirmaApi.Core.Exceptions
{
    public class AuthException:BaseException
    {
        public AuthException(string message, int statusCode = StatusCodes.Status400BadRequest) : base(message, statusCode) { }
    }
}
