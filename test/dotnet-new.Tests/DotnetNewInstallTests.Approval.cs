// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.DotNet.Cli.New.IntegrationTests
{
    public partial class DotnetNewInstallTests : BaseIntegrationTest
    {
        [TestMethod]
        public Task CannotInstallPackageAvailableFromBuiltIns()
        {
            CommandResult commandResult = new DotnetNewCommand(MSTestContext, "install", "Microsoft.DotNet.Common.ItemTemplates::6.0.100")
                .WithCustomHive(CreateTemporaryFolder(folderName: "Home"))
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute();

            commandResult
                .Should()
                .Fail();

            return Verify(commandResult.StdErr)
                .AddScrubber(output =>
                {
                    output.ScrubByRegex("   Microsoft\\.DotNet\\.Common\\.ItemTemplates::[A-Za-z0-9.-]+", "   Microsoft.DotNet.Common.ItemTemplates::%VERSION%");
                });
        }

        [TestMethod]
        public Task CanInstallPackageAvailableFromBuiltInsWithForce()
        {
            CommandResult commandResult = new DotnetNewCommand(MSTestContext, "install", "Microsoft.DotNet.Common.ItemTemplates::6.0.100", "--force")
                .WithCustomHive(CreateTemporaryFolder(folderName: "Home"))
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute();

            commandResult
                .Should()
                .Pass();

            return Verify(commandResult.StdOut)
                .AddScrubber(output =>
                {
                    output.ScrubByRegex("   Microsoft.DotNet.Common.ItemTemplates::[A-Za-z0-9.-]+", "   Microsoft.DotNet.Common.ItemTemplates::%VERSION%");
                });
        }

        [TestMethod]
        public Task CannotInstallMultiplePackageAvailableFromBuiltIns()
        {
            CommandResult commandResult = new DotnetNewCommand(MSTestContext, "install", "Microsoft.DotNet.Common.ItemTemplates::6.0.100", "Microsoft.DotNet.Web.ItemTemplates::5.0.0")
                .WithCustomHive(CreateTemporaryFolder(folderName: "Home"))
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute();

            commandResult
                .Should()
                .Fail();

            return Verify(commandResult.StdErr)
                .AddScrubber(output =>
                {
                    output.ScrubByRegex("   Microsoft\\.DotNet\\.Common\\.ItemTemplates::[A-Za-z0-9.-]+", "   Microsoft.DotNet.Common.ItemTemplates::%VERSION%");
                });
        }

        [TestMethod]
        [DataRow("-i")]
        [DataRow("--install")]
        public Task CanShowDeprecationMessage_WhenLegacyCommandIsUsed(string commandName)
        {
            CommandResult commandResult = new DotnetNewCommand(MSTestContext, commandName, "Microsoft.DotNet.Web.ItemTemplates::5.0.0")
                .WithCustomHive(CreateTemporaryFolder(folderName: "Home"))
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute();

            commandResult
                .Should()
                .Pass();

            return Verify(commandResult.StdOut)
                .UseTextForParameters("common")
                .DisableRequireUniquePrefix();
        }

        [TestMethod]
        public Task DoNotShowDeprecationMessage_WhenNewCommandIsUsed()
        {
            CommandResult commandResult = new DotnetNewCommand(MSTestContext, "install", "Microsoft.DotNet.Web.ItemTemplates::5.0.0")
                .WithCustomHive(CreateTemporaryFolder(folderName: "Home"))
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute();

            commandResult
                .Should()
                .Pass();

            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CanShowWarning_WhenConstraintTemplateIsInstalled()
        {
            string testTemplateLocation = GetTestTemplateLocation("Constraints/RestrictedTemplate");
            CommandResult commandResult = new DotnetNewCommand(MSTestContext, "install", testTemplateLocation)
                .WithCustomHive(CreateTemporaryFolder(folderName: "Home"))
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute();

            commandResult
                .Should()
                .Pass();

            return Verify(commandResult.StdOut)
                .AddScrubber(output =>
                {
                    output.ScrubAndReplace(testTemplateLocation, "%TEMPLATE FOLDER%");
                    output.ScrubByRegex("dotnetcli \\(version: [A-Za-z0-9.-]+\\)", "dotnetcli (version: %VERSION%)");
                });
        }

        [TestMethod]
        public Task CanInstallSameSourceTwice_Folder_WhenSourceIsSpecified()
        {
            string home = CreateTemporaryFolder(folderName: "Home");
            string basicFSharp = GetTestTemplateLocation("TemplateResolution/DifferentLanguagesGroup/BasicFSharp");
            new DotnetNewCommand(MSTestContext, "install", basicFSharp)
                .WithCustomHive(home)
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute()
                .Should()
                .ExitWith(0);

            CommandResult commandResult = new DotnetNewCommand(MSTestContext, "install", basicFSharp, "--force")
                 .WithCustomHive(home)
                 .WithWorkingDirectory(CreateTemporaryFolder())
                 .Execute();

            commandResult.Should().Pass();
            return Verify(commandResult.StdOut)
                .AddScrubber(output => output.ScrubAndReplace(basicFSharp, "%TEMPLATE FOLDER%"));
        }

        [TestMethod]
        public Task CanInstallSameSourceTwice_RemoteNuGet_WhenSourceIsSpecified()
        {
            string home = CreateTemporaryFolder(folderName: "Home");
            string basicFSharp = GetTestTemplateLocation("TemplateResolution/DifferentLanguagesGroup/BasicFSharp");
            new DotnetNewCommand(MSTestContext, "install", "Microsoft.DotNet.Common.ProjectTemplates.5.0::5.0.0")
                .WithCustomHive(home)
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute()
                .Should()
                .ExitWith(0);

            CommandResult commandResult = new DotnetNewCommand(MSTestContext, "install", "Microsoft.DotNet.Common.ProjectTemplates.5.0::5.0.0", "--force")
                 .WithCustomHive(home)
                 .WithWorkingDirectory(CreateTemporaryFolder())
                 .Execute();

            commandResult.Should().Pass();
            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CannotInstallSameSourceTwice_NuGet()
        {
            string home = CreateTemporaryFolder(folderName: "Home");

            new DotnetNewCommand(MSTestContext, "install", "Microsoft.DotNet.Common.ProjectTemplates.5.0::5.0.0")
                .WithCustomHive(home)
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("console")
                .And.HaveStdOutContaining("classlib");

            CommandResult commandResult = new DotnetNewCommand(MSTestContext, "install", "Microsoft.DotNet.Common.ProjectTemplates.5.0::5.0.0")
                 .WithCustomHive(home)
                 .WithWorkingDirectory(CreateTemporaryFolder())
                 .Execute();

            commandResult.Should().Fail();
            return Verify(commandResult.StdErr);
        }

        [TestMethod]
        public Task CannotInstallSameSourceTwice_Folder()
        {
            string home = CreateTemporaryFolder(folderName: "Home");
            string basicFSharp = GetTestTemplateLocation("TemplateResolution/DifferentLanguagesGroup/BasicFSharp");
            new DotnetNewCommand(MSTestContext, "install", basicFSharp)
                .WithCustomHive(home)
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("basic");

            new DotnetNewCommand(MSTestContext, "install", basicFSharp)
                 .WithCustomHive(home)
                 .WithWorkingDirectory(CreateTemporaryFolder())
                 .Execute()
                 .Should().Fail()
                 .And.HaveStdErrContaining($"{basicFSharp} is already installed");

            CommandResult commandResult = new DotnetNewCommand(MSTestContext, "install", basicFSharp)
                .WithCustomHive(home)
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute();

            commandResult.Should().Fail();
            return Verify(commandResult.StdErr)
                .AddScrubber(output => output.ScrubAndReplace(basicFSharp, "%TEMPLATE FOLDER%"));
        }

        [TestMethod]
        public Task CanShowMessageInCaseShortNameConflict()
        {
            string customHivePath = CreateTemporaryFolder(folderName: "Home");
            string templateToInstall = GetTestTemplateLocation("TemplateWithConflictShortName");

            CommandResult commandResult = new DotnetNewCommand(MSTestContext, "install", templateToInstall)
                  .WithCustomHive(customHivePath)
                  .WithoutBuiltInTemplates()
                  .Execute();

            commandResult
                .Should()
                .Pass();

            return Verify(commandResult.StdOut)
                .AddScrubber(output => output.ScrubAndReplace(templateToInstall, "%TEMPLATE FOLDER%"));
        }

        [TestMethod]
        public Task CanShowError_WhenGlobalSettingsFileIsCorrupted()
        {
            string homeDirectory = CreateTemporaryFolder();
            InstallTestTemplate("TemplateWithRequiredParameters", MSTestContext, homeDirectory);

            var globalSettingsFile = Path.Combine(homeDirectory, "packages.json");
            File.WriteAllText(globalSettingsFile, string.Empty);

            string templateToInstall = GetTestTemplateLocation("TemplateWithTags");
            CommandResult commandResult = new DotnetNewCommand(MSTestContext, "install", templateToInstall)
                .WithCustomHive(homeDirectory)
                .Execute();

            return Verify(commandResult.StdOut)
                .AddScrubber(
                output =>
                {
                    output.ScrubAndReplace(templateToInstall, "%TEMPLATE FOLDER%");
                    output.ScrubAndReplace(globalSettingsFile, "%GLOBAL SETTINGS FILE%");
                }
                );
        }
    }
}
