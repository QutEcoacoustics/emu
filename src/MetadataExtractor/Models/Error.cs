namespace MetadataExtractor.Models
{
    public class Error : INotice
    {
        public string Title {get; set;}
        public string Message {get; set;}
        public string Code {get; set;}
    }
}