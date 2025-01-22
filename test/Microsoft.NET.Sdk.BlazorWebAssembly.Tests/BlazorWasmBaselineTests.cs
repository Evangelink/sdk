// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.NET.Sdk.Razor.Tests;

namespace Microsoft.NET.Sdk.BlazorWebAssembly.Tests
{
    public class BlazorWasmBaselineTests(MSTestContext testContext, bool generateBaselines) : AspNetSdkBaselineTest(testContext, generateBaselines)
    {
        protected override string EmbeddedResourcePrefix => string.Join('.', "Microsoft.NET.Sdk.BlazorWebAssembly.Tests", "StaticWebAssetsBaselines");

        protected override string ComputeBaselineFolder() =>
            Path.Combine(MSTestContext.GetRepoRoot() ?? AppContext.BaseDirectory, "test", "Microsoft.NET.Sdk.BlazorWebAssembly.Tests", "StaticWebAssetsBaselines");
    }
}
