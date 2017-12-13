using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using TopYoung.MV.Core;

namespace System.Matrix
{
    public partial class FrmProgressBar : Form
    {
        public FrmProgressBar()
        {
            InitializeComponent();
        }
        public FrmProgressBar(string notice, ref BackgroundWorker bgwWorker)
        {
            InitializeComponent();

            Text = notice;
            bgw = bgwWorker;

            bgw.ProgressChanged += Bgw_ProgressChanged;
            bgw.RunWorkerCompleted += Bgw_RunWorkerCompleted;
            bgw.RunWorkerAsync();
        }

        private void Bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prgTask.Value = e.ProgressPercentage;
        }

        private void Bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
            Thread resultThread = new Thread(() =>
            {
                //FrmOutput.GetFrmOutput().OutputLog = "Completed!";
                SingletonFactory<FrmOutput>.CreateInstance().OutputLog = "Completed!";
                MessageBox.Show("Completed!", "Notify", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
            resultThread.Start();
        }
    }
}
