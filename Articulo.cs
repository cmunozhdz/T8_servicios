using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiendaServices
{
/// <summary>
/// Define la estructura de un articulo del catalogo.
/// </summary>
    internal class Articulo
    {
        public int Id;
        public String  Descripcion;
        public double Cantidad;
        public double Precio;
        public byte[] Foto =new byte[1024]; //Ya no se usa por cuestiones de rendimiento el cliente
                                            //almacena en un url el archivo creado y su extension.
                                            //evitamos estar convirtiendo de bytes a string 64 y viceversa.
        public String FotoUrl { get; set; } //Determina la url  donde se guarda la foto.



    }
}
