namespace Unity.FPS.API
{
    public readonly struct APIResult<T>
    {
        public bool Success { get; }
        public T Value { get; }
        public string Error { get; }

        private APIResult(bool success, T value, string error)
        {
            Success = success;
            Value = value;
            Error = error;
        }

        public static APIResult<T> Ok(T value)
        {
            return new APIResult<T>(true, value, null);
        }

        public static APIResult<T> Fail(string error)
        {
            return new APIResult<T>(false, default, error);
        }
    }
}