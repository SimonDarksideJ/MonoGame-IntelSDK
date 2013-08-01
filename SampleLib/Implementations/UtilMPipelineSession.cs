using System.Threading;

namespace SimpleLib
{
    class UtilMPipelineSession : IVideoCapture
    {
        PXCMSession session;
        UtilMPipeline pp;
        PXCMGesture gesture;

        private PXCMGesture.GeoNode[][] nodes = new PXCMGesture.GeoNode[2][] { new PXCMGesture.GeoNode[11], new PXCMGesture.GeoNode[11] };
        private PXCMGesture.Gesture[] gestures = new PXCMGesture.Gesture[2];


        protected int width = 320;
        protected int height = 240;
        protected int DEVICE_ID = 0;
        protected int BufferSize;
        Thread thread;
        private bool capturing;
        protected byte[] frameRGBA;

        public byte[] FrameBGRA
        {
            get { return frameRGBA.ConvertBetweenBGRAandRGBA(width, height); }
        }

        public byte[] FrameRGBA
        {
            get { return frameRGBA; }
        }

        public bool Capturing
        {
            get { return capturing; }
        }

        public ThreadState State
        {
            get { return thread.ThreadState; }
        }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public void Initialise()
        {
            PXCMSession.CreateInstance(out session);

            /* request a color stream */

            var ColourSize = new PXCMSizeU32() { width = 320, height = 240 };
            var DepthSize = new PXCMSizeU32() { width = 320, height = 240 };

            pp = new UtilMPipeline();
            pp.EnableGesture();
            pp.EnableVoiceRecognition();
            pp.EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH, (uint)Width, (uint)Height); // select the stream


            if (pp.Init())
            {
                thread = new Thread(new ThreadStart(StartCaptureStream));
                thread.Start();
            }

        }

        public void StartCaptureStream()
        {
            PXCMImage image = null;
            capturing = true;
            while (capturing)
            {
                if (!pp.AcquireFrame(true)) break;
                else
                {
                    gesture = pp.QueryGesture();
                    image = pp.QueryImage(PXCMImage.ImageType.IMAGE_TYPE_DEPTH); // retrieve the sample

                    PXCMImage.ImageData data;
                    if (image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                    {
                        PXCMImage.ImageInfo imageInfo = image.imageInfo;
                        width = (int)imageInfo.width;
                        height = (int)imageInfo.height;
                        BufferSize = width * height * 4;
                        frameRGBA = new byte[BufferSize];
                        frameRGBA = data.ToByteArray(0, BufferSize);
                        image.ReleaseAccess(ref data);
                    }
                    PXCMGesture.GeoNode[] hand_data = new PXCMGesture.GeoNode[5];

                    gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY | PXCMGesture.GeoNode.Label.LABEL_FINGER_THUMB, hand_data);

                    pp.ReleaseFrame(); // go fetching the next sample

                    //Testing
                    var gstr = pp.QueryGesture();
                    //Geonodes(gstr);
                    //Gestures(gstr);
                }
            }
            image.Dispose();
        }

        public void Dispose(bool disposing)
        {
            capturing = false;
        }

        private void Geonodes(PXCMGesture gesture)
        {
            var status = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, nodes[0]);
            gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_SECONDARY, nodes[1]);
            gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_ELBOW_PRIMARY, out nodes[0][nodes.Length - 1]);
            gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_ELBOW_SECONDARY, out nodes[1][nodes.Length - 1]);
            if (status >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                //Do something with Gestures

            }
        }

        private void Gesture(PXCMGesture gesture)
        {
            var status = gesture.QueryGestureData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, 0, out gestures[0]);
            gesture.QueryGestureData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_SECONDARY, 0, out gestures[1]);
            if (status >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                //Do something with Gestures

            }
        }
    }
}
