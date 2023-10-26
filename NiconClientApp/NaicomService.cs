using System.Data.SqlClient;
using System.Text.Json;
using Dapper;
using System.Data;
using NiacomClientApp.Exceptions;

namespace NiacomClientApp
{
	class NaicomService
	{
		private static SqlConnection _connection { get; set; }
		public static async Task Execute()
		{
			using (_connection = new SqlConnection(Constants.ConnectionString))
			{
				while (true)
				{
					try
					{
						Naicom naicom = await GetNextPolicyForProcessing();
						string responseStr = await RetrievePolicyFromNaicom(naicom);
						await ProcessPolicy(responseStr, naicom);
					}
					catch (Exception ex)
					{
						if (ex is NoPolicyDataException)
						{
							Console.WriteLine("No More Policy Data");
							break;
						}
						else
						{
							DecodeException(ex);
						}
					}
				}
			}
		}

		private static void DecodeException(Exception ex)
		{
			switch (ex)
			{
				case RetrievePolicyException e:
					Console.WriteLine(e);
					break;
				case ProcessPolicyException e:
					Console.WriteLine(e);
					break;
				case HttpRequestException e:
					Console.WriteLine(e);
					break;
				case Exception e:
					Console.WriteLine(e);
					break;
			}
		}

		private static async Task<Naicom> GetNextPolicyForProcessing()
		{
			try
			{
				var parameters = new { Status = false };
				var naicom = await _connection.QueryFirstOrDefaultAsync<Naicom>(
				   "GetNaiconByStatus",
				   parameters,
				   commandType: CommandType.StoredProcedure) ?? throw new NoPolicyDataException("No policy with status 'false'.");
				return naicom;
			}
			catch
			{
				throw;
			}
		}

		private static async Task<string> RetrievePolicyFromNaicom(Naicom naicom)
		{
			try
			{
				var naicomRequest = naicom.MapNaicomToRequest();
				var requestStr = JsonSerializer.Serialize(naicomRequest);
				return await GetApiData(Constants.GetPolicy, requestStr);
			}
			catch
			{
				throw;
			}
		}

		private static async Task ProcessPolicy(string naicomResponseStr, Naicom naicom)
		{
			try
			{
				var naicomResponse = JsonSerializer.Deserialize<NaicomPolicyResponse>(naicomResponseStr);
				if (naicomResponse is null) throw new ProcessPolicyException("Naicom response can not be null");
				if (!naicomResponse.IsFound) throw new ProcessPolicyException("Naicom response not found");
				string productName = naicomResponse.DataGroup?.SelectMany(x => x.AttArray)
									 .FirstOrDefault(x => x.Name == "Insurance Type")?.Value ?? string.Empty;
				var insertParameter = new
				{
					PolicyNumber = naicom.PolicyNumber,
					PolicyUniqueID = naicom.PolicyUniqueID,
					JsonData = naicomResponseStr,
					ProcessingStatus = "NEW",
					ProductName = productName
				};
				// Insert the status to true in the database
				await _connection.ExecuteAsync(
					"InsertNaiconData",
					insertParameter,
					commandType: CommandType.StoredProcedure);
				var updateParameter = new
				{
					Id = naicom.Id,
					Status = true
				};
				//Update the status to true in the database
				await _connection.ExecuteAsync(
					"UpdateSpoolStatus",
					updateParameter,
					commandType: CommandType.StoredProcedure);
			}
			catch
			{
				throw;
			}

		}

		private static async Task<string> GetApiData(string apiURL, string query)
		{
			try
			{
				using var httpClient = new HttpClient();
				httpClient.BaseAddress = new Uri(Constants.BaseUrl);
				httpClient.Timeout = new TimeSpan(6000000000);
				var response = await httpClient.GetAsync(apiURL + query);
				response.EnsureSuccessStatusCode();
				return await response.Content.ReadAsStringAsync();
			}
			catch
			{
				throw;
			}
		}
	}
}
