// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Cli.New.IntegrationTests
{
    public class SourcesTests : BaseIntegrationTest
    {
        public SourcesTests(MSTestContext testContext) : base(testContext)
        {
        }

        [TestMethod]
        public void EnsureItsPossibleToIncludePackagesLockJson()
        {
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate("SourceWithExcludeAndWithout", MSTestContext, home, workingDirectory);
            new DotnetNewCommand(MSTestContext, "withexclude")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0);
            Assert.AreEqual(
                new[] { "packages.lock.json", "foo.cs", "bar.cs" }.OrderBy(s => s),
                Directory.EnumerateFiles(workingDirectory, "*", SearchOption.AllDirectories).Select(Path.GetFileName).OrderBy(s => s));

            workingDirectory = CreateTemporaryFolder();
            new DotnetNewCommand(MSTestContext, "withoutexclude")
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0);
            Assert.AreEqual(
                new[] { "foo.cs", "bar.cs" }.OrderBy(s => s),
                Directory.EnumerateFiles(workingDirectory, "*", SearchOption.AllDirectories).Select(Path.GetFileName).OrderBy(s => s));
        }
    }
}
