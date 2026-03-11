namespace WinFormsTest;

public partial class FormMain : Form
{
    public FormMain()
    {
        InitializeComponent();
    }

    private void FormMain_Load(object sender, EventArgs e)
    {
        var api = menuTree.API;
        var group = api.AddGroup("Live viewing");

        group.AddItem(
            name: "Simple live view",
            tooltip: "A simple viewer showing just the core log entry fields",
            parent: this,
            createForm: () => new SimpleListLogViewer.FormDemo());

        group.AddItem(
            name: "Domain-specific live view",
            tooltip: "A viewer showing a domain-specific log entry. The extracts and displays domain-specific knowledge of scopes and structured messages",
            parent: this,
            createForm: () => new DomainSpecificLiveLogViewer.FormDemo());

        group.AddItem(
            name: "Simple offline log viewer",
            tooltip: "A simple offline log viewer demo",
            parent: this,
            createForm: () => new SimpleOfflineLogViewer.FormDemo());

        api.ExpandAllGroups();
    }
}
