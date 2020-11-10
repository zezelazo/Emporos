namespace Emporos.Core.Services
{
    public class OperationResult<T> : OperationResult
    {
        protected internal OperationResult(T value, bool success, string error)
            : base(success, error)
        {
            Value = value;
        }

        public T Value { get; set; }
    }
}