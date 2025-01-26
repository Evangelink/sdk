// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Cli.New.IntegrationTests
{
    [TestClass]
    public partial class DotnetNewArgumentsTests
    {
        private MSTestContext MSTestContext { get; }

        public DotnetNewArgumentsTests(MSTestContext testContext)
        {
            MSTestContext = testContext;
        }

        [TestMethod]
        public void ShowsDetailedOutputOnMissedRequiredParam()
        {
            var dotnetNewHelpOutput = new DotnetNewCommand(MSTestContext, "--help")
                .WithoutCustomHive()
                .Execute();

            new DotnetNewCommand(MSTestContext, "-v")
                .WithoutCustomHive()
                .Execute()
                .Should()
                .ExitWith(127)
                .And.HaveStdErrContaining("Required argument missing for option: '-v'")
                .And.HaveStdOutContaining(dotnetNewHelpOutput.StdOut ?? string.Empty);
        }
    }
}
