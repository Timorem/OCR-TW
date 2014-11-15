using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TIPE_OCR.Algorithms;

namespace TIPE_OCR
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            imagePanelInput.AllowDrop = true;
        }

        public Image CurrentImage
        {
            get { return imagePanelInput.Image; }
            set { imagePanelInput.Image = value; }
        }
        
        private void imagePanelInput_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.Bitmap))
                e.Effect = DragDropEffects.Copy;

        }

        private void imagePanelInput_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length != 1)
            {
                WindowHelper.ShowError("Drop only one file at the time !");
                return;
            }

            OpenPicture(files.Single());

        }

        public void OpenPicture(string file)
        {
            if (!File.Exists(file))
            {
                WindowHelper.ShowError(string.Format("Cannot open file : File {0} doesn't exist", file));
                return;
            }

            imagePanelInput.Image = Image.FromFile(file);
        }

        private void buttonGrayScaling_Click(object sender, EventArgs e)
        {
            var alg = new GrayScaling(CurrentImage);
            alg.Compute();
            CurrentImage = alg.Output;
        }
    }
}