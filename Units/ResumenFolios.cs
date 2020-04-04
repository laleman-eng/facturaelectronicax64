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
using System.Data.SqlClient;
using System.Data;

namespace Factura_Electronica_VK.ResumenFolios
{
    class TResumenFolios : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbouiCOM.DataTable oDataTable;
        private SAPbouiCOM.DBDataSource oDBDSCAF;
        private SAPbouiCOM.DBDataSource oDBDSDISTH;
        private SAPbouiCOM.DBDataSource oDBDSDISTD;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Grid ogrid;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.GridColumn oColumn;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            SAPbouiCOM.ComboBox oComboBox;
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                Lista = new List<string>();

                FSBOf.LoadForm(xmlPath, "VID_FERESFOL.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All
                oForm.EnableMenu("1281",false);//Buscar
                oForm.EnableMenu("1282",false);//Crear

                //VID_DelRow := true;
                //VID_DelRowOK := true;
   
                //        oForm.DataBrowser.BrowseBy := "Code"; 
                //        oDBDSCAF := oForm.DataSources.DBDataSources.Add("@VID_FECAF");
                //        oDBDSDISTH := oForm.DataSources.DBDataSources.Add("@VID_FEDIST");
                //        oDBDSDISTD := oForm.DataSources.DBDataSources.Add("@VID_FEDISTD");
                ogrid = (Grid)(oForm.Items.Item("grid").Specific);
                oDataTable = oForm.DataSources.DataTables.Add("dt");

                ogrid.DataTable = oDataTable;

                FSBOApp.StatusBar.SetText("Actualizando estado de folios asignados", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                ActualizarRegistros();

                                            // Ok Ad  Fnd Vw Rq Sec
                //        Lista.Add('TipoDoc   , f,  t,  t,  f, r, 1');
                //        FSBOf.SetAutoManaged(var oForm, Lista);

                oForm.Mode = BoFormMode.fm_OK_MODE;
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
                if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED)
                {
                    if ((pVal.ItemUID == "btnAct") && (!pVal.BeforeAction))
                    {
                        BubbleEvent = false;
                        FSBOApp.StatusBar.SetText("Actualizando estado de folios asignados", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        ActualizarRegistros();
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



        private void ActualizarRegistros()
        {
            TFunctions Reg;
            SqlDataAdapter Adapter;
            SqlConnection ConexionADO;
            DataSet cDataSet;
            String sCnn;
            int lRetCode;
            String User, Pass;

            try
            {
                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"select TOP 1 * from [@VID_FEPARAM]"; }
                else
                {   s = @"select TOP 1 * from ""@VID_FEPARAM"""; }

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                {
                    Reg = new TFunctions();
                    Reg.SBO_f = FSBOf;
                    User = Reg.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Usuario").Value));
                    Pass = Reg.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Password").Value));
                    sCnn = Reg.sConexion((System.String)(oRecordSet.Fields.Item("U_Servidor").Value), (System.String)(oRecordSet.Fields.Item("U_Base").Value), User, Pass);
                    if (sCnn.Substring(0,1) != "E")
                    {
                        ConexionADO = new SqlConnection(sCnn);
                        if (ConexionADO.State == ConnectionState.Closed)
                        {   ConexionADO.Open(); }

                        if (GlobalSettings.RunningUnderSQLServer)
                        {   s = @"SELECT T0.DocEntry, T1.VisOrder, T0.U_TipoDoc, T1.U_Folio, T1.LineId
                                  FROM [@VID_FEDIST] T0 WITH(nolock)
                                  JOIN [@VID_FEDISTD] T1 WITH(nolock) ON T1.DocEntry = T0.DocEntry
                                 WHERE T1.U_Estado = 'D'
                                   AND T0.U_Sucursal <> 'Principal'
                                "; }
                        else
                        {   s = @"SELECT T0.""DocEntry"", T1.""VisOrder"", T0.""U_TipoDoc"", T1.""U_Folio"", T1.""LineId""
                                  FROM ""@VID_FEDIST"" T0
                                  JOIN ""@VID_FEDISTD"" T1 ON T1.""DocEntry"" = T0.""DocEntry""
                                 WHERE T1.""U_Estado"" = 'D'
                                   AND T0.""U_Sucursal"" <> 'Principal'"; }

                        oRecordSet.DoQuery(s);
                        ConexionADO = new SqlConnection(sCnn);
                        if (ConexionADO.State == ConnectionState.Closed)
                        {   ConexionADO.Open(); }

                        while (!oRecordSet.EoF)
                        {
                            var VID_SP_EXISTEFOLIO= new SqlCommand("VID_SP_EXISTEFOLIO", ConexionADO);
                            var oParameter = new SqlParameter();
                            VID_SP_EXISTEFOLIO.CommandType = CommandType.StoredProcedure;
                            oParameter = VID_SP_EXISTEFOLIO.Parameters.Add("@TipoDoc", SqlDbType.VarChar, 10);
                            oParameter.Value = (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value);
	                        oParameter = VID_SP_EXISTEFOLIO.Parameters.Add("@FolioNum", SqlDbType.Int);
                            oParameter.Value = (System.Double)(oRecordSet.Fields.Item("U_Folio").Value);
	                    
                            Adapter = new SqlDataAdapter(VID_SP_EXISTEFOLIO);
                            cDataSet = new DataSet(VID_SP_EXISTEFOLIO.CommandText);
                            Adapter.Fill(cDataSet);
                            s = (System.String)(cDataSet.Tables[0].Rows[0][0].ToString()).Trim();

                            if (s == "Y")
                            {
                                s = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value));
                                s = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("LineId").Value));
                                s = Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_Folio").Value));
                                lRetCode = Reg.ActEstadoFolioUpt((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value), (System.Int32)(oRecordSet.Fields.Item("LineId").Value), (System.Double)(oRecordSet.Fields.Item("U_Folio").Value), (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value), "", "", "");
                                lRetCode = 1;
                                if (lRetCode == 0)
                                {
                                    FSBOApp.StatusBar.SetText("No se ha actualizado estado de Folio " + Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_Folio").Value)), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    OutLog("No se ha actualizado estado de Folio " + Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_Folio").Value)));
                                }
                            }

                            VID_SP_EXISTEFOLIO = null;

                            oRecordSet.MoveNext();
                        }

                        if (ConexionADO.State == ConnectionState.Open)
                        {   ConexionADO.Close(); }

                        Grilla();
               
                        if (ConexionADO.State == ConnectionState.Open)
                        {   ConexionADO.Close(); }

                        FSBOApp.StatusBar.SetText("Estado de Folios actualizados", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    }
                    else
                    {   FSBOApp.StatusBar.SetText("Faltan datos Conexion. " + sCnn.Substring(1, sCnn.Length-1), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
                }
                else
                {   FSBOApp.StatusBar.SetText("Debe ingresar datos de conexion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }

            }
            catch(Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("GuardarRegistros: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin ActualizarRegistros



        public new void MenuEvent(ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
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
                    {}
                }
     
                if ((pVal.MenuUID == "1282") || (pVal.MenuUID == "1281"))
                {}
            }
            catch(Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace,1,"Ok","","");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent


        private void Grilla()
        {
            try
            {
                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"SELECT T0.U_Sucursal
                              ,T0.U_TipoDoc
	                          ,COUNT(*) 'Asignados'
	                          ,SUM(CASE WHEN T1.U_Estado = 'D' THEN 1 ELSE 0 END) 'Disponibles'
	                          ,SUM(CASE WHEN T1.U_Estado = 'U' THEN 1 ELSE 0 END) 'Utilizados'
                          FROM [@VID_FEDIST] T0 WITH(nolock)
                          JOIN [@VID_FEDISTD] T1 WITH(nolock) ON T1.DocEntry = T0.DocEntry
                          GROUP BY
                               T0.U_Sucursal
                              ,T0.U_TipoDoc
                         ORDER BY T0.U_Sucursal, T0.U_TipoDoc"; }
                else
                {   s = @"SELECT T0.""U_Sucursal""
                              ,T0.""U_TipoDoc""
	                          ,COUNT(*) ""Asignados""
	                          ,SUM(CASE WHEN T1.""U_Estado"" = 'D' THEN 1 ELSE 0 END) ""Disponibles""
	                          ,SUM(CASE WHEN T1.""U_Estado"" = 'U' THEN 1 ELSE 0 END) ""Utilizados""
                          FROM ""@VID_FEDIST"" T0 
                          JOIN ""@VID_FEDISTD"" T1 ON T1.""DocEntry"" = T0.""DocEntry""
                          GROUP BY
                               T0.""U_Sucursal""
                              ,T0.""U_TipoDoc""
                         ORDER BY T0.""U_Sucursal"", T0.""U_TipoDoc"" "; }
                oDataTable.ExecuteQuery(s);

                ogrid.Columns.Item("U_Sucursal").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_Sucursal"));
                //EditTextColumn(oColumn).LinkedObjectType := '86';
                oColumn.Editable = false;
                oColumn.Visible = false;
                oColumn.TitleObject.Caption = "Código";

                ogrid.Columns.Item("U_TipoDoc").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("U_TipoDoc"));
                //EditTextColumn(oColumn).LinkedObjectType := '86';
                oColumn.Editable = false;
                oColumn.RightJustified = true;
                oColumn.TitleObject.Caption = "Tipo Documento";

                ogrid.Columns.Item("Asignados").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("Asignados"));
                //EditTextColumn(oColumn).LinkedObjectType := '86';
                oColumn.Editable = false;
                oColumn.RightJustified = true;
                oColumn.TitleObject.Caption = "Total Asignados";

                ogrid.Columns.Item("Disponibles").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("Disponibles"));
                //EditTextColumn(oColumn).LinkedObjectType := '86';
                oColumn.Editable = false;
                oColumn.RightJustified = true;
                oColumn.TitleObject.Caption = "Total Disponibles";

                ogrid.Columns.Item("Utilizados").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(ogrid.Columns.Item("Utilizados"));
                //EditTextColumn(oColumn).LinkedObjectType := '86';
                oColumn.Editable = false;
                oColumn.RightJustified = true;
                oColumn.TitleObject.Caption = "Total Utilizados";

                ogrid.AutoResizeColumns();

            }
            catch(Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace,1,"Ok","","");
                OutLog("Grilla: " + e.Message + " ** Trace: " + e.StackTrace);
            }

        }//fin Grilla
    }
}
