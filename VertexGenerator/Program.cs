using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using VertexGenerator.Cubes;
using Index = VertexGenerator.Utilities.Index;

namespace VertexGenerator
{
    class Program
    {
        private const int MatrixLength = 3;
        private static readonly HashSet<CubeForm> AllCubeForms = new HashSet<CubeForm>();
        static void Main(string[] args)
        {
            Calculate();
        }


        private static void Calculate()
        {
            var defaultCube = CubeForm.Default;
            AllCubeForms.Add(defaultCube);
            while (AllCubeForms.Any())
            {
                var first = AllCubeForms.First();
                CalculateModifications(first);
                AllCubeForms.Remove(first);
            }
        }

        private static void CalculateModifications(CubeForm cube)
        {
            for (int x = 0; x < MatrixLength; x++)
            {
                for (int y = 0; y < MatrixLength; y++)
                {
                    for (int z = 0; z < MatrixLength; z++)
                    {
                        var newCubes = cube.PermutateNewCube(new Index(x, y, z));
                        foreach (var newCube in newCubes)
                        {
                            newCube.PrettyPrint();
                            newCube.Page();
                            AllCubeForms.Add(newCube);
                        }
                    }
                }
            }
        }
    }
}
