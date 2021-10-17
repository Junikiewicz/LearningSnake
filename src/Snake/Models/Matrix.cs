using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace Snake.Models
{
    class Matrix
    {
        public int Count
        {
            private set;
            get;
        }
        private readonly double[,] _matrix;
        private readonly int _rows;
        private readonly int _columns;
        private static readonly Random _rnd = new Random();
        public static double GetRandomWeight() //Values greater or equal to -1 and less than 1
        {
            var weight = _rnd.NextDouble();
            weight = _rnd.Next(0, 2) == 0 ? weight : -weight;
            return weight;
        }
        public Matrix(int rows, int columns)
        {
            _rows = rows;
            _columns = columns;
            _matrix = new double[rows, columns];
            Count = rows * columns;
        }
        public void Randomize()
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _matrix[i, j] = GetRandomWeight();
                }
            }
        }
        public void SaveValuesToArray(double[] array,ref int index)
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                     array[index++]= _matrix[i, j];
                }
            }
        }
        public void LoadValuesFromArray(double[] initialValues, ref int index)
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _matrix[i, j] = initialValues[index++];
                }
            }
        }
        public static double[] MatrixToArray(Matrix matrix)
        {
            var array = new double[matrix.Count];
            var index = 0;
            for(int i=0;i<matrix._rows;i++)
            {
                for(int j=0;j<matrix._columns;j++)
                {
                    array[index++] = matrix._matrix[i,j];
                }
            }
            return array;
        }
        public static Matrix ArrayToOneColumnMatrix(double[] array)
        {
            var matrix = new Matrix(array.Length,1);
            int index = 0;
            foreach(var number in array)
            {
                matrix._matrix[index++,0] = number;
            }
            return matrix;
        }
        public static Matrix Multiply(Matrix a, Matrix b)
        {
            var result = new Matrix(a._rows,b._columns);
            double temp;
            for(int i=0;i<a._rows;i++)
            {
                for(int j=0;j<b._columns; j++)
                {
                    temp = 0;
                    for(int k=0;k<a._columns;k++)
                    {
                        temp += a._matrix[i, k] * b._matrix[k, j];
                    }
                    result._matrix[i, j] = temp;
                }
            }
            return result;
        }
        public void Add(Matrix other)
        {
            for(int i=0;i<_rows;i++)
            {
                for(int j=0;j<_columns;j++)
                {
                    _matrix[i, j] += other._matrix[i, j];
                }
            }
        }
        public void Map(Func<double, double> function)
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _matrix[i, j] = function(_matrix[i, j]);
                }
            }
        }
    }
}
