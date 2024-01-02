using System.Linq.Expressions;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using ZstdSharp.Unsafe;

namespace TiendaServices
{
    public class FnAgregarItemCarrito
    {
        private readonly ILogger _logger;
        MySqlConnection _cn;
        public FnAgregarItemCarrito(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FnAgregarItemCarrito>();
        }

        [Function("FnAgregarItemCarrito")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            String IdCarrito = req.Query["IdCarrito"];
            String IdArticulo = req.Query["IdArticulo"];
            String Cantidad  = req.Query["Cantidad"];
            String Precio  = req.Query["Precio"];
            String SQLQry;
            String _msg = "";
            bool _tranini ;
            int renglones;

            if ( String.IsNullOrEmpty(IdCarrito) || String.IsNullOrEmpty(IdArticulo) || String.IsNullOrEmpty(Cantidad) || String.IsNullOrEmpty(Precio)   )
            {
                //Se encontro un error en el proceso los datos sob obligatorios.
                _msg = "Error:Proporcione los datos faltantes: IdCarrito:" + IdCarrito + " IdArticulo:" + IdArticulo + " Cantidad:" + Cantidad + "Precio:" + Precio;
            }
            else
            {
                /*
                 * Intentamos impactar las tablas del carrito y exsistencias 
                 * Por lo que preparamos una transacion 
                 * Impactamos la tabla del carrito, la de existencias 
                 * Confirmamos 
                 */
                DbConector db = new DbConector();
                
                _cn = db.Conexion();
                //validamos conexion exitosa
                if (_cn==null )
                {
                    _msg = db.GetMessage(); //Obtiene el mensaje de error al conectarse.

                }
                else
                {
                    using (MySqlTransaction mySqlTransaction = _cn.BeginTransaction() )
                    {
                        _tranini = true;
                        try
                        {
                            SQLQry = new string("");
                            SQLQry = SQLQry + "Insert into CarritoArticulos (CarritoId,ArticuloId,Cantidad) ";
                            SQLQry = SQLQry + " Select " + IdCarrito + " CarritoId,ArticuloId,";
                            SQLQry = SQLQry + Cantidad + " ArticuloCantidad from Articulo";
                            SQLQry = SQLQry + " where ArticuloCantidad >= " + Cantidad;
                            SQLQry = SQLQry + " and  ArticuloId= " + IdArticulo;

                            MySqlCommand QrCarrito = new MySqlCommand(SQLQry, _cn);
                            //Ejecuta y valida que el qry afecto un registro.
                            renglones = QrCarrito.ExecuteNonQuery();
                            if (renglones == 1)
                            {
                                // Impactamos la segunda tabla.
                                SQLQry = "Update Articulo set ArticuloCantidad = ArticuloCantidad - " + Cantidad + " where ArticuloId= " + IdArticulo
                                            + " and ArticuloCantidad - " + Cantidad + " >= 0"; //Agregamos condicion en el qry para garantizar que las existencias sean positivas
                                MySqlCommand QrCarrito2 = new MySqlCommand(SQLQry, _cn);
                                renglones = QrCarrito2.ExecuteNonQuery();
                                if (renglones == 1)
                                {
                                    // el proceso es correcto confirmamos transaccion.
                                    mySqlTransaction.Commit();
                                    _tranini = false;
                                    _msg = "OK:El articulo " + IdArticulo + " se agrego con exito";
                                }
                            }
                            else
                            {
                                mySqlTransaction.Rollback();
                                _tranini = false;
                                _msg = "Error:El articulo " + IdArticulo + " no se puede agregar al carito ya que no existe o no hay existencias.";


                            }
                        }
                        catch (Exception e)
                        {
                            _msg = e.ToString();
                            mySqlTransaction.Rollback();
                            _tranini = false;
                        }
                        finally
                        {
                            if (_cn!=null)
                            {
                                
                                if (_cn.State== System.Data.ConnectionState.Open)
                                {
                                    _cn.Close();
                                }
                                    

                            }
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
