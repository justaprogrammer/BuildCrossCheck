using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace BCC.Web.Services
{
    public interface ITelemetryService
    {
        void TrackPageView(Pages page);
        void CreateToken(string user);
        void RevokeToken(string user);
        void CreateCheckRun(string repositoryOwner, string repositoryName);
    }

    public class TelemetryService : ITelemetryService
    {
        private readonly TelemetryClient _telemetryClient;

        public TelemetryService(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void TrackPageView(Pages page)
        {
            _telemetryClient.TrackPageView(page.ToString());
        }

        private void TrackEvent(string eventName,
            IDictionary<string, string> properties = null,
            IDictionary<string, double> metrics = null)
        {
            _telemetryClient.TrackEvent(eventName, properties, metrics);
        }

        public void CreateToken(string user)
        {
            TrackEvent("CreateToken", new Dictionary<string, string>
            {
                {"User", user}
            });

            _telemetryClient.GetMetric("CreateToken").TrackValue(1);
        }

        public void RevokeToken(string user)
        {
            TrackEvent("RevokeToken", new Dictionary<string, string>
            {
                {"User", user}
            });

            _telemetryClient.GetMetric("RevokeToken").TrackValue(1);
        }

        public void CreateCheckRun(string repositoryOwner, string repositoryName)
        {
            TrackEvent("CreateCheckRun", new Dictionary<string, string>
            {
                {"RepositoryOwner", repositoryOwner},
                {"RepositoryName", repositoryName}
            });

            _telemetryClient.GetMetric("CreateCheckRun").TrackValue(1);
        }
    }

    public enum Pages
    {
        Home,
        Error,
        SignIn,
        SignOut,
        ListRepositories
    }
}
