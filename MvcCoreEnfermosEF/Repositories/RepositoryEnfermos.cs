using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MvcCoreEnfermosEF.Data;
using MvcCoreEnfermosEF.Models;
using System.Data.Common;

#region STORED PROCEDURES

//CREATE PROCEDURE SP_ALL_ENFERMOS
//AS
//	SELECT * FROM ENFERMO
//GO


//CREATE PROCEDURE SP_FIND_ENFERMO
//(@inscripcion NVARCHAR(50))
//AS
//	SELECT * FROM ENFERMO WHERE INSCRIPCION = @inscripcion
//GO


//CREATE PROCEDURE SP_ELIMINA_ENFERMO
//(@inscripcion NVARCHAR(50))
//AS
//	DELETE FROM ENFERMO WHERE INSCRIPCION = @inscripcion;
//GO

#endregion


namespace MvcCoreEnfermosEF.Repositories
{
    public class RepositoryEnfermos
    {
        EnfermosContext context;

        public RepositoryEnfermos(EnfermosContext context)
        {
            this.context = context;
        }

        public async Task<List<Enfermo>> GetEnfermosAsync()
        {
            //NECESITAMOS UN COMMAND, VAMOS A UTILIZAR UN USING
            //PARA TODO
            //EL COMMAND, EN SU CREACION, NECESITA DE UNA CADENA DE CONEXION (OBJETO)
            //EL OBJETO CONNECTION NOS LO OFREDE EF
            //LAS CONEXIONES SE CREAN A PARTIR DEL CONTEXT
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                string sql = "SP_ALL_ENFERMOS";
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;
                //ABRIMOS LA CONEXION A PARTIR DEL COMMAND
                await com.Connection.OpenAsync();
                //EJECUTAMOS NUESTRO READER
                DbDataReader reader = await com.ExecuteReaderAsync();
                //DEBEMOS MAPEAR LOS DATOS MANUALMENTE
                List<Enfermo> enfermos = new List<Enfermo>();
                while(await reader.ReadAsync())
                {
                    Enfermo enfermo = new Enfermo
                    {
                        Inscripcion = reader["INSCRIPCION"].ToString(),
                        Apellido = reader["APELLIDO"].ToString(),
                        Direccion = reader["DIRECCION"].ToString(),
                        FechaNac = DateTime.Parse(reader["FECHA_NAC"].ToString()),
                        Sexo = reader["S"].ToString(),
                        NumeroSeguridad = reader["NSS"].ToString()
                    };
                    enfermos.Add(enfermo);
                }
                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return enfermos;
            }
        }
    }
}
