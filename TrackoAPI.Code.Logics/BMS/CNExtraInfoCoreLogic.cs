using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;

namespace TrackoAPI.Code.Logics.BMS
{
    public class CNExtraInfoCoreLogic : BaseLogic<CNExtraInfo>
    {
       

        //protected IDataContextAsync _db;
        //public override IBaseLogic Bind(IDataContextAsync db)
        //{
        //    _db = db;
        //    return this;
        //}

        //public override void Execute(DbEntityEntry entry)
        //{
        //    Execute(entry, false);
        //}

        /// <summary>
        /// Executes the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="isPostLogicCall">if set to <c>true</c> [is post logic call].</param>
        public override void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
           
            //if (entry.Entity is CNExtraInfo extraInfo)
            //{
            //    var cnrepo = _db.Set<CNMaster>();
                
            //    switch (extraInfo.ObjectState)
            //    {
            //        case ObjectState.Added:
            //        case ObjectState.Modified:
            //            if (extraInfo.CNId > 0)
            //            {
            //                var cn = cnrepo.Find(extraInfo.CNId);
            //                if (cn == null)break;
            //                cn.ReachTime = extraInfo.ReachDate;
            //                cn.DeliveryDate = extraInfo.UnloadDate;
            //                cn.PODDate = extraInfo.PODDate;
            //                cn.ObjectState = ObjectState.Modified;
            //                cnrepo.AddOrUpdate(cn);
            //            }
                        
            //            break;
            //        case ObjectState.Deleted:
            //            if (extraInfo.CNId > 0)
            //            {
            //                var dcn = cnrepo.Find(extraInfo.CNId);
            //                if (dcn == null) break;
            //                dcn.ReachTime = null;
            //                dcn.DeliveryDate = null;
            //                dcn.PODDate = null;
            //                dcn.ObjectState = ObjectState.Modified;
            //                cnrepo.AddOrUpdate(dcn);
            //            }

            //            break;
            //    }
            //}
        }
       
        //public override bool SaveAfterPostLogic { get; set; }
        //public override DbSet<CNExtraInfo> DbSet => _db.Set<CNExtraInfo>();
    }
}