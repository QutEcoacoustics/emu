// <copyright file="ErrorConsole.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Spectre.Console;

    public class ErrorConsole
    {
        private TextWriter writer;
        private ColorSystemSupport colorSystemSupport = ColorSystemSupport.Detect;

        public ErrorConsole()
        {
            this.Writer = Console.Error;
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

        public IAnsiConsole AnsiConsole { get; set; }

        private void UpdateAnsiConsole()
        {
            this.AnsiConsole = Spectre.Console.AnsiConsole.Create(new AnsiConsoleSettings()
            {
                ColorSystem = this.ColorSystemSupport,
                Out = new AnsiConsoleOutput(this.writer),
            });
        }
    }
}
