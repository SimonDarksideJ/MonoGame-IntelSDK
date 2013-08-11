
namespace SimpleLib.Input
{
    public interface IController
    {
        void Initialise(object input);
        void Recognise(object input, bool debug = false);
        void DebugDraw(Microsoft.Xna.Framework.Graphics.SpriteBatch spritebatch);
        void EnableDebug(Microsoft.Xna.Framework.Graphics.GraphicsDevice device);
        void SetScale(int scale);

    }
}
