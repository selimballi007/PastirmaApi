namespace PastirmaApi.Core.Exceptions
{
    public class BusinessException : BaseException
    {
        public BusinessException(string message, int statusCode=StatusCodes.Status400BadRequest) : base(message, statusCode) { }
    }
}
