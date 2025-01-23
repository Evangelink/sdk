// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Microsoft.DotNet.Watch.UnitTests;

public class DebugTestOutputLogger(MSTestContext testContext) : ITestOutputHelper
{
    public event Action<string>? OnMessage;

    public void WriteLine(string message)
    {
        Debug.WriteLine($"[TEST] {message}");
        testContext.WriteLine(message);
        OnMessage?.Invoke(message);
    }

    public void WriteLine(string format, params object[] args)
        => WriteLine(string.Format(format, args));
}
