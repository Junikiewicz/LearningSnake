using System;
using System.Collections.Generic;
using System.Text;

namespace Snake.Models
{
    enum ActivactionFunction
    {
        Sigmoid, Relu
    }
    class ActivactionFunctions
    {
        public static Func<double, double> GetActivactionFunction(ActivactionFunction activactionFunction)
        {
            switch (activactionFunction)
            {
                case ActivactionFunction.Sigmoid:
                    {
                        return Sigmoid;
                    }
                case ActivactionFunction.Relu:
                    {
                        return Relu;
                    }
                default:
                    {
                        throw new Exception("FASDFASDF");
                    }
            }
        }
        public static double Sigmoid(double x)
        {
            return 1 / (1 + Math.Pow(Math.E, -x));
        }
        public static double Relu(double x)
        {
            return Math.Max(0, x);
        }
    }
}


