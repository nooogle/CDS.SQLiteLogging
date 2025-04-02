namespace WinFormsTest;

public partial class FormMain : Form
{
    public FormMain()
    {
        InitializeComponent();
    }

    private void FormMain_Load(object sender, EventArgs e)
    {
        var group = menuTree.AddGroup("Live viewing");

        group.AddDemo(
            name: "Simple live view",
            tooltip: "A simple viewer showing just the core log entry fields",
            parent: this,
            action: () =>
            {
                using var form = new SimpleListLogViewer.FormDemo();
                form.ShowDialog(this);
            });

        menuTree.ExpandAllGroups();
    }
}
