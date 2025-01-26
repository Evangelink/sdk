// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.Sln.Internal;
using Microsoft.DotNet.Tools;
using Microsoft.DotNet.Tools.Common;
using CommandLocalizableStrings = Microsoft.DotNet.Tools.Sln.LocalizableStrings;

namespace Microsoft.DotNet.Cli.Sln.List.Tests
{
    [TestClass]
    public class GivenDotnetSlnList : SdkTest
    {
        private Func<string, string> HelpText = (defaultVal) => $@"Description:
  List all projects in a solution file.

Usage:
  dotnet solution <SLN_FILE> list [options]

Arguments:
  <SLN_FILE>    The solution file to operate on. If not specified, the command will search the current directory for one. [default: {PathUtility.EnsureTrailingSlash(defaultVal)}]

Options:
  --solution-folders  Display solution folder paths.
  -?, -h, --help    Show command line help.";


        public GivenDotnetSlnList(MSTestContext testContext) : base(testContext)
        {
        }

        [TestMethod]
        [DataRow("sln", "--help")]
        [DataRow("sln", "-h")]
        [DataRow("solution", "--help")]
        [DataRow("solution", "-h")]
        public void WhenHelpOptionIsPassedItPrintsUsage(string solutionCommand, string helpArg)
        {
            var cmd = new DotnetCommand(MSTestContext)
                .Execute(solutionCommand, "list", helpArg);
            cmd.Should().Pass();
            cmd.StdOut.Should().BeVisuallyEquivalentToIfNotLocalized(HelpText(Directory.GetCurrentDirectory()));
        }

        [TestMethod]
        [DataRow("sln", "")]
        [DataRow("sln", "unknownCommandName")]
        [DataRow("solution", "")]
        [DataRow("solution", "unknownCommandName")]
        public void WhenNoCommandIsPassedItPrintsError(string solutionCommand, string commandName)
        {
            var cmd = new DotnetCommand(MSTestContext)
                .Execute(solutionCommand, commandName);
            cmd.Should().Fail();
            cmd.StdErr.Should().Be(CommonLocalizableStrings.RequiredCommandNotPassed);
        }

        [TestMethod]
        [DataRow("sln")]
        [DataRow("solution")]
        public void WhenTooManyArgumentsArePassedItPrintsError(string solutionCommand)
        {
            var cmd = new DotnetCommand(MSTestContext)
                .Execute(solutionCommand, "one.sln", "two.sln", "three.sln", "list");
            cmd.Should().Fail();
            cmd.StdErr.Should().BeVisuallyEquivalentTo($@"{string.Format(CommandLineValidation.LocalizableStrings.UnrecognizedCommandOrArgument, "two.sln")}
{string.Format(CommandLineValidation.LocalizableStrings.UnrecognizedCommandOrArgument, "three.sln")}");
        }

        [TestMethod]
        [DataRow("sln", "idontexist.sln")]
        [DataRow("sln", "ihave?invalidcharacters.sln")]
        [DataRow("sln", "ihaveinv@lidcharacters.sln")]
        [DataRow("sln", "ihaveinvalid/characters")]
        [DataRow("sln", "ihaveinvalidchar\\acters")]
        [DataRow("solution", "idontexist.sln")]
        [DataRow("solution", "ihave?invalidcharacters.sln")]
        [DataRow("solution", "ihaveinv@lidcharacters.sln")]
        [DataRow("solution", "ihaveinvalid/characters")]
        [DataRow("solution", "ihaveinvalidchar\\acters")]
        public void WhenNonExistingSolutionIsPassedItPrintsErrorAndUsage(string solutionCommand, string solutionName)
        {
            var cmd = new DotnetCommand(MSTestContext)
                .Execute(solutionCommand, solutionName, "list");
            cmd.Should().Fail();
            cmd.StdErr.Should().Be(string.Format(CommonLocalizableStrings.CouldNotFindSolutionOrDirectory, solutionName));
            cmd.StdOut.Should().BeVisuallyEquivalentToIfNotLocalized("");
        }

        [TestMethod]
        [DataRow("sln")]
        [DataRow("solution")]
        public void WhenInvalidSolutionIsPassedItPrintsErrorAndUsage(string solutionCommand)
        {
            var projectDirectory = _testAssetsManager
                .CopyTestAsset("InvalidSolution", identifier: $"GivenDotnetSlnList-InvalidSolutionPassed-{solutionCommand}")
                .WithSource()
                .Path;

            var cmd = new DotnetCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute(solutionCommand, "InvalidSolution.sln", "list");
            cmd.Should().Fail();
            cmd.StdErr.Should().Contain(
                string.Format(CommonLocalizableStrings.InvalidSolutionFormatString, Path.Combine(projectDirectory, "InvalidSolution.sln"), "").TrimEnd('.'));
            cmd.StdOut.Should().BeVisuallyEquivalentToIfNotLocalized("");
        }

        [TestMethod]
        [DataRow("sln", ".sln")]
        [DataRow("solution", ".sln")]
        [DataRow("sln", ".slnx")]
        [DataRow("solution", ".slnx")]
        public void WhenInvalidSolutionIsFoundListPrintsErrorAndUsage(string solutionCommand, string solutionExtension)
        {
            var projectRootDirectory = _testAssetsManager
                .CopyTestAsset("InvalidSolution", identifier: $"GivenDotnetSlnList-InvalidSolutionFound-{solutionCommand}{solutionExtension}")
                .WithSource()
                .Path;

            var projectDirectory = solutionExtension == ".sln"
                ? Path.Join(projectRootDirectory, "Sln")
                : Path.Join(projectRootDirectory, "Slnx");

            var solutionFullPath = Path.Combine(projectDirectory, $"InvalidSolution{solutionExtension}");
            var cmd = new DotnetCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute(solutionCommand, "list");
            cmd.Should().Fail();
            cmd.StdErr.Should().Contain(
                string.Format(CommonLocalizableStrings.InvalidSolutionFormatString, solutionFullPath, "").TrimEnd('.'));
            cmd.StdOut.Should().BeVisuallyEquivalentToIfNotLocalized("");
        }

        [TestMethod]
        [DataRow("sln")]
        [DataRow("solution")]
        public void WhenNoSolutionExistsInTheDirectoryListPrintsErrorAndUsage(string solutionCommand)
        {
            var projectDirectory = _testAssetsManager
                .CopyTestAsset("TestAppWithSlnAndCsprojFiles", identifier: $"GivenDotnetSlnList-{solutionCommand}")
                .WithSource()
                .Path;

            var solutionDir = Path.Combine(projectDirectory, "App");
            var cmd = new DotnetCommand(MSTestContext)
                .WithWorkingDirectory(solutionDir)
                .Execute(solutionCommand, "list");
            cmd.Should().Fail();
            cmd.StdErr.Should().Be(string.Format(CommonLocalizableStrings.SolutionDoesNotExist, solutionDir + Path.DirectorySeparatorChar));
            cmd.StdOut.Should().BeVisuallyEquivalentToIfNotLocalized("");
        }

        [TestMethod]
        [DataRow("sln")]
        [DataRow("solution")]
        public void WhenMoreThanOneSolutionExistsInTheDirectoryItPrintsErrorAndUsage(string solutionCommand)
        {
            var projectDirectory = _testAssetsManager
                .CopyTestAsset("TestAppWithMultipleSlnFiles", identifier: $"GivenDotnetSlnList-{solutionCommand}")
                .WithSource()
                .Path;

            var cmd = new DotnetCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute(solutionCommand, "list");
            cmd.Should().Fail();
            cmd.StdErr.Should().Be(string.Format(CommonLocalizableStrings.MoreThanOneSolutionInDirectory, projectDirectory + Path.DirectorySeparatorChar));
            cmd.StdOut.Should().BeVisuallyEquivalentToIfNotLocalized("");
        }

        [TestMethod]
        [DataRow("sln", ".sln")]
        [DataRow("solution", ".sln")]
        [DataRow("sln", ".slnx")]
        [DataRow("solution", ".slnx")]
        public void WhenNoProjectsArePresentInTheSolutionItPrintsANoProjectMessage(string solutionCommand, string solutionExtension)
        {
            var projectDirectory = _testAssetsManager
                .CopyTestAsset("TestAppWithEmptySln", identifier: $"GivenDotnetSlnList-{solutionCommand}{solutionExtension}")
                .WithSource()
                .Path;

            var cmd = new DotnetCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute(solutionCommand, $"App{solutionExtension}", "list");
            cmd.Should().Pass();
            cmd.StdOut.Should().Be(CommonLocalizableStrings.NoProjectsFound);
        }

        [TestMethod]
        [DataRow("sln", ".sln")]
        [DataRow("solution", ".sln")]
        [DataRow("sln", ".slnx")]
        [DataRow("solution", ".slnx")]
        public void WhenProjectsPresentInTheSolutionItListsThem(string solutionCommand, string solutionExtension)
        {
            var expectedOutput = $@"{CommandLocalizableStrings.ProjectsHeader}
{new string('-', CommandLocalizableStrings.ProjectsHeader.Length)}
{Path.Combine("App", "App.csproj")}
{Path.Combine("Lib", "Lib.csproj")}";

            var projectDirectory = _testAssetsManager
                .CopyTestAsset("TestAppWithSlnAndExistingCsprojReferences", identifier: $"GivenDotnetSlnList-{solutionCommand}{solutionExtension}")
                .WithSource()
                .Path;

            var cmd = new DotnetCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute(solutionCommand, $"App{solutionExtension}", "list");
            cmd.Should().Pass();
            cmd.StdOut.Should().BeVisuallyEquivalentTo(expectedOutput);
        }

        [TestMethod]
        [DataRow("sln", ".sln")]
        [DataRow("solution", ".sln")]
        [DataRow("sln", ".slnx")]
        [DataRow("solution", ".slnx")]
        public void WhenProjectsPresentInTheReadonlySolutionItListsThem(string solutionCommand, string solutionExtension)
        {
            var expectedOutput = $@"{CommandLocalizableStrings.ProjectsHeader}
{new string('-', CommandLocalizableStrings.ProjectsHeader.Length)}
{Path.Combine("App", "App.csproj")}
{Path.Combine("Lib", "Lib.csproj")}";

            var projectDirectory = _testAssetsManager
                .CopyTestAsset("TestAppWithSlnAndExistingCsprojReferences", identifier: $"GivenDotnetSlnList-Readonly-{solutionCommand}{solutionExtension}")
                .WithSource()
                .Path;

            var slnFileName = Path.Combine(projectDirectory, $"App{solutionExtension}");
            var attributes = File.GetAttributes(slnFileName);
            File.SetAttributes(slnFileName, attributes | FileAttributes.ReadOnly);

            var cmd = new DotnetCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute(solutionCommand, $"App{solutionExtension}", "list");
            cmd.Should().Pass();
            cmd.StdOut.Should().BeVisuallyEquivalentTo(expectedOutput);
        }

        [TestMethod]
        [DataRow("sln", ".sln")]
        [DataRow("solution", ".sln")]
        [DataRow("sln", ".slnx")]
        [DataRow("solution", ".slnx")]
        public void WhenProjectsInSolutionFoldersPresentInTheSolutionItListsSolutionFolderPaths(string solutionCommand, string solutionExtension)
        {
            string[] expectedOutput = { $"{CommandLocalizableStrings.SolutionFolderHeader}",
$"{new string('-', CommandLocalizableStrings.SolutionFolderHeader.Length)}",
$"{Path.Combine("NestedSolution", "NestedFolder", "NestedFolder")}" };

            var projectDirectory = _testAssetsManager
                .CopyTestAsset("SlnFileWithSolutionItemsInNestedFolders", identifier: $"GivenDotnetSlnList-{solutionCommand}")
                .WithSource()
                .Path;

            var cmd = new DotnetCommand(MSTestContext)
                .WithWorkingDirectory(projectDirectory)
                .Execute(solutionCommand, $"App{solutionExtension}", "list", "--solution-folders");
            cmd.Should().Pass();
            cmd.StdOut.Should().ContainAll(expectedOutput);
        }
    }
}
