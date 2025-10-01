using System;
using System.IO;
using Wastelands.Core.Data;

namespace Wastelands.Persistence
{
    public interface IOverworldSnapshotGateway
    {
        string SaveToString(WorldData world);
        void SaveToStream(WorldData world, Stream destination);
        void SaveToFile(WorldData world, string filePath);
        WorldData LoadFromString(string json);
        WorldData LoadFromStream(Stream source);
        WorldData LoadFromFile(string filePath);
    }

    /// <summary>
    /// Entry point for persisting overworld snapshots through <see cref="WorldDataSerializer"/>.
    /// </summary>
    public sealed class OverworldSnapshotGateway : IOverworldSnapshotGateway
    {
        private readonly IWorldDataSerializer _serializer;

        public OverworldSnapshotGateway(IWorldDataSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public string SaveToString(WorldData world)
        {
            return _serializer.Serialize(world);
        }

        public void SaveToStream(WorldData world, Stream destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            _serializer.SerializeToStream(world, destination);
            if (destination.CanSeek)
            {
                destination.Seek(0, SeekOrigin.Begin);
            }
        }

        public void SaveToFile(WorldData world, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path must be provided.", nameof(filePath));
            }

            using var file = File.Create(filePath);
            SaveToStream(world, file);
        }

        public WorldData LoadFromString(string json)
        {
            return _serializer.Deserialize(json);
        }

        public WorldData LoadFromStream(Stream source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return _serializer.DeserializeFromStream(source);
        }

        public WorldData LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path must be provided.", nameof(filePath));
            }

            using var file = File.OpenRead(filePath);
            return LoadFromStream(file);
        }
    }
}
