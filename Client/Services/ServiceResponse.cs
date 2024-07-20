namespace Client.Services;

public sealed class ServiceResponse<T>
{
    private ServiceResponse()
	{
	}

	public static ServiceResponse<T> Success(T? data=default)
	{
		return new ServiceResponse<T>
		{
			IsSuccess = true,
			Message = "Success",
			Data = data
		};
	}

	public static ServiceResponse<T> Fail(string message)
	{
		return new ServiceResponse<T>
		{
			IsSuccess = false,
			Message = message
		};
	}

    public T? Data { get; set; }	
	public bool IsSuccess { get; set; }
	public string Message { get; set; } = string.Empty;
}
