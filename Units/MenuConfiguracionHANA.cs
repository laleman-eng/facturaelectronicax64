using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Configuration;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using VisualD.SBOObjectMg1;
using VisualD.Main;
using VisualD.MainObjBase;
using System.Threading;
using System.Data.SqlClient;
using SAPbouiCOM;
using SAPbobsCOM;
using System.IO;
using System.Data;
using VisualD.ADOSBOScriptExecute;
using Factura_Electronica_VK.Functions;

namespace Factura_Electronica_VK.MenuConfiguracionHANA
{
    public class TMenuConfiguracionHANA : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.DBDataSource oDBDSHeader;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.ComboBox oComboBox;
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
                FSBOf.LoadForm(xmlPath, "VID_MenuConf.srf", uid);

                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.Visible = true;
                oForm.AutoManaged = true;
                oForm.SupportedModes = -1;

                ((StaticText)oForm.Items.Item("18").Specific).Caption = "Conexión a HANA";
                oForm.Mode = BoFormMode.fm_ADD_MODE;

                oDBDSHeader = oForm.DataSources.DBDataSources.Item("@VID_MENUSU");

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT Code FROM [@VID_MENUSU]";
                else
                    s = @"SELECT ""Code"" FROM ""@VID_MENUSU"" ";
                oRecordSet.DoQuery(s);

                if (oRecordSet.RecordCount == 0)
                {
                    if (((System.String)oRecordSet.Fields.Item("Code").Value) == "1")
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    else
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                }
                else
                {
                    oDBDSHeader.Query(null);
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                }

            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            finally
            {
                oForm.Freeze(false);
            }


            return Result;
        }//fin InitForm


        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            //SAPbouiCOM.DataTable oDataTable;
            //SAPbouiCOM.IChooseFromListEvent oCFLEvento = null;
            Boolean bRes = false;
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);

            try
            {
                if ((pVal.ItemUID == "1") && (pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if (oForm.Mode == BoFormMode.fm_OK_MODE)
                        BubbleEvent = true;
                    else
                    {
                        BubbleEvent = false;

                        if (oForm.Mode == BoFormMode.fm_ADD_MODE)
                        {
                            oDBDSHeader.SetValue("Code", 0, "1");
                            bRes = Funciones.AddDataSource1("VID_mSU", oDBDSHeader, "", null, "", null, "", null, "", null);
                        }
                        else if (oForm.Mode == BoFormMode.fm_UPDATE_MODE)
                            bRes = Funciones.UpdDataSource1("M", ((System.String)oDBDSHeader.GetValue("Code", 0)).Trim(), "VID_mSU", oDBDSHeader, "", null, "", null, "", null, "", null);


                        if (bRes)
                        {
                            oForm.Mode = BoFormMode.fm_OK_MODE;
                            FSBOApp.StatusBar.SetText("Se registraron satisfactoriamente los datos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                        }
                        else
                            FSBOApp.StatusBar.SetText("No se ha registrado los datos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    }
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


    }//fin class
}
