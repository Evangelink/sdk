// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.NET.TestFramework
{
    public abstract class SdkTest
    {
        protected TestAssetsManager _testAssetsManager;

        protected bool? UsingFullFrameworkMSBuild => TestContext.Current.ToolsetUnderTest?.ShouldUseFullFrameworkMSBuild;

        protected MSTestContext MSTestContext { get; }

        protected SdkTest(MSTestContext testContext)
        {
            MSTestContext = testContext;
            _testAssetsManager = new TestAssetsManager(testContext);
        }

        protected static void WaitForUtcNowToAdvance()
        {
            var start = DateTime.UtcNow;

            while (DateTime.UtcNow <= start)
            {
                Thread.Sleep(millisecondsTimeout: 1);
            }
        }
    }
}
