using Nano.Loading;
using Nano.Science.Simulation.ForceField;
using Nano.Science.Simulation.Instantiated;
using Nano.Science.Simulation.Residues;
using Nano.Science.Simulation.Spawning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Simbox.QML.Load
{
    class QMLForceFieldTerms : IForceFieldTerms
    {
        public void AddToTopology(InstantiatedTopology parentTopology, IResidue residue, ref SpawnContext context)
        {
        }

        public void Load(LoadContext context, XmlNode node)
        {
            //TODO add representations & configurations. 
        }

        public XmlElement Save(LoadContext context, XmlElement element)
        {
            return element;
        }
    }
}
