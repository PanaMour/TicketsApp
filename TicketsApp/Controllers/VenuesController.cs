using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketsApp.Models;
using Azure.Storage.Blobs;
using System.IO;
using Azure.Storage.Sas;
using Azure.Storage;


namespace TicketsApp.Controllers
{
    public class VenuesController : Controller
    {
        private readonly TicketsappdbContext _context;
        private readonly IConfiguration _configuration;
        public VenuesController(TicketsappdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private string GenerateBlobSasToken(string containerName, string blobName)
        {
            string connectionString = _configuration["AzureStorage:ConnectionString"];

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            containerClient.CreateIfNotExists();

            var blobClient = containerClient.GetBlobClient(blobName);

            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTime.UtcNow.AddSeconds(30)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);

            BlobSasQueryParameters sasQueryParameters = sasBuilder.ToSasQueryParameters(
                new StorageSharedKeyCredential(
                    blobServiceClient.AccountName,
                    _configuration["AzureStorage:AccountKey"]));

            return blobClient.Uri + "?" + sasQueryParameters.ToString();
        }

        [HttpGet]
        public IActionResult GetUploadSasToken(string blobName)
        {
            if (string.IsNullOrEmpty(blobName))
            {
                return BadRequest("Blob name is required.");
            }

            string sasToken = GenerateBlobSasToken("venue-images", blobName);
            return Json(new { sasToken });

        }
        // GET: Venues
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venues.ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,VenueName,Location,Capacity")] Venue venue, string imageUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save the image URL that was uploaded via SAS token from the client
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        venue.ImageUrl = imageUrl;
                    }
                    else
                    {
                        Console.WriteLine("No image uploaded.");
                    }

                    // Save venue details to the database
                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving to the database: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Model state is invalid.");
            }

            return View(venue);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: Venues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity")] Venue venue)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
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
            return View(venue);
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue != null)
            {
                _context.Venues.Remove(venue);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}
