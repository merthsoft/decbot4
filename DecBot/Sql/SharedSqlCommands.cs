namespace Cemetech.DecBot4.DecBot.Sql
{
    static class SharedSqlCommands
    {
        public const string CountTotals = "SELECT * FROM (SELECT COUNT(*) AS NameCount, COALESCE(SUM(Score), 0) AS TotalScore FROM scores) AS t1, (SELECT COUNT(*) AS LinkCount FROM links) AS t2, (SELECT COUNT(DISTINCT AddedBy) AS AddedByCount, COUNT(*) As TotalQuotes FROM quotes WHERE Active = 1) as t3";
    }
}
