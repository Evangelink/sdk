// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Tests.Commands
{
    public class CommandIntegrationTests : SdkTest
    {
        public CommandIntegrationTests(MSTestContext testContext) : base(testContext) { }

        [TestMethod]
        public void GivenNoArgumentsProvided()
        {
            var cmd = new DotnetCommand(MSTestContext).Execute(string.Empty);
            cmd.StdErr.Should().BeEmpty();
        }

        [TestMethod]
        public void GivenOnlyArgumentProvidedIsDiagnosticsFlag()
        {
            var cmd = new DotnetCommand(MSTestContext).Execute("-d");
            cmd.ExitCode.Should().Be(0);
            cmd.StdErr.Should().BeEmpty();
        }
    }
}
