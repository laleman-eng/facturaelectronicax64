using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Configuration;
using System.Xml;
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

namespace Factura_Electronica_VK.LibrosElectronicos
{
    public class TLibrosElectronicos : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.DataTable odt;
        private SAPbouiCOM.DataTable odt2;
        private SAPbouiCOM.Grid grid;
        private SAPbouiCOM.Grid grid2;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.ComboBox oComboBox;
        private TFunctions Funciones = new TFunctions();
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;

        public static String TipoLibro
        { get; set; }

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            Int32 CantRol;

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            Funciones.SBO_f = FSBOf;
            try
            {
                Lista = new List<string>();

                FSBOf.LoadForm(xmlPath, "VID_FELibrosElect.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;

                oForm.DataSources.UserDataSources.Add("Periodo", BoDataType.dt_SHORT_TEXT, 10);
                ((ComboBox)oForm.Items.Item("Periodo").Specific).DataBind.SetBound(true, "", "Periodo");

                oForm.DataSources.UserDataSources.Add("PeriodoD", BoDataType.dt_SHORT_TEXT, 10);
                ((ComboBox)oForm.Items.Item("PeriodoD").Specific).DataBind.SetBound(true, "", "PeriodoD");

                oForm.DataSources.UserDataSources.Add("PeriodoH", BoDataType.dt_SHORT_TEXT, 10);
                ((ComboBox)oForm.Items.Item("PeriodoH").Specific).DataBind.SetBound(true, "", "PeriodoH");

                odt = oForm.DataSources.DataTables.Add("grid");
                grid = ((Grid)oForm.Items.Item("grid").Specific);
                grid.DataTable = odt;


                if ((TipoLibro == "D") || (TipoLibro == "M")) //Libro Mayor y Diario
                {
                    s = "Periodo";
                    oForm.Items.Item(s).Click(BoCellClickType.ct_Regular);
                    if (TipoLibro == "D")
                        oForm.Title = "Libro Diario y Mayor";

                    oForm.PaneLevel = 1;
                    oForm.Items.Item("5").Visible = false;
                    oForm.Items.Item("PeriodoD").Visible = false;
                    oForm.Items.Item("7").Visible = false;
                    oForm.Items.Item("PeriodoH").Visible = false;

                    oForm.Items.Item("grid").Height = 210;
                    oForm.Items.Item("grid2").Visible = true;

                    odt2 = oForm.DataSources.DataTables.Add("grid2");
                    grid2 = ((Grid)oForm.Items.Item("grid2").Specific);
                    grid2.DataTable = odt2;

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT CAST(AbsEntry AS VARCHAR(20)) 'Code', Code 'Name'
                              FROM OFPR
                             WHERE Category BETWEEN YEAR(GETDATE()) -1 AND YEAR(GETDATE())";
                    else
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT TO_VARCHAR(""AbsEntry"") ""Code"", ""Code"" ""Name""
                              FROM ""OFPR""
                             WHERE ""Category"" BETWEEN EXTRACT(YEAR FROM CURRENT_DATE) -1 AND EXTRACT(YEAR FROM CURRENT_DATE)";
                    oRecordSet.DoQuery(s);

                    oComboBox = ((ComboBox)oForm.Items.Item("Periodo").Specific);
                    FSBOf.FillCombo(oComboBox, ref oRecordSet, false);

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT CAST(AbsEntry AS VARCHAR(20)) 'Code' FROM OFPR WHERE CONVERT(CHAR(6), F_RefDate, 112) = CONVERT(CHAR(6), GETDATE(), 112)";
                    else
                        s = @"SELECT TO_VARCHAR(""AbsEntry"") ""Code"" FROM ""OFPR"" WHERE TO_VARCHAR(""F_RefDate"", 'yyyyMM') = TO_VARCHAR(CURRENT_DATE, 'yyyyMM')";
                    oRecordSet.DoQuery(s);
                    oComboBox.Select(((System.String)oRecordSet.Fields.Item("Code").Value), BoSearchKey.psk_ByValue);
                }
                else if (TipoLibro == "B")//Balance
                {
                    s = "PeriodoD";
                    oForm.Items.Item(s).Click(BoCellClickType.ct_Regular);
                    oForm.Title = "Balance";
                    oForm.PaneLevel = 1;
                    oForm.Items.Item("4").Visible = false;
                    oForm.Items.Item("Periodo").Visible = false;

                    oForm.Items.Item("grid").Height = 210;
                    oForm.Items.Item("grid2").Visible = true;

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT CAST(AbsEntry AS VARCHAR(20)) 'Code', Code 'Name'
                              FROM OFPR
                             WHERE Category BETWEEN YEAR(GETDATE()) -1 AND YEAR(GETDATE())";
                    else
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT TO_VARCHAR(""AbsEntry"") ""Code"", ""Code"" ""Name""
                              FROM ""OFPR""
                             WHERE ""Category"" BETWEEN EXTRACT(YEAR FROM CURRENT_DATE) -1 AND EXTRACT(YEAR FROM CURRENT_DATE)";
                    oRecordSet.DoQuery(s);
                    oForm.Items.Item("PeriodoD").DisplayDesc = true;
                    oComboBox = ((ComboBox)oForm.Items.Item("PeriodoD").Specific);
                    FSBOf.FillCombo(oComboBox, ref oRecordSet, false);

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT CAST(AbsEntry AS VARCHAR(20)) 'Code' FROM OFPR WHERE CONVERT(CHAR(6), F_RefDate, 112) = CONVERT(CHAR(6), GETDATE(), 112)";
                    else
                        s = @"SELECT TO_VARCHAR(""AbsEntry"") ""Code"" FROM ""OFPR"" WHERE TO_VARCHAR(""F_RefDate"", 'yyyyMM') = TO_VARCHAR(CURRENT_DATE, 'yyyyMM')";
                    oRecordSet.DoQuery(s);
                    oComboBox.Select(((System.String)oRecordSet.Fields.Item("Code").Value), BoSearchKey.psk_ByValue);

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT CAST(AbsEntry AS VARCHAR(20)) 'Code', Code 'Name'
                              FROM OFPR
                             WHERE Category BETWEEN YEAR(GETDATE()) -1 AND YEAR(GETDATE())";
                    else
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT TO_VARCHAR(""AbsEntry"") ""Code"", ""Code"" ""Name""
                              FROM ""OFPR""
                             WHERE ""Category"" BETWEEN EXTRACT(YEAR FROM CURRENT_DATE) -1 AND EXTRACT(YEAR FROM CURRENT_DATE)";
                    oRecordSet.DoQuery(s);
                    oForm.Items.Item("PeriodoH").DisplayDesc = true;
                    oComboBox = ((ComboBox)oForm.Items.Item("PeriodoH").Specific);                   
                    FSBOf.FillCombo(oComboBox, ref oRecordSet, false);

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT CAST(AbsEntry AS VARCHAR(20)) 'Code' FROM OFPR WHERE CONVERT(CHAR(6), F_RefDate, 112) = CONVERT(CHAR(6), GETDATE(), 112)";
                    else
                        s = @"SELECT TO_VARCHAR(""AbsEntry"") ""Code"" FROM ""OFPR"" WHERE TO_VARCHAR(""F_RefDate"", 'yyyyMM') = TO_VARCHAR(CURRENT_DATE, 'yyyyMM')";
                    oRecordSet.DoQuery(s);
                    oComboBox.Select(((System.String)oRecordSet.Fields.Item("Code").Value), BoSearchKey.psk_ByValue);

                    odt2 = oForm.DataSources.DataTables.Add("grid2");
                    grid2 = ((Grid)oForm.Items.Item("grid2").Specific);
                    grid2.DataTable = odt2;
                }
                else if (TipoLibro == "C") //para Diccionario de cuentas
                {
                    oForm.Title = "Diccionario";
                    //oForm.PaneLevel = 2;
                    oForm.Items.Item("btnExp").Enabled = true;
                    oForm.PaneLevel = 1;
                    oForm.Items.Item("5").Visible = false;
                    oForm.Items.Item("PeriodoD").Visible = false;
                    oForm.Items.Item("7").Visible = false;
                    oForm.Items.Item("PeriodoH").Visible = false;

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT CAST(AbsEntry AS VARCHAR(20)) 'Code', Code 'Name'
                              FROM OFPR
                             WHERE Category BETWEEN YEAR(GETDATE()) -1 AND YEAR(GETDATE())";
                    else
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT TO_VARCHAR(""AbsEntry"") ""Code"", ""Code"" ""Name""
                              FROM ""OFPR""
                             WHERE ""Category"" BETWEEN EXTRACT(YEAR FROM CURRENT_DATE) -1 AND EXTRACT(YEAR FROM CURRENT_DATE)";
                    oRecordSet.DoQuery(s);

                    oComboBox = ((ComboBox)oForm.Items.Item("Periodo").Specific);
                    FSBOf.FillCombo(oComboBox, ref oRecordSet, false);

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT CAST(AbsEntry AS VARCHAR(20)) 'Code' FROM OFPR WHERE CONVERT(CHAR(6), F_RefDate, 112) = CONVERT(CHAR(6), GETDATE(), 112)";
                    else
                        s = @"SELECT TO_VARCHAR(""AbsEntry"") ""Code"" FROM ""OFPR"" WHERE TO_VARCHAR(""F_RefDate"", 'yyyyMM') = TO_VARCHAR(CURRENT_DATE, 'yyyyMM')";
                    oRecordSet.DoQuery(s);
                    oComboBox.Select(((System.String)oRecordSet.Fields.Item("Code").Value), BoSearchKey.psk_ByValue);
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
            SAPbouiCOM.DataTable oDataTable;
            SAPbouiCOM.IChooseFromListEvent oCFLEvento = null;
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);

            try
            {
                if ((pVal.ItemUID == "btnBuscar") && (pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction))
                    if (Validar())
                        CargarGrid();

                if ((pVal.ItemUID == "btnExp") && (pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction))
                    Exportar();
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
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent

        private Boolean Validar()
        {
            String PeriodoD = "";
            String PeriodoH = "";
            String Periodo = "";
            try
            {
                if (oForm.Title == "Balance")
                {
                    PeriodoD = ((System.String)((ComboBox)oForm.Items.Item("PeriodoD").Specific).Selected.Value).Trim();
                    PeriodoH = ((System.String)((ComboBox)oForm.Items.Item("PeriodoH").Specific).Selected.Value).Trim();

                    if (PeriodoD == "")
                    {
                        FSBOApp.StatusBar.SetText("Debe seleccionar Periodo Desde", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                    else if (PeriodoH == "")
                    {
                        FSBOApp.StatusBar.SetText("Debe seleccionar Periodo Hasta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                    else if (Convert.ToInt32(PeriodoD) > Convert.ToInt32(PeriodoH))
                    {
                        FSBOApp.StatusBar.SetText("Debe seleccionar intervalo de Periodos correctos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }
                else
                {
                        Periodo = ((System.String)((ComboBox)oForm.Items.Item("Periodo").Specific).Selected.Value).Trim();
                        if (Periodo == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe seleccionar Periodo", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    
                }

                return true;
            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText(x.Message + " ** Trace: " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("Validar: " + x.Message + " ** Trace: " + x.StackTrace);
                return false;
            }
        }

        private void CargarGrid()
        {
            String Periodo = "";
            String PeriodoD = "";
            String PeriodoH = "";

            try
            {
                if (oForm.Title == "Libro Diario y Mayor")
                {
                    Periodo = ((System.String)((ComboBox)oForm.Items.Item("Periodo").Specific).Selected.Value).Trim();
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"EXEC VID_SP_FE_LibroDiario '{0}'";
                    else
                        s = @"CALL VID_SP_FE_LibroDiario ('{0}')";
                    s = String.Format(s, Periodo);
                    odt.ExecuteQuery(s);

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"EXEC VID_SP_FE_LibroMayor '{0}'";
                    else
                        s = @"CALL VID_SP_FE_LibroMayor ('{0}')";
                    s = String.Format(s, Periodo);
                    odt2.ExecuteQuery(s);
                }
                else if (oForm.Title == "Balance")
                {
                    PeriodoD = ((System.String)((ComboBox)oForm.Items.Item("PeriodoD").Specific).Selected.Value).Trim();
                    PeriodoH = ((System.String)((ComboBox)oForm.Items.Item("PeriodoH").Specific).Selected.Value).Trim();
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"EXEC VID_SP_FE_Balance '{0}', '{1}'";
                    else
                        s = @"CALL VID_SP_FE_Balance ('{0}', '{1}')";
                    s = String.Format(s, PeriodoD, PeriodoH);
                    odt.ExecuteQuery(s);

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"EXEC VID_SP_FE_Diccionario '{0}' ";
                    else
                        s = @"CALL VID_SP_FE_Diccionario ('{0}')";
                    s = String.Format(s, PeriodoD);
                    odt2.ExecuteQuery(s);
                }
                else if (oForm.Title == "Diccionario")
                {
                    Periodo = ((System.String)((ComboBox)oForm.Items.Item("Periodo").Specific).Selected.Value).Trim();
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"EXEC VID_SP_FE_Diccionario '{0}' ";
                    else
                        s = @"CALL VID_SP_FE_Diccionario ('{0}')";
                    s = String.Format(s, Periodo);
                    odt.ExecuteQuery(s);
                }


                if (!odt.IsEmpty)
                    oForm.Items.Item("btnExp").Enabled = true;
                else
                    oForm.Items.Item("btnExp").Enabled = false;


                for (Int32 z = 0; z < grid.Columns.Count; z++)
                    grid.Columns.Item(z).Editable = false;

                grid.AutoResizeColumns();

                if ((oForm.Title == "Libro Diario y Mayor") || (oForm.Title == "Balance"))
                {
                    if (!odt2.IsEmpty)
                    {
                        for (Int32 z = 0; z < grid2.Columns.Count; z++)
                            grid2.Columns.Item(z).Editable = false;

                        grid2.AutoResizeColumns();
                    }
                }

            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText(x.Message + " ** Trace: " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CargarGrid: " + x.Message + " ** Trace: " + x.StackTrace);
            }
        }

        private void Exportar()
        {
            String path;
            String xmlString = "";
            String Periodo = "";
            String PeriodoD = "";
            String PeriodoH = "";
            XmlDocument oXml = null;
            System.Object[] array1;

            try
            {
                if (odt.IsEmpty)
                    FSBOApp.StatusBar.SetText("No se encontrado datos para exportar", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                else
                {
                    if (!Directory.Exists("C:\\Libros Contable Elect"))
                        Directory.CreateDirectory("C:\\Libros Contable Elect");
                    //path = "C:\\Libros Contable Elect\\DICC" + Periodo.Replace("-","") + ".xml";

                    if (oForm.Title == "Libro Diario y Mayor")
                        Periodo = ((System.String)((ComboBox)oForm.Items.Item("Periodo").Specific).Selected.Description).Trim();    
                    else if (oForm.Title == "Balance")
                    {
                        PeriodoD = ((System.String)((ComboBox)oForm.Items.Item("PeriodoD").Specific).Selected.Description).Trim();
                        PeriodoH = ((System.String)((ComboBox)oForm.Items.Item("PeriodoH").Specific).Selected.Description).Trim();
                    }
                    else if (oForm.Title == "Diccionario")
                        Periodo = ((System.String)((ComboBox)oForm.Items.Item("Periodo").Specific).Selected.Description).Trim();


                    if (oForm.Title == "Diccionario")
                    {
                        //xmlString = CrearXMLDiccionario(Periodo);
                        array1 = new System.Object[3];
                        array1[0] = Periodo;
                        array1[1] = odt;
                        array1[2] = FSBOApp;
                        var miExtensionAssembly = System.Reflection.Assembly.LoadFile(System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\LibrosElectronicos.dll");
                        var miExtensionType = miExtensionAssembly.GetType("LibrosElectronicosXml.LEXml");
                        var miExtensionObjeto = Activator.CreateInstance(miExtensionType);
                        xmlString = ((System.String)miExtensionObjeto.GetType().InvokeMember("CrearXMLDiccionario", System.Reflection.BindingFlags.InvokeMethod, System.Type.DefaultBinder, miExtensionObjeto, array1));
                        FSBOf._ReleaseCOMObject(miExtensionAssembly);
                        FSBOf._ReleaseCOMObject(miExtensionType);
                        FSBOf._ReleaseCOMObject(miExtensionObjeto);
                    }
                    else if (oForm.Title == "Libro Diario y Mayor")
                    {
                        //xmlString = LibrosElectronicosXml.LEXml.CrearXMLLibroDiarioMayor(Periodo, ref odt, ref odt2, ref FSBOApp);
                        array1 = new System.Object[4];
                        array1[0] = Periodo;
                        array1[1] = odt;
                        array1[2] = odt2;
                        array1[3] = FSBOApp;
                        var miExtensionAssembly = System.Reflection.Assembly.LoadFile(System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\LibrosElectronicos.dll");
                        var miExtensionType = miExtensionAssembly.GetType("LibrosElectronicosXml.LEXml");
                        var miExtensionObjeto = Activator.CreateInstance(miExtensionType);
                        xmlString = ((System.String)miExtensionObjeto.GetType().InvokeMember("CrearXMLLibroDiarioMayor", System.Reflection.BindingFlags.InvokeMethod, System.Type.DefaultBinder, miExtensionObjeto, array1));
                        FSBOf._ReleaseCOMObject(miExtensionAssembly);
                        FSBOf._ReleaseCOMObject(miExtensionType);
                        FSBOf._ReleaseCOMObject(miExtensionObjeto);
                    }
                    else if (oForm.Title == "Balance")
                    {
                        xmlString = CrearXMLBalance(PeriodoD, PeriodoH);
                        /*array1 = new System.Object[4];
                        array1[0] = PeriodoD;
                        array1[1] = PeriodoH;
                        array1[2] = odt;
                        array1[3] = FSBOApp;
                        var miExtensionAssembly = System.Reflection.Assembly.LoadFile(System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\LibrosElectronicos.dll");
                        var miExtensionType = miExtensionAssembly.GetType("LibrosElectronicosXml.LEXml");
                        var miExtensionObjeto = Activator.CreateInstance(miExtensionType);
                        xmlString = ((System.String)miExtensionObjeto.GetType().InvokeMember("CrearXMLBalance", System.Reflection.BindingFlags.InvokeMethod, System.Type.DefaultBinder, miExtensionObjeto, array1));
                        FSBOf._ReleaseCOMObject(miExtensionAssembly);
                        FSBOf._ReleaseCOMObject(miExtensionType);
                        FSBOf._ReleaseCOMObject(miExtensionObjeto);*/
                    }

                    if (xmlString == "")
                        FSBOApp.StatusBar.SetText("No se ha creado el XML", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    else
                    {
                        oXml = new XmlDocument();
                        oXml.LoadXml(xmlString);

                        if (oForm.Title == "Diccionario")
                        {
                            if (File.Exists("C:\\Libros Contable Elect\\DICC" + Periodo.Replace("-", "") + ".xml"))
                                File.Delete("C:\\Libros Contable Elect\\DICC" + Periodo.Replace("-", "") + ".xml");
                            oXml.Save("C:\\Libros Contable Elect\\DICC" + Periodo.Replace("-", "") + ".xml");
                            FSBOApp.StatusBar.SetText("Exportado satisfactoriamente en C:\\Libros Contable Elect\\", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                        }
                        else if (oForm.Title == "Libro Diario y Mayor")
                        {
                            if (File.Exists("C:\\Libros Contable Elect\\LDIMA" + Periodo.Replace("-", "") + ".xml"))
                                File.Delete("C:\\Libros Contable Elect\\LDIMA" + Periodo.Replace("-", "") + ".xml");
                            oXml.Save("C:\\Libros Contable Elect\\LDIMA" + Periodo.Replace("-", "") + ".xml");
                            FSBOApp.StatusBar.SetText("Exportado satisfactoriamente en C:\\Libros Contable Elect\\", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                        }
                        else if (oForm.Title == "Balance")
                        {
                            if (File.Exists("C:\\Libros Contable Elect\\BALANCE" + PeriodoH.Replace("-", "") + ".xml"))
                                File.Delete("C:\\Libros Contable Elect\\BALANCE" + PeriodoH.Replace("-", "") + ".xml");
                            oXml.Save("C:\\Libros Contable Elect\\BALANCE" + PeriodoH.Replace("-", "") + ".xml");
                            FSBOApp.StatusBar.SetText("Exportado satisfactoriamente en C:\\Libros Contable Elect\\", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                        }
                        
                    }
                }
            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText(x.Message + " ** Trace: " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("Exportar: " + x.Message + " ** Trace: " + x.StackTrace);
            }
        }


        //Para pruebas
        private String CrearXMLDiccionario(String Periodo)
        {
            String NomCol;
            try
            {
                s = @"<?xml version=""1.0"" encoding=""utf-8""?><LceCoCierre version=""1.0""><LceDiccionario version=""1.0""><DocumentoDiccionario ID=""DICC{0}"">";
                s = String.Format(s, Periodo.Replace("-","."));

                NomCol = odt.Columns.Item(0).Name;
                var Col = NomCol.Split('/');
                s = s + @"<{0}><{1}>{2}</{1}>";
                s = String.Format(s, Col[0], Col[1], ((System.String)odt.GetValue(0, 0)).Trim());

                NomCol = odt.Columns.Item(1).Name;
                Col = NomCol.Split('/');
                s = s + @"<{1}>{2}</{1}></{0}>";
                s = String.Format(s, Col[0], Col[1], ((System.String)odt.GetValue(1, 0)).Trim());

                for (Int32 i = 0; i < odt.Rows.Count; i++)
                {
                    NomCol = odt.Columns.Item(2).Name;
                    Col = NomCol.Split('/');
                    var sFin = Col[0];
                    s = s + @"<{0}>";
                    s = String.Format(s, sFin);

                    for (Int32 c = 2; c < odt.Columns.Count-1; c++)
                    {
                        NomCol = odt.Columns.Item(c).Name;
                        Col = NomCol.Split('/');
                        s = s + @"<{0}>{1}</{0}>";
                        s = String.Format(s, Col[1], ((System.String)odt.GetValue(c, i)).Trim());
                    }

                    s = s + @"</{0}>";
                    s = String.Format(s, sFin);
                }

                s = s + @"</DocumentoDiccionario></LceDiccionario></LceCoCierre>";

                return s;
            }
            catch (Exception z)
            {
                FSBOApp.StatusBar.SetText(z.Message + " ** Trace: " + z.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CrearXMLDiccionario: " + z.Message + " ** Trace: " + z.StackTrace);
                return "";
            }
        }

        //Para Pruebas
        private String CrearXMLLibroDiarioMayor(String Periodo)
        {
            String NomCol;
            String NomCol2;
            Int32 i;
            Int32 c = 0;
            Boolean bIden;
            String[] Col2;
            try
            {
                s = odt.GetAsXML();
                s = @"<?xml version=""1.0"" encoding=""utf-8""?><LceCoCierre version=""1.0""><LceDiarioRes version=""1.0""><DocumentoDiarioRes ID=""DIARIO_RES_{0}"">";
                s = String.Format(s, Periodo.Replace(".", "-"));

                NomCol = odt.Columns.Item(0).Name;
                var Col = NomCol.Split('/');
                s = s + @"<{0}><{1}>{2}</{1}>";
                s = String.Format(s, Col[0], Col[1], ((System.String)odt.GetValue(0, 0)).Trim());

                NomCol = odt.Columns.Item(1).Name;
                Col = NomCol.Split('/');
                s = s + @"<{0}><{1}>{2}</{1}>";
                s = String.Format(s, Col[1], Col[2], ((System.String)odt.GetValue(1, 0)).Trim());

                NomCol = odt.Columns.Item(2).Name;
                Col = NomCol.Split('/');
                s = s + @"<{0}>{1}</{0}></{2}></{3}>";
                //[Identificacion/PeriodoTributario/Final]
                s = String.Format(s, Col[2], ((System.String)odt.GetValue(2, 0)).Trim(), Col[1], Col[0]);

                //RegistroDiario
                for (i = 0; i < odt.Rows.Count; i++)
                {
                    NomCol = odt.Columns.Item(3).Name;
                    Col = NomCol.Split('/');
                    var sFin = Col[0];
                    s = s + @"<{0}>";
                    s = String.Format(s, sFin);

                    for (c = 3; c < odt.Columns.Count; c++)
                    {
                        NomCol = odt.Columns.Item(c).Name;
                        Col = NomCol.Split('/');
                        if (Col[0] != sFin)
                            break;
                        s = s + @"<{0}>{1}</{0}>";
                        s = String.Format(s, Col[1], ((System.String)odt.GetValue(c, i)).Trim());
                    }
                    s = s + @"</{0}>";
                    s = String.Format(s, sFin);
                }
                //Cierre
                for (Int32 x = 0; x == 0; x++)
                {
                    NomCol = odt.Columns.Item(c).Name;
                    Col = NomCol.Split('/');
                    var sFin = Col[0];
                    s = s + @"<{0}>";
                    s = String.Format(s, sFin);

                    for (Int32 z = c; z < odt.Columns.Count; z++)
                    {
                        NomCol = odt.Columns.Item(z).Name;
                        Col = NomCol.Split('/');
                        if (Col[0] != sFin)
                            break;
                        s = s + @"<{0}>{1}</{0}>";
                        s = String.Format(s, Col[1], ((System.String)odt.GetValue(z, x)).Trim());
                    }
                    s = s + @"</{0}>";
                    s = String.Format(s, sFin);
                }
                s = s + @"</DocumentoDiarioRes></LceDiarioRes>"; //</LceCoCierre>";

                //Inicia Libro Mayor
                s = s + @"<LceMayorRes version=""1.0""><DocumentoMayorRes ID=""MAYOR_RES_{0}"">";
                s = String.Format(s, Periodo.Replace(".", "-"));

                //Identificacion
                NomCol = odt2.Columns.Item(0).Name;
                Col = NomCol.Split('/');
                s = s + @"<{0}><{1}>{2}</{1}>";
                s = String.Format(s, Col[0], Col[1], ((System.String)odt2.GetValue(0, 0)).Trim());

                NomCol = odt2.Columns.Item(1).Name;
                Col = NomCol.Split('/');
                s = s + @"<{0}><{1}>{2}</{1}>";
                s = String.Format(s, Col[1], Col[2], ((System.String)odt2.GetValue(1, 0)).Trim());

                NomCol = odt2.Columns.Item(2).Name;
                Col = NomCol.Split('/');
                s = s + @"<{0}>{1}</{0}></{2}></{3}>";
                //[Identificacion/PeriodoTributario/Final]
                s = String.Format(s, Col[2], ((System.String)odt2.GetValue(2, 0)).Trim(), Col[1], Col[0]);

                //Cuenta
                for (i = 0; i < odt2.Rows.Count; i++)
                {
                    NomCol = odt2.Columns.Item(3).Name;
                    Col = NomCol.Split('/');
                    var sFin = Col[0];
                    s = s + @"<{0}>";
                    s = String.Format(s, sFin);

                    for (c = 3; c < odt2.Columns.Count; c++)
                    {
                        NomCol = odt2.Columns.Item(c).Name;
                        Col = NomCol.Split('/');
                        if (Col[0] != sFin)
                            break;

                        if (Col.Count() == 2)
                        {
                            s = s + @"<{0}>{1}</{0}>";
                            s = String.Format(s, Col[1], ((System.String)odt2.GetValue(c, i)).Trim());
                        }
                        else
                        {
                            var ff = Col[1];
                            s = s + @"<{0}>";
                            s = String.Format(s, ff);//coloca Inicio Cierre

                            //MontosPeriodo - Debe
                            s = s + @"<{0}><{1}>{2}</{1}>";
                            s = String.Format(s, Col[2], Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca Debe
                            c++;

                            //MontosPeriodo - Haber
                            NomCol = odt2.Columns.Item(c).Name;
                            Col = NomCol.Split('/');
                            s = s + @"<{0}>{1}</{0}>";
                            s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca Haber
                            c++;

                            //MontosPeriodo - Deudor
                            if (((System.String)odt2.GetValue(c, i)).Trim() != "0")
                            {
                                NomCol = odt2.Columns.Item(c).Name;
                                Col = NomCol.Split('/');
                                s = s + @"<{0}>{1}</{0}>";
                                s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca Deudor
                            }
                            c++;

                            //MontosPeriodo - Acreedor
                            if (((System.String)odt2.GetValue(c, i)).Trim() != "0")
                            {
                                NomCol = odt2.Columns.Item(c).Name;
                                Col = NomCol.Split('/');
                                s = s + @"<{0}>{1}</{0}>";
                                s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca Acreedor
                            }
                            s = s + @"</{0}>"; //Coloca Final MontosPeriodo
                            s = String.Format(s, Col[2]);
                            c++;

                            //**Agrega Montos Acumulados
                            NomCol = odt2.Columns.Item(c).Name;
                            Col = NomCol.Split('/');
                            s = s + @"<{0}>";
                            s = String.Format(s, Col[2]);//coloca Inicio MontosAcumulado

                            //MontosAcumulado - Debe
                            s = s + @"<{0}>{1}</{0}>";
                            s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca Debe
                            c++;

                            //MontosAcumulado - Haber
                            NomCol = odt2.Columns.Item(c).Name;
                            Col = NomCol.Split('/');
                            s = s + @"<{0}>{1}</{0}>";
                            s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca Haber
                            c++;

                            if (((System.String)odt2.GetValue(c, i)).Trim() != "0")
                            {
                                NomCol = odt2.Columns.Item(c).Name;
                                Col = NomCol.Split('/');
                                s = s + @"<{0}>{1}</{0}>";
                                s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca SaldoDeudor
                            }
                            c++;

                            if (((System.String)odt2.GetValue(c, i)).Trim() != "0")
                            {
                                NomCol = odt2.Columns.Item(c).Name;
                                Col = NomCol.Split('/');
                                s = s + @"<{0}>{1}</{0}>";
                                s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca SaldoAcreedor
                            }
                            s = s + @"</{0}>"; //Coloca Final MontosAcumulado
                            s = String.Format(s, Col[2]);

                            c++;

                            s = s + @"</{0}>"; //Coloca Final Cierre
                            s = String.Format(s, ff);
                        }
                    }
                    s = s + @"</{0}>";
                    s = String.Format(s, sFin); //Cierra Cuenta
                }
                //[Cuenta/CodigoCuenta]
                //[Cuenta/Cierre/MontosPeriodo/Deudor]
                s = s + @"</DocumentoMayorRes></LceMayorRes></LceCoCierre>";
                

                return s;
            }
            catch (Exception z)
            {
                FSBOApp.StatusBar.SetText(z.Message + " ** Trace: " + z.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CrearXMLDiccionario: " + z.Message + " ** Trace: " + z.StackTrace);
                return "";
            }
        }


        //para Pruebas
        private String CrearXMLBalance(String PeriodoD, String PeriodoH)
        {
            String NomCol;
            String NomCol2;
            Int32 i;
            Int32 c = 0;
            Boolean bIden;
            String[] Col2;

            try
            {
                s = odt.GetAsXML();
                s = @"<?xml version=""1.0"" encoding=""utf-8""?><LceCoCierre version=""1.0""><LceBalance version=""1.0"">"; // xsi:schemaLocation=""http://www.sii.cl/SiiLce LceBalance_v10.xsd"">
                s = s + @"<DocumentoBalance ID=""BALANCE_{0}"">";
                s = String.Format(s, PeriodoH.Replace(".", "-"));

                NomCol = odt.Columns.Item(0).Name;
                var Col = NomCol.Split('/');
                s = s + @"<{0}><{1}>{2}</{1}>";
                s = String.Format(s, Col[0], Col[1], ((System.String)odt.GetValue(0, 0)).Trim());

                NomCol = odt.Columns.Item(1).Name;
                Col = NomCol.Split('/');
                s = s + @"<{1}>{2}</{1}></{0}>";
                s = String.Format(s, Col[0], Col[1], ((System.String)odt.GetValue(1, 0)).Trim());

                //Balance
                for (i = 0; i < odt.Rows.Count; i++)
                {
                    NomCol = odt.Columns.Item(2).Name;
                    Col = NomCol.Split('/');
                    var sFin = Col[0];
                    s = s + @"<{0}>";
                    s = String.Format(s, sFin);

                    for (c = 2; c < odt.Columns.Count; c++)
                    {
                        NomCol = odt.Columns.Item(c).Name;
                        Col = NomCol.Split('/');
                        if (Col[0] != sFin)
                            break;
                        s = s + @"<{0}>{1}</{0}>";
                        s = String.Format(s, Col[1], ((System.String)odt.GetValue(c, i)).Trim());
                    }
                    s = s + @"</{0}>";
                    s = String.Format(s, sFin);
                }
                s = s + "</DocumentoBalance></LceBalance>";
                

                //Diccionario de Datos********************
                s = s + @"<LceDiccionario version=""1.0""><DocumentoDiccionario ID=""DICC{0}"">";
                s = String.Format(s, PeriodoH.Replace("-","."));

                NomCol = odt2.Columns.Item(0).Name;
                Col = NomCol.Split('/');
                s = s + @"<{0}><{1}>{2}</{1}>";
                s = String.Format(s, Col[0], Col[1], ((System.String)odt2.GetValue(0, 0)).Trim());

                NomCol = odt2.Columns.Item(1).Name;
                Col = NomCol.Split('/');
                s = s + @"<{1}>{2}</{1}></{0}>";
                s = String.Format(s, Col[0], Col[1], ((System.String)odt2.GetValue(1, 0)).Trim());

                for (Int32 m = 0; m < odt2.Rows.Count; m++)
                {
                    NomCol = odt2.Columns.Item(2).Name;
                    Col = NomCol.Split('/');
                    var sFin = Col[0];
                    s = s + @"<{0}>";
                    s = String.Format(s, sFin);

                    for (Int32 b = 2; b < odt2.Columns.Count-1; b++)
                    {
                        NomCol = odt2.Columns.Item(b).Name;
                        Col = NomCol.Split('/');
                        s = s + @"<{0}>{1}</{0}>";
                        s = String.Format(s, Col[1], ((System.String)odt2.GetValue(b, m)).Trim());
                    }

                    s = s + @"</{0}>";
                    s = String.Format(s, sFin);
                }

                s = s + @"</DocumentoDiccionario></LceDiccionario>";

                s = s + @"</LceCoCierre>";
                return s;
            }
            catch (Exception m)
            {
                FSBOApp.StatusBar.SetText(m.Message + " ** Trace: " + m.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CrearXMLBalance: " + m.Message + " ** Trace: " + m.StackTrace);
                return "";
            }
        }
    }//fin class
}
