﻿using System.Data.SqlClient;
using System.Text.Json;
using Dapper;
using NiconClientApp;
using System.Data;
using NiacomClientApp;
using NiconClientApp.Exceptions;

class NaicomService
{
	public static SqlConnection Connection { get; set; }
	public static async Task Execute()
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
			var naicom = await Connection.QueryFirstOrDefaultAsync<Naicom>(
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
			// Update the status to true in the database
			await Connection.ExecuteAsync(
				"InsertNaiconData",
				insertParameter,
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
