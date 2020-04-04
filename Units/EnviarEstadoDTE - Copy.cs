using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Configuration;
using System.Threading;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net;
using System.Linq;
using System.Data;
using System.IO;
using System.Xml;
using System.Drawing;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.ReportSource;
using CrystalDecisions.Shared;
using SAPbouiCOM;
using SAPbobsCOM;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using VisualD.SBOObjectMg1;
using VisualD.Main;
using VisualD.MainObjBase;
using VisualD.ADOSBOScriptExecute;
using Factura_Electronica_VK.Functions;
using Newtonsoft.Json;

namespace Factura_Electronica_VK.EnviarEstadoDTE
{
    public class TEnviarEstadoDTE : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.DBDataSource oDBDSHeader;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.ComboBox oComboBox;
        private SAPbouiCOM.Grid oGrid;
        private SAPbouiCOM.DataTable oDataTable;
        private SAPbouiCOM.DBDataSource oDBDSHC;
        private SAPbouiCOM.DBDataSource oDBDSHV;
        private TFunctions Funciones = new TFunctions();
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            Int32 CantRol;

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            Funciones.SBO_f = FSBOf;
            try
            {
                Lista = new List<string>();
                FSBOf.LoadForm(xmlPath, "VID_EnviarEstadoDTE.srf", uid);
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

                oDataTable = oForm.DataSources.DataTables.Add("dt");
                oGrid = (Grid)(oForm.Items.Item("grid").Specific);
                oGrid.DataTable = oDataTable;
                oGrid.SelectionMode = BoMatrixSelect.ms_Single;

                /*oComboBox = (ComboBox)(oForm.Items.Item("Cliente").Specific);
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
                */
                BuscarDocumento();
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
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction) && (pVal.ItemUID == "Buscar"))
                    BuscarDocumento();

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.ItemUID == "1") && (pVal.BeforeAction))
                {
                    BubbleEvent = false;
                    if (ActualizarEstados())
                        BuscarDocumento();
                }

                if ((pVal.EventType == BoEventTypes.et_DOUBLE_CLICK) && (!pVal.BeforeAction) && (pVal.ColUID == "U_Folio"))
                    MostrarPDF(pVal.Row);

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


        private void BuscarDocumento()
        {
            try
            {
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT T0.U_RUT
                              ,T0.U_Razon
	                          ,T0.U_TipoDoc
	                          ,T0.U_Folio
                              ,T0.U_Validacion
                              ,T0.U_FechaEmi
	                          ,CAST(REPLACE(CONVERT(CHAR(10), T0.U_FechaRecep, 102),'.','-') +'  '+ 
								                            CASE WHEN LEN(T0.U_HoraRecep) = 4 THEN LEFT(CAST(T0.U_HoraRecep AS VARCHAR(10)),2) + ':' + RIGHT(CAST(T0.U_HoraRecep AS VARCHAR(10)),2) + ':00'
									 	                         WHEN LEN(T0.U_HoraRecep) = 3 THEN '0' + LEFT(CAST(T0.U_HoraRecep AS VARCHAR(10)),1) + ':' + RIGHT(CAST(T0.U_HoraRecep AS VARCHAR(10)),2) + ':00'
									 	                         WHEN LEN(T0.U_HoraRecep) = 2 THEN '00:'+ CAST(T0.U_HoraRecep AS VARCHAR(10)) + ':00'
										                         WHEN LEN(T0.U_HoraRecep) = 1 THEN '00:0' + CAST(T0.U_HoraRecep AS VARCHAR(10)) + ':00'
										                         ELSE '00:00:00'
								                            END AS VARCHAR(50)) 'U_FechaRecep'
	                          ,T0.U_IVA
	                          ,T0.U_Monto
	                          ,T0.U_EstadoLey
                              ,T0.U_EstadoLey 'EstadoLeyOld'
                              ,T0.DocEntry
                              ,T0.U_Xml                              
                              ,ISNULL((SELECT TOP 1 A.U_FolioRef FROM [@VID_FEXMLCR] A WHERE A.Code = T0.DocEntry AND A.U_TpoDocRef = '801'),'') 'OCOri'
                              ,CAST(' ' AS VARCHAR(30)) 'OC'
                          FROM [@VID_FEDTECPRA] T0
                         WHERE (ISNULL(T0.U_EstadoLey,'') = '' OR ISNULL(T0.U_EstadoLey,'') = 'ERM')
                           AND T0.U_TipoDoc IN ('33', '34', '43')";
                else
                    s = @"SELECT T0.""U_RUT""
                              ,T0.""U_Razon""
	                          ,T0.""U_TipoDoc""
	                          ,T0.""U_Folio""
                              ,T0.""U_Validacion""
                              ,T0.""U_FechaEmi""
	                          ,CAST(TO_VARCHAR(T0.""U_FechaRecep"", 'yyyy-MM-dd') ||'T'|| 
								   CASE WHEN LENGTH(T0.""U_HoraRecep"") = 4 THEN LEFT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),2) || ':' || RIGHT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),2) || ':00'
										WHEN LENGTH(T0.""U_HoraRecep"") = 3 THEN '0' || LEFT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),1) || ':' || RIGHT(CAST(T0.""U_HoraRecep"" AS VARCHAR(10)),2) || ':00'
										WHEN LENGTH(T0.""U_HoraRecep"") = 2 THEN '00:' || CAST(T0.""U_HoraRecep"" AS VARCHAR(10)) || ':00'
										WHEN LENGTH(T0.""U_HoraRecep"") = 1 THEN '00:0' || CAST(T0.""U_HoraRecep"" AS VARCHAR(10)) || ':00'
										ELSE '00:00:00'
								   END AS VARCHAR(50)) ""U_FechaRecep""
	                          ,T0.""U_IVA""
	                          ,T0.""U_Monto""
	                          ,T0.""U_EstadoLey""
                              ,T0.""U_EstadoLey"" ""EstadoLeyOld""
                              ,T0.""DocEntry""
                              ,T0.""U_Xml""
                              ,IFNULL((SELECT MAX(A.""U_FolioRef"") FROM ""@VID_FEXMLCR"" A WHERE A.""Code"" = T0.""DocEntry"" AND A.""U_TpoDocRef"" = '801'),'') ""OCOri""
                              ,CAST(' ' AS VARCHAR(30)) ""OC""
                          FROM ""@VID_FEDTECPRA"" T0
                         WHERE (IFNULL(T0.""U_EstadoLey"",'') = '' OR IFNULL(T0.""U_EstadoLey"",'') = 'ERM')
                           AND T0.""U_TipoDoc"" IN ('33', '34', '43')";

                oGrid = (Grid)(oForm.Items.Item("grid").Specific);
                oGrid.DataTable.ExecuteQuery(s);

                oGrid.Columns.Item("U_RUT").Type = BoGridColumnType.gct_EditText;
                var oColumn = (GridColumn)(oGrid.Columns.Item("U_RUT"));
                var oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "RUT";

                oGrid.Columns.Item("U_Razon").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_Razon"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Razón Social";
                oEditColumn.Width = 220;

                oGrid.Columns.Item("U_TipoDoc").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_TipoDoc"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.RightJustified = true;
                oEditColumn.TitleObject.Caption = "Tipo Doc";

                oGrid.Columns.Item("U_Folio").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_Folio"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.RightJustified = true;
                oEditColumn.TitleObject.Caption = "Folio";

                oGrid.Columns.Item("U_Validacion").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_Validacion"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.Visible = true;
                oEditColumn.TitleObject.Caption = "Validación";

                oGrid.Columns.Item("U_FechaEmi").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_FechaEmi"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Fecha Emisión";

                oGrid.Columns.Item("U_FechaRecep").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_FechaRecep"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Fecha Recepción";

                oGrid.Columns.Item("U_IVA").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_IVA"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.RightJustified = true;
                oEditColumn.TitleObject.Caption = "IVA";

                oGrid.Columns.Item("U_Monto").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_Monto"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.RightJustified = true;
                oEditColumn.TitleObject.Caption = "Monto";

                oGrid.Columns.Item("U_EstadoLey").Type = BoGridColumnType.gct_ComboBox;
                var ocbxColumns = (GridColumn)(oGrid.Columns.Item("U_EstadoLey"));
                var ocbxColumn = (ComboBoxColumn)(ocbxColumns);
                ocbxColumn.Editable = true;
                ocbxColumn.DisplayType = BoComboDisplayType.cdt_Description;
                ocbxColumn.TitleObject.Caption = "Estado Ley 20.956";
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT T1.FldValue Code, T1.Descr Name
                          FROM CUFD T0
                          JOIN UFD1 T1 ON T1.TableID = T0.TableID
                                      AND T1.FieldID = T0.FieldID
                         WHERE T0.TableID = '@VID_FEDTEVTA'
                           AND T0.AliasID = 'EstadoLey'
                           AND T1.FldValue <> 'ACO'";
                else
                    s = @"SELECT T1.""FldValue"" ""Code"", T1.""Descr"" ""Name""
                          FROM ""CUFD"" T0
                          JOIN ""UFD1"" T1 ON T1.""TableID"" = T0.""TableID""
                                      AND T1.""FieldID"" = T0.""FieldID""
                         WHERE T0.""TableID"" = '@VID_FEDTEVTA'
                           AND T0.""AliasID"" = 'EstadoLey'
                           AND T1.""FldValue"" <> 'ACO'";
                oRecordSet.DoQuery(s);
                FSBOf.FillComboGrid(ocbxColumns, ref oRecordSet, true);

                oGrid.Columns.Item("EstadoLeyOld").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("EstadoLeyOld"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.Visible = false;

                oGrid.Columns.Item("DocEntry").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("DocEntry"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Visible = false;

                oGrid.Columns.Item("U_Xml").Type = BoGridColumnType.gct_EditText;
                oEditColumn = ((EditTextColumn)oGrid.Columns.Item("U_Xml"));
                oEditColumn.Visible = false;

                oGrid.Columns.Item("OC").Type = BoGridColumnType.gct_EditText;
                oEditColumn = ((EditTextColumn)oGrid.Columns.Item("OC"));
                oEditColumn.Visible = true;
                oEditColumn.TitleObject.Caption = "OC Manual";

                oGrid.Columns.Item("OCOri").Type = BoGridColumnType.gct_EditText;
                oEditColumn = ((EditTextColumn)oGrid.Columns.Item("OCOri"));
                oEditColumn.Visible = false;

                ColorearGrid();
                oGrid.AutoResizeColumns();

            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText("BuscarDocumento: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("BuscarDocumento: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }

        private Boolean ActualizarEstados()
        {
            String URL = "http://portal1.easydoc.cl/Consulta/GeneracionDte.aspx?RUT={0}&amp;FOLIO={1}&amp;TIPODTE={2}&amp;RUT1={3}&amp;CODAR={4}&amp;OP=31";
            String URLFinal;
            String TaxIdNum;
            String UserWS = "";
            String PassWS = "";
            WebRequest request;
            string postData;
            byte[] byteArray;
            Stream dataStream;
            WebResponse response;
            StreamReader reader;
            string responseFromServer;
            String EstadoOriginal;
            String EstadoFinal;
            String EstadoOld;
            Int32 lRetCode;
            SAPbouiCOM.Conditions oConditions;
            SAPbouiCOM.Condition oCondition;
            String EstadoDescrip;

            try
            {

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT ISNULL(TaxIdNum,'') TaxIdNum, CompnyName FROM OADM ";
                else
                    s = @"SELECT IFNULL(""TaxIdNum"",'') ""TaxIdNum"", ""CompnyName"" FROM ""OADM"" ";
                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar RUT de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return false;
                }
                else
                    TaxIdNum = ((System.String)oRecordSet.Fields.Item("TaxIdNum").Value).Trim();

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT ISNULL(U_UserWSCL,'') 'UserWS', ISNULL(U_PassWSCL,'') 'PassWS' FROM [@VID_FEPARAM]";
                else
                    s = @"SELECT IFNULL(""U_UserWSCL"",'') ""UserWS"", IFNULL(""U_PassWSCL"",'') ""PassWS"" FROM ""@VID_FEPARAM"" ";
                oRecordSet.DoQuery(s);
                if (((System.String)oRecordSet.Fields.Item("UserWS").Value).Trim() != "")
                    UserWS = Funciones.DesEncriptar(((System.String)oRecordSet.Fields.Item("UserWS").Value).Trim());
                if (((System.String)oRecordSet.Fields.Item("PassWS").Value).Trim() != "")
                    PassWS = Funciones.DesEncriptar(((System.String)oRecordSet.Fields.Item("PassWS").Value).Trim());

                oGrid = ((Grid)oForm.Items.Item("grid").Specific);

                for (Int32 i = 0; i < oGrid.DataTable.Rows.Count; i++)
                {
                    if (((System.String)oGrid.DataTable.GetValue("U_EstadoLey", i)).Trim() != "")
                    {
                        EstadoOld = ((System.String)oGrid.DataTable.GetValue("EstadoLeyOld", i)).Trim();
                        EstadoOriginal = ((System.String)oGrid.DataTable.GetValue("U_EstadoLey", i)).Trim();
                        if (EstadoOriginal == EstadoOld)
                            continue;
                        if ((EstadoOriginal == "ACD") && (EstadoOld != "ERM"))
                            EstadoFinal = "ERM";
                        else
                            EstadoFinal = EstadoOriginal;

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT T1.FldValue Code, T1.Descr Name
                                  FROM CUFD T0
                                  JOIN UFD1 T1 ON T1.TableID = T0.TableID
                                              AND T1.FieldID = T0.FieldID
                                 WHERE T0.TableID = '@VID_FEDTEVTA'
                                   AND T0.AliasID = 'EstadoLey'
                                   AND T1.FldValue = '{0}'";
                        else
                            s = @"SELECT T1.""FldValue"" ""Code"", T1.""Descr"" ""Name""
                                  FROM ""CUFD"" T0
                                  JOIN ""UFD1"" T1 ON T1.""TableID"" = T0.""TableID""
                                              AND T1.""FieldID"" = T0.""FieldID""
                                 WHERE T0.""TableID"" = '@VID_FEDTEVTA'
                                   AND T0.""AliasID"" = 'EstadoLey'
                                   AND T1.""FldValue"" = '{0}'";
                        s = String.Format(s, EstadoFinal);
                        oRecordSet.DoQuery(s);
                        EstadoDescrip = ((System.String)oRecordSet.Fields.Item("Name").Value).Trim();

                        URLFinal = URL.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                        URLFinal = URLFinal.Replace("{1}", ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)).ToString());
                        URLFinal = URLFinal.Replace("{2}", ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim());
                        URLFinal = URLFinal.Replace("{3}", ((System.String)oGrid.DataTable.GetValue("U_RUT", i)).Replace(".", "").Trim());
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
                            oDBDSHC.Clear();
                            oConditions = new SAPbouiCOM.Conditions();
                            oCondition = oConditions.Add();
                            oCondition.Alias = "DocEntry";
                            oCondition.Operation = BoConditionOperation.co_EQUAL;
                            var DocEntry = ((System.Int32)oGrid.DataTable.GetValue("DocEntry", i)).ToString();
                            oCondition.CondVal = DocEntry;
                            oDBDSHC.Query(oConditions);

                            oDBDSHC.SetValue("U_EstadoLey", 0, EstadoFinal);
                            oDBDSHC.SetValue("U_EstadoSII", 0, "A");
                            oDBDSHC.SetValue("U_Descrip", 0, EstadoDescrip);
                            oDBDSHC.SetValue("U_FechaMov", 0, DateTime.Now.Date.ToString("yyyyMMdd"));
                            oDBDSHC.SetValue("U_HoraMov", 0, DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString("00"));

                            lRetCode = Funciones.UpdDataSourceInt1("VID_FEDTECPRA", oDBDSHC, "", null, "", null, "", null);
                            if (lRetCode == 0)
                            {
                                FSBOApp.StatusBar.SetText("No se actualizado tabla @VID_FEDTECPRA, dejar en estado " + EstadoFinal + ", RUT " + ((System.String)oGrid.DataTable.GetValue("U_RUT", i)).Trim() + ", Tipo doc " + ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() + ", Folio " + ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)).ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                OutLog("No se actualizado tabla @VID_FEDTECPRA, dejar en estado " + EstadoFinal + ", RUT " + ((System.String)oGrid.DataTable.GetValue("U_RUT", i)).Trim() + ", Tipo doc " + ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() + ", Folio " + ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)).ToString());
                            }
                            else
                            {
                                FSBOApp.StatusBar.SetText("Documento actualizado en el portal, dejar en estado " + EstadoFinal + ", RUT " + ((System.String)oGrid.DataTable.GetValue("U_RUT", i)).Trim() + ", Tipo doc " + ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() + ", Folio " + ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)).ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                if ((EstadoFinal == "ACD") && ((((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() == "33") || (((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() == "34")))
                                    CrearDocto(((System.Int32)oGrid.DataTable.GetValue("DocEntry", i)), ((System.String)oGrid.DataTable.GetValue("U_RUT", i)), ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)), ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim());
                            }

                            //para la aceptacion primero envia el recibo de mercaderia y luego debe enviar la aceptacion
                            if ((EstadoOriginal == "ACD") && (EstadoOld != "ERM"))
                            {
                                URLFinal = URL.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                                URLFinal = URLFinal.Replace("{1}", ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)).ToString());
                                URLFinal = URLFinal.Replace("{2}", ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim());
                                URLFinal = URLFinal.Replace("{3}", ((System.String)oGrid.DataTable.GetValue("U_RUT", i)).Replace(".", "").Trim());
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

                                //if (((System.String)jDescripcion1.Value).Contains("Acción Completada OK"))
                                if (((System.String)jStatus1.Value).Trim() == "OK")
                                {
                                    if (GlobalSettings.RunningUnderSQLServer)
                                        s = @"SELECT T1.FldValue Code, T1.Descr Name
                                              FROM CUFD T0
                                              JOIN UFD1 T1 ON T1.TableID = T0.TableID
                                                          AND T1.FieldID = T0.FieldID
                                             WHERE T0.TableID = '@VID_FEDTEVTA'
                                               AND T0.AliasID = 'EstadoLey'
                                               AND T1.FldValue = '{0}'";
                                    else
                                        s = @"SELECT T1.""FldValue"" ""Code"", T1.""Descr"" ""Name""
                                              FROM ""CUFD"" T0
                                              JOIN ""UFD1"" T1 ON T1.""TableID"" = T0.""TableID""
                                                          AND T1.""FieldID"" = T0.""FieldID""
                                             WHERE T0.""TableID"" = '@VID_FEDTEVTA'
                                               AND T0.""AliasID"" = 'EstadoLey'
                                               AND T1.""FldValue"" = '{0}'";
                                    s = String.Format(s, EstadoOriginal);
                                    oRecordSet.DoQuery(s);
                                    EstadoDescrip = ((System.String)oRecordSet.Fields.Item("Name").Value).Trim();

                                    oDBDSHC.Clear();
                                    oConditions = new SAPbouiCOM.Conditions();
                                    oCondition = oConditions.Add();
                                    oCondition.Alias = "DocEntry";
                                    oCondition.Operation = BoConditionOperation.co_EQUAL;
                                    oCondition.CondVal = DocEntry;
                                    oDBDSHC.Query(oConditions);

                                    oDBDSHC.SetValue("U_EstadoLey", 0, EstadoOriginal);
                                    oDBDSHC.SetValue("U_EstadoSII", 0, "A");
                                    oDBDSHC.SetValue("U_Descrip", 0, EstadoDescrip);
                                    oDBDSHC.SetValue("U_FechaMov", 0, DateTime.Now.Date.ToString("yyyyMMdd"));
                                    oDBDSHC.SetValue("U_HoraMov", 0, DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString("00"));
                                    if (Funciones.UpdDataSourceInt1("VID_FEDTECPRA", oDBDSHC, "", null, "", null, "", null) == 0)
                                    {
                                        FSBOApp.StatusBar.SetText("No se actualizado tabla @VID_FEDTECPRA, dejar en estado " + EstadoOriginal + ", RUT " + ((System.String)oGrid.DataTable.GetValue("U_RUT", i)).Trim() + ", Tipo doc " + ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() + ", Folio " + ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)).ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        OutLog("No se actualizado tabla @VID_FEDTECPRA, dejar en estado " + EstadoOriginal + ", RUT " + ((System.String)oGrid.DataTable.GetValue("U_RUT", i)).Trim() + ", Tipo doc " + ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() + ", Folio " + ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)).ToString());
                                    }
                                    else
                                    {
                                        FSBOApp.StatusBar.SetText("Documento actualizado en el portal, dejar en estado " + EstadoOriginal + ", RUT " + ((System.String)oGrid.DataTable.GetValue("U_RUT", i)).Trim() + ", Tipo doc " + ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() + ", Folio " + ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)).ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                        //
                                        if ((((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() == "33") || (((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() == "34"))
                                            CrearDocto(((System.Int32)oGrid.DataTable.GetValue("DocEntry", i)), ((System.String)oGrid.DataTable.GetValue("U_RUT", i)), ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)), ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim());
                                    }
                                }
                                else
                                {
                                    FSBOApp.StatusBar.SetText("No se actualizado en el portal(" + ((System.String)jDescripcion.Value).Trim() + "), RUT " + ((System.String)oGrid.DataTable.GetValue("U_RUT", i)).Trim() + ", Tipo doc " + ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() + ", Folio " + ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)).ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    OutLog("No se actualizado en el portal(" + ((System.String)jDescripcion.Value).Trim() + "), RUT " + ((System.String)oGrid.DataTable.GetValue("U_RUT", i)).Trim() + ", Tipo doc " + ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() + ", Folio " + ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)).ToString());
                                }
                            }
                        }
                        else
                        {
                            FSBOApp.StatusBar.SetText("No se actualizado en el portal(" + ((System.String)jDescripcion.Value).Trim() + "), RUT " + ((System.String)oGrid.DataTable.GetValue("U_RUT", i)).Trim() + ", Tipo doc " + ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() + ", Folio " + ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)).ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            OutLog("No se actualizado en el portal(" + ((System.String)jDescripcion.Value).Trim() + "), RUT " + ((System.String)oGrid.DataTable.GetValue("U_RUT", i)).Trim() + ", Tipo doc " + ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", i)).Trim() + ", Folio " + ((System.Int32)oGrid.DataTable.GetValue("U_Folio", i)).ToString());
                        }
                    }
                }
                return true;
            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText("ActualizarEstado: " + x.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("ActualizarEstado: " + x.Message + " ** Trace: " + x.StackTrace);
                return false;
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
            ReportDocument rpt = new ReportDocument();
            ConnectionInfo connection = new ConnectionInfo();
            TableLogOnInfo logOnInfo;
            Boolean flag = true;
            String Pass = "";

            try
            {
                oGrid = (Grid)(oForm.Items.Item("grid").Specific);
                Code = Convert.ToString(((System.Int32)oGrid.DataTable.GetValue("DocEntry", Linea)), _nf);
                sXml = ((System.String)oGrid.DataTable.GetValue("U_Xml", Linea));
                TipoDoc = ((System.String)oGrid.DataTable.GetValue("U_TipoDoc", Linea));
                RUTEmisor = ((System.String)oGrid.DataTable.GetValue("U_RUT", Linea));
                Folio = Convert.ToString(((System.Int32)oGrid.DataTable.GetValue("U_Folio", Linea)), _nf);
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

        private void CrearDocto(Int32 DocEntry, String RUT, Int32 FolioNum, String TipoDoc)
        {
            SAPbobsCOM.Recordset ors = ((SAPbobsCOM.Recordset)FCmpny.GetBusinessObject(BoObjectTypes.BoRecordset));
            SAPbobsCOM.Recordset orsAux = ((SAPbobsCOM.Recordset)FCmpny.GetBusinessObject(BoObjectTypes.BoRecordset));
            String CardCode;
            Int32 OC;
            Int32 lRetCode;
            Int32 nErr;
            String sErr;
            String DocACrear;
            SAPbobsCOM.Documents oDocumentsOC;
            SAPbobsCOM.Documents oDocuments;
            try
            {
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT ISNULL(U_CrearDocC,'N') 'Crear', ISNULL(U_FProv,'Y') 'FProv' FROM [@VID_FEPARAM]";
                else
                    s = @"SELECT IFNULL(""U_CrearDocC"",'N') ""Crear"", IFNULL(""U_FProv"",'Y') ""FProv"" FROM ""@VID_FEPARAM"" ";
                ors.DoQuery(s);

                if (((System.String)ors.Fields.Item("Crear").Value).Trim() == "Y")
                {
                    if (((System.String)ors.Fields.Item("Crear").Value).Trim() == "Y")
                        DocACrear = "P"; //Preliminar
                    else
                        DocACrear = "R"; //Real

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT CardCode FROM OCRD WHERE REPLACE(LicTradNum,'.','') = '{0}' AND CardType = 'S' AND frozenFor = 'N'";
                    else
                        s = @"SELECT ""CardCode"" FROM ""OCRD"" WHERE REPLACE(""LicTradNum"",'.','') = '{0}' AND ""CardType"" = 'S' AND ""frozenFor"" = 'N'";
                    s = String.Format(s, RUT.Replace(".", ""));
                    ors.DoQuery(s);
                    if (ors.RecordCount == 0)
                        FSBOApp.StatusBar.SetText("No se ha encontrado proveedor en el Maestro SN, RUT " + RUT, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    else
                    {
                        CardCode = ((System.String)ors.Fields.Item("CardCode").Value).Trim();
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT T0.U_CodRef, T0.U_Folio, T1.U_Xml
                                      FROM [@VID_FEDTECPRAD] T0
                                      JOIN [@VID_FEDTECPRA] T1 ON T1.DocEntry = T0.DocEntry
                                     WHERE T0.DocEntry = {0}
                                       AND T0.U_CodRef = '801'
                                       AND ISNUMERIC(T0.U_Folio) = 1";
                        else
                            s = @"SELECT T0.""U_CodRef"", T0.""U_Folio"", T1.""U_Xml""
                                      FROM ""@VID_FEDTECPRAD"" T0
                                      JOIN ""@VID_FEDTECPRA"" T1 ON T1.""DocEntry"" = T0.""DocEntry""
                                     WHERE T0.""DocEntry"" = {0}
                                       AND T0.""U_CodRef"" = '801'";
                        s = String.Format(s, DocEntry);
                        ors.DoQuery(s);
                        if (ors.RecordCount == 0)
                            FSBOApp.StatusBar.SetText("No se ha encontrado Orden de Compra para la factura " + FolioNum.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        else
                        {
                            if (ors.RecordCount > 1)
                            {
                                FSBOApp.StatusBar.SetText("Documento posee mas de una Orden de Compra -> " + FolioNum.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                return;
                            }

                            if (((System.String)ors.Fields.Item("U_Xml").Value).Trim() == "")
                            {
                                FSBOApp.StatusBar.SetText("Documento no posee XML -> " + FolioNum.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                return;
                            }

                            var sOC = ((System.String)ors.Fields.Item("U_Folio").Value).Trim();
                            if (Int32.TryParse(sOC, out OC))
                            {
                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = @"SELECT T0.DocEntry, T0.DocStatus, T0.DocTotal, T0.VatSum, COUNT(*) 'Cant'
                                            FROM OPOR T0
                                            JOIN POR1 T1 ON T1.DocEntry = T0.DocEntry
                                            WHERE T0.DocNum = {0}
                                            AND T0.CardCode = '{1}'
                                            GROUP BY T0.DocEntry, T0.DocStatus, T0.DocTotal, T0.VatSum, T0.DocDate";
                                else
                                    s = @"SELECT T0.""DocEntry"", T0.""DocStatus"", T0.""DocTotal"", T0.""VatSum"", COUNT(*) ""Cant""
                                            FROM ""OPOR"" T0
                                            JOIN ""POR1"" T1 ON T1.""DocEntry"" = T0.""DocEntry""
                                            WHERE T0.""DocNum"" = {0}
                                            AND T0.""CardCode"" = '{1}'
                                            GROUP BY T0.""DocEntry"", T0.""DocStatus"", T0.""DocTotal"", T0.""VatSum"", T0.""DocDate"" ";
                                s = String.Format(s, OC, CardCode);
                                ors.DoQuery(s);
                                if (ors.RecordCount == 0)
                                {
                                    FSBOApp.StatusBar.SetText("No se ha encontrado Orden de Compra en SAP -> " + sOC, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                    return;
                                }
                                else
                                {
                                    var CantLineasOC = ((System.Int32)ors.Fields.Item("Cant").Value);
                                    var OCDocEntry = ((System.Int32)ors.Fields.Item("DocEntry").Value);
                                    var EMDocEntry = 0;
                                    var EMDocNum = 0;
                                    var OCDocStatus = ((System.String)ors.Fields.Item("DocStatus").Value).Trim();
                                    var OCDocTotal = ((System.Double)ors.Fields.Item("DocTotal").Value);
                                    var OCVatSum = ((System.Double)ors.Fields.Item("VatSum").Value);
                                    var BaseType = 22;

                                    if (GlobalSettings.RunningUnderSQLServer)
                                        s = @"SELECT COUNT(*) 'Cant'
                                                                    FROM [@VID_FEXMLCD]
                                                                    WHERE Code = '{0}'";
                                    else
                                        s = @"SELECT COUNT(*) ""Cant""
                                                                    FROM ""@VID_FEXMLCD""
                                                                    WHERE ""Code"" = '{0}'";
                                    s = String.Format(s, DocEntry);
                                    ors.DoQuery(s);
                                    var CantLinFE = ((System.Int32)ors.Fields.Item("Cant").Value);

                                    if (CantLineasOC != CantLinFE)
                                    {
                                        FSBOApp.StatusBar.SetText("Cantidad de lineas entre OC y Documento Elec. son diferentes", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        return;
                                    }

                                    //mejorar query para que muestre toda la OC con sus entregas para dejar todo cerrado con la factura***************************
                                    if (GlobalSettings.RunningUnderSQLServer)
                                        s = @"SELECT U_FchEmis, U_FchVenc, U_RznSoc, U_MntNeto, U_MntExe, U_MntTotal, U_IVA
                                                      FROM [@VID_FEXMLC]
                                                     WHERE Code = '{0}'";
                                    else
                                        s = @"SELECT ""U_FchEmis"", ""U_FchVenc"", ""U_RznSoc"", ""U_MntNeto"", ""U_MntExe"", ""U_MntTotal"", ""U_IVA""
                                                      FROM ""@VID_FEXMLC""
                                                     WHERE ""Code"" = '{0}'";
                                    s = String.Format(s, DocEntry);
                                    orsAux.DoQuery(s);
                                    var FchEmis = ((System.DateTime)orsAux.Fields.Item("U_FchEmis").Value);
                                    var Fchvenc = ((System.DateTime)orsAux.Fields.Item("U_FchVenc").Value);
                                    var RznSoc = ((System.String)orsAux.Fields.Item("U_RznSoc").Value).Trim();
                                    var MntNeto = ((System.Double)orsAux.Fields.Item("U_MntNeto").Value);
                                    var MntExe = ((System.Double)orsAux.Fields.Item("U_MntExe").Value);
                                    var MntTotal = ((System.Double)orsAux.Fields.Item("U_MntTotal").Value);
                                    var IVA = ((System.Double)orsAux.Fields.Item("U_IVA").Value);

                                    if (GlobalSettings.RunningUnderSQLServer)
                                        s = @"SELECT SELECT T0.DocEntry, T1.LineNum, T1.ObjType, T1.OpenQty 'Quantity'
                                              FROM OPOR T0
                                              JOIN POR1 T1 ON T1.DocEntry = T0.DocEntry
                                             WHERE 1=1
                                               AND T0.DocNum = {0}
                                            UNION 
                                            SELECT P1.DocEntry, P1.LineNum, P1.ObjType, P1.Quantity--, P1.BaseLine
                                              FROM OPOR T0
                                              JOIN POR1 T1 ON T1.DocEntry = T0.DocEntry
                                              JOIN PDN1 P1 ON P1.BaseEntry = T1.DocEntry
                                                          AND P1.BaseType = T0.ObjType
			                                              AND P1.BaseLine = T1.LineNum
                                             WHERE 1=1
                                               AND T0.DocNum = {0}";
                                    else
                                        s = @"SELECT SELECT T0.""DocEntry"", T1.""LineNum"", T1.""ObjType"", T1.""OpenQty"" ""Quantity""
                                              FROM ""OPOR"" T0
                                              JOIN ""POR1"" T1 ON T1.""DocEntry"" = T0.""DocEntry""
                                             WHERE 1=1
                                               AND T0.""DocNum"" = {0}
                                            UNION 
                                            SELECT P1.""DocEntry"", P1.""LineNum"", P1.""ObjType"", P1.""Quantity"" --, P1.BaseLine
                                              FROM ""OPOR"" T0
                                              JOIN ""POR1"" T1 ON T1.""DocEntry"" = T0.""DocEntry""
                                              JOIN ""PDN1"" P1 ON P1.""BaseEntry"" = T1.""DocEntry""
                                                          AND P1.""BaseType"" = T0.""ObjType""
			                                              AND P1.""BaseLine"" = T1.""LineNum""
                                             WHERE 1=1
                                               AND T0.""DocNum"" = {0}";
                                    s = String.Format(s, OC);
                                    orsAux.DoQuery(s);

                                    if (orsAux.RecordCount > 0)
                                    {
                                        //como no salio por algun return sigo con la creacion del documento en SAP
                                        var men = "";
                                        if (DocACrear == "P")
                                        {
                                            oDocuments = ((SAPbobsCOM.Documents)FCmpny.GetBusinessObject(BoObjectTypes.oDrafts));
                                            men = "Factura Preliminar";
                                        }
                                        else
                                        {
                                            oDocuments = ((SAPbobsCOM.Documents)FCmpny.GetBusinessObject(BoObjectTypes.oPurchaseInvoices));
                                            men = "Factura";
                                        }

                                        oDocuments.CardCode = CardCode;
                                        oDocuments.CardName = RznSoc;
                                        oDocuments.DocDate = FchEmis;
                                        oDocuments.DocDueDate = Fchvenc;
                                        oDocuments.FolioPrefixString = TipoDoc;
                                        oDocuments.FolioNumber = FolioNum;
                                        oDocuments.Comments = "Creado por addon FE en Aceptación del DTE";
                                        if (DocACrear == "P")
                                            oDocuments.DocObjectCode = BoObjectTypes.oPurchaseInvoices;

                                        while (!orsAux.EoF)
                                        {
                                            oDocuments.Lines.BaseEntry = ((System.Int32)orsAux.Fields.Item("DocEntry").Value);
                                            oDocuments.Lines.BaseLine = ((System.Int32)orsAux.Fields.Item("LineNum").Value);
                                            oDocuments.Lines.BaseType = ((System.Int32)orsAux.Fields.Item("ObjType").Value);
                                            oDocuments.Lines.Quantity = ((System.Double)orsAux.Fields.Item("Quantity").Value);
                                            oDocuments.Lines.Add();
                                            orsAux.MoveNext();
                                        }

                                        //oDocuments.VatSum = 0;
                                        oDocuments.DocTotal = MntTotal;

                                        lRetCode = oDocuments.Add();
                                        if (lRetCode != 0)
                                        {
                                            FCmpny.GetLastError(out nErr, out sErr);
                                            OutLog("No se ha creado documento en SAP, " + men + " -> " + FolioNum.ToString() + " - " + sErr);
                                            FSBOApp.StatusBar.SetText("No se ha creado documento en SAP, " + men + " -> " + FolioNum.ToString() + " - " + sErr, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                        }
                                        else
                                        {
                                            FSBOApp.StatusBar.SetText("Se ha creado satisfactoriamente el documento en SAP, " + men + " -> " + FolioNum.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                            //guardar registro para monitor de registros creados
                                            var NuevoDocEntry = FCmpny.GetNewObjectKey();
                                            if (GlobalSettings.RunningUnderSQLServer)
                                                s = @"UPDATE [@VID_FEDTECPRA] SET U_DocEntry = {1}, U_ObjType = '{2}' WHERE DocEntry = {0}";
                                            else
                                                s = @"UPDATE ""@VID_FEDTECPRA"" SET ""U_DocEntry"" = {1}, ""U_ObjType"" = '{2}' WHERE ""DocEntry"" = {0}";
                                            s = String.Format(s, DocEntry, NuevoDocEntry, (DocACrear == "P" ? "112" : "18"));
                                            orsAux.DoQuery(s);

                                        }
                                    }
                                    else
                                    {
                                        FSBOApp.StatusBar.SetText("No se ha encontrado detalle OC " + OC, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                FSBOApp.StatusBar.SetText("Numero Orden de Compra no es valido -> " + ((System.String)ors.Fields.Item("U_Folio").Value), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                return;
                            }

                        }
                    }
                }

            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText("CrearDocto: " + x.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CrearDocto: " + x.Message + " ** Trace: " + x.StackTrace);
            }
            finally
            {
                FSBOf._ReleaseCOMObject(ors);
                FSBOf._ReleaseCOMObject(orsAux);
            }
        }


        private void ColorearGrid()
        {
            Int32 valorFila;
            try
            {
                for (Int32 numfila = 0; numfila <= oGrid.Rows.Count - 1; numfila++)
                {
                    valorFila = oGrid.GetDataTableRowIndex(numfila);
                    if (valorFila != -1)
                    {
                        if (((System.String)oGrid.DataTable.GetValue("U_Validacion", valorFila)).Trim() == "OK")
                            oGrid.CommonSetting.SetCellBackColor(numfila + 1, 5, ColorTranslator.ToOle(Color.LightGreen));
                        else
                            oGrid.CommonSetting.SetCellBackColor(numfila + 1, 5, ColorTranslator.ToOle(Color.Red));

                        //colorea en verde para mostrar que tiene PDF
                        if (((System.String)oGrid.DataTable.GetValue("U_Xml", valorFila)).Trim() != "")
                            oGrid.CommonSetting.SetCellBackColor(numfila + 1, 4, ColorTranslator.ToOle(Color.LightGreen));
                        
                        if (((System.String)oGrid.DataTable.GetValue("OCOri", valorFila)).Trim() == "")
                            oGrid.CommonSetting.SetCellEditable(valorFila + 1, 15, true);
                        else
                            oGrid.CommonSetting.SetCellEditable(valorFila + 1, 15, false);

                    }
                }
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText("Error ColorearGrid -> " + e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("Error ColorearGrid -> " + e.Message + ", TRACE " + e.StackTrace);
            }
        }

    }//fin class
}
