using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoAPI.Hubs
{
    //public interface IClientHub
    //{
    //    Task SyncTransaction(ApiPubSubStore message);
    //    void IntimateVTSStatusChangeForTripLog(VTSStatusLog vts);
    //    Task JoinVehicleVTSGroup(long vehicleId);
    //    Task JoinGroup(string tenantId);
    //    Task LeaveGroup(string tenantId);
    //    void BroadCastMessageToConnection(string connection, string message, string title);
    //    void BroadCastMessageToSessionId(long sessionId, string message, string title);
    //    Task OnConnected();
    //    Task OnDisconnected(bool stopCalled);
    //    Task OnReconnected();
    //    IHubCallerConnectionContext<dynamic> Clients { get; set; }
    //    HubCallerContext Context { get; set; }
    //    IGroupManager Groups { get; set; }
    //    void Dispose();
    //    Task PushEvent(string eventName, string data);

    //    Task PushEvent(string eventName, ApiPubSubStore message, bool sendDataAsParameter = true,
    //        bool trrigerEventNow = true, string groupName = "");

    //    Task CheckInRoom(string roomName);
    //    Task CheckOutRoom(string roomName);
    //}
    public interface IClientPatchUpdateHub
    {
        Task JoinGroup(string tenantId);
        Task LeaveGroup(string tenantId);
        void BroadCastMessageToConnection(string connection, string message, string title);
        void BroadCastMessageToSessionId(long sessionId, string message, string title);
        Task OnConnected();
        Task OnDisconnected(bool stopCalled);
        Task OnReconnected();
        IHubCallerConnectionContext<dynamic> Clients { get; set; }
        HubCallerContext Context { get; set; }
        IGroupManager Groups { get; set; }
        void Dispose();
    }
}