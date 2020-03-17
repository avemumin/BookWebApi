using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookWeb.Models;

namespace BookWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishersController : ControllerBase
    {
        private readonly BookStoresDBContext _context;

        public PublishersController(BookStoresDBContext context)
        {
            _context = context;
        }

        // GET: api/Publishers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Publisher>>> GetPublisher()
        {
            return await _context.Publisher.ToListAsync();
        }


        // GET: api/Publishers/5
        [HttpGet("GetPublisherDetails/{id}")]
        public async Task<ActionResult<Publisher>> GetPublisherDetails(int id)
        {
            //eager loading
            //var publisher = await _context.Publisher
            //                                        .Include(pub => pub.Book)
            //                                                .ThenInclude(book => book.Sale)
            //                                        .Include(pub => pub.User)
            //                                                .ThenInclude(user => user.Job)
            //                                        .Where(pub => pub.PubId == id)
            //                                        .FirstOrDefaultAsync();

            //explicit
            var publisher = await _context.Publisher.SingleAsync(pub => pub.PubId == id);

            _context.Entry(publisher)
                .Collection(pub => pub.User)
                .Query()
                .Where(usr => usr.EmailAddress.Contains("karin"))
                .Load();

            _context.Entry(publisher)
                .Collection(pub => pub.Book)
                .Query()
                .Include(book => book.Sale)
                .Load();

            if (publisher == null)
            {
                return NotFound();
            }

            return publisher;
        }


        //Post
        [HttpGet("PostPublisherDetails/")]
        public async Task<ActionResult<Publisher>> PostPublisherDetails()
        {
            var publisher = new Publisher();
            publisher.PublisherName = "Harper & Brothers";
            publisher.City = "New York City";
            publisher.State = "NY";
            publisher.Country = "USA";

            Book book1 = new Book();
            book1.Title = "Good night moon - 1";
            book1.PublishedDate = DateTime.Now;
            Book book2 = new Book();
            book2.Title = "Good night moon - 2";
            book2.PublishedDate = DateTime.Now;

            Sale sale1 = new Sale();
            sale1.Quantity = 2;
            sale1.StoreId = "8042";
            sale1.OrderNum = "XYZ";
            sale1.PayTerms = "Net 30";
            sale1.OrderDate = DateTime.Now;

            Sale sale2 = new Sale();
            sale2.Quantity = 2;
            sale2.StoreId = "7131";
            sale2.OrderNum = "QA879.1";
            sale2.PayTerms = "Net 30";
            sale2.OrderDate = DateTime.Now;

            book1.Sale.Add(sale1);
            book2.Sale.Add(sale2);

            publisher.Book.Add(book1);
            publisher.Book.Add(book2);

            _context.Add(publisher);
            _context.SaveChanges();

            var publishers = await _context.Publisher
                                                    .Include(pub => pub.Book)
                                                            .ThenInclude(book => book.Sale)
                                                    .Include(pub => pub.User)
                                                            .ThenInclude(user => user.Job)
                                                    .Where(pub => pub.PubId == publisher.PubId)
                                                    .FirstOrDefaultAsync();

            if (publisher == null)
            {
                return NotFound();
            }

            return publishers;
        }

        // GET: api/Publishers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Publisher>> GetPublisher(int id)
        {
            var publisher = await _context.Publisher.FindAsync(id);

            if (publisher == null)
            {
                return NotFound();
            }

            return publisher;
        }

        // PUT: api/Publishers/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPublisher(int id, Publisher publisher)
        {
            if (id != publisher.PubId)
            {
                return BadRequest();
            }

            _context.Entry(publisher).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PublisherExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Publishers
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Publisher>> PostPublisher(Publisher publisher)
        {
            _context.Publisher.Add(publisher);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPublisher", new { id = publisher.PubId }, publisher);
        }

        // DELETE: api/Publishers/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Publisher>> DeletePublisher(int id)
        {
            var publisher = await _context.Publisher.FindAsync(id);
            if (publisher == null)
            {
                return NotFound();
            }

            _context.Publisher.Remove(publisher);
            await _context.SaveChangesAsync();

            return publisher;
        }

        private bool PublisherExists(int id)
        {
            return _context.Publisher.Any(e => e.PubId == id);
        }
    }
}
