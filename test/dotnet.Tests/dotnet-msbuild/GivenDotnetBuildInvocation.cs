// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BuildCommand = Microsoft.DotNet.Tools.Build.BuildCommand;

namespace Microsoft.DotNet.Cli.MSBuild.Tests
{
    [Collection(TestConstants.UsesStaticTelemetryState)]
    public class GivenDotnetBuildInvocation : IClassFixture<NullCurrentSessionIdFixture>
    {
        const string ExpectedPrefix = "-maxcpucount -verbosity:m -tlp:default=auto -nologo";

        private static readonly string WorkingDirectory =
            TestPathUtilities.FormatAbsolutePath(nameof(GivenDotnetBuildInvocation));

        [TestMethod]
        [DataRow(new string[] { }, "")]
        [DataRow(new string[] { "-o", "foo" }, "-property:OutputPath=<cwd>foo -property:_CommandLineDefinedOutputPath=true")]
        [DataRow(new string[] { "-property:Verbosity=diag" }, "--property:Verbosity=diag")]
        [DataRow(new string[] { "--output", "foo" }, "-property:OutputPath=<cwd>foo -property:_CommandLineDefinedOutputPath=true")]
        [DataRow(new string[] { "--artifacts-path", "foo" }, "-property:ArtifactsPath=<cwd>foo")]
        [DataRow(new string[] { "-o", "foo1 foo2" }, "\"-property:OutputPath=<cwd>foo1 foo2\" -property:_CommandLineDefinedOutputPath=true")]
        [DataRow(new string[] { "--no-incremental" }, "-target:Rebuild")]
        [DataRow(new string[] { "-r", "rid" }, "-property:RuntimeIdentifier=rid -property:_CommandLineDefinedRuntimeIdentifier=true")]
        [DataRow(new string[] { "-r", "linux-amd64" }, "-property:RuntimeIdentifier=linux-x64 -property:_CommandLineDefinedRuntimeIdentifier=true")]
        [DataRow(new string[] { "--runtime", "rid" }, "-property:RuntimeIdentifier=rid -property:_CommandLineDefinedRuntimeIdentifier=true")]
        [DataRow(new string[] { "--use-current-runtime" }, "-property:UseCurrentRuntimeIdentifier=True")]
        [DataRow(new string[] { "--ucr" }, "-property:UseCurrentRuntimeIdentifier=True")]
        [DataRow(new string[] { "-c", "config" }, "-property:Configuration=config")]
        [DataRow(new string[] { "--configuration", "config" }, "-property:Configuration=config")]
        [DataRow(new string[] { "--version-suffix", "mysuffix" }, "-property:VersionSuffix=mysuffix")]
        [DataRow(new string[] { "--no-dependencies" }, "-property:BuildProjectReferences=false")]
        [DataRow(new string[] { "-v", "diag" }, "-verbosity:diag")]
        [DataRow(new string[] { "--verbosity", "diag" }, "-verbosity:diag")]
        [DataRow(new string[] { "--no-incremental", "-o", "myoutput", "-r", "myruntime", "-v", "diag", "/ArbitrarySwitchForMSBuild" },
                                  "-target:Rebuild -property:RuntimeIdentifier=myruntime -property:_CommandLineDefinedRuntimeIdentifier=true -verbosity:diag -property:OutputPath=<cwd>myoutput -property:_CommandLineDefinedOutputPath=true /ArbitrarySwitchForMSBuild")]
        [DataRow(new string[] { "/t:CustomTarget" }, "/t:CustomTarget")]
        [DataRow(new string[] { "--disable-build-servers" }, "--property:UseRazorBuildServer=false --property:UseSharedCompilation=false /nodeReuse:false")]
        public void MsbuildInvocationIsCorrect(string[] args, string expectedAdditionalArgs)
        {
            CommandDirectoryContext.PerformActionWithBasePath(WorkingDirectory, () =>
            {
                expectedAdditionalArgs =
                    (string.IsNullOrEmpty(expectedAdditionalArgs) ? "" : $" {expectedAdditionalArgs}")
                    .Replace("<cwd>", WorkingDirectory);

                var msbuildPath = "<msbuildpath>";
                var command = BuildCommand.FromArgs(args, msbuildPath);

                command.SeparateRestoreCommand.Should().BeNull();

                command.GetArgumentsToMSBuild()
                    .Should()
                    .Be($"{ExpectedPrefix} -restore -consoleloggerparameters:Summary{expectedAdditionalArgs}");
            });
        }

        [TestMethod]
        [DataRow(new string[] { "-f", "tfm" }, "-target:Restore -tlp:verbosity=quiet", "-property:TargetFramework=tfm")]
        [DataRow(new string[] { "-p:TargetFramework=tfm" }, "-target:Restore -tlp:verbosity=quiet", "--property:TargetFramework=tfm")]
        [DataRow(new string[] { "/p:TargetFramework=tfm" }, "-target:Restore -tlp:verbosity=quiet", "--property:TargetFramework=tfm")]
        [DataRow(new string[] { "-t:Run", "-f", "tfm" }, "-target:Restore -tlp:verbosity=quiet", "-property:TargetFramework=tfm -t:Run")]
        [DataRow(new string[] { "/t:Run", "-f", "tfm" }, "-target:Restore -tlp:verbosity=quiet", "-property:TargetFramework=tfm /t:Run")]
        [DataRow(new string[] { "-o", "myoutput", "-f", "tfm", "-v", "diag", "/ArbitrarySwitchForMSBuild" },
                                  "-target:Restore -tlp:verbosity=quiet -verbosity:diag -property:OutputPath=<cwd>myoutput -property:_CommandLineDefinedOutputPath=true /ArbitrarySwitchForMSBuild",
                                  "-property:TargetFramework=tfm -verbosity:diag -property:OutputPath=<cwd>myoutput -property:_CommandLineDefinedOutputPath=true /ArbitrarySwitchForMSBuild")]
        [DataRow(new string[] { "-f", "tfm", "-getItem:Compile", "-getProperty:TargetFramework", "-getTargetResult:Build" }, "-target:Restore -tlp:verbosity=quiet -nologo -verbosity:quiet", "-property:TargetFramework=tfm -getItem:Compile -getProperty:TargetFramework -getTargetResult:Build")]
        public void MsbuildInvocationIsCorrectForSeparateRestore(
            string[] args,
            string expectedAdditionalArgsForRestore,
            string expectedAdditionalArgs)
        {
            CommandDirectoryContext.PerformActionWithBasePath(WorkingDirectory, () =>
            {
                expectedAdditionalArgsForRestore = expectedAdditionalArgsForRestore.Replace("<cwd>", WorkingDirectory);

                expectedAdditionalArgs = (string.IsNullOrEmpty(expectedAdditionalArgs) ? "" : $" {expectedAdditionalArgs}");
                expectedAdditionalArgs = expectedAdditionalArgs.Replace("<cwd>", WorkingDirectory);

                var msbuildPath = "<msbuildpath>";
                var command = BuildCommand.FromArgs(args, msbuildPath);

                command.SeparateRestoreCommand.GetArgumentsToMSBuild()
                    .Should()
                    .Be($"{ExpectedPrefix} {expectedAdditionalArgsForRestore}");

                command.GetArgumentsToMSBuild()
                    .Should()
                    .Be($"{ExpectedPrefix} -nologo -consoleloggerparameters:Summary{expectedAdditionalArgs}");
            });
        }
    }
}
