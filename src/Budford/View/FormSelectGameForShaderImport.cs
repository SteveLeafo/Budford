using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Budford.View
{
    public partial class FormSelectGameForShaderImport : Form
    {
        Model.Model model;
        public string Id = "";

        public FormSelectGameForShaderImport(Model.Model modelIn)
        {
            InitializeComponent();
            model = modelIn;

            List<string> games = new List<string>();
            foreach (var game in model.GameData)
            {
                games.Add(game.Value.Name);
            }
            games.Sort();
            comboBox1.Items.AddRange(games.ToArray());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            foreach (var game in model.GameData)
            {
                if (game.Value.Name == comboBox1.Text)
                {
                    Id = game.Value.SaveDir;
                }
            }
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
