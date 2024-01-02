using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiendaServices
{
    internal class ItemCarrito
    {
        public int _IdCarrito;
        //public datetime Fecha; 
        public int IdArticulo;
        public String Descripcion;
        public double Cantidad;
        public double Precio;
        public String Archivo;
        public byte[] Foto ;
        public String SFoto; // conversion de foto a string 
    }
}
