using System.Text.Json.Serialization;
namespace NiacomClientApp
{
	public class NaicomPolicyResponse
	{
		[JsonPropertyName("IsFound")]
		public bool IsFound { get; set; }

		[JsonPropertyName("ErrMsg")]
		public string? ErrMsg { get; set; }

		[JsonPropertyName("DataGroup")]
		public List<DataGroup>? DataGroup { get; set; }
	}
	public class AttArray
	{
		[JsonPropertyName("Name")]
		public string? Name { get; set; }

		[JsonPropertyName("Value")]
		public string? Value { get; set; }
	}

	public class DataGroup
	{
		[JsonPropertyName("GroupName")]
		public string? GroupName { get; set; }

		[JsonPropertyName("AttArray")]
		public List<AttArray>? AttArray { get; set; }
	}
}
