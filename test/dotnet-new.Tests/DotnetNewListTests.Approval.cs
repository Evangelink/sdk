// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.DotNet.Cli.New.IntegrationTests
{
    public partial class DotnetNewListTests
    {
#pragma warning disable xUnit1004 // Test methods should not be skipped
        [TestMethod(IgnoreMessage = "https://github.com/dotnet/sdk/issues/45406")]
        [DataRow("-l")]
        [DataRow("--list")]
        public Task BasicTest_WhenLegacyCommandIsUsed(string commandName)
        {
            CommandResult commandResult = new DotnetNewCommand(_testContext, commandName)
                .WithCustomHive(_sharedHome.HomeDirectory)
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute();

            commandResult
                .Should()
                .Pass();

            return Verify(commandResult.StdOut)
                .UniqueForOSPlatform()
                .UseTextForParameters("common")
                .DisableRequireUniquePrefix();
        }

        [TestMethod(IgnoreMessage = "https://github.com/dotnet/sdk/issues/45406")]
        public Task BasicTest_WhenListCommandIsUsed()
        {
            CommandResult commandResult = new DotnetNewCommand(_testContext, "list")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute();

            commandResult
                .Should()
                .Pass();

            return Verify(commandResult.StdOut).UniqueForOSPlatform();
        }
#pragma warning restore xUnit1004

        [TestMethod]
        public Task Constraints_CanShowMessageIfTemplateGroupIsRestricted()
        {
            string customHivePath = CreateTemporaryFolder(folderName: "Home");
            InstallTestTemplate("Constraints/RestrictedTemplate", _testContext, customHivePath);
            InstallTestTemplate("TemplateWithSourceName", _testContext, customHivePath);

            CommandResult commandResult = new DotnetNewCommand(_testContext, "list", "RestrictedTemplate")
                  .WithCustomHive(customHivePath)
                  .Execute();

            commandResult
                .Should()
                .Fail();

            return Verify(commandResult.StdErr);
        }

        [TestMethod]
        public Task Constraints_CanIgnoreConstraints()
        {
            string customHivePath = CreateTemporaryFolder(folderName: "Home");
            InstallTestTemplate("Constraints/RestrictedTemplate", _testContext, customHivePath);
            InstallTestTemplate("TemplateWithSourceName", _testContext, customHivePath);

            CommandResult commandResult = new DotnetNewCommand(_testContext, "list", "RestrictedTemplate", "--ignore-constraints")
                  .WithCustomHive(customHivePath)
                  .Execute();

            commandResult
                .Should()
                .Pass();

            return Verify(commandResult.StdOut);
        }

        [TestMethod]
        public Task CanShowMessageInCaseShortNameConflict()
        {
            string customHivePath = CreateTemporaryFolder(folderName: "Home");
            InstallTestTemplate("TemplateWithConflictShortName", _testContext, customHivePath);

            CommandResult commandResult = new DotnetNewCommand(_testContext, "list")
                  .WithCustomHive(customHivePath)
                  .WithoutBuiltInTemplates()
                  .Execute();

            commandResult
                .Should()
                .Pass();

            return Verify(commandResult.StdOut);
        }
    }
}
