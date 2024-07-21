using Cemetech.DecBot4.DecBot.Sql;
using MySqlConnector;
using System.Data;
using System.Text;

namespace Cemetech.DecBot4.DecBot;

partial class DecBot4
{
	private async Task<string?> getLinkedName(MySqlCommand Command, string name)
	{
		using var reader = await Command.ExecuteFullCommandAsync(LinkSqlCommands.GetLink, ("@name", name));
        return await reader.ReadAsync() ? reader.GetString("Link") : null;
    }

	private async Task LinkNames(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
	{
		if (parameters.Count == 1)
		{
			await CheckLinks(messageSender, channel, hasPrivs, parameters);
			return;
		}

		if (!hasPrivs)
        {
			await SendMessage(channel, $"User {messageSender} doesn't have privileges.");
			return;
		}

		if (parameters.Count != 2)
		{
			await SendMessage(channel, "Syntax: `!link <link> <main name>`");
			return;
		}
		var main = parameters[1].ToLowerInvariant();
		var link = parameters[0].ToLowerInvariant();

		if (main == link)
		{
            await SendMessage(channel, "Can't like name to itself.");
			return;
		}

		var fname = main;

		using (Connection.Open())
		using (var sqlCommand = new MySqlCommand() { Connection = Connection })
		{
			// Get final name, and check for a circular link.
			fname = await getLinkedName(sqlCommand, main) ?? main;
			if (fname == link)
			{
				await SendMessage(channel, "Trying to create a circular link. Nice try. ;)");
				return;
			}
			// Check to see if link exists.
			var linkedName = await getLinkedName(sqlCommand, link);
			if (linkedName != null)
			{
				await SendMessage(channel, "Link {0} already exists as {1}.", link, linkedName);
				return;
			}
			// Get the old name's score for tracking
			var (_, oldScore) = await getKarma(sqlCommand, link);
			// Insert the link into the table.
			var rows = await sqlCommand.ExecuteFullUpdateAsync(LinkSqlCommands.InsertLink, ("@link", link), ("@fname", fname));
			if (rows == 0)
			{
				await SendMessage(channel, "Failed to add link.");
				return;
			}
			if (fname == main)
			{
				await SendMessage(channel, "Link {0} => {1} added.", link, main);
			}
			else
			{
				await SendMessage(channel, "Link {0} => {1} => {2} added.", link, main, fname);
			}
			// Update associated links
			rows = await sqlCommand.ExecuteFullUpdateAsync(LinkSqlCommands.UpdateLinks, ("@fname", fname), ("@link", link));
			if (rows > 0)
			{
				await SendMessage(channel, "Had to update {0} links.", rows);
			}
			// Check if there's a score in the DB, and update if necessary.
			await UpdateScores(sqlCommand, channel, link, fname, oldScore);
			// Check if there are quotes in the DB, and update if necessary.
			await UpdateQuotes(sqlCommand, channel, link, fname);
		}
	}

	private async Task UpdateQuotes(MySqlCommand sqlCommand, string channel, string link, string fname)
	{
		var rows = await sqlCommand.ExecuteFullUpdateAsync(QuoteSqlCommands.UpdateQuoteLink, ("@fname", fname), ("@name", link));
		if (rows > 0)
		{
			await SendMessage(channel, "Had to update {0} quotes.", rows);
		}
	}

	private async Task UpdateScores(MySqlCommand sqlCommand, string channel, string link, string fname, int oldScore)
	{
		using var sumReader = await sqlCommand.ExecuteFullCommandAsync(KarmaSqlCommands.GetSum, ("@link", link), ("@fname", fname));
		if (await sumReader.ReadAsync())
		{
			try
			{
				var total = sumReader.GetInt32("Total");
				await sumReader.CloseAsync();
				// Perform the update
				var rows = await sqlCommand.ExecuteFullUpdateAsync(KarmaSqlCommands.InsertScore, ("@fname", fname), ("@score", total));
				if (rows == 0)
				{
					await SendMessage(channel, "Could not update linked scores...");
					return;
				}
				// Track the score change
				rows = await sqlCommand.ExecuteFullUpdateAsync(KarmaSqlCommands.TrackScoreChange, ("@name", fname), ("@change", oldScore));
				if (rows == 0)
				{
					await SendMessage(channel, "Could not add tracking row for {0}.", fname);
				}
				// Delete the link's score
				rows = await sqlCommand.ExecuteFullUpdateAsync(KarmaSqlCommands.DeleteScore, ("@link", link));
				if (rows == 0)
				{
					await SendMessage(channel, "Could not remove link's score...");
					return;
				}
				rows = await sqlCommand.ExecuteFullUpdateAsync(KarmaSqlCommands.TrackScoreChange, ("@name", link), ("@change", oldScore));
				if (rows == 0)
				{
					await SendMessage(channel, "Could not add tracking row for {0}.", link);
				}
			}
			catch
			{
				await SendMessage(channel, "Adding link failed.");
			}
		}
	}


	private async Task UnlinkNames(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
	{
		if (!hasPrivs)
		{
            await SendMessage("channel", $"User {messageSender} doesn't have privileges.");
            return;
		}

		if (parameters.Count != 1)
		{
			await SendMessage(channel, "Syntax: `!unlink <name>`", messageSender);
			return;
		}

		var name = parameters[0].ToLowerInvariant();

		using (Connection.Open())
		using (var sqlCommand = new MySqlCommand() { Connection = Connection })
		{
			// Check to see if link exists.
			var old = await getLinkedName(sqlCommand, name);
			if (old == null)
			{
				await SendMessage(channel, "Link {0} does not exist.", name);
				return;
			}
			// Remove it
			var rows = await sqlCommand.ExecuteFullUpdateAsync(LinkSqlCommands.DeleteLink, ("@name", name));
            switch (rows)
            {
                case >= 1:
                    await SendMessage(channel, "Link: {0} => {1} removed.", name, old);
                    break;
                default:
                    await SendMessage(channel, "Link {0} does not exist.", name);
                    break;
            }
        }
	}

	private async Task CheckLinks(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
	{
		if (parameters.Count > 1)
		{
            await SendMessage(channel, "Syntax: `!links <name>`");
			return;
		}

		string checkedName;
		if (parameters.Count == 1)
		{
			checkedName = parameters[0].ToLowerInvariant();
		}
		else
		{
			checkedName = messageSender.ToLowerInvariant();
		}

		using (Connection.Open())
		using (var sqlCommand = new MySqlCommand(LinkSqlCommands.GetLinks, Connection))
		{
			sqlCommand.Parameters.AddWithValue("@name", checkedName);

			using var reader = await sqlCommand.ExecuteReaderAsync(CommandBehavior.SingleResult);
			if (reader.HasRows)
			{
				var links = new StringBuilder();
				string name = null!;
				while (await reader.ReadAsync())
				{
					name = reader["Link"].ToString()!;
					var link = reader["Name"].ToString();
					links.AppendFormat("{0}, ", link);
				}
				links.Length -= 2;
				if (name == checkedName)
				{
                    await SendMessage(channel, "{0} is linked to: {1}.", name, links);
				}
				else
				{
					await SendMessage(channel, "{0} ({2}) is linked to: {1}.", name, links, checkedName);
				}
			}
			else
			{
				await SendMessage(channel, "{0} has no links.", checkedName);
			}
		}
	}
}
