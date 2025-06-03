using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PropertyGalla.Models;

namespace PropertyGalla.Data
{
    public class PropertyGallaContext : DbContext
    {
        public PropertyGallaContext(DbContextOptions<PropertyGallaContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<ViewRequest> ViewRequests { get; set; }

        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<SavedProperty> SavedProperties { get; set; }

        // ✅ Add this method to configure foreign key constraints
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Property → User (Owner)
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.OwnedProperties)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // PropertyImage → Property
            modelBuilder.Entity<PropertyImage>()
                .HasOne(i => i.Property)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // ViewRequest → User & Property
            modelBuilder.Entity<ViewRequest>()
                .HasOne(v => v.User)
                .WithMany(u => u.ViewRequests)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ViewRequest>()
                .HasOne(v => v.Property)
                .WithMany()
                .HasForeignKey(v => v.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);


            // Feedback → Reviewer & Owner
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Reviewer)
                .WithMany(u => u.GivenFeedbacks)
                .HasForeignKey(f => f.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Owner)
                .WithMany(u => u.ReceivedFeedbacks)
                .HasForeignKey(f => f.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Enforce unique (ReviewerId, OwnerId) to prevent duplicate feedback
            modelBuilder.Entity<Feedback>()
                .HasIndex(f => new { f.ReviewerId, f.OwnerId })
                .IsUnique();


            // Report → Reporter & Property
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany(u => u.SubmittedReports)
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Property)
                .WithMany()
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
                .HasIndex(r => new { r.ReporterId, r.PropertyId })
                .IsUnique();


            // SavedProperty → User & Property
            modelBuilder.Entity<SavedProperty>()
                .HasOne(s => s.User)
                .WithMany(u => u.SavedProperties)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SavedProperty>()
                .HasOne(s => s.Property)
                .WithMany()
                .HasForeignKey(s => s.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
