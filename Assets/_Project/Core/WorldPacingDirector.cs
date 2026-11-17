using System;

namespace Signal.Core
{
    /// <summary>
    /// Shared gate for consequential world developments. It never creates or queues
    /// developments; a caller first establishes that a candidate is contextually valid,
    /// then asks this director for permission to resolve it.
    /// </summary>
    public sealed class WorldPacingDirector
    {
        private readonly int _minimumIntervalMinutes;
        private readonly int _maximumIntervalMinutes;
        private readonly int _rampDurationMinutes;

        private string _scenarioPhaseId;
        private long _scenarioPhaseStartedAt;
        private long _lastAuthorizedAt;
        private long _lastObservedAt;

        public bool HasScenarioPhase => !string.IsNullOrEmpty(_scenarioPhaseId);
        public string ScenarioPhaseId => _scenarioPhaseId;
        public int CurrentIntervalMinutes
        {
            get
            {
                if (!HasScenarioPhase)
                    return _minimumIntervalMinutes;

                long elapsed = Math.Max(0L, _lastObservedAt - _scenarioPhaseStartedAt);
                double progress = _rampDurationMinutes <= 0
                    ? 1d
                    : Math.Min(1d, elapsed / (double)_rampDurationMinutes);
                return (int)Math.Round(
                    _minimumIntervalMinutes +
                    (_maximumIntervalMinutes - _minimumIntervalMinutes) * progress,
                    MidpointRounding.AwayFromZero);
            }
        }

        public WorldPacingDirector(int minimumIntervalMinutes,
            int maximumIntervalMinutes, int rampDurationMinutes)
        {
            if (minimumIntervalMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimumIntervalMinutes));
            if (maximumIntervalMinutes < minimumIntervalMinutes)
                throw new ArgumentOutOfRangeException(nameof(maximumIntervalMinutes));
            if (rampDurationMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(rampDurationMinutes));

            _minimumIntervalMinutes = minimumIntervalMinutes;
            _maximumIntervalMinutes = maximumIntervalMinutes;
            _rampDurationMinutes = rampDurationMinutes;
        }

        /// <summary>
        /// Sets the active authored scenario phase. Re-applying the same phase is a no-op.
        /// A new phase starts a fresh restraint ramp, but still requires its initial quiet
        /// interval; unused permission from an earlier phase is never banked.
        /// </summary>
        public bool SetScenarioPhase(string phaseId, int day, int minuteOfDay)
        {
            if (string.IsNullOrWhiteSpace(phaseId))
                throw new ArgumentException("A scenario phase id is required.", nameof(phaseId));

            long now = ToAbsoluteMinute(day, minuteOfDay);
            if (HasScenarioPhase && now < _lastObservedAt)
                return false;
            if (_scenarioPhaseId == phaseId)
            {
                _lastObservedAt = Math.Max(_lastObservedAt, now);
                return false;
            }

            _scenarioPhaseId = phaseId;
            _scenarioPhaseStartedAt = now;
            _lastAuthorizedAt = now;
            _lastObservedAt = now;
            return true;
        }

        /// <summary>
        /// Consumes the one shared development permission when the caller has already
        /// established valid circumstances. Invalid candidates do not consume pacing.
        /// Only one candidate can pass per interval, regardless of its system/category.
        /// </summary>
        public bool TryAuthorizeDevelopment(bool hasValidContext, int day, int minuteOfDay)
        {
            if (!hasValidContext || !HasScenarioPhase)
                return false;

            long now = ToAbsoluteMinute(day, minuteOfDay);
            if (now < _lastObservedAt)
                return false;

            _lastObservedAt = now;
            long phaseElapsed = Math.Max(0L, now - _scenarioPhaseStartedAt);
            double progress = Math.Min(1d, phaseElapsed / (double)_rampDurationMinutes);
            int interval = (int)Math.Round(
                _minimumIntervalMinutes +
                (_maximumIntervalMinutes - _minimumIntervalMinutes) * progress,
                MidpointRounding.AwayFromZero);

            long quietSince = Math.Max(_lastAuthorizedAt, _scenarioPhaseStartedAt);
            if (now - quietSince < interval)
                return false;

            _lastAuthorizedAt = now;
            return true;
        }

        private static long ToAbsoluteMinute(int day, int minuteOfDay)
        {
            if (day < 1)
                throw new ArgumentOutOfRangeException(nameof(day));
            if (minuteOfDay < 0 || minuteOfDay >= 1440)
                throw new ArgumentOutOfRangeException(nameof(minuteOfDay));

            return ((long)day - 1L) * 1440L + minuteOfDay;
        }
    }
}
