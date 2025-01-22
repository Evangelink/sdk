// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Tools.List.PackageReferences;

namespace Microsoft.DotNet.Cli.List.Package.Tests
{
    public class GivenDotnetListPackage : SdkTest
    {
        public GivenDotnetListPackage(MSTestContext testContext) : base(testContext)
        {
        }

        [TestMethod]
        public void ItShowsCoreOutputOnMinimalVerbosity()
        {
            var testAssetName = "NewtonSoftDependentProject";
            var testAsset = _testAssetsManager
                .CopyTestAsset(testAssetName)
                .WithSource();
            var projectDirectory = testAsset.Path;

            new RestoreCommand(testAsset)
                .Execute()
                .Should()
                .Pass()
                .And.NotHaveStdErr();

            new ListPackageCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute("--verbosity", "quiet")
                .Should()
                .Pass()
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("NewtonSoft.Json");
        }

        [TestMethod]
        public void RequestedAndResolvedVersionsMatch()
        {
            var testAssetName = "TestAppSimple";
            var testAsset = _testAssetsManager
                .CopyTestAsset(testAssetName)
                .WithSource();

            var projectDirectory = testAsset.Path;

            var packageName = "Newtonsoft.Json";
            var packageVersion = ToolsetInfo.GetNewtonsoftJsonPackageVersion();
            var cmd = new DotnetCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute("add", "package", packageName, "--version", packageVersion);
            cmd.Should().Pass();

            new RestoreCommand(testAsset)
                .Execute()
                .Should()
                .Pass()
                .And.NotHaveStdErr();

            new ListPackageCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should()
                .Pass()
                .And.NotHaveStdErr()
                .And.HaveStdOutContainingIgnoreSpaces(packageName + packageVersion + packageVersion);
        }

        [TestMethod]
        public void ItListsAutoReferencedPackages()
        {
            var testAssetName = "TestAppSimple";
            var testAsset = _testAssetsManager
                .CopyTestAsset(testAssetName)
                .WithSource()
                .WithProjectChanges(ChangeTargetFrameworkTo2_1);
            var projectDirectory = testAsset.Path;

            new RestoreCommand(testAsset)
                .Execute()
                .Should()
                .Pass()
                .And.NotHaveStdErr();

            new ListPackageCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should()
                .Pass()
                .And.NotHaveStdErr()
                .And.HaveStdOutContainingIgnoreSpaces("Microsoft.NETCore.App(A)")
                .And.HaveStdOutContainingIgnoreSpaces("(A):Auto-referencedpackage");

            static void ChangeTargetFrameworkTo2_1(XDocument project)
            {
                project.Descendants()
                       .Single(e => e.Name.LocalName == "TargetFramework")
                       .Value = "netcoreapp2.1";
            }
        }

        [TestMethod]
        public void ItRunOnSolution()
        {
            var sln = "TestAppWithSlnAndSolutionFolders";
            var testAsset = _testAssetsManager
                .CopyTestAsset(sln)
                .WithSource();
            var projectDirectory = testAsset.Path;

            new RestoreCommand(testAsset, "App.sln")
                .Execute()
                .Should()
                .Pass()
                .And.NotHaveStdErr();

            new ListPackageCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should()
                .Pass()
                .And.NotHaveStdErr()
                .And.HaveStdOutContainingIgnoreSpaces("NewtonSoft.Json");
        }

        [TestMethod]
        public void AssetsPathExistsButNotRestored()
        {
            var testAsset = "NewtonSoftDependentProject";
            var projectDirectory = _testAssetsManager
                .CopyTestAsset(testAsset)
                .WithSource()
                .Path;

            new ListPackageCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should()
                .Fail()
                .And.HaveStdErr();
        }

        [TestMethod]
        public void ItListsTransitivePackage()
        {
            var testProject = new TestProject
            {
                Name = "NewtonSoftDependentProject",
                TargetFrameworks = ToolsetInfo.CurrentTargetFramework,
                IsExe = true,
                SourceFiles =
                {
["Program.cs"] = @"
using System;
using System.Collections;
using Newtonsoft.Json.Linq;

class Program
{
    public static void Main(string[] args)
    {
        ArrayList argList = new ArrayList(args);
        JObject jObject = new JObject();

        foreach (string arg in argList)
        {
            jObject[arg] = arg;
        }
        Console.WriteLine(jObject.ToString());
    }
}
",
                }
            };

            testProject.PackageReferences.Add(new TestPackageReference("NewtonSoft.Json", "9.0.1"));
            var testAsset = _testAssetsManager.CreateTestProject(testProject);
            var projectDirectory = Path.Combine(testAsset.Path, testProject.Name);

            new RestoreCommand(testAsset)
                .Execute()
                .Should()
                .Pass()
                .And.NotHaveStdErr();

            new ListPackageCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should()
                .Pass()
                .And.NotHaveStdErr()
                .And.NotHaveStdOutContaining("System.IO.FileSystem");

            new ListPackageCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute(args: "--include-transitive")
                .Should()
                .Pass()
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("System.IO.FileSystem");
        }

        [TestMethod]
        [DataRow("", "[net451]", null)]
        [DataRow("", $"[{ToolsetInfo.CurrentTargetFramework}]", null)]
        [DataRow($"--framework {ToolsetInfo.CurrentTargetFramework} --framework net451", "[net451]", null)]
        [DataRow($"--framework {ToolsetInfo.CurrentTargetFramework} --framework net451", $"[{ToolsetInfo.CurrentTargetFramework}]", null)]
        [DataRow($"--framework {ToolsetInfo.CurrentTargetFramework}", $"[{ToolsetInfo.CurrentTargetFramework}]", "[net451]")]
        [DataRow("--framework net451", "[net451]", "[netcoreapp3.0]")]
        [DataRow($"-f {ToolsetInfo.CurrentTargetFramework} -f net451", "[net451]", null)]
        [DataRow($"-f {ToolsetInfo.CurrentTargetFramework} -f net451", $"[{ToolsetInfo.CurrentTargetFramework}]", null)]
        [DataRow($"-f {ToolsetInfo.CurrentTargetFramework}", $"[{ToolsetInfo.CurrentTargetFramework}]", "[net451]")]
        [DataRow("-f net451", "[net451]", "[netcoreapp3.0]")]
        public void ItListsValidFrameworks(string args, string shouldInclude, string shouldntInclude)
        {
            var testAssetName = "MSBuildAppWithMultipleFrameworks";
            var testAsset = _testAssetsManager
                .CopyTestAsset(testAssetName, identifier: args.GetHashCode().ToString() + shouldInclude)
                .WithSource();
            var projectDirectory = testAsset.Path;

            new RestoreCommand(testAsset)
                .Execute()
                .Should()
                .Pass()
                .And.NotHaveStdErr();

            if (shouldntInclude == null)
            {
                new ListPackageCommand(MSTestContext)
                    .WithWorkingDirectory(projectDirectory)
                    .Execute(args.Split(' ', options: StringSplitOptions.RemoveEmptyEntries))
                    .Should()
                    .Pass()
                    .And.NotHaveStdErr()
                    .And.HaveStdOutContainingIgnoreSpaces(shouldInclude.Replace(" ", ""));
            }
            else
            {
                new ListPackageCommand(MSTestContext)
                    .WithWorkingDirectory(projectDirectory)
                    .Execute(args.Split(' ', options: StringSplitOptions.RemoveEmptyEntries))
                    .Should()
                    .Pass()
                    .And.NotHaveStdErr()
                    .And.HaveStdOutContainingIgnoreSpaces(shouldInclude.Replace(" ", ""))
                    .And.NotHaveStdOutContaining(shouldntInclude.Replace(" ", ""));
            }

        }

        [TestMethod]
        public void ItDoesNotAcceptInvalidFramework()
        {
            var testAssetName = "MSBuildAppWithMultipleFrameworks";
            var testAsset = _testAssetsManager
                .CopyTestAsset(testAssetName)
                .WithSource();
            var projectDirectory = testAsset.Path;

            new RestoreCommand(testAsset)
                .Execute()
                .Should()
                .Pass();

            new ListPackageCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute("--framework", "invalid")
                .Should()
                .Fail();
        }

        [FullMSBuildOnlyFact]
        public void ItListsFSharpProject()
        {
            var testAssetName = "FSharpTestAppSimple";
            var testAsset = _testAssetsManager
                .CopyTestAsset(testAssetName)
                .WithSource();
            var projectDirectory = testAsset.Path;

            new RestoreCommand(testAsset)
                .Execute()
                .Should()
                .Pass()
                .And.NotHaveStdErr();

            new ListPackageCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should()
                .Pass()
                .And.NotHaveStdErr();
        }

        [TestMethod]
        [DataRow(false, "--vulnerable")]
        [DataRow(false, "--vulnerable", "--include-transitive")]
        [DataRow(false, "--vulnerable", "--include-prerelease")]
        [DataRow(false, "--deprecated", "--highest-minor")]
        [DataRow(false, "--deprecated", "--highest-patch")]
        [DataRow(false, "--outdated", "--include-prerelease")]
        [DataRow(false, "--outdated", "--highest-minor")]
        [DataRow(false, "--outdated", "--highest-patch")]
        [DataRow(false, "--config")]
        [DataRow(false, "--configfile")]
        [DataRow(false, "--source")]
        [DataRow(false, "-s")]
        [DataRow(false, "--config", "--deprecated")]
        [DataRow(false, "--configfile", "--deprecated")]
        [DataRow(false, "--source", "--vulnerable")]
        [DataRow(false, "-s", "--vulnerable")]
        [DataRow(true, "--vulnerable", "--deprecated")]
        [DataRow(true, "--vulnerable", "--outdated")]
        [DataRow(true, "--deprecated", "--outdated")]
        public void ItEnforcesOptionRules(bool throws, params string[] options)
        {
            var parseResult = Parser.Instance.Parse($"dotnet list package {string.Join(' ', options)}");
            Action checkRules = () => ListPackageReferencesCommand.EnforceOptionRules(parseResult);

            if (throws)
            {
                Assert.Throws<GracefulException>(checkRules);
            }
            else
            {
                checkRules(); // Test for no throw
            }
        }

        [UnixOnlyFact]
        public void ItRunsInCurrentDirectoryWithPoundInPath()
        {
            // Regression test for https://github.com/dotnet/sdk/issues/19654
            var testAssetName = "TestAppSimple";
            var testAsset = _testAssetsManager
                .CopyTestAsset(testAssetName, "C#")
                .WithSource();
            var projectDirectory = testAsset.Path;

            new RestoreCommand(testAsset)
                .Execute()
                .Should()
                .Pass();

            new ListPackageCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should()
                .Pass();
        }

        [TestMethod]
        public void ItRecognizesRelativePathsForAProject()
        {
            var testAssetName = "TestAppSimple";
            var testAsset = _testAssetsManager
                .CopyTestAsset(testAssetName)
                .WithSource();

            var projectDirectory = testAsset.Path;

            new RestoreCommand(testAsset)
                .Execute()
                .Should()
                .Pass();

            new ListPackageCommand(MSTestContext)
                .WithProject("TestAppSimple.csproj")
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should()
                .Pass();
        }

        [TestMethod]
        public void ItRecognizesRelativePathsForASolution()
        {
            var sln = "TestAppWithSlnAndSolutionFolders";
            var testAsset = _testAssetsManager
                .CopyTestAsset(sln)
                .WithSource();

            var projectDirectory = testAsset.Path;

            new RestoreCommand(testAsset, "App.sln")
                .Execute()
                .Should()
                .Pass();

            new ListPackageCommand(MSTestContext)
                .WithProject("App.sln")
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should()
                .Pass();
        }

        [TestMethod]
        public void ItRecognizesRelativePathsForASolutionFromSubFolder()
        {
            var sln = "TestAppWithSlnAndSolutionFolders";
            var testAsset = _testAssetsManager
                .CopyTestAsset(sln)
                .WithSource();

            var projectDirectory = testAsset.Path;

            string subFolderName = "subFolder";
            var subFolderPath = Path.Combine(projectDirectory, subFolderName);
            Directory.CreateDirectory(subFolderPath);

            new RestoreCommand(testAsset, "App.sln")
                .Execute()
                .Should()
                .Pass();

            new ListPackageCommand(MSTestContext)
                .WithProject("../App.sln")
                .WithWorkingDirectory(subFolderPath)
                .Execute()
                .Should()
                .Pass();
        }
    }
}
