using System.Collections.Generic;
using VertexGenerator.Cubes;
using VertexGenerator.Storage;

namespace VertexGenerator.Materials
{
    /// <summary>
    /// A list of transformations defined successively from the parent cube
    /// The list is required so we can handle recursive situations around materials changing.
    /// </summary>
    public class MetaMaterialTransformations
    {
        public IList<MetaMaterialTransformation> OrderedTransforms { get; }
        public CubeForm GenesisCubeForm { get; }

        public MetaMaterialTransformations(CubeForm start)
        {
            OrderedTransforms = new List<MetaMaterialTransformation>();
        }
    }

    /// <summary>
    /// Container to describe a new cube created by a material transformation 
    /// </summary>
    public class MetaMaterialTransformation
    {
        public CubeDelta CubeDelta { get; set;  }
        public MetaDataRepository MetaData { get; set; }

        public MetaMaterialTransformation()
        {
            MetaData = new MetaDataRepository();
        }
    }
}
