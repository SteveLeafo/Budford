using System.Reflection;

namespace Budford.View
{
    public static class ControlExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="enable"></param>
        public static void DoubleBuffered(this System.Windows.Forms.Control control, bool enable)
        {
            var doubleBufferPropertyInfo = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            doubleBufferPropertyInfo.SetValue(control, enable, null);
        }
    }
}
