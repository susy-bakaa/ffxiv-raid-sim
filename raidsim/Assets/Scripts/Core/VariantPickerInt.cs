using System;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Core.Rng
{
    /// <summary>
    /// Per-key picker with selectable behavior:
    /// - PureRandom
    /// - NoRepeatConsecutive
    /// - ShuffleBag (random without replacement; reshuffle each cycle)
    /// </summary>
    public sealed class VariantPickerInt
    {
        private readonly Pcg32 _rng;
        private RngMode _mode;

        private int _count;

        // NoRepeatConsecutive
        private bool _hasLast;
        private int _last;

        // ShuffleBag
        private int[] _bag = Array.Empty<int>();
        private int _index;

        public VariantPickerInt(Pcg32 rng, int count, RngMode mode)
        {
            _rng = rng;
            Configure(count, mode);
        }

        public void Configure(int count, RngMode mode)
        {
            if (count <= 0)
                throw new ArgumentException("count must be > 0");
            if (mode == RngMode.NoRepeatConsecutive && count < 2)
                throw new ArgumentException("NoRepeatConsecutive needs at least 2 options.");

            bool countChanged = _count != count;
            bool modeChanged = _mode != mode;

            _count = count;
            _mode = mode;

            if (mode == RngMode.ShuffleBag && (countChanged || _bag.Length != count))
            {
                _bag = new int[count];
                RefillAndShuffle();
            }

            if (modeChanged && mode == RngMode.NoRepeatConsecutive)
            {
                _hasLast = false;
                _last = default;
            }

            if (modeChanged && mode == RngMode.ShuffleBag)
            {
                RefillAndShuffle();
            }
        }

        public int Next()
        {
            return _mode switch
            {
                RngMode.PureRandom => _rng.NextInt(0, _count),
                RngMode.NoRepeatConsecutive => NextNoRepeat(),
                RngMode.ShuffleBag => NextFromBag(),
                _ => _rng.NextInt(0, _count)
            };
        }

        private int NextNoRepeat()
        {
            int v;
            do
            { v = _rng.NextInt(0, _count); }
            while (_hasLast && v == _last);

            _last = v;
            _hasLast = true;
            return v;
        }

        private int NextFromBag()
        {
            if (_index >= _bag.Length)
                RefillAndShuffle();

            return _bag[_index++];
        }

        private void RefillAndShuffle()
        {
            for (int i = 0; i < _bag.Length; i++)
                _bag[i] = i;

            // Fisher–Yates
            for (int i = _bag.Length - 1; i > 0; i--)
            {
                int j = _rng.NextInt(0, i + 1);
                (_bag[i], _bag[j]) = (_bag[j], _bag[i]);
            }

            _index = 0;
        }
    }
}