using System;
using System.Collections.Generic;
using VertexGenerator.Cubes;
using VertexGenerator.Utilities;
using Index = VertexGenerator.Utilities.Index;

namespace VertexGenerator.Materials
{

    public class SolidMetaMaterial : MetaMaterial
    {
        public override IEnumerable<string> MaterialFriendlyNames => new[] {"Stone", "Rock"};
        public override string MetaMaterialName => "Solid";
        public override bool IsValid(CubeForm cubeForm, out IReadOnlyCollection<Index> indexesOfInvalidVertices)
        {
            var matrixCopy = cubeForm.MatrixCopy;
            indexesOfInvalidVertices = Array.Empty<Index>();
            foreach (var indexTuple in matrixCopy.TraverseMatrix())
            {
                var current = indexTuple.Item1;
                var currentVertex = indexTuple.Item2;
                if (matrixCopy.TryGetXPrior(current, out var xPrior) && xPrior.X > currentVertex.X)
                {
                    return false;
                }

                if (matrixCopy.TryGetYPrior(current, out var yPrior) && yPrior.Y > currentVertex.Y)
                {
                    return false; // the vertex to the north of us, has a y value putting it to the south of us
                }

                if (matrixCopy.TryGetZPrior(current, out var zPrior) && zPrior.Z > currentVertex.Z)
                {
                    return false; // the vertex above up has a Z less than us
                }
            }

            return true;
        }

        public override CubeForm MakeValid(CubeForm cubeFormCopy, IReadOnlyCollection<Index> indexesOfInvalidVertices)
        {
            return CubeForm.InvalidCubeForm;
        }
    }
}
