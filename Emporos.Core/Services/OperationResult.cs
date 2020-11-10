namespace Emporos.Core.Services
{
    public class OperationResult
    {
        protected OperationResult(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        public bool Success { get; }

        public string Error { get; }

        public static OperationResult Fail(string error)
        {
            return new OperationResult(false, error);
        }

        public static OperationResult Ok()
        {
            return new OperationResult(true, null);
        }

        public static OperationResult<T> Ok<T>(T value)
        {
            return new OperationResult<T>(value, true, null);
        }

        public static OperationResult<T> OkWithWarnings<T>(T value, string error)
        {
            return new OperationResult<T>(value, true, error);
        }

        public static OperationResult<T> Fail<T>(string error)
        {
            return new OperationResult<T>(default, false, error);
        }
    }
}