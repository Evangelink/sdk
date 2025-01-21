// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace Microsoft.DotNet.Watch.UnitTests
{
    internal class TestConsole : IConsole
    {
        public event Action<ConsoleKeyInfo>? KeyPressed;

        private readonly TestOutputWriter _testWriter;

        public TextWriter Error { get; }
        public TextWriter Out { get; }
        public TextReader In { get; set; } = new StringReader(string.Empty);
        public bool IsInputRedirected { get; set; } = false;
        public bool IsOutputRedirected { get; } = false;
        public bool IsErrorRedirected { get; } = false;
        public ConsoleColor ForegroundColor { get; set; }

        public TestConsole(MSTestContext testContext)
        {
            _testWriter = new TestOutputWriter(testContext);
            Error = _testWriter;
            Out = _testWriter;
        }

        public void Clear() { }

        public void PressKey(ConsoleKeyInfo key)
        {
            Assert.IsNotNull(KeyPressed);
            KeyPressed.Invoke(key);
        }

        public void ResetColor()
        {
        }

        public string GetOutput()
        {
            return _testWriter.GetOutput();
        }

        public void ClearOutput()
        {
            _testWriter.ClearOutput();
        }

        private class TestOutputWriter : TextWriter
        {
            private readonly MSTestContext _testContext;
            private readonly StringBuilder _sb = new();
            private readonly StringBuilder _currentOutput = new();

            public TestOutputWriter(MSTestContext testContext)
            {
                _testContext = testContext;
            }

            public override Encoding Encoding => Encoding.Unicode;

            public override void Write(char value)
            {
                if (value == '\r' || value == '\n')
                {
                    if (_sb.Length > 0)
                    {
                        _testContext.WriteLine(_sb.ToString());
                        _sb.Clear();
                    }

                    _currentOutput.Append(value);
                }
                else
                {
                    _sb.Append(value);
                    _currentOutput.Append(value);
                }
            }

            public string GetOutput()
            {
                return _currentOutput.ToString();
            }

            public void ClearOutput()
            {
                _currentOutput.Clear();
            }
        }
    }
}
