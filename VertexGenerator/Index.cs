using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace VertexGenerator
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

                    if (delta.Z > 0)
                    {
                        return temp.ZNext();
                    }

                    return temp.ZPrior();
                default:
                    throw new ArgumentException($"Hit unexpected exception for enum value: {mutationEnum}");
            }
        }

        /// <summary>
        /// return the next index plus one X value, or itself
        /// </summary>
        /// <returns></returns>
        public Index XNext()
        {
            if (X + 1 > 2)
            {
                return OutOfBoundsIndex;
            }

            return new Index(X + 1, Y, Z);
        }

        /// <summary>
        /// Return the index one minus x value, or out of bounds vertex
        /// </summary>
        /// <returns></returns>
        public Index XPrior()
        {
            if (X - 1 < -1)
            {
                return OutOfBoundsIndex;
            }

            return new Index(X - 1, Y, Z);
        }

        /// <summary>
        /// return the next index plus one Y value, or out of bounds vertex
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
        /// Return the index one minus Y value, or out of bounds vertex
        /// </summary>
        /// <returns></returns>
        public Index YPrior()
        {
            if (Y - 1 < -1)
            {
                return OutOfBoundsIndex;
            }

            return new Index(X, Y - 1, Z);
        }

        /// <summary>
        /// return the next index plus one Z value, or out of bounds vertex
        /// </summary>
        /// <returns></returns>
        public Index ZNext()
        {
            if (Z + 1 > 2)
            {
                return OutOfBoundsIndex;
            }

            return new Index(X, Y, Z + 1);
        }

        /// <summary>
        /// Return the index one minus Z value, or out of bounds vertex
        /// </summary>
        /// <returns></returns>
        public Index ZPrior()
        {
            if (Z - 1 < -1)
            {
                return OutOfBoundsIndex;
            }

            return new Index(X, Y , Z + 1);
        }

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
    }
}
