// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Core.Rng
{
    /// <summary>
    /// One service per simulation run.
    /// - Seeded for deterministic runs (Frozen RNG)
    /// - Provides independent RNG streams per key
    /// - Provides per-key VariantPickers (pure / no-repeat / shuffle-bag)
    /// </summary>
    public sealed class RngService
    {
        public ulong Seed { get; private set; }

        private readonly Dictionary<string, Pcg32> _streams = new();
        private readonly Dictionary<string, VariantPickerInt> _pickers = new();

        public RngService(ulong seed)
        {
            Seed = seed;
        }

        public void Reset(ulong seed)
        {
            Seed = seed;
            _streams.Clear();
            _pickers.Clear();
        }

        /// <summary>
        /// Get a dedicated RNG stream for a given key (mechanic/system).
        /// This prevents "loot roll changed my safe-side roll" type coupling.
        /// </summary>
        public Pcg32 Stream(string key)
        {
            if (_streams.TryGetValue(key, out var rng))
                return rng;

            ulong k = Fnv1a64(key);
            // seed & stream are derived deterministically from (Seed, key)
            ulong derivedSeed = Seed ^ Mix64(k);
            ulong derivedStream = (Mix64(k ^ 0x9E3779B97F4A7C15UL) | 1UL);

            rng = new Pcg32(derivedSeed, derivedStream);
            _streams[key] = rng;
            return rng;
        }

        /// <summary>
        /// Pick an int in [0, optionCount) according to mode.
        /// Cached per key, so each key maintains its own "no-repeat"/"bag" state.
        /// </summary>
        public int Pick(string key, int optionCount, RngMode mode)
        {
            if (!_pickers.TryGetValue(key, out var picker))
            {
                picker = new VariantPickerInt(Stream(key), optionCount, mode);
                _pickers[key] = picker;
                return picker.Next();
            }

            picker.Configure(optionCount, mode);
            return picker.Next();
        }

        public float Value01(string key)
        {
            return Stream(key).NextFloat01();
        }

        public float Range(string key, float minInclusive, float maxInclusive)
        {
            return minInclusive + (maxInclusive - minInclusive) * Stream(key).NextFloat01();
        }

        // Stable hashing/bit-mixing (Do not use string.GetHashCode as it is NOT stable)
        private static ulong Fnv1a64(string s)
        {
            unchecked
            {
                const ulong offset = 14695981039346656037UL;
                const ulong prime = 1099511628211UL;

                ulong hash = offset;
                for (int i = 0; i < s.Length; i++)
                {
                    // Hash UTF-16 chars deterministically (2 bytes)
                    char c = s[i];
                    hash ^= (byte)c;
                    hash *= prime;
                    hash ^= (byte)(c >> 8);
                    hash *= prime;
                }
                return hash;
            }
        }

        private static ulong Mix64(ulong x)
        {
            // SplitMix64 finalizer
            x ^= x >> 30;
            x *= 0xBF58476D1CE4E5B9UL;
            x ^= x >> 27;
            x *= 0x94D049BB133111EBUL;
            x ^= x >> 31;
            return x;
        }
    }
}