// <copyright file="EmuEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Cli
{
    using System.CommandLine;
    using System.CommandLine.Help;
    using System.CommandLine.Rendering;

    public class EmuHelpBuilder : HelpBuilder
    {
        private readonly ITerminal renderer;

        public EmuHelpBuilder(IConsole console, int maxWidth = int.MaxValue)
            : base(console, maxWidth)
        {
            var terminal = Terminal.GetTerminal(console, true, OutputMode.Ansi);

            this.renderer = terminal;
        }

        protected override void AddSynopsis(ICommand command)
        {
            base.AddSynopsis(command);
            var r = Ansi.Cursor.Move.Right(50);
            this.Console.Out.Write($@"{Ansi.Cursor.Move.Up(4)}{Ansi.Color.Foreground.Rgb(176, 144, 107)}
{r}    _
{r} -=(')
{r}   ;;
{r}  //
{r} //
{r}: '.---.__
{r}|  --_-_)__)
{r}`.____,'
{r}   \  \
{r} ___\  \
{r}(       \
{r}         \ 
{r}         /
{Ansi.Color.Foreground.White}");
        }
    }
}
