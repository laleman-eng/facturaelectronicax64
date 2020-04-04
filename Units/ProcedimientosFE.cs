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

namespace Factura_Electronica_VK.ProcedimientosFE
{
    class TProcedimientosFE : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private String s;
        private SAPbouiCOM.Matrix oMtx;
        private List<string> Lista;
        private SAPbouiCOM.DBDataSource oDBDSH;
        private SAPbouiCOM.Column oColumn;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            Int32 i;
            //TFunctions Reg;
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                Lista = new List<string>();

                FSBOf.LoadForm(xmlPath, "VID_FEPROCED.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = true;
                oForm.SupportedModes = -1;             // afm_All

                //        VID_DelRow := true;
                //        VID_DelRowOK := true;

                //oForm.DataBrowser.BrowseBy := "Code"; 
                oDBDSH = oForm.DataSources.DBDataSources.Item("@VID_FEPROCED");

                // Ok Ad  Fnd Vw Rq Sec
                Lista.Add("mtx      , f,  t,  f,  f, r, 1");
                //Lista.Add('Name      , f,  t,  t,  f, r, 1');
                //Lista.Add('CardCode  , f,  t,  t,  f, r, 1');
                //FSBOf.SetAutoManaged(var oForm, Lista);

                //oCombo := ComboBox(oForm.Items.Item('TipDoc').Specific);
                //oCombo.ValidValues.Add('33', 'Factura');

                //s := '1';
                //oCombo.Select(s, BoSearchKey.psk_ByValue);

                //        AddChooseFromList();
                oMtx = (Matrix)(oForm.Items.Item("mtx").Specific);
                //        oColumn                    := SAPbouiCOM.Column(oMtx.Columns.Item('V_0')); 
                //        oColumn.ChooseFromListUID  := 'CFL0';
                //        oColumn.ChooseFromListAlias:= 'Code'; 
                //        oMtx.AutoResizeColumns();


                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select DocEntry, ISNULL(U_TipoDoc,'') TipoDoc, ISNULL(U_ProcNomE,'') ProcNomE, ISNULL(U_ProcNomD,'') ProcNomD, ISNULL(U_ProcNomR,'') ProcNomR, ISNULL(U_ProcNomC,'') ProcNomC, ISNULL(U_ProcNomDe,'') ProcNomDe, ISNULL(U_ProcNomL,'') ProcNomL, ISNULL(U_ProcNomS,'') ProcNomS, ISNULL(U_Habili,'Y') 'Habilitada', ISNULL(U_CantLineas,0) 'CantLineas' from [@VID_FEPROCED]"; 
                else
                    s = @"select ""DocEntry"", IFNULL(""U_TipoDoc"",'') ""TipoDoc"", IFNULL(""U_ProcNomE"",'') ""ProcNomE"", IFNULL(""U_ProcNomD"",'') ""ProcNomD"", IFNULL(""U_ProcNomR"",'') ""ProcNomR"", IFNULL(""U_ProcNomC"",'') ""ProcNomC"", IFNULL(""U_ProcNomDe"",'') ""ProcNomDe"", IFNULL(""U_ProcNomL"",'') ""ProcNomL"", IFNULL(""U_ProcNomS"",'') ""ProcNomS"", IFNULL(""U_Habili"",'Y') ""Habilitada"", IFNULL(""U_CantLineas"",0) ""CantLineas"" from ""@VID_FEPROCED"" ";
                oRecordSet.DoQuery(s);

                i = 0;
                oDBDSH.Clear();
                while (!oRecordSet.EoF)
                {
                    oDBDSH.InsertRecord(i);
                    oDBDSH.SetValue("DocEntry", i, Convert.ToString((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value)));
                    oDBDSH.SetValue("U_TipoDoc", i, (System.String)(oRecordSet.Fields.Item("TipoDoc").Value));
                    //oDBDSH.SetValue("U_TipoDocPE", i, (System.String)(oRecordSet.Fields.Item("TipoDocPE").Value));
                    oDBDSH.SetValue("U_ProcNomE", i, (System.String)(oRecordSet.Fields.Item("ProcNomE").Value));
                    oDBDSH.SetValue("U_ProcNomD", i, (System.String)(oRecordSet.Fields.Item("ProcNomD").Value));
                    oDBDSH.SetValue("U_ProcNomR", i, (System.String)(oRecordSet.Fields.Item("ProcNomR").Value));
                    oDBDSH.SetValue("U_ProcNomC", i, (System.String)(oRecordSet.Fields.Item("ProcNomC").Value));
                    //oDBDSH.SetValue("U_ProcNomDe", i, (System.String)(oRecordSet.Fields.Item("ProcNomDe").Value));
                    //oDBDSH.SetValue("U_ProcNomL", i, (System.String)(oRecordSet.Fields.Item("ProcNomL").Value));
                    //oDBDSH.SetValue("U_ProcNomS", i, (System.String)(oRecordSet.Fields.Item("ProcNomS").Value));
                    oDBDSH.SetValue("U_Habili", i, (System.String)(oRecordSet.Fields.Item("Habilitada").Value));
                    oDBDSH.SetValue("U_CantLineas", i, Convert.ToString((System.Int32)(oRecordSet.Fields.Item("CantLineas").Value)));
                    oRecordSet.MoveNext();
                    i++;
                }

                oDBDSH.InsertRecord(i);
                oDBDSH.SetValue("DocEntry", i, "");
                oDBDSH.SetValue("U_TipoDoc", i, "");
                //oDBDSH.SetValue("U_TipoDocPE", i, "");
                oDBDSH.SetValue("U_ProcNomE", i, "");
                oDBDSH.SetValue("U_ProcNomD", i, "");
                oDBDSH.SetValue("U_ProcNomR", i, "");
                oDBDSH.SetValue("U_ProcNomC", i, "");
                //oDBDSH.SetValue("U_ProcNomDe", i, "");
                //oDBDSH.SetValue("U_ProcNomL", i, "");
                //oDBDSH.SetValue("U_ProcNomS", i, "");
                oDBDSH.SetValue("U_Habili", i, "Y");
                oDBDSH.SetValue("U_CantLineas", i, "60");

                if (GlobalSettings.RunningUnderSQLServer) //TipoDoc
                    s = @"select U1.FldValue 'Code', U1.Descr 'Name' from UFD1 U1 join CUFD U0 on U0.TableID = U1.TableID and U0.FieldID = U1.FieldID where U1.TableID = '@VID_FEPROCED' and U0.AliasID = '{0}'";
                else
                    s = @"select U1.""FldValue"" ""Code"", U1.""Descr"" ""Name"" from ""UFD1"" U1 join ""CUFD"" U0 on U0.""TableID"" = U1.""TableID"" and U0.""FieldID"" = U1.""FieldID"" where U1.""TableID"" = '@VID_FEPROCED' and U0.""AliasID"" = '{0}' ";

                s = String.Format(s, "TipoDoc");
                oRecordSet.DoQuery(s);
                oColumn = (SAPbouiCOM.Column)(oMtx.Columns.Item("TipoDoc"));
                FSBOf.FillComboMtx(oColumn, ref oRecordSet, false);
                //((SAPbouiCOM.Column)oMtx.Columns.Item("TipoDocPE")).Visible = false;

                if (GlobalSettings.RunningUnderSQLServer) //Habilitado
                    s = @"select FldValue 'Code', Descr 'Name' from UFD1 where TableID = '@VID_FEPROCED' and FieldID = 2";
                else
                    s = @"select ""FldValue"" ""Code"", ""Descr"" ""Name"" from ""UFD1"" where ""TableID"" = '@VID_FEPROCED' and ""FieldID"" = 2";
                oRecordSet.DoQuery(s);
                oColumn = (SAPbouiCOM.Column)(oMtx.Columns.Item("Habili"));
                FSBOf.FillComboMtx(oColumn, ref oRecordSet, false);


                //EditText(oForm.Items.Item('CardCode').Specific).Active := True;
                oMtx.LoadFromDataSource();


                oForm.Mode = BoFormMode.fm_OK_MODE;
                oMtx.AutoResizeColumns();
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
            SAPbouiCOM.EditText oEditText;
            SAPbouiCOM.ComboBox oComboBox;
            Int32 i;

            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED)
                {
                    if ((pVal.ItemUID == "1") && ((oForm.Mode == BoFormMode.fm_ADD_MODE) || (oForm.Mode == BoFormMode.fm_UPDATE_MODE)) && (pVal.BeforeAction))
                    {
                        BubbleEvent = false;
                        if (Validar())
                            if (Limpiar())
                            {
                                oForm.Freeze(true);
                                GuardarRegistros();
                                oMtx.FlushToDataSource();
                                i = oMtx.RowCount + 1;
                                oMtx.AddRow(1, i);
                                oComboBox = (ComboBox)(oMtx.Columns.Item("TipoDoc").Cells.Item(i).Specific);
                                oComboBox.Select("33", BoSearchKey.psk_ByValue);

                                oEditText = (EditText)(oMtx.Columns.Item("ProcNomE").Cells.Item(i).Specific);
                                oEditText.Value = "";

                                oEditText = (EditText)(oMtx.Columns.Item("ProcNomD").Cells.Item(i).Specific);
                                oEditText.Value = "";

                                oEditText = (EditText)(oMtx.Columns.Item("ProcNomR").Cells.Item(i).Specific);
                                oEditText.Value = "";

                                oEditText = (EditText)(oMtx.Columns.Item("ProcNomC").Cells.Item(i).Specific);
                                oEditText.Value = "";

                                oEditText = (EditText)(oMtx.Columns.Item("ProcNomDe").Cells.Item(i).Specific);
                                oEditText.Value = "";

                                oEditText = (EditText)(oMtx.Columns.Item("ProcNomL").Cells.Item(i).Specific);
                                oEditText.Value = "";

                                oEditText = (EditText)(oMtx.Columns.Item("ProcNomS").Cells.Item(i).Specific);
                                oEditText.Value = "";

                                oComboBox = (ComboBox)(oMtx.Columns.Item("Habili").Cells.Item(i).Specific);
                                oComboBox.Select("Y", BoSearchKey.psk_ByValue);

                                oEditText = (EditText)(oMtx.Columns.Item("DocEntry").Cells.Item(i).Specific);
                                oEditText.Value = "";

                                oEditText = (EditText)(oMtx.Columns.Item("CantLineas").Cells.Item(i).Specific);
                                oEditText.Value = "60";

                                oMtx.FlushToDataSource();
                            }
                    }

                    if ((pVal.ItemUID == "btnBorrar") && (!pVal.BeforeAction))
                        Borrar();
                }
            }
            catch (Exception e)
            {
                FCmpny.GetLastError(out nErr, out sErr);
                FSBOApp.StatusBar.SetText("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
            oForm.Freeze(false);
        }//fin FormEvent

        private Boolean Validar()
        {
            Boolean _return = true;
            SAPbouiCOM.EditText oEditText;
            String CantLineas;
            Int32 i;
            try
            {
                oMtx.FlushToDataSource();
                i = 1;
                while (i <= oMtx.RowCount)
                {
                    oEditText = (EditText)(oMtx.Columns.Item("CantLineas").Cells.Item(i).Specific);
                    CantLineas = (System.String)(oEditText.Value).Trim();
                    if (Convert.ToInt32(CantLineas) > 60)
                    {
                        _return = false;
                        FSBOApp.StatusBar.SetText("Cantidad maxima de lineas debe ser menor a 60, linea " + i.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        i = oMtx.RowCount;
                    }
                    else if (Convert.ToInt32(CantLineas) <= 0)
                    {
                        _return = false;
                        FSBOApp.StatusBar.SetText("Cantidad maxima de lineas debe ser mayor a 0, linea " + i.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        i = oMtx.RowCount;
                    }
                    i++;
                }
                return _return;
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("Validar: " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
        }

        private void GuardarRegistros()
        {
            TFunctions Reg;
            Int32 i;
            SAPbouiCOM.EditText oEditText;
            SAPbouiCOM.ComboBox oComboBox;
            String DocEntry, ProcNomE, ProcNomD, ProcNomR, ProcNomC, ProcNomL, ProcNomS, Hab, TipoDoc, CantLineas, ProcNomDe;
            Int32 _return = 0;

            try
            {
                Reg = new TFunctions();
                Reg.SBO_f = FSBOf;

                oMtx.FlushToDataSource();
                i = 1;
                while (i <= oMtx.RowCount)
                {
                    oEditText = (EditText)(oMtx.Columns.Item("DocEntry").Cells.Item(i).Specific);
                    DocEntry = (System.String)(oEditText.Value).Trim();

                    oEditText = (EditText)(oMtx.Columns.Item("ProcNomE").Cells.Item(i).Specific);
                    ProcNomE = (System.String)(oEditText.Value).Trim();

                    oEditText = (EditText)(oMtx.Columns.Item("ProcNomD").Cells.Item(i).Specific);
                    ProcNomD = (System.String)(oEditText.Value).Trim();

                    oEditText = (EditText)(oMtx.Columns.Item("ProcNomR").Cells.Item(i).Specific);
                    ProcNomR = (System.String)(oEditText.Value).Trim();

                    oEditText = (EditText)(oMtx.Columns.Item("ProcNomC").Cells.Item(i).Specific);
                    ProcNomC = (System.String)(oEditText.Value).Trim();

                    //oEditText = (EditText)(oMtx.Columns.Item("ProcNomDe").Cells.Item(i).Specific);
                    //ProcNomDe = (System.String)(oEditText.Value).Trim();

                    //oEditText = (EditText)(oMtx.Columns.Item("ProcNomL").Cells.Item(i).Specific);
                    //ProcNomL = (System.String)(oEditText.Value).Trim();

                    //oEditText = (EditText)(oMtx.Columns.Item("ProcNomS").Cells.Item(i).Specific);
                    //ProcNomS = (System.String)(oEditText.Value).Trim();

                    oComboBox = (ComboBox)(oMtx.Columns.Item("Habili").Cells.Item(i).Specific);
                    Hab = oComboBox.Value;

                    oComboBox = (ComboBox)(oMtx.Columns.Item("TipoDoc").Cells.Item(i).Specific);
                    TipoDoc = oComboBox.Value;

                    //oComboBox = (ComboBox)(oMtx.Columns.Item("TipoDocPE").Cells.Item(i).Specific);
                    //TipoDocPE = oComboBox.Value;

                    oEditText = (EditText)(oMtx.Columns.Item("CantLineas").Cells.Item(i).Specific);
                    CantLineas = (System.String)(oEditText.Value).Trim();

                    oDBDSH.Clear();
                    oDBDSH.InsertRecord(0);
                    oDBDSH.SetValue("DocEntry", 0, DocEntry);
                    oDBDSH.SetValue("U_TipoDoc", 0, TipoDoc);
                    //oDBDSH.SetValue("U_TipoDocPE", 0, TipoDocPE);
                    oDBDSH.SetValue("U_Habili", 0, Hab);
                    oDBDSH.SetValue("U_ProcNomE", 0, ProcNomE);
                    oDBDSH.SetValue("U_ProcNomD", 0, ProcNomD);
                    oDBDSH.SetValue("U_ProcNomR", 0, ProcNomR);
                    oDBDSH.SetValue("U_ProcNomC", 0, ProcNomC);
                    //oDBDSH.SetValue("U_ProcNomDe", 0, ProcNomDe);
                    //oDBDSH.SetValue("U_ProcNomL", 0, ProcNomL);
                    //oDBDSH.SetValue("U_ProcNomS", 0, ProcNomS);
                    oDBDSH.SetValue("U_CantLineas", 0, CantLineas);


                    if (DocEntry.Trim() == "")
                        _return = Reg.FEPROCAdd(oDBDSH);
                    else
                        _return = Reg.FEPROCUpt(oDBDSH);

                    if (_return != 0)
                    {
                        oEditText = (EditText)(oMtx.Columns.Item("DocEntry").Cells.Item(i).Specific);
                        oEditText.Value = _return.ToString();
                        FSBOApp.StatusBar.SetText("Se ha guardado regsitro Tipo Documento " + TipoDoc, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    }
                    else
                        FSBOApp.StatusBar.SetText("No se ha guardado regsitro Tipo Documento " + TipoDoc, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);

                    i++;
                }

                oMtx.FlushToDataSource();
                oMtx.AutoResizeColumns();

                if (_return > 0) oForm.Mode = BoFormMode.fm_OK_MODE;
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("GuardarRegistros: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin GuardarRegistro


        private Boolean Limpiar()
        {
            Boolean _result;
            Int32 i;
            SAPbouiCOM.EditText oEditText;
            SAPbouiCOM.ComboBox oComboBox;
            String ProcNomE, ProcNomD, ProcNomR, ProcNomC, ProcNomL, ProcNomS, TipoDoc, Hab;

            try
            {
                _result = true;
                oMtx.FlushToDataSource();
                i = 1;
                while (i <= oMtx.RowCount)
                {
                    oEditText = (EditText)(oMtx.Columns.Item("ProcNomE").Cells.Item(i).Specific);
                    ProcNomE = (System.String)(oEditText.Value).Trim();

                    oEditText = (EditText)(oMtx.Columns.Item("ProcNomD").Cells.Item(i).Specific);
                    ProcNomD = (System.String)(oEditText.Value).Trim();

                    oEditText = (EditText)(oMtx.Columns.Item("ProcNomR").Cells.Item(i).Specific);
                    ProcNomR = (System.String)(oEditText.Value).Trim();

                    oEditText = (EditText)(oMtx.Columns.Item("ProcNomC").Cells.Item(i).Specific);
                    ProcNomC = (System.String)(oEditText.Value).Trim();

                    //oEditText = (EditText)(oMtx.Columns.Item("ProcNomS").Cells.Item(i).Specific);
                    //ProcNomS = (System.String)(oEditText.Value).Trim();

                    //oEditText = (EditText)(oMtx.Columns.Item("ProcNomL").Cells.Item(i).Specific);
                    //ProcNomL = (System.String)(oEditText.Value).Trim();

                    oComboBox = (ComboBox)(oMtx.Columns.Item("Habili").Cells.Item(i).Specific);
                    Hab = oComboBox.Value;

                    oComboBox = (ComboBox)(oMtx.Columns.Item("TipoDoc").Cells.Item(i).Specific);
                    TipoDoc = oComboBox.Value;

                    if ((TipoDoc == "39") || (TipoDoc == "41"))
                    {
                        if (ProcNomD == "")
                            ProcNomD = "X";

                        if (ProcNomR == "")
                            ProcNomR = "X";
                    }

                    if ((ProcNomE == "") || (ProcNomD == "") || (ProcNomR == "") || (Hab == "") || (TipoDoc == ""))
                    {
                        oMtx.DeleteRow(i);
                        i = i - 1;
                    }

                    i++;
                }
                oMtx.FlushToDataSource();

                return _result;
            }
            catch (Exception e)
            {
                OutLog("Limpiar: " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
        }//fin Limpiar


        public new void MenuEvent(ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
            //Reg : TRemuneraciones_MyFunctions;
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
                    {

                    }
                }

                if ((pVal.MenuUID == "1282") || (pVal.MenuUID == "1281"))
                {
                    //oDBDSH.SetValue("Name", 0, "");
                    //oDBDSH.SetValue("Code", 0, "");
                }
                //inherited MenuEvent(Var pVal,var BubbleEvent);
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent


        private void Borrar()
        {
            Int32 i;
            Boolean Paso = false;
            String DocEntry;
            SAPbouiCOM.EditText oEditText;
            TFunctions Reg;

            try
            {
                oForm.Freeze(true);
                Reg = new TFunctions();
                Reg.SBO_f = FSBOf;
                i = 1;
                while (i <= oMtx.RowCount)
                {
                    if (oMtx.IsRowSelected(i))
                    {
                        oEditText = (EditText)(oMtx.Columns.Item("DocEntry").Cells.Item(i).Specific);
                        DocEntry = (System.String)(oEditText.Value).Trim();
                        if (DocEntry != "")
                        {
                            Reg.DelDataSource("D", "VID_FEPROCED", "", FSBOf.StrToInteger(DocEntry));
                            oMtx.DeleteRow(i);
                            Paso = true;
                            i = oMtx.RowCount;
                        }
                    }
                    i++;
                }
                if (!Paso)
                    FSBOApp.StatusBar.SetText("Debe seleccionar una linea", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                else
                    oForm.Mode = BoFormMode.fm_UPDATE_MODE;
            }
            catch (Exception x)
            {
                oForm.Freeze(false);
            }

        }


    }//fin Class
}
