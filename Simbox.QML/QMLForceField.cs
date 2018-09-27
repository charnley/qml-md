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
        private string modelPath;
        private dynamic numpy;
        private dynamic QmlPredict;

        private float[] positions1d;

        private int[] atomElements;
        private const float kJMolPerHartree = 2625.50f;

        private int callCount = 0;
        /// <inheritdoc />
        public QMLForceField(string modelPath, string pythonHome = "", IReporter reporter = null)
        {
            this.Reporter = reporter;
            this.modelPath = modelPath;
            PythonHome = pythonHome;
            LoadPyConfiguration();
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();


            ImportQML();
            LoadFile(modelPath);

            reporter?.PrintEmphasized("Successfully initialized QML Forcefield.");
        }

        private void LoadFile(string modelPath)
        {
            var fullPath = Helper.ResolvePath(modelPath);
            using (Py.GIL())
            {
                dynamic nmax = QmlPredict.read_model(fullPath);
            }
        }

        private void InitialiseFields(IAtomicSystem system)
        {
            if (positions1d == null)
                positions1d = new float[system.NumberOfParticles * 3];
            if (atomElements == null)
            {
                atomElements = new int[system.NumberOfParticles];
                for (int i = 0; i < system.NumberOfParticles; i++)
                {
                    atomElements[i] = (int)system.Topology.Elements[i];
                }
            }



        }
        /// <inheritdoc />
        public float CalculateForceField(IAtomicSystem system, List<Vector3> forces)
        {

            InitialiseFields(system);

            GetPositions(system, ref positions1d);

            float energy;
            using (Py.GIL())
            {
                dynamic result = QmlPredict.calculate(atomElements, positions1d);
                energy = result[0] * kJMolPerHartree;
                dynamic pyForces = result[1];
                CopyForces(pyForces, forces);
                dynamic result2 = QmlPredict.calculate(atomElements, positions1d);
            }
            Console.WriteLine($"Energy: {energy}");
            return energy ;
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
                    //convert hartree / A to kJ/(mol*nm)
                    vec[j] = pyForces[i * 3 + j] * kJMolPerHartree * Units.NmPerAngstrom;
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

                numpy = Py.Import("numpy");
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
