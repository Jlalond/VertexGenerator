using System;

namespace VertexGenerator.Utilities
{
    /// <summary>
    /// Class to handle easy conversion from int and float as well as simpler comparison semantics
    /// </summary>
    public readonly struct UtilityFloat : IEquatable<float>, IEquatable<int>
    {
        private readonly float _value;

        private UtilityFloat(float value)
        {
            _value = value;
        }

        public bool Equals(UtilityFloat other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            return obj is UtilityFloat other && Equals(other);
        }

        public bool Equals( float other)
        {
            return Math.Abs(_value - other) < 0.01;
        }

        public bool Equals(int other)
        {
            return Math.Abs(_value - other) < 0.01;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static implicit operator float(UtilityFloat utilityFloat) => utilityFloat._value;
        public static implicit operator UtilityFloat(float floatValue) => new UtilityFloat(floatValue);

        public static bool operator ==(UtilityFloat uFloat, int value)
        {
            return uFloat.Equals(value);
        }

        public static bool operator ==(UtilityFloat uFloat, float value)
        {
            return uFloat.Equals(value);
        }

        public static bool operator !=(UtilityFloat uFloat, float value)
        {
            return !(uFloat == value);
        }

        public static bool operator !=(UtilityFloat uFloat, int value)
        {
            return !(uFloat == value);
        }
    }
}
