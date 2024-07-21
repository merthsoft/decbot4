using Cemetech.DecBot4.DecBot.Sql;
using MySqlConnector;
using System.Data;

namespace Cemetech.DecBot4.DecBot;

partial class DecBot4
{
    private async Task GetTotals(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
    {
        using (Connection.Open())
        using (var sqlCommand = new MySqlCommand(SharedSqlCommands.CountTotals, Connection))
        {
            using var reader = await sqlCommand.ExecuteReaderAsync(CommandBehavior.SingleResult);
            var message = "Unable to get totals.";
            if (await reader.ReadAsync())
                message = $"The total score is {reader["TotalScore"]} points " +
                            $"between {reader["NameCount"]} names, " +
                            $"with {reader["LinkCount"]} links. " +
                            $"There are a total of {reader["TotalQuotes"]} quotes " +
                            $"added by {reader["AddedByCount"]} people.";

            await SendMessage(channel, message);
        }
    }
}
