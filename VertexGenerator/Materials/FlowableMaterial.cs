using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VertexGenerator.Cubes;
using VertexGenerator.Utilities;
using Index = VertexGenerator.Utilities.Index;

namespace VertexGenerator.Materials
{
    /// <summary>
    /// A material that allows a material to 'flow' relative to gravity
    /// </summary>
    public class FlowableMaterial : MetaMaterial
    {
        private const float Threshold = 0.45f;
        private const float MaxDepth = 10;
        public override IEnumerable<string> MaterialFriendlyNames => new[] {"Dirt", "Sand"};
        public override string MetaMaterialName => "Flowable/Granular";
        public override bool IsValid(CubeForm cubeForm, out IReadOnlyCollection<Index> indexesOfInvalidVertices)
        {
            var verticesToUpdate = new List<Index>();
            var isValid = true;
            foreach (var vertex in cubeForm.ReadOnlyMatrix.TraverseMatrixBackwards(1))
            {
                var index = vertex.Item1;
                var vertexValue = vertex.Item2;
                foreach (var neighbor in index.GetNeighborsOnCurrentPlane())
                {
                    if (Math.Abs(neighbor.Z - vertexValue.Z) > Threshold)
                    {
                        verticesToUpdate.Add(neighbor);
                        isValid = false;
                    }
                }
            }

            indexesOfInvalidVertices = verticesToUpdate;
            return isValid;
        }

        public override CubeForm MakeValid(CubeForm cubeFormCopy, IReadOnlyCollection<Index> indexesOfInvalidVertices)
        {
            var deltas = new List<ValueTuple<Index, UtilityVector3>>();
            foreach (var invalidVertex in indexesOfInvalidVertices)
            {
                var vertex = cubeFormCopy[invalidVertex];
                var lowestNeighbor = invalidVertex.GetNeighborsOnCurrentPlane()
                                                  .OrderByDescending(neighbor => Math.Abs(vertex.Z - cubeFormCopy[neighbor].Z))
                                                  .First();
                var diff = Math.Abs(vertex.Z - lowestNeighbor.Z);
                var split = diff / 2;

                deltas.Add(new ValueTuple<Index, UtilityVector3>(invalidVertex, new UtilityVector3(0, 0, split * -1)));
                deltas.Add(new ValueTuple<Index, UtilityVector3>(invalidVertex, new UtilityVector3(0, 0, split)));
            }

            var newCube = CubeForm.Mutate(cubeFormCopy, deltas);
            if (newCube.IsValid)
            {
                if (IsValid(newCube, out indexesOfInvalidVertices))
                {
                    return newCube;
                }

                return MakeValid(newCube, indexesOfInvalidVertices);
            }

            // new cube is invalid
            return newCube;
        }
    }
}
