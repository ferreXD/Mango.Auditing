// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    public class AuditingFeaturesConfiguration
    {
        private readonly Dictionary<AuditFeature, bool> _featureStates = new();

        public AuditingFeaturesConfiguration()
        {
            // Opt-in for all features by default (user must explicitly enable them)
            _featureStates[AuditFeature.Auditing] = false;
            _featureStates[AuditFeature.SoftDeletion] = false;
            _featureStates[AuditFeature.Compression] = false;
            _featureStates[AuditFeature.NotificationsScheduling] = false;
            _featureStates[AuditFeature.RealTimeNotifications] = false;
            _featureStates[AuditFeature.Enrichment] = false;
            _featureStates[AuditFeature.TrackUnmodified] = false;
            _featureStates[AuditFeature.IncludeEntityValues] = false;
        }

        public bool IsEnabled(AuditFeature feature) => _featureStates.TryGetValue(feature, out var enabled) && enabled;
        public void SetFeatureState(AuditFeature feature, bool enabled = true) => _featureStates[feature] = enabled;
    }
}