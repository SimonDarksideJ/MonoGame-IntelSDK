#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace SimpleLib
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D DisplayImage;

        IVideoCapture capture;

        public Game1()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            ///Comment out or add each implementation to test it's use
            ///* Note each lib has different capabilities, sone only deal with video streams whilst others have gesture and recognition capabilities
            ///

            ///Ultil M capture - stream only
            //capture = new UtilMCaptureSession();

            //Util M Pipeline - stream plus capture
            capture = new UtilMPipelineSession();

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            capture.Initialise();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            DisplayImage = new Texture2D(GraphicsDevice, capture.Width, capture.Height);
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            if (capture.FrameRGBA != null && capture.FrameRGBA.Length > 0)
            {
                DisplayImage = capture.FrameBGRA.ToTexture2D(graphics.GraphicsDevice, capture.Width, capture.Height);
            }

            base.Update(gameTime);
        }
  
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            spriteBatch.Begin();
            spriteBatch.Draw(DisplayImage, Vector2.Zero, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            capture.Dispose(disposing);
            while (capture.State != System.Threading.ThreadState.Stopped) { }
            base.Dispose(disposing);
        }


    }

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
