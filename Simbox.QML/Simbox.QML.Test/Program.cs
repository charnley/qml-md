using System;
using Simbox.QML;

namespace Simbox.QML.Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            string pythonPath = "";
            if(args.Length == 0)
            {
                Console.WriteLine("Usage: Simbox.QML.Test.exe modelDirectory pythonPath");
            }
            string modelPath = args[0];
            
            if (args.Length == 2)
            {
                pythonPath = args[1];
            }
            var forcefield = new QMLForceField(modelPath, pythonPath);
        }
    }
}