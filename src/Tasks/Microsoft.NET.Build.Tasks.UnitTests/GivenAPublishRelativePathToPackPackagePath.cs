// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FluentAssertions;
using Xunit;

namespace Microsoft.NET.Build.Tasks.UnitTests
{
    public class GivenAResolveToolPackagePaths
    {
        [TestMethod]
        [DataRow("tools/myfile.exe", "tools")]
        [DataRow(@"tools\myfile.exe", "tools")]
        [DataRow(@"tools\/myfile.exe", "tools")]
        [DataRow(@"tools/\myfile.exe", "tools")]
        [DataRow(@"myfile.exe", "")]
        [DataRow(@"myfile", "")]
        [DataRow("tools/myfile", "tools")]
        [DataRow("/myfile", "")]
        [DataRow("\\myfile", "")]
        [DataRow("tools/sub/myfile.exe", "tools/sub")]
        [DataRow("tools\\sub\\myfile.exe", "tools/sub")]
        public void ItConvertsFromPublishRelativePathToPackPackagePath(string publishRelativePath, string packPackagePath)
        {
            ResolveToolPackagePaths.GetDirectoryPathInRelativePath(publishRelativePath).Should().Be(packPackagePath);
        }
    }
}
