// <copyright file="TestOutputHelperTextWriterAdapter - Copy.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.IO;
    using System.Text;
    using FluentAssertions.Equivalency.Tracing;
    using Xunit.Abstractions;

    public class TestOutputHelperITraceWriterAdapter : ITraceWriter
    {
        private readonly ITestOutputHelper output;

        private int depth = 1;

        public TestOutputHelperITraceWriterAdapter(ITestOutputHelper output)
        {
            this.output = output;
        }

        public IDisposable AddBlock(string trace)
        {
            this.output.WriteLine(trace);
            this.output.WriteLine("{");
            this.depth++;

            return new Disposable(() =>
            {
                this.depth--;
                this.output.WriteLine("}");
            });
        }

        public void AddSingle(string trace)
        {
            this.output.WriteLine(trace);
        }

        internal class Disposable : IDisposable
        {
            private readonly Action action;

            public Disposable(Action action)
            {
                this.action = action;
            }

            public void Dispose()
            {
                this.action();
            }
        }
    }
}
