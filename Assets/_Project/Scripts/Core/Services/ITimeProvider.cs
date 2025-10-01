using System;

namespace Wastelands.Core.Services
{
    /// <summary>
    /// Provides deterministic time progression for overworld and base mode ticks.
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// Current absolute tick value.
        /// </summary>
        long CurrentTick { get; }

        /// <summary>
        /// Advances the tick counter by the supplied amount.
        /// </summary>
        /// <param name="ticks">Number of ticks to advance.</param>
        void AdvanceTicks(long ticks);

        /// <summary>
        /// Resets the tick counter to the provided absolute value.
        /// </summary>
        /// <param name="tick">Tick to set.</param>
        void SetTick(long tick);

        /// <summary>
        /// Converts overworld years into base mode daily ticks.
        /// </summary>
        long ConvertYearsToDailyTicks(int years);

        /// <summary>
        /// Converts base daily ticks into overworld years.
        /// </summary>
        int ConvertDailyTicksToYears(long dailyTicks);
    }

    /// <summary>
    /// Manual implementation used for deterministic tests and bootstrap flows.
    /// </summary>
    public sealed class ManualTimeProvider : ITimeProvider
    {
        private readonly int _ticksPerYear;
        private readonly long _ticksPerDay;
        private long _currentTick;

        public ManualTimeProvider(int ticksPerYear = 1, long ticksPerDay = 24)
        {
            if (ticksPerYear <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ticksPerYear));
            }

            if (ticksPerDay <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ticksPerDay));
            }

            _ticksPerYear = ticksPerYear;
            _ticksPerDay = ticksPerDay;
        }

        public long CurrentTick => _currentTick;

        public void AdvanceTicks(long ticks)
        {
            if (ticks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ticks));
            }

            _currentTick += ticks;
        }

        public void SetTick(long tick)
        {
            if (tick < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tick));
            }

            _currentTick = tick;
        }

        public long ConvertYearsToDailyTicks(int years)
        {
            if (years < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(years));
            }

            return years * _ticksPerYear * _ticksPerDay;
        }

        public int ConvertDailyTicksToYears(long dailyTicks)
        {
            if (dailyTicks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dailyTicks));
            }

            var ticksPerYear = (long)_ticksPerYear * _ticksPerDay;
            return (int)(dailyTicks / ticksPerYear);
        }
    }
}
