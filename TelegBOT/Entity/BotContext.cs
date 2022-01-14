using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace TelegBOT.Entity
{
    public partial class BotContext : DbContext
    {
        public BotContext()
        {
        }

        public BotContext(DbContextOptions<BotContext> options)
            : base(options)
        {
        }

        public virtual DbSet<GroupByCollege> GroupByColleges { get; set; }
        public virtual DbSet<GroupByGuild> GroupByGuilds { get; set; }
        public virtual DbSet<GroupByGuildOfUser> GroupByGuildOfUsers { get; set; }
        public virtual DbSet<HeadOfGroup> HeadOfGroups { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserStatus> UserStatuses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseLazyLoadingProxies();
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=31.44.0.51;Initial Catalog=Bot;Persist Security Info=False;User ID=clown;Password=Billy4You!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<GroupByCollege>(entity =>
            {
                entity.ToTable("GroupByCollege");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GroupByGuild>(entity =>
            {
                entity.ToTable("GroupByGuild");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GroupByGuildOfUser>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.GroupByGuildId })
                    .HasName("PK__GroupByG__09CC5060AC3D8E0B");

                entity.ToTable("GroupByGuildOfUser");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.GroupByGuildId).HasColumnName("GroupByGuildID");

                entity.HasOne(d => d.GroupByGuild)
                    .WithMany(p => p.GroupByGuildOfUsers)
                    .HasForeignKey(d => d.GroupByGuildId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__GroupByGu__Group__2E1BDC42");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.GroupByGuildOfUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__GroupByGu__UserI__2F10007B");
            });

            modelBuilder.Entity<HeadOfGroup>(entity =>
            {
                entity.HasKey(e => new { e.HeadId, e.GroupId })
                    .HasName("PK__HeadOfGr__5A768DC0266F2F69");

                entity.ToTable("HeadOfGroup");

                entity.Property(e => e.HeadId).HasColumnName("HeadID");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.HeadOfGroups)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__HeadOfGro__Group__300424B4");

                entity.HasOne(d => d.Head)
                    .WithMany(p => p.HeadOfGroups)
                    .HasForeignKey(d => d.HeadId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__HeadOfGro__HeadI__30F848ED");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.GroupByCollegeId).HasColumnName("GroupByCollegeID");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Login).HasMaxLength(50);

                entity.Property(e => e.Password).HasMaxLength(50);

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.SecondName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.TelegramId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("TelegramID");

                entity.Property(e => e.UserStatusId).HasColumnName("UserStatusID");

                entity.HasOne(d => d.GroupByCollege)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.GroupByCollegeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__User__GroupByCol__31EC6D26");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__User__RoleID__32E0915F");

                entity.HasOne(d => d.UserStatus)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.UserStatusId)
                    .HasConstraintName("FK_User_UserStatus");
            });

            modelBuilder.Entity<UserStatus>(entity =>
            {
                entity.ToTable("UserStatus");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
