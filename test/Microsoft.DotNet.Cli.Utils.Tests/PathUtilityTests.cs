// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Tools.Common;

namespace Microsoft.DotNet.Cli.Utils
{
    public class PathUtilityTests
    {
        /// <summary>
        /// Tests that PathUtility.GetRelativePath treats drive references as case insensitive on Windows.
        /// </summary>
        [WindowsOnlyFact]
        public void GetRelativePathWithCaseInsensitiveDrives()
        {
            Assert.AreEqual(@"bar\", PathUtility.GetRelativePath(@"C:\foo\", @"C:\foo\bar\"));
            Assert.AreEqual(@"Bar\Baz\", PathUtility.GetRelativePath(@"c:\foo\", @"C:\Foo\Bar\Baz\"));
            Assert.AreEqual(@"baz\Qux\", PathUtility.GetRelativePath(@"C:\fOO\bar\", @"c:\foo\BAR\baz\Qux\"));
            Assert.AreEqual(@"d:\foo\", PathUtility.GetRelativePath(@"C:\foo\", @"d:\foo\"));
        }

        [WindowsOnlyFact]
        public void GetRelativePathForFilePath()
        {
            Assert.AreEqual(
                $@"mytool\1.0.1\mytool\1.0.1\tools\{ToolsetInfo.CurrentTargetFramework}\any\mytool.dll",
                PathUtility.GetRelativePath(
                    @"C:\Users\myuser\.dotnet\tools\mytool.exe",
                    $@"C:\Users\myuser\.dotnet\tools\mytool\1.0.1\mytool\1.0.1\tools\{ToolsetInfo.CurrentTargetFramework}\any\mytool.dll"));
        }

        [WindowsOnlyFact]
        public void GetRelativePathRequireTrailingSlashForDirectoryPath()
        {
            Assert.AreNotEqual(
                $@"mytool\1.0.1\mytool\1.0.1\tools\{ToolsetInfo.CurrentTargetFramework}\any\mytool.dll",
                PathUtility.GetRelativePath(
                    @"C:\Users\myuser\.dotnet\tools",
                    $@"C:\Users\myuser\.dotnet\tools\mytool\1.0.1\mytool\1.0.1\tools\{ToolsetInfo.CurrentTargetFramework}\any\mytool.dll"));

            Assert.AreEqual(
                $@"mytool\1.0.1\mytool\1.0.1\tools\{ToolsetInfo.CurrentTargetFramework}\any\mytool.dll",
                PathUtility.GetRelativePath(
                    @"C:\Users\myuser\.dotnet\tools\",
                    $@"C:\Users\myuser\.dotnet\tools\mytool\1.0.1\mytool\1.0.1\tools\{ToolsetInfo.CurrentTargetFramework}\any\mytool.dll"));
        }

        /// <summary>
        /// Tests that PathUtility.RemoveExtraPathSeparators works correctly with drive references on Windows.
        /// </summary>
        [WindowsOnlyFact]
        public void RemoveExtraPathSeparatorsWithDrives()
        {
            Assert.AreEqual(@"c:\foo\bar\baz\", PathUtility.RemoveExtraPathSeparators(@"c:\\\foo\\\\bar\baz\\"));
            Assert.AreEqual(@"D:\QUX\", PathUtility.RemoveExtraPathSeparators(@"D:\\\\\QUX\"));
        }
    }
}
