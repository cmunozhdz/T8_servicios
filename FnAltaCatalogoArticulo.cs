using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Pqc.Crypto.Utilities;

namespace TiendaServices
{
    public class FnAltaCatalogoArticulo
    {
        private readonly ILogger _logger;
        private MySqlConnection _cn;
        public FnAltaCatalogoArticulo(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FnAltaCatalogoArticulo>();
        }

        [Function("FnAltaCatalogoArticulo")]
        
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            //Lee los parametros de entrada.
            
            String _Msg = "Leyendo Variables.";

            Articulo _Art = new Articulo();
            
            String IdStr = req.Query["Id"];
            String Descripcion = req.Query["Descripcion"];
            String Cantidad = req.Query["Cantidad"];
            String Precio = req.Query["Precio"];
            String Foto = req.Query["Foto"];
            //Determina si el  producto se va a actualizar o se va  insertar 
            //El modo por default es insercion.
            String Modo = String.IsNullOrEmpty(req.Query["Modo"]) ? "INS" : req.Query["Modo"];
            Modo = Modo.Replace("\"", "").ToLower();



            int.TryParse(IdStr, out _Art.Id );
            double.TryParse(Cantidad, out _Art.Cantidad);
            double.TryParse(Precio, out _Art.Precio);
            _Art.Descripcion = Descripcion;
            if (String.IsNullOrEmpty(Foto)  )
            {
                _Art.Foto = new Byte[1024];
            }
            else
            {
                _Art.Foto = Encoding.UTF8.GetBytes(Foto);
            }
            //Una vez que ha leido los parametros impactaremos la base de datos.
            try
            {
                DbConector _conector = new DbConector();
                _cn = _conector.Conexion();
                MySqlParameter p ;
                if (_cn!=null)
                {
                    MySqlCommand qcmd = new MySqlCommand();
                    //using hace que al finalizar se libere el recurso
                   
                        qcmd.Connection = _cn;

                        if (Modo.Equals("ins")) {
                            //Insertamos un registro.

                            //Creamos la instrucion SQl que va a agregar un registro.
                            qcmd.CommandText = "Insert into Articulo ( ArticuloDescripcion,ArticuloCantidad,ArticuloPrecio ,ArticuloFoto,ArticuloFoto_GXI)  values (? ,? ,? , ? ,? ) ";
                            p = new MySqlParameter();


                            p.Value = _Art.Descripcion;
                            qcmd.Parameters.Add(p);

                            p = new MySqlParameter();


                            p.Value = _Art.Cantidad;
                            qcmd.Parameters.Add(p);

                            p = new MySqlParameter();


                            p.Value = _Art.Precio;
                            qcmd.Parameters.Add(p);

                            p = new MySqlParameter();


                            p.Value = _Art.Foto;
                            qcmd.Parameters.Add(p);

                            p = new MySqlParameter();


                            p.Value = Foto; //Almacena la url de la foto;

                            qcmd.Parameters.Add(p);

                            int intRows = qcmd.ExecuteNonQuery();
                            _Msg = "OK:Registros Agregados " + intRows;
                        }
                        if (Modo.Equals("upd")) {
                            //Actualizamos un registro.
                            if (_Art.Id != 0)
                            {
                                qcmd.CommandText = "Update  Articulo  Set ArticuloDescripcion=?,ArticuloCantidad=?,ArticuloPrecio=? ,ArticuloFoto_GXI=? where   ArticuloId=? ";
                                p = new MySqlParameter();
                                p.Value = _Art.Descripcion;
                                qcmd.Parameters.Add(p);
                                
                                p = new MySqlParameter();
                                p.Value = _Art.Cantidad;
                                qcmd.Parameters.Add(p);

                            p = new MySqlParameter();
                            p.Value = _Art.Precio;
                            qcmd.Parameters.Add(p);

                            p = new MySqlParameter();
                            p.Value = _Art.Foto;
                            qcmd.Parameters.Add(p);

                            p = new MySqlParameter();
                            p.Value = _Art.Id;
                            qcmd.Parameters.Add(p);

                            int intRows = qcmd.ExecuteNonQuery();
                                _Msg = "OK:Registros Actualizados " + intRows;
                            }
                            else
                            {
                                _Msg = "Error:El id de articulos es obligatorio.";
                            }


                        }
                        if (Modo.Equals("dlt")) {
                            //Borramos un registro
                            if (_Art.Id != 0)
                            {
                                qcmd.CommandText = "Delete From Articulo where   ArticuloId=? ";
                                p = new MySqlParameter();
                                p.Value = _Art.Id;
                                qcmd.Parameters.Add(p);
                            
                                int intRows = qcmd.ExecuteNonQuery();
                                _Msg = "OK:Registros Eliminados " + intRows;
                            }
                            else
                            {
                                _Msg = "Error:El id de articulos es obligatorio.";
                            }

                        }
                    
                }

                
            }
            catch(Exception ex)
            {
                _Msg = ex.ToString();
               
            }
            finally
            {
                //cerramos la conexion a sql en caso de que siga abierta
                
                if (_cn!=null)
                {
                    if (_cn.State== System.Data.ConnectionState.Open)
                    {
                        _cn.Close();
                    }
                        
                }
                

            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(_Msg);

            return response;
        }
    }
}
