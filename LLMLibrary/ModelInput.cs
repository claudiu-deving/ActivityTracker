using Microsoft.ML.Data;

namespace LLMLibrary
{
	public class ModelInput
	{
		[VectorType(1)]
		[ColumnName("input_ids")]
		public long[] InputIds { get; set; }

		[VectorType(1)]
		[ColumnName("attention_mask")]
		public long[] AttentionMask { get; set; }
	}
}
