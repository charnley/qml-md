using System;
using Simbox.QML;

namespace Simbox.QML.Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            string pythonPath = "";
            if (args.Length == 1)
            {
                pythonPath = args[0];
            }
            var forcefield = new QMLForceField(pythonPath);
        }
    }
}