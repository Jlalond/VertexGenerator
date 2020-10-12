using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VertexGenerator
{
    public class CubeForm
    {
        private Vertex[,,] _matrix;
        public HashSet<CubeDelta> Deltas { get; }
        public int Id { get; private set; }
        private volatile bool _paged = false;
        private object _lock = new object();

        static CubeForm()
        {
            DefaultMatrix = new Vertex[3, 3, 3];
            Bottom(DefaultMatrix);
            Middle(DefaultMatrix);
            Top(DefaultMatrix);
        }

        static void Bottom(Vertex[,,] matrix)
        {
            // Z Up/Down
            // Y North South
            // X W (-1) - E (1)
            //bottom level, North West corner
            matrix[0, 0, 0] = new Vertex(-1, 1, -1);
            // bottom level, Northern Middle vertex
            matrix[0, 0, 1] = new Vertex(0, 1, -1);
            // bottom level, North East Corner
            matrix[0, 0, 2] = new Vertex(1, 1, -1);
            // bottom level, Western Middle Index
            matrix[0, 1, 0] = new Vertex(-1, 0, -1);
            // bottom level, Middle face index
            matrix[0, 1, 1] = new Vertex(0, 0, -1);
            // bottom level, Eastern middle Index
            matrix[0, 1, 2] = new Vertex(1, 0, -1);
            // bottom level, South West Corner
            matrix[0, 2, 0] = new Vertex(-1,-1,-1);
            // bottom level, southern edge
            matrix[0, 2, 1] = new Vertex(0, -1, -1);
            // bottom level, south eastern corner
            matrix[0, 2, 2] = new Vertex(1, -1, -1);
        }

        static void Middle(Vertex[,,] matrix)
        {
            //middle level, North West corner
            matrix[0, 0, 0] = new Vertex(-1, 1, 0);
            // middle level, Northern Middle vertex
            matrix[0, 0, 1] = new Vertex(0, 1, 0);
            // middle level, North East Corner
            matrix[0, 0, 2] = new Vertex(1, 1, 0);
            // middle level, Western Middle Index
            matrix[0, 1, 0] = new Vertex(-1, 0, 0);
            // middle level, Middle face index
            matrix[0, 1, 1] = new Vertex(0, 0, 0);
            // middle level, Eastern middle Index
            matrix[0, 1, 2] = new Vertex(1, 0, 0);
            // middle level, South West Corner
            matrix[0, 2, 0] = new Vertex(-1, -1, 0);
            // middle level, southern edge
            matrix[0, 2, 1] = new Vertex(0, -1, 0);
            // middle level, south eastern corner
            matrix[0, 2, 2] = new Vertex(1, -1, 0);
        }

        static void Top(Vertex[,,] matrix)
        {
            //top level, North West corner
            matrix[0, 0, 0] = new Vertex(-1, 1, 1);
            // top level, Northern top vertex
            matrix[0, 0, 1] = new Vertex(0, 1, 1);
            // top level, North East Corner
            matrix[0, 0, 2] = new Vertex(1, 1, 1);
            // top level, Western top Index
            matrix[0, 1, 0] = new Vertex(-1, 0, 1);
            // top level, top face index
            matrix[0, 1, 1] = new Vertex(0, 0, 1);
            // top level, Eastern top Index
            matrix[0, 1, 2] = new Vertex(1, 0, 1);
            // top level, South West Corner
            matrix[0, 2, 0] = new Vertex(-1, -1, 1);
            // top level, southern edge
            matrix[0, 2, 1] = new Vertex(0, -1, 1);
            // top level, south eastern corner
            matrix[0, 2, 2] = new Vertex(1, -1, 1);
        }

        private static readonly Vertex[,,] DefaultMatrix;
        private CubeForm(Vertex[,,] matrix)
        {
            _matrix = matrix;
            Deltas = new List<CubeDelta>();
            _paged = false;
        }

        public async Task<IEnumerable<CubeForm>> PermutateAsync(Index index)
        {
            if (_paged)
            {
                await Unpage();
            }

            return Permutate(index);
        }

        private IEnumerable<CubeForm> Permutate(Index index)
        {
            if (index.Any(c => c > 2 || c < 0))
            {
                Console.Error.WriteLine($"Encountered index outside of bounds: [{index}]");
                Environment.Exit(1);
            }

            var vertex = _matrix[index.X, index.Y, index.Z];
            // ReSharper disable once SuggestVarOrType_Elsewhere
            Span<bool> validDirections = stackalloc bool[3];
            var i = 0;
            foreach (var value in index)
            {
                if (Math.Abs(value) == 1)
                {
                    validDirections[i] = true;
                }
                else
                {
                    validDirections[i] = false;
                }

            }

            foreach (var mutation in vertex.GetMutations(validDirections))
            {
                if (mutation != vertex)
                {
                    var newCube = CopyAndMutate(ref index, mutation);
                    var valueDelta = vertex.CalculateVectorDelta(mutation);
                    var delta = new CubeDelta(this, valueDelta, index, newCube);
                    newCube.Id = Id + 1;
                    newCube.Deltas.Add(delta);
                    this.Deltas.Add(delta);
                    yield return newCube;
                }
            }
        }

        public async Task Page()
        {
            if (_paged) return;
            _paged = true;
            await File.WriteAllTextAsync(Path.Combine("./", Id.ToString()), JsonConvert.SerializeObject(_matrix));
            _matrix = null;
        }

        private async Task Unpage()
        {
            if (!_paged) return;
            var content = await File.ReadAllTextAsync(Path.Combine("./", Id.ToString()));
            _matrix = JsonConvert.DeserializeObject<Vertex[,,]>(content);
            _paged = false;
        }

        private CubeForm CopyAndMutate(ref Index index, Vertex vertex)
        {
            var matrix = new Vertex[3,3,3];
            _matrix.CopyTo(matrix, 0);
            matrix[index.X, index.Y, index.Z] = vertex;
            return new CubeForm(matrix);
        }

        public static CubeForm Default()
        {
            return new CubeForm(DefaultMatrix);
        }

        private readonly struct Vertex : IEnumerable<float>, IEquatable<Vertex>
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

            public Vector3 CalculateVectorDelta(Vertex other)
            {
                return new Vector3(Math.Abs(X - other.X), Math.Abs(Y - other.Y), Math.Abs(Z - other.Y));
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

            public IEnumerator<float> GetEnumerator()
            {
                return CreateEnumerator().GetEnumerator();
            }

            private IEnumerable<float> CreateEnumerator()
            {
                yield return X;
                yield return Y;
                yield return Z;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

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

            public IEnumerable<Vertex> GetMutations(Span<bool> validDirections)
            {
                if (validDirections[0])
                {
                    yield return this.xPerpendicularIncrease();
                    yield return this.xPerpendicularDecrease();
                }

                if (validDirections[1])
                {
                    yield return this.yPerpendicularIncrease();
                    yield return this.yPerpendicularDecrease();
                }

                if (validDirections[2])
                {
                    yield return this.zPerpendicularIncrease();
                    yield return this.zPerpendicularDecrease();
                }

                if (validDirections[0] && validDirections[1])
                {
                    yield return this.xPerpendicularIncrease().yPerpendicularIncrease();
                    yield return this.xPerpendicularIncrease().yPerpendicularDecrease();
                    yield return this.xPerpendicularDecrease().yPerpendicularIncrease();
                    yield return this.xPerpendicularDecrease().yPerpendicularDecrease();
                }

                if (validDirections[1] && validDirections[2])
                {
                    yield return this.yPerpendicularIncrease().zPerpendicularIncrease();
                    yield return this.yPerpendicularIncrease().zPerpendicularDecrease();
                    yield return this.yPerpendicularDecrease().zPerpendicularIncrease();
                    yield return this.yPerpendicularDecrease().zPerpendicularDecrease();
                }

                if (validDirections[0] && validDirections[2])
                {
                    yield return this.xPerpendicularIncrease().zPerpendicularIncrease();
                    yield return this.xPerpendicularIncrease().zPerpendicularDecrease();
                    yield return this.xPerpendicularDecrease().zPerpendicularIncrease();
                    yield return this.xPerpendicularDecrease().zPerpendicularDecrease();
                }
            }
        }
    }
}
