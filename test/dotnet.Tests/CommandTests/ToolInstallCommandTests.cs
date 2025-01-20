// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Tools.Tool.Install;
using Microsoft.DotNet.Tools.Tool.Run;
using LocalizableStrings = Microsoft.DotNet.Tools.Tool.Install.LocalizableStrings;
using Parser = Microsoft.DotNet.Cli.Parser;

namespace Microsoft.DotNet.Tests.Commands.Tool
{
    public class ToolInstallCommandTests : SdkTest
    {
        private const string PackageId = "global.tool.console.demo";

        public ToolInstallCommandTests(MSTestContext testContext) : base(testContext)
        {
        }

        [TestMethod]
        public void WhenRunWithBothGlobalAndToolPathShowErrorMessage()
        {
            var parseResult = Parser.Instance.Parse($"dotnet tool install -g --tool-path /tmp/folder {PackageId}");

            var toolInstallCommand = new ToolInstallCommand(
                parseResult);

            Action a = () => toolInstallCommand.Execute();

            a.Should().Throw<GracefulException>().And.Message
                .Should().Contain(string.Format(
                    LocalizableStrings.InstallToolCommandInvalidGlobalAndLocalAndToolPath,
                    "--global --tool-path"));
        }

        [TestMethod(IgnoreMessage = "https://github.com/dotnet/sdk/issues/42346")]
        public void WhenRunWithRoot()
        {
            Directory.CreateDirectory("/tmp/folder/sub");
            var directory = Directory.GetCurrentDirectory();
            var ridGraphPath = TestContext.GetRuntimeGraphFilePath();
            try
            {
                Directory.SetCurrentDirectory("/tmp/folder");

                new DotnetNewCommand(Log, "tool-manifest").WithCustomHive("/tmp/folder").WithWorkingDirectory("/tmp/folder").Execute().Should().Pass();
                var parseResult = Parser.Instance.Parse("tool install dotnetsay");
                new ToolInstallLocalCommand(parseResult, runtimeJsonPathForTests: ridGraphPath).Execute().Should().Be(0);

                Directory.SetCurrentDirectory("/tmp/folder/sub");
                new DotnetNewCommand(Log, "tool-manifest").WithCustomHive("/tmp/folder/sub").WithWorkingDirectory("/tmp/folder/sub").Execute().Should().Pass();
                parseResult = Parser.Instance.Parse("tool install dotnetsay");
                new ToolInstallLocalCommand(parseResult, runtimeJsonPathForTests: ridGraphPath).Execute().Should().Be(0);

                new ToolRunCommand(Parser.Instance.Parse($"tool run dotnetsay")).Execute().Should().Be(0);
            }
            finally
            {
                Directory.SetCurrentDirectory(directory);
            }
        }

        [TestMethod]
        public void WhenRunWithBothGlobalAndLocalShowErrorMessage()
        {
            var parseResult = Parser.Instance.Parse(
                new[] { "dotnet", "tool", "install", "--local", "--tool-path", "/tmp/folder", PackageId });

            var toolInstallCommand = new ToolInstallCommand(
                parseResult);

            Action a = () => toolInstallCommand.Execute();

            a.Should().Throw<GracefulException>().And.Message
                .Should().Contain(
                    string.Format(LocalizableStrings.InstallToolCommandInvalidGlobalAndLocalAndToolPath,
                        "--local --tool-path"));
        }

        [TestMethod]
        public void WhenRunWithGlobalAndToolManifestShowErrorMessage()
        {
            var parseResult = Parser.Instance.Parse(
                new[] { "dotnet", "tool", "install", "-g", "--tool-manifest", "folder/my-manifest.format", "PackageId" });

            var toolInstallCommand = new ToolInstallCommand(
                parseResult);

            Action a = () => toolInstallCommand.Execute();

            a.Should().Throw<GracefulException>().And.Message
                .Should().Contain(Tools.Tool.Common.LocalizableStrings.OnlyLocalOptionSupportManifestFileOption);
        }

        [TestMethod]
        public void WhenRunWithToolPathAndToolManifestShowErrorMessage()
        {
            var parseResult = Parser.Instance.Parse(
                new[]
                {
                    "dotnet", "tool", "install", "--tool-path", "/tmp/folder", "--tool-manifest", "folder/my-manifest.format", PackageId
                });

            var toolInstallCommand = new ToolInstallCommand(
                parseResult);

            Action a = () => toolInstallCommand.Execute();

            a.Should().Throw<GracefulException>().And.Message
                .Should().Contain(Tools.Tool.Common.LocalizableStrings.OnlyLocalOptionSupportManifestFileOption);
        }

        [TestMethod]
        public void WhenRunWithLocalAndFrameworkShowErrorMessage()
        {
            var parseResult = Parser.Instance.Parse(
                new[]
                {
                    "dotnet", "tool", "install", PackageId, "--framework", ToolsetInfo.CurrentTargetFramework
                });

            var toolInstallCommand = new ToolInstallCommand(
                parseResult);

            Action a = () => toolInstallCommand.Execute();

            a.Should().Throw<GracefulException>().And.Message
                .Should().Contain(LocalizableStrings.LocalOptionDoesNotSupportFrameworkOption);
        }
    }
}
