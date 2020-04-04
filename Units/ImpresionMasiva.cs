using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using VisualD.untLog;
using Factura_Electronica_VK.Functions;
using Factura_Electronica_VK.CreditNotes;
using Factura_Electronica_VK.DeliveryNote;
using Factura_Electronica_VK.Invoice;
using Factura_Electronica_VK.PurchaseInvoice;
using FactRemota;
using SAPbouiCOM;
using SAPbobsCOM;

namespace Factura_Electronica_VK.ImpresionMasiva
{
    class TImpresionMasiva : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private String s;
        private SqlConnection ConexionADO = null;
        private String Localidad = "";


        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                //FSBOf.LoadForm(xmlPath, 'VID_Entrega.srf', Uid);
                oForm = FSBOApp.Forms.Item(uid);
                //Flag := false;
                /*
                   if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select ISNULL(T0.U_Localidad,'CL') Localidad from [@VID_FEPARAM] T0";
                    else
                        s = @"select IFNULL(T0.""U_Localidad"",'CL') ""Localidad"" from ""@VID_FEPARAM"" T0 ";
                
                oRecordSet.DoQuery(s);*/
                Localidad = "CL"; //((System.String)oRecordSet.Fields.Item("Localidad").Value).Trim();
                if (Localidad == "CL")
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                    {
                        try
                        {
                            if (GlobalSettings.RunningUnderSQLServer)
                                s = "SELECT T0.U_Srvr 'Server', T0.U_Usr 'Usuario', T0.U_Pw 'Password' FROM [dbo].[@VID_MENUSU] T0";
                            else
                                s = @"SELECT T0.""U_Srvr"" ""Server"", T0.""U_Usr"" ""Usuario"", T0.""U_Pw"" ""Password"" FROM ""@VID_MENUSU"" T0";
                            oRecordSet.DoQuery(s);
                        }
                        catch //(Exception t)
                        {
                            FSBOApp.StatusBar.SetText("Los datos de acceso al servidor SQL no son validos (Gestion->Definiciones->Recursos Humanos->Setup Acceso SQL Server), guarde los datos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            FSBOApp.ActivateMenuItem("VID_RHSQL");
                            return false;
                        }

                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            ConexionADO = new SqlConnection("Data Source = " + FCmpny.Server + "; Initial Catalog = " + FCmpny.CompanyDB + "; User Id=" + ((System.String)oRecordSet.Fields.Item("Usuario").Value).Trim() + ";Password=" + ((System.String)oRecordSet.Fields.Item("Password").Value).Trim());

                            try
                            {
                                ConexionADO.Open();
                            }
                            catch //(Exception t)
                            {
                                FSBOApp.StatusBar.SetText("Los datos de acceso al servidor SQL no son validos (Gestion->Definiciones->Recursos Humanos->Setup Acceso SQL Server), guarde los datos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                FSBOApp.ActivateMenuItem("VID_RHSQL");
                                return false;
                            }
                            ConexionADO.Close();
                        }
                    }
                }
                oForm.Freeze(true);
            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            oForm.Visible = true;
            oForm.Freeze(false);
            return Result;
        }//fin InitForm


        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            Int32 nErr;
            String sErr;
            SAPbouiCOM.Form oFormAux;
            String DocEntry;
            String Tabla;
            XmlDocument _xmlDocument;
            XmlNode N;
            Boolean FolioUnico;
            Boolean bMultiSoc;
            String nMultiSoc;
            SAPbouiCOM.EditText oEditText;
            String[] FE52 = { "15", "67", "21" };

            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if (pVal.ItemUID == "4")
                    {
                        s = GlobalSettings.PrevFormUID;
                        oFormAux = FSBOApp.Forms.Item(s);
                        //if (oFormAux.BusinessObject.Type in ['15','67','21'])
                        if (FE52.Contains(oFormAux.BusinessObject.Type))
                        {
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = "select isnull(U_FolioGuia,'N') FolioUnico, ISNULL(U_Distrib,'N') 'Distribuido', ISNULL(U_MultiSoc,'N') MultiSoc from [@VID_FEPARAM] where code = '1'"; }
                            else
                            { s = @"select IFNULL(""U_FolioGuia"",'N') ""FolioUnico"", IFNULL(""U_Distrib"",'N') ""Distribuido"", IFNULL(""U_MultiSoc"",'N') ""MultiSoc"" from ""@VID_FEPARAM"" where ""Code"" = '1' "; }
                            oRecordSet.DoQuery(s);

                            if ((System.String)(oRecordSet.Fields.Item("Distribuido").Value) == "N")
                            {
                                if ((System.String)(oRecordSet.Fields.Item("MultiSoc").Value) == "Y")
                                { bMultiSoc = true; }
                                else
                                { bMultiSoc = false; }
                                if ((System.String)(oRecordSet.Fields.Item("FolioUnico").Value) == "Y")
                                { FolioUnico = true; }
                                else
                                { FolioUnico = false; }

                                if (FolioUnico)
                                {
                                    if (oFormAux.BusinessObject.Type == "15")
                                    { Tabla = "ODLN"; }
                                    else if (oFormAux.BusinessObject.Type == "21")
                                    { Tabla = "ORPD"; }
                                    else
                                    { Tabla = "OWTR"; }

                                    _xmlDocument = new XmlDocument();
                                    _xmlDocument.LoadXml(oFormAux.BusinessObject.Key);
                                    N = _xmlDocument.SelectSingleNode("DocumentParams");
                                    DocEntry = N.InnerText;

                                    if (GlobalSettings.RunningUnderSQLServer)
                                    {
                                        s = @"SELECT Count(*) Cont, SUBSTRING(ISNULL(T0.BeginStr,''), 2, LEN(T0.BeginStr)) Inst
                                                FROM NNM1 T0
                                                JOIN {0} T1 ON T1.Series = T0.Series
                                               WHERE (SUBSTRING(UPPER(T0.BeginStr), 1, 1) = 'E') 
                                                 AND T1.DocEntry = {1}
                                                 --AND T0.ObjectCode = '{2}'
                                               GROUP BY SUBSTRING(ISNULL(T0.BeginStr,''), 2, LEN(T0.BeginStr))";
                                    }
                                    else
                                    {
                                        s = @"SELECT Count(*) Cont, SUBSTRING(IFNULL(T0.""BeginStr"",''), 2, LENGTH(T0.""BeginStr"")) ""Inst""
                                                FROM ""NNM1"" T0
                                                JOIN ""{0}"" T1 ON T1.""Series"" = T0.""Series""
                                               WHERE (SUBSTRING(UPPER(T0.""BeginStr""), 1, 1) = 'E') 
                                                 AND T1.""DocEntry"" = {1} 
                                                 --AND T0.""ObjectCode"" = '{2}'
                                               GROUP BY SUBSTRING(IFNULL(T0.""BeginStr"",''), 2, LENGTH(T0.""BeginStr"")) ";
                                    }
                                    s = String.Format(s, Tabla, DocEntry, oFormAux.BusinessObject.Type);
                                    oRecordSet.DoQuery(s);

                                    if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                                    {
                                        nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                                        if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        { FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                                        else
                                        {
                                            BubbleEvent = false;
                                            IngresarFolio(DocEntry, oFormAux.BusinessObject.Type);
                                            s = "--";
                                            var oDeliveryNote = new TDeliveryNote();
                                            oDeliveryNote.SBO_f = FSBOf;
                                            if (oFormAux.BusinessObject.Type == "15")
                                                oDeliveryNote.EnviarFE_WebService(DocEntry, s, false, false, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52", oFormAux.BusinessObject.Type, false);
                                            else if (oFormAux.BusinessObject.Type == "21")
                                                oDeliveryNote.EnviarFE_WebService(DocEntry, s, false, false, true, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52D", oFormAux.BusinessObject.Type, false);
                                            else if (oFormAux.BusinessObject.Type == "67")
                                                oDeliveryNote.EnviarFE_WebService(DocEntry, s, true, false, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52T", oFormAux.BusinessObject.Type, false);
                                            oForm.Close();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_FORM_LOAD) && (!pVal.BeforeAction))
                {
                    s = GlobalSettings.PrevFormUID;
                    oFormAux = FSBOApp.Forms.Item(s);
                    //if (oFormAux.BusinessObject.Type in ['15','67','21'])
                    if (FE52.Contains(oFormAux.BusinessObject.Type))
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                        { s = "select isnull(U_FolioGuia,'N') FolioUnico from [@VID_FEPARAM] where code = '1'"; }
                        else
                        { s = @"select IFNULL(""U_FolioGuia"",'N') ""FolioUnico"" from ""@VID_FEPARAM"" where ""Code"" = '1' "; }

                        oRecordSet.DoQuery(s);
                        if ((System.String)(oRecordSet.Fields.Item("FolioUnico").Value) == "Y")
                        { FolioUnico = true; }
                        else
                        { FolioUnico = false; }

                        if (FolioUnico)
                        {
                            if (oFormAux.BusinessObject.Type == "15")
                            { Tabla = "ODLN"; }
                            else if (oFormAux.BusinessObject.Type == "21")
                            { Tabla = "ORPD"; }
                            else
                            { Tabla = "OWTR"; }

                            _xmlDocument = new XmlDocument();
                            _xmlDocument.LoadXml(oFormAux.BusinessObject.Key);
                            N = _xmlDocument.SelectSingleNode("DocumentParams");
                            DocEntry = N.InnerText;

                            if (GlobalSettings.RunningUnderSQLServer)
                            {
                                s = @"SELECT Count(*) Cont
                                        FROM NNM1 T0
                                        JOIN {0} T1 ON T1.Series = T0.Series
                                       WHERE (SUBSTRING(UPPER(T0.BeginStr), 1, 1) = 'E') 
                                         AND T1.DocEntry = {1}
                                         --AND T0.ObjectCode = '{2}' ";
                            }
                            else
                            {
                                s = @"SELECT Count(*) ""Cont""
                                        FROM ""NNM1"" T0
                                        JOIN ""{0}"" T1 ON T1.""Series"" = T0.""Series"" 
                                       WHERE (SUBSTRING(UPPER(T0.""BeginStr""), 1, 1) = 'E') 
                                         AND T1.""DocEntry"" = {1}
                                         --AND T0.""ObjectCode"" = '{2}' ";
                            }
                            s = String.Format(s, Tabla, DocEntry, oFormAux.BusinessObject.Type);
                            oRecordSet.DoQuery(s);
                            if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                            {
                                if (GlobalSettings.RunningUnderSQLServer)
                                { s = "SELECT 'GE' BeginStr, NextNumber FROM NNM1 WHERE (ObjectCode = 'VD_FEEntreg')"; }
                                else
                                { s = @"SELECT 'GE' ""BeginStr"", ""NextNumber"" FROM ""NNM1"" WHERE (""ObjectCode"" = 'VD_FEEntreg') "; }

                                oRecordSet.DoQuery(s);
                                s = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("NextNumber").Value));
                                oEditText = (EditText)(oForm.Items.Item("7").Specific);
                                oEditText.Value = s;
                                //s := System.String(oRecordSet.Fields.Item("BeginStr").Value);
                                //EditText(oForm.Items.Item("3").Specific).Value := s;
                            }
                        }
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


        public new void FormDataEvent(ref BusinessObjectInfo BusinessObjectInfo, ref Boolean BubbleEvent)
        {
            String DocEntry;
            Boolean bMultiSoc;
            String nMultiSoc;
            XmlDocument _xmlDocument;
            XmlNode N;
            String TipoDocElect;
            String GeneraT = "";
            String TaxIdNum = "";
            String DocSubType = "";
            String[] FE52 = { "15", "67", "21" };
            SAPbobsCOM.Documents oDocuments = null;
            SAPbobsCOM.StockTransfer oStockTransfer = null;
            base.FormDataEvent(ref BusinessObjectInfo, ref BubbleEvent);
            try
            {
                if ((BusinessObjectInfo.BeforeAction == false) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_UPDATE) && (BusinessObjectInfo.ActionSuccess))
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select ISNULL(T0.U_Distrib,'N') 'Distribuido', ISNULL(T0.U_MultiSoc,'N') MultiSoc, ISNULL(T0.U_GenerarT,'N') GeneraT
                                  , REPLACE(ISNULL(A0.TaxIdNum,''),'.','') TaxIdNum from [@VID_FEPARAM] T0, OADM A0";
                    else
                        s = @"select IFNULL(T0.""U_Distrib"",'N') ""Distribuido"", IFNULL(T0.""U_MultiSoc"",'N') ""MultiSoc"", IFNULL(T0.""U_GenerarT"",'N') ""GeneraT""
                                  , REPLACE(IFNULL(A0.""TaxIdNum"",''),'.','') ""TaxIdNum"" from ""@VID_FEPARAM"" T0, ""OADM"" A0 ";

                    oRecordSet.DoQuery(s);

                    if (oRecordSet.RecordCount > 0)
                    {
                        if ((System.String)(oRecordSet.Fields.Item("Distribuido").Value) == "N")
                        {
                            GeneraT = ((System.String)oRecordSet.Fields.Item("GeneraT").Value).Trim();
                            TaxIdNum = ((System.String)oRecordSet.Fields.Item("TaxIdNum").Value).Trim();
                            if ((System.String)(oRecordSet.Fields.Item("MultiSoc").Value) == "Y")
                                bMultiSoc = true;
                            else
                                bMultiSoc = false;

                            if (oForm.BusinessObject.Type == "13") //And (Flag = true)) then
                            {
                                //Flag := false;
                                DocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = @"select T0.DocSubType, 
                                                SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', 
                                                SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst,
                                                SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) TipoDocElect 
                                           from oinv T0 
                                           JOIN NNM1 T2 ON T0.Series = T2.Series 
                                          where T0.DocEntry = {0}";
                                else
                                    s = @"select T0.""DocSubType"", 
                                                SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", 
                                                SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"",
                                                SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""TipoDocElect"" 
                                           from ""OINV"" T0 
                                           JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                                          where T0.""DocEntry"" = {0} ";

                                s = String.Format(s, DocEntry);
                                oRecordSet.DoQuery(s);
                                DocSubType = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                if (((System.String)(oRecordSet.Fields.Item("TipoDocElect").Value) == "111") && (DocSubType == "DN"))
                                    TipoDocElect = "111";
                                else if (((System.String)(oRecordSet.Fields.Item("TipoDocElect").Value) != "111") && (DocSubType == "DN"))
                                    TipoDocElect = "56";
                                else if ((System.String)(oRecordSet.Fields.Item("DocSubType").Value) == "IE") //Factura Exenta
                                    TipoDocElect = "34";
                                else if ((System.String)(oRecordSet.Fields.Item("DocSubType").Value) == "IB") //Boleta
                                    TipoDocElect = "39";
                                else if ((System.String)(oRecordSet.Fields.Item("DocSubType").Value) == "EB") //Boleta Exenta
                                    TipoDocElect = "41";
                                else if ((System.String)(oRecordSet.Fields.Item("DocSubType").Value) == "IX") //Factura Exportacion
                                    TipoDocElect = "110";
                                else
                                    TipoDocElect = "--";

                                s = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                if ((System.String)(oRecordSet.Fields.Item("Tipo").Value) == "E")
                                {
                                    nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                                    if ((bMultiSoc == true) && (nMultiSoc == ""))
                                    { FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                                    else
                                    {
                                        if (GeneraT == "Y")
                                        {
                                            if (oForm.BusinessObject.Type == "203")
                                                oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oDownPayments));
                                            else
                                                oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oInvoices));

                                            if (oDocuments.GetByKey(Convert.ToInt32(DocEntry)))
                                            {
                                                if (bMultiSoc == false)
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta";
                                                    else
                                                        s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" ";
                                                    s = String.Format(s, TipoDocElect, oDocuments.FolioNumber);
                                                }
                                                else
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta and U_BaseMul = '{2}'";
                                                    else
                                                        s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" and ""U_BaseMul"" = '{2}'";
                                                    s = String.Format(s, TipoDocElect, oDocuments.FolioNumber, nMultiSoc);
                                                }
                                                oRecordSet.DoQuery(s);
                                                String CAF = ((System.String)oRecordSet.Fields.Item("U_CAF").Value).Trim();

                                                //Colocar Timbre
                                                XmlDocument xmlCAF = new XmlDocument();
                                                XmlDocument xmlTimbre = new XmlDocument();

                                                if (CAF == "")
                                                    throw new Exception("No se ha encontrado xml de CAF");
                                                //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                xmlCAF.LoadXml(CAF);
                                                xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElect, Convert.ToString(oDocuments.FolioNumber), oDocuments.DocDate.ToString("yyyyMMdd"), oDocuments.FederalTaxID.Replace(".",""), oDocuments.CardName, Convert.ToString(Math.Round(oDocuments.DocTotal, 0)), oDocuments.Lines.ItemDescription, xmlCAF, TaxIdNum);

                                                var sw = new StringWriter();
                                                XmlTextWriter tx = new XmlTextWriter(sw);
                                                xmlTimbre.WriteTo(tx);

                                                s = sw.ToString();// 

                                                if (s != "")
                                                    oDocuments.UserFields.Fields.Item("U_FETimbre").Value = s;
                                                else
                                                    throw new Exception("No se ha creado timbre Guia Electronica");

                                                var lRetCode = oDocuments.Update();
                                                if (lRetCode != 0)
                                                {
                                                    s = FCmpny.GetLastErrorDescription();
                                                    FSBOApp.StatusBar.SetText("Error actualizar documento con firma FE, " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                    OutLog("Error actualizar documento con firma FE, " + s);
                                                }
                                            }
                                        }
                                        var oInvoice = new TInvoice();
                                        oInvoice.SBO_f = FSBOf;
                                        oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oInvoices));
                                        if (oDocuments.GetByKey(Convert.ToInt32(DocEntry)))//**se dejo la normal mientras se termina la modificacion en el portal 20170202
                                            oInvoice.EnviarFE_WebService(oForm.BusinessObject.Type, oDocuments, TipoDocElect, bMultiSoc, nMultiSoc, GlobalSettings.RunningUnderSQLServer, DocSubType, TipoDocElect, false);
                                            //oInvoice.EnviarFE(DocEntry, DocSubType, oForm.BusinessObject.Type, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, TipoDocElec);
                                        else
                                            FSBOApp.StatusBar.SetText("No se ha encontrado documento " + TipoDocElect, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    }
                                }
                            }
                            else if (oForm.BusinessObject.Type == "203") //Factura de Anticipo
                            {
                                //Flag := false;
                                TipoDocElect = "33";
                                DocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst 
                                            from ODPI T0 JOIN NNM1 T2 ON T0.Series = T2.Series where T0.DocEntry = {0}";
                                else
                                    s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"" 
                                            from ""ODPI"" T0 JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" where T0.""DocEntry"" = {0} ";

                                s = String.Format(s, DocEntry);
                                oRecordSet.DoQuery(s);
                                s = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                if ((System.String)(oRecordSet.Fields.Item("Tipo").Value) == "E")
                                {
                                    nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                                    if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                    else
                                    {
                                        if (GeneraT == "Y")
                                        {
                                            oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oDownPayments));
                                            
                                            if (oDocuments.GetByKey(Convert.ToInt32(DocEntry)))
                                            {
                                                if (bMultiSoc == false)
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta";
                                                    else
                                                        s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" ";
                                                    s = String.Format(s, TipoDocElect, oDocuments.FolioNumber);
                                                }
                                                else
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta and U_BaseMul = '{2}'";
                                                    else
                                                        s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" and ""U_BaseMul"" = '{2}'";
                                                    s = String.Format(s, TipoDocElect, oDocuments.FolioNumber, nMultiSoc);
                                                }
                                                oRecordSet.DoQuery(s);
                                                String CAF = ((System.String)oRecordSet.Fields.Item("U_CAF").Value).Trim();

                                                //Colocar Timbre
                                                XmlDocument xmlCAF = new XmlDocument();
                                                XmlDocument xmlTimbre = new XmlDocument();

                                                if (CAF == "")
                                                    throw new Exception("No se ha encontrado xml de CAF");
                                                //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                xmlCAF.LoadXml(CAF);
                                                xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElect, Convert.ToString(oDocuments.FolioNumber), oDocuments.DocDate.ToString("yyyyMMdd"), oDocuments.FederalTaxID.Replace(".",""), oDocuments.CardName, Convert.ToString(Math.Round(oDocuments.DocTotal, 0)), oDocuments.Lines.ItemDescription, xmlCAF, TaxIdNum);

                                                var sw = new StringWriter();
                                                XmlTextWriter tx = new XmlTextWriter(sw);
                                                xmlTimbre.WriteTo(tx);

                                                s = sw.ToString();// 

                                                if (s != "")
                                                    oDocuments.UserFields.Fields.Item("U_FETimbre").Value = s;
                                                else
                                                    throw new Exception("No se ha creado timbre Guia Electronica");

                                                var lRetCode = oDocuments.Update();
                                                if (lRetCode != 0)
                                                {
                                                    s = FCmpny.GetLastErrorDescription();
                                                    FSBOApp.StatusBar.SetText("Error actualizar documento con firma FE, " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                    OutLog("Error actualizar documento con firma FE, " + s);
                                                }
                                            }
                                        }

                                        var oInvoice = new TInvoice();
                                        oInvoice.SBO_f = FSBOf;
                                        oDocuments = null;
                                        oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oDownPayments));
                                        if (oDocuments.GetByKey(Convert.ToInt32(DocEntry)))//**se dejo la normal mientras se termina la modificacion en el portal 20170202
                                            oInvoice.EnviarFE_WebService(oForm.BusinessObject.Type, oDocuments, TipoDocElect, false, "", GlobalSettings.RunningUnderSQLServer, "--", "33A", false);
                                        //EnviarFE(DocEntry);
                                    }
                                }
                            }
                            else if ((oForm.BusinessObject.Type == "18") || (oForm.BusinessObject.Type == "204")) //And (Flag = true)) then
                            {
                                //Flag := false;
                                DocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                                var table = "";
                                if (oForm.BusinessObject.Type == "204")
                                    table = "ODPO";
                                else
                                    table = "OPCH";

                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = "select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst from {1} T0 JOIN NNM1 T2 ON T0.Series = T2.Series where T0.DocEntry = {0}";
                                else
                                    s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"" from ""{1}"" T0 JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" where T0.""DocEntry"" = {0} ";

                                s = String.Format(s, DocEntry, table);
                                oRecordSet.DoQuery(s);
                                s = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                if ((System.String)(oRecordSet.Fields.Item("Tipo").Value) == "E")
                                {
                                    nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                                    if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                    else
                                    {
                                        if (GeneraT == "Y")
                                        {
                                            if (oForm.BusinessObject.Type == "18")
                                                oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oPurchaseInvoices));
                                            else
                                                oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oPurchaseDownPayments));
                                            TipoDocElect = "46";
                                            if (oDocuments.GetByKey(Convert.ToInt32(DocEntry)))
                                            {
                                                if (bMultiSoc == false)
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta";
                                                    else
                                                        s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" ";
                                                    s = String.Format(s, TipoDocElect, oDocuments.FolioNumber);
                                                }
                                                else
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta and U_BaseMul = '{2}'";
                                                    else
                                                        s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" and ""U_BaseMul"" = '{2}'";
                                                    s = String.Format(s, TipoDocElect, oDocuments.FolioNumber, nMultiSoc);
                                                }
                                                oRecordSet.DoQuery(s);
                                                String CAF = ((System.String)oRecordSet.Fields.Item("U_CAF").Value).Trim();

                                                //Colocar Timbre
                                                XmlDocument xmlCAF = new XmlDocument();
                                                XmlDocument xmlTimbre = new XmlDocument();

                                                if (CAF == "")
                                                    throw new Exception("No se ha encontrado xml de CAF");
                                                //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                xmlCAF.LoadXml(CAF);
                                                xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElect, Convert.ToString(oDocuments.FolioNumber), oDocuments.DocDate.ToString("yyyyMMdd"), oDocuments.FederalTaxID.Replace(".",""), oDocuments.CardName, Convert.ToString(Math.Round(oDocuments.DocTotal, 0)), oDocuments.Lines.ItemDescription, xmlCAF, TaxIdNum);

                                                var sw = new StringWriter();
                                                XmlTextWriter tx = new XmlTextWriter(sw);
                                                xmlTimbre.WriteTo(tx);

                                                s = sw.ToString();// 

                                                if (s != "")
                                                    oDocuments.UserFields.Fields.Item("U_FETimbre").Value = s;
                                                else
                                                    throw new Exception("No se ha creado timbre Guia Electronica");

                                                var lRetCode = oDocuments.Update();
                                                if (lRetCode != 0)
                                                {
                                                    s = FCmpny.GetLastErrorDescription();
                                                    FSBOApp.StatusBar.SetText("Error actualizar documento con firma FE, " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                    OutLog("Error actualizar documento con firma FE, " + s);
                                                }
                                            }
                                        }

                                        var oPurchaseInvoice = new TPurchaseInvoice();
                                        oPurchaseInvoice.SBO_f = FSBOf;
                                        oPurchaseInvoice.EnviarFE_WebService(DocEntry, s, oForm.BusinessObject.Type, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "46", (oForm.BusinessObject.Type == "18" ? "46": "46A"), false);
                                        //EnviarFE(DocEntry);
                                    }
                                }
                            }
                            else if ((oForm.BusinessObject.Type == "14") || (oForm.BusinessObject.Type == "19")) //And (Flag = true)) then
                            {
                                var table = "";
                                var TipoDocElecAddon = "";
                                if (oForm.BusinessObject.Type == "14")
                                {
                                    table = "ORIN";
                                    TipoDocElecAddon = "61";
                                }
                                else
                                {
                                    table = "ORPC";
                                    TipoDocElecAddon = "61C";
                                }

                                //Flag := false;
                                DocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = @"select T0.DocSubType, 
                                                SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', 
                                                SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst,
                                                SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) TipoDocElect 
                                           from {1} T0 
                                           JOIN NNM1 T2 ON T0.Series = T2.Series 
                                          where T0.DocEntry = {0}";
                                else
                                    s = @"select T0.""DocSubType"", 
                                                 SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", 
                                                 SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"",
                                                 SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""TipoDocElect"" 
                                            from ""{1}"" T0 
                                            JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                                           where T0.""DocEntry"" = {0} ";

                                s = String.Format(s, DocEntry, table);
                                oRecordSet.DoQuery(s);

                                if ((System.String)(oRecordSet.Fields.Item("TipoDocElect").Value) == "112")
                                {
                                    TipoDocElect = "112";
                                    TipoDocElecAddon = "112";
                                }
                                else
                                    TipoDocElect = "61";

                                var DocSubTypeNC = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                if ((System.String)(oRecordSet.Fields.Item("Tipo").Value) == "E")
                                {
                                    nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                                    if ((bMultiSoc == true) && (nMultiSoc == ""))
                                    { FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                                    else
                                    {
                                        if (GeneraT == "Y")
                                        {
                                            if (oForm.BusinessObject.Type == "14")
                                                oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oCreditNotes));
                                            else
                                                oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oPurchaseCreditNotes));

                                            if (oDocuments.GetByKey(Convert.ToInt32(DocEntry)))
                                            {
                                                if (bMultiSoc == false)
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta";
                                                    else
                                                        s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" ";
                                                    s = String.Format(s, TipoDocElect, oDocuments.FolioNumber);
                                                }
                                                else
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta and U_BaseMul = '{2}'";
                                                    else
                                                        s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" and ""U_BaseMul"" = '{2}'";
                                                    s = String.Format(s, TipoDocElect, oDocuments.FolioNumber, nMultiSoc);
                                                }
                                                oRecordSet.DoQuery(s);
                                                String CAF = ((System.String)oRecordSet.Fields.Item("U_CAF").Value).Trim();

                                                //Colocar Timbre
                                                XmlDocument xmlCAF = new XmlDocument();
                                                XmlDocument xmlTimbre = new XmlDocument();

                                                if (CAF == "")
                                                    throw new Exception("No se ha encontrado xml de CAF");
                                                //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                xmlCAF.LoadXml(CAF);
                                                xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElect, Convert.ToString(oDocuments.FolioNumber), oDocuments.DocDate.ToString("yyyyMMdd"), oDocuments.FederalTaxID.Replace(".",""), oDocuments.CardName, Convert.ToString(Math.Round(oDocuments.DocTotal, 0)), oDocuments.Lines.ItemDescription, xmlCAF, TaxIdNum);

                                                var sw = new StringWriter();
                                                XmlTextWriter tx = new XmlTextWriter(sw);
                                                xmlTimbre.WriteTo(tx);

                                                s = sw.ToString();// 

                                                if (s != "")
                                                    oDocuments.UserFields.Fields.Item("U_FETimbre").Value = s;
                                                else
                                                    throw new Exception("No se ha creado timbre Guia Electronica");

                                                var lRetCode = oDocuments.Update();
                                                if (lRetCode != 0)
                                                {
                                                    s = FCmpny.GetLastErrorDescription();
                                                    FSBOApp.StatusBar.SetText("Error actualizar documento con firma FE, " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                    OutLog("Error actualizar documento con firma FE, " + s);
                                                }
                                            }
                                        }
                                        var oCreditNotes = new TCreditNotes();
                                        oCreditNotes.SBO_f = FSBOf;
                                        oCreditNotes.EnviarFE_WebServiceNotaCredito(DocEntry, DocSubTypeNC, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, oForm.BusinessObject.Type, TipoDocElect, TipoDocElecAddon, false);
                                    }
                                }
                            }
                            //else if (oForm.BusinessObject.Type in ['15','67','21']) //And (Flag = true)) then
                            else if (FE52.Contains(oForm.BusinessObject.Type))
                            {
                                //Flag := false;
                                DocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                                if (GlobalSettings.RunningUnderSQLServer)
                                {
                                    if (oForm.BusinessObject.Type == "15")
                                        s = "select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst from ODLN T0 JOIN NNM1 T2 ON T0.Series = T2.Series where T0.DocEntry = {0}";
                                    else if (oForm.BusinessObject.Type == "21")
                                        s = "select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst from ORPD T0 JOIN NNM1 T2 ON T0.Series = T2.Series where T0.DocEntry = {0}";
                                    else if (oForm.BusinessObject.Type == "67")
                                        s = "select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst from OWTR T0 JOIN NNM1 T2 ON T0.Series = T2.Series where T0.DocEntry = {0}";
                                }
                                else
                                {
                                    if (oForm.BusinessObject.Type == "15")
                                        s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"" from ""ODLN"" T0 JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" where T0.""DocEntry"" = {0} ";
                                    else if (oForm.BusinessObject.Type == "21")
                                        s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"" from ""ORPD"" T0 JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" where T0.""DocEntry"" = {0} ";
                                    else if (oForm.BusinessObject.Type == "67")
                                        s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"" from ""OWTR"" T0 JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" where T0.""DocEntry"" = {0} ";
                                }

                                s = String.Format(s, DocEntry);
                                oRecordSet.DoQuery(s);
                                s = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                if ((System.String)(oRecordSet.Fields.Item("Tipo").Value) == "E")
                                {
                                    nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                                    if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                    else
                                    {
                                        TipoDocElect = "52";
                                        if (GeneraT == "Y")
                                        {
                                            if ((oForm.BusinessObject.Type == "15") || (oForm.BusinessObject.Type == "21"))
                                            {
                                                if (oForm.BusinessObject.Type == "15")
                                                    oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oDeliveryNotes));
                                                else
                                                    oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oPurchaseReturns));

                                                if (oDocuments.GetByKey(Convert.ToInt32(DocEntry)))
                                                {
                                                    if (bMultiSoc == false)
                                                    {
                                                        if (GlobalSettings.RunningUnderSQLServer)
                                                            s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta";
                                                        else
                                                            s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" ";
                                                        s = String.Format(s, TipoDocElect, oDocuments.FolioNumber);
                                                    }
                                                    else
                                                    {
                                                        if (GlobalSettings.RunningUnderSQLServer)
                                                            s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta and U_BaseMul = '{2}'";
                                                        else
                                                            s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" and ""U_BaseMul"" = '{2}'";
                                                        s = String.Format(s, TipoDocElect, oDocuments.FolioNumber, nMultiSoc);
                                                    }
                                                    oRecordSet.DoQuery(s);
                                                    String CAF = ((System.String)oRecordSet.Fields.Item("U_CAF").Value).Trim();

                                                    //Colocar Timbre
                                                    XmlDocument xmlCAF = new XmlDocument();
                                                    XmlDocument xmlTimbre = new XmlDocument();

                                                    if (CAF == "")
                                                        throw new Exception("No se ha encontrado xml de CAF");
                                                    //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                    xmlCAF.LoadXml(CAF);
                                                    xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElect, Convert.ToString(oDocuments.FolioNumber), oDocuments.DocDate.ToString("yyyyMMdd"), oDocuments.FederalTaxID.Replace(".",""), oDocuments.CardName, Convert.ToString(Math.Round(oDocuments.DocTotal, 0)), oDocuments.Lines.ItemDescription, xmlCAF, TaxIdNum);

                                                    var sw = new StringWriter();
                                                    XmlTextWriter tx = new XmlTextWriter(sw);
                                                    xmlTimbre.WriteTo(tx);

                                                    s = sw.ToString();// 

                                                    if (s != "")
                                                        oDocuments.UserFields.Fields.Item("U_FETimbre").Value = s;
                                                    else
                                                        throw new Exception("No se ha creado timbre Guia Electronica");

                                                    var lRetCode = oDocuments.Update();
                                                    if (lRetCode != 0)
                                                    {
                                                        s = FCmpny.GetLastErrorDescription();
                                                        FSBOApp.StatusBar.SetText("Error actualizar documento con firma FE, " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                        OutLog("Error actualizar documento con firma FE, " + s);
                                                    }
                                                }
                                            }
                                            if (oForm.BusinessObject.Type == "67")
                                            {
                                                oStockTransfer = (SAPbobsCOM.StockTransfer)(FCmpny.GetBusinessObject(BoObjectTypes.oStockTransfer));

                                                if (oStockTransfer.GetByKey(Convert.ToInt32(DocEntry)))
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = @"select T0.DocSubType
                                                                   , SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo'
                                                                   , SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst
                                                                   , T0.FolioNum 
                                                                   , C0.LicTradNum
                                                                   , T0.DocTotal
                                                                   , T0.CANCELED
                                                                from OWTR T0 
                                                                JOIN OCRD C0 ON C0.CardCode = T0.CardCode
                                                                JOIN NNM1 T2 ON T0.Series = T2.Series 
                                                                where T0.DocEntry = {0}";
                                                    else
                                                        s = @"select T0.""DocSubType""
                                                                   , SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo""
                                                                   , SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                                                                   , T0.""FolioNum"" 
                                                                   , C0.""LicTradNum""
                                                                   , T0.""DocTotal""
                                                                   , T0.""CANCELED""
                                                                from ""OWTR"" T0 
                                                                JOIN ""OCRD"" C0 ON C0.""CardCode"" = T0.""CardCode""
                                                                JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                                                               where T0.""DocEntry"" = {0} ";
                                                    s = String.Format(s, DocEntry);
                                                    oRecordSet.DoQuery(s);
                                                    var LicTradNum = ((System.String)oRecordSet.Fields.Item("LicTradNum").Value).Trim();
                                                    var DocTotal = ((System.Double)oRecordSet.Fields.Item("DocTotal").Value);

                                                    if (bMultiSoc == false)
                                                    {
                                                        if (GlobalSettings.RunningUnderSQLServer)
                                                            s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta";
                                                        else
                                                            s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" ";
                                                        s = String.Format(s, TipoDocElect, oStockTransfer.FolioNumber);
                                                    }
                                                    else
                                                    {
                                                        if (GlobalSettings.RunningUnderSQLServer)
                                                            s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta and U_BaseMul = '{2}'";
                                                        else
                                                            s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" and ""U_BaseMul"" = '{2}'";
                                                        s = String.Format(s, TipoDocElect, oStockTransfer.FolioNumber, nMultiSoc);
                                                    }
                                                    oRecordSet.DoQuery(s);
                                                    String CAF = ((System.String)oRecordSet.Fields.Item("U_CAF").Value).Trim();

                                                    //Colocar Timbre
                                                    XmlDocument xmlCAF = new XmlDocument();
                                                    XmlDocument xmlTimbre = new XmlDocument();

                                                    if (CAF == "")
                                                        throw new Exception("No se ha encontrado xml de CAF");
                                                    //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                    xmlCAF.LoadXml(CAF);
                                                    xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElect, Convert.ToString(oStockTransfer.FolioNumber), oStockTransfer.DocDate.ToString("yyyyMMdd"), LicTradNum.Replace(".",""), oDocuments.CardName, Convert.ToString(Math.Round(DocTotal, 0)), oStockTransfer.Lines.ItemDescription, xmlCAF, TaxIdNum);

                                                    var sw = new StringWriter();
                                                    XmlTextWriter tx = new XmlTextWriter(sw);
                                                    xmlTimbre.WriteTo(tx);

                                                    s = sw.ToString();// 

                                                    if (s != "")
                                                        oDocuments.UserFields.Fields.Item("U_FETimbre").Value = s;
                                                    else
                                                        throw new Exception("No se ha creado timbre Guia Electronica");

                                                    var lRetCode = oDocuments.Update();
                                                    if (lRetCode != 0)
                                                    {
                                                        s = FCmpny.GetLastErrorDescription();
                                                        FSBOApp.StatusBar.SetText("Error actualizar documento con firma FE, " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                        OutLog("Error actualizar documento con firma FE, " + s);
                                                    }
                                                }
                                            }
                                        }

                                        var oDeliveryNote = new TDeliveryNote();
                                        oDeliveryNote.SBO_f = FSBOf;
                                        if (oForm.BusinessObject.Type == "15")
                                            oDeliveryNote.EnviarFE_WebService(DocEntry, s, false, false, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52", oForm.BusinessObject.Type, false);
                                        else if (oForm.BusinessObject.Type == "21")
                                            oDeliveryNote.EnviarFE_WebService(DocEntry, s, false, false, true, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52D", oForm.BusinessObject.Type, false);
                                        else if (oForm.BusinessObject.Type == "67")
                                            oDeliveryNote.EnviarFE_WebService(DocEntry, s, true, false, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52T", oForm.BusinessObject.Type, false);
                                    }
                                }
                            }
                        }
                    }
                    else
                    { FSBOApp.StatusBar.SetText("Debe Parametrizar el Addon", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                }
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText("FormDataEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormDataEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }

        }//fin FormDataEvent


        private void IngresarFolio(String sDocEntry, String ObjType)
        {
            Boolean FolioUnico = false;
            Int32 lRetCode;

            try
            {
                if (GlobalSettings.RunningUnderSQLServer)
                { s = @"select isnull(U_FolioGuia,'N') FolioUnico from [@VID_FEPARAM] where code = '1'"; }
                else
                { s = @"select IFNULL(""U_FolioGuia"",'N') ""FolioUnico"" from ""@VID_FEPARAM"" where ""Code"" = '1'"; }

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                {
                    if ((System.String)(oRecordSet.Fields.Item("FolioUnico").Value) == "Y")
                    { FolioUnico = true; }
                }

                if (FolioUnico)
                {
                    var oTransfer = (SAPbobsCOM.StockTransfer)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer));
                    FCmpny.StartTransaction();
                    if (GlobalSettings.RunningUnderSQLServer)
                    { s = "SELECT 'GE' BeginStr, NextNumber FROM NNM1 WHERE (ObjectCode = 'VD_FEEntreg')"; }
                    else
                    { s = @"SELECT 'GE' ""BeginStr"", ""NextNumber"" FROM ""NNM1"" WHERE (""ObjectCode"" = 'VD_FEEntreg') "; }

                    oRecordSet.DoQuery(s);
                    if (ObjType == "67")
                    {
                        if (oTransfer.GetByKey(Convert.ToInt32(sDocEntry)))
                        {
                            oTransfer.FolioNumber = (System.Int32)(oRecordSet.Fields.Item("NextNumber").Value);
                            oTransfer.FolioPrefixString = (System.String)(oRecordSet.Fields.Item("BeginStr").Value);
                            lRetCode = oTransfer.Update();
                            if (lRetCode != 0)
                            {
                                if (GlobalSettings.RunningUnderSQLServer)
                                { s = "UPDATE OWTR SET FolioPref = '{0}', FolioNum = '{1}', Printed = 'Y' WHERE DocEntry = {3}"; }
                                else
                                { s = @"UPDATE ""OWTR"" SET ""FolioPref"" = '{0}', ""FolioNum"" = '{1}', ""Printed"" = 'Y' WHERE ""DocEntry"" = {3} "; }

                                s = String.Format(s, oTransfer.FolioPrefixString, Convert.ToString(oTransfer.FolioNumber), oTransfer.DocEntry);
                                oRecordSet.DoQuery(s);
                            }

                            //actualiza siguiente numero folio para documento
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = "UPDATE NNM1 SET NextFolio = {0} WHERE (Series = {1})"; }
                            else
                            { s = @"UPDATE ""NNM1"" SET ""NextFolio"" = {0} WHERE (""Series"" = {1}) "; }

                            s = String.Format(s, oTransfer.FolioNumber + 1, oTransfer.Series);
                            oRecordSet.DoQuery(s);
                            //actualiza siguiente numero folio para serie del addon entrega electronica
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = "UPDATE NNM1 SET NextNumber = {0} WHERE (ObjectCode = 'VD_FEEntreg')"; }
                            else
                            { s = @"UPDATE ""NNM1"" SET ""NextNumber"" = {0} WHERE (""ObjectCode"" = 'VD_FEEntreg') "; }

                            s = String.Format(s, oTransfer.FolioNumber + 1);
                            oRecordSet.DoQuery(s);
                            //actualiza LPgFolioN
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = "update OWTR set LPgFolioN = FolioNum where DocEntry = {0}"; }
                            else
                            { s = @"update ""OWTR"" set ""LPgFolioN"" = ""FolioNum"" where ""DocEntry"" = {0} "; }

                            s = String.Format(s, oTransfer.DocEntry);
                            oRecordSet.DoQuery(s);
                        }
                    }
                    else if (ObjType == "15")
                    {
                        var oDocumento = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes));
                        if (oDocumento.GetByKey(Convert.ToInt32(sDocEntry)))
                        {
                            oDocumento.FolioNumber = (System.Int32)(oRecordSet.Fields.Item("NextNumber").Value);
                            oDocumento.FolioPrefixString = (System.String)(oRecordSet.Fields.Item("BeginStr").Value);

                            lRetCode = oDocumento.Update();
                            if (lRetCode != 0)
                            {
                                if (GlobalSettings.RunningUnderSQLServer)
                                { s = "UPDATE ODLN SET FolioPref = '{0}', FolioNum = '{1}', Printed = 'Y' WHERE DocEntry = {3}"; }
                                else
                                { s = @"UPDATE ""ODLN"" SET ""FolioPref"" = '{0}', ""FolioNum"" = '{1}', ""Printed"" = 'Y' WHERE ""DocEntry"" = {3} "; }

                                s = String.Format(s, oDocumento.FolioPrefixString, Convert.ToString(oDocumento.FolioNumber), oDocumento.DocEntry);
                                oRecordSet.DoQuery(s);
                            }

                            //actualiza siguiente numero folio para documento
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = "UPDATE NNM1 SET NextFolio = {0} WHERE (Series = {1})"; }
                            else
                            { s = @"UPDATE ""NNM1"" SET ""NextFolio"" = {0} WHERE (""Series"" = {1}) "; }

                            s = String.Format(s, oDocumento.FolioNumber + 1, oDocumento.Series);
                            oRecordSet.DoQuery(s);
                            //actualiza siguiente numero folio para serie del addon entrega electronica
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = "UPDATE NNM1 SET NextNumber = {0} WHERE (ObjectCode = 'VD_FEEntreg')"; }
                            else
                            { s = @"UPDATE ""NNM1"" SET ""NextNumber"" = {0} WHERE (""ObjectCode"" = 'VD_FEEntreg') "; }

                            s = String.Format(s, oDocumento.FolioNumber + 1);
                            oRecordSet.DoQuery(s);
                            //actualiza LPgFolioN
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = "update ODLN set LPgFolioN = FolioNum where DocEntry = {0}"; }
                            else
                            { s = @"update ""ODLN"" set ""LPgFolioN"" = ""FolioNum"" where ""DocEntry"" = {0} "; }

                            s = String.Format(s, oDocumento.DocEntry);
                            oRecordSet.DoQuery(s);
                        }
                    }
                    else if (ObjType == "21")
                    {
                        var oDocumento = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseReturns));
                        if (oDocumento.GetByKey(Convert.ToInt32(sDocEntry)))
                        {
                            oDocumento.FolioNumber = (System.Int32)(oRecordSet.Fields.Item("NextNumber").Value);
                            oDocumento.FolioPrefixString = (System.String)(oRecordSet.Fields.Item("BeginStr").Value);

                            lRetCode = oDocumento.Update();
                            if (lRetCode != 0)
                            {
                                if (GlobalSettings.RunningUnderSQLServer)
                                { s = "UPDATE ORPD SET FolioPref = '{0}', FolioNum = '{1}', Printed = 'Y' WHERE DocEntry = {3}"; }
                                else
                                { s = @"UPDATE ""ORPD"" SET ""FolioPref"" = '{0}', ""FolioNum"" = '{1}', ""Printed"" = 'Y' WHERE ""DocEntry"" = {3} "; }

                                s = String.Format(s, oDocumento.FolioPrefixString, Convert.ToString(oDocumento.FolioNumber), oDocumento.DocEntry);
                                oRecordSet.DoQuery(s);
                            }

                            //actualiza siguiente numero folio para documento
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = "UPDATE NNM1 SET NextFolio = {0} WHERE (Series = {1})"; }
                            else
                            { s = @"UPDATE ""NNM1"" SET ""NextFolio"" = {0} WHERE (""Series"" = {1}) "; }

                            s = String.Format(s, oDocumento.FolioNumber + 1, oDocumento.Series);
                            oRecordSet.DoQuery(s);
                            //actualiza siguiente numero folio para serie del addon entrega electronica
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = "UPDATE NNM1 SET NextNumber = {0} WHERE (ObjectCode = 'VD_FEEntreg')"; }
                            else
                            { s = @"UPDATE ""NNM1"" SET ""NextNumber"" = {0} WHERE (""ObjectCode"" = 'VD_FEEntreg') "; }

                            s = String.Format(s, oDocumento.FolioNumber + 1);
                            oRecordSet.DoQuery(s);
                            //actualiza LPgFolioN
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = "update ORPD set LPgFolioN = FolioNum where DocEntry = {0}"; }
                            else
                            { s = @"update ""ORPD"" set ""LPgFolioN"" = ""FolioNum"" where ""DocEntry"" = {0} "; }

                            s = String.Format(s, oDocumento.DocEntry);
                            oRecordSet.DoQuery(s);
                        }
                    }

                    FCmpny.EndTransaction(BoWfTransOpt.wf_Commit);
                }
            }
            catch (Exception e)
            {
                OutLog("IngresarFolio " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);  // Captura errores no manejados
                if (FCmpny.InTransaction) FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
            }
        }//fin IngresarFolio


    }//fin Class
}
