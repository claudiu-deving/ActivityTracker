using Microsoft.ML;
using System.Text;

namespace LLMLibrary;
public class LocalLLMService:ILocalLlmService
{
	private readonly MLContext _mlContext;
	private readonly PredictionEngine<ModelInput, ModelOutput> _predictionEngine;
	private readonly string[] _vocabulary;

	public LocalLLMService(string modelPath, string vocabularyPath)
	{
		_mlContext = new MLContext();

		// Load model
		var pipeline = _mlContext.Transforms.ApplyOnnxModel(modelPath);
		var model = pipeline.Fit(_mlContext.Data.LoadFromEnumerable(new List<ModelInput>()));
		_predictionEngine = _mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);

		// Load vocabulary
		_vocabulary = File.ReadAllLines(vocabularyPath);
	}

	public string GetGroupSuggestions(IEnumerable<string> titles)
	{
		// Prepare input
		var input = PrepareInput(string.Join(" ", titles));

		// Make prediction
		var output = _predictionEngine.Predict(input);

		// Process output
		return ProcessOutput(output);
	}

	private ModelInput PrepareInput(string text)
	{
		// Tokenize input (simplified version, you might need a more sophisticated tokenizer)
		var tokens = text.Split(' ').Select(t => Array.IndexOf(_vocabulary, t)).Where(i => i != -1).ToArray();

		var inputIds = new long[512];
		var attentionMask = new long[512];

		for (int i = 0; i < Math.Min(tokens.Length, 512); i++)
		{
			inputIds[i] = tokens[i];
			attentionMask[i] = 1;
		}

		return new ModelInput
		{
			InputIds = inputIds,
			AttentionMask = attentionMask
		};
	}

	private string ProcessOutput(ModelOutput output)
	{
		// This is a simplified output processing. You'll need to adjust based on your model's output format.
		var result = new StringBuilder();
		foreach (var prob in output.Output)
		{
			int tokenId = Array.IndexOf(output.Output, prob);
			if (tokenId < _vocabulary.Length)
			{
				result.Append(_vocabulary[tokenId]);
				result.Append(" ");
			}
		}
		return result.ToString().Trim();
	}
}