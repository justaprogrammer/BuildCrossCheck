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
    }

    public class TelemetryService: ITelemetryService
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
    }

    public enum Pages
    {
        Home
    }
}
