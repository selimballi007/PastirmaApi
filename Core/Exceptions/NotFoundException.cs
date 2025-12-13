namespace PastirmaApi.Core.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message, int statusCode = StatusCodes.Status404NotFound) : base(message, statusCode) { }
    }
}