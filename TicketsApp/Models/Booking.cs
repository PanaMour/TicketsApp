using System;
using System.Collections.Generic;

namespace TicketsApp.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int? UserId { get; set; }

    public int? EventId { get; set; }

    public int? NumberOfTickets { get; set; }

    public DateTime? BookingDate { get; set; }

    public virtual ICollection<Attendee> Attendees { get; set; } = new List<Attendee>();

    public virtual Event? Event { get; set; }

    public virtual User? User { get; set; }
}
