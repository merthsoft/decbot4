namespace Cemetech.DecBot4.DecBot.Sql
{
    class QuoteSqlCommands
    {
        public const string AddQuote = "INSERT INTO quotes (Timestamp, AddedBy, Quote) VALUES (UTC_TIMESTAMP(), @name, @quote)";

        public const string GetQuoteById = "SELECT * FROM quotes WHERE Id = @id AND Active=1";
        public const string SearchQuote = "SELECT * FROM quotes WHERE Quote LIKE @search AND Active=1 ORDER BY (ScoreUp - ScoreDown)";
        public const string RandomQuote = "SELECT * FROM quotes WHERE Active=1 ORDER BY RAND() LIMIT 1";

        public const string DeleteQuote = "UPDATE quotes SET Active = 0, DeletedBy = @name WHERE Id = @id";

        public const string UpdateQuoteLink = "UPDATE quotes SET AddedBy = @fname where AddedBy = @name";
    }
}
