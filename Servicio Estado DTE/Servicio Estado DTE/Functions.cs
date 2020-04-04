using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using SAPbobsCOM;

namespace Servicio_Estado_DTE.Functions
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

        public String DatosConfig(String Valor0, String Valor, XmlDocument xDoc)
        {
            XmlNodeList Configuracion;
            XmlNodeList lista;
            TFunctions Func;
            String _result = "";

            try
            {
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
                Func.AddLog("DatosConfig: " + w.Message + " ** Trace: " + w.StackTrace);
                return "";
            }
        }

        //public String DesEncriptar(String _cadenaAdesencriptar)
        //{
        //    String sresult;
        //    System.Byte[] decryted;

        //    sresult = System.String.Empty;
        //    decryted = Convert.FromBase64String(_cadenaAdesencriptar);
        //    //result = System       .Text.Encoding.Unicode.GetString(decryted, 0, decryted.ToArray().Length);
        //    sresult = System.Text.Encoding.Unicode.GetString(decryted);
        //    return sresult;
        //}

        public String Encriptar(String _cadenaAencriptar)
        {
            System.String sresult;
            System.Byte[] encryted;

            sresult = System.String.Empty;
            encryted = System.Text.Encoding.Unicode.GetBytes(_cadenaAencriptar);
            sresult = Convert.ToBase64String(encryted);
            return sresult;
        }//fin Encriptar


        public String DesEncriptar(String _cadenaAdesencriptar)
        {
            String sresult;
            System.Byte[] decryted;

            sresult = System.String.Empty;
            decryted = Convert.FromBase64String(_cadenaAdesencriptar);
            //result = System       .Text.Encoding.Unicode.GetString(decryted, 0, decryted.ToArray().Length);
            sresult = System.Text.Encoding.Unicode.GetString(decryted);
            return sresult;
        }
        
    }
}
