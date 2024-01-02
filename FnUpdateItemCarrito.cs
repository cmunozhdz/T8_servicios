using System.Linq.Expressions;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace TiendaServices
{
    public class FnUpdateItemCarrito
    {
        private readonly ILogger _logger;

        public FnUpdateItemCarrito(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FnUpdateItemCarrito>();
        }

        [Function("FnUpdateItemCarrito")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            String _IdCarrito = req.Query["IdCarrito"];
            String _IdArticulo = req.Query["IdArticulo"];
            String _CantidadNva = req.Query["Cantidad"];
            String SQLQry;
            Double  CantidadNva = 0;
            Double CantidadActual = 0;
            String _Msg = "";
            Double Diferencia;
            int RowsAfectados;
            MySqlConnection _cn = new MySqlConnection();
            
            bool TransaccionAbierta = false;
            if (String.IsNullOrEmpty(_IdCarrito) || String.IsNullOrEmpty(_IdArticulo)
                || double.TryParse(_CantidadNva, out CantidadNva) == false)
            {
                _Msg = "Error en la parametrizacion IdCarrito:" + _IdCarrito + " IdArticulo:" + _IdArticulo + "Cantidad:" + _CantidadNva;
            }
            else 
            { 
            
                DbConector _dbConector = new DbConector();
                _cn = _dbConector.Conexion();
                if (_cn != null)
                {
                    using (_cn )
                    {

                        using (MySqlTransaction _mySqlTransaction = _cn.BeginTransaction())
                        {
                            try
                            {
                                //Calcula la diferencia 
                                TransaccionAbierta = true;
                                //Lee en la base de datos la cantidad actual 
                                using (MySqlCommand _Cmd = _cn.CreateCommand())
                                {
                                    _Cmd.CommandText = "Select B.Cantidad   from Carrito A , CarritoArticulos B, Articulo C where A.CarritoId= B.CarritoId and C.ArticuloId = B.ArticuloId and A.CarritoId= ? and B.ArticuloId = ?  ";
                                    MySqlParameter p = new MySqlParameter();
                                    p.Value = _IdCarrito;
                                    _Cmd.Parameters.Add(p);

                                    p = new MySqlParameter();
                                    p.Value = _IdArticulo;
                                    _Cmd.Parameters.Add(p);
                                    using (MySqlDataReader _mrd = _Cmd.ExecuteReader())
                                    {
                                        while (_mrd.Read())
                                        {
                                            CantidadActual = _mrd.GetDouble(0);
                                        }
                                    }
                                }
                                if (CantidadActual == CantidadNva )
                                {
                                    _Msg = "Error: No hay cambios a aplicar.";
                                    _mySqlTransaction.Rollback();
                                }
                                else { 
                                if (CantidadActual < CantidadNva)
                                {
                                    //Cuando la cantidad nueva es mayor hace la reserva adicional
                                    SQLQry = new string("");
                                    Diferencia = CantidadNva - CantidadActual;
                                    SQLQry = "Update Articulo set ArticuloCantidad = ArticuloCantidad - " + Diferencia + " where ArticuloId= " + _IdArticulo
                                                + " and ArticuloCantidad - " + Diferencia + " >= 0"; //Agregamos condicion en el qry para garantizar que las existencias sean positivas
                                    using (MySqlCommand _CmdArticulo = _cn.CreateCommand())
                                    {
                                        _CmdArticulo.CommandText = SQLQry;
                                        _CmdArticulo.Transaction = _mySqlTransaction;
                                        RowsAfectados = _CmdArticulo.ExecuteNonQuery();

                                    }
                                    if (RowsAfectados == 0)
                                    {
                                        _Msg = "Error:Las existencias no pudieron actualizarse por que el articulo no existe o no hay existencias suficientes";
                                        _mySqlTransaction.Rollback();
                                        TransaccionAbierta = false;
                                    }
                                    else
                                    {
                                        SQLQry = new string("");
                                        SQLQry = SQLQry + "Update CarritoArticulos  Set Cantidad =" + CantidadNva;
                                        SQLQry = SQLQry + " where ArticuloId= " + _IdArticulo;
                                        SQLQry = SQLQry + " and  CarritoId= " + _IdCarrito;
                                        using (MySqlCommand CmdCarrito = _cn.CreateCommand())
                                        {
                                            CmdCarrito.Transaction = _mySqlTransaction;
                                            CmdCarrito.CommandText = SQLQry;
                                            RowsAfectados = CmdCarrito.ExecuteNonQuery();

                                        }
                                        if (RowsAfectados > 0)
                                        {
                                            _mySqlTransaction.Commit();
                                            _Msg = "OK:Carrito Actualizado IdCarrito:" + _IdCarrito + "IdArticulo:" + _IdArticulo;
                                        }
                                        else
                                        {
                                            _mySqlTransaction.Rollback();
                                            _Msg = "Error: La actualización fallo IdCarrito:" + _IdCarrito + "IdArticulo:" + _IdArticulo;
                                        }
                                    }






                                }
                                if (CantidadActual > CantidadNva)
                                {
                                    //Cuando la cantidad es menor hace el incremento de existencias
                                    SQLQry = new string("");
                                    Diferencia = CantidadActual - CantidadNva;
                                    SQLQry = "Update Articulo set ArticuloCantidad = ArticuloCantidad + " + Diferencia + " where ArticuloId= " + _IdArticulo;
                                    //Agregamos condicion en el qry para garantizar que las existencias sean positivas
                                    using (MySqlCommand _CmdArticulo = _cn.CreateCommand())
                                    {
                                        _CmdArticulo.CommandText = SQLQry;
                                        _CmdArticulo.Transaction = _mySqlTransaction;
                                        RowsAfectados = _CmdArticulo.ExecuteNonQuery();

                                    }
                                    if (RowsAfectados == 0)
                                    {
                                        _Msg = "Error:Las existencias no pudieron actualizarse por que el articulo no existe ";
                                        _mySqlTransaction.Rollback();
                                        TransaccionAbierta = false;
                                    }
                                    else
                                    {
                                        SQLQry = new string("");
                                        SQLQry = SQLQry + "Update CarritoArticulos  Set Cantidad =" + CantidadNva;
                                        SQLQry = SQLQry + " where ArticuloId= " + _IdArticulo;
                                        SQLQry = SQLQry + " and  CarritoId= " + _IdCarrito;
                                        using (MySqlCommand CmdCarrito = _cn.CreateCommand())
                                        {
                                            CmdCarrito.Transaction = _mySqlTransaction;
                                            CmdCarrito.CommandText = SQLQry;
                                            RowsAfectados = CmdCarrito.ExecuteNonQuery();

                                        }
                                        if (RowsAfectados > 0)
                                        {
                                            _mySqlTransaction.Commit();
                                            _Msg = "OK:Carrito Actualizado IdCarrito:" + _IdCarrito + "IdArticulo:" + _IdArticulo;
                                        }
                                        else
                                        {
                                            _mySqlTransaction.Rollback();
                                            _Msg = "Error: La actualización fallo IdCarrito:" + _IdCarrito + "IdArticulo:" + _IdArticulo;
                                        }
                                    }

                                }
                                    // Cuando la cantidad es menor aumenta 
                                    // y actualiza el monto del carrito
                                }
                            }
                            catch (Exception e)
                            {
                                _Msg = "Error:" + e.ToString();
                                if (_cn != null)
                                {
                                    if (TransaccionAbierta)
                                    {
                                        _mySqlTransaction.Rollback();

                                    }
                                }
                            }
                        }
                    }

                }
                else
                {
                    _Msg = _dbConector.GetMessage();
                }
            }

                    
                    

            

            


            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(_Msg);

            return response;
        }
    }
}
