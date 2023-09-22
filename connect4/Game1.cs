using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace connect4
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public byte[][] board = new byte[7][];
        const int SCREEN_WIDTH = 800;
        const int SCREEN_HEIGHT = 480;
        const int BOARD_SIZE_X = 640;
        const int BOARD_SIZE_Y = 480;
        const int BOARD_BEGIN_X = (SCREEN_WIDTH - BOARD_SIZE_X) / 2;
        const int BOARD_BEGIN_Y = 0;
        const int CHIP_BEGIN_X = BOARD_BEGIN_X + 10;
        const int CHIP_BEGIN_Y = BOARD_BEGIN_Y;
        const int CHIP_OFFSET_X = 10;
        const int CHIP_OFFSET_Y = 0;
        const int CHIP_SIZE_X = 80;
        const int CHIP_SIZE_Y = 80;
        Vector2 BOARD_BEGIN = new Vector2(BOARD_BEGIN_X, BOARD_BEGIN_Y);
        Texture2D[] _chipTextures = new Texture2D[2];
        Texture2D _boardTexture;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            for (int i = 0; i < board.Length; i++)
            {
                board[i] = new byte[6];
                board[i][0] = 1;
                board[i][1] = 2;
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _chipTextures[0] = Content.Load<Texture2D>("ChipRed");
            _chipTextures[1] = Content.Load<Texture2D>("ChipYellow");
            _boardTexture = Content.Load<Texture2D>("board");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.AntiqueWhite);

            // TODO: Add your drawing code here
            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.Begin();

            for (int i = 0; i < board.Length; i++)
            {
                for (int j = 0; j < board[i].Length; j++)
                {
                    if (board[i][j] != 0)
                    {
                        spriteBatch.Draw(_chipTextures[board[i][j] - 1], new Rectangle(CHIP_BEGIN_X + i * (CHIP_SIZE_X + CHIP_OFFSET_X), CHIP_BEGIN_Y + j * (CHIP_SIZE_Y + CHIP_OFFSET_Y), CHIP_SIZE_X, CHIP_SIZE_Y), Color.White);
                    }
                }
            }
            spriteBatch.Draw(_boardTexture, new Rectangle(BOARD_BEGIN_X, BOARD_BEGIN_Y, BOARD_SIZE_X, BOARD_SIZE_Y), Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}