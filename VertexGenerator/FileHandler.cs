using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VertexGenerator.Cubes;
using VertexGenerator.Utilities;
using Index = VertexGenerator.Utilities.Index;

namespace VertexGenerator
{
    public static class FileHandler
    {
        private static JsonConverter[] jsonConverters = new JsonConverter[] {new VertexConverter(), new IndexConverter()};
        public static void PageCubeForm(CubeForm cubeForm)
        {
            File.WriteAllText(Path.Combine("./", cubeForm.Id.ToString() + ".json"), JsonConvert.SerializeObject(cubeForm.ForwardIterator));
        }

        public static Vertex[,,] UnPageCubeForm(CubeForm cubeForm)
        {
            var content = File.ReadAllText(Path.Combine("./", cubeForm.Id.ToString() + ".json"));
            var array = JsonConvert.DeserializeObject<ValueTuple<Index, Vertex>[]>(content, jsonConverters);
            var vertexMatrix = Vertex.Matrix;
            foreach (var (index, vertex) in array)
            {
                vertexMatrix.Update(index, vertex);
            }

            return vertexMatrix;
        }

        private class VertexConverter : JsonConverter<Vertex>
        {
            public override void WriteJson(JsonWriter writer, Vertex value, JsonSerializer serializer)
            {
                writer.WriteValue(value);
            }

            public override Vertex ReadJson(JsonReader reader, Type objectType, Vertex existingValue, bool hasExistingValue,
                                            JsonSerializer serializer)
            {
                var jArr = JToken.ReadFrom(reader).Children().ToArray();
                return new Vertex(jArr[0].ToObject<float>(), jArr[1].ToObject<float>(), jArr[2].ToObject<float>());
            }
        }

        private class IndexConverter : JsonConverter<Index>
        {
            public override void WriteJson(JsonWriter writer, Index value, JsonSerializer serializer)
            {
                writer.WriteValue(value);
            }

            public override Index ReadJson(JsonReader reader, Type objectType, Index existingValue, bool hasExistingValue,
                                           JsonSerializer serializer)
            {
                var jArr = JToken.ReadFrom(reader).Children().ToArray();
                return new Index(jArr[0].ToObject<int>(), jArr[1].ToObject<int>(), jArr[2].ToObject<int>());
            }
        }
    }
}
