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
using Simbox.MD;
using Nano.Science;

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

        private float[] positions1d;

        private Element[] atomElements; 

        /// <inheritdoc />
        public QMLForceField(string modelPath, string pythonHome = "", IReporter reporter = null)
        {
            this.Reporter = reporter;
            PythonHome = pythonHome;
            
            LoadPyConfiguration();

            ImportQML();

            LoadFile(modelPath);

            reporter?.PrintEmphasized("Successfully initialized QML Forcefield.");
        }

        private void LoadFile(string modelPath)
        {
            var fullPath = Helper.ResolvePath(modelPath);
            dynamic nmax = QmlPredict.read_model(fullPath);
            
        }

        /// <inheritdoc />
        public float CalculateForceField(IAtomicSystem system, List<Vector3> forces)
        {
            if (positions1d == null)
                positions1d = new float[system.NumberOfParticles * 3];
            if (atomElements == null)
                atomElements = system.Topology.Elements.ToArray();
            GetPositions(system, ref positions1d);

            float energy;
            using (Py.GIL())
            {
                dynamic result = QmlPredict.calculate(atomElements, positions1d);
                energy = result[0];
                dynamic pyForces = result[1];
                CopyForces(pyForces, forces);
            }

            return energy;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            //Dispose of things here.
        }

        private void CopyForces(dynamic pyForces, List<Vector3> forces)
        {
            for (int i = 0; i < forces.Count; i++)
            {
                //lame memory allocation.
                var vec = Vector3.Zero;
                for (int j = 0; j < 3; j++)
                {
                    vec[j] = pyForces[i * 3 + j];
                }

                forces[i] += vec;
            }
        }

        private void GetPositions(IAtomicSystem system, ref float[] positions)
        {
            for (int i = 0; i < system.NumberOfParticles; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    positions[i * 3 + j] = system.Particles.Position[i][j] * Units.AngstromsPerNm;
                }
            }
        }



        private void LoadPyConfiguration(string condaEnvironment="")
        {
            
            if (PythonHome != string.Empty)
            {
                Reporter?.PrintDetail($"Python Home: {PythonHome} ");
                //set up the python variables.
                var pyPath = $@"{PythonHome};" + Environment.GetEnvironmentVariable("PATH");
                //TODO these are all the paths that conda activate sets. We should extract these with a script deal with this properly! ExtractCondaEnvironment.
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


                QmlPredict = Py.Import("qml_md.predict");
            }

            
        }

        private string ExtractCondaEnvironment(string envName)
        {

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //TODO generalise to other OS. 
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/c activate {envName} && set PATH";
            process.StartInfo = startInfo;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();
            //read out the path
            string output = process.StandardOutput.ReadToEnd();
            string pathVar = "Path=";
            string path = output.Split('\n')[0];
            path = path.Substring(pathVar.Length, path.Length - pathVar.Length);
            return path;
        }
    }
}
