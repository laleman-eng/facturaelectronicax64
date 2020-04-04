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
using System.Data.SqlClient;
using VisualD.vkFormInterface;
using Factura_Electronica_VK.Functions;

namespace Factura_Electronica_VK.DetalleLog
{
    class TDetalleLog : TvkBaseForm, IvkFormInterface
    {
        private SAPbouiCOM.DataTable dt;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.Grid ogrid;
        private SAPbouiCOM.GridColumn oColumn;
        private String s;

        public static String prmKey
        { get; set; }

       
        public new bool InitForm(   string uid, string xmlPath, ref            Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
    
                //Lista    := New list<string>;

                FSBOf.LoadForm(xmlPath, "VID_FEDetalleLog.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All
                oForm.EnableMenu("1282", false); //Crear
                oForm.EnableMenu("1281", false); //Actualizar
   
                //oForm.DataBrowser.BrowseBy := "DocNum"; 

                                        // Ok Ad  Fnd Vw Rq Sec
                //Lista.Add('DocNum    , f,  f,  t,  f, n, 1');
                //Lista.Add('DocDate   , f,  t,  f,  f, r, 1');
                //Lista.Add('CardCode  , f,  t,  t,  f, r, 1');
                //FSBOf.SetAutoManaged(var oForm, Lista);

                //s := '1';
                //oCombo.Select(s, BoSearchKey.psk_ByValue);

                oForm.Items.Item("grid").Enabled = false;
                ogrid = (Grid)(oForm.Items.Item("grid").Specific);
                dt = oForm.DataSources.DataTables.Add("dt");
                ogrid.DataTable = dt;

                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"select U_TipoDoc
                              ,LTRIM(STR(U_FolioNum,18,0)) 'U_FolioNum'
                              ,U_ID_Log
                              ,U_Glosa
                              ,U_FechaIn
                          from [@VID_FELOG] T0
                          join [@VID_FELOGD] T1 ON T1.DocEntry = T0.DocEntry
                         where T0.DocEntry = {0}
                         order by T1.LineId DESC"; }
                else
                {   s = @"select ""U_TipoDoc""
                              ,LTRIM(ROUND(""U_FolioNum"",0)) ""U_FolioNum""
                              ,""U_ID_Log""
                              ,""U_Glosa""
                              ,""U_FechaIn""
                          from ""@VID_FELOG"" T0 
                          join ""@VID_FELOGD"" T1 ON T1.""DocEntry"" = T0.""DocEntry""
                         where T0.""DocEntry"" = {0}
                         order by T1.""LineId"" DESC "; }

                s = String.Format(s, prmKey);
                dt.ExecuteQuery(s);
                ogrid.AutoResizeColumns();

                ogrid.Columns.Item("U_TipoDoc").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_TipoDoc"));
                var oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Tipo Documento";
                oEditColumn.RightJustified = true;

                ogrid.Columns.Item("U_FolioNum").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_FolioNum"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Número Folio";
                oEditColumn.RightJustified = true;

                ogrid.Columns.Item("U_ID_Log").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_ID_Log"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "ID Log Portal";
                oEditColumn.RightJustified = true;

                ogrid.Columns.Item("U_Glosa").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_Glosa"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Glosa";
                oEditColumn.RightJustified = false;

                ogrid.Columns.Item("U_FechaIn").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_FechaIn"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Fecha Movimiento";
                oEditColumn.RightJustified = false;

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
            //String sValue;
            //SAPbouiCOM.DataTable oDataTable;
            //String TipoDoc;
            //SAPbouiCOM.LinkedButton oLink;
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction == false) && (pVal.ItemUID == "btn1"))
                {
                    BubbleEvent = false;
                }
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin FormEvent

    }//fin class
}
