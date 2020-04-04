using SAPbouiCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using SAPbobsCOM;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using VisualD.untLog;
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using Factura_Electronica_VK.Functions;


namespace Factura_Electronica_VK.AsignarFolios
{

    public class TAsignarFolios : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbouiCOM.DBDataSource oDBDSH;
        private SAPbouiCOM.DBDataSource oDBDSD;
        private SAPbouiCOM.DBDataSource oDBDSCAF;
        private SAPbouiCOM.DBDataSource oDBDSDISTH;
        private SAPbouiCOM.DBDataSource oDBDSDISTD;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Matrix oMtx;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.Column oColumn;
        private SAPbouiCOM.EditText oEditText;
        private Boolean bSubirSuc = false;
        //oItem       : SAPbouiCOM.Item;
        //oColumn     : SAPBouiCOM.Column;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            //SAPbouiCOM.ComboBox oCombo;
            //Int32 i;
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                Lista = new List<string>();

                FSBOf.LoadForm(xmlPath, "VID_FEASIGFOL.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = true;
                oForm.SupportedModes = -1;             // afm_All

                VID_DelRow = false;
                VID_DelRowOK = false;

                oForm.DataBrowser.BrowseBy = "DocEntry";
                oDBDSH = oForm.DataSources.DBDataSources.Item("@VID_FEASIGFOL");
                oDBDSD = oForm.DataSources.DBDataSources.Item("@VID_FEASIGFOLD");
                oDBDSCAF = oForm.DataSources.DBDataSources.Add("@VID_FECAF");
                oDBDSDISTH = oForm.DataSources.DBDataSources.Add("@VID_FEDIST");
                oDBDSDISTD = oForm.DataSources.DBDataSources.Add("@VID_FEDISTD");

                // Ok Ad  Fnd Vw Rq Sec
                Lista.Add("DocEntry  , f,  f,  t,  f, r, 1");
                Lista.Add("Desde     , f,  f,  f,  f, r, 1");
                Lista.Add("Hasta     , f,  f,  f,  f, r, 1");
                Lista.Add("DocDate   , f,  t,  f,  f, r, 1");
                Lista.Add("CAF       , f,  t,  t,  f, r, 1");
                Lista.Add("TipoDoc   , f,  f,  t,  f, r, 1");
                Lista.Add("btnAsignar, f,  t,  f,  f, n, 1");
                Lista.Add("FormaAsig , f,  t,  f,  f, n, 1");
                Lista.Add("CantAAsig , f,  t,  f,  f, n, 1");
                Lista.Add("mtx       , f,  t,  f,  f, n, 1");
                FSBOf.SetAutoManaged(oForm, Lista);

                AddChooseFromList();
                oEditText = (EditText)(oForm.Items.Item("CAF").Specific);
                oEditText.ChooseFromListUID = "CFL0";
                //oEditText.ChooseFromListAlias:= 'Code'; 
                oMtx = (Matrix)(oForm.Items.Item("mtx").Specific);
                oMtx.AutoResizeColumns();
                //EditText(oForm.Items.Item('CardCode').Specific).Active := True;
                oForm.Mode = BoFormMode.fm_ADD_MODE;


                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT ISNULL(U_SubirSuc,'N') 'Suc' FROM [@VID_FEPARAM]";
                else
                    s = @"SELECT IFNULL(""U_SubirSuc"",'N') ""Suc"" FROM ""@VID_FEPARAM"" ";

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                {
                    if (((System.String)oRecordSet.Fields.Item("Suc").Value).Trim() == "Y")
                        bSubirSuc = true;
                    else
                        bSubirSuc = false;

                }
                else
                    bSubirSuc = false;

            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            oForm.Freeze(false);

            return Result;
        }//fin initForm

        private void AddChooseFromList()
        {
            SAPbouiCOM.ChooseFromListCollection oCFLs;
            SAPbouiCOM.ChooseFromList oCFL;
            SAPbouiCOM.ChooseFromListCreationParams oCFLCreationParams;
            SAPbouiCOM.Conditions oCons;
            SAPbouiCOM.Condition oCon;

            try
            {
                oCFLs = oForm.ChooseFromLists;
                oCFLCreationParams = (ChooseFromListCreationParams)(FSBOApp.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_ChooseFromListCreationParams));

                oCFLCreationParams.MultiSelection = false;
                oCFLCreationParams.ObjectType = "VID_FECAF";
                oCFLCreationParams.UniqueID = "CFL0";
                oCFL = oCFLs.Add(oCFLCreationParams);

                oCFL.SetConditions(null);
                oCons = oCFL.GetConditions();
                oCon = oCons.Add();
                oCon.Alias = "U_Asignables";
                oCon.Operation = SAPbouiCOM.BoConditionOperation.co_GRATER_THAN;
                oCon.CondVal = "0";

                oCFL.SetConditions(oCons);
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("CFL: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin AddChooseFromList


        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            Int32 nErr;
            String sErr;
            SAPbouiCOM.DataTable oDataTable;
            String sHasta;
            Int32 iDif;
            SAPbouiCOM.IChooseFromListEvent oCFLEvento = null;
            //inherited FormEvent(FormUID,Var pVal,Var BubbleEvent);
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);

            try
            {
                if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED && pVal.BeforeAction)
                {
                    if (pVal.ItemUID == "1" && oForm.Mode == BoFormMode.fm_ADD_MODE)
                    {
                        BubbleEvent = false;
                        if (ValidarMatrix())
                            GuardarRegistros();
                    }
                }

                if (pVal.EventType == BoEventTypes.et_CHOOSE_FROM_LIST && !pVal.BeforeAction)
                {
                    if (pVal.ItemUID == "CAF")
                    {
                        oCFLEvento = (SAPbouiCOM.IChooseFromListEvent)(pVal);
                        oDataTable = oCFLEvento.SelectedObjects;
                        if (oDataTable != null)
                        {
                            var sValue = (System.String)(oDataTable.GetValue("Code", 0));
                            var sValue1 = Convert.ToString((System.Int32)(oDataTable.GetValue("U_Desde", 0)));
                            var sValue2 = Convert.ToString((System.Int32)(oDataTable.GetValue("U_Hasta", 0)));
                            var sValue3 = (System.String)(oDataTable.GetValue("U_TipoDoc", 0));
                            var sValue4 = Convert.ToString((System.Int32)(oDataTable.GetValue("U_Utilizados", 0)));
                            var sValue5 = Convert.ToString((System.Int32)(oDataTable.GetValue("U_Asignables", 0)));
                            var sValue6 = Convert.ToString((System.Int32)(oDataTable.GetValue("U_FolioDesde", 0)));

                            //EditText(oForm.Items.Item("CAF").Specific).Value := sValue;
                            //EditText(oForm.Items.Item("Desde").Specific).Value := sValue1;
                            //EditText(oForm.Items.Item("Hasta").Specific).Value := sValue2;
                            //EditText(oForm.Items.Item("TipoDoc").Specific).Value := sValue3;
                            oDBDSH.SetValue("U_CAF", 0, sValue);
                            oDBDSH.SetValue("U_Desde", 0, sValue1);
                            oDBDSH.SetValue("U_Hasta", 0, sValue2);
                            oDBDSH.SetValue("U_TipoDoc", 0, sValue3);
                            oDBDSH.SetValue("U_CantAAsig", 0, sValue5); //Convert.ToString((Convert.ToInt32(svalue2) - Convert.ToInt32(sValue1))+1));
                            oDBDSH.SetValue("U_FoliosAsig", 0, sValue4);
                            oDBDSH.SetValue("U_FoliosDisp", 0, sValue5);
                            oDBDSH.SetValue("U_FolioDesde", 0, sValue6);
                        }
                    }
                }

                if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED && !pVal.BeforeAction)
                {
                    if (pVal.ItemUID == "btnAsignar")
                    {
                        if (Validar())
                            CargarSucursal();
                    }
                }

                if (pVal.EventType == BoEventTypes.et_VALIDATE && pVal.BeforeAction)
                {
                    if (pVal.ItemUID == "mtx")
                    {
                        oMtx.FlushToDataSource();
                        OutLog((System.String)(oDBDSD.GetValue("U_Hasta", pVal.Row - 1)).Trim());
                        if (pVal.ColUID == "Desde")
                        {
                            sHasta = (System.String)(oDBDSD.GetValue("U_Hasta", pVal.Row - 1)).Trim();
                            if (sHasta == "")
                                sHasta = "0";
                            if ((System.String)(oDBDSD.GetValue("U_Desde", pVal.Row - 1)).Trim() == "")
                            {
                                FSBOApp.StatusBar.SetText("Debe ingresar inicio rango de Folios", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                BubbleEvent = false;
                            }
                            else if (Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Desde", pVal.Row - 1)).Trim()) > Convert.ToInt32(sHasta) && Convert.ToInt32(sHasta) != 0)
                            {
                                FSBOApp.StatusBar.SetText("Rango de Folios no valido", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                BubbleEvent = false;
                            }
                            else
                            {
                                if (ValidarManual(pVal.Row, pVal.ColUID))
                                {
                                    if (Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Desde", pVal.Row - 1)).Trim()) > 0 && Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Hasta", pVal.Row - 1)).Trim()) > 0)
                                    {
                                        iDif = (Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Hasta", pVal.Row - 1)).Trim()) - Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Desde", pVal.Row - 1)).Trim()));
                                        iDif = iDif + 1;
                                        oDBDSD.SetValue("U_CantAsig", pVal.Row - 1, iDif.ToString());
                                        oMtx.LoadFromDataSource();
                                        BubbleEvent = true;
                                    }
                                }
                                else
                                    BubbleEvent = false;
                            }
                        }

                        if (pVal.ColUID == "Hasta")
                        {
                            if ((System.String)(oDBDSD.GetValue("U_Hasta", pVal.Row - 1)).Trim() == "")
                            {
                                FSBOApp.StatusBar.SetText("Debe ingresar final rango de Folios", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                BubbleEvent = false;
                            }
                            else if (Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Hasta", pVal.Row - 1)).Trim()) < Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Desde", pVal.Row - 1)).Trim()))
                            {
                                FSBOApp.StatusBar.SetText("Rango de Folios no valido", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                BubbleEvent = false;
                            }
                            else
                            {
                                if (ValidarManual(pVal.Row, pVal.ColUID))
                                {
                                    if (Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Desde", pVal.Row - 1)).Trim()) > 0 && Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Hasta", pVal.Row - 1)).Trim()) > 0)
                                    {
                                        iDif = (Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Hasta", pVal.Row - 1)).Trim()) - Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Desde", pVal.Row - 1)).Trim()));
                                        iDif = iDif + 1;
                                        oDBDSD.SetValue("U_CantAsig", pVal.Row - 1, iDif.ToString());
                                        oMtx.LoadFromDataSource();
                                        BubbleEvent = true;
                                    }
                                }
                                else
                                    BubbleEvent = false;
                            }
                        }
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

        public void MenuEvent(ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
            //Reg : TRemuneraciones_MyFunctions;
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

                if (pVal.MenuUID != "" && pVal.BeforeAction == false)
                    if ((pVal.MenuUID == "1288") || (pVal.MenuUID == "1289") || (pVal.MenuUID == "1290") || (pVal.MenuUID == "1291"))
                    { }

                if (pVal.MenuUID == "1282")
                { }

                //inherited MenuEvent(Var pVal,var BubbleEvent);

            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent


        private void GuardarRegistros()
        {
            TFunctions Reg;
            Int32 _return = 0;
            Int32 ii;
            Int32 i;
            Int32 iFolio;
            Int32 Desde;
            Int32 Hasta;
            Boolean bOk;
            Int32 DocEntryAsig;
            String s;
            try
            {
                if (Validar())
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                    { s = "select count(*) 'CONT' from [@VID_FEASIGFOL] where DocEntry = '{0}'"; }
                    else
                    { s = @"select count(*) ""CONT"" from ""@VID_FEASIGFOL"" where ""DocEntry"" = '{0}' "; }

                    s = String.Format(s, ((System.String)oDBDSH.GetValue("DocEntry", 0)).Trim() == "" ? "0" : ((System.String)oDBDSH.GetValue("DocEntry", 0)).Trim());
                    oRecordSet.DoQuery(s);
                    Reg = new TFunctions();
                    Reg.SBO_f = FSBOf;
                    oMtx.FlushToDataSource();
                    FCmpny.StartTransaction();
                    bOk = true;

                    if ((System.Int32)(oRecordSet.Fields.Item("CONT").Value) == 0)
                    { DocEntryAsig = Reg.FEAsigAdd(oDBDSH, oDBDSD); }
                    else
                    { DocEntryAsig = Reg.FEAsigUpt(oDBDSH, oDBDSD); }

                    if (DocEntryAsig > 0) //registra Asignacion de Folios
                    {
                        oDBDSH.SetValue("DocEntry", 0, Convert.ToString(_return));
                        oDBDSCAF.Clear();
                        oDBDSCAF.InsertRecord(0);

                        if (GlobalSettings.RunningUnderSQLServer)
                        { s = "SELECT U_TipoDoc, CAST(U_Desde AS VARCHAR(20)) Desde, CAST(U_Hasta AS vARCHAR(20)) Hasta, U_Utilizados, U_Asignables, ISNULL(U_FolioDesde,0) U_FolioDesde, U_CAF FROM [@VID_FECAF] WHERE Code ='{0}'"; }
                        else
                        { s = @"SELECT ""U_TipoDoc"", CAST(""U_Desde"" AS VARCHAR(20)) ""Desde"", CAST(""U_Hasta"" AS VARCHAR(20)) ""Hasta"", ""U_Utilizados"", ""U_Asignables"", IFNULL(""U_FolioDesde"",0) ""U_FolioDesde"", ""U_CAF"" FROM ""@VID_FECAF"" WHERE ""Code"" ='{0}' "; }

                        s = String.Format(s, (System.String)(oDBDSH.GetValue("U_CAF", 0)).Trim());
                        oRecordSet.DoQuery(s);

                        oDBDSCAF.SetValue("Code", 0, (System.String)(oDBDSH.GetValue("U_CAF", 0)).Trim());
                        oDBDSCAF.SetValue("U_TipoDoc", 0, (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value).ToString().Trim());
                        oDBDSCAF.SetValue("U_Desde", 0, (System.String)(oRecordSet.Fields.Item("Desde").Value).ToString().Trim());
                        oDBDSCAF.SetValue("U_Hasta", 0, (System.String)(oRecordSet.Fields.Item("Hasta").Value).ToString().Trim());
                        ii = (System.Int32)(oRecordSet.Fields.Item("U_Utilizados").Value);
                        ii = ii + Convert.ToInt32((System.String)(oDBDSH.GetValue("U_CantAAsig", 0)));
                        oDBDSCAF.SetValue("U_Utilizados", 0, ii.ToString());
                        ii = (System.Int32)(oRecordSet.Fields.Item("U_Asignables").Value);
                        ii = ii - Convert.ToInt32((System.String)(oDBDSH.GetValue("U_CantAAsig", 0)));
                        oDBDSCAF.SetValue("U_Asignables", 0, ii.ToString());
                        ii = (System.Int32)(oRecordSet.Fields.Item("U_Asignables").Value);
                        ii = ii - Convert.ToInt32((System.String)(oDBDSH.GetValue("U_CantAAsig", 0)));
                        oDBDSCAF.SetValue("U_Asignables", 0, ii.ToString());

                        oDBDSCAF.SetValue("U_CAF", 0, (System.String)(oRecordSet.Fields.Item("U_CAF").Value).ToString().Trim());

                        //ii := System.Int32(oRecordSet.Fields.Item("U_FolioDesde").Value);
                        ii = Int32.Parse((System.String)(oDBDSH.GetValue("U_FolioDesde", 0)));
                        ii = ii + Int32.Parse((System.String)(oDBDSH.GetValue("U_CantAAsig", 0)));
                        oDBDSCAF.SetValue("U_FolioDesde", 0, ii.ToString());

                        if (Reg.CAFUpd(oDBDSCAF) == true) //actualiza cantidades en el CAF
                        {
                            //carga Datasource para registrar tablas Distribucion de Folios
                            ii = 0;
                            while (ii < oMtx.RowCount)
                            {
                                if (Convert.ToInt32((System.String)(oDBDSD.GetValue("U_CantAsig", ii))) != 0)
                                {
                                    oDBDSDISTH.Clear();
                                    oDBDSDISTD.Clear();
                                    oDBDSDISTH.InsertRecord(0);
                                    oDBDSDISTH.SetValue("U_TipoDoc", 0, (System.String)(oDBDSH.GetValue("U_TipoDoc", 0)).Trim());
                                    oDBDSDISTH.SetValue("U_Sucursal", 0, (System.String)(oDBDSD.GetValue("U_Sucursal", ii)).Trim());
                                    oDBDSDISTH.SetValue("U_Desde", 0, (System.String)(oDBDSD.GetValue("U_Desde", ii)).Trim());
                                    oDBDSDISTH.SetValue("U_Hasta", 0, (System.String)(oDBDSD.GetValue("U_Hasta", ii)).Trim());
                                    oDBDSDISTH.SetValue("U_RangoF", 0, (System.String)(oDBDSH.GetValue("U_CAF", 0)).Trim());
                                    //crear detalle de distribucion
                                    Desde = Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Desde", ii)));
                                    Hasta = Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Hasta", ii)));
                                    i = 0;
                                    for (iFolio = Desde; iFolio <= Hasta; iFolio++)
                                    {
                                        oDBDSDISTD.InsertRecord(i);
                                        oDBDSDISTD.SetValue("U_Folio", i, Convert.ToString(iFolio));
                                        oDBDSDISTD.SetValue("U_Estado", i, "D");
                                        i++;
                                    }

                                    _return = Reg.FEDistAdd(oDBDSDISTH, oDBDSDISTD);
                                    if (_return == 0)
                                    {
                                        FSBOApp.StatusBar.SetText("No se ha registrado la asignación de folios, registrar Distribucion, sucursal " + (System.String)(oDBDSD.GetValue("U_Sucursal", ii)).Trim(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        bOk = false;
                                        ii = oMtx.RowCount;
                                    }
                                }
                                ii++;
                            }

                            if (bOk == true)
                            {
                                //mando al portal las asignaciones creadas
                                //DocEntryAsig DocEntry 
                                if (!bSubirSuc)
                                {
                                    FCmpny.EndTransaction(BoWfTransOpt.wf_Commit);
                                    oForm.Mode = BoFormMode.fm_OK_MODE;
                                }
                                else if (EnviarPortal(DocEntryAsig))
                                {
                                    FCmpny.EndTransaction(BoWfTransOpt.wf_Commit);
                                    oForm.Mode = BoFormMode.fm_OK_MODE;
                                }
                                else
                                {
                                    FSBOApp.StatusBar.SetText("No se ha registrado la asignación de folios, no se envio al portal", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                                }
                            }
                            else
                                FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }
                        else
                        {
                            FSBOApp.StatusBar.SetText("No se ha registrado la asignación de folios, se actualizo el CAF", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }
                    }
                    else
                    {
                        FSBOApp.StatusBar.SetText("No se ha registrado la asignación de folios", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                    }
                }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("GuardarRegistros: " + e.Message + " ** Trace: " + e.StackTrace);
                if (FCmpny.InTransaction)
                    FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
            }
        }//fin GuardarRegistros

        private Boolean EnviarPortal(Int32 DocEntryAsig)
        {
            //SqlDataAdapter cmd;
            SqlConnection ConexionADO = null;
            TFunctions Reg;
            String sCnn;
            String sUser;
            String sPass;

            try
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

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                {
                    Reg = new TFunctions();
                    Reg.SBO_f = FSBOf;

                    sUser = Reg.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Usuario").Value).ToString().Trim());
                    sPass = Reg.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Password").Value).ToString().Trim());

                    sCnn = Reg.sConexion((System.String)(oRecordSet.Fields.Item("U_Servidor").Value), (System.String)(oRecordSet.Fields.Item("U_Base").Value), sUser, sPass);
                    if (sCnn.Substring(0, 1) != "E")
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"select T0.DocEntry
                                      ,T0.U_TipoDoc
                                      ,T0.U_Desde 'U_CAFDesde'
	                                  ,T0.U_Hasta 'U_CAFHasta'
	                                  ,CONVERT(CHAR(8), T0.U_DocDate, 112) 'U_CAFFecha'
	                                  ,T1.U_Sucursal 
	                                  ,T1.U_Desde
	                                  ,T1.U_Hasta
	                                  ,T1.U_CantAsig
                                  from [@VID_FEASIGFOL] T0
                                  join [@VID_FEASIGFOLD] T1 on T1.DocEntry = T0.DocEntry
                                 where T0.DocEntry = {0}";
                        }
                        else
                        {
                            s = @"select T0.""DocEntry"" ,T0.""U_TipoDoc"" ,T0.""U_Desde"" ""U_CAFDesde"" ,T0.""U_Hasta"" ""U_CAFHasta""
	                                  ,TO_NVARCHAR(T0.""U_DocDate"",'yyyymmdd') ""U_CAFFecha""
	                                  ,T1.""U_Sucursal"", T1.""U_Desde"", T1.""U_Hasta"", T1.""U_CantAsig"" 
                                  from ""@VID_FEASIGFOL"" T0 
                                  join ""@VID_FEASIGFOLD"" T1 on T1.""DocEntry"" = T0.""DocEntry"" 
                                  where T0.""DocEntry"" = {0} ";
                        };
                        s = String.Format(s, DocEntryAsig.ToString());
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount == 0)
                        {
                            FSBOApp.StatusBar.SetText("No se ha encontrado registrado la asignación de folios " + DocEntryAsig.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }
                        else
                        {
                            ConexionADO = new SqlConnection(sCnn);
                            if (ConexionADO.State == ConnectionState.Closed)
                                ConexionADO.Open();

                            while (!oRecordSet.EoF)
                            {
                                var VID_SP_FoliosSucursal = new SqlCommand("VID_SP_FoliosSucursal", ConexionADO);
                                var oParameter = new SqlParameter();
                                VID_SP_FoliosSucursal.CommandType = CommandType.StoredProcedure;
                                oParameter = VID_SP_FoliosSucursal.Parameters.Add("@DocEntry", SqlDbType.Float);
                                oParameter.Value = Convert.ToDouble(DocEntryAsig);
                                oParameter = VID_SP_FoliosSucursal.Parameters.Add("@TipoDoc", SqlDbType.VarChar, 10);
                                oParameter.Value = (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value);
                                oParameter = VID_SP_FoliosSucursal.Parameters.Add("@CAFDesde", SqlDbType.Int);
                                oParameter.Value = (System.Int32)(oRecordSet.Fields.Item("U_CAFDesde").Value);
                                oParameter = VID_SP_FoliosSucursal.Parameters.Add("@CAFHasta", SqlDbType.Int);
                                oParameter.Value = (System.Int32)(oRecordSet.Fields.Item("U_CAFHasta").Value);
                                oParameter = VID_SP_FoliosSucursal.Parameters.Add("@CAFFecha", SqlDbType.VarChar, 10);
                                oParameter.Value = (System.String)(oRecordSet.Fields.Item("U_CAFFecha").Value);
                                oParameter = VID_SP_FoliosSucursal.Parameters.Add("@Sucursal", SqlDbType.VarChar, 30);
                                oParameter.Value = (System.String)(oRecordSet.Fields.Item("U_Sucursal").Value);
                                oParameter = VID_SP_FoliosSucursal.Parameters.Add("@Desde", SqlDbType.Int);
                                oParameter.Value = (System.Int32)(oRecordSet.Fields.Item("U_Desde").Value);
                                oParameter = VID_SP_FoliosSucursal.Parameters.Add("@Hasta", SqlDbType.Int);
                                oParameter.Value = (System.Int32)(oRecordSet.Fields.Item("U_Hasta").Value);
                                oParameter = VID_SP_FoliosSucursal.Parameters.Add("@CantAsig", SqlDbType.Int);
                                oParameter.Value = (System.Int32)(oRecordSet.Fields.Item("U_CantAsig").Value);

                                VID_SP_FoliosSucursal.ExecuteNonQuery();
                                VID_SP_FoliosSucursal = null;

                                oRecordSet.MoveNext();
                            }

                            if (ConexionADO.State == ConnectionState.Open)
                                ConexionADO.Close();
                            return true;
                        }
                    }
                    else
                    {
                        FSBOApp.StatusBar.SetText("Faltan datos Conexion. " + sCnn.Substring(1, sCnn.Length - 1), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        OutLog("Faltan datos Conexion");
                        return false;
                    }
                }
                else
                {
                    FSBOApp.StatusBar.SetText("Falta parametrizar datos Conexion.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    OutLog("Falta parametrizar datos Conexion.");
                    return false;
                }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("EnviarPortal: " + e.Message + " ** Trace: " + e.StackTrace);
                if (ConexionADO.State == ConnectionState.Open)
                    ConexionADO.Close();
                return false;
            }
        }//fin EnviarPortal


        private Boolean ValidarManual(Int32 iRow, String sColumna)
        {
            Boolean _result;
            Int32 i;
            Int32 ivalor;
            Int32 iCantAsig;
            Int32 iSum;
            Boolean bSeSolapa;
            String Suc;

            try
            {
                _result = true;
                bSeSolapa = false;
                Suc = "";
                if (sColumna == "Desde")
                {
                    if (Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Desde", iRow - 1))) != 0)
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"select T0.DocEntry, T1.LineId, T1.U_Folio
                                    from [@VID_FEDIST] T0
	                                join [@VID_FEDISTD] T1 on T1.DocEntry = T0.DocEntry
                                   where T0.U_TipoDoc = '{0}'
								     and T0.U_RangoF = '{1}'
	                                 and T1.U_Folio > 0
                                   order by T1.U_Folio DESC";
                        }
                        else
                        {
                            s = @"select T0.""DocEntry"", T1.""LineId"", T1.""U_Folio""
                                    from ""@VID_FEDIST"" T0 join ""@VID_FEDISTD"" T1 ON T1.""DocEntry"" = T0.""DocEntry""
                                   where T0.""U_TipoDoc"" = '{0}' and T0.""U_RangoF"" = '{1}' and T1.""U_Folio"" > 0 
                                   order by T1.""U_Folio"" DESC";
                        }
                        s = String.Format(s, (System.String)(oDBDSH.GetValue("U_TipoDoc", 0)).Trim(), (System.String)(oDBDSH.GetValue("U_CAF", 0)).Trim());
                        oRecordSet.DoQuery(s);
                        //if oRecordSet.RecordCount > 0 then
                        //    ivalor := Convert.ToInt32(System.Double(oRecordSet.Fields.Item("U_Folio").Value)) + 1
                        //else
                        //AC - lo deje asi para controlar el folio inicio en el registro del CAF 20141021 0039
                        ivalor = Convert.ToInt32((System.String)(oDBDSH.GetValue("U_FolioDesde", 0)));
                        if (Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Desde", iRow - 1))) < ivalor)
                        {
                            FSBOApp.StatusBar.SetText("Folio Inicial debe ser mayor al ultimo folio asignado", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            _result = false;
                        }
                    }
                }
                //        else if sColumna = 'Hasta' then
                //        begin
                //            ivalor := Convert.ToInt32(System.String(oDBDSH.GetValue("U_CantAAsig", 0)));
                //            if ((Convert.ToInt32(System.String(oDBDSD.GetValue("U_Hasta", i))) - Convert.ToInt32(System.String(oDBDSD.GetValue("U_Desde", i)))) + 1) > iValor then
                //            begin
                //                FSBOApp.StatusBar.SetText('Cantidad de folios asignados no coincide con el total a asignar', BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                //                _result := false;
                //            end;
                //        end;

                if (_result)
                {
                    i = 0;
                    iSum = 0;
                    iCantAsig = Convert.ToInt32((System.String)(oDBDSH.GetValue("U_CantAAsig", 0)));
                    ivalor = Convert.ToInt32((System.String)(oDBDSD.GetValue("U_" + sColumna, iRow - 1)));
                    while (i < iRow)
                    {
                        if (i != iRow - 1)
                        {
                            if ((Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Desde", i))) > 0) || (Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Desde", i))) > 0))
                            {
                                if ((ivalor >= Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Desde", i)))) && (ivalor <= Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Hasta", i)))))
                                {
                                    bSeSolapa = true;
                                    Suc = (System.String)(oDBDSD.GetValue("U_Sucursal", i));
                                    i = iRow;
                                }
                            }
                        }
                        if (Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Hasta", i))) > 0)
                            iSum = iSum + ((Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Hasta", i)).Trim()) - Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Desde", i)).Trim())) + 1);
                        i++;
                    }

                    if (_result && iSum > iCantAsig)
                    {
                        FSBOApp.StatusBar.SetText("Cantidad de folios asignados no coincide con el total a asignar", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        oDBDSD.SetValue("U_" + sColumna, iRow - 1, "0");
                        _result = false;
                    }
                    else if (_result && iSum >= iCantAsig && sColumna == "Desde" && Convert.ToInt32((System.String)(oDBDSD.GetValue("U_Hasta", iRow - 1))) == 0)
                    {
                        FSBOApp.StatusBar.SetText("Cantidad de folios asignados no coincide con el total a asignar", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        oDBDSD.SetValue("U_" + sColumna, iRow - 1, "0");
                        oMtx.LoadFromDataSource();
                        _result = false;
                    }

                    if (_result && bSeSolapa)
                    {
                        FSBOApp.StatusBar.SetText("Folio " + sColumna + " se solapa en " + Suc, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        _result = false;
                    }
                }

                return _result;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText("ValidarManual: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                return false;
            }
        }//fin ValidarManual


        private Boolean ValidarMatrix()
        {
            Boolean _result;
            //Int32 Desde;
            //Int32 Hasta;
            Int32 i;
            Int32 TCantAsig;
            Int32 CantAsig;

            try
            {
                _result = true;
                i = 0;
                oMtx.FlushToDataSource();
                TCantAsig = 0;
                while (i < oMtx.RowCount)
                {
                    if (((System.String)(oDBDSD.GetValue("U_Hasta", i)) == "0") && ((System.String)(oDBDSD.GetValue("U_Desde", i)) != "0") && (((System.String)(oDBDSD.GetValue("U_CantAsig", i)) == "0") || ((System.String)(oDBDSD.GetValue("U_CantAsig", i)) == "")))
                        oDBDSD.SetValue("U_Desde", i, "0");

                    s = (System.String)(oDBDSD.GetValue("U_CantAsig", i));
                    if (s == "")
                        s = "0";
                    CantAsig = Convert.ToInt32(s);
                    TCantAsig = TCantAsig + CantAsig;
                    i++;
                }
                oMtx.LoadFromDataSource();

                s = (System.String)(oDBDSH.GetValue("U_CantAAsig", 0));
                if (s == "")
                    s = "0";
                CantAsig = Convert.ToInt32(s);
                if (TCantAsig != CantAsig)
                {
                    FSBOApp.StatusBar.SetText("Cantidad de folios asignados no coincide con el total a asignar", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
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


        private Boolean Validar()
        {
            Boolean _result;
            Int32 Desde = 0;
            Int32 Hasta = 0;
            SqlConnection ConexionADO;
            SqlDataAdapter cmd1;
            System.Data.DataTable resultDataTable;
            String sCnn;
            TFunctions Reg;
            String sUser;
            String sPass;

            _result = true;
            if ((System.String)(oDBDSH.GetValue("U_TipoDoc", 0)) == "")
            {
                FSBOApp.StatusBar.SetText("Debe ingresar Tipo Documento electronico", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                _result = false;
            }
            else if ((System.String)(oDBDSH.GetValue("U_CAF", 0)).Trim() == "")
            {
                FSBOApp.StatusBar.SetText("Debe ingresar CAF que se usara en la asignacion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                _result = false;
            }
            else if ((System.String)(oDBDSH.GetValue("U_Desde", 0)) == "")
            {
                FSBOApp.StatusBar.SetText("Debe ingresar Folio inicial a asignar", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                _result = false;
            }
            else if ((System.String)(oDBDSH.GetValue("U_Hasta", 0)) == "")
            {
                FSBOApp.StatusBar.SetText("Debe ingresar Folio final a asignar", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                _result = false;
            }
            else if (((System.String)(oDBDSH.GetValue("U_CantAAsig", 0)) == "") || ((System.String)(oDBDSH.GetValue("U_CantAAsig", 0)) == "0"))
            {
                FSBOApp.StatusBar.SetText("Debe ingresar cantidad de folios a asignar", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                _result = false;
            }
            else if (Convert.ToInt32((System.String)(oDBDSH.GetValue("U_Desde", 0))) > Convert.ToInt32((System.String)(oDBDSH.GetValue("U_Hasta", 0))))
            {
                FSBOApp.StatusBar.SetText("Ingrese rango folios valido", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                _result = false;
            }
            //else if Convert.ToInt32(System.String(oDBDSH.GetValue("U_CantAAsig", 0)))  > (Convert.ToInt32(System.String(oDBDSH.GetValue("U_Hasta", 0))) - Convert.ToInt32(System.String(oDBDSH.GetValue("U_Desde", 0)))+1)  then
            else if (Convert.ToInt32((System.String)(oDBDSH.GetValue("U_CantAAsig", 0))) > Convert.ToInt32((System.String)(oDBDSH.GetValue("U_FoliosDisp", 0))))
            {
                FSBOApp.StatusBar.SetText("Cantidad a asignar es mayor rango del CAF", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                _result = false;
            }
            else if ((System.String)(oDBDSH.GetValue("U_DocDate", 0)) == "")
            {
                FSBOApp.StatusBar.SetText("Debe ingresar fecha asignación", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                _result = false;
            }
            else
            {
                //valido que rango ingresado sea valido para el CAF
                if (GlobalSettings.RunningUnderSQLServer)
                {
                    s = @"select T0.Code,T0.U_Desde, T0.U_Hasta, T0.U_TipoDoc
                          from [@VID_FECAF] T0
                         where T0.U_TipoDoc = '{2}'
                           and T0.Code = '{3}'
                           and (({0} < T0.U_Desde) 
                            or ({1} > T0.U_Hasta))";
                }
                else
                {
                    s = @"select T0.""Code"", T0.""U_Desde"", T0.""U_Hasta"", T0.""U_TipoDoc""
                          from ""@VID_FECAF"" T0 
                         where T0.""U_TipoDoc"" = '{2}'
                           and T0.""Code"" = '{3}'
                           and (({0} < T0.""U_Desde"")  
                            or ({1} > T0.""U_Hasta"")) ";
                }
                s = String.Format(s, (System.String)(oDBDSH.GetValue("U_Desde", 0)), (System.String)(oDBDSH.GetValue("U_Hasta", 0)), (System.String)(oDBDSH.GetValue("U_TipoDoc", 0)), (System.String)(oDBDSH.GetValue("U_CAF", 0)));
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
                    {
                        s = @"select T0.DocEntry, T1.LineId, T1.U_Folio
                                    from [@VID_FEDIST] T0
	                                join [@VID_FEDISTD] T1 on T1.DocEntry = T0.DocEntry
                                   where T0.U_TipoDoc = '{0}'
								     and T0.U_RangoF = '{1}'
	                                 and T1.U_Folio > 0
                                   order by T1.U_Folio DESC";
                    }
                    else
                    {
                        s = @"select T0.""DocEntry"", T1.""LineId"", T1.""U_Folio"" 
                                from ""@VID_FEDIST"" T0 
	                            join ""@VID_FEDISTD"" T1 on T1.""DocEntry"" = T0.""DocEntry"" 
                               where T0.""U_TipoDoc"" = '{0}' 
						   		 and T0.""U_RangoF"" = '{1}' 
	                             and T1.""U_Folio"" > 0 
                               order by T1.""U_Folio"" DESC ";
                    }
                    s = String.Format(s, (System.String)(oDBDSH.GetValue("U_TipoDoc", 0)).Trim(), (System.String)(oDBDSH.GetValue("U_CAF", 0)).Trim());
                    oRecordSet.DoQuery(s);
                    //            if oRecordSet.RecordCount > 0 then
                    //                Desde := Convert.ToInt32(System.Double(oRecordSet.Fields.Item("U_Folio").Value)) + 1
                    //            else
                    //AC - lo deje asi para controlar el folio inicio en el registro del CAF 20141021 0039
                    Desde = Convert.ToInt32((System.String)(oDBDSH.GetValue("U_FolioDesde", 0)));
                    Hasta = (Desde + Convert.ToInt32((System.String)(oDBDSH.GetValue("U_CantAAsig", 0)))) - 1;

                    if (GlobalSettings.RunningUnderSQLServer)
                    {
                        s = @"select T0.U_RangoF,T0.U_Desde, T0.U_Hasta, T0.U_TipoDoc
                                  from [@VID_FEDIST] T0
                                 where T0.U_TipoDoc = '{2}'
                                   and T0.U_RangoF = '{3}'
                                   and (({0} between T0.U_Desde and T0.U_Hasta) 
                                    or ({1} between T0.U_Desde and T0.U_Hasta)
	                                or ({0} < T0.U_Desde and {1} > T0.U_Hasta))";
                    }
                    else
                    {
                        s = @"select T0.""U_RangoF"", T0.""U_Desde"", T0.""U_Hasta"", T0.""U_TipoDoc""
                              from ""@VID_FEDIST"" T0 
                             where T0.""U_TipoDoc"" = '{2}' 
                               and T0.""U_RangoF"" = '{3}' 
                               and (({0} between T0.""U_Desde"" and T0.""U_Hasta"") 
                                or ({1} between T0.""U_Desde"" and T0.""U_Hasta"") 
	                            or ({0} < T0.""U_Desde"" and {1} > T0.""U_Hasta"")) ";
                    }
                    s = String.Format(s, Desde.ToString(), Hasta.ToString(), (System.String)(oDBDSH.GetValue("U_TipoDoc", 0)).Trim(), (System.String)(oDBDSH.GetValue("U_CAF", 0)).Trim());
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount > 0)
                    {
                        FSBOApp.StatusBar.SetText("El rango de folios se solapan", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }
                }

                if (_result == true)
                {
                    //Valido que no este ocupado un numero de folio en el portal
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

                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount > 0)
                    {
                        Reg = new TFunctions();
                        Reg.SBO_f = FSBOf;

                        sUser = Reg.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Usuario").Value).ToString().Trim());
                        sPass = Reg.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_Password").Value).ToString().Trim());

                        sCnn = Reg.sConexion((System.String)(oRecordSet.Fields.Item("U_Servidor").Value), (System.String)(oRecordSet.Fields.Item("U_Base").Value), sUser, sPass);
                        if (sCnn.Substring(0, 1) != "E")
                        {
                            ConexionADO = new SqlConnection(sCnn);
                            if (ConexionADO.State == ConnectionState.Closed)
                                ConexionADO.Open();
                            // -----------------------------------------------------
                            s = @"WITH n(n) AS
                                (
                                SELECT 1
                                    UNION ALL
                                SELECT n + 1 
                                  FROM n WHERE n < {0} )
                                SELECT CAST(n + {1} AS VARCHAR(20))  'Valor', ISNULL(CAST(T0.CAB_FOL_DOCTO_INT AS VARCHAR(20)),'') 'CAB_FOL_DOCTO_INT'
                                  FROM n left outer join Faet_Erp_Encabezado_Doc T0 on n.n + {1} = T0.CAB_FOL_DOCTO_INT and T0.CAB_COD_TP_FACTURA = '{2}' 
                                   and T0.CAB_FOL_DOCTO_INT between {3} and {4} 
                                ORDER BY n 
                                OPTION (MAXRECURSION 0)"; // antes OPTION (MAXRECURSION {0})";

                            s = String.Format(s, (Hasta - Desde) + 1, Desde - 1, (System.String)(oDBDSH.GetValue("U_TipoDoc", 0)).Trim(), Desde, Hasta);
                            cmd1 = new SqlDataAdapter(s, ConexionADO);
                            resultDataTable = new System.Data.DataTable();
                            cmd1.Fill(resultDataTable);
                            var FolioPrimero = "";
                            var FolioUltimo = "";

                            foreach (System.Data.DataRow oRow in resultDataTable.Rows)
                            {
                                if ((oRow.Field<String>("CAB_FOL_DOCTO_INT") == "") && (FolioPrimero == ""))
                                    FolioPrimero = oRow.Field<String>("Valor");

                                if ((oRow.Field<String>("CAB_FOL_DOCTO_INT") != "") && (FolioPrimero != "") && (FolioUltimo == ""))
                                {
                                    FolioUltimo = Convert.ToString(Int32.Parse(oRow.Field<String>("CAB_FOL_DOCTO_INT")) - 1);
                                    break;
                                }
                            }

                            if ((FolioPrimero != "") && (FolioUltimo != ""))
                            {
                                if ((FolioPrimero == (System.String)(oDBDSH.GetValue("U_FolioDesde", 0)).Trim()) && (Convert.ToString(((Int32.Parse(FolioUltimo) - Int32.Parse(FolioPrimero)) + 1)) == (System.String)(oDBDSH.GetValue("U_CantAAsig", 0))))
                                { _result = true; }
                                else
                                {
                                    oDBDSH.SetValue("U_FolioDesde", 0, FolioPrimero);
                                    FSBOApp.StatusBar.SetText("Se encuentra un folio usado en el portal, use rango " + FolioPrimero + " a " + FolioUltimo, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                    //oDBDSH.SetValue("U_CantAAsig", 0, Convert.ToString(( ( Int32.Parse(FolioUltimo) - Int32.Parse(FolioPrimero)   ) + 1)));
                                    _result = false;
                                }
                            }
                            else if ((FolioPrimero != "") && (FolioUltimo == ""))
                            {
                                if ((FolioPrimero == (System.String)(oDBDSH.GetValue("U_FolioDesde", 0)).Trim()) && (Convert.ToString(((Hasta) - Int32.Parse(FolioPrimero)) + 1) == (System.String)(oDBDSH.GetValue("U_CantAAsig", 0))))
                                { _result = true; }
                                else
                                {
                                    oDBDSH.SetValue("U_FolioDesde", 0, FolioPrimero);
                                    FSBOApp.StatusBar.SetText("Se encuentra un folio usado en el portal, use rango " + FolioPrimero + " a " + Hasta.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                    //oDBDSH.SetValue("U_CantAAsig", 0, Convert.ToString(((Hasta) - Int32.Parse(FolioPrimero)) + 1));
                                    _result = false;
                                }
                            }
                            else if ((FolioPrimero == "") && (FolioUltimo != ""))
                            {
                                if ((Desde.ToString() == (System.String)(oDBDSH.GetValue("U_FolioDesde", 0)).Trim()) && (Convert.ToString(((Int32.Parse(FolioUltimo)) - Desde) + 1) == (System.String)(oDBDSH.GetValue("U_CantAAsig", 0))))
                                { _result = true; }
                                else
                                {
                                    FSBOApp.StatusBar.SetText("Se encuentra un folio usado en el portal, use rango " + Desde.ToString() + " a " + Hasta.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                    //oDBDSH.SetValue("U_CantAAsig", 0, Convert.ToString(((Int32.Parse(FolioUltimo)) - Desde) + 1));
                                    _result = false;
                                }
                            }
                        }
                        else
                        { FSBOApp.StatusBar.SetText("Faltan datos Conexion. " + sCnn.Substring(1, sCnn.Length - 1), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
                    }
                    else
                    { FSBOApp.StatusBar.SetText("Debe ingresar datos de conexion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
                }

            }

            return _result;
        }//fin Validar


        private void CargarSucursal()
        {
            Int32 i;
            String Forma;
            Int32 CantDist;
            Int32 TotSuc;
            Int32 CantxSuc;
            Int32 SumAsig;
            Int32 Desde;
            Int32 Hasta;
            //SAPbouiCOM.Column oColumn;

            try
            {
                Forma = (System.String)(oDBDSH.GetValue("U_FormaAsig", 0)).Trim();
                if (Forma == "A")
                {
                    oMtx.Columns.Item("Desde").Editable = false;
                    oMtx.Columns.Item("Hasta").Editable = false;
                }
                else
                {
                    oMtx.Columns.Item("Desde").Editable = true;
                    oMtx.Columns.Item("Hasta").Editable = true;
                }

                if (GlobalSettings.RunningUnderSQLServer)
                {
                    s = @"SELECT T0.Code, COUNT(T2.U_Estado) 'Disponible'
                          FROM [@VID_FESUC] T0
                          LEFT JOIN [@VID_FEDIST] T1 ON T1.U_Sucursal = T0.Code
                                                 AND T1.U_TipoDoc = '{0}'
                          LEFT JOIN [@VID_FEDISTD] T2 ON T2.DocEntry = T1.DocEntry
                                                 AND T2.U_Estado = 'D'
                         WHERE ISNULL(T0.U_Habilitada,'Y') = 'Y'
                         GROUP BY T0.Code";
                }
                else
                {
                    s = @"SELECT T0.""Code"", COUNT(T2.""U_Estado"") ""Disponible"" 
                          FROM ""@VID_FESUC"" T0 
                          LEFT JOIN ""@VID_FEDIST"" T1 ON T1.""U_Sucursal"" = T0.""Code"" 
                                                 AND T1.""U_TipoDoc"" = '{0}'
                          LEFT JOIN ""@VID_FEDISTD"" T2 ON T2.""DocEntry"" = T1.""DocEntry"" 
                                                 AND T2.""U_Estado"" = 'D'
                         WHERE IFNULL(T0.""U_Habilitada"",'Y') = 'Y'
                         GROUP BY T0.""Code"" ";
                }
                s = String.Format(s, (System.String)(oDBDSH.GetValue("U_TipoDoc", 0)).Trim());
                oRecordSet.DoQuery(s);
                i = 0;
                oDBDSD.Clear();
                while (!oRecordSet.EoF)
                {
                    oDBDSD.InsertRecord(i);
                    oDBDSD.SetValue("U_Sucursal", i, (System.String)(oRecordSet.Fields.Item("Code").Value));
                    oDBDSD.SetValue("U_CantDisp", i, Convert.ToString((System.Int32)(oRecordSet.Fields.Item("Disponible").Value)));
                    oDBDSD.SetValue("U_Desde", i, "0");
                    oDBDSD.SetValue("U_Hasta", i, "0");
                    oDBDSD.SetValue("U_CantAsig", i, "0");

                    i++;
                    oRecordSet.MoveNext();
                }

                oMtx.LoadFromDataSource();
                oMtx.AutoResizeColumns();

                if (oMtx.RowCount > 0)
                {
                    if (Forma == "A")
                    {
                        CantDist = Convert.ToInt32((System.String)(oDBDSH.GetValue("U_CantAAsig", 0)));
                        TotSuc = oMtx.RowCount;
                        CantxSuc = Convert.ToInt32(System.Math.Round((Convert.ToDouble(CantDist) / Convert.ToDouble(TotSuc)), 0));
                        SumAsig = 0;
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"select T0.DocEntry, T1.LineId, T1.U_Folio
                                    from [@VID_FEDIST] T0
	                                join [@VID_FEDISTD] T1 on T1.DocEntry = T0.DocEntry
                                   where T0.U_TipoDoc = '{0}'
								     and T0.U_RangoF = '{1}'
	                                 and T1.U_Folio > 0
                                   order by T1.U_Folio DESC";
                        }
                        else
                        {
                            s = @"select T0.""DocEntry"", T1.""LineId"", T1.""U_Folio"" 
                                    from ""@VID_FEDIST"" T0 
	                                join ""@VID_FEDISTD"" T1 on T1.""DocEntry"" = T0.""DocEntry"" 
                                   where T0.""U_TipoDoc"" = '{0}' 
						    		 and T0.""U_RangoF"" = '{1}' 
	                                 and T1.""U_Folio"" > 0 
                                   order by T1.""U_Folio"" DESC ";
                        }
                        s = String.Format(s, (System.String)(oDBDSH.GetValue("U_TipoDoc", 0)).Trim(), (System.String)(oDBDSH.GetValue("U_CAF", 0)).Trim());
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount > 0)
                        {
                            if (Convert.ToInt32((System.Double)(oRecordSet.Fields.Item("U_Folio").Value)) + 1 > Convert.ToInt32((System.String)(oDBDSH.GetValue("U_FolioDesde", 0))))
                                Desde = Convert.ToInt32((System.Double)(oRecordSet.Fields.Item("U_Folio").Value)) + 1;
                            else
                                Desde = Convert.ToInt32((System.String)(oDBDSH.GetValue("U_FolioDesde", 0)));
                        }
                        else
                        { Desde = Convert.ToInt32((System.String)(oDBDSH.GetValue("U_FolioDesde", 0))); }

                        i = 0;
                        while (i < TotSuc)
                        {
                            oDBDSD.SetValue("U_Desde", i, Convert.ToString(Desde));

                            if (i == (TotSuc - 1))
                            {
                                Hasta = Desde + (CantDist - SumAsig) - 1;
                                oDBDSD.SetValue("U_Hasta", i, Hasta.ToString().Trim());
                                oDBDSD.SetValue("U_CantAsig", i, Convert.ToString(CantDist - SumAsig));
                                Desde = Desde + (Convert.ToInt32(CantDist) - SumAsig);
                            }
                            else
                            {
                                Hasta = Desde + CantxSuc - 1;
                                oDBDSD.SetValue("U_Hasta", i, Hasta.ToString().Trim());
                                oDBDSD.SetValue("U_CantAsig", i, Convert.ToString(CantxSuc));
                                Desde = Desde + CantxSuc;
                            }
                            SumAsig = SumAsig + CantxSuc;
                            i++;
                        }
                        oMtx.LoadFromDataSource();
                    }
                    else
                    {
                        TotSuc = oMtx.RowCount;
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"select T0.DocEntry, T1.LineId, T1.U_Folio
                                    from [@VID_FEDIST] T0
	                                join [@VID_FEDISTD] T1 on T1.DocEntry = T0.DocEntry
                                   where T0.U_TipoDoc = '{0}'
								     and T0.U_RangoF = '{1}'
	                                 and T1.U_Folio > 0
                                   order by T1.U_Folio DESC";
                        }
                        else
                        {
                            s = @"select T0.""DocEntry"", T1.""LineId"", T1.""U_Folio"" 
                                    from ""@VID_FEDIST"" T0 
	                                join ""@VID_FEDISTD"" T1 on T1.""DocEntry"" = T0.""DocEntry"" 
                                   where T0.""U_TipoDoc"" = '{0}'
						   	     and T0.""U_RangoF"" = '{1}' 
	                                 and T1.""U_Folio"" > 0 
                                   order by T1.""U_Folio"" DESC ";
                        }
                        s = String.Format(s, (System.String)(oDBDSH.GetValue("U_TipoDoc", 0)).Trim(), (System.String)(oDBDSH.GetValue("U_CAF", 0)).Trim());
                        oRecordSet.DoQuery(s);
                        //if oRecordSet.RecordCount > 0 then
                        //    Desde := Convert.ToInt32(System.Double(oRecordSet.Fields.Item("U_Folio").Value)) + 1
                        //else
                        //AC - lo deje asi para controlar el folio inicio en el registro del CAF 20141021 0039
                        Desde = Convert.ToInt32((System.String)(oDBDSH.GetValue("U_FolioDesde", 0)));

                        i = 0;
                        if (TotSuc > 0)
                        {
                            oDBDSD.SetValue("U_Desde", i, Convert.ToString(Desde));
                        }
                        oMtx.LoadFromDataSource();
                    }
                }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("CargarSucursal: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin CargarSucursal

    }// fin class TASignarFolios
}
