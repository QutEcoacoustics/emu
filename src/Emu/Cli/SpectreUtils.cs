// <copyright file="SpectreUtils.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Cli
{
    using Spectre.Console;

    public static class SpectreUtils
    {
        public static readonly string EmuName = "[orange4_1]EMU[/]";

        public static string MarkupBool(bool value)
        {
            return value ? "[lime]✓[/]" : "[red]✗[/]";
        }

        public static string MarkupNumber(string value)
        {
            return $"[aqua]{value}[/]";
        }

        public static string MarkupDate(string value)
        {
            return $"[dodgerblue1]{value}[/]";
        }

        public static string MarkupType(string value)
        {
            return $"[mediumpurple2]{value}[/]";
        }

        public static string MarkupEnum(string value)
        {
            return $"[seagreen1]{value}[/]";
        }

        public static string MarkupCode(string code)
        {
            var emuHighlighted = MarkupEmu(code);

            return $"`[blue on grey7]{emuHighlighted}[/]`";
        }

        public static string MarkupCodeBlock(string code)
        {
            var emuHighlighted = MarkupEmu(code);

            return $"\n[blue on grey7]{emuHighlighted}[/]\n";
        }

        public static string MarkupEmu(string code)
        {
            return code?.Replace("emu", "[orange4_1]$1[/]", StringComparison.InvariantCultureIgnoreCase);
        }

        public static string MarkupPath(string path)
        {
            return $"[dodgerblue3]{path.EscapeMarkup()}[/]";
        }

        public static string MarkupLink(string url, string title = null)
        {
            title ??= url;
            return $"[blue][link={url.EscapeMarkup()}]{title.EscapeMarkup()}[/][/]";
        }

        public static Rule MarkupRule(string text)
        {
            return new Rule($"[green]{text}[/]").LeftJustified();
        }

        public static string MarkupFileSection(string path)
        {
            return $"File {MarkupPath(path)}:\n";
        }

        public static string MarkupInfo(string text)
        {
            return $"[teal]{text}[/]";
        }

        public static string MarkupWarning(string message)
        {
            return $"[yellow]{message}[/]";
        }

        public static string MarkupError(string message)
        {
            return $"[red]{message}[/]";
        }
    }
}
