// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.NET.Publish.Tests
{
    public class GivenThatWeWantToPublishASingleFileLibrary : SdkTest
    {
        public GivenThatWeWantToPublishASingleFileLibrary(MSTestContext testContext) : base(testContext)
        {

        }

        [WindowsOnlyFact]
        // Tests regression on https://github.com/dotnet/sdk/pull/28484
        public void ItPublishesSuccessfullyWithRIDAndPublishSingleFileLibrary()
        {
            var targetFramework = ToolsetInfo.CurrentTargetFramework;
            TestProject referencedProject = new("Library")
            {
                TargetFrameworks = targetFramework,
                IsExe = false
            };

            TestProject testProject = new("MainProject")
            {
                TargetFrameworks = targetFramework,
                IsExe = true
            };
            testProject.ReferencedProjects.Add(referencedProject);
            testProject.RecordProperties("RuntimeIdentifier");
            referencedProject.RecordProperties("RuntimeIdentifier");

            string rid = EnvironmentInfo.GetCompatibleRid(targetFramework);
            List<string> args = new() { "/p:PublishSingleFile=true", $"/p:RuntimeIdentifier={rid}" };

            var testAsset = _testAssetsManager.CreateTestProject(testProject);
            new PublishCommand(testAsset)
                .Execute(args.ToArray())
                .Should()
                .Pass();

            var referencedProjProperties = referencedProject.GetPropertyValues(testAsset.TestRoot, targetFramework: targetFramework);
            var mainProjProperties = testProject.GetPropertyValues(testAsset.TestRoot, targetFramework: targetFramework);
            Assert.IsTrue(mainProjProperties["RuntimeIdentifier"] == rid);
            Assert.IsTrue(referencedProjProperties["RuntimeIdentifier"] == "");
        }
    }

}
