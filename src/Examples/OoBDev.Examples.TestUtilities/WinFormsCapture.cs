using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace OoBDev.Examples.TestUtilities;

/// <summary>Renders a Windows Forms form to a PNG; the form is shown far off-screen first, because a form that was never shown paints no child controls.</summary>
public static class WinFormsCapture
{
    /// <summary>Captures <paramref name="form"/> as <paramref name="imageName"/> in the controls guide; true when the committed image was replaced.</summary>
    public static bool Save(Form form, string imageName)
    {
        ArgumentNullException.ThrowIfNull(form);
        if (!form.Visible)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-10000, -10000);
            form.ShowInTaskbar = false;
            form.Show();
        }

        Application.DoEvents();
        return ExampleGuide.Write(imageName, path =>
        {
            using var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
            bitmap.Save(path, ImageFormat.Png);
        });
    }
}
