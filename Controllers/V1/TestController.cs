using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;
namespace WebApiAutores.Controllers.V1
{
    [Route("/api/test")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public TestController(ApplicationDbContext context)
        {
            this.context = context;
        }
        [HttpGet]
        public ActionResult<List<Autor>> Get()
        {
            return new List<Autor>()
            {
                new Autor(){Id=1,Nombre="user a"},
                new Autor(){Id=1,Nombre="user a"}
            };
        }
    }
}
