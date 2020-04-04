using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.IO;
using System.Xml;
using SAPbouiCOM;
using SAPbobsCOM;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using VisualD.untLog;
using Factura_Electronica_VK.Functions;


namespace Factura_Electronica_VK.RegistrarCAF
{
    class TRegistrarCAF : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbouiCOM.DataTable oDataTable;
        private SAPbouiCOM.DBDataSource oDBDSH;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Grid ogrid;
        private SAPbouiCOM.Form oForm;
        private Boolean bMultiSoc;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;
        private TFunctions Funciones = new TFunctions();

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            //SAPbouiCOM.ComboBox oCombo;
            SAPbouiCOM.GridColumn oColumn;
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                Funciones.SBO_f = FSBOf;
                Lista = new List<string>();

                FSBOf.LoadForm(xmlPath, "VID_FECAF.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All
                oForm.EnableMenu("1281", false);//Buscar
                oForm.EnableMenu("1282", false);//Crear

                //        VID_DelRow := true;
                //        VID_DelRowOK := true;

                //        oForm.DataBrowser.BrowseBy := "Code"; 
                oDBDSH = oForm.DataSources.DBDataSources.Add("@VID_FECAF");
                ogrid = (Grid)(oForm.Items.Item("grid").Specific);
                oDataTable = oForm.DataSources.DataTables.Add("dt");

                ogrid.DataTable = oDataTable;

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select ISNULL(U_MultiSoc,'N') MultiSoc from [@VID_FEPARAM]";
                else
                    s = @"select IFNULL(""U_MultiSoc"",'N') ""MultiSoc"" from ""@VID_FEPARAM"" ";
                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                    if (((System.String)oRecordSet.Fields.Item("MultiSoc").Value).Trim() == "Y")
                        bMultiSoc = true;
                    else
                        bMultiSoc = false;
                else
                    bMultiSoc = false;


                ActualizarGrilla();
                // Ok Ad  Fnd Vw Rq Sec
                //        Lista.Add('TipoDoc   , f,  t,  t,  f, r, 1');
                //        FSBOf.SetAutoManaged(var oForm, Lista);

                oForm.Mode = BoFormMode.fm_OK_MODE;

            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            oForm.Freeze(false);
            return Result;
        }//fin Initform


        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            Int32 nErr;
            string sErr;
            //SAPbouiCOM.DataTable oDataTable;
            //Int32 iDif;

            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED)
                {
                    if ((pVal.ItemUID == "btn1") && (!pVal.BeforeAction))
                    {
                        BubbleEvent = false;
                        ActualizarRegistrosWS();
                    }
                }
            }
            catch (Exception e)
            {
                FCmpny.GetLastError(out nErr, out sErr);
                FSBOApp.StatusBar.SetText("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin FormEvent

        public new void MenuEvent(ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
            //Int32 Entry;
            base.MenuEvent(ref pVal, ref BubbleEvent);
            try
            {
                //1281 Buscar; 
                //1282 Crear
                //1284 cancelar; 
                //1285 Restablecer; 
                //1286 Cerrar; 
                //1288 Registro siguiente;
                //1289 Registro anterior; 
                //1290 Primer Registro; 
                //1291 Ultimo Registro; 

                if ((pVal.MenuUID != "") && (pVal.BeforeAction == false))
                {
                    if ((pVal.MenuUID == "1288") || (pVal.MenuUID == "1289") || (pVal.MenuUID == "1290") || (pVal.MenuUID == "1291"))
                    {
                    }
                }

                if ((pVal.MenuUID == "1282") || (pVal.MenuUID == "1281"))
                {
                }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent

        private void ActualizarRegistrosWS()
        {
            String sCode = "";
            //SqlDataAdapter cmd2;
            System.Data.DataTable resultDataTable;
            //System.Data.DataTable resultDataTable1;
            Int32 i;
            Boolean _return;
            SAPbouiCOM.GridColumn oColumn;
            String sDocEntry = "";
            SAPbobsCOM.Recordset orsAux;
            String RUTEmpresa = "";
            String URL = "";
            String URLFinal;
            String[] TipoDocs = { "33", "34", "39", "41", "43", "46", "52", "56", "61", "110", "111", "112" };
            String UserWS = "";
            String PassWS = "";
            String sMessage;
            String xmlCAF = "";
            XmlDocument oXml;
            String Desde = "";
            String Hasta = "";
            String Fecha = "";

            try
            {
                ActualizarGrilla();

                orsAux = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT U_BuscarCAF, ISNULL(U_UserWSCL,'') 'UserWS', ISNULL(U_PassWSCL,'') 'PassWS' FROM [@VID_FEPARAM] WHERE Code = '1'";
                else
                    s = @"SELECT ""U_BuscarCAF"", IFNULL(""U_UserWSCL"",'') ""UserWS"", IFNULL(""U_PassWSCL"",'') ""PassWS"" FROM ""@VID_FEPARAM"" WHERE ""Code"" = '1'";
                orsAux.DoQuery(s);

                if (orsAux.RecordCount == 0)
                    throw new Exception("Debe parametrizar el addon");
                else if (((System.String)orsAux.Fields.Item("U_BuscarCAF").Value).Trim() == "")
                    throw new Exception("Debe ingresar http para buscar CAF en paramatros del addon");
                else
                {
                    URL = ((System.String)orsAux.Fields.Item("U_BuscarCAF").Value).Trim();
                    if (((System.String)orsAux.Fields.Item("UserWS").Value).Trim() != "")
                        UserWS = Funciones.DesEncriptar(((System.String)orsAux.Fields.Item("UserWS").Value).Trim());
                    if (((System.String)orsAux.Fields.Item("PassWS").Value).Trim() != "")
                        PassWS = Funciones.DesEncriptar(((System.String)orsAux.Fields.Item("PassWS").Value).Trim());
                }

                if (bMultiSoc)
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT DocEntry, REPLACE(REPLACE(ISNULL(U_RUT,''),'.',''),'-','') TaxIdNum FROM [@VID_FEMULTISOC] WHERE U_Habilitada = 'Y'";
                    else
                        s = @"SELECT TO_VARCHAR(""DocEntry"") ""DocEntry"", REPLACE(REPLACE(IFNULL(""U_RUT"",''),'.',''),'-','') ""TaxIdNum"" FROM ""@VID_FEMULTISOC"" WHERE ""U_Habilitada"" = 'Y' ";
                }
                else
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT REPLACE(REPLACE(ISNULL(TaxIdNum,''),'-',''),'.','') TaxIdNum FROM OADM";
                    else
                        s = @"SELECT REPLACE(REPLACE(IFNULL(""TaxIdNum"",''),'-',''),'.','') ""TaxIdNum"" FROM ""OADM"" ";
                }
                oRecordSet.DoQuery(s);

                if (oRecordSet.RecordCount > 0)
                {
                    while (!oRecordSet.EoF)
                    {
                        try
                        {
                            RUTEmpresa = ((System.String)oRecordSet.Fields.Item("TaxIdNum").Value).Trim();
                            if (bMultiSoc)
                                sDocEntry = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value));
                            else
                                sDocEntry = "0";

                            foreach (String xTipo in TipoDocs)
                            {
                                try
                                {
                                    //http://portal1.easydoc.cl/consulta/generaciondte.aspx?RUT={0}&TIPODTE={1}&OP=22
                                    URLFinal = URL;
                                    URLFinal = String.Format(URLFinal, RUTEmpresa, xTipo);


                                    WebRequest request = WebRequest.Create(URLFinal);
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

                                    if (sMessage != "")
                                    {
                                        xmlCAF = sMessage;
                                        xmlCAF = xmlCAF.Replace(@"<?xml version=""1.0""?>", "");
                                        //xmlCAF = xmlCAF.Replace("\n", "");
                                        //xmlCAF = xmlCAF.Replace(@"\", "");
                                        //xmlCAF = xmlCAF.Replace(Environment.NewLine, "");
                                        oXml = new XmlDocument();
                                        oXml.LoadXml(xmlCAF);

                                        var DA = oXml.GetElementsByTagName("DA");
                                        var RNG = ((XmlElement)DA[0]).GetElementsByTagName("RNG");
                                        foreach (XmlElement nodo in RNG)
                                        {
                                            var nDesde = nodo.GetElementsByTagName("D");
                                            var nHasta = nodo.GetElementsByTagName("H");
                                            Desde = (System.String)(nDesde[0].InnerText);
                                            Hasta = (System.String)(nHasta[0].InnerText);
                                        }

                                        var CAF = oXml.GetElementsByTagName("CAF");
                                        var DA2 = ((XmlElement)CAF[0]).GetElementsByTagName("DA");
                                        foreach (XmlElement nodo in CAF)
                                        {
                                            var nFecha = nodo.GetElementsByTagName("FA");
                                            Fecha = (System.String)(nFecha[0].InnerText);
                                        }

                                        if (GlobalSettings.RunningUnderSQLServer)
                                            s = @"SELECT Code
                                          FROM [@VID_FECAF]
                                         WHERE U_TipoDoc = '{0}'
                                           AND U_Desde = {1}
                                           AND U_Hasta = {2}
                                           {3}";
                                        else
                                            s = @"SELECT ""Code""
                                          FROM ""@VID_FECAF""
                                         WHERE ""U_TipoDoc"" = '{0}'
                                           AND ""U_Desde"" = {1}
                                           AND ""U_Hasta"" = {2}
                                            {3}";
                                        s = String.Format(s, xTipo, Desde, Hasta, (bMultiSoc ? " AND U_BaseMul = " + sDocEntry : ""));
                                        orsAux.DoQuery(s);
                                        if (orsAux.RecordCount == 0) //no existe en la base de datos
                                        {
                                            sCode = Funciones.sNuevoDocEntryLargo("@VID_FECAF", GlobalSettings.RunningUnderSQLServer);
                                            oDBDSH.Clear();
                                            oDBDSH.InsertRecord(0);
                                            oDBDSH.SetValue("Code", 0, sCode);
                                            //OutLog("Code -> " + oDBDSH.GetValue("Code",0));
                                            oDBDSH.SetValue("U_TipoDoc", 0, xTipo);
                                            //OutLog("TipoDoc -> " + oDBDSH.GetValue("U_TipoDoc", 0));
                                            oDBDSH.SetValue("U_Desde", 0, Desde);
                                            //OutLog("Desde -> " + oDBDSH.GetValue("U_Desde", 0));
                                            oDBDSH.SetValue("U_Hasta", 0, Hasta);
                                            //OutLog("Hasta -> " + oDBDSH.GetValue("U_Hasta", 0));
                                            DateTime oFecha;
                                            DateTime.TryParse(Fecha, out oFecha);
                                            oDBDSH.SetValue("U_Fecha", 0, oFecha.ToString("yyyyMMdd"));
                                            //OutLog("Fecha -> " + oDBDSH.GetValue("U_Fecha", 0));
                                            oDBDSH.SetValue("U_BaseMul", 0, sDocEntry);
                                            //OutLog("BaseMul -> " + oDBDSH.GetValue("U_BaseMul", 0));
                                            oDBDSH.SetValue("U_CAF", 0, xmlCAF);
                                            //OutLog("CAF -> " + oDBDSH.GetValue("U_CAF", 0));
                                            oDBDSH.SetValue("U_Utilizados", 0, "0");
                                            //OutLog("Utilizados -> " + oDBDSH.GetValue("U_Utilizados", 0));
                                            var iDif = (Convert.ToInt32(Hasta) - Convert.ToInt32(Desde)) + 1;
                                            oDBDSH.SetValue("U_Asignables", 0, iDif.ToString());
                                            //OutLog("Asignables -> " + oDBDSH.GetValue("U_Asignables", 0));
                                            oDBDSH.SetValue("U_FolioDesde", 0, Desde);
                                            //OutLog("FolioDesde -> " + oDBDSH.GetValue("U_FolioDesde", 0));
                                            _return = Funciones.CAFAdd(oDBDSH);
                                            if (_return)
                                                FSBOApp.StatusBar.SetText("CAF Registrado, TipoDoc " + xTipo, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                            else
                                                FSBOApp.StatusBar.SetText("CAF no se ha registrado, TipoDoc " + xTipo, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        }
                                    }
                                    else
                                        FSBOApp.StatusBar.SetText("No se ha encontrado CAF para documento " + xTipo, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                }
                                catch (Exception v)
                                {
                                    OutLog("Error actualizar CAF Tipo " + xTipo + " RUT Empresa " + RUTEmpresa + ", " + v.Message + ", TRACE " + v.StackTrace);
                                    FSBOApp.StatusBar.SetText("Error actualizar CAF Tipo " + xTipo + " RUT Empresa " + RUTEmpresa + ", " + v.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                }

                            }//Fin Foreach
                            FSBOApp.StatusBar.SetText("CAF actualizados", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                        }
                        catch (Exception v)
                        {
                            OutLog("Error actualizar empresa RUT " + RUTEmpresa + ", " + v.Message + ", TRACE " + v.StackTrace);
                            FSBOApp.StatusBar.SetText("Error actualizar empresa RUT " + RUTEmpresa + ", " + v.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        }
                        oRecordSet.MoveNext();
                    }

                    ActualizarGrilla();
                }
                else
                { FSBOApp.StatusBar.SetText("Debe ingresar datos de conexion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("ActualizarRegistros: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin ActualizarRegistros

        private void ActualizarRegistros()
        {
            TFunctions Reg;
            String sCode = "";
            SqlDataAdapter cmd;
            //SqlDataAdapter cmd2;
            SqlConnection ConexionADO;
            System.Data.DataTable resultDataTable;
            //System.Data.DataTable resultDataTable1;
            Int32 i;
            Boolean _return;
            String sCnn;
            SAPbouiCOM.GridColumn oColumn;
            Boolean bExiste;
            SqlCommand cmd1;
            Int32 iDif;
            String User, Pass, sDocEntry;
            SAPbobsCOM.Recordset orsAux;

            try
            {
                ActualizarGrilla();

                orsAux = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                if (bMultiSoc)
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select DocEntry, U_Servidor, U_Base, U_Usuario, U_Password from [@VID_FEMULTISOC] where U_Habilitada = 'Y'";
                    else
                        s = @"select TO_VARCHAR(""DocEntry"") ""DocEntry"", ""U_Servidor"", ""U_Base"", ""U_Usuario"", ""U_Password"" from ""@VID_FEMULTISOC"" where ""U_Habilitada"" = 'Y' ";
                }
                else
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select TOP 1 * from [@VID_FEPARAM]";
                    else
                        s = @"select TOP 1 * from ""@VID_FEPARAM"" ";
                }
                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                {
                    Reg = new TFunctions();
                    Reg.SBO_f = FSBOf;
                    while (!oRecordSet.EoF)
                    {
                        if (bMultiSoc)
                        {
                            User = (System.String)(oRecordSet.Fields.Item("U_Usuario").Value);
                            Pass = (System.String)(oRecordSet.Fields.Item("U_Password").Value);
                        }
                        else
                        {
                            User = Reg.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Usuario").Value));
                            Pass = Reg.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Password").Value));
                        }

                        sCnn = Reg.sConexion((System.String)(oRecordSet.Fields.Item("U_Servidor").Value), (System.String)(oRecordSet.Fields.Item("U_Base").Value), User, Pass);
                        sDocEntry = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value));
                        if (sCnn.Substring(0, 1) != "E")
                        {
                            ConexionADO = new SqlConnection(sCnn);
                            if (ConexionADO.State == ConnectionState.Closed) ConexionADO.Open();

                            s = @"SELECT CAST(TPDOCUMENTO AS VARCHAR(20)) 'TipoDoc'
                                  ,CAST(RANGODESDE AS VARCHAR(20)) 'Desde'
	                              ,CAST(RANGOHASTA AS VARCHAR(20)) 'Hasta'
	                              ,CONVERT(Char(8),FECHA,112) 'Fecha'
                                  ,CAST((RANGOHASTA - (RANGODESDE + UTILIZADOS)) + 1 AS VARCHAR(20)) 'Asig'
                                  ,CAST(ID as varchar(20)) 'ID'
						          ,CAST((RANGODESDE + UTILIZADOS) AS VARCHAR(20)) 'FolioDesde'
                                  ,CAST(UTILIZADOS AS VARCHAR(20)) 'Utilizados'
                                  ,ISNULL(XML_CAF,'') 'XML_CAF'
                              FROM folios WITH(nolock)";

                            cmd = new SqlDataAdapter(s, ConexionADO);
                            resultDataTable = new System.Data.DataTable();
                            cmd.Fill(resultDataTable);
                            foreach (System.Data.DataRow oRow in resultDataTable.Rows)
                            {
                                bExiste = false;
                                i = 0;
                                if (!oDataTable.IsEmpty)
                                {
                                    while (i < oDataTable.Rows.Count)
                                    {
                                        s = oDataTable.Rows.Count.ToString();
                                        var dFecha = (System.DateTime)(oDataTable.GetValue("U_Fecha", i));
                                        if (((System.String)(oDataTable.GetValue("U_TipoDoc", i)) == oRow.Field<String>("TipoDoc").ToString()) &&
                                            (Convert.ToString((System.Int32)(oDataTable.GetValue("U_Desde", i))) == oRow.Field<String>("Desde")) &&
                                            (Convert.ToString((System.Int32)(oDataTable.GetValue("U_Hasta", i))) == oRow.Field<String>("Hasta")) &&
                                            (dFecha.ToString("yyyyMMdd") == oRow.Field<String>("Fecha")))
                                        {
                                            bExiste = true;
                                            i = oDataTable.Rows.Count;
                                        }
                                        i++;
                                    }
                                }

                                if (!bExiste)
                                {
                                    sCode = Reg.sNuevoDocEntryLargo("@VID_FECAF", GlobalSettings.RunningUnderSQLServer);
                                    oDBDSH.Clear();
                                    oDBDSH.InsertRecord(0);
                                    oDBDSH.SetValue("Code", 0, sCode);
                                    //OutLog("Code -> " + oDBDSH.GetValue("Code",0));
                                    oDBDSH.SetValue("U_TipoDoc", 0, oRow.Field<String>("TipoDoc"));
                                    //OutLog("TipoDoc -> " + oDBDSH.GetValue("U_TipoDoc", 0));
                                    oDBDSH.SetValue("U_Desde", 0, oRow.Field<String>("Desde"));
                                    //OutLog("Desde -> " + oDBDSH.GetValue("U_Desde", 0));
                                    oDBDSH.SetValue("U_Hasta", 0, oRow.Field<String>("Hasta"));
                                    //OutLog("Hasta -> " + oDBDSH.GetValue("U_Hasta", 0));
                                    oDBDSH.SetValue("U_Fecha", 0, oRow.Field<String>("Fecha"));
                                    //OutLog("Fecha -> " + oDBDSH.GetValue("U_Fecha", 0));
                                    oDBDSH.SetValue("U_BaseMul", 0, sDocEntry);
                                    //OutLog("BaseMul -> " + oDBDSH.GetValue("U_BaseMul", 0));
                                    s = oRow.Field<String>("XML_CAF");
                                    oDBDSH.SetValue("U_CAF", 0, s);
                                    //OutLog("CAF -> " + oDBDSH.GetValue("U_CAF", 0));

                                    iDif = (Int32.Parse(oRow.Field<String>("Hasta")) - Int32.Parse(oRow.Field<String>("Desde"))) + 1;

                                    cmd1 = new SqlCommand();
                                    cmd1.CommandTimeout = 0;
                                    cmd1.CommandType = CommandType.Text;
                                    cmd1.Connection = ConexionADO;
                                    s = @"WITH n(n) AS
                                        (
                                        SELECT 1
                                            UNION ALL
                                        SELECT n + 1 
                                          FROM n WHERE n < {0} )
                                        SELECT COUNT(*) --n + {1} , T0.CAB_FOL_DOCTO_INT
                                          FROM n left outer join Faet_Erp_Encabezado_Doc T0 on n.n + {1} = T0.CAB_FOL_DOCTO_INT and T0.CAB_COD_TP_FACTURA = '{2}' 
                                           and T0.CAB_FOL_DOCTO_INT between {3} and {4} 
                                          where t0.CAB_FOL_DOCTO_INT is not null 
                                        --ORDER BY n 
                                        OPTION (MAXRECURSION 0)"; // antes OPTION (MAXRECURSION {0})";

                                    s = String.Format(s, iDif.ToString(), Int32.Parse(oRow.Field<String>("Desde")) - 1, oRow.Field<String>("TipoDoc"), oRow.Field<String>("Desde"), oRow.Field<String>("Hasta"));
                                    cmd1.CommandText = s;

                                    s = cmd1.ExecuteScalar().ToString();

                                    oDBDSH.SetValue("U_Utilizados", 0, s);
                                    //OutLog("Utilizados -> " + oDBDSH.GetValue("U_Utilizados", 0));
                                    iDif = iDif - Int32.Parse(s);
                                    oDBDSH.SetValue("U_Asignables", 0, iDif.ToString());
                                    //OutLog("Asignables -> " + oDBDSH.GetValue("U_Asignables", 0));

                                    s = @"WITH n(n) AS
                                        (
                                        SELECT 1
                                            UNION ALL
                                        SELECT n + 1 
                                          FROM n WHERE n < {0} )
                                        SELECT TOP 1 n + {1} --, T0.CAB_FOL_DOCTO_INT
                                          FROM n left outer join Faet_Erp_Encabezado_Doc T0 on n.n + {1} = T0.CAB_FOL_DOCTO_INT and T0.CAB_COD_TP_FACTURA = '{2}' 
                                           and T0.CAB_FOL_DOCTO_INT between {3} and {4} 
                                          where t0.CAB_FOL_DOCTO_INT is null 
                                        ORDER BY n 
                                        OPTION (MAXRECURSION 0)"; // antes OPTION (MAXRECURSION {0})";

                                    s = String.Format(s, iDif.ToString(), Int32.Parse(oRow.Field<String>("Desde")) - 1, oRow.Field<String>("TipoDoc"), oRow.Field<String>("Desde"), oRow.Field<String>("Hasta"));
                                    cmd1.CommandText = s;
                                    if (cmd1.ExecuteScalar() == null)
                                    { s = oRow.Field<String>("Desde"); }
                                    else
                                    { s = cmd1.ExecuteScalar().ToString(); }
                                    oDBDSH.SetValue("U_FolioDesde", 0, s);
                                    //OutLog("FolioDesde -> " + oDBDSH.GetValue("U_FolioDesde", 0));

                                    _return = Reg.CAFAdd(oDBDSH);
                                    if (_return)
                                    { FSBOApp.StatusBar.SetText("CAF Registrado, ID " + oRow.Field<String>("ID"), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success); }
                                    else
                                    { FSBOApp.StatusBar.SetText("CAF no se ha registrado, ID " + oRow.Field<String>("ID"), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
                                }
                            }

                            if (ConexionADO.State == ConnectionState.Open) ConexionADO.Close();

                            FSBOApp.StatusBar.SetText("CAF actualizados", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                        }
                        else
                            FSBOApp.StatusBar.SetText("Faltan datos Conexion. " + sCnn.Substring(1, sCnn.Length - 1), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        oRecordSet.MoveNext();
                    }

                    ActualizarGrilla();
                }
                else
                { FSBOApp.StatusBar.SetText("Debe ingresar datos de conexion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("ActualizarRegistros: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin ActualizarRegistros

        private void ActualizarGrilla()
        {
            SAPbouiCOM.GridColumn oColumn;

            try
            {
                //Actualiza grilla
                if (GlobalSettings.RunningUnderSQLServer)
                {
                    s = @"select Code, CAST(U_BaseMul AS VARCHAR(11)) U_BaseMul, U_TipoDoc, U_Fecha, U_Desde, U_Hasta, U_Utilizados, U_Asignables, U_CAF
                                from [@VID_FECAF]";
                }
                else
                {
                    s = @"select ""Code"", TO_VARCHAR(""U_BaseMul"") ""U_BaseMul"", ""U_TipoDoc"", ""U_Fecha"", ""U_Desde"", ""U_Hasta"", ""U_Utilizados"", ""U_Asignables"", ""U_CAF""
                                from ""@VID_FECAF"" ";
                }
                oDataTable.ExecuteQuery(s);

                ogrid.Columns.Item("Code").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("Code"));
                //EditTextColumn(oColumn).LinkedObjectType := '86';
                oColumn.Editable = false;
                oColumn.Visible = false;
                oColumn.TitleObject.Caption = "Código";

                ogrid.Columns.Item("U_BaseMul").Type = BoGridColumnType.gct_ComboBox;
                var colCombo = (ComboBoxColumn)(ogrid.Columns.Item("U_BaseMul"));
                colCombo.DisplayType = BoComboDisplayType.cdt_Description;
                colCombo.TitleObject.Caption = "Sociedad";
                colCombo.Editable = false;
                if (bMultiSoc)
                {
                    colCombo.Visible = true;
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select DocEntry, U_Sociedad 'Sociedad' from [@VID_FEMULTISOC]";
                    else
                        s = @"select ""DocEntry"", ""U_Sociedad"" ""Sociedad"" from ""@VID_FEMULTISOC"" ";
                    oRecordSet.DoQuery(s);
                    while (!oRecordSet.EoF)
                    {
                        colCombo.ValidValues.Add(((System.Int32)oRecordSet.Fields.Item("DocEntry").Value).ToString(), ((System.String)oRecordSet.Fields.Item("Sociedad").Value).Trim());
                        oRecordSet.MoveNext();
                    }
                }
                else
                    colCombo.Visible = false;

                ogrid.Columns.Item("U_TipoDoc").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_TipoDoc"));
                //EditTextColumn(oColumn).LinkedObjectType := '86';
                oColumn.Editable = false;
                oColumn.RightJustified = true;
                oColumn.TitleObject.Caption = "Tipo Documento";

                ogrid.Columns.Item("U_Fecha").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_Fecha"));
                //EditTextColumn(oColumn).LinkedObjectType := '86';
                oColumn.Editable = false;
                oColumn.TitleObject.Caption = "Fecha CAF";

                ogrid.Columns.Item("U_Desde").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_Desde"));
                //EditTextColumn(oColumn).LinkedObjectType := '86';
                oColumn.Editable = false;
                oColumn.RightJustified = true;
                oColumn.TitleObject.Caption = "Desde";

                ogrid.Columns.Item("U_Hasta").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_Hasta"));
                //EditTextColumn(oColumn).LinkedObjectType := '86';
                oColumn.Editable = false;
                oColumn.RightJustified = true;
                oColumn.TitleObject.Caption = "Hasta";

                ogrid.Columns.Item("U_Utilizados").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_Utilizados"));
                //EditTextColumn(oColumn).LinkedObjectType := '86';
                oColumn.Editable = false;
                oColumn.RightJustified = true;
                oColumn.TitleObject.Caption = "CAF Utilizados";

                ogrid.Columns.Item("U_Asignables").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_Asignables"));
                //EditTextColumn(oColumn).LinkedObjectType := '86';
                oColumn.Editable = false;
                oColumn.RightJustified = true;
                oColumn.TitleObject.Caption = "Asignables";

                ogrid.Columns.Item("U_CAF").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_CAF"));
                //EditTextColumn(oColumn).LinkedObjectType := '86';
                oColumn.Editable = false;
                oColumn.TitleObject.Caption = "XML CAF";

                ogrid.AutoResizeColumns();
            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText(x.Message + " ** Trace: " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("ActualizarGrilla: " + x.Message + " ** Trace: " + x.StackTrace);
            }
        }

    }//fin Class
}
