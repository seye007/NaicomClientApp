namespace NiconClientApp
{
	public class Naicom
	{
		public int Id { get; set; }
		public string? PolicyNumber { get; set; }
		public string? PolicyUniqueID { get; set; }
		public string? Data { get; set; }
		public string? ProductName { get; set; }
		public bool status { get; set; }

		public RevisePolicyRequestPayload MapNaicomToRequest()
		{
			var request = new RevisePolicyRequestPayload
			{
				SID = "922836c8-86f5-4831-9f48-7097ac97982d",
				PolicyUniqueID = this.PolicyUniqueID ?? throw new Exception("Policy unique ID cannot be null"),
				Token = "GMH8CyNX0kjINiiS9YYxSJ9IcJesl5gt"
			};
			return request;
		}
	}
}
