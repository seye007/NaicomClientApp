using NiconClientApp;
using System.Data.SqlClient;
using (var connection = new SqlConnection(Constants.ConnectionString))
{
	await connection.OpenAsync();
	NaicomService.Connection = connection;
    await NaicomService.Execute();
}
