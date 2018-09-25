using Nano.Science.Simulation;
using Nano.Science.Simulation.ForceField;
using SlimMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        private dynamic QmlPredict;
        
        /// <inheritdoc />
        public QMLForceField(string pythonHome = "", IReporter reporter = null)
        {
            this.Reporter = reporter;
            PythonHome = pythonHome;
            
            LoadPyConfiguration();

            ImportQML();
            
            reporter?.PrintEmphasized("Successfully initialized QML Forcefield.");
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
            if (PythonHome != string.Empty)
            {
                Reporter?.PrintDetail($"Python Home: {PythonHome} ");
                //set up the python variables.
                Environment.SetEnvironmentVariable("PATH", $@"{PythonHome};" + Environment.GetEnvironmentVariable("PATH"));
                Environment.SetEnvironmentVariable("PYTHONHOME", PythonHome);
                PythonEngine.PythonHome = PythonHome;
            }
            else
            {
                Reporter?.PrintDetail("Python home not set, assuming python with pythonnet is configured.");
            }


        }

        private void ImportQML()
        {
            //specify the path to the qml_md python scripts.
            var qmlDir = Helper.ResolvePath("~/Plugins/Simbox.QML/qml_md");
            using(Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                sys.path.append(qmlDir);
                QmlPredict = Py.Import("predict");
            }

            
        }
    }
}
