namespace System.Matrix
{
    partial class FrmProgressBar
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmProgressBar));
            this.prgTask = new System.Windows.Forms.ProgressBar();
            this.bgw = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // prgTask
            // 
            this.prgTask.BackColor = System.Drawing.Color.Salmon;
            this.prgTask.ForeColor = System.Drawing.Color.Salmon;
            this.prgTask.Location = new System.Drawing.Point(17, 30);
            this.prgTask.Margin = new System.Windows.Forms.Padding(20, 0, 3, 3);
            this.prgTask.MarqueeAnimationSpeed = 1;
            this.prgTask.Maximum = 1000;
            this.prgTask.Name = "prgTask";
            this.prgTask.Size = new System.Drawing.Size(450, 20);
            this.prgTask.Step = 1;
            this.prgTask.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgTask.TabIndex = 0;
            // 
            // bgw
            // 
            this.bgw.WorkerReportsProgress = true;
            // 
            // FrmProgressBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(484, 81);
            this.ControlBox = false;
            this.Controls.Add(this.prgTask);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmProgressBar";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }

        #endregion
        private System.ComponentModel.BackgroundWorker bgw;
        protected System.Windows.Forms.ProgressBar prgTask;
    }
}