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
using Factura_Electronica_VK.Functions;

namespace Factura_Electronica_VK.ConfigFE
{
    public class TConfigFE : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbouiCOM.DataTable dt;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Matrix oMtx;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.Item oItem;
        private SAPbouiCOM.DBDataSource oDBDSHeader;
        private SAPbouiCOM.Column oColumn;
        private SAPbouiCOM.UserDataSource DSOpDif;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            //SAPbouiCOM.ComboBox oCombo;
            TFunctions Param;
            SAPbouiCOM.CheckBox oCheckBox;
            SAPbouiCOM.EditText oEditText;

            //
            //  obetener recurso
            //  try
            //  .....
            //  finally
            //  liberar recurso
            //  end

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            try
            {

                //Lista    := New list<string>;

                FSBOf.LoadForm(xmlPath, "strCnn.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All

                //oForm.DataBrowser.BrowseBy := "DocNum"; 

                // Ok Ad  Fnd Vw Rq Sec
                //Lista.Add('DocNum    , f,  f,  t,  f, n, 1');
                //Lista.Add('DocDate   , f,  t,  f,  f, r, 1');
                //Lista.Add('CardCode  , f,  t,  t,  f, r, 1');
                //FSBOf.SetAutoManaged(var oForm, Lista);

                oDBDSHeader = (DBDataSource)(oForm.DataSources.DBDataSources.Item("@VID_FEPARAM"));

                oCheckBox = (CheckBox)(oForm.Items.Item("chkMon").Specific);
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";

                oCheckBox = (CheckBox)(oForm.Items.Item("chkDteTra").Specific);
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";

                //oCheckBox = (CheckBox)(oForm.Items.Item("chkCrearD").Specific);
                //oCheckBox.ValOn = "Y";
                //oCheckBox.ValOff = "N";

                //oCheckBox = (CheckBox)(oForm.Items.Item("chkCrearDS").Specific);
                //oCheckBox.ValOn = "Y";
                //oCheckBox.ValOff = "N";

                oCheckBox = (CheckBox)(oForm.Items.Item("chkPrint").Specific);
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";

                oCheckBox = (CheckBox)(oForm.Items.Item("chkDistrib").Specific);
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";

                oCheckBox = (CheckBox)(oForm.Items.Item("chkVal90").Specific);
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";

                oCheckBox = (CheckBox)(oForm.Items.Item("MultiSoc").Specific);
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";

                oCheckBox = (CheckBox)(oForm.Items.Item("ValDescL").Specific);
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";

                oCheckBox = (CheckBox)(oForm.Items.Item("GeneraT").Specific);
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";

                oCheckBox = (CheckBox)(oForm.Items.Item("SubirSuc").Specific);
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";

                oCheckBox = (CheckBox)(oForm.Items.Item("AbrirDoc").Specific);
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";

                oCheckBox = (CheckBox)(oForm.Items.Item("chkFProv").Specific);
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";

                oEditText = (EditText)(oForm.Items.Item("Pasword").Specific);
                oEditText.IsPassword = true;


                //Configuración RadioButtons
                DSOpDif = oForm.DataSources.UserDataSources.Add("rbOpDif", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                ((OptionBtn)oForm.Items.Item("opDifPor").Specific).DataBind.SetBound(true, "", "rbOpDif");//1

                ((OptionBtn)oForm.Items.Item("opDifMon").Specific).DataBind.SetBound(true, "", "rbOpDif");//2
                ((OptionBtn)oForm.Items.Item("opDifMon").Specific).GroupWith("opDifPor");


                if (!GlobalSettings.RunningUnderSQLServer)
                    oForm.Items.Item("btnProcFE").Visible = false;
                else
                    oForm.Items.Item("btnProcFE").Visible = true;

                //s := 'Select count(*) cant from [@VID_FEPARAM]';
                if (GlobalSettings.RunningUnderSQLServer)
                {
                    s = @"Select count(*) CANT
                                ,U_Usuario
                                ,U_Password
                                ,U_UserWSCL
                                ,U_PassWSCL
                                ,ISNULL(U_TipoDif,'M') 'U_TipoDif'
                            from [@VID_FEPARAM] 
                            group by U_Usuario
                                ,U_Password
                                ,U_UserWSCL
                                ,U_PassWSCL
                                ,ISNULL(U_TipoDif,'M')";
                }
                else
                {
                    s = @"Select count(*) ""CANT"" 
                           ,""U_Usuario"" 
                           ,""U_Password""
                           ,""U_UserWSCL""
                           ,""U_PassWSCL""
                           ,IFNULL(""U_TipoDif"",'M') ""U_TipoDif""
                      from ""@VID_FEPARAM"" 
                     group by ""U_Usuario"" 
                             ,""U_Password""
                             ,""U_UserWSCL""
                             ,""U_PassWSCL""
                             ,IFNULL(""U_TipoDif"",'M') ";
                }
                oRecordSet.DoQuery(s);
                if ((System.Int32)(oRecordSet.Fields.Item("CANT").Value) > 0)
                {
                    Param = new TFunctions();
                    Param.SBO_f = FSBOf;

                    oForm.SupportedModes = 1;
                    oForm.Mode = BoFormMode.fm_UPDATE_MODE;
                    oDBDSHeader.Query(null);

                    s = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Usuario").Value).ToString().Trim());
                    oDBDSHeader.SetValue("U_Usuario", 0, s);

                    s = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Password").Value).ToString().Trim());
                    oDBDSHeader.SetValue("U_Password", 0, s);

                    s = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_UserWSCL").Value).ToString().Trim());
                    oDBDSHeader.SetValue("U_UserWSCL", 0, s);

                    s = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_PassWSCL").Value).ToString().Trim());
                    oDBDSHeader.SetValue("U_PassWSCL", 0, s);

                    oForm.PaneLevel = 101;

                    if (((System.String)oDBDSHeader.GetValue("U_MultiSoc", 0)).Trim() == "Y")
                    {
                        oForm.Items.Item("chkDistrib").Visible = false;
                        oForm.Items.Item("AbrirDoc").Visible = true;
                    }
                    else if (((System.String)oDBDSHeader.GetValue("U_MultiSoc", 0)).Trim() == "N")
                    {
                        oForm.Items.Item("chkDistrib").Visible = true;
                        oForm.Items.Item("AbrirDoc").Visible = false;
                    }
                    
                    oForm.Freeze(false);
                    if (((System.String)oRecordSet.Fields.Item("U_TipoDif").Value).Trim() == "M")
                        DSOpDif.Value = "2";
                        //((OptionBtn)oForm.Items.Item("opDifMon").Specific).Selected = true;
                    else
                        DSOpDif.Value = "1";
                        //((OptionBtn)oForm.Items.Item("opDifPor").Specific).Selected = true;
                }
                else
                {
                    oForm.SupportedModes = 3;
                    oForm.Mode = BoFormMode.fm_ADD_MODE;
                    oForm.PaneLevel = 101;
                    ((OptionBtn)oForm.Items.Item("opDifMon").Specific).Selected = true;
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
            //inherited FormEvent(FormUID,var pVal,var BubbleEvent);
            String Local;
            SAPbouiCOM.CheckBox oCheckBox;
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);


            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction) && (pVal.ItemUID == "Folder1"))
                    oForm.PaneLevel = 101;

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction) && (pVal.ItemUID == "Folder2"))
                    oForm.PaneLevel = 102;

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction == true))
                {
                    if ((pVal.ItemUID == "1") && ((oForm.Mode == BoFormMode.fm_ADD_MODE) || (oForm.Mode == BoFormMode.fm_UPDATE_MODE)))
                    {
                        s = "1";
                        oDBDSHeader.SetValue("Code", 0, s);
                        if (1 != FSBOApp.MessageBox("¿ Desea actualizar los parametros ?", 1, "Ok", "Cancelar", ""))
                            BubbleEvent = false;
                        else
                        {
                            BubbleEvent = false;
                            if (oForm.SupportedModes == 1)
                                s = "1";
                            else
                                s = "3";

                            if (AddDatos(s))
                            {
                                FSBOApp.StatusBar.SetText("Datos actualizados satisfactoriamente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                oForm.Mode = BoFormMode.fm_OK_MODE;
                                //Remover menu y colocar los nuevos segun parametros


                                System.Xml.XmlDocument oXmlDoc = null;
                                oXmlDoc = new System.Xml.XmlDocument();
                                oXmlDoc.Load(System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\Menus\\RemoveMenu.xml");

                                string sXML = oXmlDoc.InnerXml.ToString();
                                FSBOApp.LoadBatchActions(ref sXML);

                                oXmlDoc.Load(System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\Menus\\Menu.xml");

                                sXML = oXmlDoc.InnerXml.ToString();
                                FSBOApp.LoadBatchActions(ref sXML);
                            }
                        }
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction))
                {
                    if (pVal.ItemUID == "btnTest")
                        TestConexion();
                    // FSBOApp.Menus.Item("4873").Activate();


                    if (pVal.ItemUID == "btnProcFE")
                        CargarProcedimientos();

                    if (pVal.ItemUID == "btnProcPor")
                        CargarProcedimientosPortal();
                }


                if ((pVal.ItemUID == "MultiSoc") && (pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction == false))
                {
                    oCheckBox = (SAPbouiCOM.CheckBox)oForm.Items.Item("MultiSoc").Specific;
                    if (oCheckBox.Checked)
                    {
                        oForm.Items.Item("chkDistrib").Visible = false;
                        oDBDSHeader.SetValue("U_Distrib", 0, "N");
                        oForm.Items.Item("chkFPortal").Visible = false;
                        oDBDSHeader.SetValue("U_FPortal", 0, "N");

                        oForm.Items.Item("AbrirDoc").Visible = true;
                        oDBDSHeader.SetValue("U_AbrirDoc", 0, "N");
                    }
                    else
                    {
                        oForm.Items.Item("chkDistrib").Visible = true;
                        oForm.Items.Item("chkFPortal").Visible = true;
                        oForm.Items.Item("AbrirDoc").Visible = false;
                        
                    }

                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction == false))
                {
                    if (pVal.ItemUID == "chkDistrib")
                    {
                        if (((CheckBox)oForm.Items.Item("chkDistrib").Specific).Checked == true)
                            oDBDSHeader.SetValue("U_FPortal", 0, "N");
                    }

                    if (pVal.ItemUID == "chkFPortal")
                    {
                        if (((CheckBox)oForm.Items.Item("chkFPortal").Specific).Checked == true)
                        {
                            oDBDSHeader.SetValue("U_Distrib", 0, "N");
                            oDBDSHeader.SetValue("U_GenerarT", 0, "N");
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

        }//fin FormEvent


        private Boolean AddDatos(String Tipo)
        {
            TFunctions Param;
            String usuario;
            String password;
            Boolean _return;
            String UserWSCL;
            String PassWSCL;

            try
            {
                _return = false;
                if (ValidacionFinal())
                {
                    usuario = (System.String)(oDBDSHeader.GetValue("U_Usuario", 0)).Trim();
                    password = (System.String)(oDBDSHeader.GetValue("U_Password", 0)).Trim();
                    UserWSCL = (System.String)(oDBDSHeader.GetValue("U_UserWSCL", 0)).Trim();
                    PassWSCL = (System.String)(oDBDSHeader.GetValue("U_PassWSCL", 0)).Trim();

                    Param = new TFunctions();
                    Param.SBO_f = FSBOf;

                    s = Param.Encriptar(usuario);
                    oDBDSHeader.SetValue("U_Usuario", 0, s);

                    s = Param.Encriptar(password);
                    oDBDSHeader.SetValue("U_Password", 0, s);

                    if (UserWSCL != "")
                    {
                        s = Param.Encriptar(UserWSCL);
                        oDBDSHeader.SetValue("U_UserWSCL", 0, s);
                    }
                    else
                        oDBDSHeader.SetValue("U_UserWSCL", 0, "");

                    if (PassWSCL != "")
                    {
                        s = Param.Encriptar(PassWSCL);
                        oDBDSHeader.SetValue("U_PassWSCL", 0, s);
                    }
                    else
                        oDBDSHeader.SetValue("U_PassWSCL", 0, "");

                    if (DSOpDif.Value == "1")
                        oDBDSHeader.SetValue("U_TipoDif", 0, "P");
                    else
                        oDBDSHeader.SetValue("U_TipoDif", 0, "M");

                    if (Tipo == "1")
                        _return = Param.ParamUpd(oDBDSHeader);
                    else
                        _return = Param.ParamAdd(oDBDSHeader);

                    oDBDSHeader.SetValue("U_Usuario", 0, usuario);
                    oDBDSHeader.SetValue("U_Password", 0, password);
                    oDBDSHeader.SetValue("U_UserWSCL", 0, UserWSCL);
                    oDBDSHeader.SetValue("U_PassWSCL", 0, PassWSCL);

                    _return = true;
                }
                return _return;
            }
            catch (Exception e)
            {
                OutLog("AddDatos : " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.StatusBar.SetText("AddDatos : " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                return false;
            }
        }//fin AddDatos


        private Boolean ValidacionFinal()
        {
            Boolean _result;
            SAPbouiCOM.CheckBox oCheckBox;
            SAPbouiCOM.CheckBox oCheckBoxD;
            try
            {
                _result = true;

                oCheckBox = (CheckBox)(oForm.Items.Item("MultiSoc").Specific);
                oCheckBoxD = (CheckBox)(oForm.Items.Item("chkDistrib").Specific);
                s = (System.String)(oDBDSHeader.GetValue("U_Servidor", 0)).Trim();
                if ((System.String)(oDBDSHeader.GetValue("U_Servidor", 0)).Trim() == "" && oCheckBox.Checked == false)
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar Servidor", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    _result = false;
                }

                if (((System.String)(oDBDSHeader.GetValue("U_Usuario", 0)).Trim() == "") && (_result) && (oCheckBox.Checked == false))
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar Usuario", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    _result = false;
                }

                if (((System.String)(oDBDSHeader.GetValue("U_Password", 0)).Trim() == "") && (_result) && (oCheckBox.Checked == false))
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar Password", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    _result = false;
                }

                if (((System.String)(oDBDSHeader.GetValue("U_OficinaSII", 0)).Trim() == "") && (_result) && (oCheckBox.Checked == false) && (oCheckBoxD.Checked == true))
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar Oficina SII", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    _result = false;
                }

                return _result;
            }
            catch (Exception e)
            {
                OutLog("ValidacionFinal : " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.StatusBar.SetText("ValidacionFinal : " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                return false;
            }

        }//lfin ValidacionFinal


        private void TestConexion()
        {
            SqlConnection cnn = null;
            TFunctions Param;

            try
            {
                Param = new TFunctions();
                Param.SBO_f = FSBOf;
                s = Param.sConexion((System.String)(oDBDSHeader.GetValue("U_Servidor", 0)), (System.String)(oDBDSHeader.GetValue("U_Base", 0)), (System.String)(oDBDSHeader.GetValue("U_Usuario", 0)), (System.String)(oDBDSHeader.GetValue("U_Password", 0)));

                if (s.Substring(0, 1) != "E")
                {
                    try
                    {
                        cnn = new SqlConnection(s);
                        cnn.Open();
                        FSBOApp.StatusBar.SetText("Cadena de conexión válida", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                        //rescatar Comuna SII, fecha y numero resolucion
                        s = "select SUB_TIPO_PARAM, VALOR from parametros where tipo_param ='empresa' and sub_tipo_param in ('ComunaSII','NumeroResolucion','FechaResolucion')";
                        var command = new SqlCommand(s, cnn);
                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            s = ((System.String)reader.GetValue(0)).Trim();
                            s = ((System.String)reader.GetValue(1)).Trim();
                            if (((System.String)reader.GetValue(0)).Trim() == "ComunaSII")
                                oDBDSHeader.SetValue("U_OficinaSII", 0, ((System.String)reader.GetValue(1)).Trim());
                            else if (((System.String)reader.GetValue(0)).Trim() == "NumeroResolucion")
                                oDBDSHeader.SetValue("U_NumRes", 0, ((System.String)reader.GetValue(1)).Trim());
                            else if (((System.String)reader.GetValue(0)).Trim() == "FechaResolucion")
                            {
                                var fecha = DateTime.ParseExact(s, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                oDBDSHeader.SetValue("U_FechaRes", 0, fecha.ToString("yyyyMMdd"));
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        FSBOApp.StatusBar.SetText("ingrese cadena de conexión válida...," + s, SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        OutLog("TestConexion(1) : " + ex.Message + " ** Trace: " + ex.StackTrace);
                    }
                    finally
                    {
                        if (cnn.State == System.Data.ConnectionState.Open)
                        { cnn.Close(); }
                    }
                }
                else
                { FSBOApp.StatusBar.SetText(s.Substring(1, s.Length - 1), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
            }
            catch (Exception e)
            {
                OutLog("TestConexion : " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.StatusBar.SetText("TestConexion : " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }//fin TestConexion


        private void CargarProcedimientos()
        {
            String SQLFile;
            System.IO.StreamReader sr;
            String ruta;
            System.String[] awords;
            String[] charArray;

            try
            {

                charArray = new String[] { "GO--" };
                if (GlobalSettings.RunningUnderSQLServer)
                {//cargar procedimiento SQL
                    //ruta = TMultiFunctions.ExtractFilePath(TMultiFunctions.ParamStr(0)) + "\\SQLs\\SQLServer\\";
                    ruta = Directory.GetCurrentDirectory() + "\\SQLs\\CL\\SQLServer\\";
                    DirectoryInfo oDirectorio = new DirectoryInfo(ruta);

                    //obtengo ls ficheros contenidos en la ruta
                    foreach (FileInfo file in oDirectorio.GetFiles())
                    {
                        try
                        {
                            if (file.Extension == ".sql")
                            {
                                SQLFile = file.FullName;
                                sr = new System.IO.StreamReader(SQLFile, System.Text.Encoding.GetEncoding("ISO8859-1"));
                                s = sr.ReadToEnd();
                                sr.Close();
                                if (s != "")
                                {
                                    awords = s.Split(charArray, StringSplitOptions.None);
                                    foreach (String aword in awords)
                                    {
                                        oRecordSet.DoQuery(aword);
                                    }
                                    FSBOApp.StatusBar.SetText("Cargado exitosamente, " + file.Name, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            FSBOApp.StatusBar.SetText(ex.Message + " ** Trace: " + ex.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            OutLog("CargarProcedimientos: " + ex.Message + " ** Trace: " + ex.StackTrace);
                        }

                    }

                }
                else
                {//cargar procedimiento HANA
                    ruta = Directory.GetCurrentDirectory() + "\\SQLs\\CL\\HANA\\";
                    DirectoryInfo oDirectorio = new DirectoryInfo(ruta);

                    //obtengo ls ficheros contenidos en la ruta
                    foreach (FileInfo file in oDirectorio.GetFiles())
                    {
                        try
                        {
                            if (file.Extension == ".sql")
                            {
                                SQLFile = file.FullName;
                                sr = new System.IO.StreamReader(SQLFile, System.Text.Encoding.GetEncoding("ISO8859-1"));
                                s = sr.ReadToEnd();
                                sr.Close();
                                if (s != "")
                                {
                                    //OutLog(s);
                                    awords = s.Split(charArray, StringSplitOptions.None);
                                    foreach (String aword in awords)
                                    {
                                        try
                                        {
                                            //OutLog(aword.Replace("GO--", ""));
                                            oRecordSet.DoQuery(aword.Replace("GO--", ""));
                                            FSBOApp.StatusBar.SetText("Cargado exitosamente, " + file.Name, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                        }
                                        catch (Exception ej1)
                                        {
                                            //FSBOApp.StatusBar.SetText(ej1.Message + " ** Trace: " + ej1.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                            OutLog("CargarProcedimientos :" + SQLFile + " - " + ej1.Message + " ** Trace: " + ej1.StackTrace);
                                        }
                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            FSBOApp.StatusBar.SetText(ex.Message + " ** Trace: " + ex.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            OutLog("CargarProcedimientos: " + ex.Message + " ** Trace: " + ex.StackTrace);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CargarProcedimientos: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }


        private void CargarProcedimientosPortal()
        {
            String SQLFile;
            System.IO.StreamReader sr;
            String ruta;
            System.String[] awords;
            String[] charArray;
            SqlConnection ConexionADO;
            String sUser;
            String sPass;
            TFunctions Param;
            String sCnn;
            SqlCommand cmd;
            Boolean bMultiSoc;

            try
            {
                if (((System.String)oDBDSHeader.GetValue("U_MultiSoc", 0)).Trim() == "Y")
                    bMultiSoc = true;
                else
                    bMultiSoc = false;

                charArray = new String[] { "GO--" };
                //cargar procedimiento SQL
                //ruta = TMultiFunctions.ExtractFilePath(TMultiFunctions.ParamStr(0)) + "\\SQLs\\SQLServer\\";
                ruta = Directory.GetCurrentDirectory() + "\\SQLs\\CL\\Portal\\";
                DirectoryInfo oDirectorio = new DirectoryInfo(ruta);


                if (bMultiSoc)
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select U_Servidor, U_Base, U_Usuario, U_Password from [@VID_FEMULTISOC] where U_Habilitada = 'Y'";
                    else
                        s = @"select ""U_Servidor"", ""U_Base"", ""U_Usuario"", ""U_Password"" from ""@VID_FEMULTISOC"" where ""U_Habilitada"" = 'Y'";
                }
                else
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                    {
                        s = @"Select U_Servidor
                                           ,U_Base
                                           ,U_Usuario
                                           ,U_Password
                                      from [@VID_FEPARAM] ";
                    }
                    else
                    {
                        s = @"Select ""U_Servidor""
                                           ,""U_Base""
                                           ,""U_Usuario""
                                           ,""U_Password""
                                      from ""@VID_FEPARAM"" ";
                    }
                }
                oRecordSet.DoQuery(s);
                Param = new TFunctions();
                Param.SBO_f = FSBOf;


                while (!oRecordSet.EoF)
                {
                    if (bMultiSoc)
                    {
                        sUser = (System.String)(oRecordSet.Fields.Item("U_Usuario").Value).ToString().Trim();
                        sPass = (System.String)(oRecordSet.Fields.Item("U_Password").Value).ToString().Trim();
                    }
                    else
                    {
                        sUser = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Usuario").Value).ToString().Trim());
                        sPass = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Password").Value).ToString().Trim());
                    }
                    sCnn = Param.sConexion((System.String)(oRecordSet.Fields.Item("U_Servidor").Value), (System.String)(oRecordSet.Fields.Item("U_Base").Value), sUser, sPass);
                    if (sCnn.Substring(0, 1) != "E")
                    {
                        //obtengo ls ficheros contenidos en la ruta
                        foreach (FileInfo file in oDirectorio.GetFiles())
                        {
                            try
                            {
                                if (file.Extension == ".sql")
                                {
                                    ConexionADO = new SqlConnection(sCnn);
                                    if (ConexionADO.State == ConnectionState.Closed) ConexionADO.Open();

                                    SQLFile = file.FullName;
                                    sr = new System.IO.StreamReader(SQLFile, System.Text.Encoding.GetEncoding("ISO8859-1"));
                                    s = sr.ReadToEnd();
                                    sr.Close();
                                    if (s != "")
                                    {
                                        awords = s.Split(charArray, StringSplitOptions.None);
                                        foreach (String aword in awords)
                                        {
                                            cmd = new SqlCommand(aword);
                                            cmd.Connection = ConexionADO;
                                            cmd.ExecuteNonQuery();
                                        }
                                        FSBOApp.StatusBar.SetText("Cargado exitosamente " + (System.String)(oRecordSet.Fields.Item("U_Base").Value) + ", " + file.Name, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                FSBOApp.StatusBar.SetText((System.String)(oRecordSet.Fields.Item("U_Base").Value) + " - " + ex.Message + " ** Trace: " + ex.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                OutLog("CargarProcedimientosPortal " + (System.String)(oRecordSet.Fields.Item("U_Base").Value) + ": " + ex.Message + " ** Trace: " + ex.StackTrace);
                            }

                        }
                    }
                    else
                        FSBOApp.StatusBar.SetText("Debe ingresar datos de conexion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);

                    oRecordSet.MoveNext();
                }
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CargarProcedimientosPortal: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }


    }//fin class
}
