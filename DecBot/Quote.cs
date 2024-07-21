using MySqlConnector;


namespace Cemetech.DecBot4.DecBot;

class Quote
{
	public int Id { get; set; }
	public DateTime Timestamp { get; set; }
	public string AddedBy { get; set; }
	public string Text { get; set; }
	public int ScoreUp { get; set; }
	public int ScoreDown { get; set; }
	public bool Active { get; set; }
	public string? DeletedBy { get; set; }

	private string FormattedText { get { return Text.Trim().Replace(@"\|", "|").Replace(@"\\", @"\"); } }

	public Quote(MySqlDataReader reader)
	{
		Id = (int)reader["Id"];
		Timestamp = (DateTime)reader["Timestamp"];
		AddedBy = (string)reader["AddedBy"];
		Text = (string)reader["Quote"];
		ScoreUp = (int)reader["ScoreUp"];
		ScoreDown = (int)reader["ScoreDown"];
		Active = (bool)reader["Active"];
		DeletedBy = reader["DeletedBy"]?.ToString();
	}

	public override string ToString()
	{
		return string.Format("{0}: `{1}` [Added: `{2}` at `{3:yyyy.MM.dd HH:mm:ss}` UTC]", Id, FormattedText, AddedBy, Timestamp);
		//return string.Format("{0}: {1} [Added: {2} at {3} UTC; score: {4}]", Id, Text.Trim(), AddedBy, Timestamp, ScoreUp - ScoreDown);
	}
}