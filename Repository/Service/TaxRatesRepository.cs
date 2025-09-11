using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class TaxRatesRepository
    {
        private readonly BubbleTeaContext _context;
        public TaxRatesRepository(BubbleTeaContext context)
        {
            _context = context;
        }

    }
}
