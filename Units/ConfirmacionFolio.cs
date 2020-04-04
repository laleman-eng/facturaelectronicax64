using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.SqlClient;
using System.Reflection;
using System.Globalization;
using System.IO;
using SAPbouiCOM;
using SAPbobsCOM;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using Factura_Electronica_VK.Functions;
using Factura_Electronica_VK.CreditNotes;
using Factura_Electronica_VK.DeliveryNote;
using Factura_Electronica_VK.Invoice;
using Factura_Electronica_VK.PurchaseInvoice;
using FactRemota;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Data;
using System.Drawing.Printing;

namespace Factura_Electronica_VK.ConfirmacionFolio
{
    public class TConfirmacionFolio : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private String s;
        private Boolean bMultiSoc;
        private Boolean AbrirDoc;
        private TFunctions Param;
        private String ImpFiscal = "N";
        private String CantImp = "0";
        private String Impresora = "";
        //por Peru
        private String Localidad;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                Param = new TFunctions();
                Param.SBO_f = FSBOf;
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                //FSBOf.LoadForm(xmlPath, 'VID_Entrega.srf', Uid);
                oForm = FSBOApp.Forms.Item(uid);

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select ISNULL(U_MultiSoc,'N') MultiSoc, ISNULL(U_AbrirDoc,'N') 'AbrirDoc'
                                ,ISNULL(U_ImpFiscal,'N') 'ImpFiscal', ISNULL(U_CantImp,'0') 'CantImp', ISNULL(U_httpBol,'') 'URLCL' 
                                ,ISNULL(U_NomImp,'') 'Impresora'
                            from [@VID_FEPARAM] where Code = '1'";
                else
                    s = @"select IFNULL(""U_MultiSoc"",'N') ""MultiSoc"", IFNULL(""U_AbrirDoc"",'N') ""AbrirDoc""
                                ,IFNULL(""U_ImpFiscal"",'N') ""ImpFiscal"", IFNULL(""U_CantImp"",'0') ""CantImp"", IFNULL(""U_httpBol"",'') ""URLCL"" 
                                ,IFNULL(""U_NomImp"",'') ""Impresora""
                            from ""@VID_FEPARAM"" where ""Code"" = '1' ";

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                    throw new Exception("Debe parametrizar el Addon Factura Electronica");
                else
                {
                    Localidad = "CL"; // ((System.String)oRecordSet.Fields.Item("Localidad").Value).Trim();
                    if (((System.String)oRecordSet.Fields.Item("MultiSoc").Value).Trim() == "Y")
                        bMultiSoc = true;
                    else
                        bMultiSoc = false;

                    if (((System.String)oRecordSet.Fields.Item("AbrirDoc").Value).Trim() == "Y")
                    {
                        AbrirDoc = true;
                        CantImp = ((System.String)oRecordSet.Fields.Item("CantImp").Value).Trim();
                        ImpFiscal = ((System.String)oRecordSet.Fields.Item("ImpFiscal").Value).Trim();
                        Impresora = ((System.String)oRecordSet.Fields.Item("Impresora").Value).Trim();
                    }
                    else
                        AbrirDoc = false;

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

                            /*if (GlobalSettings.RunningUnderSQLServer)
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
                            }*/
                        }
                    }
                }

                //Flag := false;
                oForm.Freeze(true);
            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            finally
            {
                oForm.Visible = true;
                oForm.Freeze(false);
            }
            return Result;
        }//fin InitForm

        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            Int32 nErr;
            String sErr;
            SAPbouiCOM.Form oFormAux;
            String DocEntry;
            String Tabla;
            XmlDocument _xmlDocument;
            XmlNode N;
            Boolean FolioUnico;
            Boolean bMultiSoc;
            String nMultiSoc = "";
            String[] FE52 = { "15", "67", "21" };
            String Canceled = "";

            //inherited FormEvent(FormUID,Var pVal,Var BubbleEvent);
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if (pVal.ItemUID == "4")
                    {
                        if (Localidad == "CL")
                        {
                            s = GlobalSettings.PrevFormUID;
                            oFormAux = FSBOApp.Forms.Item(s);
                            //if (oFormAux.BusinessObject.Type in ("15","67","21"))
                            if (FE52.Contains(oFormAux.BusinessObject.Type))
                            {
                                if (GlobalSettings.RunningUnderSQLServer)
                                { s = @"select isnull(U_FolioGuia,'N') FolioUnico, ISNULL(U_Distrib,'N') 'Distribuido', ISNULL(U_MultiSoc,'N') MultiSoc from [@VID_FEPARAM] where Code = '1'"; }
                                else
                                { s = @"select IFNULL(""U_FolioGuia"",'N') ""FolioUnico"", IFNULL(""U_Distrib"",'N') ""Distribuido"", IFNULL(""U_MultiSoc"",'N') ""MultiSoc"" FROM ""@VID_FEPARAM"" where ""Code"" = '1' "; }
                                oRecordSet.DoQuery(s);

                                if ((System.String)(oRecordSet.Fields.Item("Distribuido").Value) == "N")
                                {
                                    if ((System.String)(oRecordSet.Fields.Item("MultiSoc").Value) == "Y")
                                    { bMultiSoc = true; }
                                    else
                                    { bMultiSoc = false; }

                                    if (((System.String)(oRecordSet.Fields.Item("FolioUnico").Value) == "Y"))
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
                                            s = @"SELECT Count(*) Cont, SUBSTRING(ISNULL(T0.BeginStr,''), 2, LEN(T0.BeginStr)) Inst, T0.CANCELED
                                                FROM NNM1 T0
                                                JOIN {0} T1 ON T1.Series = T0.Series
                                               WHERE (SUBSTRING(UPPER(T0.BeginStr), 1, 1) = 'E') 
                                                 AND T1.DocEntry = {1}
                                                 --AND T0.ObjectCode = '{2}'
                                               GROUP BY SUBSTRING(ISNULL(T0.BeginStr,''), 2, LEN(T0.BeginStr))";
                                        }
                                        else
                                        {
                                            s = @"SELECT Count(*) ""Cont"", SUBSTRING(IFNULL(T0.""BeginStr"",''), 2, LENGTH(T0.""BeginStr"")) ""Inst"", T0.""CANCELED""
                                                FROM ""NNM1"" T0 
                                                JOIN {0} T1 ON T1.""Series"" = T0.""Series"" 
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
                                        }
                                        else
                                        {
                                            BubbleEvent = false;
                                            Canceled = (System.String)(oRecordSet.Fields.Item("CANCELED").Value);
                                            if (Canceled == "N")
                                            {
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
                                            }
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
                        if (Localidad == "CL")
                        {
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = @"select isnull(U_FolioGuia,'N') FolioUnico from [@VID_FEPARAM] where code = '1'"; }
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
                                         --AND T0.ObjectCode = '{2}'";
                                }
                                else
                                {
                                    s = @"SELECT Count(*) ""Cont"" 
                                        FROM ""NNM1"" T0 
                                        JOIN {0} T1 ON T1.""Series"" = T0.""Series""
                                       WHERE (SUBSTRING(UPPER(T0.""BeginStr""), 1, 1) = 'E')  
                                         AND T1.""DocEntry"" = {1} 
                                         --AND T0.""ObjectCode"" = '{2}' ";
                                }
                                s = String.Format(s, Tabla, DocEntry, oFormAux.BusinessObject.Type);
                                oRecordSet.DoQuery(s);
                                if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                                {
                                    if (GlobalSettings.RunningUnderSQLServer)
                                    { s = @"SELECT 'GE' BeginStr, NextNumber FROM NNM1 WHERE (ObjectCode = 'VD_FEEntreg')"; }
                                    else
                                    { s = @"SELECT 'GE' ""BeginStr"", ""NextNumber"" FROM ""NNM1"" WHERE (""ObjectCode"" = 'VD_FEEntreg') "; }

                                    oRecordSet.DoQuery(s);
                                    s = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("NextNumber").Value));
                                    var oEditText = (EditText)(oForm.Items.Item("7").Specific);
                                    oEditText.Value = s;
                                    //s := System.String(oRecordSet.Fields.Item("BeginStr").Value);
                                    //EditText(oForm.Items.Item("3").Specific).Value := s;
                                }
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
            Int32 nErr;
            String sErr;
            XmlDocument _xmlDocument;
            XmlNode N;
            Boolean bMultiSoc;
            String nMultiSoc;
            String[] FE52 = { "15", "67", "21" };
            String GeneraT = "";
            String TaxIdNum = "";
            Int32 FolioNum;
            String DocSubType;
            String TipoDocElec;
            String LicTradNum;
            Double DocTotal;
            String Canceled = "";
            String sNombreArchivo = "";
            SAPbobsCOM.Documents oDocumento;
            SAPbobsCOM.Documents oDocuments;
            String TTipoDoc = "";
            //inherited FormDataEvent(var BusinessObjectInfo,var BubbleEvent);
            base.FormDataEvent(ref BusinessObjectInfo, ref BubbleEvent);

            try
            {
                if ((BusinessObjectInfo.BeforeAction == false) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_UPDATE) && (BusinessObjectInfo.ActionSuccess))
                {

                    if (Localidad == "CL")
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select ISNULL(T0.U_Distrib,'N') 'Distribuido', ISNULL(T0.U_MultiSoc,'N') MultiSoc, ISNULL(T0.U_GenerarT,'N') GeneraT, REPLACE(ISNULL(A0.TaxIdNum,''),'.','') TaxIdNum from [@VID_FEPARAM] T0, OADM A0";
                        else
                            s = @"select IFNULL(T0.""U_Distrib"",'N') ""Distribuido"", IFNULL(T0.""U_MultiSoc"",'N') ""MultiSoc"", IFNULL(T0.""U_GenerarT"",'N') ""GeneraT"", REPLACE(IFNULL(A0.""TaxIdNum"",''),'.','') ""TaxIdNum"" from ""@VID_FEPARAM"" T0, ""OADM"" A0 ";
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

                                if ((oForm.BusinessObject.Type == "13") || (oForm.BusinessObject.Type == "203"))//And (Flag = true)) then
                                {
                                    //Flag := false;
                                    DocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);

                                    if (GlobalSettings.RunningUnderSQLServer)
                                    {
                                        s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo'
                                                , SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst
                                                , SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) TipoDocElect
                                                , T0.FolioNum, T0.CANCELED
                                            from {1} T0 JOIN NNM1 T2 ON T0.Series = T2.Series 
                                           where T0.DocEntry = {0}";
                                    }
                                    else
                                    {
                                        s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo""
                                                , SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                                                , SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""TipoDocElect""
                                                , T0.""FolioNum"", T0.""CANCELED""
                                            from ""{1}"" T0 JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                                           where T0.""DocEntry"" = {0} ";
                                    }

                                    if (oForm.BusinessObject.Type == "203")
                                    {
                                        s = String.Format(s, DocEntry, "ODPI");
                                        TTipoDoc = "33A";
                                    }
                                    else
                                    {
                                        s = String.Format(s, DocEntry, "OINV");
                                        TTipoDoc = "33";
                                    }

                                    oRecordSet.DoQuery(s);
                                    DocSubType = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                    FolioNum = (System.Int32)(oRecordSet.Fields.Item("FolioNum").Value);
                                    Canceled = (System.String)(oRecordSet.Fields.Item("CANCELED").Value);
                                    TipoDocElec = "";
                                    if (((System.String)(oRecordSet.Fields.Item("Tipo").Value) == "E") && (Canceled == "N"))
                                    {
                                        if (DocSubType == "--") //Factura
                                            TipoDocElec = "33";
                                        else if (DocSubType == "IE") //Factura Exenta
                                        {
                                            TTipoDoc = "34";
                                            TipoDocElec = "34";
                                        }
                                        else if ((DocSubType == "DN") && ((System.String)(oRecordSet.Fields.Item("TipoDocElect").Value) == "111")) //Nota Debito Exportacion
                                        {
                                            TTipoDoc = "111";
                                            TipoDocElec = "111";
                                        }
                                        else if ((DocSubType == "DN") && ((System.String)(oRecordSet.Fields.Item("TipoDocElect").Value) != "111")) //Nota Debito
                                        {
                                            TTipoDoc = "56";
                                            TipoDocElec = "56";
                                        }
                                        else if (DocSubType == "IB") //Boleta
                                        {
                                            TTipoDoc = "39";
                                            TipoDocElec = "39";
                                        }
                                        else if (DocSubType == "EB") //Boleta Exenta
                                        {
                                            TTipoDoc = "41";
                                            TipoDocElec = "41";
                                        }
                                        else if (DocSubType == "IX") //Factura Exportacion
                                        {
                                            TTipoDoc = "110";
                                            TipoDocElec = "110";
                                        }

                                        nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                                        if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        { FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                                        else
                                        {
                                            if (GeneraT == "Y")
                                            {
                                                if (bMultiSoc == false)
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta";
                                                    else
                                                        s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" ";
                                                    s = String.Format(s, TipoDocElec, FolioNum);
                                                }
                                                else
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta and U_BaseMul = '{2}'";
                                                    else
                                                        s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" and ""U_BaseMul"" = '{2}'";
                                                    s = String.Format(s, TipoDocElec, FolioNum, nMultiSoc);
                                                }
                                                oRecordSet.DoQuery(s);
                                                String CAF = ((System.String)oRecordSet.Fields.Item("U_CAF").Value).Trim();
                                                if (oForm.BusinessObject.Type == "203")
                                                    oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oDownPayments));
                                                else
                                                    oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oInvoices));

                                                if (oDocuments.GetByKey(Convert.ToInt32(DocEntry)))
                                                {
                                                    //Colocar Timbre
                                                    XmlDocument xmlCAF = new XmlDocument();
                                                    XmlDocument xmlTimbre = new XmlDocument();

                                                    if (CAF == "")
                                                        throw new Exception("No se ha encontrado xml de CAF");
                                                    //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                    xmlCAF.LoadXml(CAF);
                                                    xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElec, Convert.ToString(oDocuments.FolioNumber), oDocuments.DocDate.ToString("yyyyMMdd"), oDocuments.FederalTaxID.Replace(".",""), oDocuments.CardName, Convert.ToString(Math.Round(oDocuments.DocTotal, 0)), oDocuments.Lines.ItemDescription, xmlCAF, TaxIdNum);

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
                                            oDocumento = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oInvoices));
                                            if (oDocumento.GetByKey(Convert.ToInt32(DocEntry)))//**se dejo la normal mientras se termina la modificacion en el portal 20170202
                                                oInvoice.EnviarFE_WebService(oForm.BusinessObject.Type, oDocumento, TipoDocElec, bMultiSoc, nMultiSoc, GlobalSettings.RunningUnderSQLServer, DocSubType, TTipoDoc, false);
                                            //oInvoice.EnviarFE(DocEntry, DocSubType, oForm.BusinessObject.Type, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, TipoDocElec);
                                            else
                                                FSBOApp.StatusBar.SetText("No se ha encontrado documento " + TipoDocElec + " folio " + Convert.ToString(oDocumento.FolioNumber), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);

                                            if ((AbrirDoc) && ((TipoDocElec == "39") || (TipoDocElec == "41") || (TipoDocElec == "61")))
                                            {
                                                AbrirPDF(DocEntry, oForm.BusinessObject.Type, FolioNum.ToString(), TipoDocElec, nMultiSoc.ToString().Trim());
                                            }
                                        }
                                    }
                                }
                                if ((oForm.BusinessObject.Type == "18") || (oForm.BusinessObject.Type == "204"))//And (Flag = true)) then
                                {
                                    //Flag := false;
                                    DocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);

                                    if (GlobalSettings.RunningUnderSQLServer)
                                    {
                                        s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo'
                                                , SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst 
                                                , T0.FolioNum, T0.CANCELED
                                            from {1} T0 JOIN NNM1 T2 ON T0.Series = T2.Series 
                                           where T0.DocEntry = {0}";
                                    }
                                    else
                                    {
                                        s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo""
                                                , SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                                                , T0.""FolioNum"", T0.""CANCELED""
                                            from ""{1}"" T0 JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                                           where T0.""DocEntry"" = {0} ";
                                    }

                                    if (oForm.BusinessObject.Type == "204")
                                        s = String.Format(s, DocEntry, "ODPO");
                                    else
                                        s = String.Format(s, DocEntry, "OPCH");

                                    oRecordSet.DoQuery(s);
                                    DocSubType = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                    FolioNum = (System.Int32)(oRecordSet.Fields.Item("FolioNum").Value);
                                    Canceled = (System.String)(oRecordSet.Fields.Item("CANCELED").Value);
                                    TipoDocElec = "";
                                    if (((System.String)(oRecordSet.Fields.Item("Tipo").Value) == "E") && (Canceled == "N"))
                                    {
                                        if (DocSubType == "--") //Factura
                                            TipoDocElec = "46";

                                        nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                                        if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        { FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                                        else
                                        {
                                            if (GeneraT == "Y")
                                            {
                                                if (bMultiSoc == false)
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta";
                                                    else
                                                        s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" ";
                                                    s = String.Format(s, TipoDocElec, FolioNum);
                                                }
                                                else
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta and U_BaseMul = '{2}'";
                                                    else
                                                        s = @"select ""U_CAF"" from ""@VID_FECAF"" where ""U_TipoDoc"" = '{0}' and {1} between ""U_Desde"" and ""U_Hasta"" and ""U_BaseMul"" = '{2}'";
                                                    s = String.Format(s, TipoDocElec, FolioNum, nMultiSoc);
                                                }
                                                oRecordSet.DoQuery(s);
                                                String CAF = ((System.String)oRecordSet.Fields.Item("U_CAF").Value).Trim();
                                                oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oInvoices));
                                                if (oDocuments.GetByKey(Convert.ToInt32(DocEntry)))
                                                {
                                                    //Colocar Timbre
                                                    XmlDocument xmlCAF = new XmlDocument();
                                                    XmlDocument xmlTimbre = new XmlDocument();

                                                    if (CAF == "")
                                                        throw new Exception("No se ha encontrado xml de CAF");
                                                    //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                    xmlCAF.LoadXml(CAF);
                                                    xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElec, Convert.ToString(oDocuments.FolioNumber), oDocuments.DocDate.ToString("yyyyMMdd"), oDocuments.FederalTaxID.Replace(".",""), oDocuments.CardName, Convert.ToString(Math.Round(oDocuments.DocTotal, 0)), oDocuments.Lines.ItemDescription, xmlCAF, TaxIdNum);

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
                                            oPurchaseInvoice.EnviarFE_WebService(DocEntry, DocSubType, oForm.BusinessObject.Type, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "46", (oForm.BusinessObject.Type == "18" ? "46": "46A"), false);
                                        }
                                    }
                                }
                                else if ((oForm.BusinessObject.Type == "14") || (oForm.BusinessObject.Type == "19")) //And (Flag = true)) then
                                {
                                    //Flag := false;
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

                                    DocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                                    if (GlobalSettings.RunningUnderSQLServer)
                                    {
                                        s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo'
                                                 ,SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'Inst' 
                                                 ,SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'TipoDocElect' 
                                                 , T0.FolioNum, T0.CANCELED
                                            from {1} T0 JOIN NNM1 T2 ON T0.Series = T2.Series 
                                           where T0.DocEntry = {0}";
                                    }
                                    else
                                    {
                                        s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo""
                                             ,SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                                             ,SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""TipoDocElect""
                                             , T0.""FolioNum"", T0.""CANCELED""
                                        from ""{1}"" T0 JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                                       where T0.""DocEntry"" = {0} ";
                                    }

                                    s = String.Format(s, DocEntry, table);
                                    oRecordSet.DoQuery(s);
                                    DocSubType = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                    FolioNum = (System.Int32)(oRecordSet.Fields.Item("FolioNum").Value);
                                    Canceled = (System.String)(oRecordSet.Fields.Item("CANCELED").Value);

                                    if ((System.String)(oRecordSet.Fields.Item("TipoDocElect").Value) == "112")
                                    {
                                        TipoDocElec = "112";
                                        TipoDocElecAddon = "112";
                                    }
                                    else
                                        TipoDocElec = "61";

                                    if (((System.String)(oRecordSet.Fields.Item("Tipo").Value) == "E") && (Canceled == "N"))
                                    {
                                        nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                                        if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        { FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                                        else
                                        {
                                            if (GeneraT == "Y")
                                            {
                                                s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta";
                                                s = String.Format(s, TipoDocElec, FolioNum);
                                                oRecordSet.DoQuery(s);
                                                String CAF = ((System.String)oRecordSet.Fields.Item("U_CAF").Value).Trim();
                                                oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oCreditNotes));
                                                if (oDocuments.GetByKey(Convert.ToInt32(DocEntry)))
                                                {
                                                    //Colocar Timbre
                                                    XmlDocument xmlCAF = new XmlDocument();
                                                    XmlDocument xmlTimbre = new XmlDocument();

                                                    if (CAF == "")
                                                        throw new Exception("No se ha encontrado xml de CAF");
                                                    //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                    xmlCAF.LoadXml(CAF);
                                                    xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElec, Convert.ToString(oDocuments.FolioNumber), oDocuments.DocDate.ToString("yyyyMMdd"), oDocuments.FederalTaxID.Replace(".",""), oDocuments.CardName, Convert.ToString(Math.Round(oDocuments.DocTotal, 0)), oDocuments.Lines.ItemDescription, xmlCAF, TaxIdNum);

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
                                            oCreditNotes.EnviarFE_WebServiceNotaCredito(DocEntry, DocSubType, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, oForm.BusinessObject.Type, TipoDocElec, TipoDocElecAddon, false);

                                            if (AbrirDoc)
                                                AbrirPDF(DocEntry, oForm.BusinessObject.Type, FolioNum.ToString(), TipoDocElec, nMultiSoc.ToString().Trim());
                                        }
                                    }
                                }
                                //else if (oForm.BusinessObject.Type in ['15','67','21']) //And (Flag = true)) then
                                else if (FE52.Contains(oForm.BusinessObject.Type))
                                {
                                    //Flag := false;
                                    if (oForm.BusinessObject.Type == "15")
                                        DocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                                    else if (oForm.BusinessObject.Type == "21")
                                        DocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                                    else
                                    {
                                        _xmlDocument = new XmlDocument();
                                        _xmlDocument.LoadXml(BusinessObjectInfo.ObjectKey);
                                        N = _xmlDocument.SelectSingleNode("StockTransferParams");
                                        DocEntry = (System.String)(N.InnerText).Trim();
                                    }

                                    if (GlobalSettings.RunningUnderSQLServer)
                                    {
                                        if (oForm.BusinessObject.Type == "15")
                                            s = @"select T0.DocSubType
                                                       , SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo'
                                                       , SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst
                                                       , T0.FolioNum
                                                       , C0.LicTradNum
                                                       , T0.DocTotal
                                                       , T0.CANCELED
                                                    from ODLN T0 
                                                    JOIN OCRD C0 ON C0.CardCode = T0.CardCode
                                                    JOIN NNM1 T2 ON T0.Series = T2.Series 
                                                   where T0.DocEntry = {0}";
                                        else if (oForm.BusinessObject.Type == "21")
                                            s = @"select T0.DocSubType
                                                       , SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo'
                                                       , SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst
                                                       , T0.FolioNum 
                                                       , C0.LicTradNum
                                                       , T0.DocTotal
                                                       , T0.CANCELED
                                                    from ORPD T0 
                                                    JOIN OCRD C0 ON C0.CardCode = T0.CardCode
                                                    JOIN NNM1 T2 ON T0.Series = T2.Series 
                                                   where T0.DocEntry = {0}";
                                        else if (oForm.BusinessObject.Type == "67")
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
                                    }
                                    else
                                    {
                                        if (oForm.BusinessObject.Type == "15")
                                            s = @"select T0.""DocSubType""
                                                       , SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo""
                                                       , SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                                                       , T0.""FolioNum""
                                                       , C0.""LicTradNum""
                                                       , T0.""DocTotal"" 
                                                       , T0.""CANCELED""
                                                    from ""ODLN"" T0 
                                                    JOIN ""OCRD"" C0 ON C0.""CardCode"" = T0.""CardCode""
                                                    JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                                                   where T0.""DocEntry"" = {0} ";
                                        else if (oForm.BusinessObject.Type == "21")
                                            s = @"select T0.""DocSubType""
                                                       , SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo""
                                                       , SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                                                       , T0.""FolioNum"" 
                                                       , C0.""LicTradNum""
                                                       , T0.""DocTotal""
                                                       , T0.""CANCELED""
                                                    from ""ORPD"" T0 
                                                    JOIN ""OCRD"" C0 ON C0.""CardCode"" = T0.""CardCode""
                                                    JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                                                   where T0.""DocEntry"" = {0} ";
                                        else if (oForm.BusinessObject.Type == "67")
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
                                    }

                                    s = String.Format(s, DocEntry);
                                    oRecordSet.DoQuery(s);
                                    DocSubType = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                    FolioNum = (System.Int32)(oRecordSet.Fields.Item("FolioNum").Value);
                                    TipoDocElec = "52";
                                    LicTradNum = ((System.String)oRecordSet.Fields.Item("LicTradNum").Value).Trim();
                                    DocTotal = ((System.Double)oRecordSet.Fields.Item("DocTotal").Value);
                                    Canceled = (System.String)(oRecordSet.Fields.Item("CANCELED").Value);

                                    if (((System.String)(oRecordSet.Fields.Item("Tipo").Value) == "E") && (Canceled == "N"))
                                    {
                                        nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                                        if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        { FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                                        else
                                        {
                                            if (GeneraT == "Y")
                                            {
                                                SAPbobsCOM.StockTransfer oStockTransfer;
                                                s = "select U_CAF from [@VID_FECAF] where U_TipoDoc = '{0}' and {1} between U_Desde and U_Hasta";
                                                s = String.Format(s, TipoDocElec, FolioNum);
                                                oRecordSet.DoQuery(s);
                                                String CAF = ((System.String)oRecordSet.Fields.Item("U_CAF").Value).Trim();

                                                if (oForm.BusinessObject.Type == "67")
                                                {
                                                    oStockTransfer = (SAPbobsCOM.StockTransfer)(FCmpny.GetBusinessObject(BoObjectTypes.oStockTransfer));
                                                    if (oStockTransfer.GetByKey(Convert.ToInt32(DocEntry)))
                                                    {

                                                        //Colocar Timbre
                                                        XmlDocument xmlCAF = new XmlDocument();
                                                        XmlDocument xmlTimbre = new XmlDocument();

                                                        if (CAF == "")
                                                            throw new Exception("No se ha encontrado xml de CAF");
                                                        //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                        xmlCAF.LoadXml(CAF);
                                                        xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElec, Convert.ToString(oStockTransfer.FolioNumber), oStockTransfer.DocDate.ToString("yyyyMMdd"), LicTradNum.Replace(".",""), oStockTransfer.CardName, Convert.ToString(Math.Round(DocTotal, 0)), oStockTransfer.Lines.ItemDescription, xmlCAF, TaxIdNum);

                                                        var sw = new StringWriter();
                                                        XmlTextWriter tx = new XmlTextWriter(sw);
                                                        xmlTimbre.WriteTo(tx);

                                                        s = sw.ToString();// 

                                                        if (s != "")
                                                            oStockTransfer.UserFields.Fields.Item("U_FETimbre").Value = s;
                                                        else
                                                            throw new Exception("No se ha creado timbre Guia Electronica");

                                                        var lRetCode = oStockTransfer.Update();
                                                        if (lRetCode != 0)
                                                        {
                                                            s = FCmpny.GetLastErrorDescription();
                                                            FSBOApp.StatusBar.SetText("Error actualizar documento con firma FE, " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                            OutLog("Error actualizar documento con firma FE, " + s);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (oForm.BusinessObject.Type == "21")
                                                        oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oPurchaseReturns));
                                                    else
                                                        oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oDeliveryNotes));

                                                    if (oDocuments.GetByKey(Convert.ToInt32(DocEntry)))
                                                    {
                                                        //Colocar Timbre
                                                        XmlDocument xmlCAF = new XmlDocument();
                                                        XmlDocument xmlTimbre = new XmlDocument();

                                                        if (CAF == "")
                                                            throw new Exception("No se ha encontrado xml de CAF");
                                                        //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                        xmlCAF.LoadXml(CAF);
                                                        xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElec, Convert.ToString(oDocuments.FolioNumber), oDocuments.DocDate.ToString("yyyyMMdd"), oDocuments.FederalTaxID.Replace(".",""), oDocuments.CardName, Convert.ToString(Math.Round(oDocuments.DocTotal, 0)), oDocuments.Lines.ItemDescription, xmlCAF, TaxIdNum);

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
                                                oDeliveryNote.EnviarFE_WebService(DocEntry, DocSubType, false, false, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52", oForm.BusinessObject.Type, false);
                                            else if (oForm.BusinessObject.Type == "21")
                                                oDeliveryNote.EnviarFE_WebService(DocEntry, DocSubType, false, false, true, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52D", oForm.BusinessObject.Type, false);
                                            else if (oForm.BusinessObject.Type == "67")
                                                oDeliveryNote.EnviarFE_WebService(DocEntry, DocSubType, true, false, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52T", oForm.BusinessObject.Type, false);
                                        }
                                    }
                                }
                            }
                        }
                        else
                            FSBOApp.StatusBar.SetText("Debe Parametrizar el Addon", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    }
                }
            }
            catch (Exception e)
            {
                FCmpny.GetLastError(out nErr, out sErr);
                FSBOApp.StatusBar.SetText("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormDataEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
            finally
            {
                oDocumento = null;
                oDocuments = null;
                FSBOf._ReleaseCOMObject(oDocuments);
                FSBOf._ReleaseCOMObject(oDocumento);
            }

        }//fin FormDataEvent


        private void IngresarFolio(String sDocEntry, String ObjType)
        {
            Boolean FolioUnico = false;
            Int32 lRetCode;

            try
            {
                if (GlobalSettings.RunningUnderSQLServer)
                { s = "select ISNULL(U_FolioGuia,'N') FolioUnico from [@VID_FEPARAM] where Code = '1'"; }
                else
                { s = @"select IFNULL(""U_FolioGuia"",'N') ""FolioUnico"" from ""@VID_FEPARAM"" where ""Code"" = '1' "; }

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                {
                    if ((System.String)(oRecordSet.Fields.Item("FolioUnico").Value) == "Y")
                        FolioUnico = true;
                }

                if (FolioUnico)
                {
                    var oTransfer = (SAPbobsCOM.StockTransfer)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer));
                    FCmpny.StartTransaction();

                    if (GlobalSettings.RunningUnderSQLServer)
                    { s = @"SELECT 'GE' BeginStr, NextNumber FROM NNM1 WHERE (ObjectCode = 'VD_FEEntreg')"; }
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
                                { s = @"UPDATE OWTR SET FolioPref = '{0}', FolioNum = '{1}', Printed = 'Y' WHERE DocEntry = {3}"; }
                                else
                                { s = @"UPDATE ""OWTR"" SET ""FolioPref"" = '{0]', ""FolioNum"" = '{1}', ""Printed"" = 'Y' WHERE ""DocEntry"" = {3} "; }

                                s = String.Format(s, oTransfer.FolioPrefixString, Convert.ToString(oTransfer.FolioNumber), oTransfer.DocEntry);
                                oRecordSet.DoQuery(s);
                            }

                            //actualiza siguiente numero folio para documento
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = @"UPDATE NNM1 SET NextFolio = {0} WHERE (Series = {1})"; }
                            else
                            { s = @"UPDATE ""NNM1"" SET ""NextFolio"" = {0} WHERE (""Series"" = {1}) "; }

                            s = String.Format(s, oTransfer.FolioNumber + 1, oTransfer.Series);
                            oRecordSet.DoQuery(s);

                            //actualiza siguiente numero folio para serie del addon entrega electronica
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = @"UPDATE NNM1 SET NextNumber = {0} WHERE (ObjectCode = 'VD_FEEntreg')"; }
                            else
                            { s = @"UPDATE ""NNM1"" SET ""NextNumber"" = {0} WHERE (""ObjectCode"" = 'VD_FEEntreg') "; }

                            s = String.Format(s, oTransfer.FolioNumber + 1);
                            oRecordSet.DoQuery(s);

                            //actualiza LPgFolioN
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = @"update OWTR set LPgFolioN = FolioNum where DocEntry = {0}"; }
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
                                { s = @"UPDATE ODLN SET FolioPref = '{0}', FolioNum = '{1}', Printed = 'Y' WHERE DocEntry = {3}"; }
                                else
                                { s = @"UPDATE ""ODLN"" SET ""FolioPref"" = '{0}', ""FolioNum"" = '{1}', ""Printed"" = 'Y' WHERE ""DocEntry"" = {3} "; }

                                s = String.Format(s, oDocumento.FolioPrefixString, Convert.ToString(oDocumento.FolioNumber), oDocumento.DocEntry);
                                oRecordSet.DoQuery(s);
                            }

                            //actualiza siguiente numero folio para documento
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = @"UPDATE NNM1 SET NextFolio = {0} WHERE (Series = {1})"; }
                            else
                            { s = @"UPDATE ""NNM1"" SET ""NextFolio"" = {0} WHERE (""Series"" = {1}) "; }

                            s = String.Format(s, oDocumento.FolioNumber + 1, oDocumento.Series);
                            oRecordSet.DoQuery(s);
                            //actualiza siguiente numero folio para serie del addon entrega electronica
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = @"UPDATE NNM1 SET NextNumber = {0} WHERE (ObjectCode = 'VD_FEEntreg')"; }
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
                                { s = @"UPDATE ORPD SET FolioPref = '{0}', FolioNum = '{1}', Printed = 'Y' WHERE DocEntry = {3}"; }
                                else
                                { s = @"UPDATE ""ORPD"" SET ""FolioPref"" = '{0}', ""FolioNum"" = '{1}', ""Printed"" = 'Y' WHERE ""DocEntry"" = {3}"; }

                                s = String.Format(s, oDocumento.FolioPrefixString, Convert.ToString(oDocumento.FolioNumber), oDocumento.DocEntry);
                                oRecordSet.DoQuery(s);
                            }

                            //actualiza siguiente numero folio para documento
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = @"UPDATE NNM1 SET NextFolio = {0} WHERE (Series = {1})"; }
                            else
                            { s = @"UPDATE ""NNM1"" SET ""NextFolio"" = {0} WHERE (""Series"" = {1}) "; }

                            s = String.Format(s, oDocumento.FolioNumber + 1, oDocumento.Series);
                            oRecordSet.DoQuery(s);

                            //actualiza siguiente numero folio para serie del addon entrega electronica
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = @"UPDATE NNM1 SET NextNumber = {0} WHERE (ObjectCode = 'VD_FEEntreg')"; }
                            else
                            { s = @"UPDATE ""NNM1"" SET ""NextNumber"" = {0} WHERE (""ObjectCode"" = 'VD_FEEntreg') "; }
                            s = String.Format(s, oDocumento.FolioNumber + 1);
                            oRecordSet.DoQuery(s);

                            //actualiza LPgFolioN
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = @"update ORPD set LPgFolioN = FolioNum where DocEntry = {0}"; }
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
                if (FCmpny.InTransaction)
                { FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack); }
            }

        }//fin IngresarFolio

        private Boolean AbrirPDF(String DocEntry, String ObjType, String FolioNum, String TipoDocElect, String nMultiSoc)
        {
            TableLogOnInfo logOnInfo;
            //CrystalDecisions.CrystalReports.Engine.Table tabla;
            ReportDocument rpt = new ReportDocument();
            ConnectionInfo connection = new ConnectionInfo();
            String oPath;
            String sNombreArchivo;
            String Pass = "";
            String Usuario = "";
            Boolean Seguir = false;

            oPath = System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0));

            sNombreArchivo = oPath + "\\Reports\\" + Localidad + "\\" + TipoDocElect + "_" + FCmpny.CompanyDB + ".rpt";
            if (!File.Exists(sNombreArchivo))
            {
                Seguir = Param.RescatarRPT(TipoDocElect, sNombreArchivo);
            }
            else
                Seguir = true;


            if (!Seguir)
            {
                FSBOApp.StatusBar.SetSystemMessage("No se ha encontrado layout, " + sNombreArchivo, SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("No se ha encontrado layout, " + sNombreArchivo);
                return false;
            }
            else
            {
                FSBOf.AddRepKey(DocEntry + "-" + FCmpny.UserSignature, "FEManual", TipoDocElect);//oForm.TypeEx);

                try
                {

                    rpt.Load(sNombreArchivo);
                    rpt.Refresh();
                    connection.ServerName = FCmpny.Server.ToString().Trim();
                    connection.DatabaseName = FCmpny.CompanyDB.ToString().Trim();
                    try
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = "SELECT T0.U_Srvr 'Server', T0.U_Usr 'Usuario', T0.U_Pw 'Password' FROM [dbo].[@VID_MENUSU] T0";
                        else
                            s = @"SELECT T0.""U_Srvr"" ""Server"", T0.""U_Usr"" ""Usuario"", T0.""U_Pw"" ""Password"" FROM ""@VID_MENUSU"" T0";
                        oRecordSet.DoQuery(s);

                        if (oRecordSet.RecordCount == 0)
                        {
                            FSBOApp.StatusBar.SetText("Los datos de acceso al servidor SQL no son validos (Gestion->Definiciones->Factura Electrónica->Configuración Conexión), guarde los datos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }
                        connection.IntegratedSecurity = false;
                        Pass = ((System.String)oRecordSet.Fields.Item("Password").Value).Trim();
                        Usuario = ((System.String)oRecordSet.Fields.Item("Usuario").Value).Trim();

                        connection.UserID = Usuario; // FCmpny.DbUserName.ToString().Trim();
                        connection.Password = Pass;
                    }
                    catch (Exception ex)
                    {
                        OutLog(ex.Message + ", TRACE " + ex.StackTrace + ", USER sa, PASS " + Pass + ", SERVER " + FCmpny.Server.ToString().Trim() + ", DATA BASE " + FCmpny.CompanyDB.ToString().Trim() + ", file " + sNombreArchivo);
                    }


                    foreach (CrystalDecisions.CrystalReports.Engine.Table tabla in rpt.Database.Tables)
                    {
                        logOnInfo = tabla.LogOnInfo;
                        logOnInfo.ConnectionInfo = connection;
                        tabla.ApplyLogOnInfo(logOnInfo);
                    }

                    if (rpt.Subreports.Count > 0)
                    {
                        foreach (CrystalDecisions.CrystalReports.Engine.Table tabla in rpt.Subreports[0].Database.Tables)
                        {
                            logOnInfo = tabla.LogOnInfo;
                            logOnInfo.ConnectionInfo = connection;
                            tabla.ApplyLogOnInfo(logOnInfo);
                        }
                    }

                    rpt.VerifyDatabase();
                    rpt.SetParameterValue("ObjectId@", ObjType);
                    rpt.SetParameterValue("DocKey@", DocEntry);
                    //    'rpt.PrintOptions.PrinterName = "certificado"

                    if (ImpFiscal == "Y")
                    {
                        if (Impresora == "")
                            FSBOApp.MessageBox("No se ha paramterizado impresora, Gestión -> Definiciones -> Facturación Electrónica -> Parametrización Addon FE");
                        else
                        {
                            for (Int32 x = 1; x <= Convert.ToInt32(CantImp); x++)
                            {
                                //rpt.PrintOptions.PrinterName = "FiscalFE";
                                //rpt.PrintToPrinter(1, true, 0, 0);
                                System.Drawing.Printing.PrinterSettings printer = new System.Drawing.Printing.PrinterSettings();
                                printer.PrinterName = Impresora.Trim();
                                System.Drawing.Printing.PageSettings page = new System.Drawing.Printing.PageSettings();
                                rpt.PrintToPrinter(printer, page, false);
                            }
                        }
                    }

                    String fecha;
                    //Dim expFile As CrystalDecisions.Shared.DiskFileDestinationOptions
                    //expFile = New DiskFileDestinationOptions
                    if (!Directory.Exists(oPath + "\\PDF"))
                        Directory.CreateDirectory(oPath + "\\PDF");

                    if (nMultiSoc == "")
                        s = oPath + "\\PDF\\" + TipoDocElect + "_" + FolioNum + ".pdf";
                    else
                        s = oPath + "\\PDF\\" + nMultiSoc + "_" + TipoDocElect + "_" + FolioNum + ".pdf";
                    //OutLog(s);
                    rpt.ExportToDisk(ExportFormatType.PortableDocFormat, s);

                    if (FSBOApp.MessageBox("¿ Desea abrir documento PDF ?", 2, "Si", "No", "") == 1)
                        System.Diagnostics.Process.Start(s);

                    rpt.Close();
                    return true;
                }
                catch (Exception e)
                {
                    //oSBOApplication.MessageBox(e.Message + ", TRACE " + e.StackTrace);
                    OutLog(e.Message + ", TRACE " + e.StackTrace + ", file " + sNombreArchivo);
                    FSBOApp.StatusBar.SetSystemMessage("GenerateCrystalReport() : " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                finally
                {
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(rpt);
                    rpt.Dispose();
                    connection = null;
                    rpt.Dispose();
                    rpt = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    FSBOf._ReleaseCOMObject(rpt);
                }
            }
        }

    }//fin clase
}
