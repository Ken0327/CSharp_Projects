namespace PTE_Web.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=PTEWEB")
        {
        }

        public virtual DbSet<PTEWEB_Issues_ByDaily> PTEWEB_Issues_ByDaily { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PTEWEB_Issues_ByDaily>()
                .Property(e => e.Support_Org)
                .IsUnicode(false);
        }
    }
}
