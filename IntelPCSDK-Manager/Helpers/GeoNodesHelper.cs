
namespace IntelPCSDK_Manager.Helpers
{
    public class GeoNodesHelper
    {
        private static PXCMGesture.GeoNode[][] nodes = new PXCMGesture.GeoNode[2][] { new PXCMGesture.GeoNode[11], new PXCMGesture.GeoNode[11] };

        public static PXCMGesture.GeoNode[][] CaptureGeonodes(PXCMGesture gesture)
        {

            var status = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, nodes[0]);
            gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_SECONDARY, nodes[1]);
            gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_ELBOW_PRIMARY, out nodes[0][nodes.Length - 1]);
            gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_ELBOW_SECONDARY, out nodes[1][nodes.Length - 1]);
            if (status >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                return nodes;

            }
            return null;
        }
    }
}
