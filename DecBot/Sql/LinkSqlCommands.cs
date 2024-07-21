namespace Cemetech.DecBot4.DecBot.Sql
{
    class LinkSqlCommands
    {
        public const string GetLinks = "SELECT * FROM links WHERE Link = @name OR Link = (SELECT link FROM links WHERE Name = @name)";
        public const string GetLink = "SELECT * FROM links WHERE Name = @name;";

        public const string InsertLink = "INSERT INTO links (Name, Link) VALUES (@link, @fname);";
        public const string DeleteLink = "DELETE FROM links WHERE Name = @name;";
        public const string UpdateLinks = "UPDATE links SET Link = @fname WHERE Link = @link;";
    }
}
