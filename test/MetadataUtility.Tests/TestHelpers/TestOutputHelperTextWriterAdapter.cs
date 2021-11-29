namespace MetadataUtility.Tests.TestHelpers
{
    using System.IO;
    using System.Text;
    using Xunit.Abstractions;

    public class TestOutputHelperTextWriterAdapter : TextWriter
    {
        private readonly ITestOutputHelper output;

        private string currentLine = string.Empty;

        public TestOutputHelperTextWriterAdapter(ITestOutputHelper output)
        {
            this.output = output;
        }

        public override Encoding Encoding { get; }

        public bool Enabled { get; set; } = true;

        public override void Write(char value)
        {
            if (!this.Enabled) { return; }
            if (value == '\n')
            {
                this.WriteCurrentLine();
            }
            else
            {
                this.currentLine += value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.currentLine != string.Empty)
            {
                this.WriteCurrentLine();
            }

            base.Dispose(disposing);
        }

        private void WriteCurrentLine()
        {
            this.output.WriteLine(this.currentLine);
            this.currentLine = string.Empty;
        }
    }
}
