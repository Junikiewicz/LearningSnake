using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Snake.Models
{
    class Configuration
    {
        public GameConfiguration GameConfiguration { get; set; }
        public GeneticAlgorithmConfiguration GeneticAlgorithmConfiguration { get; set; }
        public NeuralNetworkConfiguration NeuralNetworkConfiguration { get; set; }
        public void SaveToFile(string path)
        {
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                outputFile.WriteLine(GameConfiguration.BoardWidth);
                outputFile.WriteLine(GameConfiguration.BoardHeight);
                outputFile.WriteLine(GameConfiguration.SnakeStartingMoves);
                outputFile.WriteLine(GameConfiguration.SnakeMovesGainedAfterEatingFood);
                outputFile.WriteLine(GameConfiguration.SnakeMaxMoves);
                outputFile.WriteLine(GameConfiguration.StartingSnakeLength);
                outputFile.WriteLine(GameConfiguration.SnakeLenghtAdditionAfterEatingFood);
                outputFile.WriteLine(GameConfiguration.BinaryVision);

                outputFile.WriteLine(GeneticAlgorithmConfiguration.PopulationSize);
                outputFile.WriteLine(GeneticAlgorithmConfiguration.MutationRate);
                outputFile.WriteLine(GeneticAlgorithmConfiguration.ParentPercentage);
                outputFile.WriteLine(GeneticAlgorithmConfiguration.PreservedParents);

                outputFile.WriteLine(NeuralNetworkConfiguration.OutputNodes);
                outputFile.WriteLine(NeuralNetworkConfiguration.InputNodes);
                outputFile.WriteLine(NeuralNetworkConfiguration.HiddenNeuronLayers);
                outputFile.WriteLine(NeuralNetworkConfiguration.NeuronsPerHiddenLayer);
                outputFile.WriteLine(NeuralNetworkConfiguration.HiddenLayersActivacionFunction);
                outputFile.WriteLine(NeuralNetworkConfiguration.OutputLayerActivactionFunction);
            }
        }
        public void LoadFromFile(string path)
        {
            using (StreamReader outputFile = new StreamReader(path))
            {
                GameConfiguration.BoardWidth = int.Parse(outputFile.ReadLine());
                GameConfiguration.BoardHeight = int.Parse(outputFile.ReadLine());
                GameConfiguration.SnakeStartingMoves = int.Parse(outputFile.ReadLine());
                GameConfiguration.SnakeMovesGainedAfterEatingFood = int.Parse(outputFile.ReadLine());
                GameConfiguration.SnakeMaxMoves = int.Parse(outputFile.ReadLine());
                GameConfiguration.StartingSnakeLength = int.Parse(outputFile.ReadLine());
                GameConfiguration.SnakeLenghtAdditionAfterEatingFood = int.Parse(outputFile.ReadLine());
                GameConfiguration.BinaryVision = bool.Parse(outputFile.ReadLine());

                GeneticAlgorithmConfiguration.PopulationSize = int.Parse(outputFile.ReadLine());
                GeneticAlgorithmConfiguration.MutationRate = double.Parse(outputFile.ReadLine());
                GeneticAlgorithmConfiguration.ParentPercentage = double.Parse(outputFile.ReadLine());
                GeneticAlgorithmConfiguration.PreservedParents = double.Parse(outputFile.ReadLine());

                NeuralNetworkConfiguration.OutputNodes = int.Parse(outputFile.ReadLine());
                NeuralNetworkConfiguration.InputNodes = int.Parse(outputFile.ReadLine());
                NeuralNetworkConfiguration.HiddenNeuronLayers = int.Parse(outputFile.ReadLine());
                NeuralNetworkConfiguration.NeuronsPerHiddenLayer = int.Parse(outputFile.ReadLine());
                NeuralNetworkConfiguration.HiddenLayersActivacionFunction = (ActivactionFunction)System.Enum.Parse(typeof(ActivactionFunction), outputFile.ReadLine());
                NeuralNetworkConfiguration.OutputLayerActivactionFunction = (ActivactionFunction)System.Enum.Parse(typeof(ActivactionFunction), outputFile.ReadLine());
            }
        }
    }
    class GameConfiguration
    {
        public int BoardWidth { get; set; }
        public int BoardHeight { get; set; }
        public int SnakeStartingMoves { get; set; }
        public int SnakeMovesGainedAfterEatingFood { get; set; }
        public int SnakeMaxMoves { get; set; }
        public int StartingSnakeLength { get; set; }
        public int SnakeLenghtAdditionAfterEatingFood { get; set; }
        public bool BinaryVision { get; set; }
    }
    class GeneticAlgorithmConfiguration
    {
        public int PopulationSize { get; set; }
        public double MutationRate { get; set; }
        public double ParentPercentage { get; set; }
        public double PreservedParents { get; set; }
    }
    class NeuralNetworkConfiguration
    {
        public int OutputNodes { get; set; }
        public int InputNodes { get; set; }
        public int HiddenNeuronLayers { get; set; }
        public int NeuronsPerHiddenLayer { get; set; }
        public ActivactionFunction HiddenLayersActivacionFunction { get; set; }
        public ActivactionFunction OutputLayerActivactionFunction { get; set; }
        public static bool HaveSameValues(NeuralNetworkConfiguration n1, NeuralNetworkConfiguration n2)
        {
            return (n1.OutputNodes == n2.OutputNodes) &&
                (n1.InputNodes == n2.InputNodes) &&
                (n1.HiddenNeuronLayers == n2.HiddenNeuronLayers) &&
                (n1.NeuronsPerHiddenLayer == n2.NeuronsPerHiddenLayer) &&
                (n1.HiddenLayersActivacionFunction == n2.HiddenLayersActivacionFunction) &&
                (n1.OutputLayerActivactionFunction == n2.OutputLayerActivactionFunction);
        }
    }
}
