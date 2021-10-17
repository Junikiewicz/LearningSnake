using System;
using System.Collections.Generic;
using System.Text;

namespace Snake.Models
{
    class NeuralNetwork
    {
        public int Count
        {
            private set; get;
        }
        public double Fitness { get; set; }
        public int Score { get; set; }
        readonly Layer[] layers;
        public NeuralNetwork(NeuralNetworkConfiguration configuration)
        {
            Count = 0;
            layers = new Layer[configuration.HiddenNeuronLayers + 1];
            layers[0] = new Layer(configuration.NeuronsPerHiddenLayer, configuration.InputNodes, ActivactionFunctions.GetActivactionFunction(configuration.HiddenLayersActivacionFunction));
            Count += layers[0].Count;
            int i;
            for (i=1; i < configuration.HiddenNeuronLayers; i++)
            {
                layers[i] = new Layer(configuration.NeuronsPerHiddenLayer, configuration.NeuronsPerHiddenLayer, ActivactionFunctions.GetActivactionFunction(configuration.HiddenLayersActivacionFunction));
                Count += layers[i].Count;
            }
            layers[i] = new Layer(configuration.OutputNodes, configuration.NeuronsPerHiddenLayer, ActivactionFunctions.GetActivactionFunction(configuration.OutputLayerActivactionFunction));
            Count += layers[i].Count;
        }
        public void Randomize()
        {
            foreach (var layer in layers)
            {
                layer.Randomize();
            }
        }
        public void LoadValuesFromArray(double[] array)
        {
            int index = 0;
            foreach (var layer in layers)
            {
                layer.LoadValuesFromArray(array, ref index);
            }
        }
        public double[] ToArray()
        {
            var array = new double[Count];
            int index = 0;
            foreach (var layer in layers)
            {
                layer.SaveValuesToArray(array,ref index);
            }
            return array;
        }
        public double[] FeedForward(double[] input)
        {
            var matrixInput = Matrix.ArrayToOneColumnMatrix(input);
            var layerOutput = layers[0].CalculateOutput(matrixInput);
            for (int i = 1; i < layers.Length; i++)
            {
                layerOutput = layers[i].CalculateOutput(layerOutput);
            }
            var outputArray = Matrix.MatrixToArray(layerOutput);
            return outputArray;
        }
    }
}
