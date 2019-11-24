using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunatService.Entities
{
    public class Contribuyente
    {
        /// <summary>
        /// Número de RUC
        /// </summary>
        public string Ruc { get; set; }

        /// <summary>
        /// Razon Social
        /// </summary>
        public string RazonSocial { get; set; }

        /// <summary>
        /// Nombre Comercial
        /// </summary>

        public string NombreComercial { get; set; }

        /// <summary>
        /// Tipo de Contribuyente
        /// </summary>

        public string Tipo { get; set; }

        /// <summary>
        /// Estado de Contribuyente
        /// </summary>

        public string Estado { get; set; }

        /// <summary>
        /// Condición del Contribuyente
        /// </summary>
        public string Condicion { get; set; }

        /// <summary>
        /// Fecha de Inscripción
        /// </summary>
        public string FechaInscripcion { get; set; }

        /// <summary>
        /// Fecha de Inicio de Actividades
        /// </summary>
        public string FechaInicioActividades { get; set; }

        /// <summary>
        /// Dirección del Contribuyente
        /// </summary>
        public string Direccion { get; set; }

        /// <summary>
        /// Distrito del Contribuyente
        /// </summary>
        public string Distrito { get; set; }

        /// <summary>
        /// Provincia del Contribuyente
        /// </summary>
        public string Provincia { get; set; }

        /// <summary>
        /// Departamento del Contribuyente
        /// </summary>
        public string Departamento { get; set; }

        /// <summary>
        /// Profesión u Oficio
        /// </summary>
        public string ProfesionOficio { get; set; }

        /// <summary>
        /// Sistema de emisión del Comprobante
        /// </summary>
        public string SisEmisionComprobante { get; set; }

        /// <summary>
        /// Sistema de Contabilidad
        /// </summary>
        public string SisContabilidad { get; set; }

        /// <summary>
        /// Actividad comercio Exterior
        /// </summary>
        public string ActComercioExterior { get; set; }

        /// <summary>
        /// Listado de actividades Economicas
        /// </summary>
        public List<string> ActividadesEconomicas { get; set; }

        /// <summary>
        /// Listado de Comprobantes de Pago
        /// </summary>
        public List<string> ComprobantesPagoAutImpresion { get; set; }

        /// <summary>
        /// Obligado emitir comprobante Electronico
        /// </summary>
        public string ObligadoEmitirCPE { get; set; }

        /// <summary>
        /// Lista de Sistemas de emisión Electronica
        /// </summary>
        public List<string> SisEmisionElectronica { get; set; }

        /// <summary>
        /// Emisor Electronico desde
        /// </summary>
        public string EmisorElectronicoDesde { get; set; }

        /// <summary>
        /// Lista de Comprobantes Electronicos
        /// </summary>
        public List<string> ComprobantesElectronicos { get; set; }

        /// <summary>
        /// Fecha de inicio de Afiliación
        /// </summary>
        public string AfiliadoPLEDesde { get; set; }

        /// <summary>
        /// Lista de Padrones
        /// </summary>
        public List<string> Padrones { get; set; }

        public Contribuyente()
        {
            Ruc = "-";
            RazonSocial = "-";
            NombreComercial = "-";
            Tipo = "-";
            Estado = "-";
            Condicion = "-";
            FechaInscripcion = "-";
            FechaInicioActividades = "-";
            Direccion = "-";
            Distrito = "-";
            Provincia = "-";
            Departamento = "-";
            ProfesionOficio = "-";
            SisEmisionComprobante = "-";
            SisContabilidad = "-";
            ActComercioExterior = "-";
            ActividadesEconomicas = new List<string>();
            ComprobantesPagoAutImpresion = new List<string>();
            ObligadoEmitirCPE = "-";
            SisEmisionElectronica = new List<string>();
            EmisorElectronicoDesde = "-";
            ComprobantesElectronicos = new List<string>();
            AfiliadoPLEDesde = "-";
            Padrones = new List<string>();
        }
    }
}
