using Microsoft.Xna.Framework.Graphics;

namespace SimpleLib
{
    public static class IntelExtensionsForXNA
    {
        public static Texture2D ToTexture2D(this byte[] input, GraphicsDevice graphics, int width, int height)
        {
            Texture2D texture = null;
            texture = new Texture2D(graphics, width, height);
            texture.SetData(input);
            return texture;

        }

        public static byte[] ConvertBetweenBGRAandRGBA(this byte[] input, int pixel_width, int pixel_height)
        {
            int offset = 0;
            var output = new byte[input.Length];

            for (int y = 0; y < pixel_height; y++)
            {
                for (int x = 0; x < pixel_width; x++)
                {
                    output[offset] = input[offset + 2];
                    output[offset + 1] = input[offset + 1];
                    output[offset + 2] = input[offset];
                    output[offset + 3] = input[offset + 3];

                    offset += 4;
                }
            }
            return output;
        }
    }
}
