using HtmlAgilityPack;
using Newtonsoft.Json;
using SunatService.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using tessnet2;

namespace SunatService
{
    public class ConsultaRuc
    {
        /// <summary>
        /// Daryl Marcapura Apaza 2019-11-24
        /// </summary>
        #region Variables

        private static readonly CookieContainer _cookies = new CookieContainer();

        private static HttpClient httpClient = new HttpClient(new HttpClientHandler { UseProxy = true, });

        #endregion

        #region Metodos

        /// <summary>
        /// Metodo de consulta de RUC by DMA 2019-11-24
        /// </summary>
        /// <param name="nroRUC">Número de RUC del Contribuyente</param>
        /// <param name="nroIntentos">Número de Intentos</param>
        /// <returns>JSON de respuesta</returns>
        public static async Task<string> ConsultaContribuyente(string nroRUC, int nroIntentos = 3)
        {
            SunatDTO resultado = new SunatDTO();
            if (nroRUC.Length == 11)
            {
                for (int i = 0; i < nroIntentos; i++)
                {
                    resultado = await ConsultaSunat(nroRUC);
                    if (resultado.status == 1)
                        break;
                }
                return JsonConvert.SerializeObject(resultado);
            }
            else
            {
                resultado.status = 0;
                resultado.contribuyente = null;
                resultado.mensaje = "El número de RUC debe de contener 11 dígitos";
                return JsonConvert.SerializeObject(resultado);
            }
        }

        private static async Task<SunatDTO> ConsultaSunat(string ruc)
        {
            SunatDTO resultado = new SunatDTO();
            try
            {
                if (httpClient.BaseAddress == null)
                    httpClient.BaseAddress = new Uri("http://www.sunat.gob.pe/");

                #region Captcha

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.sunat.gob.pe/cl-ti-itmrconsruc/captcha?accion=image");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                httpWebRequest.CookieContainer = _cookies;
                Bitmap bitmap = new Bitmap(httpWebRequest.GetResponse().GetResponseStream());
                Tesseract tesseract = new Tesseract();
                string str = Path.Combine(Environment.CurrentDirectory, "Content/tessdata");
                if (!Directory.Exists(str))
                    Directory.CreateDirectory(str);
                tesseract.Init(str, "eng", false);
                foreach (Word word in tesseract.DoOCR(bitmap, Rectangle.Empty))
                    resultado.captcha = word.Text;

                #endregion

                //Consulta Sunat
                string rawResponseAsync = await getRawResponseAsync(string.Format("http://www.sunat.gob.pe/cl-ti-itmrconsruc/jcrS00Alias?accion=consPorRuc&nroRuc={0}&codigo={1}&tipdoc=1", ruc, resultado.captcha));
                
                #region Documento

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(rawResponseAsync.Trim());
                if (htmlDocument.DocumentNode.SelectNodes("//html[1]/head[1]/title[1]").FirstOrDefault<HtmlNode>().InnerText.Equals("Consulta RUC"))
                {
                    HtmlNode htmlNode = htmlDocument.DocumentNode.SelectNodes("//html[1]/body[1]/table[1]").FirstOrDefault();
                    if (htmlNode != null)
                    {
                        HtmlNodeCollection htmlNodeCollection1 = htmlNode.SelectNodes("tr");

                        #region Recorre respuesta y rellena propiedades

                        resultado.contribuyente = new Contribuyente();
                        foreach (HtmlNode htmlNode1 in htmlNodeCollection1)
                        {
                            HtmlNodeCollection htmlNodeCollection2 = htmlNode1.SelectNodes("td");
                            if (htmlNodeCollection2[0].InnerHtml.Contains("Estado del"))
                                resultado.contribuyente.Estado = beautifulString(htmlNodeCollection2[1].InnerText);
                            if (htmlNodeCollection2.Count == 2)
                            {
                                if (htmlNodeCollection2[0].InnerHtml.Contains("RUC:"))
                                {
                                    string[] strArray = htmlNodeCollection2[1].InnerText.Split('-');
                                    resultado.contribuyente.Ruc = beautifulString(strArray[0]);
                                    resultado.contribuyente.RazonSocial = beautifulString(strArray[1].TrimStart());
                                }
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Tipo Contribuyente:"))
                                    resultado.contribuyente.Tipo = htmlNodeCollection2[1].InnerText;
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Nombre Comercial:"))
                                    resultado.contribuyente.NombreComercial = beautifulString(htmlNodeCollection2[1].InnerHtml);
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Direcci&oacute;n del Domicilio Fiscal"))
                                {
                                    string[] strArray = htmlNodeCollection2[1].InnerText.Split('-');
                                    Regex regex = new Regex("  ", RegexOptions.RightToLeft);
                                    if (strArray.Length == 3)
                                    {
                                        int index = regex.Match(beautifulString(strArray[0])).Index;
                                        resultado.contribuyente.Direccion = beautifulString(strArray[0].Substring(0, index));
                                        resultado.contribuyente.Departamento = beautifulString(strArray[0].Substring(index));
                                    }
                                    if (strArray.Length > 3)
                                    {
                                        string empty = string.Empty;
                                        for (int index = 0; index < strArray.Length - 2; ++index)
                                            empty += strArray[index];
                                        int index1 = regex.Match(beautifulString(empty)).Index;
                                        if (index1 != 0)
                                        {
                                            resultado.contribuyente.Direccion = beautifulString(empty.Substring(0, index1));
                                            resultado.contribuyente.Departamento = beautifulString(empty.Substring(index1));
                                        }
                                        else
                                        {
                                            resultado.contribuyente.Direccion = beautifulString(empty);
                                            resultado.contribuyente.Departamento = "-";
                                        }
                                    }
                                    resultado.contribuyente.Provincia = beautifulString(strArray[strArray.Length - 2]);
                                    resultado.contribuyente.Distrito = beautifulString(strArray[strArray.Length - 1]);
                                }
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Sistema de Contabilidad:"))
                                    resultado.contribuyente.SisContabilidad = beautifulString(htmlNodeCollection2[1].InnerText);
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Emisor electr&oacute;nico desde:"))
                                    resultado.contribuyente.EmisorElectronicoDesde = beautifulString(htmlNodeCollection2[1].InnerText);
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Comprobantes Electr&oacute;nicos"))
                                {
                                    string[] strArray = htmlNodeCollection2[1].InnerText.Split(',');
                                    resultado.contribuyente.ComprobantesElectronicos = ((IEnumerable<string>)strArray).ToList<string>();
                                }
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Afiliado al PLE desde:"))
                                    resultado.contribuyente.AfiliadoPLEDesde = beautifulString(htmlNodeCollection2[1].InnerText);
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Actividad(es) Econ&oacute;mica(s):"))
                                {
                                    HtmlNodeCollection htmlNodeCollection3 = htmlNodeCollection2[1].SelectNodes("select/option");
                                    if (htmlNodeCollection3 != null)
                                    {
                                        foreach (HtmlNode htmlNode2 in (IEnumerable<HtmlNode>)htmlNodeCollection3)
                                        {
                                            resultado.contribuyente.ActividadesEconomicas.Add(beautifulString(htmlNode2.InnerText));
                                        }
                                    }
                                }
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Sistema de Emision Electronica:"))
                                {
                                    HtmlNodeCollection htmlNodeCollection3 = htmlNodeCollection2[1].SelectNodes("select/option");
                                    if (htmlNodeCollection3 != null)
                                    {
                                        foreach (HtmlNode htmlNode2 in (IEnumerable<HtmlNode>)htmlNodeCollection3)
                                        {
                                            resultado.contribuyente.SisEmisionElectronica.Add(beautifulString(htmlNode2.InnerText));
                                        }
                                    }
                                }
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Padrones"))
                                {
                                    HtmlNodeCollection htmlNodeCollection3 = htmlNodeCollection2[1].SelectNodes("select/option");
                                    if (htmlNodeCollection3 != null)
                                    {
                                        foreach (HtmlNode htmlNode2 in (IEnumerable<HtmlNode>)htmlNodeCollection3)
                                        {
                                            resultado.contribuyente.Padrones.Add(beautifulString(htmlNode2.InnerText));
                                        }
                                    }
                                }
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Condici&oacute;n del Contribuyente:"))
                                    resultado.contribuyente.Condicion = beautifulString(htmlNodeCollection2[1].InnerText);
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Comprobantes de Pago c/aut."))
                                {
                                    HtmlNodeCollection htmlNodeCollection3 = htmlNodeCollection2[1].SelectNodes("select/option");
                                    if (htmlNodeCollection3 != null)
                                    {
                                        foreach (HtmlNode htmlNode2 in (IEnumerable<HtmlNode>)htmlNodeCollection3)
                                        {
                                            resultado.contribuyente.ComprobantesPagoAutImpresion.Add(beautifulString(htmlNode2.InnerText));
                                        }
                                    }
                                }
                            }
                            if (htmlNodeCollection2.Count == 3 && htmlNodeCollection2[0].InnerHtml.Contains("Comprobantes de Pago c/aut."))
                            {
                                HtmlNodeCollection htmlNodeCollection3 = htmlNodeCollection2[1].SelectNodes("select/option");
                                if (htmlNodeCollection3 != null)
                                {
                                    foreach (HtmlNode htmlNode2 in (IEnumerable<HtmlNode>)htmlNodeCollection3)
                                    {
                                        resultado.contribuyente.ComprobantesPagoAutImpresion.Add(beautifulString(htmlNode2.InnerText));
                                    }
                                }
                                resultado.contribuyente.ObligadoEmitirCPE = beautifulString(htmlNodeCollection2[2].InnerText.Substring(22));
                            }
                            if (htmlNodeCollection2.Count == 4)
                            {
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Fecha de Inscripci&oacute;n:"))
                                {
                                    resultado.contribuyente.FechaInscripcion = beautifulString(htmlNodeCollection2[1].InnerText);
                                    resultado.contribuyente.FechaInicioActividades = beautifulString(htmlNodeCollection2[3].InnerText);
                                }
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Condici&oacute;n del Contribuyente:"))
                                {
                                    resultado.contribuyente.Condicion = beautifulString(htmlNodeCollection2[1].InnerText);
                                    resultado.contribuyente.ProfesionOficio = beautifulString(htmlNodeCollection2[3].InnerText);
                                }
                                if (htmlNodeCollection2[0].InnerHtml.Contains("Sistema de Emisi&oacute;n de Comprobante:"))
                                {
                                    resultado.contribuyente.SisEmisionComprobante = beautifulString(htmlNodeCollection2[1].InnerText);
                                    resultado.contribuyente.ActComercioExterior = beautifulString(htmlNodeCollection2[3].InnerText);
                                }
                            }
                        }

                        resultado.status = 1;
                        resultado.mensaje = "Se ha encontrado contribuyente!";
                        return resultado;

                        #endregion
                    }
                }

                #endregion

                return resultado;
            }
            catch (Exception ex)
            {
                resultado.status = 0;
                resultado.contribuyente = null;
                resultado.mensaje = ex.Message;
                return resultado;
            }
        }

        #endregion

        #region Metodos Utiles

        private static async Task<string> getRawResponseAsync(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.CookieContainer = _cookies;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            return await new StreamReader(httpWebRequest.GetResponse().GetResponseStream(), Encoding.GetEncoding("ISO-8859-1")).ReadToEndAsync();
        }

        private static string beautifulString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            string[] strArray1 = new string[3] { "\t", "\n", "\r" };
            string[] strArray2 = new string[3] { "", "", "" };
            for (int index = 0; index < strArray1.Length; ++index)
                text = Regex.Replace(text, strArray1[index], strArray2[index]);
            text = text.TrimStart().TrimEnd();
            return text;
        }

        #endregion

    }
}
