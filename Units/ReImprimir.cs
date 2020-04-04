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
using Factura_Electronica_VK.CreditNotes;
using Factura_Electronica_VK.DeliveryNote;
using Factura_Electronica_VK.Invoice;
using Factura_Electronica_VK.PurchaseInvoice;
using System.Data;

namespace Factura_Electronica_VK.ReImprimir
{
    class TReImprimir : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;
        private SAPbouiCOM.EditText oEditText;
        private Boolean bFolioPortal;
        private Boolean bMultiSoc;
        private String URL_CL;
        private SqlConnection ConexionADO = null;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            SAPbouiCOM.ComboBox oCombo;
            //return inherited InitForm(uid, xmlPath,var application,var company,var sboFunctions,var _GlobalSettings );
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                //Lista    := New list<string>;

                FSBOf.LoadForm(xmlPath, "VID_ReImprimir.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All

                //oForm.DataBrowser.BrowseBy := "DocNum"; 

                // Ok Ad  Fnd Vw Rq Sec
                //Lista.Add('DocNum    , f,  f,  t,  f, n, 1');
                //Lista.Add('DocDate   , f,  t,  f,  f, r, 1');
                //Lista.Add('CardCode  , f,  t,  t,  f, r, 1');
                //FSBOf.SetAutoManaged(var oForm, Lista);

                if (GlobalSettings.RunningUnderSQLServer)
                    s = "select ISNULL(U_MultiSoc,'N') MultiSoc, ISNULL(U_httpBol,'') 'URLCL' from [@VID_FEPARAM] where Code = '1'";
                else
                    s = @"select IFNULL(""U_MultiSoc"",'N') ""MultiSoc"", IFNULL(""U_httpBol"",'') ""URLCL"" from ""@VID_FEPARAM"" where ""Code"" = '1' ";

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                    throw new Exception("Debe parametrizar el Addon Factura Electronica");
                else
                {
                    if ((System.String)(oRecordSet.Fields.Item("MultiSoc").Value) == "Y")
                        bMultiSoc = true;
                    else
                        bMultiSoc = false;

                    URL_CL = ((System.String)oRecordSet.Fields.Item("URLCL").Value).Trim();

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

                oCombo = (ComboBox)(oForm.Items.Item("TipDoc").Specific);
                oCombo.ValidValues.Add("33", "Factura");
                oCombo.ValidValues.Add("33A", "Factura por Anticipo");
                oCombo.ValidValues.Add("34", "Factura Exenta");
                oCombo.ValidValues.Add("39", "Boleta");
                oCombo.ValidValues.Add("41", "Boleta Exenta");
                oCombo.ValidValues.Add("43", "Liquidacion Factura");
                oCombo.ValidValues.Add("43N", "Liquidacion Factura por Nota Debito");
                oCombo.ValidValues.Add("46", "Factura de Compra a terceros");
                oCombo.ValidValues.Add("46A", "Factura Anticipo Compra a terceros");
                oCombo.ValidValues.Add("52", "Guia Despacho");
                oCombo.ValidValues.Add("52T", "Guia Despacho por Transferencia Stock");
                oCombo.ValidValues.Add("52S", "Guia Despacho por Solicitud Traslado");
                oCombo.ValidValues.Add("52D", "Guia Despacho por Devolución Compra");
                oCombo.ValidValues.Add("56", "Nota Debito");
                oCombo.ValidValues.Add("61", "Nota de Credito");
                oCombo.ValidValues.Add("61C", "Nota de Credito Compra");
                oCombo.ValidValues.Add("110", "Factura Exportación Electronica");
                oCombo.ValidValues.Add("110R", "Factura Exportación Elect. por Reserva");
                oCombo.ValidValues.Add("111", "Nota de Debito Exportación Elect.");
                oCombo.ValidValues.Add("112", "Nota de Credito Exportacion Elect.");

                oForm.Items.Item("Folio").Visible = false;
                oForm.Items.Item("FolioPref").Visible = false;
                oForm.Items.Item("FolioNum").Visible = true;

                if (bMultiSoc == true)
                {
                    oForm.Items.Item("l3").Visible = true;
                    oForm.Items.Item("TipoInst").Visible = true;
                    oCombo = (ComboBox)(oForm.Items.Item("TipoInst").Specific);
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select DocEntry, U_Sociedad 'Sociedad' from [@VID_FEMULTISOC] where isnull(U_Habilitada,'N') = 'Y'";
                    else
                        s = @"select ""DocEntry"", ""U_Sociedad"" ""Sociedad"" from ""@VID_FEMULTISOC"" where IFNULL(""U_Habilitada"",'N') = 'Y' ";
                    oRecordSet.DoQuery(s);
                    while (!oRecordSet.EoF)
                    {
                        oCombo.ValidValues.Add(((System.Int32)oRecordSet.Fields.Item("DocEntry").Value).ToString(), ((System.String)oRecordSet.Fields.Item("Sociedad").Value).Trim());
                        oRecordSet.MoveNext();
                    }

                }
                else
                {
                    oForm.Items.Item("l3").Visible = false;
                    ((SAPbouiCOM.EditText)oForm.Items.Item("FolioNum").Specific).Active = true;
                    oForm.Items.Item("TipoInst").Visible = false;
                }

                oForm.DataSources.UserDataSources.Add("FolioNum", BoDataType.dt_LONG_NUMBER);
                oEditText = (EditText)(oForm.Items.Item("FolioNum").Specific);
                oEditText.DataBind.SetBound(true, "", "FolioNum");

                oForm.DataSources.UserDataSources.Add("Folio", BoDataType.dt_LONG_NUMBER);
                oEditText = (EditText)(oForm.Items.Item("Folio").Specific);
                oEditText.DataBind.SetBound(true, "", "Folio");

                oForm.DataSources.UserDataSources.Add("FolioPref", BoDataType.dt_SHORT_TEXT, 4);
                oEditText = (EditText)(oForm.Items.Item("FolioPref").Specific);
                oEditText.DataBind.SetBound(true, "", "FolioPref");



                oForm.DataSources.UserDataSources.Add("DocNum", BoDataType.dt_LONG_NUMBER);
                oEditText = (EditText)(oForm.Items.Item("DocNum").Specific);
                oEditText.DataBind.SetBound(true, "", "DocNum");

                ((ComboBox)oForm.Items.Item("TipDoc").Specific).Active = true;
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select ISNULL(U_FPortal,'N') 'FolioPortal' from [@VID_FEPARAM] where code = '1'";
                else
                    s = @"select IFNULL(""U_FPortal"",'N') ""FolioPortal"" from ""@VID_FEPARAM"" where ""Code"" = '1'";
                oRecordSet.DoQuery(s);
                if (((System.String)(oRecordSet.Fields.Item("FolioPortal").Value) == "Y"))
                {
                    bFolioPortal = true;
                    oForm.Items.Item("LbDocNum").Visible = true;
                    oForm.Items.Item("DocNum").Visible = true;

                    oForm.Items.Item("5").Visible = false;
                    oForm.Items.Item("FolioNum").Visible = false;
                }
                else
                {
                    bFolioPortal = false;
                    oForm.Items.Item("LbDocNum").Visible = false;
                    oForm.Items.Item("DocNum").Visible = false;

                    oForm.Items.Item("5").Visible = true;
                    oForm.Items.Item("FolioNum").Visible = true;
                }

                //s := '1';
                //oCombo.Select(s, BoSearchKey.psk_ByValue);

                //EditText(oForm.Items.Item('CardCode').Specific).Active := True;
                oForm.Mode = BoFormMode.fm_OK_MODE;
            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            oForm.Freeze(false);
            return Result;
        }//fin InitForm


        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction == false) && (pVal.ItemUID == "btn1"))
                {
                    //***************************
                    /* ApprovalRequestsService oApprovalRequestsService = ((ApprovalRequestsService)FCmpny.GetCompanyService().GetBusinessService(ServiceTypes.ApprovalRequestsService)); 
                     ApprovalRequestsParams oApprovalRequestsParams = ((ApprovalRequestsParams)oApprovalRequestsService.GetDataInterface(ApprovalRequestsServiceDataInterfaces.arsApprovalRequestsParams));
                     ApprovalRequest oApprovalRequest = ((ApprovalRequest)oApprovalRequestsService.GetDataInterface(ApprovalRequestsServiceDataInterfaces.arsApprovalRequest)); 
                     ApprovalRequestParams oApprovalRequestParams = ((ApprovalRequestParams)oApprovalRequestsService.GetDataInterface(ApprovalRequestsServiceDataInterfaces.arsApprovalRequestParams)); 
 
                     //Get request list 
                     oApprovalRequestsParams = oApprovalRequestsService.GetAllApprovalRequestsList();
                     oApprovalRequestParams = oApprovalRequestsParams.Item(oApprovalRequestsParams.Count - 1);
 
                     //Approve request  
                     oApprovalRequest = oApprovalRequestsService.GetApprovalRequest(oApprovalRequestParams);
                     oApprovalRequest.ApprovalRequestDecisions.Add();
                     oApprovalRequest.ApprovalRequestDecisions.Item(0).Remarks = "Approved";
                     oApprovalRequest.ApprovalRequestDecisions.Item(0).Status = BoApprovalRequestDecisionEnum.ardApproved;
                     // Incase we want to approve with another user, uncomment the following 2 lines 
                     //oApprovalRequest.ApprovalRequestDecisions.Item(0).ApproverUserName = B1User 
                     //oApprovalRequest.ApprovalRequestDecisions.Item(0).ApproverPassword = B1Password 
 
                     try
                     {
                         oApprovalRequestsService.UpdateRequest(oApprovalRequest);
                     }
                     catch (Exception x)
                     {
                         FSBOApp.StatusBar.SetSystemMessage(x.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); 
                     }
                     */

                    //*********

                    BubbleEvent = false;
                    if (Validar())
                        Imprimir();

                }
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin FormEvent


        private Boolean Validar()
        {
            Boolean _result = true;
            String sFolio = "";
            String sTipo;
            String sTipoInst;
            String sDocNum = "";
            String Tabla = "";
            String TablaDir = "";
            String TablaDetalle = "";
            Int32 i32;
            Boolean canConvert;
            String sDocSubType = "";
            String sDocEntry;
            String ObjType;
            String TipoDocElect = "";
            SAPbouiCOM.EditText oEditText;
            SAPbouiCOM.ComboBox oComboBox;

            try
            {
                oEditText = (EditText)(oForm.Items.Item("FolioNum").Specific);
                sFolio = oEditText.Value;
                oEditText = (EditText)(oForm.Items.Item("DocNum").Specific);
                sDocNum = oEditText.Value;
                oComboBox = (ComboBox)(oForm.Items.Item("TipDoc").Specific);
                sTipo = oComboBox.Value;
                oComboBox = (ComboBox)(oForm.Items.Item("TipoInst").Specific);
                sTipoInst = oComboBox.Value;

                if (sTipo == "33") //Factura venta
                {
                    Tabla = "OINV";
                    TablaDetalle = "INV1";
                    TablaDir = "INV12";
                    sDocSubType = "--";
                    TipoDocElect = "33";
                }
                else if (sTipo == "33A") //Factura Anticipo
                {
                    Tabla = "ODPI";
                    TablaDetalle = "DPI1";
                    TablaDir = "DPI12";
                    sDocSubType = "--";
                    TipoDocElect = "33";
                }
                else if (sTipo == "34") //Factura Exenta
                {
                    Tabla = "OINV";
                    TablaDetalle = "INV1";
                    TablaDir = "INV12";
                    sDocSubType = "IE";
                    TipoDocElect = "34";
                }
                else if (sTipo == "39") //Boleta
                {
                    Tabla = "OINV";
                    TablaDetalle = "INV1";
                    TablaDir = "INV12";
                    sDocSubType = "IB";
                    TipoDocElect = "39";
                }
                else if (sTipo == "41") //Boleta exenta
                {
                    Tabla = "OINV";
                    TablaDetalle = "INV1";
                    TablaDir = "INV12";
                    sDocSubType = "EB";
                    TipoDocElect = "41";
                }
                else if (sTipo == "43") //liquidacion de factura
                {
                    Tabla = "ORIN";
                    TablaDetalle = "RIN1";
                    TablaDir = "RIN12";
                    sDocSubType = "--";
                    TipoDocElect = "43";
                }
                else if (sTipo == "43N") //liquidacion de factura por Nota de debito
                {
                    Tabla = "OINV";
                    TablaDetalle = "INV1";
                    TablaDir = "INV12";
                    sDocSubType = "DN";
                    TipoDocElect = "43";
                }
                else if (sTipo == "46") //Factura compra
                {
                    Tabla = "OPCH";
                    TablaDetalle = "PCH1";
                    TablaDir = "PCH12";
                    sDocSubType = "--";
                    TipoDocElect = "46";
                }
                else if (sTipo == "46A") //Factura Anticipo compra
                {
                    Tabla = "ODPO";
                    TablaDetalle = "DPO1";
                    TablaDir = "DPO12";
                    sDocSubType = "--";
                    TipoDocElect = "46";
                }
                else if (sTipo == "56") //nota debito
                {
                    Tabla = "OINV";
                    TablaDetalle = "INV1";
                    TablaDir = "INV12";
                    sDocSubType = "DN";
                    TipoDocElect = "56";
                }
                else if (sTipo == "61") //nota de credito
                {
                    Tabla = "ORIN";
                    TablaDetalle = "RIN1";
                    TablaDir = "RIN12";
                    sDocSubType = "--";
                    TipoDocElect = "61";
                }
                else if (sTipo == "61C") //nota de credito Compra
                {
                    Tabla = "ORPC";
                    TablaDetalle = "RPC1";
                    TablaDir = "RPC12";
                    sDocSubType = "--";
                    TipoDocElect = "61";
                }
                else if (sTipo == "52") //guia despacho por entrega
                {
                    Tabla = "ODLN";
                    TablaDetalle = "DLN1";
                    TablaDir = "DLN12";
                    sDocSubType = "--";
                    TipoDocElect = "52";
                }
                else if (sTipo == "52T") //guia despacho por transferencia stock
                {
                    Tabla = "OWTR";
                    TablaDetalle = "WTR1";
                    TablaDir = "WTR12";
                    sDocSubType = "--";
                    TipoDocElect = "52";
                }
                else if (sTipo == "52S") //guia despacho por solicitud transferencia stock
                {
                    Tabla = "OWTQ";
                    TablaDetalle = "WTQ1";
                    TablaDir = "WTQ12";
                    sDocSubType = "--";
                    TipoDocElect = "52";
                }
                else if (sTipo == "52D") //guia despacho por devolucion mercancia en Compras
                {
                    Tabla = "ORPD";
                    TablaDetalle = "RPD1";
                    TablaDir = "RPD12";
                    sDocSubType = "--";
                    TipoDocElect = "52";
                }
                else if (sTipo == "110") //factura exportacion
                {
                    Tabla = "OINV";
                    TablaDetalle = "INV1";
                    TablaDir = "INV12";
                    sDocSubType = "IX";
                    TipoDocElect = "110";
                }
                else if (sTipo == "110R") //factura exportacion por Reserva
                {
                    Tabla = "OINV";
                    TablaDetalle = "INV1";
                    TablaDir = "INV12";
                    sDocSubType = "--";
                    TipoDocElect = "110";
                }
                else if (sTipo == "111") //nota de debito exportacion
                {
                    Tabla = "OINV";
                    TablaDetalle = "INV1";
                    TablaDir = "INV12";
                    sDocSubType = "DN";
                    TipoDocElect = "111";
                }
                else if (sTipo == "112") //nota de credito exportacion
                {
                    Tabla = "ORIN";
                    TablaDetalle = "RIN1";
                    TablaDir = "RIN12";
                    sDocSubType = "--";
                    TipoDocElect = "112";
                }

                if (sTipo == "")
                {
                    _result = false;
                    FSBOApp.StatusBar.SetText("Debe seleccionar Tipo Documento Electronico", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                }
                if ((sTipoInst == "") && (bMultiSoc == true))
                {
                    _result = false;
                    FSBOApp.StatusBar.SetText("Debe seleccionar Institución", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                }
                else if (Tabla == "")
                {
                    _result = false;
                    FSBOApp.StatusBar.SetText("No se reconoce Tipo Documento Electronico", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                }
                else if ((sFolio == "") && (!bFolioPortal))
                {
                    _result = false;
                    FSBOApp.StatusBar.SetText("Debe ingresar Numero de Folio", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                }
                else if ((sDocNum == "") && (bFolioPortal))
                {
                    _result = false;
                    FSBOApp.StatusBar.SetText("Debe ingresar Número de Documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                }
                //else if ((sFolio != "") && (sDocNum != "") && (bFolioPortal))
                //{
                //    _result = false;
                //    FSBOApp.StatusBar.SetText("Debe ingresar Numero de Folio ó Número de Documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                //}
                else
                {
                    if (bFolioPortal)
                        canConvert = System.Int32.TryParse(sDocNum, out i32);
                    else
                        canConvert = System.Int32.TryParse(sFolio, out i32);
                    if (!canConvert)
                    {
                        _result = false;
                        FSBOApp.StatusBar.SetText("Numero de " + (bFolioPortal ? "Documento" : "Folio") + " debe ser numerico", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    }
                }

                if (_result)
                {
                    if (!bFolioPortal)
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"SELECT SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'DocElec', T0.DocEntry, T0.ObjType, SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst 
                                    FROM {0} T0 
                                    JOIN NNM1 T2 ON T0.Series = T2.Series 
                                   WHERE 1 = 1
                                     AND T0.FolioNum = {1}
                                     --AND T0.DocStatus = 'O'
                                     AND T0.DocSubType = '{2}'
                                    {3}
                                    {4}
                                    {5}
                                    {6}
                                    {7}
                                   ORDER BY T0.DocEntry DESC"; //Confirmar si solo busca documentos abietos
                            s = String.Format(s, Tabla, sFolio, sDocSubType, (bMultiSoc ? " and SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) = '" + sTipoInst + "'" : "")
                                , (sTipo == "110R" ? @" AND T0.isIns = 'Y' " : ""), (sTipo == "43" ? " and SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) = '" + sTipo + "'" : "")
                                , (sTipo == "46" ? " and SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) <> '43'" : "")
                                , (sTipo != "43N" ? " and SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) <> '43'" : ""));
                        }
                        else
                        {
                            s = @"SELECT SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""DocElec"", T0.""DocEntry"", T0.""ObjType"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                                    FROM ""{0}"" T0 
                                    JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                                   WHERE 1 = 1
                                     AND T0.""FolioNum"" = {1}
                                     --AND T0.""DocStatus"" = 'O'
                                     AND T0.""DocSubType"" = '{2}'
                                     {3}
                                     {4}
                                     {5}
                                     {6}
                                     {7}
                                   ORDER BY T0.""DocEntry"" DESC";
                            s = String.Format(s, Tabla, sFolio, sDocSubType, (bMultiSoc ? @" and SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) = '" + sTipoInst + "'" : "")
                               , (sTipo == "110R" ? @" AND T0.""isIns"" = 'Y' " : ""), (sTipo == "43" ? @" and SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) = '" + sTipo + "'" : "")
                               , (sTipo == "46" ? @" and SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) <> '43'" : "")
                               , (sTipo != "43N" ? @" and SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) <> '43'" : ""));
                        }

                    }
                    else
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"SELECT SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'DocElec', T0.DocEntry, T0.ObjType, SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst 
                                    FROM {0} T0 
                                    JOIN NNM1 T2 ON T0.Series = T2.Series 
                                   WHERE 1 = 1
                                     AND T0.DocNum = {1}
                                     --AND T0.DocStatus = 'O'
                                     AND T0.DocSubType = '{2}'
                                     {3}
                                     {4}
                                     {5}
                                     {6}
                                     {7}
                                   ORDER BY T0.DocEntry DESC";  //Confirmar si solo busca documentos abietos
                            s = String.Format(s, Tabla, sDocNum, sDocSubType, (bMultiSoc ? @" and SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) = '" + sTipoInst + "'" : "")
                                  , (sTipo == "110R" ? @" AND T0.isIns = 'Y'" : ""), (sTipo == "43" ? " and SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) = '" + sTipo + "'" : "")
                                  , (sTipo == "46" ? " and SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) <> '43'" : "")
                                  , (sTipo != "43N" ? " and SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) <> '43'" : ""));
                        }
                        else
                        {
                            s = @"SELECT SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""DocElec"", T0.""DocEntry"", T0.""ObjType"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                                    FROM ""{0}"" T0 
                                    JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                                   WHERE 1 = 1
                                     AND T0.""DocNum"" = {1}
                                     --AND T0.""DocStatus"" = 'O'
                                     AND T0.""DocSubType"" = '{2}'
                                     {3}
                                     {4}
                                     {5}
                                     {6}
                                     {7}
                                   ORDER BY T0.""DocEntry"" DESC";
                            s = String.Format(s, Tabla, sDocNum, sDocSubType, (bMultiSoc ? @" and SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) = '" + sTipoInst + "'" : "")
                                   , (sTipo == "110R" ? @" AND T0.""isIns"" = 'Y'" : ""), (sTipo == "43" ? @" and SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) = '" + sTipo + "'" : "")
                                   , (sTipo == "46" ? @" and SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) <> '43'" : "")
                                   , (sTipo != "43N" ? @" and SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) <> '43'" : ""));
                        }

                    }

                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount == 0)
                    {
                        _result = false;
                        FSBOApp.StatusBar.SetText("Numero de " + (bFolioPortal ? "Documento" : "Folio") + " no se ha encontrado", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    }
                    else if ((System.String)(oRecordSet.Fields.Item("DocElec").Value) != "E")
                    {
                        _result = false;
                        FSBOApp.StatusBar.SetText("Documento seleccionado no es electronico", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    }
                    else
                    {
                        sDocEntry = oRecordSet.Fields.Item("DocEntry").Value.ToString();
                        ObjType = oRecordSet.Fields.Item("ObjType").Value.ToString();
                        _result = ValidarDatos(TipoDocElect, Tabla, sDocSubType, sDocEntry, ObjType, TablaDir, TablaDetalle);
                    }
                }

                return _result;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("Validar: " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
        }//fin Validar


        private Boolean ValidarDatos(String TipoDocElec, String Tabla, String DocSubType, String DocEntry, String ObjType, String TablaDir, String TablaDetalle)
        {
            Boolean _result = true;
            String[] CaracteresInvalidos = { "Ñ", "°", "|", "!", @"""", "#", "$", "=", "?", "\\", "¿", "¡", "~", "´", "+", "{", "}", "[", "]", "-", ":", "%" };
            TFunctions Param;
            int i;
            int c;
            Int32 canttotal;
            String nMultiSoc;
            String TablaAux;
            Boolean PedirRef = true;
            //SAPbouiCOM.DBDataSource oDBDSDir;
            //SAPbouiCOM.DBDataSource oDBDSH;
            SAPbouiCOM.DBDataSource oDBDS5 = null;
            SAPbobsCOM.Recordset ors = ((SAPbobsCOM.Recordset)FCmpny.GetBusinessObject(BoObjectTypes.BoRecordset));
            string[] lines;
            string[] lines2;
            Int32 x;

            try
            {
                var oDBDSH = oForm.DataSources.DBDataSources.Add(Tabla);
                var oDBDSDir = oForm.DataSources.DBDataSources.Add(TablaDir);
                var oDBDSDet = oForm.DataSources.DBDataSources.Add(TablaDetalle);

                if (ObjType == "18")
                    oDBDS5 = oForm.DataSources.DBDataSources.Add("PCH5");
                else if (ObjType == "204")
                    oDBDS5 = oForm.DataSources.DBDataSources.Add("DPO5");

                SAPbouiCOM.Conditions oConditions;
                SAPbouiCOM.Condition oCondition;

                oConditions = new SAPbouiCOM.Conditions();
                oCondition = oConditions.Add();
                oCondition.Alias = "DocEntry";
                oCondition.Operation = BoConditionOperation.co_EQUAL;
                oCondition.CondVal = DocEntry;

                oDBDSH.Query(oConditions);
                oDBDSDet.Query(oConditions);
                oDBDSDir.Query(oConditions);


                if ((ObjType == "18") || (ObjType == "204"))
                {
                    oConditions = new SAPbouiCOM.Conditions();
                    oCondition = oConditions.Add();
                    oCondition.Alias = "AbsEntry";
                    oCondition.Operation = BoConditionOperation.co_EQUAL;
                    oCondition.CondVal = DocEntry;

                    oDBDS5.Query(oConditions);
                }


                if (GlobalSettings.RunningUnderSQLServer)
                {
                    s = @"select SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'Inst', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'TipoDocElect'
                                             FROM NNM1 T2 WITH (NOLOCK)
                                               WHERE Series = {0}
                                                 --AND ObjectCode = '14'";
                }
                else
                {
                    s = @"select SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""TipoDocElect""
                                             FROM ""NNM1"" T2 
                                            WHERE ""Series"" = {0}
                                              --AND ""ObjectCode"" = '14' ";
                }

                s = String.Format(s, (System.String)(oDBDSH.GetValue("Series", 0)).Trim());
                oRecordSet.DoQuery(s);

                nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);

                if (TipoDocElec == "46")
                {
                    //Valida que tenga ingreado el impuesto para compras (15 en Impuestos Adicionales)
                    if ((_result) && (bMultiSoc == false))
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = "select COUNT(*) 'Cont' from [@VID_FEIMPADIC] where U_CodImpto = '15'";
                        else
                            s = @"select COUNT(*) ""Cont"" from ""@VID_FEIMPADIC"" where ""U_CodImpto"" = '15' ";
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount > 0)
                        {
                            if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) == 0)
                            {
                                FSBOApp.StatusBar.SetText("Debe ingresar Codigo Adicional 15 - IVA Retenido Total, Gestión -> Definiciones -> Factura Electrónica -> Codificación Impto. Ad.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                return false;
                            }
                        }
                    }
                }

                if (TipoDocElec == "52")
                {
                    //valida para guias de despacho
                    if ((oDBDSH.GetValue("U_Traslado", 0) == "") && (_result))
                    {
                        FSBOApp.StatusBar.SetText("Guia de Despacho Electronica, debe seleccionar Indicador de Traslado", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }
                }
                else if ((ObjType == "14") && (TipoDocElec != "43"))
                {
                    //valida para nota credito
                    if (_result)
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT T0.U_CodRef, T0.U_RazRef, T1.U_DocEntry 'DocEntryRef', T1.U_DocFolio, T1.U_DocDate, T1.U_TipoDTE, T1.U_DocTotal, T1.U_DocTotalFC, ISNULL(T0.U_IndGlobal,'0') 'U_IndGlobal', ISNULL(T0.U_TipoDTE,'00') 'TipoDTE_E'
                                  FROM [@VID_FEREF] T0
                                  LEFT JOIN [@VID_FEREFD] T1 ON T1.DocEntry = T0.DocEntry
                                 WHERE T0.U_DocEntry = {0}
                                   AND T0.U_DocSBO = '{1}'";
                        else
                            s = @"SELECT T0.""U_CodRef"", T0.""U_RazRef"", T1.""U_DocEntry"" ""DocEntryRef"", T1.""U_DocFolio"", T1.""U_DocDate"", T1.""U_TipoDTE"", T1.""U_DocTotal"", T1.""U_DocTotalFC"", IFNULL(T0.""U_IndGlobal"",'0') ""U_IndGlobal"", IFNULL(T0.""U_TipoDTE"",'00') ""TipoDTE_E""
                                  FROM ""@VID_FEREF"" T0
                                  LEFT JOIN ""@VID_FEREFD"" T1 ON T1.""DocEntry"" = T0.""DocEntry""
                                 WHERE T0.""U_DocEntry"" = {0}
                                   AND T0.""U_DocSBO"" = '{1}'";
                        s = String.Format(s, DocEntry, ObjType);
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount == 0)
                        {
                            FSBOApp.StatusBar.SetText("No se encuentra Referencias", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }


                        if (((System.String)oRecordSet.Fields.Item("U_CodRef").Value).Trim() == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe seleccionar Código Referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else
                        {

                            var CodigoReferencia = ((System.String)oRecordSet.Fields.Item("U_CodRef").Value).Trim();
                            c = 0;
                            var BaseEntry = (System.String)(oDBDSDet.GetValue("BaseEntry", 0));
                            var PedirRefCab = false;

                            if ((((System.String)oRecordSet.Fields.Item("U_IndGlobal").Value).Trim() == "1") && (((System.String)oRecordSet.Fields.Item("TipoDTE_E").Value).Trim() == "00"))
                            {
                                FSBOApp.StatusBar.SetText("Debe seleccinar tipo documento referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                _result = false;
                            }
                            else if (((System.String)oRecordSet.Fields.Item("U_IndGlobal").Value).Trim() != "1")
                            {
                                i = 0;
                                while (i < oDBDSDet.Size)
                                {
                                    var BaseEntry2 = (System.String)(oDBDSDet.GetValue("BaseEntry", i));//BaseEntry
                                    var BaseType2 = (System.String)(oDBDSDet.GetValue("BaseType", i)); //basetype

                                    if (BaseEntry != BaseEntry2)
                                        c = c + 1;

                                    if ((BaseEntry2 == "") || ((BaseType2 != "13") && (BaseType2 != "203")))
                                        PedirRefCab = true;
                                    i++;
                                }

                                if ((c > 0) && (((System.String)oRecordSet.Fields.Item("U_CodRef").Value) != "3"))
                                {
                                    FSBOApp.StatusBar.SetText("Nota de credito solo debe tener una Factura de referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    _result = false;
                                }
                                else
                                {
                                    if (PedirRefCab)
                                    {
                                        /*se comenta 20160722 se cambia por grilla de referencia
                                        if (oDBDSH.GetValue("U_TipoRef", 0) == "")
                                        {
                                            FSBOApp.StatusBar.SetText("Debe seleccionar Tipo Doc Referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                            _result = false;
                                        }
                                        else if (oDBDSH.GetValue("U_folioRef", 0) == "")
                                        {
                                            FSBOApp.StatusBar.SetText("Debe ingresar Folio de Factura referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                            _result = false;
                                        }*/
                                        var bDocTotal = false;
                                        var TotalDoc = 0.0;
                                        oRecordSet.MoveFirst();
                                        while (!oRecordSet.EoF)
                                        {
                                            if (TipoDocElec == "61")
                                                TotalDoc = TotalDoc + ((System.Double)oRecordSet.Fields.Item("U_DocTotal").Value);
                                            else if (TipoDocElec == "112")
                                                TotalDoc = TotalDoc + ((System.Double)oRecordSet.Fields.Item("U_DocTotalFC").Value);

                                            s = ((System.String)oRecordSet.Fields.Item("U_TipoDTE").Value);
                                            if (s.IndexOf("b") == -1)
                                            {
                                                bDocTotal = true;
                                                if (((System.String)oRecordSet.Fields.Item("U_TipoDTE").Value).Trim() == "33a")
                                                    TablaAux = "ODPI";
                                                else
                                                    TablaAux = "OINV";

                                                if (GlobalSettings.RunningUnderSQLServer)
                                                    s = @"SELECT COUNT(*) 'Cont'
                                                        FROM {0} T1 WITH (NOLOCK)
                                                        JOIN NNM1 T2 WITH (NOLOCK) ON T1.Series = T2.Series
                                                       WHERE ISNULL(T1.FolioNum, -1) = {1}
                                                         AND CASE 
                                                                WHEN '{2}' = '33'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                                                                WHEN '{2}' = '33a' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                            	                                WHEN '{2}' = '39'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') AND ('{3}' = 'N') THEN 'IB' --para no multibase
				                                                WHEN '{2}' = '41'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') AND ('{3}' = 'N') THEN 'EB' --para no multibase
				                                                WHEN '{2}' = '39'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') AND ('{3}' = 'Y') AND SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) = '{4}' THEN 'IB' --
                                                                WHEN '{2}' = '41'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') AND ('{3}' = 'Y') AND SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) = '{4}' THEN 'EB' --
                            	                                WHEN '{2}' = '110' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'IX'
                            	                                WHEN '{2}' = '34'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'IE'
                            	                                WHEN '{2}' = '56'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'DN'
                                                                WHEN '{2}' = '61'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                                                                WHEN '{2}' = '111' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'DN'
                                                                WHEN '{2}' = '112' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                                                                WHEN '{2}' = '30'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                                                                WHEN '{2}' = '32'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'IE'
                                                                WHEN '{2}' = '35'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'IB'
                            	                                WHEN '{2}' = '38'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'BE'
                                                                WHEN '{2}' = '55'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'DN'
                                                                WHEN '{2}' = '60'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                                                                WHEN '{2}' = '101' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'IX'
                                                                WHEN '{2}' = '104' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'DN'
                                                                WHEN '{2}' = '106' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                            	                                Else '-1'
                                                             END = T1.DocSubType";
                                                else
                                                    s = @"SELECT COUNT(*) ""Cont""
                                                        FROM ""{0}"" T1
                                                        JOIN ""NNM1"" T2 ON T1.""Series"" = T2.""Series""
                                                       WHERE IFNULL(T1.""FolioNum"", -1) = {1}
                                                         AND CASE
                                                                WHEN '{2}' = '33'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                                                                WHEN '{2}' = '33a' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                            	                                WHEN '{2}' = '39'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') AND ('{3}' = 'N') THEN 'IB' --para no multibase
				                                                WHEN '{2}' = '41'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') AND ('{3}' = 'N') THEN 'EB' --para no multibase
				                                                WHEN '{2}' = '39'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') AND ('{3}' = 'Y') AND SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) = '{4}' THEN 'IB' --
                                                                WHEN '{2}' = '41'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') AND ('{3}' = 'Y') AND SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) = '{4}' THEN 'EB' --
                            	                                WHEN '{2}' = '110' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'IX'
                            	                                WHEN '{2}' = '34'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'IE'
                            	                                WHEN '{2}' = '56'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'DN'
                                                                WHEN '{2}' = '61'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                                                                WHEN '{2}' = '111' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'DN'
                                                                WHEN '{2}' = '112' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                                                                WHEN '{2}' = '30'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                                                                WHEN '{2}' = '32'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'IE'
                                                                WHEN '{2}' = '35'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'IB'
                            	                                WHEN '{2}' = '38'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'BE'
                                                                WHEN '{2}' = '55'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'DN'
                                                                WHEN '{2}' = '60'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                                                                WHEN '{2}' = '101' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'IX'
                                                                WHEN '{2}' = '104' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'DN'
                                                                WHEN '{2}' = '106' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                            	                                Else '-1'
                                                             END = T1.""DocSubType"" ";
                                                s = String.Format(s, TablaAux, ((System.Int32)oRecordSet.Fields.Item("U_DocFolio").Value), ((System.String)oRecordSet.Fields.Item("U_TipoDTE").Value), bMultiSoc == true ? "Y" : "N", nMultiSoc);
                                                ors.DoQuery(s);
                                                if ((System.Int32)(ors.Fields.Item("Cont").Value) > 0)
                                                    _result = true;
                                                else
                                                {
                                                    FSBOApp.StatusBar.SetText("No se ha encontrado documento de referencia,", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                    _result = false;
                                                    break;
                                                }

                                                if (!_result)
                                                    break;


                                            }
                                            oRecordSet.MoveNext();
                                        }//fin while

                                        //--if ((FSBOf.StrToDouble(oDBDSH.GetValue("DocTotal", 0)) > TotalDoc) && (_result) && (CodigoReferencia != "3") && (bDocTotal))
                                        //{
                                        //    FSBOApp.StatusBar.SetText("Total del documento Nota de Crédito no puede ser mayor al total de las facturas de venta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        //    _result = false;
                                        //}------
                                        if ((FSBOf.StrToDouble(oDBDSH.GetValue("DocTotal", 0)) > TotalDoc) && (CodigoReferencia != "3") && (bDocTotal) && (TipoDocElec == "61") && (_result))
                                        {
                                            FSBOApp.StatusBar.SetText("Total del documento Nota de Crédito no puede ser mayor al total de las facturas de venta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                            _result = false;
                                        }
                                        else if ((FSBOf.StrToDouble(oDBDSH.GetValue("DocTotalFC", 0)) > TotalDoc) && (CodigoReferencia != "3") && (bDocTotal) && (TipoDocElec == "112") && (_result))
                                        {
                                            FSBOApp.StatusBar.SetText("Total del documento Nota de Crédito no puede ser mayor al total de las facturas de venta(1)", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                            _result = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (ObjType == "19")
                {
                    //valida para nota credito
                    if (_result)
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT T0.U_CodRef, T0.U_RazRef, T1.U_DocEntry 'DocEntryRef', T1.U_DocFolio, T1.U_DocDate, T1.U_TipoDTE, ISNULL(T0.U_IndGlobal,'0') 'U_IndGlobal', ISNULL(T0.U_TipoDTE,'00') 'TipoDTE_E'
                                  FROM [@VID_FEREF] T0
                                  LEFT JOIN [@VID_FEREFD] T1 ON T1.DocEntry = T0.DocEntry
                                 WHERE T0.U_DocEntry = {0}
                                   AND T0.U_DocSBO = '{1}'";
                        else
                            s = @"SELECT T0.""U_CodRef"", T0.""U_RazRef"", T1.""U_DocEntry"" ""DocEntryRef"", T1.""U_DocFolio"", T1.""U_DocDate"", T1.""U_TipoDTE"", IFNULL(T0.""U_IndGlobal"",'0') ""U_IndGlobal"", IFNULL(T0.""U_TipoDTE"",'00') ""TipoDTE_E""
                                  FROM ""@VID_FEREF"" T0
                                  LEFT JOIN ""@VID_FEREFD"" T1 ON T1.""DocEntry"" = T0.""DocEntry""
                                 WHERE T0.""U_DocEntry"" = {0}
                                   AND T0.""U_DocSBO"" = '{1}'";
                        s = String.Format(s, DocEntry, ObjType);
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount == 0)
                        {
                            FSBOApp.StatusBar.SetText("No se encuentra Referencias", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        if (((System.String)oRecordSet.Fields.Item("U_CodRef").Value).Trim() == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe seleccionar Código Referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else
                        {
                            c = 0;
                            var BaseEntry = (System.String)(oDBDSDet.GetValue("BaseEntry", 0));
                            var PedirRefCab = false;

                            if ((((System.String)oRecordSet.Fields.Item("U_IndGlobal").Value).Trim() == "1") && (((System.String)oRecordSet.Fields.Item("TipoDTE_E").Value).Trim() == "00"))
                            {
                                FSBOApp.StatusBar.SetText("Debe seleccinar tipo documento referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                _result = false;
                            }
                            else if (((System.String)oRecordSet.Fields.Item("U_IndGlobal").Value).Trim() != "1")
                            {
                                i = 0;
                                while (i < oDBDSDet.Size)
                                {
                                    var BaseEntry2 = (System.String)(oDBDSDet.GetValue("BaseEntry", i));//BaseEntry
                                    var BaseType2 = (System.String)(oDBDSDet.GetValue("BaseType", i)); //basetype

                                    if (BaseEntry != BaseEntry2)
                                    { c = c + 1; }

                                    if ((BaseEntry2 == "") || (BaseType2 != "18"))
                                    { PedirRefCab = true; }
                                    i++;
                                }

                                if ((c > 0) && (((System.String)oRecordSet.Fields.Item("U_CodRef").Value).Trim() != "3"))
                                {
                                    FSBOApp.StatusBar.SetText("Nota de credito solo debe tener una Factura de referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    _result = false;
                                }
                                else
                                {
                                    if (PedirRefCab)
                                    {
                                        while (!oRecordSet.EoF)
                                        {
                                            s = ((System.String)oRecordSet.Fields.Item("U_TipoDTE").Value).Trim();
                                            if (s.IndexOf("b") == -1)
                                            {
                                                TablaAux = "OPCH";
                                                if (GlobalSettings.RunningUnderSQLServer)
                                                {
                                                    s = @"SELECT COUNT(*) 'Cont'
                                                                        FROM {0} T1 WITH (NOLOCK)
                                                                        JOIN NNM1 T2 WITH (NOLOCK) ON T1.Series = T2.Series
                                                                       WHERE ISNULL(T1.FolioNum, -1) = {1}
                                                                         AND CASE 
                                                                               WHEN '{2}' = '46'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                                                                               WHEN '{2}' = '46a' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                            	                                               WHEN '{2}' = '45'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                                                                               Else '-1'
                                                                             END = T1.DocSubType";
                                                }
                                                else
                                                {
                                                    s = @"SELECT COUNT(*) ""Cont""
                                                                    FROM ""{0}"" T1
                                                                    JOIN ""NNM1"" T2 ON T1.""Series"" = T2.""Series""
                                                                   WHERE IFNULL(T1.""FolioNum"", -1) = {1}
                                                                     AND CASE
                                                                           WHEN '{2}' = '46'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                                                                           WHEN '{2}' = '46a' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                            	                                           WHEN '{2}' = '45'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                                                                           Else '-1'
                                                                         END = T1.""DocSubType"" ";
                                                }
                                                s = String.Format(s, TablaAux, ((System.Int32)oRecordSet.Fields.Item("U_DocFolio").Value), ((System.String)oRecordSet.Fields.Item("U_TipoDTE").Value), bMultiSoc == true ? "Y" : "N", nMultiSoc);
                                                ors.DoQuery(s);
                                                if ((System.Int32)(ors.Fields.Item("Cont").Value) > 0)
                                                    _result = true;
                                                else
                                                {
                                                    FSBOApp.StatusBar.SetText("No se ha encontrado documento de referencia,", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                    _result = false;
                                                    break;
                                                }

                                                if (!_result)
                                                    break;

                                            }
                                            oRecordSet.MoveNext();
                                        }//fin while
                                    }
                                }
                            }
                        }
                    }
                }
                else if ((ObjType == "18") || (ObjType == "204"))
                {
                    if (oDBDS5.Size == 0)
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar Retención total de IVA en el documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }
                    else
                    {
                        var W2 = ((System.String)oDBDS5.GetValue("WTCode", 0)).Trim();

                        if (W2 == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar Retención total de IVA", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else
                        {
                            if (GlobalSettings.RunningUnderSQLServer)
                                s = @"select U_CodImpto, U_Porc, Code from [@VID_FEIMPADIC] where Code = '{0}'";
                            else
                                s = @"select ""U_CodImpto"", ""U_Porc"", ""Code"" from ""@VID_FEIMPADIC"" where ""Code"" = '{0}'";
                            s = String.Format(s, W2);
                            oRecordSet.DoQuery(s);
                            if (oRecordSet.RecordCount > 0)
                            {
                                if (((System.Double)oRecordSet.Fields.Item("U_Porc").Value) != FSBOf.StrToDouble(((System.String)oDBDS5.GetValue("Rate", 0))))
                                {
                                    FSBOApp.StatusBar.SetText("Porcentaje del impuesto " + W2 + " no coincide con el impuesto adicional FE " + ((System.String)oRecordSet.Fields.Item("U_CodImpto").Value).Trim(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    return false;
                                }
                            }
                            else
                            {
                                FSBOApp.StatusBar.SetText("Debe ingresar Codigo Adicional - IVA Retenido, Gestión -> Definiciones -> Factura Electrónica -> Codificación Impto. Ad.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                return false;
                            }
                        }
                    }
                }
                else if ((ObjType == "13") && (TipoDocElec == "56"))
                {
                    //valida para nota debito
                    if ((_result) && ((System.String)(oDBDSH.GetValue("DocSubType", 0)).Trim() == "DN"))
                    {

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT T0.U_CodRef, T0.U_RazRef, T1.U_DocEntry 'DocEntryRef', T1.U_DocFolio, T1.U_DocDate, T1.U_TipoDTE, ISNULL(T0.U_IndGlobal,'0') 'U_IndGlobal', ISNULL(T0.U_TipoDTE,'00') 'TipoDTE_E'
                                  FROM [@VID_FEREF] T0
                                  LEFT JOIN [@VID_FEREFD] T1 ON T1.DocEntry = T0.DocEntry
                                 WHERE T0.U_DocEntry = {0}
                                   AND T0.U_DocSBO = '{1}'";
                        else
                            s = @"SELECT T0.""U_CodRef"", T0.""U_RazRef"", T1.""U_DocEntry"" ""DocEntryRef"", T1.""U_DocFolio"", T1.""U_DocDate"", T1.""U_TipoDTE"", IFNULL(T0.""U_IndGlobal"",'0') ""U_IndGlobal"", IFNULL(T0.""U_TipoDTE"",'00') ""TipoDTE_E""
                                  FROM ""@VID_FEREF"" T0
                                  LEFT JOIN ""@VID_FEREFD"" T1 ON T1.""DocEntry"" = T0.""DocEntry""
                                 WHERE T0.""U_DocEntry"" = {0}
                                   AND T0.""U_DocSBO"" = '{1}'";
                        s = String.Format(s, DocEntry, ObjType);
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount == 0)
                        {
                            FSBOApp.StatusBar.SetText("No se encuentra Referencias", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                            oRecordSet.MoveLast();
                        }

                        while (!oRecordSet.EoF)
                        {
                            if (((System.String)oRecordSet.Fields.Item("U_CodRef").Value).Trim() == "2")
                            {
                                FSBOApp.StatusBar.SetText("Corrige texto documento no es permitido para Nota de Debito", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                _result = false;
                                oRecordSet.MoveLast();
                            }
                            else if (((System.String)oRecordSet.Fields.Item("U_CodRef").Value).Trim() == "")
                            {
                                FSBOApp.StatusBar.SetText("Debe seleccionar Código Referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                _result = false;
                                oRecordSet.MoveLast();
                            }
                            else if ((((System.String)oRecordSet.Fields.Item("U_TipoDTE").Value) == "") && (((System.String)oRecordSet.Fields.Item("U_IndGlobal").Value).Trim() != "1"))
                            {
                                FSBOApp.StatusBar.SetText("Debe seleccionar Tipo Doc Referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                _result = false;
                                oRecordSet.MoveLast();
                            }
                            else if ((((System.Int32)oRecordSet.Fields.Item("U_DocFolio").Value) == 0) && (((System.String)oRecordSet.Fields.Item("U_IndGlobal").Value).Trim() != "1"))
                            {
                                FSBOApp.StatusBar.SetText("Debe ingresar Folio de documento referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                _result = false;
                                oRecordSet.MoveLast();
                            }
                            else
                            {
                                if ((((System.String)oRecordSet.Fields.Item("U_IndGlobal").Value).Trim() == "1") && (((System.String)oRecordSet.Fields.Item("TipoDTE_E").Value).Trim() == "00"))
                                {
                                    FSBOApp.StatusBar.SetText("Debe seleccinar tipo documento referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    _result = false;
                                }
                                else if (((System.String)oRecordSet.Fields.Item("U_IndGlobal").Value).Trim() != "1")
                                {
                                    s = ((System.String)oRecordSet.Fields.Item("U_TipoDTE").Value).Trim();
                                    if (s.IndexOf("b") == -1)
                                    {
                                        if ((((System.String)oRecordSet.Fields.Item("U_TipoDTE").Value).Trim() == "61") || (((System.String)oRecordSet.Fields.Item("U_TipoDTE").Value).Trim() == "60"))
                                        { Tabla = "ORIN"; }
                                        else
                                        { Tabla = "OINV"; }

                                        if (GlobalSettings.RunningUnderSQLServer)
                                        {
                                            s = @"SELECT COUNT(*) 'Cont'
                                            FROM {0} T1 WITH (NOLOCK)
                                            JOIN NNM1 T2 WITH (NOLOCK) ON T1.Series = T2.Series
                                           WHERE ISNULL(T1.FolioNum, -1) = {1}
                                             AND CASE 
                                                   WHEN '{2}' = '33'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                            	                   WHEN '{2}' = '39'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'IB'
                            	                   WHEN '{2}' = '41'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'BE'
                            	                   WHEN '{2}' = '110' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'IX'
                            	                   WHEN '{2}' = '34'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'IE'
                            	                   WHEN '{2}' = '56'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'DN'
                                                   WHEN '{2}' = '61'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                                                   WHEN '{2}' = '111' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'DN'
                                                   WHEN '{2}' = '112' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                                                   WHEN '{2}' = '30'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                                                   WHEN '{2}' = '32'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'IE'
                                                   WHEN '{2}' = '55'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'DN'
                                                   WHEN '{2}' = '60'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                                                   WHEN '{2}' = '101' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'IX'
                                                   WHEN '{2}' = '104' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'DN'
                                                   WHEN '{2}' = '106' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                            	                   Else '-1'
                                                 END = T1.DocSubType";
                                        }
                                        else
                                        {
                                            s = @"SELECT COUNT(*) ""Cont""
                                            FROM ""{0}"" T1
                                            JOIN ""NNM1"" T2 ON T1.""Series"" = T2.""Series""
                                           WHERE IFNULL(T1.""FolioNum"", -1) = {1}
                                             AND CASE 
                                                   WHEN '{2}' = '33'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--' 
                            	                   WHEN '{2}' = '39'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'IB'
                            	                   WHEN '{2}' = '41'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'BE' 
                            	                   WHEN '{2}' = '110' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'IX'
                            	                   WHEN '{2}' = '34'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'IE'
                            	                   WHEN '{2}' = '56'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'DN'
                                                   WHEN '{2}' = '61'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                                                   WHEN '{2}' = '111' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'DN'
                                                   WHEN '{2}' = '112' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                                                   WHEN '{2}' = '30'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                                                   WHEN '{2}' = '32'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'IE'
                                                   WHEN '{2}' = '55'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'DN'
                                                   WHEN '{2}' = '60'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                                                   WHEN '{2}' = '101' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'IX'
                                                   WHEN '{2}' = '104' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'DN'
                                                   WHEN '{2}' = '106' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                            	                   Else '-1' 
                                                 END = T1.""DocSubType"" ";
                                        }
                                        s = String.Format(s, Tabla, ((System.Int32)oRecordSet.Fields.Item("U_DocFolio").Value), ((System.String)oRecordSet.Fields.Item("U_TipoDTE").Value).Trim());
                                        oRecordSet.DoQuery(s);

                                        if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                                        { _result = true; }
                                        else
                                        {
                                            FSBOApp.StatusBar.SetText("No se ha encontrado documento de referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                            _result = false;
                                            oRecordSet.MoveLast();
                                        }
                                    }
                                }
                            }
                            oRecordSet.MoveNext();
                        }//fin while
                    }
                }
                else
                {
                    if ((ObjType == "67" || ObjType =="1250000001")) 
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"SELECT City, MailCity,County, MailCounty, Address, MailAddres
                                    FROM OCRD WITH (NOLOCK)
                                   WHERE CardCode = '{0}'";
                        }
                        else
                        {
                            s = @"SELECT ""City"", ""MailCity"", ""County"", ""MailCounty"", ""Address"", ""MailAddres""
                                FROM ""OCRD""
                               WHERE ""CardCode"" = '{0}' ";
                        }
                        s = String.Format(s, (System.String)(oDBDSH.GetValue("CardCode", 0)).Trim());
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount == 0)
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar Cliente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oRecordSet.Fields.Item("City").Value) == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oRecordSet.Fields.Item("MailCity").Value) == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oRecordSet.Fields.Item("County").Value) == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oRecordSet.Fields.Item("MailCounty").Value) == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oRecordSet.Fields.Item("Address").Value) == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oRecordSet.Fields.Item("MailAddres").Value) == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }
                    else
                    {
                        if (((System.String)(oDBDSDir.GetValue("CityB", 0)).Trim() == "") && (TipoDocElec != "39") && (TipoDocElec != "41"))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        if (((System.String)(oDBDSDir.GetValue("CityS", 0)).Trim() == "") && (_result) && (TipoDocElec != "39") && (TipoDocElec != "41"))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        if (((System.String)(oDBDSDir.GetValue("CountyB", 0)).Trim() == "") && (_result) && (TipoDocElec != "39") && (TipoDocElec != "41"))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        if (((System.String)(oDBDSDir.GetValue("CountyS", 0)).Trim() == "") && (_result) && (TipoDocElec != "39") && (TipoDocElec != "41"))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        if (((System.String)(oDBDSDir.GetValue("StreetB", 0)).Trim() == "") && (_result) && (TipoDocElec != "39") && (TipoDocElec != "41"))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        if (((System.String)(oDBDSDir.GetValue("StreetS", 0)).Trim() == "") && (_result) && (TipoDocElec != "39") && (TipoDocElec != "41"))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }


                    //Validar que ingresaron Nombre Cliente
                    s = (System.String)(oDBDSH.GetValue("CardName", 0)).Trim();
                    if ((s == "") && (_result))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar Nombre Cliente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    if (((TipoDocElec == "110") || (TipoDocElec == "111")) && (_result))
                    {
                        s = (System.String)(oDBDSH.GetValue("U_CodModVenta", 0)).Trim();
                        if (s == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe seleccionar Codigo Modo Venta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        var CardCode = (System.String)(oDBDSH.GetValue("CardCode", 0)).Trim();
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT COUNT(*) 'Conta' FROM OCRD WHERE CardCode = '{0}' AND U_FE_Export = 'Y'";

                        else
                            s = @"SELECT COUNT(*) ""Conta"" FROM ""OCRD"" WHERE ""CardCode"" = '{0}' AND ""U_FE_Export"" = 'Y' ";
                        s = String.Format(s, CardCode);
                        oRecordSet.DoQuery(s);
                        if (((System.Int32)oRecordSet.Fields.Item("Conta").Value) == 0)
                        {
                            FSBOApp.StatusBar.SetText("Cliente no puede generar Factura Electronica de Exportacion, revisar Maestro Socio Negocio campo Cliente Exportacion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }

                    //validar caracteres invalidos en el nombre del cliente
                    //se comenta segun reunion de viernes 20150320, se creo una funcion que limpia lo caracteres invalidos al momento de enviar al portal
                    //if (_result)
                    //{
                    //    foreach (String cara in CaracteresInvalidos)
                    //    {
                    //        if (s.IndexOf(cara) > 0)
                    //        {
                    //            FSBOApp.StatusBar.SetText(@"Nombre Cliente tiene caracteres prohibidos (" + cara + ")", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    //            _result = false;
                    //            break;
                    //        }
                    //    }
                    //}

                    //valida rut
                    if ((_result) && ((TipoDocElec != "110") && (TipoDocElec != "111") && (TipoDocElec != "112")))
                    {
                        Param = new TFunctions();
                        Param.SBO_f = FSBOf;
                        if (ObjType != "67" && ObjType != "1250000001")
                        { s = Param.ValidarRut((System.String)(oDBDSH.GetValue("LicTradNum", 0))); }
                        else
                        {
                            if (GlobalSettings.RunningUnderSQLServer)
                            {
                                s = @"SELECT LicTradNum
                                        FROM OCRD WITH (NOLOCK)
                                       WHERE CardCode = '{0}'";
                            }
                            else
                            {
                                s = @"SELECT ""LicTradNum""
                                    FROM ""OCRD""
                                   WHERE ""CardCode"" = '{0}' ";
                            }
                            s = String.Format(s, (System.String)(oDBDSH.GetValue("CardCode", 0)).Trim());
                            oRecordSet.DoQuery(s);
                            if (oRecordSet.RecordCount == 0)
                                s = "No se ha encontrado cliente";
                            else if ((System.String)(oRecordSet.Fields.Item("LicTradNum").Value) == "")
                                s = "No se ha encontrado RUT del cliente";
                            else
                                s = Param.ValidarRut((System.String)(oRecordSet.Fields.Item("LicTradNum").Value));
                        }

                        if (s != "OK")
                        {
                            FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }

                    //valida descuentos negativos en el detalle del documento, caracteres especiales y descripcion de articulo
                    if (_result)
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = "select ISNULL(U_ValDescL,'Y') 'ValDescL' from [@VID_FEPARAM]";
                        else
                            s = @"select IFNULL(""U_ValDescL"",'Y') ""ValDescL"" from ""@VID_FEPARAM"" ";

                        oRecordSet.DoQuery(s);
                        while (!oRecordSet.EoF)
                        {
                            i = 0;
                            while (i < oDBDSDet.Size)
                            {
                                if ((System.String)(oRecordSet.Fields.Item("ValDescL").Value) == "Y")
                                {
                                    if (Convert.ToDouble(((System.String)oDBDSDet.GetValue("DiscPrcnt", i)).Replace(",", "."), _nf) < 0)
                                    {
                                        s = "Descuento negativo en la linea " + Convert.ToString(i);
                                        FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        _result = false;
                                        i = oDBDSDet.Size;
                                    }
                                }

                                if (_result)
                                {
                                    s = (System.String)(oDBDSDet.GetValue("Dscription", i));
                                    if (s == "")
                                    {
                                        FSBOApp.StatusBar.SetText("Debe ingresar descripción en la linea " + Convert.ToString(i), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        _result = false;
                                        i = oDBDSDet.Size;
                                    }

                                    //se comenta segun reunion de viernes 20150320, se creo una funcion que limpia lo caracteres invalidos al momento de enviar al portal
                                    //if (_result)
                                    //{
                                    //    foreach (String cara in CaracteresInvalidos)
                                    //    {
                                    //        if (s.IndexOf(cara) > 0)
                                    //        {
                                    //            FSBOApp.StatusBar.SetText(@"Descripción tiene caracteres prohibidos (" + cara + "), linea " + i.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    //            _result = false;
                                    //            break;
                                    //        }
                                    //    }
                                    //}
                                }

                                //Para Facturas Exentas, validar que usen impuesto Exento
                                if (((TipoDocElec == "34") || (TipoDocElec == "110") || (TipoDocElec == "111")) && (_result))
                                {
                                    var vatsum = Convert.ToDouble(((System.String)oDBDSDet.GetValue("VatSum", i)).Replace(",", "."), _nf);
                                    if (vatsum != 0)
                                    {
                                        FSBOApp.StatusBar.SetText("Existe lineas con impuesto, el documento que esta generando es exento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        _result = false;
                                    }
                                }

                                i++;
                            }
                            oRecordSet.MoveNext();
                        }
                    }

                    //valida total de lineas en el documento
                    if (_result)
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = "select ISNULL(U_CantLineas,0) CantLineas from [@VID_FEPROCED] where U_TipoDoc = '" + TipoDocElec + "' and U_Habili = 'Y'";
                        else
                            s = @"select IFNULL(""U_CantLineas"",0) ""CantLineas"" from ""@VID_FEPROCED"" where ""U_TipoDoc"" = '" + TipoDocElec + @"' and ""U_Habili"" = 'Y'";
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount > 0)
                        {
                            canttotal = (System.Int32)(oRecordSet.Fields.Item("CantLineas").Value);

                        }
                        else
                        {
                            FSBOApp.StatusBar.SetText("Debe parametrizar el maximo de lineas para documento " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }


                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            if (GlobalSettings.RunningUnderSQLServer)
                                s = @"SELECT COUNT(*) CONT
                                      FROM {1} T1
                                      LEFT JOIN OITT T2 ON T2.Code = T1.ItemCode
                                     WHERE T1.DocEntry = {0}
                                       AND CASE T1.TreeType  
                                             WHEN 'I' THEN ISNULL((SELECT TOP 1 C.HideComp FROM {1} B JOIN OITT C ON C.Code = B.ItemCode WHERE B.VisOrder < T1.VisOrder AND B.DocEntry = T1.DocEntry AND B.TreeType <> 'I' ORDER BY B.VisOrder DESC),'N')
                                             ELSE 'N' 
	                                       END = 'N'";
                            else
                                s = @"SELECT COUNT(*) ""CONT""
                                      FROM ""{1}"" T1
                                      LEFT JOIN ""OITT"" T2 ON T2.""Code"" = T1.""ItemCode""
                                     WHERE T1.""DocEntry"" = {0}
                                       AND CASE T1.""TreeType""
                                             WHEN 'I' THEN IFNULL((SELECT TOP 1 C.""HideComp"" FROM ""{1}"" B JOIN ""OITT"" C ON C.""Code"" = B.""ItemCode"" WHERE B.""VisOrder"" < T1.""VisOrder"" AND B.""DocEntry"" = T1.""DocEntry"" AND B.""TreeType"" <> 'I' ORDER BY B.""VisOrder"" DESC),'N')
                                             ELSE 'N' 
	                                       END = 'N'";
                            if (ObjType == "13")
                                s = String.Format(s, (System.String)(oDBDSH.GetValue("DocEntry", 0)), "INV1");
                            else if (ObjType == "203")
                                s = String.Format(s, (System.String)(oDBDSH.GetValue("DocEntry", 0)), "DPI1");
                            else if (ObjType == "14")
                                s = String.Format(s, (System.String)(oDBDSH.GetValue("DocEntry", 0)), "RIN1");
                            else if (ObjType == "15")
                                s = String.Format(s, (System.String)(oDBDSH.GetValue("DocEntry", 0)), "DLN1");
                            else if (ObjType == "21")
                                s = String.Format(s, (System.String)(oDBDSH.GetValue("DocEntry", 0)), "RPD1");
                            else if (ObjType == "67")
                                s = String.Format(s, (System.String)(oDBDSH.GetValue("DocEntry", 0)), "WTR1");
                            else if (ObjType == "1250000001")
                                s = String.Format(s, (System.String)(oDBDSH.GetValue("DocEntry", 0)), "WTQ1");
                            else if (ObjType == "18")
                                s = String.Format(s, (System.String)(oDBDSH.GetValue("DocEntry", 0)), "PCH1");
                            else if (ObjType == "19")
                                s = String.Format(s, (System.String)(oDBDSH.GetValue("DocEntry", 0)), "RPC1");
                            else if (ObjType == "204")
                                s = String.Format(s, (System.String)(oDBDSH.GetValue("DocEntry", 0)), "DPO1");
                            oRecordSet.DoQuery(s);
                            if (oRecordSet.RecordCount > 0)
                            {
                                var cantlin = ((System.Int32)oRecordSet.Fields.Item("CONT").Value);
                                if (cantlin > canttotal)
                                {
                                    FSBOApp.StatusBar.SetText("Cantidad lineas supera a lo parametrizado en FE", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    _result = false;
                                }
                            }
                        }

                    }
                }
                ors = null;
                return _result;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("ValidarDatos: " + e.Message + " ** Trace: " + e.StackTrace);
                ors = null;
                return false;
            }
        }

        private Boolean Imprimir()
        {
            String sDocEntry = "";
            String sFolio = "";
            String sDocNum = "";
            String Tabla = "";
            String sTipo = "";
            String sDocSubType = "";
            String nMultiSoc = "";
            String ObjType = "";
            String GLOB_EncryptSQL;
            String TipoDocElect = "";
            SAPbouiCOM.EditText oEditText;
            SAPbouiCOM.ComboBox oComboBox;
            String[] FE52 = { "52", "52T", "52D" , "52S" };
            String[] FEOt = { "33", "33A", "34", "39", "41", "56", "110", "110R", "111" };
            SAPbobsCOM.Documents oDocumento;

            try
            {
                oForm.Freeze(true);
                GLOB_EncryptSQL = GlobalSettings.GLOB_EncryptSQL;
                oEditText = (EditText)(oForm.Items.Item("FolioNum").Specific);
                sFolio = oEditText.Value;
                oEditText = (EditText)(oForm.Items.Item("DocNum").Specific);
                sDocNum = oEditText.Value;
                oComboBox = (ComboBox)(oForm.Items.Item("TipDoc").Specific);
                sTipo = oComboBox.Value;
                sDocSubType = "";

                if (sTipo == "33") //Factura venta
                {
                    Tabla = "OINV";
                    sDocSubType = "--";
                    ObjType = "13";
                    TipoDocElect = "33";
                }
                else if (sTipo == "33A") //Factura Anticipo venta
                {
                    Tabla = "ODPI";
                    sDocSubType = "--";
                    ObjType = "203";
                    TipoDocElect = "33";
                }
                else if (sTipo == "46") //Factura compra terceros
                {
                    Tabla = "OPCH";
                    sDocSubType = "--";
                    ObjType = "18";
                    TipoDocElect = "46";
                }
                else if (sTipo == "46A") //Factura Anticipo Compra a terceros
                {
                    Tabla = "ODPO";
                    sDocSubType = "--";
                    ObjType = "204";
                    TipoDocElect = "46";
                }
                else if (sTipo == "34") //Factura Exenta
                {
                    Tabla = "OINV";
                    sDocSubType = "IE";
                    ObjType = "13";
                    TipoDocElect = "34";
                }
                else if (sTipo == "39") //Boleta
                {
                    Tabla = "OINV";
                    sDocSubType = "IB";
                    ObjType = "13";
                    TipoDocElect = "39";
                }
                else if (sTipo == "41") //Boleta exenta
                {
                    Tabla = "OINV";
                    sDocSubType = "EB";
                    ObjType = "13";
                    TipoDocElect = "41";
                }
                else if (sTipo == "43") //liquidacion Factura
                {
                    Tabla = "ORIN";
                    sDocSubType = "--";
                    ObjType = "14";
                    TipoDocElect = "43";
                }
                else if (sTipo == "43N") //liquidacion Factura por Nota de debito
                {
                    Tabla = "OINV";
                    sDocSubType = "DN";
                    ObjType = "13";
                    TipoDocElect = "43";
                }
                else if (sTipo == "46") //Factura de compra
                {
                    Tabla = "OPCH";
                    sDocSubType = "--";
                    ObjType = "18";
                    TipoDocElect = "46";
                }
                else if (sTipo == "56") //nota debito
                {
                    Tabla = "OINV";
                    sDocSubType = "DN";
                    ObjType = "13";
                    TipoDocElect = "56";
                }
                else if (sTipo == "61") //nota de credito
                {
                    Tabla = "ORIN";
                    sDocSubType = "--";
                    ObjType = "14";
                    TipoDocElect = "61";
                }
                else if (sTipo == "61C") //nota de credito Compra
                {
                    Tabla = "ORPC";
                    sDocSubType = "--";
                    ObjType = "19";
                    TipoDocElect = "61";
                }
                else if (sTipo == "52") //guia despacho por entrega
                {
                    Tabla = "ODLN";
                    sDocSubType = "--";
                    ObjType = "15";
                    TipoDocElect = "52";
                }
                else if (sTipo == "52T") //guia despacho por transferencia stock
                {
                    Tabla = "OWTR";
                    sDocSubType = "--";
                    ObjType = "67";
                    TipoDocElect = "52";
                }
                else if (sTipo == "52S") //guia despacho por transferencia stock
                {
                    Tabla = "OWTQ";
                    sDocSubType = "--";
                    ObjType = "1250000001";
                    TipoDocElect = "52";
                }
                else if (sTipo == "52D") //guia despacho por devolucion de mercancia Compra
                {
                    Tabla = "ORPD";
                    sDocSubType = "--";
                    ObjType = "21";
                    TipoDocElect = "52";
                }
                else if (sTipo == "110") //factura exportacion
                {
                    Tabla = "OINV";
                    sDocSubType = "IX";
                    ObjType = "13";
                    TipoDocElect = "110";
                }
                else if (sTipo == "110R") //factura exportacion por Reserva
                {
                    Tabla = "OINV";
                    sDocSubType = "--";
                    ObjType = "13";
                    TipoDocElect = "110";
                }
                else if (sTipo == "111") //Nota de Debito exportacion
                {
                    Tabla = "OINV";
                    sDocSubType = "DN";
                    ObjType = "13";
                    TipoDocElect = "111";
                }
                else if (sTipo == "112") //Nota de Credito exportacion
                {
                    Tabla = "ORIN";
                    sDocSubType = "--";
                    ObjType = "14";
                    TipoDocElect = "112";
                }


                if (GlobalSettings.RunningUnderSQLServer)
                {
                    s = @"SELECT CAST(T0.DocEntry AS VARCHAR(20)) 'DocEntry', T0.DocSubType, SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst
                            FROM {0} T0 
                            JOIN NNM1 T2 ON T0.Series = T2.Series 
                           WHERE T0.{7} = {1}
                             AND SUBSTRING(UPPER(T2.BeginStr), 1, 1) = 'E'
                             AND T0.DocSubType = '{2}'
                             {3}
                             {4}
                             {5}
                             {6}
                           ORDER BY T0.DocEntry DESC";
                }
                else
                {
                    s = @"SELECT CAST(T0.""DocEntry"" AS VARCHAR(20)) ""DocEntry"", T0.""DocSubType"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                            FROM ""{0}"" T0 
                            JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                           WHERE T0.""{7}"" = {1}
                             AND SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) = 'E'
                             AND T0.""DocSubType"" = '{2}'
                             {3}
                             {4}
                             {5}
                             {6}
                           ORDER BY T0.""DocEntry"" DESC";
                }
                s = String.Format(s, Tabla, (sFolio == "" ? sDocNum : sFolio), sDocSubType, (sTipo == "110R" ? (GlobalSettings.RunningUnderSQLServer ? " AND T0.isIns = 'Y' " : @" AND T0.""isIns"" = 'Y' ") : "")
                , (sTipo == "43" || sTipo == "43N" ? (GlobalSettings.RunningUnderSQLServer ? @" and SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) = '" + TipoDocElect + "'" : (sTipo == "43" || sTipo == "43N" ? @" and SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) = '" + sTipo + "'" : "")) : "")
                , (sTipo == "46" ? (GlobalSettings.RunningUnderSQLServer ? @" and SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) <> '43'" : (sTipo == "46" ? @" and SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) <> '43'" : "")) : "")
                , (sTipo != "43N" ? (GlobalSettings.RunningUnderSQLServer ? @" and SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) <> '43'" : (sTipo != "43N" ? @" and SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) <> '43'" : "")) : "")
                , (sFolio == "" ? "DocNum" : "FolioNum")
                    );
                oRecordSet.DoQuery(s);

                sDocEntry = (System.String)(oRecordSet.Fields.Item("DocEntry").Value);
                s = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                if ((bMultiSoc == true) && (nMultiSoc == ""))
                    FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                else
                {
                    //if (sTipo in ['33','33A','34','39','41','46','56','46','46A'])
                    if ((FEOt.Contains(sTipo)) || (sTipo == "43N"))
                    {
                        if (sTipo == "33A")
                            oDocumento = ((SAPbobsCOM.Documents)FCmpny.GetBusinessObject(BoObjectTypes.oDownPayments));
                        else
                            oDocumento = ((SAPbobsCOM.Documents)FCmpny.GetBusinessObject(BoObjectTypes.oInvoices));

                        var oInvoice = new TInvoice();
                        oInvoice.SBO_f = FSBOf;
                        if (oDocumento.GetByKey(Convert.ToInt32(sDocEntry)))//**se dejo la normal mientras se termina la modificacion en el portal 20170202
                            oInvoice.EnviarFE_WebService(ObjType, oDocumento, TipoDocElect, bMultiSoc, nMultiSoc, GlobalSettings.RunningUnderSQLServer, sDocSubType, sTipo, bFolioPortal);
                        else
                            FSBOApp.StatusBar.SetText("No se ha encontrado documento " + sTipo + " folio " + sFolio, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    }
                    else if ((sTipo == "46") || (sTipo == "46A"))
                    {
                        var oPurchaseInvoice = new TPurchaseInvoice();
                        oPurchaseInvoice.SBO_f = FSBOf;
                        oPurchaseInvoice.EnviarFE_WebService(sDocEntry, s, ObjType, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "46", sTipo, bFolioPortal);
                    }
                    else if ((sTipo == "61") || (sTipo == "61C") || (sTipo == "112") || (sTipo == "43"))
                    {
                        var oCreditNotes = new TCreditNotes();
                        oCreditNotes.SBO_f = FSBOf;
                        oCreditNotes.EnviarFE_WebServiceNotaCredito(sDocEntry, s, bMultiSoc, nMultiSoc, GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, ObjType, TipoDocElect, sTipo, bFolioPortal);
                    }
                    //else if (sTipo in ['52','52T','52D'])
                    else if (FE52.Contains(sTipo))
                    {
                        var oDeliveryNote = new TDeliveryNote();
                        oDeliveryNote.SBO_f = FSBOf;
                        if (sTipo == "52")
                            oDeliveryNote.EnviarFE_WebService(sDocEntry, s, false, true, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, sTipo, "15", bFolioPortal);
                        else if (sTipo == "52T")
                            oDeliveryNote.EnviarFE_WebService(sDocEntry, s, true, true, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, sTipo, "67", bFolioPortal);
                        else if (sTipo == "52S")
                            oDeliveryNote.EnviarFE_WebService(sDocEntry, s, true, true, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, sTipo, "1250000001", bFolioPortal);
                        else if (sTipo == "52D")
                            oDeliveryNote.EnviarFE_WebService(sDocEntry, s, false, true, true, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, sTipo, "21", bFolioPortal);
                    }
                }

                oForm.DataSources.UserDataSources.Item("FolioNum").Value = "";
                oForm.Freeze(false);
                return true;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("Imprimir: " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
            finally
            {
                oForm.Freeze(false);
            }
        }//fin Imprimir

    }//fin Class
}
