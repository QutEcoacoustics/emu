// <copyright file="EmuHelpBuilder.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Cli
{
    using System.CommandLine;
    using System.CommandLine.Help;
    using System.CommandLine.IO;
    using System.CommandLine.Rendering;
    using System.Diagnostics;

    public class EmuHelpBuilder : HelpBuilder
    {
        private const string Title = @"
    ______    __  ___   __  __  p     _
   / ____/   /  |/  /  / / / /  p  -=(')
  / __/     / /|_/ /  / / / /   p    ;;
 / /___    / /  / /  / /_/ /    p   //
/_____/   /_/  /_/   \____/     p  //
                                p : '.---.__
                                p |  --_-_)__)
                                p `.____,'
                                p    \  \
                                p  ___\  \
                                p (       \
                                p          \
                                p          /";

        private static readonly AnsiControlCode EmuColor = Ansi.Color.Foreground.Rgb(176, 144, 107);

        public EmuHelpBuilder(IConsole console, int maxWidth = int.MaxValue)
            : base(console, maxWidth)
        {
        }

        public override void Write(ICommand command)
        {
            // We're seeing intermittent errors where the underlying stream is closed.
            // This is only happening for tests though (with concurrent access to System.CommandLine).
            try
            {
                base.Write(command);

                // in our own post-help notes if present
                if (command is IHelpPostScript commandWithPostScript)
                {
                    var postScript = commandWithPostScript.PostScript;
                    if (string.IsNullOrWhiteSpace(postScript))
                    {
                        return;
                    }

                    this.Console.Out.Write(postScript);
                    this.Console.Out.WriteLine();
                }
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine("Could not write postscript because the writer was disposed");
            }
        }

        protected override void AddSynopsis(ICommand command)
        {
            var padding = new string(' ', Math.Min(this.MaxWidth, 80) - 46);
            var adjusted = Title.TrimStart(Environment.NewLine.ToCharArray()).Replace("p", padding);
            var title = $@"{EmuColor}{adjusted}{Ansi.Color.Foreground.Default}{Environment.NewLine}";

            // We're seeing intermittent errors where the underlying stream is closed.
            // This is only happening for tests though (with concurrent access to System.CommandLine).
            try
            {
                this.Console.Out.Write(title);

                this.Console.Out.Write($"{Ansi.Cursor.Move.Up(6)}");
                base.AddSynopsis(command);
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine("Could not write synopsis because the writer was disposed");
            }
        }
    }
}
