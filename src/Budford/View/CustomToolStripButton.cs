using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Budford.View
{
    public class CustomToolStripButton : ToolStripButton
    {
        [Description("The image that will be displayed when the item is disabled"), Category("Data")]
        public Image DisbledImage { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Enabled)
            {
                base.OnPaint(e);
            }
            else
            {
                if (DisbledImage == null)
                {
                    ControlPaint.DrawImageDisabled(e.Graphics, Image, Margin.Top, Margin.Bottom, BackColor);
                }
                else
                {
                    e.Graphics.DrawImage(DisbledImage, Margin.Top, Margin.Bottom);
                }
            }
        }
    }
}
