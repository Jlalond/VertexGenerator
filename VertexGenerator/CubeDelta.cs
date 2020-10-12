using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VertexGenerator
{
    public class CubeDelta : IEquatable<CubeDelta>
    {
        public CubeDelta(CubeForm origin, Vector3 delta, Index vertexIndex, CubeForm newValue)
        {
            Origin = origin;
            ValueDelta = delta;
            MatrixIndex = vertexIndex;
            NewValue = newValue;
        }

        public CubeForm Origin { get; }
        public Vector3 ValueDelta { get; }
        public Index MatrixIndex { get; }
        public CubeForm NewValue { get; }

        public bool Equals(CubeDelta other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Origin, other.Origin) && ValueDelta.Equals(other.ValueDelta) && MatrixIndex.Equals(other.MatrixIndex) && Equals(NewValue, other.NewValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((CubeDelta) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Origin, ValueDelta, MatrixIndex);
        }
    }
}
