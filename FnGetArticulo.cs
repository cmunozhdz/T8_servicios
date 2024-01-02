using System.Linq.Expressions;
using System.Net;
using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace TiendaServices
{
    public class FnGetArticulo
    {
        private readonly ILogger _logger;

        public FnGetArticulo(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FnGetArticulo>();
        }

        [Function("FnGetArticulo")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            String _IdArticulo = req.Query["Id"];
            String _Msg = "";
            int Id = 0;
            if (int.TryParse(_IdArticulo, out Id)) {
                try
                {
                    Articulo _Articulo= new Articulo();
                    DbConector obCnn = new DbConector();
                    MySqlConnection _MySqlConnection = obCnn.Conexion();
                    using (MySqlCommand _cmd = _MySqlConnection.CreateCommand())
                    {
                        _cmd.CommandText = "Select ArticuloId ,ArticuloDescripcion,ArticuloCantidad,ArticuloPrecio ,ArticuloFoto_GXI  From Articulo where ArticuloId = ?";
                        MySqlParameter _p = new MySqlParameter();
                        _p.Value = Id;
                        _cmd.Parameters.Add(_p);
                        using (MySqlDataReader _rdr = _cmd.ExecuteReader()) {
                            _Articulo.Id = -1;
                            while (_rdr.Read())
                            {
                                _Articulo.Id = _rdr.GetInt32(0);
                                _Articulo.Descripcion = _rdr.GetString(1);
                                _Articulo.Cantidad = _rdr.GetDouble(2) ;
                                _Articulo.Precio = _rdr.GetDouble(3);
                                _Articulo.FotoUrl = _rdr.GetString(4);
                            }
                            _Msg = JsonConvert.SerializeObject(_Articulo);
                        } 
                        
                    }
                }
                catch (Exception e)
                {
                    _Msg ="Error:" + e.Message;
                }
                




            } else
            {
                _Msg = "Error: El valor del Id:" + _IdArticulo + " debe ser numerico.";
            }
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(_Msg);
            return response;
        }
    }
}
