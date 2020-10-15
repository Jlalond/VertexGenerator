using System;
using System.Collections.Generic;
using System.Linq;

namespace VertexGenerator.Storage
{
    /// <summary>
    /// Utility dictionary to store meta data, and allow bidirectional lookup
    /// </summary>
    public class MetaDataRepository
    {
        private readonly Dictionary<string, object> metadataKeyValuePairs;

        public MetaDataRepository()
        {
            this.metadataKeyValuePairs = new Dictionary<string, object>();
        }

        public object this[string val] => metadataKeyValuePairs[val];

        public IEnumerable<string> this[object val] =>
            metadataKeyValuePairs.Where(t => t.Value.Equals(val)).Select(t => t.Key);

        public IEnumerable<string> Keys => metadataKeyValuePairs.Select(t => t.Key);

        public IEnumerable<Tuple<string, object>> GetCopy =>
            metadataKeyValuePairs.Select(t => new Tuple<string, object>(t.Key, t.Value));

        public bool Add(string key, object obj)
        {
            return metadataKeyValuePairs.TryAdd(key, obj);
        }

        public bool Add(KeyValuePair<string, object> pair) => Add(pair.Key, pair.Value);
        public bool Add(ValueTuple<string, object> pair) => Add(pair.Item1, pair.Item2);
        public bool Add(Tuple<string, object> pair) => Add(pair.Item1, pair.Item2);
    }
}
