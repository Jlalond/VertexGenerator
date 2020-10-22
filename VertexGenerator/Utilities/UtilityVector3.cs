using System;
using System.Collections;
using System.Collections.Generic;

namespace VertexGenerator.Utilities
{
    /// <summary>
    /// Utility struct to give me the delta as an array along with other helper methods
    /// </summary>
    public readonly struct UtilityVector3 : IReadOnlyList<UtilityFloat>, IEquatable<UtilityVector3>
    {
        public readonly UtilityFloat X;
        public readonly UtilityFloat Y;
        public readonly UtilityFloat Z;

        public UtilityVector3(UtilityFloat x, UtilityFloat y, UtilityFloat z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool IsZero => !this.Equals(default);

        public bool IsNonZero => !IsZero;

        public IEnumerator<UtilityFloat> GetEnumerator()
        {
            yield return X;
            yield return Y;
            yield return Z;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => 3;

        public UtilityFloat this[int index] => GetAtIndex(index);

        public UtilityFloat GetAtIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return X;
                case 1:
                    return Y;
                case 2:
                    return Z;
                default:
                    throw new ArgumentException("Utility vector only supports 0-2");
            }
        }

        public bool Equals(UtilityVector3 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            return obj is UtilityVector3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }
    }
}
