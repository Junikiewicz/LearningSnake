using MAIN.ViewModel.Helper;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Snake.Enum;
using Snake.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake
{
    class ViewModel : BindableClass
    {
        //buttons
        private ICommand _closeButtonCommand;
        private ICommand _playButtonCommand;
        private ICommand _pauseButtonCommand;
        private ICommand _deleteButtonCommand;
        private ICommand _loadConfigurationButtonCommand;
        private ICommand _saveConfigurationButtonCommand;
        private ICommand _loadPopulationButtonCommand;
        private ICommand _savePopulationButtonCommand;
        private string _loadPopulationText = "Load Population";
        private string _savePopulationText = "Save Population";
        private string _loadConfigurationText = "Load Configuration";
        private string _saveConfigurationText = "Save Configuration";

        //visualization
        private ICollection<Rectangle> _elements;
        private SolidColorBrush _snakeBrush = Brushes.Yellow;
        private SolidColorBrush _foodBrush = Brushes.Red;
        private const int maxSquareSize = 40;
        private int squareSize = maxSquareSize;
        public int _boardWidth;
        public int _boardHeight;
        private bool _play = false;
        private bool _unlockedNeuralNetworkSettings = true;
        private bool _unlockedGameAndGeneticAlgorithmSettings = true;
        private bool _unlockedLoadingAndSavingButtons = true;
        private bool _unlockedMainButtons = true;

        //current game
        private int _score;
        private int _maxScore;
        private int _movesLeft;
        private Direction _lastMove;
        private DeathReason _deathReason;
        Game _bestSnakeGame;

        //general
        Population _population;
        private int _generation;
        private Configuration _configuration;
        private bool _pauseOnMaxScore = true;
        private bool _showBestGame = true;
        private bool _multiThreadSimulation = false;
        private int _frameTime = 10;

        public ViewModel()
        {
            _configuration = new Configuration
            {
                GameConfiguration = new GameConfiguration
                {
                    BoardHeight = 20,
                    BoardWidth = 20,
                    SnakeStartingMoves = 100,
                    SnakeMovesGainedAfterEatingFood = 100,
                    SnakeMaxMoves = 500,
                    StartingSnakeLength = 5,
                    SnakeLenghtAdditionAfterEatingFood = 1,
                    BinaryVision = false
                },
                NeuralNetworkConfiguration = new NeuralNetworkConfiguration
                {
                    HiddenLayersActivacionFunction = ActivactionFunction.Relu,
                    OutputLayerActivactionFunction = ActivactionFunction.Sigmoid,
                    HiddenNeuronLayers = 2,
                    NeuronsPerHiddenLayer = 16,
                    InputNodes = 24,
                    OutputNodes = 4
                },
                GeneticAlgorithmConfiguration = new GeneticAlgorithmConfiguration
                {
                    PopulationSize = 2000,
                    MutationRate = 0.01,
                    ParentPercentage = 0.05,
                    PreservedParents = 0.5
                }
            };
            CalculateWindowSize();
            ResetGame();
        }

        public void ResetGame()
        {
            _play = false;
            Score = 0;
            MaxScore = 0;
            Generation = 1;
            MovesLeft = 0;
            DeathReason = DeathReason._;
            LastMove = Direction._;
            UnlockedNeuralNetworkSettings = true;
            UnlockedGameAndGeneticAlgorithmSettings = true;
            Elements = null;
        }

        public void DeletePopulation()
        {
            _population = null;
            ResetGame();
            //NEURAL NETWORK CONFIGUATION SHOULD BE ENABLED FROM NOW
            //PLAY BUTTON SHOULD BE DISABLED
        }

        public void CreateNewPopulation()
        {
            _population = new Population();
            _population.GenerateInitialPopulation(_configuration.NeuralNetworkConfiguration, _configuration.GeneticAlgorithmConfiguration.PopulationSize);
        }

        async void Play()
        {
            _play = true;
            UnlockedNeuralNetworkSettings = false;
            UnlockedGameAndGeneticAlgorithmSettings = false;
            UnlockedLoadingAndSavingButtons = false;
            if (_population == null)
            {
                CreateNewPopulation();
            }
            while (_play)
            {
                var seed = (int)DateTime.Now.Ticks;
                await Task.Run(() => _population.SimulateGameForEntirePopulation(seed, _configuration.GameConfiguration, _multiThreadSimulation));
                var bestScoreInSimulation = _population.GetBestSnake().Score;
                if (bestScoreInSimulation > _maxScore)
                {
                    MaxScore = bestScoreInSimulation;
                }
                if (_showBestGame)
                {
                    await ShowBestSnakeGame(seed);
                }
                if (_score > _maxScore)
                {
                    MaxScore = _score;
                }
                CheckEndGame();
                _population.CreateNewGenerationFromPreviousOne(_configuration.GeneticAlgorithmConfiguration);
                Generation++;
            }
            UnlockedLoadingAndSavingButtons = true;
            UnlockedGameAndGeneticAlgorithmSettings = true;
        }

        void PauseGame()
        {
            _play = false;
            Score = _bestSnakeGame.Score;
            MovesLeft = _bestSnakeGame.MovesLeft;
        }

        void CheckEndGame()
        {
            if (_pauseOnMaxScore)
            {
                if (Score >= _configuration.GameConfiguration.BoardWidth * _configuration.GameConfiguration.BoardHeight - _configuration.GameConfiguration.StartingSnakeLength - 1)
                {
                    PauseGame();
                    UnlockedGameAndGeneticAlgorithmSettings = true;
                    UnlockedLoadingAndSavingButtons = true;
                    UnlockedMainButtons = true;
                }
            }
        }

        async Task ShowBestSnakeGame(int seed)
        {
            var bestSnake = _population.GetBestSnake();
            _bestSnakeGame = new Game(bestSnake, seed, _configuration.GameConfiguration);
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            var progressReport = new Action(() =>
            {
                NewFrame();
            });
            await Task.Run(() =>
            {
                _bestSnakeGame.ShowGame(progressReport, dispatcher, _frameTime);
            });
            LastMove = _bestSnakeGame.LastMove;
            DeathReason = _bestSnakeGame.DeathReason;
        }

        // files

        public async void LoadPopulationFromFolder()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                UnlockedMainButtons = false;
                UnlockedLoadingAndSavingButtons = false;
                UnlockedNeuralNetworkSettings = false;
                string path = dialog.FileName;
                var buttonText = LoadPopulationText;
                var progress = new Progress<int>(percent =>
                {
                    LoadPopulationText = percent + "%";
                });
                await Task.Run(() => {
                    CreateNewPopulation();
                    _configuration.NeuralNetworkConfiguration = _population.LoadPopulationFromFolder(path, progress);
                    UnlockedMainButtons = true;
                    UnlockedLoadingAndSavingButtons = true;
                });
                LoadPopulationText = buttonText;
                ResetGame();
                RefreshGUI();
            }
        }

        public async void SavePopulationToFolder()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                UnlockedMainButtons = false;
                UnlockedLoadingAndSavingButtons = false;
                string path = dialog.FileName;
                var buttonText = SavePopulationText;
                var progress = new Progress<int>(percent =>
                {
                    SavePopulationText = percent + "%";
                });
                await Task.Run(() => {
                    if(_population == null)
                    {
                        CreateNewPopulation();
                    }
                    _population.SavePopulationToFolder(path, progress);
                    UnlockedMainButtons = true;
                    UnlockedLoadingAndSavingButtons = true;
                });
                SavePopulationText = buttonText;
            }
        }

        public async void SaveConfigurationToFile()
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "Save configuration",
                FileName = "Configuration",
                Filter = "Text Documents|*.txt"
            };
            string path = string.Empty;
            if (dlg.ShowDialog() == true)
            {
                path = dlg.FileName;
                var buttonText = SaveConfigurationText;
                SaveConfigurationText = "100%";
                await Task.Run(() => _configuration.SaveToFile(path));
                await Task.Delay(500).ContinueWith(t => SaveConfigurationText = buttonText);
            }
        }

        public async void LoadConfigurationFromFile()
        {
            OpenFileDialog op = new OpenFileDialog
            {
                Title = "Load configuration",
                Filter = "Text Documents|*.txt"
            };
            string path = string.Empty;
            if (op.ShowDialog() == true)
            {
                DeletePopulation(); //SINCE SETTINGS CHANGED WE ARE NO LONGER SURE IF WE HAVE PROPER POPULATION
                path = op.FileName;
                var buttonText = LoadConfigurationText;
                LoadConfigurationText = "100%";
                await Task.Run(() => _configuration.LoadFromFile(path));
                await Task.Delay(500).ContinueWith(t => LoadConfigurationText = buttonText);
                RefreshGUI();
            }
        }

        // GUI

        private void RefreshGUI()
        {
            RaisePropertyChanged(nameof(BoardWidth));
            RaisePropertyChanged(nameof(BoardHeight));
            RaisePropertyChanged(nameof(SnakeStartingMoves));
            RaisePropertyChanged(nameof(SnakeMovesGainedAfterEatingFood));
            RaisePropertyChanged(nameof(SnakeMaxMoves));
            RaisePropertyChanged(nameof(StartingSnakeLength));
            RaisePropertyChanged(nameof(SnakeLenghtAdditionAfterEatingFood));
            RaisePropertyChanged(nameof(BinaryVision));
            RaisePropertyChanged(nameof(HiddenLayersActivacionFunction));
            RaisePropertyChanged(nameof(OutputLayerActivactionFunction));
            RaisePropertyChanged(nameof(HiddenNeuronLayers));
            RaisePropertyChanged(nameof(NeuronsPerHiddenLayer));
            RaisePropertyChanged(nameof(InputNodes));
            RaisePropertyChanged(nameof(OutputNodes));
            RaisePropertyChanged(nameof(PopulationSize));
            RaisePropertyChanged(nameof(MutationRate));
            RaisePropertyChanged(nameof(ParentPercentage));
            RaisePropertyChanged(nameof(PreservedSnakesPercentage));
        }

        private void NewFrame()
        {
            Score = _bestSnakeGame.Score;
            MovesLeft = _bestSnakeGame.MovesLeft;
            Draw();
        }

        private void CalculateWindowSize()
        {
            var boardWidth = _configuration.GameConfiguration.BoardWidth * maxSquareSize;
            var boardHeight = _configuration.GameConfiguration.BoardHeight * maxSquareSize;

            const int MaxWindowWidth = 1200;
            const int MaxWindowHeight = 700;

            double widthScale = 1;
            double heightScale = 1;

            if (boardWidth > MaxWindowWidth)
            {
                widthScale = (double)MaxWindowWidth / (double)boardWidth;
            }
            if (boardHeight > MaxWindowHeight)
            {
                heightScale = (double)MaxWindowHeight / (double)boardHeight;
            }

            double scale = widthScale < heightScale ? widthScale : heightScale;

            squareSize = (int)((double)maxSquareSize * scale);
            CanvasWidth = _configuration.GameConfiguration.BoardWidth * squareSize;
            CanvasHeight = _configuration.GameConfiguration.BoardHeight * squareSize;
            Elements = null;
        }

        private void Draw()
        {
            var snakeParts = _bestSnakeGame.Snake;
            var elements = new List<Rectangle>();
            foreach (System.Drawing.Point snakePart in snakeParts)
            {
                var rectangle = new Rectangle()
                {
                    Width = squareSize,
                    Height = squareSize,
                    Fill = _snakeBrush
                };
                elements.Add(rectangle);
                Canvas.SetTop(rectangle, snakePart.Y * squareSize);
                Canvas.SetLeft(rectangle, snakePart.X * squareSize);
            }

            var foodPosition = _bestSnakeGame.FoodPosition;
            var food = new Rectangle()
            {
                Width = squareSize,
                Height = squareSize,
                Fill = _foodBrush
            };
            elements.Add(food);
            Canvas.SetTop(food, foodPosition.Y * squareSize);
            Canvas.SetLeft(food, foodPosition.X * squareSize);

            Elements = elements;
        }

        // properties

        public int Score
        {
            get { return _score; }
            set
            {
                _score = value;
                RaisePropertyChanged(nameof(Score));
            }
        }
        public int MaxScore
        {
            get { return _maxScore; }
            set
            {
                _maxScore = value;
                RaisePropertyChanged(nameof(MaxScore));
            }
        }
        public int MovesLeft
        {
            get { return _movesLeft; }
            set
            {
                _movesLeft = value;
                RaisePropertyChanged(nameof(MovesLeft));
            }
        }
        public Direction LastMove
        {
            get { return _lastMove; }
            set
            {
                _lastMove = value;
                RaisePropertyChanged(nameof(LastMove));
            }
        }
        public DeathReason DeathReason
        {
            get { return _deathReason; }
            set
            {
                _deathReason = value;
                RaisePropertyChanged(nameof(DeathReason));
            }
        }
        public int Generation
        {
            get { return _generation; }
            set
            {
                _generation = value;
                RaisePropertyChanged(nameof(Generation));
            }
        }
        public int CanvasWidth
        {
            get { return _boardWidth; }
            set
            {
                _boardWidth = value;
                RaisePropertyChanged(nameof(CanvasWidth));
            }
        }
        public int CanvasHeight
        {
            get { return _boardHeight; }
            set
            {
                _boardHeight = value;
                RaisePropertyChanged(nameof(CanvasHeight));
            }
        }
        public int BoardWidth
        {
            get { return _configuration.GameConfiguration.BoardWidth; }
            set
            {
                _configuration.GameConfiguration.BoardWidth = value;
                CalculateWindowSize();
                RaisePropertyChanged(nameof(BoardWidth));
            }
        }
        public int BoardHeight
        {
            get { return _configuration.GameConfiguration.BoardHeight; }
            set
            {
                _configuration.GameConfiguration.BoardHeight = value;
                CalculateWindowSize();
                RaisePropertyChanged(nameof(BoardHeight));
            }
        }
        public int SnakeStartingMoves
        {
            get { return _configuration.GameConfiguration.SnakeStartingMoves; }
            set
            {
                _configuration.GameConfiguration.SnakeStartingMoves = value;
                RaisePropertyChanged(nameof(SnakeStartingMoves));
            }
        }
        public int SnakeMovesGainedAfterEatingFood
        {
            get { return _configuration.GameConfiguration.SnakeMovesGainedAfterEatingFood; }
            set
            {
                _configuration.GameConfiguration.SnakeMovesGainedAfterEatingFood = value;
                RaisePropertyChanged(nameof(SnakeMovesGainedAfterEatingFood));
            }
        }
        public int SnakeMaxMoves
        {
            get { return _configuration.GameConfiguration.SnakeMaxMoves; }
            set
            {
                _configuration.GameConfiguration.SnakeMaxMoves = value;
                RaisePropertyChanged(nameof(SnakeMaxMoves));
            }
        }
        public int StartingSnakeLength
        {
            get { return _configuration.GameConfiguration.StartingSnakeLength; }
            set
            {
                _configuration.GameConfiguration.StartingSnakeLength = value;
                RaisePropertyChanged(nameof(StartingSnakeLength));
            }
        }
        public int SnakeLenghtAdditionAfterEatingFood
        {
            get { return _configuration.GameConfiguration.SnakeLenghtAdditionAfterEatingFood; }
            set
            {
                _configuration.GameConfiguration.SnakeLenghtAdditionAfterEatingFood = value;
                RaisePropertyChanged(nameof(SnakeLenghtAdditionAfterEatingFood));
            }
        }
        public bool BinaryVision
        {
            get { return _configuration.GameConfiguration.BinaryVision; }
            set
            {
                _configuration.GameConfiguration.BinaryVision = value;
                RaisePropertyChanged(nameof(BinaryVision));
            }
        }
        public ActivactionFunction HiddenLayersActivacionFunction
        {
            get { return _configuration.NeuralNetworkConfiguration.HiddenLayersActivacionFunction; }
            set
            {
                _configuration.NeuralNetworkConfiguration.HiddenLayersActivacionFunction = value;
                RaisePropertyChanged(nameof(HiddenLayersActivacionFunction));
            }
        }
        public ActivactionFunction OutputLayerActivactionFunction
        {
            get { return _configuration.NeuralNetworkConfiguration.OutputLayerActivactionFunction; }
            set
            {
                _configuration.NeuralNetworkConfiguration.OutputLayerActivactionFunction = value;
                RaisePropertyChanged(nameof(OutputLayerActivactionFunction));
            }
        }
        public int HiddenNeuronLayers
        {
            get { return _configuration.NeuralNetworkConfiguration.HiddenNeuronLayers; }
            set
            {
                _configuration.NeuralNetworkConfiguration.HiddenNeuronLayers = value;
                RaisePropertyChanged(nameof(HiddenNeuronLayers));
            }
        }
        public int NeuronsPerHiddenLayer
        {
            get { return _configuration.NeuralNetworkConfiguration.NeuronsPerHiddenLayer; }
            set
            {
                _configuration.NeuralNetworkConfiguration.NeuronsPerHiddenLayer = value;
                RaisePropertyChanged(nameof(NeuronsPerHiddenLayer));
            }
        }
        public int InputNodes
        {
            get { return _configuration.NeuralNetworkConfiguration.InputNodes; }
            set
            {
                _configuration.NeuralNetworkConfiguration.InputNodes = value;
                RaisePropertyChanged(nameof(InputNodes));
            }
        }
        public int OutputNodes
        {
            get { return _configuration.NeuralNetworkConfiguration.OutputNodes; }
            set
            {
                _configuration.NeuralNetworkConfiguration.OutputNodes = value;
                RaisePropertyChanged(nameof(OutputNodes));
            }
        }
        public int PopulationSize
        {
            get { return _configuration.GeneticAlgorithmConfiguration.PopulationSize; }
            set
            {
                _configuration.GeneticAlgorithmConfiguration.PopulationSize = value;
                RaisePropertyChanged(nameof(PopulationSize));
            }
        }
        public int MutationRate
        {
            get { return (int)(_configuration.GeneticAlgorithmConfiguration.MutationRate * 100); }
            set
            {
                _configuration.GeneticAlgorithmConfiguration.MutationRate = (double)value / 100;
                RaisePropertyChanged(nameof(MutationRate));
            }
        }
        public int ParentPercentage
        {
            get { return (int)(_configuration.GeneticAlgorithmConfiguration.ParentPercentage * 100); }
            set
            {
                _configuration.GeneticAlgorithmConfiguration.ParentPercentage = (double)value / 100;
                RaisePropertyChanged(nameof(ParentPercentage));
            }
        }
        public int PreservedSnakesPercentage
        {
            get { return (int)(_configuration.GeneticAlgorithmConfiguration.PreservedParents * 100); }
            set
            {
                _configuration.GeneticAlgorithmConfiguration.PreservedParents = (double)value / 100;
                RaisePropertyChanged(nameof(PreservedSnakesPercentage));
            }
        }
        public bool UnlockedMainButtons
        {
            get { return _unlockedMainButtons; }
            set
            {
                _unlockedMainButtons = value;
                RaisePropertyChanged(nameof(UnlockedMainButtons));
            }
        }
        public bool UnlockedLoadingAndSavingButtons
        {
            get { return _unlockedLoadingAndSavingButtons; }
            set
            {
                _unlockedLoadingAndSavingButtons = value;
                RaisePropertyChanged(nameof(UnlockedLoadingAndSavingButtons));
            }
        }
        public bool UnlockedNeuralNetworkSettings
        {
            get { return _unlockedNeuralNetworkSettings; }
            set
            {
                _unlockedNeuralNetworkSettings = value;
                RaisePropertyChanged(nameof(UnlockedNeuralNetworkSettings));
            }
        }
        public bool UnlockedGameAndGeneticAlgorithmSettings
        {
            get { return _unlockedGameAndGeneticAlgorithmSettings; }
            set
            {
                _unlockedGameAndGeneticAlgorithmSettings = value;
                RaisePropertyChanged(nameof(UnlockedGameAndGeneticAlgorithmSettings));
            }
        }
        public int FrameTime
        {
            get { return _frameTime; }
            set
            {
                _frameTime = value;
                RaisePropertyChanged(nameof(FrameTime));
            }
        }
        public bool MultiThreadSimulation
        {
            get { return _multiThreadSimulation; }
            set
            {
                _multiThreadSimulation = value;
                RaisePropertyChanged(nameof(MultiThreadSimulation));
            }
        }
        public bool ShowBestGame
        {
            get { return _showBestGame; }
            set
            {
                _showBestGame = value;
                RaisePropertyChanged(nameof(ShowBestGame));
            }
        }
        public bool PauseOnMaxScore
        {
            get { return _pauseOnMaxScore; }
            set
            {
                _pauseOnMaxScore = value;
                RaisePropertyChanged(nameof(PauseOnMaxScore));
            }
        }
        public string LoadPopulationText
        {
            get { return _loadPopulationText; }
            set
            {
                _loadPopulationText = value;
                RaisePropertyChanged(nameof(LoadPopulationText));
            }
        }
        public string SavePopulationText
        {
            get { return _savePopulationText; }
            set
            {
                _savePopulationText = value;
                RaisePropertyChanged(nameof(SavePopulationText));
            }
        }
        public string LoadConfigurationText
        {
            get { return _loadConfigurationText; }
            set
            {
                _loadConfigurationText = value;
                RaisePropertyChanged(nameof(LoadConfigurationText));
            }
        }
        public string SaveConfigurationText
        {
            get { return _saveConfigurationText; }
            set
            {
                _saveConfigurationText = value;
                RaisePropertyChanged(nameof(SaveConfigurationText));
            }
        }
        public ICollection<Rectangle> Elements
        {
            get
            {
                return _elements;
            }
            set
            {
                _elements = value;
                RaisePropertyChanged(nameof(Elements));
            }
        }

        // buttons

        public Action CloseAction { get; set; }
        public ICommand CloseButtonCommand
        {
            get
            {
                if (_closeButtonCommand == null)
                {
                    _closeButtonCommand = new RelayCommand(p => CloseAction(), p => true);
                }
                return _closeButtonCommand;
            }
        }
        public ICommand PlayButtonCommand
        {
            get
            {
                if (_playButtonCommand == null)
                {
                    _playButtonCommand = new RelayCommand(p => Play(), p => true);
                }
                return _playButtonCommand;
            }
        }
        public ICommand PauseButtonCommand
        {
            get
            {
                if (_pauseButtonCommand == null)
                {
                    _pauseButtonCommand = new RelayCommand(p => PauseGame(), p => true);
                }
                return _pauseButtonCommand;
            }
        }
        public ICommand DeleteButtonCommand
        {
            get
            {
                if (_deleteButtonCommand == null)
                {
                    _deleteButtonCommand = new RelayCommand(p => DeletePopulation(), p => true);
                }
                return _deleteButtonCommand;
            }
        }
        public ICommand LoadConfigurationButtonCommand
        {
            get
            {
                if (_loadConfigurationButtonCommand == null)
                {
                    _loadConfigurationButtonCommand = new RelayCommand(p => LoadConfigurationFromFile(), p => true);
                }
                return _loadConfigurationButtonCommand;
            }
        }
        public ICommand SaveConfigurationButtonCommand
        {
            get
            {
                if (_saveConfigurationButtonCommand == null)
                {
                    _saveConfigurationButtonCommand = new RelayCommand(p => SaveConfigurationToFile(), p => true);
                }
                return _saveConfigurationButtonCommand;
            }
        }
        public ICommand LoadPopulationButtonCommand
        {
            get
            {
                if (_loadPopulationButtonCommand == null)
                {
                    _loadPopulationButtonCommand = new RelayCommand(p => LoadPopulationFromFolder(), p => true);
                }
                return _loadPopulationButtonCommand;
            }
        }
        public ICommand SavePopulationButtonCommand
        {
            get
            {
                if (_savePopulationButtonCommand == null)
                {
                    _savePopulationButtonCommand = new RelayCommand(p => SavePopulationToFolder(), p => true);
                }
                return _savePopulationButtonCommand;
            }
        }
    }
}
