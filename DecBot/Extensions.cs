using MySqlConnector;

namespace Cemetech.DecBot4.DecBot;

static class Extensions
{
	public static Task<MySqlDataReader> ExecuteFullCommandAsync(this MySqlCommand c, string commandText, params (string Name, object Value)[] parameters)
	{
		c.CommandText = commandText;
		c.Parameters.Clear();
		foreach (var (Name, Value) in parameters)
		{
			c.Parameters.AddWithValue(Name, Value);
		}

		return c.ExecuteReaderAsync();
	}

	public static Task<int> ExecuteFullUpdateAsync(this MySqlCommand c, string commandText, params (string Name, object Value)[] parameters)
	{
		c.CommandText = commandText;
		c.Parameters.Clear();
		foreach (var (Name, Value) in parameters)
		{
			c.Parameters.AddWithValue(Name, Value);
		}

		return c.ExecuteNonQueryAsync();
	}

	public static bool StartsWith(this string s, params string[] args)
	{
		foreach (var a in args)
		{
			if (s.StartsWith(a))
			{
				return true;
			}
		}

		return false;
	}

	public static bool EndsWith(this string s, params string[] args)
	{
		foreach (var a in args)
		{
			if (s.EndsWith(a))
			{
				return true;
			}
		}

		return false;
	}

	public static string RemoveAll(this string s, params string[] args)
	{
		var ret = s;
		foreach (var a in args)
		{
			ret = ret.Replace(a, "");
		}

		return ret;
	}
}
