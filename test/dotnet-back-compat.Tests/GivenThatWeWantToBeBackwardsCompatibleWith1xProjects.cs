// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Microsoft.DotNet.Cli.Build.Tests
{
    [TestClass]
    public class GivenThatWeWantToBeBackwardsCompatibleWith1xProjects : SdkTest
    {
        public GivenThatWeWantToBeBackwardsCompatibleWith1xProjects(MSTestContext testContext) : base(testContext)
        {
        }

        [TestMethod]
        [FrameworkVersionCondition("netcoreapp1.1")]
        [DataRow(ToolsetInfo.CurrentTargetFramework)]
        public void ItRestoresBuildsAndRuns(string target)
        {

            var testAppName = "TestAppSimple";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName, identifier: target.Replace('.', '_'))
                .WithSource();

            //   Replace the 'TargetFramework'
            ChangeProjectTargetFramework(Path.Combine(testInstance.Path, $"{testAppName}.csproj"), target);

            var buildCommand = new BuildCommand(testInstance);

            buildCommand
                .Execute()
                .Should().Pass();

            var configuration = Environment.GetEnvironmentVariable("CONFIGURATION") ?? "Debug";

            var outputDll = Path.Combine(buildCommand.GetOutputDirectory(target, configuration).FullName, $"{testAppName}.dll");

            new DotnetCommand(MSTestContext)
                .Execute(outputDll)
                .Should().Pass()
                .And.HaveStdOutContaining("Hello World");
        }

        [TestMethod]
        [DataRow("netstandard1.3")]
        [DataRow("netstandard1.6")]
        public void ItRestoresBuildsAndPacks(string target)
        {

            var testAppName = "TestAppSimple";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName, identifier: target.Replace('.', '_'))
                .WithSource();

            //   Replace the 'TargetFramework'
            ChangeProjectTargetFramework(Path.Combine(testInstance.Path, $"{testAppName}.csproj"), target);

            new BuildCommand(testInstance)
                .Execute()
                .Should().Pass();

            new PackCommand(MSTestContext, testInstance.Path)
                .Execute()
                .Should().Pass();
        }

        [TestMethod]
        [FrameworkVersionCondition("netcoreapp1.0")] // https://github.com/dotnet/cli/issues/6087
        public void ItRunsABackwardsVersionedTool()
        {
            var testInstance = _testAssetsManager
                .CopyTestAsset("11TestAppWith10CLIToolReferences")
                .WithSource();

            NuGetConfigWriter.Write(testInstance.Path, TestContext.Current.TestPackages);

            new RestoreCommand(testInstance)
                .Execute()
                .Should()
                .Pass();

            new DotnetCommand(MSTestContext)
                .WithWorkingDirectory(testInstance.Path)
                .Execute("outputsframeworkversion-netcoreapp1.0")
                .Should()
                .Pass()
                .And
                .HaveStdOutContaining("netcoreapp1.0");
        }

        void ChangeProjectTargetFramework(string projectFile, string target)
        {
            var projectXml = XDocument.Load(projectFile);
            var ns = projectXml.Root.Name.Namespace;
            var propertyGroup = projectXml.Root.Elements(ns + "PropertyGroup").First();
            var rootNamespaceElement = propertyGroup.Element(ns + "TargetFramework");
            rootNamespaceElement.SetValue(target);
            projectXml.Save(projectFile.ToString());
        }

    }
}
