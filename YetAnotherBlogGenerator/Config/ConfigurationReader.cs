// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YetAnotherBlogGenerator.Config;

public class ConfigurationReader(ISourceRootProvider sourceRootProvider) : IConfigurationReader {
  public IConfiguration Read() {
    var sourceRoot = sourceRootProvider.SourceRoot;

    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    var siteConfiguration = Deserialize<SiteConfiguration>(
        deserializer,
        Path.Combine(sourceRoot, Constants.SiteConfigFileName),
        "site");

    var localConfiguration = Deserialize<LocalConfiguration>(
        deserializer,
        Path.Combine(sourceRoot, Constants.LocalConfigFileName),
        "local");

    return new Configuration(sourceRoot, siteConfiguration, localConfiguration);
  }

  private static T Deserialize<T>(IDeserializer deserializer, string filePath, string configFileType) {
    if (!Path.Exists(filePath)) {
      throw new Exception(
          $"The {configFileType} configuration file '{filePath}' does not exist, cannot build this site.");
    }

    var configYaml = File.ReadAllText(filePath);
    return deserializer.Deserialize<T>(configYaml);
  }
}
