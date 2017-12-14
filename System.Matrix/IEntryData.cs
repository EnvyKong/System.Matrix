namespace System.Matrix
{
    public interface IEntryData : ICalibratable
    {
        string IPToMatrix { get; }
        string IPToVNA { get; }
        string IPToVertex1 { get; }
        string IPToVertex2 { get; }
        string IPToCalBoxForMatrix { get; }
        string IPToCalBoxForVertex { get; }

        int PortNumToMatrix { get; }
        int PortNumToVNA { get; }
        int PortNumToVertex { get; }
        int PortNumToCalBoxToMatrix { get; }
        int PortNumToCalBoxToVertex { get; }

        long Frequency { get; }
        double AttenuationStep { get; }
        double PhaseStep { get; }
        PhaseStepShiftDirection PhaseStepShiftDirection { get; }

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
