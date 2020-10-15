using System;
using System.Collections.Generic;
using System.Text;
using VertexGenerator.Cubes;

namespace VertexGenerator.Utilities
{
    public static class MatrixExtensions
    {
        public static IEnumerable<ValueTuple<Index, Vertex>> TraverseMatrix(this Vertex[,,] matrix, int planesToTraverse)
        {
            for (var x = 0; x < matrix.GetLength(0); x++)
            {
                for (var y = 0; y < matrix.GetLength(0); y++)
                {
                    for (var z = 0; z < matrix.GetLength(0); z++)
                    {
                        var index = new Index(x,y,z);
                        yield return new ValueTuple<Index, Vertex>(index, matrix.At(index));
                    }
                }
            }
        }

        public static IEnumerable<ValueTuple<Index, Vertex>> TraverseMatrixBackwards(this Vertex[,,] matrix, int planesToTraverse)
        {
            var size = matrix.GetLength(0);
            for (var x = size; x > -1 && planesToTraverse > -1; x--, planesToTraverse--)
            {
                for (var y = size; y > -1; y--)
                {
                    for (var z = size; z > -1; z--)
                    {
                        var index = new Index(x, y, z);
                        yield return new ValueTuple<Index, Vertex>(index, matrix.At(index));
                    }
                }
            }
        }

        public static Vertex At(this Vertex[,,] matrix, Index index)
        {
            if (index.Equals(Index.OutOfBoundsIndex))
            {
                // either destroy the cube or set up a state machine to modify the adjacent
                throw new NotImplementedException(
                    "Handling indexes that create out of bounds scenarios for cubes is not yet implemented");
            }

            return matrix[index.X, index.Y, index.Z];
        }

        public static bool TryGetXNext(this Vertex[,,] matrix, Index currentIndex, out Vertex vertex)
        {
            var xNext = currentIndex.XNext();
            if (OutOfBounds(xNext))
            {
                vertex = default(Vertex);
                return false;
            }

            vertex = matrix.At(xNext);
            return true;
        }

        public static bool TryGetXPrior(this Vertex[,,] matrix, Index currentIndex, out Vertex vertex)
        {
            var xPrior = currentIndex.XPrior();
            if (OutOfBounds(xPrior))
            {
                vertex = default(Vertex);
                return false;
            }

            vertex = matrix.At(xPrior);
            return true;
        }

        public static bool TryGetYNext(this Vertex[,,] matrix, Index currentIndex, out Vertex vertex)
        {
            var yNext = currentIndex.YNext();
            if (OutOfBounds(yNext))
            {
                vertex = default(Vertex);
                return false;
            }

            vertex = matrix.At(yNext);
            return true;
        }

        public static bool TryGetYPrior(this Vertex[,,] matrix, Index currentIndex, out Vertex vertex)
        {
            var yPrior = currentIndex.YPrior();
            if (OutOfBounds(yPrior))
            {
                vertex = default(Vertex);
                return false;
            }

            vertex = matrix.At(yPrior);
            return true;
        }

        public static bool TryGetZNext(this Vertex[,,] matrix, Index currentIndex, out Vertex vertex)
        {
            var zNext = currentIndex.ZNext();
            if (OutOfBounds(zNext))
            {
                vertex = default(Vertex);
                return false;
            }

            vertex = matrix.At(zNext);
            return true;
        }

        public static bool TryGetZPrior(this Vertex[,,] matrix, Index currentIndex, out Vertex vertex)
        {
            var zPrior = currentIndex.ZPrior();
            if (OutOfBounds(zPrior))
            {
                vertex = default(Vertex);
                return false;
            }

            vertex = matrix.At(zPrior);
            return true;
        }

        private static bool OutOfBounds(Index index)
        {
            return index.Equals(Index.OutOfBoundsIndex);
        }
    }
}
