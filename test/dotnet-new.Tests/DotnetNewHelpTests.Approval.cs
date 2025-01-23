// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.DotNet.Cli.New.IntegrationTests
{
    public partial class DotnetNewHelpTests : BaseIntegrationTest
    {
        [TestMethod]
        [DataRow("-h")]
        [DataRow("/h")]
        [DataRow("--help")]
        [DataRow("-?")]
        [DataRow("/?")]
        public Task CanShowHelp(string command)
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, command)
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult.Should().ExitWith(0)
                .And.NotHaveStdErr();

            return Verify(commandResult.StdOut)
                .UseTextForParameters("common")
                .DisableRequireUniquePrefix();
        }

        [TestMethod]
        [DataRow("-h")]
        [DataRow("--help")]
        public Task CanShowHelp_Create(string option)
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "create", option)
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult.Should().ExitWith(0)
                .And.NotHaveStdErr();

            return Verify(commandResult.StdOut)
                .UseTextForParameters("common")
                .DisableRequireUniquePrefix();
        }

        [TestMethod]
        [DataRow("-h")]
        [DataRow("--help")]
        public Task CanShowHelp_Install(string option)
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "install", option)
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult.Should().ExitWith(0)
                .And.NotHaveStdErr();

            return Verify(commandResult.StdOut)
                .UseTextForParameters("common")
                .DisableRequireUniquePrefix();
        }

        [TestMethod]
        [DataRow("-h")]
        [DataRow("--help")]
        public Task CanShowHelp_Update(string option)
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "update", option)
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult.Should().ExitWith(0)
                .And.NotHaveStdErr();

            return Verify(commandResult.StdOut)
                .UseTextForParameters("common")
                .DisableRequireUniquePrefix();
        }

        [TestMethod]
        [DataRow("-h")]
        [DataRow("--help")]
        public Task CanShowHelp_Uninstall(string option)
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "uninstall", option)
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult.Should().ExitWith(0)
                .And.NotHaveStdErr();

            return Verify(commandResult.StdOut)
                 .UseTextForParameters("common")
                .DisableRequireUniquePrefix();
        }

        [TestMethod]
        [DataRow("-h")]
        [DataRow("--help")]
        public Task CanShowHelp_List(string option)
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "list", option)
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult.Should().ExitWith(0)
                .And.NotHaveStdErr();

            return Verify(commandResult.StdOut)
                .UseTextForParameters("common")
                .DisableRequireUniquePrefix();
        }

        [TestMethod]
        [DataRow("-h")]
        [DataRow("--help")]
        public Task CanShowHelp_Search(string option)
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "search", option)
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult.Should().ExitWith(0)
                .And.NotHaveStdErr();

            return Verify(commandResult.StdOut)
                .UseTextForParameters("common")
                .DisableRequireUniquePrefix();
        }

        [TestMethod]
        [DataRow("console -h", "console")]
        [DataRow("console --help", "console")]
        [DataRow("classlib -h", "classlib")]
        [DataRow("classlib --help", "classlib")]
        [DataRow("globaljson -h", "globaljson")]
        public Task CanShowHelpForTemplate(string command, string setName)
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, command.Split(" "))
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.NotHaveStdOutContaining("Usage: new [options]");

            return Verify(commandResult.StdOut)
                .UseTextForParameters(setName)
                .DisableRequireUniquePrefix();
        }

        [TestMethod]
        public Task CannotShowHelpForTemplate_PartialNameMatch()
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "classli", "-h")
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult.Should().Pass().And.NotHaveStdErr();
            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CannotShowHelpForTemplate_FullNameMatch()
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "Console App", "-h")
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            //help command cannot fail, therefore the output is written to stdout
            commandResult.Should().Pass().And.NotHaveStdErr();
            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CannotShowHelpForTemplate_WhenAmbiguousLanguageChoice()
        {
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate("TemplateResolution/DifferentLanguagesGroup/BasicFSharp", _testContext, _fixture.HomeDirectory, workingDirectory);
            InstallTestTemplate("TemplateResolution/DifferentLanguagesGroup/BasicVB", _testContext, _fixture.HomeDirectory, workingDirectory);

            CommandResult commandResult = new DotnetNewCommand(_testContext, "basic", "--help")
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            //help command cannot fail, therefore the output is written to stdout
            commandResult.Should().Pass().And.NotHaveStdErr();
            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CanShowHelpForTemplate_MultipleValueChoice()
        {
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate("TemplateWithMultiValueChoice", _testContext, _fixture.HomeDirectory, workingDirectory);

            CommandResult commandResult = new DotnetNewCommand(_testContext, "TestAssets.TemplateWithMultiValueChoice", "--help")
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            //help command should not fail, therefore the output is written to stdout
            commandResult.Should().Pass().And.NotHaveStdErr();
            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CanShowHelpForTemplate_MatchOnChoice()
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "console", "--help", "--framework", "net7.0")
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult
                .Should().Pass()
                .And.NotHaveStdErr()
                .And.NotHaveStdOutContaining("Usage: new [options]");

            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CannotShowHelpForTemplate_MatchOnChoiceWithoutValue()
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "console", "--help", "--framework")
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            //help command cannot fail, therefore the output is written to stdout
            commandResult.Should().Pass().And.NotHaveStdErr();
            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CannotShowHelpForTemplate_MatchOnUnexistingParam()
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "console", "--help", "--do-not-exist")
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            //help command cannot fail, therefore the output is written to stdout
            commandResult.Should().Pass().And.NotHaveStdErr();
            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CanShowHelpForTemplate_MatchOnNonChoiceParam()
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "console", "--help", "--langVersion", "8.0")
                    .WithCustomHive(_fixture.HomeDirectory)
                    .WithWorkingDirectory(workingDirectory)
                    .Execute();

            //help command cannot fail, therefore the output is written to stdout
            commandResult.Should().Pass().And.NotHaveStdErr().And.NotHaveStdOutContaining("Usage: new [options]");
            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CanShowHelpForTemplate_MatchOnLanguage()
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "console", "--help", "--language", "F#")
                    .WithCustomHive(_fixture.HomeDirectory)
                    .WithWorkingDirectory(workingDirectory)
                    .Execute();

            commandResult
                    .Should().Pass()
                    .And.NotHaveStdErr()
                    .And.NotHaveStdOutContaining("Usage: new [options]");

            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CannotShowHelpForTemplate_MatchOnNonChoiceParamWithoutValue()
        {
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_testContext, "console", "--help", "--langVersion")
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            //help command cannot fail, therefore the testContext is written to stdout
            commandResult.Should().Pass().And.NotHaveStdErr();
            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CanShowAllowScriptsOption()
        {
            string templateLocation = "PostActions/RunScript/Basic";
            string templateName = "TestAssets.PostActions.RunScript.Basic";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, _testContext, home, workingDirectory);

            CommandResult commandResult = new DotnetNewCommand(_testContext, templateName, "--help")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult.Should().Pass().And.NotHaveStdErr();
            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CanShowHelpForTemplate_RequiredParams()
        {
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate("TemplateWithRequiredParameters", _testContext, _fixture.HomeDirectory, workingDirectory);

            CommandResult commandResult = new DotnetNewCommand(_testContext, "TestAssets.TemplateWithRequiredParameters", "--help")
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr();

            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CanShowHelpForTemplate_ConditionalParams()
        {
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate("TemplateWithConditionalParameters", _testContext, _fixture.HomeDirectory, workingDirectory);

            CommandResult commandResult = new DotnetNewCommand(_testContext, "TestAssets.TemplateWithConditionalParameters", "--help")
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr();

            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CanShowHelpForTemplateWhenRequiredParamIsMissed()
        {
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate($"TemplateResolution/MissedRequiredParameter/BasicTemplate1", _testContext, _fixture.HomeDirectory, workingDirectory);

            CommandResult commandResult = new DotnetNewCommand(_testContext, "basic", "--help")
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr();

            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CanShowHelpForTemplateWhenRequiredParamIsMissedAndConditionIntroduced()
        {
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate($"TemplateResolution/MissedRequiredParameter/BasicTemplate2", _testContext, _fixture.HomeDirectory, workingDirectory);

            CommandResult commandResult = new DotnetNewCommand(_testContext, "basic2", "--help")
                .WithCustomHive(_fixture.HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr();

            return Verify(commandResult.StdOut);
        }
    }
}
