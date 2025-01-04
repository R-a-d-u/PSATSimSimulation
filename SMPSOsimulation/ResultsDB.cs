namespace SMPSOsimulation;

using System;
using System.Data.SQLite;
using System.IO;

public class ResultsDB
{
    private string _dbPath;
    private string _connectionString;

    public ResultsDB()
    {
        // Define the path of the SQLite database file
        _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache.db");
        _connectionString = $"Data Source={_dbPath};Version=3;";

        // Ensure the database and table exist
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        if (!File.Exists(_dbPath))
        {
            SQLiteConnection.CreateFile(_dbPath);
        }

        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS CpuConfigCache (
                        Hash TEXT NOT NULL,
                        EnvironmentHash TEXT NOT NULL,
                        CPI REAL NOT NULL,
                        Energy REAL NOT NULL,
                        PRIMARY KEY (Hash, EnvironmentHash)
                    );
                ";
                command.ExecuteNonQuery();
            }
        }
    }

    public void AddEntry(string hash, string environmentHash, double cpi, double energy)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT OR REPLACE INTO CpuConfigCache (Hash, EnvironmentHash, CPI, Energy)
                    VALUES (@Hash, @EnvironmentHash, @CPI, @Energy);
                ";
                command.Parameters.AddWithValue("@Hash", hash);
                command.Parameters.AddWithValue("@EnvironmentHash", environmentHash);
                command.Parameters.AddWithValue("@CPI", cpi);
                command.Parameters.AddWithValue("@Energy", energy);
                command.ExecuteNonQuery();
            }
        }
    }

    public (double CPI, double Energy)? GetEntry(string hash, string environmentHash)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT CPI, Energy
                    FROM CpuConfigCache
                    WHERE Hash = @Hash AND EnvironmentHash = @EnvironmentHash;
                ";
                command.Parameters.AddWithValue("@Hash", hash);
                command.Parameters.AddWithValue("@EnvironmentHash", environmentHash);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        double cpi = reader.GetDouble(0);
                        double energy = reader.GetDouble(1);
                        return (cpi, energy);
                    }
                }
            }
        }
        return null; // Entry not found
    }
}
