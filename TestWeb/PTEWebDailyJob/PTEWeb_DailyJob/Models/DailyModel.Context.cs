﻿//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace PTEWeb_DailyJob.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class PTEDBEntities : DbContext
    {
        public PTEDBEntities()
            : base("name=PTEDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<PTEWEB_ItemNameType_ByDaily> PTEWEB_ItemNameType_ByDaily { get; set; }
        public virtual DbSet<PTEWEB_ItemNameType_ByDaily_TOP10_FailItem> PTEWEB_ItemNameType_ByDaily_TOP10_FailItem { get; set; }
        public virtual DbSet<PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime> PTEWEB_ItemNameType_ByDaily_TOP10_LongCycleTime { get; set; }
        public virtual DbSet<PTEWEB_Athena_ByDaily_TOP10_FailItem> PTEWEB_Athena_ByDaily_TOP10_FailItem { get; set; }
        public virtual DbSet<PTEWEB_uTube_ByDaily> PTEWEB_uTube_ByDaily { get; set; }
        public virtual DbSet<PTEWEB_nonITMXP_ByDaily> PTEWEB_nonITMXP_ByDaily { get; set; }
    }
}
