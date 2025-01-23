// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Cli.New.IntegrationTests
{
    public class FirstRunTest : BaseIntegrationTest
    {
        private readonly MSTestContext _testContext;

        public FirstRunTest(MSTestContext testContext) : base(testContext)
        {
            _testContext = testContext;
        }

        [TestMethod]
        public void FirstRunSuccess()
        {
            string home = CreateTemporaryFolder(folderName: "Home");
            new DotnetNewCommand(_testContext)
                .WithCustomHive(home)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.NotHaveStdOutContaining("Error");

            new DotnetNewCommand(_testContext, "--list")
                .WithCustomHive(home)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("classlib");
        }
    }
}
