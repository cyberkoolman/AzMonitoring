using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceHealthAlertPOC
{
    public class RecentlyResolvedState
    {
        public DateTime unavailableOccurredTime { get; set; }
        public DateTime resolvedTime { get; set; }
        public string unavailabilitySummary { get; set; }
    }

    public class RecommendedAction
    {
        public string action { get; set; }
        public string actionUrl { get; set; }
        public string actionUrlText { get; set; }
    }

    public class Properties
    {
        public string availabilityState { get; set; }
        public string summary { get; set; }
        public string reasonType { get; set; }
        public string reasonChronicity { get; set; }
        public string detailedStatus { get; set; }
        public DateTime occuredTime { get; set; }
        public DateTime reportedTime { get; set; }
        public RecentlyResolvedState recentlyResolvedState { get; set; }
        public List<RecommendedAction> recommendedActions { get; set; }
        public DateTime? rootCauseAttributionTime { get; set; }
        public DateTime? resolutionETA { get; set; }
    }

    public class AvailabilityStatus
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string location { get; set; }
        public Properties properties { get; set; }
    }

    public class AvailabilityStatusListResult    
    {
        public List<AvailabilityStatus> value { get; set; }
        public string nextLink { get; set; }
    }
}