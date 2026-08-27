using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace TrackoAPI.WebUtilities.InsightProcessors
{
    public class FilterSQLTelemetryProcessor: ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        public FilterSQLTelemetryProcessor(ITelemetryProcessor processor)
        {
            Next = processor;
        }
        public void Process(ITelemetry item)
        {
            if (IsSQLDependency(item)) { return; }
            this.Next.Process(item);
        }
        private bool IsSQLDependency(ITelemetry item)
        {
            var dependency = item as DependencyTelemetry;
            if (dependency?.Type == "SQL")
            {
                return true;
            }
            return false;
        }
    }
}
