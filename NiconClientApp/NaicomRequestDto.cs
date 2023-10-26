namespace NiacomClientApp
{
	public class RevisePolicyRequestPayload
	{
		public string SID { get; set; } = string.Empty;

		public string Token { get; set; } = string.Empty;

		public string PolicyUniqueID { get; set; } = string.Empty;

		public List<FieldValueItemGroup> DataGroup { get; set; } = new List<FieldValueItemGroup>();

	}
	public class FieldValueItemGroup
	{
		public string GroupName { get; set; } = string.Empty;

		public int GroupTag { get; set; }

		public int GroupCount { get; set; }

		public List<FieldValueItem> AttArray { get; set; } = new List<FieldValueItem>();
	}
	public class FieldValueItem
	{
		public string Name { get; set; } = string.Empty;

		public string Value { get; set; } = string.Empty;
	}
}
