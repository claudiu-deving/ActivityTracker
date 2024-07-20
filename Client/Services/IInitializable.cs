namespace Client.Services;

public interface IInitializable
{
	Task<bool> Initialize();
}