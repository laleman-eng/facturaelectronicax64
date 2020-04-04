using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Timers;
using SAPbobsCOM;
using System.Globalization;
using System.Net;
using System.Xml;
using ServiceStack.Text;
//using System.Net.Http;
using System.Net.Mail;
//using System.Core;
using Microsoft.CSharp;
using Servicio_Estado_DTE.Functions;
using System.Net.NetworkInformation;
using System.Data.Sql;
using System.Data.SqlClient;
using Newtonsoft.Json;
using SAPbobsCOM;

namespace Servicio_Estado_DTE
{
    public partial class Service1 : ServiceBase
    {
        public Timer Tiempo;
        public String s;
        public SAPbobsCOM.CompanyClass oCompany;
        public SAPbobsCOM.Recordset oRecordSet = null;
        public CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        public TFunctions Func;
        public String sVersion = "1.0";
        public String TaxIdNum;
        public Boolean RunningSQLServer = false;
        public Boolean bMultiSoc;
        public String OP21 = "";
        public String OP27 = "";
        public String OP28 = "";
        public String OP29 = "";
        public String OP30 = "";
        public String OP36 = "";
        public String Glob_Servidor;
        public String Glob_Licencia;
        public String Glob_UserSAP;
        public String Glob_PassSAP;
        public String Glob_SQL;
        public String Glob_UserSQL;
        public String Glob_PassSQL;
        public XmlDocument xDoc;
        public XmlNodeList Configuracion;
        public XmlNodeList lista;

        public Service1()
        {
            InitializeComponent();
            Tiempo = new Timer();
            Tiempo.Interval = 30000;
            Tiempo.Elapsed += new ElapsedEventHandler(tiempo_elapsed);


        }

        protected override void OnStart(string[] args)
        {
            Tiempo.Enabled = true;
        }

        protected override void OnStop()
        {
            Tiempo.Stop();
            Tiempo.Enabled = false;
        }

        public void tiempo_elapsed(object sender, EventArgs e)
        {
            Func = new TFunctions();
            Tiempo.Enabled = false;
            XmlNodeList BasesSAP;
            XmlNodeList BaseSAP;
            String sPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
            String BaseName;
            String UserWS = "";
            String PassWS = "";
            String CompnyName = "";
            Boolean bEnviarMail;


            try
            {
                string contenido = String.Empty;
                contenido = File.ReadAllText(sPath + "\\Config.txt");
                contenido = Func.DesEncriptar(contenido);
                xDoc = new XmlDocument();
                xDoc.LoadXml(contenido);
                s = Func.DatosConfig("SistemaSAP", "SAP", xDoc);
                if (s == "")
                    throw new Exception("Debe parametrizar si SAP es SQL o HANA en xml de Configuración, en tag SistemaSAP -> SAP (SQL o HANA)");
                else if (s == "HANA")
                    RunningSQLServer = false;
                else
                    RunningSQLServer = true;

                //Consulta si envia Mail o no
                s = Func.DatosConfig("Mail", "EnviarMail", xDoc);
                if (s.Trim() == "Si")
                    bEnviarMail = true;
                else
                    bEnviarMail = false;

                //Consultar estado DTE enviado por el addon
                s = Func.DatosConfig("EasyDoc", "Procesa21", xDoc).Replace("'", "");
                //Func.AddLog("Procesa 21 " + s);
                if (s.Trim() == "Si")
                {
                    s = Func.DatosConfig("EasyDoc", "OP21", xDoc).Replace("'", "");
                    if (s == "")
                        throw new Exception("Debe ingresar URL en xml de Configuración, en tag EasyDoc -> OP21");
                    else
                        OP21 = s;
                }
                else
                    OP21 = "";
                //Func.AddLog(OP21);

                //rescate de informacion documento proveedores 1 a 1
                s = Func.DatosConfig("EasyDoc", "Procesa27", xDoc).Replace("'", "");
                if (s.Trim() == "Si")
                {
                    s = Func.DatosConfig("EasyDoc", "OP27", xDoc).Replace("'", "");
                    if (s == "")
                        throw new Exception("Debe ingresar URL en xml de Configuración, en tag EasyDoc -> OP27");
                    else
                        OP27 = s;
                }
                else
                    OP27 = "";

                //rescate de informacion documento emitidos 1 a 1
                s = Func.DatosConfig("EasyDoc", "Procesa28", xDoc).Replace("'", "");
                if (s.Trim() == "Si")
                {
                    s = Func.DatosConfig("EasyDoc", "OP28", xDoc).Replace("'", "");
                    if (s == "")
                        throw new Exception("Debe ingresar URL en xml de Configuración, en tag EasyDoc -> OP28");
                    else
                        OP28 = s;
                }
                else
                    OP28 = "";

                //Documentos Emitidos,  uso de web services de forma directa: para obtener el estado del documento en el SII, su fecha de recepcion y el registro de aceptacion o reclamo que pueda tener el documento
                s = Func.DatosConfig("EasyDoc", "Procesa29", xDoc).Replace("'", "");
                if (s.Trim() == "Si")
                {
                    s = Func.DatosConfig("EasyDoc", "OP29", xDoc).Replace("'", "");
                    if (s == "")
                        throw new Exception("Debe ingresar URL en xml de Configuración, en tag EasyDoc -> OP29");
                    else
                        OP29 = s;
                }
                else
                    OP29 = "";

                //Documentos Proveedores,  uso de web services de forma directa: para obtener validez del documento en el SII, su fecha de recepcion y el registro de aceptacion o reclamo que pueda tener el documento
                s = Func.DatosConfig("EasyDoc", "Procesa30", xDoc).Replace("'", "");
                if (s.Trim() == "Si")
                {
                    s = Func.DatosConfig("EasyDoc", "OP30", xDoc).Replace("'", "");
                    if (s == "")
                        throw new Exception("Debe ingresar URL en xml de Configuración, en tag EasyDoc -> OP30");
                    else
                        OP30 = s;
                }
                else
                    OP30 = "";


                //rescate documentos de compra por intervalo de fechas
                s = Func.DatosConfig("EasyDoc", "Procesa36", xDoc).Replace("'", "");
                if (s.Trim() == "Si")
                {
                    s = Func.DatosConfig("EasyDoc", "OP36", xDoc).Replace("'", "");
                    if (s == "")
                        throw new Exception("Debe ingresar URL en xml de Configuración, en tag EasyDoc -> OP36");
                    else
                        OP36 = s;
                }
                else
                    OP36 = "";

                //Registro de Aceptacion o Reclamo, el uso de este web service es para dejar el registro de aceptacion o reclamo en el SII el cual ademas dejara un registro en las tablas de easydoc.
                //se debe pasar el paaemetro extra "CODAR", para indicar el registro deseado
                //s = Func.DatosConfig("EasyDoc", "Procesa31").Replace("'", "");
                //if (s.Trim() == "Y")
                //{
                //    s = Func.DatosConfig("EasyDoc", "OP31").Replace("'", "");
                //    if (s == "")
                //        throw new Exception("Debe ingresar URL en xml de Configuración, en tag EasyDoc -> OP31");
                //    else
                //        OP31 = s;
                //}
                //else
                //    OP31 = "";

                try
                {
                    Func.AddLog("Inicio");
                    //Func.AddLog("Inicio1");

                    if (DatosConexion())
                    {
                        BasesSAP = xDoc.GetElementsByTagName("BasesSAP");
                        BaseSAP = ((XmlElement)BasesSAP[0]).GetElementsByTagName("BaseSAP");
                        foreach (XmlElement nodo in BaseSAP)
                        {
                            try
                            {
                                BaseName = nodo.InnerText;
                                BaseName = BaseName.Trim();
                                if (oCompany == null)
                                    oCompany = new SAPbobsCOM.CompanyClass();
                                else
                                {
                                    oCompany.Disconnect();
                                    oCompany = null;
                                    oCompany = new SAPbobsCOM.CompanyClass();
                                }

                                if (ConectarBaseSAP(BaseName.Trim()))
                                {
                                    Func.AddLog("Conectado a SAP -> Base Datos " + BaseName.Trim());
                                    if (oRecordSet == null)
                                        oRecordSet = (SAPbobsCOM.Recordset)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                                    //Func.AddLog("Inicio2");
                                    if (RunningSQLServer)
                                        s = @"SELECT ISNULL(TaxIdNum,'') TaxIdNum, CompnyName FROM OADM ";
                                    else
                                        s = @"SELECT IFNULL(""TaxIdNum"",'') ""TaxIdNum"", ""CompnyName"" FROM ""OADM"" ";
                                    oRecordSet.DoQuery(s);
                                    //Func.AddLog(s);
                                    if (oRecordSet.RecordCount == 0)
                                        throw new Exception("Debe ingresar RUT de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1");
                                    else
                                    {
                                        if (((System.String)oRecordSet.Fields.Item("TaxIdNum").Value).Trim() == "")
                                            throw new Exception("Debe ingresar RUT de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1");
                                        TaxIdNum = ((System.String)oRecordSet.Fields.Item("TaxIdNum").Value).Trim();
                                        CompnyName = ((System.String)oRecordSet.Fields.Item("CompnyName").Value).Trim();
                                    }


                                    if (RunningSQLServer)
                                        s = @"SELECT ISNULL(U_MultiSoc,'N') 'MultiSoc', ISNULL(U_UserWSCL,'') 'UserWS', ISNULL(U_PassWSCL,'') 'PassWS' FROM [@VID_FEPARAM]";
                                    else
                                        s = @"SELECT IFNULL(""U_MultiSoc"",'N') ""MultiSoc"", IFNULL(""U_UserWSCL"",'') ""UserWS"", IFNULL(""U_PassWSCL"",'') ""PassWS"" FROM ""@VID_FEPARAM"" ";
                                    oRecordSet.DoQuery(s);
                                    if (oRecordSet.RecordCount == 0)
                                        throw new Exception("No se encuentra parametrizado si es multisociedad, Gestión -> Definiciones -> Factura Electronica -> Parametros");
                                    else
                                    {
                                        if (((System.String)oRecordSet.Fields.Item("MultiSoc").Value).Trim() == "Y")
                                            bMultiSoc = true;
                                        else
                                            bMultiSoc = false;

                                        if (((System.String)oRecordSet.Fields.Item("UserWS").Value).Trim() != "")
                                            UserWS = Func.DesEncriptar(((System.String)oRecordSet.Fields.Item("UserWS").Value).Trim());
                                        if (((System.String)oRecordSet.Fields.Item("PassWS").Value).Trim() != "")
                                            PassWS = Func.DesEncriptar(((System.String)oRecordSet.Fields.Item("PassWS").Value).Trim());
                                    }

                                    //Func.AddLog(OP21);
                                    if (OP21 != "")//Consulta estado de documentos venta enviados al portal
                                        ConsultarEstado21(UserWS, PassWS, CompnyName);

                                    DejarAceptadoporDefectoVenta(CompnyName);

                                    if (OP28 != "")//Consulta estado documentos de venta 1 a 1
                                        ConsultarEstado28_29(UserWS, PassWS, CompnyName);
                                    //Func.AddLog("OP36");
                                    if (OP36 != "")//Consulta para traer documentos de compra desde el portal
                                        ConsultarEstado36(UserWS, PassWS, CompnyName);

                                    DejarAceptadoporDefectoCompra(CompnyName);

                                    if (OP27 != "")//Consulta estado documentos de proveedor 1 a 1
                                        ConsultarEstado27_30(UserWS, PassWS, CompnyName);


                                    if (bEnviarMail)
                                        EnviarMail(CompnyName);
                                }
                            }
                            catch (Exception we)
                            {
                                Func.AddLog("Error foreach busca bases: version " + sVersion + " - " + we.Message + " ** Trace: " + we.StackTrace);
                            }
                            finally
                            {
                                if (oCompany != null)
                                    oCompany.Disconnect();
                                oCompany = null;
                                oRecordSet = null;
                            }
                        }
                    }
                    else
                        Func.AddLog("No se ha podido conectar a la Base SAP, revisar datos de conexion");//no se ha podido conectar
                }
                catch (Exception w)
                {
                    Func.AddLog("Error Time1: version " + sVersion + " - " + w.Message + ". ** Trace: " + w.StackTrace);
                }
            }
            catch (Exception we)
            {
                Func.AddLog("Error Time: version " + sVersion + " - " + we.Message + " ** Trace: " + we.StackTrace);
            }
            finally
            {
                Tiempo.Enabled = true;
                Func.AddLog("Fin");
            }
        }


        //Consulta estado de documentos venta enviados al portal
        public void ConsultarEstado21(String UserWS, String PassWS, String CompnyName)
        {
            String sObjType;
            String sDocSubType;
            String TipoDocElec;
            String sDocEntry;
            Boolean SeProceso = false;
            String Json, Id, Validation, Status, sMessage;
            String SerieP = "";
            String sFolioNum = "0";
            String sTabla;
            String nMultiSoc;
            Int32 lRetCode;
            String sErrMsg;
            String EstadoDTE = "";
            String OP21Final;
            SAPbobsCOM.Recordset oRecordSetAux = (SAPbobsCOM.Recordset)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            SAPbobsCOM.Documents oDocuments;
            SAPbobsCOM.StockTransfer oStockTransfer;

            try
            {
                if (RunningSQLServer)
                    s = @"SELECT T0.DocEntry
                                      ,T0.U_DocEntry
                                      ,T0.U_SubType
                                      ,T0.U_FolioNum
                                      ,T0.U_ObjType
                                      ,T0.U_TipoDoc
                                      ,T0.U_Status
                                      ,T0.U_UserCode
                                      ,T0.U_Json
									  ,T0.U_SeriePE
									  ,T0.U_Id
									  ,T0.U_BaseMul
									  ,T0.U_DocDate
                                      ,T0.U_Validation
                                  FROM [@VID_FELOG] T0 WITH (nolock)
                                 WHERE T0.U_Status IN ('EC')
                                   ";//saque opcion 'EE' que estaba en el monitor
                else
                    s = @"SELECT T0.""DocEntry""
                                      ,T0.""U_DocEntry""
                                      ,T0.""U_SubType""
                                      ,T0.""U_FolioNum""
                                      ,T0.""U_ObjType""
                                      ,T0.""U_TipoDoc""
                                      ,T0.""U_Status""
                                      ,T0.""U_UserCode""
                                      ,T0.""U_Json""
									  ,T0.""U_SeriePE""
									  ,T0.""U_Id""
									  ,T0.""U_BaseMul""
									  ,T0.""U_DocDate""
                                      ,T0.""U_Validation""
                                  FROM ""@VID_FELOG"" T0
                                 WHERE T0.""U_Status"" IN ('EC')";//saque opcion 'EE' que estaba en el monitor

                oRecordSet.DoQuery(s);

                if (oRecordSet.RecordCount > 0)
                {
                    while (!oRecordSet.EoF)
                    {
                        try
                        {
                            s = (System.String)(oRecordSet.Fields.Item("U_ObjType").Value);
                            if (s == "15")
                                sTabla = "ODLN";
                            else if (s == "14")
                                sTabla = "ORIN";
                            else if (s == "67")
                                sTabla = "OWTR";
                            else if (s == "21")
                                sTabla = "ORPD";
                            else if (s == "18")
                                sTabla = "OPCH";
                            else if (s == "203")
                                sTabla = "ODPI";
                            else if (s == "204")
                                sTabla = "ODPO";
                            else if (s == "19")
                                sTabla = "ORPC";
                            else
                                sTabla = "OINV";

                            sObjType = s;
                            sDocEntry = Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_DocEntry").Value));

                            if (bMultiSoc)
                            {
                                if (RunningSQLServer)
                                    s = @"SELECT T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo'
                                            ,SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst 
                                        FROM {1} T0 
                                        JOIN NNM1 T2 ON T0.Series = T2.Series 
                                       WHERE T0.DocEntry = {0}";
                                else
                                    s = @"SELECT T0.""DocSubType""
                                            ,SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo""
                                            ,SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                                        FROM ""{1}"" T0 
                                        JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                                       WHERE T0.""DocEntry"" = {0} ";

                                s = String.Format(s, (System.Double)(oRecordSet.Fields.Item("U_DocEntry").Value), sTabla);
                                oRecordSetAux.DoQuery(s);
                                s = (System.String)(oRecordSetAux.Fields.Item("DocSubType").Value);
                                if ((System.String)(oRecordSetAux.Fields.Item("Tipo").Value) == "E")
                                {
                                    nMultiSoc = (System.String)(oRecordSetAux.Fields.Item("Inst").Value);
                                    if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        Func.AddLog("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento, base " + CompnyName);
                                    else
                                    {
                                        if (RunningSQLServer)
                                            s = @"SELECT U_RUT
                                                FROM [@VID_FEMULTISOC] WITH (NOLOCK)
                                               WHERE DocEntry = {0}";
                                        else
                                            s = @"SELECT ""U_RUT""
                                                FROM ""@VID_FEMULTISOC""
                                               WHERE ""DocEntry"" = {0} ";
                                        s = String.Format(s, nMultiSoc);
                                        oRecordSetAux.DoQuery(s);
                                        if (oRecordSetAux.RecordCount > 0)
                                            TaxIdNum = ((System.String)oRecordSetAux.Fields.Item("U_Usuario").Value).Trim();
                                        else
                                        {
                                            Func.AddLog("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametros para conexión, base " + CompnyName);
                                            continue;
                                        }
                                    }
                                }
                            }

                            //Consulta estado al portal
                            OP21Final = OP21.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                            OP21Final = OP21Final.Replace("{1}", ((System.String)oRecordSet.Fields.Item("U_TipoDoc").Value).Trim());
                            OP21Final = OP21Final.Replace("{2}", ((System.Double)oRecordSet.Fields.Item("U_FolioNum").Value).ToString());
                            OP21Final = OP21Final.Replace("&amp;", "&");
                            //Func.AddLog(OP21Final);

                            WebRequest request = WebRequest.Create(OP21Final);
                            if ((UserWS != "") && (PassWS != ""))
                                request.Credentials = new NetworkCredential(UserWS, PassWS);
                            request.Method = "POST";
                            string postData = "";//** xmlDOC.InnerXml;
                            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                            request.ContentType = "text/xml";
                            request.ContentLength = byteArray.Length;
                            Stream dataStream = request.GetRequestStream();
                            dataStream.Write(byteArray, 0, byteArray.Length);
                            dataStream.Close();
                            WebResponse response = request.GetResponse();
                            Console.WriteLine(((HttpWebResponse)(response)).StatusDescription);
                            dataStream = response.GetResponseStream();
                            StreamReader reader = new StreamReader(dataStream);
                            string responseFromServer = reader.ReadToEnd();
                            reader.Close();
                            dataStream.Close();
                            response.Close();
                            sMessage = responseFromServer;
                            request = null;
                            response = null;
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            EstadoDTE = "";
                            if (sMessage == "")
                            {
                                Func.AddLog("WebService devolvio en blanco -> base " + CompnyName + " - Tipo Documento " + ((System.String)oRecordSet.Fields.Item("U_TipoDoc").Value).Trim() + " - Folio " + ((System.Double)oRecordSet.Fields.Item("U_FolioNum").Value).ToString());
                                sMessage = "WebService devolvio en blanco";
                                EstadoDTE = "EC";
                            }
                            else
                            {
                                sMessage = sMessage.Trim();
                                if (sMessage == "No Existe")
                                    EstadoDTE = "EE";
                                else if (sMessage == "Integrado")
                                    EstadoDTE = "EC";
                                else if (sMessage == "Firmado")
                                    EstadoDTE = "EC";
                                else if (sMessage == "Ensobrado")
                                    EstadoDTE = "EC";
                                else if (sMessage == "Aceptado")
                                    EstadoDTE = "RR";
                                else if (sMessage.Contains("Aceptado con Reparo:"))
                                    EstadoDTE = "AR";
                                else if (sMessage.Contains("Rechazado:"))
                                    EstadoDTE = "RZ";
                                else if (sMessage.Substring(0, 5) == "Error")
                                    EstadoDTE = "EE";

                                if (EstadoDTE == "")
                                {
                                    Func.AddLog("WebService devolvio otro valor -> base " + CompnyName + " - Tipo Documento " + ((System.String)oRecordSet.Fields.Item("U_TipoDoc").Value).Trim() + " - Folio " + ((System.Double)oRecordSet.Fields.Item("U_FolioNum").Value).ToString());
                                    sMessage = "WebService devolvio otro valor - " + sMessage;
                                    EstadoDTE = "EC";
                                }
                            }

                            request = null;
                            response = null;
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            lRetCode = FELOGUptM(((System.Int32)oRecordSet.Fields.Item("DocEntry").Value), ((System.Double)oRecordSet.Fields.Item("U_DocEntry").Value), sObjType, ((System.String)oRecordSet.Fields.Item("U_SubType").Value).Trim(), ((System.String)oRecordSet.Fields.Item("U_SeriePE").Value).Trim(), ((System.Double)oRecordSet.Fields.Item("U_FolioNum").Value), EstadoDTE, sMessage, ((System.String)oRecordSet.Fields.Item("U_TipoDoc").Value).Trim(), ((System.String)oRecordSet.Fields.Item("U_UserCode").Value).Trim(), ((System.String)oRecordSet.Fields.Item("U_Json").Value).Trim(), ((System.String)oRecordSet.Fields.Item("U_Id").Value).Trim(), ((System.String)oRecordSet.Fields.Item("U_Validation").Value).Trim(), ((System.DateTime)oRecordSet.Fields.Item("U_DocDate").Value));
                            if (lRetCode == 0)
                                Func.AddLog("Error al actualizar Log de Documento Electronico, base " + CompnyName + " -> TipoDoc " + (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value) + " " + Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_FolioNum").Value)));
                            else
                            {
                                //Insertar registro en tabla de [@VID_FEDTEVTA] una vez que esta aceptado (RR, AR)
                                if ((EstadoDTE == "RR") || (EstadoDTE == "AR"))
                                {
                                    if (RunningSQLServer)
                                        s = @"SELECT REPLACE(LicTradNum,'.','') LicTradNum, CardName, DocTotal, VatSum FROM {0} WHERE DocEntry = {1} ";
                                    else
                                        s = @"SELECT REPLACE(""LicTradNum"",'.','') ""LicTradNum"", ""CardName"", ""DocTotal"", ""VatSum"" FROM ""{0}"" WHERE ""DocEntry"" = {1} ";
                                    s = String.Format(s, sTabla, ((System.Double)oRecordSet.Fields.Item("U_DocEntry").Value));
                                    //Func.AddLog(s);
                                    oRecordSetAux.DoQuery(s);
                                    lRetCode = FEDTEVentaAdd((System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value), ((System.Double)oRecordSet.Fields.Item("U_FolioNum").Value), ((System.String)oRecordSetAux.Fields.Item("LictradNum").Value).Trim()
                                                          , ((System.Double)oRecordSetAux.Fields.Item("DocTotal").Value), ((System.Double)oRecordSetAux.Fields.Item("VatSum").Value), null, "P", null, ((System.Double)oRecordSet.Fields.Item("U_DocEntry").Value)
                                                          , ((System.String)oRecordSet.Fields.Item("U_ObjType").Value), "", ((System.String)oRecordSetAux.Fields.Item("CardName").Value));
                                    if (lRetCode == 0)
                                    {
                                        Func.AddLog("Error Insertar en [@VID_FEDTEVTA] de Documento Electronico, base " + CompnyName + " TipoDoc -> " + (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value) + " " + Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_FolioNum").Value)));
                                        if (RunningSQLServer)
                                            s = @"UPDATE [@VID_FELOG] SET U_Message = ISNULL(U_Message,'') + '- No se ingreso VentaDTE' WHERE DocEntry = {0}";
                                        else
                                            s = @"UPDATE ""@VID_FELOG"" SET ""U_Message"" = IFNULL(""U_Message"",'') || '- No se ingreso VentaDTE' WHERE ""DocEntry"" = {0}";
                                        s = String.Format(s, ((System.Double)oRecordSet.Fields.Item("DocEntry").Value));
                                        oRecordSetAux.DoQuery(s);
                                    }

                                }

                                //actualizar campo crear en cabecera de documento con el estado
                                if (sObjType == "67")
                                {
                                    oStockTransfer = (SAPbobsCOM.StockTransfer)(oCompany.GetBusinessObject(BoObjectTypes.oStockTransfer));
                                    if (oStockTransfer.GetByKey(Convert.ToInt32(sDocEntry)))
                                    {
                                        if (EstadoDTE == "RR")
                                            oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                        else if (EstadoDTE == "AR")
                                            oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "E";
                                        else if (EstadoDTE == "RZ")
                                            oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "R";
                                        else if (EstadoDTE == "EC")
                                            oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                        else if (EstadoDTE == "EE")
                                            oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                        else
                                            oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "N";

                                        lRetCode = oStockTransfer.Update();
                                        if (lRetCode != 0)
                                        {
                                            sErrMsg = oCompany.GetLastErrorDescription();
                                            Func.AddLog("No se actualizado estado de documento, base " + CompnyName + " TipoDoc -> " + (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value) + " folio " + (System.Double)(oRecordSet.Fields.Item("U_FolioNum").Value) + " - " + sErrMsg);
                                            if (RunningSQLServer)
                                                s = @"UPDATE [@VID_FELOG] SET U_Message = ISNULL(U_Message,'') + '- No se actualizo Doc' WHERE DocEntry = {0}";
                                            else
                                                s = @"UPDATE ""@VID_FELOG"" SET ""U_Message"" = IFNULL(""U_Message"",'') || '- No se actualizo Doc' WHERE ""DocEntry"" = {0}";
                                            s = String.Format(s, ((System.Double)oRecordSet.Fields.Item("DocEntry").Value));
                                            oRecordSetAux.DoQuery(s);
                                        }
                                    }
                                    oStockTransfer = null;
                                }
                                else
                                {
                                    if (sObjType == "15")
                                        oDocuments = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(BoObjectTypes.oDeliveryNotes));
                                    else if (sObjType == "14")
                                        oDocuments = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(BoObjectTypes.oCreditNotes));
                                    else if (sObjType == "18")
                                        oDocuments = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(BoObjectTypes.oPurchaseInvoices));
                                    else if (sObjType == "19")
                                        oDocuments = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(BoObjectTypes.oPurchaseCreditNotes));
                                    else if (sObjType == "21")
                                        oDocuments = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(BoObjectTypes.oPurchaseReturns));
                                    else if (sObjType == "203")
                                        oDocuments = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(BoObjectTypes.oDownPayments));
                                    else if (sObjType == "204")
                                        oDocuments = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(BoObjectTypes.oPurchaseDownPayments));
                                    else
                                        oDocuments = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(BoObjectTypes.oInvoices));

                                    if (oDocuments.GetByKey(Convert.ToInt32(sDocEntry)))
                                    {
                                        if (EstadoDTE == "RR")
                                            oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                        else if (EstadoDTE == "AR")
                                            oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "E";
                                        else if (EstadoDTE == "RZ")
                                            oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "R";
                                        else if (EstadoDTE == "EC")
                                            oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                        else if (EstadoDTE == "EE")
                                            oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                        else
                                            oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "N";

                                        lRetCode = oDocuments.Update();
                                        if (lRetCode != 0)
                                        {
                                            sErrMsg = oCompany.GetLastErrorDescription();
                                            Func.AddLog("No se actualizado estado de documento, base " + CompnyName + " TipoDoc -> " + (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value) + " Folio " + (System.Double)(oRecordSet.Fields.Item("U_FolioNum").Value) + " - " + sErrMsg);
                                            if (RunningSQLServer)
                                                s = @"UPDATE [@VID_FELOG] SET U_Message = ISNULL(U_Message,'') + '- No se actualizo Doc' WHERE DocEntry = {0}";
                                            else
                                                s = @"UPDATE ""@VID_FELOG"" SET ""U_Message"" = IFNULL(""U_Message"",'') || '- No se actualizo Doc' WHERE ""DocEntry"" = {0}";
                                            s = String.Format(s, ((System.Int32)oRecordSet.Fields.Item("DocEntry").Value));
                                            oRecordSetAux.DoQuery(s);
                                        }
                                    }
                                    oDocuments = null;

                                }
                            }
                        }
                        catch (Exception x)
                        {
                            Func.AddLog("Err base " + CompnyName + " -> documento " + (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value) + " folio " + (System.Double)(oRecordSet.Fields.Item("U_FolioNum").Value) + " - " + x.Message + ", StackTrace " + x.StackTrace);
                        }
                        oRecordSet.MoveNext();
                    }//fin while

                }

            }
            catch (Exception o)
            {
                Func.AddLog("**Error ConsultaEstado21, base " + CompnyName + ": version " + sVersion + " - " + o.Message + " ** Trace: " + o.StackTrace);
            }
        }

        //Consulta estado documentos de venta 1 a 1
        public void ConsultarEstado28_29(String UserWS, String PassWS, String CompnyName)
        {
            Int32 lRetCode;
            String sErrMsg;
            String EstadoDTE;
            String OP28Final, OP29Final;
            String EstadoC = "";
            String EstadoSII = "";
            String EstadoLey = "";
            DateTime FechaRecep;
            DateTime FechaEmi;
            String Descrip;
            String[] EstadosValidos = { "ACD", "RCD", "ERM", "RFP", "RFT" };
            SAPbobsCOM.Recordset oRecordSetAux = (SAPbobsCOM.Recordset)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

            try
            {
                if (RunningSQLServer)
                    s = @"SELECT T0.U_Folio 'FolioNum', T0.U_RUT 'RUTReceptor', T0.U_TipoDoc 'TipoDoc', T0.DocEntry, T0.U_ObjType, T0.U_DocEntry
                          FROM [@VID_FEDTEVTA] T0
                         WHERE ISNULL(T0.U_EstadoSII,'P') = 'P'
                           AND ISNULL(T0.U_EstadoLey,'') = ''
                            ";
                else
                    s = @"SELECT T0.""U_Folio"" ""FolioNum"", T0.""U_RUT"" ""RUTReceptor"", T0.""U_TipoDoc"" ""TipoDoc"", T0.""DocEntry"", T0.""U_ObjType"", T0.""U_DocEntry""
                          FROM ""@VID_FEDTEVTA"" T0
                         WHERE IFNULL(T0.""U_EstadoSII"",'P') = 'P'
                           AND IFNULL(T0.""U_EstadoLey"",'') = ''";
                oRecordSet.DoQuery(s);

                if (oRecordSet.RecordCount > 0)
                {
                    while (!oRecordSet.EoF)
                    {
                        try
                        {
                        //Consulta estado al portal
                        //http://portal1.easydoc.cl/Consulta/GeneracionDte.aspx?RUT={0}&amp;FOLIO={1}&amp;TIPODTE={2}&amp;RUT1={3}&amp;OP=28
                            OP28Final = OP28.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                            OP28Final = OP28Final.Replace("{1}", ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString());
                            OP28Final = OP28Final.Replace("{2}", ((System.String)oRecordSet.Fields.Item("TipoDoc").Value).Trim());
                            OP28Final = OP28Final.Replace("{3}", ((System.String)oRecordSet.Fields.Item("RUTReceptor").Value).Trim());
                            OP28Final = OP28Final.Replace("&amp;", "&");

                            WebRequest request = WebRequest.Create(OP28Final);
                            if ((UserWS != "") && (PassWS != ""))
                                request.Credentials = new NetworkCredential(UserWS, PassWS);
                            request.Method = "POST";
                            string postData = "";//** xmlDOC.InnerXml;
                            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                            request.ContentType = "text/xml";
                            request.ContentLength = byteArray.Length;
                            Stream dataStream = request.GetRequestStream();
                            dataStream.Write(byteArray, 0, byteArray.Length);
                            dataStream.Close();
                            WebResponse response = request.GetResponse();
                            Console.WriteLine(((HttpWebResponse)(response)).StatusDescription);
                            dataStream = response.GetResponseStream();
                            StreamReader reader = new StreamReader(dataStream);
                            string responseFromServer = reader.ReadToEnd();
                            reader.Close();
                            dataStream.Close();
                            response.Close();
                            s = responseFromServer;
                            var results = JsonConvert.DeserializeObject<dynamic>(s);
                            var jStatus = results.Status;
                            var jCodigo = results.Codigo;
                            var jDescripcion = results.Descripcion;
                            var jFechaSII = results.FechaSII;
                            var jFechaEmis = results.FechaEmis;
                            var jMonto = results.Monto;
                            var jAcuse = results.Acuse;
                            var jAprobacionComercial = results.AprobacionComercial;

                            request = null;
                            response = null;
                            dataStream = null;
                            reader = null;
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            DateTime.TryParse(jFechaEmis.Value, out FechaEmi);
                            DateTime.TryParse(jFechaSII.Value, out FechaRecep);
                            EstadoLey = jCodigo.Value;
                            Descrip = jDescripcion.Value;

                            if (jCodigo.Value == "ACD")
                                EstadoSII = "A";
                            else if (jCodigo.Value == "RCD")
                                EstadoSII = "R";
                            else if (jCodigo.Value == "ERM")
                                EstadoSII = "A";
                            else if (jCodigo.Value == "RFP")
                                EstadoSII = "R";
                            else if (jCodigo.Value == "RFT")
                                EstadoSII = "R";
                            else
                                EstadoSII = "P";

                            
                            if (!EstadosValidos.Contains(EstadoLey))
                                EstadoLey = null; //16 -> no hay accion hecha todavia

                            lRetCode = FEDTEVentaUpt(Convert.ToString(((System.Int32)oRecordSet.Fields.Item("DocEntry").Value)), ((System.String)oRecordSet.Fields.Item("TipoDoc").Value), ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value)
                                                   , ((System.String)oRecordSet.Fields.Item("RUTReceptor").Value).Trim(), FechaRecep, FechaEmi, jMonto.Value, EstadoC, EstadoSII, EstadoLey, jAcuse.Value, jAprobacionComercial.Value, Descrip);
                            if (lRetCode == 0)
                                Func.AddLog("Error al actualizar  tabla VID_FEDTEVTA, base " + CompnyName + " TipoDoc: " + ((System.String)oRecordSet.Fields.Item("TipoDoc").Value) + " Folio: " + ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString() + " RUT: " + ((System.String)oRecordSet.Fields.Item("RUTReceptor").Value).Trim() + " EstadoC:" + EstadoC + " EstadoSII: " + EstadoSII + " EstadoLey: " + EstadoLey);
                            else// se consulta por WS 29 para tener un mensaje mas especifico por que aun no tiene Aceptacion o Reclamo
                            {
                                var tabla = "";
                                if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "14")
                                    tabla = "ORIN";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "15")
                                    tabla = "ODLN";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "18")
                                    tabla = "OPCH";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "19")
                                    tabla = "ORPC";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "21")
                                    tabla = "ORPD";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "67")
                                    tabla = "OWTR";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "203")
                                    tabla = "ODPI";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "204")
                                    tabla = "ODPO";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "13")
                                    tabla = "OINV";
                                if ((EstadoLey != "") && (tabla != ""))
                                {
                                    if (RunningSQLServer)
                                        s = @"UPDATE {0} SET U_EstadoSII = '{2}' WHERE DocEntry = {1}";
                                    else
                                        s = @"UPDATE ""{0}"" SET ""U_EstadoSII"" = '{2}' WHERE ""DocEntry"" = {1}";
                                    s = String.Format(s, tabla, ((System.Double)oRecordSet.Fields.Item("U_DocEntry").Value), EstadoLey);
                                    oRecordSetAux.DoQuery(s);
                                }

                                if ((EstadoLey == "") && (OP29 != ""))
                                {
                                    //Consulta estado al portal
                                    //http://portal1.easydoc.cl/Consulta/GeneracionDte.aspx?RUT={0}&amp;FOLIO={1}&amp;TIPODTE={2}&amp;RUT1={3}&amp;OP=29
                                    OP29Final = OP29.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                                    OP29Final = OP29Final.Replace("{1}", ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString());
                                    OP29Final = OP29Final.Replace("{2}", ((System.String)oRecordSet.Fields.Item("TipoDoc").Value).Trim());
                                    OP29Final = OP29Final.Replace("{3}", ((System.String)oRecordSet.Fields.Item("RUTReceptor").Value).Trim());
                                    OP29Final = OP29Final.Replace("&amp;", "&");

                                    request = WebRequest.Create(OP29Final);
                                    if ((UserWS != "") && (PassWS != ""))
                                        request.Credentials = new NetworkCredential(UserWS, PassWS);
                                    request.Method = "POST";
                                    postData = "";//** xmlDOC.InnerXml;
                                    byteArray = Encoding.UTF8.GetBytes(postData);
                                    request.ContentType = "text/xml";
                                    request.ContentLength = byteArray.Length;
                                    dataStream = request.GetRequestStream();
                                    dataStream.Write(byteArray, 0, byteArray.Length);
                                    dataStream.Close();
                                    response = request.GetResponse();
                                    Console.WriteLine(((HttpWebResponse)(response)).StatusDescription);
                                    dataStream = response.GetResponseStream();
                                    reader = new StreamReader(dataStream);
                                    responseFromServer = reader.ReadToEnd();
                                    reader.Close();
                                    dataStream.Close();
                                    response.Close();
                                    s = responseFromServer;
                                    results = JsonConvert.DeserializeObject<dynamic>(s);
                                    jStatus = results.Status;
                                    jCodigo = results.Codigo;
                                    jDescripcion = results.Descripcion;
                                    request = null;
                                    response = null;
                                    dataStream = null;
                                    reader = null;
                                    GC.Collect();
                                    GC.WaitForPendingFinalizers();

                                    if (jCodigo != null)
                                        Descrip = jCodigo.Value + "-" + jDescripcion.Value;
                                    else
                                        Descrip = jDescripcion.Value;

                                    lRetCode = FEDTEVentaUpt(Convert.ToString(((System.Int32)oRecordSet.Fields.Item("DocEntry").Value)), null, 0, null, DateTime.ParseExact("19000101", "yyyyMMdd", CultureInfo.InvariantCulture), DateTime.ParseExact("19000101", "yyyyMMdd", CultureInfo.InvariantCulture), 0, null, null, null, null, null, Descrip);
                                    if (lRetCode == 0)
                                        Func.AddLog("Error al actualizar  tabla VID_FEDTEVTA, base " + CompnyName + " TipoDoc: " + ((System.String)oRecordSet.Fields.Item("TipoDoc").Value) + " Folio: " + ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString() + " RUT: " + ((System.String)oRecordSet.Fields.Item("RUT").Value).Trim() + " EstadoC:" + EstadoC + " EstadoSII: " + EstadoSII + " EstadoLey: " + EstadoLey);
                                    else
                                    {
                                        if (EstadoLey != "")
                                        {
                                            if (RunningSQLServer)
                                                s = @"UPDATE {0} SET U_EstadoSII = '{2}' WHERE DocEntry = {1}";
                                            else
                                                s = @"UPDATE ""{0}"" SET ""U_EstadoSII"" = '{2}' WHERE ""DocEntry"" = {1}";
                                            s = String.Format(s, tabla, ((System.Double)oRecordSet.Fields.Item("U_DocEntry").Value), EstadoLey);
                                            oRecordSetAux.DoQuery(s);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception x)
                        {
                            Func.AddLog("Err, base " + CompnyName + " -> documento " + (System.String)(oRecordSet.Fields.Item("TipoDoc").Value) + " folio " + (System.Int32)(oRecordSet.Fields.Item("FolioNum").Value) + " RUTEmisor " + (System.String)(oRecordSet.Fields.Item("RUTReceptor").Value) + " - " + x.Message + ", StackTrace " + x.StackTrace);
                        }
                        oRecordSet.MoveNext();
                    }//fin while

                }

            }
            catch (Exception o)
            {
                Func.AddLog("**Error ConsultaEstado28, base " + CompnyName + ": version " + sVersion + " - " + o.Message + " ** Trace: " + o.StackTrace);
            }
        }//Fin 28

        public void DejarAceptadoporDefectoCompra(String CompnyName)
        {
            Int32 lRetCode;
            try
            {
                if (RunningSQLServer)
                    //s = @"SELECT 4819 'FolioNum', '76255466-6' 'RUTEmisor', '33' 'TipoDoc'";
                    s = @"SELECT T0.U_Folio 'FolioNum', T0.U_RUT 'RUTEmisor', T0.U_TipoDoc 'TipoDoc', T0.DocEntry, T0.U_FechaRecep, T0.U_EstadoLey, T0.DocEntry
                          FROM [@VID_FEDTECPRA] T0
                         WHERE 1=1
                           --AND ISNULL(T0.U_EstadoSII,'P') = 'P'
                           AND ISNULL(T0.U_EstadoLey,'') = ''
						   AND T0.U_FechaRecep IS NOT NULL
						   AND CAST(REPLACE(CONVERT(CHAR(10), T0.U_FechaRecep, 102),'.','-') +'T'+ 
								    CASE WHEN LEN(T0.U_HoraRecep) = 4 THEN LEFT(CAST(T0.U_HoraRecep AS VARCHAR(10)),2) + ':' + RIGHT(CAST(T0.U_HoraRecep AS VARCHAR(10)),2) + ':00'
									 	 WHEN LEN(T0.U_HoraRecep) = 3 THEN '0' + LEFT(CAST(T0.U_HoraRecep AS VARCHAR(10)),1) + ':' + RIGHT(CAST(T0.U_HoraRecep AS VARCHAR(10)),2) + ':00'
									 	 WHEN LEN(T0.U_HoraRecep) = 2 THEN '00:'+ CAST(T0.U_HoraRecep AS VARCHAR(10)) + ':00'
										 WHEN LEN(T0.U_HoraRecep) = 1 THEN '00:0' + CAST(T0.U_HoraRecep AS VARCHAR(10)) + ':00'
										 ELSE '00:00:00'
								    END AS DATETIME) < GETDATE()-8";
                else
                    s = @"SELECT T0.""U_Folio"" ""FolioNum"", T0.""U_RUT"" ""RUTEmisor"", T0.""U_TipoDoc"" ""TipoDoc"", T0.""DocEntry"", T0.""U_FechaRecep"", T0.""U_EstadoLey"", T0.""DocEntry""
                          FROM ""@VID_FEDTECPRA"" T0
                         WHERE 1=1
                           --AND IFNULL(T0.""U_EstadoSII"",'P') = 'P'
                           AND IFNULL(T0.""U_EstadoLey"",'') = ''
						   AND T0.""U_FechaRecep"" IS NOT NULL
                           AND CAST(TO_VARCHAR(T0.""U_FechaRecep"", 'yyyy-MM-dd') ||'T'|| 
								   CASE WHEN LENGTH(T0.""U_HoraRecep"") = 4 THEN LEFT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),2) || ':' || RIGHT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),2) || ':00'
										WHEN LENGTH(T0.""U_HoraRecep"") = 3 THEN '0' || LEFT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),1) || ':' || RIGHT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),2) || ':00'
										WHEN LENGTH(T0.""U_HoraRecep"") = 2 THEN '00:' || CAST(T0.""U_HoraRecep"" AS VARCHAR(10)) || ':00'
										WHEN LENGTH(T0.""U_HoraRecep"") = 1 THEN '00:0' || CAST(T0.""U_HoraRecep"" AS VARCHAR(10)) || ':00'
										ELSE '00:00:00'
								   END AS DATETIME) < ADD_DAYS(CURRENT_DATE, -8)";
                oRecordSet.DoQuery(s);

                if (oRecordSet.RecordCount > 0)
                {
                    while (!oRecordSet.EoF)
                    {
                        try
                        {
                            lRetCode = FEDTECompraUpt(Convert.ToString(((System.Int32)oRecordSet.Fields.Item("DocEntry").Value)), "", 0, "", DateTime.ParseExact("19000101", "yyyyMMdd", CultureInfo.InvariantCulture), DateTime.ParseExact("19000101", "yyyyMMdd", CultureInfo.InvariantCulture), 0.0, "", "", "A", "ACO", "", "", "Aceptada por omision", "");
                            if (lRetCode == 0)
                                Func.AddLog("Error al actualizar  tabla VID_FEDTECPRA, base " + CompnyName + " TipoDoc: " + ((System.String)oRecordSet.Fields.Item("TipoDoc").Value) + " Folio: " + ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString() + " RUT: " + ((System.String)oRecordSet.Fields.Item("RUTEmisor").Value).Trim());
                            else
                                Func.AddLog("Se actualizo tabla VID_FEDTECPRA, base " + CompnyName + " TipoDoc: " + ((System.String)oRecordSet.Fields.Item("TipoDoc").Value) + " Folio: " + ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString() + " RUT: " + ((System.String)oRecordSet.Fields.Item("RUTEmisor").Value).Trim() + " EstadoSII: Aceptado EstadoLey: Acepta por Omision");

                        }
                        catch (Exception x)
                        {
                            Func.AddLog("Err Compra Acepta por omision, base " + CompnyName + " -> documento " + (System.String)(oRecordSet.Fields.Item("TipoDoc").Value) + " folio " + (System.Int32)(oRecordSet.Fields.Item("FolioNum").Value) + " RUTEmisor " + (System.String)(oRecordSet.Fields.Item("RUTEmisor").Value) + " - " + x.Message + ", StackTrace " + x.StackTrace);
                        }
                        oRecordSet.MoveNext();
                    }//fin while
                }
            }
            catch (Exception o)
            {
                Func.AddLog("**Error DejarAceptadoporDefectoCompra, base " + CompnyName + ": version " + sVersion + " - " + o.Message + " ** Trace: " + o.StackTrace);
            }
            finally
            {
                ;
            }
        }

        public void DejarAceptadoporDefectoVenta(String CompnyName)
        {
            Int32 lRetCode;
            SAPbobsCOM.Recordset oRecordSetAux = ((SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset));

            try
            {
                if (RunningSQLServer)
                    //s = @"SELECT 4819 'FolioNum', '76255466-6' 'RUTEmisor', '33' 'TipoDoc'";
                    s = @"SELECT T0.U_Folio 'FolioNum', T0.U_RUT 'RUTEmisor', T0.U_TipoDoc 'TipoDoc', T0.DocEntry, T0.U_FechaRecep, T0.U_EstadoLey, T0.U_DocEntry, T0.U_ObjType
                          FROM [@VID_FEDTEVTA] T0
                         WHERE 1=1
                           --AND ISNULL(T0.U_EstadoSII,'P') = 'P'
                           AND ISNULL(T0.U_EstadoLey,'') = ''
						   AND T0.U_FechaRecep IS NOT NULL
						   AND CAST(REPLACE(CONVERT(CHAR(10), T0.U_FechaRecep, 102),'.','-') +'T'+ 
								    CASE WHEN LEN(T0.U_HoraRecep) = 4 THEN LEFT(CAST(T0.U_HoraRecep AS VARCHAR(10)),2) + ':' + RIGHT(CAST(T0.U_HoraRecep AS VARCHAR(10)),2) + ':00'
									 	 WHEN LEN(T0.U_HoraRecep) = 3 THEN '0' + LEFT(CAST(T0.U_HoraRecep AS VARCHAR(10)),1) + ':' + RIGHT(CAST(T0.U_HoraRecep AS VARCHAR(10)),2) + ':00'
									 	 WHEN LEN(T0.U_HoraRecep) = 2 THEN '00:'+ CAST(T0.U_HoraRecep AS VARCHAR(10)) + ':00'
										 WHEN LEN(T0.U_HoraRecep) = 1 THEN '00:0' + CAST(T0.U_HoraRecep AS VARCHAR(10)) + ':00'
										 ELSE '00:00:00'
								    END AS DATETIME) < GETDATE()-8";
                else
                    s = @"SELECT T0.""U_Folio"" ""FolioNum"", T0.""U_RUT"" ""RUTEmisor"", T0.""U_TipoDoc"" ""TipoDoc"", T0.""DocEntry"", T0.""U_FechaRecep"", T0.""U_EstadoLey"", T0.""U_DocEntry"", T0.""U_ObjType""
                          FROM ""@VID_FEDTEVTA"" T0
                         WHERE 1=1
                           --AND IFNULL(T0.""U_EstadoSII"",'P') = 'P'
                           AND IFNULL(T0.""U_EstadoLey"",'') = ''
						   AND T0.""U_FechaRecep"" IS NOT NULL
						   AND CAST(TO_VARCHAR(T0.""U_FechaRecep"", 'yyyy-MM-dd') ||'T'|| 
								   CASE WHEN LENGTH(T0.""U_HoraRecep"") = 4 THEN LEFT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),2) || ':' || RIGHT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),2) || ':00'
										WHEN LENGTH(T0.""U_HoraRecep"") = 3 THEN '0' || LEFT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),1) || ':' || RIGHT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),2) || ':00'
										WHEN LENGTH(T0.""U_HoraRecep"") = 2 THEN '00:' || CAST(T0.""U_HoraRecep"" AS VARCHAR(10)) || ':00'
										WHEN LENGTH(T0.""U_HoraRecep"") = 1 THEN '00:0' || CAST(T0.""U_HoraRecep"" AS VARCHAR(10)) || ':00'
										ELSE '00:00:00'
								   END AS DATETIME) < ADD_DAYS(CURRENT_DATE, -8)";
                oRecordSet.DoQuery(s);

                if (oRecordSet.RecordCount > 0)
                {
                    while (!oRecordSet.EoF)
                    {
                        try
                        {
                            lRetCode = FEDTEVentaUpt(Convert.ToString(((System.Int32)oRecordSet.Fields.Item("DocEntry").Value)), null, 0, null, DateTime.ParseExact("19000101", "yyyyMMdd", CultureInfo.InvariantCulture), DateTime.ParseExact("19000101", "yyyyMMdd", CultureInfo.InvariantCulture), 0, null, null, "ACO", null, null, "Apobada por omision");
                            if (lRetCode == 0)
                                Func.AddLog("Error al actualizar  tabla VID_FEDTEVTA, base " + CompnyName + " TipoDoc: " + ((System.String)oRecordSet.Fields.Item("TipoDoc").Value) + " Folio: " + ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString() + " RUT: " + ((System.String)oRecordSet.Fields.Item("RUT").Value).Trim());
                            else
                            {
                                Func.AddLog("Se actualizo tabla VID_FEDTEVTA, base " + CompnyName + " TipoDoc: " + ((System.String)oRecordSet.Fields.Item("TipoDoc").Value) + " Folio: " + ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString() + " RUT: " + ((System.String)oRecordSet.Fields.Item("RUT").Value).Trim() + " EstadoSII: Aceptado EstadoLey: Acepta por Omision");
                                var tabla = "";
                                if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "14")
                                    tabla = "ORIN";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "15")
                                    tabla = "ODLN";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "18")
                                    tabla = "OPCH";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "19")
                                    tabla = "ORPC";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "21")
                                    tabla = "ORPD";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "67")
                                    tabla = "OWTR";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "203")
                                    tabla = "ODPI";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "204")
                                    tabla = "ODPO";
                                else if (((System.String)oRecordSet.Fields.Item("U_ObjType").Value).Trim() == "13")
                                    tabla = "OINV";

                                if (tabla != "")
                                {
                                    if (RunningSQLServer)
                                        s = @"UPDATE {0} SET U_EstadoSII = '{2}' WHERE DocEntry = {1}";
                                    else
                                        s = @"UPDATE ""{0}"" SET ""U_EstadoSII"" = '{2}' WHERE ""DocEntry"" = {1}";
                                    s = String.Format(s, tabla, ((System.Double)oRecordSet.Fields.Item("U_DocEntry").Value), "ACO");
                                    oRecordSetAux.DoQuery(s);
                                }
                            }
                        }
                        catch (Exception x)
                        {
                            Func.AddLog("Err Venta  Acepta por omision, base " + CompnyName + " -> documento " + (System.String)(oRecordSet.Fields.Item("TipoDoc").Value) + " folio " + (System.Int32)(oRecordSet.Fields.Item("FolioNum").Value) + " RUTEmisor " + (System.String)(oRecordSet.Fields.Item("RUTEmisor").Value) + " - " + x.Message + ", StackTrace " + x.StackTrace);
                        }
                        oRecordSet.MoveNext();
                    }//fin while
                }
            }
            catch (Exception o)
            {
                Func.AddLog("**Error DejarAceptadoporDefectoVenta, base " + CompnyName + ": version " + sVersion + " - " + o.Message + " ** Trace: " + o.StackTrace);
            }
            finally
            {
                oRecordSetAux = null;
            }
        }

        //Consulta para traer documentos de compra desde el portal
        public void ConsultarEstado36(String UserWS, String PassWS, String CompnyName)
        {
            Int32 lRetCode = 0;
            String sErrMsg;
            String OP36Final;
            DateTime FECHARECEPSII;
            DateTime FECHAEMIS;
            DateTime FECHADOC;
            DateTime FECHAVENC;
            String Descrip;
            String Estado;
            Boolean bLB = false;
            Boolean bLN = false;
            String respuesta = "";
            SAPbobsCOM.Recordset orsaux = ((SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            Dictionary<string, string> Documents = new Dictionary<string, string>();
            //SAPbobsCOM.Recordset oRecordSetAux = (SAPbobsCOM.Recordset)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

            try
            {
                //Consulta estado al portal

                OP36Final = OP36.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                OP36Final = OP36Final.Replace("{1}", DateTime.Now.AddDays(-20).ToString("yyyy-MM-dd"));
                OP36Final = OP36Final.Replace("{2}", DateTime.Now.ToString("yyyy-MM-dd"));
                OP36Final = OP36Final.Replace("&amp;", "&");
                //Func.AddLog(OP36Final);
                WebRequest request = WebRequest.Create(OP36Final);
                if ((UserWS != "") && (PassWS != ""))
                    request.Credentials = new NetworkCredential(UserWS, PassWS);
                request.Method = "POST";
                string postData = "";//** xmlDOC.InnerXml;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "text/xml";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                Console.WriteLine(((HttpWebResponse)(response)).StatusDescription);
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                s = responseFromServer;
                var results = JsonConvert.DeserializeObject<dynamic>(s);
                //var jStatus = results.Status;

                request = null;
                response = null;
                dataStream = null;
                reader = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();


                foreach (var item in results["Table"])
                {
                    try
                    {
                        var FOLIO = item.FOLIO.Value;//9698123.0
                        var TIPODOC = item.TIPODOC.Value;//"33"
                        var RUTEMISOR = item.RUTEMISOR.Value;//"96556940-5"
                        var RUTRECEPTOR = item.RUTRECEPTOR.Value;//"96792070-3"
                        var MONTO_TOTAL = item.MONTO_TOTAL.Value;//546256
                        FECHAEMIS = item.FECHAEMIS.Value;//"2017-08-14T00:00:00",
                        FECHARECEPSII = item.FECHARECEPSII.Value;// "2017-08-14T07:55:25",
                        //DateTime.TryParse(item.FECHA.Value, out FECHADOC);// "2017-08-16T08:42:26.29",
                        //"COD_ESTADODTE": -1,
                        //"ID_SETDTE_RECEP": 3161.0,
                        //"GLOSA_ESTADO_RECEP": "                                                  ",
                        //var GIRO_EMISOR = item.GIRO_EMISOR.Value; // "Compra y Venta de Articulos de Oficina",
                        var RAZON_SOCIAL = item.RZN_SOCIAL_EMISOR.Value;// "PROVEEDORES INTEGRALES PRISA S A",
                        //"DIREC_ORIGEN": "Las Rosas 5757",
                        //"COMUNA_ORIGEN": "Cerrillos",
                        //"CIUDAD_ORIGEN": "Santiago",
                        //"GIRO_RECEPTOR": "VENTAS E IMPL.SOFTW",
                        //"RZN_SOCIAL_RECEPTOR": "VISUAL KNOWLEDGE CHILE S.A.",
                        //"DIREC_RECEPTOR": "RICARDO LYON 385",
                        //"COMUNA_RECEPTOR": "PROVIDENCIA",
                        //"CIUDAD_RECEPTOR": "SANTIAGO",
                        var IVA = item.IVA.Value;// 87217.0,
                        //var MONTO_NETO = item.MONTO_NETO.Value;// 459039.0,
                        var MONTO_EXENTO = item.MONTO_EXENTO.Value; // 0.0,
                        var DESCUENTO_GLOBAL = item.DESCUENTO_GLOBAL.Value;// 0.0000,
                        //"ID_SOBRE": 3161.0,
                        FECHAVENC = item.FECHAVENC.Value;// "2017-09-13T00:00:00"
                        var OC = (item.OC.Value == null ? "" : item.OC.Value);
                        var NV = (item.NV.Value == null ? "" : item.NV.Value);
                        Descrip = "Creado por Servicio Estado DTE";

                        if (RunningSQLServer)
                            s = @"SELECT COUNT(*) 'Cont'
                                  FROM [@VID_FEDTECPRA]
                                 WHERE U_TipoDoc = '{0}'
                                   AND U_Folio = {1}
                                   AND U_RUT = '{2}'";
                        else
                            s = @"SELECT COUNT(*) ""Cont""
                                  FROM ""@VID_FEDTECPRA""
                                 WHERE ""U_TipoDoc"" = '{0}'
                                   AND ""U_Folio"" = {1}
                                   AND ""U_RUT"" = '{2}'";
                        s = String.Format(s, TIPODOC.Trim(), FOLIO, RUTEMISOR.Trim());
                        oRecordSet.DoQuery(s);

                        if (((System.Int32)oRecordSet.Fields.Item("Cont").Value) == 0)
                        {
                            lRetCode = FEDTECompraAdd(TIPODOC, FOLIO, RUTEMISOR, MONTO_TOTAL, IVA, "P", Descrip, FECHARECEPSII, FECHAEMIS, FECHAVENC, MONTO_EXENTO, DESCUENTO_GLOBAL, RAZON_SOCIAL, OC, NV);
                            if (lRetCode == 0)
                                Func.AddLog("Error al actualizar  tabla VID_FEDTECPRA, base " + CompnyName + " TipoDoc: " + TIPODOC.Trim() + " Folio: " + FOLIO.ToString() + " RUT: " + RUTEMISOR);
                            else
                            {
                                //Consulta para aceptar o reclamar segun listas blanca y negra segun corresponda
                                if (RunningSQLServer)//consulto si esta en lista Blanca
                                    s = @"SELECT COUNT(*) 'count' FROM [@VID_FELISTABL] T0 JOIN OCRD T1 ON T1.CardCode = T0.U_CardCode WHERE REPLACE(T1.LicTradNum,'.','') = '{0}' AND ISNULL(U_Activado,'N') = 'Y'";
                                else
                                    s = @"SELECT COUNT(*) ""count"" FROM ""@VID_FELISTABL"" T0 JOIN ""OCRD"" T1 ON T1.""CardCode"" = T0.""U_CardCode"" WHERE REPLACE(T1.""LicTradNum"",'.','') = '{0}' AND IFNULL(""U_Activado"",'N') = 'Y'";
                                s = String.Format(s, RUTEMISOR.ToString().Replace(".", "").Trim());
                                orsaux.DoQuery(s);
                                if (((System.Int32)orsaux.Fields.Item("count").Value) > 0)
                                    bLB = true;

                                if (RunningSQLServer)//consulto si esta en lista Negra
                                    s = @"SELECT COUNT(*) 'count' FROM [@VID_FELISTANE] T0 JOIN OCRD T1 ON T1.CardCode = T0.U_CardCode WHERE REPLACE(T1.LicTradNum,'.','') = '{0}' AND ISNULL(U_Activado,'N') = 'Y'";
                                else
                                    s = @"SELECT COUNT(*) ""count"" FROM ""@VID_FELISTANE"" T0 JOIN ""OCRD"" T1 ON T1.""CardCode"" = T0.""U_CardCode"" WHERE REPLACE(T1.""LicTradNum"",'.','') = '{0}' AND IFNULL(""U_Activado"",'N') = 'Y'";
                                s = String.Format(s, RUTEMISOR.ToString().Replace(".", "").Trim());
                                orsaux.DoQuery(s);
                                if (((System.Int32)orsaux.Fields.Item("count").Value) > 0)
                                    bLN = true;

                                if (bLB)
                                    Estado = "ACD";
                                else if (bLN)
                                    Estado = "RCD";
                                else
                                    Estado = "";

                                if (Estado != "")
                                {
                                    //envia estado al portal
                                    if (AceptacionReclamacion(Estado, UserWS, PassWS, FOLIO.ToString(), TIPODOC, RUTEMISOR, CompnyName))
                                    {
                                        lRetCode = FEDTECompraEstadoUpt(lRetCode, TIPODOC, ((System.Int32)FOLIO), RUTEMISOR, "A", Estado, (Estado == "ACD" ? "Aceptada por lista Blanca" : "Reclamada por lista Negra"));
                                        if (lRetCode == 0)
                                            Func.AddLog("Error al actualizar estado tabla VID_FEDTECPRA, base " + CompnyName + " TipoDoc: " + TIPODOC + " Folio: " + FOLIO.ToString() + " RUT: " + RUTEMISOR.Trim());
                                        else
                                            Func.AddLog("Se actualizo estado tabla VID_FEDTECPRA, base " + CompnyName + " TipoDoc: " + TIPODOC + " Folio: " + FOLIO + " RUT: " + RUTEMISOR.Trim() + " EstadoSII: Aceptado EstadoLey: " + (Estado == "ACD" ? "Acepta por Lista Blanca" : "Reclamado por Lista Negra"));
                                    }
                                }

                                //recupera xml del documento
                                //var xx = TaxIdNum.Replace("-", "").Replace(".", "");
                                //xx = xx.Substring(0, xx.Length -1) + "-" + xx.Substring(xx.Length-1,1);
                                var ss = @"http://portal1.easydoc.cl//Consulta/GeneracionDte.aspx?RUT1={0}&FOLIO={1}&TIPODTE={2}&RUT={3}&OP=37";
                                ss = String.Format(ss, RUTEMISOR, FOLIO, TIPODOC, TaxIdNum.Replace("-", "").Replace(".", ""));
                                //Func.AddLog(ss);
                                //var ss = @"http://portal1.easydoc.cl//Consulta/GeneracionDte.aspx?RUT1=76308961-4&FOLIO=2033308&TIPODTE=33&RUT=158367211&OP=37";
                                WebRequest request1 = WebRequest.Create(ss);
                                if ((UserWS != "") && (PassWS != ""))
                                    request1.Credentials = new NetworkCredential(UserWS, PassWS);
                                request1.Method = "POST";
                                string postData1 = "";//** xmlDOC.InnerXml;
                                byte[] byteArray1 = Encoding.UTF8.GetBytes(postData1);
                                request1.ContentType = "text/xml";
                                request1.ContentLength = byteArray1.Length;
                                Stream dataStream1 = request1.GetRequestStream();
                                dataStream1.Write(byteArray, 0, byteArray1.Length);
                                dataStream1.Close();
                                WebResponse response1 = request1.GetResponse();
                                Console.WriteLine(((HttpWebResponse)(response1)).StatusDescription);
                                dataStream1 = response1.GetResponseStream();
                                StreamReader reader1 = new StreamReader(dataStream1);
                                string responseFromServer1 = reader1.ReadToEnd();
                                reader1.Close();
                                dataStream1.Close();
                                response1.Close();
                                var xmlResponse = responseFromServer1;
                                xmlResponse = (xmlResponse == null ? "" : xmlResponse);

                                //Func.AddLog("Descarga Doc Compra xml" + xmlResponse);
                                
                                request1 = null;
                                response1 = null;
                                dataStream1 = null;
                                reader1 = null;
                                GC.Collect();
                                GC.WaitForPendingFinalizers();

                                if (xmlResponse == "")
                                    Func.AddLog("Error descargar xml del documento, base " + CompnyName + " TipoDoc: " + TIPODOC.Trim() + " Folio: " + FOLIO.ToString() + " RUT: " + RUTEMISOR);
                                else
                                {
                                    var DocEntryLog = lRetCode;
                                    lRetCode = FEDTECompraUpt(DocEntryLog.ToString(), ((System.String)TIPODOC), ((System.Int32)FOLIO), ((System.String)RUTEMISOR), FECHARECEPSII, FECHAEMIS, ((System.Double)MONTO_TOTAL), ((System.String)RAZON_SOCIAL), "", "", "", "", "", "", xmlResponse);
                                    if (lRetCode == 0)
                                        Func.AddLog("Error guardar xml del documento, base " + CompnyName + " TipoDoc: " + TIPODOC.Trim() + " Folio: " + FOLIO.ToString() + " RUT: " + RUTEMISOR);
                                    else
                                    {
                                        DescomponerXML(lRetCode, xmlResponse, TIPODOC.Trim(), FOLIO.ToString(), RUTEMISOR, CompnyName);
                                        //Validacion Busca OC en datos guardados de xml 
                                        if (RunningSQLServer)
                                            s = @"SELECT COUNT(*) 'Cant', T2.U_FolioRef, T0.U_FchEmis, T0.U_FchVenc, T0.U_RznSoc, T0.U_MntNeto, T0.U_MntExe, T0.U_MntTotal, T0.U_IVA, T0.U_RUTEmisor
                                                    FROM [@VID_FEXMLCR] T2
                                                    JOIN [@VID_FEXMLC] T0 ON T0.Code = T2.Code
                                                    WHERE T2.Code = 195
                                                    AND T2.U_TpoDocRef = '801'
                                                    GROUP BY T2.U_FolioRef
                                                    , T0.U_FchEmis, T0.U_FchVenc, T0.U_RznSoc, T0.U_MntNeto, T0.U_MntExe, T0.U_MntTotal, T0.U_IVA, T0.U_RUTEmisor";
                                        else
                                            s = @"SELECT COUNT(*) ""Cant"", T2.""U_FolioRef"", T0.""U_FchEmis"", T0.""U_FchVenc"", T0.""U_RznSoc"", T0.""U_MntNeto"", T0.""U_MntExe"", T0.""U_MntTotal"", T0.""U_IVA"", , T0.""U_RUTEmisor""
                                                  FROM ""@VID_FEXMLCR"" T2
                                                  JOIN ""@VID_FEXMLC"" T0 ON T0.""Code"" = T2.""Code""
                                                 WHERE T2.""Code"" = '{0}'
                                                   AND T2.""U_TpoDocRef"" = '801'
                                                  GROUP BY T2.""U_FolioRef"" 
                                                 , T0.""U_FchEmis"", T0.""U_FchVenc"", T0.""U_RznSoc"", T0.""U_MntNeto"", T0.""U_MntExe"", T0.""U_MntTotal"", T0.""U_IVA"", T0.""U_RUTEmisor"" ";
                                        s = String.Format(s, lRetCode);
                                        orsaux.DoQuery(s);
                                        if (((System.Int32)orsaux.Fields.Item("Cant").Value) == 0)
                                            respuesta = "No tiene OC";
                                        else if (((System.Int32)orsaux.Fields.Item("Cant").Value) > 1)
                                            respuesta = "Tiene mas de una OC";
                                        else
                                            respuesta = "";

                                        if (respuesta == "")//asi filtro que solo sea para el caso que tenga una OC
                                        {
                                            var FchEmisXml = ((System.DateTime)orsaux.Fields.Item("U_FchEmis").Value);
                                            var FchvencXml = ((System.DateTime)orsaux.Fields.Item("U_FchVenc").Value);
                                            var RznSocXml = ((System.String)orsaux.Fields.Item("U_RznSoc").Value).Trim();
                                            var MntNetoXml = ((System.Double)orsaux.Fields.Item("U_MntNeto").Value);
                                            var MntExeXml = ((System.Double)orsaux.Fields.Item("U_MntExe").Value);
                                            var MntTotalXml = ((System.Double)orsaux.Fields.Item("U_MntTotal").Value);
                                            var IVAXml = ((System.Double)orsaux.Fields.Item("U_IVA").Value);
                                            var FolioOC = ((System.String)orsaux.Fields.Item("U_FolioRef").Value).Trim();
                                            var RUTxml = ((System.String)orsaux.Fields.Item("U_RUTEmisor").Value).Trim();

                                            if (RunningSQLServer)//Busca CardCode
                                                s = @"SELECT CardCode FROM OCRD WHERE REPLACE(LicTradNum,'.','') = '{0}' AND CardType = 'S' AND frozenFor = 'N'";
                                            else
                                                s = @"SELECT ""CardCode"" FROM ""OCRD"" WHERE REPLACE(""LicTradNum"",'.','') = '{0}' AND ""CardType"" = 'S' AND ""frozenFor"" = 'N'";
                                            s = String.Format(s, RUTxml.Replace(".", ""));
                                            orsaux.DoQuery(s);
                                            var CardCode = "";
                                            if (orsaux.RecordCount == 0)
                                                respuesta = "No se ha encontrado proveedor en el Maestro SN";
                                            else
                                                CardCode = ((System.String)orsaux.Fields.Item("CardCode").Value).Trim();

                                            if (respuesta == "")//si se encontro el SN
                                            {
                                                //Busca parametros para validar
                                                if (RunningSQLServer)
                                                    s = @"SELECT ISNULL(U_FProv,'Y') 'FProv', ISNULL(U_DiasOC,999) 'DiasOC', ISNULL(U_TipoDif,'M') 'TipoDif', ISNULL(U_DifMon,0) 'DifMon'
                                                            , ISNULL(U_DifPor,0.0) 'DifPor', ISNULL(U_EntMer,'N') 'EntMer' 
                                                        FROM [@VID_FEPARAM] ";
                                                else
                                                    s = @"SELECT IFNULL(""U_FProv"",'Y') ""FProv"", IFNULL(""U_DiasOC"",999) ""DiasOC"", IFNULL(""U_TipoDif"",'M') ""TipoDif"", IFNULL(""U_DifMon"",0) ""DifMon""
                                                            , IFNULL(""U_DifPor"",0.0) ""DifPor"", IFNULL(""U_EntMer"",'N') ""EntMer""
                                                        FROM ""@VID_FEPARAM"" ";
                                                orsaux.DoQuery(s);
                                                if (orsaux.RecordCount > 0)
                                                {
                                                    var TipoDif = ((System.String)orsaux.Fields.Item("TipoDif").Value).Trim();
                                                    var DifPor = ((System.Double)orsaux.Fields.Item("DifPor").Value);
                                                    var DifMon = ((System.Double)orsaux.Fields.Item("DifMon").Value);
                                                    var DiasOC = ((System.Int32)orsaux.Fields.Item("DiasOC").Value);

                                                    //Busca datos de la OC
                                                    if (RunningSQLServer)
                                                        s = @"SELECT T0.DocEntry, T0.DocStatus, T0.DocTotal, T0.VatSum, T0.DocDate, COUNT(*) 'Cant'
                                                        FROM OPOR T0
                                                        JOIN POR1 T1 ON T1.DocEntry = T0.DocEntry
                                                        WHERE T0.DocNum = {0}
                                                        AND T0.CardCode = '{1}'
                                                        GROUP BY T0.DocEntry, T0.DocStatus, T0.DocTotal, T0.VatSum, T0.DocDate";
                                                    else
                                                        s = @"SELECT T0.""DocEntry"", T0.""DocStatus"", T0.""DocTotal"", T0.""VatSum"", T0.""DocDate"", COUNT(*) ""Cant""
                                                        FROM ""OPOR"" T0
                                                        JOIN ""POR1"" T1 ON T1.""DocEntry"" = T0.""DocEntry""
                                                        WHERE T0.""DocNum"" = {0}
                                                        AND T0.""CardCode"" = '{1}'
                                                        GROUP BY T0.""DocEntry"", T0.""DocStatus"", T0.""DocTotal"", T0.""VatSum"", T0.""DocDate"" ";
                                                    s = String.Format(s, FolioOC, CardCode);
                                                    orsaux.DoQuery(s);

                                                    if (orsaux.RecordCount == 0)
                                                        respuesta = "No se ha encontrado OC en SAP";
                                                    else
                                                    {
                                                        var OCDocEntry = ((System.Int32)orsaux.Fields.Item("DocEntry").Value);
                                                        var OCDocStatus = ((System.String)orsaux.Fields.Item("DocStatus").Value).Trim();
                                                        var OCDocTotal = ((System.Double)orsaux.Fields.Item("DocTotal").Value);
                                                        var OCVatSum = ((System.Double)orsaux.Fields.Item("VatSum").Value);
                                                        var OCDocDate = ((System.DateTime)orsaux.Fields.Item("DocDate").Value);
                                                        var CantLineasOC = ((System.Int32)orsaux.Fields.Item("Cant").Value);

                                                        if (RunningSQLServer)
                                                            s = @"SELECT COUNT(*) 'Cant'
                                                                    FROM [@VID_FEXMLCD]
                                                                    WHERE Code = '{0}'";
                                                        else
                                                            s = @"SELECT COUNT(*) ""Cant""
                                                                    FROM ""@VID_FEXMLCD""
                                                                    WHERE ""Code"" = '{0}'";
                                                        s = String.Format(s, lRetCode);
                                                        orsaux.DoQuery(s);
                                                        var CantLinFE = ((System.Int32)orsaux.Fields.Item("Cant").Value);


                                                        if (CantLineasOC != CantLinFE)
                                                            respuesta = "Cant Lineas no coincide";

                                                        //Valida total OC y total FE
                                                        s = ValidarDif(TipoDif, DifPor, DifMon, DiasOC, MntTotalXml, OCDocTotal, DateTime.Now, DateTime.Now);
                                                        if (s != "")
                                                            if (respuesta != "")
                                                                respuesta = ", Total Doc " + s;
                                                            else
                                                                respuesta = "Total Doc " + s;
                                                        //para validar fecha
                                                        s = ValidarDif("D", 0, 0, DiasOC, 0, 0, FchEmisXml, OCDocDate);
                                                        if (s != "")
                                                            if (respuesta != "")
                                                                respuesta = respuesta + ", Fecha Doc " + s;
                                                            else
                                                                respuesta = "Fecha Doc " + s;
                                                    }
                                                }//fin if busca datos de parametros FE
                                            }//Fin if respuesta por si encontro CardCode
                                        }//fin if respuesta por si tiene mas de una OC o no tiene

                                        if (respuesta == "")
                                            respuesta = "OK";
                                        
                                        //actualizo campo en tabla FELOG para registrar la respuesta
                                        if (RunningSQLServer)
                                            s = @"UPDATE [@VID_FEDTECPRA] SET U_Validacion = '{0}' WHERE DocEntry = {1}";
                                        else 
                                            s = @"UPDATE ""@VID_FEDTECPRA"" SET ""U_Validacion"" = '{0}' WHERE ""DocEntry"" = {1}";
                                        s = String.Format(s, respuesta, lRetCode);
                                        orsaux.DoQuery(s);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception o)
                    {
                        Func.AddLog("**Error ConsultaEstado36, base " + CompnyName + ": version " + sVersion + " - " + o.Message + " ** Trace: " + o.StackTrace);
                    }
                }//fin foreach

            }
            catch (Exception o)
            {
                Func.AddLog("**Error ConsultaEstado36, base " + CompnyName + ": version " + sVersion + " - " + o.Message + " ** Trace: " + o.StackTrace);
            }
            finally
            {
                orsaux = null;
            }
        }//Fin 36

        private String ValidarDif(String TipoDif, Double DifPor, Double DifMon, Int32 Dias, Double Monto, Double MontoOC, DateTime Fecha, DateTime FechaOC)
        {
            Double DifCal;
            String respuesta = "";    
            try
            {
                if (TipoDif == "M")  //revisar pra que sea + o - 
                {
                    DifCal = MontoOC - Monto;
                    if (DifCal < 0)
                        DifCal = DifCal * -1;
                    if (DifCal > DifMon)
                        return "Documento supera valor OC";
                }
                else if (TipoDif == "P")   //revisar pra que sea + o -
                {
                    DifCal = MontoOC - Monto;
                    if (DifCal < 0)
                        DifCal = DifCal * -1;

                    var Valor = (DifPor * MontoOC) / 100;

                    if (DifCal > Valor)
                        return "Documento supera valor OC";
                }
                
                //Para las diferencias de dias con la OC
                if (TipoDif == "D")
                {
                    var diferenciaDias = Fecha - FechaOC;
                    var difReal = diferenciaDias.Days;
                    if (difReal < 0)
                        difReal = difReal * -1;
                    if (difReal > Dias)
                        return "Supera los dias maximo entre OC y FE";
                }
                return "";
            }
            catch (Exception e)
            {
                return "Error Validar";
            }
        }

        private void DescomponerXML(Int32 DocEntry, String sXML, String TipoDoc, String Folio, String RUTEmisor, String CompnyName)
        {
            XmlDocument oXml = new XmlDocument();
            XmlNodeList nTipoDTE = null;
            XmlNodeList nFolio = null;
            XmlNodeList nFchEmis = null;
            XmlNodeList nFchVenc = null;
            XmlNodeList nFmaPago = null;
            XmlNodeList nRUTEmisor = null;
            XmlNodeList nRznSoc = null;
            XmlNodeList nGiroEmis = null;
            XmlNodeList nDirOrigen = null;
            XmlNodeList nCmnaOrigen = null;
            XmlNodeList nCiudadOrigen = null;
            XmlNodeList nRUTRecep = null;
            XmlNodeList nRznSocRecep = null;
            XmlNodeList nGiroRecep = null;
            XmlNodeList nContacto = null;
            XmlNodeList nDirRecep = null;
            XmlNodeList nCmnaRecep = null;
            XmlNodeList nCiudadRecep = null;
            XmlNodeList nMntNeto = null;
            XmlNodeList nMntExe = null;
            XmlNodeList nTasaIVA = null;
            XmlNodeList nIVA = null;
            XmlNodeList nMntTotal = null;
            XmlNodeList nFchResol = null;
            XmlNodeList nNroResol = null;
            String Timbre = "";
            SAPbobsCOM.GeneralService oFEXML = null;
            SAPbobsCOM.GeneralData oFEXMLData = null;
            SAPbobsCOM.GeneralDataCollection oFEXMLLines = null;
            SAPbobsCOM.GeneralDataParams oFEXMLParameter = null;
            SAPbobsCOM.GeneralData oChild = null;
            SAPbobsCOM.GeneralDataCollection oChildren = null;
            SAPbobsCOM.CompanyService CmpnyService;
            DateTime oDate;
            CmpnyService = oCompany.GetCompanyService();

            try
            {
                oXml.LoadXml(sXML);
                XmlNodeList Tag = oXml.GetElementsByTagName("Encabezado");
                XmlNodeList TagD = ((XmlElement)Tag[0]).GetElementsByTagName("IdDoc");
                foreach (XmlElement nodo in TagD)
                {
                    nTipoDTE = nodo.GetElementsByTagName("TipoDTE");
                    nFolio = nodo.GetElementsByTagName("Folio");
                    nFchEmis = nodo.GetElementsByTagName("FchEmis");

                    if (nodo.GetElementsByTagName("FchVenc").Count != 0)
                        nFchVenc = nodo.GetElementsByTagName("FchVenc");
                    if (nodo.GetElementsByTagName("FmaPago").Count != 0)
                        nFmaPago = nodo.GetElementsByTagName("FmaPago");
                }

                Tag = oXml.GetElementsByTagName("Encabezado");
                TagD = ((XmlElement)Tag[0]).GetElementsByTagName("Emisor");
                foreach (XmlElement nodo in TagD)
                {
                    nRUTEmisor = nodo.GetElementsByTagName("RUTEmisor");
                    nRznSoc = nodo.GetElementsByTagName("RznSoc");

                    if (nodo.GetElementsByTagName("GiroEmis").Count != 0)
                        nGiroEmis = nodo.GetElementsByTagName("GiroEmis");

                    if (nodo.GetElementsByTagName("DirOrigen").Count != 0)
                        nDirOrigen = nodo.GetElementsByTagName("DirOrigen");

                    if (nodo.GetElementsByTagName("CmnaOrigen").Count != 0)
                        nCmnaOrigen = nodo.GetElementsByTagName("CmnaOrigen");

                    if (nodo.GetElementsByTagName("CiudadOrigen").Count != 0)
                        nCiudadOrigen = nodo.GetElementsByTagName("CiudadOrigen");
                }

                Tag = oXml.GetElementsByTagName("Encabezado");
                TagD = ((XmlElement)Tag[0]).GetElementsByTagName("Receptor");
                foreach (XmlElement nodo in TagD)
                {
                    nRUTRecep = nodo.GetElementsByTagName("RUTRecep");
                    nRznSocRecep = nodo.GetElementsByTagName("RznSocRecep");

                    if (nodo.GetElementsByTagName("GiroRecep").Count != 0)
                        nGiroRecep = nodo.GetElementsByTagName("GiroRecep");

                    if (nodo.GetElementsByTagName("Contacto").Count != 0)
                        nContacto = nodo.GetElementsByTagName("Contacto");

                    if (nodo.GetElementsByTagName("DirRecep").Count != 0)
                        nDirRecep = nodo.GetElementsByTagName("DirRecep");

                    if (nodo.GetElementsByTagName("CmnaRecep").Count != 0)
                        nCmnaRecep = nodo.GetElementsByTagName("CmnaRecep");

                    if (nodo.GetElementsByTagName("CiudadRecep").Count != 0)
                        nCiudadRecep = nodo.GetElementsByTagName("CiudadRecep");
                }

                Tag = oXml.GetElementsByTagName("Encabezado");
                TagD = ((XmlElement)Tag[0]).GetElementsByTagName("Totales");
                foreach (XmlElement nodo in TagD)
                {
                    if (nodo.GetElementsByTagName("MntNeto").Count != 0)
                        nMntNeto = nodo.GetElementsByTagName("MntNeto");

                    if (nodo.GetElementsByTagName("MntExe").Count != 0)
                        nMntExe = nodo.GetElementsByTagName("MntExe");

                    if (nodo.GetElementsByTagName("TasaIVA").Count != 0)
                        nTasaIVA = nodo.GetElementsByTagName("TasaIVA");

                    if (nodo.GetElementsByTagName("IVA").Count != 0)
                        nIVA = nodo.GetElementsByTagName("IVA");

                    if (nodo.GetElementsByTagName("MntTotal").Count != 0)
                        nMntTotal = nodo.GetElementsByTagName("MntTotal");
                }

                Tag = oXml.GetElementsByTagName("SetDTE");
                TagD = ((XmlElement)Tag[0]).GetElementsByTagName("Caratula");
                foreach (XmlElement nodo in TagD)
                {
                    if (nodo.GetElementsByTagName("FchResol").Count != 0)
                        nFchResol = nodo.GetElementsByTagName("FchResol");

                    if (nodo.GetElementsByTagName("NroResol").Count != 0)
                        nNroResol = nodo.GetElementsByTagName("NroResol");
                }

                Tag = oXml.GetElementsByTagName("Documento");
                TagD = ((XmlElement)Tag[0]).GetElementsByTagName("TED");
                foreach (XmlElement nodo in TagD)
                {
                    Timbre = nodo.InnerXml;
                }
                Timbre = @"<TED version=""1.0"">" + Timbre + "</TED>";

                oFEXML = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEXMLC"));
                //Create data for new row in main UDO
                oFEXMLData = (SAPbobsCOM.GeneralData)(oFEXML.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oFEXMLData.SetProperty("Code", DocEntry.ToString());
                oFEXMLData.SetProperty("U_RUTEmisor", RUTEmisor);
                oFEXMLData.SetProperty("U_TipoDTE", TipoDoc);
                oFEXMLData.SetProperty("U_Folio", Folio);
                DateTime.TryParse(nFchEmis[0].InnerText, out oDate);
                oFEXMLData.SetProperty("U_FchEmis", oDate);
                if (nFchVenc != null)
                {
                    DateTime.TryParse(nFchVenc[0].InnerText, out oDate);
                    oFEXMLData.SetProperty("U_FchVenc", oDate);
                }
                oFEXMLData.SetProperty("U_RznSoc", nRznSoc[0].InnerText);
                
                if (nFmaPago!= null)
                    oFEXMLData.SetProperty("U_FmaPago", nFmaPago[0].InnerText);
                if (nGiroEmis != null)
                    oFEXMLData.SetProperty("U_GiroEmis", nGiroEmis[0].InnerText);
                if (nDirOrigen != null)
                    oFEXMLData.SetProperty("U_DirOrigen", nDirOrigen[0].InnerText);
                if (nCmnaOrigen != null)
                    oFEXMLData.SetProperty("U_CmnaOrigen", nCmnaOrigen[0].InnerText);
                if (nCiudadOrigen != null)
                    oFEXMLData.SetProperty("U_CiudadOrigen", nCiudadOrigen[0].InnerText);
                oFEXMLData.SetProperty("U_RUTReceptor", nRUTRecep[0].InnerText);
                oFEXMLData.SetProperty("U_RznSocRecep", nRznSocRecep[0].InnerText);
                if (nGiroRecep != null)
                    oFEXMLData.SetProperty("U_GiroRecep", nGiroRecep[0].InnerText);
                if (nContacto != null)
                    oFEXMLData.SetProperty("U_Contacto", nContacto[0].InnerText);
                if (nDirRecep != null)
                    oFEXMLData.SetProperty("U_DirRecep", nDirRecep[0].InnerText);
                if (nCmnaRecep != null)
                    oFEXMLData.SetProperty("U_CmnaRecep", nCmnaRecep[0].InnerText);
                if (nCiudadRecep != null)
                    oFEXMLData.SetProperty("U_CiudadRecep", nCiudadRecep[0].InnerText);
                if (nMntNeto != null)
                    oFEXMLData.SetProperty("U_MntNeto", Convert.ToDouble(nMntNeto[0].InnerText, _nf));
                if (nMntExe != null)
                    oFEXMLData.SetProperty("U_MntExe", Convert.ToDouble(nMntExe[0].InnerText, _nf));
                if (nTasaIVA != null)
                    oFEXMLData.SetProperty("U_TasaIVA", nTasaIVA[0].InnerText);
                if (nIVA != null)
                    oFEXMLData.SetProperty("U_IVA", Convert.ToDouble(nIVA[0].InnerText, _nf));
                if (nMntTotal != null)
                    oFEXMLData.SetProperty("U_MntTotal", Convert.ToDouble(nMntTotal[0].InnerText, _nf));
                if (nFchResol[0].InnerText != "")
                {
                    DateTime.TryParse(nFchResol[0].InnerText, out oDate);
                    oFEXMLData.SetProperty("U_FchResol", oDate);
                }
                oFEXMLData.SetProperty("U_NroResol", nNroResol[0].InnerText);
                oFEXMLData.SetProperty("U_PDF417", Timbre);

                oChildren = oFEXMLData.Child("VID_FEXMLCD");
                Int32 i = 0;
                Tag = oXml.GetElementsByTagName("Documento");
                TagD = ((XmlElement)Tag[0]).GetElementsByTagName("Detalle");
                foreach (XmlElement nodo in TagD)
                {
                    XmlNodeList nvar;
                    oChild = oChildren.Add();
                    if (nodo.GetElementsByTagName("VlrCodigo").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("VlrCodigo");
                        oChild.SetProperty("U_VlrCodigo", nvar[0].InnerText);
                    }

                    if (nodo.GetElementsByTagName("NmbItem").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("NmbItem");
                        if (nvar[0].InnerText.Length > 254)
                            oChild.SetProperty("U_NmbItem", nvar[0].InnerText.Substring(0, 253));
                        else
                            oChild.SetProperty("U_NmbItem", nvar[0].InnerText);
                    }

                    if (nodo.GetElementsByTagName("DscItem").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("DscItem");
                        if (nvar[0].InnerText.Length > 254)
                            oChild.SetProperty("U_DscItem", nvar[0].InnerText.Substring(0, 253));
                        else
                            oChild.SetProperty("U_DscItem", nvar[0].InnerText);
                    }

                    if (nodo.GetElementsByTagName("QtyItem").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("QtyItem");
                        oChild.SetProperty("U_QtyItem", Convert.ToDouble(nvar[0].InnerText, _nf));
                    }

                    if (nodo.GetElementsByTagName("PrcItem").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("PrcItem");
                        oChild.SetProperty("U_PrcItem", Convert.ToDouble(nvar[0].InnerText, _nf));
                    }

                    if (nodo.GetElementsByTagName("MontoItem").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("MontoItem");
                        oChild.SetProperty("U_MontoItem", Convert.ToDouble(nvar[0].InnerText, _nf));
                    }

                    if (nodo.GetElementsByTagName("CodImpAdic").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("CodImpAdic");
                        oChild.SetProperty("U_CodImpAdic", nvar[0].InnerText);
                    }

                    if (nodo.GetElementsByTagName("UnmdItem").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("UnmdItem");
                        oChild.SetProperty("U_UnmdItem", nvar[0].InnerText);
                    }
                }

                oChildren = oFEXMLData.Child("VID_FEXMLCR");
                i = 0;
                Tag = oXml.GetElementsByTagName("Documento");
                TagD = ((XmlElement)Tag[0]).GetElementsByTagName("Referencia");
                foreach (XmlElement nodo in TagD)
                {
                    XmlNodeList nvar;
                    oChild = oChildren.Add();
                    if (nodo.GetElementsByTagName("TpoDocRef").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("TpoDocRef");
                        oChild.SetProperty("U_TpoDocRef", nvar[0].InnerText);
                    }

                    if (nodo.GetElementsByTagName("FolioRef").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("FolioRef");
                        oChild.SetProperty("U_FolioRef", nvar[0].InnerText);
                    }

                    if (nodo.GetElementsByTagName("FchRef").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("FchRef");
                        DateTime.TryParse(nvar[0].InnerText, out oDate);
                        oChild.SetProperty("U_FchRef", oDate);
                    }

                    if (nodo.GetElementsByTagName("RazonRef").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("RazonRef");
                        oChild.SetProperty("U_RazonRef", nvar[0].InnerText);
                    }
                }

                oChildren = oFEXMLData.Child("VID_FEXMLCI");
                i = 0;
                Tag = oXml.GetElementsByTagName("Totales");
                TagD = ((XmlElement)Tag[0]).GetElementsByTagName("ImptoReten");
                foreach (XmlElement nodo in TagD)
                {
                    XmlNodeList nvar;
                    oChild = oChildren.Add();
                    if (nodo.GetElementsByTagName("TipoImp").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("TipoImp");
                        oChild.SetProperty("U_TipoImp", nvar[0].InnerText);
                    }

                    /*if (nodo.GetElementsByTagName("TasaImp").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("TasaImp");
                        oChild.SetProperty("U_TasaImp", nvar[0].InnerText);
                    }*/

                    if (nodo.GetElementsByTagName("MontoImp").Count != 0)
                    {
                        nvar = nodo.GetElementsByTagName("MontoImp");
                        oChild.SetProperty("U_MontoImp", Convert.ToDouble(nvar[0].InnerText, _nf));
                    }
                }

                oFEXMLParameter = oFEXML.Add(oFEXMLData);


            }
            catch (Exception z)
            {
                Func.AddLog("Error DescomponerXML, base " + CompnyName + " TipoDoc: " + TipoDoc + " Folio: " + Folio + " RUT: " + RUTEmisor + " -> " + z.Message + ", TRACE " + z.StackTrace);
            }
        }

        private Boolean AceptacionReclamacion(String EstadoOriginal, String UserWS, String PassWS, String Folio, String TipoDoc, String RUT, String CompnyName)//, String U_ObjType, Double U_DocEntry)
        {
            String URL = @"http://portal1.easydoc.cl/Consulta/GeneracionDte.aspx?RUT={0}&amp;FOLIO={1}&amp;TIPODTE={2}&amp;RUT1={3}&amp;CODAR={4}&amp;OP=31";
            String URLFinal;
            String EstadoFinal;
            WebRequest request;
            string postData;
            byte[] byteArray;
            Stream dataStream;
            WebResponse response;
            StreamReader reader;
            string responseFromServer;
            SAPbobsCOM.Recordset oRecordSetAux = ((SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset));

            try
            {
                if (EstadoOriginal == "ACD")
                    EstadoFinal = "ERM";
                else
                    EstadoFinal = EstadoOriginal;
                URLFinal = URL.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                URLFinal = URLFinal.Replace("{1}", Folio);
                URLFinal = URLFinal.Replace("{2}", TipoDoc);
                URLFinal = URLFinal.Replace("{3}", RUT);
                URLFinal = URLFinal.Replace("{4}", EstadoFinal);
                URLFinal = URLFinal.Replace("&amp;", "&");

                request = WebRequest.Create(URLFinal);
                if ((UserWS != "") && (PassWS != ""))
                    request.Credentials = new NetworkCredential(UserWS, PassWS);
                request.Method = "POST";
                postData = "";//** xmlDOC.InnerXml;
                byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "text/xml";
                request.ContentLength = byteArray.Length;
                dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                response = request.GetResponse();
                Console.WriteLine(((HttpWebResponse)(response)).StatusDescription);
                dataStream = response.GetResponseStream();
                reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                s = responseFromServer;
                var results = JsonConvert.DeserializeObject<dynamic>(s);
                var jStatus = results.Status;
                var jCodigo = results.Codigo;
                var jDescripcion = results.Descripcion;
                request = null;
                response = null;
                dataStream = null;
                reader = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                //Evento registrado previamente
                //if (((System.String)jDescripcion.Value).Contains("Acción Completada OK"))
                if (((System.String)jStatus.Value).Trim() == "OK")
                {
                    /*var tabla = "";
                    if (U_ObjType == "14")
                        tabla = "ORIN";
                    else if (U_ObjType == "15")
                        tabla = "ODLN";
                    else if (U_ObjType == "18")
                        tabla = "OPCH";
                    else if (U_ObjType == "19")
                        tabla = "ORPC";
                    else if (U_ObjType == "21")
                        tabla = "ORPD";
                    else if (U_ObjType == "67")
                        tabla = "OWTR";
                    else if (U_ObjType == "203")
                        tabla = "ODPI";
                    else if (U_ObjType == "204")
                        tabla = "ODPO";
                    else
                        tabla = "OINV";

                    if (RunningSQLServer)
                        s = @"UPDATE {0} SET U_EstadoSII = '{2}' WHERE DocEntry = {1}";
                    else
                        s = @"UPDATE ""{0}"" SET ""U_EstadoSII"" = '{2}' WHERE ""DocEntry"" = {1}";
                    s = String.Format(s, tabla, U_DocEntry, EstadoFinal);
                    oRecordSetAux.DoQuery(s);*/

                    if (EstadoOriginal == "ACD")
                    {
                        URLFinal = URL.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                        URLFinal = URLFinal.Replace("{1}", Folio);
                        URLFinal = URLFinal.Replace("{2}", TipoDoc);
                        URLFinal = URLFinal.Replace("{3}", RUT);
                        URLFinal = URLFinal.Replace("{4}", EstadoOriginal);
                        URLFinal = URLFinal.Replace("&amp;", "&");

                        request = WebRequest.Create(URLFinal);
                        if ((UserWS != "") && (PassWS != ""))
                            request.Credentials = new NetworkCredential(UserWS, PassWS);
                        request.Method = "POST";
                        postData = "";//** xmlDOC.InnerXml;
                        byteArray = Encoding.UTF8.GetBytes(postData);
                        request.ContentType = "text/xml";
                        request.ContentLength = byteArray.Length;
                        dataStream = request.GetRequestStream();
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        dataStream.Close();
                        response = request.GetResponse();
                        Console.WriteLine(((HttpWebResponse)(response)).StatusDescription);
                        dataStream = response.GetResponseStream();
                        reader = new StreamReader(dataStream);
                        responseFromServer = reader.ReadToEnd();
                        reader.Close();
                        dataStream.Close();
                        response.Close();
                        s = responseFromServer;
                        var results1 = JsonConvert.DeserializeObject<dynamic>(s);
                        var jStatus1 = results1.Status;
                        var jCodigo1 = results1.Codigo;
                        var jDescripcion1 = results1.Descripcion;

                        request = null;
                        response = null;
                        dataStream = null;
                        reader = null;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        if (((System.String)jStatus1.Value).Trim() == "OK")
                        {
                            Func.AddLog("Se enviado estado DTE al portal, base " + CompnyName + " TipoDoc " + TipoDoc + " Folio " + Folio.ToString() + " Estado " + EstadoOriginal + " -> " + ((System.String)jDescripcion1.Value).Trim());
                            /*if (RunningSQLServer)
                                s = @"UPDATE {0} SET U_EstadoSII = '{2}' WHERE DocEntry = {1}";
                            else
                                s = @"UPDATE ""{0}"" SET ""U_EstadoSII"" = '{2}' WHERE ""DocEntry"" = {1}";
                            s = String.Format(s, tabla, U_DocEntry, EstadoOriginal);
                            oRecordSetAux.DoQuery(s);*/
                            return true;
                        }
                        else
                        {
                            Func.AddLog("No se enviado estado DTE al portal, base " + CompnyName + " TipoDoc " + TipoDoc + " Folio " + Folio.ToString() + " Estado " + EstadoOriginal + " -> " + ((System.String)jDescripcion1.Value).Trim());
                            return false;
                        }

                    }
                    else
                    {
                        Func.AddLog("Se enviado estado DTE al portal, base " + CompnyName + " TipoDoc " + TipoDoc + " Folio " + Folio.ToString() + " Estado " + EstadoFinal);
                        return true;
                    }
                }
                else
                {
                    Func.AddLog("No se enviado estado DTE al portal, base " + CompnyName + " TipoDoc " + TipoDoc + " Folio " + Folio.ToString() + " Estado " + EstadoFinal + " -> " + ((System.String)jDescripcion.Value).Trim());
                    return false;
                }
            }
            catch (Exception z)
            {
                Func.AddLog("**Error AceptacionReclamacion, base " + CompnyName + ": version " + sVersion + " - " + z.Message + " ** Trace: " + z.StackTrace);
                return false;
            }
        }

        //Consulta estado documentos de proveedor 1 a 1
        public void ConsultarEstado27_30(String UserWS, String PassWS, String CompnyName)
        {
            Int32 lRetCode = 0;
            String sErrMsg;
            String OP27Final, OP30Final;
            Double Monto = 0;
            Double IVA = 0;
            String EstadoC = "";
            String EstadoSII = "";
            String EstadoLey = "";
            DateTime FechaRecep;
            DateTime FechaEmi;
            String Descrip;
            String RazonSocial;
            SAPbobsCOM.Recordset oRecordSetAux = (SAPbobsCOM.Recordset)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

            try
            {
                if (RunningSQLServer)
                    //s = @"SELECT 4819 'FolioNum', '76255466-6' 'RUTEmisor', '33' 'TipoDoc'";
                    /*s = @"SELECT T0.FolioPref 'TipoDoc', T0.FolioNum, T0.DocEntry, T0.ObjType, T0.LicTradNum 'RUTEmisor'
                              FROM OPCH T0
                             WHERE T0.FolioNum NOT IN (SELECT U_Folio FROM [@VID_FEDTECPRA] WHERE U_TipoDoc = T0.FolioPref)
                               AND T0.FolioPref IN ('33','34','43')";*/
                    s = @"SELECT T0.U_RUT 'RUTEmisor'
                              ,T0.U_Razon
	                          ,T0.U_TipoDoc 'TipoDoc'
	                          ,T0.U_Folio 'FolilNum'
                              ,T0.DocEntry
                          FROM [@VID_FEDTECPRA] T0
                         WHERE ISNULL(T0.U_EstadoLey,'') = ''
                           AND T0.U_TipoDoc IN ('33', '34', '43')";
                else
                    /*s = @"SELECT T0.""FolioPref"" ""TipoDoc"", T0.""FolioNum"", T0.""DocEntry"", T0.""ObjType, T0.""LicTradNum"" ""RUTEmisor""
                              FROM ""OPCH"" T0
                             WHERE T0.""FolioNum"" NOT IN (SELECT ""U_Folio"" FROM ""@VID_FEDTECPRA"" WHERE ""U_TipoDoc"" = T0.""FolioPref"")
                               AND T0.""FolioPref"" IN ('33','34''43')";*/
                    s = @"SELECT T0.""U_RUT"" ""RUTEmisor""
                              ,T0.""U_Razon""
	                          ,T0.""U_TipoDoc"" ""TipoDoc""
	                          ,T0.""U_Folio"" ""FolioNum""
                              ,T0.""DocEntry""
                          FROM ""@VID_FEDTECPRA"" T0
                         WHERE IFNULL(T0.""U_EstadoLey"",'') = ''
                           AND T0.""U_TipoDoc"" IN ('33', '34', '43')";
                oRecordSet.DoQuery(s);

                if (oRecordSet.RecordCount > 0)
                {
                    while (!oRecordSet.EoF)
                    {
                        try
                        {
                            //Consulta estado al portal
                            //http://portal1.easydoc.cl/Consulta/GeneracionDte.aspx?RUT={0}&amp;FOLIO={1}&amp;TIPODTE={2}&amp;RUT1={3}&amp;OP=27
                            OP27Final = OP27.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                            OP27Final = OP27Final.Replace("{1}", ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString());
                            OP27Final = OP27Final.Replace("{2}", ((System.String)oRecordSet.Fields.Item("TipoDoc").Value).Trim());
                            OP27Final = OP27Final.Replace("{3}", ((System.String)oRecordSet.Fields.Item("RUTEmisor").Value).Trim());
                            OP27Final = OP27Final.Replace("&amp;", "&");

                            WebRequest request = WebRequest.Create(OP27Final);
                            if ((UserWS != "") && (PassWS != ""))
                                request.Credentials = new NetworkCredential(UserWS, PassWS);
                            request.Method = "POST";
                            string postData = "";//** xmlDOC.InnerXml;
                            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                            request.ContentType = "text/xml";
                            request.ContentLength = byteArray.Length;
                            Stream dataStream = request.GetRequestStream();
                            dataStream.Write(byteArray, 0, byteArray.Length);
                            dataStream.Close();
                            WebResponse response = request.GetResponse();
                            Console.WriteLine(((HttpWebResponse)(response)).StatusDescription);
                            dataStream = response.GetResponseStream();
                            StreamReader reader = new StreamReader(dataStream);
                            string responseFromServer = reader.ReadToEnd();
                            reader.Close();
                            dataStream.Close();
                            response.Close();
                            s = responseFromServer;
                            var results = JsonConvert.DeserializeObject<dynamic>(s);
                            var jStatus = results.Status;
                            var jCodigo = results.Codigo;
                            var jDescripcion = results.Descripcion;
                            var jFechaSII = results.FechaSII;
                            var jFechaEmis = results.FechaEmis;
                            var jMonto = results.Monto;
                            var jOC = results.OC;
                            var jNV = results.NV;
                            var jRazonSocial = results.RazonSocial;

                            request = null;
                            response = null;
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            DateTime.TryParse(jFechaEmis.Value, out FechaEmi);

                            EstadoLey = (jCodigo.Value == null ? "" : jCodigo.Value);
                            RazonSocial = jRazonSocial.Value;
                            Descrip = (jDescripcion.Value == null ? "" : jDescripcion.Value);

                            if (DateTime.TryParse(jFechaSII.Value, out FechaRecep))
                            {
                                if (FechaRecep.ToString("yyyyMMdd") != "19000101")
                                {
                                    lRetCode = FEDTECompraUpt(Convert.ToString(((System.Int32)oRecordSet.Fields.Item("DocEntry").Value)), ((System.String)oRecordSet.Fields.Item("TipoDoc").Value), ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value)
                                        , ((System.String)oRecordSet.Fields.Item("RUTEmisor").Value).Trim(), FechaRecep, FechaEmi, jMonto.Value, RazonSocial, EstadoC, EstadoSII, EstadoLey, (jOC.Value == null ? "" : jOC.Value), (jNV.Value == null ? "" : jNV.Value), Descrip, "");
                                    if (lRetCode == 0)
                                        Func.AddLog("Error al actualizar  tabla VID_FEDTECPRA, base " + CompnyName + " TipoDoc: " + ((System.String)oRecordSet.Fields.Item("TipoDoc").Value) + " Folio: " + ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString() + " RUT: " + ((System.String)oRecordSet.Fields.Item("RUT").Value).Trim() + " EstadoC:" + EstadoC + " EstadoSII: " + EstadoSII + " EstadoLey: " + EstadoLey);
                                    else// se consulta por WS 30 para tener un mensaje mas especifico por que aun no tiene Aceptacion o Reclamo
                                    {
                                        if ((EstadoLey == "") && (OP30 != ""))
                                        {
                                            //Consulta estado al portal
                                            //http://portal1.easydoc.cl/Consulta/GeneracionDte.aspx?RUT={0}&amp;FOLIO={1}&amp;TIPODTE={2}&amp;RUT1={3}&amp;OP=29
                                            OP30Final = OP30.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                                            OP30Final = OP30Final.Replace("{1}", ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString());
                                            OP30Final = OP30Final.Replace("{2}", ((System.String)oRecordSet.Fields.Item("TipoDoc").Value).Trim());
                                            OP30Final = OP30Final.Replace("{3}", ((System.String)oRecordSet.Fields.Item("RUTEmisor").Value).Trim()); //antes decia RUTReceptor
                                            OP30Final = OP30Final.Replace("&amp;", "&");

                                            request = WebRequest.Create(OP30Final);
                                            if ((UserWS != "") && (PassWS != ""))
                                                request.Credentials = new NetworkCredential(UserWS, PassWS);
                                            request.Method = "POST";
                                            postData = "";//** xmlDOC.InnerXml;
                                            byteArray = Encoding.UTF8.GetBytes(postData);
                                            request.ContentType = "text/xml";
                                            request.ContentLength = byteArray.Length;
                                            dataStream = request.GetRequestStream();
                                            dataStream.Write(byteArray, 0, byteArray.Length);
                                            dataStream.Close();
                                            response = request.GetResponse();
                                            Console.WriteLine(((HttpWebResponse)(response)).StatusDescription);
                                            dataStream = response.GetResponseStream();
                                            reader = new StreamReader(dataStream);
                                            responseFromServer = reader.ReadToEnd();
                                            reader.Close();
                                            dataStream.Close();
                                            response.Close();
                                            s = responseFromServer;
                                            results = JsonConvert.DeserializeObject<dynamic>(s);
                                            jStatus = results.Status;
                                            jCodigo = results.Codigo;
                                            jDescripcion = results.Descripcion;
                                            request = null;
                                            response = null;
                                            dataStream = null;
                                            reader = null;
                                            GC.Collect();
                                            GC.WaitForPendingFinalizers();

                                            Descrip = jCodigo.Value + "-" + jDescripcion.Value;

                                            lRetCode = FEDTECompraUpt(Convert.ToString(((System.Int32)oRecordSet.Fields.Item("DocEntry").Value)), "", 0, "", DateTime.ParseExact("19000101", "yyyyMMdd", CultureInfo.InvariantCulture), DateTime.ParseExact("19000101", "yyyyMMdd", CultureInfo.InvariantCulture), 0, "", "", "", "", "", "", Descrip, "");
                                            if (lRetCode == 0)
                                                Func.AddLog("Error al actualizar  tabla VID_FEDTECOMPRA, base " + CompnyName + " TipoDoc: " + ((System.String)oRecordSet.Fields.Item("TipoDoc").Value) + " Folio: " + ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString() + " RUT: " + ((System.String)oRecordSet.Fields.Item("RUT").Value).Trim() + " EstadoC:" + EstadoC + " EstadoSII: " + EstadoSII + " EstadoLey: " + EstadoLey);
                                        }
                                    }
                                }
                                else
                                    Func.AddLog("No se tiene Fecha SII desde el portal, base " + CompnyName + ", TipoDoc: " + ((System.String)oRecordSet.Fields.Item("TipoDoc").Value) + " Folio: " + ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString() + " RUTEmisor: " + ((System.String)oRecordSet.Fields.Item("RUTEmisor").Value).Trim());
                            }
                            else
                                Func.AddLog("No se tiene Fecha SII desde el portal, base " + CompnyName + ", TipoDoc: " + ((System.String)oRecordSet.Fields.Item("TipoDoc").Value) + " Folio: " + ((System.Int32)oRecordSet.Fields.Item("FolioNum").Value).ToString() + " RUTEmisor: " + ((System.String)oRecordSet.Fields.Item("RUTEmisor").Value).Trim());
                        }
                        catch (Exception x)
                        {
                            Func.AddLog("Err, base " + CompnyName + " -> documento " + (System.String)(oRecordSet.Fields.Item("TipoDoc").Value) + " folio " + (System.Int32)(oRecordSet.Fields.Item("FolioNum").Value) + " RUTEmisor " + (System.String)(oRecordSet.Fields.Item("RUTEmisor").Value) + " - " + x.Message + ", StackTrace " + x.StackTrace);
                        }
                        oRecordSet.MoveNext();
                    }//fin while

                }

            }
            catch (Exception o)
            {
                Func.AddLog("**Error, base " + CompnyName + " ConsultaEstado27: version " + sVersion + " - " + o.Message + " ** Trace: " + o.StackTrace);
            }
        }//Fin 27

        public Boolean ConectarBaseSAP(String BaseName)
        {
            Int32 lRetCode;
            TFunctions Func;
            String sErrMsg;

            Func = new TFunctions();
            try
            {
                oCompany.Server = Glob_Servidor;
                oCompany.LicenseServer = Glob_Licencia;
                oCompany.DbUserName = Glob_UserSQL;
                oCompany.DbPassword = Glob_PassSQL;

                if (Glob_SQL == "2008")
                    oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008;
                else if (Glob_SQL == "2012")
                    oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012;
                else if (Glob_SQL == "2014")
                    oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2014;
                else if (Glob_SQL == "2016")
                    oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2016;
                // else if (Glob_SQL == "2017")
                //    oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2017;
                else if (Glob_SQL == "HANA")
                    oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB;

                oCompany.UseTrusted = false;
                oCompany.CompanyDB = BaseName;
                oCompany.UserName = Glob_UserSAP;
                oCompany.Password = Glob_PassSAP;

                //            Func.AddLog(oCompany.Server);
                //            Func.AddLog(oCompany.LicenseServer);
                //            Func.AddLog(oCompany.DbUserName);
                //            Func.AddLog(oCompany.DbPassword);
                //            Func.AddLog(oCompany.CompanyDB);
                //            Func.AddLog(oCompany.UserName);
                //            Func.AddLog(oCompany.Password);
                lRetCode = oCompany.Connect();
                if (lRetCode != 0)
                {
                    sErrMsg = oCompany.GetLastErrorDescription();
                    //Func := new TFunciones;
                    Func.AddLog("Error de conexión base SAP, " + sErrMsg);
                    return false;
                }
                else
                    return true;
            }
            catch (Exception w)
            {
                //Func := new TFunciones;
                Func.AddLog("ConectarBase: " + w.Message + " ** Trace: " + w.StackTrace);
                return false;
            }
        }

        private Boolean DatosConexion()
        {
            XmlNodeList Configuracion;
            XmlNodeList lista;
            Int32 lRetCode;
            TFunctions Func;
            String sErrMsg;
            Boolean _return = false;

            Func = new TFunctions();
            try
            {
                Configuracion = xDoc.GetElementsByTagName("Configuracion");
                lista = ((XmlElement)Configuracion[0]).GetElementsByTagName("ServidorSAP");

                foreach (XmlElement nodo in lista)
                {
                    var i = 0;
                    var nServidor = nodo.GetElementsByTagName("Servidor");
                    var nLicencia = nodo.GetElementsByTagName("ServLicencia");
                    var nUserSAP = nodo.GetElementsByTagName("UsuarioSAP");
                    var nPassSAP = nodo.GetElementsByTagName("PasswordSAP");
                    var nSQL = nodo.GetElementsByTagName("SQL");
                    var nUserSQL = nodo.GetElementsByTagName("UsuarioSQL");
                    var nPassSQL = nodo.GetElementsByTagName("PasswordSQL");
                    var nBaseSAP = nodo.GetElementsByTagName("BaseSAP");

                    Glob_Servidor = (System.String)(nServidor[i].InnerText);
                    Glob_Licencia = (System.String)(nLicencia[i].InnerText);
                    Glob_UserSAP = (System.String)(nUserSAP[i].InnerText);
                    Glob_PassSAP = (System.String)(nPassSAP[i].InnerText);
                    Glob_SQL = (System.String)(nSQL[i].InnerText);
                    Glob_UserSQL = (System.String)(nUserSQL[i].InnerText);
                    Glob_PassSQL = (System.String)(nPassSQL[i].InnerText);
                    return true;
                }
                return false;
            }
            catch (Exception w)
            {
                //Func := new TFunciones;
                Func.AddLog("DatosConexion: " + w.Message + " ** Trace: " + w.StackTrace);
                return false;
            }

        }

        private void EnviarMail(String CompnyName)
        {
            List<string> Lista = new List<string>();
            String mensaje = "";
            var msg = new MailMessage();
            String Mails = "";
            String MailFrom;
            String MailSmtpHost;
            String MailUser;
            String MailPass;
            String Hora1;
            String Hora2;
            String Puerto;

            try
            {
                Hora1 = Func.DatosConfig("Mail", "HoraEnvio1", xDoc);
                Hora2 = Func.DatosConfig("Mail", "HoraEnvio2", xDoc);

                if ((Hora1 == "") && (Hora2 == ""))
                    throw new Exception("No se ha definido hora de envio para el mail en xml de Configuración, debe ingresar al menos una hora");

                s = DateTime.Now.ToString("HHmm");
                Hora1 = Hora1.Replace(":", "").Replace(".", "");
                Hora2 = Hora2.Replace(":", "").Replace(".", "");

                if ((Hora1 == s) || (Hora2 == s))
                {
                    //buscar mail en tabla VID_FEPARAM
                    if (RunningSQLServer)
                        s = @"SELECT ISNULL(U_Mails_CL,'') 'Mail' FROM [@VID_FEPARAM]";
                    else
                        s = @"SELECT IFNULL(""U_Mails_CL"",'') ""Mail"" FROM ""@VID_FEPARAM"" ";
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount == 0)
                        throw new Exception("No se ha encontrado mail donde enviar los DTE con problemas");
                    else
                        Mails = ((System.String)oRecordSet.Fields.Item("Mail").Value).Trim();

                    MailFrom = Func.DatosConfig("Mail", "MailFrom", xDoc);
                    MailSmtpHost = Func.DatosConfig("Mail", "MailSmtpHost", xDoc);
                    MailUser = Func.DatosConfig("Mail", "MailUser", xDoc);
                    MailPass = Func.DatosConfig("Mail", "MailPass", xDoc);
                    Puerto = Func.DatosConfig("Mail", "Puerto", xDoc);

                    if (MailFrom == "")
                        throw new Exception("Debe Ingresar direccion de mail de donde se enviara el mail");

                    if (MailUser == "")
                        throw new Exception("Debe Ingresar usuario para enviar el mail");

                    if (MailPass == "")
                        throw new Exception("Debe Ingresar password para enviar el mail");

                    if (MailPass == "")
                        Puerto = "587";


                    if (RunningSQLServer)
                        s = @"SELECT T0.DocEntry
                                      ,T0.U_DocEntry
                                      ,T0.U_SubType
                                      ,T0.U_FolioNum
                                      ,T0.U_ObjType
                                      ,T0.U_TipoDoc
                                      ,T0.U_Status
                                      ,ISNULL((SELECT D1.Descr
									             FROM CUFD D0
												 JOIN UFD1 D1 ON D1.TableID = D0.TableID
												             AND D1.FieldID = D0.FieldID
											    WHERE D0.TableID = '@VID_FELOG'
												  AND D0.AliasID = 'Status'
												  AND D1.FldValue = T0.U_Status),'') 'Estado'
                                      ,T0.U_UserCode
									  ,T0.U_DocDate
                                  FROM [@VID_FELOG] T0 WITH (nolock)
                                 WHERE T0.U_Status IN ('EE', 'RZ')";
                    else
                        s = @"SELECT T0.""DocEntry""
                                      ,T0.""U_DocEntry""
                                      ,T0.""U_SubType""
                                      ,T0.""U_FolioNum""
                                      ,T0.""U_ObjType""
                                      ,T0.""U_TipoDoc""
                                      ,T0.""U_Status""
                                      ,IFNULL((SELECT D1.""Descr""
									             FROM ""CUFD"" D0
												 JOIN ""UFD1"" D1 ON D1.""TableID"" = D0.""TableID""
												             AND D1.""FieldID"" = D0.""FieldID""
											    WHERE D0.""TableID"" = '@VID_FELOG'
												  AND D0.""AliasID"" = 'Status'
												  AND D1.""FldValue"" = T0.""U_Status""),'') ""Estado""
                                      ,T0.""U_UserCode""
									  ,T0.""U_DocDate""
                                  FROM ""@VID_FELOG"" T0
                                 WHERE T0.""U_Status"" IN ('EE', 'RZ')";

                    oRecordSet.DoQuery(s);
                    while (!oRecordSet.EoF)
                    {
                        s = ((System.String)oRecordSet.Fields.Item("U_TipoDoc").Value).Trim() + "," + ((System.Double)oRecordSet.Fields.Item("U_FolioNum").Value).ToString().Trim() + ","
                            + ((System.String)oRecordSet.Fields.Item("Estado").Value).Trim() + "," + ((System.String)oRecordSet.Fields.Item("U_UserCode").Value).Trim();
                        Lista.Add(s);
                        oRecordSet.MoveNext();
                    }
                    var i = 0;
                    foreach (String Valor in Lista)
                    {
                        var valores = Valor.Split(',');
                        if (i == 0)
                        {
                            mensaje = "Detalle de documento que ha quedado en estado Rechazado en SII o han tenido errores y no han llegado a SII." + Environment.NewLine;
                            mensaje = mensaje + "En caso de estar rechazado debe revisar el mail enviado por SII al mail que tenga registrado para ver el problema del rechazo." + Environment.NewLine;
                            mensaje = mensaje + Environment.NewLine;
                        }
                        mensaje = mensaje + "Tipo DTE:" + valores[0] + "    Folio:" + valores[1] + "    Estado:" + valores[2] + "   Usuario:" + valores[3] + Environment.NewLine;
                        i++;
                    }

                    if (mensaje != "")
                    {
                        mensaje = mensaje + Environment.NewLine;
                        mensaje = mensaje + Environment.NewLine;
                        mensaje = mensaje + "mail enviado automatico por Servicio Estado FE";
                    }

                    foreach (String enviar_a in Mails.Split(';'))
                    {
                        msg.To.Add(new MailAddress(enviar_a));
                    }

                    msg.From = new MailAddress(MailFrom.Trim());
                    msg.Subject = "DTE con Rechazados o con Errrores, " + CompnyName;
                    msg.SubjectEncoding = System.Text.Encoding.UTF8;
                    msg.Body = mensaje;
                    msg.BodyEncoding = System.Text.Encoding.UTF8;
                    msg.Priority = MailPriority.High;

                    //Generar lista de Archivos a Adjuntar
                    //var archivo = Adjuntar(Directory.GetFiles(tomarde), tomarde);
                    //if (archivo.Count == 0) Application.Exit();
                    //Adjuntando los Archivos al Correo
                    //foreach (string arch in archivo)
                    //{
                    //    msg.Attachments.Add(new Attachment(arch));
                    //}

                    var smtpClient = new SmtpClient();
                    if (MailSmtpHost != "")
                    {
                        smtpClient.Host = MailSmtpHost.Trim();
                        smtpClient.EnableSsl = true;
                        smtpClient.Port = Convert.ToInt32(Puerto);

                    }
                    else
                        smtpClient.EnableSsl = false;
                    smtpClient.Credentials = new System.Net.NetworkCredential(MailUser.Trim(), MailPass.Trim());
                    smtpClient.Send(msg);
                    Func.AddLog("Mail enviado para " + CompnyName);
                }
            }
            catch (Exception we)
            {
                Func.AddLog("EnviarMail: version " + sVersion + " - " + we.Message + " ** Trace: " + we.StackTrace);
            }
        }


        public Int32 FEDTEVentaAdd(String TipoDoc, Double Folio, String RUT, Double Monto, Double IVA, String EstadoC, String EstadoSII, String EstadoLey, Double U_DocEntry, String U_ObjType, String Descrip, String CardName)//, ref SAPbobsCOM.Company oCompany)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            //SAPbobsCOM.GeneralDataCollection oFELOGLines = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            SAPbobsCOM.CompanyService CmpnyService;

            CmpnyService = oCompany.GetCompanyService();

            try
            {
                //Get GeneralService (oCmpSrv is the CompanyService)
                oFELOG = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEDTEVTA"));

                //Create data for new row in main UDO
                oFELOGData = (SAPbobsCOM.GeneralData)(oFELOG.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oFELOGData.SetProperty("U_TipoDoc", TipoDoc);
                oFELOGData.SetProperty("U_Folio", Convert.ToInt32(Folio));
                oFELOGData.SetProperty("U_RUT", RUT);
                //oFELOGData.SetProperty("U_FechaRecep", FechaRecep);
                //oFELOGData.SetProperty("U_FechaEmi", FechaEmi);
                oFELOGData.SetProperty("U_Monto", Monto);
                oFELOGData.SetProperty("U_IVA", IVA);
                //oFELOGData.SetProperty("U_EstadoC", EstadoC);
                oFELOGData.SetProperty("U_EstadoSII", EstadoSII);
                //oFELOGData.SetProperty("U_EstadoLey", EstadoLey);
                oFELOGData.SetProperty("U_DocEntry", U_DocEntry);
                oFELOGData.SetProperty("U_ObjType", U_ObjType);
                oFELOGData.SetProperty("U_Descrip", Descrip);
                oFELOGData.SetProperty("U_Razon", CardName);

                //Add the new row, including children, to database
                //oGeneralParams := oGeneralService.Add(oGeneralData);
                //Cmpny.StartTransaction();
                oFELOGParameter = oFELOG.Add(oFELOGData);
                return (System.Int32)(oFELOGParameter.GetProperty("DocEntry"));

                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                Func.AddLog("VentaAdd -> TipoDoc: " + TipoDoc + " Folio: " + Folio.ToString() + " RUT: " + RUT + " EstadoC:" + EstadoC + " EstadoSII: " + EstadoSII + " EstadoLey: " + EstadoLey + " - Error insertar datos: " + e.Message + " ** Trace: " + e.StackTrace);
                return 0;
            }
            finally
            {
                oFELOG = null;
                oFELOGData = null;
                //oFELOGLines = null;
                oFELOGParameter = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }//fin

        public Int32 FEDTEVentaUpt(String DocEntry, String TipoDoc, Int32 Folio, String RUT, DateTime FechaRecep, DateTime FechaEmi, Double Monto, String EstadoC, String EstadoSII, String EstadoLey, String AcuseRecibo, String AprobacionComercial, String Descrip)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            //SAPbobsCOM.GeneralDataCollection oFELOGLines = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            String StrDummy;
            SAPbobsCOM.CompanyService CmpnyService;
            CmpnyService = oCompany.GetCompanyService();

            try
            {
                oFELOG = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEDTEVTA"));
                oFELOGParameter = (SAPbobsCOM.GeneralDataParams)(oFELOG.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = DocEntry;
                oFELOGParameter.SetProperty("DocEntry", StrDummy);
                oFELOGData = oFELOG.GetByParams(oFELOGParameter);
                if (TipoDoc != null)
                    oFELOGData.SetProperty("U_TipoDoc", TipoDoc);
                if (Folio != 0)
                    oFELOGData.SetProperty("U_Folio", Folio);
                if (RUT != null)
                    oFELOGData.SetProperty("U_RUT", RUT);
                if (FechaRecep.ToString("yyyyMMdd") != "19000101")
                {
                    oFELOGData.SetProperty("U_FechaRecep", FechaRecep.Date);
                    oFELOGData.SetProperty("U_HoraRecep", FechaRecep);
                }
                if (FechaEmi.ToString("yyyyMMdd") != "19000101")
                    oFELOGData.SetProperty("U_FechaEmi", FechaEmi);
                if (Monto != 0)
                    oFELOGData.SetProperty("U_Monto", Monto);
                if (EstadoC != null)
                    oFELOGData.SetProperty("U_EstadoC", EstadoC);
                if (EstadoSII != null)
                    oFELOGData.SetProperty("U_EstadoSII", EstadoSII);
                if (EstadoLey != null)
                    oFELOGData.SetProperty("U_EstadoLey", EstadoLey);
                if (AcuseRecibo != null)
                    oFELOGData.SetProperty("U_AcuseRecibo", AcuseRecibo);
                if (AprobacionComercial != null)
                    oFELOGData.SetProperty("U_AprobComer", AprobacionComercial);
                if (Descrip != null)
                    oFELOGData.SetProperty("U_Descrip", Descrip);
                //oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oFELOGData);
                //Cmpny.StartTransaction();
                oFELOG.Update(oFELOGData);
                //Result :=Convert.ToInt32(TMultiFunctions.Trim(System.String(oFELOGData.GetProperty('DocEntry'))));
                return (System.Int32)(oFELOGData.GetProperty("DocEntry"));
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                Func.AddLog("VentaUpt -> TipoDoc: " + TipoDoc + " Folio: " + Folio.ToString() + " RUT: " + RUT + " EstadoC:" + EstadoC + " EstadoSII: " + EstadoSII + " EstadoLey: " + EstadoLey + " - Error actualizar datos: " + e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                oFELOG = null;
                oFELOGData = null;
                //oFELOGLines = null;
                oFELOGParameter = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }//fin

        public Int32 FEDTECompraUpt(String DocEntry, String TipoDoc, Int32 Folio, String RUT, DateTime FechaRecep, DateTime FechaEmi, Double Monto, String RazonSocial, String EstadoC, String EstadoSII, String EstadoLey, String OC, String NV, String Descrip, String xmlResponse)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            SAPbobsCOM.GeneralData oChild = null;
            SAPbobsCOM.GeneralDataCollection oChildren = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            String StrDummy;
            SAPbobsCOM.CompanyService CmpnyService;
            CmpnyService = oCompany.GetCompanyService();

            try
            {
                oFELOG = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEDTECPRA"));
                oFELOGParameter = (SAPbobsCOM.GeneralDataParams)(oFELOG.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = DocEntry;
                oFELOGParameter.SetProperty("DocEntry", Convert.ToInt32(StrDummy));
                oFELOGData = oFELOG.GetByParams(oFELOGParameter);
                if (TipoDoc != "")
                    oFELOGData.SetProperty("U_TipoDoc", TipoDoc);

                if (Folio != 0)
                    oFELOGData.SetProperty("U_Folio", Folio);

                if (RUT != "")
                    oFELOGData.SetProperty("U_RUT", RUT);

                if (FechaRecep.ToString("yyyyMMdd") != "19000101")
                    oFELOGData.SetProperty("U_FechaRecep", FechaRecep);

                if (FechaEmi.ToString("yyyyMMdd") != "19000101")
                    oFELOGData.SetProperty("U_FechaEmi", FechaEmi);

                if (Monto != 0)
                    oFELOGData.SetProperty("U_Monto", Monto);

                if (EstadoC != "")
                    oFELOGData.SetProperty("U_EstadoC", EstadoC);

                if (EstadoSII != "")
                    oFELOGData.SetProperty("U_EstadoSII", EstadoSII);

                if (EstadoLey != "")
                    oFELOGData.SetProperty("U_EstadoLey", EstadoLey);

                if (Descrip != "")
                    oFELOGData.SetProperty("U_Descrip", Descrip);

                if (xmlResponse != "")
                    oFELOGData.SetProperty("U_Xml", xmlResponse);
                //oFELOGData.SetProperty("U_FechaMov", DateTime.Now.Date);               
                //oFELOGData.SetProperty("U_HoraMov", DateTime.Now);


                if ((OC != "") || (NV != ""))
                {
                    oChildren = oFELOGData.Child("VID_FEDTECPRAD");
                    int i = 0;
                    if (OC != "")
                    {
                        oChild = oChildren.Add();
                        oChild.SetProperty("U_CodRef", "801");
                        oChild.SetProperty("U_Folio", OC);
                    }

                    if (NV != "")
                    {
                        oChild = oChildren.Add();
                        oChild.SetProperty("U_CodRef", "802");
                        oChild.SetProperty("U_Folio", NV);
                    }

                }


                //oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oFELOGData);
                //Cmpny.StartTransaction();
                oFELOG.Update(oFELOGData);
                //Result :=Convert.ToInt32(TMultiFunctions.Trim(System.String(oFELOGData.GetProperty('DocEntry'))));
                return (System.Int32)(oFELOGData.GetProperty("DocEntry"));
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                Func.AddLog("CompraUpt -> TipoDoc: " + TipoDoc + " Folio: " + Folio.ToString() + " RUT: " + RUT + " EstadoC:" + EstadoC + " EstadoSII: " + EstadoSII + " EstadoLey: " + EstadoLey + " - Error actualizar datos: " + e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                oFELOG = null;
                oFELOGData = null;
                oChild = null;
                oChildren = null;
                oFELOGParameter = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }//fin

        public Int32 FEDTECompraEstadoUpt(Int32 DocEntry, String TipoDoc, Int32 Folio, String RUT, String EstadoSII, String EstadoLey, String Descrip)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            SAPbobsCOM.CompanyService CmpnyService;
            CmpnyService = oCompany.GetCompanyService();

            try
            {
                oFELOG = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEDTECPRA"));
                oFELOGParameter = (SAPbobsCOM.GeneralDataParams)(oFELOG.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                oFELOGParameter.SetProperty("DocEntry", DocEntry);
                oFELOGData = oFELOG.GetByParams(oFELOGParameter);

                oFELOGData.SetProperty("U_EstadoSII", EstadoSII);
                oFELOGData.SetProperty("U_EstadoLey", EstadoLey);
                oFELOGData.SetProperty("U_Descrip", Descrip);
                //oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oFELOGData);
                //Cmpny.StartTransaction();
                oFELOG.Update(oFELOGData);
                //Result :=Convert.ToInt32(TMultiFunctions.Trim(System.String(oFELOGData.GetProperty('DocEntry'))));
                return (System.Int32)(oFELOGData.GetProperty("DocEntry"));
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                Func.AddLog("CompraEstadoUpt -> TipoDoc: " + TipoDoc + " Folio: " + Folio.ToString() + " RUT: " + RUT + " EstadoLey: " + EstadoLey + " - Error actualizar estado: " + e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                oFELOG = null;
                oFELOGData = null;
                oFELOGParameter = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }//fin

        public Int32 FEDTECompraAdd(String TipoDoc, Double Folio, String RUT, Double Monto, Double IVA, String EstadoSII, String Descrip, DateTime FechaRecep, DateTime FechaEmi, DateTime FechaVenc, Double MontoExento, Double Descuento, String Razon, String OC, String NV)//, ref SAPbobsCOM.Company oCompany)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            //SAPbobsCOM.GeneralDataCollection oFELOGLines = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            SAPbobsCOM.CompanyService CmpnyService;
            SAPbobsCOM.GeneralData oChild = null;
            SAPbobsCOM.GeneralDataCollection oChildren = null;

            CmpnyService = oCompany.GetCompanyService();

            try
            {
                //Get GeneralService (oCmpSrv is the CompanyService)
                oFELOG = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEDTECPRA"));

                //Create data for new row in main UDO
                oFELOGData = (SAPbobsCOM.GeneralData)(oFELOG.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oFELOGData.SetProperty("U_TipoDoc", TipoDoc);
                oFELOGData.SetProperty("U_Folio", Convert.ToInt32(Folio));
                oFELOGData.SetProperty("U_RUT", RUT);
                oFELOGData.SetProperty("U_FechaRecep", FechaRecep.Date);

                oFELOGData.SetProperty("U_HoraRecep", FechaRecep);
                oFELOGData.SetProperty("U_FechaEmi", FechaEmi);
                oFELOGData.SetProperty("U_FechaVenc", FechaVenc);
                oFELOGData.SetProperty("U_MontoExe", MontoExento);
                oFELOGData.SetProperty("U_Descuento", Descuento);
                oFELOGData.SetProperty("U_Razon", Razon);
                oFELOGData.SetProperty("U_Monto", Monto);
                oFELOGData.SetProperty("U_IVA", IVA);
                //oFELOGData.SetProperty("U_EstadoC", EstadoC);
                oFELOGData.SetProperty("U_EstadoSII", EstadoSII);
                //oFELOGData.SetProperty("U_EstadoLey", EstadoLey);
                //oFELOGData.SetProperty("U_DocEntry", U_DocEntry);
                //oFELOGData.SetProperty("U_ObjType", U_ObjType);
                oFELOGData.SetProperty("U_Descrip", Descrip);


                if ((OC != "") || (NV != ""))
                {
                    oChildren = oFELOGData.Child("VID_FEDTECPRAD");
                    int i = 0;
                    if (OC != "")
                    {
                        oChild = oChildren.Add();
                        oChild.SetProperty("U_CodRef", "801");
                        oChild.SetProperty("U_Folio", OC);
                    }

                    if (NV != "")
                    {
                        oChild = oChildren.Add();
                        oChild.SetProperty("U_CodRef", "802");
                        oChild.SetProperty("U_Folio", NV);
                    }

                }

                //Cmpny.StartTransaction();
                oFELOGParameter = oFELOG.Add(oFELOGData);
                return (System.Int32)(oFELOGParameter.GetProperty("DocEntry"));

                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                Func.AddLog("CompraAdd -> TipoDoc: " + TipoDoc + " Folio: " + Folio.ToString() + " RUT: " + RUT + " EstadoSII: " + EstadoSII + " - Error insertar datos: " + e.Message + " ** Trace: " + e.StackTrace);
                return 0;
            }
            finally
            {
                oFELOG = null;
                oFELOGData = null;
                oChild = null;
                oChildren = null;
                //oFELOGLines = null;
                oFELOGParameter = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }//fin

        //Inserta registro LOG
        public Int32 FELOGAdd(Int32 DocEntry, String ObjType, String SubType, String SeriePE, Int32 FolioNum, String Status, String sMessage, String TipoDoc, String UserCode, String JsonText, String Id, String Validation)//, ref SAPbobsCOM.Company oCompany)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            SAPbobsCOM.GeneralDataCollection oFELOGLines = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            SAPbobsCOM.CompanyService CmpnyService;

            CmpnyService = oCompany.GetCompanyService();

            try
            {
                //Get GeneralService (oCmpSrv is the CompanyService)
                oFELOG = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FELOG"));

                //Create data for new row in main UDO
                oFELOGData = (SAPbobsCOM.GeneralData)(oFELOG.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oFELOGData.SetProperty("U_DocEntry", DocEntry);
                oFELOGData.SetProperty("U_ObjType", ObjType);
                oFELOGData.SetProperty("U_FolioNum", FolioNum);
                oFELOGData.SetProperty("U_SubType", SubType);
                oFELOGData.SetProperty("U_Status", Status);
                oFELOGData.SetProperty("U_Message", sMessage);
                oFELOGData.SetProperty("U_TipoDoc", TipoDoc);
                oFELOGData.SetProperty("U_UserCode", UserCode);
                oFELOGData.SetProperty("U_Json", JsonText);
                oFELOGData.SetProperty("U_SeriePE", SeriePE);
                oFELOGData.SetProperty("U_Id", Id);
                oFELOGData.SetProperty("U_Validation", Validation);

                //Add the new row, including children, to database
                //oGeneralParams := oGeneralService.Add(oGeneralData);
                //Cmpny.StartTransaction();
                oFELOGParameter = oFELOG.Add(oFELOGData);
                return (System.Int32)(oFELOGParameter.GetProperty("DocEntry"));

                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                Func.AddLog("DocEntry: " + DocEntry.ToString() + " ObjType: " + ObjType + " SubType: " + SubType + "Error insertar datos en FELOG: " + e.Message + " ** Trace: " + e.StackTrace);
                return 0;
            }
            finally
            {
                oFELOG = null;
                oFELOGData = null;
                oFELOGLines = null;
                oFELOGParameter = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }//fin FELOGAdd

        //Actualiza registro en el LOG
        public Int32 FELOGUptM(Int32 DocEntry, Double DocEntryDoc, String ObjType, String SubType, String SeriePE, Double FolioNum, String Status, String sMessage, String TipoDoc, String UserCode, String JsonText, String Id, String Validation, DateTime DocDate)//, ref SAPbobsCOM.Company oCompany)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            SAPbobsCOM.GeneralDataCollection oFELOGLines = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            String StrDummy;
            SAPbobsCOM.CompanyService CmpnyService;

            CmpnyService = oCompany.GetCompanyService();

            try
            {
                oFELOG = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FELOG"));
                oFELOGParameter = (SAPbobsCOM.GeneralDataParams)(oFELOG.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = Convert.ToString(DocEntry);
                oFELOGParameter.SetProperty("DocEntry", StrDummy);
                oFELOGData = oFELOG.GetByParams(oFELOGParameter);
                oFELOGData.SetProperty("U_DocEntry", Convert.ToString(DocEntryDoc));
                oFELOGData.SetProperty("U_FolioNum", Convert.ToString(FolioNum));
                oFELOGData.SetProperty("U_Status", Status);
                oFELOGData.SetProperty("U_Message", sMessage);
                oFELOGData.SetProperty("U_TipoDoc", TipoDoc);
                oFELOGData.SetProperty("U_UserCode", UserCode);
                if (JsonText != null)
                    oFELOGData.SetProperty("U_Json", JsonText);

                oFELOGData.SetProperty("U_SeriePE", SeriePE);

                if (Id != null)
                    oFELOGData.SetProperty("U_Id", Id);

                if (Validation != null)
                    oFELOGData.SetProperty("U_Validation", Validation);

                if (DocDate != null)
                    oFELOGData.SetProperty("U_DocDate", DocDate);

                //oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oFELOGData);

                //Cmpny.StartTransaction();
                oFELOG.Update(oFELOGData);
                //Result :=Convert.ToInt32(TMultiFunctions.Trim(System.String(oFELOGData.GetProperty('DocEntry'))));
                return (System.Int32)(oFELOGData.GetProperty("DocEntry"));
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                Func.AddLog("Actualizar tabla FELOG: " + e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                oFELOG = null;
                oFELOGData = null;
                oFELOGLines = null;
                oFELOGParameter = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }//fin FELOGUptM
    }
}
