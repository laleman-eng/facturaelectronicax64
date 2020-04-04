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

namespace Factura_Electronica_VK.Sucursal
{
    class TSucursal : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private String s;
        private SAPbouiCOM.Matrix oMtx;
        private List<string> Lista;
        private SAPbouiCOM.DBDataSource oDBDSH;
        private SAPbouiCOM.Column oColumn;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            Int32 i;
            //SAPbouiCOM.EditTextColumn oEditText;
            SAPbouiCOM.CommonSetting oSetting;
            TFunctions Reg;
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                Lista = new List<string>();

                FSBOf.LoadForm(xmlPath, "VID_Sucursal.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = true;
                oForm.SupportedModes = -1;             // afm_All

                //        VID_DelRow := true;
                //        VID_DelRowOK := true;
   
                //oForm.DataBrowser.BrowseBy := "Code"; 
                oDBDSH = oForm.DataSources.DBDataSources.Item("@VID_FESUC");

                                    // Ok Ad  Fnd Vw Rq Sec
                Lista.Add("mtx      , f,  t,  f,  f, r, 1");
                //Lista.Add('Name      , f,  t,  t,  f, r, 1');
                //Lista.Add('CardCode  , f,  t,  t,  f, r, 1');
                //FSBOf.SetAutoManaged(var oForm, Lista);

                //oCombo := ComboBox(oForm.Items.Item('TipDoc').Specific);
                //oCombo.ValidValues.Add('33', 'Factura');

                //s := '1';
                //oCombo.Select(s, BoSearchKey.psk_ByValue);

                //        AddChooseFromList();
                oMtx =(Matrix)(oForm.Items.Item("mtx").Specific);
                //        oColumn                    := SAPbouiCOM.Column(oMtx.Columns.Item('V_0')); 
                //        oColumn.ChooseFromListUID  := 'CFL0';
                //        oColumn.ChooseFromListAlias:= 'Code'; 
                //        oMtx.AutoResizeColumns();

                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"select Code, Name, ISNULL(U_Habilitada,'Y') 'Habilitada' from [@VID_FESUC]"; }
                else
                {   s = @"select ""Code"", ""Name"", IFNULL(""U_Habilitada"",'Y') ""Habilitada"" from ""@VID_FESUC"" ";}
                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                    {   s = "select Code from [@VID_FESUC] where Code = 'Principal'"; }
                    else
                    {   s = @"select ""Code"" from ""@VID_FESUC"" where ""Code"" = 'Principal' "; }
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount == 0)
                    {
                        Reg = new TFunctions();
                        Reg.SBO_f = FSBOf;
                        oDBDSH.Clear();
                        oDBDSH.InsertRecord(0);
                        oDBDSH.SetValue("Code", 0, "Principal");
                        oDBDSH.SetValue("Name", 0, "SAP BO");
                        oDBDSH.SetValue("U_Habilitada", 0, "Y");
                        if (Reg.FESUCAdd(oDBDSH) == true)
                        {   FSBOApp.StatusBar.SetText("Sucursal Principal creada correctamente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success); }
                        else
                        {   FSBOApp.StatusBar.SetText("Sucursal Principal no ha sido creada", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
                    }
                }
        
                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"select Code, Name, ISNULL(U_Habilitada,'Y') 'Habilitada' from [@VID_FESUC]"; }
                else
                {   s = @"select ""Code"", ""Name"", IFNULL(""U_Habilitada"",'Y') ""Habilitada"" from ""@VID_FESUC"" "; }
                oRecordSet.DoQuery(s);
                i = 0;
                oDBDSH.Clear();
                while (!oRecordSet.EoF)
                {
                    oDBDSH.InsertRecord(i);
                    oDBDSH.SetValue("Code", i, (System.String)(oRecordSet.Fields.Item("Code").Value));
                    oDBDSH.SetValue("Name", i, (System.String)(oRecordSet.Fields.Item("Name").Value));
                    oDBDSH.SetValue("U_Habilitada", i, (System.String)(oRecordSet.Fields.Item("Habilitada").Value));
                    oRecordSet.MoveNext();
                    i++;
                }

                oDBDSH.InsertRecord(i);
                oDBDSH.SetValue("Code", i, "");
                oDBDSH.SetValue("Name", i, "");
                oDBDSH.SetValue("U_Habilitada", i, "Y");

                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"select FldValue 'Code', Descr 'Name' from UFD1 where TableID = '@VID_FESUC' and FieldID = 0"; }
                else
                {   s = @"select ""FldValue"" ""Code"", ""Descr"" ""Name"" from ""UFD1"" where ""TableID"" = '@VID_FESUC' and ""FieldID"" = 0"; }
                oRecordSet.DoQuery(s);
                oColumn = (SAPbouiCOM.Column)(oMtx.Columns.Item("Habilitada"));
                FSBOf.FillComboMtx(oColumn, ref oRecordSet, false);

                //EditText(oForm.Items.Item('CardCode').Specific).Active := True;
                oMtx.LoadFromDataSource();

                oSetting = oMtx.CommonSetting;
                i = 1;
                while (i <= oMtx.RowCount)
                {
                    if ((System.String)(oDBDSH.GetValue("Code",  i-1)).Trim() != "")
                    {   oSetting.SetCellEditable(i, 1, false); }
                    i++;
                }

                oForm.Mode = BoFormMode.fm_OK_MODE;
                oMtx.AutoResizeColumns();
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
            SAPbouiCOM.EditText oEditText;
            SAPbouiCOM.ComboBox oComboBox;
            SAPbouiCOM.CommonSetting oSetting;
            Int32 i;

            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED)
                {
                    if ((pVal.ItemUID == "1") && ((oForm.Mode == BoFormMode.fm_ADD_MODE) || (oForm.Mode == BoFormMode.fm_UPDATE_MODE)) && (pVal.BeforeAction))
                    {
                        BubbleEvent = false;
                        if (Limpiar())
                        {
                            GuardarRegistros();
                            oMtx.FlushToDataSource();
                            i = oMtx.RowCount + 1;
                            oMtx.AddRow(1, i);
                            oEditText = (EditText)(oMtx.Columns.Item("Code").Cells.Item(i).Specific);
                            oEditText.Value = "";

                            oEditText = (EditText)(oMtx.Columns.Item("Name").Cells.Item(i).Specific);
                            oEditText.Value = "";

                            oComboBox = (ComboBox)(oMtx.Columns.Item("Habilitada").Cells.Item(i).Specific);
                            oComboBox.Select("Y", BoSearchKey.psk_ByValue);


                            oSetting = oMtx.CommonSetting;
                            i = 1;
                            while (i < oMtx.RowCount)
                            {
                                if ((System.String)(oDBDSH.GetValue("Code", i - 1)).Trim() != "")
                                { oSetting.SetCellEditable(i, 1, false); }
                                i++;
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


        private void GuardarRegistros()
        {
            TFunctions Reg;
            Int32 i;
            SAPbouiCOM.EditText oEditText;
            SAPbouiCOM.ComboBox oComboBox;
            String Code, Name, Hab;
            Boolean _return = false;

            try
            {
                Reg = new TFunctions();
                Reg.SBO_f = FSBOf;

                oMtx.FlushToDataSource();
                i = 1;
                while (i <= oMtx.RowCount)
                {
                    oEditText = (EditText)(oMtx.Columns.Item("Code").Cells.Item(i).Specific);
                    Code = (System.String)(oEditText.Value).Trim();

                    oEditText = (EditText)(oMtx.Columns.Item("Name").Cells.Item(i).Specific);
                    Name = (System.String)(oEditText.Value).Trim();
            
                    oComboBox = (ComboBox)(oMtx.Columns.Item("Habilitada").Cells.Item(i).Specific);
                    Hab = oComboBox.Value;

                    oDBDSH.Clear();
                    oDBDSH.InsertRecord(0);
                    oDBDSH.SetValue("Code", 0, Code);
                    oDBDSH.SetValue("Name", 0, Name);
                    oDBDSH.SetValue("U_Habilitada", 0, Hab);

                    if (GlobalSettings.RunningUnderSQLServer)
                    {   s = @"select count(*) 'cont' from [@VID_FESUC] where Code = '{0}'"; }
                    else
                    {   s = @"select COUNT(*) ""cont"" from ""@VID_FESUC"" where ""Code"" = '{0}' "; }
                    s = String.Format(s, Code);
                    oRecordSet.DoQuery(s);
                    if ((System.Int32)(oRecordSet.Fields.Item("cont").Value) == 0)
                    {   _return = Reg.FESUCAdd(oDBDSH); }
                    else
                    {   _return = Reg.FESUCUpd(oDBDSH); }

                    i++;
                }

                if (_return) oForm.Mode = BoFormMode.fm_OK_MODE;
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace,1,"Ok","","");
                OutLog("GuardarRegistros: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin GuardarRegistro


        private Boolean Limpiar()
        {
            Boolean _result;
            Int32 i;
            SAPbouiCOM.EditText oEditText;
            
            try
            {
                _result = true;
                oMtx.FlushToDataSource();
                i = 1;
                while (i <= oMtx.RowCount)
                {
                    oEditText = (EditText)(oMtx.Columns.Item("Code").Cells.Item(i).Specific);
                    if ((System.String)(oEditText.Value).Trim() == "")
                    {
                        oMtx.DeleteRow(i);
                        i = i - 1;
                    }
                    i++;
                }
                oMtx.FlushToDataSource();
        
                return _result;
            }
            catch(Exception e)
            {
                OutLog("Limpiar: " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
        }//fin Limpiar



        public new void MenuEvent(ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
            //Reg : TRemuneraciones_MyFunctions;
            Int32 Entry;
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
        
                if ((pVal.MenuUID != "") && (pVal.BeforeAction == false))
                {
                    if ((pVal.MenuUID == "1288") || (pVal.MenuUID == "1289") || (pVal.MenuUID == "1290") || (pVal.MenuUID == "1291"))
                    {
              
                    }
                }
     
                if ((pVal.MenuUID == "1282") || (pVal.MenuUID == "1281"))
                {
                    oDBDSH.SetValue("Name", 0, "");
                    oDBDSH.SetValue("Code", 0, "");
                }
                //inherited MenuEvent(Var pVal,var BubbleEvent);
            }
            catch(Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace,1,"Ok","","");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent

    }
}
