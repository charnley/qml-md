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
using Nano;
using Nano.Science.Simulation;

namespace Simbox.QML.Load
{
    /// <summary>
    /// Represents the QML forcefield terms for a given <see cref="Residue"/>. 
    /// </summary>
    [XmlName("QMLForceField")]
    class QMLForceFieldTerms : IForceFieldTerms, IInstantiatedForceFieldTerms
    {
        private string pythonHome;

        private string modelPath; 

        /// <summary>
        /// Add this forcefield to the instantiated topology. 
        /// </summary>
        /// <param name="parentTopology">The topology.</param>
        /// <param name="residue">The atom group this forcefield is associated with.</param>
        /// <param name="context">The spawn context this forcefield is associated with.</param>
        public void AddToTopology(InstantiatedTopology parentTopology, IResidue residue, ref SpawnContext context)
        {
            //TODO generalize this to multiple templates/atom groups.
            parentTopology.ForceFieldTerms.Add(this);
        }

        /// <summary>
        /// Loads this forcefield from XML file.
        /// </summary>
        /// <param name="context">Load context.</param>
        /// <param name="node">Node from which to load forcefield.</param>
        public void Load(LoadContext context, XmlNode node)
        {
            //TODO add representations & configurations. 
            //grab a python path, so we can specify environments.
            pythonHome = Helper.ResolvePath(Helper.GetAttributeValue(node, "PythonHome", ""));
            modelPath = Helper.ResolvePath(Helper.GetAttributeValue(node, "ModelPath", ""));
            if(modelPath == string.Empty)
            {
                context.Error("Missing field ModelPath", true);
            }

        }

        /// <summary>
        /// Saves this forcefield to an XML node.
        /// </summary>
        /// <param name="context">Load context.</param>
        /// <param name="element">Node to save the forcefield to.</param>
        /// <returns></returns>
        public XmlElement Save(LoadContext context, XmlElement element)
        {
            //TODO save any necessary information.
            Helper.AppendAttributeAndValue(element, "PythonHome", pythonHome);
            Helper.AppendAttributeAndValue(element, "ModelPath", modelPath);
            return element;
        }

        /// <summary>
        /// Generates the forcefield, given the topology.
        /// </summary>
        /// <param name="parentTopology">The topology to spawn the forcefield into.</param>
        /// <param name="properties">Properties of the system.</param>
        /// <param name="reporter">Reporter to print output to.</param>
        /// <returns></returns>
        public IForceField GenerateForceField(InstantiatedTopology parentTopology, SystemProperties properties, IReporter reporter)
        {
            //TODO do any extra stuff thats needed.
            var forcefield = new QMLForceField(modelPath, pythonHome, reporter);
            reporter.PrintEmphasized("Successfully initialised QML forcefield.");
            return forcefield;
        }
    }
}
