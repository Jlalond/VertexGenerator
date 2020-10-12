using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;

namespace VertexGenerator
{
    public class CubeDelta : IEquatable<CubeDelta>
    {
        public CubeDelta(CubeForm origin, IList<ValueTuple<Index, UtilityVector3>> deltasByValue, CubeForm newValue)
        {
            Origin = origin;
            DeltasByIndex = new ReadOnlyCollection<(Index, UtilityVector3)>(deltasByValue);
            NewValue = newValue;
        }

        public CubeForm Origin { get; }
        public IReadOnlyCollection<ValueTuple<Index, UtilityVector3>> DeltasByIndex { get; }
        public CubeForm NewValue { get; }

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
            return HashCode.Combine(Origin, DeltasByIndex.Select(t => t.Item1.GetHashCode() + t.Item2.GetHashCode()));
        }

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

            return Equals(Origin, other.Origin) && Equals(DeltasByIndex, other.DeltasByIndex) && Equals(NewValue, other.NewValue);
        }
    }
}
