namespace SensorX.Master.Application.Common.ResponseClient
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? Message { get; }

        protected Result(bool isSuccess, T? value, string? message)
        {
            if (!isSuccess && value != null && !value.Equals(default(T)))
                throw new InvalidOperationException();
            if (!isSuccess && message == null)
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            Value = value;
            Message = message;
        }

        public static Result<T> Success(T value, string message = "Success") => new(true, value, message);
        public static Result<T> Failure(string message) => new(false, default, message);
        public static implicit operator bool(Result<T>? result) => result is not null && result.IsSuccess;
    }

    public class Result
    {
        public bool IsSuccess { get; }
        public string? Message { get; }

        protected Result(bool isSuccess, string? message)
        {
            if (!isSuccess && message == null)
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            Message = message;
        }

        public static Result Success(string message = "Success") => new(true, message);
        public static Result Failure(string message) => new(false, message);
        public static implicit operator bool(Result? result) => result is not null && result.IsSuccess;
    }
}
