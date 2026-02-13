using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using MvcCoreEnfermosEF.Data;
using MvcCoreEnfermosEF.Models;

#region VISTAS

//CREATE VIEW V_TRABAJADORES
//AS
//	SELECT EMP_NO AS IDTRABAJADOR
//	, APELLIDO, OFICIO, SALARIO FROM EMP
//	UNION
//	SELECT DOCTOR_NO, APELLIDO, ESPECIALIDAD, SALARIO FROM DOCTOR
//	UNION
//	SELECT EMPLEADO_NO, APELLIDO, FUNCION, SALARIO FROM PLANTILLA
//GO

#endregion

#region STORED PROCEDURES

//CREATE PROCEDURE SP_TRABAJADORES_OFICIO
//(@oficio NVARCHAR(50), @personas int OUT, @media int OUT, @suma int OUT)
//AS
//	SELECT * FROM V_TRABAJADORES WHERE OFICIO = @oficio
//	SELECT @personas = COUNT(IDTRABAJADOR), @media = AVG(SALARIO), @suma = SUM(SALARIO) FROM V_TRABAJADORES WHERE OFICIO = @oficio
//GO

#endregion

namespace MvcCoreEnfermosEF.Repositories
{
    public class RepositoryEmpleados
    {
        HospitalContext context;

        public RepositoryEmpleados(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<List<VistaEmpleado>> GetVistaEmpleadosAsync()
        {
            var consulta = from datos in this.context.VistaEmpleados
                           select datos;
            return await consulta.ToListAsync();
        }

        public async Task<TrabajadoresModel> GetTrabajadoresModelAsync()
        {
            //PRIMERO CON LINQ
            var consulta = from datos in this.context.Trabajadores
                           select datos;
            TrabajadoresModel model = new TrabajadoresModel();
            model.Trabajadores = await consulta.ToListAsync();
            model.Personas = await consulta.CountAsync();
            model.MediaSalarial = (int) await consulta.AverageAsync(x => x.Salario);
            model.SumaSalarial = await consulta.SumAsync(x => x.Salario);
            return model;
        }

        public async Task<List<string>> GetOficiosAsync()
        {
            var consulta = (from datos in this.context.Trabajadores
                            select datos.Oficio).Distinct();
            return await consulta.ToListAsync();
        }

        public async Task<TrabajadoresModel> GetTrabajadoresModelOficioAsync(string oficio)
        {
            //YA QUE TENEMOS EL MODEL, VAMOS A LLAMARLO CON EF
            //LA UNICA DIFERENCIA CUANDO TENEMOS PARAMETROS DE SALIDA 
            //ES INDICAR LA PALABRA OUT EN LA DECLARACION DE LAS VARIABLES
            string sql = "SP_TRABAJADORES_OFICIO @oficio, @personas out, @media out, @suma out";
            SqlParameter pamOficio = new SqlParameter("@oficio", oficio);
            SqlParameter pamPersonas = new SqlParameter("@personas", -1);
            pamPersonas.Direction = System.Data.ParameterDirection.Output;
            SqlParameter pamMedia = new SqlParameter("@media", -1);
            pamMedia.Direction = System.Data.ParameterDirection.Output;
            SqlParameter pamSuma = new SqlParameter("@suma", -1);
            pamSuma.Direction = System.Data.ParameterDirection.Output;

            var consulta = this.context.Trabajadores.FromSqlRaw(sql, pamOficio, pamPersonas, pamMedia, pamSuma);
            TrabajadoresModel model = new TrabajadoresModel();
            model.Trabajadores = await consulta.ToListAsync();
            model.Personas = int.Parse(pamPersonas.Value.ToString());
            model.MediaSalarial = int.Parse(pamMedia.Value.ToString());
            model.SumaSalarial = int.Parse(pamSuma.Value.ToString());
            return model;
           
        }



    }
}
