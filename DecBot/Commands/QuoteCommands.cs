using Cemetech.DecBot4.DecBot.Sql;
using MySqlConnector;
using System.Data;

namespace Cemetech.DecBot4.DecBot;

partial class DecBot4
{
    private async Task DeleteQuote(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
    {
        if (!hasPrivs)
        {
            await SendMessage("channel", $"User {messageSender} doesn't have privileges.");
            return;
        }
        int id;
        if (parameters.Count != 1 || !int.TryParse(parameters[0], out id))
        {
            await SendMessage(channel, "Must supply id of quote to delete.");
            return;
        }

        using (Connection.Open())
        using (var sqlCommand = new MySqlCommand(QuoteSqlCommands.DeleteQuote, Connection))
        {
            sqlCommand.Parameters.AddWithValue("@id", id);
            sqlCommand.Parameters.AddWithValue("@name", messageSender);

            sqlCommand.ExecuteNonQuery();
            await SendMessage(channel, "Deleted quote #{0}.", id);
        }
    }

    private async Task AddQuote(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
    {
        if (parameters.Count == 0)
        {
            await SendMessage(channel, "Cannot add empty quote.");
            return;
        }

        var text = string.Join(" ", parameters);
        using (Connection.Open())
        using (var sqlCommand = new MySqlCommand() { Connection = Connection })
        {
            // Get the final name
            messageSender = await getLinkedName(sqlCommand, messageSender) ?? messageSender;

            await sqlCommand.ExecuteFullUpdateAsync(QuoteSqlCommands.AddQuote, ("@name", messageSender), ("@quote", text.Trim()));
            await SendMessage(channel, "Added quote #{0}.", sqlCommand.LastInsertedId);
        }
    }

    private async Task GetQuote(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
    {
        int quoteNumber;
        if (parameters.Count == 0)
        {
            await RandomQuote(messageSender, channel, hasPrivs, parameters);
            return;
        }
        else if (parameters.Count > 1 || !int.TryParse(parameters[0], out quoteNumber))
        {
            await SearchQuote(messageSender, channel, hasPrivs, parameters);
            return;
        }

        using (Connection.Open())
        using (var sqlCommand = new MySqlCommand(QuoteSqlCommands.GetQuoteById, Connection))
        {
            sqlCommand.Parameters.AddWithValue("@id", quoteNumber);

            using var reader = await sqlCommand.ExecuteReaderAsync(CommandBehavior.SingleResult);
            if (!await reader.ReadAsync())
            {
                var q = new Quote(reader);
                await SendMessage(channel, q.ToString());
            }
            else
                await SendMessage(channel, "No quote found with id {0}.", quoteNumber);
        }
    }

    private async Task SearchQuote(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
    {
        if (parameters.Count == 0)
        {
            await SendMessage(channel, "Cannot add search for empty string.");
            return;
        }

        var text = string.Format("%{0}%", string.Join(" ", parameters));

        using (Connection.Open())
        using (var Command = new MySqlCommand(QuoteSqlCommands.SearchQuote, Connection))
        {
            Command.Parameters.AddWithValue("@search", text);

            using var reader = await Command.ExecuteReaderAsync(CommandBehavior.SingleResult);
            if (!reader.HasRows)
            {
                await SendMessage(channel, "No quote found matching {0}.", text);
                return;
            }
            
            await reader.ReadAsync();
            var q = new Quote(reader);
            var ids = new List<int>() { q.Id };
            var numQuotes = 1;
            while (await reader.ReadAsync())
            {
                numQuotes++;
                ids.Add((int)reader["Id"]);
            }
            if (numQuotes == 1)
                await SendMessage(channel, "1 quote found: {0}", q);
            else
                await SendMessage(channel, "{0} quotes found: {1}. Top quote: {2}", numQuotes, string.Join(", ", ids), q);
        }
    }

    private async Task RandomQuote(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
    {
        if (parameters.Count > 0)
            await SendMessage(channel, "Parameters ignored for random quote.");

        using (Connection.Open())
        using (var sqlCommand = new MySqlCommand(QuoteSqlCommands.RandomQuote, Connection))
        using (var reader = await sqlCommand.ExecuteReaderAsync(CommandBehavior.SingleResult))
        {
            if (!await reader.ReadAsync())
            {
                await SendMessage(channel, "No quotes found.");
                return;
            }
            
            var q = new Quote(reader);
            await SendMessage(channel, q.ToString());
        }
    }
}
