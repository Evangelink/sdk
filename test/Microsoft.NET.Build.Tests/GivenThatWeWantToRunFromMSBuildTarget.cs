// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.NET.Build.Tests
{
    public class GivenThatWeWantToRunFromMSBuildTarget : SdkTest
    {
        public GivenThatWeWantToRunFromMSBuildTarget(MSTestContext testContext) : base(testContext)
        {
        }

        [TestMethod]
        public void It_runs_successfully()
        {
            TestProject testProject = new()
            {
                Name = "TestRunTargetProject",
                IsExe = true,
                TargetFrameworks = ToolsetInfo.CurrentTargetFramework
            };

            var testAsset = _testAssetsManager.CreateTestProject(testProject);

            var buildCommand = new BuildCommand(testAsset);
            buildCommand
                .Execute()
                .Should()
                .Pass();

            var runTargetCommand = new MSBuildCommand(MSTestContext, "run", Path.Combine(testAsset.TestRoot, testProject.Name));
            runTargetCommand
                .Execute()
                .Should()
                .Pass()
                .And.HaveStdOutContaining("Hello World!");
        }
    }
}
