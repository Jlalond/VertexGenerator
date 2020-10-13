using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;

namespace VertexGenerator
{
    /// <summary>
    /// Class to denote the difference between two cubes, allowing a cube to know the next cubestate, as well as the prior
    /// </summary>
    public class CubeDelta : IEquatable<CubeDelta>
    {
        public CubeDelta(CubeForm origin, IList<ValueTuple<Index, UtilityVector3>> deltasByValue, CubeForm newValue)
        {
            Origin = origin;
            DeltasByIndex = new ReadOnlyCollection<(Index, UtilityVector3)>(deltasByValue);
            NewValue = newValue;
        }

        /// <summary>
        /// The progenitor cube
        /// </summary>
        public CubeForm Origin { get; }
        /// <summary>
        /// A read only set of deltas by index
        /// </summary>
        public IReadOnlyCollection<ValueTuple<Index, UtilityVector3>> DeltasByIndex { get; }
        /// <summary>
        /// a reference to the new cube it will generate
        /// </summary>
        public CubeForm NewValue { get; }

        /// <summary>
        /// Metadata collection for a cube delta, I.E. is this an after affect of a prior cube state
        /// </summary>
        public List<string> DeltaMetaData { get; }

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
            // new cube is skipped so we can't accidentally generate two identical copies of the same new cube on a diagonal mutation
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
