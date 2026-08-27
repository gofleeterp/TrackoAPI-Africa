using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.Reports.ViewModels.Global.Integration
{
    public class ICICIBalanceRequestViewModel: BaseICICIRequest
    {
        public ICICIBalanceRequestViewModel()
        {
            VehicleNumbers = new List<string>();
        }
        public List<string> VehicleNumbers { get; set; }
    }
    public class ICICIBalanceResponseViewModel: BaseICICIRequest
    {
        public ICICIBalanceResponseViewModel()
        {
            VehicleDetails = new List<ICICIBalanceReponseItem>();
        }
        /// <summary>
        /// Returns the available CUG Wallet Balance 
        /// </summary>
        public decimal CUGWalletBalance { get; set; } = 0;
        /// <summary>
        /// Returns Total number of vehicles of the corporate customer.
        /// </summary>
        public int FleetSize { get; set; } = 0;
        /// <summary>
        /// Returns the total number of tags assigned for the vehicles of corporate customer.
        /// </summary>
        public int TotalTagsAssigned { get; set; } = 0;
        /// <summary>
        /// Contains Collection of Vehicle details that need to be returned for the requested vehicle numbers in the request.
        /// </summary>
        public List<ICICIBalanceReponseItem> VehicleDetails { get; set; }

    }

    public class ICICIBalanceReponseItem
    {
        /// <summary>
        /// Returns the available toll balance of the vehicle
        /// </summary>
        public decimal VehicleAvailableBalance { get; set; } = 0;
        /// <summary>
        /// Returns true if the balance of the vehicle is beyond the defined threshold balance else false.
        /// </summary>
        public bool IsLowBalance { get; set; } = false;
        /// <summary>
        /// Account number of vehicle.
        /// </summary>
        public long VehicleAccountNumber { get; set; }
        public ICICIVehicleStatus VehicleStatus { get; set; }
        /// <summary>
        /// Vehicle number
        /// </summary>
        public string VehicleNumber { get; set; }
    }
    public enum ICICIVehicleStatus
    {
        Suspended=0,
        Active =1,
        Closed=2,
        WriteOff=3,
        RefundRequested=4,
        PendingClosed=5,
        Inactive=6
    }
    public class VehicleList
    {
        public int CurrentPageNumber { get; set; }
        public int TotalPages { get; set; }
        public int VehiclesInCurrentPage { get; set; }
        public int TotalVehicles { get; set; }
        public List<ICICIFastTagVehicle> Vehicles { get; set; }
    }
    public class ICICIFastTagVehicle
    {
        public decimal TollBalance { get; set; }
        public string VehicleNumber { get; set; }
        public DateTimeOffset? LastSyncDate { get; set; }        
        public long VehicleId { get; set; }
    }
}
