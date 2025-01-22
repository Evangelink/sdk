// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace EndToEnd.Tests
{
    public class GivenDotnetUsesDotnetTools(MSTestContext testContext) : SdkTest(testContext)
    {
        [TestMethod]
        public void ThenOneDotnetToolsCanBeCalled()
        {
            new DotnetCommand(MSTestContext)
                .Execute("dev-certs", "--help")
                .Should().Pass();
        }
    }
}
