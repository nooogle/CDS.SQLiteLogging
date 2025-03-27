namespace WinFormsTest
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            listView1 = new ListView();
            timer = new System.Windows.Forms.Timer(components);
            systemInfoPanel1 = new WinFormsTest.Utils.SystemInfoPanel();
            SuspendLayout();
            // 
            // listView1
            // 
            listView1.Dock = DockStyle.Fill;
            listView1.Location = new Point(0, 60);
            listView1.Name = "listView1";
            listView1.Size = new Size(800, 390);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // timer
            // 
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Tick += timer_Tick;
            // 
            // systemInfoPanel1
            // 
            systemInfoPanel1.Dock = DockStyle.Top;
            systemInfoPanel1.Location = new Point(0, 0);
            systemInfoPanel1.Name = "systemInfoPanel1";
            systemInfoPanel1.Size = new Size(800, 60);
            systemInfoPanel1.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(listView1);
            Controls.Add(systemInfoPanel1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private ListView listView1;
        private System.Windows.Forms.Timer timer;
        private Utils.SystemInfoPanel systemInfoPanel1;
    }
}
