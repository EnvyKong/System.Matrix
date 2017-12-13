using Ivi.Visa.Interop;

namespace System.Matrix
{
    public abstract class VNA : Device
    {
        protected FormattedIO488 messageBased;

        public VNA(string ip, int portNum) : base(ip, portNum)
        {

        }

        public bool ReadIDN(out string IDN)
        {
            try
            {
                IDN = QueryString("*IDN?");
                return true;
            }
            catch
            {
                IDN = "";
                return false;
            }
        }

        /// <summary>
        /// 读取指定Trace 数据上的最小点（先选择对应 Trace，再读取）
        /// </summary>
        double ReadTraceMin(string[] arrValue)
        {
            double revValue = double.NaN;
            double revValueTmp = 0;
            bool isFZ = false;

            for (int i = 0; i < arrValue.Length; i++)
            {
                if (double.TryParse(arrValue[i], out revValueTmp))
                {
                    if (!isFZ)
                    {
                        revValue = revValueTmp;
                        isFZ = true;
                    }
                    else if (revValue > revValueTmp)
                    {
                        revValue = revValueTmp;
                    }
                }
            }
            //messageBased.Write("INIT1:CONT OFF");
            return revValue;
        }

        /// <summary>
        /// 读取指定Trace 数据上的最小点（先选择对应 Trace，再读取）
        /// </summary>
        public double ReadTraceMax(string[] arrValue)
        {
            double revValue = double.NaN;
            double revValueTmp = 0;
            bool isFZ = false;
            for (int i = 0; i < arrValue.Length; i++)
            {
                if (double.TryParse(arrValue[i], out revValueTmp))
                {
                    if (!isFZ)
                    {
                        revValue = revValueTmp;
                        isFZ = true;
                    }
                    else if (revValue < revValueTmp)
                    {
                        revValue = revValueTmp;
                    }
                }
            }
            //messageBased.Write("INIT1:CONT OFF");
            return revValue;
        }

        /// <summary>
        /// 查询命令
        /// </summary>
        public double[] QueryBinary(string cmd)
        {
            double[] messageReCmd = null;
            messageBased.WriteString(cmd);
            messageReCmd = messageBased.ReadIEEEBlock(IEEEBinaryType.BinaryType_R8, false, true) as double[];
            if (messageReCmd == null)//容错机制
            {
                messageBased.WriteString(cmd);
                messageReCmd = messageBased.ReadIEEEBlock(IEEEBinaryType.BinaryType_R8, false, true) as double[];
                return messageReCmd;
            }
            else
            {
                return messageReCmd;
            }
        }


        /// <summary>
        /// 查询命令
        /// </summary>
        public string QueryString(string cmd)
        {
            string messageReCmd = "";
            messageBased.WriteString(cmd);
            messageReCmd = messageBased.ReadString();
            if (messageReCmd == "")//容错机制
            {
                messageBased.WriteString(cmd);
                messageReCmd = messageBased.ReadString();
                return messageReCmd;
            }
            else
            {
                return messageReCmd;
            }
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="cmd"></param>
        public void Write(string cmd)
        {
            messageBased.WriteString(cmd);
        }

        /// <summary>
        /// 断开链接
        /// </summary>
        public override void Close()
        {
            try
            {
                if (messageBased != null && Connected)
                {
                    messageBased.IO.Close();
                    Connected = false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
