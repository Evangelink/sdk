// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace EndToEnd.Tests
{
    [TestClass]
    public class GivenNetFrameworkSupportsNetStandard2(MSTestContext testContext) : SdkTest(testContext)
    {
        [TestMethod]
        [OSCondition(OperatingSystems.Windows)]
        public void Anet462ProjectCanReferenceANETStandardProject()
        {
            var _testInstance = _testAssetsManager
                .CopyTestAsset("NETFrameworkReferenceNETStandard20", testAssetSubdirectory: TestAssetSubdirectories.DesktopTestProjects)
                .WithSource();

            string projectDirectory = Path.Combine(_testInstance.Path, "TestApp");

            new BuildCommand(MSTestContext, projectDirectory)
                .Execute()
                .Should().Pass();

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should().Pass()
                    .And.HaveStdOutContaining("This string came from the test library!");

        }
    }
}
