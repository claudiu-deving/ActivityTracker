using Microsoft.ML.Data;

namespace LLMLibrary;
public class ModelOutput
{
	[VectorType(1)]
	[ColumnName("output")]
	public float[] Output { get; set; }
}