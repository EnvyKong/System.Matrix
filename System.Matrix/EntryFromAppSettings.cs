using System.Collections.Generic;
using System.Configuration;

namespace System.Matrix
{
    public class EntryFromAppSettings
    {
        public virtual List<string> IPToMatrix => new List<string> { ConfigurationManager.AppSettings["IPToMatrix"].ToStringOrDefault() };
        public virtual string IPToVNA => ConfigurationManager.AppSettings["IPToVNA"].ToStringOrDefault();
        public virtual List<string> IPToVertex => new List<string> { ConfigurationManager.AppSettings["IPToVertex1"].ToStringOrDefault(), ConfigurationManager.AppSettings["IPToVertex2"].ToStringOrDefault() };
        public virtual List<string> IPToCalBoxToMatrix => new List<string> { ConfigurationManager.AppSettings["IPToCalBoxToMatrix"].ToStringOrDefault() };
        public virtual List<string> IPToCalBoxToVertex => new List<string> { ConfigurationManager.AppSettings["IPToCalBoxToVertex"].ToStringOrDefault() };
        public List<string> IPToCalBoxWhole => new List<string> { ConfigurationManager.AppSettings["IPToCalBoxWhole"].ToStringOrDefault() };
        public virtual int PortNumToMatrix => ConfigurationManager.AppSettings["PortNumToMatrix"].ToInt32OrDefault();
        public virtual int PortNumToVNA => ConfigurationManager.AppSettings["PortNumToVNA"].ToInt32OrDefault();
        public virtual int PortNumToVertex => ConfigurationManager.AppSettings["PortNumToVertex"].ToInt32OrDefault();
        public virtual int PortNumToCalBoxToMatrix => ConfigurationManager.AppSettings["PortNumToCalBoxToMatrix"].ToInt32OrDefault();
        public virtual int PortNumToCalBoxToVertex => ConfigurationManager.AppSettings["PortNumToCalBoxToVertex"].ToInt32OrDefault();
        public int PortNumToCalBoxWhole => ConfigurationManager.AppSettings["PortNumToCalBoxWhole"].ToInt32OrDefault();
        public virtual long Frequency => ConfigurationManager.AppSettings["Frequency"].ToLongOrDefault();
        public virtual double AttenuationStep => ConfigurationManager.AppSettings["AttenuationStep"].ToDoubleOrDefault();
        public virtual double PhaseStep => ConfigurationManager.AppSettings["PhaseStep"].ToDoubleOrDefault();
        public virtual PhaseStepShiftDirection PhaseStepShiftDirection { get; set; }
        public virtual int CalBoxToVertexBConnectNum => ConfigurationManager.AppSettings["CalBoxToVertexBConnectNum"].ToInt32OrDefault();
        public virtual int CalBoxToVertexAConnectNum => ConfigurationManager.AppSettings["CalBoxToVertexAConnectNum"].ToInt32OrDefault();
        public virtual int CalBoxToVertexBNum => ConfigurationManager.AppSettings["CalBoxToVertexBNum"].ToInt32OrDefault();
        public virtual int CalBoxToVertexANum => ConfigurationManager.AppSettings["CalBoxToVertexANum"].ToInt32OrDefault();
        public virtual int CalBoxToMatrixBConnectNum => ConfigurationManager.AppSettings["CalBoxToMatrixBConnectNum"].ToInt32OrDefault();
        public virtual int CalBoxToMatrixAConnectNum => ConfigurationManager.AppSettings["CalBoxToMatrixAConnectNum"].ToInt32OrDefault();
        public virtual int CalBoxToMatrixBNum => ConfigurationManager.AppSettings["CalBoxToMatrixBNum"].ToInt32OrDefault();
        public virtual int CalBoxToMatrixANum => ConfigurationManager.AppSettings["CalBoxToMatrixANum"].ToInt32OrDefault();
        public virtual int CalBoxWholeBConnectNum => ConfigurationManager.AppSettings["CalBoxWholeBConnectNum"].ToInt32OrDefault();
        public virtual int CalBoxWholeAConnectNum => ConfigurationManager.AppSettings["CalBoxWholeAConnectNum"].ToInt32OrDefault();
        public virtual int CalBoxWholeBNum => ConfigurationManager.AppSettings["CalBoxWholeBNum"].ToInt32OrDefault();
        public virtual int CalBoxWholeANum => ConfigurationManager.AppSettings["CalBoxWholeANum"].ToInt32OrDefault();
        public virtual VNAType VNAType { get; set; }
        public virtual int VertexBConnectNum => ConfigurationManager.AppSettings["VertexBConnectNum"].ToInt32OrDefault();
        public virtual int VertexAConnectNum => ConfigurationManager.AppSettings["VertexAConnectNum"].ToInt32OrDefault();
        public virtual int VertexBNum => ConfigurationManager.AppSettings["VertexBNum"].ToInt32OrDefault();
        public virtual int VertexANum => ConfigurationManager.AppSettings["VertexANum"].ToInt32OrDefault();
        public virtual int MatrixBConnectNum => ConfigurationManager.AppSettings["MatrixBConnectNum"].ToInt32OrDefault();
        public virtual int MatrixAConnectNum => ConfigurationManager.AppSettings["MatrixAConnectNum"].ToInt32OrDefault();
        public virtual int MatrixBNum => ConfigurationManager.AppSettings["MatrixBNum"].ToInt32OrDefault();
        public virtual int MatrixANum => ConfigurationManager.AppSettings["MatrixANum"].ToInt32OrDefault();
        public virtual int PhaMarkPoint => ConfigurationManager.AppSettings["PhaMarkPoint"].ToInt32OrDefault();
        public virtual int AttMarkPoint => ConfigurationManager.AppSettings["AttMarkPoint"].ToInt32OrDefault();
        public virtual int AttCalFre => ConfigurationManager.AppSettings["AttCalFre"].ToInt32OrDefault();
        public virtual int PhaCalFre => ConfigurationManager.AppSettings["PhaCalFre"].ToInt32OrDefault();
    }
}
