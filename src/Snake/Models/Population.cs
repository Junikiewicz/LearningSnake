using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Snake.Models
{
    class Population
    {
        List<NeuralNetwork> population;
        static readonly Random rnd = new Random();
        private NeuralNetworkConfiguration _configuration;
        public Population()
        {
        }
        public void GenerateInitialPopulation(NeuralNetworkConfiguration configuration, int populationSize)
        {
            _configuration = configuration;
            population = new List<NeuralNetwork>();
            for (int i = 0; i < populationSize; i++)
            {
                var newSnake = new NeuralNetwork(configuration);
                newSnake.Randomize();
                population.Add(newSnake);
            }
        }
        public NeuralNetworkConfiguration LoadPopulationFromFolder(string folderPath, IProgress<int> progress)
        {
            _configuration = null;
            population = new List<NeuralNetwork>();
            string[] fileEntries = Directory.GetFiles(folderPath);
            for (int i = 0; i < fileEntries.Length; i++){
                var percent = (int)(100 * (double)i / (double)fileEntries.Length);
                progress.Report(percent);

                using (StreamReader outputFile = new StreamReader(Path.Combine(folderPath, fileEntries[i])))
                {
                    int index = 0;

                    var configuration = new NeuralNetworkConfiguration();
                    configuration.HiddenNeuronLayers = int.Parse(outputFile.ReadLine());
                    configuration.NeuronsPerHiddenLayer = int.Parse(outputFile.ReadLine());
                    configuration.InputNodes = int.Parse(outputFile.ReadLine());
                    configuration.OutputNodes = int.Parse(outputFile.ReadLine());
                    configuration.HiddenLayersActivacionFunction = (ActivactionFunction)System.Enum.Parse(typeof(ActivactionFunction), outputFile.ReadLine());
                    configuration.OutputLayerActivactionFunction = (ActivactionFunction)System.Enum.Parse(typeof(ActivactionFunction), outputFile.ReadLine());
                    var newSnake = new NeuralNetwork(configuration);
                    double[] genotype = new double[newSnake.Count];
                    if (_configuration == null)
                    {
                        _configuration = configuration;
                    }
                    else if (!NeuralNetworkConfiguration.HaveSameValues(_configuration,configuration))
                    {
                        throw new Exception($"Neural network configuration from {fileEntries[i]} file is incopatibile with previous ones");
                    }
                    while (true)
                    {
                        string Line = outputFile.ReadLine();
                        if (string.IsNullOrEmpty(Line))
                        {
                            break;
                        }
                        genotype[index++] = double.Parse(Line);
                    }
                    newSnake.LoadValuesFromArray(genotype);
                    population.Add(newSnake);
                }
            }
            return _configuration;
        }
        public void SavePopulationToFolder(string path, IProgress<int> progress)
        {
            var thisPopulationFolderPath = Path.Combine(path, $"{DateTime.Now:yyyy-dd-M--HH-mm-ss}");
            DirectoryInfo di = Directory.CreateDirectory(thisPopulationFolderPath);
            for(int i=0;i<population.Count;i++)
            {
                var percent = (int)(100 * (double)i / (double)population.Count);
                progress.Report(percent);

                using (StreamWriter outputFile = new StreamWriter(Path.Combine(thisPopulationFolderPath, $"{i}.txt")))
                {
                    outputFile.WriteLine(_configuration.HiddenNeuronLayers);
                    outputFile.WriteLine(_configuration.NeuronsPerHiddenLayer);
                    outputFile.WriteLine(_configuration.InputNodes);
                    outputFile.WriteLine(_configuration.OutputNodes);
                    outputFile.WriteLine(_configuration.HiddenLayersActivacionFunction);
                    outputFile.WriteLine(_configuration.OutputLayerActivactionFunction);
                    var genotype = population[i].ToArray();
                    foreach(var weight in genotype)
                    {
                        outputFile.WriteLine(weight);
                    }
                }
            }
        }
        public NeuralNetwork GetBestSnake()
        {
            return population[0];
        }
        public void SimulateGameForEntirePopulation(int seed, GameConfiguration configuration,bool _multiThreadSimulation)
        {
            if(_multiThreadSimulation)
            {
                List<Task> tasks = new List<Task>();
                foreach (var snake in population)
                {
                    tasks.Add(new Task(() => new Game(snake, seed, configuration).SimulateGame()));
                }
                Parallel.ForEach(tasks, (task) => task.Start());
                Task.WaitAll(tasks.ToArray());
            }
            else
            {
                foreach (var snake in population)
                {
                    new Game(snake, seed, configuration).SimulateGame();
                }
            }
            population.Sort((x, y) => y.Fitness.CompareTo(x.Fitness));
        }
        public void CreateNewGenerationFromPreviousOne(GeneticAlgorithmConfiguration configuration)
        {
            int parentSnakes = (int)(configuration.ParentPercentage * population.Count);
            int snakesToRemove = population.Count - parentSnakes;
            population.RemoveRange(parentSnakes, snakesToRemove);//remove weak snakes which are not going to reproduce
            Populate(configuration);
        }

        private void Populate(GeneticAlgorithmConfiguration configuration)
        {
            var newGeneration = new List<NeuralNetwork>();
            newGeneration.AddRange(population.GetRange(0, (int)(population.Count *configuration.PreservedParents)));//preserve best snakes
            while (newGeneration.Count < configuration.PopulationSize)
            {
                int firstSnake, secondSnake;
                do
                {
                    firstSnake = rnd.Next(0, population.Count);
                    secondSnake = rnd.Next(0, population.Count);
                } while (firstSnake == secondSnake);
                var genotypeLenght = population[firstSnake].Count;
                var firstGenotype = population[firstSnake].ToArray();
                var secondGenotype = population[secondSnake].ToArray();

                int breakPoint = rnd.Next(0, genotypeLenght);

                var newGenotype = new double[genotypeLenght];
                int i;
                for (i = 0; i < breakPoint; i++)
                {
                    newGenotype[i] = firstGenotype[i];
                }
                for (; i < genotypeLenght; i++)
                {
                    newGenotype[i] = secondGenotype[i];
                }
                MutateGenomRandomly(newGenotype,configuration.MutationRate);
                var newSnake = new NeuralNetwork(_configuration);
                newSnake.LoadValuesFromArray(newGenotype);
                newGeneration.Add(newSnake);
            }
            population = newGeneration;
        }
        void MutateGenomRandomly(double[] genotype, double mutationRate)
        {
            List<int> alreadyMutatedChromosomes = new List<int>();
            for (int j = 0; j < genotype.Length * mutationRate; j++)
            {
                int newIndex;
                do
                {
                    newIndex = rnd.Next(0, genotype.Length);
                } while (alreadyMutatedChromosomes.Contains(newIndex));
                alreadyMutatedChromosomes.Add(newIndex);
                genotype[newIndex] = Matrix.GetRandomWeight();
            }
        }
    }
}
