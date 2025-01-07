using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NEAProjectLcokedIn
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D rectangleTexture;

        //x&y positions of an object
        private float posX;
        private float posY;
        //x&y velocities of an object
        private float velX;
        private float velY;
        private float accelerationY = 20f; //self explanatory
        private const float RectangleWidth = 50f;
        private const float RectangleHeight = 50f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            base.Initialize();

            posX = graphics.PreferredBackBufferWidth / 2 - RectangleWidth / 2;
            posY = graphics.PreferredBackBufferHeight / 2 - RectangleHeight / 2;
            velX = 0f;
            velY = 0f;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            rectangleTexture = new Texture2D(GraphicsDevice, 1, 1);
            rectangleTexture.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            velY += accelerationY * deltaTime;
            posX += velX * deltaTime;
            posY += velY * deltaTime;

            if (posY + RectangleHeight >= graphics.PreferredBackBufferHeight || posY <= 0)
            {
                //gravity effect to makes the object bounce
                velY *= -1;
                if (posY + RectangleHeight > graphics.PreferredBackBufferHeight)
                    posY = graphics.PreferredBackBufferHeight - RectangleHeight;
                if (posY < 0)
                    posY = 0;
            }

            if (posX + RectangleWidth >= graphics.PreferredBackBufferWidth || posX <= 0)
            {
                //makes objects bounce of the sides of the window probably
                velX *= -1;
                if (posX + RectangleWidth > graphics.PreferredBackBufferWidth)
                    posX = graphics.PreferredBackBufferWidth - RectangleWidth;
                if (posX < 0)
                    posX = 0;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(rectangleTexture, new Rectangle((int)posX, (int)posY, (int)RectangleWidth, (int)RectangleHeight), Color.Red);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }

}
