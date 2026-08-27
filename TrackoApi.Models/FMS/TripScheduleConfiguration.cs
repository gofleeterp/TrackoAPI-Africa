// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TripScheduleConfiguration.cs" company="India WebLab Technologies Pvt Ltd">
//   Copyright 2016-2019 @India WebLab technologies
// </copyright>
// <summary>
//   The trip schedule configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using TrackoApi.Models.BMS;

namespace TrackoApi.Models.FMS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AMS;
    using Base;

    using CronExpressionDescriptor;

    using Newtonsoft.Json;

    using TrackoApi.Models.Global;

    /// <summary>
    /// The trip schedule configuration.
    /// </summary>
    [Table("tTripScheduleConfig")]
    public class TripScheduleConfiguration : AuditableEntity,IValidatableObject
    {
        public TripScheduleConfiguration()
        {
            
        }
        public long? RouteId { get; set; }
        [ForeignKey("RouteId")]
        public virtual RouteMaster fk_Route { get; set; }

        public long? ConsignorId { get; set; }
        [ForeignKey("ConsignorId")]
        public virtual Ledger fk_Consignor { get; set; }

        public long? InchargeId { get; set; }
        [ForeignKey("InchargeId")]
        public virtual GenericMaster fk_Incharge { get; set; }

        public TimeSpan? PlacementTime { get; set; }
        public TimeSpan? DepartureTime { get; set; }
        public CronViewModel Cron { get; set; }

        public long? LoadTypeId { get; set; }
        [ForeignKey("LoadTypeId")]
        public virtual LoadType fk_LoadType { get; set; }

        public long? VehicleTypeId { get; set; }
        [ForeignKey("VehicleTypeId")]
        public virtual GenericMaster fk_VehicleType { get; set; }

        [Column("InTimeCronExp")]
        public string CronExpression { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CronDescription { get; set; }

        public string IsCronValid(string expression)
        {
            string message = "";
            try
            {
                CronDescription = MyCron.GetDescription(expression, true);
            }
            catch (Exception e)
            {
                this.CronExpression = "";
                this.CronDescription = "";
                message = e.GetBaseException().Message;
            }
            
            return message;
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var validationResult in ValidateLogic()) yield return validationResult;
        }

        public IEnumerable<ValidationResult> ValidateLogic()
        {
            if (!string.IsNullOrWhiteSpace(this.CronExpression))
            {
                string message = IsCronValid(this.CronExpression);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    yield return new ValidationResult(message);
                }
            }
            if (Cron != null && string.IsNullOrWhiteSpace(CronExpression))
            {
                this.CronExpression = Cron.ToString();
                string message = IsCronValid(this.CronExpression);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    yield return new ValidationResult(message);
                }
            }

            if (PlacementTime != null && string.IsNullOrWhiteSpace(CronExpression))
            {
                var val = PlacementTime.GetValueOrDefault();
                this.CronExpression = MyCron.Daily(val.Hours, val.Minutes);
                string message = IsCronValid(this.CronExpression);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    yield return new ValidationResult(message);
                }
            }

            if (string.IsNullOrWhiteSpace(CronExpression))
            {
                yield return new ValidationResult("Schedule Interval or Cron Expression is Required");
            }

            if (PlacementTime != null || string.IsNullOrWhiteSpace(this.CronExpression)) yield break;
            var parts = this.CronExpression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int.TryParse(parts[0], out var minute);
            int.TryParse(parts[1], out var hour);
            PlacementTime = new TimeSpan(hour, minute, 0);
        }
    }

}
