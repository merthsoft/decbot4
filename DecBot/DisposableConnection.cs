using MySqlConnector;

namespace Cemetech.DecBot4.DecBot;

// I'm totally cheating with this. You can dispose of it multiple times. Do something like
// using (Connection.Open()) { ... }
// Makes it so I don't have to wrap it in a try...finally just to close the connection.
class DisposableConnection : IDisposable
{
    public MySqlConnection Connection { get; private set; }

    public static implicit operator MySqlConnection(DisposableConnection conn)
    {
        return conn.Connection;
    }

    public DisposableConnection(MySqlConnection connection)
    {
        Connection = connection;
    }

    public DisposableConnection Open()
    {
        Connection.Open();
        return this;
    }

    public void Dispose()
    {
        Connection.Close();
    }
}
