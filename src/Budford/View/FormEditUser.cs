using Budford.Model;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Budford.View
{
    public partial class FormEditUser : Form
    {
        // The currently active user
        readonly User user;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userIn"></param>
        public FormEditUser(User userIn)
        {
            InitializeComponent();

            user = userIn;

            textBox1.Text = user.Name;
            textBox2.Text = user.Image;

            NewMethod();
        }

        /// <summary>
        /// 
        /// </summary>
        private void NewMethod()
        {
            if (File.Exists("Users\\" + user.Image))
            {
                using (FileStream stream = new FileStream("Users\\" + user.Image, FileMode.Open, FileAccess.Read))
                {
                    pictureBox1.Image = Image.FromStream(stream);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            user.Name = textBox1.Text;

            if (user.Image != textBox2.Text)
            {
                user.Image = textBox2.Text;
                pictureBox1.Image.Save("Users\\" + user.Image, ImageFormat.Png);
            }
            
            Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            // Configure open file dialog box 
            OpenFileDialog dlg = new OpenFileDialog {Filter = GetSupportedCodecs()};

            // Show open file dialog box 
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using (FileStream stream = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read))
                {
                    pictureBox1.Image = Image.FromStream(stream);
                }
                // ReSharper disable once LocalizableElement
                textBox2.Text = Path.GetFileNameWithoutExtension(dlg.FileName) + ".png";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetSupportedCodecs()
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            StringBuilder codecFilter = new StringBuilder("Image Files|");

            foreach (var codec in codecs)
            {
                codecFilter.Append(codec.FilenameExtension + ";");
            }

            return codecFilter.ToString();
        }
    }
}
