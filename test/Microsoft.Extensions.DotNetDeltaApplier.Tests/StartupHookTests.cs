// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Watch.UnitTests
{
    [TestClass]
    public class StartupHookTests
    {
        [TestMethod]
        public void ClearHotReloadEnvironmentVariables_ClearsStartupHook()
        {
            Assert.AreEqual("", StartupHook.RemoveCurrentAssembly(typeof(StartupHook).Assembly.Location));
        }

        [TestMethod]
        public void ClearHotReloadEnvironmentVariables_PreservedOtherStartupHooks()
        {
            var customStartupHook = "/path/mycoolstartup.dll";
            Assert.AreEqual(customStartupHook, StartupHook.RemoveCurrentAssembly(typeof(StartupHook).Assembly.Location + Path.PathSeparator + customStartupHook));
        }

        [TestMethod]
        public void ClearHotReloadEnvironmentVariables_RemovesHotReloadStartup_InCaseInvariantManner()
        {
            var customStartupHook = "/path/mycoolstartup.dll";
            Assert.AreEqual(customStartupHook, StartupHook.RemoveCurrentAssembly(customStartupHook + Path.PathSeparator + typeof(StartupHook).Assembly.Location.ToUpperInvariant()));
        }

        [TestMethod]
        [CombinatorialData]
        public void IsMatchingProcess_Matching_SimpleName(
            [CombinatorialValues("", ".dll", ".exe")] string extension,
            [CombinatorialValues("", ".dll", ".exe")] string targetExtension)
        {
            var dir = Path.GetDirectoryName(typeof(StartupHookTests).Assembly.Location)!;
            var name = "a";
            var processPath = Path.Combine(dir, name + extension);
            var targetProcessPath = Path.Combine(dir, "a" + targetExtension);

            Assert.IsTrue(StartupHook.IsMatchingProcess(processPath, targetProcessPath));
        }

        [TestMethod]
        [CombinatorialData]
        public void IsMatchingProcess_Matching_DotInName(
            [CombinatorialValues("", ".dll", ".exe")] string extension,
            [CombinatorialValues("", ".dll", ".exe")] string targetExtension)
        {
            var dir = Path.GetDirectoryName(typeof(StartupHookTests).Assembly.Location)!;
            var name = "a.b";
            var processPath = Path.Combine(dir, name + extension);
            var targetProcessPath = Path.Combine(dir, name + targetExtension);

            Assert.IsTrue(StartupHook.IsMatchingProcess(processPath, targetProcessPath));
        }

        [TestMethod]
        [CombinatorialData]
        public void IsMatchingProcess_Matching_DotDllInName(
            [CombinatorialValues("", ".dll", ".exe")] string extension,
            [CombinatorialValues("", ".dll", ".exe")] string targetExtension)
        {
            var dir = Path.GetDirectoryName(typeof(StartupHookTests).Assembly.Location)!;
            var name = "a.dll";
            var processPath = Path.Combine(dir, name + extension);
            var targetProcessPath = Path.Combine(dir, name + targetExtension);

            Assert.IsTrue(StartupHook.IsMatchingProcess(processPath, targetProcessPath));
        }

        [TestMethod]
        [CombinatorialData]
        public void IsMatchingProcess_NotMatching(
            [CombinatorialValues("", ".dll", ".exe")] string extension,
            [CombinatorialValues("", ".dll", ".exe")] string targetExtension)
        {
            var dir = Path.GetDirectoryName(typeof(StartupHookTests).Assembly.Location)!;
            var processPath = Path.Combine(dir, "a" + extension);
            var targetProcessPath = Path.Combine(dir, "b" + targetExtension);

            Assert.IsFalse(StartupHook.IsMatchingProcess(processPath, targetProcessPath));
        }
    }
}
