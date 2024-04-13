using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketsApp.Models;
using QRCoder;
using MimeKit;

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
            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                var bookingsForUser = _context.Bookings
                 .Where(b => b.UserId == userId)
                 .Include(b => b.Event)
                 .Include(b => b.User);
                return View(await bookingsForUser.ToListAsync());
            }else
            {
                var ticketsappdbContext = _context.Bookings.Include(b => b.Event).Include(b => b.User);
                return View(await ticketsappdbContext.ToListAsync());
            }
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
        public async Task<IActionResult> Create([Bind("EventId,NumberOfTickets")] Booking booking, bool sendEmail = false)
        {
            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                booking.UserId = userId;
                booking.BookingDate = DateTime.Now.ToString("yyyy-MM-dd");
                _context.Add(booking);
                await _context.SaveChangesAsync();

                var eventDetails = await _context.Events
                                                .Include(e => e.Venue)
                                                .FirstOrDefaultAsync(e => e.EventId == booking.EventId);

                if (eventDetails != null && sendEmail)
                {
                    var userEmail = User.FindFirstValue(ClaimTypes.Email);
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        await SendEmailToUser(booking.BookingId, eventDetails, userEmail);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("UserId", "There was a problem retrieving your user information. Please try again.");
                return View(booking);
            }
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
            // Generate QR code
            var qrGenerator = new QRCodeGenerator();
            var qrInfo = $"Booking ID: {bookingId}\n" +
                 $"Event: {eventDetails.EventName}\n" +
                 $"Date: {eventDetails.EventDate}\n" +
                 $"Time: {eventDetails.EventTime}\n" +
                 $"Venue: {eventDetails.Venue.VenueName}\n" +
                 $"Description: {eventDetails.Description}\n" +
                 $"Location: {eventDetails.Venue.Location}\n";

            var qrCodeData = qrGenerator.CreateQrCode(qrInfo, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            // Create MIME part for attachment
            var attachment = new MimePart("image", "png")
            {
                Content = new MimeContent(new MemoryStream(qrCodeBytes), ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = "QRCode.png"
            };

            // Prepare email content
            var emailContent = $@"
<html>
<head>
    <title>Booking Confirmation</title>
</head>
<body>
    <h1>Thank you for your booking</h1>
    <p><strong>Booking ID:</strong> {bookingId}</p>
    <p><strong>Event Name:</strong> {eventDetails.EventName}</p>
    <p><strong>Description:</strong> {eventDetails.Description}</p>
    <p><strong>Venue Name:</strong> {eventDetails.Venue.VenueName}</p>
    <img src='{eventDetails.Venue.ImageUrl}' alt='Venue Image' style='width:100%; max-width:600px; height:auto;'>
    <p>If you have any questions, please contact us.</p>
</body>
</html>";

            // List of attachments (currently just one)
            var attachments = new List<MimePart> { attachment };

            // Send the email
            await _emailService.SendEmailAsync(userEmail, "Your Booking Confirmation", emailContent, attachments);
        }

    }
}
