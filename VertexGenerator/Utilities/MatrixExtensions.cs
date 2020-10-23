using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using VertexGenerator.Cubes;

namespace VertexGenerator.Utilities
{
    public static class MatrixExtensions
    {
        public static IEnumerable<ValueTuple<Index, Vertex>> TraverseMatrix(this Vertex[,,] matrix, int planesToTraverse = 3)
        {
            for (var z = 0; z < matrix.GetLength(0) && z < planesToTraverse; z++)
            {
                for (var x = 0; x < matrix.GetLength(0); x++)
                {
                    for (var y = 0; y < matrix.GetLength(0); y++)
                    {
                        var index = new Index(x,y,z);
                        yield return new ValueTuple<Index, Vertex>(index, matrix.At(index));
                    }
                }
            }
        }

        public static void Update(this Vertex[,,] matrix, Index index, Vertex vertex)
        {
            matrix[index.X, index.Y, index.Z] = vertex;
        }

        public static IEnumerable<ValueTuple<Index, Vertex>> TraverseMatrixBackwards(this Vertex[,,] matrix, int planesToTraverse = 3)
        {
            var size = matrix.GetLength(0);
            for (var z = size - 1; z > -1 && planesToTraverse > -1; z--, planesToTraverse--)
            {
                for (var x = size - 1; x > -1; x--)
                {
                    for (var y = size -1 ; y > -1; y--)
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
                return Vertex.OutOfBoundsVertex;
            }

            try
            {
                return matrix[index.X, index.Y, index.Z];
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.Write($"Caught index out of range error with index: {index}, and trace: {ex.StackTrace}");
                //throw;
                return Vertex.OutOfBoundsVertex;
            }
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
