using Snake.Enum;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Snake.Models
{
    struct VisionInDirection
    {
        public double distanceToFood, distanceToItself, distanceToWall;
    }
    struct VisionInEachDirection
    {
        public VisionInDirection Up;
        public VisionInDirection Down;
        public VisionInDirection Right;
        public VisionInDirection Left;
        public VisionInDirection UpRight;
        public VisionInDirection UpLeft;
        public VisionInDirection DownRight;
        public VisionInDirection DownLeft;
    }
    class SnakeVision
    {
        VisionInEachDirection vision;
        List<Point> _snakeParts;
        int _boardWidth;
        int _boardHeigth;
        Point _foodPosition;
        bool _binaryVision = false;
        public SnakeVision(List<Point> snakeParts, int boardWidth, int boardHeigth, Point foodPosition, bool binaryVision)
        {
            _snakeParts = snakeParts;
            _boardWidth = boardWidth;
            _boardHeigth = boardHeigth;
            _foodPosition = foodPosition;
            _binaryVision = binaryVision;
            CalculateVisionInEachDirection();
        }
        public double[] ToArray()
        {
            return new double[]
            {
                vision.Left.distanceToFood,
                vision.Left.distanceToItself,
                vision.Left.distanceToWall,

                vision.UpLeft.distanceToFood,
                vision.UpLeft.distanceToItself,
                vision.UpLeft.distanceToWall,

                vision.Up.distanceToFood,
                vision.Up.distanceToItself,
                vision.Up.distanceToWall,

                vision.UpRight.distanceToFood,
                vision.UpRight.distanceToItself,
                vision.UpRight.distanceToWall,

                vision.Down.distanceToFood,
                vision.Down.distanceToItself,
                vision.Down.distanceToWall,

                vision.DownRight.distanceToFood,
                vision.DownRight.distanceToItself,
                vision.DownRight.distanceToWall,

                vision.Right.distanceToFood,
                vision.Right.distanceToItself,
                vision.Right.distanceToWall,

                vision.DownLeft.distanceToFood,
                vision.DownLeft.distanceToItself,
                vision.DownLeft.distanceToWall
            };
        }
        void CalculateVisionInEachDirection()
        {
            vision = new VisionInEachDirection
            {
                Up = GetVisionInDirection(Direction.Up),
                Down = GetVisionInDirection(Direction.Down),
                Right = GetVisionInDirection(Direction.Right),
                Left = GetVisionInDirection(Direction.Left),
                UpRight = GetVisionInDirection(Direction.UpRight),
                UpLeft = GetVisionInDirection(Direction.UpLeft),
                DownRight = GetVisionInDirection(Direction.DownRight),
                DownLeft = GetVisionInDirection(Direction.DownLeft)
            };
        }
        
        VisionInDirection GetVisionInDirection(Direction direction)
        {
            VisionInDirection visionInDirection = new VisionInDirection
            {
                distanceToFood = double.PositiveInfinity,
                distanceToItself = double.PositiveInfinity
            };
            var snakeHead = _snakeParts[^1];
            double distance = 0;
            var currentPosition = snakeHead;
            var vector = GetVector(direction);;
            currentPosition = Add(currentPosition, vector);
            distance ++;
            while (currentPosition.X >= 0 && currentPosition.Y >= 0 && currentPosition.X < _boardWidth && currentPosition.Y < _boardHeigth)
            {
                if (visionInDirection.distanceToFood == double.PositiveInfinity && _foodPosition == currentPosition)
                {
                    visionInDirection.distanceToFood = distance;
                }
                else if (visionInDirection.distanceToItself == double.PositiveInfinity && _snakeParts.Contains(currentPosition))
                {
                    visionInDirection.distanceToItself = distance;
                }
                currentPosition = Add(currentPosition, vector);
                distance ++;
            }
            if(_binaryVision)
            {
                visionInDirection.distanceToFood = visionInDirection.distanceToFood == double.PositiveInfinity ? 0 :1;
                visionInDirection.distanceToItself = visionInDirection.distanceToItself == double.PositiveInfinity ? 0 : 1;
            }
            else
            {
                visionInDirection.distanceToFood = 1 / visionInDirection.distanceToFood;
                visionInDirection.distanceToItself = 1 / visionInDirection.distanceToItself;
            }
            visionInDirection.distanceToWall = 1/distance;
            
            return visionInDirection;
        }
        Point Add(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }
        Point GetVector(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    return new Point(-1, 0);
                case Direction.Right:
                    return new Point(1, 0);
                case Direction.Up:
                    return new Point(0, -1);
                case Direction.Down:
                    return new Point(0, 1);
                case Direction.UpLeft:
                    return new Point(-1, -1);
                case Direction.UpRight:
                    return new Point(1, -1);
                case Direction.DownLeft:
                    return new Point(-1, 1);
                case Direction.DownRight:
                    return new Point(1, 1);
                default:
                    throw new Exception("How is this even possible?");
            }
        }
    }
}
