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
using Factura_Electronica_VK.Libros;

namespace Factura_Electronica_VK.GLibro
{
    class TGLibro : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        //private SAPbouiCOM.Grid ogrid;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.EditText oEditText;
        private SAPbouiCOM.ComboBox oComboBox;
        private String s;
        private Boolean bMultiSoc = false;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            //SAPbouiCOM.ComboBox oComboBox;
            SAPbouiCOM.Column oColumn;

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                //Lista    := New list<string>;

                FSBOf.LoadForm(xmlPath, "VID_GLibro.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All

                oForm.EnableMenu("1281", false);//Actualizar
                oForm.EnableMenu("1282", false);//Crear

                oForm.DataSources.UserDataSources.Add("TipoLibro", BoDataType.dt_SHORT_TEXT);
                oComboBox = (ComboBox)(oForm.Items.Item("TipoLibro").Specific);
                oComboBox.DataBind.SetBound(true, "", "TipoLibro");
                oComboBox.ValidValues.Add("V", "Venta");
                oComboBox.ValidValues.Add("C", "Compra");
                oComboBox.ValidValues.Add("B", "Boletas");
                oComboBox.ValidValues.Add("G", "Guias");
                oComboBox.Select("V", BoSearchKey.psk_ByValue);

                oForm.DataSources.UserDataSources.Add("Desde", BoDataType.dt_DATE, 10);
                oEditText = (EditText)(oForm.Items.Item("Desde").Specific);
                oEditText.DataBind.SetBound(true, "", "Desde");
                oEditText.Value = DateTime.Now.ToString("yyyyMM") + "01";

                oForm.DataSources.UserDataSources.Add("Hasta", BoDataType.dt_DATE, 10);
                oEditText = (EditText)(oForm.Items.Item("Hasta").Specific);
                oEditText.DataBind.SetBound(true, "", "Hasta");
                oEditText.Value = DateTime.Now.ToString("yyyyMMdd");

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select ISNULL(U_MultiSoc,'N') MultiSoc from [@VID_FEPARAM]";
                else
                    s = @"select IFNULL(""U_MultiSoc"",'N') ""MultiSoc"" from ""@VID_FEPARAM"" ";
                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                    if (((System.String)oRecordSet.Fields.Item("MultiSoc").Value) == "Y")
                        bMultiSoc = true;

                if (bMultiSoc)
                {
                    oForm.Items.Item("LInstituto").Visible = true;
                    oForm.Items.Item("Instituto").Visible = true;
                    oComboBox = (ComboBox)(oForm.Items.Item("Instituto").Specific);
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select DocEntry, U_Sociedad 'Sociedad' from [@VID_FEMULTISOC] where isnull(U_Habilitada,'N') = 'Y'";
                    else
                        s = @"select ""DocEntry"", ""U_Sociedad"" ""Sociedad"" from ""@VID_FEMULTISOC"" where IFNULL(""U_Habilitada"",'N') = 'Y' ";
                    oRecordSet.DoQuery(s);
                    while (!oRecordSet.EoF)
                    {
                        oComboBox.ValidValues.Add(((System.Int32)oRecordSet.Fields.Item("DocEntry").Value).ToString(), ((System.String)oRecordSet.Fields.Item("Sociedad").Value).Trim());
                        oRecordSet.MoveNext();
                    }

                }
                else
                {
                    oForm.Items.Item("LInstituto").Visible = false;
                    oForm.Items.Item("Instituto").Visible = false;
                }

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
            IvkFormInterface oFormB;
            String oUid;

            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction))
                {
                    if (pVal.ItemUID == "btn1") 
                    {
                        oFormB = (IvkFormInterface)(new TLibros());
                        oComboBox = (ComboBox)(oForm.Items.Item("TipoLibro").Specific);
                        TLibros.TipoLibro = oComboBox.Selected.Value;
                        oEditText = (EditText)(oForm.Items.Item("Desde").Specific);
                        TLibros.Desde = oEditText.Value;
                        oEditText = (EditText)(oForm.Items.Item("Hasta").Specific);
                        TLibros.Hasta = oEditText.Value;
                        TLibros.bMultiSoc = bMultiSoc;
                        oUid = FSBOf.generateFormId(FGlobalSettings.SBOSpaceName, FGlobalSettings);
                        oFormB.InitForm(oUid, "forms\\", ref FSBOApp, ref FCmpny, ref FSBOf, ref FGlobalSettings);
                        FoForms.Add(oFormB);
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
                        ;
                    }

                    if (pVal.MenuUID == "1282")
                    {
                        ;
                    }
                }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent


    }//fin class
}
