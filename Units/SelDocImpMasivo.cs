using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using SAPbouiCOM;
using SAPbobsCOM;
using System.Globalization;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using System.Reflection;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using VisualD.untLog;
using Factura_Electronica_VK.Functions;
using Factura_Electronica_VK.CreditNotes;
using Factura_Electronica_VK.DeliveryNote;
using Factura_Electronica_VK.Invoice;

namespace Factura_Electronica_VK.SelDocImpMasivo
{
    class TSelDocImpMasivo : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private String s;
        private SAPbouiCOM.ComboBox oComboBox;
        private SAPbouiCOM.EditText oEditText;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                //FSBOf.LoadForm(xmlPath, 'VID_Entrega.srf', Uid);
                oForm = FSBOApp.Forms.Item(uid);
                //Flag := false;
                oForm.Freeze(true);
            }
            catch(Exception e)
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
            Int32 nErr = 0;
            //String sErr = "";
            SAPbouiCOM.Form oFormAux;
            String DocEntry = "";
            String Tabla = "";
            String ObjType = "";
            XmlNode N;
            Boolean FolioUnico;
            Int32 i;
            Boolean bMultiSoc;
            String nMultiSoc;
            String[] FE52 = {"15","67","21"};
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if (pVal.ItemUID == "4")
                    {
                        s = GlobalSettings.PrevFormUID;
                        oFormAux = FSBOApp.Forms.Item(s);
                        
                        //if (oFormAux.BusinessObject.Type in ["15","67","21"])
                        if (FE52.Contains(oFormAux.BusinessObject.Type))
                        {
                            if (GlobalSettings.RunningUnderSQLServer)
                            {   s = "select isnull(U_FolioGuia,'N') FolioUnico, ISNULL(U_Distrib,'N') 'Distribuido', ISNULL(U_MultiSoc,'N') MultiSoc from [@VID_FEPARAM] where code = '1'"; }
                            else
                            {   s = @"select IFNULL(""U_FolioGuia"",'N') ""FolioUnico"", IFNULL(""U_Distrib"",'N') ""Distribuido"", IFNULL(""U_MultiSoc"",'N') ""MultiSoc"" from ""@VID_FEPARAM"" where ""Code"" = '1'"; }
                            oRecordSet.DoQuery(s);

                            if ((System.String)(oRecordSet.Fields.Item("Distribuido").Value) == "N")
                            {
                                if ((System.String)(oRecordSet.Fields.Item("MultiSoc").Value) == "Y")
                                {   bMultiSoc = true; }
                                else
                                {   bMultiSoc = false; }

                                if ((System.String)(oRecordSet.Fields.Item("FolioUnico").Value) == "Y")
                                {   FolioUnico = true; }
                                else
                                {   FolioUnico = false; }

                                if (FolioUnico)
                                {
                                    if (oFormAux.BusinessObject.Type == "15")
                                    {   Tabla = "ODLN"; }
                                    else if (oFormAux.BusinessObject.Type == "21")
                                    {   Tabla = "ORPD"; }
                                    else
                                    {   Tabla = "OWTR"; }

                                    //DocEntry := N.InnerText;
                                    if (GlobalSettings.RunningUnderSQLServer)
                                    {   s = @"SELECT Count(*) Cont, SUBSTRING(ISNULL(T0.BeginStr,''), 2, LEN(T0.BeginStr)) Inst
                                                FROM NNM1 T0
                                                JOIN {0} T1 ON T1.Series = T0.Series
                                               WHERE (SUBSTRING(UPPER(T0.BeginStr), 1, 1) = 'E') 
                                                 AND T1.DocEntry = {1}
                                                 --AND T0.ObjectCode = '{2}'
                                              GROUP BY SUBSTRING(ISNULL(T0.BeginStr,''), 2, LEN(T0.BeginStr))"; }
                                    else
                                    {   s = @"SELECT Count(*) ""Cont"", SUBSTRING(IFNULL(T0.""BeginStr"",''), 2, LENGTH(T0.""BeginStr"")) ""Inst""
                                                FROM ""NNM1"" T0
                                                JOIN ""{0}"" T1 ON T1.""Series"" = T0.""Series""
                                               WHERE (SUBSTRING(UPPER(T0.""BeginStr""), 1, 1) = 'E') 
                                                 AND T1.""DocEntry"" = {1}
                                                 --AND T0.""ObjectCode"" = '{2}'
                                              GROUP BY SUBSTRING(IFNULL(T0.""BeginStr"",''), 2, LENGTH(T0.""BeginStr""))"; }
                                    s = String.Format(s, Tabla, DocEntry, oFormAux.BusinessObject.Type);
                                    oRecordSet.DoQuery(s);
                                    if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                                    {
                                        nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                                        if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        {   FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
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
                    //s := GlobalSettings.PrevFormUID;
                    //oFormAux := FSBOApp.Forms.Item(s);
                    i = 0;
                    while (i <= oForm.DataSources.DBDataSources.Count -1)
                    {
                        Tabla = (System.String)(oForm.DataSources.DBDataSources.Item(i).TableName);
                        if ((Tabla == "ODLN") || (Tabla == "OWTR") || (Tabla == "ORPD"))
                        {   
                            if (GlobalSettings.RunningUnderSQLServer)
                            {   s = @"select isnull(U_FolioGuia,'N') FolioUnico from [@VID_FEPARAM] where code = '1'"; }
                            else
                            {   s = @"select IFNULL(""U_FolioGuia"",'N') ""FolioUnico"" from ""@VID_FEPARAM"" where ""Code"" = '1'"; }
                            oRecordSet.DoQuery(s);
                            if ((System.String)(oRecordSet.Fields.Item("FolioUnico").Value) == "Y")
                            {   FolioUnico = true; }
                            else
                            {   FolioUnico = false; }

                            if (FolioUnico)
                            {
                                if (Tabla == "ODLN")
                                {   ObjType = "15"; }
                                else if (Tabla == "ORPD")
                                {   ObjType = "21"; }
                                else
                                {   ObjType = "67"; }

                                DocEntry = oForm.DataSources.DBDataSources.Item(i).GetValue("DocEntry", 0);

                                if (GlobalSettings.RunningUnderSQLServer)
                                {   s = @"SELECT Count(*) Cont
                                            FROM NNM1 T0
                                            JOIN {0} T1 ON T1.Series = T0.Series
                                           WHERE (SUBSTRING(UPPER(T0.BeginStr), 1, 1) = 'E') 
                                             AND T1.DocEntry = {1}
                                             --AND T0.ObjectCode = '{2}'"; }
                                else
                                {   s = @"SELECT Count(*) ""Cont""
                                            FROM ""NNM1"" T0
                                            JOIN ""{0}"" T1 ON T1.""Series"" = T0.""Series""
                                           WHERE (SUBSTRING(UPPER(T0.""BeginStr""), 1, 1) = 'E') 
                                             AND T1.""DocEntry"" = {1}
                                             --AND T0.""ObjectCode"" = '{2}'"; }
                                s = String.Format(s, Tabla, DocEntry, ObjType);
                                oRecordSet.DoQuery(s);
                                if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                                {
                                    if (GlobalSettings.RunningUnderSQLServer)
                                    {   s = @"SELECT 'GE' BeginStr, NextNumber FROM NNM1 WHERE (ObjectCode = 'VD_FEEntreg')"; }
                                    else
                                    {   s = @"SELECT 'GE' ""BeginStr"", ""NextNumber"" FROM ""NNM1"" WHERE (""ObjectCode"" = 'VD_FEEntreg') "; }
                                    oRecordSet.DoQuery(s);
                                    s = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("NextNumber").Value));
                                    oEditText = (EditText)(oForm.Items.Item("39").Specific);
                                    oEditText.Value = s;
                                }
                            }
                            i = oForm.DataSources.DBDataSources.Count;
                        }
                        i++;
                    }
                }

            }
            catch (Exception e)
            {
            }
        }//fin FormEvent


        public new void FormDataEvent(ref BusinessObjectInfo BusinessObjectInfo, ref Boolean BubbleEvent)
        {
            String DocEntry;
            Boolean bMultiSoc;
            String nMultiSoc;
            String TipoDocElect;
            String[] FE52 = { "15", "67", "21" };
            SAPbobsCOM.Documents oDocuments;

            base.FormDataEvent(ref BusinessObjectInfo, ref BubbleEvent);
            try
            {
                if ((BusinessObjectInfo.BeforeAction == false) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_UPDATE) && (BusinessObjectInfo.ActionSuccess))
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = "select ISNULL(U_Distrib,'N') 'Distribuido', ISNULL(U_MultiSoc,'N') MultiSoc from [@VID_FEPARAM]";
                    else
                        s = @"select IFNULL(""U_Distrib"",'N') ""Distribuido"", IFNULL(""U_MultiSoc"",'N') ""MultiSoc"" from ""@VID_FEPARAM"" ";
                    oRecordSet.DoQuery(s);

                    if (oRecordSet.RecordCount > 0)
                    {
                        if ((System.String)(oRecordSet.Fields.Item("Distribuido").Value) == "N")
                        {
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
                                           where T0.""DocEntry"" = {0}";
                                s = String.Format(s, DocEntry);
                                oRecordSet.DoQuery(s);

                                if (((System.String)(oRecordSet.Fields.Item("TipoDocElect").Value) == "111") && ((System.String)(oRecordSet.Fields.Item("DocSubType").Value) == "DN"))
                                    TipoDocElect = "111";
                                else if (((System.String)(oRecordSet.Fields.Item("TipoDocElect").Value) != "111") && ((System.String)(oRecordSet.Fields.Item("DocSubType").Value) == "DN"))
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
                                    TipoDocElect = "33";

                                s = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                if ((System.String)(oRecordSet.Fields.Item("Tipo").Value) == "E")
                                {
                                    nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                                    if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                    else
                                    {
                                        var oInvoice = new TInvoice();
                                        oInvoice.SBO_f = FSBOf;
                                        oDocuments = null;
                                        oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oInvoices));
                                        if (oDocuments.GetByKey(Convert.ToInt32(DocEntry)))//**se dejo la normal mientras se termina la modificacion en el portal 20170202
                                            oInvoice.EnviarFE_WebService(oForm.BusinessObject.Type, oDocuments, TipoDocElect, false, "", GlobalSettings.RunningUnderSQLServer, "--", TipoDocElect, false);

                                    }
                                }
                            }
                            else if (oForm.BusinessObject.Type == "14") //And (Flag = true)) then
                            {
                                //Flag := false;
                                DocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst, SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) TipoDocElect, T0.ObjType from ORIN T0 JOIN NNM1 T2 ON T0.Series = T2.Series where T0.DocEntry = {0}";
                                else
                                    s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""TipoDocElect"", T0.""ObjType"" from ""ORIN"" T0 JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" where T0.""DocEntry"" = {0}";
                                s = String.Format(s, DocEntry);
                                oRecordSet.DoQuery(s);

                                if ((System.String)(oRecordSet.Fields.Item("TipoDocElect").Value) == "112")
                                    TipoDocElect = "112";
                                else
                                    TipoDocElect = "61";

                                var DocSubTypeNC = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                if ((System.String)(oRecordSet.Fields.Item("Tipo").Value) == "E")
                                {
                                    nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                                    if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                    else
                                    {
                                        var oCreditNotes = new TCreditNotes();
                                        oCreditNotes.SBO_f = FSBOf;
                                        oCreditNotes.EnviarFE_WebServiceNotaCredito(DocEntry, DocSubTypeNC, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, (System.String)(oRecordSet.Fields.Item("ObjType").Value), TipoDocElect, TipoDocElect, false);
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
                                        s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst from ODLN T0 JOIN NNM1 T2 ON T0.Series = T2.Series where T0.DocEntry = {0}";
                                    else if (oForm.BusinessObject.Type == "21")
                                        s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst from ORPD T0 JOIN NNM1 T2 ON T0.Series = T2.Series where T0.DocEntry = {0}";
                                    else if (oForm.BusinessObject.Type == "67")
                                        s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst from OWTR T0 JOIN NNM1 T2 ON T0.Series = T2.Series where T0.DocEntry = {0}";
                                }
                                else
                                {
                                    if (oForm.BusinessObject.Type == "15")
                                        s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"" from ""ODLN"" T0 JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" where T0.""DocEntry"" = {0}";
                                    else if (oForm.BusinessObject.Type == "21")
                                        s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"" from ""ORPD"" T0 JOIN ""NNM1"" T2 ON T0.""Series"" = ""T2.Series"" where T0.""DocEntry"" = {0}";
                                    else if (oForm.BusinessObject.Type == "67")
                                        s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"" from ""OWTR"" T0 JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" where T0.""DocEntry"" = {0}";
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
                    {   FSBOApp.StatusBar.SetText("Debe Parametrizar el Addon", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                }
            }
            catch (Exception e)
            {
            }

        }//fin FormDataEvent

        private void IngresarFolio(String sDocEntry, String ObjType)
        {
            Boolean FolioUnico = false;
            Int32 lRetCode;

            try
            {
                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"select isnull(U_FolioGuia,'N') FolioUnico from [@VID_FEPARAM] where code = '1'"; }
                else
                {   s = @"select IFNULL(""U_FolioGuia"",'N') ""FolioUnico"" from ""@VID_FEPARAM"" where ""Code"" = '1'"; }
                
                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                {
                    if ((System.String)(oRecordSet.Fields.Item("FolioUnico").Value) == "Y") FolioUnico = true;
                }

                if (FolioUnico)
                {
                    var oTransfer = (SAPbobsCOM.StockTransfer)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer));
                    FCmpny.StartTransaction();
                    if (GlobalSettings.RunningUnderSQLServer)
                    {   s = @"SELECT 'GE' BeginStr, NextNumber FROM NNM1 WHERE (ObjectCode = 'VD_FEEntreg')"; }
                    else
                    {   s = @"SELECT 'GE' ""BeginStr"", ""NextNumber"" FROM ""NNM1"" WHERE (""ObjectCode"" = 'VD_FEEntreg')"; }
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
                                {   s = @"UPDATE OWTR SET FolioPref = '{0}', FolioNum = '{1}', Printed = 'Y' WHERE DocEntry = {3}"; }
                                else
                                {   s = @"UPDATE ""OWTR"" SET ""FolioPref"" = '{0}', ""FolioNum"" = '{1}', ""Printed"" = 'Y' WHERE ""DocEntry"" = {3}"; }
                                s = String.Format(s, oTransfer.FolioPrefixString, Convert.ToString(oTransfer.FolioNumber), oTransfer.DocEntry);
                                oRecordSet.DoQuery(s);
                            }
                    
                            //actualiza siguiente numero folio para documento
                            if (GlobalSettings.RunningUnderSQLServer)
                                s = @"UPDATE NNM1 SET NextFolio = {0} WHERE (Series = {1})";
                            else
                                s = @"UPDATE ""NNM1"" SET ""NextFolio"" = {0} WHERE (""Series"" = {1})";
                            s = String.Format(s, oTransfer.FolioNumber + 1, oTransfer.Series);
                            oRecordSet.DoQuery(s);
                            //actualiza siguiente numero folio para serie del addon entrega electronica
                            if (GlobalSettings.RunningUnderSQLServer)
                                s = @"UPDATE NNM1 SET NextNumber = {0} WHERE (ObjectCode = 'VD_FEEntreg')";
                            else
                                s = @"UPDATE ""NNM1"" SET ""NextNumber"" = {0} WHERE (""ObjectCode"" = 'VD_FEEntreg')";
                            s = String.Format(s, oTransfer.FolioNumber + 1);
                            oRecordSet.DoQuery(s);
                            //actualiza LPgFolioN
                            if (GlobalSettings.RunningUnderSQLServer)
                                s = @"update OWTR set LPgFolioN = FolioNum where DocEntry = {0}";
                            else
                                s = @"update ""OWTR"" set ""LPgFolioN"" = ""FolioNum"" where ""DocEntry"" = {0}";
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
                                {   s = @"UPDATE ODLN SET FolioPref = '{0}', FolioNum = '{1}', Printed = 'Y' WHERE DocEntry = {3}"; }
                                else
                                {   s = @"UPDATE ""ODLN"" SET ""FolioPref"" = '{0}', ""FolioNum"" = '{1}', ""Printed"" = 'Y' WHERE ""DocEntry"" = {3}"; }
                                s = String.Format(s, oDocumento.FolioPrefixString, Convert.ToString(oDocumento.FolioNumber), oDocumento.DocEntry);
                                oRecordSet.DoQuery(s);
                            }

                            //actualiza siguiente numero folio para documento
                            if (GlobalSettings.RunningUnderSQLServer)
                            {   s = @"UPDATE NNM1 SET NextFolio = {0} WHERE (Series = {1})"; }
                            else
                            {   s = @"UPDATE ""NNM1"" SET ""NextFolio"" = {0} WHERE (""Series"" = {1})"; }
                            s = String.Format(s, oDocumento.FolioNumber + 1, oDocumento.Series);
                            oRecordSet.DoQuery(s);
                            //actualiza siguiente numero folio para serie del addon entrega electronica
                            if (GlobalSettings.RunningUnderSQLServer)
                            {   s = @"UPDATE NNM1 SET NextNumber = {0} WHERE (ObjectCode = 'VD_FEEntreg')"; }
                            else
                            {   s = @"UPDATE ""NNM1"" SET ""NextNumber"" = {0} WHERE (""ObjectCode"" = 'VD_FEEntreg')"; }
                            s = String.Format(s, oDocumento.FolioNumber + 1);
                            oRecordSet.DoQuery(s);
                            //actualiza LPgFolioN
                            if (GlobalSettings.RunningUnderSQLServer)
                            {   s = @"update ODLN set LPgFolioN = FolioNum where DocEntry = {0}"; }
                            else
                            {   s = @"update ""ODLN"" set ""LPgFolioN"" = ""FolioNum"" where ""DocEntry"" = {0}"; }
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
                                {   s = @"UPDATE ORPD SET FolioPref = '{0}', FolioNum = '{1}', Printed = 'Y' WHERE DocEntry = {3}"; }
                                else
                                {   s = @"UPDATE ""ORPD"" SET ""FolioPref"" = '{0}', ""FolioNum"" = '{1}', ""Printed"" = 'Y' WHERE ""DocEntry"" = {3}"; }
                                s = String.Format(s, oDocumento.FolioPrefixString, Convert.ToString(oDocumento.FolioNumber), oDocumento.DocEntry);
                                oRecordSet.DoQuery(s);
                            }

                            //actualiza siguiente numero folio para documento
                            if (GlobalSettings.RunningUnderSQLServer)
                            {   s = @"UPDATE NNM1 SET NextFolio = {0} WHERE (Series = {1})"; }
                            else
                            {   s = @"UPDATE ""NNM1"" SET ""NextFolio"" = {0} WHERE (""Series"" = {1})"; }
                            s = String.Format(s, oDocumento.FolioNumber + 1, oDocumento.Series);
                            oRecordSet.DoQuery(s);
                            //actualiza siguiente numero folio para serie del addon entrega electronica
                            if (GlobalSettings.RunningUnderSQLServer)
                            {   s = @"UPDATE NNM1 SET NextNumber = {0} WHERE (ObjectCode = 'VD_FEEntreg')"; }
                            else
                            {   s = @"UPDATE ""NNM1"" SET ""NextNumber"" = {0} WHERE (""ObjectCode"" = 'VD_FEEntreg')"; }
                            s = String.Format(s, oDocumento.FolioNumber + 1);
                            oRecordSet.DoQuery(s);
                            //actualiza LPgFolioN
                            if (GlobalSettings.RunningUnderSQLServer)
                            {   s = @"update ORPD set LPgFolioN = FolioNum where DocEntry = {0}"; }
                            else
                            {   s = @"update ""ORPD"" set ""LPgFolioN"" = ""FolioNum"" where ""DocEntry"" = {0}"; }
                            s = String.Format(s, oDocumento.DocEntry);
                            oRecordSet.DoQuery(s);
                        }
                    }

                    FCmpny.EndTransaction(BoWfTransOpt.wf_Commit);
                }
            }
            catch(Exception e)
            {
                OutLog("IngresarFolio " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);  // Captura errores no manejados
                if (FCmpny.InTransaction) FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
            }
        }//fin IngresarFolio
    }
}
