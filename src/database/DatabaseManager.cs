using Harmony;
using Microsoft.Data.Sqlite;

namespace matechat.database
{
    public class DatabaseManager
    {
        private readonly string _connectionString;

        public DatabaseManager(string databasePath)
        {
            _connectionString = $"Data Source={databasePath};";

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS AIInteractions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Role TEXT NOT NULL, -- 'user' or 'assistant'
                    Message TEXT NOT NULL,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                );"
                ;

                using (var command = new SqliteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddMessage(string role, string message)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string insertQuery = "INSERT INTO AIInteractions (Role, Message) VALUES (@Role, @Message);";

                using (var command = new SqliteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Role", role);
                    command.Parameters.AddWithValue("@Message", message);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<(string Role, string Message, DateTime Timestamp)> GetLastMessages(int count)
        {
            var messages = new List<(string Role, string Message, DateTime Timestamp)>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string selectQuery = $@"
                SELECT Role, Message, Timestamp
                FROM AIInteractions
                ORDER BY Timestamp DESC
                LIMIT @Count;
                ";

                using (var command = new SqliteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@Count", count);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var role = reader.GetString(0);
                            var message = reader.GetString(1);
                            var timestamp = reader.GetDateTime(2);
                            messages.Add((role, message, timestamp));
                        }
                    }
                }
            }

            messages.Reverse(); // Reverse to get messages in ascending order (oldest to newest)
            return messages;
        }

        public void ClearMessages()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM AIInteractions;";

                using (var command = new SqliteCommand(deleteQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
