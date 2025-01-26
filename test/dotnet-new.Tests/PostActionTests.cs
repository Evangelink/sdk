// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Nodes;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.TemplateEngine.Utils;

namespace Microsoft.DotNet.Cli.New.IntegrationTests
{
    public partial class PostActionTests : BaseIntegrationTest
    {
        public PostActionTests(MSTestContext testContext) : base(testContext)
        {
        }

        [TestMethod]
        [DataRow("PostActions/RestoreNuGet/Basic", "TestAssets.PostActions.RestoreNuGet.Basic")]
        [DataRow("PostActions/RestoreNuGet/BasicWithFiles", "TestAssets.PostActions.RestoreNuGet.BasicWithFiles")]
        [DataRow("PostActions/RestoreNuGet/CustomSourcePath", "TestAssets.PostActions.RestoreNuGet.CustomSourcePath")]
        [DataRow("PostActions/RestoreNuGet/CustomSourcePathFiles", "TestAssets.PostActions.RestoreNuGet.CustomSourcePathFiles")]
        [DataRow("PostActions/RestoreNuGet/CustomTargetPath", "TestAssets.PostActions.RestoreNuGet.CustomTargetPath", "./Custom/Path/")]
        [DataRow("PostActions/RestoreNuGet/CustomTargetPathFiles", "TestAssets.PostActions.RestoreNuGet.CustomTargetPathFiles", "./Custom/Path/")]
        [DataRow("PostActions/RestoreNuGet/CustomSourceTargetPath", "TestAssets.PostActions.RestoreNuGet.CustomSourceTargetPath", "./Target/Output/")]
        [DataRow("PostActions/RestoreNuGet/CustomSourceTargetPathFiles", "TestAssets.PostActions.RestoreNuGet.CustomSourceTargetPathFiles", "./Target/Output/")]
        public void Restore_Basic(string templatePartLocation, string templateName, string targetSubfolder = "")
        {
            string templateLocation = _testAssetsManager.CopyTestAsset(templatePartLocation, testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, templateName, "-n", "MyProject")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.HaveStdOutContaining("Restore succeeded.")
                .And.NotHaveStdOutContaining("Manual instructions: Run 'dotnet restore'");

            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, targetSubfolder, $"MyProject.csproj")));
            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, targetSubfolder, $"Program.cs")));

            new DotnetBuildCommand(MSTestContext, "--no-restore")
                .WithWorkingDirectory(Path.Combine(workingDirectory, targetSubfolder))
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("Build succeeded.")
                .And.HaveStdOutContaining("MyProject");
        }

        [TestMethod]
        public void Restore_WithOutputAbsolutePath()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/RestoreNuGet/Basic", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string expectedTemplateName = "TestAssets.PostActions.RestoreNuGet.Basic";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            string outputDirectory = CreateTemporaryFolder("output");
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, expectedTemplateName, "-n", "MyProject", "-o", outputDirectory)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{expectedTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Restore succeeded.")
                .And.NotHaveStdOutContaining("Manual instructions: Run 'dotnet restore'")
                .And.HaveStdOutContaining(Path.Combine(outputDirectory, "MyProject.csproj"));

            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, $"MyProject.csproj")));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, $"Program.cs")));

            new DotnetBuildCommand(MSTestContext, "--no-restore")
                .WithWorkingDirectory(outputDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("Build succeeded.")
                .And.HaveStdOutContaining("MyProject");
        }

        [TestMethod]
        public void Restore_WithOutputRelativePath()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/RestoreNuGet/Basic", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string expectedTemplateName = "TestAssets.PostActions.RestoreNuGet.Basic";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            string outputDirectory = "output";
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, expectedTemplateName, "-n", "MyProject", "-o", outputDirectory)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{expectedTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Restore succeeded.")
                .And.NotHaveStdOutContaining("Manual instructions: Run 'dotnet restore'")
                .And.HaveStdOutContaining(Path.Combine(workingDirectory, outputDirectory, "MyProject.csproj"));

            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, outputDirectory, $"MyProject.csproj")));
            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, outputDirectory, $"Program.cs")));

            new DotnetBuildCommand(MSTestContext, "--no-restore")
                .WithWorkingDirectory(Path.Combine(workingDirectory, outputDirectory))
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("Build succeeded.")
                .And.HaveStdOutContaining("MyProject");
        }

        [TestMethod]
        [DataRow("PostActions/RestoreNuGet/SourceRename", "TestAssets.PostActions.RestoreNuGet.SourceRename")]
        [DataRow("PostActions/RestoreNuGet/SourceRenameFiles", "TestAssets.PostActions.RestoreNuGet.SourceRenameFiles")]
        public void Restore_SourceRenameTest(string templatePartLocation, string templateName)
        {
            string templateLocation = _testAssetsManager.CopyTestAsset(templatePartLocation, testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, templateName, "-n", "MyProject", "--firstRename", "Awesome")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.HaveStdOutContaining("Restore succeeded.")
                .And.NotHaveStdOutContaining("Manual instructions: Run 'dotnet restore'");

            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, $"MyAwesomeTestProject.csproj")));
            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, $"Program.cs")));

            new DotnetBuildCommand(MSTestContext, "--no-restore")
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("Build succeeded.")
                .And.HaveStdOutContaining("MyAwesomeTestProject");
        }

        [TestMethod]
        [DataRow("PostActions/RestoreNuGet/TwoProjectsPrimaryOutputs", "TestAssets.PostActions.RestoreNuGet.TwoProjectsPrimaryOutputs")]
        [DataRow("PostActions/RestoreNuGet/TwoProjectsFiles", "TestAssets.PostActions.RestoreNuGet.TwoProjectsFiles")]
        public void Restore_RestoreOneProjectFromTwo(string templatePartLocation, string templateName)
        {
            string templateLocation = _testAssetsManager.CopyTestAsset(templatePartLocation, testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, templateName, "-n", "TemplateApplication")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.HaveStdOutContaining("Restore succeeded.")
                .And.NotHaveStdOutContaining("Manual instructions: Run 'dotnet restore'");

            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, $"src/TemplateApplication/TemplateApplication.csproj")));
            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, $"test/TemplateApplication.Tests/TemplateApplication.Tests.csproj")));

            new DotnetBuildCommand(MSTestContext, "src/TemplateApplication", "--no-restore")
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("Build succeeded.")
                .And.HaveStdOutContaining("TemplateApplication");

            new DotnetBuildCommand(MSTestContext, "test/TemplateApplication.Tests", "--no-restore")
                  .WithWorkingDirectory(workingDirectory)
                  .Execute()
                  .Should().Fail()
                  .And.NotHaveStdOutContaining("Build succeeded.")
                  .And.HaveStdOutContaining("TemplateApplication.Tests");
        }

        [TestMethod]
        [DataRow("PostActions/RestoreNuGet/TwoProjectsWithSourceRenames", "TestAssets.PostActions.RestoreNuGet.TwoProjectsWithSourceRenames")]
        [DataRow("PostActions/RestoreNuGet/TwoProjectsWithSourceRenames2", "TestAssets.PostActions.RestoreNuGet.TwoProjectsWithSourceRenames2")]
        public void Restore_SourceRenameTwoProjectsTest(string templatePartLocation, string templateName)
        {
            string templateLocation = _testAssetsManager.CopyTestAsset(templatePartLocation, testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, templateName, "-n", "TemplateApplication")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.HaveStdOutContaining("Restore succeeded.")
                .And.NotHaveStdOutContaining("Manual instructions: Run 'dotnet restore'");

            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, $"TemplateApplication.UI/TemplateApplication.UI.csproj")));
            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, $"TemplateApplication.Tests/TemplateApplication.Tests.csproj")));

            new DotnetBuildCommand(MSTestContext, "TemplateApplication.UI", "--no-restore")
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("Build succeeded.")
                .And.HaveStdOutContaining("TemplateApplication.UI");

            new DotnetBuildCommand(MSTestContext, "TemplateApplication.Tests", "--no-restore")
                  .WithWorkingDirectory(workingDirectory)
                  .Execute()
                  .Should()
                  .ExitWith(0)
                  .And
                  .NotHaveStdErr()
                  .And.HaveStdOutContaining("Build succeeded.")
                  .And.HaveStdOutContaining("TemplateApplication.Tests");
        }

        [TestMethod]
        [DataRow("PostActions/RestoreNuGet/Files_MatchSpecifiedFiles", "TestAssets.PostActions.RestoreNuGet.Files_MatchSpecifiedFiles", "Tool.Library/Tool.Library.csproj;Tool.Test/Tool.Test.csproj", "Tool/Tool.csproj")]
        [DataRow("PostActions/RestoreNuGet/Files_MismatchSpecifiedFiles", "TestAssets.PostActions.RestoreNuGet.Files_MismatchSpecifiedFiles", "Tool.Library/Tool.Library.csproj;Tool/Tool.csproj", "Tool.Test/Tool.Test.csproj")]
        [DataRow("PostActions/RestoreNuGet/Files_PatternWithFileName", "TestAssets.PostActions.RestoreNuGet.Files_PatternWithFileName", "Tool.Library/Tool.Library.csproj;Tool/Tool.csproj", "Tool.Test/Tool.Test.csproj")]
        [DataRow("PostActions/RestoreNuGet/Files_PatternWithWildcard", "TestAssets.PostActions.RestoreNuGet.Files_PatternWithWildcard", "Tool.Library/Tool.Library.csproj;Tool.Test/Tool.Test.csproj", "Tool/Tool.csproj")]
        [DataRow("PostActions/RestoreNuGet/Files_PatternWithGlobstar", "TestAssets.PostActions.RestoreNuGet.Files_PatternWithGlobstar", "Tool.Library/Tool.Library.csproj", "Tool/Tool.csproj;Tool.Test/Tool.Test.csproj")]
        [DataRow("PostActions/RestoreNuGet/Files_SupportSemicolonDelimitedList", "TestAssets.PostActions.RestoreNuGet.Files_SupportSemicolonDelimitedList", "Tool.Library/Tool.Library.csproj;Tool/Tool.csproj", "Tool.Test/Tool.Test.csproj")]
        public void Restore_FilesTest(string templatePartLocation, string templateName, string expectedRestoredProjects, string unexpectedRestoredProjects)
        {
            string templateLocation = _testAssetsManager.CopyTestAsset(templatePartLocation, testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            expectedRestoredProjects = expectedRestoredProjects.Replace('/', Path.DirectorySeparatorChar);
            unexpectedRestoredProjects = unexpectedRestoredProjects.Replace('/', Path.DirectorySeparatorChar);
            string sourceName = "Tool";
            string rename = "MyTool";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            var result = new DotnetNewCommand(MSTestContext, templateName, "-n", rename)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.NotHaveStdOutContaining("Manual instructions: Run 'dotnet restore'");

            var expectedRestoredProjectList = expectedRestoredProjects
                .Split(';')
                .Select(p => p.Replace(sourceName, rename));
            var restoredProjectsPatterns = expectedRestoredProjectList
                .Select(p => ($"Restoring(.*?){p}(.*?)Restored(.*?){p}(.*?)Restore succeeded.").Replace("\\", "\\\\"));
            restoredProjectsPatterns.ForEach(r => result.And.HaveStdOutMatching(r, System.Text.RegularExpressions.RegexOptions.Singleline));

            var unexpectedRestoredList = unexpectedRestoredProjects
                .Split(';')
                .Select(p => p.Replace(sourceName, rename));
            unexpectedRestoredList.ForEach(t => result.And.NotHaveStdOutContaining(t));

            expectedRestoredProjectList.ForEach(p =>
            {
                string projectName = Path.GetFileNameWithoutExtension(p);
                new DotnetBuildCommand(MSTestContext, projectName, "--no-restore")
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("Build succeeded.")
                .And.HaveStdOutContaining(projectName);
            });
        }

        [TestMethod]
        public void RunScript_Basic()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/RunScript/Basic", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string templateName = "TestAssets.PostActions.RunScript.Basic";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            CommandResult commandResult = new DotnetNewCommand(MSTestContext, templateName, "--allow-scripts", "yes")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.NotHaveStdOutContaining("Run 'chmod +x *.sh'")
                .And.NotHaveStdOutContaining("Manual instructions: Run 'setup.cmd'")
                .And.NotHaveStdOutContaining("Manual instructions: Run 'setup.sh'");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                commandResult.Should().HaveStdOutContaining("Hello Windows");
            }
            else
            {
                commandResult.Should().HaveStdOutContaining("Hello Unix");
            }
        }

        [TestMethod]
        public void RunScript_DoNotRedirect()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/RunScript/DoNotRedirect", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string templateName = "TestAssets.PostActions.RunScript.DoNotRedirect";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            CommandResult commandResult = new DotnetNewCommand(MSTestContext, templateName, "--allow-scripts", "yes")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult
                .Should()
                .ExitWith(0)
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.HaveStdOutContaining("This line goes to stdout")
                .And.HaveStdErrContaining("This line goes to stderr")
                .And.NotHaveStdOutContaining("Run 'chmod +x *.sh'")
                .And.NotHaveStdOutContaining("Manual instructions: Run 'setup.cmd'")
                .And.NotHaveStdOutContaining("Manual instructions: Run 'setup.sh'");
        }

        [TestMethod]
        public void RunScript_Redirect()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/RunScript/Redirect", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string templateName = "TestAssets.PostActions.RunScript.Redirect";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            CommandResult commandResult = new DotnetNewCommand(MSTestContext, templateName, "--allow-scripts", "yes")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.NotHaveStdOutContaining("This line goes to stdout")
                .And.NotHaveStdOutContaining("Run 'chmod +x *.sh'")
                .And.NotHaveStdOutContaining("Manual instructions: Run 'setup.cmd'")
                .And.NotHaveStdOutContaining("Manual instructions: Run 'setup.sh'");
        }

        [TestMethod]
        public void RunScript_RedirectOnError()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/RunScript/RedirectOnError", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string templateName = "TestAssets.PostActions.RunScript.RedirectOnError";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            CommandResult commandResult = new DotnetNewCommand(MSTestContext, templateName, "--allow-scripts", "yes")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult
                .Should()
                .Fail()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.HaveStdErrContaining("Command failed")
                .And.HaveStdErrContaining("This line goes to stdout")
                .And.HaveStdErrContaining("This line goes to stderr")
                .And.NotHaveStdErrContaining("Run 'chmod +x *.sh'");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                commandResult.Should().HaveStdErrContaining("Manual instructions: Run 'setup.cmd'");
            }
            else
            {
                commandResult.Should().HaveStdErrContaining("Manual instructions: Run 'setup.sh'");
            }
        }

        [TestMethod]
        [DataRow("PostActions/AddPackageReference/Basic", "TestAssets.PostActions.AddPackageReference.Basic")]
        [DataRow("PostActions/AddPackageReference/BasicWithFiles", "TestAssets.PostActions.AddPackageReference.BasicWithFiles")]
        public void AddPackageReference_Basic(string templatePartLocation, string templateName)
        {
            string templateLocation = _testAssetsManager.CopyTestAsset(templatePartLocation, testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, templateName)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added a reference to the project file.")
                .And.HaveStdOutContaining($"Adding a package reference Newtonsoft.Json (version: {ToolsetInfo.GetNewtonsoftJsonPackageVersion()}) to project file")
                .And.NotHaveStdOutContaining("Manual instructions: Manually add");

            new DotnetBuildCommand(MSTestContext)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();
        }

        [TestMethod]
        public void AddPackageReference_WithOutputAbsolutePath()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/AddPackageReference/Basic", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string expectedTemplateName = "TestAssets.PostActions.AddPackageReference.Basic";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            string outputDirectory = CreateTemporaryFolder("output");
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, expectedTemplateName, "-o", outputDirectory, "-n", "MyProject")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{expectedTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added")
                .And.HaveStdOutContaining("reference Newtonsoft.Json")
                .And.NotHaveStdOutContaining("Manual instructions: Manually add")
                .And.HaveStdOutContaining(Path.Combine(outputDirectory, "MyProject.csproj"));

            new DotnetBuildCommand(MSTestContext, Path.Combine(outputDirectory, "MyProject.csproj"))
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();
        }

        [TestMethod]
        public void AddPackageReference_WithOutputRelativePath()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/AddPackageReference/Basic", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string expectedTemplateName = "TestAssets.PostActions.AddPackageReference.Basic";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, expectedTemplateName, "-o", "output", "-n", "MyProject")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{expectedTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added")
                .And.HaveStdOutContaining("reference Newtonsoft.Json")
                .And.NotHaveStdOutContaining("Manual instructions: Manually add")
                .And.HaveStdOutContaining(Path.Combine(workingDirectory, "output", "MyProject.csproj"));

            new DotnetBuildCommand(MSTestContext, Path.Combine("output", "MyProject.csproj"))
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();
        }

        [TestMethod]
        public void AddProjectReference_Basic()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/AddProjectReference/Basic", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string expectedTemplateName = "TestAssets.PostActions.AddProjectReference.Basic";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, expectedTemplateName)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{expectedTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added");

            new DotnetBuildCommand(MSTestContext, "Project1/Project1.csproj")
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();

            new DotnetBuildCommand(MSTestContext, "Project2/Project2.csproj")
                 .WithWorkingDirectory(workingDirectory)
                 .Execute()
                 .Should()
                 .ExitWith(0)
                 .And
                 .NotHaveStdErr();
        }

        [TestMethod]
        public void AddProjectReference_ExistingProject()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/AddProjectReference/Existing", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string expectedTemplateName = "TestAssets.PostActions.AddProjectReference.Existing";

            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            var result = new DotnetCommand(MSTestContext, "--version")
                .Execute();

            new DotnetNewCommand(MSTestContext)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute("classlib", "-n", "ExistingProject");

            new DotnetNewCommand(MSTestContext, expectedTemplateName)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{expectedTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added");

            new DotnetBuildCommand(MSTestContext, "ExistingProject/ExistingProject.csproj")
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();

            new DotnetBuildCommand(MSTestContext, "Project1/Project1.csproj")
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();
        }

        [TestMethod]
        public void AddProjectReference_ExistingProjectWithSymbolFileRename()
        {
            var templateLocation = _testAssetsManager.CopyTestAsset("PostActions/AddProjectReference/ExistingWithRename", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            var expectedTemplateName = "TestAssets.PostActions.AddProjectReference.ExistingWithRename";

            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            string src = Path.Combine(workingDirectory, "src");
            Directory.CreateDirectory(src);
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            var result = new DotnetCommand(MSTestContext, "--version")
                .Execute();

            new DotnetNewCommand(MSTestContext)
                .WithCustomHive(home)
                .WithWorkingDirectory(src)
                .Execute("classlib", "-n", "AlreadyExisting");

            new DotnetNewCommand(MSTestContext, expectedTemplateName, "--existingProject", "src/AlreadyExisting/AlreadyExisting.csproj")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{expectedTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added");

            new DotnetBuildCommand(MSTestContext, "Project1/Project1.csproj")
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();

            new DotnetBuildCommand(MSTestContext, "src/AlreadyExisting/AlreadyExisting.csproj")
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();
        }

        [TestMethod]
        public void AddProjectReference_WithOutputAbsolutePath()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/AddProjectReference/Basic", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string expectedTemplateName = "TestAssets.PostActions.AddProjectReference.Basic";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            string outputDirectory = CreateTemporaryFolder("output");
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, expectedTemplateName, "-o", outputDirectory)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{expectedTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added")
                .And.HaveStdOutContaining(Path.Combine(outputDirectory, "Project1", "Project1.csproj"))
                .And.HaveStdOutContaining(Path.Combine(outputDirectory, "Project2", "Project2.csproj"));

            new DotnetBuildCommand(MSTestContext, Path.Combine(outputDirectory, "Project1", "Project1.csproj"))
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();
        }

        [TestMethod]
        public void AddProjectReference_WithOutputRelativePath()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/AddProjectReference/Basic", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string expectedTemplateName = "TestAssets.PostActions.AddProjectReference.Basic";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, expectedTemplateName, "-o", "output")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{expectedTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added")
                .And.HaveStdOutContaining(Path.Combine(workingDirectory, "output", "Project1", "Project1.csproj"))
                .And.HaveStdOutContaining(Path.Combine(workingDirectory, "output", "Project2", "Project2.csproj"));

            new DotnetBuildCommand(MSTestContext, Path.Combine("output", "Project1", "Project1.csproj"))
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();
        }

        [TestMethod]
        [DataRow("PostActions/RestoreNuGet/Invalid", "TestAssets.PostActions.RestoreNuGet.Invalid", true)]
        [DataRow("PostActions/RestoreNuGet/Invalid_ContinueOnError", "TestAssets.PostActions.RestoreNuGet.Invalid.ContinueOnError", false)]
        public void ErrorExitCodeOnFailedPostAction(string templatePartLocation, string templateName, bool errorExpected)
        {
            string templateLocation = _testAssetsManager.CopyTestAsset(templatePartLocation, testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            CommandResult commandResult = new DotnetNewCommand(MSTestContext, templateName)
                   .WithCustomHive(home)
                   .WithWorkingDirectory(workingDirectory)
                   .Execute();

            if (errorExpected)
            {
                commandResult.Should().Fail();
            }
            else
            {
                commandResult.Should().Pass();
            }

            commandResult
                  .Should()
                  .HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                  .And.HaveStdOutContaining("Processing post-creation actions...")
                  .And.HaveStdOutContaining("Restoring")
                  .And.NotHaveStdOutContaining("Restore succeeded.")
                  .And.HaveStdErrContaining("Post action failed.")
                  .And.HaveStdErrContaining("Manual instructions: Run 'dotnet restore'");

            new DotnetBuildCommand(MSTestContext, "--no-restore")
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should().Fail();
        }

        [TestMethod]
        [DataRow("PostActions/AddProjectToSolution/Basic", "TestAssets.PostActions.AddProjectToSolution.Basic")]
        [DataRow("PostActions/AddProjectToSolution/BasicWithFiles", "TestAssets.PostActions.AddProjectToSolution.BasicWithFiles")]
        public void AddProjectToSolution_Basic(string templatePartLocation, string templateName)
        {
            string templateLocation = _testAssetsManager.CopyTestAsset(templatePartLocation, testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            //creating solution file to add to
            new DotnetNewCommand(MSTestContext, "sln", "-n", "MySolution")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr();

            new DotnetNewCommand(MSTestContext, templateName, "-n", "MyProject")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added project(s) to a solution file.")
                .And.HaveStdOutContaining("solution folder: src")
                .And.NotHaveStdOutContaining("Manual instructions: Add the generated files to solution manually.");

            Assert.Contains("MyProject.csproj", File.ReadAllText(Path.Combine(workingDirectory, "MySolution.sln")));
        }

        [TestMethod]
        public void AddProjectToSolution_BasicInSolutionRoot()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/AddProjectToSolution/BasicInSolutionRoot", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string expectedTemplateName = "TestAssets.PostActions.AddProjectToSolution.BasicInSolutionRoot";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            string outputDirectory = CreateTemporaryFolder("output");
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            //creating solution file to add to
            new DotnetNewCommand(MSTestContext, "sln", "-n", "MySolution", "-o", outputDirectory)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr();

            new DotnetNewCommand(MSTestContext, expectedTemplateName, "-n", "MyProject", "-o", outputDirectory)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{expectedTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added")
                .And.HaveStdOutContaining("in the root of solution file")
                .And.NotHaveStdOutContaining("Manual instructions: Add the generated files to solution manually.")
                .And.HaveStdOutContaining(Path.Combine(outputDirectory, "MySolution.sln"))
                .And.HaveStdOutContaining(Path.Combine(outputDirectory, "MyProject.csproj"));

            Assert.Contains("MyProject.csproj", File.ReadAllText(Path.Combine(outputDirectory, "MySolution.sln")));
        }

        [TestMethod]
        public void AddProjectToSolution_WithOutputAbsolutePath()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/AddProjectToSolution/Basic", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string expectedTemplateName = "TestAssets.PostActions.AddProjectToSolution.Basic";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            string outputDirectory = CreateTemporaryFolder("output");
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            //creating solution file to add to
            new DotnetNewCommand(MSTestContext, "sln", "-n", "MySolution", "-o", outputDirectory)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr();

            new DotnetNewCommand(MSTestContext, expectedTemplateName, "-n", "MyProject", "-o", outputDirectory)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{expectedTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added")
                .And.HaveStdOutContaining("solution folder: src")
                .And.NotHaveStdOutContaining("Manual instructions: Add the generated files to solution manually.")
                .And.HaveStdOutContaining(Path.Combine(outputDirectory, "MySolution.sln"))
                .And.HaveStdOutContaining(Path.Combine(outputDirectory, "MyProject.csproj"));

            Assert.Contains("MyProject.csproj", File.ReadAllText(Path.Combine(outputDirectory, "MySolution.sln")));
        }

        [TestMethod]
        public void AddProjectToSolution_WithOutputRelativePath()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/AddProjectToSolution/Basic", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string expectedTemplateName = "TestAssets.PostActions.AddProjectToSolution.Basic";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            string outputDirectory = "output";
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            //creating solution file to add to
            new DotnetNewCommand(MSTestContext, "sln", "-n", "MySolution", "-o", outputDirectory)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr();

            new DotnetNewCommand(MSTestContext, expectedTemplateName, "-n", "MyProject", "-o", outputDirectory)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{expectedTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added")
                .And.HaveStdOutContaining("solution folder: src")
                .And.NotHaveStdOutContaining("Manual instructions: Add the generated files to solution manually.")
                .And.HaveStdOutContaining(Path.Combine(workingDirectory, outputDirectory, "MySolution.sln"))
                .And.HaveStdOutContaining(Path.Combine(workingDirectory, outputDirectory, "MyProject.csproj"));

            Assert.Contains("MyProject.csproj", File.ReadAllText(Path.Combine(workingDirectory, outputDirectory, "MySolution.sln")));
        }

        [TestMethod]
        public void AddProjectToSolution_PrimaryOutputIndexes()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/AddProjectToSolution/BasicWithIndexes", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string expectedTemplateName = "TestAssets.PostActions.AddProjectToSolution.BasicWithIndexes";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            //creating solution file to add to
            new DotnetNewCommand(MSTestContext, "sln", "-n", "MySolution")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr();

            new DotnetNewCommand(MSTestContext, expectedTemplateName)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{expectedTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added project(s) to a solution file.")
                .And.HaveStdOutContaining("solution folder: Server")
                .And.NotHaveStdOutContaining("Manual instructions: Add generated Server project to solution manually to folder 'Server'.");

            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "MySolution.sln")));
            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Server/Server.csproj")));
            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Client/Client.csproj")));

            string solutionFileContents = File.ReadAllText(Path.Combine(workingDirectory, "MySolution.sln"));
            Assert.Contains("Server.csproj", solutionFileContents);
            Assert.DoesNotContain("Client.csproj", solutionFileContents);
        }

        [TestMethod]
        public void PrintInstructions_Basic()
        {
            string templateLocation = _testAssetsManager.CopyTestAsset("PostActions/Instructions/Basic", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            string templateName = "TestAssets.PostActions.Instructions.Basic";
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            CommandResult commandResult = new DotnetNewCommand(MSTestContext, templateName)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.HaveStdOutContaining($"Description: Manual actions needed")
                .And.HaveStdOutContaining($"Manual instructions: Run the following command:")
                .And.HaveStdOutContaining($"Actual command: setup.cmd <your project name>");
        }

        [TestMethod]
        public void ItCanCreateTemplate_WithAddProjectReference()
        {
            string workingDirectory = CreateTemporaryFolder();
            string tempSettingsDir = CreateTemporaryFolder("Home");
            string templateLocation = _testAssetsManager.CopyTestAsset("AddProjectReference", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            CommandResult cmd = new DotnetNewCommand(MSTestContext)
                .WithCustomHive(tempSettingsDir)
                .Execute("install", templateLocation);
            cmd.Should().Pass();

            cmd = new DotnetNewCommand(MSTestContext)
                .WithCustomHive(tempSettingsDir)
                .WithWorkingDirectory(workingDirectory)
                .Execute("TestAssets.AddReference");

            cmd.Should().Pass()
                .And.HaveStdOutContaining("Adding a project reference")
                .And.HaveStdOutContaining("Successfully added a reference to the project file.");
        }

        [TestMethod]
        public void ItCanCreateTemplate_WithAddPackageReference()
        {
            string workingDirectory = CreateTemporaryFolder();
            string tempSettingsDir = CreateTemporaryFolder("Home");
            string templateLocation = _testAssetsManager.CopyTestAsset("AddPackageReference", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            CommandResult cmd = new DotnetNewCommand(MSTestContext)
                .WithCustomHive(tempSettingsDir)
                .Execute("install", templateLocation);
            cmd.Should().Pass();

            cmd = new DotnetNewCommand(MSTestContext)
                .WithCustomHive(tempSettingsDir)
                .Execute("TestAssets.AddReference", "-o", workingDirectory);
            cmd.Should().Pass()
                .And.HaveStdOutContaining($"Adding a package reference Newtonsoft.Json (version: {ToolsetInfo.GetNewtonsoftJsonPackageVersion()}) to project file")
                .And.HaveStdOutContaining("Successfully added a reference to the project file.");
        }

        [TestMethod]
        public void ItCanCreateTemplate_WithAddProjectToSolution()
        {
            string workingDirectory = CreateTemporaryFolder();
            string tempSettingsDir = CreateTemporaryFolder("Home");
            string templateLocation = _testAssetsManager.CopyTestAsset("AddProjectToSolution", testAssetSubdirectory: DotnetNewTestTemplatesBasePath).WithSource().Path;
            CommandResult cmd = new DotnetNewCommand(MSTestContext)
                .WithCustomHive(tempSettingsDir)
                .Execute("install", templateLocation);
            cmd.Should().Pass();

            cmd = new DotnetNewCommand(MSTestContext)
                .WithCustomHive(tempSettingsDir)
                .Execute("TestAssets.AddProjectToSolution", "-o", workingDirectory);
            cmd.Should().Pass()
                .And.HaveStdOutContaining("Successfully added project(s) to a solution file.");
        }

        [TestMethod]
        public void ItCanCreateTemplate_WithRestore()
        {
            string workingDirectory = CreateTemporaryFolder();
            string tempSettingsDir = CreateTemporaryFolder("Home");

            CommandResult cmd = new DotnetNewCommand(MSTestContext)
                .WithCustomHive(tempSettingsDir)
                .WithWorkingDirectory(workingDirectory)
                .Execute("console");

            cmd.Should().Pass()
                .And.HaveStdOutContaining("Determining projects to restore...")
                .And.HaveStdOutContaining("Restore succeeded.");
        }

        [TestMethod]
        public void AddJsonProperty_Basic()
        {
            const string templateLocation = "PostActions/AddJsonProperty/Basic";
            const string templateName = "TestAssets.PostActions.AddJsonProperty.Basic";

            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, templateName)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully modified testfile.json.")
                .And.NotHaveStdOutContaining("Manual instructions: Modify the JSON file manually.");
        }

        [TestMethod]
        public void AddJsonProperty_InOtherProjectOfSameSolution()
        {
            const string existingProjectTemplateLocation = "PostActions/AddJsonProperty/WithExistingProject/ExistingProject";
            const string existingProjectTemplateName = "TestAssets.PostActions.AddJsonProperty.WithExistingProject.ExistingProject";

            const string myProjectTemplateLocation = "PostActions/AddJsonProperty/WithExistingProject/MyTestProject";
            const string myProjectTemplateName = "TestAssets.PostActions.AddJsonProperty.WithExistingProject.MyProject";

            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();

            InstallTestTemplate(existingProjectTemplateLocation, MSTestContext, home, workingDirectory);
            InstallTestTemplate(myProjectTemplateLocation, MSTestContext, home, workingDirectory);

            // Create a solution that already contains a project that has a JSON file present.
            // This is actually simulating an Azure IoT Edge project.
            new DotnetNewCommand(MSTestContext, "sln", "-n", "MySolution")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr();

            // Add the IoT Edge Project to the solution.  This project contains a deployment.template.json file
            // When we add another project that represents an IoT Edge module, we want to make some modifications
            // in that deployment.template.json file.
            new DotnetNewCommand(MSTestContext, existingProjectTemplateName, "-o", "ExistingProject")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{existingProjectTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully added project(s) to a solution file.")
                .And.NotHaveStdOutContaining("Manual instructions: Add generated project to solution manually.");

            // Create the project that represents an IoT Edge module.  This project template must modify the
            // deployment.template.json file that is part of the existing project that has been created in the step before.
            new DotnetNewCommand(MSTestContext, myProjectTemplateName, "-o", "custommodule1", "-n", "custommodule1")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{myProjectTemplateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully modified deployment.template.json.")
                .And.NotHaveStdOutContaining("Manual instructions: Modify the JSON file manually.");
        }

        [TestMethod]
        public void AddJsonProperty_WithSourceNameReplacementInNewJsonProperty()
        {
            const string templateLocation = "PostActions/AddJsonProperty/WithSourceNameChangeInJson";
            const string templateName = "TestAssets.PostActions.AddJsonProperty.WithSourceNameChangeInJson";

            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();

            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            // Create the project that represents an IoT Edge module.  This project template must modify the
            // deployment.template.json file that is part of the existing project that has been created in the step before.
            new DotnetNewCommand(MSTestContext, templateName, "-n", "TheProjectName")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully modified testfile.json.")
                .And.NotHaveStdOutContaining("Manual instructions: Modify the JSON file manually.");

            // Verify if the expected property is added to the deployment.template.json file
            string jsonFileContents = File.ReadAllText(Path.Combine(workingDirectory, "testfile.json"));

            JsonNode? jsonContents = JsonNode.Parse(jsonFileContents);
            Assert.IsNotNull(jsonContents);
            Assert.IsNotNull(jsonContents["moduleConfiguration"]?["edgeAgent"]?["properties.desired"]?["modules"]?["TheProjectName"]);
            Assert.AreEqual("${MODULEDIR<../TheProjectName>}", jsonContents["moduleConfiguration"]?["edgeAgent"]?["properties.desired"]?["modules"]?["TheProjectName"]?.ToString());
        }

        [TestMethod]
        public void AddJsonProperty_WithMultipleAddJsonPropertyActions()
        {
            const string templateLocation = "PostActions/AddJsonProperty/WithAddMultipleProperties";
            const string templateName = "TestAssets.PostActions.AddJsonProperty.WithAddMultipleProperties";

            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();

            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            new DotnetNewCommand(MSTestContext, templateName, "-n", "TheProjectName")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"The template \"{templateName}\" was created successfully.")
                .And.HaveStdOutContaining("Successfully modified testfile.json.")
                .And.NotHaveStdOutContaining("Manual instructions: Modify the JSON file manually.");

            // Verify if the expected property is added to the deployment.template.json file
            string jsonFileContents = File.ReadAllText(Path.Combine(workingDirectory, "testfile.json"));

            JsonNode? jsonContents = JsonNode.Parse(jsonFileContents);
            Assert.IsNotNull(jsonContents);

            Assert.IsNotNull(jsonContents["root"]?["prop1"]);
            Assert.IsNotNull(jsonContents["root"]?["prop1"]?["prop2"]);
            Assert.AreEqual("bar", jsonContents["root"]?["prop1"]?["prop2"]?.ToString());
        }

        [TestMethod]
        public void AddJsonProperty_FailsWhenJsonFileNotFound()
        {
            const string templateLocation = "PostActions/AddJsonProperty/FailsWhenJsonFileNotFound";
            const string templateName = "TestAssets.PostActions.AddJsonProperty.FailsWhenJsonFileNotFound";

            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();

            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            // Create the project that represents an IoT Edge module.  This project template must modify the
            // deployment.template.json file that is part of the existing project that has been created in the step before.
            new DotnetNewCommand(MSTestContext, templateName)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.HaveStdErrContaining("Unable to find the json file in the solution")
                .And.HaveStdErrContaining("Manual instructions: Modify the JSON file manually.");
        }

        [TestMethod]
        public void AddJsonProperty_FailsWhenJsonFileNotFoundInEligableDirectories()
        {
            const string templateLocation = "PostActions/AddJsonProperty/FailsWhenJsonFileNotFound";
            const string templateName = "TestAssets.PostActions.AddJsonProperty.FailsWhenJsonFileNotFound";

            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();

            InstallTestTemplate(templateLocation, MSTestContext, home, workingDirectory);

            string jsonFileLocation = Path.Combine(workingDirectory, "testfile.json");

            File.WriteAllText(jsonFileLocation, @"{""foo"":{""bar"":{}}}");

            // Create the project that represents an IoT Edge module.  This project template must modify the
            // deployment.template.json file that is part of the existing project that has been created in the step before.
            new DotnetNewCommand(MSTestContext, templateName, "-o", "SolutionFolder/SomeTestFolder")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.HaveStdErrContaining("Unable to find the json file in the solution")
                .And.HaveStdErrContaining("Manual instructions: Modify the JSON file manually.");

            File.Delete(jsonFileLocation);
        }
    }
}
