// <copyright file="SdCardCid.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Models
{
    using LanguageExt;
    using Error = LanguageExt.Common.Error;

    /// <summary>
    /// Describes a passive acoustic monitor/sensor that
    /// was used to generate a recording.
    /// </summary>
    public record SdCardCid
    {
        public const int OEMIDOffset = 2;
        public const int OEMIDLength = 2;
        public const int ProductNameOffset = 6;
        public const int ProductNameLength = 5;
        public const int ProductRevisionOffset = 16;
        public const int SerialNumberOffset = 18;
        public const int ManufactureDateOffset = 27;
        public const string ManufacturerIDKey = "ManufacturerID";
        public const string OEMIDKey = "OEMID";
        public const string ProductNameKey = "ProductName";
        public const string ProductRevisionKey = "ProductRevision";
        public const string SerialNumberKey = "SerialNumber";
        public const string ManufactureDateKey = "ManufactureDate";

        public static readonly Func<string, Error> CIDInvalid = x => Error.New($"CID `{x}` can't be parsed");

        public SdCardCid(string cid)
        {
            this.CID = cid;
        }

        /// <summary>
        /// Gets the cid of a memory card.
        /// </summary>
        public string CID { get; init; }

        public Fin<MemoryCard> ExtractSdInfo()
        {
            try
            {
                var card = new MemoryCard() with
                {
                    ManufacturerID = this.ParseManufacturerID(),
                    OEMID = this.ParseOEMID(),
                    ProductName = this.ParseProductName(),
                    ProductRevision = this.ParseProductRevision(),
                    SerialNumber = this.ParseSerialNumber(),
                    ManufactureDate = this.ParseManufactureDate(),
                };

                return card;
            }
            catch (IndexOutOfRangeException)
            {
                return CIDInvalid(this.CID);
            }
        }

        private byte ParseManufacturerID() => byte.Parse(this.CID[..2], System.Globalization.NumberStyles.HexNumber);

        private string ParseOEMID()
        {
            string oemId = string.Empty;
            int offset = OEMIDOffset;

            // Parse OEM ID one ASCII character at a time
            for (int j = 0; j < OEMIDLength; j++)
            {
                oemId += Convert.ToChar(uint.Parse(this.CID.Substring(offset, 2), System.Globalization.NumberStyles.HexNumber));
                offset += 2;
            }

            return oemId;
        }

        private string ParseProductName()
        {
            string productName = string.Empty;
            int offset = ProductNameOffset;

            // Parse product name one ASCII character at a time
            for (int j = 0; j < ProductNameLength; j++)
            {
                productName += Convert.ToChar(uint.Parse(this.CID.Substring(offset, 2), System.Globalization.NumberStyles.HexNumber));
                offset += 2;
            }

            return productName;
        }

        private float ParseProductRevision()
        {
            int offset = ProductRevisionOffset;

            byte productRevisionWholePart = byte.Parse(this.CID.Substring(offset, 1), System.Globalization.NumberStyles.HexNumber);
            offset++;
            byte productRevisionDecimalPart = byte.Parse(this.CID.Substring(offset, 1), System.Globalization.NumberStyles.HexNumber);

            float productRevision = productRevisionWholePart + ((float)productRevisionDecimalPart / 10);

            return productRevision;
        }

        private uint ParseSerialNumber() => uint.Parse(this.CID.Substring(SerialNumberOffset, 8), System.Globalization.NumberStyles.HexNumber);

        private string ParseManufactureDate()
        {
            int offset = ManufactureDateOffset;

            string year = Convert.ToString(2000 + byte.Parse(this.CID.Substring(offset, 2), System.Globalization.NumberStyles.HexNumber));
            offset += 2;
            string month = Convert.ToString(byte.Parse(this.CID.Substring(offset, 1), System.Globalization.NumberStyles.HexNumber));

            // Ensure month is in MM format
            month = month.Length == 1 ? "0" + month : month;

            string manufactureDate = year + "-" + month;

            return manufactureDate;
        }
    }
}
