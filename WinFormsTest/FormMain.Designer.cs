namespace WinFormsTest
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuTree = new CDS.WinFormsMenus.Basic.MenuTree();
            SuspendLayout();
            // 
            // menuTree
            // 
            menuTree.Dock = DockStyle.Fill;
            menuTree.Location = new Point(0, 0);
            menuTree.Margin = new Padding(2, 1, 2, 1);
            menuTree.Name = "menuTree";
            menuTree.Size = new Size(800, 450);
            menuTree.TabIndex = 0;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(menuTree);
            Name = "FormMain";
            Text = "FormMain";
            Load += FormMain_Load;
            ResumeLayout(false);
        }

        #endregion

        private CDS.WinFormsMenus.Basic.MenuTree menuTree;
    }
}