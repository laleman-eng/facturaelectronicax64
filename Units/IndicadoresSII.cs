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

namespace Factura_Electronica_VK.IndicadoresSII
{
    class TIndicadoresSII : TvkBaseForm, IvkFormInterface
    {
        private SAPbouiCOM.DBDataSource oDBDSH;
        private SAPbouiCOM.DBDataSource oDBDSD;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Matrix oMtx;
        private SAPbouiCOM.Form oForm;
        private String s;


        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            //SAPbouiCOM.ComboBox oCombo;
            SAPbouiCOM.Column oColumn;
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                //Lista    := New list<string>;

                FSBOf.LoadForm(xmlPath, "VID_IndicadoresSII.srf", uid);
                //EnableCrystal := true;
                //oForm = FSBOApp.Forms.Item(uid);
                oForm = FSBOApp.Forms.ActiveForm;
                oForm.Freeze(true);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All

                VID_DelRow = true;
                VID_DelRowOK = true;
   
                oForm.DataBrowser.BrowseBy = "Code"; 
                oDBDSH = oForm.DataSources.DBDataSources.Item("@VID_FEDOCE");
                oDBDSD = oForm.DataSources.DBDataSources.Item("@VID_FEDOCED");

                                    // Ok Ad  Fnd Vw Rq Sec
                //Lista.Add('DocNum    , f,  f,  t,  f, n, 1');
                //Lista.Add('DocDate   , f,  t,  f,  f, r, 1');
                //Lista.Add('CardCode  , f,  t,  t,  f, r, 1');
                //FSBOf.SetAutoManaged(var oForm, Lista);

                //oCombo := ComboBox(oForm.Items.Item('TipDoc').Specific);
                //oCombo.ValidValues.Add('33', 'Factura');

                //s := '1';
                //oCombo.Select(s, BoSearchKey.psk_ByValue);

                AddChooseFromList();
                oMtx = (Matrix)(oForm.Items.Item("mtx").Specific);
                oColumn = (SAPbouiCOM.Column)(oMtx.Columns.Item("V_0")); 
                oColumn.ChooseFromListUID = "CFL0";
                oColumn.ChooseFromListAlias = "Code"; 

                oMtx.AutoResizeColumns();

                //EditText(oForm.Items.Item('CardCode').Specific).Active := True;
                //oForm.Mode := BoFormMode.fm_OK_MODE;
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
            SAPbouiCOM.DataTable oDataTable;
            String sValue,sValue1;
            SAPbouiCOM.IChooseFromListEvent oCFLEvento = null;

            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if ((pVal.ItemUID == "1") && (oForm.Mode == BoFormMode.fm_ADD_MODE))
                    {
                //BubbleEvent := ValidarDatosFE();
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_CHOOSE_FROM_LIST) && (!pVal.BeforeAction))
                {
                    if (pVal.ColUID == "V_0")
                    {
                        oCFLEvento = (SAPbouiCOM.IChooseFromListEvent)(pVal);
                        oDataTable = oCFLEvento.SelectedObjects;
                        if (oDataTable != null)
                        {
                            sValue = (System.String)(oDataTable.GetValue("Code", 0));
                            sValue1 = (System.String)(oDataTable.GetValue("Name", 0));
           
                            oMtx.FlushToDataSource();
                            oDBDSD.SetValue("U_Indicato", pVal.Row-1, sValue);
                            oDBDSD.SetValue("U_Descrip", pVal.Row-1, sValue1);
                            if (pVal.Row == oMtx.RowCount)
                            {
                                oDBDSD.InsertRecord(pVal.Row);
                            }
                            oMtx.LoadFromDataSource();
                            oMtx.AutoResizeColumns();
                            if (oForm.Mode == BoFormMode.fm_OK_MODE)
                            {   oForm.Mode = BoFormMode.fm_UPDATE_MODE; }
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


        private void AddChooseFromList()
        {
            SAPbouiCOM.ChooseFromListCollection oCFLs;
            SAPbouiCOM.ChooseFromList oCFL;
            SAPbouiCOM.ChooseFromListCreationParams oCFLCreationParams;
            //SAPbouiCOM.Conditions oCons;
            //SAPbouiCOM.Condition oCon;

            try
            {
                oCFLs = oForm.ChooseFromLists;
                oCFLCreationParams = (ChooseFromListCreationParams)(FSBOApp.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_ChooseFromListCreationParams));

                oCFLCreationParams.MultiSelection = false;
                oCFLCreationParams.ObjectType = "138";     
                oCFLCreationParams.UniqueID = "CFL0";
                oCFL = oCFLs.Add(oCFLCreationParams);

                //oCFL.SetConditions(nil);
	            //// Adding Conditions to CFL0
	            //oCons := oCFL.GetConditions();
	            //// Condition 1: U_Estado = "C"
	            //oCon := oCons.Add();
	            //oCon.Alias := "U_Estado";
	            //oCon.Operation := SAPbouiCOM.BoConditionOperation.co_EQUAL;
	            //oCon.CondVal := "C";
	            //oCFL.SetConditions(oCons);
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace,1,"Ok","","");
                OutLog("CFL: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin AddChooseFromList


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

                if ((pVal.MenuUID != "") && (pVal.BeforeAction == false))
                {
                    if ((pVal.MenuUID == "1288") || (pVal.MenuUID == "1289") || (pVal.MenuUID == "1290") || (pVal.MenuUID == "1291"))
                    {
                        if (oMtx.RowCount > 0)
                        {
                            if ((System.String)(oDBDSD.GetValue("U_Indicato", oMtx.RowCount-1)) != "")
                            {
                                oMtx.AddRow(1,oMtx.RowCount);
                                oMtx.FlushToDataSource();
                                oDBDSD.SetValue("U_Indicato",oMtx.RowCount-1,"");
                                oDBDSD.SetValue("U_Descrip",oMtx.RowCount-1,"");
                                oMtx.LoadFromDataSource();
                            }
                        }
                    }
     
                    if (pVal.MenuUID == "1282")
                    {
                        oMtx.AddRow(1,1);
                        oMtx.FlushToDataSource();
                        oDBDSD.SetValue("U_Indicato",oMtx.RowCount-1,"");
                        oDBDSD.SetValue("U_Descrip",oMtx.RowCount-1,"");
                        oMtx.LoadFromDataSource();
                    }
                }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace,1,"Ok","","");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent

    }//fin class
}
