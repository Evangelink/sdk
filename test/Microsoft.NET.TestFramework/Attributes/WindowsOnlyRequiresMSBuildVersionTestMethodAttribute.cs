// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.NET.TestFramework
{
    public class WindowsOnlyRequiresMSBuildVersionTestMethodAttribute : TestMethodAttribute
    {
        /// <summary>
        /// Gets or sets the reason for potentially skipping the test if conditions are not met.
        /// </summary>
        public string? Reason { get; set; }
        
        public WindowsOnlyRequiresMSBuildVersionTestMethodAttribute(string version)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IgnoreMessage = "This test requires Windows to run";
            }

            RequiresMSBuildVersionTheoryAttribute.CheckForRequiredMSBuildVersion(this, version);
        }
    }
}
