using VertexGenerator.Cubes;
using VertexGenerator.Utilities;

namespace VertexGenerator.Materials
{
    /// <summary>
    /// Class to denote a behavior of a material in the cube space,
    /// I.E. Granular terrain like dirt and sound would likely flow and 'normalize' after a specific delta
    /// This class is to handle this material specific transform rules
    /// </summary>
    public class MetaMaterial
    {
        /// <summary>
        /// Identify if a cube form is valid
        /// </summary>
        /// <param name="inCubeForm"></param>
        /// <param name="transforms">The ordered lists of transforms up until now</param>
        /// <returns></returns>
        protected virtual bool IsValid(CubeForm inCubeForm, MetaMaterialTransformations transforms)
        {
            return true;
        }

        private static bool DefaultValid(CubeForm inCubeForm)
        {
            // default impl is just that no vertex passes a vertex that should be greater or lesser than itself.
            // I.E., the middle vertex shouldn't be drawn so that's actually the farthest X
            var matrixCopy = inCubeForm.ReadOnlyMatrix;
            for (int x = 0; x < matrixCopy.Length; x++)
            {
                for (int y = 0; y < matrixCopy.Length; y++)
                {
                    for (int z = 0; z < matrixCopy.Length; z++)
                    {
                        var current = new Index(x, y, z);
                        if (matrixCopy.TryGetXPrior(current, out var xPrior) && xPrior.X < matrixCopy.At(current).X)
                        {
                            return false; // the vertex to the west of us has a x value that puts it to the east of us.
                        }

                        if (matrixCopy.TryGetYNext(current, out var yPrior) && yPrior.Y < matrixCopy.At(current).Y)
                        {
                            return false; // the vertex to the north of us, has a y value putting it to the south of us
                        }

                        if (matrixCopy.TryGetZNext(current, out var zPrior) && zPrior.Z < matrixCopy.At(current).Z)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public bool IsValidCubeForm(CubeForm inCubeForm, out MetaMaterialTransformations transformations)
        {
            transformations = new MetaMaterialTransformations(inCubeForm);
            return DefaultValid(inCubeForm) && IsValid(inCubeForm, transformations);
        }
    }
}
