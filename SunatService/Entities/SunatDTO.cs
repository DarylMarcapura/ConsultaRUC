using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunatService.Entities
{
    public class SunatDTO
    {
        /// <summary>
        /// Status 1 consulta correcta, status 0 consulta incorrecta
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// Mensaje de consulta
        /// </summary>
        public string mensaje { get; set; }

        /// <summary>
        /// Codigo Captcha resuelto
        /// </summary>
        public string captcha { get; set; }

        /// <summary>
        /// Contribuyente DTO
        /// </summary>
        public Contribuyente contribuyente { get; set; }

    }
}
