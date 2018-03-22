using Budford.Model;
using Budford.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Budford.View
{
    public partial class FormEditGameSettings : Form
    {
        // The active game
        readonly GameInformation information;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="informationIn"></param>
        /// <param name="installedVersionsIn"></param>
        internal FormEditGameSettings(GameInformation informationIn, List<InstalledVersion>installedVersionsIn)
        {
            InitializeComponent();
            information = informationIn;

            comboBox1.Items.Add("Latest");
            if (installedVersionsIn != null)
            {
                comboBox1.Items.AddRange(installedVersionsIn.OrderByDescending(version => version.Name).Select(a => a.Name).ToArray());
            }

            trackBar1.SetRange(0, 100);

            PopulateGameSettings();
            PopulateGraphicsPack();
            PopulateGameInformation();

            Text = information.Name;
            if (information.Name != "The Legend of Zelda Breath of the Wild")
            {
                label52.Visible = false;
                comboBox32.Visible = false;
                checkBox1.Visible = false;
                numericUpDown1.Visible = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void PopulateGameInformation()
        {
            textBox1.Text = information.Name;
            textBox2.Text = information.ProductCode;
            textBox3.Text = information.SaveDir;
            textBox4.Text = information.TitleId;
            textBox5.Text = information.LaunchFile;
            textBox6.Text = information.GroupId;
            textBox7.Text = information.Region;
            textBox8.Text = information.Publisher;
            textBox9.Text = information.Size;
            textBox10.Text = information.GameSetting.PreferedVersion;
            textBox11.Text = information.GameSetting.OfficialEmulationState.ToString();
            textBox12.Text = information.CompanyCode;
            textBox13.Text = information.LaunchFileName;
            textBox14.Text = information.Comments;

            linkLabel1.Text = information.GameSetting.CompatibilityUrl;

            string iconFile;
            string logoFile;

            if (information.Image)
            {
                string folder = Path.GetDirectoryName(information.RpxFile);
                folder = Path.GetDirectoryName(folder);
                iconFile = Path.Combine(folder, "meta" , "iconTex.tga");
                logoFile = Path.Combine(folder, "meta", "bootLogoTex.tga");
            }
            else
            {
                iconFile = information.LaunchFile.Replace(information.LaunchFileName, "").Replace("code" + Path.DirectorySeparatorChar, "meta" + Path.DirectorySeparatorChar + "iconTex.tga");
                logoFile = information.LaunchFile.Replace(information.LaunchFileName, "").Replace("code" + Path.DirectorySeparatorChar, "meta" + Path.DirectorySeparatorChar + "bootLogoTex.tga");
            }
            if (File.Exists(iconFile))
            {
                try
                {
                    pictureBox1.Image = TgaReader.Load(iconFile);
                }
                catch (Exception)
                {
                    // No code
                }
            }
            if (File.Exists(logoFile))
            {
                try
                {
                    pictureBox2.Image = TgaReader.Load(logoFile);
                }
                catch (Exception)
                {
                    // No code
                }
            }

            trackBar1.Value = information.GameSetting.Volume;
        }

        /// <summary>
        /// 
        /// </summary>
        private void PopulateGraphicsPack()
        {
            if (information.TitleId != "")
            {
                listView1.Items.Clear();
                information.GameSetting.graphicsPacks = new HashSet<GraphicsPack>(information.GameSetting.graphicsPacks.OrderBy(p => p.Folder));
                foreach (var pack in information.GameSetting.graphicsPacks)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.SubItems.Add(pack.Title);
                    listView1.Items.Add(lvi);
                    lvi.Checked = pack.Active;
                    lvi.Tag = pack;
                }
            }
            listView1.Sort();
        }

        /// <summary>
        /// 
        /// </summary>
        private void PopulateGameSettings()
        {
            SetPreferedVersion(information.GameSetting.PreferedVersion);
            comboBox2.SelectedIndex = (int)information.GameSetting.OfficialEmulationState;
            comboBox19.SelectedIndex = (int)information.GameSetting.EmulationState;
            comboBox20.SelectedIndex = (information.Rating - 5) * -1;

            comboBox3.SelectedIndex = information.GameSetting.FullScreen;
            comboBox4.SelectedIndex = information.GameSetting.EnableVSync;
            comboBox5.SelectedIndex = (int)information.GameSetting.UpscaleFilter;
            comboBox6.SelectedIndex = (int)information.GameSetting.FullScreenScaling;
            comboBox7.SelectedIndex = (int)information.GameSetting.GpuBufferCacheAccuracy;

            comboBox8.SelectedIndex = (int)information.GameSetting.CpuMode;
            comboBox9.SelectedIndex = (int)information.GameSetting.CpuTimer;
            comboBox10.SelectedIndex = 0;

            comboBox11.SelectedIndex = information.GameSetting.RenderUpsideDown;
            comboBox12.SelectedIndex = information.GameSetting.DisableAudio;
            comboBox13.SelectedIndex = information.GameSetting.EnableBoTwCrashWorkaround;
            comboBox14.SelectedIndex = information.GameSetting.FullSyncAtGx2DrawDone;

            comboBox15.SelectedIndex = information.GameSetting.AccaccurateShaderMul;
            comboBox16.SelectedIndex = information.GameSetting.DisableGpuFence;
            comboBox17.SelectedIndex = information.GameSetting.SeparateGamePadView;
            comboBox18.SelectedIndex = information.GameSetting.EmulateSinglePrecision;
            comboBox29.SelectedIndex = information.GameSetting.UseRtdsc;
            comboBox30.SelectedIndex = information.GameSetting.Online;
            comboBox31.SelectedIndex = information.GameSetting.DefaultView;

            comboBox32.SelectedIndex = information.GameSetting.ClarityPreset;
            comboBox33.SelectedIndex = information.GameSetting.UseCafeLibs;

            comboBox21.SelectedIndex = information.CemuHookSetting.CustomTimerMode;
            comboBox22.SelectedIndex = information.CemuHookSetting.CustomTimerMultiplier;
            comboBox23.SelectedIndex = information.CemuHookSetting.DisableAvx ? 1 : 0;
            comboBox24.SelectedIndex = information.CemuHookSetting.DisableLzcnt ? 1 : 0;
            comboBox25.SelectedIndex = information.CemuHookSetting.DisableMovbe ? 1 : 0;
            comboBox26.SelectedIndex = information.CemuHookSetting.MotionSource;
            comboBox27.SelectedIndex = information.CemuHookSetting.MmTimerAccuracy;
            comboBox28.SelectedIndex = information.CemuHookSetting.IgnorePrecompiledShaderCache ? 1 : 0;

            comboBox34.SelectedIndex = information.GameSetting.ControllerOverride1;
            comboBox35.SelectedIndex = information.GameSetting.ControllerOverride2;
            comboBox36.SelectedIndex = information.GameSetting.ControllerOverride3;
            comboBox37.SelectedIndex = information.GameSetting.ControllerOverride4;
            comboBox38.SelectedIndex = information.GameSetting.ControllerOverride5;
            comboBox39.SelectedIndex = information.GameSetting.ControllerOverride6;
            comboBox40.SelectedIndex = information.GameSetting.ControllerOverride7;
            comboBox41.SelectedIndex = information.GameSetting.ControllerOverride8;

            comboBox42.SelectedIndex = information.GameSetting.SwapButtons1;
            comboBox43.SelectedIndex = information.GameSetting.SwapButtons2;
            comboBox44.SelectedIndex = information.GameSetting.SwapButtons3;
            comboBox45.SelectedIndex = information.GameSetting.SwapButtons4;
            comboBox46.SelectedIndex = information.GameSetting.SwapButtons5;
            comboBox47.SelectedIndex = information.GameSetting.SwapButtons6;
            comboBox48.SelectedIndex = information.GameSetting.SwapButtons7;
            comboBox49.SelectedIndex = information.GameSetting.SwapButtons8;

            numericUpDown1.Value = information.GameSetting.Fps;
            checkBox1.Checked = information.GameSetting.OverrideFps;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="preferedVersion"></param>
        void SetPreferedVersion(string preferedVersion)
        {
            for (int i = 0; i < comboBox1.Items.Count; ++i)
            {
                if (comboBox1.Items[i].ToString() == preferedVersion)
                {
                    comboBox1.SelectedIndex = i;
                    return;
                }
            }
            comboBox1.SelectedIndex = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            information.GameSetting.PreferedVersion = comboBox1.Text;
            information.GameSetting.OfficialEmulationState = (GameSettings.EmulationStateType)comboBox2.SelectedIndex;
            information.GameSetting.EmulationState = (GameSettings.EmulationStateType)comboBox19.SelectedIndex;
            information.Rating = (comboBox20.SelectedIndex - 5) * -1;

            information.GameSetting.FullScreen = (byte)comboBox3.SelectedIndex;
            information.GameSetting.EnableVSync = (byte)comboBox4.SelectedIndex ;
            information.GameSetting.UpscaleFilter = (GameSettings.UpscaleFilterType)comboBox5.SelectedIndex;
            information.GameSetting.FullScreenScaling = (GameSettings.FullScreenScalingType)comboBox6.SelectedIndex;
            information.GameSetting.GpuBufferCacheAccuracy = (GameSettings.GpuBufferCacheAccuracyType)comboBox7.SelectedIndex;

            information.GameSetting.CpuMode = (GameSettings.CpuModeType)comboBox8.SelectedIndex;
            information.GameSetting.CpuTimer = (GameSettings.CpuTimerType)comboBox9.SelectedIndex;

            comboBox10.SelectedIndex = 0;

            information.GameSetting.RenderUpsideDown = (byte)comboBox11.SelectedIndex;
            information.GameSetting.DisableAudio = (byte)comboBox12.SelectedIndex;
            information.GameSetting.EnableBoTwCrashWorkaround = (byte)comboBox13.SelectedIndex;
            information.GameSetting.FullSyncAtGx2DrawDone = (byte)comboBox14.SelectedIndex;

            information.GameSetting.AccaccurateShaderMul = (byte)comboBox15.SelectedIndex;
            information.GameSetting.DisableGpuFence = (byte)comboBox16.SelectedIndex;
            information.GameSetting.EmulateSinglePrecision = (byte)comboBox18.SelectedIndex;
            information.GameSetting.SeparateGamePadView = (byte)comboBox17.SelectedIndex;
            information.GameSetting.UseRtdsc = (byte)comboBox29.SelectedIndex;
            information.GameSetting.Online = (byte)comboBox30.SelectedIndex;
            information.GameSetting.DefaultView = (byte)comboBox31.SelectedIndex;

            information.GameSetting.ClarityPreset = comboBox32.SelectedIndex;
            information.GameSetting.UseCafeLibs = comboBox33.SelectedIndex;

            information.GameSetting.ControllerOverride1 = comboBox34.SelectedIndex;
            information.GameSetting.ControllerOverride2 = comboBox35.SelectedIndex;
            information.GameSetting.ControllerOverride3 = comboBox36.SelectedIndex;
            information.GameSetting.ControllerOverride4 = comboBox37.SelectedIndex;
            information.GameSetting.ControllerOverride5 = comboBox38.SelectedIndex;
            information.GameSetting.ControllerOverride6 = comboBox39.SelectedIndex;
            information.GameSetting.ControllerOverride7 = comboBox40.SelectedIndex;
            information.GameSetting.ControllerOverride8 = comboBox41.SelectedIndex;

            information.GameSetting.SwapButtons1 = comboBox42.SelectedIndex;
            information.GameSetting.SwapButtons2 = comboBox43.SelectedIndex;
            information.GameSetting.SwapButtons3 = comboBox44.SelectedIndex;
            information.GameSetting.SwapButtons4 = comboBox45.SelectedIndex;
            information.GameSetting.SwapButtons5 = comboBox46.SelectedIndex;
            information.GameSetting.SwapButtons6 = comboBox47.SelectedIndex;
            information.GameSetting.SwapButtons7 = comboBox48.SelectedIndex;
            information.GameSetting.SwapButtons8 = comboBox49.SelectedIndex;

            information.CemuHookSetting.CustomTimerMode = comboBox21.SelectedIndex;
            information.CemuHookSetting.CustomTimerMultiplier = comboBox22.SelectedIndex;
            information.CemuHookSetting.DisableAvx = comboBox23.SelectedIndex != 0;
            information.CemuHookSetting.DisableLzcnt = comboBox24.SelectedIndex != 0;
            information.CemuHookSetting.DisableMovbe = comboBox25.SelectedIndex != 0;
            information.CemuHookSetting.MotionSource = comboBox26.SelectedIndex;
            information.CemuHookSetting.MmTimerAccuracy = comboBox27.SelectedIndex;
            information.CemuHookSetting.IgnorePrecompiledShaderCache = comboBox28.SelectedIndex != 0;

            information.GameSetting.Fps = (int)numericUpDown1.Value;
            information.GameSetting.OverrideFps = checkBox1.Checked;

            foreach (ListViewItem lvi in listView1.Items)
            {
                ((GraphicsPack)lvi.Tag).Active = lvi.Checked;
            }

            information.Comments = textBox14.Text;

            information.GameSetting.Volume = (byte)trackBar1.Value;

            base.OnFormClosed(e);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            FileStream stream = File.Create("C:\\Development\\icontest.ico");
            IconHelper helper = new IconHelper();
            helper.AddImage((Bitmap)pictureBox1.Image, pictureBox1.Image.Width);
            helper.ConvertToIcon(stream);
            stream.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            label52.Enabled = false;
            checkBox1.Enabled = false;

            comboBox32.Enabled = false;
            numericUpDown1.Enabled = false;

            foreach (ListViewItem v in listView1.Items)
            {
                if (((GraphicsPack)v.Tag).Folder.Contains("BreathOfTheWild_StaticFPS"))
                {
                    if (v.Checked)
                    {                       
                        checkBox1.Enabled = true;
                        if (checkBox1.Checked)
                        {
                            numericUpDown1.Enabled = true;
                        }
                    }
                }
                if (((GraphicsPack)v.Tag).Folder.Contains("Clarity"))
                {
                    if (v.Checked)
                    {
                        comboBox32.Enabled = true;
                        label52.Enabled = true;
                    }
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown1.Enabled = checkBox1.Checked;
        }
    }
}
