using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using ZstdSharp.Unsafe;

namespace TiendaServices
{
    public class FnDeleteCarrito
    {
        private readonly ILogger _logger;

        public FnDeleteCarrito(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FnDeleteCarrito>();
        }

        [Function("FnDeleteCarrito")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            String _msg = "";
            String IdCarrito = req.Query["IdCarrito"];
            int _RowsAfectados;
            bool TransaccionAbierta = false;
            if (String.IsNullOrEmpty(IdCarrito)) {
                _msg = "Error:Proporcione un Id Carrito a eliminar";
            }
            else {
                DbConector _dbConector = new();
                MySqlConnection _Cn = _dbConector.Conexion();
                if (_Cn != null)
                {
                    using (_Cn)
                    {
                        MySqlTransaction mt = _Cn.BeginTransaction();
                        try
                        {
                            TransaccionAbierta = true;
                            using (mt)
                            {
                                //Actualiza las existencias de todos los materiales del carrito.
                                using (MySqlCommand cmd1 = _Cn.CreateCommand())
                                {
                                    cmd1.Transaction = mt;
                                    cmd1.CommandText = "Update  Articulo as A, CarritoArticulos as B " +
                                                        "set A.ArticuloCantidad = A.ArticuloCantidad + B.Cantidad "
                                                            + " where A.ArticuloId = B.ArticuloId and B.CarritoId =  " + IdCarrito;
                                    _RowsAfectados = cmd1.ExecuteNonQuery();
                                    if (_RowsAfectados > 0)
                                    {
                                        using (MySqlCommand cmd2 = _Cn.CreateCommand())
                                        {
                                            cmd2.Transaction = mt;
                                            cmd2.CommandText = "Delete From CarritoArticulos " +
                                                                " where CarritoId =  " + IdCarrito;

                                            _RowsAfectados = cmd2.ExecuteNonQuery();
                                            if (_RowsAfectados > 0)
                                            {
                                                using (MySqlCommand cmd3 = _Cn.CreateCommand())
                                                {
                                                    cmd3.Transaction = mt;
                                                    cmd3.CommandText = "Delete From  Carrito where CarritoId = " + IdCarrito;
                                                    _RowsAfectados = cmd3.ExecuteNonQuery();
                                                    if (_RowsAfectados == 1)
                                                    {
                                                        _msg = "Ok: Carrito Eliminado: " + IdCarrito;
                                                        mt.Commit();
                                                        TransaccionAbierta = false;
                                                    }
                                                    else
                                                    {
                                                        _msg = "Error: Carrito no se pudo eliminar : " + IdCarrito;
                                                        mt.Rollback();
                                                        TransaccionAbierta = false;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                _msg = "Error: no se pudieron eliminar los articulos del carrito. " + IdCarrito;
                                                mt.Rollback();
                                                TransaccionAbierta = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _msg = "Error: no se pudieron acttualizar las existencias. " + IdCarrito;
                                        mt.Rollback();
                                        TransaccionAbierta = false;
                                    }
                                }


                            }
                        }
                        catch (Exception e)
                        {

                            _msg = e.ToString();
                            if (TransaccionAbierta == true)
                            {
                                try
                                {
                                    mt.Rollback();
                                }   
                                catch (MySqlException e2)
                                {
                                    _msg = e2.ToString();
                                }
                                
                            }


                        }
                    }
                }
                else
                {
                    _msg = _dbConector.GetMessage();
                }
            }
             var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(_msg);

            return response;
        }
    }
}
