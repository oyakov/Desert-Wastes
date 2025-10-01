using System;
using System.IO;
using NUnit.Framework;
using Wastelands.Persistence;

namespace Wastelands.Tests.EditMode
{
    public class OverworldSnapshotGatewayTests
    {
        [Test]
        public void SaveAndLoadString_RoundTripsWorld()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            var gateway = new OverworldSnapshotGateway(new WorldDataSerializer());

            var json = gateway.SaveToString(world);
            var loaded = gateway.LoadFromString(json);

            var serializer = new WorldDataSerializer();
            Assert.AreEqual(json, serializer.Serialize(loaded));
        }

        [Test]
        public void SaveAndLoadStream_RoundTripsWorld()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            var gateway = new OverworldSnapshotGateway(new WorldDataSerializer());

            using var stream = new MemoryStream();
            gateway.SaveToStream(world, stream);
            var loaded = gateway.LoadFromStream(stream);

            var serializer = new WorldDataSerializer();
            Assert.AreEqual(serializer.Serialize(world), serializer.Serialize(loaded));
        }

        [Test]
        public void SaveAndLoadFile_RoundTripsWorld()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            var gateway = new OverworldSnapshotGateway(new WorldDataSerializer());

            var path = Path.Combine(Path.GetTempPath(), $"wastelands_{Guid.NewGuid():N}.json");
            try
            {
                gateway.SaveToFile(world, path);
                var loaded = gateway.LoadFromFile(path);

                var serializer = new WorldDataSerializer();
                Assert.AreEqual(serializer.Serialize(world), serializer.Serialize(loaded));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
