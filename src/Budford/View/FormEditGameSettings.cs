﻿using Budford.Model;
using Budford.Utilities;
using System.Collections.Generic;
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

            string iconFile = information.LaunchFile.Replace(information.LaunchFileName, "").Replace("code\\", "meta\\iconTex.tga");
            string logoFile = information.LaunchFile.Replace(information.LaunchFileName, "").Replace("code\\", "meta\\bootLogoTex.tga");
            if (File.Exists(iconFile))
            {
                pictureBox1.Image = TgaReader.Load(iconFile);
            }
            if (File.Exists(logoFile))
            {
                pictureBox2.Image = TgaReader.Load(logoFile);
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
            comboBox13.SelectedIndex = information.GameSetting.EnableBoTWCrashWorkaround;
            comboBox14.SelectedIndex = information.GameSetting.FullSyncAtGX2DrawDone;

            comboBox15.SelectedIndex = information.GameSetting.AccaccurateShaderMul;
            comboBox16.SelectedIndex = information.GameSetting.DisableGPUFence;
            comboBox17.SelectedIndex = information.GameSetting.SeparateGamePadView;
            comboBox18.SelectedIndex = information.GameSetting.EmulateSinglePrecision;
            comboBox29.SelectedIndex = information.GameSetting.UseRtdsc;
            comboBox30.SelectedIndex = information.GameSetting.Online;

            comboBox21.SelectedIndex = information.CemuHookSetting.CustomTimerMode;
            comboBox22.SelectedIndex = information.CemuHookSetting.CustomTimerMultiplier;
            comboBox23.SelectedIndex = information.CemuHookSetting.DisableAVX ? 1 : 0;
            comboBox24.SelectedIndex = information.CemuHookSetting.DisableLZCNT ? 1 : 0;
            comboBox25.SelectedIndex = information.CemuHookSetting.DisableMOVBE ? 1 : 0;
            comboBox26.SelectedIndex = information.CemuHookSetting.MotionSource;
            comboBox27.SelectedIndex = information.CemuHookSetting.MMTimerAccuracy;
            comboBox28.SelectedIndex = information.CemuHookSetting.IgnorePrecompiledShaderCache ? 1 : 0;

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
            information.GameSetting.EnableBoTWCrashWorkaround = (byte)comboBox13.SelectedIndex;
            information.GameSetting.FullSyncAtGX2DrawDone = (byte)comboBox14.SelectedIndex;

            information.GameSetting.AccaccurateShaderMul = (byte)comboBox15.SelectedIndex;
            information.GameSetting.DisableGPUFence = (byte)comboBox16.SelectedIndex;
            information.GameSetting.EmulateSinglePrecision = (byte)comboBox18.SelectedIndex;
            information.GameSetting.SeparateGamePadView = (byte)comboBox17.SelectedIndex;
            information.GameSetting.UseRtdsc = (byte)comboBox29.SelectedIndex;
            information.GameSetting.Online = (byte)comboBox30.SelectedIndex;

            information.CemuHookSetting.CustomTimerMode = comboBox21.SelectedIndex;
            information.CemuHookSetting.CustomTimerMultiplier = comboBox22.SelectedIndex;
            information.CemuHookSetting.DisableAVX = comboBox23.SelectedIndex != 0;
            information.CemuHookSetting.DisableLZCNT = comboBox24.SelectedIndex != 0;
            information.CemuHookSetting.DisableMOVBE = comboBox25.SelectedIndex != 0;
            information.CemuHookSetting.MotionSource = comboBox26.SelectedIndex;
            information.CemuHookSetting.MMTimerAccuracy = comboBox27.SelectedIndex;
            information.CemuHookSetting.IgnorePrecompiledShaderCache = comboBox28.SelectedIndex != 0;

            foreach (ListViewItem lvi in listView1.Items)
            {
                ((GraphicsPack)lvi.Tag).Active = lvi.Checked;
            }

            information.Comments = textBox14.Text;

            information.GameSetting.Volume = (byte)trackBar1.Value;

            base.OnFormClosed(e);
        }
    }
}
