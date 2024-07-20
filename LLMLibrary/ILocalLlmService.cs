namespace LLMLibrary;
public interface ILocalLlmService
{
	string GetGroupSuggestions(IEnumerable<string> titles);
}