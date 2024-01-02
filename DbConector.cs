using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace TiendaServices
{
    internal class DbConector
    {
        private String Server;
        private String User;
        private String Password;
        private String DB;
        private MySqlConnection? _con;
        private String _Msg;


        /// <summary>
        /// Constructor para inicializar variables.
        /// </summary>
        public DbConector() {
        /*Server= "t8-2020630308-bd.mysql.database.azure.com";
            User = "hugo";
        Password ="Ta1234567890+";
        DB= "servicio_web";
        */
        _Msg = "";
            Server = Environment.GetEnvironmentVariable("Server");
            User = Environment.GetEnvironmentVariable("UserID");
            Password = Environment.GetEnvironmentVariable("Password");
            DB = Environment.GetEnvironmentVariable("Database");
        }
        /// <summary>
        /// Intenta la conexion a la base de datos. 
        /// </summary>
        /// <returns></returns>
        public MySqlConnection Conexion() {
            string sc = "Server=" + Server + ";UserID=" + User +
                ";Password=" + Password + ";Database=" + DB +
                ";SslMode=Preferred;";

            if (_con == null)
            {

                try
                {
                    _con = new MySqlConnection(sc);
                    _con.Open();
                    _Msg = "Conectado con exito.";
                    
                }
                catch(Exception ex)
                {
                    _Msg=ex.Message;
                    _con = null;

                   
                }



            }
            return _con;
        }
        /// <summary>
        ///  Devuelve el valor del mensaje de la conexion.
        /// </summary>
        /// <returns></returns>
        public String GetMessage()
        {
            return _Msg;

        }
}
}
