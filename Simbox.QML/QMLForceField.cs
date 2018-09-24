using Nano.Science.Simulation;
using Nano.Science.Simulation.ForceField;
using SlimMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nano;
using Python.Runtime;

namespace Simbox.QML
{
    /// <summary>
    /// A forcefield interface to the QML package.
    /// </summary>
    public class QMLForceField : IForceField
    {
        /// <summary>
        /// Python path to use.
        /// </summary>
        /// <remarks>
        /// If not set, any existing python installation on the PATH, PYTHONPATH and PYTHONHOME variables will be used.
        /// </remarks>
        public string PythonHome;

        /// <inheritdoc />
        public string Name => "QML Force Field";

        public IReporter Reporter;
        /// <inheritdoc />
        public QMLForceField()
        {
            LoadPyConfiguration();
        }

        /// <inheritdoc />
        public float CalculateForceField(IAtomicSystem system, List<Vector3> forces)
        {
            //TODO implement the python calls here.
            return 0f;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            //Dispose of things here.
        }

        private void LoadPyConfiguration()
        {
            Reporter?.PrintDetail($"Python Home: {PythonHome} ");
            //set up the python variables.
            Environment.SetEnvironmentVariable("PATH", $@"{PythonHome};" + Environment.GetEnvironmentVariable("PATH"));
            Environment.SetEnvironmentVariable("PYTHONHOME", PythonHome);
            //specify the path to the qml_md python scripts.
            var applicationDir = Helper.ResolvePath("~/Plugins/QMLForceField/qml_md");
            var pythonlibPath =  $@"{PythonHome}\DLLs;{PythonHome}\Lib;{PythonHome}\Lib\site-packages;{applicationDir}";
            Reporter?.PrintDetail($"Python Path: {pythonlibPath} ");
            Environment.SetEnvironmentVariable("PYTHONPATH ", pythonlibPath);
            PythonEngine.PythonHome = PythonHome;
            PythonEngine.PythonPath = pythonlibPath;
        }
    }
}
