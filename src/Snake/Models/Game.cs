using MAIN.ViewModel.Helper;
using Snake.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;
using System.Windows.Threading;
using System.Threading;

namespace Snake.Models
{
    class Game : BindableClass
    {
        //USED BY GUI
        public int Score { get; private set; } = 0;
        public int MovesLeft { get; private set; }
        public Direction LastMove { get; private set; }
        public DeathReason DeathReason { get; private set; }
        public ICollection<Point> Snake { get { return _snake; } }
        public Point FoodPosition { get; private set; }
        //Configuration
        private int _snakeStartingMoveS;
        private int _snakeAdditionalMovesOnEachEatenFood;
        private int _snakeStartLength;
        private int _maxMoves;
        private int _snakeLenghtAdditionAfterEatingFood;
        private bool binaryVision;
        private readonly int _boardWidth;
        private readonly int _boardHeight;
        //Game
        private List<Point> _snake = new List<Point>();
        private Direction snakeDirection;
        private int snakeLength;
        private int lifeTime;
        //Other
        private readonly NeuralNetwork _neuralNetwork;
        private readonly Random rnd;
        public Game(NeuralNetwork neuralNetwork, int seed, GameConfiguration configuration)
        {
            _boardWidth = configuration.BoardWidth;
            _boardHeight = configuration.BoardHeight;
            _snakeStartingMoveS = configuration.SnakeStartingMoves;
            _snakeAdditionalMovesOnEachEatenFood = configuration.SnakeMovesGainedAfterEatingFood;
            _maxMoves = configuration.SnakeMaxMoves;
            _snakeStartLength = configuration.StartingSnakeLength;
            _snakeLenghtAdditionAfterEatingFood = configuration.SnakeLenghtAdditionAfterEatingFood;
            _neuralNetwork = neuralNetwork;
            rnd = new Random(seed);
            binaryVision = configuration.BinaryVision;
        }
        private double CalculateFitness()
        {
            return lifeTime + (Math.Pow(2, Score) + Math.Pow(Score, 2.1) * 500) - Math.Pow(Score, 1.2) * Math.Pow((0.25 * lifeTime), 1.3);
        }
        public void ShowGame(Action gameState, Dispatcher dispatcher, int frameTime)
        {
            StartNewGame();
            while (!MoveSnake())
            {
                dispatcher.Invoke(gameState);
                Thread.Sleep(frameTime);
            }
            LastMove = snakeDirection;
        }
        public void SimulateGame()
        {
            StartNewGame();
            while (!MoveSnake())
            {
            }
            _neuralNetwork.Fitness = CalculateFitness();
            _neuralNetwork.Score = Score;
        }
        private void StartNewGame()
        {
            MovesLeft = _snakeStartingMoveS;
            Snake.Clear();
            GetNextFoodPosition();
            Snake.Add(new Point(_boardWidth / 2, _boardHeight / 2));
            Score = 0;
            snakeLength = _snakeStartLength;
        }
        private Direction DecideDirection(double[] outputArray)
        {
            var maxOutput = outputArray[0];
            var maxOutputIndex = 0;
            for (int i = 1; i < outputArray.Length; i++)
            {
                if (outputArray[i] > maxOutput)
                {
                    maxOutput = outputArray[i];
                    maxOutputIndex = i;
                }
            }
            switch (maxOutputIndex)
            {
                case 0:
                    {
                        return Direction.Up;
                    }
                case 1:
                    {
                        return Direction.Down;
                    }
                case 2:
                    {
                        return Direction.Left;
                    }
                case 3:
                    {
                        return Direction.Right;
                    }
                default:
                    {
                        throw new Exception("Bararara");
                    }
            }
        }
        private bool MoveSnake()
        {
            while (_snake.Count >= snakeLength)
            {
                _snake.RemoveAt(0);
            }
            var vision = new SnakeVision(_snake, _boardWidth, _boardHeight, FoodPosition, binaryVision);
            var output = _neuralNetwork.FeedForward(vision.ToArray());
            snakeDirection = DecideDirection(output);
            Point snakeHead = _snake[^1];
            int nextX = snakeHead.X;
            int nextY = snakeHead.Y;
            switch (snakeDirection)
            {
                case Direction.Left:
                    nextX--;
                    break;
                case Direction.Right:
                    nextX++;
                    break;
                case Direction.Up:
                    nextY--;
                    break;
                case Direction.Down:
                    nextY++;
                    break;
            }
            Snake.Add(new Point()
            {
                X = nextX,
                Y = nextY
            });
            lifeTime++;
            MovesLeft--;
            if (MovesLeft < 0)
            {
                DeathReason = DeathReason.Limit;
                return true;
            }

            return DoCollisionCheck();
        }
        private bool DoCollisionCheck()
        {
            Point snakeHead = _snake[^1];

            if ((snakeHead.X == FoodPosition.X) && (snakeHead.Y == FoodPosition.Y))
            {
                return EatSnakeFood();
            }

            if ((snakeHead.Y < 0) || (snakeHead.Y >= _boardHeight) || (snakeHead.X < 0) || (snakeHead.X >= _boardWidth))
            {
                DeathReason = DeathReason.Wall;
                return true;
            }

            for (int i = 0; i < Snake.Count - 1; i++)
            {
                if ((snakeHead.X == _snake[i].X) && (snakeHead.Y == _snake[i].Y))
                {
                    DeathReason = DeathReason.Tail;
                    return true;
                }
            }
            return false;
        }

        private bool EatSnakeFood()
        {
            snakeLength += _snakeLenghtAdditionAfterEatingFood; ;
            MovesLeft += _snakeAdditionalMovesOnEachEatenFood;
            Score++;
            if (MovesLeft > _maxMoves)
            {
                MovesLeft = _maxMoves;
            }
            return GetNextFoodPosition();
        }
        private bool GetNextFoodPosition()
        {
            if (snakeLength >= _boardWidth * _boardHeight)
            {
                return true;
            }
            int foodX, foodY;
            bool free;
            do
            {
                free = true;
                foodX = rnd.Next(0, _boardWidth);
                foodY = rnd.Next(0, _boardHeight);
                foreach (Point snakePart in Snake)
                {
                    if ((snakePart.X == foodX) && (snakePart.Y == foodY))
                    {
                        free = false;
                        break;
                    }
                }
            } while (!free);
            FoodPosition = new Point(foodX, foodY);
            return false;
        }

    }
}
