using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using ZstdSharp.Unsafe;

namespace TiendaServices
{
    public class FnAltaCarrito
    {
        private readonly ILogger _logger;
        MySqlConnection _cn;

        public FnAltaCarrito(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FnAltaCarrito>();
        }

        [Function("FnAltaCarrito")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            String _msg ="";
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            String  IdCarrito = req.Query["IdCarrito"];
            //String IdCarrito = req.Query("IdCarrito");
            if ( String.IsNullOrEmpty(IdCarrito))  {
                _msg = "Error Indicar el folio del carrito a generar";
            }
            else
            {
                int _IdCarrito=0;
                if (int.TryParse(IdCarrito,out _IdCarrito)) { }; //Convierte el parametro de entrada a numerico.
                DbConector _Connector = new DbConector();
                _cn = _Connector.Conexion();
                if (_cn!=null ) {
                    //Conexion exitosa enviara el impacto por mysql
                    //
                    try
                     {
                        MySqlCommand _cmd = new MySqlCommand("Insert into Carrito  (CarritoId, CarritoFecha) values (?,?)  ", _cn);
                        //Crea los parametros.
                        MySqlParameter p = new MySqlParameter();
                        p.Value = _IdCarrito;
                        _cmd.Parameters.Add(p);

                        p = new MySqlParameter();
                        p.Value = DateTime.Now;


                        _cmd.Parameters.Add(p);
                        int iRecords = _cmd.ExecuteNonQuery();
                        _msg = "Registro Actualizados:" + iRecords;
                    }
                    catch (Exception e)
                    {

                        _msg = e.ToString();
                    }
                    finally
                    {
                        if (_cn.State == System.Data.ConnectionState.Open)
                        {
                            _cn.Close(); //ceramos conexion abierta.
                        }
                    }
                  

                }
                else
                {
                    //La conexion fallo enviamos el codigo de error.
                    _msg = _Connector.GetMessage(); 
                }

            }


            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(_msg);

            return response;
        }
    }
}
