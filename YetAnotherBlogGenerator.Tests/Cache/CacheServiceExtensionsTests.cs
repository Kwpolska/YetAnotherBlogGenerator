// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text.Json;
using NSubstitute;
using Shouldly;
using YetAnotherBlogGenerator.Cache;

namespace YetAnotherBlogGenerator.Tests.Cache;

[TestClass]
public class CacheServiceExtensionsTests {
  [TestMethod]
  public void GetT_ReadsNull_ReturnsDefault() {
    // Arrange
    var cache = Substitute.For<ICacheService>();
    const string source = "source";
    const string key = "key";
    const string? value = null;
    cache.Get(source, key).Returns(value);

    // Act & Assert
    cache.Get<LastRenderStatus>(source, key).ShouldBeNull();
    cache.Received(1).Get(source, key);
  }

  [TestMethod]
  public void GetT_ReadsString_DeserializesJson() {
    // Arrange
    var cache = Substitute.For<ICacheService>();
    const string source = "source";
    const string key = "key";
    var value = new LastRenderStatus(StartTime: DateTimeOffset.UtcNow, Successful: false);
    var jsonValue = JsonSerializer.Serialize(value);
    cache.Get(source, key).Returns(jsonValue);

    // Act & Assert
    cache.Get<LastRenderStatus>(source, key).ShouldBe(value);
    cache.Received(1).Get(source, key);
  }

  [TestMethod]
  public void SetT_Called_SerializesJsonAndSaves() {
    // Arrange
    var cache = Substitute.For<ICacheService>();
    const string source = "source";
    const string key = "key";
    var value = new LastRenderStatus(StartTime: DateTimeOffset.UtcNow, Successful: false);
    var jsonValue = JsonSerializer.Serialize(value);
    cache.Get(source, key).Returns(jsonValue);

    // Act
    cache.Set(source, key, value);

    // Assert
    cache.Received(1).Set(source, key, jsonValue);
  }
}
