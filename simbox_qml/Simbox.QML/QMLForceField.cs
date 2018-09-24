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
    public class QMLForceField : IForceField
    {
        public string Name => "QML Force Field";

        public float CalculateForceField(IAtomicSystem system, List<Vector3> forces)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
