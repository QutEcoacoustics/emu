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
        private readonly ILogger<AnsiConsoleFormatter> logger;

        private TextWriter writer;
        private IAnsiConsole ansiConsole;
        private ColorSystemSupport colorSystemSupport = ColorSystemSupport.Detect;

        public AnsiConsoleFormatter(ILogger<AnsiConsoleFormatter> logger)
        {
            this.logger = logger;
        }

        public TextWriter Writer
        {
            get
            {
                return this.writer;
            }

            set
            {
                this.writer = value;
                this.UpdateAnsiConsole();
            }
        }

        public ColorSystemSupport ColorSystemSupport
        {
            get
            {
                return this.colorSystemSupport;
            }

            set
            {
                this.colorSystemSupport = value;
                this.UpdateAnsiConsole();
            }
        }

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
                ColorSystem = this.ColorSystemSupport,
                Out = new AnsiConsoleOutput(this.writer),
            });
        }
    }
}
