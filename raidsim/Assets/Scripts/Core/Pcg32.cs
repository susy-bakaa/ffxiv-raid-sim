using System;

namespace dev.susybaka.raidsim.Core.Rng
{
    /// <summary>
    /// Deterministic PRNG (PCG32).
    /// https://en.wikipedia.org/wiki/Permuted_congruential_generator
    /// </summary>
    public sealed class Pcg32
    {
        private ulong _state;
        private readonly ulong _inc; // must be odd

        public Pcg32(ulong seed, ulong stream = 54UL)
        {
            _state = 0UL;
            _inc = (stream << 1) | 1UL;
            NextUInt();
            _state += seed;
            NextUInt();
        }

        public uint NextUInt()
        {
            ulong oldState = _state;
            _state = oldState * 6364136223846793005UL + _inc;

            uint xorshifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
            int rot = (int)(oldState >> 59);
            return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                throw new ArgumentException("maxExclusive must be > minInclusive");

            uint range = (uint)(maxExclusive - minInclusive);

            // Rejection sampling to avoid modulo bias
            uint threshold = (uint)(-range) % range;
            uint r;
            do
            { r = NextUInt(); } while (r < threshold);

            return (int)(r % range) + minInclusive;
        }

        public bool NextBool() => (NextUInt() & 1u) != 0;

        public float NextFloat01()
        {
            // 24-bit mantissa uniform [0,1)
            return (NextUInt() >> 8) * (1f / (1u << 24));
        }
    }
}