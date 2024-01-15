using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TicketsApp.Models;

public partial class TicketsappdbContext : DbContext
{
    public TicketsappdbContext()
    {
    }

    public TicketsappdbContext(DbContextOptions<TicketsappdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendee> Attendees { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Venue> Venues { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=ticketsappdb.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendee>(entity =>
        {
            entity.Property(e => e.AttendeeId).HasColumnName("AttendeeID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.CheckInTime).HasColumnType("TIME");

            entity.HasOne(d => d.Booking).WithMany(p => p.Attendees).HasForeignKey(d => d.BookingId);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.BookingDate).HasColumnType("DATE");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Event).WithMany(p => p.Bookings).HasForeignKey(d => d.EventId);

            entity.HasOne(d => d.User).WithMany(p => p.Bookings).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.EventDate).HasColumnType("DATE");
            entity.Property(e => e.EventTime).HasColumnType("TIME");
            entity.Property(e => e.VenueId).HasColumnName("VenueID");

            entity.HasOne(d => d.Venue).WithMany(p => p.Events).HasForeignKey(d => d.VenueId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.Property(e => e.VenueId).HasColumnName("VenueID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
