
namespace IntelPCSDK_Manager.Helpers
{
    public class GestureHelper
    {
        private static PXCMGesture.Gesture[] gestures = new PXCMGesture.Gesture[2];

        public static PXCMGesture.Gesture[] CaptureGestures(PXCMGesture gesture)
        {
            var status = gesture.QueryGestureData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, 0, out gestures[0]);
            gesture.QueryGestureData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_SECONDARY, 0, out gestures[1]);
            if (status >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                return gestures;

            }
            return null;
        }
    }
}
