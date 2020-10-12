using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        #region StaticConstructor
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
        #endregion

        private static readonly Vertex[,,] DefaultMatrix;
        private CubeForm(Vertex[,,] matrix)
        {
            _matrix = matrix;
            Deltas = new HashSet<CubeDelta>();
            _paged = false;
        }

        /// <summary>
        /// Create all valid permutations for a current index, async to load from disk if data has been paged
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
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

            foreach (var mutationTuple in vertex.GetMutations(validDirections))
            {
                //TODO: here we only modify a vertex, when in reality we need to modify the X,Y,Z +/- 1 depending
                // so that we maintain the same distance between points
                var mutation = mutationTuple.Item1;
                if (mutation != vertex)
                {
                    var valueDelta = vertex.CalculateVectorDelta(mutation);
                    var nextIndex = index.CalculateIndexForMutation(mutationTuple.Item2, valueDelta);
                    if (nextIndex.Equals(Index.OutOfBoundsIndex))
                    {
                        // If we go out of bounds, we would *Create* a cube, explore that later.
                        continue;
                    }

                    // Now that we know what the next vertex should be, we access it and see if we can apply the same change to it.
                    var nextVertex = _matrix[nextIndex.X, nextIndex.Y, nextIndex.Z];
                    var modifiedNextVertex = nextVertex.CoMutate(valueDelta);
                    var coMutatedDelta = nextVertex.CalculateVectorDelta(modifiedNextVertex);

                    // We never want a vertex to exceed the next vertex or the bounds, as it will cause a mesh to double back on itself
                    // so if they are too close (within 10% of each other on any axis) we should destroy the cube if all the values fall below a certain value
                    // or redirect the impact depending on the material type. This is the most difficult part of this because a 'soil' should self level. This is to be explored later.
                    if (Vertex.DistanceIsAboveThreshold(modifiedNextVertex, mutation))
                    {
                        continue;
                    }


                    // creates tuples for so we can create the next cube from the mutated values
                    // then create the delta link so we know the differences between thw two
                    var newVerticesByIndex = new ValueTuple<Index, Vertex>[2];
                    newVerticesByIndex[0] = new ValueTuple<Index, Vertex>(index, mutation);
                    newVerticesByIndex[1] = new ValueTuple<Index, Vertex>(nextIndex, nextVertex);

                    var deltasByIndex = new ValueTuple<Index, UtilityVector3>[2];
                    deltasByIndex[0] = new ValueTuple<Index, UtilityVector3>(index, valueDelta);
                    deltasByIndex[1] = new ValueTuple<Index, UtilityVector3>(nextIndex, coMutatedDelta);

                    var newCube = CopyAndMutate(newVerticesByIndex);
                    var delta = new CubeDelta(this, deltasByIndex, newCube);
                    newCube.Id = Id + 1;
                    newCube.Deltas.Add(delta);
                    this.Deltas.Add(delta);
                    yield return newCube;
                }
            }
        }

        /// <summary>
        /// write the matrix to json
        /// </summary>
        /// <returns></returns>
        public async Task Page()
        {
            if (_paged) return;
            _paged = true;
            await File.WriteAllTextAsync(Path.Combine("./", Id.ToString()), JsonConvert.SerializeObject(_matrix));
            _matrix = null;
        }

        /// <summary>
        /// Load the cube data from json
        /// </summary>
        /// <returns></returns>
        private async Task Unpage()
        {
            if (!_paged) return;
            var content = await File.ReadAllTextAsync(Path.Combine("./", Id.ToString()));
            _matrix = JsonConvert.DeserializeObject<Vertex[,,]>(content);
            _paged = false;
        }

        /// <summary>
        /// create a copy of the current matrix with the mutations
        /// </summary>
        /// <param name="deltasByIndex"></param>
        /// <returns></returns>
        private CubeForm CopyAndMutate(IEnumerable<ValueTuple<Index, Vertex>> deltasByIndex)
        {
            var matrix = new Vertex[3,3,3];
            _matrix.CopyTo(matrix, 0);
            foreach (var delta in deltasByIndex)
            {
                var index = delta.Item1;
                _matrix[index.X, index.Y, index.Z] = delta.Item2;
            }
            return new CubeForm(matrix);
        }

        /// <summary>
        /// Default cube that fills the entire region
        /// </summary>
        /// <returns></returns>
        public static CubeForm Default()
        {
            return new CubeForm(DefaultMatrix);
        }
    }
}
