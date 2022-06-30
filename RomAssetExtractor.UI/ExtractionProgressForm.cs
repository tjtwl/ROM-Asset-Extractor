using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomAssetExtractor.Ui
{
    public partial class ExtractionProgressForm : Form
    {
        public ExtractionProgressForm()
        {
            InitializeComponent();
        }

        private void ExtractionProgressForm_Load(object sender, EventArgs e)
        {

        }

        private void btnRomPath_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "GBA File (*.gba)|*.gba",
                Title = "Choose a Pokémon ROM-file to extract the assets from"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            txtRomPath.Text = dialog.FileName;
        }

        private void btnOutputPath_Click(object sender, EventArgs e)
        {

            var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            txtOutputPath.Text = dialog.SelectedPath;
        }

        private async void btnExtract_Click(object sender, EventArgs e)
        {
            var romPath = txtRomPath.Text;
            var outputDirectory = txtOutputPath.Text;

            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                MessageBox.Show("No output path specified!", "Invalid path!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Directory.CreateDirectory(outputDirectory);

            if (!Directory.Exists(outputDirectory))
            {
                MessageBox.Show("Invalid output path specified!", "Invalid path!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(romPath))
            {
                MessageBox.Show("No ROM path specified!", "Invalid path!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(romPath))
            {
                MessageBox.Show("Invalid ROM path specified!", "Invalid path!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var buttons = new Button[] { btnExtract, btnOutputPath, btnRomPath };

            foreach (var button in buttons)
                button.Enabled = false;

            var logWriter = new TextBoxWriter(txtOutput);
            await AssetExtractor.ExtractRom(
                romPath, 
                outputDirectory, 
                chkBitmaps.Checked,
                chkTrainers.Checked,
                chkMaps.Checked,
                chkMapRenders.Checked,
                logWriter: logWriter);
        }

        private void chkMaps_CheckedChanged(object sender, EventArgs e)
        {
            chkMapRenders.Enabled = chkMapRenders.Checked = chkMaps.Checked;
        }
    }
}
