using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.OleDb;
using System.Data;
using Factura_Electronica_VK.Functions;
using VisualD.SBOGeneralService;
//using System.Net.Http;
using VisualD.SBOFunctions;
using VisualD.GlobalVid;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using VisualD.untLog;
using SAPbouiCOM;
using SAPbobsCOM;
using System.Globalization;
using Factura_Electronica_VK.CuentaSAP;

namespace Factura_Electronica_VK.PlanCuentaSII
{
    class TPlanCuentaSII : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.DataTable oDataTable;
        private SAPbouiCOM.DBDataSource oDBDSHeader;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.Grid oGrid;
        private SAPbouiCOM.EditText oEditText;
        private TFunctions Funciones = new TFunctions();
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;
        private List<string> Lista;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            Int32 CantRol;

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            Funciones.SBO_f = FSBOf;
            try
            {
                Lista = new List<string>();
                FSBOf.LoadForm(xmlPath, "VID_FEPLANCTA.srf", uid);
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = true;

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT COUNT(*) AS cant FROM [@VID_FEPLANCTA]";
                else
                    s = @"SELECT COUNT(*) ""cant"" FROM ""@VID_FEPLANCTA""";
                oRecordSet.DoQuery(s);
                oForm.SupportedModes = (((System.Int32)oRecordSet.Fields.Item("cant").Value) > 0 ? 1 : 3);
                oForm.Mode = (((System.Int32)oRecordSet.Fields.Item("cant").Value) > 0 ? BoFormMode.fm_OK_MODE : BoFormMode.fm_ADD_MODE);

                oDBDSHeader = oForm.DataSources.DBDataSources.Add("@VID_FEPLANCTA");

                if (((System.Int32)oRecordSet.Fields.Item("cant").Value) == 0)
                {
                    FSBOApp.StatusBar.SetText("Iniciando carga de cuentas SII", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    CargarCtaSII();
                }

                // Ok  Ad  Fnd Vw Rq Sec
                Lista.Add("grid       , t,  t,  f,  t, n, 1 ");
                FSBOf.SetAutoManaged(oForm, Lista);

                oForm.DataSources.UserDataSources.Add("Cuenta", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                oEditText = ((EditText)oForm.Items.Item("Cuenta").Specific);
                oEditText.DataBind.SetBound(true, "", "Cuenta");

                oForm.DataSources.UserDataSources.Add("Descr", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 60);
                oEditText = ((EditText)oForm.Items.Item("Descr").Specific);
                oEditText.DataBind.SetBound(true, "", "Descr");

                oDataTable = oForm.DataSources.DataTables.Add("VID_FEPLANCTA");
                oGrid = ((SAPbouiCOM.Grid)oForm.Items.Item("grid").Specific);
                oGrid.DataTable = oDataTable;
                oGrid.SelectionMode = BoMatrixSelect.ms_Single;

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

                if ((pVal.ItemUID == "btnCtaSAP") && (!pVal.BeforeAction) && (pVal.EventType == BoEventTypes.et_ITEM_PRESSED))
                {
                    var bpaso = false;
                    for (Int32 iCant = 0; iCant <= oGrid.Rows.Count - 1; iCant++)
                    {
                        if (oGrid.Rows.IsSelected(iCant))
                        {
                            bpaso = true;
                            if (((System.Int32)oDataTable.GetValue("DocEntry", iCant)) == 0)
                                FSBOApp.StatusBar.SetText("Debe guardar registro de Plan de Cuentas SII", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            else
                            {
                                CuentaSAP(iCant);
                                break;
                            }
                        }
                    }
                    if (!bpaso)
                        FSBOApp.StatusBar.SetText("Debe seleccionar una linea", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                }

                if ((pVal.ItemUID == "btnCrear") && (!pVal.BeforeAction) && (pVal.EventType == BoEventTypes.et_ITEM_PRESSED))
                {
                    if (!oForm.Items.Item("Cuenta").Visible)
                    {
                        oForm.Items.Item("Cuenta").Visible = true;
                        oForm.Items.Item("Descr").Visible = true;
                    }
                    else
                    {
                        if (CrearNuevaCta())
                        {
                            oForm.Freeze(true);
                            oForm.DataSources.UserDataSources.Item("Cuenta").Value = "";
                            oForm.DataSources.UserDataSources.Item("Descr").Value = "";
                            //((EditText)oForm.Items.Item("Cuenta").Specific).Value = "";
                            //((EditText)oForm.Items.Item("Descr").Specific).Value = "";
                            s = "XXX";
                            oForm.Items.Item("XXX").Click(BoCellClickType.ct_Regular);
                            oForm.Items.Item("Cuenta").Visible = false;
                            oForm.Items.Item("Descr").Visible = false;
                            CargarGrilla();
                        }
                    }
                }

                if ((pVal.ItemUID == "btnBorrar") && (!pVal.BeforeAction) && (pVal.EventType == BoEventTypes.et_ITEM_PRESSED))
                    BorrarCta();


                if ((pVal.ItemUID == "grid") && (pVal.ColUID == "Cuenta") && (pVal.EventType == BoEventTypes.et_VALIDATE) && (!pVal.BeforeAction))
                    ValidarDataTable(((System.String)oDataTable.GetValue("Cuenta", pVal.Row)), pVal.Row);
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
                    s = @"SELECT DocEntry, ISNULL(U_Cuenta,'') Cuenta, ISNULL(U_Desc,'') Descripcion, ISNULL(U_Clasif,'') Clasificacion FROM [@VID_FEPLANCTA]";
                else
                    s = @"SELECT ""DocEntry"", IFNULL(""U_Cuenta"",'') ""Cuenta"", IFNULL(""U_Desc"",'') ""Descripcion"", IFNULL(""U_Clasif"",'') ""Clasificacion"" FROM ""@VID_FEPLANCTA"" ";

                oDataTable.ExecuteQuery(s);

                if (oDataTable.IsEmpty)
                {
                    oForm.Items.Item("btnCtaSAP").Enabled = false;
                    //oForm.Mode = BoFormMode.fm_ADD_MODE;
                }
                else
                {
                    oForm.Mode = BoFormMode.fm_UPDATE_MODE;
                    oDataTable.Rows.Add(1);
                }

                var col = ((EditTextColumn)oGrid.Columns.Item("DocEntry"));
                col.Editable = false;
                col.Visible = false;
                col.TitleObject.Caption = "DocEntry";

                col = ((EditTextColumn)oGrid.Columns.Item("Cuenta"));
                //col.LinkedObjectType = "171"; // Link to Employee
                col.Editable = false;
                col.Visible = true;
                col.TitleObject.Caption = "Código Cuenta";

                col = ((EditTextColumn)oGrid.Columns.Item("Descripcion"));
                col.Editable = true;
                col.Visible = true;
                col.TitleObject.Caption = "Descripción Cuenta";

                oGrid.Columns.Item("Clasificacion").Type = BoGridColumnType.gct_ComboBox;
                var colC = ((ComboBoxColumn)oGrid.Columns.Item("Clasificacion"));
                colC.Editable = true;
                colC.Visible = true;
                colC.DisplayType = BoComboDisplayType.cdt_Description;
                colC.TitleObject.Caption = "Clasificación";

                var Combo = ((ComboBoxColumn)oGrid.Columns.Item("Clasificacion"));
                Combo.ValidValues.Add("1", "Activo");
                Combo.ValidValues.Add("2", "Pasivo");
                Combo.ValidValues.Add("3", "Patrimonio");
                Combo.ValidValues.Add("4", "Pérdidas y Ganancias");
                Combo.ValidValues.Add("5", "Orden");

                oGrid.AutoResizeColumns();
            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText("CargarGrilla: " + x.Message + " ** Trace: " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CargarGrilla: " + x.Message + " ** Trace: " + x.StackTrace);
            }
        }


        private Boolean ValidarDataTable(String Cuenta, Int32 pRow)
        {
            try
            {
                for (Int32 i = 0; i < oDataTable.Rows.Count; i++)
                {
                    if (((System.String)oDataTable.GetValue("Cuenta", i)) != "")
                    {
                        if ((((System.String)oDataTable.GetValue("Cuenta", i)).Trim() == Cuenta) && (pRow != i))
                        {
                            FSBOApp.StatusBar.SetText("Cuenta " + Cuenta + " ya se encuentra ingresada en el formulario", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            oDataTable.SetValue("Cuenta", pRow, "");
                            oDataTable.SetValue("Descripcion", pRow, "");
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
                for (Int32 iCant = 0; iCant <= oDataTable.Rows.Count - 1; iCant++)
                {
                    if (((System.String)oDataTable.GetValue("Cuenta", iCant)).Trim() != "")
                    {
                        if (((System.String)oDataTable.GetValue("Descripcion", iCant)).Trim() == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe Ingresar descripción en linea " + Convert.ToString(iCant + 1), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            return false;
                        }

                    }
                }

                return true;
            }
            catch (Exception r)
            {
                FSBOApp.StatusBar.SetText("ValidarDatos: " + r.Message + " ** Trace: " + r.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("ValidarDatos: " + r.Message + " ** Trace: " + r.StackTrace);
                return false;
            }
        }


        private void GuardarDatos()
        {
            Int32 Entry;
            oForm.Freeze(true);
            try
            {
                for (Int32 iCant = 0; iCant <= oDataTable.Rows.Count - 1; iCant++)
                {
                    if (((System.String)oDataTable.GetValue("Cuenta", iCant)).Trim() != "")
                    {
                        oDBDSHeader.Clear();
                        oDBDSHeader.InsertRecord(0);
                        oDBDSHeader.SetValue("U_Cuenta", 0, ((System.String)oDataTable.GetValue("Cuenta", iCant)).Trim());
                        oDBDSHeader.SetValue("U_Desc", 0, ((System.String)oDataTable.GetValue("Descripcion", iCant)).Trim());
                        oDBDSHeader.SetValue("U_Clasif", 0, ((System.String)oDataTable.GetValue("Clasificacion", iCant)).Trim());


                        if (((System.Int32)oDataTable.GetValue("DocEntry", iCant)) == 0)
                            Entry = Funciones.AddDataSourceInt1("VID_FEPLANCTA", oDBDSHeader, "", null, "", null, "", null);
                        else
                        {
                            oDBDSHeader.SetValue("DocEntry", 0, oDataTable.GetValue("DocEntry", iCant).ToString().Trim());
                            Entry = Funciones.UpdDataSourceInt1("VID_FEPLANCTA", oDBDSHeader, "", null, "", null, "", null);
                        }

                        if (Entry > 0)
                            FSBOApp.StatusBar.SetText("Se ha guardado satisfactoriamente el Plan de Cuenta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);

                        else
                            FSBOApp.StatusBar.SetText("No se ha guardado Plan de Cuenta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    }
                }

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


        private void CuentaSAP(Int32 Linea)
        {
            IvkFormInterface oFormvk = null;
            String oPath = System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0));
            String sArchivo = "";
            String oUid;
            try
            {
                oDBDSHeader.Clear();
                oDBDSHeader.InsertRecord(0);
                oDBDSHeader.SetValue("DocEntry", 0, oDataTable.GetValue("DocEntry", Linea).ToString().Trim());
                oDBDSHeader.SetValue("U_Cuenta", 0, ((System.String)oDataTable.GetValue("Cuenta", Linea)).Trim());
                oDBDSHeader.SetValue("U_Desc", 0, ((System.String)oDataTable.GetValue("Descripcion", Linea)).Trim());
                oDBDSHeader.SetValue("U_Clasif", 0, ((System.String)oDataTable.GetValue("Clasificacion", Linea)).Trim());

                oFormvk = (IvkFormInterface)(new TCuentaSAP());
                TCuentaSAP.DocEntry = ((System.Int32)oDataTable.GetValue("DocEntry", Linea));
                TCuentaSAP.oDBDSHeader = oDBDSHeader;

                oUid = FSBOf.generateFormId(FGlobalSettings.SBOSpaceName, FGlobalSettings);
                oFormvk.InitForm(oUid, "forms\\", ref FSBOApp, ref FCmpny, ref FSBOf, ref FGlobalSettings);
                FoForms.Add(oFormvk);
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("Error Abrri Sueldos: " + e.Message + " ** Trace: " + e.StackTrace);
            }

        }


        private void CargarCtaSII()
        {
            SAPbouiCOM.ProgressBar oProgressBar = null;
            Int32 NroLineas;
            System.Data.DataTable dtConceptos;
            Boolean bRegistroOk;
            String sHojaExcel;
            String sCuenta;
            String sRegistro;
            String Padre;
            //String Hijo;
            String sPeriodoActual;
            String strConn;
            Boolean bIngresaConceptos = false;
            DataSet ADOQueryExcel;
            Int32 Entry;
            OleDbDataAdapter adapter;
            String sPathArchivo = "";

            try
            {
                sPathArchivo = Directory.GetCurrentDirectory() + "\\SQLs\\CL\\Carga\\DiccionarioSII.xls";
                sHojaExcel = "[CTASII$]";
                strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=Excel 8.0;Mode=ReadWrite";
                strConn = String.Format(strConn, sPathArchivo);

                adapter = new OleDbDataAdapter("Select * from [CTASII$]", strConn);
                ADOQueryExcel = new DataSet();
                try
                {
                    adapter.Fill(ADOQueryExcel, "CTASII");
                    dtConceptos = new System.Data.DataTable();
                    dtConceptos = ADOQueryExcel.Tables["CTASII"];
                }
                catch //(Exception y)
                {
                    FSBOApp.StatusBar.SetText("Error leyendo archivo excel...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return;
                }

                if (dtConceptos.Rows.Count == 0)
                {
                    FSBOApp.StatusBar.SetText("No existen Cuentas SII para importar...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return;
                }

                oProgressBar = FSBOApp.StatusBar.CreateProgressBar("Importando Cuentas SII...", dtConceptos.Rows.Count + 2, false);
                NroLineas = 1;
                foreach (DataRow oRow in dtConceptos.Rows)
                {
                    bIngresaConceptos = false;
                    sCuenta = oRow.Field<String>("Cuenta");
                    if (sCuenta != null)
                    {
                        sRegistro = Convert.ToString(oRow.Field<Double>("Registro"));

                        if (Convert.ToString(oRow.Field<String>("Glosa")).Trim() == "")
                        {
                            s = @"UPDATE {0} SET validacion = 'Debe ingresar Glosa' where [Registro] = {1}";
                            s = String.Format(s, sHojaExcel, sRegistro);
                            Funciones.EjecutarSQLOleDb(s, strConn);
                        }
                        else
                            bIngresaConceptos = true;


                        if (bIngresaConceptos)
                        {
                            oDBDSHeader.Clear();

                            oDBDSHeader.InsertRecord(0);
                            oDBDSHeader.SetValue("U_Cuenta", 0, Convert.ToString(oRow.Field<String>("Cuenta")).Trim());
                            oDBDSHeader.SetValue("U_Desc", 0, Convert.ToString(oRow.Field<String>("Glosa")).Trim());
                            oDBDSHeader.SetValue("U_Clasif", 0, Convert.ToString(oRow.Field<String>("Clasificacion")).Trim());

                            Entry = Funciones.AddDataSourceInt1("VID_FEPLANCTA", oDBDSHeader, "", null, "", null, "", null);


                            if (Entry > 0)
                            {
                                var ss = FCmpny.GetNewObjectKey();
                                s = @"UPDATE {0} SET validacion = 'OK' WHERE [Registro] = {1}";
                                s = String.Format(s, sHojaExcel, sRegistro);
                            }
                            else
                            {
                                var err = FCmpny.GetLastErrorDescription();
                                s = @"UPDATE {0} SET validacion = 'Error al crear {1}' WHERE [Registro] = {2}";
                                s = String.Format(s, sHojaExcel, err, sRegistro);
                            }
                            Funciones.EjecutarSQLOleDb(s, strConn);

                        }
                    }
                    else
                    {
                        oProgressBar.Value = oProgressBar.Maximum;
                        break;
                    }

                    oProgressBar.Value = NroLineas;
                    NroLineas = NroLineas + 1;
                }

                FSBOApp.StatusBar.SetText("Fin de la importación", BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Success);
                FSBOApp.MessageBox("Fin de la importación", 1, "Ok", "", "");
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("Error importando Conceptos desde: " + sPathArchivo + ". Error: " + e.Message + " ** Trace: " + e.StackTrace);
            }
            finally
            {
                if (oProgressBar != null)
                    oProgressBar.Stop();
                FSBOf._ReleaseCOMObject(oProgressBar);
            }
        }

        private Boolean CrearNuevaCta()
        {
            String Cuenta;
            String Glosa;
            try
            {
                Cuenta = ((System.String)((EditText)oForm.Items.Item("Cuenta").Specific).Value).Trim();
                Glosa = ((System.String)((EditText)oForm.Items.Item("Descr").Specific).Value).Trim();

                if (Cuenta == "")
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar Cuenta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return false;
                }
                else if (Glosa == "")
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar descripción cuenta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return false;
                }
                else
                {
                    for (Int32 x = 0; x < oDataTable.Rows.Count; x++)
                    {
                        if (((System.String)oDataTable.GetValue("Cuenta", x)).Trim() == Cuenta)
                        {
                            FSBOApp.StatusBar.SetText("Cuenta " + Cuenta + " ya existe", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    }
                }

                oDBDSHeader.Clear();
                oDBDSHeader.InsertRecord(0);
                oDBDSHeader.SetValue("U_Cuenta", 0, Cuenta);
                oDBDSHeader.SetValue("U_Desc", 0, Glosa);

                var Entry = Funciones.AddDataSourceInt1("VID_FEPLANCTA", oDBDSHeader, "", null, "", null, "", null);

                if (Entry > 0)
                    FSBOApp.StatusBar.SetText("Se ha ingresado satisfactoriamente la cuenta " + Cuenta, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                else
                {
                    FSBOApp.StatusBar.SetText("No se ha registrado la cuenta " + Cuenta, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return false;
                }

                return true;
            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText(x.Message + " ** Trace: " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CrearNuevaCta: " + x.Message + " ** Trace: " + x.StackTrace);
                return false;
            }
        }

        private void BorrarCta()
        {
            Boolean Paso = false;
            Int32 DocEntry = 0;

            try
            {

                for (Int32 i = 0; i <= oDataTable.Rows.Count - 1; i++)
                {
                    if (oGrid.Rows.IsSelected(i))
                    {
                        Paso = true;
                        DocEntry = ((System.Int32)oDataTable.GetValue("DocEntry", i));
                        break;
                    }
                }

                if (!Paso)
                    FSBOApp.StatusBar.SetText("Debe seleccionar una linea", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                else
                {
                    if (FSBOApp.MessageBox("¿Esta seguro que desea borrar cuenta?", 1, "Si", "No", "") == 1)
                    {
                        if (Funciones.DelDataSource("D", "VID_FEPLANCTA", "", DocEntry))
                        {
                            FSBOApp.StatusBar.SetText("Cuenta eliminada", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                            CargarGrilla();
                        }
                        else
                            FSBOApp.StatusBar.SetText("No fue eliminada la cuenta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    }
                }

            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText(x.Message + " ** Trace: " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("BorrarCta: " + x.Message + " ** Trace: " + x.StackTrace);
            }
        }
    }
}
