// <copyright file="SpectreUtils.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Cli
{
    using System.ComponentModel;
    using System.Text;
    using System.Text.RegularExpressions;
    using Spectre.Console;

    public static class SpectreUtils
    {
        public static readonly string EmuName = "[orange4_1]EMU[/]";

        private static Regex emuRegex = new Regex(@"(emu)", RegexOptions.IgnoreCase);

        public static string MarkupBool(bool value)
        {
            return value ? "[lime]✓[/]" : "[red]✗[/]";
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
            return emuRegex.Replace(code, "[orange4_1]$1[/]");
        }

        public static string FormatList<T>(object record)
        {
            var builder = new StringBuilder();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(record))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(record)?.ToString()?.EscapeMarkup();
                builder.AppendLine($"[yellow]{name}[/] = {value}");
            }

            return builder.ToString();
        }
    }
}
