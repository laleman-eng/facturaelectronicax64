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
using System.Data.SqlClient;
using System.Data;
using Factura_Electronica_VK.DetalleLog;
using System.Diagnostics;
//using ServiceStack.Text;
//using System.Net.Http;
using System.Configuration;

namespace Factura_Electronica_VK.Monitor
{
    class TMonitor : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Grid oGrid;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.Item oItem;
        private SAPbouiCOM.EditText oEditText;
        private SAPbouiCOM.CheckBox oCheckBox;
        private SAPbouiCOM.GridColumn oColumn;
        private SAPbouiCOM.DataTable oDataTable;
        private SAPbouiCOM.DBDataSource oDBDSHeader;
        private SAPbouiCOM.DBDataSource oDBDSD;
        private System.Timers.Timer ttime;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;
        private Boolean bMultiSoc;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                //Lista    := New list<string>;

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



                FSBOf.LoadForm(xmlPath, "VID_Monitor.srf", uid);
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

                oDBDSHeader = oForm.DataSources.DBDataSources.Add("@VID_FELOG");
                oDBDSD = oForm.DataSources.DBDataSources.Add("@VID_FELOGD");

                oForm.DataSources.UserDataSources.Add("FechaD", BoDataType.dt_DATE, 10);
                oEditText = (EditText)(oForm.Items.Item("FechaD").Specific);
                oEditText.DataBind.SetBound(true, "", "FechaD");
                oEditText.Value = DateTime.Now.ToString("yyyyMMdd");

                oForm.DataSources.UserDataSources.Add("FechaH", BoDataType.dt_DATE, 10);
                oEditText = (EditText)(oForm.Items.Item("FechaH").Specific);
                oEditText.DataBind.SetBound(true, "", "FechaH");
                oEditText.Value = DateTime.Now.ToString("yyyyMMdd");

                oForm.DataSources.UserDataSources.Add("chk_Todo", BoDataType.dt_SHORT_TEXT, 1);
                oCheckBox = (CheckBox)(oForm.Items.Item("chk_Todo").Specific);
                oCheckBox.DataBind.SetBound(true, "", "chk_Todo");
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";
                oCheckBox.Checked = false;

                oForm.DataSources.UserDataSources.Add("Rechazados", BoDataType.dt_SHORT_TEXT, 1);
                oCheckBox = (CheckBox)(oForm.Items.Item("Rechazados").Specific);
                oCheckBox.DataBind.SetBound(true, "", "Rechazados");
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";
                oCheckBox.Checked = true;

                oForm.DataSources.UserDataSources.Add("Pendientes", BoDataType.dt_SHORT_TEXT, 1);
                oCheckBox = (CheckBox)(oForm.Items.Item("Pendientes").Specific);
                oCheckBox.DataBind.SetBound(true, "", "Pendientes");
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";
                oCheckBox.Checked = true;

                oForm.DataSources.UserDataSources.Add("Aceptados", BoDataType.dt_SHORT_TEXT, 1);
                oCheckBox = (CheckBox)(oForm.Items.Item("Aceptados").Specific);
                oCheckBox.DataBind.SetBound(true, "", "Aceptados");
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";
                oCheckBox.Checked = false;

                oForm.DataSources.UserDataSources.Add("AceptadosR", BoDataType.dt_SHORT_TEXT, 1);
                oCheckBox = (CheckBox)(oForm.Items.Item("AceptadosR").Specific);
                oCheckBox.DataBind.SetBound(true, "", "AceptadosR");
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";
                oCheckBox.Checked = false;

                oForm.DataSources.UserDataSources.Add("Errores", BoDataType.dt_SHORT_TEXT, 1);
                oCheckBox = (CheckBox)(oForm.Items.Item("Errores").Specific);
                oCheckBox.DataBind.SetBound(true, "", "Errores");
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";
                oCheckBox.Checked = true;

                oForm.DataSources.UserDataSources.Add("chkActEst", BoDataType.dt_SHORT_TEXT, 1);
                oCheckBox = (CheckBox)(oForm.Items.Item("chkActEst").Specific);
                oCheckBox.DataBind.SetBound(true, "", "chkActEst");
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";
                oCheckBox.Checked = false;


                oDataTable = oForm.DataSources.DataTables.Add("dt");
                oGrid = (Grid)(oForm.Items.Item("grid").Specific);
                oGrid.DataTable = oDataTable;

                ttime = new System.Timers.Timer();
                TTimer();

                CargarDatos();
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
            IvkFormInterface oFormVk;
            String oUid;
            String prmKey;
            SAPbouiCOM.EditTextColumn oEditColumn;

            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if ((pVal.EventType == BoEventTypes.et_MATRIX_LINK_PRESSED) && (pVal.BeforeAction) && (pVal.ItemUID == "grid"))
                {
                    s = (System.String)(oDataTable.GetValue("ObjType", pVal.Row));
                    oColumn = (GridColumn)(oGrid.Columns.Item("DocEntry"));
                    oEditColumn = (EditTextColumn)(oColumn);
                    oEditColumn.LinkedObjectType = s;
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction))
                {
                    if (pVal.ItemUID == "btnAct")
                    {
                        FSBOApp.StatusBar.SetText("Iniciando actualización de estado", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                        RevisarEstado();
                        CargarDatos();
                        FSBOApp.StatusBar.SetText("Ha finalizado actualización de estado", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    }

                    if (pVal.ItemUID == "ActGrilla")
                        CargarDatos();

                    if (pVal.ItemUID == "chk_Todo")
                        CargarDatos();

                    if (pVal.ItemUID == "Rechazados")
                        CargarDatos();
                     

                    if (pVal.ItemUID == "Pendientes")
                        CargarDatos();

                    if (pVal.ItemUID == "Aceptados")
                        CargarDatos();

                    if (pVal.ItemUID == "AceptadosR")
                        CargarDatos();

                    if (pVal.ItemUID == "Errores")
                        CargarDatos();

                    if (pVal.ItemUID == "chkActEst")
                    {
                        oCheckBox = (SAPbouiCOM.CheckBox)(oForm.Items.Item("chkActEst").Specific);
                        if (oCheckBox.Checked)
                        {
                            ttime.Enabled = true;
                            ttime.Start();
                        }
                        else
                        {
                            ttime.Stop();
                            ttime.Enabled = false;
                        }
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_DOUBLE_CLICK) && (!pVal.BeforeAction) && (pVal.ColUID == "RowsHeader") && (pVal.ItemUID == "grid") && (pVal.Row != -1))
                {
                    prmKey = Convert.ToString((System.Int32)(oDataTable.GetValue("Key", pVal.Row)));
                    if (GlobalSettings.RunningUnderSQLServer)
                    { s = @"select * from [@VID_FELOGD] WITH (NOLOCK) where DocEntry = {0}"; }
                    else
                    { s = @"select * from ""@VID_FELOGD"" where ""DocEntry"" = {0} "; }
                    s = String.Format(s, prmKey);
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount == 0)
                    { FSBOApp.StatusBar.SetText("No se ha encontrado detalle del documento seleccionado", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                    else
                    {
                        //abrir nuevo formulario para mostrar detalle 
                        oFormVk = (IvkFormInterface)new TDetalleLog();
                        TDetalleLog.prmKey = prmKey;
                        oUid = FSBOf.generateFormId(FGlobalSettings.SBOSpaceName, FGlobalSettings);
                        oFormVk.InitForm(oUid, "forms\\", ref FSBOApp, ref FCmpny, ref FSBOf, ref FGlobalSettings);
                        FoForms.Add(oFormVk);
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_VALIDATE) && (pVal.BeforeAction) && (pVal.ItemUID == "FechaD"))
                {
                    oEditText = (EditText)(oForm.Items.Item("FechaD").Specific);
                    if ((System.String)(oEditText.Value) == "")
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar una fecha desde", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        BubbleEvent = false;
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_VALIDATE) && (pVal.BeforeAction) && (pVal.ItemUID == "FechaH"))
                {
                    oEditText = (EditText)(oForm.Items.Item("FechaH").Specific);
                    if ((System.String)(oEditText.Value) == "")
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar una fecha hasta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        BubbleEvent = false;
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_FORM_CLOSE) && (pVal.BeforeAction))
                {
                    ttime.Stop();
                    ttime.Enabled = false;
                    ttime = null;
                }
            }
            catch (Exception e)
            {
                if (FCmpny.InTransaction) FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);

                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin FormEvent


        private void CargarDatos()
        {
            SAPbouiCOM.CommonSetting setting;
            SAPbouiCOM.EditTextColumn oEditColumn;
            SAPbouiCOM.CheckBox oChkRechazados;
            SAPbouiCOM.CheckBox oChkPendientes;
            SAPbouiCOM.CheckBox oChkAceptados;
            SAPbouiCOM.CheckBox oChkAceptadosR;
            SAPbouiCOM.CheckBox oChkErrores;
            String FechaD, FechaH, Status;
            try
            {
                oForm.Freeze(true);
                oChkRechazados = (CheckBox)(oForm.Items.Item("Rechazados").Specific);
                oChkPendientes = (CheckBox)(oForm.Items.Item("Pendientes").Specific);
                oChkAceptados = (CheckBox)(oForm.Items.Item("Aceptados").Specific);
                oChkAceptadosR = (CheckBox)(oForm.Items.Item("AceptadosR").Specific);
                oChkErrores = (CheckBox)(oForm.Items.Item("Errores").Specific);

                if ((oChkRechazados.Checked) || (oChkPendientes.Checked) || (oChkAceptados.Checked) || (oChkAceptadosR.Checked) || (oChkErrores.Checked))
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                    {
                        Status = "and T0.U_Status in (";
                        Status = Status + (oChkRechazados.Checked ? "'RZ'," : "");
                        Status = Status + (oChkPendientes.Checked ? "'EC'," : "");
                        Status = Status + (oChkAceptados.Checked ? "'RR'," : "");
                        Status = Status + (oChkAceptadosR.Checked ? "'AR'," : "");
                        Status = Status + (oChkErrores.Checked ? "'EE'," : "");
                        Status = Status.Substring(0, Status.Length - 1);
                        Status = Status + ")";
                    }
                    else
                    {
                        Status = @"and T0.""U_Status"" in (";
                        Status = Status + (oChkRechazados.Checked ? "'RZ'," : "");
                        Status = Status + (oChkPendientes.Checked ? "'EC'," : "");
                        Status = Status + (oChkAceptados.Checked ? "'RR'," : "");
                        Status = Status + (oChkAceptadosR.Checked ? "'AR'," : "");
                        Status = Status + (oChkErrores.Checked ? "'EE'," : "");
                        Status = Status.Substring(0, Status.Length - 1);
                        Status = Status + ")";
                    }
                }
                else if ((!oChkRechazados.Checked) && (!oChkPendientes.Checked) && (!oChkAceptados.Checked) && (!oChkAceptadosR.Checked) && (!oChkErrores.Checked))
                {
                    //en caso de no encontrar ninguno marcado sale de la funcion
                    oForm.Freeze(false);
                    return;
                }
                else
                    Status = "";



                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select
                               case U_ObjType
							    when '13' then (select SUBSTRING(ISNULL(N2.BeginStr,''), 2, LEN(N2.BeginStr)) from OINV N0 JOIN NNM1 N2 ON N0.Series = N2.Series where DocEntry = T0.U_DocEntry)
		                        when '14' then (select SUBSTRING(ISNULL(N2.BeginStr,''), 2, LEN(N2.BeginStr)) from ORIN N0 JOIN NNM1 N2 ON N0.Series = N2.Series where DocEntry = T0.U_DocEntry)
		                        when '15' then (select SUBSTRING(ISNULL(N2.BeginStr,''), 2, LEN(N2.BeginStr)) from ODLN N0 JOIN NNM1 N2 ON N0.Series = N2.Series where DocEntry = T0.U_DocEntry)
		                        when '67' then (select SUBSTRING(ISNULL(N2.BeginStr,''), 2, LEN(N2.BeginStr)) from OWTR N0 JOIN NNM1 N2 ON N0.Series = N2.Series where DocEntry = T0.U_DocEntry)
                                when '21' then (select SUBSTRING(ISNULL(N2.BeginStr,''), 2, LEN(N2.BeginStr)) from ORPD N0 JOIN NNM1 N2 ON N0.Series = N2.Series where DocEntry = T0.U_DocEntry)
	                           end				'Inst' 
                              ,LTRIM(STR(T0.U_DocEntry,18,0))	'DocEntry'
	                          ,case U_ObjType
		                        when '13' then (select DocNum from OINV where DocEntry = T0.U_DocEntry)
		                        when '14' then (select DocNum from ORIN where DocEntry = T0.U_DocEntry)
		                        when '15' then (select DocNum from ODLN where DocEntry = T0.U_DocEntry)
		                        when '67' then (select DocNum from OWTR where DocEntry = T0.U_DocEntry)
                                when '21' then (select DocNum from ORPD where DocEntry = T0.U_DocEntry)
	                           end				'DocNum'
                              ,T0.U_TipoDoc		'TipoDoc'
                              ,LTRIM(STR(T0.U_FolioNum,18,0))	'Folio'
                              ,(select C1.Descr from CUFD C0 join UFD1 C1 ON C1.TableID=C0.TableID and C1.FieldID=C0.FieldID where C0.TableID = '@VID_FELOG' and C0.AliasID='Status' and C1.FldValue= T0.U_Status)	'Estado'
                              ,T0.U_Status
                              ,T0.U_Message		'Mensaje'
                              ,T0.U_ObjType     'ObjType'
                              ,T0.DocEntry		'Key'
                          from [@vid_felog] T0 WITH (NOLOCK)
                          join OUSR T2 on T2.USER_CODE = T0.U_UserCode
                         where {0}
                           {3}
                           and ISNULL(T0.U_DocDate, T0.CreateDate) between '{1}' and '{2}'
                           
                         order by T0.DocEntry DESC";
                else
                    s = @"select
                               case ""U_ObjType""
							    when '13' then (select SUBSTRING(IFNULL(N2.""BeginStr"",''), 2, LENGTH(N2.""BeginStr"")) from ""OINV"" N0 JOIN ""NNM1"" N2 ON N0.""Series"" = N2.""Series"" where ""DocEntry"" = T0.""U_DocEntry"")
		                        when '14' then (select SUBSTRING(IFNULL(N2.""BeginStr"",''), 2, LENGTH(N2.""BeginStr"")) from ""ORIN"" N0 JOIN ""NNM1"" N2 ON N0.""Series"" = N2.""Series"" where ""DocEntry"" = T0.""U_DocEntry"")
		                        when '15' then (select SUBSTRING(IFNULL(N2.""BeginStr"",''), 2, LENGTH(N2.""BeginStr"")) from ""ODLN"" N0 JOIN ""NNM1"" N2 ON N0.""Series"" = N2.""Series"" where ""DocEntry"" = T0.""U_DocEntry"")
		                        when '67' then (select SUBSTRING(IFNULL(N2.""BeginStr"",''), 2, LENGTH(N2.""BeginStr"")) from ""OWTR"" N0 JOIN ""NNM1"" N2 ON N0.""Series"" = N2.""Series"" where ""DocEntry"" = T0.""U_DocEntry"")
                                when '21' then (select SUBSTRING(IFNULL(N2.""BeginStr"",''), 2, LENGTH(N2.""BeginStr"")) from ""ORPD"" N0 JOIN ""NNM1"" N2 ON N0.""Series"" = N2.""Series"" where ""DocEntry"" = T0.""U_DocEntry"")
	                           end				""Inst""  
                              ,LTRIM(TO_ALPHANUM(T0.""U_DocEntry""))	""DocEntry""
	                          ,case ""U_ObjType""
		                        when '13' then (select ""DocNum"" from ""OINV"" where ""DocEntry"" = T0.""U_DocEntry"")
		                        when '14' then (select ""DocNum"" from ""ORIN"" where ""DocEntry"" = T0.""U_DocEntry"")
		                        when '15' then (select ""DocNum"" from ""ODLN"" where ""DocEntry"" = T0.""U_DocEntry"")
		                        when '67' then (select ""DocNum"" from ""OWTR"" where ""DocEntry"" = T0.""U_DocEntry"")
                                when '21' then (select ""DocNum"" from ""ORPD"" where ""DocEntry"" = T0.""U_DocEntry"")
	                           end				""DocNum""
                              ,T0.""U_TipoDoc""		""TipoDoc""
                              ,LTRIM(TO_ALPHANUM(T0.""U_FolioNum""))	""Folio""
                              ,(select C1.""Descr"" from ""CUFD"" C0 join ""UFD1"" C1 ON C1.""TableID""=C0.""TableID"" and C1.""FieldID""=C0.""FieldID"" where C0.""TableID"" = '@VID_FELOG' and C0.""AliasID""='Status' and C1.""FldValue""= T0.""U_Status"")	""Estado""
                              ,T0.""U_Status""
                              ,T0.""U_Message""		""Mensaje"" 
                              ,T0.""U_ObjType""     ""ObjType""
                              ,T0.""DocEntry""		""Key""
                          from ""@VID_FELOG"" T0 
                          join ""OUSR"" T2 on T2.""USER_CODE"" = T0.""U_UserCode""
                         where {0}
                           {3}
                           and IFNULL(T0.""U_DocDate"", T0.""CreateDate"") between '{1}' and '{2}'
                         order by T0.""DocEntry"" DESC ";

                oCheckBox = (CheckBox)(oForm.Items.Item("chk_Todo").Specific);
                oEditText = (EditText)(oForm.Items.Item("FechaD").Specific);
                FechaD = (System.String)(oEditText.Value).Trim();

                oEditText = (EditText)(oForm.Items.Item("FechaH").Specific);
                FechaH = (System.String)(oEditText.Value).Trim();

                if (GlobalSettings.RunningUnderSQLServer)
                { s = String.Format(s, oCheckBox.Checked == false ? "T0.U_UserCode = '" + FSBOApp.Company.UserName + "'" : "1=1", FechaD, FechaH, Status); }
                else
                { s = String.Format(s, oCheckBox.Checked == false ? @"T0.""U_UserCode"" = '" + FSBOApp.Company.UserName + "'" : "1=1", FechaD, FechaH, Status); }
                oDataTable.ExecuteQuery(s);

                oGrid.Columns.Item("TipoDoc").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("TipoDoc"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Tipo Documento";

                oGrid.Columns.Item("Inst").Type = BoGridColumnType.gct_ComboBox;
                var colCombo = (ComboBoxColumn)(oGrid.Columns.Item("Inst"));
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

                oGrid.Columns.Item("DocEntry").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("DocEntry"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Llave SAP";
                oEditColumn.LinkedObjectType = "13";
                oEditColumn.RightJustified = true;

                oGrid.Columns.Item("DocNum").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("DocNum"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Número SAP";
                oEditColumn.RightJustified = true;

                oGrid.Columns.Item("Folio").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Folio"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Número Folio";
                oEditColumn.RightJustified = true;

                oGrid.Columns.Item("Estado").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Estado"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Estado";

                oGrid.Columns.Item("U_Status").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_Status"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "U_Status";
                oEditColumn.Visible = false;

                oGrid.Columns.Item("Mensaje").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Mensaje"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Comentario";

                oGrid.Columns.Item("ObjType").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("ObjType"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "ObjType";
                oEditColumn.Visible = false;

                oGrid.Columns.Item("Key").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Key"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Key";
                oEditColumn.Visible = false;

                oGrid.AutoResizeColumns();
            }
            catch (Exception e)
            {
                OutLog("CargarDatos : " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.StatusBar.SetText("CargarDatos : " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }//fin CargarDatos

        private void RevisarEstado()
        {
            SqlConnection CnnADO = null;
            String sCnn = "";
            TFunctions Param;
            System.Data.DataTable dt;
            Int32 i;
            String ss;
            String sRUT;
            String sGlosa;
            Boolean bMultiSoc;
            String nMultiSoc;
            String User, Pass;
            SAPbobsCOM.Recordset oRecordSetAux;
            String sTabla;
            String FechaD, FechaH;
            SAPbobsCOM.Documents oDocuments;
            SAPbobsCOM.StockTransfer oStockTransfer;
            Boolean RecuperaTimbre = false;
            String EstadoDTE = "";
            String Timbre = "";
            String ObjType = "";
            String DocEntry = "";
            Int32 lRetCode;
            String sErrMsg;

            try
            {
                Param = new TFunctions();
                Param.SBO_f = FSBOf;
                oRecordSetAux = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select replace(replace(replace(TaxIdNum,',',''),'.',''),'-','') RUT from OADM";
                else
                    s = @"select replace(replace(replace(""TaxIdNum"",'',''),'.',''),'-','') ""RUT"" from ""OADM"" ";
                oRecordSet.DoQuery(s);
                sRUT = (System.String)(oRecordSet.Fields.Item("RUT").Value);


                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select ISNULL(U_Distrib,'N') 'Distribuido', ISNULL(U_MultiSoc,'N') MultiSoc, ISNULL(U_GenerarT,'N') GenerarT from [@VID_FEPARAM]";
                else
                    s = @"select IFNULL(""U_Distrib"",'N') ""Distribuido"", IFNULL(""U_MultiSoc"",'N') ""MultiSoc"", IFNULL(""U_GenerarT"",'N') ""GenerarT"" from ""@VID_FEPARAM"" ";
                oRecordSet.DoQuery(s);

                if (oRecordSet.RecordCount > 0)
                {
                    if ((System.String)(oRecordSet.Fields.Item("MultiSoc").Value) == "Y")
                        bMultiSoc = true;
                    else
                        bMultiSoc = false;

                    if ((System.String)(oRecordSet.Fields.Item("GenerarT").Value) == "Y") //si esta marcado genera timbre (Y) en parametros el monitor no debe recuperarlo del portal
                        RecuperaTimbre = false;
                    else
                        RecuperaTimbre = true;

                }
                else
                { bMultiSoc = false; }

                if (!bMultiSoc)
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"Select U_Servidor
                                  ,U_Base
                                  ,U_Usuario
                                  ,U_Password
                              from [@VID_FEPARAM] ";
                    else
                        s = @"Select ""U_Servidor""
                                   ,""U_Base""
                                   ,""U_Usuario""
                                   ,""U_Password""
                              from ""@VID_FEPARAM"" ";
                }
                else
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select U_Servidor, U_Base, U_Usuario, U_Password
                                from [@VID_FEMULTISOC] WITH (NOLOCK)";
                    else
                        s = @"select ""U_Servidor"", ""U_Base"", ""U_Usuario"", ""U_Password""
                               from ""@VID_FEMULTISOC"" ";
                }

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                {
                    if (!bMultiSoc)
                    {
                        User = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Usuario").Value));
                        Pass = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Password").Value));
                        sCnn = Param.sConexion((System.String)(oRecordSet.Fields.Item("U_Servidor").Value), (System.String)(oRecordSet.Fields.Item("U_Base").Value), User, Pass);
                    }

                    if (sCnn.Substring(0, 1) != "E")
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"select T0.DocEntry
                                      ,T0.U_DocEntry
                                      ,T0.U_SubType
                                      ,T0.U_FolioNum
                                      ,T0.U_ObjType
                                      ,T0.U_TipoDoc
                                      ,T0.U_Status
                                      ,T0.U_UserCode
                                  from [@VID_FELOG] T0 with (nolock)
                                 where {0}
                                   and T0.U_Status in ('EC','EE')
                                   and ISNULL(T0.U_DocDate, T0.CreateDate) BETWEEN '{1}' AND '{2}' ";
                        }
                        else
                        {
                            s = @"select T0.""DocEntry""
                                      ,T0.""U_DocEntry""
                                      ,T0.""U_SubType""
                                      ,T0.""U_FolioNum""
                                      ,T0.""U_ObjType""
                                      ,T0.""U_TipoDoc""
                                      ,T0.""U_Status""
                                      ,T0.""U_UserCode""
                                  from ""@VID_FELOG"" T0
                                 where {0}
                                   and T0.""U_Status"" in ('EC','EE')
                                   and IFNULL(T0.""U_DocDate"", T0.""CreateDate"") BETWEEN '{1}' AND '{2}' ";
                        }

                        oCheckBox = (CheckBox)(oForm.Items.Item("chk_Todo").Specific);

                        oEditText = (EditText)(oForm.Items.Item("FechaD").Specific);
                        FechaD = (System.String)(oEditText.Value).Trim();

                        oEditText = (EditText)(oForm.Items.Item("FechaH").Specific);
                        FechaH = (System.String)(oEditText.Value).Trim();

                        if (GlobalSettings.RunningUnderSQLServer)
                        { s = String.Format(s, oCheckBox.Checked == false ? "T0.U_UserCode = '" + FSBOApp.Company.UserName + "'" : "1=1", FechaD, FechaH); }
                        else
                        { s = String.Format(s, oCheckBox.Checked == false ? @"T0.""U_UserCode"" = '" + FSBOApp.Company.UserName + "'" : "1=1", FechaD, FechaH); }

                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount > 0)
                        {
                            //if (!bMultiSoc) CnnADO = new SqlConnection(sCnn);
                            CnnADO = new SqlConnection(sCnn);

                            dt = new System.Data.DataTable();

                            while (!oRecordSet.EoF)
                            {
                                s = (System.String)(oRecordSet.Fields.Item("U_ObjType").Value);
                                if (s == "15")
                                    sTabla = "ODLN";
                                else if (s == "14")
                                    sTabla = "ORIN";
                                else if (s == "67")
                                    sTabla = "OWTR";
                                else if (s == "21")
                                    sTabla = "ORPD";
                                else if (s == "18")
                                    sTabla = "OPCH";
                                else if (s == "203")
                                    sTabla = "ODPI";
                                else if (s == "204")
                                    sTabla = "ODPO";
                                else if (s == "19")
                                    sTabla = "ORPC";
                                else
                                    sTabla = "OINV";

                                ObjType = s;
                                DocEntry = Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_DocEntry").Value));

                                if (bMultiSoc)
                                {
                                    if (GlobalSettings.RunningUnderSQLServer)
                                        s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo'
                                                  , SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst 
                                              from {1} T0 JOIN NNM1 T2 ON T0.Series = T2.Series 
                                             where T0.DocEntry = {0}";
                                    else
                                        s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo""
                                                     , SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                                                 from ""{1}"" T0 JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                                                where T0.""DocEntry"" = {0} ";

                                    s = String.Format(s, (System.Double)(oRecordSet.Fields.Item("U_DocEntry").Value), sTabla);
                                    oRecordSetAux.DoQuery(s);
                                    s = (System.String)(oRecordSetAux.Fields.Item("DocSubType").Value);
                                    if ((System.String)(oRecordSetAux.Fields.Item("Tipo").Value) == "E")
                                    {
                                        nMultiSoc = (System.String)(oRecordSetAux.Fields.Item("Inst").Value);
                                        if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        { FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                                        else
                                        {
                                            if (GlobalSettings.RunningUnderSQLServer)
                                                s = @"SELECT U_Servidor, U_Base, U_Usuario, U_Password
                                                        FROM [@VID_FEMULTISOC] WITH (NOLOCK)
                                                       WHERE DocEntry = {0}";
                                            else
                                                s = @"SELECT ""U_Servidor"", ""U_Base"", ""U_Usuario"", ""U_Password""
                                                        FROM ""@VID_FEMULTISOC""
                                                       WHERE ""DocEntry"" = {0} ";
                                            s = String.Format(s, nMultiSoc);
                                            oRecordSetAux.DoQuery(s);
                                            if (oRecordSetAux.RecordCount > 0)
                                            {
                                                User = Param.DesEncriptar((System.String)(oRecordSetAux.Fields.Item("U_Usuario").Value));
                                                Pass = Param.DesEncriptar((System.String)(oRecordSetAux.Fields.Item("U_Password").Value));
                                                sCnn = Param.sConexion((System.String)(oRecordSetAux.Fields.Item("U_Servidor").Value), (System.String)(oRecordSetAux.Fields.Item("U_Base").Value), User, Pass);
                                                CnnADO = new SqlConnection(sCnn);
                                            }
                                            else
                                            { FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametros para conexión", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                                        }
                                    }

                                }

                                dt.Clear();
                                Timbre = "";
                                EstadoDTE = "";
                                //modificar para boletas en la tabla
                                s = @"select T0.Id_Log
                                          ,T0.Dyt_Evento
                                          ,T0.Dyt_GlosaErr
                                          ,T0.FechaInsercion
                                          ,T0.Dyt_TipoEvento
								                          ,ISNULL(EST.ESTADOVERIFDESC,'') [ESTADO_DTE]
								                          ,CASE WHEN ISNULL(EST.ESTADOVERIFDESC,'') LIKE '%Rechazados: 1%' THEN 'RZ'
										                         WHEN ISNULL(EST.ESTADOVERIFDESC,'') LIKE '%Reparos: 1%' THEN 'AR'
										                         WHEN ISNULL(EST.ESTADOVERIFDESC,'') LIKE '%Aceptados: 1%' THEN 'RR'
								                           ELSE 'EC'
								                           END [ESTADO]
                                                          ,ISNULL((select TOP 1 XML_TED from DTE_XML where FOLIO = DOC.FOLIO_SII and TIPODTE = DOC.TIPODTE and ISNULL(CAST(XML_TED as varchar(max)),'') <> ''),'') XML_DTE
                                                      from FaeT_LogHistoricoProcesoCab T0 with (nolock)
							                          LEFT JOIN ENCABEZADO_DOC DOC WITH(NOLOCK) ON DOC.FOLIO_SII = T0.Cab_Fol_Docto_Int
							                                                                   AND DOC.TIPODTE = T0.Cab_Cod_Tp_Factura
                                                      LEFT JOIN DTE_SET_DTE DTE WITH(NOLOCK) ON DOC.FOLIO_SII = DTE.FOLIO_SII
                                                                                  AND DOC.TIPODTE = DTE.TIPODTE
                                                      LEFT JOIN SET_DTE EST WITH(NOLOCK) ON EST.ID_SETDTE = DTE.ID_SETDTE
							                          LEFT JOIN PARAMETROS PARA WITH(NOLOCK) ON PARA.TIPO_PARAM = 'TipoEvento' AND PARA.VALOR = DOC.ESTADO
                                                     where T0.Cab_Cod_Tp_Factura  = '{0}'
                                                       And T0.Cab_Fol_Docto_Int = {1}
                                                     order by T0.FechaInsercion asc";
                                s = String.Format(s, (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value), (System.Double)(oRecordSet.Fields.Item("U_FolioNum").Value));
                                dt = Param.GetDataTable(s, CnnADO);
                                if (dt != null)
                                {
                                    if (dt.Rows.Count > 0)
                                    {
                                        oDBDSHeader.Clear();
                                        oDBDSD.Clear();
                                        i = 0;
                                        foreach (System.Data.DataRow myRow in dt.Rows)
                                        {
                                            sGlosa = myRow.Field<String>("Dyt_GlosaErr");
                                            if (i + 1 == dt.Rows.Count)
                                            {
                                                oDBDSHeader.InsertRecord(0);
                                                oDBDSHeader.SetValue("DocEntry", 0, Convert.ToString((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value)));
                                                oDBDSHeader.SetValue("U_DocEntry", 0, Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_DocEntry").Value)));
                                                oDBDSHeader.SetValue("U_SubType", 0, (System.String)(oRecordSet.Fields.Item("U_SubType").Value));
                                                oDBDSHeader.SetValue("U_FolioNum", 0, Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_FolioNum").Value)));
                                                oDBDSHeader.SetValue("U_ObjType", 0, (System.String)(oRecordSet.Fields.Item("U_ObjType").Value));
                                                oDBDSHeader.SetValue("U_TipoDoc", 0, (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value));
                                                oDBDSHeader.SetValue("U_UserCode", 0, (System.String)(oRecordSet.Fields.Item("U_UserCode").Value));
                                                if (myRow.Field<String>("ESTADO_DTE") == "")
                                                    oDBDSHeader.SetValue("U_Message", 0, sGlosa);
                                                else
                                                    oDBDSHeader.SetValue("U_Message", 0, myRow.Field<String>("ESTADO_DTE"));

                                                if (myRow.Field<String>("ESTADO") == "RZ")
                                                {
                                                    oDBDSHeader.SetValue("U_Status", 0, "RZ");
                                                    EstadoDTE = myRow.Field<String>("ESTADO");
                                                }
                                                else if ((myRow.Field<String>("ESTADO") == "RR") || (myRow.Field<String>("ESTADO") == "AR"))
                                                {
                                                    oDBDSHeader.SetValue("U_Status", 0, myRow.Field<String>("ESTADO"));
                                                    EstadoDTE = myRow.Field<String>("ESTADO");
                                                    if (myRow.Field<String>("XML_DTE") != "")
                                                        Timbre = myRow.Field<String>("XML_DTE");
                                                }
                                                else if (sGlosa.StartsWith("Ok, DTE enviado correctamente"))
                                                    oDBDSHeader.SetValue("U_Status", 0, "EC");
                                                else if (sGlosa.Substring(0, 5) == "Error")
                                                    oDBDSHeader.SetValue("U_Status", 0, "EE");
                                                else
                                                    oDBDSHeader.SetValue("U_Status", 0, (System.String)(oRecordSet.Fields.Item("U_Status").Value));

                                            }

                                            oDBDSD.InsertRecord(i);
                                            s = Convert.ToString(myRow.Field<Int32>("Id_Log"));
                                            oDBDSD.SetValue("U_ID_Log", i, s);
                                            s = Convert.ToString(myRow.Field<Int32>("Dyt_Evento"));
                                            oDBDSD.SetValue("U_Evento", i, s);
                                            oDBDSD.SetValue("U_Glosa", i, sGlosa);
                                            s = Convert.ToString(myRow.Field<DateTime>("FechaInsercion"));
                                            oDBDSD.SetValue("U_FechaIn", i, s);
                                            s = Convert.ToString(myRow.Field<Int32>("Dyt_TipoEvento"));
                                            oDBDSD.SetValue("U_TipoEvento", i, s);

                                            i++;
                                        }//fin ForEach

                                        i = Param.FELOGUpt(oDBDSHeader, oDBDSD);
                                        if (i == 0)
                                            OutLog("Error al actualizar Log de Documento Electronico " + (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value) + " " + Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_FolioNum").Value)));
                                        else
                                        {
                                            //actualizar campo crear en cabecera de documento con el estado
                                            if (ObjType == "67")
                                            {
                                                oStockTransfer = (SAPbobsCOM.StockTransfer)(FCmpny.GetBusinessObject(BoObjectTypes.oStockTransfer));
                                                if (oStockTransfer.GetByKey(Convert.ToInt32(DocEntry)))
                                                {
                                                    if (EstadoDTE == "RR")
                                                        oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                                    else if (EstadoDTE == "AR")
                                                        oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "E";
                                                    else if (EstadoDTE == "RZ")
                                                        oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "R";
                                                    else if (EstadoDTE == "EC")
                                                        oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                                    else if (EstadoDTE == "EE")
                                                        oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                                    else
                                                        oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "N";

                                                    if (RecuperaTimbre)
                                                        oStockTransfer.UserFields.Fields.Item("U_FETimbre").Value = Timbre;

                                                    lRetCode = oStockTransfer.Update();
                                                    if (lRetCode != 0)
                                                    {
                                                        sErrMsg = FCmpny.GetLastErrorDescription();
                                                        FSBOApp.StatusBar.SetText("No se actualizado estado de documento " + (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value) + " folio " + (System.Double)(oRecordSet.Fields.Item("U_FolioNum").Value) + " - " + sErrMsg, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                    }
                                                }
                                                oStockTransfer = null;
                                            }
                                            else
                                            {
                                                if (ObjType == "15")
                                                    oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oDeliveryNotes));
                                                else if (ObjType == "14")
                                                    oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oCreditNotes));
                                                else if (ObjType == "18")
                                                    oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oPurchaseInvoices));
                                                else if (ObjType == "19")
                                                    oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oPurchaseCreditNotes));
                                                else if (ObjType == "21")
                                                    oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oPurchaseReturns));
                                                else if (ObjType == "203")
                                                    oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oDownPayments));
                                                else if (ObjType == "204")
                                                    oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oPurchaseDownPayments));
                                                else
                                                    oDocuments = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(BoObjectTypes.oInvoices));

                                                if (oDocuments.GetByKey(Convert.ToInt32(DocEntry)))
                                                {
                                                    if (EstadoDTE == "RR")
                                                        oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                                    else if (EstadoDTE == "AR")
                                                        oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "E";
                                                    else if (EstadoDTE == "RZ")
                                                        oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "R";
                                                    else if (EstadoDTE == "EC")
                                                        oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                                    else if (EstadoDTE == "EE")
                                                        oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                                    else
                                                        oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "N";

                                                    if (RecuperaTimbre)
                                                        oDocuments.UserFields.Fields.Item("U_FETimbre").Value = Timbre;

                                                    lRetCode = oDocuments.Update();
                                                    if (lRetCode != 0)
                                                    {
                                                        sErrMsg = FCmpny.GetLastErrorDescription();
                                                        FSBOApp.StatusBar.SetText("No se actualizado estado de documento " + (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value) + " folio " + (System.Double)(oRecordSet.Fields.Item("U_FolioNum").Value) + " - " + sErrMsg, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                    }
                                                }
                                                oDocuments = null;

                                            }
                                        }
                                    }
                                }

                                if (bMultiSoc)
                                { CnnADO.Close(); }

                                oRecordSet.MoveNext();
                            }//fin while

                            if (!bMultiSoc) CnnADO.Close();
                        }
                    }
                    else
                    { FSBOApp.StatusBar.SetText("Faltan datos Conexion. " + sCnn.Substring(1, sCnn.Length - 1), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
                }
                else
                { FSBOApp.StatusBar.SetText("Debe ingresar datos de conexion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
            }
            catch (Exception e)
            {
                OutLog("RevisarEstado : " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.StatusBar.SetText("RevisarEstado : " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }//fin RevisarEstado

        private void TTimer()
        {
            SAPbouiCOM.CheckBox oCheckBox;
            ttime.Interval = 60000;

            ttime.Enabled = true;
            ttime.Elapsed += new System.Timers.ElapsedEventHandler(Timercall);

            oCheckBox = (SAPbouiCOM.CheckBox)(oForm.Items.Item("chkActEst").Specific);
            if (oCheckBox.Checked)
            {
                ttime.Enabled = true;
                ttime.Start();
            }
            else
            {
                ttime.Stop();
                ttime.Enabled = false;
            }
        }//fin TTimer


        private void Timercall(System.Object sender, EventArgs e)
        {
            ttime.Stop();

            RevisarEstado();
            CargarDatos();

            ttime.Start();
        }//fin Timercall


    }//fin class
}
