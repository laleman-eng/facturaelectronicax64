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

namespace Factura_Electronica_VK.MultiplesBases
{
    class TMultiplesBases : TvkBaseForm, IvkFormInterface
    {
        public static List<string> Lista;
        public static SAPbouiCOM.DBDataSource oDBDSH;
        public static SAPbouiCOM.DBDataSource oDBDSBases;
        public static SAPbobsCOM.Recordset oRecordSet;
        public static SAPbouiCOM.Matrix oMtx;
        public static SAPbouiCOM.Form oForm;
        public static SAPbouiCOM.Column oColumn;
        public static SAPbouiCOM.EditText oEditText;
        public static CultureInfo _nf = new CultureInfo("en-US");
        public static String s;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            Int32 i;
            TFunctions Param;

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                Lista = new List<string>();

                FSBOf.LoadForm(xmlPath, "VID_FEMULTISOC.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = true;
                oForm.SupportedModes = -1;             // afm_All

                VID_DelRow = false;
                VID_DelRowOK = false;

                Param = new TFunctions();
                Param.SBO_f = FSBOf;
   
                //oForm.DataBrowser.BrowseBy := "DocEntry"; 
                oDBDSH = oForm.DataSources.DBDataSources.Item("@VID_FEMULTISOC");
                //oDBDSBases := oForm.DataSources.DBDataSources.Item("@VID_FEMULTISOC");

                                            // Ok Ad  Fnd Vw Rq Sec
                //        Lista.Add('DocEntry  , f,  f,  t,  f, r, 1');
                //        Lista.Add('Desde     , f,  f,  f,  f, r, 1');
                //        Lista.Add('mtx       , f,  t,  f,  f, n, 1');
                //        FSBOf.SetAutoManaged(var oForm, Lista);

                oMtx = (Matrix)(oForm.Items.Item("mtx").Specific);

                oMtx.AutoResizeColumns();
                //EditText(oForm.Items.Item('CardCode').Specific).Active := True;

                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"select U_Sociedad
                              ,U_RUT 
                              ,U_Servidor
                              ,U_Base
	                          ,U_Usuario
	                          ,U_Password
	                          ,DocEntry
	                          ,U_Habilitada
                          from [@VID_FEMULTISOC]"; }
                else
                {   s = @"select ""U_Sociedad""
                              ,""U_RUT""
                              ,""U_Servidor""
                              ,""U_Base""
	                          ,""U_Usuario""
	                          ,""U_Password""
	                          ,""DocEntry""
	                          ,""U_Habilitada""
                          from ""@VID_FEMULTISOC"" "; }
                oRecordSet.DoQuery(s);

                if (oRecordSet.RecordCount == 0)
                {
                    oMtx.AddRow(1,1);
                    oMtx.FlushToDataSource();
                    oMtx.AutoResizeColumns();
                }
                else
                {
                    i = 0;
                    oDBDSH.Clear();
                    while (!oRecordSet.EoF)
                    {
                        oDBDSH.InsertRecord(i);
                        oDBDSH.SetValue("U_Sociedad", i, (System.String)(oRecordSet.Fields.Item("U_Sociedad").Value));
                        oDBDSH.SetValue("U_RUT", i, (System.String)(oRecordSet.Fields.Item("U_RUT").Value));
                        oDBDSH.SetValue("U_Servidor", i , (System.String)(oRecordSet.Fields.Item("U_Servidor").Value));
                        oDBDSH.SetValue("U_Base", i , (System.String)(oRecordSet.Fields.Item("U_Base").Value));

                        oDBDSH.SetValue("U_Usuario", i , (System.String)(oRecordSet.Fields.Item("U_Usuario").Value));
                        //s = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Password").Value).ToString().Trim());
                        oDBDSH.SetValue("U_Password", i , (System.String)(oRecordSet.Fields.Item("U_Password").Value));
                        
                        oDBDSH.SetValue("DocEntry", i , Convert.ToString((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value)));
                        oDBDSH.SetValue("U_Habilitada", i , (System.String)(oRecordSet.Fields.Item("U_Habilitada").Value));
                        i++;
                        oRecordSet.MoveNext();
                    }
                    oDBDSH.InsertRecord(i);
                    oMtx.LoadFromDataSource();
                    oMtx.AutoResizeColumns();
                }



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


        public void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            Int32 nErr;
            String sErr;
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if ((pVal.ItemUID == "1") && ((oForm.Mode == BoFormMode.fm_ADD_MODE) || (oForm.Mode == BoFormMode.fm_UPDATE_MODE)))
                    {
                        BubbleEvent = false;
                        if (ValidarMatrix()) GuardarRegistros();
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction))
                {
                    if (pVal.ItemUID == "btnProbar")
                    {
                        ProbarConexion();
                    }
                }
            }
            catch (Exception e)
            {
                FCmpny.GetLastError(out nErr,out sErr);
                FSBOApp.StatusBar.SetText("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin FormEvent


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
                    { }

                    if (pVal.MenuUID == "1282")
                    { }
                }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace,1,"Ok","","");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent


        private void GuardarRegistros()
        {
            TFunctions Reg;
            Int32 i;
            Int32 DocEntryAsig;
            String User, Pass;

            try
            {
                if (ValidarMatrix())
                {
                    Reg = new TFunctions();
                    Reg.SBO_f = FSBOf;
                    oMtx.FlushToDataSource();
                    FCmpny.StartTransaction();

                    i = 0;
                    while (i < oMtx.RowCount)
                    {
                        User = (System.String)(oDBDSH.GetValue("U_Usuario", i));

                        Pass = (System.String)(oDBDSH.GetValue("U_Password", i));
                        //s = Reg.Encriptar(Pass);
                        oDBDSH.SetValue("U_Password", i, Pass);

                        if ((System.String)(oDBDSH.GetValue("DocEntry", i)) == "")
                        {
                            DocEntryAsig = Reg.FEMultiSocAdd((System.String)(oDBDSH.GetValue("U_Servidor", i)), (System.String)(oDBDSH.GetValue("U_RUT", i)), (System.String)(oDBDSH.GetValue("U_Base", i)), (System.String)(oDBDSH.GetValue("U_Usuario", i)), (System.String)(oDBDSH.GetValue("U_Password", i)), (System.String)(oDBDSH.GetValue("U_Sociedad", i)), (System.String)(oDBDSH.GetValue("U_Habilitada", i)));
                            oDBDSH.SetValue("DocEntry", i , DocEntryAsig.ToString());
                        }
                        else
                        {
                            Reg.FEMultiSocUpt((System.String)(oDBDSH.GetValue("DocEntry", i)), (System.String)(oDBDSH.GetValue("U_Servidor", i)), (System.String)(oDBDSH.GetValue("U_RUT", i)), (System.String)(oDBDSH.GetValue("U_Base", i)), (System.String)(oDBDSH.GetValue("U_Usuario", i)), (System.String)(oDBDSH.GetValue("U_Password", i)), (System.String)(oDBDSH.GetValue("U_Sociedad", i)), (System.String)(oDBDSH.GetValue("U_Habilitada", i)));
                        }

                        //oDBDSH.SetValue("U_Password", i, Pass);
                        i++;
                    }
                    FCmpny.EndTransaction(BoWfTransOpt.wf_Commit);
                    oDBDSH.InsertRecord(i);
                    oMtx.LoadFromDataSource();
                    oMtx.AutoResizeColumns();
                    FSBOApp.StatusBar.SetText("Sociedades registradas satisfactoriamente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }
            }
            catch(Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace,1,"Ok","","");
                OutLog("GuardarRegistros: " + e.Message + " ** Trace: " + e.StackTrace);
                if (FCmpny.InTransaction) FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
            }
        }//fin GuardarRegistros


        private void ProbarConexion()
        {
            SqlConnection cnn = null;
            TFunctions Param;
            Int32 i;

            try
            {
                Param = new TFunctions();
                Param.SBO_f = FSBOf;
                i = 0;
                while (i < oMtx.RowCount)
                {
                    if (((System.String)(oDBDSH.GetValue("U_Servidor",i)) != "") 
                        && ((System.String)(oDBDSH.GetValue("U_Base",i)) != "")
                        && ((System.String)(oDBDSH.GetValue("U_Usuario",i)) != "")
                        && ((System.String)(oDBDSH.GetValue("U_Password",i)) != ""))
                    {
                        s = Param.sConexion((System.String)(oDBDSH.GetValue("U_Servidor",i)), (System.String)(oDBDSH.GetValue("U_Base",i)), (System.String)(oDBDSH.GetValue("U_Usuario",i)), (System.String)(oDBDSH.GetValue("U_Password",i)));
                        if (s.Substring(0,1) != "E")
                        {
                            try
                            {
                                cnn = new SqlConnection(s);
                                cnn.Open();
                                FSBOApp.StatusBar.SetText("Cadena de conexión válida, base " + (System.String)(oDBDSH.GetValue("U_Base",i)), SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                            }
                            catch(Exception e)
                            {
                                FSBOApp.StatusBar.SetText("ingrese cadena de conexión válida, base " + (System.String)(oDBDSH.GetValue("U_Base",i)), SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                OutLog("Base " + (System.String)(oDBDSH.GetValue("U_Base",i)) + ",TestConexion(1) : " + e.Message + " ** Trace: " + e.StackTrace);
                            }
                            finally
                            {
                                if (cnn.State == System.Data.ConnectionState.Open) cnn.Close();
                            }
                        }
                        else
                        {
                            FSBOApp.StatusBar.SetText(s.Substring(1, s.Length-1), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        }
                    }
                    i++;
                }
            }
            catch (Exception e)
            {
                OutLog("TestConexion : " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.StatusBar.SetText("TestConexion : " + e.Message + " ** Trace: " + e.StackTrace , BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }//fin ProbarConexion


        private Boolean ValidarMatrix()
        {
            Boolean _result;
            Int32 Desde;
            Int32 Hasta;
            Int32 i;
            Int32 TCantAsig;
            Int32 CantAsig;

            try
            {
                _result = true;
                oMtx.FlushToDataSource();
                if (LimpiarMatrix())
                {
                    oMtx.FlushToDataSource();
                    i = 0;
                    while (i < oMtx.RowCount)
                    {
                        if (((System.String)(oDBDSH.GetValue("U_Servidor", i)) == "")
                            || ((System.String)(oDBDSH.GetValue("U_Base", i)) == "")
                            || ((System.String)(oDBDSH.GetValue("U_Usuario", i)) == "")
                            || ((System.String)(oDBDSH.GetValue("U_Password", i)) == "")
                            || ((System.String)(oDBDSH.GetValue("U_RUT", i)) == "")
                            || ((System.String)(oDBDSH.GetValue("U_Sociedad", i)) == ""))
                        {
                            FSBOApp.StatusBar.SetText("Linea " + Convert.ToString(i+1) + " debe ingresar todos los parametros", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                            i = oMtx.RowCount;
                        }
                        i++;
                    }
                }
                else
                {
                    _result = false;
                }

                return _result;
            }
            catch (Exception e)
            {
                OutLog("ValidarMatrix: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                return false;
            }
        }//fin ValidarMatrix



        private Boolean LimpiarMatrix()
        {
            Int32 i;

            try
            {
                i = 0;
                while (i < oMtx.RowCount)
                {
                    if (((System.String)(oDBDSH.GetValue("U_Servidor", i)) == "")
                        && ((System.String)(oDBDSH.GetValue("U_Base", i)) == "")
                        && ((System.String)(oDBDSH.GetValue("U_Usuario", i)) == "")
                        && ((System.String)(oDBDSH.GetValue("U_RUT", i)) == "")
                        && ((System.String)(oDBDSH.GetValue("U_Password", i)) == "")
                        && ((System.String)(oDBDSH.GetValue("U_Sociedad", i)) == ""))
                    {
                        oMtx.DeleteRow(i+1);
                        i = i - 1;
                    }
                    i++;
                }

                return true;
            }
            catch (Exception e)
            {
                OutLog("LimpiarMatrix: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                return false;
            }
        }//fin LimpiarMatrix


    }//fin Class
}
