namespace ExpenseTracker.Common.Models
{
    public class Result
    {
        public bool IsSuccess { get; }
        public IReadOnlyDictionary<string, string> Errors { get; }

        protected Result(bool isSuccess, IReadOnlyDictionary<string, string> errors)
        {
            if (isSuccess && errors != null && errors.Any())
                throw new InvalidOperationException("A successful result cannot have error messages.");
            if (!isSuccess && (errors == null || !errors.Any()))
                throw new InvalidOperationException("A failure result must have at least one error message.");

            IsSuccess = isSuccess;
            Errors = errors ?? new Dictionary<string, string>();
        }

        public static Result Success() => new Result(true, new Dictionary<string, string>());
        public static Result Failure(IReadOnlyDictionary<string, string> errors) => new Result(false, errors);
    }

    public class Result<T> : Result
    {
        public T Value { get; }

        private Result(bool isSuccess, T value, IReadOnlyDictionary<string, string> errors)
            : base(isSuccess, errors)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new(true, value, new Dictionary<string, string>());
        public static new Result<T> Failure(IReadOnlyDictionary<string, string> errors) => new(false, default!, errors);
    }
}
