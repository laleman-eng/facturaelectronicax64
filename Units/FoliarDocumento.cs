using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using SAPbouiCOM;
using SAPbobsCOM;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using VisualD.untLog;
using FactRemota;
using System.Threading;
using System.Data;
using System.Xml;
using System.IO;
using System.Data.SqlClient;
using Factura_Electronica_VK.Functions;
using Factura_Electronica_VK.CreditNotes;
using Factura_Electronica_VK.DeliveryNote;
using Factura_Electronica_VK.Invoice;
using Factura_Electronica_VK.PurchaseInvoice;

namespace Factura_Electronica_VK.FoliarDocumento
{
    public class TFoliarDocumento : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbouiCOM.DataTable odt;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Grid ogrid;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.GridColumn oColumn;
        private SAPbouiCOM.EditText oEditText;
        private SAPbouiCOM.EditTextColumn oEditColumn;
        private SAPbouiCOM.ComboBox oComboBox;
        private Boolean Distribuido = false;
        private Boolean bFolioPortal = false;
        private Boolean Timbre = false;
        private String tabla;
        private String tablaDetalle;
        private String sDocSubType;
        private String ObjType;
        private String TipoDocElect;
        private TFunctions Reg;
        private Boolean bMultiSoc = false;
        //oItem       : SAPbouiCOM.Item;
        //oColumn     : SAPBouiCOM.Column;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            //SAPbouiCOM.ComboBox oCombo;
            //Int32 i;
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                Lista = new List<string>();
                Reg = new TFunctions();
                Reg.SBO_f = FSBOf;

                if (GlobalSettings.RunningUnderSQLServer)
                    s = "select ISNULL(U_MultiSoc,'N') MultiSoc from [@VID_FEPARAM] where Code = '1'";
                else
                    s = @"select IFNULL(""U_MultiSoc"",'N') ""MultiSoc"" from ""@VID_FEPARAM"" where ""Code"" = '1' ";

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                    throw new Exception("Debe parametrizar el Addon Factura Electronica");
                else
                {
                    if (((System.String)oRecordSet.Fields.Item("MultiSoc").Value).Trim() == "Y")
                        bMultiSoc = true;
                    else
                        bMultiSoc = false;
                }

                if (bMultiSoc == true)
                    FSBOApp.StatusBar.SetText("Foliar Documentos no esta habilitado cuando el addon esta parametrizado con Multiples bases", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                else
                {
                    FSBOf.LoadForm(xmlPath, "VID_FEFOLIAR.srf", uid);
                    //EnableCrystal := true;
                    oForm = FSBOApp.Forms.Item(uid);
                    oForm.Freeze(true);
                    oForm.AutoManaged = true;
                    oForm.SupportedModes = -1;             // afm_All
                    oForm.EnableMenu("1282", false); //Crear
                    oForm.EnableMenu("1281", false); //Actualizar

                    VID_DelRow = false;
                    VID_DelRowOK = false;

                    Reg = new TFunctions();
                    Reg.SBO_f = FSBOf;
                    // Ok Ad  Fnd Vw Rq Sec
                    //Lista.Add("DocEntry  , f,  f,  t,  f, r, 1");
                    //Lista.Add("Desde     , f,  f,  f,  f, r, 1");
                    //FSBOf.SetAutoManaged(ref oForm, Lista);

                    oComboBox = (ComboBox)(oForm.Items.Item("TipoDoc").Specific);
                    oComboBox.ValidValues.Add("33", "Factura Electronica");
                    oComboBox.ValidValues.Add("33A", "Factura por Anticipo");
                    oComboBox.ValidValues.Add("34", "Factura Exenta");
                    oComboBox.ValidValues.Add("39", "Boleta");
                    oComboBox.ValidValues.Add("41", "Boleta Exenta");
                    oComboBox.ValidValues.Add("43", "Liquidacion Factura");
                    oComboBox.ValidValues.Add("46", "Factura de Compra a terceros");
                    oComboBox.ValidValues.Add("46A", "Factura Anticipo Compra a terceros");
                    oComboBox.ValidValues.Add("52", "Guia Despacho");
                    oComboBox.ValidValues.Add("52T", "Guia Despacho por Transferencia Stock");
                    oComboBox.ValidValues.Add("52D", "Guia Despacho por Devolución Compra");
                    oComboBox.ValidValues.Add("56", "Nota Debito");
                    oComboBox.ValidValues.Add("61", "Nota de Credito");
                    oComboBox.ValidValues.Add("61C", "Nota de Credito Compra");
                    oComboBox.ValidValues.Add("110", "Factura Exportación Electronica");
                    oComboBox.ValidValues.Add("110R", "Factura Exportación Elect. por Reserva");
                    oComboBox.ValidValues.Add("111", "Nota de Debito Export. Electronica");
                    oComboBox.ValidValues.Add("112", "Nota de Credito Export. Electronica");



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
                            FSBOApp.StatusBar.SetText("Los datos de acceso al servidor SQL no son validos (Gestion->Definiciones->Factura Electrónica->Configuración Conexión), guarde los datos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
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
                                FSBOApp.StatusBar.SetText("Los datos de acceso al servidor SQL no son validos (Gestion->Definiciones->Factura Electrónica->Configuración Conexión), guarde los datos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                FSBOApp.ActivateMenuItem("VID_RHSQL");
                                return false;
                            }
                            ConexionADO.Close();
                        }*/
                    }


                    oForm.DataSources.UserDataSources.Add("Desde", BoDataType.dt_DATE, 10);
                    oEditText = (EditText)(oForm.Items.Item("Desde").Specific);
                    oEditText.DataBind.SetBound(true, "", "Desde");
                    oEditText.Value = DateTime.Now.ToString("yyyyMMdd");

                    oForm.DataSources.UserDataSources.Add("Hasta", BoDataType.dt_DATE, 10);
                    oEditText = (EditText)(oForm.Items.Item("Hasta").Specific);
                    oEditText.DataBind.SetBound(true, "", "Hasta");
                    oEditText.Value = DateTime.Now.ToString("yyyyMMdd");

                    odt = oForm.DataSources.DataTables.Add("dt");
                    ogrid = (Grid)(oForm.Items.Item("grid").Specific);
                    ogrid.DataTable = odt;

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = "select ISNULL(U_Distrib,'N') 'Distribuido', ISNULL(U_FPortal,'N') 'FolioPortal', ISNULL(U_MultiSoc,'N') MultiSoc, ISNULL(U_GenerarT,'N') GeneraT from [@VID_FEPARAM] WITH (NOLOCK)";
                    else
                        s = @"select IFNULL(""U_Distrib"",'N') ""Distribuido"", IFNULL(""U_FPortal"",'N') ""FolioPortal"", IFNULL(""U_MultiSoc"",'N') ""MultiSoc"", IFNULL(""U_GenerarT"",'N') ""GeneraT"" from ""@VID_FEPARAM"" ";
                    oRecordSet.DoQuery(s);

                    if (oRecordSet.RecordCount > 0)
                    {
                        if (((System.String)oRecordSet.Fields.Item("Distribuido").Value).Trim() != "Y")
                        {
                            FSBOApp.StatusBar.SetText("Debe parametrizar el addon para generar folio distribuido o Manejo folio en el Portal", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            oForm.Close();
                        }

                        if ((System.String)(oRecordSet.Fields.Item("MultiSoc").Value) == "Y")
                        {
                            FSBOApp.StatusBar.SetText("El addon se encuentra parametrizado como Multiples Sociedades", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            oForm.Close();
                        }

                        if (((System.String)oRecordSet.Fields.Item("Distribuido").Value).Trim() == "Y")
                            Distribuido = true;

                        if (((System.String)oRecordSet.Fields.Item("GeneraT").Value).Trim() == "Y")
                            Timbre = true;

                        bFolioPortal = (((System.String)oRecordSet.Fields.Item("FolioPortal").Value).Trim() == "Y");

                    }
                    else
                    {
                        FSBOApp.StatusBar.SetText("Debe parametrizar el addon para generar folio distribuido o Manejo Folio Portal, y el timbre (2)", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        oForm.Close();
                    }
                }

            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            oForm.Freeze(false);

            return Result;
        }//fin initForm


        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            String TipoDoc;
            String sDesde;
            String sHasta;
            Int32 nErr;
            String sErr;

            //inherited FormEvent(FormUID,Var pVal,Var BubbleEvent);
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);

            try
            {
                if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED && !pVal.BeforeAction)
                {
                    if (pVal.ItemUID == "Buscar")
                    {
                        //s = Reg.PDFenString("39", "4854", "13", "", "1", false, "CL");

                        oEditText = (EditText)(oForm.Items.Item("Desde").Specific);
                        sDesde = oEditText.Value;
                        oEditText = (EditText)(oForm.Items.Item("Hasta").Specific);
                        sHasta = oEditText.Value;
                        oComboBox = (ComboBox)(oForm.Items.Item("TipoDoc").Specific);
                        TipoDoc = oComboBox.Value;

                        if (TipoDoc == "")
                            FSBOApp.StatusBar.SetText("Debe seleccionar Tipo Documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        else if (sDesde == "")
                            FSBOApp.StatusBar.SetText("Debe ingresar fecha Desde", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        else if (sHasta == "")
                            FSBOApp.StatusBar.SetText("Debe ingresar fecha Hasta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        else
                            BuscarDoc(TipoDoc, sDesde, sHasta);
                    }

                    if (pVal.ItemUID == "Selecciona")
                        Seleccionar("Y");

                    if (pVal.ItemUID == "BorrarSel")
                        Seleccionar("N");

                    if (pVal.ItemUID == "Foliar")
                        FoliarDoctos();
                        //CrearPDF();
                }

            }
            catch (Exception e)
            {
                FCmpny.GetLastError(out nErr, out sErr);
                FSBOApp.StatusBar.SetText("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }

        }//fin FormEvent

        public void MenuEvent(ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
            //Reg : TRemuneraciones_MyFunctions;
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

                if (pVal.MenuUID != "" && pVal.BeforeAction == false)
                    if ((pVal.MenuUID == "1288") || (pVal.MenuUID == "1289") || (pVal.MenuUID == "1290") || (pVal.MenuUID == "1291"))
                    { }

                if (pVal.MenuUID == "1282")
                { }

                //inherited MenuEvent(Var pVal,var BubbleEvent);

            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent


        private void BuscarDoc(String TipoDoc, String sDesde, String sHasta)
        {
            String st;
            tabla = "";
            tablaDetalle = "";
            sDocSubType = "";
            ObjType = "";

            oForm.Freeze(true);
            try
            {

                if (TipoDoc == "33") //Factura venta
                {
                    tabla = "OINV";
                    tablaDetalle = "INV1";
                    sDocSubType = "--";
                    ObjType = "13";
                    TipoDocElect = "33";
                }
                else if (TipoDoc == "33A") //Factura Anticipo
                {
                    tabla = "ODPI";
                    tablaDetalle = "DPI1";
                    sDocSubType = "--";
                    ObjType = "203";
                    TipoDocElect = "33";
                }
                else if (TipoDoc == "34") //Factura Exenta
                {
                    tabla = "OINV";
                    tablaDetalle = "INV1";
                    sDocSubType = "IE";
                    ObjType = "13";
                    TipoDocElect = "34";
                }
                else if (TipoDoc == "39") //Boleta
                {
                    tabla = "OINV";
                    tablaDetalle = "INV1";
                    sDocSubType = "IB";
                    ObjType = "13";
                    TipoDocElect = "39";
                }
                else if (TipoDoc == "41") //Boleta exenta
                {
                    tabla = "OINV";
                    tablaDetalle = "INV1";
                    sDocSubType = "EB";
                    ObjType = "13";
                    TipoDocElect = "41";
                }
                else if (TipoDoc == "43") //liquidacion factura
                {
                    tabla = "ORIN";
                    tablaDetalle = "RIN1";
                    sDocSubType = "--";
                    ObjType = "14";
                    TipoDocElect = "43";
                }
                else if (TipoDoc == "46") //Factura compra
                {
                    tabla = "OPCH";
                    tablaDetalle = "PCH1";
                    sDocSubType = "--";
                    ObjType = "18";
                    TipoDocElect = "46";
                }
                else if (TipoDoc == "46A") //Factura Anticipo compra
                {
                    tabla = "ODPO";
                    tablaDetalle = "DPO1";
                    sDocSubType = "--";
                    ObjType = "204";
                    TipoDocElect = "46";
                }
                else if (TipoDoc == "56") //nota debito
                {
                    tabla = "OINV";
                    tablaDetalle = "INV1";
                    sDocSubType = "DN";
                    ObjType = "13";
                    TipoDocElect = "56";
                }
                else if (TipoDoc == "61") //nota de credito
                {
                    tabla = "ORIN";
                    tablaDetalle = "RIN1";
                    sDocSubType = "--";
                    ObjType = "14";
                    TipoDocElect = "61";
                }
                else if (TipoDoc == "61C") //nota de credito Compra
                {
                    tabla = "ORPC";
                    tablaDetalle = "RPC1";
                    sDocSubType = "--";
                    ObjType = "19";
                    TipoDocElect = "61";
                }
                else if (TipoDoc == "52") //guia despacho por entrega
                {
                    tabla = "ODLN";
                    tablaDetalle = "DLN1";
                    sDocSubType = "--";
                    ObjType = "15";
                    TipoDocElect = "52";
                }
                else if (TipoDoc == "52T") //guia despacho por transferencia stock
                {
                    tabla = "OWTR";
                    tablaDetalle = "WTR1";
                    sDocSubType = "--";
                    ObjType = "67";
                    TipoDocElect = "52";
                }
                else if (TipoDoc == "52D") //guia despacho por devolucion mercancia en Compras
                {
                    tabla = "ORPD";
                    tablaDetalle = "RPD1";
                    sDocSubType = "--";
                    ObjType = "21";
                    TipoDocElect = "52";
                }
                else if (TipoDoc == "110") //factura exportacion
                {
                    tabla = "OINV";
                    tablaDetalle = "INV1";
                    sDocSubType = "IX";
                    ObjType = "13";
                    TipoDocElect = "110";
                }
                else if (TipoDoc == "110R") //factura exportacion por reserva
                {
                    tabla = "OINV";
                    tablaDetalle = "INV1";
                    sDocSubType = "--";
                    ObjType = "13";
                    TipoDocElect = "110";
                }
                else if (TipoDoc == "111") //nota debito exportacion
                {
                    tabla = "OINV";
                    tablaDetalle = "INV1";
                    sDocSubType = "DN";
                    ObjType = "13";
                    TipoDocElect = "111";
                }
                else if (TipoDoc == "112") //nota de credito exportacion
                {
                    tabla = "ORIN";
                    tablaDetalle = "RIN1";
                    sDocSubType = "--";
                    ObjType = "14";
                    TipoDocElect = "112";
                }

                if ((TipoDocElect == "111") || (TipoDocElect == "112") || (TipoDocElect == "43"))
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        st = " AND SUBSTRING(ISNULL(N1.BeginStr,''), 2, LEN(N1.BeginStr)) =  '{0}'";
                    else
                        st = @" AND SUBSTRING(IFNULL(N1.""BeginStr"",''), 2, LENGTH(N1.""BeginStr"")) = '{0}'";
                    st = String.Format(st, TipoDocElect);
                }
                else
                    st = "";

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT 'Y' 'Sel', T0.DocEntry, T0.DocNum, T0.DocDate, T0.CardCode, T0.CardName, T0.DocTotal
                          FROM {0} T0
                          JOIN NNM1 N1 ON N1.Series = T0.Series
                                      --AND N1.ObjectCode = T0.ObjType
                         WHERE T0.DocSubType = '{1}'
                           AND T0.DocDate BETWEEN '{2}' AND '{3}'
                           AND UPPER(LEFT(ISNULL(N1.BeginStr,''),1)) = 'E'
                           AND ISNULL(T0.FolioNum, 0) = 0
                           AND T0.CANCELED = 'N'
                           {4}
                           {5}
                        ";
                else
                    s = @"SELECT 'Y' ""Sel"", T0.""DocEntry"", T0.""DocNum"", T0.""DocDate"", T0.""CardCode"", T0.""CardName"", T0.""DocTotal""
                          FROM ""{0}"" T0
                          JOIN ""NNM1"" N1 ON N1.""Series"" = T0.""Series""
                                      --AND N1.""ObjectCode"" = T0.""ObjType""
                         WHERE T0.""DocSubType"" = '{1}'
                           AND T0.""DocDate"" BETWEEN '{2}' AND '{3}'
                           AND UPPER(LEFT(IFNULL(N1.""BeginStr"",''),1)) = 'E'
                           AND IFNULL(T0.""FolioNum"", 0) = 0
                           AND T0.""CANCELED"" = 'N'
                           {4}
                           {5}
                        ";

                s = String.Format(s, tabla, sDocSubType, sDesde, sHasta, st, (TipoDoc == "110R" ? (GlobalSettings.RunningUnderSQLServer ? @" AND T0.isIns = 'Y' ": @" AND T0.""isIns"" = 'Y' "): ""));

                odt.ExecuteQuery(s);

                ogrid.Columns.Item("Sel").Type = BoGridColumnType.gct_CheckBox;
                oColumn = (GridColumn)(ogrid.Columns.Item("Sel"));
                oColumn.TitleObject.Caption = "Selecionar";
                oColumn.Editable = true;

                ogrid.Columns.Item("DocEntry").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("DocEntry"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Llave SAP";
                oEditColumn.LinkedObjectType = ObjType;
                oEditColumn.RightJustified = true;

                ogrid.Columns.Item("DocNum").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("DocNum"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Número SAP";
                oEditColumn.RightJustified = true;

                ogrid.Columns.Item("DocDate").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("DocDate"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Fecha Contable";
                oEditColumn.RightJustified = false;

                ogrid.Columns.Item("CardCode").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("CardCode"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Código SN";
                oEditColumn.LinkedObjectType = "2";

                ogrid.Columns.Item("CardName").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("CardName"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Nombre SN";

                ogrid.Columns.Item("DocTotal").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("DocTotal"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Total Documento";
                oEditColumn.RightJustified = true;

                ogrid.AutoResizeColumns();

                if (odt.Rows.Count > 0)
                {
                    var valor = Convert.ToString(odt.GetValue("DocEntry", 0));
                    if (valor != "0")
                    {
                        oForm.Items.Item("Selecciona").Enabled = true;
                        oForm.Items.Item("BorrarSel").Enabled = true;
                        oForm.Items.Item("Foliar").Enabled = true;
                    }
                    else
                    {
                        oForm.Items.Item("Selecciona").Enabled = false;
                        oForm.Items.Item("BorrarSel").Enabled = false;
                        oForm.Items.Item("Foliar").Enabled = false;
                    }
                }
                else
                {
                    oForm.Items.Item("Selecciona").Enabled = false;
                    oForm.Items.Item("BorrarSel").Enabled = false;
                    oForm.Items.Item("Foliar").Enabled = false;
                }
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText("BuscarDoc: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("BuscarDoc: " + e.Message + " ** Trace: " + e.StackTrace);
            }
            finally
            {
                oForm.Freeze(false);
            }
        }


        private void FoliarDoctos()
        {
            String sDocEntry;
            String TaxIdNum;
            String CAF;
            Int32 FolioNum;
            Int32 FDocEntry = 0;
            Int32 FLineId = -1;
            Int32 lRetCode;
            String LicTradNum = "";
            String sDocTotal = "";
            Boolean bFolioAsignado = false;
            SAPbobsCOM.Documents oDocument = null;
            SAPbobsCOM.StockTransfer oStockTransfer = null;
            String[] FE52 = { "52", "52T", "52D" };
            String[] FEOt = { "33", "33A", "34", "39", "41", "56", "110", "110R", "111" };
            String TTipoDoc = "";
            String SubType = "";
            String tabla = "";
            try
            {
                if (FSBOApp.MessageBox("¿Esta seguro que desea foliar los documentos seleccionados?", 1, "Si", "No", "") == 1)
                {
                    oComboBox = (ComboBox)(oForm.Items.Item("TipoDoc").Specific);
                    TTipoDoc = oComboBox.Value;

                    if (TTipoDoc == "33") //Factura venta
                    {
                        sDocSubType = "--";
                        ObjType = "13";
                        TipoDocElect = "33";
                        tabla = "OINV";
                    }
                    else if (TTipoDoc == "33A") //Factura Anticipo
                    {
                        sDocSubType = "--";
                        ObjType = "203";
                        TipoDocElect = "33";
                        tabla = "ODPI";
                    }
                    else if (TTipoDoc == "34") //Factura Exenta
                    {
                        sDocSubType = "IE";
                        ObjType = "13";
                        TipoDocElect = "34";
                        tabla = "OINV";
                    }
                    else if (TTipoDoc == "39") //Boleta
                    {
                        sDocSubType = "IB";
                        ObjType = "13";
                        TipoDocElect = "39";
                        tabla = "OINV";
                    }
                    else if (TTipoDoc == "41") //Boleta exenta
                    {
                        sDocSubType = "EB";
                        ObjType = "13";
                        TipoDocElect = "41";
                        tabla = "OINV";
                    }
                    else if (TTipoDoc == "43") //Liquidacion Factura
                    {
                        sDocSubType = "--";
                        ObjType = "14";
                        TipoDocElect = "43";
                        tabla = "ORIN";
                    }
                    else if (TTipoDoc == "46") //Factura compra
                    {
                        sDocSubType = "--";
                        ObjType = "18";
                        TipoDocElect = "46";
                        tabla = "OPCH";
                    }
                    else if (TTipoDoc == "46A") //Factura Anticipo compra
                    {
                        sDocSubType = "--";
                        ObjType = "204";
                        TipoDocElect = "46";
                        tabla = "ODPO";
                    }
                    else if (TTipoDoc == "56") //nota debito
                    {
                        sDocSubType = "DN";
                        ObjType = "13";
                        TipoDocElect = "56";
                        tabla = "OINV";
                    }
                    else if (TTipoDoc == "61") //nota de credito
                    {
                        sDocSubType = "--";
                        ObjType = "14";
                        TipoDocElect = "61";
                        tabla = "ORIN";
                    }
                    else if (TTipoDoc == "61C") //nota de credito Compra
                    {
                        sDocSubType = "--";
                        ObjType = "19";
                        TipoDocElect = "61";
                        tabla = "ORPC";
                    }
                    else if (TTipoDoc == "52") //guia despacho por entrega
                    {
                        sDocSubType = "--";
                        ObjType = "15";
                        TipoDocElect = "52";
                        tabla = "ODLN";
                    }
                    else if (TTipoDoc == "52T") //guia despacho por transferencia stock
                    {
                        sDocSubType = "--";
                        ObjType = "67";
                        TipoDocElect = "52";
                        tabla = "OWTR";
                    }
                    else if (TTipoDoc == "52D") //guia despacho por devolucion mercancia en Compras
                    {
                        sDocSubType = "--";
                        ObjType = "21";
                        TipoDocElect = "52";
                        tabla = "ORPD";
                    }
                    else if (TTipoDoc == "110") //factura exportacion
                    {
                        sDocSubType = "IX";
                        ObjType = "13";
                        TipoDocElect = "110";
                        tabla = "OINV";
                    }
                    else if (TTipoDoc == "110R") //factura exportacion por reserva
                    {
                        sDocSubType = "--";
                        ObjType = "13";
                        TipoDocElect = "110";
                        tabla = "OINV";
                    }
                    else if (TTipoDoc == "111") //nota debito exportacion
                    {
                        sDocSubType = "DN";
                        ObjType = "13";
                        TipoDocElect = "111";
                        tabla = "OINV";
                    }
                    else if (TTipoDoc == "112") //nota de credito exportacion
                    {
                        sDocSubType = "--";
                        ObjType = "14";
                        TipoDocElect = "112";
                        tabla = "ORIN";
                    }

                    for (Int32 i = 0; i < odt.Rows.Count; i++)
                    {
                        if (((System.String)odt.GetValue("Sel", i)).Trim() == "Y")
                        {
                            bFolioAsignado = false;
                            FDocEntry = 0;
                            FLineId = -1;
                            sDocEntry = Convert.ToString(((System.Int32)odt.GetValue("DocEntry", i))).Trim();

                            if (bFolioPortal)
                            {
                                if (ObjType == "13")
                                    oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices));
                                else if (ObjType == "14")
                                    oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes));
                                else if (ObjType == "19")
                                    oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseCreditNotes));
                                else if (ObjType == "15")
                                    oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes));
                                else if (ObjType == "203")
                                    oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments));
                                else if (ObjType == "18")
                                    oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices));
                                else if (ObjType == "21")
                                    oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseReturns));
                                else if (ObjType == "67")
                                    oStockTransfer = (SAPbobsCOM.StockTransfer)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer));
                                else if (ObjType == "204")
                                    oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseDownPayments));

                                if ((ObjType == "67") && (oStockTransfer != null))
                                {
                                    if (GlobalSettings.RunningUnderSQLServer)
                                        s = "update {0} set Printed = 'Y' where DocEntry = {1}";
                                    else
                                        s = @"update ""{0}"" set ""Printed"" = 'Y' where ""DocEntry"" = {1}";
                                    s = String.Format(s, tabla, sDocEntry);
                                    oRecordSet.DoQuery(s);
                                    var oDeliveryNote = new TDeliveryNote();
                                    oDeliveryNote.SBO_f = FSBOf;
                                    if (TTipoDoc == "52T")
                                        oDeliveryNote.EnviarFE_WebService(sDocEntry, "--", true, true, false, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, TTipoDoc, "67", true);
                                }
                                else if (oDocument != null)
                                {
                                    if (oDocument.GetByKey(Convert.ToInt32(sDocEntry)))
                                    {
                                        oDocument.Printed = PrintStatusEnum.psYes;
                                        lRetCode = oDocument.Update();
                                        if (lRetCode != 0)
                                        {
                                            if (GlobalSettings.RunningUnderSQLServer)
                                                s = "update {0} set Printed = 'Y' where DocEntry = {1}";
                                            else
                                                s = @"update ""{0}"" set ""Printed"" = 'Y' where ""DocEntry"" = {1}";
                                            s = String.Format(s, tabla, sDocEntry);
                                            oRecordSet.DoQuery(s);
                                        }
                                    }

                                    if (FEOt.Contains(TTipoDoc))
                                    {
                                        var oInvoice = new TInvoice();
                                        oInvoice.SBO_f = FSBOf;
                                        oInvoice.EnviarFE_WebService(ObjType, oDocument, TipoDocElect, false, "", GlobalSettings.RunningUnderSQLServer, SubType, TTipoDoc, true);
                                        //oInvoice.EnviarFE(sDocEntry, SubType, ObjType, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, TipoDocElect);
                                    }
                                    else if ((TTipoDoc == "46") || (TTipoDoc == "46A"))
                                    {
                                        var oPurchaseInvoice = new TPurchaseInvoice();
                                        oPurchaseInvoice.SBO_f = FSBOf;
                                        oPurchaseInvoice.EnviarFE_WebService(sDocEntry, SubType, ObjType, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "46", TTipoDoc, true);
                                    }
                                    else if ((TTipoDoc == "61") || (TTipoDoc == "61C") || (TTipoDoc == "112") || (TTipoDoc == "43"))
                                    {
                                        var oCreditNotes = new TCreditNotes();
                                        oCreditNotes.SBO_f = FSBOf;
                                        oCreditNotes.EnviarFE_WebServiceNotaCredito(sDocEntry, SubType, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, ObjType, TipoDocElect, TTipoDoc, true);
                                    }
                                    //else if (sTipo in ['52','52T','52D'])
                                    else if (FE52.Contains(TTipoDoc))
                                    {
                                        var oDeliveryNote = new TDeliveryNote();
                                        oDeliveryNote.SBO_f = FSBOf;
                                        if (TTipoDoc == "52")
                                            oDeliveryNote.EnviarFE_WebService(sDocEntry, SubType, false, true, false, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52", "15", true);
                                        else if (TTipoDoc == "52T")
                                            oDeliveryNote.EnviarFE_WebService(sDocEntry, SubType, true, true, false, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52T", "67", true);
                                        else if (TTipoDoc == "52D")
                                            oDeliveryNote.EnviarFE_WebService(sDocEntry, SubType, false, true, true, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52D", "21", true);
                                    }
                                }


                            }
                            else if (Distribuido)//
                            {
                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = @"EXEC VID_SP_FE_BUSCAR_FOLIO '{0}'";
                                else
                                    s = @"CALL VID_SP_FE_BUSCAR_FOLIO ('{0}')";

                                s = String.Format(s, TipoDocElect);
                                oRecordSet.DoQuery(s);
                                if (oRecordSet.RecordCount > 0)
                                {
                                    TaxIdNum = (System.String)(oRecordSet.Fields.Item("TaxIdNum").Value).ToString().Trim();
                                    CAF = (System.String)(oRecordSet.Fields.Item("CAF").Value).ToString().Trim();
                                    FolioNum = (System.Int32)(oRecordSet.Fields.Item("Folio").Value);
                                    FDocEntry = (System.Int32)(oRecordSet.Fields.Item("DocEntry").Value);
                                    FLineId = (System.Int32)(oRecordSet.Fields.Item("LineId").Value);

                                    if (FolioNum == 0)
                                        throw new Exception("No se ha encontrado número de Folio disponible");

                                    if (TaxIdNum == "")
                                        throw new Exception("Debe ingresar RUT de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1");

                                    if (ObjType == "13")
                                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices));
                                    else if (ObjType == "14")
                                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes));
                                    else if (ObjType == "19")
                                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseCreditNotes));
                                    else if (ObjType == "15")
                                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes));
                                    else if (ObjType == "203")
                                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments));
                                    else if (ObjType == "18")
                                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices));
                                    else if (ObjType == "21")
                                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseReturns));
                                    else if (ObjType == "67")
                                        oStockTransfer = (SAPbobsCOM.StockTransfer)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer));
                                    else if (ObjType == "204")
                                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseDownPayments));


                                    if ((ObjType == "67") && (oStockTransfer != null))
                                    {
                                        if (oStockTransfer.GetByKey(Convert.ToInt32(sDocEntry)))
                                        {
                                            oStockTransfer.FolioNumber = FolioNum;
                                            oStockTransfer.FolioPrefixString = "GE";
                                            //oTransfer.Printed := BoYesNoEnum.tYES;

                                            lRetCode = oStockTransfer.Update();
                                            if (lRetCode != 0)
                                            {
                                                bFolioAsignado = false;
                                                if (GlobalSettings.RunningUnderSQLServer)
                                                    s = "update [@VID_FEDISTD] set U_Estado = 'D' where DocEntry = {0} and LineId = {1}";
                                                else
                                                    s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'D' where ""DocEntry"" = {0} and ""LineId"" = {1}";
                                                s = String.Format(s, FDocEntry, FLineId);
                                                oRecordSet.DoQuery(s);
                                                s = FCmpny.GetLastErrorDescription();
                                                FSBOApp.StatusBar.SetText("No se ha asignado Folio al Documento, " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                OutLog("No se ha asignado Folio al Documento DocEntry: " + sDocEntry + " ObjType: " + ObjType + " Documento Electronico: " + TipoDocElect + " -  " + s);
                                            }
                                            else
                                            {
                                                if (GlobalSettings.RunningUnderSQLServer)
                                                    s = "update [@VID_FEDISTD] set U_Estado = 'U', U_DocEntry = {0}, U_ObjType = '{1}', U_SubType = '{2}' where DocEntry = {3} and LineId = {4}";
                                                else
                                                    s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'U', ""U_DocEntry"" = {0}, ""U_ObjType"" = '{1}', ""U_SubType"" = '{2}' where ""DocEntry"" = {3} and ""LineId"" = {4}";
                                                s = String.Format(s, sDocEntry, ObjType, sDocSubType, FDocEntry, FLineId);
                                                oRecordSet.DoQuery(s);
                                                bFolioAsignado = true;


                                                if (Timbre == true)
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = @"SELECT C0.LicTradNum, ROUND(T0.DocTotal,0) DocTotal FROM OWTR T0 JOIN OCRD C0 ON C0.CardCode = T0.CardCode WHERE T0.DocEntry = {0}";
                                                    else
                                                        s = @"SELECT C0.""LicTradNum"", ROUND(T0.""DocTotal"",0) ""DocTotal"" FROM ""OWTR"" T0 JOIN ""OCRD"" C0 ON C0.""CardCode"" = T0.""CardCode"" WHERE T0.""DocEntry"" = {0}";
                                                    s = String.Format(s, sDocEntry);
                                                    oRecordSet.DoQuery(s);

                                                    if (oRecordSet.RecordCount > 0)
                                                    {
                                                        LicTradNum = ((System.String)oRecordSet.Fields.Item("LicTradNum").Value).Trim();
                                                        sDocTotal = FSBOf.DoubleToStr(((System.Double)oRecordSet.Fields.Item("DocTotal").Value));
                                                    }

                                                    //Colocar Timbre
                                                    XmlDocument xmlCAF = new XmlDocument();
                                                    XmlDocument xmlTimbre = new XmlDocument();
                                                    if (CAF == "")
                                                        throw new Exception("No se ha encontrado xml de CAF");
                                                    //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                    xmlCAF.LoadXml(CAF);
                                                    xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElect, Convert.ToString(oStockTransfer.FolioNumber), oStockTransfer.DocDate.ToString("yyyyMMdd"), LicTradNum.Replace(".",""), oStockTransfer.CardName, sDocTotal, oStockTransfer.Lines.ItemDescription, xmlCAF, TaxIdNum);
                                                    StringWriter sw = new StringWriter();
                                                    XmlTextWriter tx = new XmlTextWriter(sw);
                                                    xmlTimbre.WriteTo(tx);

                                                    s = sw.ToString();// 

                                                    if (s != "")
                                                    {
                                                        if (oStockTransfer.GetByKey(Convert.ToInt32(sDocEntry)))
                                                        {
                                                            oStockTransfer.UserFields.Fields.Item("U_FETimbre").Value = s;
                                                            lRetCode = oStockTransfer.Update();
                                                            if (lRetCode != 0)
                                                            {
                                                                s = FCmpny.GetLastErrorDescription();
                                                                FSBOApp.StatusBar.SetText("No se ha creado Timbre en el documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                                OutLog("No se ha creado Timbre en el documento: " + sDocEntry + " Tipo: 67 - " + s);
                                                            }
                                                            else
                                                                FSBOApp.StatusBar.SetText("Se ha creado satisfactoriamente Timbre en el documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                                        }
                                                    }
                                                    else
                                                        FSBOApp.StatusBar.SetText("No se ha creado Timbre en el documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);

                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = @"UPDATE OWTR SET Printed = 'Y', LPgFolioN = FolioNum where DocEntry = {0}";
                                                    else
                                                        s = @"UPDATE ""OWTR"" SET ""Printed"" = 'Y', ""LPgFolioN"" = ""FolioNum"" WHERE ""DocEntry"" = {0}";
                                                    s = String.Format(s, sDocEntry);
                                                    oRecordSet.DoQuery(s);
                                                }

                                                var oDeliveryNote = new TDeliveryNote();
                                                oDeliveryNote.SBO_f = FSBOf;
                                                if (TTipoDoc == "52T")
                                                    oDeliveryNote.EnviarFE_WebService(sDocEntry, "--", true, true, false, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, TTipoDoc, "67", false);
                                            }
                                        }
                                    }
                                    else if (oDocument != null)
                                    {
                                        if (oDocument.GetByKey(Convert.ToInt32(sDocEntry)))
                                        {
                                            if (oDocument.FolioNumber == 0)
                                            {
                                                oDocument.FolioNumber = FolioNum;
                                                if (TipoDocElect == "46") //Factura Compra
                                                {
                                                    oDocument.FolioPrefixString = "FC";
                                                    SubType = "--";
                                                }
                                                else if (TipoDocElect == "52") //Guias
                                                {
                                                    oDocument.FolioPrefixString = "GE";
                                                    SubType = "--";
                                                }
                                                else if (TipoDocElect == "43") //Liquidacion facturas
                                                {
                                                    oDocument.FolioPrefixString = "LF";
                                                    SubType = "--";
                                                }
                                                else if (TipoDocElect == "33") //Facturas
                                                {
                                                    oDocument.FolioPrefixString = "FE";
                                                    SubType = "--";
                                                }
                                                else if (TipoDocElect == "34") //Factura Exenta
                                                {
                                                    oDocument.FolioPrefixString = "EE";
                                                    SubType = "IE";
                                                }
                                                else if (TipoDocElect == "56") //Nota Debito
                                                {
                                                    oDocument.FolioPrefixString = "ND";
                                                    SubType = "DN";
                                                }
                                                else if (TipoDocElect == "39") //Boleta
                                                {
                                                    oDocument.FolioPrefixString = "BE";
                                                    SubType = "IB";
                                                }
                                                else if (TipoDocElect == "41") //Boleta Exenta
                                                {
                                                    oDocument.FolioPrefixString = "BX";
                                                    SubType = "EB";
                                                }
                                                else if ((TipoDocElect == "110") && (TTipoDoc == "110R")) //Factura Exportacion
                                                {
                                                    oDocument.FolioPrefixString = "FX";
                                                    SubType = "--";
                                                }
                                                else if (TipoDocElect == "110") //Factura Exportacion
                                                {
                                                    oDocument.FolioPrefixString = "FX";
                                                    SubType = "IX";
                                                }
                                                else if (TipoDocElect == "61") //Nota de Credito
                                                {
                                                    oDocument.FolioPrefixString = "NC";
                                                    SubType = "--";
                                                }
                                                else if (TipoDocElect == "111") //Nota Debito exportacion
                                                {
                                                    oDocument.FolioPrefixString = "ND";
                                                    SubType = "DN";
                                                }
                                                else if (TipoDocElect == "112") //Nota de Credito exportacion
                                                {
                                                    oDocument.FolioPrefixString = "NC";
                                                    SubType = "--";
                                                }

                                                oDocument.Printed = PrintStatusEnum.psYes;

                                                lRetCode = oDocument.Update();
                                                if (lRetCode != 0)
                                                {
                                                    bFolioAsignado = false;
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "update [@VID_FEDISTD] set U_Estado = 'D' where DocEntry = {0} and LineId = {1}";
                                                    else
                                                        s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'D' where ""DocEntry"" = {0} and ""LineId"" = {1}";
                                                    s = String.Format(s, FDocEntry, FLineId);
                                                    oRecordSet.DoQuery(s);
                                                    s = FCmpny.GetLastErrorDescription();
                                                    FSBOApp.StatusBar.SetText("No se ha asignado Folio al Documento, " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                    OutLog("No se ha asignado Folio al Documento DocEntry: " + sDocEntry + " ObjType: " + ObjType + " Documento Electronico: " + TipoDocElect + " - " + s);
                                                }
                                                else
                                                {
                                                    //ahora debo marcar que el folio fue usado y colocar los datos del documento que uso el folio
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = "update [@VID_FEDISTD] set U_Estado = 'U', U_DocEntry = {0}, U_ObjType = '{1}', U_SubType = '{2}' where DocEntry = {3} and LineId = {4}";
                                                    else
                                                        s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'U', ""U_DocEntry"" = {0}, ""U_ObjType"" = '{1}', ""U_SubType"" = '{2}' where ""DocEntry"" = {3} and ""LineId"" = {4}";
                                                    s = String.Format(s, sDocEntry, ObjType, sDocSubType, FDocEntry, FLineId);
                                                    oRecordSet.DoQuery(s);
                                                    //lRetCode = Reg.ActEstadoFolioUpt((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value), (System.Int32)(oRecordSet.Fields.Item("LineId").Value), (System.Double)(oRecordSet.Fields.Item("U_Folio").Value), TipoDocElec, sDocEntry, "13", sDocSubType);
                                                    bFolioAsignado = true;

                                                    if (Timbre == true)
                                                    {
                                                        //Colocar Timbre
                                                        XmlDocument xmlCAF = new XmlDocument();
                                                        XmlDocument xmlTimbre = new XmlDocument();
                                                        if (CAF == "")
                                                            throw new Exception("No se ha encontrado xml de CAF Tipo documento electronico " + TipoDocElect);
                                                        //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                        xmlCAF.LoadXml(CAF);
                                                        xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElect, Convert.ToString(oDocument.FolioNumber), oDocument.DocDate.ToString("yyyyMMdd"), oDocument.FederalTaxID.Replace(".",""), oDocument.CardName, Convert.ToString(Math.Round(oDocument.DocTotal, 0)), oDocument.Lines.ItemDescription, xmlCAF, TaxIdNum);

                                                        StringWriter sw = new StringWriter();
                                                        XmlTextWriter tx = new XmlTextWriter(sw);
                                                        xmlTimbre.WriteTo(tx);

                                                        s = sw.ToString();// 

                                                        if (s != "")
                                                        {
                                                            if (oDocument.GetByKey(Convert.ToInt32(sDocEntry)))
                                                            {
                                                                oDocument.UserFields.Fields.Item("U_FETimbre").Value = s;
                                                                lRetCode = oDocument.Update();
                                                                if (lRetCode != 0)
                                                                {
                                                                    FSBOApp.StatusBar.SetText("No se ha creado Timbre en el documento - DocEntry: " + sDocEntry + " ObjType: " + ObjType + " Documento Electronico: " + TipoDocElect + " - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                                    OutLog("No se ha creado Timbre en el documento: " + sDocEntry + " Tipo: " + TipoDocElect + " - " + s);
                                                                }
                                                                else
                                                                    FSBOApp.StatusBar.SetText("Se ha creado satisfactoriamente Timbre en el documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                                            }
                                                        }
                                                        else
                                                            FSBOApp.StatusBar.SetText("No se ha creado Timbre en el documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                    }

                                                    //******************************

                                                    if (FEOt.Contains(TTipoDoc))
                                                    {
                                                        var oInvoice = new TInvoice();
                                                        oInvoice.SBO_f = FSBOf;
                                                        oInvoice.EnviarFE_WebService(ObjType, oDocument, TipoDocElect, false, "", GlobalSettings.RunningUnderSQLServer, SubType, TTipoDoc, false);
                                                        //oInvoice.EnviarFE(sDocEntry, SubType, ObjType, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, TipoDocElect);
                                                    }
                                                    else if ((TTipoDoc == "46") || (TTipoDoc == "46A"))
                                                    {
                                                        var oPurchaseInvoice = new TPurchaseInvoice();
                                                        oPurchaseInvoice.SBO_f = FSBOf;
                                                        oPurchaseInvoice.EnviarFE_WebService(sDocEntry, SubType, ObjType, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "46", TTipoDoc, false);
                                                    }
                                                    else if ((TTipoDoc == "61") || (TTipoDoc == "61C") || (TTipoDoc == "112") || (TTipoDoc == "43"))
                                                    {
                                                        var oCreditNotes = new TCreditNotes();
                                                        oCreditNotes.SBO_f = FSBOf;
                                                        oCreditNotes.EnviarFE_WebServiceNotaCredito(sDocEntry, SubType, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, ObjType, TipoDocElect, TTipoDoc, false);
                                                    }
                                                    //else if (sTipo in ['52','52T','52D'])
                                                    else if (FE52.Contains(TTipoDoc))
                                                    {
                                                        var oDeliveryNote = new TDeliveryNote();
                                                        oDeliveryNote.SBO_f = FSBOf;
                                                        if (TTipoDoc == "52")
                                                            oDeliveryNote.EnviarFE_WebService(sDocEntry, SubType, false, true, false, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52", "15", false);
                                                        else if (TTipoDoc == "52T")
                                                            oDeliveryNote.EnviarFE_WebService(sDocEntry, SubType, true, true, false, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52T", "67", false);
                                                        else if (TTipoDoc == "52D")
                                                            oDeliveryNote.EnviarFE_WebService(sDocEntry, SubType, false, true, true, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52D", "21", false);
                                                    }

                                                    //******************************
                                                }
                                            }
                                        }
                                    }
                                    oDocument = null;
                                    oStockTransfer = null;
                                }
                                else
                                    FSBOApp.StatusBar.SetText("No se encuentra numeros disponibles para SBO", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            }//fin distribuido
                        }
                    }

                    oEditText = (EditText)(oForm.Items.Item("Desde").Specific);
                    var sDesde = oEditText.Value;
                    oEditText = (EditText)(oForm.Items.Item("Hasta").Specific);
                    var sHasta = oEditText.Value;
                    oComboBox = (ComboBox)(oForm.Items.Item("TipoDoc").Specific);
                    var TipoDoc = oComboBox.Value;

                    if (TipoDoc == "")
                        FSBOApp.StatusBar.SetText("Debe seleccionar Tipo Documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    else if (sDesde == "")
                        FSBOApp.StatusBar.SetText("Debe ingresar fecha Desde", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    else if (sHasta == "")
                        FSBOApp.StatusBar.SetText("Debe ingresar fecha Hasta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    else
                        BuscarDoc(TipoDoc, sDesde, sHasta);
                }
            }
            catch (Exception e)
            {
                if ((Distribuido == true) && (bFolioAsignado == false) && (FDocEntry != 0) && (FLineId != -1))
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = "update [@VID_FEDISTD] set U_Estado = 'D' where DocEntry = {0} and LineId = {1}";
                    else
                        s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'D' where ""DocEntry"" = {0} and ""LineId"" = {1}";
                    s = String.Format(s, FDocEntry, FLineId);
                    oRecordSet.DoQuery(s);
                }
                FSBOApp.StatusBar.SetText("FoliarDoctos: " + e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FoliarDoctos: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }


        private void Seleccionar(String Valor)
        {
            oForm.Freeze(true);
            try
            {
                for (Int32 i = 0; i < odt.Rows.Count; i++)
                    odt.SetValue("Sel", i, Valor);
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText("Seleccionar: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("Seleccionar: " + e.Message + " ** Trace: " + e.StackTrace);
            }
            finally
            {
                oForm.Freeze(false);
            }
        }


        private void CrearPDF()
        {
            try
            {
                SAPbobsCOM.ReportLayoutsService oLayoutService = (SAPbobsCOM.ReportLayoutsService)FCmpny.GetCompanyService().GetBusinessService(SAPbobsCOM.ServiceTypes.ReportLayoutsService);
                SAPbobsCOM.ReportParams oReportParams = (SAPbobsCOM.ReportParams)oLayoutService.GetDataInterface(SAPbobsCOM.ReportLayoutsServiceDataInterfaces.rlsdiReportParams);
                oReportParams.ReportCode = "INV2";//defined in db table "RTYP"
                oReportParams.CardCode = "296";//business partner 
                var oReport = oLayoutService.GetDefaultReport(oReportParams);
                BlobParams oBlobParams = (SAPbobsCOM.BlobParams)FCmpny.GetCompanyService().GetDataInterface(SAPbobsCOM.CompanyServiceDataInterfaces.csdiBlobParams);
                oBlobParams.Table = "RDOC";
                oBlobParams.Field = "Template";
                oBlobParams.FileName = @"C:\Paso\salesorder.rpt";
                BlobTableKeySegment oKeySegment = oBlobParams.BlobTableKeySegments.Add();
                oKeySegment.Name = "DocCode";
                oKeySegment.Value = oReport.LayoutCode;
                FCmpny.GetCompanyService().SaveBlobToFile(oBlobParams);
                
                
  
                /*SAPbobsCOM.ReportLayoutsService oReportLayoutService = (SAPbobsCOM.ReportLayoutsService)FCmpny.GetCompanyService().GetBusinessService(SAPbobsCOM.ServiceTypes.ReportLayoutsService);  
                SAPbobsCOM.ReportLayoutPrintParams oReporPrintParams = (SAPbobsCOM.ReportLayoutPrintParams)oReportLayoutService.GetDataInterface(SAPbobsCOM.ReportLayoutsServiceDataInterfaces.rlsdiReportLayoutPrintParams);  
                SAPbobsCOM.ReportParams oReportParam = (SAPbobsCOM.ReportParams)oReportLayoutService.GetDataInterface(SAPbobsCOM.ReportLayoutsServiceDataInterfaces.rlsdiReportParams);  
                oReportParam.ReportCode = "INV2";  
                SAPbobsCOM.DefaultReportParams oReportParaDefault = oReportLayoutService.GetDefaultReport(oReportParam);  
                oReporPrintParams.LayoutCode = "INV20007";// oReportParaDefault.LayoutCode;  
                oReporPrintParams.DocEntry = 1111;

               
                oReportLayoutService.Print(oReporPrintParams);  
                
                
                var oReport = oReportLayoutService.GetDefaultReport(oReportParam);
                BlobParams oBlobParams = (SAPbobsCOM.BlobParams)FCmpny.GetCompanyService().GetDataInterface(SAPbobsCOM.CompanyServiceDataInterfaces.csdiBlobParams);
                oBlobParams.Table = "RDOC";
                oBlobParams.Field = "Template";
                oBlobParams.FileName = @"C:\salesorder.rpt";
                BlobTableKeySegment oKeySegment = oBlobParams.BlobTableKeySegments.Add();
                oKeySegment.Name = "DocCode";
                oKeySegment.Value = oReport.LayoutCode;
                FCmpny.GetCompanyService().SaveBlobToFile(oBlobParams);*/

            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText("crearPDF: " + x.Message + " ** Trace: " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CrearPDF: " + x.Message + " ** Trace: " + x.StackTrace);
            }
        }


    }// fin class TASignarFolios
}
