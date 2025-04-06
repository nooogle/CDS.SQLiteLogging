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

        group.AddDemo(
            name: "Domain-specific live view",
            tooltip: "A viewer showing a domain-specific log entry. The extracts and displays domain-specific knowledge of scopes and structured messages",
            parent: this,
            action: () =>
            {
                using var form = new DomainSpecificLiveLogViewer.FormDemo();
                form.ShowDialog(this);
            });

        group.AddDemo(
            name: "Simple offline log viewer",
            tooltip: "A simple offline log viewer demo",
            parent: this,
            action: () =>
            {
                using var form = new SimpleOfflineLogViewer.FormDemo();
                form.ShowDialog(this);
            });

        menuTree.ExpandAllGroups();
    }
}
