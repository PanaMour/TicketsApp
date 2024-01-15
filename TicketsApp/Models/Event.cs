using System;
using System.Collections.Generic;

namespace TicketsApp.Models;

public partial class Event
{
    public int EventId { get; set; }

    public string EventName { get; set; } = null!;

    public string? Description { get; set; }

    public int? VenueId { get; set; }

    public DateTime? EventDate { get; set; }

    public TimeSpan? EventTime { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Venue? Venue { get; set; }
}
