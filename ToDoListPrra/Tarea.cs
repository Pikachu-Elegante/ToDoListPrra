using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToDoListPrra
{
    public class Tarea
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public bool EsUrgente { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Completada { get; set; }

        internal void Add(Tarea tareasCompletadas)
        {
            throw new NotImplementedException();
        }
    }

}