using System;

namespace Signal.Core
{
    public enum OpportunityPhase
    {
        Dormant = 0,
        Available = 1,
        Accepted = 2,
        Resolved = 3
    }

    public enum OpportunityResolution
    {
        None = 0,
        Delivered = 1
    }

    /// <summary>
    /// Small runtime lifecycle for one opportunity. Declining does not fail it;
    /// returning a parcel releases the commitment and makes the offer available again.
    /// </summary>
    public sealed class OpportunityState
    {
        public string OpportunityId { get; }
        public OpportunityPhase Phase { get; private set; }
        public OpportunityResolution Resolution { get; private set; }
        public Guid ParcelInstanceId { get; private set; }
        public int AcceptanceCount { get; private set; }
        public int DeclineCount { get; private set; }
        public int LastAcceptedDay { get; private set; }
        public int LastAcceptedMinuteOfDay { get; private set; }
        public int LastDeclinedDay { get; private set; }
        public int LastDeclinedMinuteOfDay { get; private set; }
        public int ResolvedDay { get; private set; }
        public int ResolvedMinuteOfDay { get; private set; }
        public bool HasDeliveryAssessment { get; private set; }
        public bool DeliveredOnTime { get; private set; }
        public bool DeliveredIntact { get; private set; }
        public bool DeliveryWitnessed { get; private set; }

        public OpportunityState(string opportunityId)
        {
            if (string.IsNullOrWhiteSpace(opportunityId))
                throw new ArgumentException("An opportunity id is required.", nameof(opportunityId));
            OpportunityId = opportunityId;
            Phase = OpportunityPhase.Dormant;
        }

        public bool MakeAvailable()
        {
            if (Phase != OpportunityPhase.Dormant)
                return false;
            Phase = OpportunityPhase.Available;
            return true;
        }

        public bool RecordDecline(int day, int minuteOfDay)
        {
            ValidateTime(day, minuteOfDay);
            if (Phase != OpportunityPhase.Available)
                return false;
            DeclineCount++;
            LastDeclinedDay = day;
            LastDeclinedMinuteOfDay = minuteOfDay;
            return true;
        }

        public bool SetParcelInstanceId(Guid parcelInstanceId)
        {
            if (parcelInstanceId == Guid.Empty ||
                (Phase != OpportunityPhase.Dormant && Phase != OpportunityPhase.Available))
                return false;
            if (ParcelInstanceId != Guid.Empty && ParcelInstanceId != parcelInstanceId)
                return false;

            ParcelInstanceId = parcelInstanceId;
            return true;
        }

        public bool TryAccept(Guid parcelInstanceId, int day, int minuteOfDay)
        {
            ValidateTime(day, minuteOfDay);
            if (Phase != OpportunityPhase.Available || parcelInstanceId == Guid.Empty ||
                (ParcelInstanceId != Guid.Empty && ParcelInstanceId != parcelInstanceId))
                return false;

            ParcelInstanceId = parcelInstanceId;
            AcceptanceCount++;
            LastAcceptedDay = day;
            LastAcceptedMinuteOfDay = minuteOfDay;
            Phase = OpportunityPhase.Accepted;
            return true;
        }

        public bool TryReturnParcel(int day, int minuteOfDay)
        {
            ValidateTime(day, minuteOfDay);
            if (Phase != OpportunityPhase.Accepted)
                return false;

            Phase = OpportunityPhase.Available;
            return true;
        }

        public bool TryResolveDelivered(int day, int minuteOfDay)
        {
            return TryResolveDelivered(day, minuteOfDay, true, true, false);
        }

        public bool TryResolveDelivered(int day, int minuteOfDay,
            bool deliveredOnTime, bool deliveredIntact, bool deliveryWitnessed)
        {
            ValidateTime(day, minuteOfDay);
            if (Phase != OpportunityPhase.Accepted)
                return false;

            Phase = OpportunityPhase.Resolved;
            Resolution = OpportunityResolution.Delivered;
            ResolvedDay = day;
            ResolvedMinuteOfDay = minuteOfDay;
            HasDeliveryAssessment = true;
            DeliveredOnTime = deliveredOnTime;
            DeliveredIntact = deliveredIntact;
            DeliveryWitnessed = deliveryWitnessed;
            return true;
        }

        public bool TryExpire()
        {
            if (Phase != OpportunityPhase.Available)
                return false;
            Phase = OpportunityPhase.Dormant;
            return true;
        }

        public static bool TryRestore(string opportunityId, OpportunityPhase phase,
            OpportunityResolution resolution, Guid parcelInstanceId, int acceptanceCount, int declineCount,
            int lastAcceptedDay, int lastAcceptedMinuteOfDay, int lastDeclinedDay,
            int lastDeclinedMinuteOfDay, int resolvedDay, int resolvedMinuteOfDay,
            bool hasDeliveryAssessment, bool deliveredOnTime, bool deliveredIntact, bool deliveryWitnessed,
            out OpportunityState restored)
        {
            restored = null;
            if (string.IsNullOrWhiteSpace(opportunityId) ||
                !Enum.IsDefined(typeof(OpportunityPhase), phase) ||
                !Enum.IsDefined(typeof(OpportunityResolution), resolution) ||
                acceptanceCount < 0 || declineCount < 0)
                return false;

            if (!ValidOptionalTime(lastAcceptedDay, lastAcceptedMinuteOfDay) ||
                !ValidOptionalTime(lastDeclinedDay, lastDeclinedMinuteOfDay) ||
                !ValidOptionalTime(resolvedDay, resolvedMinuteOfDay))
                return false;

            if ((phase == OpportunityPhase.Accepted || phase == OpportunityPhase.Resolved) &&
                (parcelInstanceId == Guid.Empty || acceptanceCount < 1))
                return false;
            if (phase == OpportunityPhase.Resolved &&
                (resolution != OpportunityResolution.Delivered || resolvedDay < 1))
                return false;
            if (phase != OpportunityPhase.Resolved && resolution != OpportunityResolution.None)
                return false;
            restored = new OpportunityState(opportunityId)
            {
                Phase = phase,
                Resolution = resolution,
                ParcelInstanceId = parcelInstanceId,
                AcceptanceCount = acceptanceCount,
                DeclineCount = declineCount,
                LastAcceptedDay = lastAcceptedDay,
                LastAcceptedMinuteOfDay = lastAcceptedMinuteOfDay,
                LastDeclinedDay = lastDeclinedDay,
                LastDeclinedMinuteOfDay = lastDeclinedMinuteOfDay,
                ResolvedDay = resolvedDay,
                ResolvedMinuteOfDay = resolvedMinuteOfDay,
                HasDeliveryAssessment = hasDeliveryAssessment,
                DeliveredOnTime = deliveredOnTime,
                DeliveredIntact = deliveredIntact,
                DeliveryWitnessed = deliveryWitnessed
            };
            return true;
        }

        private static bool ValidOptionalTime(int day, int minuteOfDay)
        {
            return day == 0 ? minuteOfDay == 0 : day > 0 && minuteOfDay >= 0 && minuteOfDay < 1440;
        }

        private static void ValidateTime(int day, int minuteOfDay)
        {
            if (day < 1) throw new ArgumentOutOfRangeException(nameof(day));
            if (minuteOfDay < 0 || minuteOfDay >= 1440)
                throw new ArgumentOutOfRangeException(nameof(minuteOfDay));
        }
    }
}
