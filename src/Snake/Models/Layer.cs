using System;
using System.Collections.Generic;
using System.Text;

namespace Snake.Models
{
    class Layer
    {
        public int Count
        {
            private set; get;
        }
        Matrix _weightMatrix;
        Matrix _bias;
        Func<double, double> _activactionFunction;
        public Layer(int numberOfNodes, int previousLayerNumberOfNodes, Func<double, double> activationFunction)
        {
            Count = 0;
            _activactionFunction = activationFunction;
            _weightMatrix = new Matrix(numberOfNodes, previousLayerNumberOfNodes);
            Count += _weightMatrix.Count;
            _bias = new Matrix(numberOfNodes, 1);
            Count += _bias.Count;
        }
        public void Randomize()
        {
            _weightMatrix.Randomize();
            _bias.Randomize();
        }
        public Matrix CalculateOutput(Matrix previousLayer)
        {
            var result = Matrix.Multiply(_weightMatrix, previousLayer);
            result.Add(_bias);
            result.Map(_activactionFunction);
            return result;
        }
        public void LoadValuesFromArray(double[] initialValues, ref int index)
        {
            _weightMatrix.LoadValuesFromArray(initialValues, ref index);
            _bias.LoadValuesFromArray(initialValues, ref index);
        }
        public void SaveValuesToArray(double[] array, ref int index)
        {
            _weightMatrix.SaveValuesToArray(array, ref index);
            _bias.SaveValuesToArray(array, ref index);
        }
    }
}
