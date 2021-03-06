﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VertexGenerator.Cubes;
using VertexGenerator.Utilities;
using Index = VertexGenerator.Utilities.Index;

namespace VertexGenerator.Materials
{
    public static class MetaMaterialManager
    {
        private static readonly List<MetaMaterial> _metaMaterials;
        static MetaMaterialManager()
        {
            _metaMaterials = typeof(MetaMaterialManager)
                            .Assembly.GetTypes().Where(t => typeof(MetaMaterial).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                            .Select(t => (MetaMaterial) Activator.CreateInstance(t)).ToList();
        }

        public static void CalculateMaterials(CubeForm cubeForm)
        {
            foreach (var material in _metaMaterials)
            {
                if (cubeForm.ValidMaterials.Contains(material))
                {
                    // if this cube was generated by the below code, we don't need to check it again.
                    continue;
                }

                if (!material.IsValid(cubeForm, out var invalidVertices))
                {
                    var validatedCube = material.MakeValid(cubeForm.CopyMatrixToNewCubeForm(), invalidVertices);
                    if (validatedCube.IsValid)
                    {
                        var deltas = cubeForm.CalculateDeltas(validatedCube).ToList();
                        if (deltas.Any())
                        {
                            cubeForm.AddCubeDelta(deltas, validatedCube);
                            validatedCube.AddMaterial(material);
                        }
                    }
                    else
                    {
                        cubeForm.AddCubeDelta(new List<(Index, UtilityVector3)>(), validatedCube);
                    }

                }
                else
                {
                    cubeForm.AddMaterial(material);
                }
            }
        }
    }
}
