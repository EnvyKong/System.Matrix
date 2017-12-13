namespace System.Matrix
{
    public interface IEntryData
    {
        string IPToMatrix { get; set; }
        string IPToVNA { get; set; }
        string IPToVertex1 { get; set; }
        string IPToVertex2 { get; set; }
        string IPToCalBoxForMatrix { get; set; }
        string IPToCalBoxForVertex { get; set; }

        int PortNumToMatrix { get; set; }
        int PortNumToVNA { get; set; }
        int PortNumToVertex { get; set; }
        int PortNumToCalBoxToMatrix { get; set; }
        int PortNumToCalBoxToVertex { get; set; }

        long Frequency { get; set; }
        double AttenuationStep { get; set; }
        double PhaseStep { get; set; }
        PhaseStepShiftDirection PhaseStepShiftDirection { get; set; }

        int CalBoxToVertexBConnectNum { get; }
        int CalBoxToVertexAConnectNum { get; }
        int CalBoxToVertexBNum { get; }
        int CalBoxToVertexANum { get; }
        int CalBoxToVertexQuantity { get; }

        int CalBoxToMatrixBConnectNum { get; }
        int CalBoxToMatrixAConnectNum { get; }
        int CalBoxToMatrixBNum { get; }
        int CalBoxToMatrixANum { get; }
        int CalBoxToMatrixQuantity { get; }

        string VNAType { get; }

        int VertexBConnectNum { get; }
        int VertexAConnectNum { get; }
        int VertexBNum { get; }
        int VertexANum { get; }
        int VertexQuantity { get; }

        int MatrixBConnectNum { get; }
        int MatrixAConnectNum { get; }
        int MatrixBNum { get; }
        int MatrixANum { get; }
        int MatrixQuantity { get; }

        int PhaMarkPoint { get; }
        int AttMarkPoint { get; }

        int AttCalFre { get; }
        int PhaCalFre { get; }

        IEntryData GetEntryData();
    }
}
