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


        private void ExtractCondaEnvironment(string envName)
        {
            
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //TODO generalise to other OS. 
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "activate & set PATH";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            
           
        }

        private void LoadPyConfiguration()
        {

            if (PythonHome != string.Empty)
            {
                Reporter?.PrintDetail($"Python Home: {PythonHome} ");
                //set up the python variables.
                var pyPath = $@"{PythonHome};" + Environment.GetEnvironmentVariable("PATH");
                //TODO these are all the paths that conda activate sets. We should extract these with a script deal with this properly! 
                var condaPath = @"C:\Users\simbox-developer\AppData\Local\conda\conda\envs\net;C:\Users\simbox-developer\AppData\Local\conda\conda\envs\net\Library\mingw-w64\bin;C:\Users\simbox-developer\AppData\Local\conda\conda\envs\net\Library\usr\bin;C:\Users\simbox-developer\AppData\Local\conda\conda\envs\net\Library\bin;C:\Users\simbox-developer\AppData\Local\conda\conda\envs\net\Scripts;C:\Users\simbox-developer\AppData\Local\conda\conda\envs\net\bin;C:\ProgramData\Anaconda3;C:\ProgramData\Anaconda3\Library\mingw-w64\bin;C:\ProgramData\Anaconda3\Library\usr\bin;C:\ProgramData\Anaconda3\Library\bin;C:\ProgramData\Anaconda3\Scripts;C:\ProgramData\Anaconda3\bin";
                condaPath = condaPath + ";" + Environment.GetEnvironmentVariable("PATH");

                Environment.SetEnvironmentVariable("PATH", condaPath);
                Environment.SetEnvironmentVariable("PYTHONHOME", PythonHome);
                PythonEngine.PythonHome = PythonHome;
                //PythonEngine.PythonPath = $@"{PythonHome};{PythonHome}\DLLs;{PythonHome}\Lib;{PythonHome}\Lib\site-packages;C:\Users\simbox-developer\AppData\Roaming\Python\Python35\site-packages'";
            }
            else
            {
                Reporter?.PrintDetail("Python home not set, assuming python with pythonnet is configured.");
            }


        }

        private void ImportQML()
        {
            //specify the path to the qml_md python scripts.
            var qmlDir = Helper.ResolvePath("~/Plugins/Simbox.QML/");
            using(Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                sys.path.append(qmlDir);
                dynamic qml = Py.Import("qml_md");

                qml = Py.Import("qml.utils.alchemy");
                QmlPredict = Py.Import("qml_md.predict");
            }

            
        }
    }
}
