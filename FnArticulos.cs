using System.Linq.Expressions;
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
using System.Text;


namespace TiendaServices
{
    public class FnArticulos
    {
        private readonly ILogger _logger;

        public FnArticulos(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FnArticulos>();
        }

        [Function("FnArticulos")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            int _BufferSize=1024;
            int _posini = 0;
            
            String DescBusqueda = req.Query["Busqueda"];
            DbConector _conector = new DbConector();
            MySqlConnection _cnn = _conector.Conexion();
            if (_cnn != null)
            {
                List<Articulo> lArticulos = new List<Articulo>();
                var response = req.CreateResponse(HttpStatusCode.OK);
                
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                MySqlCommand QryArticulos = new MySqlCommand("Select ArticuloId ,ArticuloDescripcion,ArticuloCantidad,ArticuloPrecio ,ArticuloFoto_GXI  From Articulo where ArticuloDescripcion like ?", _cnn);
                MySqlParameter p = new MySqlParameter();
                p.Value = "%" + DescBusqueda + "%"; //Asigna el parametro del filtro a buscar 
                QryArticulos.Parameters.Add(p);

                using (MySqlDataReader TaArticulos = QryArticulos.ExecuteReader())
                {
                    
                    while (TaArticulos.Read())
                    {
                        
                        Articulo _tmpArticulo = new Articulo();
                        _tmpArticulo.Id = TaArticulos.GetInt32(0);
                        _tmpArticulo.Descripcion= TaArticulos.GetString(1);
                        _tmpArticulo.Cantidad = TaArticulos.GetDouble(2);
                        _tmpArticulo.Precio = TaArticulos.GetDouble(3);
                        _tmpArticulo.FotoUrl = TaArticulos.GetString(4);
                        _posini = 0;
                        /*int bufferSize = 1024; // Number of bytes to read at a time
                        byte[] ImageData = new byte[bufferSize];
                        long nBytesReturned, startIndex = 0;
                        int ordinal  =TaArticulos.GetOrdinal("ArticuloFoto");
                        string image = TaArticulos.IsDBNull(ordinal) ? null : TaArticulos.GetString("ArticuloFoto");
                        if (image != null)
                        {
                            startIndex = 0;

                            nBytesReturned = TaArticulos.GetBytes(
                            ordinal, // Column index of BLOB column
                            startIndex, // Start position of the byte to read
                            ImageData, // Byte array to recieve BLOB data
                            0, // Start index of the array
                            bufferSize // Size of buffer
                            );
                            while (nBytesReturned == bufferSize)
                            {
                                startIndex += bufferSize;
                                nBytesReturned = TaArticulos.GetBytes(ordinal, startIndex, ImageData, 0, bufferSize); // Number of bytes returned is assigned to nBytesReturned
                            }
                            _tmpArticulo.Foto=  ImageData;
                        }
                        */
                        lArticulos.Add(_tmpArticulo);

                        

                    }
                    
                    _cnn.Close(); //Cierra la conexion con Mysql 
                    String _json = JsonConvert.SerializeObject(lArticulos);
                    response.WriteString(_json);
                    return response;

                }
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
