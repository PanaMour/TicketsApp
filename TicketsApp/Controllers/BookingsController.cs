using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketsApp.Models;

namespace TicketsApp.Controllers
{
    public class BookingsController : Controller
    {
        private readonly TicketsappdbContext _context;
        private readonly IEmailService _emailService;

        public BookingsController(TicketsappdbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var ticketsappdbContext = _context.Bookings.Include(b => b.Event).Include(b => b.User);
            return View(await ticketsappdbContext.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create(int eventId, string eventName, string eventDate)
        {
            ViewData["EventId"] = eventId; 
            ViewData["EventName"] = eventName;
            ViewData["EventDate"] = eventDate;
            //ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName");
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,NumberOfTickets")] Booking booking, string userEmail)
        {
            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                booking.UserId = userId; 
                booking.BookingDate = DateTime.Now.ToString("yyyy-MM-dd"); 

                try
                {
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    var eventDetails = await _context.Events
                    .Include(e => e.Venue)
                    .FirstOrDefaultAsync(e => e.EventId == booking.EventId);

                    if (eventDetails != null)
                    {
                        //await SendEmailToUser(booking.BookingId, eventDetails, userEmail);
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "There was an error saving the booking. Please try again.");
                }
            }
            else
            {
                ModelState.AddModelError("UserId", "There was a problem retrieving your user information. Please try again.");
                return View(booking);
            }

            // If we get here, something went wrong. Include necessary ViewData or TempData for the view.
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", booking.UserId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,UserId,EventId,NumberOfTickets,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventId", booking.EventId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", booking.UserId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
        private async Task SendEmailToUser(int bookingId, Event eventDetails, string userEmail)
        {
            var emailContent = $"Thank you for your booking.\n\n" +
                               $"Booking ID: {bookingId}\n" +
                               $"Event Name: {eventDetails.EventName}\n" +
                               $"Description: {eventDetails.Description}\n" +
                               $"Venue Name: {eventDetails.Venue.VenueName}\n" +
                               $"Location: {eventDetails.Venue.Location}";

            // Use your email service to send the email
            var emailService = new EmailService(); // Replace with your email service
            await emailService.SendEmailAsync(userEmail, "Your Booking Confirmation", emailContent);

        }


    }
}
