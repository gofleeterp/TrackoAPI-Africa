using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tenant.Models;
using TrackoApi.MessageService;
using TrackoApi.Models.Base;

namespace TrackoTests
{
    [TestClass]
    public class SMSTest
    {
        [TestMethod]
        public void TestSendSMS()
        {
            var service = new SMSService();
            var result = service.SendAsync("Hi this is test messase from unit test",userId:0,tenantId: "6f2c6acd68c347fdaed6030a7654e8b2", recivers:"7011949638").ConfigureAwait(false).GetAwaiter().GetResult();
            Assert.AreEqual(result.Status, System.Net.HttpStatusCode.OK);            
        }
        [TestMethod]
        public void TestSendModel()
        {
            var service = new SMSService();
            var result = service.SendAsync(new SMSTemplate
            {
                Country="91",
                Route="4",
                Sender="IWLT",
                SMS=new List<SMSViewModel>
                {
                    new SMSViewModel
                    {
                        Message="Hi this is message 1",
                        To=new List<string>
                        {
                            "7011949638"
                        }
                    }
                }
            },userId: 0, tenantId: "6f2c6acd68c347fdaed6030a7654e8b2").ConfigureAwait(false).GetAwaiter().GetResult();
            Assert.AreEqual(result.Status, System.Net.HttpStatusCode.OK);
        }
        [TestMethod]
        public void TestNotificationPurchaseCount()
        {
            using (var db=new TenantDbContext())
            {
                var result = db.NotificationPurchaseLog.All(x => x.NoOfNotification == x.Notifications.Count());
                Assert.AreEqual(result, true);
            }
        }
    }
}
