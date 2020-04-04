using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Data.SqlClient;
using System.Data;
using Factura_Electronica_VK.CreditNotes;
using Factura_Electronica_VK.DeliveryNote;
using Factura_Electronica_VK.Invoice;
using System.Xml;

namespace Factura_Electronica_VK.PConfirmacionFolio
{
    class TPConfirmacionFolio : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private String s;
        private String Localidad;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                //FSBOf.LoadForm(xmlPath, 'VID_Entrega.srf', Uid);
                oForm = FSBOApp.Forms.Item(uid);
                //Flag := false;
                /*if (GlobalSettings.RunningUnderSQLServer)
                    s = "select ISNULL(U_Localidad,'CL') Localidad from [@VID_FEPARAM] where Code = '1'";
                else
                    s = @"select IFNULL(""U_Localidad"",'CL') ""Localidad"" from ""@VID_FEPARAM"" where ""Code"" = '1' ";

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                    throw new Exception("Debe parametrizar el Addon Factura Electronica");
                else*/
                Localidad = "CL";// ((System.String)oRecordSet.Fields.Item("Localidad").Value).Trim();

                oForm.Freeze(true);


            }
            catch(Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            oForm.Freeze(false);
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
            SAPbouiCOM.EditText oEditText;
            String[] FE52 = {"15","67","21"};

            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if (Localidad == "CL")
                    {
                        if (pVal.ItemUID == "4")
                        {
                            s = GlobalSettings.PrevFormUID;
                            oFormAux = FSBOApp.Forms.Item(s);
                            //if (oFormAux.BusinessObject.Type in ['15','67'])
                            if (FE52.Contains(oFormAux.BusinessObject.Type))
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
                                            JOIN ""{0}"" T1 ON T1.""Series"" = T0.""Series""
                                           WHERE (SUBSTRING(UPPER(T0.""BeginStr""), 1, 1) = 'E') 
                                             AND T1.""DocEntry"" = {1}
                                             --AND T0.""ObjectCode"" = '{2}'";
                                    }
                                    s = String.Format(s, Tabla, DocEntry, oFormAux.BusinessObject.Type);
                                    oRecordSet.DoQuery(s);
                                    if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                                    {
                                        BubbleEvent = false;
                                        oForm.Close();
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
                    //if (oFormAux.BusinessObject.Type in ['15','67'])
                    if (Localidad == "CL")
                    {
                        if (FE52.Contains(oFormAux.BusinessObject.Type))
                        {
                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = @"select isnull(U_FolioGuia,'N') FolioUnico from [@VID_FEPARAM] where code = '1'"; }
                            else
                            { s = @"select IFNULL(""U_FolioGuia"",'N') ""FolioUnico"" from ""@VID_FEPARAM"" where ""Code"" = '1'"; }

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
                                        JOIN ""{0}"" T1 ON T1.""Series"" = T0.""Series""
                                       WHERE (SUBSTRING(UPPER(T0.""BeginStr""), 1, 1) = 'E') 
                                         AND T1.""DocEntry"" = {1}
                                         --AND T0.""ObjectCode"" = '{2}'";
                                }
                                s = String.Format(s, Tabla, DocEntry, oFormAux.BusinessObject.Type);
                                oRecordSet.DoQuery(s);
                                if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                                {
                                    if (GlobalSettings.RunningUnderSQLServer)
                                    { s = @"SELECT 'GE' BeginStr, NextNumber FROM NNM1 WHERE (ObjectCode = 'VD_FEEntreg')"; }
                                    else
                                    { s = @"SELECT 'GE' ""BeginStr"", ""NextNumber"" FROM ""NNM1"" WHERE (""ObjectCode"" = 'VD_FEEntreg')"; }
                                    oRecordSet.DoQuery(s);
                                    s = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("NextNumber").Value));
                                    oEditText = (EditText)(oForm.Items.Item("7").Specific);
                                    oEditText.Value = s;
                                }
                            }
                        }
                    }
                }

            }
            catch(Exception e)
            {
                FCmpny.GetLastError(out nErr, out sErr);
                FSBOApp.StatusBar.SetText("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin FormEvent


        public new void FormDataEvent(ref BusinessObjectInfo BusinessObjectInfo, ref Boolean BubbleEvent)
        {
            String DocEntry;
            String Tabla;
            String[] FE52 = {"15","67","21"};

            base.FormDataEvent(ref BusinessObjectInfo, ref BubbleEvent);
            try
            {
                if ((BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_LOAD) && (!BusinessObjectInfo.BeforeAction))
                {}

                if ((BusinessObjectInfo.BeforeAction == true) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_UPDATE) && (!BusinessObjectInfo.ActionSuccess))
                {
                    if (Localidad == "CL")
                    {
                        //if (oForm.BusinessObject.Type in ['15','67'])
                        if (FE52.Contains(oForm.BusinessObject.Type))
                        {
                            if (oForm.BusinessObject.Type == "15")
                            { Tabla = "ODLN"; }
                            else if (oForm.BusinessObject.Type == "21")
                            { Tabla = "ORPD"; }
                            else
                            { Tabla = "OWTR"; }

                            DocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
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
                                    JOIN ""{0}"" T1 ON T1.""Series"" = T0.""Series""
                                   WHERE (SUBSTRING(UPPER(T0.""BeginStr""), 1, 1) = 'E') 
                                     AND T1.""DocEntry"" = {1}
                                     --AND T0.""ObjectCode"" = '{2}'";
                            }
                            s = String.Format(s, Tabla, DocEntry, oForm.BusinessObject.Type);

                            if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                            { BubbleEvent = false; }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                OutLog("FormDataEvent: " + e.Message + " ** TRACE ** " + e.StackTrace);
            }
        }//fin FormDataEvent

    }//fin Class
}
