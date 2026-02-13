using Microsoft.AspNetCore.Mvc;
using MvcCoreEnfermosEF.Models;
using MvcCoreEnfermosEF.Repositories;
using System.Threading.Tasks;

namespace MvcCoreEnfermosEF.Controllers
{
    public class EmpleadosController : Controller
    {
        RepositoryEmpleados repo;

        public EmpleadosController(RepositoryEmpleados repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            List<VistaEmpleado> empleados = await this.repo.GetVistaEmpleadosAsync();
            return View(empleados);
        }
    }
}
