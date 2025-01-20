// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using PackCommand = Microsoft.DotNet.Tools.Pack.PackCommand;

namespace Microsoft.DotNet.Cli.MSBuild.Tests
{
    [Collection(TestConstants.UsesStaticTelemetryState)]
    public class GivenDotnetPackInvocation : IClassFixture<NullCurrentSessionIdFixture>
    {
        const string ExpectedPrefix = "-maxcpucount -verbosity:m -tlp:default=auto -nologo -restore -target:pack";
        const string ExpectedNoBuildPrefix = "-maxcpucount -verbosity:m -tlp:default=auto -nologo -target:pack";
        const string ExpectedProperties = "--property:_IsPacking=true";

        private static readonly string WorkingDirectory =
            TestPathUtilities.FormatAbsolutePath(nameof(GivenDotnetPackInvocation));

        [TestMethod]
        [DataRow(new string[] { }, "")]
        [DataRow(new string[] { "-o", "<packageoutputpath>" }, "-property:PackageOutputPath=<cwd><packageoutputpath>")]
        [DataRow(new string[] { "--output", "<packageoutputpath>" }, "-property:PackageOutputPath=<cwd><packageoutputpath>")]
        [DataRow(new string[] { "--artifacts-path", "foo" }, "-property:ArtifactsPath=<cwd>foo")]
        [DataRow(new string[] { "--no-build" }, "-property:NoBuild=true")]
        [DataRow(new string[] { "--include-symbols" }, "-property:IncludeSymbols=true")]
        [DataRow(new string[] { "--include-source" }, "-property:IncludeSource=true")]
        [DataRow(new string[] { "-c", "<config>" }, "-property:Configuration=<config> -property:DOTNET_CLI_DISABLE_PUBLISH_AND_PACK_RELEASE=true")]
        [DataRow(new string[] { "--configuration", "<config>" }, "-property:Configuration=<config> -property:DOTNET_CLI_DISABLE_PUBLISH_AND_PACK_RELEASE=true")]
        [DataRow(new string[] { "--version-suffix", "<versionsuffix>" }, "-property:VersionSuffix=<versionsuffix>")]
        [DataRow(new string[] { "-s" }, "-property:Serviceable=true")]
        [DataRow(new string[] { "--serviceable" }, "-property:Serviceable=true")]
        [DataRow(new string[] { "-v", "diag" }, "-verbosity:diag")]
        [DataRow(new string[] { "--verbosity", "diag" }, "-verbosity:diag")]
        [DataRow(new string[] { "<project>" }, "<project>")]
        [DataRow(new string[] { "--disable-build-servers" }, "--property:UseRazorBuildServer=false --property:UseSharedCompilation=false /nodeReuse:false")]

        public void MsbuildInvocationIsCorrect(string[] args, string expectedAdditionalArgs)
        {
            CommandDirectoryContext.PerformActionWithBasePath(WorkingDirectory, () =>
            {
                expectedAdditionalArgs =
                    (string.IsNullOrEmpty(expectedAdditionalArgs) ? "" : $" {expectedAdditionalArgs}")
                    .Replace("<cwd>", WorkingDirectory);

                var msbuildPath = "<msbuildpath>";
                var command = PackCommand.FromArgs(args, msbuildPath);
                var expectedPrefix = args.FirstOrDefault() == "--no-build" ? ExpectedNoBuildPrefix : ExpectedPrefix;

                command.SeparateRestoreCommand.Should().BeNull();
                command.GetArgumentsToMSBuild().Should().Be($"{expectedPrefix} {ExpectedProperties}{expectedAdditionalArgs}");
            });
        }
    }
}
