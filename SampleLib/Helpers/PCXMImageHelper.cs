using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SimpleLib.Helpers
{
    public class PCXMImageHelper
    {
        public static byte[] PXCMImageToByteArray(PXCMImage image, PXCMImage.ColorFormat format, out int width, out int height)
        {
            PXCMImage.ImageData data;
            byte[] byteArray = null;

            // set default Width and Height (320,240)
            width = 320;
            height = 240;

            if (image.AcquireAccess(PXCMImage.Access.ACCESS_READ, format, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                PXCMImage.ImageInfo imageInfo = image.imageInfo;
                width = (int)imageInfo.width;
                height = (int)imageInfo.height;
                var bufferSize = width * height * 4;
                byteArray = data.ToByteArray(0, bufferSize);
                image.ReleaseAccess(ref data);
            }
            return byteArray;
        }

        public static Bitmap PXCMImageToBitmap(PXCMImage image)
        {
            Bitmap bitmap = null;
            PXCMImage.ImageData data;
            if (image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                PXCMImage.ImageInfo imageInfo = image.imageInfo;
                bitmap = data.ToBitmap(imageInfo.width, imageInfo.height);
                image.ReleaseAccess(ref data);
            }
            return bitmap;
        }

        public static PXCMImage BitmapToPXCMImage(Bitmap bitmap)
        {
            ///* Read bitmap into the memory if we're getting it from a file */
            //Bitmap bitmap = (Bitmap)Image.FromFile(file);

            /* Get a system memory allocator */
            PXCMAccelerator accel;

            PXCMSession qsession;
            PXCMSession.CreateInstance(out qsession);
            qsession.CreateAccelerator(out accel);

            PXCMImage.ImageInfo iinfo = new PXCMImage.ImageInfo();
            iinfo.width = (uint)bitmap.Width;
            iinfo.height = (uint)bitmap.Height;
            iinfo.format = PXCMImage.ColorFormat.COLOR_FORMAT_RGB32;

            /* Create the image */
            PXCMImage image;
            accel.CreateImage(ref iinfo, out image);
            accel.Dispose();

            /* Copy the data */
            PXCMImage.ImageData idata;
            image.AcquireAccess(PXCMImage.Access.ACCESS_WRITE, out idata);

            BitmapData bdata = new BitmapData();
            bdata.Scan0 = idata.buffer.planes[0];
            bdata.Stride = idata.buffer.pitches[0];
            bdata.PixelFormat = PixelFormat.Format32bppRgb;
            bdata.Width = bitmap.Width;
            bdata.Height = bitmap.Height;
            BitmapData bdata2 = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                      ImageLockMode.ReadOnly | ImageLockMode.UserInputBuffer,
                      PixelFormat.Format32bppRgb, bdata);

            image.ReleaseAccess(ref idata);
            bitmap.UnlockBits(bdata2);

            // do something with the image
            //image.Dispose();
            return image;

        }

        public static void SavePicture(Bitmap bitmap, String filename)
        {

            // Get an ImageCodecInfo object that represents the JPEG codec.
            var picImageCodecInfo = GetEncoderInfo("image/jpeg");

            // for the Quality parameter category.
            var picEncoder = System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object. 
            // An EncoderParameters object has an array of EncoderParameter 
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            var picEncoderParameters = new EncoderParameters(1);

            // Save the bitmap as a JPEG file with quality level 85.
            var picEncoderParameter = new EncoderParameter(picEncoder, 85L);
            picEncoderParameters.Param[0] = picEncoderParameter;
            bitmap.Save(filename, picImageCodecInfo, picEncoderParameters);

            PXCMImage pimage = BitmapToPXCMImage(bitmap);
            //Switch back just as a test
            pimage = BitmapToPXCMImage(bitmap);
        }

        public static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

    }
}
