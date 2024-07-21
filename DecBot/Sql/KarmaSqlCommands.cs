namespace Cemetech.DecBot4.DecBot.Sql
{
    class KarmaSqlCommands
    {
        public const string GetTop = "SELECT * FROM scores ORDER BY Score DESC LIMIT 3;";
        public const string GetScore = "SELECT * FROM scores WHERE Name = @name;";
        public const string GetScoreWithLink = "SELECT * FROM scores WHERE Name = @name OR Name = (SELECT link FROM links WHERE Name = @name);";

        public const string GetSum = "SELECT SUM(Score) AS Total FROM scores WHERE Name = @link OR Name = @fname;";
        public const string InsertScore = "INSERT INTO scores (Name, Score) VALUES (@fname, @score) ON DUPLICATE KEY UPDATE Score = @score;";
        public const string DeleteScore = "DELETE FROM scores WHERE Name = @link;";

        public const string TrackScoreChange = "INSERT INTO scores_log (Name, `Change`) VALUES (@name, @change)";
    }
}
