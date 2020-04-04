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
using System.Data.SqlClient;
using VisualD.vkFormInterface;
using Factura_Electronica_VK.Functions;

namespace Factura_Electronica_VK.DistribucionFolios
{
    class TDistribucionFolios : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbouiCOM.DBDataSource oDBDSH;
        private SAPbouiCOM.DBDataSource oDBDSD;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Matrix oMtx;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.Column oColumn;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;


        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            //SAPbouiCOM.ComboBox oComboBox;
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                Lista = new List<string>();

                FSBOf.LoadForm(xmlPath, "VID_FEDIST.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = true;
                oForm.SupportedModes = -1;             // afm_All

                VID_DelRow = false;
                VID_DelRowOK = false;
   
                oForm.DataBrowser.BrowseBy = "DocEntry"; 
                oDBDSH = oForm.DataSources.DBDataSources.Item("@VID_FEDIST");
                oDBDSD = oForm.DataSources.DBDataSources.Item("@VID_FEDISTD");

                oForm.EnableMenu("1282", false); // boton Crear

                                    // Ok Ad  Fnd Vw Rq Sec
                Lista.Add("DocEntry  , f,  f,  f,  f, r, 1");
                Lista.Add("Desde     , f,  t,  f,  f, r, 1");
                Lista.Add("Hasta     , f,  t,  f,  f, r, 1");
                Lista.Add("Hasta     , f,  t,  f,  f, r, 1");
                Lista.Add("Sucursal  , f,  t,  f,  f, r, 1");
                Lista.Add("RangoF    , f,  t,  t,  f, r, 1");
                Lista.Add("TipoDoc   , f,  t,  t,  f, r, 1");
                Lista.Add("btnDist   , f,  t,  f,  f, n, 1");
                FSBOf.SetAutoManaged(oForm, Lista);

                //carga tipo documentos
                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"select distinct U_TipoDoc 'Code', U_TipoDoc 'Name' from [@VID_FECAF]"; }
                else
                {   s = @"select distinct ""U_TipoDoc"" ""Code"", ""U_TipoDoc"" ""Name"" from ""@VID_FECAF"" "; }
                oRecordSet.DoQuery(s);
                FSBOf.FillCombo((ComboBox)(oForm.Items.Item("TipoDoc").Specific), ref oRecordSet, true);

                //carga sucursales
                if (GlobalSettings.RunningUnderSQLServer)
                {   s = "select Code, Name from [@VID_FESUC]"; }
                else
                {   s = @"select ""Code"", ""Name"" from ""@VID_FESUC"" "; }
                oRecordSet.DoQuery(s);
                FSBOf.FillCombo((ComboBox)(oForm.Items.Item("Sucursal").Specific), ref oRecordSet, true);

                //        AddChooseFromList();
                oMtx = (Matrix)(oForm.Items.Item("mtx").Specific);
                //        oColumn                    := SAPbouiCOM.Column(oMtx.Columns.Item('V_0')); 
                //        oColumn.ChooseFromListUID  := 'CFL0';
                //        oColumn.ChooseFromListAlias:= 'Code'; 

                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"select C1.FldValue 'Code', C1.Descr 'Name'
                           from CUFD C0
                           join UFD1 C1 on C1.TableID = C0.TableID
                                       and C1.FieldID = C0.FieldID
                          where C0.TableID = '@VID_FEDISTD'
                            and C0.AliasID = 'Estado'"; }
                else
                {   s = @"select C1.""FldValue"" ""Code"", C1.""Descr"" ""Name""
                           from ""CUFD"" C0
                           join ""UFD1"" C1 on C1.""TableID"" = C0.""TableID""
                                       and C1.""FieldID"" = C0.""FieldID""
                          where C0.""TableID"" = '@VID_FEDISTD'
                            and C0.""AliasID"" = 'Estado' "; }
                oRecordSet.DoQuery(s);
                oColumn = (SAPbouiCOM.Column)(oMtx.Columns.Item("Estado")); 
                FSBOf.FillComboMtx(oColumn, ref oRecordSet, false);

                oDBDSD.InsertRecord(0);
                oMtx.LoadFromDataSource();
                oMtx.AutoResizeColumns();

                //EditText(oForm.Items.Item('CardCode').Specific).Active := True;
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
            SAPbouiCOM.ComboBox oComboBox;
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);

            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if ((pVal.ItemUID == "1") && (oForm.Mode == BoFormMode.fm_ADD_MODE))
                    {
                        BubbleEvent = false;
                        GuardarRegistros();
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (!pVal.BeforeAction))
                {
                    if ((pVal.ItemUID == "TipoDoc") && (oForm.Mode == BoFormMode.fm_ADD_MODE))
                    {
                        //carga Rango de Folios
                        if (GlobalSettings.RunningUnderSQLServer)
                        {   s = @"select Code 'Code', CAST(U_Desde as varchar(20)) + '-' + CAST(U_Hasta as varchar(20))  'Name' 
                                    from [@VID_FECAF]
                                    where U_TipoDoc = '{0}'"; }
                        else
                        {   s = @"select ""Code"" ""Code"", CAST(""U_Desde"" as varchar(20)) + '-' + CAST(""U_Hasta"" as varchar(20))  ""Name""
                                    from ""@VID_FECAF""
                                   where ""U_TipoDoc"" = '{0}' "; }
                        oComboBox = (ComboBox)(oForm.Items.Item("TipoDoc").Specific);
                        s = String.Format(s, (System.String)(oComboBox.Value).Trim());
                        oRecordSet.DoQuery(s);
                        FSBOf.FillCombo((ComboBox)(oForm.Items.Item("RangoF").Specific), ref oRecordSet, true);
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction) && (pVal.ItemUID == "btnDist") && (oForm.Mode == BoFormMode.fm_ADD_MODE))
                {
                    if (Validar()) Distribuir();
                }
            }
            catch (Exception e)
            {
                FCmpny.GetLastError(out nErr, out sErr);
                FSBOApp.StatusBar.SetText("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin FormEvent


        private void Distribuir()
        {
            Int64 iFolio;
            Int64 iDesde;
            Int64 iHasta;
            Int32 i;
            SAPbouiCOM.EditText oEditText;

            try
            {
                oMtx.Clear();
                oMtx.FlushToDataSource();
                oEditText = (EditText)(oForm.Items.Item("Desde").Specific);
                iDesde = Convert.ToInt64((System.String)(oEditText.Value), _nf);
                oEditText = (EditText)(oForm.Items.Item("Hasta").Specific);
                iHasta = Convert.ToInt64((System.String)(oEditText.Value), _nf);
                if (iDesde <= 0)
                {   FSBOApp.StatusBar.SetText("Debe ingresar Folio desde", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
                else if (iHasta <= 0)
                {   FSBOApp.StatusBar.SetText("Debe ingresar Folio hasta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
                else if (iDesde >= iHasta)
                {   FSBOApp.StatusBar.SetText("Ingrese rango Folio correcto", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
                else
                {
                    i = 0;
                    for (iFolio = iDesde; iFolio <= iHasta; iFolio++)
                    {
                        oDBDSD.InsertRecord(i);
                        oDBDSD.SetValue("U_Folio", i, Convert.ToString(iFolio));
                        oDBDSD.SetValue("U_Estado", i, "D");
                        i++;
                    }

                    if (Convert.ToDouble((System.String)(oDBDSD.GetValue("U_Folio", i)), _nf) == 0)
                    {   oDBDSD.RemoveRecord(i); }

                    oMtx.LoadFromDataSource();
                    oColumn = oMtx.Columns.Item("Folio");
                    oColumn.TitleObject.Sortable = true;
                    oColumn.TitleObject.Sort(SAPbouiCOM.BoGridSortType.gst_Ascending);
                }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace,1,"Ok","","");
                OutLog("Distribuir: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin Distribuir


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
            Int32 _return;

            try
            {
                if (Validar())
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                    {   s = "select count(*) 'cont' from [@VID_FEDIST] where DocEntry = '{0}'"; }
                    else
                    {   s = @"select COUNT(*) ""cont"" from ""@VID_FEDIST"" where ""DocEntry"" = '{0}' "; }
                    s = String.Format(s, (System.String)(oDBDSH.GetValue("DocEntry",0)));
                    oRecordSet.DoQuery(s);
                    Reg = new TFunctions();
                    Reg.SBO_f = FSBOf;
                    oMtx.FlushToDataSource();
                    if ((System.Int32)(oRecordSet.Fields.Item("cont").Value) == 0)
                    {   _return = Reg.FEDistAdd(oDBDSH, oDBDSD); }
                    else
                    {   _return = Reg.FEDistUpt(oDBDSH, oDBDSD); }

                    if (_return > 0)
                    {
                        oDBDSH.SetValue("DocEntry", 0, Convert.ToString(_return));
                        oForm.Mode = BoFormMode.fm_OK_MODE;
                    }
                }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace,1,"Ok","","");
                OutLog("GuardarRegistros: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin GuardarRegistros


        private Boolean Validar()
        {
            Boolean _result;
            try
            {
                _result = true;
                if ((System.String)(oDBDSH.GetValue("U_TipoDoc", 0)) == "")
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar Tipo Documento electronico", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    _result = false;
                }
                else if ((System.String)(oDBDSH.GetValue("U_RangoF", 0)) == "")
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar CAF que se usara en la distribucion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    _result = false;
                }
                else if ((System.String)(oDBDSH.GetValue("U_Sucursal", 0)) == "")
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar Sucursal", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    _result = false;
                }
                else if (oMtx.RowCount == 0)
                {
                    FSBOApp.StatusBar.SetText("Debe distribuir los folios", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    _result = false;
                }
                else if ((System.String)(oDBDSH.GetValue("U_Desde", 0)) == "")
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar Folio inicial a distribuir", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    _result = false;
                }
                else if ((System.String)(oDBDSH.GetValue("U_Hasta", 0)) == "")
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar Folio final a distribuir", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    _result = false;
                }
                else if (Convert.ToInt32((System.String)(oDBDSH.GetValue("U_Desde", 0))) > Convert.ToInt32((System.String)(oDBDSH.GetValue("U_Hasta", 0))))
                {
                    FSBOApp.StatusBar.SetText("Ingrese rango folios valido", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    _result = false;
                }
                else 
                {
                    //valido que rango ingresado sea valido para el CAF
                    if (GlobalSettings.RunningUnderSQLServer)
                    {   s = @"select T0.Code,T0.U_Desde, T0.U_Hasta, T0.U_TipoDoc
                              from [@VID_FECAF] T0
                             where T0.U_TipoDoc = '{2}'
                               and T0.Code = '{3}'
                               and (({0} < T0.U_Desde) 
                                or ({1} > T0.U_Hasta))"; }
                    else
                    {   s = @"select T0.""Code"",T0.""U_Desde"", T0.""U_Hasta"", T0.""U_TipoDoc""
                                  from ""@VID_FECAF"" T0
                                 where T0.""U_TipoDoc"" = '{2}'
                                   and T0.""Code"" = '{3}'
                                   and (({0} < T0.""U_Desde"")
                                    or ({1} > T0.""U_Hasta"")) "; }
                    s = String.Format(s, (System.String)(oDBDSH.GetValue("U_Desde", 0)), (System.String)(oDBDSH.GetValue("U_Hasta", 0)), (System.String)(oDBDSH.GetValue("U_TipoDoc", 0)), (System.String)(oDBDSH.GetValue("U_RangoF", 0)));
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount > 0)
                    {
                    FSBOApp.StatusBar.SetText("El rango de folios ingresado no es valido para el CAF", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    _result = false;
                    }

                    if (_result == true)
                    {
                        //Valido que el rango seleccionado no este duplicado en la tabla de distribucion
                        if (GlobalSettings.RunningUnderSQLServer)
                        {   s = @"select T0.U_RangoF,T0.U_Desde, T0.U_Hasta, T0.U_TipoDoc
                                  from [@VID_FEDIST] T0
                                 where T0.U_TipoDoc = '{2}'
                                   and T0.U_RangoF = '{3}'
                                   and (({0} between T0.U_Desde and T0.U_Hasta) 
                                    or ({1} between T0.U_Desde and T0.U_Hasta)
	                                or ({0} < T0.U_Desde and {1} > T0.U_Hasta))"; }
                        else
                        {   s = @"select T0.""U_RangoF"", T0.""U_Desde"", T0.""U_Hasta"", T0.""U_TipoDoc""
                                  from ""@VID_FEDIST"" T0
                                 where T0.""U_TipoDoc"" = '{2}'
                                   and T0.""U_RangoF"" = '{3}'
                                   and (({0} between T0.""U_Desde"" and T0.""U_Hasta"")
                                    or ({1} between T0.""U_Desde"" and T0.""U_Hasta"")
	                                or ({0} < T0.""U_Desde"" and {1} > T0.""U_Hasta"")) "; }
                        s = String.Format(s, (System.String)(oDBDSH.GetValue("U_Desde", 0)), (System.String)(oDBDSH.GetValue("U_Hasta", 0)), (System.String)(oDBDSH.GetValue("U_TipoDoc", 0)), (System.String)(oDBDSH.GetValue("U_RangoF", 0)));
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount > 0)
                        {
                            FSBOApp.StatusBar.SetText("El rango de folios se solapan", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }

                    if (_result == true)
                    {
                        var Dif = (Convert.ToInt32((System.String)(oDBDSH.GetValue("U_Hasta", 0))) - Convert.ToInt32((System.String)(oDBDSH.GetValue("U_Desde", 0)))) + 1;
                        if (Dif != oMtx.RowCount)
                        {
                            FSBOApp.StatusBar.SetText("Debe distribuir los folios", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }
                }

                return _result;
            }
            catch (Exception e)
            {
                OutLog("Validar : " + e.Message + " ** TRACE ** " + e.StackTrace);
                return false;
            }
        }//fin Validar

    }//fin Class
}
