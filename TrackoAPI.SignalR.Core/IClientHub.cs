using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoAPI.SignalR.Core
{
    public interface IClientHub
    {
        Task SyncTransaction(ApiPubSubStore message);
        void IntimateVTSStatusChangeForTripLog(VTSStatusLog vts);
        Task JoinVehicleVTSGroup(long vehicleId);
        Task JoinGroup(string tenantId);
        Task LeaveGroup(string tenantId);
        void BroadCastMessageToConnection(string connection, string message, string title);
        void BroadCastMessageToSessionId(long sessionId, string message, string title);
        Task OnConnected();
        Task OnDisconnected(bool stopCalled);
        Task OnReconnected();
        void Dispose();
        Task PushEvent(string eventName, string data);

        Task PushEvent(string eventName, ApiPubSubStore message, bool sendDataAsParameter = true,
            bool trrigerEventNow = true, string groupName = "");

        Task CheckInRoom(string roomName);
        Task CheckOutRoom(string roomName);
        /// <summary>
        /// Send Event to Self
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="type">1:Information 2:Warning 3:Error</param>
        void PushEventSelf(string connectionId,string message, string title, PushSelfMessageType type);
    }
    public enum PushSelfMessageType
    {
        Information = 1,
        Warning = 2,
        Error = 3
    }
}
