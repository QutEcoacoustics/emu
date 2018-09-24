namespace MetadataExtractor.Models
{

    using System.Collections.Generic;
    
    public class Sensor
    {

        public string Name { get; set; }

        public string Firmware { get; set; }

        public string SensorSerialNumber { get; set; }

        public string PowerSource { get; set; }

        public double Voltage { get; set; }

        public Dictionary<string, string> Configuration { get; set; }
    }
}