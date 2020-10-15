using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VertexGenerator.Utilities;

namespace VertexGenerator.Cubes
{
    public readonly struct Vertex : IEnumerable<float>, IEquatable<Vertex>
    {
        public readonly UtilityFloat X;
        public readonly UtilityFloat Y;
        public readonly UtilityFloat Z;
        private const float StepValue = 0.04f; // 2 cm, on a 200 unit scale because we do -1 to + 1

        public Vertex(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool DistanceIsAboveThreshold(Vertex a, Vertex b)
        {
            var delta = a.CalculateVectorDelta(b);
            return delta.Any(c => c < (10 * StepValue)); // let's assume a 10% margin
        }

        public UtilityVector3 CalculateVectorDelta(Vertex other)
        {
            return new UtilityVector3(X - other.X, Y - other.Y, Z - other.Y);
        }

        /// <summary>
        /// Attempt to apply the delta from a prior vertex to this one
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        public Vertex CoMutate(UtilityVector3 delta)
        {
            return new Vertex(
                Math.Max(-1, Math.Min(1, delta.X + this.X)),
                Math.Max(-1, Math.Min(1, delta.Y + this.Y)),
                Math.Max(-1, Math.Min(1, Z + delta.Z)));
        }

        private Vertex xPerpendicularIncrease()
        {
            if (X + StepValue <= 1)
            {
                return new Vertex(X + StepValue, Y, Z);
            }

            return new Vertex(X, Y, Z);
        }

        private Vertex xPerpendicularDecrease()
        {
            if (X + StepValue >= -1)
            {
                return new Vertex(X - StepValue, Y, Z);
            }

            return new Vertex(X, Y, Z);
        }

        private Vertex yPerpendicularIncrease()
        {
            if (Y + StepValue <= 1)
            {
                return new Vertex(X, Y + StepValue, Z);
            }

            return new Vertex(X, Y, Z);
        }

        private Vertex yPerpendicularDecrease()
        {
            if (Y - StepValue >= -1)
            {
                return new Vertex(X, Y - StepValue, Z);
            }

            return new Vertex(X, Y, Z);
        }

        private Vertex zPerpendicularIncrease()
        {
            if (Z + StepValue <= 1)
            {
                return new Vertex(X, Y, Z + StepValue);
            }

            return new Vertex(X, Y, Z);
        }

        private Vertex zPerpendicularDecrease()
        {
            if (Z - StepValue >= -1)
            {
                return new Vertex(X, Y, Z - StepValue);
            }

            return new Vertex(X, Y, Z);
        }

        #region IEnumerable
        public IEnumerator<float> GetEnumerator()
        {
            yield return X;
            yield return Y;
            yield return Z;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Equality

        public bool Equals(Vertex other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            return obj is Vertex other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public static bool operator ==(Vertex a, Vertex b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Vertex a, Vertex b)
        {
            return !(a == b);
        }
        #endregion

        public IEnumerable <(Vertex,MutationEnum)> GetMutations(bool[] validDirections)
        {
            if (validDirections[0])
            {
                yield return (this.xPerpendicularIncrease(), MutationEnum.X);
                yield return (this.xPerpendicularDecrease(), MutationEnum.X);
            }

            if (validDirections[1])
            {
                yield return (this.yPerpendicularIncrease(), MutationEnum.Y);
                yield return (this.yPerpendicularDecrease(), MutationEnum.Y);
            }

            if (validDirections[2])
            {
                yield return (this.zPerpendicularIncrease(), MutationEnum.Z);
                yield return (this.zPerpendicularDecrease(), MutationEnum.Z);
            }

            if (validDirections[0] && validDirections[1])
            {
                yield return (this.xPerpendicularIncrease().yPerpendicularIncrease(), MutationEnum.XY);
                yield return (this.xPerpendicularIncrease().yPerpendicularDecrease(), MutationEnum.XY);
                yield return (this.xPerpendicularDecrease().yPerpendicularIncrease(), MutationEnum.XY);
                yield return (this.xPerpendicularDecrease().yPerpendicularDecrease(), MutationEnum.XY);
            }

            if (validDirections[1] && validDirections[2])
            {
                yield return (this.yPerpendicularIncrease().zPerpendicularIncrease(), MutationEnum.YZ);
                yield return (this.yPerpendicularIncrease().zPerpendicularDecrease(), MutationEnum.YZ);
                yield return (this.yPerpendicularDecrease().zPerpendicularIncrease(), MutationEnum.YZ);
                yield return (this.yPerpendicularDecrease().zPerpendicularDecrease(), MutationEnum.YZ);
            }

            if (validDirections[0] && validDirections[2])
            {
                yield return (this.xPerpendicularIncrease().zPerpendicularIncrease(), MutationEnum.XZ);
                yield return (this.xPerpendicularIncrease().zPerpendicularDecrease(), MutationEnum.XZ);
                yield return (this.xPerpendicularDecrease().zPerpendicularIncrease(), MutationEnum.XZ);
                yield return (this.xPerpendicularDecrease().zPerpendicularDecrease(), MutationEnum.XZ);
            }
        }
    }
}
