using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Data.Common;
using System.IO;

namespace connect4
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Random rnd = new Random();
        bool doAI = false;
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
        SpriteFont _fontLarge;
        byte win = 0;
        long frames = 0;
        long checkWinnerFrame = 0;
        int lastPlayedCol = 0;

        int turns = 0;
        bool debounceLeft = false;

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
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _chipTextures[0] = Content.Load<Texture2D>("ChipRed");
            _chipTextures[1] = Content.Load<Texture2D>("ChipYellow");
            _boardTexture = Content.Load<Texture2D>("board");
            _fontLarge = Content.Load<SpriteFont>("fontLarge");
        }

        public byte checkWinner(byte[][] board, int lastCol)
        {
            if (turns > 41)
                return (byte)3;
            int[] lastPos = { lastCol, 0 };
            for (int i = board[lastCol].Length - 1; i > -1; i--)
            {
                if (board[lastCol][i] != 0)
                {
                    lastPos[1] = i;
                    break;
                }
            }
            byte check = board[lastPos[0]][lastPos[1]];
            for (int dirX = -1; dirX < 2; dirX += 1)
            {
                for (int dirY = -1; dirY < 2; dirY += 1)
                {
                    if (dirX == 0 && dirY == 0)
                        break;
                    int counter = 0;
                    for (int i = -4; i < 4; i++)
                    {
                        byte[] pos = { (byte)(dirX * i + lastPos[0]), (byte)(dirY * i + lastPos[1]) };
                        if (pos[0] < 0 || pos[0] > board.Length - 1 || pos[1] < 0 || pos[1] > board[0].Length - 1)
                            continue;
                        if (board[pos[0]][pos[1]] != check)
                            counter = 0;
                        else
                            counter++;
                    }
                    if (counter > 3)
                        return check;
                }
            }
            return (byte)0;
        }
        public int bestPos(byte[][] board, byte plr)
        {
            for (int i = 0; i < board.Length; i++)
            {
                int h = -1;
                for (int j = 0; j < board[i].Length; j++)
                {
                    if (board[i][j] == 0)
                    {
                        h = j;
                        break;
                    }
                }
                if (h == -1)
                {
                    break;
                }
                byte[][] boardPtr = board;
                boardPtr[i][h] = 1;
                if (checkWinner(boardPtr, i) != 0)
                {
                    boardPtr[i][h] = 0;
                    return i;
                }
                boardPtr[i][h] = 2;
                if (checkWinner(boardPtr, i) != 0)
                {
                    boardPtr[i][h] = 0;
                    return i;
                }
                boardPtr[i][h] = 0;
            }
            return -1;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            base.Update(gameTime);

            if (mouse.LeftButton == ButtonState.Pressed && !debounceLeft && frames > checkWinnerFrame && win == 0)
            {
                debounceLeft = true;
                int relativeX = mouse.X - BOARD_BEGIN_X;
                if (relativeX > 0 && relativeX < BOARD_SIZE_X && mouse.Y > 0 && mouse.Y < Window.ClientBounds.Height)
                {
                    int column = (relativeX - CHIP_OFFSET_X) / (CHIP_SIZE_X + CHIP_OFFSET_X);
                    if (board[column][5] == 0)
                    {
                        board[column][5] = (byte)(turns % 2 + 1);
                        turns++;
                        lastPlayedCol = column;
                        checkWinnerFrame = frames + 30;
                    }
                    /* old code for putting counter at bottom instead of at top and letting animate down
                    for (int i = 0; i < board[column].Length; i++)
                    {
                        if (board[column][i] == 0)
                        {
                            board[column][i] = (byte)(turns % 2 + 1);
                            turns++;
                            break;
                        }
                    }
                    */
                }
                else if (mouse.X > CHIP_OFFSET_X && mouse.X < CHIP_OFFSET_X + CHIP_SIZE_X
                    &&   mouse.Y > BOARD_SIZE_Y - CHIP_SIZE_Y && mouse.Y < BOARD_SIZE_Y)
                    doAI = !doAI;
            }
            if (mouse.LeftButton == ButtonState.Released && debounceLeft)
                debounceLeft = false;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.AntiqueWhite);

            while (doAI && turns % 2 == 1 && frames > checkWinnerFrame && win == 0)
            {
                int bestColumn = bestPos(board, 2);
                int bestColumn2 = bestPos(board, 1);
                int column = rnd.Next(0,7);
                if (bestColumn2 != -1 && board[bestColumn2][5] == 0)
                    column = bestColumn2;
                if (bestColumn != -1 && board[bestColumn][5] == 0)
                    column = bestColumn;
                if (board[column][5] == 0)
                {
                    board[column][5] = (byte)(turns % 2 + 1);
                    turns++;
                    lastPlayedCol = column;
                    checkWinnerFrame = frames + 30;
                }
            }

            // TODO: Add your drawing code here
            if (frames % 4 == 0)
                for (int i = 0; i < board.Length; i++)
                {
                    for (int j = 1; j < board[i].Length; j++)
                    {
                        if (board[i][j - 1] == 0)
                        {
                            board[i][j - 1] = board[i][j];
                            board[i][j] = 0;
                        }
                    }
                }
            if (frames == checkWinnerFrame)
            {
                win = checkWinner(board, lastPlayedCol);
            }

            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.Begin();
            spriteBatch.DrawString(_fontLarge,"AI", new Vector2(CHIP_OFFSET_X,BOARD_SIZE_Y-CHIP_SIZE_Y), doAI ? Color.White : Color.Black);

            for (int i = 0; i < board.Length; i++)
            {
                for (int j = 0; j < board[i].Length; j++)
                {
                    if (board[i][j] != 0)
                    {
                        spriteBatch.Draw(
                            _chipTextures[board[i][j] - 1],
                            new Rectangle(
                                CHIP_BEGIN_X + i * (CHIP_SIZE_X + CHIP_OFFSET_X),
                                CHIP_BEGIN_Y + (board[i].Length - j - 1) * (CHIP_SIZE_Y + CHIP_OFFSET_Y),
                                CHIP_SIZE_X,
                                CHIP_SIZE_Y
                            ), Color.White);
                    }
                }
            }
            spriteBatch.Draw(_boardTexture, new Rectangle(BOARD_BEGIN_X, BOARD_BEGIN_Y, BOARD_SIZE_X, BOARD_SIZE_Y), Color.White);
            if (win != 0)
            {
                string winnerReadable = "Nobody";
                switch (win)
                {
                    case 1:
                        winnerReadable = "Red"; break;
                    case 2:
                        winnerReadable = "Yellow"; break;
                }
                spriteBatch.DrawString(_fontLarge, winnerReadable + " wins!", new Vector2(CHIP_OFFSET_X, 0), Color.Black);
            }

            spriteBatch.End();
            base.Draw(gameTime);

            frames++;
        }
    }
}
