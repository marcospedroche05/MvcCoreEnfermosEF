using Microsoft.AspNetCore.Mvc;
using MvcCoreEnfermosEF.Models;
using MvcCoreEnfermosEF.Repositories;

namespace MvcCoreEnfermosEF.Controllers
{
    public class DoctoresController : Controller
    {
        RepositoryDoctores repo;

        public DoctoresController(RepositoryDoctores repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            List<string> especialidades = await this.repo.GetEspecialidadesAsync();
            ViewData["ESPECIALIDADES"] = especialidades;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(string especialidad, int incremento)
        {
            List<string> especialidades = await this.repo.GetEspecialidadesAsync();
            ViewData["ESPECIALIDADES"] = especialidades;
            await this.repo.UpdateSalarioEspecialidadRawAsync(especialidad, incremento);
            List<Doctor> doctores = await this.repo.GetDoctoresEspecialidadAsync(especialidad);
            return View(doctores);
        }
    }
}
