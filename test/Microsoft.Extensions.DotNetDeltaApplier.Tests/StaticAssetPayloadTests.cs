// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.HotReload;

namespace Microsoft.DotNet.Watch.UnitTests;

public class StaticAssetPayloadTests
{
    [TestMethod]
    public async Task Roundtrip()
    {
        var initial = new StaticAssetPayload(
            assemblyName: "assembly name",
            relativePath: "some path",
            [1, 2, 3],
            isApplicationProject: true);

        using var stream = new MemoryStream();
        await initial.WriteAsync(stream, CancellationToken.None);

        stream.Position = 0;
        var read = await StaticAssetPayload.ReadAsync(stream, CancellationToken.None);

        AssertEqual(initial, read);
    }

    private static void AssertEqual(StaticAssetPayload initial, StaticAssetPayload read)
    {
        Assert.AreEqual(initial.AssemblyName, read.AssemblyName);
        Assert.AreEqual(initial.RelativePath, read.RelativePath);
        Assert.AreEqual(initial.IsApplicationProject, read.IsApplicationProject);
        Assert.AreEqual(initial.Contents, read.Contents);
    }
}
