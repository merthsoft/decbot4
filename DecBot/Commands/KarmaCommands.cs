using Cemetech.DecBot4.DecBot.Sql;
using MySqlConnector;
using System.Data;
using System.Text;

namespace Cemetech.DecBot4.DecBot;

partial class DecBot4
{
	private async Task<(string?, int)> getKarma(MySqlCommand Command, string name)
	{
		if (Command == null)
			return (null, 0);

        using var reader = await Command.ExecuteFullCommandAsync(KarmaSqlCommands.GetScoreWithLink, ("@name", name));
        if (!await reader.ReadAsync())
            return (null, 0);

		return (reader.GetString("name"), reader.GetInt32("score"));
    }

	private Task GetKerma(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
		=> GetKarma(messageSender, channel, hasPrivs, ["KermM"]);

	private async Task GetKarma(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
	{
		if (parameters.Count > 1)
		{
			await SendMessage(channel, "Syntax: `!karma <name>`");
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
		using (var sqlCommand = new MySqlCommand() { Connection = Connection })
		{
			var (name, score) = await getKarma(sqlCommand, checkedName);
			string message = "";
			if (name == checkedName)
				message = $"{name} has a score of {score}.";
			else if (name != null)
				message = $"{name} ({checkedName}) has a score of {score}.";
			else
                message = $"{checkedName} has a score of 0.";

			await SendMessage(channel, message);
		}
	}

	private async Task GetTop(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
	{
		using (Connection.Open())
		using (var Command = new MySqlCommand(KarmaSqlCommands.GetTop, Connection))
		using (var reader = await Command.ExecuteReaderAsync(CommandBehavior.Default))
		{
			var sb = new StringBuilder("The top scores are:");
			sb.AppendLine();
			while (await reader.ReadAsync())
			{
				sb.AppendLine($"{reader.GetString("Name")}: {reader.GetInt32("Score")}");
			}
			await SendMessage(channel, sb.ToString());
		}
	}

	private Task SaySite(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
		=> SendMessage(channel, "{0}: {1}", messageSender, Config.Bot.Site);
}
