using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Json;
using Newtonsoft.Json;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Data;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.SignalR.Core;

namespace TrackoAPI.Hubs
{
    [Authorize,HubName("ClientHub")]
    public class ClientHub : Hub, IClientHub
    {
        private readonly IGlobalStore _gs;

        //private IUnitOfWorkAsync _uow;
        private readonly ITrackoApiDbContext _db;

        public ClientHub(IGlobalStore globalStore)
        {
            // var id=Helper.LoggedInTenantId();
            _gs = globalStore;
            _db = new TrackoApiDbContext(globalStore);
        }
        
        [HubMethodName("NewPatchReleased")]
        public static void NewPatchReleased(string tenantId)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ClientHub>();
            context.Clients.Group(tenantId).CheckPatchUpdate();
        }

        [HubMethodName("pushevent")]
        public async Task PushEvent(string eventName,string data)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ClientHub>();
            IClientProxy  proxy=context.Clients.Group(Helper.LoggedInTenantClientKey);
            await proxy.Invoke(eventName, data);
        }

        [HubMethodName("PushEvent")]
        public async Task PushEvent(string eventName, ApiPubSubStore message,bool sendDataAsParameter=true,bool trrigerEventNow=true,string groupName="")
        {
            message.SenderId = Helper.GetLoggedInUserId();
            var users=await _db.UserConnections.Select(x => x.UserId).Distinct().ToListAsync();
            foreach (var user in users)
            {
                var newMessage = message.Clone();
                newMessage.ReceiverId = user;
                //repo.Insert(newMessage);
                _db.ApiPubSubStores.AddOrUpdate(newMessage);
            }
            await _db.SaveChangesAsync();
            if (trrigerEventNow)
            {
                IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ClientHub>();
                IClientProxy proxy = context.Clients.Group(Helper.LoggedInTenantClientKey+ (string.IsNullOrWhiteSpace(groupName) ? "" : groupName));
                if (sendDataAsParameter)
                {
                    await proxy.Invoke(eventName, message);
                }
                else
                {
                    await proxy.Invoke(eventName);
                }
                
            }
            
        }

        [HubMethodName("SyncTransaction")]
        public async Task SyncTransaction(ApiPubSubStore message)
        {
            message.SenderId = Helper.GetLoggedInUserId();
            var users=await _db.UserConnections.Select(x => x.UserId).Distinct().ToListAsync();
            foreach (var user in users)
            {
                var newMessage = message.Clone();
                newMessage.ReceiverId = user;
                //repo.Insert(newMessage);
                _db.ApiPubSubStores.AddOrUpdate(newMessage);
            }
            await _db.SaveChangesAsync();
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ClientHub>();            
            context.Clients.Group(Helper.LoggedInTenantClientKey).SyncTransaction();
        }

        public void IntimateVTSStatusChangeForTripLog(VTSStatusLog vts)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ClientHub>();
            var setting=JsonUtility.CreateDefaultSerializerSettings();
            setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            var jsondata = JsonConvert.SerializeObject(vts, setting);
            context.Clients.Group($"{Helper.LoggedInTenantId}-{vts.VehicleId}-UpdateVTSStatus").UpdateVTSStatus(jsondata);
        }
        
        [HubMethodName("JoinVehicleVTSGroup")]
        public Task JoinVehicleVTSGroup(long vehicleId)
        {
            return AddToRoom($"{Helper.LoggedInTenantId}-{vehicleId}-UpdateVTSStatus");
        }
        [HubMethodName("LeaveVehicleVTSGroup")]
        public Task LeaveVehicleVTSGroup(long vehicleId)
        {
            return AddToRoom($"{Helper.LoggedInTenantId}-{vehicleId}-UpdateVTSStatus");
        }
        [HubMethodName("CheckIn")]
        public Task CheckInRoom(string roomName)
        {
            return AddToRoom(Helper.LoggedInTenantClientKey+'-'+roomName);
        }
        [HubMethodName("CheckOut")]
        public Task CheckOutRoom(string roomName)
        {
            return RemoveFromRoom(Helper.LoggedInTenantClientKey + '-' + roomName);
        }
        [HubMethodName("JoinGroup")]
        public Task JoinGroup(string tenantId)
        {
            return AddToRoom(tenantId);
        }
        [HubMethodName("LeaveGroup")]
        public async Task LeaveGroup(string tenantId)
        {
            var connectionid = Context.ConnectionId;
            var connection = await _db.UserConnections.FirstOrDefaultAsync(x => x.ConnectionId == connectionid);
            if (connection != null)
            {
                connection.Connected = false;
                await _db.SaveChangesAsync();
            }
            await RemoveFromRoom(tenantId);
        }
        [HubMethodName("BroadCastMessageToTenant")]
        public static void BroadCastMessageToTenant(string tenantId,string message,string title)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ClientHub>();
            context.Clients.Group(tenantId).BroadCastMessage(message,title);
        }
        [HubMethodName("BroadCastMessageToConnection")]
        public void BroadCastMessageToConnection(string connection, string message, string title)
        {
            if (Context.ConnectionId == connection) return;
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ClientHub>();
            context.Clients.Client(connection).BroadCastMessage(message, title);
        }
        /// <summary>
        /// Send Event to Self
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="type">1:Information 2:Warning 3:Error</param>
        [HubMethodName("PushEventSelf")]
        public void PushEventSelf(string connectionId,string message, string title, PushSelfMessageType type)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ClientHub>();
            context.Clients.Client(connectionId).NotifySelf(message, title,(int)type);
        }
        [HubMethodName("BroadCastMessageToSessionId")]
        public void BroadCastMessageToSessionId(long sessionId, string message, string title)
        {
            if(Helper.SessionId()==sessionId)return;
            var old= _db.ApiSessions.Select(x => new
            {
                x.Id,
                x.UserId
            }).FirstOrDefault(x => x.Id == sessionId);
            
            if (old != null)
            {
                if (old.UserId == Helper.GetLoggedInUserId()) return;
                var newsession = _db.UserConnections.Where(x => x.UserId == old.UserId && x.Connected).Select(x => x.ConnectionId).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(newsession))
                {
                    BroadCastMessageToConnection(newsession, message, title);
                }
            }
        }
        
        public override async Task OnConnected()
        {
            try
            {
                var userid = Helper.GetLoggedInUserId();
                var tenantId = Helper.LoggedInTenantId;
                var user = await _db.Users
                    .Include(u => u.Connections)
                    .Include(x => x.Groups).Select(x => new
                        {x.Groups, x.Connections, x.Id, x.UserName, x.FirstName, x.LastName})
                    .FirstOrDefaultAsync(u => u.Id == userid);

                // If user does not exist in database, must add.
                if (user != null)
                {
                    var connectionid = Context.ConnectionId;
                    // Add to each assigned group.
                    foreach (var item in user.Groups)
                    {
                        await Groups.Add(connectionid, item.GroupName);
                    }

                    _db.UserConnections.RemoveRange(user.Connections.Where(x => x.ConnectionId != connectionid));
                    if (user.Connections.All(x => x.ConnectionId != connectionid))
                    {
                        var connection = new UserConnection()
                        {
                            UserId = userid,
                            ConnectionId = connectionid,
                            Connected = true
                        };
                        _db.UserConnections.AddOrUpdate(connection);
                    }

                    await _db.SaveChangesAsync();

                    var cu = new ConnectedUser()
                    {
                        TenantId = Helper.LoggedInTenantId,
                        UserName = user.UserName,
                        SessionId = Helper.SessionId(),
                        DisplayName = $"{user.FirstName} {user.LastName}",
                        UserId = user.Id,
                        ConnectedTime = DateTime.Now,
                        ConnectionId = connectionid,
                        Groups = user.Groups?.Select(x => x.GroupName).ToList()
                    };
                    if (userid > 0)
                    {
                        //GlobalStore.Instance.SignalRUsers.AddOrUpdate(tenantId, new List<ConnectedUser>(), (s, list) =>
                        //                {
                        //                    if (list == null) list = new List<ConnectedUser>();
                        //                    list.RemoveAll(y => y.TenantId == tenantId && y.UserId == userid);
                        //                    list.Add(cu);
                        //                    return list;
                        //                });
                        _gs.AddUser(tenantId, cu);
                    }

                    foreach (var group in cu.Groups)
                    {
                        Clients.OthersInGroup(group)
                            .joinedGroup(group, connectionid, user.UserName, $"{user.FirstName} {user.LastName}");
                    }

                    // send to caller
                    Clients.Caller.onConnected(connectionid, user.UserName, $"{user.FirstName} {user.LastName}",
                        _gs.GetAllConnectedUsers(tenantId), $"User {user.UserName} has loggedIn.");

                    // send to all except caller client
                    //this.Clients.AllExcept(connectionid).onNewUserConnected(connectionid, user.UserName, $"{user.FirstName} {user.LastName}");

                }

                // Add your own code here.
                // For example: in a chat application, record the association between
                // the current connection ID and user name, and mark the user as online.
                // After the code in this method completes, the client is informed that
                // the connection is established; for example, in a JavaScript client,
                // the start().done callback is executed.
                await base.OnConnected();
            }
            catch
            {
                //Ingore
            }
        }

        public async Task OnDisconnected()
        {
            try
            {
                await this.OnDisconnected(true);
            }
            catch { }
        }
        public override async Task OnDisconnected(bool stopCalled)
        {
            try
            {
                var userid = Helper.GetLoggedInUserId();
                var tenantId = Helper.LoggedInTenantId;
                if (userid > 0)
                {
                    //GlobalStore.Instance.SignalRUsers.AddOrUpdate(tenantId, new List<ConnectedUser>(), (s, list) =>
                    //{
                    //    if (list == null) list = new List<ConnectedUser>();
                    //    list?.RemoveAll(y => y.TenantId == tenantId && y.UserId == userid);
                    //    return list;
                    //});
                    _gs.RemoveUser(tenantId, userid);
                }

                var connectionid = Context.ConnectionId;
                var connection = await _db.UserConnections.FirstOrDefaultAsync(x => x.ConnectionId == connectionid);
                if (connection != null)
                {
                    connection.Connected = false;
                    await _db.SaveChangesAsync();
                }

                // Add your own code here.
                // For example: in a chat application, mark the user as offline, 
                // delete the association between the current connection id and user name.
                await base.OnDisconnected(stopCalled);
            }
            catch
            {
                //Ignore
            }
        }

        public override async Task OnReconnected()
        {
            try
            {
                var userid = Helper.GetLoggedInUserId();
                var connectionid = Context.ConnectionId;
                var tenantId = Helper.LoggedInTenantId;
                var user = await _db.Users
                    .Include(u => u.Connections)
                    .Include(x => x.Groups).Select(x => new
                        {x.Groups, x.Connections, x.Id, x.UserName, x.FirstName, x.LastName})
                    .FirstOrDefaultAsync(u => u.Id == userid);
                if (user != null)
                {
                    _db.UserConnections.RemoveRange(user.Connections.Where(x => x.ConnectionId != connectionid));
                    if (user.Connections.All(x => x.ConnectionId != connectionid))
                    {
                        var connection = new UserConnection()
                        {
                            UserId = userid,
                            ConnectionId = connectionid,
                            Connected = true
                        };
                        _db.UserConnections.AddOrUpdate(connection);
                    }
                    else
                    {

                        var connection = user.Connections.FirstOrDefault(x => x.ConnectionId == connectionid);
                        if (connection != null)
                        {
                            connection.Connected = true;
                            _db.UserConnections.AddOrUpdate(connection);
                        }
                    }

                    await _db.SaveChangesAsync();

                    var cu = new ConnectedUser()
                    {
                        TenantId = Helper.LoggedInTenantId,
                        UserName = user.UserName,
                        SessionId = Helper.SessionId(),
                        DisplayName = $"{user.FirstName} {user.LastName}",
                        UserId = user.Id,
                        ConnectedTime = DateTime.Now,
                        ConnectionId = connectionid,
                        Groups = user.Groups?.Select(x => x.GroupName).ToList()
                    };
                    if (userid > 0)
                    {
                        //GlobalStore.Instance.SignalRUsers.AddOrUpdate(tenantId, new List<ConnectedUser>(), (s, list) =>
                        //{
                        //    if (list == null) list = new List<ConnectedUser>();
                        //    list.RemoveAll(y => y.TenantId == tenantId && y.UserId == userid);
                        //    list.Add(cu);
                        //    return list;
                        //});
                        _gs.AddUser(tenantId, cu);
                    }

                    foreach (var group in cu.Groups)
                    {
                        Clients.OthersInGroup(group)
                            .joinedGroup(group, connectionid, user.UserName, $"{user.FirstName} {user.LastName}");
                    }

                    // send to caller
                    Clients.Caller.onConnected(connectionid, user.UserName, $"{user.FirstName} {user.LastName}",
                        _gs.GetAllConnectedUsers(tenantId), $"User {user.UserName} has reconnected.");
                }

                // Add your own code here.
                // For example: in a chat application, you might have marked the
                // user as offline after a period of inactivity; in that case 
                // mark the user as online again.
                await base.OnReconnected();
            }
            catch
            {
                //Ingore
            }
        }
        public async Task AddToRoom(string roomName)
        {
            try
            {
                // Retrieve room.
                var room = await _db.ConversationGroups.FirstOrDefaultAsync(x => x.GroupName == roomName) ?? new ConversationGroup()
                {
                    GroupName = roomName,
                    Users = new List<ApiUser>()
                };

                var userid = Helper.GetLoggedInUserId();
                var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userid);
                if (user == null) return;
                room.Users.Add(user);
                _db.ConversationGroups.AddOrUpdate(room);
                await _db.SaveChangesAsync();
                await Groups.Add(Context.ConnectionId, roomName);
                Clients.OthersInGroup(roomName)
                    .joinedGroup(roomName, Context.ConnectionId, user.UserName, $"{user.FirstName} {user.LastName}");
            }
            catch { }
        }

        public async Task RemoveFromRoom(string roomName)
        {
            try
            {
                var room = await _db.ConversationGroups.Select(x => new
                {
                    GroupName = x.GroupName,
                    Id = x.Id,
                }).FirstOrDefaultAsync(x => x.GroupName == roomName);
                if (room != null)
                {
                    var userid = Helper.GetLoggedInUserId();
                    await _db.Database.ExecuteSqlCommandAsync($"[dbo].[Proc_UnmapUserConnectionGroup] @parameter1={userid},@parameter2={room.Id}");
                }
                await Groups.Remove(Context.ConnectionId, roomName);
                Clients.OthersInGroup(roomName)
                    .leftGroup(roomName, Context.ConnectionId, Helper.UserName, Helper.GetLoggedInUserFullName());
            }
            catch { }
        }
    }
    //[Authorize, HubName("ClientPatchUpdateHub")]
    //public class ClientPatchUpdateHub : Hub, IClientPatchUpdateHub
    //{
    //    private ITrackoApiDbContext _db;

    //    public ClientPatchUpdateHub(ITrackoApiDbContext context)
    //    {
    //        _db = context;
    //    }
    //    [HubMethodName("NewPatchReleased")]
    //    public static void NewPatchReleased(string tenantId)
    //    {
    //        IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ClientPatchUpdateHub>();
    //        context.Clients.Group(tenantId).CheckPatchUpdate();
    //    }
    //    [HubMethodName("JoinGroup")]
    //    public Task JoinGroup(string tenantId)
    //    {
    //        return Groups.Add(Context.ConnectionId, tenantId);
    //    }
    //    [HubMethodName("LeaveGroup")]
    //    public Task LeaveGroup(string tenantId)
    //    {
    //        var sessionid = Helper.SessionId();
    //        var session = _db.Set<ApiSession>().FirstOrDefault(x => x.Id == sessionid);
    //        if (session != null)
    //        {
    //            session.ObjectState = ObjectState.Modified;
    //            session.EndDateTime = DateTime.Now;
    //            _db.SaveChanges();
    //        }
    //        return Groups.Remove(Context.ConnectionId, tenantId);
    //    }
    //    [HubMethodName("BroadCastMessageToTenant")]
    //    public static void BroadCastMessageToTenant(string tenantId, string message, string title)
    //    {
    //        IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ClientPatchUpdateHub>();
    //        context.Clients.Group(tenantId).BroadCastMessage(message, title);
    //    }
    //    [HubMethodName("BroadCastMessageToConnection")]
    //    public void BroadCastMessageToConnection(string connection, string message, string title)
    //    {
    //        if (Context.ConnectionId == connection) return;
    //        IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ClientPatchUpdateHub>();
    //        context.Clients.Client(connection).BroadCastMessage(message, title);
    //    }
    //    [HubMethodName("BroadCastMessageToSessionId")]
    //    public void BroadCastMessageToSessionId(long sessionId, string message, string title)
    //    {
    //        var old = _db.Set<ApiSession>().Select(x => new
    //        {
    //            x.Id,
    //            x.UserId,
    //            x.EndDateTime,
    //            x.ConnectionId
    //        }).FirstOrDefault(x => x.Id == sessionId);

    //        if (old != null)
    //        {
    //            if (old.UserId == Helper.GetLoggedInUserId()) return;
    //            if (old.EndDateTime != null)
    //            {
    //                var newsession = _db.Set<ApiSession>().Where(x => x.UserId == old.UserId && x.EndDateTime == null).OrderByDescending(x => x.Id).Select(x => x.ConnectionId).FirstOrDefault();
    //                if (!string.IsNullOrWhiteSpace(newsession))
    //                {
    //                    BroadCastMessageToConnection(newsession, message, title);
    //                }
    //            }
    //            else if (!string.IsNullOrWhiteSpace(old.ConnectionId))
    //            {
    //                BroadCastMessageToConnection(old.ConnectionId, message, title);
    //            }
    //        }
    //    }
    //    public override Task OnConnected()
    //    {
    //        var sessionid = Helper.SessionId();
    //        var session = _db.Set<ApiSession>().FirstOrDefault(x => x.Id == sessionid);
    //        if (session != null)
    //        {
    //            session.ConnectionId = Context.ConnectionId;
    //            session.ObjectState = ObjectState.Modified;
    //            _db.SaveChanges();
    //        }
    //        // Add your own code here.
    //        // For example: in a chat application, record the association between
    //        // the current connection ID and user name, and mark the user as online.
    //        // After the code in this method completes, the client is informed that
    //        // the connection is established; for example, in a JavaScript client,
    //        // the start().done callback is executed.
    //        return base.OnConnected();
    //    }

    //    public override Task OnDisconnected(bool stopCalled)
    //    {
    //        // Add your own code here.
    //        // For example: in a chat application, mark the user as offline, 
    //        // delete the association between the current connection id and user name.
    //        var sessionid = Helper.SessionId();
    //        var session = _db.Set<ApiSession>().FirstOrDefault(x => x.Id == sessionid);
    //        if (session != null)
    //        {
    //            session.ObjectState = ObjectState.Modified;
    //            session.EndDateTime = DateTime.Now;
    //            _db.SaveChanges();
    //        }
    //        return base.OnDisconnected(stopCalled);
    //    }

    //    public override Task OnReconnected()
    //    {
    //        // Add your own code here.
    //        // For example: in a chat application, you might have marked the
    //        // user as offline after a period of inactivity; in that case 
    //        // mark the user as online again.
    //        return base.OnReconnected();
    //    }
    //}
}