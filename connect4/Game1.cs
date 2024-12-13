using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace connect4
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        const int AI_DEPTH = 5;

        Random rnd = new Random();
        int aiLevel = -1;
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

        public byte checkWinner(byte[][] board)
        {
            if (turns > 41)
                return (byte)3;
            
            for (int x = 0; x < board.Length; x++)
            {
                int counter = 0;
                byte chip = board[x][0];

                for (int y = 1; y < board[x].Length; y++)
                {
                    if (chip == board[x][y])
                    {
                        counter++;
                    }
                    else
                    {
                        counter = 0;
                        chip = board[x][y];
                    }

                    if (counter >= 3 && chip != 0)
                    {
                        return chip;
                    }
                }
            }

            for (int y = 0; y < board[0].Length; y++)
            {
                int counter = 0;
                byte chip = board[0][y];

                for (int x = 1; x < board.Length; x++)
                {
                    if (chip == board[x][y])
                    {
                        counter++;
                    }
                    else
                    {
                        counter = 0;
                        chip = board[x][y];
                    }

                    if (counter >= 3 && chip != 0)
                    {
                        return chip;
                    }
                }
            }

            for (int x = 0; x < board.Length; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    int counter1 = 0;
                    int counter2 = 0;
                    byte chip1 = board[x][y];
                    byte chip2 = board[x][y];

                    for (int diag = 1; diag < 4; diag++)
                    {
                        if (x + diag < board.Length && chip1 == board[x + diag][y + diag])
                        {
                            counter1++;
                        }
                        else
                        {
                            counter1 = 0;
                            chip1 = board[x][y + diag];
                        }
                        if (x - diag >= 0 && chip2 == board[x - diag][y + diag])
                        {
                            counter2++;
                        }
                        else
                        {
                            counter2 = 0;
                            chip2 = board[x][y + diag];
                        }

                        if (counter1 >= 3 && chip1 != 0)
                        {
                            return chip1;
                        }
                        if (counter2 >= 3 && chip2 != 0)
                        {
                            return chip2;
                        }
                    }
                }
            }

            return (byte)0;
        }

        public int recurPositions(byte[][] board, byte plr, int eval, int depth, int maxdepth)
        {
            byte winner = checkWinner(board);
            if (winner == plr)
            {
                return eval + 100000000 / (depth + 1);
            }
            if (winner != 0)
            {
                return eval + -100000000 / (depth + 1);
            }
            if (depth > maxdepth)
            {
                return eval;
            }

            byte currentPlr = (byte)((plr - 1 + depth) % 2 + 1);

            int bestEval = currentPlr == plr ? -2147483648 : 2147483647;

            for (int x = 0; x < board.Length; x++)
            {
                byte[][] newBoard = new byte[board.Length][];
                for (int i = 0; i < newBoard.Length; i++)
                    newBoard[i] = (byte[])board[i].Clone();

                int y = -1;
                for (int row = 0; row < board[x].Length; row++)
                {
                    if (board[x][row] == 0)
                    {
                        y = row;
                        break;
                    }
                }

                if (y == -1)
                    continue;

                newBoard[x][y] = currentPlr;
                int currentEval = recurPositions(newBoard, plr, eval, depth + 1, maxdepth);

                if (currentPlr == plr)
                {
                    if (currentEval > bestEval)
                    {
                        bestEval = currentEval;
                    }
                }
                else
                {
                    if (currentEval < bestEval)
                    {
                        bestEval = currentEval;
                    }
                }
            }

            return bestEval + eval;
        }

        public int bestPos(byte[][] board, byte plr)
        {
            int bestEval = -2147483648;
            List<int> bestPos = new List<int>();

            for (int x = 0; x < board.Length; x++)
            {
                byte[][] newBoard = new byte[board.Length][];
                for (int i = 0; i < newBoard.Length; i++)
                    newBoard[i] = (byte[])board[i].Clone();

                int y = -1;
                for (int row = 0; row < board[x].Length; row++)
                {
                    if (board[x][row] == 0)
                    {
                        y = row;
                        break;
                    }
                }

                if (y == -1)
                    continue;

                newBoard[x][y] = plr;
                int currentEval = recurPositions(newBoard, plr, 0, 1, aiLevel);

                if (currentEval > bestEval)
                {
                    bestEval = currentEval;
                    bestPos.Clear();
                }
                if (currentEval == bestEval)
                {
                    bestPos.Add(x);
                }
            }

            return bestPos[rnd.Next(bestPos.Count)];
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
                    aiLevel = (aiLevel + 2) % (AI_DEPTH + 2) - 1;
            }
            if (mouse.LeftButton == ButtonState.Released && debounceLeft)
                debounceLeft = false;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.AntiqueWhite);

            while (aiLevel > -1 && turns % 2 == 1 && frames > checkWinnerFrame && win == 0)
            {
                int column = bestPos(board, 2);
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
                win = checkWinner(board);
            }

            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.Begin();
            spriteBatch.DrawString(_fontLarge,"AI", new Vector2(CHIP_OFFSET_X,BOARD_SIZE_Y-CHIP_SIZE_Y), aiLevel == -1 ? Color.Black : new Color((float)(aiLevel + 1) / (AI_DEPTH + 1), 1 - (float)(aiLevel + 1) / (AI_DEPTH + 1), 0));

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
