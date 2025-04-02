namespace Chronix.EventRepository.Tools {
    public class Result
    {
        public bool IsSuccess { get; }
        public List<string> Errors { get; }

        protected Result(bool isSuccess, List<string>? errors = null)
        {
            IsSuccess = isSuccess;
            Errors = errors ?? [];
        }

        public static Result Success() => new(true);

        public static Result Failure(params List<string> errors) => new(false, errors);
    }

    public class Result<T> : Result
    {
        public T Value { get; }

        private Result(T value) : base(true)
        {
            Value = value;
        }

        private Result(List<string> errors) : base(false, errors)
        {
            Value = default!;
        }

        public static Result<T> Success(T value) => new(value);

        public new static Result<T> Failure(params List<string> errors) => new(errors);
    }
}