namespace Client.Services;

public interface IInitializable
{
	Task<ServiceResponse<bool>> Initialize();
}