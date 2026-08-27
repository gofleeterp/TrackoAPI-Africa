using System;

namespace TrackoApi.Core
{
    public class UnifiedResultEntity<T> where T : class
    {
        public T Result { get; set; }
        public string Message { get; set; }
        public string Hint { get; set; }
        public bool IsSuccess { get; set; }
        public Exception Exception { get; set; }
    }
    public class SuccessResult<T> : UnifiedResultEntity<T> where T : class
    {
        public SuccessResult(string message = "Success", string hint = "")
        {
            IsSuccess = true;
            Message = message;
            Hint = hint;
        }
        public SuccessResult(T result, string message = "Success", string hint = "")
        {
            IsSuccess = true;
            Result = result;
            Message = message;
            Hint = hint;
        }
    }
    public class FaildResult<T> : UnifiedResultEntity<T> where T : class
    {
        public FaildResult(string message = "", string hint = "", Exception exception = null)
        {
            IsSuccess = false;
            Message = message;
            Hint = hint;
            Result = null;
            if (string.IsNullOrWhiteSpace(Message) && exception != null)
            {
                Message = exception.GetBaseException().Message;
            }
            Exception = exception;
        }
        public FaildResult(Exception exception = null)
        {
            IsSuccess = false;
            Result = null;
            if (string.IsNullOrWhiteSpace(Message) && exception != null)
            {
                Message = exception.GetBaseException().Message;
            }
            Exception = exception;
        }
    }
}
