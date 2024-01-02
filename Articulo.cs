using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiendaServices
{
    internal class Articulo
    {
        public int Id;
        public String  Descripcion;
        public double Cantidad;
        public double Precio;
        public byte[] Foto =new byte[1024];
        public String FotoUrl { get; set; }



    }
}
