// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using Microsoft.Data.Sqlite;
using YetAnotherBlogGenerator.Config;

namespace YetAnotherBlogGenerator.Cache;

internal class CacheService : ICacheService {
  private readonly string _connectionString;
  private readonly bool _dbInitialized = false;
  private readonly Lock _lock = new();

  public CacheService(IConfiguration configuration) {
    var dbPath = Path.Combine(configuration.SourceRoot, Constants.CacheFileName);
    _connectionString = $"Data Source={dbPath}";
  }

  private void Initialize() {
    if (_dbInitialized) return;
    lock (_lock) {
      using var connection = new SqliteConnection(_connectionString);
      connection.Open();
      var cmd = connection.CreateCommand();
      cmd.CommandText =
          "CREATE TABLE IF NOT EXISTS cache (source TEXT NOT NULL, key TEXT NOT NULL, value TEXT NULL, PRIMARY KEY (source, key))";
      cmd.ExecuteNonQuery();
    }
  }

  public string? Get(string source, string key) {
    if (!_dbInitialized) Initialize();
    lock (_lock) {
      using var connection = new SqliteConnection(_connectionString);
      connection.Open();
      var cmd = connection.CreateCommand();
      cmd.CommandText = "SELECT value FROM cache WHERE source = $source AND key = $key";
      cmd.Parameters.AddWithValue("$source", source);
      cmd.Parameters.AddWithValue("$key", key);
      return (string?)cmd.ExecuteScalar();
    }
  }

  public void Set(string source, string key, string? value) {
    if (!_dbInitialized) Initialize();
    lock (_lock) {
      using var connection = new SqliteConnection(_connectionString);
      connection.Open();
      var cmd = connection.CreateCommand();
      cmd.CommandText =
          "INSERT INTO cache (source, key, value) VALUES ($source, $key, $value) ON CONFLICT (source, key) DO UPDATE SET value = excluded.value";
      cmd.Parameters.AddWithValue("$source", source);
      cmd.Parameters.AddWithValue("$key", key);
      cmd.Parameters.AddWithValue("$value", value);
      cmd.ExecuteNonQuery();
    }
  }
}
