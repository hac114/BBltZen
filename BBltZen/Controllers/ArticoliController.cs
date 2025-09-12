using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Database;

namespace BBltZen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticoliController : ControllerBase
    {
        private readonly BubbleTeaContext _context;

        public ArticoliController(BubbleTeaContext context)
        {
            _context = context;
        }

        // GET: api/Articoli
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Articolo>>> GetArticoli()
        {
            return await _context.Articolo.ToListAsync();
        }

        // GET: api/Articoli/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Articolo>> GetArticolo(int id)
        {
            var articolo = await _context.Articolo.FindAsync(id);

            if (articolo == null)
            {
                return NotFound();
            }

            return articolo;
        }
    }
}