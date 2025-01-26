// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Cli.Run.Tests
{
    [TestClass]
    public class GivenThatWeCanPassNonProjectFilesToDotnetRun : SdkTest
    {
        public GivenThatWeCanPassNonProjectFilesToDotnetRun(MSTestContext testContext) : base(testContext)
        {
        }

        [TestMethod]
        public void ItFailsWithAnAppropriateErrorMessage()
        {
            var projectDirectory = _testAssetsManager
                .CopyTestAsset("SlnFileWithNoProjectReferences")
                .WithSource()
                .Path;

            var slnFullPath = Path.Combine(projectDirectory, "SlnFileWithNoProjectReferences.sln");

            new DotnetCommand(MSTestContext, "run")
                .Execute($"-p", slnFullPath)
                .Should().Fail()
                .And.HaveStdErrContaining(
                    string.Format(
                        Tools.Run.LocalizableStrings.RunCommandSpecifiedFileIsNotAValidProject,
                        slnFullPath));
        }
    }
}
