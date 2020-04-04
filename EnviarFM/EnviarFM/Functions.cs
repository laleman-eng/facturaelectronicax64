using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace EnviarFM.Functions
{
    public class TFunctions
    {

        //Funcion registra log
        public void AddLog(String Mensaje)
        {
            StreamWriter Arch;
            //Exe: String := 
            String sPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
            String NomArch;
            String NomArchB;
            NomArch = "\\VDLog_" + String.Format("{0:yyyy-MM-dd}", DateTime.Now) + ".log";
            Arch = new StreamWriter(sPath + NomArch, true);
            NomArchB = sPath + "\\VDLog_" + String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1)) + ".log";
            //Elimina archivo del dia anterior
            //if (System.IO.File.Exists(NomArchB))
            //    System.IO.File.Delete(NomArchB);

            try
            {
                Arch.WriteLine(String.Format("{0:dd-MM-yyyy HH:mm:ss}", DateTime.Now) + " " + Mensaje);
            }
            finally
            {
                Arch.Flush();
                Arch.Close();
            }
        }

        public String DatosConfig(String Valor0, String Valor)
        {
            XmlDocument xDoc;
            XmlNodeList Configuracion;
            XmlNodeList lista;
            TFunctions Func;
            String sPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
            String _result = "";

            try
            {
                xDoc = new XmlDocument();
                xDoc.Load(sPath + "\\Config.xml");
                Configuracion = xDoc.GetElementsByTagName("Configuracion");
                lista = ((XmlElement)Configuracion[0]).GetElementsByTagName(Valor0);

                foreach (XmlElement nodo in lista)
                {
                    var nArchivos = nodo.GetElementsByTagName(Valor);
                    _result = (String)(nArchivos[0].InnerText);
                }

                return _result;
            }
            catch (Exception w)
            {
                Func = new TFunctions();
                Func.AddLog("DatosSFTP: " + w.Message + " ** Trace: " + w.StackTrace);
                return "";
            }
        }

    }
}
