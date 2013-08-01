using System.Threading;

namespace SimpleLib
{
    interface IVideoCapture
    {
        //Raw byte streams from camera
        byte[] FrameRGBA { get; }
        byte[] FrameBGRA { get; }

        //State to indicate camera is streaming
        bool Capturing { get; }

        //Current dimentions of the stream image
        int Width { get; }
        int Height { get; }

        //Test for the running of the internal thread - needed to check when closing to avoid overflow errors
        ThreadState State { get; }

        //initialise the camera and start streaming
        void Initialise();

        //internal use only - direct access to stream gathering function
        void StartCaptureStream();

        //Clean up and close down current stream - Note, thread has to finish before closing to avoid memory overflow from SDK
        void Dispose(bool disposing);

        
    }
}
