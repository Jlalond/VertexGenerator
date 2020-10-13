﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VertexGenerator
{
    /// <summary>
    /// Class to denote a behavior of a material in the cube space,
    /// I.E. Granular terrain like dirt and sound would likely flow and 'normalize' after a specific delta
    /// This class is to handle this material specific transform rules
    /// </summary>
    public class MetaMaterial
    {
        /// <summary>
        /// Identify if a cube form is valid per 
        /// </summary>
        /// <param name="inCubeForm"></param>
        /// <param name="delta"></param>
        /// <param name="newCube"></param>
        /// <returns></returns>
        public virtual bool IsValid(CubeForm inCubeForm, out CubeDelta delta, out CubeForm newCube)
        {
            var matrixCopy = inCubeForm.ReadOnlyMatrix;
            delta = null;
            newCube = null;
            for (int x = 0; x < matrixCopy.Length; x++)
            {
                for (int y = 0; y < matrixCopy.Length; y++)
                {
                    for (int z = 0; z < matrixCopy.Length; z++)
                    {
                        if (x - 1 > -1 && matrixCopy[x,y,z].X > matrixCopy[x -1, y, z].X)
                        {
                            return false; // the vertex to the west of us has a value putting it to the east of us.
                        }

                        if (y - 1 > -1 && matrixCopy[x, y, z].Y < matrixCopy[x, y - 1, z].Y)
                        {
                            return false; // the vertex to the north of us, has a y value putting it to the south of us
                        }
                    }
                }
            }
        }
    }
}