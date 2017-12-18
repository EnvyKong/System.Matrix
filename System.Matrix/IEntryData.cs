using System.Collections.Generic;

namespace System.Matrix
{
    public interface IEntryData
    {
        List<string> IPToMatrix { get; }
        string IPToVNA { get; }
        List<string> IPToVertex { get; }
        List<string> IPToCalBoxToMatrix { get; }
        List<string> IPToCalBoxToVertex { get; }

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

        int CalBoxToMatrixBConnectNum { get; }
        int CalBoxToMatrixAConnectNum { get; }
        int CalBoxToMatrixBNum { get; }
        int CalBoxToMatrixANum { get; }

        int CalBoxWholeBConnectNum { get; }
        int CalBoxWholeAConnectNum { get; }
        int CalBoxWholeBNum { get; }
        int CalBoxWholeANum { get; }

        VNAType VNAType { get; }

        int VertexBConnectNum { get; }
        int VertexAConnectNum { get; }
        int VertexBNum { get; }
        int VertexANum { get; }

        int MatrixBConnectNum { get; }
        int MatrixAConnectNum { get; }
        int MatrixBNum { get; }
        int MatrixANum { get; }

        int PhaMarkPoint { get; }
        int AttMarkPoint { get; }

        int AttCalFre { get; }
        int PhaCalFre { get; }
    }
}
