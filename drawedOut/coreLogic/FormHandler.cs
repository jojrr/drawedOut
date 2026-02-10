namespace drawedOut
{
    internal partial class FormHandler : Form
    {
        public static Form Handler;

        public FormHandler()
        {
            this.WindowState = FormWindowState.Minimized ;
            this.ShowInTaskbar = false;
            this.Visible = false;
            Handler = this;

            Global.LevelResolution = Global.Resolutions.p1080;

            InitializeComponent();
        }

        private void FormHandler_Load(object sender, EventArgs e)
        { 
            MainMenu menu = new MainMenu(); 
            menu.Show(); 
        }

        public static void CloseHandler() => Handler.Close();
    }


    partial class FormHandler
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
            SuspendLayout();
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1,1);
            Name = "FormHandler";
            Text = "FormHandler";
            Load += FormHandler_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}

