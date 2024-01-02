using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

using System.Collections.Generic;
using System.IO;
using Org.BouncyCastle.Asn1.Ocsp;
using static System.Net.Mime.MediaTypeNames;


namespace TiendaServices
{
    public class FnVerCarrito
    {
        private readonly ILogger _logger;
        private MySqlConnection _cn;
        public FnVerCarrito(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FnVerCarrito>();
        }

        [Function("FnVerCarrito")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            String IdCarrito = req.Query["IdCarrito"];
            String _msg;

            if (String.IsNullOrEmpty (IdCarrito)) {
                _msg = "Error: Indicar un carrito valido."; 
            }
            else
            {
                //ConexionDB
                DbConector _Db = new DbConector();
                _cn = _Db.Conexion();
                if (_cn!=null)
                {
                    //consulta
                    using (MySqlCommand _mySqlCommand = _cn.CreateCommand())
                    {
                        ItemCarrito _mItem;
                        _mySqlCommand.CommandText = "Select A.CarritoId, B.ArticuloId,B.Cantidad,C.ArticuloDescripcion,C.ArticuloPrecio , C.ArticuloFoto_GXI, C.ArticuloFoto  from Carrito A , CarritoArticulos B, Articulo C where A.CarritoId= B.CarritoId and C.ArticuloId = B.ArticuloId and A.CarritoId= ?";
                        MySqlParameter p= new MySqlParameter();
                        p.Value = IdCarrito;
                        _mySqlCommand.Parameters.Add(p);
                        MySqlDataReader _mdr = _mySqlCommand.ExecuteReader();
                        List<ItemCarrito> _ItemsCarrito = new List<ItemCarrito>();
                        while (_mdr.Read())
                        {
                            _mItem = new ItemCarrito();
                            _mItem._IdCarrito = _mdr.GetInt32(0);
                            _mItem.IdArticulo = _mdr.GetInt32(1);
                            _mItem.Cantidad = _mdr.GetDouble(2);
                            _mItem.Descripcion = _mdr.GetString(3);
                            _mItem.Precio = _mdr.GetDouble(4);
                            _mItem.Archivo = _mdr.GetString(5);
                            //_mItem.Foto
                            _mItem.SFoto = _mdr.GetString(5);
                            _ItemsCarrito.Add(_mItem);
                         }
                        _mdr.Close();

                        //serializacion
                        _msg = JsonConvert.SerializeObject(_ItemsCarrito);
                    }
                    
                }

                else
                {
                    _msg = "Error: " + _Db.GetMessage();
                }


            }
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(_msg);

            return response;
        }
    }
}
