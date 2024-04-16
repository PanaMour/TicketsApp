using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketsApp.Models;

namespace TicketsApp.Controllers
{
    public class EventsController : Controller
    {
        private readonly TicketsappdbContext _context;
        private readonly IEmailService _emailService;


        public EventsController(TicketsappdbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Events
        public async Task<IActionResult> Index(int? year, int? month)
        {
            // Fetch all events and include related data as needed
            var allEvents = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Bookings)
                .ToListAsync();

            // Now that we have all events in memory, we can filter them
            var targetYear = year ?? DateTime.Today.Year;
            var targetMonth = month ?? DateTime.Today.Month;

            var firstDayOfMonth = new DateTime(targetYear, targetMonth, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Use LINQ to objects to filter the events
            var eventsThisMonth = allEvents
                .Where(e =>
                {
                    DateTime eventDate;
                    return DateTime.TryParse(e.EventDate, out eventDate) &&
                           eventDate >= firstDayOfMonth &&
                           eventDate <= lastDayOfMonth;
                })
                .ToList();

            /*var eventsThisMonth = allEvents
        .Where(e => DateTime.TryParse(e.EventDate, out var eventDate) &&
                    eventDate >= firstDayOfMonth &&
                    eventDate <= lastDayOfMonth &&
                    e.Bookings.Sum(b => b.NumberOfTickets) < e.Venue.Capacity)
        .ToList();*/

            return View(eventsThisMonth);
        }

        // GET: Events
        public async Task<IActionResult> Index2()
        {
            var ticketsappdbContext = _context.Events.Include(a => a.Venue);
            return View(await ticketsappdbContext.ToListAsync());
        }


        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(a => a.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        public async Task<IActionResult> Details2(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(a => a.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }
        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueId");
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,EventName,Description,VenueId,EventDate,EventTime")] Event @event)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index2));
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueId", @event.VenueId);
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueId", @event.VenueId);
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,Description,VenueId,EventDate,EventTime")] Event @event)
        {
            if (id != @event.EventId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var originalEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == id);
                    bool eventDetailsChanged = originalEvent != null &&
                        (!originalEvent.EventDate.Equals(@event.EventDate) || !originalEvent.EventTime.Equals(@event.EventTime));

                    _context.Update(@event);
                    await _context.SaveChangesAsync();

                    if (eventDetailsChanged)
                    {
                        var bookings = _context.Bookings.Where(b => b.EventId == id).Include(b => b.User);

                        foreach (var booking in bookings)
                        {
                            if (booking.User != null && !string.IsNullOrEmpty(booking.User.Email))
                            {
                                string emailContent = $@"
                                    <p>Dear {booking.User.UserName},</p>
                                    <p>The details for the event '{originalEvent.EventName}' have changed.</p>
                                    <p>New date and time: {@event.EventDate} at {@event.EventTime}.</p>
                                    <p>Please contact us if you have any questions.</p>";

                                await _emailService.SendEmailAsync(booking.User.Email, "Event Details Changed", emailContent, null);
                            }
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index2));
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueId", @event.VenueId);
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(a => a.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index2));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}
