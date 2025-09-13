using Database;
using Repository;
using Repository.Interface;
using Repository.Service;

namespace RepositoryTest
{
    public class BaseTest
    {
        protected readonly IServiceProvider _service;
        public BaseTest()
        {
            var build = WebApplication.CreateBuilder();
            build.Services.AddTransient<IOrdineRepository, OrdineRepository>();
            build.Services.AddServiceDb();
            build.Services.AddDbContext<BubbleTeaContext>();
            build.Services.AddDbContext<Database.BubbleTeaContext>();
            var dati = build.Build();
            _service = dati.Services;
        }
    }
}
