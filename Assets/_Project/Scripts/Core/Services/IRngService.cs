using System;
using System.Collections.Generic;

namespace Wastelands.Core.Services
{
    /// <summary>
    /// Deterministic random number generation entry point that exposes
    /// named channels so individual systems can own their entropy stream.
    /// </summary>
    public interface IRngService
    {
        /// <summary>
        /// Seed used for the primary sequence.
        /// </summary>
        int Seed { get; }

        /// <summary>
        /// Resets the RNG to a new seed and clears cached channels.
        /// </summary>
        /// <param name="seed">The seed to apply.</param>
        void Reset(int seed);

        /// <summary>
        /// Returns a deterministic channel for the supplied name. Channels are cached
        /// so repeated requests return the same sequence instance.
        /// </summary>
        /// <param name="name">Channel identifier (e.g. "worldgen").</param>
        /// <returns>A deterministic RNG channel.</returns>
        IRngChannel GetChannel(string name);
    }

    /// <summary>
    /// Represents a deterministic random number generator channel.
    /// </summary>
    public interface IRngChannel
    {
        /// <summary>
        /// Name associated with the channel.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Generates an integer in the [minInclusive, maxExclusive) range.
        /// </summary>
        int NextInt(int minInclusive, int maxExclusive);

        /// <summary>
        /// Generates a floating point value in the [0, 1) range.
        /// </summary>
        double NextDouble();

        /// <summary>
        /// Creates a reproducible state offset to allow deterministic branching.
        /// </summary>
        /// <param name="offset">The offset to apply to the base seed.</param>
        void Reseed(int offset);
    }

    /// <summary>
    /// Default deterministic implementation built on System.Random.
    /// </summary>
    public sealed class DeterministicRngService : IRngService
    {
        private readonly Dictionary<string, DeterministicChannel> _channels = new();
        private int _seed;

        public DeterministicRngService(int seed)
        {
            _seed = seed;
        }

        public int Seed => _seed;

        public IRngChannel GetChannel(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Channel name must be provided.", nameof(name));
            }

            if (_channels.TryGetValue(name, out var channel))
            {
                return channel;
            }

            channel = new DeterministicChannel(name, _seed);
            _channels[name] = channel;
            return channel;
        }

        public void Reset(int seed)
        {
            _seed = seed;
            _channels.Clear();
        }

        private sealed class DeterministicChannel : IRngChannel
        {
            private readonly string _name;
            private readonly int _baseSeed;
            private Random _random;

            public DeterministicChannel(string name, int seed)
            {
                _name = name;
                _baseSeed = HashSeed(name, seed);
                _random = new Random(_baseSeed);
            }

            public string Name => _name;

            public int NextInt(int minInclusive, int maxExclusive)
            {
                if (maxExclusive <= minInclusive)
                {
                    throw new ArgumentException("maxExclusive must be greater than minInclusive.");
                }

                return _random.Next(minInclusive, maxExclusive);
            }

            public double NextDouble() => _random.NextDouble();

            public void Reseed(int offset)
            {
                _random = new Random(unchecked(_baseSeed + offset));
            }

            private static int HashSeed(string key, int seed)
            {
                unchecked
                {
                    var hash = 17;
                    hash = hash * 31 + seed;
                    hash = hash * 31 + key.GetHashCode(StringComparison.Ordinal);
                    return hash;
                }
            }
        }
    }
}
