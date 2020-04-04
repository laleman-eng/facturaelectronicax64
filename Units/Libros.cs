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

namespace Factura_Electronica_VK.Libros
{
    class TLibros : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Grid ogrid;
        private SAPbouiCOM.Form oForm;
        //private SAPbouiCOM.EditText oEditText;
        private String s;
        private SAPbouiCOM.DataTable oDataTable;

        public VisualD.SBOFunctions.CSBOFunctions SBO_f;
        public static String TipoLibro
        { get; set; }
        public static String Desde
        { get; set; }
        public static String Hasta
        { get; set; }
        public static Boolean bMultiSoc
        { get; set; }

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            //SAPbouiCOM.ComboBox oComboBox;
            SAPbouiCOM.Column oColumn;

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                //Lista    := New list<string>;

                FSBOf.LoadForm(xmlPath, "VID_FELibros.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All

                oForm.EnableMenu("1281", false);//Actualizar
                oForm.EnableMenu("1282", false);//Crear

                oDataTable = oForm.DataSources.DataTables.Add("dt");
                ogrid = (SAPbouiCOM.Grid)(oForm.Items.Item("grid").Specific);
                ogrid.DataTable = oDataTable;
                if (TipoLibro == "V")
                {
                    oForm.Title = "Libro de Ventas";
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = "select isnull(U_ProcVenta,'') Libro from [@VID_FEPARAM]";
                    else
                        s = @"select IFNULL(""U_ProcVenta"",'') ""Libro"" from ""@VID_FEPARAM"" ";
                }
                else if (TipoLibro == "C")
                {
                    oForm.Title = "Libro de Compras";
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = "select isnull(U_ProcCompra,'') Libro from [@VID_FEPARAM]";
                    else
                        s = @"select IFNULL(""U_ProcCompra"",'') ""Libro"" from ""@VID_FEPARAM"" ";
                }
                else if (TipoLibro == "G")
                {
                    oForm.Title = "Libro de Guias";
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = "select isnull(U_ProcGuia,'') Libro from [@VID_FEPARAM]";
                    else
                        s = @"select IFNULL(""U_ProcGuia"",'') ""Libro"" from ""@VID_FEPARAM"" ";
                }
                else if (TipoLibro == "B")
                {
                    oForm.Title = "Libro de Boletas";
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = "select isnull(U_ProcBol,'') Libro from [@VID_FEPARAM]";
                    else
                        s = @"select IFNULL(""U_ProcBol"",'') ""Libro"" from ""@VID_FEPARAM"" ";
                }

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                {
                    if ((System.String)(oRecordSet.Fields.Item("Libro").Value) != "")
                    {
                        Query((System.String)(oRecordSet.Fields.Item("Libro").Value));
                    }
                    
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

            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    ;
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

        private void Query(String Proc)
        {

            try
            {
                if (GlobalSettings.RunningUnderSQLServer)
                { s = "exec " + Proc + " '" + Desde + "', '" + Hasta + "'"; }
                else
                { s = "CALL " + Proc + " ('" + Desde + "', '" + Hasta + "')"; }

                oDataTable.ExecuteQuery(s);

                if (oDataTable.Rows.Count > 0)
                {
                    ogrid.AutoResizeColumns();
                }

                
            }
            catch (Exception e)
            {
                OutLog("Query: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
        }
    }//fin class
}
