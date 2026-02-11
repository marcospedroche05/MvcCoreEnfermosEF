using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using MvcCoreEnfermosEF.Data;
using MvcCoreEnfermosEF.Models;
using System.Collections.Generic;
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

//CREATE PROCEDURE SP_INSERT_ENFERMO
//(@apellido NVARCHAR(50), @direccion NVARCHAR(50), @fecha_nac DATETIME, @sexo NVARCHAR(50), @nss NVARCHAR(50))
//AS
//	DECLARE @inscripcion NVARCHAR(50)
//	SELECT @inscripcion = MAX(INSCRIPCION) + 1 FROM ENFERMO;

//insert into enfermo values(@inscripcion, @apellido, @direccion, @fecha_nac, @sexo, @nss);
//PRINT 'INSERTADO'
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

        public async Task<Enfermo> FindEnfermosAsync(string inscripcion)
        {
            //PARA LLAMAR A UN PROCEDIMIENTO QUE CONTIENE PARAMETROS
            //LA LLAMADA SE REALIZA MEDIANTE EL NOMBRE DEL POCEDURE
            //Y CADA PARAMETRO A CONTINUACION DE LA DECLARACION
            //DEL SQL: SP_PROCEDURE @PAM1, @PAM2
            string sql = "SP_FIND_ENFERMO @inscripcion";
            SqlParameter pamIns = new SqlParameter("@inscripcion", inscripcion);
            //SI LOS DATOS QUE DEVUELVE EL PROCEDURE ESTAN MAPEADOS
            //CON UN MODEL, PODEMOS UTILIZAR EL METODO
            //FromSqlRaw PARA RECUPERAR DIRECTAMENTE EL MODEL/S
            //NO PODEMOS CONSULTAR Y EXTRAER A LA VEZ CON LINQ, SE
            //DEBE REALIZAR SIEMPRE EN DOS PASOS
            var consulta = this.context.Enfermos.FromSqlRaw(sql, pamIns);
            //DEBEMOS UTILIZAR AS ENUMERABLE PARA EXTRAER LOS DATOS
            Enfermo enfermo = await consulta.AsAsyncEnumerable().FirstAsync();
            return enfermo;   
        }

        public async Task DeleteEnfermoAsync(string inscripcion)
        {
            string sql = "SP_ELIMINA_ENFERMO";
            SqlParameter pamIns =
                new SqlParameter("@inscripcion", inscripcion);
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(pamIns);
                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
                com.Parameters.Clear();
            }
        }

        public async Task DeleteEnfermoRawAsync(string inscripcion)
        {
            string sql = "SP_ELIMINA_ENFERMO @inscripcion";
            SqlParameter pamIns = new SqlParameter("@inscripcion", inscripcion);
            var consulta = await this.context.Database.ExecuteSqlRawAsync(sql, pamIns);
        }

        public async Task InsertEnfermoRawAsync(string apellido, string direccion, DateTime fechaNac, string sexo, string nss)
        {
            string sql = "SP_INSERT_ENFERMO @apellido, @direccion, @fecha_nac, @sexo, @nss";
            SqlParameter pamApe = new SqlParameter("@apellido", apellido);
            SqlParameter pamDir = new SqlParameter("@direccion", direccion);
            SqlParameter pamFecha = new SqlParameter("@fecha_nac", fechaNac);
            SqlParameter pamSexo = new SqlParameter("@sexo", sexo);
            SqlParameter pamNss = new SqlParameter("@nss", nss);
            var consulta = await this.context.Database.ExecuteSqlRawAsync(sql, pamApe, pamDir, pamFecha, pamSexo, pamNss);
        }


    }
}
