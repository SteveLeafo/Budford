using Budford.Control;
using Budford.Properties;
using Budford.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Budford.Model
{
    internal class ViewShaderCache
    {
        private InstalledVersion iv1;
        readonly private Model model;
        readonly private FormMainWindow mainForm;

        internal ViewShaderCache(FormMainWindow mainFormIn, Model modelIn)
        {
            mainForm = mainFormIn;
            model = modelIn;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void updateShaderCachesToolStripMenuItem_Click()
        {
            iv1 = mainForm.GetCurrentVersion();

            mainForm.launcher.Model = model;

            foreach (var game in model.GameData)
            {
                if (!game.Value.SaveDir.StartsWith("??"))
                {
                    if (GameSettings.IsPlayable(game.Value.GameSetting.EmulationState))
                    {
                        if (game.Value.GameSetting.PreferedVersion == "Latest")
                        {
                            mainForm.launcher.CopyLargestShaderCacheToCemu(game.Value);
                        }
                    }
                }
            }

            System.Threading.ThreadPool.QueueUserWorkItem(ThreadProc2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void importShaderCacheToolStripMenuItem_Click()
        {
            // Configure open file dialog box 
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = Resources.fMainWindow_importShaderCacheToolStripMenuItem_Click_Shader_Cache_Files_____bin_;

                // Show open file dialog box 
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    FileManager.ImportShaderCache(mainForm, model, dlg.FileName);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void mergeShaderCachesToolStripMenuItem_Click()
        {
            using (FormShaderMerger merger = new FormShaderMerger())
            {
                merger.ShowDialog(mainForm);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateInfo"></param>
        void ThreadProc2(Object stateInfo)
        {
            foreach (var game in model.GameData)
            {
                if (!game.Value.SaveDir.StartsWith("??"))
                {
                    if (GameSettings.IsPlayable(game.Value.GameSetting.EmulationState))
                    {
                        if (game.Value.GameSetting.PreferedVersion == "Latest")
                        {
                            try
                            {
                                UpdateShaderCache(game);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        private void UpdateShaderCache(KeyValuePair<string, GameInformation> game)
        {
            FileInfo transferableShader = new FileInfo(Path.Combine(iv1.Folder, "shaderCache", "transferable", game.Value.SaveDir + ".bin"));
            FileInfo precompiledShader = new FileInfo(Path.Combine(iv1.Folder, "shaderCache", "precompiled", game.Value.SaveDir + ".bin"));

            if (!File.Exists(precompiledShader.FullName))
            {
                if (File.Exists(transferableShader.FullName))
                {
                    if (transferableShader.Length > 1000000)
                    {
                        model.CurrentId = game.Key;
                        mainForm.launcher.LaunchCemu(mainForm, model, game.Value, true);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mergeShaderCachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormShaderMerger merger = new FormShaderMerger())
            {
                merger.ShowDialog(mainForm);
            }
        }
    }
}
