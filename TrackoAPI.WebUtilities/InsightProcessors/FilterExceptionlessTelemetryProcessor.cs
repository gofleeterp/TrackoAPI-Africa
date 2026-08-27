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
    public class FilterExceptionlessTelemetryProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        public FilterExceptionlessTelemetryProcessor(ITelemetryProcessor processor)
        {
            Next = processor;
        }
        public void Process(ITelemetry item)
        {
            if (IsExceptionlessDependency(item)) { return; }
            this.Next.Process(item);
        }
        private bool IsExceptionlessDependency(ITelemetry item)
        {
            var dependency = item as DependencyTelemetry;
            if (dependency?.Name == "http://exceptions.indiaweblab.com"&&dependency.Type=="HTTP")
            {
                return true;
            }
            return false;
        }
        
        //private bool IsBEDependency(ITelemetry item)
        //{
        //    var dependency = item as DependencyTelemetry;
        //    if (dependency?.Name=="TrackoApi.Core.Helpers.BusinessException" && dependency?.Type == "exception")
        //    {
        //        return true;
        //    }
        //    return false;
        //}
    }
}
