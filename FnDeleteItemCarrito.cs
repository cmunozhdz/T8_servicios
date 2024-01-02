using System.Data.Common;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;

namespace TiendaServices
{
    public class FnDeleteItemCarrito
    {
        private readonly ILogger _logger;
        private MySqlConnection _cn;
        public FnDeleteItemCarrito(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FnDeleteItemCarrito>();
        }

        [Function("FnDeleteItemCarrito")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            String _msg = "";
            String IdCarrito = req.Query["IdCarrito"];
            String IdArticulo = req.Query["IdArticulo"];
            int rowsAfectados;
            bool TranIni=false ;
            DbConector _Db;
            MySqlTransaction _MySqlTransaction=null;
            if (String.IsNullOrEmpty(IdCarrito) || String.IsNullOrEmpty(IdArticulo) )
            {
                //Muestra mensaje de error .
                _msg = "Eliminacón no valida Carrito Id: " + IdCarrito + " IdArticulo:" + IdArticulo;
            }
            else
            {
                //Procesa la eliminacion
                //
                try
                {
                    _Db = new DbConector();
                    _cn = _Db.Conexion();
                    if (_cn!=null)
                    {
                        using  ( _MySqlTransaction = _cn.BeginTransaction())
                        {
                            TranIni = true;
                            //Actualiza las existencias sunando el produto que se va eliminar
                            using (MySqlCommand cmd1 = _cn.CreateCommand()  )
                            {
                                String SQLQRY = "Update  Articulo as A, CarritoArticulos as B " +
                                                "set A.ArticuloCantidad = A.ArticuloCantidad + B.Cantidad "
                                                + " where A.ArticuloId = B.ArticuloId and B.CarritoId = ? and "
                                                + " B.ArticuloId = ?";

                                cmd1.Transaction = _MySqlTransaction;
                                cmd1.CommandText = SQLQRY;
                                
                                MySqlParameter p = new MySqlParameter();
                                p.Value = IdCarrito;
                                cmd1.Parameters.Add(p);

                                p = new MySqlParameter();
                                p.Value = IdArticulo;
                                cmd1.Parameters.Add(p);

                                rowsAfectados = cmd1.ExecuteNonQuery(); //Ejecuta la instruccion y nos indica los renglones afectados.
                                if (rowsAfectados == 1 )
                                {
                                    //Elimina la linea del carrito.
                                    using (MySqlCommand cmd2 = _cn.CreateCommand())
                                    {
                                        SQLQRY = "delete from  CarritoArticulos where  CarritoId= ?  and ArticuloId=? ";
                                        cmd2.Transaction = _MySqlTransaction;
                                        cmd2.CommandText = SQLQRY;
                                        
                                        p = new MySqlParameter();
                                        p.Value = IdCarrito;
                                        cmd2.Parameters.Add(p);


                                        p = new MySqlParameter();
                                        p.Value = IdArticulo;
                                        cmd2.Parameters.Add(p);


                                        rowsAfectados = cmd2.ExecuteNonQuery();
                                        if (rowsAfectados ==1 )
                                        {
                                            
                                            _MySqlTransaction.Commit();
                                            _msg = "Ok: Se elimino el Articulo:" + IdArticulo + " del Carrito:" + IdCarrito;
                                            TranIni = false;
                                        }
                                        else
                                        {
                                            _MySqlTransaction.Rollback();
                                            _msg = "La eliminación del carrito fallo IdCarrito:" + IdCarrito + "Articulo:" + IdArticulo;
                                            TranIni = false;


                                        }
                                    }
                                }
                                else
                                {
                                    //El proceso fallo.
                                    _msg = "Error: La actualización de existencias fallo Articulo:" + IdArticulo;
                                    _MySqlTransaction.Rollback();
                                    TranIni = false;
                                }

                            }

                            

                        }
                    }
                    else
                    {
                        //La conexion con My sql fallo muestra el mensaje 
                        _msg = "Error: " + _Db.GetMessage();
                    }
                   
                }
                catch (Exception e )
                {
                    _msg = e.ToString();
                    if (TranIni)
                    {
                        if (_MySqlTransaction!=null )
                        {
                            _MySqlTransaction.Rollback();
                        }
                        

                    }
                }
                finally
                {
                    if (_cn!=null)
                    {
                        if (_cn.State == System.Data.ConnectionState.Open )
                        {
                            _cn.Close();
                        }
                    }
                }
            }


                var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(_msg);

            return response;
        }
    }
}
