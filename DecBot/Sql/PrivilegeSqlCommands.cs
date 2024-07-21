namespace Cemetech.DecBot4.DecBot.Sql;
internal class PrivilegeSqlCommands
{
    public const string GetPrivilegedUser = "SELECT * FROM privileged_users WHERE user_id = @user_id and protocol = @protocol LIMIT 1";
    public const string AddPrivilegedUser = "INSERT INTO privileged_users (user_id, protocol) VALUES (@user_id, @protocol)";
}
