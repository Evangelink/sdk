// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;

namespace Microsoft.NET.Build.Containers.IntegrationTests;

public class RegistryTests : IDisposable
{
    private MSTestContext _testContext;
    private readonly TestLoggerFactory _loggerFactory;

    public RegistryTests(MSTestContext testContext)
    {
        _testContext = testContext;
        _loggerFactory = new TestLoggerFactory(testContext);
    }

    public void Dispose()
    {
        _loggerFactory.Dispose();
    }

    [DataRow("quay.io/centos/centos")]
    [DataRow("registry.access.redhat.com/ubi8/dotnet-70")]
    [TestMethod]
    public async Task CanReadManifestFromRegistry(string fullyQualifiedContainerName)
    {
        bool parsed = ContainerHelpers.TryParseFullyQualifiedContainerName(fullyQualifiedContainerName,
                                                                           out string? containerRegistry,
                                                                           out string? containerName,
                                                                           out string? containerTag,
                                                                           out string? containerDigest,
                                                                           out bool isRegistrySpecified);
        Assert.IsTrue(parsed);
        Assert.IsTrue(isRegistrySpecified);
        Assert.IsNotNull(containerRegistry);
        Assert.IsNotNull(containerName);
        containerTag ??= "latest";

        ILogger logger = _loggerFactory.CreateLogger(nameof(CanReadManifestFromRegistry));
        Registry registry = new(containerRegistry, logger, RegistryMode.Pull);

        var ridgraphfile = ToolsetUtils.GetRuntimeGraphFilePath();

        ImageBuilder? downloadedImage = await registry.GetImageManifestAsync(
            containerName,
            containerTag,
            "linux-x64",
            ToolsetUtils.RidGraphManifestPicker,
            cancellationToken: default);

        Assert.IsNotNull(downloadedImage);
    }
}
