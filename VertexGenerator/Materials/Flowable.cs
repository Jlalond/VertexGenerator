using System;
using System.Collections.Generic;
using System.Text;
using VertexGenerator.Cubes;
using VertexGenerator.Utilities;

namespace VertexGenerator.Materials
{
    /// <summary>
    /// A material that allows a material to 'flow' relative to gravity
    /// </summary>
    public class Flowable : MetaMaterial
    {
        public const float Threshold = 0.45f;

        protected override bool IsValid(CubeForm inCubeForm, MetaMaterialTransformations transforms)
        {
            foreach (var vertex in inCubeForm.ReadOnlyMatrix.TraverseMatrixBackwards(1))
            {
                var index = vertex.Item1;
                var vertexValue = vertex.Item2;
                foreach (var neighbor in index.GetNeighborsOnCurrentPlane())
                {
                    if (Math.Abs(neighbor.Z - vertexValue.Z) > Threshold)
                    {
                        //TODO: What we need to do here is readjust the vertices,
                        //so create a delta where the current cube will have it's point over 45% difference moved to the lowest if it's neighbors.
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
