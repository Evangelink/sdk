// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.DotNet.Cli.New.IntegrationTests
{
    [TestClass]
    public partial class DotnetNewSearchTests
    {
        [TestMethod]
        [DataRow("--search")]
        [DataRow("search")]
        public Task CannotExecuteEmptyCriteria(string testCase)
        {
            CommandResult commandResult = new DotnetNewCommand(MSTestContext, testCase)
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute();

            commandResult.Should().Fail();

            return Verify(commandResult.StdErr)
                .UseTextForParameters("common")
                .DisableRequireUniquePrefix();
        }

        [TestMethod]
        public Task CanShowMessageInCaseShortNameConflict()
        {
            string customHivePath = CreateTemporaryFolder(folderName: "Home");
            InstallTestTemplate("TemplateWithConflictShortName", MSTestContext, customHivePath);

            CommandResult commandResult = new DotnetNewCommand(MSTestContext, "search", "do-not-exist")
                  .WithCustomHive(customHivePath)
                  .WithoutBuiltInTemplates()
                  .Execute();

            commandResult
                .Should()
                .Fail();

            return Verify(commandResult.StdOut);
        }
    }
}
