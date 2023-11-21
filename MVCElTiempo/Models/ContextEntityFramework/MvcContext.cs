using Microsoft.EntityFrameworkCore;

namespace MVCElTiempo.Models.ContextEntityFramework
{
    public partial class MvcContext : DbContext
    {
        public MvcContext()
        {

        }

        public MvcContext(DbContextOptions<MvcContext> options)
                            : base(options)
        {
        }
        public virtual DbSet<TbUser> TbUser { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TbUser>(entity =>
            {
                entity.HasKey(e => e.IdUser).IsClustered(false); ;

                entity.ToTable("TB_User");

                entity.Property(e => e.IdUser)
                .HasColumnName("IdUser");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasColumnName("FullName");

                entity.Property(e => e.UserName)
                .IsRequired()
                .HasColumnName("UserName");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("Email");

                entity.Property(e => e.PasswordUser)
                    .IsRequired()
                    .HasColumnName("PasswordUser");

                entity.Property(e => e.CreateDate)
                    .IsRequired()
                    .HasColumnName("CreateDate");

                entity.Property(e => e.UpdateDate)
                    .HasColumnName("UpdateDate");
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}