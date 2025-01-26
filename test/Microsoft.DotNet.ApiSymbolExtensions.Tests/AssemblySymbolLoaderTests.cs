// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.DotNet.ApiSymbolExtensions.Tests
{
    [TestClass]
    public class AssemblySymbolLoaderTests : SdkTest
    {
        public AssemblySymbolLoaderTests(MSTestContext testContext) : base(testContext) { }

        private const string SimpleAssemblySourceContents = @"
namespace MyNamespace
{
    public class MyClass
    {
    }
}
";

        // Since we use typeof(string).Assembly.Location to resolve references
        // We need to target a framework compatible with what the test is being
        // built for so that we resolve the references correctly.
#if NETCOREAPP
        private const string TargetFrameworks = ToolsetInfo.CurrentTargetFramework;
#else
        private const string TargetFrameworks = "net471";
#endif

        private class TestAssetInfo
        {
            public TestAsset TestAsset { get; set; }
            public string OutputDirectory { get; set; }
        }

        // We use the same asset in multiple tests.
        // Creating a TestAsset and building it for each test
        // creates a lot of overhead, using the cache
        // speeds up test execution ~3x in this test assembly.
        // Tests within the same class run serially in xunit, so
        // it is fine to reuse the same asset.
        private class TestAssetCache
        {
            public static TestAssetCache Instance = new();

            private ConcurrentDictionary<string, TestAssetInfo> Dictionary = new();

            private TestAssetInfo _asset = null;

            public TestAssetInfo GetSimpleAsset(TestAssetsManager manager)
                => _asset ?? InitAsset(manager);

            private TestAssetInfo InitAsset(TestAssetsManager manager)
            {
                Interlocked.CompareExchange(ref _asset, GetAsset(manager), null);
                return _asset;
            }

            private TestAssetInfo GetAsset(TestAssetsManager manager)
            {
                TestProject project = new("SimpleAsset")
                {
                    TargetFrameworks = TargetFrameworks,
                    IsExe = false
                };

                project.SourceFiles.Add("MyClass.cs", SimpleAssemblySourceContents);

                TestAsset testAsset = manager.CreateTestProject(project);
                BuildTestAsset(testAsset, out string outDir)
                    .Should()
                    .Pass();

                return new TestAssetInfo()
                {
                    TestAsset = testAsset,
                    OutputDirectory = outDir,
                };
            }
        }

        private TestAssetInfo GetSimpleTestAsset() => TestAssetCache.Instance.GetSimpleAsset(_testAssetsManager);

        [TestMethod]
        public void LoadAssembly_Throws()
        {
            TestLog log = new();
            AssemblySymbolLoader loader = new(log);
            Assert.Throws<FileNotFoundException>(() => loader.LoadAssembly(Guid.NewGuid().ToString("N").Substring(0, 8)));
        }

        [TestMethod]
        public void LoadAssemblyFromSourceFiles_Throws()
        {
            TestLog log = new();
            AssemblySymbolLoader loader = new(log);
            IEnumerable<string> paths = new[] { Guid.NewGuid().ToString("N") };
            Assert.Throws<FileNotFoundException>(() => loader.LoadAssemblyFromSourceFiles(paths, "assembly1", Array.Empty<string>()));
            Assert.Throws<ArgumentNullException>("filePaths", () => loader.LoadAssemblyFromSourceFiles(Array.Empty<string>(), "assembly1", Array.Empty<string>()));
            Assert.Throws<ArgumentNullException>("assemblyName", () => loader.LoadAssemblyFromSourceFiles(paths, null, Array.Empty<string>()));
        }

        [TestMethod]
        public void LoadMatchingAssemblies_Throws()
        {
            TestLog log = new();
            AssemblySymbolLoader loader = new(log);
            IEnumerable<string> paths = new[] { Guid.NewGuid().ToString("N") };
            IAssemblySymbol assembly = SymbolFactory.GetAssemblyFromSyntax("namespace MyNamespace { class Foo { } }");

            Assert.Throws<FileNotFoundException>(() => loader.LoadMatchingAssemblies(new[] { assembly }, paths));
        }

        [TestMethod]
        public void LoadMatchingAssembliesWarns()
        {
            IAssemblySymbol assembly = SymbolFactory.GetAssemblyFromSyntax("namespace MyNamespace { class Foo { } }");
            IEnumerable<string> paths = new[] { AppContext.BaseDirectory };

            TestLog log = new();
            AssemblySymbolLoader loader = new(log);
            IEnumerable<IAssemblySymbol> symbols = loader.LoadMatchingAssemblies(new[] { assembly }, paths);
            Assert.IsEmpty(symbols);
            Assert.IsTrue(log.HasLoggedWarnings);
            List<string> expected = [ $"{AssemblySymbolLoader.AssemblyNotFoundErrorCode} Could not find matching assembly: '{assembly.Identity.GetDisplayName()}' in any of the search directories." ];
            Assert.AreEqual(expected, log.Warnings, StringComparer.CurrentCultureIgnoreCase);
        }

        [TestMethod]
        public void LoadMatchingAssembliesSameIdentitySucceeds()
        {
            string assemblyName = nameof(LoadMatchingAssembliesSameIdentitySucceeds);
            IAssemblySymbol fromAssembly = SymbolFactory.GetAssemblyFromSyntax(SimpleAssemblySourceContents, assemblyName: assemblyName);

            TestProject testProject = new(assemblyName)
            {
                TargetFrameworks = ToolsetInfo.CurrentTargetFramework,
                IsExe = false,
            };

            testProject.SourceFiles.Add("MyClass.cs", SimpleAssemblySourceContents);
            testProject.AdditionalProperties.Add("AssemblyVersion", "0.0.0.0");
            TestAsset testAsset = _testAssetsManager.CreateTestProject(testProject);

            BuildTestAsset(testAsset, out string outputDirectory)
                .Should()
                .Pass();

            TestLog log = new();
            AssemblySymbolLoader loader = new(log);
            IEnumerable<IAssemblySymbol> matchingAssemblies = loader.LoadMatchingAssemblies(new[] { fromAssembly }, new[] { outputDirectory });

            Assert.ContainsSingle(matchingAssemblies);
            Assert.IsFalse(log.HasLoggedWarnings);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void LoadMatchingAssemblies_DifferentIdentity(bool validateIdentities)
        {
            var assetInfo = GetSimpleTestAsset();
            IAssemblySymbol fromAssembly = SymbolFactory.GetAssemblyFromSyntax(SimpleAssemblySourceContents, assemblyName: assetInfo.TestAsset.TestProject.Name);

            TestLog log = new();
            AssemblySymbolLoader loader = new(log);
            IEnumerable<IAssemblySymbol> matchingAssemblies = loader.LoadMatchingAssemblies(new[] { fromAssembly }, new[] { assetInfo.OutputDirectory }, validateMatchingIdentity: validateIdentities);

            if (validateIdentities)
            {
                Assert.IsEmpty(matchingAssemblies);
                Assert.IsTrue(log.HasLoggedWarnings);
                List<string> expected = [$"{AssemblySymbolLoader.AssemblyNotFoundErrorCode} Could not find matching assembly: '{fromAssembly.Identity.GetDisplayName()}' in any of the search directories."];
                Assert.AreEqual(expected, log.Warnings, StringComparer.CurrentCultureIgnoreCase);
            }
            else
            {
                Assert.ContainsSingle(matchingAssemblies);
                Assert.IsFalse(log.HasLoggedWarnings);
                Assert.AreNotEqual(fromAssembly.Identity, matchingAssemblies.FirstOrDefault().Identity);
            }
        }

        [TestMethod]
        public void LoadsSimpleAssemblyFromDirectory()
        {
            var assetInfo = GetSimpleTestAsset();
            TestLog log = new();
            AssemblySymbolLoader loader = new(log);
            IEnumerable<IAssemblySymbol> symbols = loader.LoadAssemblies(assetInfo.OutputDirectory);
            Assert.ContainsSingle(symbols);

            IEnumerable<ITypeSymbol> types = symbols.FirstOrDefault()
                .GlobalNamespace
                .GetNamespaceMembers()
                .FirstOrDefault((n) => n.Name == "MyNamespace")
                .GetTypeMembers();

            Assert.ContainsSingle(types);
            Assert.AreEqual("MyNamespace.MyClass", types.FirstOrDefault().ToDisplayString());
        }

        [TestMethod]
        public void LoadSimpleAssemblyFullPath()
        {
            var assetInfo = GetSimpleTestAsset();
            TestLog log = new();
            AssemblySymbolLoader loader = new(log);
            IAssemblySymbol symbol = loader.LoadAssembly(Path.Combine(assetInfo.OutputDirectory, assetInfo.TestAsset.TestProject.Name + ".dll"));

            IEnumerable<ITypeSymbol> types = symbol.GlobalNamespace
                .GetNamespaceMembers()
                .FirstOrDefault((n) => n.Name == "MyNamespace")
                .GetTypeMembers();

            Assert.ContainsSingle(types);
            Assert.AreEqual("MyNamespace.MyClass", types.FirstOrDefault().ToDisplayString());
        }

        [TestMethod]
        public void LoadsMultipleAssembliesFromDirectory()
        {
            TestProject first = new("LoadsMultipleAssembliesFromDirectory_First")
            {
                TargetFrameworks = TargetFrameworks,
                IsExe = false
            };

            TestProject second = new("LoadsMultipleAssembliesFromDirectory_Second")
            {
                TargetFrameworks = TargetFrameworks,
                IsExe = false
            };

            first.ReferencedProjects.Add(second);
            TestAsset testAsset = _testAssetsManager.CreateTestProject(first);

            BuildTestAsset(testAsset, out string outputDirectory)
                .Should()
                .Pass();

            TestLog log = new();
            AssemblySymbolLoader loader = new(log);
            IEnumerable<IAssemblySymbol> symbols = loader.LoadAssemblies(outputDirectory);

            Assert.AreEqual(2, symbols.Count());

            IEnumerable<string> expected = new[] { "LoadsMultipleAssembliesFromDirectory_First", "LoadsMultipleAssembliesFromDirectory_Second" };
            IEnumerable<string> actual = symbols.Select(a => a.Name).OrderBy(a => a, StringComparer.Ordinal);

            Assert.AreEqual(expected, actual, StringComparer.Ordinal);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void LoadAssemblyResolveReferences_WarnsWhenEnabled(bool resolveReferences)
        {
            var assetInfo = GetSimpleTestAsset();
            TestLog log = new();
            AssemblySymbolLoader loader = new(log, resolveAssemblyReferences: resolveReferences);
            loader.LoadAssembly(Path.Combine(assetInfo.OutputDirectory, assetInfo.TestAsset.TestProject.Name + ".dll"));

            if (resolveReferences)
            {
                Assert.IsTrue(log.HasLoggedWarnings);

                string expectedReference = "System.Runtime.dll";

                if (TargetFrameworks.StartsWith("net4", StringComparison.OrdinalIgnoreCase))
                {
                    expectedReference = "mscorlib.dll";
                }

                List<string> expected = [$"{AssemblySymbolLoader.AssemblyReferenceNotFoundErrorCode} Could not resolve reference '{expectedReference}' in any of the provided search directories."];
                Assert.AreEqual(expected, log.Warnings, StringComparer.CurrentCultureIgnoreCase);
            }
            else
            {
                Assert.IsEmpty(log.Warnings);
            }
        }

        [TestMethod]
        public void LoadAssembliesShouldResolveReferencesNoWarnings()
        {
            var assetInfo = GetSimpleTestAsset();
            TestLog log = new();
            AssemblySymbolLoader loader = new(log, resolveAssemblyReferences: true);
            // AddReferenceSearchDirectories should be able to handle directories as well as full path to assemblies.
            loader.AddReferenceSearchPaths(Path.GetDirectoryName(typeof(string).Assembly.Location));
            loader.AddReferenceSearchPaths(Path.GetFullPath(typeof(string).Assembly.Location));
            loader.LoadAssembly(Path.Combine(assetInfo.OutputDirectory, assetInfo.TestAsset.TestProject.Name + ".dll"));

            Assert.IsEmpty(log.Warnings);

            // Ensure we loaded more than one assembly since resolveReferences was set to true.
            Dictionary<string, MetadataReference> loadedAssemblies = (Dictionary<string, MetadataReference>)typeof(AssemblySymbolLoader)?.GetField("_loadedAssemblies", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(loader);
            Assert.IsTrue(loadedAssemblies != null && loadedAssemblies.Count > 1);
        }

        [TestMethod]
        public void LoadAssemblyFromStreamNoWarns()
        {
            var assetInfo = GetSimpleTestAsset();
            TestProject testProject = assetInfo.TestAsset.TestProject;
            TestLog log = new();
            AssemblySymbolLoader loader = new(log);
            using FileStream stream = File.OpenRead(Path.Combine(assetInfo.OutputDirectory, testProject.Name + ".dll"));
            IAssemblySymbol symbol = loader.LoadAssembly(testProject.Name, stream);

            Assert.IsFalse(log.HasLoggedWarnings);
            Assert.AreEqual(testProject.Name, symbol.Name, StringComparer.Ordinal);

            IEnumerable<ITypeSymbol> types = symbol.GlobalNamespace
                .GetNamespaceMembers()
                .FirstOrDefault((n) => n.Name == "MyNamespace")
                .GetTypeMembers();

            Assert.ContainsSingle(types);
            Assert.AreEqual("MyNamespace.MyClass", types.FirstOrDefault().ToDisplayString());
        }

        private static CommandResult BuildTestAsset(TestAsset testAsset, out string outputDirectory)
        {
            BuildCommand buildCommand = new(testAsset);
            outputDirectory = buildCommand.GetOutputDirectory(testAsset.TestProject.TargetFrameworks).FullName;
            return buildCommand.Execute();
        }
    }
}
