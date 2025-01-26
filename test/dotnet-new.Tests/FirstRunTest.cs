// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Cli.New.IntegrationTests
{
    [TestClass]
    public class FirstRunTest : BaseIntegrationTest
    {
        public FirstRunTest(MSTestContext testContext) : base(testContext)
        {
        }

        [TestMethod]
        public void FirstRunSuccess()
        {
            string home = CreateTemporaryFolder(folderName: "Home");
            new DotnetNewCommand(MSTestContext)
                .WithCustomHive(home)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.NotHaveStdOutContaining("Error");

            new DotnetNewCommand(MSTestContext, "--list")
                .WithCustomHive(home)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("classlib");
        }
    }
}
