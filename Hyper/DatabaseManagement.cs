using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace HyperDatabase
{
    public class ContainerRecord
    {
        public string Id { get; set; } = "";
        public string State { get; set; } = "";
        public string IP { get; set; } = "";
        public string Owner { get; set; } = "";
        public string Created { get; set; } = "";
    }

    public static class DatabaseManagement
    {
        private static readonly string ConnectionString = "Data Source=hyper.db";

        // ---------------- INIT ----------------
        public static void Initialize()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var settings = connection.CreateCommand();
            settings.CommandText = @"CREATE TABLE IF NOT EXISTS Settings (
                Key TEXT PRIMARY KEY,
                Value TEXT
            );";
            settings.ExecuteNonQuery();

            var containers = connection.CreateCommand();
            containers.CommandText = @"CREATE TABLE IF NOT EXISTS Containers (
                Id TEXT PRIMARY KEY,
                State TEXT NOT NULL,
                IP TEXT,
                Owner TEXT,
                Created TEXT NOT NULL
            );";
            containers.ExecuteNonQuery();

            Console.WriteLine("Database initialized.");
        }

        public static bool IsPaired()
        {
            return !string.IsNullOrWhiteSpace(GetSetting("panelid"));
        }

        // ---------------- INSERT ----------------
        public static void AddContainer(ContainerRecord container)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Containers (Id, State, IP, Owner, Created)
                VALUES ($id, $state, $ip, $owner, $created);
            ";

            command.Parameters.AddWithValue("$id", container.Id);
            command.Parameters.AddWithValue("$state", container.State);
            command.Parameters.AddWithValue("$ip", container.IP);
            command.Parameters.AddWithValue("$owner", container.Owner);
            command.Parameters.AddWithValue("$created", container.Created);

            command.ExecuteNonQuery();
        }

        // ---------------- GET BY ID ----------------
        public static ContainerRecord? GetContainer(string id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, State, IP, Owner, Created
                FROM Containers
                WHERE Id = $id;
            ";

            command.Parameters.AddWithValue("$id", id);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            return new ContainerRecord
            {
                Id = reader.GetString(0),
                State = reader.GetString(1),
                IP = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Owner = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Created = reader.GetString(4)
            };
        }

        // ---------------- GET ALL ----------------
        public static List<ContainerRecord> GetAllContainers()
        {
            List<ContainerRecord> containers = new();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, State, IP, Owner, Created FROM Containers;";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                containers.Add(new ContainerRecord
                {
                    Id = reader.GetString(0),
                    State = reader.GetString(1),
                    IP = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Owner = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Created = reader.GetString(4)
                });
            }

            return containers;
        }

        // ---------------- UPDATE STATE ----------------
        public static void UpdateContainerState(string id, string newState)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Containers
                SET State = $state
                WHERE Id = $id;
            ";

            command.Parameters.AddWithValue("$state", newState);
            command.Parameters.AddWithValue("$id", id);

            command.ExecuteNonQuery();
        }

        // ---------------- UPDATE IP ----------------
        public static void UpdateContainerIP(string id, string ip)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Containers
                SET IP = $ip
                WHERE Id = $id;
            ";

            command.Parameters.AddWithValue("$ip", ip);
            command.Parameters.AddWithValue("$id", id);

            command.ExecuteNonQuery();
        }

        // ---------------- DELETE ----------------
        public static void DeleteContainer(string id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Containers WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id);

            command.ExecuteNonQuery();
        }

        // ---------------- EXISTS ----------------
        public static bool ContainerExists(string id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Containers WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id);

            long count = (long)command.ExecuteScalar()!;
            return count > 0;
        }

        public static void SetSetting(string key, string value)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Settings (Key, Value)
                VALUES ($key, $value)
                ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value;
            ";

            command.Parameters.AddWithValue("$key", key);
            command.Parameters.AddWithValue("$value", value);

            command.ExecuteNonQuery();
        }

        public static string GetSetting(string key)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Value FROM Settings
                WHERE Key = $key;
            ";

            command.Parameters.AddWithValue("$key", key);

            object? result = command.ExecuteScalar();

            return result?.ToString() ?? "";
        }

        public static void RemoveSetting(string key)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Settings WHERE Key = $key;";
            command.Parameters.AddWithValue("$key", key);

            command.ExecuteNonQuery();
        }
    }
}