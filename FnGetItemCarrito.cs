using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace TiendaServices
{
 /// <summary>
 /// Lee el valor del item actual del carrito. 
 /// </summary>
 public class FnGetItemCarrito
    {
        private readonly ILogger _logger;

        public FnGetItemCarrito(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FnGetItemCarrito>();
        }

        [Function("FnGetItemCarrito")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            String _Msg = "";
            String _IdCarrito = req.Query["IdCarrito"];
            String _IdArticulo = req.Query["IdArticulo"];
            
            
            try
            {
                DbConector _DbC = new DbConector();
                MySqlParameter p;
                using (MySqlConnection _Cn =_DbC.Conexion() )
                {
                    using (MySqlCommand _cmd =_Cn.CreateCommand()  ) {
                        _cmd.CommandText = "Select A.CarritoId, B.ArticuloId,B.Cantidad,C.ArticuloDescripcion,C.ArticuloPrecio , C.ArticuloFoto_GXI, C.ArticuloFoto  from Carrito A , CarritoArticulos B, Articulo C where A.CarritoId= B.CarritoId and C.ArticuloId = B.ArticuloId and A.CarritoId= ? and B.ArticuloId = ?  ";
                         p = new MySqlParameter();
                        p.Value = _IdCarrito;
                        _cmd.Parameters.Add(p);

                        p = new MySqlParameter();
                        p.Value = _IdArticulo;
                        _cmd.Parameters.Add(p);


                        using (MySqlDataReader  _mrdr=_cmd.ExecuteReader()  )
                        {
                            ItemCarrito _item = new ItemCarrito();
                            _item.IdArticulo = -1;
                            while (_mrdr.Read())
                            {
                                
                                _item._IdCarrito = _mrdr.GetInt32(0);
                                _item.IdArticulo = _mrdr.GetInt32(1);
                                _item.Cantidad = _mrdr.GetDouble(2);
                                _item.Descripcion = _mrdr.GetString(3);
                                _item.Precio =  _mrdr.GetDouble(4);
                                _item.SFoto = _mrdr.GetString(5);
                            }
                            _Msg = JsonConvert.SerializeObject(_item);
                        }
                    }
                }
               
            }
            catch (Exception e )
            {
                _Msg = "Error:" + e.ToString();
            }


            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(_Msg );

            return response;
        }
    }
}
