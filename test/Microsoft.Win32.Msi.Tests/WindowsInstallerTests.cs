// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Win32.Msi.Tests
{
    public class WindowsInstallerTests
    {
        [WindowsOnlyTheory]
        [DataRow("", "", Error.INVALID_PARAMETER)]
        [DataRow("{807215B4-F42F-4E5F-BFEE-9817D7F2CEA5}", "ProductVersion", Error.UNKNOWN_PRODUCT)]
        public void InstallProductReturnsAnError(string productCode, string property, uint expectedError)
        {
            uint error = WindowsInstaller.GetProductInfo(productCode, property, out string propertyValue);

            Assert.AreEqual(error, expectedError);
        }

        [WindowsOnlyTheory]
        [DataRow("", InstallState.INVALIDARG)]
        [DataRow("{807215B4-F42F-4E5F-BFEE-9817D7F2CEA5}", InstallState.UNKNOWN)]
        public void QueryProductStateReturnsAnError(string productCode, InstallState expectedState)
        {
            InstallState state = WindowsInstaller.QueryProduct(productCode);

            Assert.AreEqual(state, expectedState);
        }
    }
}
