using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace NEAProjectLcokedIn
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D rectangleTexture;
        private Texture2D addButtonTexture;
        private Texture2D deleteButtonTexture;
        private Texture2D outlineTexture;
        private List<Vector2> positions;
        private List<Vector2> velocities;
        private List<float> weights;
        private float accelerationY = 1000f;
        private const float RectangleWidth = 50f;
        private const float RectangleHeight = 50f;
        private Rectangle addButtonRect;
        private Rectangle deleteButtonRect;
        private MouseState previousMouseState;
        private KeyboardState previousKeyboardState;
        private int? selectedObjectIndex;
        private const float ForceStrength = 2000f;
        private SpriteFont font;
        private Vector2 lastAppliedForce;
        private const float DragCoefficient = 0.5f;
        private string weightInput = "";
        private bool isEnteringWeight = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            TargetElapsedTime = System.TimeSpan.FromSeconds(1d / 120d);
            IsFixedTimeStep = true;
            graphics.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            base.Initialize();
            positions = new List<Vector2>();
            velocities = new List<Vector2>();
            weights = new List<float>();
            AddObject();
            lastAppliedForce = Vector2.Zero;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            rectangleTexture = new Texture2D(GraphicsDevice, 1, 1);
            rectangleTexture.SetData(new[] { Color.White });

            outlineTexture = new Texture2D(GraphicsDevice, 1, 1);
            outlineTexture.SetData(new[] { Color.Black });

            addButtonTexture = Content.Load<Texture2D>("plus_button");
            deleteButtonTexture = Content.Load<Texture2D>("delete_button");

            font = Content.Load<SpriteFont>("Arial");

            addButtonRect = new Rectangle(
                graphics.PreferredBackBufferWidth - addButtonTexture.Width - 10,
                10,
                addButtonTexture.Width,
                addButtonTexture.Height);

            deleteButtonRect = new Rectangle(
                10,
                10,
                deleteButtonTexture.Width,
                deleteButtonTexture.Height);
        }

        private void AddObject()
        {
            float centerX = graphics.PreferredBackBufferWidth / 2f - RectangleWidth / 2f;
            float centerY = graphics.PreferredBackBufferHeight / 2f - RectangleHeight / 2f;
            positions.Add(new Vector2(centerX, centerY));
            velocities.Add(new Vector2(300f, 0f));
            weights.Add(1f); // Default weight
        }

        private void PurgeObjects()
        {
            positions.Clear();
            velocities.Clear();
            weights.Clear();
            selectedObjectIndex = null;
            isEnteringWeight = false;
        }

        private bool CheckCollision(Vector2 pos1, Vector2 pos2)
        {
            Rectangle rect1 = new Rectangle((int)pos1.X, (int)pos1.Y, (int)RectangleWidth, (int)RectangleHeight);
            Rectangle rect2 = new Rectangle((int)pos2.X, (int)pos2.Y, (int)RectangleWidth, (int)RectangleHeight);
            return rect1.Intersects(rect2);
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Handle mouse clicks
            if (currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                if (addButtonRect.Contains(currentMouseState.Position))
                {
                    AddObject();
                }
                else if (deleteButtonRect.Contains(currentMouseState.Position))
                {
                    PurgeObjects();
                }
                else
                {
                    // Check if we clicked on an object
                    for (int i = 0; i < positions.Count; i++)
                    {
                        if (new Rectangle((int)positions[i].X, (int)positions[i].Y, (int)RectangleWidth, (int)RectangleHeight)
                            .Contains(currentMouseState.Position))
                        {
                            selectedObjectIndex = i;
                            isEnteringWeight = true;
                            weightInput = "";
                            break;
                        }
                    }
                }
            }
            else if (currentMouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed)
            {
                selectedObjectIndex = null; // Deselect the object
                isEnteringWeight = false;
            }

            // Handle weight input
            if (isEnteringWeight && selectedObjectIndex.HasValue)
            {
                Keys[] pressedKeys = currentKeyboardState.GetPressedKeys();
                foreach (Keys key in pressedKeys)
                {
                    if (key >= Keys.D0 && key <= Keys.D9 || key >= Keys.NumPad0 && key <= Keys.NumPad9)
                    {
                        if (previousKeyboardState.IsKeyUp(key))
                        {
                            weightInput += (key >= Keys.NumPad0) ? (key - Keys.NumPad0).ToString() : (key - Keys.D0).ToString();
                        }
                    }
                    else if (key == Keys.Back && weightInput.Length > 0 && previousKeyboardState.IsKeyUp(key))
                    {
                        weightInput = weightInput.Substring(0, weightInput.Length - 1);
                    }
                    else if (key == Keys.OemPeriod && !weightInput.Contains(".") && previousKeyboardState.IsKeyUp(key))
                    {
                        weightInput += ".";
                    }
                }

                if (currentKeyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))
                {
                    if (float.TryParse(weightInput, out float newWeight) && newWeight > 0)
                    {
                        weights[selectedObjectIndex.Value] = newWeight;
                    }
                    isEnteringWeight = false;
                    weightInput = "";
                }
            }

            // Handle WASD input for selected object
            lastAppliedForce = Vector2.Zero;
            if (selectedObjectIndex.HasValue && !isEnteringWeight)
            {
                Vector2 force = Vector2.Zero;
                if (currentKeyboardState.IsKeyDown(Keys.W) || currentKeyboardState.IsKeyDown(Keys.Up)) force.Y -= 1;
                if (currentKeyboardState.IsKeyDown(Keys.S) || currentKeyboardState.IsKeyDown(Keys.Down)) force.Y += 1;
                if (currentKeyboardState.IsKeyDown(Keys.A) || currentKeyboardState.IsKeyDown(Keys.Left)) force.X -= 1;
                if (currentKeyboardState.IsKeyDown(Keys.D) || currentKeyboardState.IsKeyDown(Keys.Right)) force.X += 1;

                if (force != Vector2.Zero)
                {
                    force.Normalize();
                    lastAppliedForce = force * ForceStrength;
                    velocities[selectedObjectIndex.Value] += lastAppliedForce * deltaTime / weights[selectedObjectIndex.Value];
                }
            }

            // Handle mouse dragging for selected object
            if (selectedObjectIndex.HasValue && currentMouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 mousePosition = new Vector2(currentMouseState.X, currentMouseState.Y);
                float dx = mousePosition.X - previousMouseState.Position.X;
                float dy = mousePosition.Y - previousMouseState.Position.Y;

                positions[selectedObjectIndex.Value] += new Vector2(dx, dy) * (float)Math.Min(1f, Math.Abs(dx / 10f)) * (float)Math.Min(1f, Math.Abs(dy / 10f));
            }

            // Update physics for all objects
            for (int i = 0; i < positions.Count; i++)
            {
                Vector2 position = positions[i];
                Vector2 velocity = velocities[i];
                float weight = weights[i];

                // Apply horizontal drag
                float dragForce = (float)(-DragCoefficient * velocity.X * (weight * 0.1));
                velocity.X += dragForce * deltaTime;

                // Apply vertical acceleration (gravity)
                velocity.Y += accelerationY * deltaTime;

                // Update position
                position += velocity * deltaTime;

                // Bounce off edges
                if (position.Y + RectangleHeight > graphics.PreferredBackBufferHeight || position.Y < 0)
                {
                    velocity.Y = -velocity.Y * 0.9f;
                    position.Y = (position.Y + RectangleHeight > graphics.PreferredBackBufferHeight) ?
                        graphics.PreferredBackBufferHeight - RectangleHeight : 0;
                }

                if (position.X + RectangleWidth > graphics.PreferredBackBufferWidth || position.X < 0)
                {
                    velocity.X = -velocity.X * 0.9f;
                    position.X = (position.X + RectangleWidth > graphics.PreferredBackBufferWidth) ?
                        graphics.PreferredBackBufferWidth - RectangleWidth : 0;
                }

                // Check for collisions with other objects
                for (int j = i + 1; j < positions.Count; j++)
                {
                    if (CheckCollision(position, positions[j]))
                    {
                        // Simple collision response
                        Vector2 collisionNormal = Vector2.Normalize(positions[j] - position);
                        float relativeVelocity = Vector2.Dot(velocity - velocities[j], collisionNormal);

                        if (relativeVelocity < 0)
                        {
                            float impulse = -(1 + 0.8f) * relativeVelocity / (1 / weight + 1 / weights[j]);
                            Vector2 impulseVector = impulse * collisionNormal;

                            velocity += impulseVector / weight;
                            velocities[j] -= impulseVector / weights[j];

                            // Separate the objects to prevent sticking
                            float overlap = RectangleWidth - Vector2.Distance(position, positions[j]);
                            Vector2 separationVector = overlap * 0.5f * collisionNormal;
                            position -= separationVector;
                            positions[j] += separationVector;
                        }
                    }
                }

                // Update object state
                positions[i] = position;
                velocities[i] = velocity;
            }

            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            for (int i = 0; i < positions.Count; i++)
            {
                Vector2 position = positions[i];
                if (i == selectedObjectIndex)
                {
                    // Draw black outline for selected object
                    spriteBatch.Draw(outlineTexture, new Rectangle((int)position.X - 5, (int)position.Y - 5, (int)RectangleWidth + 10, (int)RectangleHeight + 10), Color.Black);
                }
                spriteBatch.Draw(rectangleTexture, new Rectangle((int)position.X, (int)position.Y, (int)RectangleWidth, (int)RectangleHeight), Color.Red);
            }
            spriteBatch.Draw(addButtonTexture, addButtonRect, Color.White);
            spriteBatch.Draw(deleteButtonTexture, deleteButtonRect, Color.White);

            // Draw velocity, acceleration, and weight text for selected object
            if (selectedObjectIndex.HasValue)
            {
                Vector2 velocity = velocities[selectedObjectIndex.Value];
                Vector2 acceleration = new Vector2(lastAppliedForce.X / (RectangleWidth * weights[selectedObjectIndex.Value]), lastAppliedForce.Y / (RectangleHeight * weights[selectedObjectIndex.Value]) + accelerationY);
                string infoText = $"Velocity: {velocity.X:F2}, {velocity.Y:F2} | Acceleration: {acceleration.X:F2}, {acceleration.Y:F2}";
                Vector2 textSize = font.MeasureString(infoText);
                Vector2 textPosition = new Vector2(graphics.PreferredBackBufferWidth / 2f, 30f);
                spriteBatch.DrawString(font, infoText, textPosition, Color.Black, 0f, textSize / 2f, 1f, SpriteEffects.None, 0f);

                string weightText = isEnteringWeight ? $"Weight: {weightInput}_" : $"Weight: {weights[selectedObjectIndex.Value]:F2}";
                Vector2 weightTextSize = font.MeasureString(weightText);
                Vector2 weightTextPosition = new Vector2(graphics.PreferredBackBufferWidth / 2f, 60f);
                spriteBatch.DrawString(font, weightText, weightTextPosition, Color.Black, 0f, weightTextSize / 2f, 1f, SpriteEffects.None, 0f);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
