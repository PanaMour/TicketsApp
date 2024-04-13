using System;
using System.Collections.Generic;

namespace TicketsApp.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int? UserId { get; set; }

    public int? EventId { get; set; }

    public int? NumberOfTickets { get; set; }

    public string BookingDate { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Checkin { get; set; }

    public virtual ICollection<Attendee> Attendees { get; set; } = new List<Attendee>();

    public virtual Event? Event { get; set; }

    public virtual User? User { get; set; }
}
