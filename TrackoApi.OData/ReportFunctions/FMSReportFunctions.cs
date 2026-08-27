using System;
using System.Web.Http;
using System.Web.OData.Builder;
using TrackoApi.Models.AMS;
using TrackoApi.Models.FMS;
using TrackoAPI.Reporting.Models;
using TrackoAPI.Reports.ViewModels.Finance;
using TrackoAPI.Reports.ViewModels.FMS.Driver;
using TrackoAPI.Reports.ViewModels.FMS.Global;
using TrackoAPI.Reports.ViewModels.FMS.Repair;
using TrackoAPI.Reports.ViewModels.FMS.Tyre;

namespace TrackoApi.OData.ReportFunctions
{
    public static class FMSReportFunctions
    {
        public static void Register(ODataConventionModelBuilder builder)
        {
            var procReport =
                builder.EntityType<ReportRequestPool>().Function("GetReport");
            procReport.Returns<string>();
            var procReportv1 =
                builder.EntityType<ReportRequestPool>().Function("GetReportV1");
            procReportv1.Returns<string>();
        }
    }
}
