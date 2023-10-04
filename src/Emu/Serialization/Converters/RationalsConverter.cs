// <copyright file="RationalsConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Serialization.Converters
{
    using System.Numerics;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;
    using Rationals;

    public class RationalsConverter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var r = (Rational)value;

            return r.IsNaN ? null : ((decimal)r).ToString("G");
        }

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text != null)
            {
                string[] args = text.Split("/");

                if (args.Length == 1)
                {
                    return new Rational(BigInteger.Parse(args[0]));
                }
                else
                {
                    return new Rational(BigInteger.Parse(args[0]), BigInteger.Parse(args[1]));
                }
            }

            return null;
        }
    }
}
