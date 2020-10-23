using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VertexGenerator.Cubes;

namespace VertexGenerator.Utilities
{
    public readonly struct Index : IEnumerable<int>, IEquatable<Index>
    {
        public static readonly Index OutOfBoundsIndex = new Index(-1,-1,-1);
        public readonly int X;
        public readonly int Y;
        public readonly int Z;
        public Index(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Get all eight neighbor indexes on the current plane Clockwise
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Index> GetNeighborsOnCurrentPlane()
        {
            yield return YNext(); // North
            yield return YNext().XNext(); // North East
            yield return XNext(); // East
            yield return XNext().YPrior(); // SouthEast
            yield return YPrior(); // South
            yield return YPrior().XPrior(); // SW
            yield return XPrior(); // to the west
            yield return XPrior().YNext(); // North West;
        }

        public IEnumerator<int> GetEnumerator()
        {
            yield return X;
            yield return Y;
            yield return Z;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool OutOfBounds => this.Equals(OutOfBoundsIndex);

        /// <summary>
        /// Calculate what the corresponding vertex should be for the delta along a certain axis
        /// </summary>
        /// <param name="mutationEnum">The axis of the change</param>
        /// <param name="delta">The change between the old and next values</param>
        /// <returns></returns>
        public Index CalculateIndexForMutation(MutationEnum mutationEnum, UtilityVector3 delta)
        {
            Index temp;
            switch (mutationEnum)
            {
                case MutationEnum.X:
                    if (delta.X > 0)
                    {
                        return XNext();
                    }

                    return XPrior();
                case MutationEnum.Y:
                    if (delta.Y > 0)
                    {
                        return YNext();
                    }

                    return YPrior();
                case MutationEnum.Z:
                    if (delta.Z > 0)
                    {
                        return ZNext();
                    }

                    return ZPrior();

                case MutationEnum.XY:
                    if (delta.X > 0)
                    {
                        temp = XNext();
                    }
                    else
                    {
                        temp = XPrior();
                    }

                    if (temp.OutOfBounds)
                    {
                        return temp;
                    }

                    if (delta.Y > 0)
                    {
                        return temp.YNext();
                    }

                    return temp.YPrior();

                case MutationEnum.XZ:
                    if (delta.X > 0)
                    {
                        temp = XNext();
                    }
                    else
                    {
                        temp = XPrior();
                    }

                    if (temp.OutOfBounds)
                    {
                        return temp;
                    }

                    if (delta.Z > 0)
                    {
                        return temp.ZNext();
                    }

                    return temp.ZPrior();

                case MutationEnum.YZ:
                    if (delta.Y > 0)
                    {
                        temp = YNext();
                    }
                    else
                    {
                        temp = YPrior();
                    }

                    if (temp.OutOfBounds)
                    {
                        return temp;
                    }

                    if (delta.Z > 0)
                    {
                        return temp.ZNext();
                    }

                    return temp.ZPrior();
                default:
                    throw new ArgumentException($"Hit unexpected exception for enum value: {mutationEnum}");
            }
        }

        #region Index to Vertex
        // This is an especially confusing section,
        // because our cube is based around a normal, our XYZ values in the vertex DO NOT correlate to their index values
        // I,E, X should decrease you increase in the z dimension of the matrix. I.E. [0,0,0].X < [0,0,1].X
        // And [1,0,0].Z > [0,0,0].Z

        /// <summary>
        /// Returns the next index of the data matrix that accompanies an increase in X 
        /// </summary>
        /// <returns></returns>
        public Index XNext()
        {
            if (Z + 1 > 2)
            {
                return OutOfBoundsIndex;
            }

            return new Index(X, Y, Z + 1);
        }

        /// <summary>
        /// Returns the next index of the data matrix that accompanies an decrease in Z
        /// </summary>
        /// <returns></returns>
        public Index XPrior()
        {
            if (Z - 1 < 0)
            {
                return OutOfBoundsIndex;
            }

            return new Index(X, Y, Z - 1);
        }

        /// <summary>
        /// Returns the next index of the data matrix that accompanies an increase in Y
        /// </summary>
        /// <returns></returns>
        public Index YNext()
        {
            if (Y + 1 > 2)
            {
                return OutOfBoundsIndex;
            }

            return new Index(X, Y + 1, Z);
        }

        /// <summary>
        /// Returns the next index of the data matrix that accompanies an decrease in Y
        /// </summary>
        /// <returns></returns>
        public Index YPrior()
        {
            if (Y - 1 < 0)
            {
                return OutOfBoundsIndex;
            }

            return new Index(X, Y - 1, Z);
        }

        /// <summary>
        /// Returns the next index of the data matrix that accompanies an increase in Z
        /// </summary>
        /// <returns></returns>
        public Index ZNext()
        {
            if (X + 1 > 2)
            {
                return OutOfBoundsIndex;
            }

            return new Index(X + 1, Y, Z);
        }

        /// <summary>
        /// Returns the next index of the data matrix that accompanies an decrease in Z
        /// </summary>
        /// <returns></returns>
        public Index ZPrior()
        {
            if (X - 1 < 0)
            {
                return OutOfBoundsIndex;
            }

            return new Index(X - 1, Y, Z);
        }
        #endregion

        public bool Equals(Index other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is Index other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public override string ToString()
        {
            return $"[{X}, {Y}, {Z}]";
        }
    }
}
