using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VertexGenerator
{
    public struct Index : IEnumerable<int>
    {
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
    }
}
