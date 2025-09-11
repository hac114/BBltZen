using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class StatoOrdineRepository
    {
        private readonly BubbleTeaContext _context;
        public StatoOrdineRepository(BubbleTeaContext context)
        {
            _context = context;
        }
    }
}
