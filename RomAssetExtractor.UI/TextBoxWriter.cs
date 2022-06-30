using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RomAssetExtractor.Ui
{
    internal class TextBoxWriter : TextWriter
    {
        private readonly TextBox textbox;

        public TextBoxWriter(TextBox txtOutput)
        {
            this.textbox = txtOutput;
        }

        public override void Write(char value)
        {
            textbox.AppendText(value.ToString());
        }

        public override void Write(string value)
        {
            textbox.AppendText(value);
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}