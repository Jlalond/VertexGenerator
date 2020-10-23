using System.Collections.Generic;
using VertexGenerator.Cubes;
using VertexGenerator.Utilities;

namespace VertexGenerator.Materials
{
    /// <summary>
    /// Class to denote a behavior of a material in the cube space,
    /// I.E. Granular terrain like dirt and sound would likely flow and 'normalize' after a specific delta
    /// This class is to handle this material specific transform rules
    /// </summary>
    public abstract class MetaMaterial
    {
        /// <summary>
        /// A set of 'real' materials that this could apply to
        /// </summary>
        public abstract IEnumerable<string> MaterialFriendlyNames { get; }

        /// <summary>
        /// Friendly name to summarize what the material represents
        /// </summary>
        public abstract string MetaMaterialName { get; }

        /// <summary>
        /// Determine if a cube is valid, or what indexes need to change
        /// </summary>
        /// <param name="cubeForm">Input cube</param>
        /// <param name="indexesOfInvalidVertices"></param>
        /// <returns>List of invalid vertices</returns>
        public abstract bool IsValid(CubeForm cubeForm, out IReadOnlyCollection<Index> indexesOfInvalidVertices);

        /// <summary>
        /// Return a new valid cube that derives from the parent cube
        /// </summary>
        /// <param name="cubeFormCopy"></param>
        /// <param name="indexesOfInvalidVertices"></param>
        /// <returns></returns>
        public abstract CubeForm MakeValid(CubeForm cubeFormCopy, IReadOnlyCollection<Index> indexesOfInvalidVertices);
    }
}
