// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Tools.Test.Utilities;

namespace Microsoft.DotNet.Cli.Test.Tests
{
    [TestClass]
    public class GivenDotnetTestBuildsAndRunsTestFromCsprojForMultipleTFM : SdkTest
    {
        public GivenDotnetTestBuildsAndRunsTestFromCsprojForMultipleTFM(MSTestContext testContext) : base(testContext)
        {
        }

        private readonly string[] ConsoleLoggerOutputNormal = new[] { "--logger", "console;verbosity=normal" };

        [TestMethod][OSCondition(OperatingSystems.Windows)]
        public void MStestMultiTFM()
        {
            var testProjectDirectory = _testAssetsManager.CopyTestAsset("VSTestMulti", identifier: "1")
                .WithSource()
                .WithVersionVariables()
                .Path;

            NuGetConfigWriter.Write(testProjectDirectory, TestContext.Current.TestPackages);

            var runtime = EnvironmentInfo.GetCompatibleRid();

            new DotnetRestoreCommand(MSTestContext, "-r", runtime)
                .WithWorkingDirectory(testProjectDirectory)
                .Execute()
                .Should().Pass();

            var result = new DotnetTestCommand(MSTestContext, disableNewOutput: true, "-r", runtime)
                .WithWorkingDirectory(testProjectDirectory)
                .Execute(ConsoleLoggerOutputNormal);

            if (!TestContext.IsLocalized())
            {
                result.StdOut
                    .Should().Contain("Total tests: 3")
                         .And.Contain("Passed: 2")
                         .And.Contain("Failed: 1")
                         .And.Contain("Passed VSTestPassTestDesktop", "because .NET 4.6 tests will pass")
                         .And.Contain("Total tests: 3")
                         .And.Contain("Passed: 1")
                         .And.Contain("Failed: 2")
                         .And.Contain("Failed VSTestFailTestNetCoreApp", "because netcoreapp2.0 tests will fail");
            }
            result.ExitCode.Should().Be(1);
        }

        [TestMethod][OSCondition(OperatingSystems.Windows)]
        public void XunitMultiTFM()
        {
            // Copy XunitMulti project in output directory of project dotnet-test.Tests
            string testAppName = "XunitMulti";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName, identifier: "2")
                            .WithSource()
                            .WithVersionVariables();

            var testProjectDirectory = testInstance.Path;

            // Restore project XunitMulti
            new RestoreCommand(testInstance)
                .Execute()
                .Should()
                .Pass();

            // Call test
            CommandResult result = new DotnetTestCommand(MSTestContext, disableNewOutput: true)
                                       .WithWorkingDirectory(testProjectDirectory)
                                       .Execute(ConsoleLoggerOutputNormal);

            // Verify
            if (!TestContext.IsLocalized())
            {
                // for target framework net46
                result.StdOut.Should().Contain("Total tests: 3");
                result.StdOut.Should().Contain("Passed: 2");
                result.StdOut.Should().Contain("Failed: 1");
                result.StdOut.Should().Contain("Passed TestNamespace.VSTestXunitTests.VSTestXunitPassTestDesktop");

                // for target framework netcoreapp1.0
                result.StdOut.Should().Contain("Total tests: 3");
                result.StdOut.Should().Contain("Passed: 1");
                result.StdOut.Should().Contain("Failed: 2");
                result.StdOut.Should().Contain("Failed TestNamespace.VSTestXunitTests.VSTestXunitFailTestNetCoreApp");
            }

            result.ExitCode.Should().Be(1);
        }

        [TestMethod][OSCondition(OperatingSystems.Windows)]
        public void ItCreatesMergedCoverageFileForMultiTargetedProject()
        {
            // Copy XunitMulti project in output directory of project dotnet-test.Tests
            string testAppName = "XunitMulti";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName, identifier: "3")
                            .WithSource()
                            .WithVersionVariables();

            var testProjectDirectory = testInstance.Path;

            string resultsDirectory = Path.Combine(testProjectDirectory, "RD");

            // Delete resultsDirectory if it exist
            if (Directory.Exists(resultsDirectory))
            {
                Directory.Delete(resultsDirectory, true);
            }

            // Call test
            CommandResult result = new DotnetTestCommand(MSTestContext, disableNewOutput: true, ConsoleLoggerOutputNormal)
                                       .WithWorkingDirectory(testProjectDirectory)
                                       .Execute("--collect", "Code Coverage", "--results-directory", resultsDirectory);

            // Verify
            DirectoryInfo d = new(resultsDirectory);
            FileInfo[] coverageFileInfos = d.GetFiles("*.coverage", SearchOption.AllDirectories);
            Assert.ContainsSingle(coverageFileInfos);
        }

        [TestMethod]
        public void ItCanTestAMultiTFMProjectWithImplicitRestore()
        {
            var testInstance = _testAssetsManager.CopyTestAsset(
                    "MultiTFMXunitProject",
                    testAssetSubdirectory: TestAssetSubdirectories.DesktopTestProjects)
                .WithSource();

            string projectDirectory = Path.Combine(testInstance.Path, "XUnitProject");

            new DotnetTestCommand(MSTestContext, disableNewOutput: true, ConsoleLoggerOutputNormal)
               .WithWorkingDirectory(projectDirectory)
               .Execute("--framework", ToolsetInfo.CurrentTargetFramework)
               .Should().Pass();
        }

        [TestMethod]
        public void TestSlnWithMultitargetedProject()
        {
            var libraryProject = new TestProject()
            {
                Name = "LibraryProject",
                TargetFrameworks = $"netcoreapp3.1;{ToolsetInfo.CurrentTargetFramework}",
            };

            var testProject = new TestProject()
            {
                Name = "TestProject",
                TargetFrameworks = ToolsetInfo.CurrentTargetFramework,
            };

            testProject.PackageReferences.Add(new TestPackageReference("Microsoft.NET.Test.Sdk", "16.7.1"));
            testProject.PackageReferences.Add(new TestPackageReference("xunit", "2.4.1"));
            testProject.PackageReferences.Add(new TestPackageReference("xunit.runner.visualstudio", "2.4.3", privateAssets: "all"));

            testProject.ReferencedProjects.Add(libraryProject);

            var testAsset = _testAssetsManager.CreateTestProject(testProject);

            new DotnetNewCommand(MSTestContext, "sln")
                .WithVirtualHive()
                .WithWorkingDirectory(testAsset.TestRoot)
                .Execute()
                .Should()
                .Pass();

            new DotnetCommand(MSTestContext, "sln", "add", libraryProject.Name)
                .WithWorkingDirectory(testAsset.TestRoot)
                .Execute()
                .Should()
                .Pass();

            new DotnetCommand(MSTestContext, "sln", "add", testProject.Name)
                .WithWorkingDirectory(testAsset.TestRoot)
                .Execute()
                .Should()
                .Pass();

            new DotnetTestCommand(MSTestContext, disableNewOutput: true, ConsoleLoggerOutputNormal)
               .WithWorkingDirectory(testAsset.TestRoot)
               .Execute()
               .Should().Pass();
        }
    }
}
