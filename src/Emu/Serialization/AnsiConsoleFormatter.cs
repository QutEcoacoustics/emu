// <copyright file="AnsiConsoleFormatter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Serialization
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;
    using Spectre.Console.Rendering;

    /// <inheritdoc cref="IRecordFormatter"/>
    public class AnsiConsoleFormatter : IRecordFormatter
    {
        private readonly int? width;
        private TextWriter writer;
        private IAnsiConsole ansiConsole;
        private readonly ColorSystemSupport colorSystemSupport;

        public AnsiConsoleFormatter(int? width = null, ColorSystemSupport colorSystemSupport = ColorSystemSupport.Detect)
        {
            this.width = width;
            this.colorSystemSupport = colorSystemSupport;
        }

        public TextWriter Writer
        {
            get
            {
                if (this.writer == null)
                {
                    throw new InvalidOperationException("The writer has to be set before the console is ready for use");
                }

                return this.writer;
            }

            set
            {
                this.writer = value;
                this.UpdateAnsiConsole();
            }
        }

        //public ColorSystemSupport ColorSystemSupport
        //{
        //    get
        //    {
        //        return this.colorSystemSupport;
        //    }

        //    set
        //    {
        //        this.colorSystemSupport = value;
        //        this.UpdateAnsiConsole();
        //    }
        //}

        /// <inheritdoc />
        public IDisposable WriteHeader<T>(IDisposable context, T record)
        {
            this.RenderLine(record);

            return context ?? new DummyContext();
        }

        /// <inheritdoc />
        public virtual IDisposable WriteRecord<T>(IDisposable context, T record)
        {
            this.RenderLine(record);

            return context;
        }

        /// <inheritdoc />
        public virtual IDisposable WriteMessage<T>(IDisposable context, T message)
        {
            this.RenderLine(message);

            return context;
        }

        /// <inheritdoc />
        public IDisposable WriteFooter<T>(IDisposable context, T record)
        {
            this.RenderLine(record);

            return context;
        }

        /// <inheritdoc />
        public void Dispose(IDisposable context)
        {
            // noop
        }

        private void RenderLine<T>(T record)
        {
            switch (record)
            {
                case null: break;
                case IRenderable i:
                    {
                        this.ansiConsole.Write(i);
                        this.ansiConsole.WriteLine();
                        break;
                    }

                case string s:
                    {
                        this.ansiConsole.MarkupLine(s);
                        break;
                    }

                default:
                    {
                        this.ansiConsole.MarkupLine(record?.ToString() ?? string.Empty);
                        break;
                    }
            }
        }

        private void UpdateAnsiConsole()
        {
            this.ansiConsole = AnsiConsole.Create(new AnsiConsoleSettings()
            {
                ColorSystem = this.colorSystemSupport,
                Out = new AnsiConsoleOutput(this.writer),
            });

            if (this.width.HasValue)
            {
                this.ansiConsole.Profile.Width = this.width.Value;
            }
        }
    }
}
