// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.Win32;

namespace Microsoft.DotNet.Cli.Utils.Tests
{
#pragma warning disable CA1416
    public class DependencyProviderTests
    {
        [WindowsOnlyTheory]
        [DataRow(false, "NET.CORE.SDK,v6.0", @"SOFTWARE\Classes\Installer\Dependencies\NET.CORE.SDK,v6.0\Dependents", "HKEY_CURRENT_USER")]
        [DataRow(true, "NET.CORE.SDK,v6.0", @"SOFTWARE\Classes\Installer\Dependencies\NET.CORE.SDK,v6.0\Dependents", "HKEY_LOCAL_MACHINE")]
        public void ProviderProperties(bool allUsers, string providerKeyName, string expectedDependentsKeyPath, string expectedBaseKeyName)
        {
            DependencyProvider dep = new(providerKeyName, allUsers);

            Assert.AreEqual(expectedDependentsKeyPath, dep.DependentsKeyPath);
            Assert.AreEqual(expectedBaseKeyName, dep.BaseKey.Name);
            Assert.AreEqual(providerKeyName, dep.ProviderKeyName);
        }

        [WindowsOnlyFact]
        public void ItCanAddDependents()
        {
            // We cannot create per-machine entries unless the tests run elevated. The results are the
            // the same, it's only the base key that's different
            DependencyProvider dep = new(".NET_SDK_TEST_PROVIDER_KEY", allUsers: false);

            try
            {
                // We should not have any dependents
                Assert.HasCount(0, dep.Dependents);

                dep.AddDependent("Microsoft.NET.SDK,v6.0.100");

                Assert.HasCount(1, dep.Dependents);
                Assert.AreEqual("Microsoft.NET.SDK,v6.0.100", dep.Dependents.First());
            }
            finally
            {
                DeleteProviderKey(dep);
            }
        }

        [WindowsOnlyFact]
        public void ItCanFindVisualStudioDependents()
        {
            DependencyProvider dep = new(".NET_SDK_TEST_PROVIDER_KEY", allUsers: false);

            try
            {
                // We should not have any dependents
                Assert.HasCount(0, dep.Dependents);

                // Write the VS dependents key
                dep.AddDependent(DependencyProvider.VisualStudioDependentKeyName);

                Assert.IsTrue(dep.HasVisualStudioDependency);
            }
            finally
            {
                DeleteProviderKey(dep);
            }
        }

        [WindowsOnlyFact]
        public void ItWillNotRemoveTheProviderIfOtherDependentsExist()
        {
            DependencyProvider dep = new(".NET_SDK_TEST_PROVIDER_KEY", allUsers: false);

            try
            {
                // Write multiple dependents
                dep.AddDependent(DependencyProvider.VisualStudioDependentKeyName);
                dep.AddDependent("Microsoft.NET.SDK,v6.0.100");

                Assert.AreEqual(2, dep.Dependents.Count());

                dep.RemoveDependent("Microsoft.NET.SDK,v6.0.100", removeProvider: true);

                Assert.IsTrue(dep.HasVisualStudioDependency);
            }
            finally
            {
                DeleteProviderKey(dep);
            }
        }

        [WindowsOnlyFact]
        public void ItReturnsNullIfProductCodeDoesNotExist()
        {
            string providerKeyName = "Microsoft.NET.Test.Pack";
            DependencyProvider dep = new(providerKeyName, allUsers: false);
            using RegistryKey providerKey = Registry.CurrentUser.CreateSubKey(Path.Combine(DependencyProvider.DependenciesKeyRelativePath, providerKeyName), writable: true);

            try
            {
                Assert.Null(dep.ProductCode);
            }
            finally
            {
                DeleteProviderKey(dep);
            }
        }

        [WindowsOnlyFact]
        public void ItCanRetrieveTheProductCodeFromTheProviderKey()
        {
            string providerKeyName = "Microsoft.NET.Test.Pack";
            DependencyProvider dep = new(providerKeyName, allUsers: false);
            using RegistryKey providerKey = Registry.CurrentUser.CreateSubKey(Path.Combine(DependencyProvider.DependenciesKeyRelativePath, providerKeyName), writable: true);
            string productCode = Guid.NewGuid().ToString("B");
            providerKey?.SetValue(null, productCode);

            try
            {
                Assert.AreEqual(productCode, dep.ProductCode);
            }
            finally
            {
                DeleteProviderKey(dep);
            }
        }

        private void DeleteProviderKey(DependencyProvider dep)
        {
            using RegistryKey providerKey = dep.BaseKey.OpenSubKey(DependencyProvider.DependenciesKeyRelativePath, writable: true);
            providerKey?.DeleteSubKeyTree(dep.ProviderKeyName);
        }
    }
#pragma warning restore CA1416
}
