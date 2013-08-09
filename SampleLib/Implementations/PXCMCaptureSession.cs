/// Header
/// PXC M Sensor retrieval and processing
/// 
/// ** Note WIP - only enumerates video devices at the moment
/// Needs updating to allow audio and other streams

using System.Collections.Generic;
using System.Threading;

namespace SimpleLib
{
    class PXCMCaptureSession : IVideoCapture
    {
        CaptureType captureType;

        PXCMSession session;
        List<CaptureDevices> capturePoints = new List<CaptureDevices>();

        PXCMGesture gesture;
        PXCMGesture.GeoNode[][] nodes;
        PXCMGesture.Gesture[] gestures;

        #region Interface properties
        protected int width = 320;
        protected int height = 240;
        protected int DEVICE_ID = 0;
        protected int BufferSize;
        Thread thread;
        private bool capturing;
        protected byte[] depthFrame;
        protected byte[] colourFrame;

        public byte[] DepthFrame
        {
            get { return depthFrame; }
        }

        public byte[] ColourFrame
        {
            get { return colourFrame; }
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

        public PXCMGesture.GeoNode[][] Nodes
        {
            get { return nodes; }
        }

        public PXCMGesture.Gesture[] Gestures
        {
            get { return gestures; }
        }
        #endregion

        public void Initialise(CaptureType captureType = CaptureType.IMAGE_TYPE_DEPTH)
        {
            this.captureType = captureType;

            var sts = PXCMSession.CreateInstance(out session);

            /* request a color stream */

            var ColourSize = new PXCMSizeU32() { width = 320, height = 240 };
            var DepthSize = new PXCMSizeU32() { width = 320, height = 240 };

            /* enumerate modules */

            for (uint m = 0; sts >= pxcmStatus.PXCM_STATUS_NO_ERROR; m++)
            {
                var captureDevice = new CaptureDevices();

                sts = session.QueryImpl(m, out captureDevice.desc);

                if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                sts = session.CreateImpl<PXCMCapture>(ref captureDevice.desc, PXCMCapture.CUID, out captureDevice.capture);

                if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) continue;



                /* enumerate devices */

                captureDevice.devices = GetDevices(captureDevice.capture);
                captureDevice.streams = new List<VideoStreamInfo>[captureDevice.devices.Count];
                ///* enumerate streams */
                captureDevice.streams = GetDeviceStreams(captureDevice.devices);

                //    /* Configure the color stream */

                //    for (uint p = 0; sts >= pxcmStatus.PXCM_STATUS_NO_ERROR; p++)
                //    {

                //        PXCMCapture.VideoStream.ProfileInfo pinfo;

                //        sts = stream.QueryProfile(p, out pinfo);

                //        if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                //        if (pinfo.imageInfo.width != 640 || pinfo.imageInfo.height != 480) continue;



                //        sts = stream.SetProfile(ref pinfo);

                //        if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) continue;



                //        /* Stream data */

                //        //while (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                //        //{

                //        //    PXCMImage image;

                //        //    PXCMScheduler.SyncPoint sp;

                //        //    sts = stream.ReadStreamAsync(out image, out sp);

                //        //    if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                //        //    sts = sp.Synchronize();

                //        //    //... // do something with image

                //        //    sp.Dispose();

                //        //    image.Dispose();

                //        //}

                //    }  

                capturePoints.Add(captureDevice);
            }


            if (capturePoints.Count > 0)
            {
                thread = new Thread(new ThreadStart(StartCaptureStream));
                thread.Start();
            }
        }

        private List<PXCMCapture.Device> GetDevices(PXCMCapture capture)
        {
            var devices = new List<PXCMCapture.Device>();
            pxcmStatus sts = pxcmStatus.PXCM_STATUS_NO_ERROR;
            for (uint d = 0; sts >= pxcmStatus.PXCM_STATUS_NO_ERROR; d++)
            {

                PXCMCapture.Device device;

                sts = capture.CreateDevice(d, out device);

                if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                devices.Add(device);
            }
            return devices;
        }

        private List<VideoStreamInfo>[] GetDeviceStreams(List<PXCMCapture.Device> devices)
        {
            var streams = new List<VideoStreamInfo>[devices.Count];
            for (int d = 0; d < devices.Count; d++)
            {
                streams[d] = new List<VideoStreamInfo>();

                pxcmStatus sts = pxcmStatus.PXCM_STATUS_NO_ERROR;

                VideoStreamInfo stream = new VideoStreamInfo();

                for (uint s = 0; sts >= pxcmStatus.PXCM_STATUS_NO_ERROR; s++)
                {
                    sts = devices[d].QueryStream(s, out stream.StreamInfo);

                    if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                    if (stream.StreamInfo.cuid != PXCMCapture.VideoStream.CUID) break;

                    //if (sinfo.imageType != PXCMImage.ImageType.IMAGE_TYPE_COLOR) continue;

                    sts = devices[d].CreateStream<PXCMCapture.VideoStream>

                                       (s, PXCMCapture.VideoStream.CUID, out stream.Stream);

                    if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                    GetStreamProfiles(ref stream);

                    streams[d].Add(stream);
                }
            }
            return streams;
        }

        private void GetStreamProfiles(ref VideoStreamInfo streamInfo)
        {
            /* Configure the color stream */
            pxcmStatus sts = pxcmStatus.PXCM_STATUS_NO_ERROR;
            streamInfo.StreamProfiles = new List<PXCMCapture.VideoStream.ProfileInfo>();

            for (uint p = 0; sts >= pxcmStatus.PXCM_STATUS_NO_ERROR; p++)
            {

                PXCMCapture.VideoStream.ProfileInfo pinfo;

                sts = streamInfo.Stream.QueryProfile(p, out pinfo);

                if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                //if (pinfo.imageInfo.width != 640 || pinfo.imageInfo.height != 480) continue;

                sts = streamInfo.Stream.SetProfile(ref pinfo);

                if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                streamInfo.StreamProfiles.Add(pinfo);
            }
        }



        public void StartCaptureStream()
        {
            //PXCMImage image = null;
            //capturing = true;
            //while (capturing)
            //{
            //    if (!pp.AcquireFrame(true)) break;
            //    else
            //    {
            //        gesture = pp.QueryGesture();
            //        image = pp.QueryImage(PXCMImage.ImageType.IMAGE_TYPE_DEPTH); // retrieve the sample

            //        PXCMImage.ImageData data;
            //        if (image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            //        {
            //            PXCMImage.ImageInfo imageInfo = image.imageInfo;
            //            width = (int)imageInfo.width;
            //            height = (int)imageInfo.height;
            //            BufferSize = width * height * 4;
            //            frameRGBA = new byte[BufferSize];
            //            frameRGBA = data.ToByteArray(0, BufferSize);
            //            image.ReleaseAccess(ref data);
            //        }
            //        Helpers.GeoNodesHelper.CaptureGeonodes(gesture);
            //        Helpers.GestureHelper.CaptureGestures(gesture);

            //        pp.ReleaseFrame(); // go fetching the next sample

            //        //Testing
            //        var gstr = pp.QueryGesture();
            //        //Geonodes(gstr);
            //        //Gestures(gstr);
            //    }
            //}
            //image.Dispose();
        }

        public void Dispose(bool disposing)
        {
            capturing = false;
        }


    }

    public struct CaptureDevices
    {
        public PXCMSession.ImplDesc desc;
        public PXCMCapture capture;
        public List<PXCMCapture.Device> devices;
        public List<VideoStreamInfo>[] streams;
    }

    public struct VideoStreamInfo
    {
        public PXCMCapture.Device.StreamInfo StreamInfo;
        public PXCMCapture.VideoStream Stream;
        public List<PXCMCapture.VideoStream.ProfileInfo> StreamProfiles;
    }
}
