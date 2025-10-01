using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Wastelands.Core.Data;

namespace Wastelands.Persistence
{
    public interface IWorldDataSerializer
    {
        string Serialize(WorldData data);
        void SerializeToStream(WorldData data, Stream stream);
        WorldData Deserialize(string json);
        WorldData DeserializeFromStream(Stream stream);
    }

    /// <summary>
    /// JSON serializer for <see cref="WorldData"/> snapshots ensuring deterministic ordering.
    /// </summary>
    public sealed class WorldDataSerializer : IWorldDataSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        public string Serialize(WorldData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            WorldDataNormalizer.Normalize(data);
            return JsonSerializer.Serialize(data, Options);
        }

        public void SerializeToStream(WorldData data, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var json = Serialize(data);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true);
            writer.Write(json);
            writer.Flush();
        }

        public WorldData Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON payload must be provided.", nameof(json));
            }

            var world = JsonSerializer.Deserialize<WorldData>(json, Options) ?? throw new InvalidOperationException("Failed to deserialize world data.");
            WorldDataNormalizer.Normalize(world);
            return world;
        }

        public WorldData DeserializeFromStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var json = reader.ReadToEnd();
            return Deserialize(json);
        }
    }
}
