namespace Cemetech.DecBot4.DecBot.Sql
{
    static class SharedSqlCommands
    {
        public const string CountTotals = "SELECT * FROM (SELECT COUNT(*) AS NameCount, COALESCE(SUM(Score), 0) AS TotalScore FROM scores) AS t1, (SELECT COUNT(*) AS LinkCount FROM links) AS t2, (SELECT COUNT(DISTINCT AddedBy) AS AddedByCount, COUNT(*) As TotalQuotes FROM quotes WHERE Active = 1) as t3";
        public const string GetPrivilegedUser = "SELECT * FROM privileged_users WHERE user_id = @user_id and protocol = @protocol LIMIT 1";
    }
}
