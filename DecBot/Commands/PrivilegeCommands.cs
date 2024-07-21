using Cemetech.DecBot4.DecBot.Sql;
using MySqlConnector;

namespace Cemetech.DecBot4.DecBot;

partial class DecBot4
{
    private async Task<bool> GetPrivilegedUser(string userId, string protocol)
    {
        using (Connection.Open())
        using (var sqlCommand = new MySqlCommand() { Connection = Connection })
        {
            using var reader = await sqlCommand.ExecuteFullCommandAsync(PrivilegeSqlCommands.GetPrivilegedUser, ("@user_id", userId), ("@protocol", protocol));

            if (!await reader.ReadAsync() || !reader.HasRows)
                return false;

            return true;
        }
    }

    private async Task AddPrivilegedUser(string messageSender, string channel, bool hasPrivs, IList<string> parameters)
    {
        if (!hasPrivs)
        {
            await SendMessage(channel, $"User {messageSender} doesn't have privileges.");
            return;
        }

        if (parameters.Count != 2)
        {
            await SendMessage(channel, $"Syntax: `!addPrivs <userId> <protocol>`");
            return;
        }

        var userId = parameters[0];
        var protocol = parameters[1];
        using (Connection.Open())
        using (var sqlCommand = new MySqlCommand() { Connection = Connection })
        {
            var rows = await sqlCommand.ExecuteFullUpdateAsync(PrivilegeSqlCommands.AddPrivilegedUser, ("@user_id", userId), ("@protocol", protocol));
            if (rows == 1)
                await SendMessage(channel, "User granted privileges.");
            else
                await SendMessage(channel, "User NOT granted privileges.");
        }
    }
}
