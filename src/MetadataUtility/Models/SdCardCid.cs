// <copyright file="SdCardCid.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Models
{
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

        public SdCardCid(string cid)
        {
            this.CID = cid;
        }

        /// <summary>
        /// Gets the cid of a memory card.
        /// </summary>
        public string CID { get; init; }

        public Dictionary<string, object> ExtractSdInfo()
        {
            Dictionary<string, object> sdInfo = new Dictionary<string, object>();

            sdInfo[ManufacturerIDKey] = this.ParseManufacturerID();
            sdInfo[OEMIDKey] = this.ParseOEMID();
            sdInfo[ProductNameKey] = this.ParseProductName();
            sdInfo[ProductRevisionKey] = this.ParseProductRevision();
            sdInfo[SerialNumberKey] = this.ParseSerialNumber();
            sdInfo[ManufactureDateKey] = this.ParseManufactureDate();

            return sdInfo;
        }

        private byte ParseManufacturerID() => byte.Parse(this.CID.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);

        private string ParseOEMID()
        {
            string oemId = string.Empty;
            int offset = OEMIDOffset;

            // Parse OEM ID one ASCII character at a time
            for (int j = 0; j < OEMIDLength; j++)
            {
                oemId += System.Convert.ToChar(uint.Parse(this.CID.Substring(offset, 2), System.Globalization.NumberStyles.HexNumber));
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
                productName += System.Convert.ToChar(uint.Parse(this.CID.Substring(offset, 2), System.Globalization.NumberStyles.HexNumber));
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

            string year = System.Convert.ToString(2000 + byte.Parse(this.CID.Substring(offset, 2), System.Globalization.NumberStyles.HexNumber));
            offset += 2;
            string month = System.Convert.ToString(byte.Parse(this.CID.Substring(offset, 1), System.Globalization.NumberStyles.HexNumber));

            // Ensure month is in MM format
            month = month.Length == 1 ? "0" + month : month;

            string manufactureDate = year + "/" + month;

            return manufactureDate;
        }
    }
}
