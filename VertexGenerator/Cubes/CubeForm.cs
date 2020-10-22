using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VertexGenerator.Materials;
using VertexGenerator.Utilities;
using Index = VertexGenerator.Utilities.Index;

namespace VertexGenerator.Cubes
{
    public class CubeForm
    {
        private const double volumeThreshold = 0.4; // 20% of a -1 <==> 1 plane

        #region private static members

        private static int _currentId;
        private static readonly Vertex[,,] DefaultMatrix;

        #endregion

        #region private instance members

        private Vertex[,,] _matrix;
        private volatile bool _paged = false;
        private HashSet<CubeDelta> _deltas;
        private HashSet<MetaMaterial> _materials;

        #endregion

        #region public static

        public static int NextId => _currentId++;

        /// <summary>
        /// Default cube that fills the entire region
        /// </summary>
        /// <returns></returns>
        public static CubeForm Default { get; }

        public static CubeForm InvalidCubeForm { get; }

        #endregion

        public IReadOnlyCollection<CubeDelta> Deltas => _deltas;
        public IReadOnlyCollection<MetaMaterial> ValidMaterials => _materials;
        public int Id { get; }
        public Vertex[,,] ReadOnlyMatrix => CopyMatrix();
        public bool IsValid => this == InvalidCubeForm;
        public Vertex this[Index index] => _matrix.At(index);

        public Vertex[,,] MatrixCopy => CopyMatrix();

        /// <summary>
        /// Get iterator for the matrix, starting from 0,0,0 and going forward
        /// </summary>
        public IEnumerable<ValueTuple<Index, Vertex>> ForwardIterator => _matrix.TraverseMatrix();

        /// <summary>
        /// Get iterator for matrix, starting from 2,2,2 and going backwards
        /// </summary>
        public IEnumerable<ValueTuple<Index, Vertex>> BackwardsIterator => _matrix.TraverseMatrix();

        #region StaticConstructor

        static CubeForm()
        {
            DefaultMatrix = new Vertex[3, 3, 3];
            Bottom(DefaultMatrix);
            Middle(DefaultMatrix);
            Top(DefaultMatrix);
            Default = new CubeForm(DefaultMatrix);
            InvalidCubeForm = new CubeForm(new Vertex[0, 0, 0]);
        }

        static void Bottom(Vertex[,,] matrix)
        {
            // Z Up/Down
            // Y North South
            // X W (-1) - E (1)
            // bottom level, south west corner
            matrix[0, 0, 0] = new Vertex(-1, -1, -1);
            // bottom level, southern edge
            matrix[0, 0, 1] = new Vertex(0, -1, -1);
            // bottom level, south east corner
            matrix[0, 0, 2] = new Vertex(1, -1, -1);
            // bottom level, middle row, western edge
            matrix[0, 1, 0] = new Vertex(-1, 0, -1);
            // bottom level, middle row, middle index
            matrix[0, 1, 1] = new Vertex(0, 0, -1);
            // bottom level, middle row, eastern edge
            matrix[0, 1, 2] = new Vertex(1, 0, -1);
            // bottom level, top row, western edge
            matrix[0, 2, 0] = new Vertex(-1, 1, -1);
            // bottom level, top row, middle index
            matrix[0, 2, 1] = new Vertex(0, 1, -1);
            // bottom level, top row, eastern corner
            matrix[0, 2, 2] = new Vertex(1, 1, -1);
        }

        static void Middle(Vertex[,,] matrix)
        {
            // middle level, south west corner
            matrix[0, 0, 0] = new Vertex(-1, -1, 0);
            // middle level, southern edge
            matrix[0, 0, 1] = new Vertex(0, -1, 0);
            // middle level, south east corner
            matrix[0, 0, 2] = new Vertex(1, -1, 0);
            // middle level, middle row, western edge
            matrix[0, 1, 0] = new Vertex(-1, 0, 0);
            // middle level, middle row, middle index
            matrix[0, 1, 1] = new Vertex(0, 0, 0);
            // middle level, middle row, eastern edge
            matrix[0, 1, 2] = new Vertex(1, 0, 0);
            // middle level, top row, western edge
            matrix[0, 2, 0] = new Vertex(-1, 1, 0);
            // middle level, top row, middle index
            matrix[0, 2, 1] = new Vertex(0, 1, 0);
            // middle level, top row, eastern corner
            matrix[0, 2, 2] = new Vertex(1, 1, 0);
        }

        static void Top(Vertex[,,] matrix)
        {
            // middle level, south west corner
            matrix[0, 0, 0] = new Vertex(-1, -1, 1);
            // middle level, southern edge
            matrix[0, 0, 1] = new Vertex(0, -1, 1);
            // middle level, south east corner
            matrix[0, 0, 2] = new Vertex(1, -1, 1);
            // middle level, middle row, western edge
            matrix[0, 1, 0] = new Vertex(-1, 0, 1);
            // middle level, middle row, middle index
            matrix[0, 1, 1] = new Vertex(0, 0, 1);
            // middle level, middle row, eastern edge
            matrix[0, 1, 2] = new Vertex(1, 0, 1);
            // middle level, top row, western edge
            matrix[0, 2, 0] = new Vertex(-1, 1, 1);
            // middle level, top row, middle index
            matrix[0, 2, 1] = new Vertex(0, 1, 1);
            // middle level, top row, eastern corner
            matrix[0, 2, 2] = new Vertex(1, 1, 1);
        }

        #endregion

        private CubeForm(Vertex[,,] matrix)
        {
            _matrix = matrix;
            _deltas = new HashSet<CubeDelta>();
            _materials = new HashSet<MetaMaterial>();
            _paged = false;
            Id = NextId;
        }

        public static CubeForm Mutate(CubeForm cubeform, IList<ValueTuple<Index, UtilityVector3>> deltasByValue)
        {
            foreach (var change in deltasByValue)
            {
                cubeform._matrix.Update(change.Item1, cubeform._matrix.At(change.Item1).CoMutate(change.Item2));
            }

            if (!cubeform.IsValidCubeForm())
            {
                return InvalidCubeForm;
            }

            return cubeform;
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
            var validDirections = new bool[3];
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

                    var newCube = CopyAndMutate(deltasByIndex);
                    if (!AddCubeDelta(deltasByIndex, newCube))
                    {
                        continue;
                    }

                    MetaMaterialManager.CalculateMaterials(newCube);

                    yield return newCube;
                }
            }
        }

        public bool AddCubeDelta(IList<ValueTuple<Index, UtilityVector3>> deltas, CubeForm newCube)
        {
            var delta = new CubeDelta(this, deltas, newCube);
            if (this.Deltas.Contains(delta) && newCube.IsValidCubeForm())
            {
                _deltas.Add(delta);
                newCube._deltas.Add(delta);
                return true;
            }

            return false;
        }

        public void AddMaterial(MetaMaterial material)
        {
            if (!this.IsValid)
            {
                _materials.Add(material);
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

        public CubeForm CopyMatrixToNewCubeForm() => new CubeForm(CopyMatrix());

        public IEnumerable<ValueTuple<Index, UtilityVector3>> CalculateDeltas(CubeForm other)
        {
            foreach (var indexTuple in _matrix.TraverseMatrix())
            {
                var vertex = indexTuple.Item2;
                var otherVertex = other._matrix.At(indexTuple.Item1);
                var delta = vertex.CalculateVectorDelta(otherVertex);
                if (delta.IsNonZero)
                {
                    continue;
                }

                var tuple = new ValueTuple<Index, UtilityVector3>(indexTuple.Item1, delta);
                yield return tuple;
            }
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

        private Vertex[,,] CopyMatrix()
        {
            var matrix = new Vertex[3, 3, 3];
            foreach (var indexTuple in _matrix.TraverseMatrix())
            {
                matrix.Update(indexTuple.Item1, indexTuple.Item2);
            }

            return matrix;
        }

        /// <summary>
        /// create a copy of the current matrix with the mutations
        /// </summary>
        /// <param name="cubeDelta"></param>
        /// <returns></returns>
        private CubeForm CopyAndMutate(IList<ValueTuple<Index, UtilityVector3>> deltasByValue)
        {
            var copy = this.CopyMatrixToNewCubeForm();
            return CubeForm.Mutate(copy, deltasByValue);
        }

        /// <summary>
        /// Check if a cubeform should be considered invalid and marked for a deletion step
        /// </summary>
        /// <returns></returns>
        private bool IsValidCubeForm()
        {
            var avg = 0.0;
            // compare all vertices from opposite ends of the Z plane
            for (int x = 0; x < _matrix.GetLength(0); x++)
            {
                for (int y = 0; y < _matrix.GetLength(0); y++)
                {
                    avg += Math.Abs(_matrix[x, y, 0].Z - _matrix[x, y, 2].Z);
                }
            }

            if (avg < volumeThreshold)
            {
                return false;
            }

            avg = 0.0;

            // compare all vertices in opposite ends of the X plane
            for (int y = 0; y < _matrix.GetLength(0); y++)
            {
                for (int z = 0; z < _matrix.GetLength(0); z++)
                {
                    avg += Math.Abs(_matrix[0, y, z].X - _matrix[2, y, z].X);
                }
            }

            if (avg < volumeThreshold)
            {
                return false;
            }

            for (int x = 0; x < _matrix.GetLength(0); x++)
            {
                for (int z = 0; z < _matrix.GetLength(0); z++)
                {
                    avg += Math.Abs(_matrix[x, 0, z].Y - _matrix[x, 2, z].Y);
                }
            }

            if (avg < volumeThreshold)
            {
                return false;
            }

            return true;
        }

    }   
}
