// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using Microsoft.Data.Sqlite;
using NSubstitute;
using Shouldly;
using YetAnotherBlogGenerator.Cache;
using YetAnotherBlogGenerator.Config;

namespace YetAnotherBlogGenerator.Tests.Cache;

[TestClass]
public sealed class CacheServiceTests {
  [TestMethod]
  public void Get_NoCachedData_ReturnsNull() {
    // Arrange
    using var tempDirectory = CreateTempDirectory();
    var config = CreateConfiguration(tempDirectory.Path);
    var cacheService = new CacheService(config);

    // Act
    var result = cacheService.Get(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

    // Assert
    result.ShouldBeNull();
    Path.Exists(Path.Join(tempDirectory.Path, Constants.CacheFileName)).ShouldBeTrue();
  }

  [TestMethod]
  public void SaveThenGet_Called_ReturnsSavedValue() {
    // Arrange
    using var tempDirectory = CreateTempDirectory();
    var config = CreateConfiguration(tempDirectory.Path);
    var cacheService = new CacheService(config);
    var source = Guid.NewGuid().ToString();
    var key = Guid.NewGuid().ToString();
    var value = Guid.NewGuid().ToString();

    // Act
    cacheService.Set(source, key, value);

    var result = cacheService.Get(source, key);

    // Assert
    result.ShouldBe(value);
    Path.Exists(Path.Join(tempDirectory.Path, Constants.CacheFileName)).ShouldBeTrue();
  }

  [TestMethod]
  public void SaveThenGet_CalledInAnotherInstance_ReturnsSavedValue() {
    // Arrange
    using var tempDirectory = CreateTempDirectory();
    var config = CreateConfiguration(tempDirectory.Path);
    var cacheService1 = new CacheService(config);
    var cacheService2 = new CacheService(config);
    var source = Guid.NewGuid().ToString();
    var key = Guid.NewGuid().ToString();
    var value = Guid.NewGuid().ToString();

    // Act
    cacheService1.Set(source, key, value);

    SqliteConnection.ClearAllPools();

    var result = cacheService2.Get(source, key);

    // Assert
    result.ShouldBe(value);
    Path.Exists(Path.Join(tempDirectory.Path, Constants.CacheFileName)).ShouldBeTrue();
  }

  private static DisposableDirectory CreateTempDirectory() {
    var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(directory);
    return new DisposableDirectory(directory);
  }

  private static IConfiguration CreateConfiguration(string sourceRoot) {
    var config = Substitute.For<IConfiguration>();
    config.SourceRoot.Returns(sourceRoot);
    return config;
  }

  private class DisposableDirectory(string path) : IDisposable {
    public string Path { get; } = path;

    public void Dispose() {
      SqliteConnection.ClearAllPools();
      Directory.Delete(Path, true);
    }
  }
}
