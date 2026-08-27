using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Tenant.Models;
using TrackoApi.Core.Helpers;

namespace Tenant
{
    public class FuelSystemRepository : IFuelSystemRepository
    {
        private static readonly Dictionary<string, string> HPCLStateCodes = new Dictionary<string, string>()
        {
            {"AP1","Andhra Pradesh"},
            {"AS","Assam"},
            {"BR","Bihar"},
            {"CH","Chandigarh"},
            {"CT","Chhattishgarh"},
            {"DN","Dadar and Nagara Haveli"},
            {"DD","Daman and Diu"},
            {"GA","Goa"},
            {"GJ","Gujarat"},
            {"HR","Haryana"},
            {"HP","Himachal Pradesh"},
            {"JK","Jammu and Kashmir"},
            {"JH","Jharkhand"},
            {"KA","Karnataka"},
            {"KL","Kerala"},
            {"MP","Madhya Pradesh"},
            {"MH","Maharashtra"},
            {"MN","Manipur"},
            {"ML","Meghalaya"},
            {"MZ","Mizoram"},
            {"NL","Nagaland"},
            {"DL","New Delhi"},
            {"OR","Orrisa"},
            {"PY","Pondicherry"},
            {"PB","Punjab"},
            {"RJ","Rajasthan"},
            {"SK","Sikkim"},
            {"TN","Tamilnadu"},
            {"TG","Telangana"},
            {"TR","Tripura"},
            {"UP","Uttar Pradesh"},
            {"UT","Uttrakhand"},
            {"WB","West Bengal"}
        };

        private static readonly Dictionary<string, string> IOCStates = new Dictionary<string, string>()
        {
            {"AN","Andaman & Nicobar"},
            {"AP","Andhra Pradesh"},
            {"ARP","Arunachal Pradesh"},
            {"AS","Assam"},
            {"BH","Bihar"},
            {"CD","Chandigarh"},
            {"CSG","Chhatisgarh"},
            {"DH","Dadra Nagarhaveli"},
            {"DD","Daman & Diu"},
            {"DEL","Delhi"},
            {"GDD","Goa"},
            {"GJ","Gujarat"},
            {"HR","Haryana"},
            {"HP","Himachal Pradesh"},
            {"JK","Jammu & Kashmir"},
            {"JRK","Jharkhand"},
            {"KAR","Karnataka"},
            {"KER","Kerala"},
            {"MP","Madhya Pradesh"},
            {"MAH","Maharashtra"},
            {"MNP","Manipur"},
            {"MGL","Meghalaya"},
            {"MZ","Mizoram"},
            {"NG","Nagaland"},
            {"OR","Odisha"},
            {"PY","Pondicherry"},
            {"PB","Punjab"},
            {"RJ","Rajasthan"},
            {"SK","Sikkim"},
            {"TN","Tamil Nadu"},
            {"TG","Telangana"},
            {"TRP","Tripura"},
            {"UP","Uttar Pradesh"},
            {"UTK","Uttarakhand"},
            {"WB","West Bengal"}
        };

        public FuelSystemRepository()
        {
        }

        public async Task FetchHPCLRates()
        {
        }

        public async Task FetchIOCRate()
        {
            if (Helper.HostedOnPremise) return;
            List<IOCPumpViewModel> iocrates;

            using (var db = new TenantDbContext())
            {
                if (!(await db.States.AnyAsync(x => x.CompanyCode == "IOCL")))
                {
                    db.States.AddRange(IOCStates.Select(x => new StateMaster()
                    {
                        CompanyCode = "IOCL",
                        StateCode = x.Key,
                        StateName = x.Value
                    }));
                }
                await db.SaveChangesAsync();
                var states = await db.States.Where(x => x.CompanyCode == "IOCL").ToListAsync();
                iocrates = IOCRate(states);
                await db.SaveChangesAsync();
            }
            var updateDate = DateTime.Now;
            foreach (var stateGroup in iocrates.GroupBy(x => x.StateId))
            {
                using (var db = new TenantDbContext())
                {
                    var ischange = false;
                    var repo = db.Pumps;
                    var pumps = await repo.Where(x => x.StateId == stateGroup.Key).ToListAsync();
                    foreach (IOCPumpViewModel rate in stateGroup)
                    {
                        bool ratechange = false;
                        var pump = pumps.FirstOrDefault(x => x.PumpName == rate.PumpName) ?? new IOCPump();
                        if (pump.PumpId > 0)
                        {
                            if (Math.Abs(pump.PetrolPrice - rate.PetrolPrice) > 0)
                            {
                                pump.PetrolPrice = rate.PetrolPrice;
                                ratechange = true;
                            }
                            if (Math.Abs(pump.DieselPrice - rate.DieselPrice) > 0)
                            {
                                pump.DieselPrice = rate.DieselPrice;
                                if (!ratechange) ratechange = true;
                            }
                            pump.LastRateUpdated = updateDate;
                        }
                        else
                        {
                            pump.DieselPrice = rate.DieselPrice;
                            pump.PetrolPrice = rate.PetrolPrice;
                            pump.LastRateUpdated = updateDate;
                            pump.PumpName = rate.PumpName;
                            pump.StateId = rate.StateId;
                            pump.Address = rate.Address;
                            pump.AreaSalesOfficeContact = rate.AreaSalesOfficeContact;
                            pump.CompanyCode = "IOCL";
                            pump.District = rate.District;
                            pump.Latitude = rate.Latitude;
                            pump.Longitude = rate.Longitude;
                            pump.Owner = rate.Owner;
                            pump.OwnerPhone = rate.OwnerPhone;
                            ratechange = true;
                        }
                        if (ratechange)
                        {
                            var log = new RateLog()
                            {
                                DieselPrice = rate.DieselPrice,
                                PetrolPrice = rate.PetrolPrice,
                                LogDate = updateDate,
                                PumpId = pump.PumpId,
                                IocPump = pump
                            };
                            if (pump.RateLogs == null)
                            {
                                pump.RateLogs = new List<RateLog>();
                            }
                            pump.RateLogs.Add(log);
                            db.RateLogs.AddOrUpdate(log);
                        }
                        repo.AddOrUpdate(pump);
                    }
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task SyncTolls()
        {
            try
            {
                if (Helper.HostedOnPremise) return;
                using (var client = new HttpClient())
                {
                    //var result =await
                    //    client.PostAsync("http://tis.nhai.gov.in/TollPlazaService.asmx/GetTollPlazaInfoForMapOnPC", new HttpMessageContent(new HttpRequestMessage()));
                    var message = new HttpRequestMessage(HttpMethod.Post,
                        "http://tis.nhai.gov.in/TollPlazaService.asmx/GetTollPlazaInfoForMapOnPC");
                    message.Headers.Accept.Clear();
                    message.Headers.Accept.ParseAdd("application/json");
                    //message.Headers.TryAddWithoutValidation("ContentType", "application/json; charset=utf-8");//ContentType=new MediaTypeHeaderValue("application/json; charset=utf-8");
                    message.Content = new StringContent("",
                        Encoding.UTF8,
                        "application/json");
                    var result = await client.SendAsync(message, HttpCompletionOption.ResponseContentRead);
                    if (result.IsSuccessStatusCode)
                    {
                        var datastr = await result.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<RootObject>(datastr, new JavaScriptDateTimeConverter());
                        if (data.d.Any())
                        {
                            using (var db = new TenantDbContext())
                            {
                                var tr = db.Database.BeginTransaction(IsolationLevel.ReadCommitted);
                                try
                                {
                                    await db.Database.ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction,
                                        "TRUNCATE TABLE [dbo].[mTollPlaza]");
                                    db.Tolls.AddRange(data.d);
                                    await db.SaveChangesAsync();
                                    tr.Commit();
                                }
                                catch (Exception e)
                                {
                                    tr.Rollback();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Ignore
            }
        }
        private List<HPCLRateLogViewModel> GetHPCLRate(List<StateMaster> list)
        {
            var all = new List<HPCLRateLogViewModel>();

            foreach (var code in list)
            {
                var response = string.Empty;
                try
                {
                    using (var client = new HttpClient())
                    {
                        response =
                            client.GetStringAsync(
                                $"http://hproroute.hpcl.co.in/StateDistrictMap_4/fetchmshsdprice.jsp?param=T&statecode={code.StateCode}?1501149750959").Result;
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(response);
                        string json = JsonConvert.SerializeXmlNode(doc);
                        json = json.Remove(0, 67);
                        json = json.Replace("}}", "").Replace("@lat", "Latitude").Replace("@lng", "Longitude").Replace("@ms", "PetrolPrice").Replace("@hsd", "DieselPrice").Replace("@", "").Replace("[", "").Replace("]", "").Replace("N/A", "0");
                        // if (!json.EndsWith(",")) json += ",";
                        // Console.Write($"Rates For State {code.Value}\n{json}");
                        var res = JsonConvert.DeserializeObject<List<HPCLRateLogViewModel>>($"[{json}]");
                        res?.ForEach(x =>
                        {
                            x.StateName = code.StateName;
                            x.StateCode = code.StateCode;
                            x.StateId = code.Id;
                        });
                        if (res != null)
                        {
                            all.AddRange(res);
                        }
                    }
                }
                catch (Exception e)
                {
                    //Ignore
                }
            }
            return all;
        }

        private List<IOCPumpViewModel> IOCRate(List<StateMaster> list)
        {
            var all = new List<IOCPumpViewModel>();
            foreach (var code in list)
            {
                string response = string.Empty;
                try
                {
                    using (var client = new HttpClient())
                    {
                        var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("state",$"{code.StateCode}")
                        });
                        var result =
                            client.PostAsync("https://associates.indianoil.co.in/PumpLocator/StateWiseLocator", content).Result;
                        response = result.Content.ReadAsStringAsync().Result;
                        var lines = response.Split('|');
                        foreach (var line in lines)
                        {
                            try
                            {
                                var row = new IOCPumpViewModel { StateCode = code.StateCode, StateId = code.Id };
                                int columns = 0;
                                foreach (var s in line.Split(','))
                                {
                                    columns++;
                                    switch (columns)
                                    {
                                        case 1:
                                            row.PumpName = s;
                                            break;

                                        case 2:
                                            double lat = 0;
                                            double.TryParse(s, out lat);
                                            row.Latitude = lat;
                                            break;

                                        case 3:
                                            double lng = 0;
                                            double.TryParse(s, out lng);
                                            row.Longitude = lng;
                                            break;

                                        case 4:
                                            row.Address = s;
                                            break;

                                        case 26:
                                            double petrol = 0;
                                            double.TryParse(s, out petrol);
                                            row.PetrolPrice = petrol;
                                            break;

                                        case 27:
                                            double diesel = 0;
                                            double.TryParse(s, out diesel);
                                            row.DieselPrice = diesel;
                                            break;

                                        case 30:
                                            row.AreaSalesOfficeContact = s;
                                            break;

                                        case 31:
                                            row.Owner = s;
                                            break;

                                        case 35:
                                            row.District = s;
                                            break;

                                        case 36:
                                            row.State = s;
                                            break;

                                        case 37:
                                            row.OwnerPhone = s;
                                            break;
                                    }
                                }
                                all.Add(row);
                            }
                            catch (Exception e)
                            {
                                code.LastErrorTime = DateTime.Now;
                                code.LastError = e.GetBaseException().Message;
                            }
                        }
                        code.LastRateUpdated = DateTime.Now;
                    }
                }
                catch (Exception e)
                {
                    //Ignore
                }
                code.LastRateUpdated = DateTime.Now;
            }
            return all;
        }
    }

    public class HPCLRateLogViewModel
    {
        public DateTime DateTime { get; set; } = DateTime.Now;
        public double DieselPrice { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double PetrolPrice { get; set; }
        public string StateCode { get; set; }
        public long StateId { get; set; }
        public string StateName { get; set; }
        public string TownCode { get; set; }
        public string TownName { get; set; }
    }

    public class IOCPumpViewModel
    {
        public string Address { get; set; }
        public string AreaSalesOfficeContact { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public double DieselPrice { get; set; }
        public string District { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Owner { get; set; }
        public string OwnerPhone { get; set; }
        public double PetrolPrice { get; set; }
        public string PumpName { get; set; }
        public string State { get; set; }
        public string StateCode { get; set; }
        public long StateId { get; set; }
    }
}