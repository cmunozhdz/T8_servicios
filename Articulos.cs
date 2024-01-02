using System.Net;
using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace TiendaServices
{
    public class Articulos
    {
        private readonly ILogger _logger;

        public Articulos(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Articulos>();
        }

        [Function("Articulos")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            DbConector _conector = new DbConector();
            MySqlConnection _cnn = _conector.Conexion();
            if (_cnn!=null)
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                MySqlCommand QryArticulos = new MySqlCommand("Select ArticuloId ,ArticuloDescripcion,ArticuloCantidad,ArticuloPrecio ,ArticuloFoto From Articulo", _cnn);
                using (MySqlDataReader TaArticulos = QryArticulos.ExecuteReader() )
                {
                    while ( TaArticulos.Read())
                    {
                        response.WriteString(String.Format("{0}", TaArticulos[0])
                            + String.Format("{0}", TaArticulos[1])
                            + String.Format("{0}", TaArticulos[2])
                            + String.Format("{0}", TaArticulos[3])
                            + String.Format("{0}", TaArticulos[4])
                            
                            );
                        
                    }
                    _cnn.Close(); //Cierra la conexion con Mysql 
                }

                return response;


            }
            else
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.WriteString(_conector.GetMessage()); //Imprime el resultado de la validacion de la conexion.
                return response;
            }
            
            
            



            
        }
    }
}
