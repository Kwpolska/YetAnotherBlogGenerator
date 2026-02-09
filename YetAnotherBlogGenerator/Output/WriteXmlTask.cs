// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Xml.Linq;

namespace YetAnotherBlogGenerator.Output;

public record WriteXmlTask(XDocument Document, string Destination) : IOutputTask;
