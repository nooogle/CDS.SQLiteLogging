using System.Windows.Forms;

namespace CDS.SQLiteLogging.Views;

public class DoubleBufferedListView : ListView
{
    public DoubleBufferedListView()
    {
        // Enable double-buffering to reduce flicker
        this.DoubleBuffered = true;
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        this.UpdateStyles();
    }
}
