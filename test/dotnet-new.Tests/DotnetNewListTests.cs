// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.DotNet.Cli.New.IntegrationTests
{
    public partial class DotnetNewListTests : BaseIntegrationTest, IClassFixture<SharedHomeDirectory>
    {
        private readonly SharedHomeDirectory _sharedHome;

        public DotnetNewListTests(SharedHomeDirectory sharedHome, MSTestContext testContext) : base(testContext)
        {
            _sharedHome = sharedHome;
            _sharedHome.InstallPackage("Microsoft.DotNet.Web.ProjectTemplates.5.0::5.0.0");
        }

        [TestMethod]
        [DataRow("--list c")]
        [DataRow("-l c")]
        [DataRow("list c")]
        [DataRow("c --list")]
        public void BasicTest_WithNameCriteria(string command)
        {
            new DotnetNewCommand(MSTestContext, command.Split(" "))
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"These templates matched your input: 'c'")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.NotHaveStdOutMatching("dotnet gitignore file\\s+gitignore\\s+Config")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library");
        }

        [TestMethod]
        [DataRow("--list --columns-all")]
        [DataRow("--columns-all --list")]
        [DataRow("list --columns-all")]
        public void CanShowAllColumns(string command)
        {
            new DotnetNewCommand(MSTestContext, command.Split(" "))
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Type\\s+Author\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+project\\s+Microsoft\\s+Common/Console");
        }

        [TestMethod]
        [DataRow("--list --tag Common")]
        [DataRow("-l --tag Common")]
        [DataRow("list --tag Common")]
        [DataRow("--tag Common --list")]
        public void CanFilterTags(string command)
        {
            new DotnetNewCommand(MSTestContext, command.Split(" "))
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"These templates matched your input: --tag='Common'")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.NotHaveStdOutMatching("dotnet gitignore file\\s+gitignore\\s+Config")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library");
        }

        [TestMethod]
        [DataRow("app --list --tag Common")]
        [DataRow("app -l --tag Common")]
        [DataRow("--list app --tag Common")]
        [DataRow("list app --tag Common")]
        public void CanFilterTags_WithNameCriteria(string command)
        {
            new DotnetNewCommand(MSTestContext, command.Split(" "))
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining($"These templates matched your input: 'app', --tag='Common'")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.NotHaveStdOutMatching("dotnet gitignore file\\s+gitignore\\s+Config")
                .And.NotHaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library");
        }

        [TestMethod]
        public void CanShowMultipleShortNames()
        {
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();

            new DotnetNewCommand(MSTestContext, "--install", "Microsoft.DotNet.Web.ProjectTemplates.5.0")
                  .WithCustomHive(home)
                  .WithWorkingDirectory(workingDirectory)
                  .Execute()
                  .Should()
                  .ExitWith(0)
                  .And
                  .NotHaveStdErr()
                  .And.HaveStdOutMatching("ASP\\.NET Core Web App\\s+webapp,razor\\s+\\[C#\\]\\s+Web/MVC/Razor Pages");

            new DotnetNewCommand(MSTestContext, "--list")
                .WithCustomHive(home)
                .WithoutBuiltInTemplates()
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching("ASP\\.NET Core Web App\\s+webapp,razor\\s+\\[C#\\]\\s+Web/MVC/Razor Pages");

            new DotnetNewCommand(MSTestContext, "webapp", "--list")
                .WithCustomHive(home)
                .WithoutBuiltInTemplates()
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input: 'webapp'")
                .And.HaveStdOutMatching("ASP\\.NET Core Web App\\s+webapp,razor\\s+\\[C#\\]\\s+Web/MVC/Razor Pages");

            new DotnetNewCommand(MSTestContext, "razor", "--list")
                .WithCustomHive(home)
                .WithoutBuiltInTemplates()
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input: 'razor'")
                .And.HaveStdOutMatching("ASP\\.NET Core Web App\\s+webapp,razor\\s+\\[C#\\]\\s+Web/MVC/Razor Pages");
        }

#pragma warning disable xUnit1004 // Test methods should not be skipped
        [TestMethod(IgnoreMessage = "https://github.com/dotnet/sdk/issues/42541")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
        public void CanFilterByChoiceParameter()
        {
            new DotnetNewCommand(MSTestContext, "--list")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.HaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "c", "--list")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.HaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "c", "--list", "--framework")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input: 'c', --framework")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.NotHaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "c", "--list", "-f")
              .WithCustomHive(_sharedHome.HomeDirectory)
              .Execute()
              .Should()
              .ExitWith(0)
              .And.NotHaveStdErr()
              .And.HaveStdOutContaining("These templates matched your input: 'c', -f")
              .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
              .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
              .And.NotHaveStdOutMatching("dotnet gitignore file\\s+gitignore\\s+Config")
              .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
              .And.NotHaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "--list", "--framework")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input: --framework")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.NotHaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "--list", "-f")
              .WithCustomHive(_sharedHome.HomeDirectory)
              .Execute()
              .Should()
              .ExitWith(0)
              .And.NotHaveStdErr()
              .And.HaveStdOutContaining("These templates matched your input: -f")
              .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
              .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
              .And.NotHaveStdOutMatching("dotnet gitignore file\\s+gitignore\\s+Config")
              .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
              .And.NotHaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");
        }

#pragma warning disable xUnit1004 // Test methods should not be skipped
        [TestMethod(IgnoreMessage = "https://github.com/dotnet/sdk/issues/42541")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
        public void CanFilterByNonChoiceParameter()
        {
            new DotnetNewCommand(MSTestContext, "--list")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.HaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "c", "--list")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.HaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "c", "--list", "--langVersion")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input: 'c', --langVersion")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.NotHaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "--list", "--langVersion")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input: --langVersion")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.NotHaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");
        }

#pragma warning disable xUnit1004 // Test methods should not be skipped
        [TestMethod(IgnoreMessage = "https://github.com/dotnet/sdk/issues/42541")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
        public void IgnoresValueForNonChoiceParameter()
        {
            new DotnetNewCommand(MSTestContext, "--list")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.HaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "c", "--list")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.HaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "c", "--list", "--no-restore", "invalid")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input: 'c', --no-restore")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.NotHaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "--list", "--no-restore", "invalid")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input: --no-restore")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.NotHaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");
        }

#pragma warning disable xUnit1004 // Test methods should not be skipped
        [TestMethod(IgnoreMessage = "https://github.com/dotnet/sdk/issues/42541")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
        public void CanFilterByChoiceParameterWithValue()
        {
            new DotnetNewCommand(MSTestContext, "--list")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.HaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "c", "--list")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.HaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "c", "--list", "--framework", "net5.0")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input: 'c', --framework='net5.0'")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.NotHaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "c", "--list", "-f", "net5.0")
              .WithCustomHive(_sharedHome.HomeDirectory)
              .Execute()
              .Should()
              .ExitWith(0)
              .And.NotHaveStdErr()
              .And.HaveStdOutContaining("These templates matched your input: 'c', -f")
              .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
              .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
              .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
              .And.NotHaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "--list", "--framework", "net5.0")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input: --framework")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.NotHaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "--list", "-f", "net5.0")
              .WithCustomHive(_sharedHome.HomeDirectory)
              .Execute()
              .Should()
              .ExitWith(0)
              .And.NotHaveStdErr()
              .And.HaveStdOutContaining("These templates matched your input: -f")
              .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
              .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
              .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
              .And.NotHaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");
        }

#pragma warning disable xUnit1004 // Test methods should not be skipped
        [TestMethod(IgnoreMessage = "https://github.com/dotnet/sdk/issues/42541")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
        public void CannotListTemplatesWithUnknownParameter()
        {
            new DotnetNewCommand(MSTestContext, "--list")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.HaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "--list", "--unknown")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should().Fail()
                .And.HaveStdErrContaining("No templates found matching: --unknown.")
                .And.HaveStdErrContaining("9 template(s) partially matched, but failed on --unknown.")
                .And.HaveStdErrContaining($"To search for the templates on NuGet.org, run:{Environment.NewLine}   dotnet new <TEMPLATE_NAME> --search");

            new DotnetNewCommand(MSTestContext, "c", "--list", "--unknown")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should().Fail()
                .And.HaveStdErrContaining("No templates found matching: 'c', --unknown.")
                .And.HaveStdErrContaining("6 template(s) partially matched, but failed on --unknown.")
                .And.HaveStdErrContaining($"To search for the templates on NuGet.org, run:{Environment.NewLine}   dotnet new c --search");

            new DotnetNewCommand(MSTestContext, "c", "--list", "--unknown", "--language", "C#")
              .WithCustomHive(_sharedHome.HomeDirectory)
              .Execute()
              .Should().Fail()
              .And.HaveStdErrContaining("No templates found matching: 'c', language='C#', --unknown.")
              .And.HaveStdErrContaining("6 template(s) partially matched, but failed on language='C#', --unknown.")
              .And.HaveStdErrContaining($"To search for the templates on NuGet.org, run:{Environment.NewLine}   dotnet new c --search");
        }

#pragma warning disable xUnit1004 // Test methods should not be skipped
        [TestMethod(IgnoreMessage = "https://github.com/dotnet/sdk/issues/42541")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
        public void CannotListTemplatesWithUnknownValueForChoiceParameter()
        {
            new DotnetNewCommand(MSTestContext, "--list")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.HaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "--list", "--framework", "unknown")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should().Fail()
                .And.HaveStdErrContaining("No templates found matching: --framework='unknown'.")
                .And.HaveStdErrContaining("9 template(s) partially matched, but failed on --framework='unknown'.")
                .And.HaveStdErrContaining($"To search for the templates on NuGet.org, run:{Environment.NewLine}   dotnet new <TEMPLATE_NAME> --search");

            new DotnetNewCommand(MSTestContext, "c", "--list", "--framework", "unknown")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should().Fail()
                .And.HaveStdErrContaining("No templates found matching: 'c', --framework='unknown'.")
                .And.HaveStdErrContaining("6 template(s) partially matched, but failed on --framework='unknown'.")
                .And.HaveStdErrContaining($"To search for the templates on NuGet.org, run:{Environment.NewLine}   dotnet new c --search");
        }

#pragma warning disable xUnit1004 // Test methods should not be skipped
        [TestMethod(IgnoreMessage = "https://github.com/dotnet/sdk/issues/42541")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
        public void CannotListTemplatesForInvalidFilters()
        {
            new DotnetNewCommand(MSTestContext, "--list")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching("Template Name\\s+Short Name\\s+Language\\s+Tags")
                .And.HaveStdOutMatching("Console App\\s+console\\s+\\[C#\\],F#,VB\\s+Common/Console")
                .And.HaveStdOutMatching("Class Library\\s+classlib\\s+\\[C#\\],F#,VB\\s+Common/Library")
                .And.HaveStdOutMatching("NuGet Config\\s+nugetconfig\\s+Config");

            new DotnetNewCommand(MSTestContext, "--list", "--language", "unknown", "--framework", "unknown")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should().Fail()
                .And.HaveStdErrContaining("No templates found matching: language='unknown'.")
                .And.HaveStdErrContaining("9 template(s) partially matched, but failed on language='unknown'.")
                .And.HaveStdErrContaining($"To search for the templates on NuGet.org, run:{Environment.NewLine}   dotnet new <TEMPLATE_NAME> --search");

            new DotnetNewCommand(MSTestContext, "c", "--list", "--language", "unknown", "--framework", "unknown")
                .WithCustomHive(_sharedHome.HomeDirectory)
                .Execute()
                .Should().Fail()
                .And.HaveStdErrContaining("No templates found matching: 'c', language='unknown'.")
                .And.HaveStdErrContaining("6 template(s) partially matched, but failed on language='unknown'.")
                .And.HaveStdErrContaining($"To search for the templates on NuGet.org, run:{Environment.NewLine}   dotnet new c --search");
        }

        [TestMethod]
        public void TemplateGroupingTest()
        {
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDir = CreateTemporaryFolder();
            InstallTestTemplate("TemplateGrouping", MSTestContext, home, workingDir);

            new DotnetNewCommand(MSTestContext, "--list", "--columns-all")
                .WithCustomHive(home)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching("Basic FSharp +template-grouping +\\[C#],F# +item +Author1 +Test Asset +\\r?\\n +Q# +item,project +Author2 +Test Asset");
        }

        [TestMethod]
        [DataRow("author", "Author", "Microsoft")]
        [DataRow("type", "Type", "")]
        [DataRow("tags", "Tags", "Solution")]
        [DataRow("language", "Language", "")]
        public void TemplateWithSpecifiedColumnOutput(string columnName, string columnHeader, string columnValue)
        {
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();

            new DotnetNewCommand(MSTestContext, "--install", "Microsoft.DotNet.Web.ProjectTemplates.5.0")
                  .WithCustomHive(home)
                  .WithWorkingDirectory(workingDirectory)
                  .Execute()
                  .Should()
                  .ExitWith(0);

            new DotnetNewCommand(MSTestContext, "list", "--columns", columnName)
                .WithCustomHive(home)
                .Execute()
                .Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.HaveStdOutContaining("These templates matched your input:")
                .And.HaveStdOutMatching($"Template Name\\s+Short Name\\s+{columnHeader}")
                .And.HaveStdOutMatching($"Solution File\\s+sln,solution\\s+{columnValue}");
        }

        [TestMethod]
        [DataRow("c --list", "--list c")]
        [DataRow("c --list --language F#", "--list c --language F#")]
        [DataRow("c --list --columns-all", "--list c --columns-all")]
        public void CanFallbackToListOption(string command1, string command2)
        {
            CommandResult commandResult1 = new DotnetNewCommand(MSTestContext, command1.Split())
             .WithCustomHive(_sharedHome.HomeDirectory)
             .Execute();

            CommandResult commandResult2 = new DotnetNewCommand(MSTestContext, command2.Split())
               .WithCustomHive(_sharedHome.HomeDirectory)
               .Execute();

            Assert.AreEqual(commandResult1.StdOut, commandResult2.StdOut);
        }

        [TestMethod]
        [DataRow("--list foo --columns-all bar", "bar", "foo")]
        [DataRow("list foo --columns-all bar", "bar", "foo")]
        [DataRow("-l foo --columns-all bar", "bar", "foo")]
        [DataRow("--list foo bar", "bar", "foo")]
        [DataRow("list foo bar", "bar", "foo")]
        [DataRow("foo --list bar", "foo", "bar")]
        [DataRow("foo list bar", "foo", "bar")]
        [DataRow("foo --list bar --language F#", "foo", "bar")]
        [DataRow("foo --list --columns-all bar", "foo", "bar")]
        [DataRow("foo --list --columns-all --framework net6.0 bar", "bar|net6.0|foo", "--framework")]
        [DataRow("foo --list --columns-all -other-param --framework net6.0 bar", "bar|--framework|net6.0|foo", "-other-param")]
        public void CannotShowListOnParseError(string command, string invalidArguments, string validArguments)
        {
            CommandResult commandResult = new DotnetNewCommand(MSTestContext, command.Split())
             .WithCustomHive(_sharedHome.HomeDirectory)
             .Execute();

            commandResult.Should().Fail();
            foreach (string arg in invalidArguments.Split('|'))
            {
                commandResult.Should().HaveStdErrMatching($"Unrecognized command or (argument\\(s\\)\\:|argument) '{arg}'");
            }

            foreach (string arg in validArguments.Split('|'))
            {
                commandResult.Should()
                    .NotHaveStdErrContaining($"Unrecognized command or argument '{arg}'")
                    .And.NotHaveStdErrContaining($"Unrecognized command or argument(s): '{arg}'");
            }
        }
    }
}
