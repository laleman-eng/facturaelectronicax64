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

namespace Factura_Electronica_VK.ImptoAdicional
{
    class TImptoAdicional : TvkBaseForm,IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private String s;
        private Boolean Flag;
        private SAPbouiCOM.Matrix mtx;
        private SAPbouiCOM.Grid oGrid;
        private SAPbouiCOM.DataTable oDataTable;
        private SAPbouiCOM.GridColumn oColumn;
        private SAPbouiCOM.DBDataSource oDBDSHeader;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                FSBOf.LoadForm(xmlPath, "VID_FEIMPADIC.srf", uid);
                oForm = FSBOApp.Forms.Item(uid);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All
                Flag = false;
                oForm.Freeze(true);

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select count(*) Cont from [@VID_FEIMPADIC]";
                else
                    s = @"select count(*) ""Cont"" from ""@VID_FEIMPADIC"" ";
                oRecordSet.DoQuery(s);
                if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                    oForm.Mode = BoFormMode.fm_UPDATE_MODE;
                else
                    oForm.Mode = BoFormMode.fm_ADD_MODE;


                oGrid = (Grid)(oForm.Items.Item("3").Specific);
                oDBDSHeader = oForm.DataSources.DBDataSources.Add("@VID_FEIMPADIC");

                oDataTable = oForm.DataSources.DataTables.Add("Tax");
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select Code, U_CodImpto, U_Desc, U_Porc from [@VID_FEIMPADIC]
                          UNION ALL 
                          select CAST('' as varchar(20)), CAST('' as varchar(20)), CAST('' as varchar(50)), 0";
                else
                    s = @"select ""Code"", ""U_CodImpto"", ""U_Desc"", ""U_Porc"" from ""@VID_FEIMPADIC""
                          UNION ALL
                          select CAST('' as varchar(20)), CAST('' as varchar(20)), CAST('' as varchar(50)), 0 FROM DUMMY ";
                
                oDataTable.ExecuteQuery(s);
                oGrid.DataTable = oDataTable;

                oGrid.Columns.Item("Code").Type = BoGridColumnType.gct_ComboBox;
                oColumn = (GridColumn)(oGrid.Columns.Item("Code"));
                var oComboCol = (ComboBoxColumn)(oColumn);
                oComboCol.Editable = true;
                oComboCol.TitleObject.Caption = "Impuesto SAP";

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select Code, Name from OSTA
                          UNION ALL
                          select WTCode 'Code', WTName 'Name' from OWHT where Inactive = 'N'";
                else
                    s = @"select ""Code"", ""Name"" from ""OSTA""
                          UNION ALL
                          select ""WTCode"" ""Code"", ""WTName"" ""Name"" from ""OWHT"" where ""Inactive"" = 'N' ";

                oRecordSet.DoQuery(s);
                FSBOf.FillComboGrid((GridColumn)(oGrid.Columns.Item("Code")), ref oRecordSet, true);

                oGrid.Columns.Item("U_CodImpto").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_CodImpto"));
                var oEditCol = (EditTextColumn)(oColumn);
                oEditCol.Editable = true;
                oEditCol.TitleObject.Caption = "Código Impto. SII";


                oGrid.Columns.Item("U_Desc").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_Desc"));
                oEditCol = (EditTextColumn)(oColumn);
                oEditCol.Editable = true;
                oEditCol.TitleObject.Caption = "Descripción Impuesto";

                oGrid.Columns.Item("U_Porc").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_Porc"));
                oEditCol = (EditTextColumn)(oColumn);
                oEditCol.Editable = true;
                oEditCol.TitleObject.Caption = "Porcentaje Retencion";
                oEditCol.RightJustified = true;

                oGrid.AutoResizeColumns();
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
            Int32 nErr;
            String sErr;
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);

            try
            {
                if (pVal.ItemUID == "3")
                {
                    if ((pVal.ColUID == "Code") && (!pVal.BeforeAction) && (pVal.EventType == BoEventTypes.et_COMBO_SELECT))
                    {
                        if (pVal.Row == oDataTable.Rows.Count -1)
                        {
                            if ((System.String)(oDataTable.GetValue("Code", oDataTable.Rows.Count -1)) != "")
                            {
                                oDataTable.Rows.Add(1);
                                oDataTable.SetValue("Code", oDataTable.Rows.Count -1, "");
                                oDataTable.SetValue("U_CodImpto", oDataTable.Rows.Count-1, "");
                                oDataTable.SetValue("U_Desc", oDataTable.Rows.Count-1, "");
                                oDataTable.SetValue("U_Porc", oDataTable.Rows.Count - 1, 0);
                            }
                        }
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if ((pVal.ItemUID == "1") && ((oForm.Mode == BoFormMode.fm_ADD_MODE) || (oForm.Mode == BoFormMode.fm_UPDATE_MODE)))
                    {
                        if (LimpiarGrid())
                        {
                            BubbleEvent = CrearDatos();
                            if ((BubbleEvent) && (oForm.Mode == BoFormMode.fm_ADD_MODE))
                            {
                                oForm.Mode = BoFormMode.fm_OK_MODE;
                                BubbleEvent = false;
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



        private Boolean LimpiarGrid()
        {
            Boolean _result;
            Int32 i;

            try
            {
                _result = true;
                i = 0;
                while (i < oDataTable.Rows.Count)
                {
                    if ((System.String)(oDataTable.GetValue("Code", i)).ToString().Trim() == "")
                    {
                        oDataTable.Rows.Remove(i);
                        i = i - 1;
                    }
                    else if ((System.String)(oDataTable.GetValue("U_CodImpto", i)).ToString().Trim() == "") 
                    {
                        oDataTable.Rows.Remove(i);
                        i = i - 1;
                    }
                    i++;
                }

                return _result;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("LimpiarGrid " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
        }//fin LimpiarGrid



        private Boolean CrearDatos()
        {
            Boolean _result;
            Int32 i;
            TFunctions Functions;

            try
            {
                _result = true;
                i = 0;
                oDBDSHeader.Clear();
                Functions = new TFunctions();
                Functions.SBO_f = FSBOf;
                if (GlobalSettings.RunningUnderSQLServer)
                    s = "select Code, U_CodImpto, U_Desc, U_Porc from [@VID_FEIMPADIC]";
                else
                    s = @"select ""Code"", ""U_CodImpto"", ""U_Desc"", ""U_Porc"" from ""@VID_FEIMPADIC"" ";
                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                    Functions.ImpAdicDel(ref oRecordSet);

                while (i < oDataTable.Rows.Count)
                {
                    oDBDSHeader.InsertRecord(0);
                    oDBDSHeader.SetValue("Code", 0, (System.String)(oDataTable.GetValue("Code", i)).ToString().Trim());
                    oDBDSHeader.SetValue("U_CodImpto", 0, (System.String)(oDataTable.GetValue("U_CodImpto", i)).ToString().Trim());
                    oDBDSHeader.SetValue("U_Desc", 0, (System.String)(oDataTable.GetValue("U_Desc", i)).ToString().Trim());
                    oDBDSHeader.SetValue("U_Porc", 0, FSBOf.DoubleToStr(((System.Double)oDataTable.GetValue("U_Porc", i))).Trim());

                    _result = Functions.ImpAdicAdd(oDBDSHeader);

                    i++;
                }

                oDataTable.Rows.Add(1);
                oDataTable.SetValue("Code", oDataTable.Rows.Count -1, "");
                oDataTable.SetValue("U_CodImpto", oDataTable.Rows.Count-1, "");
                oDataTable.SetValue("U_Desc", oDataTable.Rows.Count-1, "");
                oDataTable.SetValue("U_Porc", oDataTable.Rows.Count - 1, 0);

                return _result;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CrearDatos " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
        }//fin CrearDatos


    }//fin Class
}
