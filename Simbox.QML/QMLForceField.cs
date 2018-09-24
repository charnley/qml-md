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
    public class QMLForceField : IForceField
    {
        /// <inheritdoc />
        public string Name => "QML Force Field";

        /// <inheritdoc />
        public QMLForceField()
        {
            
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
    }
}
