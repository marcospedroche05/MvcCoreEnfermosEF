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
        public async Task<IActionResult> Index(string especialidad, int incremento, string boton)
        {
            List<string> especialidades = await this.repo.GetEspecialidadesAsync();
            ViewData["ESPECIALIDADES"] = especialidades;
            if (boton == "raw")
            {
                await this.repo.UpdateSalarioEspecialidadRawAsync(especialidad, incremento);
            } else if (boton == "ef")
            {
                await this.repo.UpdateSalarioEspecialidadEFAsync(especialidad, incremento);
            }
            List<Doctor> doctores = await this.repo.GetDoctoresEspecialidadAsync(especialidad);
            return View(doctores);
        }
    }
}
