using System.IO;
using System.Windows.Forms;
using TopYoung.MV.Core;

namespace System.Matrix
{
    public class FrmConfig
    {
        public FrmConfig()
        {
            txtMQ.Text = SetConfig.GetConfig("Matrix Quantity");
            txtMA.Text = SetConfig.GetConfig("Matrix APort Number");
            txtMB.Text = SetConfig.GetConfig("Matrix BPort Number");
            txtMAC.Text = SetConfig.GetConfig("Matrix APort Connect Number");
            txtMBC.Text = SetConfig.GetConfig("Matrix BPort Connect Number");

            txtVQ.Text = SetConfig.GetConfig("Vertex Quantity");
            txtVA.Text = SetConfig.GetConfig("Vertex APort Number");
            txtVB.Text = SetConfig.GetConfig("Vertex BPort Number");
            txtVAC.Text = SetConfig.GetConfig("Vertex APort Connect Number");
            txtVBC.Text = SetConfig.GetConfig("Vertex BPort Connect Number");

            txtCMQ.Text = SetConfig.GetConfig("CalBoxToMatrix Quantity");
            txtCMA.Text = SetConfig.GetConfig("CalBoxToMatrix APort Number");
            txtCMB.Text = SetConfig.GetConfig("CalBoxToMatrix BPort Number");
            txtCMAC.Text = SetConfig.GetConfig("CalBoxToMatrix APort Connect Number");
            txtCMBC.Text = SetConfig.GetConfig("CalBoxToMatrix BPort Connect Number");

            txtCVQ.Text = SetConfig.GetConfig("CalBoxToVertex Quantity");
            txtCVA.Text = SetConfig.GetConfig("CalBoxToVertex APort Number");
            txtCVB.Text = SetConfig.GetConfig("CalBoxToVertex BPort Number");
            txtCVAC.Text = SetConfig.GetConfig("CalBoxToVertex APort Connect Number");
            txtCVBC.Text = SetConfig.GetConfig("CalBoxToVertex BPort Connect Number");

            var rdoVNA = Controls.Find("rdo" + SetConfig.GetConfig("VNA Type"), true);
            var rdoAtt = Controls.Find("rdoAtt" + SetConfig.GetConfig("Attenuation Calibration Frequency"), true);
            var rdoPha = Controls.Find("rdoPha" + SetConfig.GetConfig("Phase Calibration Frequency"), true);

            if (rdoVNA.Length != 0)
            {
                (rdoVNA[0] as RadioButton).Checked = true;
            }
            if (rdoAtt.Length != 0)
            {
                (rdoAtt[0] as RadioButton).Checked = true;
            }

            if (rdoPha.Length != 0)
            {
                (rdoPha[0] as RadioButton).Checked = true;
            }

            lblSaveTime.Text = "Last save time: " + new FileInfo(Application.ExecutablePath + ".config").LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");
        }

        private void btnCfgModify_Click(object sender, EventArgs e)
        {
            ChangeTxtEnable(false);
        }

        private void btnCfgSave_Click(object sender, EventArgs e)
        {
            ChangeTxtEnable(true);

            SetConfig.UpdateConfig("Matrix Quantity", txtMQ.Text);
            SetConfig.UpdateConfig("Matrix APort Number", txtMA.Text);
            SetConfig.UpdateConfig("Matrix BPort Number", txtMB.Text);
            SetConfig.UpdateConfig("Matrix APort Connect Number", txtMAC.Text);
            SetConfig.UpdateConfig("Matrix BPort Connect Number", txtMBC.Text);

            SetConfig.UpdateConfig("Vertex Quantity", txtVQ.Text);
            SetConfig.UpdateConfig("Vertex APort Number", txtVA.Text);
            SetConfig.UpdateConfig("Vertex BPort Number", txtVB.Text);
            SetConfig.UpdateConfig("Vertex APort Connect Number", txtVAC.Text);
            SetConfig.UpdateConfig("Vertex BPort Connect Number", txtVBC.Text);

            SetConfig.UpdateConfig("CalBoxToMatrix Quantity", txtCMQ.Text);
            SetConfig.UpdateConfig("CalBoxToMatrix APort Number", txtCMA.Text);
            SetConfig.UpdateConfig("CalBoxToMatrix BPort Number", txtCMB.Text);
            SetConfig.UpdateConfig("CalBoxToMatrix APort Connect Number", txtCMAC.Text);
            SetConfig.UpdateConfig("CalBoxToMatrix BPort Connect Number", txtCMBC.Text);

            SetConfig.UpdateConfig("CalBoxToVertex Quantity", txtCVQ.Text);
            SetConfig.UpdateConfig("CalBoxToVertex APort Number", txtCVA.Text);
            SetConfig.UpdateConfig("CalBoxToVertex BPort Number", txtCVB.Text);
            SetConfig.UpdateConfig("CalBoxToVertex APort Connect Number", txtCVAC.Text);
            SetConfig.UpdateConfig("CalBoxToVertex BPort Connect Number", txtCVBC.Text);

            lblSaveTime.Text = "Last save time: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        private void ChangeTxtEnable(bool enAble)
        {
            foreach (var ctr in Controls)
            {
                if (!(ctr is GroupBox))
                {
                    continue;
                }
                foreach (var txt in (ctr as GroupBox).Controls)
                {
                    if (!(txt is TextBox))
                    {
                        continue;
                    }
                    (txt as TextBox).ReadOnly = enAble;
                }
            }
        }

        private void rdoVNA_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked == true)
            {
                SetConfig.UpdateConfig("VNA Type", (sender as RadioButton).Tag.ToString());
            }
        }

        private void rdoAtt_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked == true)
            {
                SetConfig.UpdateConfig("Attenuation Calibration Frequency", (sender as RadioButton).Text);
            }
        }

        private void rdoPha_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked == true)
            {
                SetConfig.UpdateConfig("Phase Calibration Frequency", (sender as RadioButton).Text);
            }
        }
    }
}
