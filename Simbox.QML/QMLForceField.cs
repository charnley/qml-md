using Nano.Science.Simulation;
using Nano.Science.Simulation.ForceField;
using SlimMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simbox.QML
{
    /// <summary>
    /// A forcefield interface to the QML package.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class QMLForceField : IForceField
    {
        public string Name => "QML Force Field";

        public QMLForceField()
        {
            
        }
        
        public float CalculateForceField(IAtomicSystem system, List<Vector3> forces)
        {
            //TODO implement the python calls here.
            return 0f;
        }

        public void Dispose()
        {
            //Dispose of things here.
        }
    }
}
