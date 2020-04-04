using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Configuration;
using System.Data.OleDb;
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

namespace Factura_Electronica_VK.CuentaSAP
{
    public class TCuentaSAP : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.DBDataSource oDBDSDetalle;
        private SAPbouiCOM.DataTable odt;
        private SAPbouiCOM.Grid oGrid;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.ComboBox oComboBox;
        private TFunctions Funciones = new TFunctions();
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;

        public static Int32 DocEntry
        { get; set; }
        public static SAPbouiCOM.DBDataSource oDBDSHeader
        { get; set; }


        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            Int32 CantRol;

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            Funciones.SBO_f = FSBOf;
            try
            {
                //Lista = new List<string>();
                FSBOf.LoadForm(xmlPath, "VID_FECTASAP.srf", uid);
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = true;

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT COUNT(*) AS cant FROM [@VID_FEPLANCTA] WHERE DocEntry = {0}";
                else
                    s = @"SELECT COUNT(*) ""cant"" FROM ""@VID_FEPLANCTA"" WHERE ""DocEntry"" = {0} ";
                s = String.Format(s, DocEntry);
                oRecordSet.DoQuery(s);
                oForm.SupportedModes = (((System.Int32)oRecordSet.Fields.Item("cant").Value) > 0 ? 1 : 3);
                oForm.Mode = (((System.Int32)oRecordSet.Fields.Item("cant").Value) > 0 ? BoFormMode.fm_OK_MODE : BoFormMode.fm_ADD_MODE);

                oDBDSDetalle = oForm.DataSources.DBDataSources.Add("@VID_FEPLANCTAD");

                odt = oForm.DataSources.DataTables.Add("Cuentas");
                oGrid = ((SAPbouiCOM.Grid)oForm.Items.Item("grid").Specific);
                oGrid.DataTable = odt;
                oGrid.SelectionMode = BoMatrixSelect.ms_Single;

                AddChooseFromList();
                CargarGrilla();

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
            SAPbouiCOM.DataTable oDataTable;
            SAPbouiCOM.IChooseFromListEvent oCFLEvento = null;
            String sValue;
            String sValue2;

            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);

            try
            {
                if ((pVal.ItemUID == "1") && (pVal.BeforeAction) && (pVal.EventType == BoEventTypes.et_ITEM_PRESSED))
                {
                    if ((oForm.Mode == BoFormMode.fm_ADD_MODE) || (oForm.Mode == BoFormMode.fm_UPDATE_MODE))
                    {
                        BubbleEvent = false;
                        if (ValidarDatos())
                            GuardarDatos();
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_CHOOSE_FROM_LIST) && (!pVal.BeforeAction))
                {
                    oForm.Freeze(true);
                    if (pVal.ColUID == "U_CtaSAP")
                    {
                        oCFLEvento = (SAPbouiCOM.IChooseFromListEvent)(pVal);
                        oDataTable = oCFLEvento.SelectedObjects;
                        if (oDataTable != null)
                        {
                            sValue = ((System.String)oDataTable.GetValue("FormatCode", 0)).Trim();
                            sValue2 = ((System.String)oDataTable.GetValue("AcctName", 0)).Trim();

                            if (ValidarDataTable(sValue))
                            {
                                odt.SetValue("U_CtaSAP", pVal.Row, sValue);
                                odt.SetValue("U_DescSAP", pVal.Row, sValue2);

                                if ((odt.Rows.Count - 1 == pVal.Row) && (sValue != ""))
                                {
                                    odt.Rows.Add(1);

                                }
                                oGrid.AutoResizeColumns();
                                if (oForm.Mode == BoFormMode.fm_OK_MODE)
                                    oForm.Mode = BoFormMode.fm_UPDATE_MODE;
                            }
                        }
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
            finally
            {
                oForm.Freeze(false);
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


        private void CargarGrilla()
        {
            try
            {
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT DocEntry
                              ,LineId
	                          ,U_CtaSAP
	                          ,U_DescSAP
                          FROM [@VID_FEPLANCTAD]
                         WHERE DocEntry = {0}";
                else
                    s = @"SELECT ""DocEntry""
                              ,""LineId""
	                          ,""U_CtaSAP""
	                          ,""U_DescSAP""
                          FROM ""@VID_FEPLANCTAD""
                         WHERE ""DocEntry"" = {0}";
                s = String.Format(s, DocEntry);
                odt.ExecuteQuery(s);

                if (!odt.IsEmpty)
                {
                    oForm.Mode = BoFormMode.fm_UPDATE_MODE;
                    odt.Rows.Add(1);
                }

                var col = ((EditTextColumn)oGrid.Columns.Item("DocEntry"));
                col.Editable = false;
                col.Visible = false;
                col.TitleObject.Caption = "DocEntry";

                col = ((EditTextColumn)oGrid.Columns.Item("LineId"));
                col.Editable = false;
                col.Visible = false;
                col.TitleObject.Caption = "LineId";

                col = ((EditTextColumn)oGrid.Columns.Item("U_CtaSAP"));
                col.Editable = true;
                col.Visible = true;
                col.TitleObject.Caption = "Cuenta SAP";
                col.ChooseFromListUID = "CFL0";
                col.ChooseFromListAlias = "FormatCode";
                col.LinkedObjectType = "1";

                col = ((EditTextColumn)oGrid.Columns.Item("U_DescSAP"));
                col.Editable = true;
                col.Visible = true;
                col.TitleObject.Caption = "Descripción Cuenta";

                oGrid.AutoResizeColumns();
            }
            catch (Exception t)
            {
                FSBOApp.StatusBar.SetText("CargarGrilla: " + t.Message + " ** Trace: " + t.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CargarGrilla: " + t.Message + " ** Trace: " + t.StackTrace);
            }
        }


        private Boolean ValidarDataTable(String Cuenta)
        {
            try
            {
                for (Int32 i = 0; i < odt.Rows.Count; i++)
                {
                    if (((System.String)odt.GetValue("U_CtaSAP", i)) != "")
                    {
                        if (((System.String)odt.GetValue("U_CtaSAP", i)).Trim() == Cuenta)
                        {
                            FSBOApp.StatusBar.SetText("Cuenta " + Cuenta + " ya se encuentra ingresada en el formulario", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception a)
            {
                FSBOApp.StatusBar.SetText("ValidarDataTable: " + a.Message + " ** Trace: " + a.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("ValidarDataTable: " + a.Message + " ** Trace: " + a.StackTrace);
                return false;
            }
        }


        private Boolean ValidarDatos()
        {
            try
            {
                for (Int32 i = 0; i < odt.Rows.Count; i++)
                {
                    if (((System.String)odt.GetValue("U_CtaSAP", i)) != "")
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT COUNT(*) Cant FROM [@VID_FEPLANCTAD] WHERE U_CtaSAP = '{0}' AND DocEntry <> {1}";
                        else
                            s = @"SELECT COUNT(*) ""Cant"" FROM ""@VID_FEPLANCTAD"" WHERE ""U_CtaSAP"" = '{0}' AND ""DocEntry"" <> {1}";
                        s = String.Format(s, ((System.String)odt.GetValue("U_CtaSAP", i)).Trim(), DocEntry);
                        oRecordSet.DoQuery(s);
                        if (((System.Int32)oRecordSet.Fields.Item("Cant").Value) >= 1)
                        {
                            FSBOApp.StatusBar.SetText("Cuenta " + ((System.String)odt.GetValue("U_CtaSAP", i)).Trim() + " ya se encuentra ingresada en una Cuenta SII", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception t)
            {
                FSBOApp.StatusBar.SetText("ValidarDatos: " + t.Message + " ** Trace: " + t.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("ValidarDatos: " + t.Message + " ** Trace: " + t.StackTrace);
                return false;
            }
        }


        private void GuardarDatos()
        {
            Int32 Entry = 0;
            Int32 LineId = 0;
            oForm.Freeze(true);
            try
            {
                oDBDSDetalle.Clear();
                for (Int32 iCant = 0; iCant <= odt.Rows.Count - 1; iCant++)
                {
                    if (((System.String)odt.GetValue("U_CtaSAP", iCant)) != "")
                    {
                        oDBDSDetalle.InsertRecord(LineId);
                        oDBDSDetalle.SetValue("DocEntry", LineId, odt.GetValue("DocEntry", iCant).ToString().Trim());
                        oDBDSDetalle.SetValue("LineId", LineId, LineId.ToString());
                        oDBDSDetalle.SetValue("U_CtaSAP", LineId, ((System.String)odt.GetValue("U_CtaSAP", iCant)).ToString().Trim());
                        oDBDSDetalle.SetValue("U_DescSAP", LineId, ((System.String)odt.GetValue("U_DescSAP", iCant)).ToString().Trim());
                    }
                    LineId++;
                }
                Entry = Funciones.UpdDataSourceInt1("VID_FEPLANCTA", oDBDSHeader, "VID_FEPLANCTAD", oDBDSDetalle, "", null, "", null);

                if (Entry > 0)
                    FSBOApp.StatusBar.SetText("Se ha guardado satisfactoriamente el Plan de Cuentas", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                else
                    FSBOApp.StatusBar.SetText("No se ha guardado Plan de Cuentas", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);

                CargarGrilla();
                oForm.Mode = BoFormMode.fm_OK_MODE;

            }
            catch (Exception y)
            {
                FSBOApp.StatusBar.SetText("GuardarDatos: " + y.Message + " ** Trace: " + y.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("GuardarDatos: " + y.Message + " ** Trace: " + y.StackTrace);
            }
            finally
            {
                oForm.Freeze(false);
            }
        }

        private void AddChooseFromList()
        {
            SAPbouiCOM.ChooseFromListCollection oCFLs;
            SAPbouiCOM.ChooseFromList oCFL;
            SAPbouiCOM.ChooseFromListCreationParams oCFLCreationParams;
            SAPbouiCOM.Conditions oCons;
            SAPbouiCOM.Condition oCon;

            oCFLs = oForm.ChooseFromLists;
            oCFLCreationParams = (ChooseFromListCreationParams)(FSBOApp.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_ChooseFromListCreationParams));

            oCFLCreationParams.MultiSelection = false;
            oCFLCreationParams.ObjectType = "1";
            oCFLCreationParams.UniqueID = "CFL0";
            oCFL = oCFLs.Add(oCFLCreationParams);

            //oCFL.SetConditions(null);
            oCons = oCFL.GetConditions();
            oCon = oCons.Add();
            oCon.Alias = "Postable";
            oCon.Operation = BoConditionOperation.co_EQUAL;
            oCon.CondVal = "Y";
            oCFL.SetConditions(oCons);
        }

        

    }//fin class
}
