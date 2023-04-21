// <copyright file="ConsoleRedirector.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.IO;

    public class ConsoleRedirector : IDisposable
    {
        private readonly TextWriter oldOut;
        private readonly TextWriter oldError;
        private readonly StringWriter newOut;
        private readonly StringWriter newError;

        private ConsoleRedirector()
        {
            this.oldOut = Console.Out;
            this.oldError = Console.Error;
            this.newOut = new StringWriter();
            this.newError = new StringWriter();

            Console.SetOut(this.newOut);
            Console.SetError(this.newError);
        }

        public StringWriter NewOut
        {
            get
            {
                this.newOut.Flush();
                return this.newOut;
            }
        }

        public StringWriter NewError
        {
            get
            {
                this.newError.Flush();
                return this.newError;
            }
        }

        public static ConsoleRedirector Create()
        {
            return new ConsoleRedirector();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            this.newOut?.Dispose();
            this.newError?.Dispose();

            Console.SetOut(this.oldOut);
            Console.SetError(this.oldError);
        }
    }
}
