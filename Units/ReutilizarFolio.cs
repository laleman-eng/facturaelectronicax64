using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Configuration;
using System.IO;
using System.Data;
using System.Threading;
using System.Data.SqlClient;
using System.Xml;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using VisualD.SBOObjectMg1;
using VisualD.Main;
using VisualD.MainObjBase;
using SAPbouiCOM;
using SAPbobsCOM;
using VisualD.ADOSBOScriptExecute;
using FactRemota;
using Factura_Electronica_VK.Functions;
using Factura_Electronica_VK.DeliveryNote;
using Factura_Electronica_VK.Invoice;
using Factura_Electronica_VK.PurchaseInvoice;
using Factura_Electronica_VK.CreditNotes;

namespace Factura_Electronica_VK.ReutilizarFolio
{
    public class TReutilizarFolio : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.DBDataSource oDBDSHeader;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.ComboBox oComboBox;
        private SAPbouiCOM.Item oItem;
        private SAPbouiCOM.EditText oEditText;
        private SqlConnection ConexionADO = null;
        private TFunctions Funciones = new TFunctions();
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            Int32 CantRol;
            SAPbouiCOM.Grid oGrid;
            SAPbouiCOM.DataTable odt;

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            Funciones.SBO_f = FSBOf;
            try
            {
                //Lista = new List<string>();
                FSBOf.LoadForm(xmlPath, "VID_FEREUTFOL.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All
                oForm.EnableMenu("1282", false); //Crear
                oForm.EnableMenu("1281", false); //Actualizar

                // Ok Ad  Fnd Vw Rq Sec
                //Lista.Add('DocNum    , f,  f,  t,  f, n, 1');
                //Lista.Add('DocDate   , f,  t,  f,  f, r, 1');
                //Lista.Add('CardCode  , f,  t,  t,  f, r, 1');
                //FSBOf.SetAutoManaged(var oForm, Lista);

                oForm.DataSources.UserDataSources.Add("TipDoc", BoDataType.dt_SHORT_TEXT, 20);
                ((ComboBox)oForm.Items.Item("TipDoc").Specific).DataBind.SetBound(true, "", "TipDoc");

                oForm.DataSources.UserDataSources.Add("Folio", BoDataType.dt_LONG_NUMBER);
                ((EditText)oForm.Items.Item("Folio").Specific).DataBind.SetBound(true, "", "Folio");

                oForm.DataSources.UserDataSources.Add("ObjType", BoDataType.dt_SHORT_TEXT, 20);
                oForm.DataSources.UserDataSources.Add("Tabla", BoDataType.dt_SHORT_TEXT, 20);
                oForm.DataSources.UserDataSources.Add("SubType", BoDataType.dt_SHORT_TEXT, 20);
                oForm.DataSources.UserDataSources.Add("DocEntry", BoDataType.dt_SHORT_TEXT, 20);
                oForm.DataSources.UserDataSources.Add("EntryLOG", BoDataType.dt_SHORT_TEXT, 20);
                oForm.DataSources.UserDataSources.Add("TipoDoc", BoDataType.dt_SHORT_TEXT, 20);

                oComboBox = (ComboBox)(oForm.Items.Item("TipDoc").Specific);
                oComboBox.ValidValues.Add("33", "Factura");
                oComboBox.ValidValues.Add("33A", "Factura por Anticipo");
                oComboBox.ValidValues.Add("34", "Factura Exenta");
                oComboBox.ValidValues.Add("39", "Boleta");
                oComboBox.ValidValues.Add("41", "Boleta Exenta");
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
                oComboBox.ValidValues.Add("111", "Nota de Debito Exportación Elect.");
                oComboBox.ValidValues.Add("112", "Nota de Credito Exportacion Elect.");
                oComboBox.Select("33", BoSearchKey.psk_ByValue);

                oGrid = ((Grid)oForm.Items.Item("grid").Specific);
                odt = oForm.DataSources.DataTables.Add("odt");
                oGrid.DataTable = odt;

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

                    if (GlobalSettings.RunningUnderSQLServer)
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
                    }
                }

            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            finally
            {
                if (oForm != null)
                    oForm.Freeze(false);
            }


            return Result;
        }//fin InitForm


        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            SAPbouiCOM.DataTable oDataTable;
            SAPbouiCOM.IChooseFromListEvent oCFLEvento = null;
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);

            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.ItemUID == "Buscar") && (!pVal.BeforeAction))
                {
                    if (Validar())
                        Buscar();
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction) && (pVal.ItemUID == "Crear"))
                {
                    if (Validar())
                        Crear();
                }

            }
            catch (Exception e)
            {
                if (FCmpny.InTransaction)
                    FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }

        }//fin FormEvent


        public new void FormDataEvent(ref BusinessObjectInfo BusinessObjectInfo, ref Boolean BubbleEvent)
        {
            base.FormDataEvent(ref BusinessObjectInfo, ref BubbleEvent);

            try
            {

            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText("FormDataEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormDataEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin FormDataEvent


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

            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent


        private Boolean Validar()
        {
            oForm.Freeze(true);
            try
            {
                oComboBox = (ComboBox)(oForm.Items.Item("TipDoc").Specific);
                var TipoDoc = oComboBox.Value.Trim();
                var Folio = ((System.String)((EditText)oForm.Items.Item("Folio").Specific).Value);
                if ((Folio == "") || (Folio == "0"))
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar Folio Rechazado", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return false;
                }
                else if (FSBOf.StrToInteger(Folio) <= 0)
                {
                    FSBOApp.StatusBar.SetText("Folio Rechazado debe ser mayor a 0", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return false;
                }
                else if (TipoDoc == "")
                {
                    FSBOApp.StatusBar.SetText("Debe seleccionar Tipo Documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText("Validar: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("Validar: " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
            finally
            {
                oForm.Freeze(false);
            }
        }

        private void Buscar()
        {
            String TipoDoc;
            String TipoDocElect = "";
            String ObjTypeRechazado = "";
            String TablaRechazado = "";
            String DocSubTypeRechazado;
            String FolioRechazado;
            String DocEntryRechazado;
            SAPbouiCOM.Grid oGrid;
            SAPbouiCOM.DataTable odt;

            try
            {
                oForm.Freeze(true);
                oGrid = ((Grid)oForm.Items.Item("grid").Specific);
                odt = oGrid.DataTable;
                odt.Clear();
                oComboBox = (ComboBox)(oForm.Items.Item("TipDoc").Specific);
                TipoDoc = oComboBox.Value;
                TipoDoc = TipoDoc.Trim();
                FolioRechazado = ((System.String)((EditText)oForm.Items.Item("Folio").Specific).Value).Trim();
                DocSubTypeRechazado = "";

                if (TipoDoc == "33") //Factura venta
                {
                    TablaRechazado = "OINV";
                    DocSubTypeRechazado = "--";
                    ObjTypeRechazado = "13";
                    TipoDocElect = "33";
                }
                else if (TipoDoc == "33A") //Factura Anticipo venta
                {
                    TablaRechazado = "ODPI";
                    DocSubTypeRechazado = "--";
                    ObjTypeRechazado = "203";
                    TipoDocElect = "33";
                }
                else if (TipoDoc == "46") //Factura compra terceros
                {
                    TablaRechazado = "OPCH";
                    DocSubTypeRechazado = "--";
                    ObjTypeRechazado = "18";
                    TipoDocElect = "46";
                }
                else if (TipoDoc == "46A") //Factura Anticipo Compra a terceros
                {
                    TablaRechazado = "ODPO";
                    DocSubTypeRechazado = "--";
                    ObjTypeRechazado = "204";
                    TipoDocElect = "46";
                }
                else if (TipoDoc == "34") //Factura Exenta
                {
                    TablaRechazado = "OINV";
                    DocSubTypeRechazado = "IE";
                    ObjTypeRechazado = "13";
                    TipoDocElect = "34";
                }
                else if (TipoDoc == "39") //Boleta
                {
                    TablaRechazado = "OINV";
                    DocSubTypeRechazado = "IB";
                    ObjTypeRechazado = "13";
                    TipoDocElect = "39";
                }
                else if (TipoDoc == "41") //Boleta exenta
                {
                    TablaRechazado = "OINV";
                    DocSubTypeRechazado = "EB";
                    ObjTypeRechazado = "13";
                    TipoDocElect = "41";
                }
                else if (TipoDoc == "46") //Factura de compra
                {
                    TablaRechazado = "OPCH";
                    DocSubTypeRechazado = "--";
                    ObjTypeRechazado = "18";
                    TipoDocElect = "46";
                }
                else if (TipoDoc == "56") //nota debito
                {
                    TablaRechazado = "OINV";
                    DocSubTypeRechazado = "DN";
                    ObjTypeRechazado = "13";
                    TipoDocElect = "56";
                }
                else if (TipoDoc == "61") //nota de credito
                {
                    TablaRechazado = "ORIN";
                    DocSubTypeRechazado = "--";
                    ObjTypeRechazado = "14";
                    TipoDocElect = "61";
                }
                else if (TipoDoc == "61C") //nota de credito Compra
                {
                    TablaRechazado = "ORPC";
                    DocSubTypeRechazado = "--";
                    ObjTypeRechazado = "19";
                    TipoDocElect = "61";
                }
                else if (TipoDoc == "52") //guia despacho por entrega
                {
                    TablaRechazado = "ODLN";
                    DocSubTypeRechazado = "--";
                    ObjTypeRechazado = "15";
                    TipoDocElect = "52";
                }
                else if (TipoDoc == "52T") //guia despacho por transferencia stock
                {
                    TablaRechazado = "OWTR";
                    DocSubTypeRechazado = "--";
                    ObjTypeRechazado = "67";
                    TipoDocElect = "52";
                }
                else if (TipoDoc == "52D") //guia despacho por devolucion de mercancia Compra
                {
                    TablaRechazado = "ORPD";
                    DocSubTypeRechazado = "--";
                    ObjTypeRechazado = "21";
                    TipoDocElect = "52";
                }
                else if (TipoDoc == "110") //factura exportacion
                {
                    TablaRechazado = "OINV";
                    DocSubTypeRechazado = "IX";
                    ObjTypeRechazado = "13";
                    TipoDocElect = "110";
                }
                else if (TipoDoc == "110R") //factura exportacion por Reserva
                {
                    TablaRechazado = "OINV";
                    DocSubTypeRechazado = "--";
                    ObjTypeRechazado = "13";
                    TipoDocElect = "110";
                }
                else if (TipoDoc == "111") //Nota de Debito exportacion
                {
                    TablaRechazado = "OINV";
                    DocSubTypeRechazado = "DN";
                    ObjTypeRechazado = "13";
                    TipoDocElect = "111";
                }
                else if (TipoDoc == "112") //Nota de Credito exportacion
                {
                    TablaRechazado = "ORIN";
                    DocSubTypeRechazado = "--";
                    ObjTypeRechazado = "14";
                    TipoDocElect = "112";
                }


                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT CAST(T0.DocEntry AS VARCHAR(20)) 'DocEntry', T0.DocSubType, SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst
                                ,ISNULL(T0.CardName,'') + ' ' + CONVERT(VARCHAR(10), T0.DocDate, 103) + ' DocNum ' + CAST(T0.DocNum AS VARCHAR(20)) 'Mensaje'
                                ,CAST(L0.DocEntry AS VARCHAR(20)) 'EntryLOG'
                            FROM {0} T0 
                            JOIN NNM1 T2 ON T0.Series = T2.Series 
                            JOIN [@VID_FELOG] L0 ON L0.U_DocEntry = T0.DocEntry
							                    AND L0.U_ObjType = T0.ObjType
												AND L0.U_SubType = T0.DocSubType
                           WHERE (T0.FolioNum = {1})
                             AND SUBSTRING(UPPER(T2.BeginStr), 1, 1) = 'E'
                             AND T0.DocSubType = '{2}'
                             AND L0.U_Status = 'RZ'
                             {3}
                           ORDER BY T0.DocEntry DESC";
                else
                    s = @"SELECT CAST(T0.""DocEntry"" AS VARCHAR(20)) ""DocEntry"", T0.""DocSubType"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                                ,IFNULL(T0.""CardName"",'') || ' ' || TO_VARCHAR(T0.""DocDate"", 'dd/MM/yyyy') || ' DocNum ' || TO_VARCHAR(T0.""DocNum"") ""Mensaje""
                                ,TO_VARCHAR(L0.""DocEntry"") ""EntryLOG""
                            FROM ""{0}"" T0 
                            JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                            JOIN ""@VID_FELOG"" L0 ON L0.""U_DocEntry"" = T0.""DocEntry""
							                      AND L0.""U_ObjType"" = T0.""ObjType""
												  AND L0.""U_SubType"" = T0.""DocSubType""
                           WHERE (T0.""FolioNum"" = {1})
                             AND SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) = 'E'
                             AND T0.""DocSubType"" = '{2}'
                             AND L0.""U_Status"" = 'RZ'
                             {3}
                           ORDER BY T0.""DocEntry"" DESC";
                s = String.Format(s, TablaRechazado, FolioRechazado, DocSubTypeRechazado, (TipoDoc == "110R" ? (GlobalSettings.RunningUnderSQLServer ? " AND T0.isIns = 'Y' " : @" AND T0.""isIns"" = 'Y' ") : ""));
                oRecordSet.DoQuery(s);

                if (oRecordSet.RecordCount == 0)
                {
                    FSBOApp.StatusBar.SetText("No se ha encontrado el documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    OutLog(s);
                }
                else
                {
                    var Mensaje = ((System.String)oRecordSet.Fields.Item("Mensaje").Value).Trim();
                    ((StaticText)oForm.Items.Item("lbDatos").Specific).Caption = Mensaje;
                    DocEntryRechazado = ((System.String)oRecordSet.Fields.Item("DocEntry").Value).Trim();

                    oForm.DataSources.UserDataSources.Item("ObjType").Value = ObjTypeRechazado;
                    oForm.DataSources.UserDataSources.Item("Tabla").Value = TablaRechazado;
                    oForm.DataSources.UserDataSources.Item("SubType").Value = DocSubTypeRechazado;
                    oForm.DataSources.UserDataSources.Item("Folio").Value = FolioRechazado;
                    oForm.DataSources.UserDataSources.Item("DocEntry").Value = DocEntryRechazado;
                    oForm.DataSources.UserDataSources.Item("TipoDoc").Value = TipoDocElect;
                    oForm.DataSources.UserDataSources.Item("EntryLOG").Value = ((System.String)oRecordSet.Fields.Item("EntryLOG").Value).Trim();

                    //ahora busco documentos sin folio pero que sean de igual tabla y tipo de documento
                    var st = "";
                    if ((TipoDocElect == "111") || (TipoDocElect == "112"))
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
                        s = @"SELECT T0.DocEntry, T0.DocNum, T0.DocDate, T0.CardCode, T0.CardName, T0.DocTotal, T0.ObjType, T0.DocSubType
                          FROM {0} T0
                          JOIN NNM1 N1 ON N1.Series = T0.Series
                                      --AND N1.ObjectCode = T0.ObjType
                         WHERE T0.DocSubType = '{1}'
                           AND UPPER(LEFT(ISNULL(N1.BeginStr,''),1)) = 'E'
                           AND ISNULL(T0.FolioNum, 0) = 0
                           AND T0.CANCELED = 'N'
                           AND T0.DocEntry NOT IN (SELECT U_DocEntryO FROM [@VID_FERECHAZO] WHERE U_ObjTypeO = T0.ObjType)
                           {2}
                           {3}
                        ";
                    else
                        s = @"SELECT T0.""DocEntry"", T0.""DocNum"", T0.""DocDate"", T0.""CardCode"", T0.""CardName"", T0.""DocTotal"", T0.""ObjType"", T0.""DocSubType""
                          FROM ""{0}"" T0
                          JOIN ""NNM1"" N1 ON N1.""Series"" = T0.""Series""
                                      --AND N1.""ObjectCode"" = T0.""ObjType""
                         WHERE T0.""DocSubType"" = '{1}'
                           AND UPPER(LEFT(IFNULL(N1.""BeginStr"",''),1)) = 'E'
                           AND IFNULL(T0.""FolioNum"", 0) = 0
                           AND T0.""CANCELED"" = 'N'
                           AND T0.""DocEntry"" NOT IN (SELECT ""U_DocEntryO"" FROM ""@VID_FERECHAZO"" WHERE ""U_ObjTypeO"" = T0.""ObjType"")
                           {2}
                           {3}
                        ";

                    s = String.Format(s, TablaRechazado, DocSubTypeRechazado, st, (TipoDoc == "110R" ? (GlobalSettings.RunningUnderSQLServer ? @" AND T0.isIns = 'Y' " : @" AND T0.""isIns"" = 'Y' ") : ""));

                    odt.ExecuteQuery(s);
                    oGrid.Columns.Item("DocEntry").Type = BoGridColumnType.gct_EditText;
                    var oColumn = (GridColumn)(oGrid.Columns.Item("DocEntry"));
                    var oEditColumn = (EditTextColumn)(oColumn);
                    oEditColumn.Editable = false;
                    oEditColumn.TitleObject.Caption = "Llave SAP";
                    oEditColumn.LinkedObjectType = ObjTypeRechazado;
                    oEditColumn.RightJustified = true;

                    oGrid.Columns.Item("DocNum").Type = BoGridColumnType.gct_EditText;
                    oColumn = (GridColumn)(oGrid.Columns.Item("DocNum"));
                    oEditColumn = (EditTextColumn)(oColumn);
                    oEditColumn.Editable = false;
                    oEditColumn.TitleObject.Caption = "Número SAP";
                    oEditColumn.RightJustified = true;

                    oGrid.Columns.Item("DocDate").Type = BoGridColumnType.gct_EditText;
                    oColumn = (GridColumn)(oGrid.Columns.Item("DocDate"));
                    oEditColumn = (EditTextColumn)(oColumn);
                    oEditColumn.Editable = false;
                    oEditColumn.TitleObject.Caption = "Fecha Contable";
                    oEditColumn.RightJustified = false;

                    oGrid.Columns.Item("CardCode").Type = BoGridColumnType.gct_EditText;
                    oColumn = (GridColumn)(oGrid.Columns.Item("CardCode"));
                    oEditColumn = (EditTextColumn)(oColumn);
                    oEditColumn.Editable = false;
                    oEditColumn.TitleObject.Caption = "Código SN";
                    oEditColumn.LinkedObjectType = "2";

                    oGrid.Columns.Item("CardName").Type = BoGridColumnType.gct_EditText;
                    oColumn = (GridColumn)(oGrid.Columns.Item("CardName"));
                    oEditColumn = (EditTextColumn)(oColumn);
                    oEditColumn.Editable = false;
                    oEditColumn.TitleObject.Caption = "Nombre SN";

                    oGrid.Columns.Item("DocTotal").Type = BoGridColumnType.gct_EditText;
                    oColumn = (GridColumn)(oGrid.Columns.Item("DocTotal"));
                    oEditColumn = (EditTextColumn)(oColumn);
                    oEditColumn.Editable = false;
                    oEditColumn.TitleObject.Caption = "Total Documento";
                    oEditColumn.RightJustified = true;

                    oGrid.Columns.Item("ObjType").Type = BoGridColumnType.gct_EditText;
                    oColumn = (GridColumn)(oGrid.Columns.Item("ObjType"));
                    oEditColumn = (EditTextColumn)(oColumn);
                    oEditColumn.Editable = false;
                    oEditColumn.Visible = false;

                    oGrid.Columns.Item("DocSubType").Type = BoGridColumnType.gct_EditText;
                    oColumn = (GridColumn)(oGrid.Columns.Item("DocSubType"));
                    oEditColumn = (EditTextColumn)(oColumn);
                    oEditColumn.Editable = false;
                    oEditColumn.Visible = false;

                    oGrid.AutoResizeColumns();
                    oGrid.SelectionMode = BoMatrixSelect.ms_Single;
                    if (odt.Rows.Offset == 0)
                        FSBOApp.StatusBar.SetText("No se han encontrado documentos electronicos sin folio y que son del mismo tipo del documento rechazado", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                }


            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText("Buscar: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("Buscar: " + e.Message + " ** Trace: " + e.StackTrace);
            }
            finally
            {
                oForm.Freeze(false);
            }
        }

        private void Crear()
        {
            String ObjTypeRechazado;
            String TablaRechazado;
            String DocSubTypeRechazado;
            String FolioRechazado;
            String DocEntryRechazado;
            String EntryLOG;
            String TipoDocElect;
            Int32 DocEntryDocNuevo = 0;
            String ObjTypeDocNuevo = "";
            String DocSubTypeDocNuevo = "";
            Int32 lRetCode;
            String LicTradNum = "";
            String sDocTotal = "";
            String[] FE52 = { "52", "52T", "52D" };
            String[] FEOt = { "33", "33A", "34", "39", "41", "56", "110", "110R", "111" };
            String TTipoDoc = "";

            SAPbobsCOM.Documents oDocument = null;
            SAPbobsCOM.StockTransfer oStockTransfer = null;
            SAPbouiCOM.Grid oGrid;
            SAPbouiCOM.DataTable odt;

            try
            {
                oComboBox = (ComboBox)(oForm.Items.Item("TipDoc").Specific);
                TTipoDoc = oComboBox.Value;
                TTipoDoc = TTipoDoc.Trim();

                ObjTypeRechazado = ((System.String)oForm.DataSources.UserDataSources.Item("ObjType").Value).Trim();
                TablaRechazado = ((System.String)oForm.DataSources.UserDataSources.Item("Tabla").Value).Trim();
                DocSubTypeRechazado = ((System.String)oForm.DataSources.UserDataSources.Item("SubType").Value).Trim();
                FolioRechazado = ((System.String)oForm.DataSources.UserDataSources.Item("Folio").Value).Trim();
                DocEntryRechazado = ((System.String)oForm.DataSources.UserDataSources.Item("DocEntry").Value).Trim();
                EntryLOG = ((System.String)oForm.DataSources.UserDataSources.Item("EntryLOG").Value).Trim();
                TipoDocElect = ((System.String)oForm.DataSources.UserDataSources.Item("TipoDoc").Value).Trim();

                oGrid = ((Grid)oForm.Items.Item("grid").Specific);
                odt = oGrid.DataTable;

                for (Int32 i = 0; i < odt.Rows.Count; i++)
                {
                    if (oGrid.Rows.IsSelected(i))
                    {
                        DocEntryDocNuevo = ((System.Int32)odt.GetValue("DocEntry", i));
                        ObjTypeDocNuevo = ((System.String)odt.GetValue("ObjType", i));
                        DocSubTypeDocNuevo = ((System.String)odt.GetValue("DocSubType", i));
                        break;
                    }
                }

                if (DocEntryDocNuevo == 0)
                {
                    FSBOApp.StatusBar.SetText("Debe seleccionar un documento para asignar el folio " + FolioRechazado, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    return;
                }

                //se necesita estos datos para crear timbre electronico en caso que sea necesario marca paremetros
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT T0.DocEntry, T1.LineId, REPLACE(A0.TaxIdNum,'.','') 'TaxIdNum', T2.U_CAF 'CAF', ISNULL(P0.U_GenerarT,'N') 'GenerarT'
                          FROM [@VID_FEDIST] T0
                          JOIN [@VID_FEDISTD] T1 ON T1.DocEntry = T0.DocEntry
                          JOIN [@VID_FECAF] T2 ON T2.Code = T0.U_RangoF
                          ,OADM A0, [@VID_FEPARAM] P0
                         WHERE T0.U_TipoDoc = '{0}'
                           AND T0.U_Sucursal = 'Principal'
                           AND T1.U_Folio = {1}";
                else
                    s = @"SELECT T0.""DocEntry"", T1.""LineId"", REPLACE(A0.""TaxIdNum"",'.','') ""TaxIdNum"", T2.""U_CAF"" ""CAF"", IFNULL(P0.""U_GenerarT"",'N') ""GenerarT""
                          FROM ""@VID_FEDIST"" T0
                          JOIN ""@VID_FEDISTD"" T1 ON T1.""DocEntry"" = T0.""DocEntry""
                          JOIN ""@VID_FECAF"" T2 ON T2.""Code"" = T0.""U_RangoF""
                          ,""OADM"" A0, ""@VID_FEPARAM"" P0
                         WHERE T0.""U_TipoDoc"" = '{0}'
                           AND T0.""U_Sucursal"" = 'Principal'
                           AND T1.""U_Folio"" = {1}";
                s = String.Format(s, TipoDocElect, FolioRechazado);
                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                    throw new Exception("No se ha encontrado número de Folio");
                else
                {
                    var TaxIdNum = (System.String)(oRecordSet.Fields.Item("TaxIdNum").Value).ToString().Trim();
                    var CAF = (System.String)(oRecordSet.Fields.Item("CAF").Value).ToString().Trim();
                    var FDocEntry = (System.Int32)(oRecordSet.Fields.Item("DocEntry").Value);
                    var FLineId = (System.Int32)(oRecordSet.Fields.Item("LineId").Value);
                    var Timbre = (((System.String)oRecordSet.Fields.Item("GenerarT").Value).Trim() == "Y");
                    if (TaxIdNum == "")
                        throw new Exception("Debe ingresar RUT de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1");

                    if (ObjTypeDocNuevo == "13")
                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices));
                    else if (ObjTypeDocNuevo == "14")
                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes));
                    else if (ObjTypeDocNuevo == "19")
                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseCreditNotes));
                    else if (ObjTypeDocNuevo == "15")
                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes));
                    else if (ObjTypeDocNuevo == "203")
                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments));
                    else if (ObjTypeDocNuevo == "18")
                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices));
                    else if (ObjTypeDocNuevo == "21")
                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseReturns));
                    else if (ObjTypeDocNuevo == "67")
                        oStockTransfer = (SAPbobsCOM.StockTransfer)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer));
                    else if (ObjTypeDocNuevo == "204")
                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseDownPayments));

                    if ((ObjTypeDocNuevo == "67") && (oStockTransfer != null))
                    {
                        if (oStockTransfer.GetByKey(DocEntryDocNuevo))
                        {
                            oStockTransfer.FolioNumber = FSBOf.StrToInteger(FolioRechazado);
                            oStockTransfer.FolioPrefixString = "GE";
                            //oTransfer.Printed := BoYesNoEnum.tYES;

                            lRetCode = oStockTransfer.Update();
                            if (lRetCode != 0)
                            {
                                //bFolioAsignado = false;
                                s = FCmpny.GetLastErrorDescription();
                                FSBOApp.StatusBar.SetText("No se ha asignado Folio al Documento, " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                OutLog("No se ha asignado Folio al Documento DocEntry: " + DocEntryDocNuevo + " ObjType: " + ObjTypeDocNuevo + " Documento Electronico: " + TipoDocElect + " -  " + s);
                            }
                            else
                            {
                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = "update [@VID_FEDISTD] set U_Estado = 'U', U_DocEntry = {0}, U_ObjType = '{1}', U_SubType = '{2}' where DocEntry = {3} and LineId = {4}";
                                else
                                    s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'U', ""U_DocEntry"" = {0}, ""U_ObjType"" = '{1}', ""U_SubType"" = '{2}' where ""DocEntry"" = {3} and ""LineId"" = {4}";
                                s = String.Format(s, DocEntryDocNuevo, ObjTypeDocNuevo, DocSubTypeDocNuevo, FDocEntry, FLineId);
                                oRecordSet.DoQuery(s);
                                //bFolioAsignado = true;

                                //ahora debo marcar que el folio fue usado y colocar los datos del documento que uso el folio
                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = "update [@VID_FEDISTD] set U_Estado = 'U', U_DocEntry = {0}, U_ObjType = '{1}', U_SubType = '{2}' where DocEntry = {3} and LineId = {4}";
                                else
                                    s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'U', ""U_DocEntry"" = {0}, ""U_ObjType"" = '{1}', ""U_SubType"" = '{2}' where ""DocEntry"" = {3} and ""LineId"" = {4}";
                                s = String.Format(s, DocEntryDocNuevo, ObjTypeDocNuevo, DocSubTypeDocNuevo, FDocEntry, FLineId);
                                oRecordSet.DoQuery(s);
                                //ahora dejo en blanco el folio en el documento original
                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = "UPDATE {0} SET FolioNum = NULL, FolioPref = NULL, LPgFolioN = NULL, U_FETimbre = NULL WHERE DocEntry = {1} AND ObjType = '{2}' AND DocSubType = '{3}'";
                                else
                                    s = @"UPDATE ""{0}"" SET ""FolioNum"" = NULL, ""FolioPref"" = NULL, ""LPgFolioN"" = NULL, ""U_FETimbre"" = NULL WHERE ""DocEntry"" = {1} AND ""ObjType"" = '{2}' AND ""DocSubType"" = '{3}'";
                                s = String.Format(s, TablaRechazado, DocEntryRechazado, ObjTypeRechazado, DocSubTypeRechazado);
                                oRecordSet.DoQuery(s);
                                //------------
                                //Inserto registro en tabla de rechazos 
                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = @"SELECT COUNT(*) 'Cont' 
                                                FROM [@VID_FERECHAZO] 
                                               WHERE U_TipoDoc = '{0}' 
                                                 AND U_Folio = {1} 
                                                 AND U_ObjTypeO = '{2}' 
                                                 AND U_DocEntryO = {3} 
                                                 AND U_SubTypeO = '{4}'";
                                else
                                    s = @"SELECT COUNT(*) ""Cont""
                                                FROM ""@VID_FERECHAZO""
                                               WHERE ""U_TipoDoc"" = '{0}' 
                                                 AND ""U_Folio"" = {1} 
                                                 AND ""U_ObjTypeO"" = '{2}' 
                                                 AND ""U_DocEntryO"" = {3} 
                                                 AND ""U_SubTypeO"" = '{4}'";
                                s = String.Format(s, TipoDocElect, FolioRechazado, ObjTypeRechazado, DocEntryRechazado, DocSubTypeRechazado);
                                oRecordSet.DoQuery(s);

                                if (((System.Int32)oRecordSet.Fields.Item("Cont").Value) == 0)
                                {
                                    var oo = Funciones.FERechazosAdd(TipoDocElect, FSBOf.StrToInteger(FolioRechazado), FSBOf.StrToInteger(DocEntryRechazado), DocSubTypeRechazado, ObjTypeRechazado, DocEntryDocNuevo, DocSubTypeDocNuevo, ObjTypeDocNuevo);
                                    if (oo == 0)
                                    {
                                        FSBOApp.StatusBar.SetText("No se ha a registrado datos en Tabla de Rechazos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                        OutLog("No se ha a registrado datos en Tabla de Rechazos, Folio " + FolioRechazado + " TipoDTE " + TipoDocElect);
                                    }
                                }
                                //borra registro en FELOG
                                Funciones.DelDataSource("D", "VID_FELOG", "", FSBOf.StrToInteger(EntryLOG));

                                FSBOApp.MessageBox("El Folio ha sido reutilizado en el documento seleccionado. No olvidar cancelar el documento rechazado para efectos contables");

                                if (Timbre == true)
                                {
                                    if (GlobalSettings.RunningUnderSQLServer)
                                        s = @"SELECT C0.LicTradNum, ROUND(T0.DocTotal,0) DocTotal FROM OWTR T0 JOIN OCRD C0 ON C0.CardCode = T0.CardCode WHERE T0.DocEntry = {0}";
                                    else
                                        s = @"SELECT C0.""LicTradNum"", ROUND(T0.""DocTotal"",0) ""DocTotal"" FROM ""OWTR"" T0 JOIN ""OCRD"" C0 ON C0.""CardCode"" = T0.""CardCode"" WHERE T0.""DocEntry"" = {0}";
                                    s = String.Format(s, DocEntryDocNuevo);
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
                                        if (oStockTransfer.GetByKey(Convert.ToInt32(DocEntryDocNuevo)))
                                        {
                                            oStockTransfer.UserFields.Fields.Item("U_FETimbre").Value = s;
                                            lRetCode = oStockTransfer.Update();
                                            if (lRetCode != 0)
                                            {
                                                s = FCmpny.GetLastErrorDescription();
                                                FSBOApp.StatusBar.SetText("No se ha creado Timbre en el documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                OutLog("No se ha creado Timbre en el documento: " + DocEntryDocNuevo + " Tipo: 67 - " + s);
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
                                    s = String.Format(s, DocEntryDocNuevo);
                                    oRecordSet.DoQuery(s);
                                }

                                var oDeliveryNote = new TDeliveryNote();
                                oDeliveryNote.SBO_f = FSBOf;
                                oDeliveryNote.EnviarFE_WebService(DocEntryDocNuevo.ToString(), "--", true, true, false, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52T", "67", false);
                            }
                        }
                    }
                    else if (oDocument != null)
                    {
                        if (oDocument.GetByKey(DocEntryDocNuevo))
                        {
                            if (oDocument.FolioNumber == 0)
                            {
                                oDocument.FolioNumber = FSBOf.StrToInteger(FolioRechazado);
                                if (TipoDocElect == "46") //Factura Compra
                                    oDocument.FolioPrefixString = "FC";
                                else if (TipoDocElect == "52") //Guias
                                    oDocument.FolioPrefixString = "GE";
                                else if (TipoDocElect == "33") //Facturas
                                    oDocument.FolioPrefixString = "FE";
                                else if (TipoDocElect == "34") //Factura Exenta
                                    oDocument.FolioPrefixString = "EE";
                                else if (TipoDocElect == "56") //Nota Debito
                                    oDocument.FolioPrefixString = "ND";
                                else if (TipoDocElect == "39") //Boleta
                                    oDocument.FolioPrefixString = "BE";
                                else if (TipoDocElect == "41") //Boleta Exenta
                                    oDocument.FolioPrefixString = "BX";
                                else if ((TipoDocElect == "110") && (TTipoDoc == "110R")) //Factura Exportacion
                                    oDocument.FolioPrefixString = "FX";
                                else if (TipoDocElect == "110") //Factura Exportacion
                                    oDocument.FolioPrefixString = "FX";
                                else if (TipoDocElect == "61") //Nota de Credito
                                    oDocument.FolioPrefixString = "NC";
                                else if (TipoDocElect == "111") //Nota Debito exportacion
                                    oDocument.FolioPrefixString = "ND";
                                else if (TipoDocElect == "112") //Nota de Credito exportacion
                                    oDocument.FolioPrefixString = "NC";
                                oDocument.Printed = PrintStatusEnum.psYes;


                                lRetCode = oDocument.Update();
                                if (lRetCode != 0)
                                {
                                    //bFolioAsignado = false;
                                    s = FCmpny.GetLastErrorDescription();
                                    FSBOApp.StatusBar.SetText("No se ha asignado Folio al Documento, " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    OutLog("No se ha asignado Folio al Documento DocEntry: " + DocEntryDocNuevo.ToString() + " ObjType: " + ObjTypeDocNuevo + " Documento Electronico: " + TipoDocElect + " - " + s);
                                }
                                else
                                {
                                    //ahora debo marcar que el folio fue usado y colocar los datos del documento que uso el folio
                                    if (GlobalSettings.RunningUnderSQLServer)
                                        s = "update [@VID_FEDISTD] set U_Estado = 'U', U_DocEntry = {0}, U_ObjType = '{1}', U_SubType = '{2}' where DocEntry = {3} and LineId = {4}";
                                    else
                                        s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'U', ""U_DocEntry"" = {0}, ""U_ObjType"" = '{1}', ""U_SubType"" = '{2}' where ""DocEntry"" = {3} and ""LineId"" = {4}";
                                    s = String.Format(s, DocEntryDocNuevo, ObjTypeDocNuevo, DocSubTypeDocNuevo, FDocEntry, FLineId);
                                    oRecordSet.DoQuery(s);
                                    //ahora dejo en blanco el folio en el documento original
                                    if (GlobalSettings.RunningUnderSQLServer)
                                        s = "UPDATE {0} SET FolioNum = NULL, FolioPref = NULL, LPgFolioN = NULL, U_FETimbre = NULL WHERE DocEntry = {1} AND ObjType = '{2}' AND DocSubType = '{3}'";
                                    else
                                        s = @"UPDATE ""{0}"" SET ""FolioNum"" = NULL, ""FolioPref"" = NULL, ""LPgFolioN"" = NULL, ""U_FETimbre"" = NULL WHERE ""DocEntry"" = {1} AND ""ObjType"" = '{2}' AND ""DocSubType"" = '{3}'";
                                    s = String.Format(s, TablaRechazado, DocEntryRechazado, ObjTypeRechazado, DocSubTypeRechazado);
                                    oRecordSet.DoQuery(s);
                                    //------------
                                    //Inserto registro en tabla de rechazos 
                                    if (GlobalSettings.RunningUnderSQLServer)
                                        s = @"SELECT COUNT(*) 'Cont' 
                                                FROM [@VID_FERECHAZO] 
                                               WHERE U_TipoDoc = '{0}' 
                                                 AND U_Folio = {1} 
                                                 AND U_ObjTypeO = '{2}' 
                                                 AND U_DocEntryO = {3} 
                                                 AND U_SubTypeO = '{4}'";
                                    else
                                        s = @"SELECT COUNT(*) ""Cont""
                                                FROM ""@VID_FERECHAZO""
                                               WHERE ""U_TipoDoc"" = '{0}' 
                                                 AND ""U_Folio"" = {1} 
                                                 AND ""U_ObjTypeO"" = '{2}' 
                                                 AND ""U_DocEntryO"" = {3} 
                                                 AND ""U_SubTypeO"" = '{4}'";
                                    s = String.Format(s, TipoDocElect, FolioRechazado, ObjTypeRechazado, DocEntryRechazado, DocSubTypeRechazado);
                                    oRecordSet.DoQuery(s);

                                    if (((System.Int32)oRecordSet.Fields.Item("Cont").Value) == 0)
                                    {
                                        var oo = Funciones.FERechazosAdd(TipoDocElect, FSBOf.StrToInteger(FolioRechazado), FSBOf.StrToInteger(DocEntryRechazado), DocSubTypeRechazado, ObjTypeRechazado, DocEntryDocNuevo, DocSubTypeDocNuevo, ObjTypeDocNuevo);
                                        if (oo == 0)
                                        {
                                            FSBOApp.StatusBar.SetText("No se ha a registrado datos en Tabla de Rechazos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                            OutLog("No se ha a registrado datos en Tabla de Rechazos, Folio " + FolioRechazado + " TipoDTE " + TipoDocElect);
                                        }
                                    }
                                    //borra registro en FELOG
                                    Funciones.DelDataSource("D", "VID_FELOG", "", FSBOf.StrToInteger(EntryLOG));

                                    FSBOApp.MessageBox("El Folio ha sido reutilizado en el documento seleccionado. No olvidar cancelar el documento rechazado para efectos contables");
                                    //bFolioAsignado = true;

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
                                            if (oDocument.GetByKey(Convert.ToInt32(DocEntryDocNuevo)))
                                            {
                                                oDocument.UserFields.Fields.Item("U_FETimbre").Value = s;
                                                lRetCode = oDocument.Update();
                                                if (lRetCode != 0)
                                                {
                                                    FSBOApp.StatusBar.SetText("No se ha creado Timbre en el documento - DocEntry: " + DocEntryDocNuevo + " ObjType: " + ObjTypeDocNuevo + " Documento Electronico: " + TipoDocElect + " - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                    OutLog("No se ha creado Timbre en el documento: " + DocEntryDocNuevo + " Tipo: " + TipoDocElect + " - " + s);
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
                                        oInvoice.EnviarFE_WebService(ObjTypeDocNuevo, oDocument, TipoDocElect, false, "", GlobalSettings.RunningUnderSQLServer, DocSubTypeRechazado, TTipoDoc, false);
                                    }
                                    else if ((TTipoDoc == "46") || (TTipoDoc == "46A"))
                                    {
                                        var oPurchaseInvoice = new TPurchaseInvoice();
                                        oPurchaseInvoice.SBO_f = FSBOf;
                                        oPurchaseInvoice.EnviarFE_WebService(DocEntryDocNuevo.ToString(), DocSubTypeDocNuevo, ObjTypeDocNuevo, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "46", TTipoDoc, false);
                                    }
                                    else if ((TTipoDoc == "61") || (TTipoDoc == "61C") || (TTipoDoc == "112"))
                                    {
                                        var oCreditNotes = new TCreditNotes();
                                        oCreditNotes.SBO_f = FSBOf;
                                        oCreditNotes.EnviarFE_WebServiceNotaCredito(DocEntryDocNuevo.ToString(), DocSubTypeDocNuevo, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, ObjTypeDocNuevo, TipoDocElect, TTipoDoc, false);
                                    }
                                    //else if (sTipo in ['52','52T','52D'])
                                    else if (FE52.Contains(TTipoDoc))
                                    {
                                        var oDeliveryNote = new TDeliveryNote();
                                        oDeliveryNote.SBO_f = FSBOf;
                                        if (TTipoDoc == "52")
                                            oDeliveryNote.EnviarFE_WebService(DocEntryDocNuevo.ToString(), DocSubTypeDocNuevo, false, true, false, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, TTipoDoc, "15", false);
                                        else if (TTipoDoc == "52T")
                                            oDeliveryNote.EnviarFE_WebService(DocEntryDocNuevo.ToString(), DocSubTypeDocNuevo, true, true, false, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, TTipoDoc, "67", false);
                                        else if (TTipoDoc == "52D")
                                            oDeliveryNote.EnviarFE_WebService(DocEntryDocNuevo.ToString(), DocSubTypeDocNuevo, false, true, true, false, null, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, TTipoDoc, "21", false);
                                    }

                                    //******************************
                                }
                            }
                        }
                    }
                    oDocument = null;
                    oStockTransfer = null;

                    ((EditText)oForm.Items.Item("Folio").Specific).Value = "";
                    odt.Rows.Clear();
                    ((StaticText)oForm.Items.Item("lbDatos").Specific).Caption = "";
                }
            }
            catch (Exception e)
            {
                if (FCmpny.InTransaction)
                    FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                FSBOApp.StatusBar.SetText("Crear: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("Crear: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }

    }//fin class
}


