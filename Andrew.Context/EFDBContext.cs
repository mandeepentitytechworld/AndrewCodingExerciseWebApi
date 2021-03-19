using Andrew.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Andrew.Context
{
    public class EFDBContext : DbContext
    {
        public EFDBContext(DbContextOptions<EFDBContext> options)
             : base(options)
        {

        }
        
        public DbSet<UsersCreditCardDetailModel> UsersCreditCardDetail  { get; set; }
        public DbSet<PaymentDetailModel> PaymentDetail  { get; set; }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
