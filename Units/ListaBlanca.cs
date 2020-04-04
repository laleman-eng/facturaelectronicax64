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

namespace Factura_Electronica_VK.ListaBlanca
{
    public class TListaBlanca: TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.DBDataSource oDBDSHeader;
        private SAPbouiCOM.DataTable odt;
        private SAPbouiCOM.Grid ogrid;
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
                //Lista = new List<string>();
                FSBOf.LoadForm(xmlPath, "VID_FEListaBlanca.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All
                oForm.EnableMenu("1282", false); //Crear
                oForm.EnableMenu("1281", false); //Actualizar

                ogrid = ((Grid)oForm.Items.Item("ogrid").Specific);
                oDBDSHeader = ((DBDataSource)oForm.DataSources.DBDataSources.Item("@VID_FELISTABL"));
                ogrid.DataTable = oForm.DataSources.DataTables.Add("dt");

                AddChooseFromList();
                CargarGrid();
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
            String sValue;
            String sValue2;
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);

            try
            {
                if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED)
                {
                    if (pVal.BeforeAction == true)
                    {
                        if (((oForm.Mode == BoFormMode.fm_ADD_MODE) || (oForm.Mode == BoFormMode.fm_UPDATE_MODE)) && (pVal.ItemUID == "1"))
                        {
                            BubbleEvent = false;
                            if (Validar())
                                Guardar_Registros();
                        }
                    }

                    if ((pVal.BeforeAction == false) && (pVal.ItemUID == "btn_Borrar"))
                        BorrarLinea();
                }

                if ((pVal.EventType == BoEventTypes.et_CHOOSE_FROM_LIST) && (!pVal.BeforeAction))
                {
                    oForm.Freeze(true);
                    if (pVal.ColUID == "U_CardCode")
                    {
                        oCFLEvento = (SAPbouiCOM.IChooseFromListEvent)(pVal);
                        oDataTable = oCFLEvento.SelectedObjects;
                        if (oDataTable != null)
                        {
                            sValue = ((System.String)oDataTable.GetValue("CardCode", 0)).Trim();
                            sValue2 = ((System.String)oDataTable.GetValue("CardName", 0)).Trim();
                            for (Int32 iCont_1 = 0; iCont_1 < odt.Rows.Count; iCont_1++)
                            {
                                if (((System.String)odt.GetValue("U_CardCode", iCont_1)).Length > 0)
                                {
                                    var CardCode = ((System.String)odt.GetValue("U_CardCode", iCont_1));
                                    if (CardCode == sValue)
                                    {
                                        FSBOApp.StatusBar.SetText("Proveedor ya se encuentra en la lista blanca", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                        return;
                                    }
                                }
                            }
                            if (GlobalSettings.RunningUnderSQLServer)
                                s = @"SELECT COUNT(*) 'count' FROM [@VID_FELISTANE] WHERE U_CardCode = '{0}'";
                            else
                                s = @"SELECT COUNT(*) ""count"" FROM ""@VID_FELISTANE"" WHERE ""U_CardCode"" = '{0}'";
                            s = String.Format(s, sValue);
                            oRecordSet.DoQuery(s);
                            if (((System.Int32)oRecordSet.Fields.Item("count").Value) > 0)
                            {
                                FSBOApp.StatusBar.SetText("Codigo Proveedor se encuentra en la lista negra, no se puede seleccionar -> Codigo " + sValue, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                return;
                            }

                            odt.SetValue("U_CardCode", pVal.Row, sValue);
                            odt.SetValue("U_CardName", pVal.Row, sValue2);
                            odt.SetValue("U_Activado", pVal.Row, "Y");

                            if ((odt.Rows.Count - 1 == pVal.Row) && (sValue != ""))
                            {
                                odt.Rows.Add(1);

                            }
                            ogrid.AutoResizeColumns();
                            if (oForm.Mode == BoFormMode.fm_OK_MODE)
                                oForm.Mode = BoFormMode.fm_UPDATE_MODE;
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
                if (oForm != null)
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


        private void BorrarLinea()
        {
            Int32 sCode;
            Boolean bPaso;

            try
            {
                bPaso = false;
                ogrid = ((Grid)oForm.Items.Item("ogrid").Specific);
                odt = ogrid.DataTable;
                for (Int32 iCont_1 = 0; iCont_1 < odt.Rows.Count; iCont_1++)
                {
                    if (((System.String)odt.GetValue("U_CardCode", iCont_1)).Length > 0)
                    {
                        if (ogrid.Rows.IsSelected(iCont_1))
                        {
                            sCode = ((System.Int32)odt.GetValue("DocEntry", iCont_1));
                            if (sCode != 0)
                            {

                                if (Funciones.DelDataSource("D", "VID_FELISTABL", "", sCode))
                                {
                                    odt.Rows.Remove(iCont_1);
                                    oForm.Mode = BoFormMode.fm_OK_MODE;
                                    FSBOApp.StatusBar.SetText("Linea eliminada correctamente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                    bPaso = true;
                                    break;
                                }
                            }
                            else
                            {
                                odt.Rows.Remove(iCont_1);
                                FSBOApp.StatusBar.SetText("Linea eliminada correctamente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                bPaso = true;
                            }
                        }
                    }
                }

                if (bPaso == false)
                    FSBOApp.StatusBar.SetText("Debe seleccionar una linea", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
            }
            catch (Exception g)
            {
                FSBOApp.StatusBar.SetText(g.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("BorrarLinea: " + g.Message + " ** Trace: " + g.StackTrace);
            }
        }


        private void CargarGrid()
        {
            try
            {
                ogrid = ((Grid)oForm.Items.Item("ogrid").Specific);
                odt = ogrid.DataTable;
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT DocEntry, U_CardCode, U_CardName, U_Activado FROM [@VID_FELISTABL]";
                else
                    s = @"SELECT ""DocEntry"", ""U_CardCode"", ""U_CardName"", ""U_Activado"" FROM ""@VID_FELISTABL"" ";
                ogrid.DataTable.ExecuteQuery(s);

                ogrid.Columns.Item("DocEntry").Type = BoGridColumnType.gct_EditText;
                var oColumn = ((GridColumn)ogrid.Columns.Item("DocEntry"));
                ((EditTextColumn)oColumn).Editable = false;
                ((EditTextColumn)oColumn).TitleObject.Caption = "DocEntry";
                ((EditTextColumn)oColumn).Visible = false;

                ogrid.Columns.Item("U_CardCode").Type = BoGridColumnType.gct_EditText;
                oColumn = ((GridColumn)ogrid.Columns.Item("U_CardCode"));
                ((EditTextColumn)oColumn).Editable = true;
                ((EditTextColumn)oColumn).TitleObject.Caption = "Codigo SN";
                ((EditTextColumn)oColumn).Visible = true;
                ((EditTextColumn)oColumn).LinkedObjectType = "2";
                ((EditTextColumn)oColumn).ChooseFromListUID = "CFL0";
                ((EditTextColumn)oColumn).ChooseFromListAlias = "CardCode";

                ogrid.Columns.Item("U_CardName").Type = BoGridColumnType.gct_EditText;
                oColumn = ((GridColumn)ogrid.Columns.Item("U_CardName"));
                ((EditTextColumn)oColumn).Editable = false;
                ((EditTextColumn)oColumn).TitleObject.Caption = "Razón Social";
                ((EditTextColumn)oColumn).Visible = true;

                ogrid.Columns.Item("U_Activado").Type = BoGridColumnType.gct_CheckBox;
                var oColumnchx = ((GridColumn)ogrid.Columns.Item("U_Activado"));
                ((CheckBoxColumn)oColumnchx).Editable = true;
                ((CheckBoxColumn)oColumnchx).TitleObject.Caption = "Activo";
                ((CheckBoxColumn)oColumnchx).Visible = true;

                ogrid.AutoResizeColumns();
                if (((System.String)odt.GetValue("U_CardCode", 0)).Trim() != "")
                    odt.Rows.Add();

                FSBOApp.StatusBar.SetText("Lista Blanca cargada", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CargarGrid: " + e.Message + " ** Trace: " + e.StackTrace);
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
            oCFLCreationParams.ObjectType = "2";
            oCFLCreationParams.UniqueID = "CFL0";
            oCFL = oCFLs.Add(oCFLCreationParams);

            //oCFL.SetConditions(null);
            oCons = oCFL.GetConditions();
            oCon = oCons.Add();
            oCon.Alias = "CardType";
            oCon.Operation = BoConditionOperation.co_EQUAL;
            oCon.CondVal = "S";
            oCFL.SetConditions(oCons);
        }

        private Boolean Validar()
        {
            try
            {
                for (Int32 i = 0; i < odt.Rows.Count - 1; i++)
                {
                    if ((((System.String)odt.GetValue("U_CardCode", i)).Trim() == "") && (((System.Int32)odt.GetValue("DocEntry", i)) != 0))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar Codigo Proveedor, linea " + (i + 1).ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText(x.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("Validar: " + x.Message + " ** Trace: " + x.StackTrace);
                return false;
            }
        }


        private void Guardar_Registros()
        {
            String CardCode;
            String CardName;
            String Activado;
            Int32 DocEntry;
            Int32 lRetCode;
            SAPbouiCOM.ProgressBar oProgressBar = null;

            oForm.Freeze(true);
            try
            {
                FSBOApp.StatusBar.SetText("Inicio de actualización lista blanca", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                oProgressBar = FSBOApp.StatusBar.CreateProgressBar("Actualizando lista blanca...", odt.Rows.Count, false);

                for (Int32 iCont_1 = 0; iCont_1 < odt.Rows.Count; iCont_1++)
                {
                    if (((System.String)odt.GetValue("U_CardCode", iCont_1)).Trim().Length > 0)
                    {
                        oDBDSHeader.Clear();
                        oDBDSHeader.InsertRecord(0);

                        DocEntry = ((System.Int32)odt.GetValue("DocEntry", iCont_1));
                        CardCode = ((System.String)odt.GetValue("U_CardCode", iCont_1)).Trim();
                        CardName = ((System.String)odt.GetValue("U_CardName", iCont_1)).Trim();
                        s = ((System.String)odt.GetValue("U_Activado", iCont_1)).Trim();
                        if (s == "")
                            Activado = "N";
                        else
                            Activado = s;
                        oDBDSHeader.SetValue("U_CardCode", 0, CardCode);
                        oDBDSHeader.SetValue("U_CardName", 0, CardName);
                        oDBDSHeader.SetValue("U_Activado", 0, Activado);

                        if (DocEntry != 0)
                        {
                            oDBDSHeader.SetValue("DocEntry", 0, DocEntry.ToString());
                            lRetCode = Funciones.UpdDataSourceInt1("VID_FELISTABL", oDBDSHeader, "", null, "", null, "", null);
                        }
                        else
                            lRetCode = Funciones.AddDataSourceInt1("VID_FELISTABL", oDBDSHeader, "", null, "", null, "", null);

                        if (lRetCode == 0)
                            FSBOApp.StatusBar.SetText("No se ha actualizado proveedor " + CardCode, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        else
                            FSBOApp.StatusBar.SetText("Se ha actualizado proveedor " + CardCode, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    }
                    oProgressBar.Value = iCont_1 + 1;
                }
                oProgressBar.Value = oProgressBar.Maximum;

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                CargarGrid();
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("Guardar_Registros: " + e.Message + " ** Trace: " + e.StackTrace);
            }
            finally
            {
                oForm.Freeze(false);
                oProgressBar.Stop();
                FSBOf._ReleaseCOMObject(oProgressBar);
            }
        }

    }//fin class
}
