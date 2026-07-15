namespace Unity.FPS.API
{
    public readonly struct ApiResult<T>
    {
        public bool Success { get; }
        public T Value { get; }
        public string Error { get; }

        private ApiResult(bool success, T value, string error)
        {
            Success = success;
            Value = value;
            Error = error;
        }

        public static ApiResult<T> Ok(T value)
        {
            return new ApiResult<T>(true, value, null);
        }

        public static ApiResult<T> Fail(string error)
        {
            return new ApiResult<T>(false, default, error);
        }
    }
}