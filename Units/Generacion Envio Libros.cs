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

namespace Factura_Electronica_VK.GELibro
{
    class TGELibro : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Grid ogrid;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.EditText oEditText;
        private SAPbouiCOM.ComboBox oComboBox;
        private String s;
        private Boolean bMultiSoc = false;
        private String User;
        private String Pass;
        private String Servidor;
        private String sCnn;
        private TFunctions Param;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            //SAPbouiCOM.ComboBox oComboBox;
            //SAPbouiCOM.Column oColumn;

            Param = new TFunctions();
            Param.SBO_f = FSBOf;

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                //Lista    := New list<string>;

                FSBOf.LoadForm(xmlPath, "VID_GELibro.srf", uid);
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
                oComboBox.Select("V", BoSearchKey.psk_ByValue);

                oForm.DataSources.UserDataSources.Add("Periodo", BoDataType.dt_SHORT_TEXT);
                oComboBox = (ComboBox)(oForm.Items.Item("Periodo").Specific);
                if (GlobalSettings.RunningUnderSQLServer)
                    oRecordSet.DoQuery("select CAST(AbsEntry AS VARCHAR(20)) Code, Code 'Name'  from OFPR where YEAR(F_RefDate) >= YEAR(GETDATE())-1  AND YEAR(T_RefDate) <= YEAR(GETDATE())");
                else
                    oRecordSet.DoQuery(@"select TO_VARCHAR(""AbsEntry"") ""Code"", ""Code"" ""Name""  from ""OFPR"" where YEAR(""F_RefDate"") >= YEAR(NOW())-1  AND YEAR(""T_RefDate"") <= YEAR(NOW()) ");
                FSBOf.FillCombo((ComboBox)(oForm.Items.Item("Periodo").Specific), ref oRecordSet, false);
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select AbsEntry from OFPR where GETDATE() BETWEEN F_RefDate AND T_RefDate";
                else
                    s = @"select ""AbsEntry"" from ""OFPR"" where NOW() BETWEEN ""F_RefDate"" AND ""T_RefDate"" ";
                oRecordSet.DoQuery(s);
                oComboBox.Select(((System.Int32)oRecordSet.Fields.Item("AbsEntry").Value).ToString(), BoSearchKey.psk_ByValue);


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
            //IvkFormInterface oFormB;
            String oUid;
            String AbsDesde;
            String FDesde, FHasta;
            String TipoLibro;

            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction))
                {
                    if (pVal.ItemUID == "btn1")
                    {
                        //enviar libro al portal
                        oComboBox = ((ComboBox)oForm.Items.Item("Periodo").Specific);
                        AbsDesde = oComboBox.Selected.Value;

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select F_RefDate, T_RefDate,* from OFPR where AbsEntry = {0}";
                        else
                            s = @"select ""F_RefDate"", ""T_RefDate"" from ""OFPR"" where ""AbsEntry"" = {0}";
                        s = String.Format(s, AbsDesde);
                        oRecordSet.DoQuery(s);
                        FDesde = ((System.DateTime)oRecordSet.Fields.Item("F_RefDate").Value).ToString("yyyyMMdd");
                        FHasta = ((System.DateTime)oRecordSet.Fields.Item("T_RefDate").Value).ToString("yyyyMMdd");

                        oComboBox = ((ComboBox)oForm.Items.Item("TipoLibro").Specific);
                        TipoLibro = oComboBox.Selected.Value;

                        if (FSBOApp.MessageBox("¿ Desea enviar libro electronico al portal ?", 1, "Si", "No", "") == 1)
                            EnviarLibros(TipoLibro, FDesde, FHasta);
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


        private void EnviarLibros(String TipoLibro, String FDesde, String FHasta)
        {
            SqlConnection ConexionADO;
            SqlCommand SqlComan;
            SqlParameter oParameter;
            SqlDataAdapter Adapter;
            DataSet DataSet;
            SqlCommand cmd;
            Boolean Paso = false;
            String procedimiento = "";
            Int32 iCol;

            try
            {
                sCnn = sConnection();

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select ISNULL(U_ProcVenta,'') ProcVenta, ISNULL(U_ProcCompra,'') ProcCompra from [@VID_FEPARAM]";
                else
                    s = @"select IFNULL(""U_ProcVenta"",'') ""ProcVenta"", IFNULL(""U_ProcCompra"",'') ""ProcCompra"" from ""@VID_FEPARAM"" ";
                oRecordSet.DoQuery(s);
                
                if (oRecordSet.RecordCount == 0)
                {
                    Paso = false;
                    FSBOApp.StatusBar.SetText("Debe parametrizar el addon", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                }
                else
                {
                    if ((((System.String)oRecordSet.Fields.Item("ProcVenta").Value).Trim() == "") && (TipoLibro == "V"))
                    {
                        Paso = false;
                        FSBOApp.StatusBar.SetText("Debe ingresar procedimiento para Libro de Venta en parametros del addon Factura Electronica", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    }
                    else if ((((System.String)oRecordSet.Fields.Item("ProcCompra").Value).Trim() == "") && (TipoLibro == "C"))
                    {
                        Paso = false;
                        FSBOApp.StatusBar.SetText("Debe ingresar procedimiento para Libro de Compra en parametros del addon Factura Electronica", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    }
                    else
                    {
                        Paso = true;
                        if (TipoLibro == "V")
                            procedimiento = ((System.String)oRecordSet.Fields.Item("ProcVenta").Value).Trim();
                        else
                            procedimiento = ((System.String)oRecordSet.Fields.Item("ProcCompra").Value).Trim();
                    }
                }

                if (Paso == true)
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = "exec " + procedimiento + " '" + FDesde + "'" + ", '" + FHasta + "'";
                    else
                        s = "CALL " + procedimiento + "  ('" + FDesde + "', '" + FHasta + "')";

                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount > 0)
                    {
                        //primero borrar registros previos del periodo
                        ConexionADO = new SqlConnection(sCnn);
                        if (ConexionADO.State == ConnectionState.Closed) ConexionADO.Open();
                        cmd = new SqlCommand();
                        cmd.CommandTimeout = 0;
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = ConexionADO;
                        s = "exec DelDocLibroPeriodo '{0}', {1}";
                        s = String.Format(s, FDesde.Substring(0,6), TipoLibro);
                        cmd.CommandText = s;
                        cmd.ExecuteNonQuery();
                        //fin limpia registros
                        
                        if (TipoLibro == "V")
                            s = "EInsLibroVentaManual";
                        else
                            s = "EInsLibroCompraManual";

                        SqlComan = new SqlCommand(s, ConexionADO);

                        oParameter = new SqlParameter();
                        SqlComan.CommandType = CommandType.StoredProcedure;
                        iCol = 0;
                        while (!oRecordSet.EoF)
                        {
                            while (iCol < oRecordSet.Fields.Count)
                            {
                                var NomCol = "@" + oRecordSet.Fields.Item(iCol).Name;
                                s = oRecordSet.Fields.Item(iCol).Type.ToString();
                                oParameter = SqlComan.Parameters.AddWithValue(NomCol, oRecordSet.Fields.Item(iCol).Value.ToString());

                                OutLog("Parametro  " + NomCol + " - Valor " + oRecordSet.Fields.Item(iCol).Value);
                                iCol++;

                            }
                            Adapter = new SqlDataAdapter(SqlComan);
                            DataSet = new DataSet(SqlComan.CommandText);
                            Adapter.Fill(DataSet);
                            //dyt_id_traspaso = Convert.ToInt32(DataSet.Tables[0].Rows[0][1].ToString());
                            oRecordSet.MoveNext();
                        }
                        FSBOApp.MessageBox("Se han cargado " + oRecordSet.RecordCount + " documentos en el portal");
                    }
                
                
                }
            }
            catch (Exception ex)
            {
                FSBOApp.StatusBar.SetText("EnviarLibros: " + ex.Message + " ** Trace: " + ex.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("EnviarLibros: " + ex.Message + " ** Trace: " + ex.StackTrace);
            }
        }


        private String sConnection()
        {
            String sMultiSoc;
            try
            {
                if (bMultiSoc == true)
                {
                    oComboBox = (ComboBox)(oForm.Items.Item("Instituto").Specific);
                    sMultiSoc = oComboBox.Selected.Value;

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select U_Servidor, U_Base, U_Usuario, U_Password
                                from [@VID_FEMULTISOC] WITH (NOLOCK)
                               where DocEntry = {0}";
                    else
                        s = @"select ""U_Servidor"", ""U_Base"", ""U_Usuario"", ""U_Password""
                                       from ""@VID_FEMULTISOC""
                                      where ""DocEntry"" = {0} ";
                    s = String.Format(s, sMultiSoc);
                }
                else
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"Select U_Servidor
                                    ,U_Base
                                    ,U_Usuario
                                    ,U_Password
                               from [@VID_FEPARAM] ";
                    else
                        s = @"Select ""U_Servidor""
                                    ,""U_Base""
                                    ,""U_Usuario""
                                    ,""U_Password""
                               from ""@VID_FEPARAM"" ";
                }
                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                {
                    if (bMultiSoc == true)
                    {
                        User = (System.String)(oRecordSet.Fields.Item("U_Usuario").Value).ToString().Trim();
                        Pass = (System.String)(oRecordSet.Fields.Item("U_Password").Value).ToString().Trim();
                    }
                    else
                    {
                        User = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Usuario").Value).ToString().Trim());
                        Pass = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Password").Value).ToString().Trim());
                    }
                    return Param.sConexion((System.String)(oRecordSet.Fields.Item("U_Servidor").Value), (System.String)(oRecordSet.Fields.Item("U_Base").Value), User, Pass);
                }
                else
                    return "";
            }
            catch (Exception e)
            {

                return "";
            }
        }

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
