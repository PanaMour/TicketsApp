using System;
using System.Collections.Generic;

namespace TicketsApp.Models;

public partial class Attendee
{
    public int AttendeeId { get; set; }

    public int? BookingId { get; set; }

    public string AttendeeName { get; set; } = null!;

    public string CheckInTime { get; set; }

    public virtual Booking? Booking { get; set; }
}
