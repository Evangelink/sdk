// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.DotNet.Cli.Utils;
using Microsoft.TemplateEngine.Utils;
using LocalizableStrings = Microsoft.DotNet.Tools.Run.LocalizableStrings;

namespace Microsoft.DotNet.Cli.Run.Tests
{
    public class GivenDotnetRunBuildsCsproj : SdkTest
    {
        public GivenDotnetRunBuildsCsproj(MSTestContext testContext) : base(testContext)
        {
        }

        [TestMethod]
        public void ItCanRunAMSBuildProject()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            new BuildCommand(testInstance)
                .Execute()
                .Should().Pass();

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute()
                .Should().Pass()
                         .And.HaveStdOutContaining("Hello World!");
        }

        [TestMethod]
        public void ItImplicitlyRestoresAProjectWhenRunning()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute()
                .Should().Pass()
                         .And.HaveStdOutContaining("Hello World!");
        }

        [TestMethod]
        public void ItCanRunAMultiTFMProjectWithImplicitRestore()
        {
            var testInstance = _testAssetsManager.CopyTestAsset(
                    "NETFrameworkReferenceNETStandard20",
                    testAssetSubdirectory: TestAssetSubdirectories.DesktopTestProjects)
                .WithSource();

            string projectDirectory = Path.Combine(testInstance.Path, "MultiTFMTestApp");

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(projectDirectory)
                .Execute("--framework", ToolsetInfo.CurrentTargetFramework)
                .Should().Pass()
                         .And.HaveStdOutContaining("This string came from the test library!");
        }

        [TestMethod]
        public void ItDoesNotImplicitlyBuildAProjectWhenRunningWithTheNoBuildOption()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var result = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testInstance.Path)
                .Execute("--no-build", "-v:m");

            result.Should().Fail();
            if (!TestContext.IsLocalized())
            {
                result.Should().NotHaveStdOutContaining("Restore");
            }
        }

        [TestMethod]
        public void ItDoesNotImplicitlyRestoreAProjectWhenRunningWithTheNoRestoreOption()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--no-restore")
                .Should().Fail()
                .And.HaveStdOutContaining("project.assets.json");
        }

        [TestMethod]
        public void ItBuildsTheProjectBeforeRunning()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute()
                .Should().Pass()
                         .And.HaveStdOutContaining("Hello World!");
        }

        [TestMethod]
        public void ItCanRunAMSBuildProjectWhenSpecifyingAFramework()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--framework", ToolsetInfo.CurrentTargetFramework)
                .Should().Pass()
                         .And.HaveStdOutContaining("Hello World!");
        }

        [TestMethod]
        public void ItRunsPortableAppsFromADifferentPathAfterBuilding()
        {
            var testInstance = _testAssetsManager.CopyTestAsset("MSBuildTestApp")
                .WithSource();

            new BuildCommand(testInstance)
                .Execute()
                .Should().Pass();

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testInstance.Path)
                .Execute($"--no-build")
                .Should().Pass()
                         .And.HaveStdOutContaining("Hello World!");
        }

        [TestMethod]
        public void ItRunsPortableAppsFromADifferentPathWithoutBuilding()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            var projectFile = Path.Combine(testInstance.Path, testAppName + ".csproj");

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(Directory.GetParent(testInstance.Path).FullName)
                .Execute($"--project", projectFile)
                .Should().Pass()
                         .And.HaveStdOutContaining("Hello World!")
                         .And.NotHaveStdOutContaining(LocalizableStrings.RunCommandProjectAbbreviationDeprecated);
        }

        [TestMethod]
        public void ItRunsPortableAppsFromADifferentPathSpecifyingOnlyTheDirectoryWithoutBuilding()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            var testProjectDirectory = testInstance.Path;

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(Directory.GetParent(testInstance.Path).FullName)
                .Execute("--project", testProjectDirectory)
                .Should().Pass()
                         .And.HaveStdOutContaining("Hello World!")
                         .And.NotHaveStdOutContaining(LocalizableStrings.RunCommandProjectAbbreviationDeprecated);
        }

        [TestMethod]
        public void ItWarnsWhenShortFormOfProjectArgumentIsUsed()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            var projectFile = Path.Combine(testInstance.Path, testAppName + ".csproj");

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(Directory.GetParent(testInstance.Path).FullName)
                .Execute($"-p", projectFile)
                .Should().Pass()
                         .And.HaveStdOutContaining("Hello World!")
                         .And.HaveStdOutContaining(LocalizableStrings.RunCommandProjectAbbreviationDeprecated);
        }

        [TestMethod]
        [DataRow("-p project1 -p project2")]
        [DataRow("--project project1 -p project2")]
        public void ItErrorsWhenMultipleProjectsAreSpecified(string args)
        {
            new DotnetCommand(MSTestContext, "run")
                .Execute(args.Split(" "))
                .Should()
                .Fail()
                .And
                .HaveStdErrContaining(LocalizableStrings.OnlyOneProjectAllowed);
        }

        [TestMethod]
        public void ItRunsAppWhenRestoringToSpecificPackageDirectory()
        {
            var rootPath = _testAssetsManager.CreateTestDirectory().Path;

            string dir = "pkgs";
            string[] args = new string[] { "--packages", dir };

            string[] newArgs = new string[] { "console", "-o", rootPath, "--no-restore" };
            new DotnetNewCommand(MSTestContext)
                .WithVirtualHive()
                .WithWorkingDirectory(rootPath)
                .Execute(newArgs)
                .Should()
                .Pass();

            new DotnetRestoreCommand(MSTestContext)
                .WithWorkingDirectory(rootPath)
                .Execute(args)
                .Should()
                .Pass();

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(rootPath)
                .Execute("--no-restore")
                .Should().Pass()
                         .And.HaveStdOutContaining("Hello, World");
        }

        [TestMethod]
        public void ItReportsAGoodErrorWhenProjectHasMultipleFrameworks()
        {
            var testAppName = "MSBuildAppWithMultipleFrameworks";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            // use --no-build so this test can run on all platforms.
            // the test app targets net451, which can't be built on non-Windows
            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testInstance.Path)
                .Execute("--no-build")
                .Should().Fail()
                    .And.HaveStdErrContaining("--framework");
        }

        [TestMethod]
        public void ItCanPassArgumentsToSubjectAppByDoubleDash()
        {
            const string testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            var testProjectDirectory = testInstance.Path;

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--", "foo", "bar", "baz")
                .Should()
                .Pass()
                .And.HaveStdOutContaining("echo args:foo;bar;baz");
        }

        [TestMethod]
        public void ItCanPassOptionArgumentsToSubjectAppByDoubleDash()
        {
            const string testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            var testProjectDirectory = testInstance.Path;

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--", "-d", "-a")
                .Should()
                .Pass()
                .And.HaveStdOutContaining("echo args:-d;-a");
        }

        [TestMethod]
        public void ItCanPassOptionAndArgumentsToSubjectAppByDoubleDash()
        {
            const string testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            var testProjectDirectory = testInstance.Path;

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--", "foo", "-d", "-a")
                .Should()
                .Pass()
                .And.HaveStdOutContaining("echo args:foo;-d;-a");
        }

        [TestMethod]
        public void ItCanPassArgumentsToSubjectAppWithoutDoubleDash()
        {
            const string testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            var testProjectDirectory = testInstance.Path;

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("foo", "bar", "baz")
                .Should()
                .Pass()
                .And.HaveStdOutContaining("echo args:foo;bar;baz");
        }

        [TestMethod]
        public void ItCanPassUnrecognizedOptionArgumentsToSubjectAppWithoutDoubleDash()
        {
            const string testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            var testProjectDirectory = testInstance.Path;

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("-x", "-y", "-z")
                .Should()
                .Pass()
                .And.HaveStdOutContaining("echo args:-x;-y;-z");
        }

        [TestMethod]
        public void ItCanPassOptionArgumentsAndArgumentsToSubjectAppWithoutAndByDoubleDash()
        {
            const string testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            var testProjectDirectory = testInstance.Path;

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("foo", "--", "-z")
                .Should()
                .Pass()
                .And.HaveStdOutContaining("echo args:foo;-z");
        }

        [TestMethod]
        public void ItGivesAnErrorWhenAttemptingToUseALaunchProfileThatDoesNotExistWhenThereIsNoLaunchSettingsFile()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            var runResult = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--launch-profile", "test");

            string[] expectedErrorWords = LocalizableStrings.RunCommandExceptionCouldNotLocateALaunchSettingsFile.Replace("\'{0}\'", "").Split(" ");
            runResult
                .Should()
                .Pass()
                .And
                .HaveStdOutContaining("Hello World!");

            expectedErrorWords.ForEach(word => runResult.Should().HaveStdErrContaining(word));
        }

        [TestMethod]
        public void ItUsesLaunchProfileOfTheSpecifiedName()
        {
            var testAppName = "AppWithLaunchSettings";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            var cmd = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--launch-profile", "Second");

            cmd.Should().Pass()
                .And.HaveStdOutContaining("Second");

            cmd.StdErr.Should().BeEmpty();
        }

        [TestMethod]
        [Ignore("https://github.com/dotnet/sdk/issues/42841")]
        public void ItDefaultsToTheFirstUsableLaunchProfile()
        {
            var testAppName = "AppWithLaunchSettings";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;
            var launchSettingsPath = Path.Combine(testProjectDirectory, "Properties", "launchSettings.json");

            var cmd = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute();

            cmd.Should().Pass()
                .And.NotHaveStdOutContaining(string.Format(LocalizableStrings.UsingLaunchSettingsFromMessage, launchSettingsPath))
                .And.HaveStdOutContaining("First");

            cmd.StdErr.Should().BeEmpty();
        }

        [TestMethod]
        public void ItSetsTheDotnetLaunchProfileEnvironmentVariableToDefaultLaunchProfileName()
        {
            var testAppName = "AppThatOutputsDotnetLaunchProfile";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;
            var launchSettingsPath = Path.Combine(testProjectDirectory, "Properties", "launchSettings.json");

            var cmd = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute();

            cmd.Should().Pass()
                .And.HaveStdOutContaining("DOTNET_LAUNCH_PROFILE=<<<First>>>");

            cmd.StdErr.Should().BeEmpty();
        }

        [TestMethod]
        public void ItSetsTheDotnetLaunchProfileEnvironmentVariableToSuppliedLaunchProfileName()
        {
            var testAppName = "AppThatOutputsDotnetLaunchProfile";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;
            var launchSettingsPath = Path.Combine(testProjectDirectory, "Properties", "launchSettings.json");

            var cmd = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--launch-profile", "Second");

            cmd.Should().Pass()
                .And.HaveStdOutContaining("DOTNET_LAUNCH_PROFILE=<<<Second>>>");

            cmd.StdErr.Should().BeEmpty();
        }

        [TestMethod]
        public void ItSetsTheDotnetLaunchProfileEnvironmentVariableToEmptyWhenInvalidProfileSpecified()
        {
            var testAppName = "AppThatOutputsDotnetLaunchProfile";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;
            var launchSettingsPath = Path.Combine(testProjectDirectory, "Properties", "launchSettings.json");

            var cmd = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--launch-profile", "DoesNotExist");

            cmd.Should().Pass()
                .And.HaveStdOutContaining("DOTNET_LAUNCH_PROFILE=<<<>>>");

            cmd.StdErr.Should().Contain("DoesNotExist");
        }

        [TestMethod]
        public void ItSetsTheDotnetLaunchProfileEnvironmentVariableToEmptyWhenNoLaunchProfileSwitchIsUsed()
        {
            var testAppName = "AppThatOutputsDotnetLaunchProfile";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;
            var launchSettingsPath = Path.Combine(testProjectDirectory, "Properties", "launchSettings.json");

            var cmd = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--no-launch-profile");

            cmd.Should().Pass()
                .And.HaveStdOutContaining("DOTNET_LAUNCH_PROFILE=<<<>>>");

            cmd.StdErr.Should().BeEmpty();
        }

        [TestMethod]
        public void ItPrintsUsingLaunchSettingsMessageWhenNotQuiet()
        {
            var testInstance = _testAssetsManager.CopyTestAsset("AppWithLaunchSettings")
                            .WithSource();

            var testProjectDirectory = testInstance.Path;
            var launchSettingsPath = Path.Combine(testProjectDirectory, "Properties", "launchSettings.json");

            var cmd = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("-v:m");

            cmd.Should().Pass()
                .And.HaveStdOutContaining(string.Format(LocalizableStrings.UsingLaunchSettingsFromMessage, launchSettingsPath))
                .And.HaveStdOutContaining("First");

            cmd.StdErr.Should().BeEmpty();
        }

        [TestMethod]
        public void ItPrefersTheValueOfAppUrlFromEnvVarOverTheProp()
        {
            var testAppName = "AppWithApplicationUrlInLaunchSettings";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            var cmd = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--launch-profile", "First");

            cmd.Should().Pass()
                .And.HaveStdOutContaining("http://localhost:12345/");

            cmd.StdErr.Should().BeEmpty();
        }

        [TestMethod]
        public void ItUsesTheValueOfAppUrlIfTheEnvVarIsNotSet()
        {
            var testAppName = "AppWithApplicationUrlInLaunchSettings";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            var cmd = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--launch-profile", "Second");

            cmd.Should().Pass()
                .And.HaveStdOutContaining("http://localhost:54321/");

            cmd.StdErr.Should().BeEmpty();
        }

        [TestMethod]
        public void ItGivesAnErrorWhenTheLaunchProfileNotFound()
        {
            var testAppName = "AppWithLaunchSettings";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--launch-profile", "Third")
                .Should().Pass()
                         .And.HaveStdOutContaining("(NO MESSAGE)")
                         .And.HaveStdErrContaining(string.Format(LocalizableStrings.RunCommandExceptionCouldNotApplyLaunchSettings, "Third", "").Trim());
        }

        [TestMethod]
        public void ItGivesAnErrorWhenTheLaunchProfileCanNotBeHandled()
        {
            var testAppName = "AppWithLaunchSettings";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--launch-profile", "IIS Express")
                .Should().Pass()
                         .And.HaveStdOutContaining("(NO MESSAGE)")
                         .And.HaveStdErrContaining(string.Format(LocalizableStrings.RunCommandExceptionCouldNotApplyLaunchSettings, "IIS Express", "").Trim());
        }

        [TestMethod]
        public void ItSkipsLaunchProfilesWhenTheSwitchIsSupplied()
        {
            var testAppName = "AppWithLaunchSettings";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            var cmd = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--no-launch-profile");

            cmd.Should().Pass()
                .And.HaveStdOutContaining("(NO MESSAGE)");

            cmd.StdErr.Should().BeEmpty();
        }

        [TestMethod]
        public void ItSkipsLaunchProfilesWhenTheSwitchIsSuppliedWithoutErrorWhenThereAreNoLaunchSettings()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            var cmd = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute("--no-launch-profile");

            cmd.Should().Pass()
                .And.HaveStdOutContaining("Hello World!");

            cmd.StdErr.Should().BeEmpty();
        }

        [TestMethod]
        public void ItSkipsLaunchProfilesWhenThereIsNoUsableDefault()
        {
            var testAppName = "AppWithLaunchSettingsNoDefault";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            var cmd = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute();

            cmd.Should().Pass()
                .And.HaveStdOutContaining("(NO MESSAGE)")
                .And.HaveStdErrContaining(string.Format(LocalizableStrings.RunCommandExceptionCouldNotApplyLaunchSettings, LocalizableStrings.DefaultLaunchProfileDisplayName, "").Trim());
        }

        [TestMethod]
        public void ItPrintsAnErrorWhenLaunchSettingsAreCorrupted()
        {
            var testAppName = "AppWithCorruptedLaunchSettings";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var testProjectDirectory = testInstance.Path;

            var cmd = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testProjectDirectory)
                .Execute();

            cmd.Should().Pass()
                .And.HaveStdOutContaining("(NO MESSAGE)")
                .And.HaveStdErrContaining(string.Format(LocalizableStrings.RunCommandExceptionCouldNotApplyLaunchSettings, LocalizableStrings.DefaultLaunchProfileDisplayName, "").Trim());
        }

        [TestMethod]
        public void ItRunsWithTheSpecifiedVerbosity()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                            .WithSource();

            var result = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testInstance.Path)
                .Execute("-v:n");

            result.Should().Pass()
                .And.HaveStdOutContaining("Hello World!");

            if (!TestContext.IsLocalized())
            {
                result.Should().HaveStdOutContaining("Restore")
                    .And.HaveStdOutContaining("CoreCompile");
            }
        }

        [TestMethod]
        public void ItDoesNotShowImportantLevelMessageByDefault()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource()
                .WithProjectChanges(ProjectModification.AddDisplayMessageBeforeRestoreToProject);

            var result = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testInstance.Path)
                .Execute();

            result.Should().Pass()
                .And.NotHaveStdOutContaining("Important text");
        }

        [TestMethod]
        public void ItShowImportantLevelMessageWhenPassInteractive()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource()
                .WithProjectChanges(ProjectModification.AddDisplayMessageBeforeRestoreToProject);

            var result = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testInstance.Path)
                .Execute("--interactive");

            result.Should().Pass()
                .And.HaveStdOutContaining("Important text");
        }

        [TestMethod]
        public void ItPrintsDuplicateArguments()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            var result = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testInstance.Path)
                .Execute("a", "b", "c", "a", "c");

            result.Should().Pass()
                .And.HaveStdOutContaining("echo args:a;b;c;a;c");
        }

        [TestMethod]
        public void ItRunsWithDotnetWithoutApphost()
        {
            var testInstance = _testAssetsManager.CopyTestAsset("AppOutputsExecutablePath").WithSource();

            var command = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testInstance.Path)
                .WithEnvironmentVariable("UseAppHost", "false");

            command.Execute()
                   .Should()
                   .Pass()
                   .And
                   .HaveStdOutContaining($"dotnet{Constants.ExeSuffix}");
        }

        [TestMethod][OSCondition(OperatingSystems.Windows | OperatingSystems.Linux | OperatingSystems.FreeBSD)]
        public void ItRunsWithApphost()
        {
            var testInstance = _testAssetsManager.CopyTestAsset("AppOutputsExecutablePath").WithSource();

            var result = new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testInstance.Path)
                .Execute();

            result.Should().Pass()
                .And.HaveStdOutContaining($"AppOutputsExecutablePath{Constants.ExeSuffix}");
        }

        [TestMethod]
        public void ItForwardsEmptyArgumentsToTheApp()
        {
            var testAppName = "TestAppSimple";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            new DotnetCommand(MSTestContext, "run")
                .WithWorkingDirectory(testInstance.Path)
                .Execute("a", "", "c")
                .Should()
                .Pass()
                .And
                .HaveStdOutContaining($"0 = a{Environment.NewLine}1 = {Environment.NewLine}2 = c");
        }

        [TestMethod]
        public void ItDoesNotPrintBuildingMessageByDefault()
        {
            var expectedValue = "Building...";
            var testAppName = "TestAppSimple";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            new DotnetCommand(MSTestContext, "run")
               .WithWorkingDirectory(testInstance.Path)
               .Execute()
               .Should()
               .Pass()
               .And
               .NotHaveStdOutContaining(expectedValue);
        }

        [TestMethod]
        public void ItPrintsBuildingMessageIfLaunchSettingHasDotnetRunMessagesSet()
        {
            var expectedValue = "Building...";
            var testAppName = "TestAppWithLaunchSettings";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            new DotnetCommand(MSTestContext, "run")
               .WithWorkingDirectory(testInstance.Path)
               .Execute()
               .Should()
               .Pass()
               .And
               .HaveStdOutContaining(expectedValue);
        }

        [TestMethod]
        public void ItIncludesEnvironmentVariablesSpecifiedInLaunchSettings()
        {
            var expectedValue = "MyCoolEnvironmentVariableKey=MyCoolEnvironmentVariableValue";
            var testAppName = "TestAppWithLaunchSettings";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            new DotnetCommand(MSTestContext, "run")
               .WithWorkingDirectory(testInstance.Path)
               .Execute()
               .Should()
               .Pass()
               .And
               .HaveStdOutContaining(expectedValue);
        }

        [TestMethod]
        public void ItIncludesCommandArgumentsSpecifiedInLaunchSettings()
        {
            var expectedValue = "TestAppCommandLineArguments";
            var secondExpectedValue = "SecondTestAppCommandLineArguments";
            var testAppName = "TestAppWithLaunchSettings";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            new DotnetCommand(MSTestContext, "run")
               .WithWorkingDirectory(testInstance.Path)
               .Execute()
               .Should()
               .Pass()
               .And
               .HaveStdOutContaining(expectedValue)
               .And
               .HaveStdOutContaining(secondExpectedValue);
        }

        [TestMethod]
        public void ItCLIArgsOverrideCommandArgumentsSpecifiedInLaunchSettings()
        {
            var expectedValue = "TestAppCommandLineArguments";
            var secondExpectedValue = "SecondTestAppCommandLineArguments";
            var testAppName = "TestAppWithLaunchSettings";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            new DotnetCommand(MSTestContext, "run", "-- test")
               .WithWorkingDirectory(testInstance.Path)
               .Execute()
               .Should()
               .Pass()
               .And
               .NotHaveStdOutContaining(expectedValue)
               .And
               .NotHaveStdOutContaining(secondExpectedValue);
        }
    }
}
