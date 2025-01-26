// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Cli.Fsi.Tests
{
    [TestClass]
    public class GivenDotnetFsiExecutesAndGeneratesHelpText : SdkTest
    {
        public GivenDotnetFsiExecutesAndGeneratesHelpText(MSTestContext testContext) : base(testContext)
        {
        }

        [TestMethod]
        public void ItRuns()
        {
            new DotnetCommand(MSTestContext, "fsi")
                .Execute("--help")
                .Should().Pass();
        }
    }
}
