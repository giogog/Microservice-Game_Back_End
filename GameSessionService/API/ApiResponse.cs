namespace GameSession.API;

public record ApiResponse
{
    public string Message { get; set; }
    public bool IsSuccess { get; set; }
    public object Data { get; set; }
    public int StatusCode { get; set; }
    public ApiResponse()
    {

    }
    public ApiResponse(string message, bool success, object data, int statuscode)
    {
        Message = message;
        IsSuccess = success;
        Data = data;
        StatusCode = statuscode;
    }
}
