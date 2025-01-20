// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using PublishCommand = Microsoft.DotNet.Tools.Publish.PublishCommand;

namespace Microsoft.DotNet.Cli.MSBuild.Tests
{
    [Collection(TestConstants.UsesStaticTelemetryState)]
    public class GivenDotnetPublishInvocation : IClassFixture<NullCurrentSessionIdFixture>
    {
        private static readonly string WorkingDirectory =
            TestPathUtilities.FormatAbsolutePath(nameof(GivenDotnetPublishInvocation));
        private readonly MSTestContext testContext;

        public GivenDotnetPublishInvocation(MSTestContext testContext)
        {
            this.testContext = testContext;
        }

        const string ExpectedPrefix = "-maxcpucount -verbosity:m -tlp:default=auto -nologo";
        const string ExpectedProperties = "--property:_IsPublishing=true";

        [TestMethod]
        [DataRow(new string[] { }, "")]
        [DataRow(new string[] { "-r", "<rid>" }, "-property:RuntimeIdentifier=<rid> -property:_CommandLineDefinedRuntimeIdentifier=true")]
        [DataRow(new string[] { "-r", "linux-amd64" }, "-property:RuntimeIdentifier=linux-x64 -property:_CommandLineDefinedRuntimeIdentifier=true")]
        [DataRow(new string[] { "--runtime", "<rid>" }, "-property:RuntimeIdentifier=<rid> -property:_CommandLineDefinedRuntimeIdentifier=true")]
        [DataRow(new string[] { "--use-current-runtime" }, "-property:UseCurrentRuntimeIdentifier=True")]
        [DataRow(new string[] { "--ucr" }, "-property:UseCurrentRuntimeIdentifier=True")]
        [DataRow(new string[] { "-o", "<publishdir>" }, "-property:PublishDir=<cwd><publishdir> -property:_CommandLineDefinedOutputPath=true")]
        [DataRow(new string[] { "--output", "<publishdir>" }, "-property:PublishDir=<cwd><publishdir> -property:_CommandLineDefinedOutputPath=true")]
        [DataRow(new string[] { "--artifacts-path", "foo" }, "-property:ArtifactsPath=<cwd>foo")]
        [DataRow(new string[] { "-c", "<config>" }, "-property:Configuration=<config> -property:DOTNET_CLI_DISABLE_PUBLISH_AND_PACK_RELEASE=true")]
        [DataRow(new string[] { "--configuration", "<config>" }, "-property:Configuration=<config> -property:DOTNET_CLI_DISABLE_PUBLISH_AND_PACK_RELEASE=true")]
        [DataRow(new string[] { "--version-suffix", "<versionsuffix>" }, "-property:VersionSuffix=<versionsuffix>")]
        [DataRow(new string[] { "--manifest", "<manifestfiles>" }, "-property:TargetManifestFiles=<cwd><manifestfiles>")]
        [DataRow(new string[] { "-v", "minimal" }, "-verbosity:minimal")]
        [DataRow(new string[] { "--verbosity", "minimal" }, "-verbosity:minimal")]
        [DataRow(new string[] { "<project>" }, "<project>")]
        [DataRow(new string[] { "<project>", "<extra-args>" }, "<project> <extra-args>")]
        [DataRow(new string[] { "--disable-build-servers" }, "--property:UseRazorBuildServer=false --property:UseSharedCompilation=false /nodeReuse:false")]
        public void MsbuildInvocationIsCorrect(string[] args, string expectedAdditionalArgs)
        {
            CommandDirectoryContext.PerformActionWithBasePath(WorkingDirectory, () =>
            {
                expectedAdditionalArgs =
                    (string.IsNullOrEmpty(expectedAdditionalArgs) ? "" : $" {expectedAdditionalArgs}")
                    .Replace("<cwd>", WorkingDirectory);

                var msbuildPath = "<msbuildpath>";
                var command = PublishCommand.FromArgs(args, msbuildPath);

                command.SeparateRestoreCommand
                    .Should()
                    .BeNull();

                command.GetArgumentsToMSBuild()
                    .Should()
                    .Be($"{ExpectedPrefix} -restore -target:Publish {ExpectedProperties}{expectedAdditionalArgs}");
            });
        }

        [TestMethod]
        [DataRow(new string[] { "-f", "<tfm>" }, "-property:TargetFramework=<tfm>")]
        [DataRow(new string[] { "--framework", "<tfm>" }, "-property:TargetFramework=<tfm>")]
        public void MsbuildInvocationIsCorrectForSeparateRestore(string[] args, string expectedAdditionalArgs)
        {
            expectedAdditionalArgs = (string.IsNullOrEmpty(expectedAdditionalArgs) ? "" : $" {expectedAdditionalArgs}");

            var msbuildPath = "<msbuildpath>";
            var command = PublishCommand.FromArgs(args, msbuildPath);

            command.SeparateRestoreCommand
                   .GetArgumentsToMSBuild()
                   .Should()
                   .Be($"{ExpectedPrefix} -target:Restore -tlp:verbosity=quiet {ExpectedProperties}");

            command.GetArgumentsToMSBuild()
                   .Should()
                   .Be($"{ExpectedPrefix} -nologo -target:Publish {ExpectedProperties}{expectedAdditionalArgs}");
        }

        [TestMethod]
        public void MsbuildInvocationIsCorrectForNoBuild()
        {
            var msbuildPath = "<msbuildpath>";
            var command = PublishCommand.FromArgs(new[] { "--no-build" }, msbuildPath);

            command.SeparateRestoreCommand
                   .Should()
                   .BeNull();

            // NOTE --no-build implies no-restore hence no -restore argument to msbuild below.
            command.GetArgumentsToMSBuild()
                   .Should()
                   .Be($"{ExpectedPrefix} -target:Publish {ExpectedProperties} -property:NoBuild=true");
        }

        [TestMethod]
        public void CommandAcceptsMultipleCustomProperties()
        {
            var msbuildPath = "<msbuildpath>";
            var command = PublishCommand.FromArgs(new[] { "/p:Prop1=prop1", "/p:Prop2=prop2" }, msbuildPath);

            command.GetArgumentsToMSBuild()
               .Should()
               .Be($"{ExpectedPrefix} -restore -target:Publish {ExpectedProperties} --property:Prop1=prop1 --property:Prop2=prop2");
        }
    }
}
