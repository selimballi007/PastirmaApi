namespace PastirmaApi.Core.Exceptions
{
    public class EmailException : BaseException
    {
        public EmailException(string message, int statusCode=StatusCodes.Status503ServiceUnavailable) : base(message, statusCode) { }
    }
}
