using Microsoft.Xna.Framework;

namespace SimpleLib.Helpers
{
    public static class Vector3PXCMPoint3DF32Extensions
    {
        public static Vector3 ToVector3(this PXCMPoint3DF32 input)
        {
            return new Vector3(input.x, input.y, input.z);
        }

        public static PXCMPoint3DF32 ToPXCMPoint3DF32(this Vector3 input)
        {
            return new PXCMPoint3DF32() { x = input.X, y = input.Y, z = input.Z };
        }
    }
}
