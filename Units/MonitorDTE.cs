using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Configuration;
using System.Xml;
using System.IO;
using System.Data;
using System.Threading;
using System.Data.SqlClient;
using System.Drawing;
using System.ComponentModel;
//using System.Windows.Forms;
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
using Factura_Electronica_VK.Functions;

namespace Factura_Electronica_VK.MonitorDTE
{
    public class TMonitorDTE : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.DBDataSource oDBDSHeader;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.ComboBox oComboBox;
        private SAPbouiCOM.Grid oGrid;
        private SAPbouiCOM.EditText oEditText;
        private SAPbouiCOM.DataTable oDataTable;
        private SAPbouiCOM.DBDataSource oDBDSHC;
        private SAPbouiCOM.DBDataSource oDBDSHV;
        private SAPbouiCOM.UserDataSource DSOpFec;
        private TFunctions Funciones = new TFunctions();
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;

        public new bool InitForm(string uid, string xmlPath, ref SAPbouiCOM.Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            Int32 CantRol;

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            Funciones.SBO_f = FSBOf;
            try
            {
                Lista = new List<string>();
                FSBOf.LoadForm(xmlPath, "VID_FEMonDTE.srf", uid);
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

                oDBDSHC = oForm.DataSources.DBDataSources.Add("@VID_FEDTECPRA");
                oDBDSHV = oForm.DataSources.DBDataSources.Add("@VID_FEDTEVTA");

                oForm.DataSources.UserDataSources.Add("FechaD", BoDataType.dt_DATE, 10);
                oEditText = (EditText)(oForm.Items.Item("FechaD").Specific);
                oEditText.DataBind.SetBound(true, "", "FechaD");
                oEditText.Value = DateTime.Now.ToString("yyyyMMdd");

                oForm.DataSources.UserDataSources.Add("FechaH", BoDataType.dt_DATE, 10);
                oEditText = (EditText)(oForm.Items.Item("FechaH").Specific);
                oEditText.DataBind.SetBound(true, "", "FechaH");
                oEditText.Value = DateTime.Now.ToString("yyyyMMdd");

                oComboBox = (ComboBox)(oForm.Items.Item("TipoDTE").Specific);
                oForm.DataSources.UserDataSources.Add("TipoDTE", BoDataType.dt_SHORT_TEXT, 10);
                oComboBox.DataBind.SetBound(true, "", "TipoDTE");
                oComboBox.ValidValues.Add("V", "Venta");
                oComboBox.ValidValues.Add("C", "Compra");
                oComboBox.Select("V", BoSearchKey.psk_ByValue);
                oForm.Items.Item("TipoDTE").DisplayDesc = true;

                oDataTable = oForm.DataSources.DataTables.Add("dt");
                oGrid = (Grid)(oForm.Items.Item("grid").Specific);
                oGrid.DataTable = oDataTable;

                DSOpFec = oForm.DataSources.UserDataSources.Add("FechaEmi", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                ((OptionBtn)oForm.Items.Item("FechaEmi").Specific).DataBind.SetBound(true, "", "FechaEmi");

                ((OptionBtn)oForm.Items.Item("FechaRecep").Specific).DataBind.SetBound(true, "", "FechaEmi");
                ((OptionBtn)oForm.Items.Item("FechaRecep").Specific).GroupWith("FechaEmi");
                ((OptionBtn)oForm.Items.Item("FechaEmi").Specific).Selected = true;


                oComboBox = (ComboBox)(oForm.Items.Item("Cliente").Specific);
                oForm.DataSources.UserDataSources.Add("Cliente", BoDataType.dt_SHORT_TEXT, 10);
                oComboBox.DataBind.SetBound(true, "", "Cliente");
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT 'Todos' Code, 'Todos' Name UNION ALL 
                          SELECT T1.FldValue Code, T1.Descr Name
                          FROM CUFD T0
                          JOIN UFD1 T1 ON T1.TableID = T0.TableID
                                      AND T1.FieldID = T0.FieldID
                         WHERE T0.TableID = '{0}'
                           AND T0.AliasID = 'EstadoC'";
                else
                    s = @"SELECT 'Todos' ""Code"", 'Todos' ""Name"" FROM DUMMY UNION ALL
                          SELECT T1.""FldValue"" ""Code"", T1.""Descr"" ""Name""
                          FROM ""CUFD"" T0
                          JOIN ""UFD1"" T1 ON T1.""TableID"" = T0.""TableID""
                                      AND T1.""FieldID"" = T0.""FieldID""
                         WHERE T0.""TableID"" = '{0}'
                           AND T0.""AliasID"" = 'EstadoC'";
                s = String.Format(s, "@VID_FEDTEVTA");
                oRecordSet.DoQuery(s);
                FSBOf.FillCombo(oComboBox, ref oRecordSet, false);
                oComboBox.Select("Todos", BoSearchKey.psk_ByValue);

                oComboBox = (ComboBox)(oForm.Items.Item("SII").Specific);
                oForm.DataSources.UserDataSources.Add("SII", BoDataType.dt_SHORT_TEXT, 10);
                oComboBox.DataBind.SetBound(true, "", "SII");
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT 'Todos' Code, 'Todos' Name UNION ALL
                          SELECT T1.FldValue Code, T1.Descr Name
                          FROM CUFD T0
                          JOIN UFD1 T1 ON T1.TableID = T0.TableID
                                      AND T1.FieldID = T0.FieldID
                         WHERE T0.TableID = '{0}'
                           AND T0.AliasID = 'EstadoSII'";
                else
                    s = @"SELECT 'Todos' ""Code"", 'Todos' ""Name"" FROM DUMMY UNION ALL
                          SELECT T1.""FldValue"" ""Code"", T1.""Descr"" ""Name""
                          FROM ""CUFD"" T0
                          JOIN ""UFD1"" T1 ON T1.""TableID"" = T0.""TableID""
                                      AND T1.""FieldID"" = T0.""FieldID""
                         WHERE T0.""TableID"" = '{0}'
                           AND T0.""AliasID"" = 'EstadoSII'";
                s = String.Format(s, "@VID_FEDTEVTA");
                oRecordSet.DoQuery(s);
                FSBOf.FillCombo(oComboBox, ref oRecordSet, false);
                oComboBox.Select("Todos", BoSearchKey.psk_ByValue);

                oComboBox = (ComboBox)(oForm.Items.Item("Ley").Specific);
                oForm.DataSources.UserDataSources.Add("Ley", BoDataType.dt_SHORT_TEXT, 10);
                oComboBox.DataBind.SetBound(true, "", "Ley");
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT 'Todos' Code, 'Todos' Name UNION ALL
                          SELECT T1.FldValue Code, T1.Descr Name
                          FROM CUFD T0
                          JOIN UFD1 T1 ON T1.TableID = T0.TableID
                                      AND T1.FieldID = T0.FieldID
                         WHERE T0.TableID = '{0}'
                           AND T0.AliasID = 'EstadoLey'";
                else
                    s = @"SELECT 'Todos' ""Code"", 'Todos' ""Name"" FROM DUMMY UNION ALL
                          SELECT T1.""FldValue"" ""Code"", T1.""Descr"" ""Name""
                          FROM ""CUFD"" T0
                          JOIN ""UFD1"" T1 ON T1.""TableID"" = T0.""TableID""
                                      AND T1.""FieldID"" = T0.""FieldID""
                         WHERE T0.""TableID"" = '{0}'
                           AND T0.""AliasID"" = 'EstadoLey'";
                s = String.Format(s, "@VID_FEDTEVTA");
                oRecordSet.DoQuery(s);
                FSBOf.FillCombo(oComboBox, ref oRecordSet, false);
                oComboBox.Select("Todos", BoSearchKey.psk_ByValue);

                BuscarDatos();
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
                    var DateD = DateTime.ParseExact(((EditText)oForm.Items.Item("FechaD").Specific).Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    var DateH = DateTime.ParseExact(((EditText)oForm.Items.Item("FechaH").Specific).Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    TimeSpan ts = DateH - DateD;
                    var differenceInDays = ts.Days + 1;
                    if (differenceInDays > 31)
                        FSBOApp.StatusBar.SetText("El intervalo de fechas no puede ser superior a 30 dias", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    else
                        BuscarDatos();
                }

                if ((pVal.EventType == BoEventTypes.et_DOUBLE_CLICK) && (!pVal.BeforeAction) && (pVal.ColUID == "Folio"))
                {
                    var TipoDTE = ((System.String)((ComboBox)oForm.Items.Item("TipoDTE").Specific).Selected.Value).Trim();
                    if (TipoDTE == "C")
                        MostrarPDF(pVal.Row);
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


        private void BuscarDatos()
        {
            String FechaD;
            String FechaH;
            String TipoDTE;
            try
            {
                oForm.Freeze(true);
                FechaD = ((System.String)((EditText)oForm.Items.Item("FechaD").Specific).Value);
                FechaH = ((System.String)((EditText)oForm.Items.Item("FechaH").Specific).Value);
                TipoDTE = ((System.String)((ComboBox)oForm.Items.Item("TipoDTE").Specific).Selected.Value).Trim();

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT T0.DocEntry
                              ,T0.U_TipoDoc TipoDoc
	                          ,T0.U_Folio Folio
	                          ,T0.U_RUT RUT
                              ,T0.U_Razon Razon
	                          ,T0.U_FechaEmi FechaEmi
	                          ,CAST(REPLACE(CONVERT(CHAR(10), T0.U_FechaRecep, 102),'.','-') +'  '+ 
								                            CASE WHEN LEN(T0.U_HoraRecep) = 4 THEN LEFT(CAST(T0.U_HoraRecep AS VARCHAR(10)),2) + ':' + RIGHT(CAST(T0.U_HoraRecep AS VARCHAR(10)),2) + ':00'
									 	                         WHEN LEN(T0.U_HoraRecep) = 3 THEN '0' + LEFT(CAST(T0.U_HoraRecep AS VARCHAR(10)),1) + ':' + RIGHT(CAST(T0.U_HoraRecep AS VARCHAR(10)),2) + ':00'
									 	                         WHEN LEN(T0.U_HoraRecep) = 2 THEN '00:'+ CAST(T0.U_HoraRecep AS VARCHAR(10)) + ':00'
										                         WHEN LEN(T0.U_HoraRecep) = 1 THEN '00:0' + CAST(T0.U_HoraRecep AS VARCHAR(10)) + ':00'
										                         ELSE '00:00:00'
								                            END AS VARCHAR(50)) FechaRecep
	                          ,T0.U_Monto Monto
	                          ,T0.U_IVA IVA
	                          ,T0.U_EstadoC EstadoC
	                          ,T0.U_EstadoSII EstadoSII
	                          ,T0.U_EstadoLey EstadoLey
                              ,CAST(T0.U_DocEntry AS INT) DocEntryDoc
                              ,{4} 'xml'  
                          FROM [{2}] T0
                         WHERE 1 = 1
                           AND T0.{3} BETWEEN '{0}' AND '{1}'";
                else
                    s = @"SELECT T0.""DocEntry""
                              ,T0.""U_TipoDoc"" ""TipoDoc""
	                          ,T0.""U_Folio"" ""Folio""
	                          ,T0.""U_RUT"" ""RUT""
                              ,T0.""U_Razon"" ""Razon""
	                          ,T0.""U_FechaEmi"" ""FechaEmi""
	                          ,CAST(TO_VARCHAR(T0.""U_FechaRecep"", 'yyyy-MM-dd') ||'  '|| 
								   CASE WHEN LENGTH(T0.""U_HoraRecep"") = 4 THEN LEFT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),2) || ':' || RIGHT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),2) || ':00'
										WHEN LENGTH(T0.""U_HoraRecep"") = 3 THEN '0' || LEFT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),1) || ':' || RIGHT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),2) || ':00'
										WHEN LENGTH(T0.""U_HoraRecep"") = 2 THEN '00:' || CAST(T0.""U_HoraRecep"" AS VARCHAR(10)) || ':00'
										WHEN LENGTH(T0.""U_HoraRecep"") = 1 THEN '00:0' || CAST(T0.""U_HoraRecep"" AS VARCHAR(10)) || ':00'
										ELSE '00:00:00'
								   END AS VARCHAR(50)) ""FechaRecep""
	                          ,T0.""U_Monto"" ""Monto""
	                          ,T0.""U_IVA"" ""IVA""
	                          ,T0.""U_EstadoC"" ""EstadoC""
	                          ,T0.""U_EstadoSII"" ""EstadoSII""
	                          ,T0.""U_EstadoLey"" ""EstadoLey""
                              ,TO_INT(T0.""U_DocEntry"") ""DocEntryDoc""
                              ,{4} ""xml""  
                          FROM ""{2}"" T0
                         WHERE 1 = 1
                           AND T0.""{3}"" BETWEEN '{0}' AND '{1}'";
                s = String.Format(s, FechaD, FechaH, (TipoDTE == "V" ? "@VID_FEDTEVTA" : "@VID_FEDTECPRA"), (DSOpFec.Value == "1" ? "U_FechaEmi" : "U_FechaRecep"), (TipoDTE == "C" ? (GlobalSettings.RunningUnderSQLServer ? @"T0.U_Xml" : @"T0.""U_Xml"" ") : "''"));

                if (((System.String)((ComboBox)oForm.Items.Item("Cliente").Specific).Selected.Value).Trim() != "Todos")
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = s + @" AND T0.U_EstadoC = '{0}'";
                    else
                        s = s + @" AND T0.""U_EstadoC"" = '{0}'";
                    s = String.Format(s, ((System.String)((ComboBox)oForm.Items.Item("Cliente").Specific).Selected.Value).Trim());
                }

                if (((System.String)((ComboBox)oForm.Items.Item("SII").Specific).Selected.Value).Trim() != "Todos")
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = s + @" AND T0.U_EstadoSII = '{0}'";
                    else
                        s = s + @" AND T0.""U_EstadoSII"" = '{0}'";
                    s = String.Format(s, ((System.String)((ComboBox)oForm.Items.Item("SII").Specific).Selected.Value).Trim());
                }

                if (((System.String)((ComboBox)oForm.Items.Item("Ley").Specific).Selected.Value).Trim() != "Todos")
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = s + @" AND T0.U_EstadoLey = '{0}'";
                    else
                        s = s + @" AND T0.""U_EstadoLey"" = '{0}'";
                    s = String.Format(s, ((System.String)((ComboBox)oForm.Items.Item("Ley").Specific).Selected.Value).Trim());
                }

                oDataTable.ExecuteQuery(s);

                oGrid.Columns.Item("DocEntry").Type = BoGridColumnType.gct_EditText;
                var col = (EditTextColumn)(oGrid.Columns.Item("DocEntry"));
                col.Visible = false;

                oGrid.Columns.Item("TipoDoc").Type = BoGridColumnType.gct_EditText;
                col = (EditTextColumn)(oGrid.Columns.Item("TipoDoc"));
                col.Editable = false;
                col.TitleObject.Caption = "Tipo DTE";
                col.TitleObject.Sortable = true;

                oGrid.Columns.Item("Folio").Type = BoGridColumnType.gct_EditText;
                col = ((EditTextColumn)oGrid.Columns.Item("Folio"));
                col.Editable = false;
                col.RightJustified = true;
                col.TitleObject.Sortable = true;
                col.TitleObject.Caption = "Nro Folio";

                oGrid.Columns.Item("RUT").Type = BoGridColumnType.gct_EditText;
                col = ((EditTextColumn)oGrid.Columns.Item("RUT"));
                col.Editable = false;
                col.TitleObject.Sortable = true;
                col.TitleObject.Caption = "RUT";
                if (TipoDTE == "C")
                    col.Visible = true;
                else
                    col.Visible = false;

                oGrid.Columns.Item("Razon").Type = BoGridColumnType.gct_EditText;
                col = ((EditTextColumn)oGrid.Columns.Item("Razon"));
                col.Editable = false;
                col.TitleObject.Sortable = true;
                col.TitleObject.Caption = "Razon Social";

                oGrid.Columns.Item("FechaEmi").Type = BoGridColumnType.gct_EditText;
                col = ((EditTextColumn)oGrid.Columns.Item("FechaEmi"));
                col.Editable = false;
                col.TitleObject.Sortable = true;
                col.TitleObject.Caption = "Fecha Emisión";

                oGrid.Columns.Item("FechaRecep").Type = BoGridColumnType.gct_EditText;
                col = ((EditTextColumn)oGrid.Columns.Item("FechaRecep"));
                col.Editable = false;
                col.TitleObject.Sortable = true;
                col.TitleObject.Caption = "Fecha Recepción";

                oGrid.Columns.Item("Monto").Type = BoGridColumnType.gct_EditText;
                col = ((EditTextColumn)oGrid.Columns.Item("Monto"));
                col.Editable = false;
                col.RightJustified = true;
                col.TitleObject.Sortable = true;
                col.TitleObject.Caption = "Monto";

                oGrid.Columns.Item("IVA").Type = BoGridColumnType.gct_EditText;
                col = ((EditTextColumn)oGrid.Columns.Item("IVA"));
                col.Editable = false;
                col.RightJustified = true;
                col.TitleObject.Sortable = true;
                col.TitleObject.Caption = "IVA";

                oGrid.Columns.Item("EstadoC").Type = BoGridColumnType.gct_ComboBox;
                var colCombo = ((ComboBoxColumn)oGrid.Columns.Item("EstadoC"));
                colCombo.Editable = false;
                colCombo.DisplayType = BoComboDisplayType.cdt_both;
                colCombo.TitleObject.Sortable = true;
                colCombo.TitleObject.Caption = "Estado Cliente";
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT T1.FldValue Code, T1.Descr Name
                          FROM CUFD T0
                          JOIN UFD1 T1 ON T1.TableID = T0.TableID
                                      AND T1.FieldID = T0.FieldID
                         WHERE T0.TableID = '{0}'
                           AND T0.AliasID = 'EstadoC'";
                else
                    s = @"SELECT T1.""FldValue"" ""Code"", T1.""Descr"" ""Name""
                          FROM ""CUFD"" T0
                          JOIN ""UFD1"" T1 ON T1.""TableID"" = T0.""TableID""
                                      AND T1.""FieldID"" = T0.""FieldID""
                         WHERE T0.""TableID"" = '{0}'
                           AND T0.""AliasID"" = 'EstadoC'";
                s = String.Format(s, (TipoDTE == "V" ? "@VID_FEDTEVTA" : "@VID_FEDTECPRA"));
                oRecordSet.DoQuery(s);
                FSBOf.FillComboGrid(((GridColumn)oGrid.Columns.Item("EstadoC")), ref oRecordSet, false);

                oGrid.Columns.Item("EstadoSII").Type = BoGridColumnType.gct_ComboBox;
                colCombo = ((ComboBoxColumn)oGrid.Columns.Item("EstadoSII"));
                colCombo.Editable = false;
                colCombo.DisplayType = BoComboDisplayType.cdt_both;
                colCombo.TitleObject.Sortable = true;
                colCombo.TitleObject.Caption = "Estado SII";
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT T1.FldValue Code, T1.Descr Name
                          FROM CUFD T0
                          JOIN UFD1 T1 ON T1.TableID = T0.TableID
                                      AND T1.FieldID = T0.FieldID
                         WHERE T0.TableID = '{0}'
                           AND T0.AliasID = 'EstadoSII'";
                else
                    s = @"SELECT T1.""FldValue"" ""Code"", T1.""Descr"" ""Name""
                          FROM ""CUFD"" T0
                          JOIN ""UFD1"" T1 ON T1.""TableID"" = T0.""TableID""
                                      AND T1.""FieldID"" = T0.""FieldID""
                         WHERE T0.""TableID"" = '{0}'
                           AND T0.""AliasID"" = 'EstadoSII'";
                s = String.Format(s, (TipoDTE == "V" ? "@VID_FEDTEVTA" : "@VID_FEDTECPRA"));
                oRecordSet.DoQuery(s);
                FSBOf.FillComboGrid(((GridColumn)oGrid.Columns.Item("EstadoSII")), ref oRecordSet, false);

                oGrid.Columns.Item("EstadoLey").Type = BoGridColumnType.gct_ComboBox;
                colCombo = ((ComboBoxColumn)oGrid.Columns.Item("EstadoLey"));
                colCombo.Editable = false;
                colCombo.DisplayType = BoComboDisplayType.cdt_both;
                colCombo.TitleObject.Sortable = true;
                colCombo.TitleObject.Caption = "Estado Ley 20.956";
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT T1.FldValue Code, T1.Descr Name
                          FROM CUFD T0
                          JOIN UFD1 T1 ON T1.TableID = T0.TableID
                                      AND T1.FieldID = T0.FieldID
                         WHERE T0.TableID = '{0}'
                           AND T0.AliasID = 'EstadoLey'";
                else
                    s = @"SELECT T1.""FldValue"" ""Code"", T1.""Descr"" ""Name""
                          FROM ""CUFD"" T0
                          JOIN ""UFD1"" T1 ON T1.""TableID"" = T0.""TableID""
                                      AND T1.""FieldID"" = T0.""FieldID""
                         WHERE T0.""TableID"" = '{0}'
                           AND T0.""AliasID"" = 'EstadoLey'";
                s = String.Format(s, (TipoDTE == "V" ? "@VID_FEDTEVTA" : "@VID_FEDTECPRA"));
                oRecordSet.DoQuery(s);
                FSBOf.FillComboGrid(((GridColumn)oGrid.Columns.Item("EstadoLey")), ref oRecordSet, false);

                oGrid.Columns.Item("DocEntryDoc").Type = BoGridColumnType.gct_EditText;
                col = ((EditTextColumn)oGrid.Columns.Item("DocEntryDoc"));
                col.Editable = false;
                col.RightJustified = true;
                if (TipoDTE == "V")
                    col.Visible = false;
                else
                    col.Visible = true;
                col.TitleObject.Sortable = false;
                col.TitleObject.Caption = "Factura en SAP";
                col.LinkedObjectType = "18";

                oGrid.Columns.Item("xml").Type = BoGridColumnType.gct_EditText;
                col = ((EditTextColumn)oGrid.Columns.Item("xml"));
                col.Editable = false;
                col.Visible = false;

                oGrid.AutoResizeColumns();
            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText(x.Message + " ** Trace: " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("BuscarDatos: " + x.Message + " ** Trace: " + x.StackTrace);
            }
            finally
            {
                oForm.Freeze(false);
            }
        }

        private void MostrarPDF(Int32 Linea)
        {
            String Code;
            String sXml;
            String TipoDoc;
            String Folio;
            String RUTEmisor;
            String oPath;
            String sNombreArchivo;
            String sNombrePDF;
            Boolean flag = true;
            String Pass = "";

            try
            {
                oGrid = (Grid)(oForm.Items.Item("grid").Specific);
                Code = Convert.ToString(((System.Int32)oGrid.DataTable.GetValue("DocEntry", Linea)), _nf);
                sXml = ((System.String)oGrid.DataTable.GetValue("xml", Linea));
                TipoDoc = ((System.String)oGrid.DataTable.GetValue("TipoDoc", Linea));
                RUTEmisor = ((System.String)oGrid.DataTable.GetValue("RUT", Linea));
                Folio = Convert.ToString(((System.Int32)oGrid.DataTable.GetValue("Folio", Linea)), _nf);
                if (sXml != "")
                {
                    oPath = System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0));
                    try
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                            sNombreArchivo = oPath + "\\Reports\\CL\\SQL\\ReporteXML.rpt";
                        else
                            sNombreArchivo = oPath + "\\Reports\\CL\\HANA\\ReporteXML.rpt";
                        sNombrePDF = oPath + @"\PDF\" + RUTEmisor + "_" + TipoDoc + "_" + Folio + ".pdf";
                        if (File.Exists(sNombrePDF))
                        {
                            System.Diagnostics.Process proc = new System.Diagnostics.Process();
                            proc.StartInfo.FileName = sNombrePDF;
                            proc.Start();
                        }
                        else
                        {

                            FSBOf.AddRepKey(Code, "FEREPORTXML", "FEREPORTXML");//oForm.TypeEx);
                            GlobalSettings.CrystalReportFileName = sNombreArchivo;
                            try
                            {
                                FSBOApp.Menus.Item("4873").Activate();
                            }
                            catch { }

                            /*FSBOApp.Menus.Item("4873").Activate();
                            var oFormB = FSBOApp.Forms.ActiveForm;
                            ((EditText)oFormB.Items.Item("410000004").Specific).Value = sNombreArchivo;
                            oFormB.Items.Item("410000005").Click(BoCellClickType.ct_Regular);*/
                        }
                    }
                    catch (Exception p)
                    {
                        FSBOApp.StatusBar.SetText(p.Message + " ** Trace: " + p.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        OutLog("Cargar Crystal: " + p.Message + " ** Trace: " + p.StackTrace);
                    }
                }
                else
                    FSBOApp.StatusBar.SetText("No se ha encontrado xml que genera PDF", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText(x.Message + " ** Trace: " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("MostrarPDF: " + x.Message + " ** Trace: " + x.StackTrace);
            }
        }


    }//fin class
}
