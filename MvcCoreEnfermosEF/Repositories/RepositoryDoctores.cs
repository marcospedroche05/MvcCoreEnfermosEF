using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using MvcCoreEnfermosEF.Data;
using MvcCoreEnfermosEF.Models;
using System.Data.Common;

#region STORED PROCEDURES

//CREATE PROCEDURE SP_ESPECIALIDADES
//AS
//	SELECT DISTINCT ESPECIALIDAD FROM DOCTOR
//GO

//CREATE PROCEDURE SP_INCREMENTA_POR_ESPECIALIDAD
//(@especialidad NVARCHAR(50), @incremento int)
//AS
//	UPDATE DOCTOR SET SALARIO = SALARIO + @incremento WHERE ESPECIALIDAD = @especialidad
//GO

//CREATE PROCEDURE SP_DOCTORES_ESPECIALIDAD
//(@especialidad NVARCHAR(50))
//AS
//	SELECT * FROM DOCTOR WHERE ESPECIALIDAD = @especialidad;
//GO

#endregion


namespace MvcCoreEnfermosEF.Repositories
{
    public class RepositoryDoctores
    {
        EnfermosContext context;

        public RepositoryDoctores(EnfermosContext context)
        {
            this.context = context;
        }

        public async Task<List<string>> GetEspecialidadesAsync()
        {
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                string sql = "SP_ESPECIALIDADES";
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;

                await com.Connection.OpenAsync();
                DbDataReader reader = await com.ExecuteReaderAsync();
                List<string> especialidades = new List<string>();
                while(await reader.ReadAsync())
                {
                    string especialidad = reader["ESPECIALIDAD"].ToString();
                    especialidades.Add(especialidad);
                }
                await reader.CloseAsync();
                await com.Connection.CloseAsync();

                return especialidades;
            }
        }

        public async Task UpdateSalarioEspecialidadRawAsync(string especialidad, int incremento)
        {
            string sql = "SP_INCREMENTA_POR_ESPECIALIDAD @especialidad, @incremento";
            SqlParameter pamEspe = new SqlParameter("@especialidad", especialidad);
            SqlParameter pamIncr = new SqlParameter("@incremento", incremento);
            var consulta = await this.context.Database.ExecuteSqlRawAsync(sql, pamEspe, pamIncr);
        }

        public async Task<List<Doctor>> GetDoctoresEspecialidadAsync(string especialidad)
        {
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                string sql = "SP_DOCTORES_ESPECIALIDAD";
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;

                SqlParameter pamEspe = new SqlParameter("@especialidad", especialidad);
                com.Parameters.Add(pamEspe);

                await com.Connection.OpenAsync();
                DbDataReader reader = await com.ExecuteReaderAsync();
                List<Doctor> doctores = new List<Doctor>();
                while(await reader.ReadAsync())
                {
                    Doctor doctor = new Doctor
                    {
                        HospitalCod = int.Parse(reader["HOSPITAL_COD"].ToString()),
                        DoctorNo = int.Parse(reader["DOCTOR_NO"].ToString()),
                        Apellido = reader["APELLIDO"].ToString(),
                        Especialidad = reader["ESPECIALIDAD"].ToString(),
                        Salario = int.Parse(reader["SALARIO"].ToString())
                    };
                    doctores.Add(doctor);
                }
                await reader.CloseAsync();
                await com.Connection.CloseAsync();

                com.Parameters.Clear();
                return doctores;
            }
        }
    }
}
