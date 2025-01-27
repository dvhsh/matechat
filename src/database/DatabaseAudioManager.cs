using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace matechat.database
{
    public class DatabaseAudioManager
    {
        private readonly string _connectionString;

        public DatabaseAudioManager(string databasePath)
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
                CREATE TABLE IF NOT EXISTS AudioLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Message TEXT NOT NULL,
                    AudioPath TEXT NOT NULL,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                );";

                using (var command = new SqliteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddAudioPath(string message, string audioPath)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string insertQuery = "INSERT INTO AudioLogs (Message, AudioPath) VALUES (@Message, @AudioPath);";

                using (var command = new SqliteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Message", message);
                    command.Parameters.AddWithValue("@AudioPath", audioPath);
                    command.ExecuteNonQuery();
                }
            }
        }

        public string GetAudioPath(string message)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT AudioPath FROM AudioLogs WHERE Message = @Message ORDER BY Timestamp DESC LIMIT 1;";

                using (var command = new SqliteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@Message", message);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(0);
                        }
                    }
                }
            }

            return null; // No audio found
        }
    }
}
