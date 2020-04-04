using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
//using System.Net.Http;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.Data.SqlClient;
using System.IO;
using System.CodeDom.Compiler;
using System.Data;
using System.Xml;
using System.Xml.Linq;
using SAPbouiCOM;
using SAPbobsCOM;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using VisualD.untLog;
using Factura_Electronica_VK.Functions;
using FactRemota;
//using ServiceStack.Text;
using Newtonsoft.Json;
using DLLparaXML;

namespace Factura_Electronica_VK.DeliveryNote
{
    public class TDeliveryNote : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private String s;
        private Boolean Flag;
        private SAPbouiCOM.Matrix mtx;
        private SAPbouiCOM.Item oItem = null;
        private SAPbouiCOM.Item oItemB;
        private SAPbouiCOM.StaticText oStatic;
        private SAPbouiCOM.EditText oEditText;
        private SAPbouiCOM.ComboBox oComboBox;
        private List<string> Lista;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");

        private String JsonText;

        public static String DocSubType
        { get; set; }
        public static Boolean bFolderAdd
        { get; set; }
        public static Boolean Transferencia
        { get; set; }
        public static Boolean Devolucion
        { get; set; }

        public static Boolean SolicitudTraslado
        { get; set; }

        public VisualD.SBOFunctions.CSBOFunctions SBO_f;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            SAPbouiCOM.Folder oFolder;

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {

                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                //FSBOf.LoadForm(xmlPath, 'VID_Entrega.srf', Uid);
                oForm = FSBOApp.Forms.Item(uid);
                Flag = false;
                oForm.Freeze(true);

                if (bFolderAdd)
                {
                    //if (SolicitudTraslado)
                    //{
                    //    oItemB = oForm.Items.Item("17");
                    //    oItem = oForm.Items.Add("lblFol", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                    //    oItem.Left = oItemB.Left;
                    //    oItem.Width = oItemB.Width;
                    //    oItem.Top = oItemB.Top + oItemB.Height + 10;
                    //    oItem.Height = oItem.Height;
                    //    oItem.LinkTo = "FolioNum";
                    //    oStatic = (StaticText)(oForm.Items.Item("lblFol").Specific);
                    //    oStatic.Caption = "Folio ";

                    //    oItemB = oForm.Items.Item("lblFol");
                    //    oItem = oForm.Items.Add("FolioNum", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                    //    oItem.Left = oItemB.Left + oItemB.Width + 5;
                    //    oItem.Width = oItemB.Width + 60;
                    //    oItem.Top = oItemB.Top;
                    //    oItem.Height = oItem.Height;
                    //    oEditText = (EditText)(oForm.Items.Item("FolioNum").Specific);
                    //    oEditText.DataBind.SetBound(true, "OWTQ", "FolioNum");



                    //}

                    oForm.DataSources.UserDataSources.Add("VID_FEDCTO", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                    oItem = oForm.Items.Add("VID_FEDCTO", SAPbouiCOM.BoFormItemTypes.it_FOLDER);

                    //para SAP 882 en adelante
                    if (Transferencia)
                        oItemB = oForm.Items.Item("1320000082");
                    else
                        oItemB = oForm.Items.Item("1320002137");

                    oItem.Left = oItemB.Left + 60;
                    oItem.Width = oItemB.Width;
                    oItem.Top = oItemB.Top;
                    oItem.Height = oItem.Height;
                    oFolder = (Folder)((oItem.Specific));
                    oFolder.Caption = "Factura Electrónica";
                    oFolder.Pane = 333;
                    oFolder.DataBind.SetBound(true, "", "VID_FEDCTO");
                    if (Transferencia)
                    { oFolder.GroupWith("1320000082"); }
                    else
                    { oFolder.GroupWith("1320002137"); }

                    //cargar campos de usuarios
                    if (!Transferencia)
                    { 
                           oItemB = oForm.Items.Item("47"); 
                    }
                    else
                    { 
                        oItemB = oForm.Items.Item("VID_FEDCTO"); 
                    }
                    oItem = oForm.Items.Add("lblTrasl", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                    oItem.Left = 50; //oItemB.Left;
                    oItem.Width = 70;//;oItemB.Width;
                    if (!Transferencia)
                    {
                        oItem.Top = oItemB.Top;//195
                        oItem.Height = oItem.Height;//14
                    }
                    else
                    {
                        oItem.Top = oItemB.Top + 40;//195
                        oItem.Height = 14;
                    }
                    oItem.FromPane = 333;
                    oItem.ToPane = 333;
                    oItem.LinkTo = "VID_FETras";
                    oStatic = (StaticText)(oForm.Items.Item("lblTrasl").Specific);
                    oStatic.Caption = "Tipo Traslado";

                    oItemB = oForm.Items.Item("lblTrasl");
                    oItem = oForm.Items.Add("VID_FETras", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                    oItem.Left = oItemB.Left + oItemB.Width + 5;
                    oItem.Width = oItemB.Width + 60;
                    oItem.Top = oItemB.Top;
                    oItem.Height = oItem.Height;
                    oItem.FromPane = 333;
                    oItem.ToPane = 333;

                    oItemB = oForm.Items.Item("lblTrasl");
                    oItem = oForm.Items.Add("lblTipDesp", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                    oItem.Left = oItemB.Left;
                    oItem.Width = oItemB.Width;
                    oItem.Top = oItemB.Top + oItemB.Height + 10;
                    oItem.Height = oItem.Height;
                    oItem.FromPane = 333;
                    oItem.ToPane = 333;
                    oItem.LinkTo = "VID_FEDesp";
                    oStatic = (StaticText)(oForm.Items.Item("lblTipDesp").Specific);
                    oStatic.Caption = "Tipo Despacho";

                    oItemB = oForm.Items.Item("lblTipDesp");
                    oItem = oForm.Items.Add("VID_FEDesp", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                    oItem.Left = oItemB.Left + oItemB.Width + 5;
                    oItem.Width = oItemB.Width + 60;
                    oItem.Top = oItemB.Top;
                    oItem.Height = oItem.Height;
                    oItem.FromPane = 333;
                    oItem.ToPane = 333;

                    if (GlobalSettings.RunningUnderSQLServer)
                    {
                        s = @"select C1.FldValue 'Code', C1.Descr 'Name'
                              from CUFD C0
                              JOIN UFD1 C1 ON C1.TableID = C0.TableID
                                          AND C1.FieldID = C0.FieldID
                             WHERE C0.TableID = 'ODLN'
                               AND C0.AliasID = 'Traslado'";
                    }
                    else
                    {
                        s = @"select C1.""FldValue"" ""Code"", C1.""Descr"" ""Name""
                              from ""CUFD"" C0
                              JOIN ""UFD1"" C1 ON C1.""TableID"" = C0.""TableID""
                                            AND C1.""FieldID"" = C0.""FieldID""
                             WHERE C0.""TableID"" = 'ODLN'
                               AND C0.""AliasID"" = 'Traslado' ";
                    }
                    oRecordSet.DoQuery(s);    //trae datos creados por campos de usuario 
                    FSBOf.FillCombo((ComboBox)(oForm.Items.Item("VID_FETras").Specific), ref oRecordSet, false);

                    if (GlobalSettings.RunningUnderSQLServer)
                    {
                        s = @"select C1.FldValue 'Code', C1.Descr 'Name'
                              from CUFD C0
                              JOIN UFD1 C1 ON C1.TableID = C0.TableID
                                          AND C1.FieldID = C0.FieldID
                             WHERE C0.TableID = 'ODLN'
                               AND C0.AliasID = 'TipDespacho'";
                    }
                    else
                    {
                        s = @"select C1.""FldValue"" ""Code"", C1.""Descr"" ""Name""
                              from ""CUFD"" C0
                              JOIN ""UFD1"" C1 ON C1.""TableID"" = C0.""TableID""
                                            AND C1.""FieldID"" = C0.""FieldID""
                             WHERE C0.""TableID"" = 'ODLN'
                               AND C0.""AliasID"" = 'TipDespacho' ";
                    }
                    oRecordSet.DoQuery(s); //trae datos creados por campos de usuario 
                    FSBOf.FillCombo((ComboBox)(oForm.Items.Item("VID_FEDesp").Specific), ref oRecordSet, false);


                    if (!Transferencia)
                    {
                        if (!Devolucion)
                        {
                            oComboBox = (ComboBox)(oForm.Items.Item("VID_FETras").Specific);
                            oComboBox.DataBind.SetBound(true, "ODLN", "U_Traslado");
                            oComboBox = (ComboBox)(oForm.Items.Item("VID_FEDesp").Specific);
                            oComboBox.DataBind.SetBound(true, "ODLN", "U_TipDespacho");
                        }
                        else
                        {
                            oComboBox = (ComboBox)(oForm.Items.Item("VID_FETras").Specific);
                            oComboBox.DataBind.SetBound(true, "ORPD", "U_Traslado");
                            oComboBox = (ComboBox)(oForm.Items.Item("VID_FEDesp").Specific);
                            oComboBox.DataBind.SetBound(true, "ORPD", "U_TipDespacho");
                        }
                    }
                    else
                    {
                        if (!SolicitudTraslado)
                        {
                            oComboBox = (ComboBox)(oForm.Items.Item("VID_FETras").Specific);
                            oComboBox.DataBind.SetBound(true, "OWTR", "U_Traslado");
                            oComboBox = (ComboBox)(oForm.Items.Item("VID_FEDesp").Specific);
                            oComboBox.DataBind.SetBound(true, "OWTR", "U_TipDespacho");
                        }
                        else
                        {
                            oComboBox = (ComboBox)(oForm.Items.Item("VID_FETras").Specific);
                            oComboBox.DataBind.SetBound(true, "OWTQ", "U_Traslado");
                            oComboBox = (ComboBox)(oForm.Items.Item("VID_FEDesp").Specific);
                            oComboBox.DataBind.SetBound(true, "OWTQ", "U_TipDespacho");
                        }
                    }

                    oForm.Items.Item("VID_FETras").DisplayDesc = true;
                    oForm.Items.Item("VID_FEDesp").DisplayDesc = true;

                }

                if (!Transferencia)
                {
                    //Campo con el estado de DTE
                    oItemB = oForm.Items.Item("84");
                    oItem = oForm.Items.Add("lblEstado", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                    oItem.Left = oItemB.Left;
                    oItem.Width = oItemB.Width;
                    oItem.Top = oItemB.Top + oItemB.Height + 5;
                    oItem.Height = oItem.Height;
                    oItem.LinkTo = "VID_FEEstado";
                    oStatic = (StaticText)(oForm.Items.Item("lblEstado").Specific);
                    oStatic.Caption = "Estado SII";

                    oItemB = oForm.Items.Item("208");
                    oItem = oForm.Items.Add("VID_Estado", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                    oItem.Left = oItemB.Left;
                    oItem.Width = oItemB.Width + 30;
                    oItem.Top = oItemB.Top + oItemB.Height + 5;
                    oItem.Height = oItem.Height;
                    oItem.DisplayDesc = true;
                    oItem.Enabled = false;
                    oComboBox = (ComboBox)(oForm.Items.Item("VID_Estado").Specific);
                    if (!Devolucion)
                        oComboBox.DataBind.SetBound(true, "ODLN", "U_EstadoFE");
                    else
                        oComboBox.DataBind.SetBound(true, "ORPD", "U_EstadoFE");

                    oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                    var sSeries = (System.String)(oComboBox.Value);

                    if (GlobalSettings.RunningUnderSQLServer)
                    { s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor' from NNM1 where Series = {0} --AND ObjectCode = '{1}' "; }
                    else
                    { s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"" from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' "; }
                    s = String.Format(s, sSeries, Devolucion ? "21" : "15");
                    if (sSeries != "")
                    {
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount > 0)
                        {
                            if ((System.String)(oRecordSet.Fields.Item("Valor").Value) == "E")
                            {
                                oForm.Items.Item("VID_Estado").Visible = true;
                                oForm.Items.Item("lblEstado").Visible = true;
                                ((ComboBox)oForm.Items.Item("VID_Estado").Specific).Select("N", BoSearchKey.psk_ByValue);
                            }
                            else
                            {
                                oForm.Items.Item("VID_Estado").Visible = false;
                                oForm.Items.Item("lblEstado").Visible = false;
                            }
                        }
                    }
                }
                else
                {
                    //Campo con el estado de DTE
                    oItemB = oForm.Items.Item("37");
                    oItem = oForm.Items.Add("lblEstado", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                    oItem.Left = oItemB.Left;
                    oItem.Width = oItemB.Width;
                    oItem.Top = oItemB.Top + oItemB.Height + 5;
                    oItem.Height = oItem.Height;
                    oItem.LinkTo = "VID_FEEstado";
                    oStatic = (StaticText)(oForm.Items.Item("lblEstado").Specific);
                    oStatic.Caption = "Estado SII";

                    oItemB = oForm.Items.Item("36");
                    oItem = oForm.Items.Add("VID_Estado", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                    oItem.Left = oItemB.Left;
                    oItem.Width = oItemB.Width;
                    oItem.Top = oItemB.Top + oItemB.Height + 5;
                    oItem.Height = oItem.Height;
                    oItem.DisplayDesc = true;
                    oItem.Enabled = false;
                    if (!SolicitudTraslado)
                    {
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_Estado").Specific);
                        oComboBox.DataBind.SetBound(true, "OWTR", "U_EstadoFE");
                    }
                    else
                    {
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_Estado").Specific);
                        oComboBox.DataBind.SetBound(true, "OWTQ", "U_EstadoFE");
                    }

                    oComboBox = (ComboBox)(oForm.Items.Item("40").Specific);
                    var sSeries = (System.String)(oComboBox.Value);

                    if (GlobalSettings.RunningUnderSQLServer)
                    { 
                        s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor' from NNM1 where Series = {0} --AND ObjectCode = '67' "; 
                    }
                    else
                    { 
                        s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"" from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '67'  "; 
                    }
                    s = String.Format(s, sSeries);
                    if (sSeries != "")
                    {
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount > 0)
                        {
                            if ((System.String)(oRecordSet.Fields.Item("Valor").Value) == "E")
                            {
                                oForm.Items.Item("VID_Estado").Visible = true;
                                oForm.Items.Item("lblEstado").Visible = true;
                                if (oForm.Mode == BoFormMode.fm_ADD_MODE)
                                    ((ComboBox)oForm.Items.Item("VID_Estado").Specific).Select("N", BoSearchKey.psk_ByValue);
                            }
                            else
                            {
                                oForm.Items.Item("VID_Estado").Visible = false;
                                oForm.Items.Item("lblEstado").Visible = false;
                            }
                        }
                    }
                }

                Lista = new List<string>();
                // Ok Ad  Fnd Vw Rq Sec
                Lista.Add("VID_Estado  , f,  f,  f,  f, n, 1");
                //Lista.Add('CardCode  , f,  t,  t,  f, r, 1');
                FSBOf.SetAutoManaged(oForm, Lista);

            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            return Result;

        }//fin InitForm


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
                //1287 Duplicar;

                if ((pVal.MenuUID != "") && (pVal.BeforeAction == false))
                {
                    if ((pVal.MenuUID == "1288") || (pVal.MenuUID == "1289") || (pVal.MenuUID == "1290") || (pVal.MenuUID == "1291"))
                    {
                        oForm.Freeze(true);
                        //oForm.Items.Item("VID_Estado").Enabled = false;
                        if (oForm.BusinessObject.Type != "67" && oForm.BusinessObject.Type != "1250000001")
                            oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                        else
                            oComboBox = (ComboBox)(oForm.Items.Item("40").Specific);
                        var sSeries = (System.String)(oComboBox.Value);

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor' from NNM1 where Series = {0} --AND ObjectCode = '{1}' ";
                        else
                            s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"" from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' ";
                        s = String.Format(s, sSeries, oForm.BusinessObject.Type);
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount > 0)
                        {
                            if ((System.String)(oRecordSet.Fields.Item("Valor").Value) == "E")
                            {
                                oForm.Items.Item("VID_Estado").Visible = true;
                                oForm.Items.Item("lblEstado").Visible = true;
                            }
                            else
                            {
                                oForm.Items.Item("VID_Estado").Visible = false;
                                oForm.Items.Item("lblEstado").Visible = false;
                            }
                        }
                        oForm.Freeze(false);
                    }

                    if ((pVal.MenuUID == "1282") || (pVal.MenuUID == "1281") || (pVal.MenuUID == "1287"))
                    {
                        oForm.Freeze(true);
                        //oForm.Items.Item("VID_Estado").Enabled = false;
                        if (oForm.BusinessObject.Type != "67" && oForm.BusinessObject.Type != "1250000001")
                            oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                        else
                            oComboBox = (ComboBox)(oForm.Items.Item("40").Specific);
                        var sSeries = (System.String)(oComboBox.Value);

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor' from NNM1 where Series = {0} --AND ObjectCode = '{1}'";
                        else
                            s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"" from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' ";
                        s = String.Format(s, sSeries, oForm.BusinessObject.Type);
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount > 0)
                        {
                            if ((System.String)(oRecordSet.Fields.Item("Valor").Value) == "E")
                            {
                                oForm.Items.Item("VID_Estado").Visible = true;
                                oForm.Items.Item("lblEstado").Visible = true;
                            }
                            else
                            {
                                oForm.Items.Item("VID_Estado").Visible = false;
                                oForm.Items.Item("lblEstado").Visible = false;
                            }

                            if ((pVal.MenuUID == "1282") || (pVal.MenuUID == "1287"))
                            {
                                ((ComboBox)oForm.Items.Item("VID_Estado").Specific).Select("N", BoSearchKey.psk_ByValue);
                            }
                        }
                        oForm.Freeze(false);
                    }
                }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent


        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            Int32 nErr;
            String sErr;

            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if ((pVal.ItemUID == "1") && (oForm.Mode == BoFormMode.fm_ADD_MODE))
                        BubbleEvent = ValidarDatosFE();
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction))
                {
                    if (pVal.ItemUID == "VID_FEDCTO")
                    {
                        oForm.PaneLevel = 333;
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_FORM_ACTIVATE) && (!pVal.BeforeAction))
                {
                    GlobalSettings.PrevFormUID = oForm.UniqueID;
                }

                if (((pVal.ItemUID == "40") || (pVal.ItemUID == "88")) && (pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (!pVal.BeforeAction))
                {
                    if (oForm.BusinessObject.Type != "67" && oForm.BusinessObject.Type != "1250000001")
                        oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                    else
                        oComboBox = (ComboBox)(oForm.Items.Item("40").Specific);
                    var sSeries = (System.String)(oComboBox.Value);

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor' from NNM1 where Series = {0} --AND ObjectCode = '{1}' ";
                    else
                        s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"" from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' ";
                    s = String.Format(s, sSeries, oForm.BusinessObject.Type);
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount > 0)
                    {
                        if ((System.String)(oRecordSet.Fields.Item("Valor").Value) == "E")
                        {
                            oForm.Items.Item("VID_Estado").Visible = true;
                            oForm.Items.Item("lblEstado").Visible = true;
                        }
                        else
                        {
                            oForm.Items.Item("VID_Estado").Visible = false;
                            oForm.Items.Item("lblEstado").Visible = false;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                FCmpny.GetLastError(out nErr, out sErr);
                FSBOApp.StatusBar.SetText("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
                if (oForm != null)
                    oForm.Freeze(false);
            }
        }//fin FormEvent


        public new void FormDataEvent(ref BusinessObjectInfo BusinessObjectInfo, ref Boolean BubbleEvent)
        {
            String sDocEntry;
            String sDocSubType;
            String TipoDocElec = "";
            Int32 lRetCode;
            String Tipo;
            TFunctions Reg;
            Boolean bMultiSoc;
            String nMultiSoc = "";
            String[] FE52 = { "15", "67", "21", "1250000001" };
            XmlDocument _xmlDocument;
            XmlNode N;
            SAPbobsCOM.Documents oDocument = null;
            SAPbobsCOM.StockTransfer oTransfer = null;
            String TaxIdNum;
            String LicTradNum;
            String sDocTotal;
            String Canceled = "";
            String GeneraT = "";
            String CAF = "";
            Int32 FolioNum;
            Int32 FDocEntry = 0;
            Int32 FLineId = -1;
            Boolean bFolioDistribuido = false;
            Boolean bFolioAsignado = false;
            String FolioPortal = "N";
            String Distribuido = "N";

            base.FormDataEvent(ref BusinessObjectInfo, ref BubbleEvent);


            try
            {

                if ((BusinessObjectInfo.BeforeAction == false) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_ADD) && (BusinessObjectInfo.ActionSuccess) && (BusinessObjectInfo.Type != "112"))
                {
                    if (FE52.Contains(oForm.BusinessObject.Type)) //And (Flag = true)) then
                    {
                        Flag = false;
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = "select ISNULL(U_Distrib,'N') 'Distribuido', ISNULL(U_FPortal,'N') 'FolioPortal', ISNULL(U_MultiSoc,'N') MultiSoc, ISNULL(U_GenerarT,'N') GeneraT from [@VID_FEPARAM] WITH (NOLOCK)";
                        else
                            s = @"select IFNULL(""U_Distrib"",'N') ""Distribuido"", IFNULL(""U_FPortal"",'N') ""FolioPortal"", IFNULL(""U_MultiSoc"",'N') ""MultiSoc"", IFNULL(""U_GenerarT"",'N') ""GeneraT"" from ""@VID_FEPARAM"" ";
                        oRecordSet.DoQuery(s);

                        if (oRecordSet.RecordCount > 0)
                        {
                            GeneraT = ((System.String)oRecordSet.Fields.Item("GeneraT").Value).Trim();
                            FolioPortal = ((System.String)oRecordSet.Fields.Item("FolioPortal").Value).Trim();
                            Distribuido = ((System.String)oRecordSet.Fields.Item("Distribuido").Value).Trim();

                            if ((System.String)(oRecordSet.Fields.Item("MultiSoc").Value) == "Y")
                                bMultiSoc = true;
                            else
                                bMultiSoc = false;

                            if ((Distribuido == "Y") && (FolioPortal == "N"))
                            {
                                if (oForm.BusinessObject.Type == "67" || oForm.BusinessObject.Type == "1250000001")
                                {
                                    _xmlDocument = new XmlDocument();
                                    _xmlDocument.LoadXml(BusinessObjectInfo.ObjectKey);
                                    N = _xmlDocument.SelectSingleNode("StockTransferParams");
                                    sDocEntry = (System.String)(N.InnerText).Trim();
                                }
                                else
                                    sDocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);

                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'Inst', C0.LicTradNum, ROUND(T0.DocTotal,0) DocTotal
                                                      ,T0.CANCELED
                                                 FROM {1} T0 WITH (NOLOCK)
                                                    JOIN OCRD C0 ON C0.CardCode = T0.CardCode
                                                    JOIN NNM1 T2 ON T0.Series = T2.Series 
                                                   WHERE T0.DocEntry = {0}";
                                else
                                    s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"", C0.""LicTradNum"", ROUND(T0.""DocTotal"",0) ""DocTotal""
                                                          ,T0.""CANCELED""
                                                 FROM ""{1}"" T0
                                                    JOIN ""OCRD"" C0 ON C0.""CardCode"" = T0.""CardCode""
                                                    JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                                                   WHERE T0.""DocEntry"" = {0} ";
                                if (oForm.BusinessObject.Type == "15")
                                    s = String.Format(s, sDocEntry, "ODLN");
                                else if (oForm.BusinessObject.Type == "67")
                                    s = String.Format(s, sDocEntry, "OWTR");
                                else if (oForm.BusinessObject.Type == "1250000001")
                                    s = String.Format(s, sDocEntry, "OWTQ");
                                else //21 devolucion de compra
                                    s = String.Format(s, sDocEntry, "ORPD");
                                oRecordSet.DoQuery(s);
                                sDocSubType = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                Tipo = (System.String)(oRecordSet.Fields.Item("Tipo").Value);
                                LicTradNum = (System.String)(oRecordSet.Fields.Item("LicTradNum").Value);
                                sDocTotal = FSBOf.DoubleToStr((System.Double)(oRecordSet.Fields.Item("DocTotal").Value));
                                Canceled = (System.String)(oRecordSet.Fields.Item("CANCELED").Value);

                                if ((Tipo == "E") && (Canceled == "N"))
                                {

                                    if ((bMultiSoc == true) && (nMultiSoc == ""))
                                        FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                    else
                                    {
                                        if (sDocSubType == "--") //Guia de Despacho
                                            TipoDocElec = "52";

                                        bFolioDistribuido = true;
                                        if (GlobalSettings.RunningUnderSQLServer)
                                            s = @"EXEC VID_SP_FE_BUSCAR_FOLIO '{0}'";
                                        else
                                            s = @"CALL VID_SP_FE_BUSCAR_FOLIO ('{0}')";

                                        s = String.Format(s, TipoDocElec);
                                        oRecordSet.DoQuery(s);
                                        if (oRecordSet.RecordCount > 0)
                                        {
                                            TaxIdNum = (System.String)(oRecordSet.Fields.Item("TaxIdNum").Value).ToString().Trim();
                                            CAF = (System.String)(oRecordSet.Fields.Item("CAF").Value).ToString().Trim();
                                            FolioNum = (System.Int32)(oRecordSet.Fields.Item("Folio").Value);
                                            FDocEntry = (System.Int32)(oRecordSet.Fields.Item("DocEntry").Value);
                                            FLineId = (System.Int32)(oRecordSet.Fields.Item("LineId").Value);

                                            if (FolioNum == 0)
                                                throw new Exception("No se ha encontrado número de Folio disponible");

                                            if (TaxIdNum == "")
                                                throw new Exception("Debe ingresar RUT de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1");

                                            if (oForm.BusinessObject.Type == "15")
                                                oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes));
                                            else if (oForm.BusinessObject.Type == "67")
                                                oTransfer = (SAPbobsCOM.StockTransfer)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer));
                                            else if (oForm.BusinessObject.Type == "1250000001")
                                                oTransfer = (SAPbobsCOM.StockTransfer)FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryTransferRequest);  // ##REVISAR
                                            else //21
                                                oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseReturns));

                                            if (oForm.BusinessObject.Type == "67" || oForm.BusinessObject.Type == "1250000001")
                                            {
                                                if (oTransfer.GetByKey(Convert.ToInt32(sDocEntry)))
                                                {
                                                    oTransfer.FolioNumber = FolioNum;
                                                    oTransfer.FolioPrefixString = "GE";
                                                    //oTransfer.Printed := BoYesNoEnum.tYES;

                                                    lRetCode = oTransfer.Update();
                                                    if (lRetCode != 0)
                                                    {
                                                        bFolioAsignado = false;
                                                        if (GlobalSettings.RunningUnderSQLServer)
                                                            s = "update [@VID_FEDISTD] set U_Estado = 'D' where DocEntry = {0} and LineId = {1}";
                                                        else
                                                            s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'D' where ""DocEntry"" = {0} and ""LineId"" = {1}";
                                                        s = String.Format(s, FDocEntry, FLineId);
                                                        oRecordSet.DoQuery(s);
                                                        s = FCmpny.GetLastErrorDescription();

                                                        FSBOApp.MessageBox("*****   No se ha asignado Folio al Documento   *****", 1, "Aceptar");

                                                        FSBOApp.StatusBar.SetText("No se ha asignado Folio al Documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                        OutLog("No se ha asignado Folio al Documento DocEntry: " + sDocEntry + " Tipo: " + oForm.BusinessObject.Type + " - " + FCmpny.GetLastErrorDescription());
                                                    }
                                                    else
                                                    {
                                                        if (GlobalSettings.RunningUnderSQLServer)
                                                            s = "update [@VID_FEDISTD] set U_Estado = 'U', U_DocEntry = {0}, U_ObjType = '{1}', U_SubType = '{2}' where DocEntry = {3} and LineId = {4}";
                                                        else
                                                            s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'U', ""U_DocEntry"" = {0}, ""U_ObjType"" = '{1}', ""U_SubType"" = '{2}' where ""DocEntry"" = {3} and ""LineId"" = {4}";
                                                        s = String.Format(s, sDocEntry, oForm.BusinessObject.Type, sDocSubType, FDocEntry, FLineId);
                                                        oRecordSet.DoQuery(s);
                                                        bFolioAsignado = true;

                                                        if (GlobalSettings.RunningUnderSQLServer)
                                                            s = @"update owtr set printed = 'Y', LPgFolioN = FolioNum where DocEntry = {0}";
                                                        else
                                                            s = @"update ""OWTR"" set ""Printed"" = 'Y', ""LPgFolioN"" = ""FolioNum"" where ""DocEntry"" = {0}";
                                                        s = String.Format(s, sDocEntry);
                                                        oRecordSet.DoQuery(s);

                                                        //ahora debo marcar que el folio fue usado y colocar los datos del documento que uso el folio
                                                        Reg = new TFunctions();
                                                        Reg.SBO_f = FSBOf;
                                                        //s = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value));
                                                        //s = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("LineId").Value));
                                                        //s = Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_Folio").Value));
                                                        //lRetCode = Reg.ActEstadoFolioUpt((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value), (System.Int32)(oRecordSet.Fields.Item("LineId").Value), (System.Double)(oRecordSet.Fields.Item("U_Folio").Value), TipoDocElec, sDocEntry, oForm.BusinessObject.Type, sDocSubType);

                                                        if (GeneraT == "Y")
                                                        {
                                                            //Colocar Timbre
                                                            XmlDocument xmlCAF = new XmlDocument();
                                                            XmlDocument xmlTimbre = new XmlDocument();
                                                            if (CAF == "")
                                                                throw new Exception("No se ha encontrado xml de CAF");
                                                            //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                            xmlCAF.LoadXml(CAF);
                                                            xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElec, Convert.ToString(oTransfer.FolioNumber), oTransfer.DocDate.ToString("yyyyMMdd"), LicTradNum.Replace(".",""), oTransfer.CardName, sDocTotal, oTransfer.Lines.ItemDescription, xmlCAF, TaxIdNum);

                                                            StringWriter sw = new StringWriter();
                                                            XmlTextWriter tx = new XmlTextWriter(sw);
                                                            xmlTimbre.WriteTo(tx);

                                                            s = sw.ToString();// 

                                                            if (s != "")
                                                            {
                                                                if (oTransfer.GetByKey(Convert.ToInt32(sDocEntry)))
                                                                {
                                                                    oTransfer.UserFields.Fields.Item("U_FETimbre").Value = s;
                                                                    lRetCode = oTransfer.Update();
                                                                    if (lRetCode != 0)
                                                                    {
                                                                        s = FCmpny.GetLastErrorDescription();
                                                                        FSBOApp.StatusBar.SetText("No se ha creado Timbre en el documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                                        OutLog("No se ha creado Timbre en el documento: " + sDocEntry + " Tipo: 67 - " + s + " - " + FCmpny.GetLastErrorDescription());
                                                                    }
                                                                    else
                                                                        FSBOApp.StatusBar.SetText("Se ha creado satisfactoriamente Timbre en el documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                                                }
                                                            }
                                                            else
                                                                FSBOApp.StatusBar.SetText("No se ha creado Timbre en el documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                        }

                                                        lRetCode = 1;
                                                        if (lRetCode != 0)
                                                        {
                                                            SBO_f = FSBOf;
                                                            if (oForm.BusinessObject.Type == "67")
                                                                EnviarFE_WebService(sDocEntry, sDocSubType, true, false, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52T", oForm.BusinessObject.Type, (FolioPortal == "Y" ? true : false));
                                                            else if (oForm.BusinessObject.Type == "1250000001")
                                                                EnviarFE_WebService(sDocEntry, sDocSubType, true, false, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52S", oForm.BusinessObject.Type, (FolioPortal == "Y" ? true : false));
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (oDocument.GetByKey(Convert.ToInt32(sDocEntry)))
                                                {
                                                    oDocument.FolioNumber = FolioNum;
                                                    oDocument.FolioPrefixString = "GE";
                                                    oDocument.Printed = PrintStatusEnum.psYes;

                                                    lRetCode = oDocument.Update();
                                                    if (lRetCode != 0)
                                                    {
                                                        bFolioAsignado = false;
                                                        if (GlobalSettings.RunningUnderSQLServer)
                                                            s = "update [@VID_FEDISTD] set U_Estado = 'D' where DocEntry = {0} and LineId = {1}";
                                                        else
                                                            s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'D' where ""DocEntry"" = {0} and ""LineId"" = {1}";
                                                        s = String.Format(s, FDocEntry, FLineId);
                                                        oRecordSet.DoQuery(s);
                                                        s = FCmpny.GetLastErrorDescription();

                                                        FSBOApp.MessageBox("*****   No se ha asignado Folio al Documento   *****", 1, "Aceptar");
                                                        FSBOApp.StatusBar.SetText("No se ha asignado Folio al Documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                        OutLog("No se ha asignado Folio al Documento DocEntry: " + sDocEntry + " Tipo: " + oForm.BusinessObject.Type + " - " + s + " - " + FCmpny.GetLastErrorDescription());
                                                    }
                                                    else
                                                    {
                                                        //ahora debo marcar que el folio fue usado y colocar los datos del documento que uso el folio
                                                        Reg = new TFunctions();
                                                        Reg.SBO_f = FSBOf;
                                                        //s = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value));
                                                        //s = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("LineId").Value));
                                                        //s = Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_Folio").Value));
                                                        //lRetCode = Reg.ActEstadoFolioUpt((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value), (System.Int32)(oRecordSet.Fields.Item("LineId").Value), (System.Double)(oRecordSet.Fields.Item("U_Folio").Value), TipoDocElec, sDocEntry, oForm.BusinessObject.Type, sDocSubType);

                                                        if (GlobalSettings.RunningUnderSQLServer)
                                                            s = "update [@VID_FEDISTD] set U_Estado = 'U', U_DocEntry = {0}, U_ObjType = '{1}', U_SubType = '{2}' where DocEntry = {3} and LineId = {4}";
                                                        else
                                                            s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'U', ""U_DocEntry"" = {0}, ""U_ObjType"" = '{1}', ""U_SubType"" = '{2}' where ""DocEntry"" = {3} and ""LineId"" = {4}";
                                                        s = String.Format(s, sDocEntry, oForm.BusinessObject.Type, sDocSubType, FDocEntry, FLineId);
                                                        oRecordSet.DoQuery(s);
                                                        bFolioAsignado = true;

                                                        if (GlobalSettings.RunningUnderSQLServer)
                                                            s = @"update {1} set printed = 'Y', LPgFolioN = FolioNum where DocEntry = {0}";
                                                        else
                                                            s = @"update ""{1}"" set ""Printed"" = 'Y', ""LPgFolioN"" = ""FolioNum"" where ""DocEntry"" = {0}";
                                                        s = String.Format(s, sDocEntry, (oForm.BusinessObject.Type == "15" ? "ODLN" : "ORPD"));
                                                        oRecordSet.DoQuery(s);

                                                        if (GeneraT == "Y")
                                                        {
                                                            //Colocar Timbre
                                                            XmlDocument xmlCAF = new XmlDocument();
                                                            XmlDocument xmlTimbre = new XmlDocument();
                                                            if (CAF == "")
                                                                throw new Exception("No se ha encontrado xml de CAF");
                                                            //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                            xmlCAF.LoadXml(CAF);
                                                            xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElec, Convert.ToString(oDocument.FolioNumber), oDocument.DocDate.ToString("yyyyMMdd"), oDocument.FederalTaxID.Replace(".",""), oDocument.CardName, Convert.ToString(Math.Round(oDocument.DocTotal, 0)), oDocument.Lines.ItemDescription, xmlCAF, TaxIdNum);

                                                            StringWriter sw = new StringWriter();
                                                            XmlTextWriter tx = new XmlTextWriter(sw);
                                                            xmlTimbre.WriteTo(tx);

                                                            s = sw.ToString();// 

                                                            if (s != "")
                                                            {
                                                                if (oDocument.GetByKey(Convert.ToInt32(sDocEntry)))
                                                                {
                                                                    oDocument.UserFields.Fields.Item("U_FETimbre").Value = s;
                                                                    lRetCode = oDocument.Update();
                                                                    if (lRetCode != 0)
                                                                    {
                                                                        FSBOApp.StatusBar.SetText("No se ha creado Timbre en el documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                                        OutLog("No se ha creado Timbre en el documento: " + sDocEntry + " Tipo: " + oForm.BusinessObject.Type + " - " + FCmpny.GetLastErrorDescription());
                                                                    }
                                                                    else
                                                                        FSBOApp.StatusBar.SetText("Se ha creado satisfactoriamente Timbre en el documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                                                }
                                                            }
                                                            else
                                                                FSBOApp.StatusBar.SetText("No se ha creado Timbre en el documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                        }

                                                        lRetCode = 1;
                                                        if (lRetCode != 0)
                                                        {
                                                            SBO_f = FSBOf;
                                                            if (oForm.BusinessObject.Type == "15")
                                                                EnviarFE_WebService(sDocEntry, sDocSubType, false, false, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52", oForm.BusinessObject.Type, (FolioPortal == "Y" ? true : false));
                                                            else //21 devolucion de compra
                                                                EnviarFE_WebService(sDocEntry, sDocSubType, false, false, true, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52D", oForm.BusinessObject.Type, (FolioPortal == "Y" ? true : false));
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        { FSBOApp.StatusBar.SetText("No se encuentra numeros disponibles para SBO", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                                    }
                                }
                                else
                                {
                                    if (Canceled == "N")
                                        FSBOApp.StatusBar.SetText("Documento creado no es electronico", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                }
                            }//Fin if Distribuido
                            else if ((Distribuido == "N") && (FolioPortal == "Y")) //Portal maneja el folio
                            {
                                if (oForm.BusinessObject.Type == "67" || oForm.BusinessObject.Type == "1250000001")
                                {
                                    _xmlDocument = new XmlDocument();
                                    _xmlDocument.LoadXml(BusinessObjectInfo.ObjectKey);
                                    N = _xmlDocument.SelectSingleNode("StockTransferParams");
                                    sDocEntry = (System.String)(N.InnerText).Trim();
                                }
                                else
                                    sDocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = @"select SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo'
                                                      ,T0.CANCELED
                                                 FROM {1} T0 WITH (NOLOCK)
                                                    JOIN OCRD C0 ON C0.CardCode = T0.CardCode
                                                    JOIN NNM1 T2 ON T0.Series = T2.Series 
                                                   WHERE T0.DocEntry = {0}";
                                else
                                    s = @"select SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo""
                                                          ,T0.""CANCELED""
                                                 FROM ""{1}"" T0
                                                    JOIN ""OCRD"" C0 ON C0.""CardCode"" = T0.""CardCode""
                                                    JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                                                   WHERE T0.""DocEntry"" = {0} ";
                                if (oForm.BusinessObject.Type == "15")
                                    s = String.Format(s, sDocEntry, "ODLN");
                                else if (oForm.BusinessObject.Type == "67")
                                    s = String.Format(s, sDocEntry, "OWTR");
                                else if (oForm.BusinessObject.Type == "1250000001")
                                    s = String.Format(s, sDocEntry, "OWTQ");
                                else //21 devolucion de compra
                                    s = String.Format(s, sDocEntry, "ORPD");
                                oRecordSet.DoQuery(s);
                                Tipo = (System.String)(oRecordSet.Fields.Item("Tipo").Value);
                                Canceled = (System.String)(oRecordSet.Fields.Item("CANCELED").Value);
                                if ((Tipo == "E") && (Canceled == "N"))
                                {
                                    if (oForm.BusinessObject.Type == "15")
                                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes));
                                    else if (oForm.BusinessObject.Type == "67")
                                        oTransfer = (SAPbobsCOM.StockTransfer)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer));
                                    else if (oForm.BusinessObject.Type == "1250000001")
                                        oTransfer = (SAPbobsCOM.StockTransfer)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryTransferRequest));
                                    else //21
                                        oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseReturns));

                                    if (oForm.BusinessObject.Type == "67" || oForm.BusinessObject.Type == "1250000001")
                                    {
                                        if (oTransfer.GetByKey(Convert.ToInt32(sDocEntry)))
                                        {
                                            if (GlobalSettings.RunningUnderSQLServer)
                                                s = "update {0} set Printed = 'Y' where DocEntry = {1}";
                                            else
                                                s = @"update ""{0}"" set ""Printed"" = 'Y' where ""DocEntry"" = {1}";
                                            s = String.Format(s, "OWTR", sDocEntry);
                                            oRecordSet.DoQuery(s);
                                        }
                                    }
                                    else
                                    {
                                        if (oDocument.GetByKey(Convert.ToInt32(sDocEntry)))
                                        {
                                            oDocument.Printed = PrintStatusEnum.psYes;
                                            lRetCode = oDocument.Update();
                                            if (lRetCode != 0)
                                            {
                                                if (GlobalSettings.RunningUnderSQLServer)
                                                    s = "update {1} set Printed = 'Y' where DocEntry = {0}";
                                                else
                                                    s = @"update ""{1}"" set ""Printed"" = 'Y' where ""DocEntry"" = {0}";
                                                if (oForm.BusinessObject.Type == "15")
                                                    s = String.Format(s, sDocEntry, "ODLN");
                                                else //21 devolucion de compra
                                                    s = String.Format(s, sDocEntry, "ORPD");
                                                oRecordSet.DoQuery(s);
                                            }
                                        }
                                    }

                                    SBO_f = FSBOf;
                                    if (oForm.BusinessObject.Type == "67")  //Transferencia
                                        EnviarFE_WebService(sDocEntry, "--", true, false, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52T", oForm.BusinessObject.Type, (FolioPortal == "Y" ? true : false));
                                    else if (oForm.BusinessObject.Type == "1250000001") //solicitud Transferencia
                                        EnviarFE_WebService(sDocEntry, "--", true, false, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52T", oForm.BusinessObject.Type, (FolioPortal == "Y" ? true : false));
                                    else if (oForm.BusinessObject.Type == "15") // Entrega
                                        EnviarFE_WebService(sDocEntry, "--", false, false, false, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52", oForm.BusinessObject.Type, (FolioPortal == "Y" ? true : false));
                                    else //21 devolucion de compra
                                        EnviarFE_WebService(sDocEntry, "--", false, false, true, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, "52D", oForm.BusinessObject.Type, (FolioPortal == "Y" ? true : false));
                                }
                            }
                        }
                        else
                        { FSBOApp.StatusBar.SetText("Debe Parametrizar el Addon", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                    }
                    else
                        Flag = true;
                }
            }
            catch (Exception e)
            {
                if ((bFolioDistribuido == true) && (bFolioAsignado == false) && (FDocEntry != 0) && (FLineId != -1))
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = "update [@VID_FEDISTD] set U_Estado = 'D' where DocEntry = {0} and LineId = {1}";
                    else
                        s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'D' where ""DocEntry"" = {0} and ""LineId"" = {1}";
                    s = String.Format(s, FDocEntry, FLineId);
                    oRecordSet.DoQuery(s);
                }

                OutLog("FormDataEvent - " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }

        }//fin FormDataEvent


        public new void PrintEvent(ref SAPbouiCOM.PrintEventInfo eventInfo, ref Boolean BubbleEvent)
        {
            //XmlDocument _xmlDocument;
            //XmlNode N;

            base.PrintEvent(ref eventInfo, ref BubbleEvent);

        }//fin PrintEvent


        public new void ReportDataEvent(ref SAPbouiCOM.ReportDataInfo eventInfo, ref Boolean BubbleEvent)
        {
            base.ReportDataEvent(ref eventInfo, ref BubbleEvent);

            
        }//fin ReportDataEvent


        public void EnviarFE_WebService(String DocEntry, String SubType, Boolean bTransferencia, Boolean RImprimir, Boolean bDevolucion, Boolean bMultiSoc, String nMultiSoc, String GLOB_EncryptSQL, Boolean RunningUnderSQLServer, String TipoDocElecAddon, String ObjType, Boolean bFPortal)
        {
            String URL;
            XmlDocument oXml = null;
            XDocument miXML = null;
            XElement xNodo;
            String sXML = "";
            String userED = "";
            String passED = "";
            Boolean DocElec;
            SAPbobsCOM.Documents oDocument = null;
            SAPbobsCOM.StockTransfer oTransfer = null;
            TFunctions Reg = new TFunctions();
            Reg.SBO_f = SBO_f;
            String sCnn;
            Boolean bExento = false;
            String TipoDocElec = "52";
            SAPbobsCOM.Company Cmpny = SBO_f.Cmpny;
            SAPbobsCOM.Recordset ors = ((SAPbobsCOM.Recordset)Cmpny.GetBusinessObject(BoObjectTypes.BoRecordset));
            SAPbobsCOM.Recordset ors2 = ((SAPbobsCOM.Recordset)Cmpny.GetBusinessObject(BoObjectTypes.BoRecordset));
            String Tabla;
            String Status;
            String sMessage = "";
            String jStatus = "";
            String jCodigo = "";
            String jDescripcion = "";
            String jFolio = "";
            Int32 lRetCode;
            String DocDate = "";
            String ProcE = "";
            String ProcD = "";
            String ProcR = "";
            String TaxIdNum = "";
            String OP18 = "";
            String OP8 = "";
            String URLPDF = "";
            Boolean bPaso;
            String MostrarXML = "N";
            TDLLparaXML Dll = new TDLLparaXML();
            Dll.SBO_f = SBO_f;
            String URLPDFConstruyeApirest = "http://rest1.easydoc.cl/api/Dte/ObtenerPdf";
            //ºº

            try
            {
                string aux = ObjType;

                if (ObjType == "21") //Devolucion
                    Tabla = "ORPD";
                else if (ObjType == "67") //Transferencia
                    Tabla = "OWTR";
                else if (ObjType == "15")  //Entrega
                    Tabla = "ODLN";
                else
                    Tabla = "OWTQ"; //Solicitud Traslado

                //Solicitud traslado

                if (RunningUnderSQLServer)
                    s = @"SELECT U_httpBol 'URL', ISNULL(U_UserWSCL,'') 'User', ISNULL(U_PassWSCL,'') 'Pass', REPLACE(ISNULL(TaxIdNum,''),'.','') TaxIdNum 
                               , ISNULL(U_OP18,'') 'OP18', ISNULL(U_OP8,'') 'OP8', ISNULL(U_URLPDF,'') 'URLPDF', ISNULL(U_MostrarXML,'N') 'MostrarXML', ISNULL(U_Safepdf,'') 'ObtPdf'
                           FROM [@VID_FEPARAM] T0, OADM A0";
                else
                    s = @"SELECT ""U_httpBol"" ""URL"", IFNULL(""U_UserWSCL"",'') ""User"", IFNULL(""U_PassWSCL"",'') ""Pass"", REPLACE(IFNULL(""TaxIdNum"",''),'.','') ""TaxIdNum"" 
                               , IFNULL(""U_OP18"",'') ""OP18"", IFNULL(""U_OP8"",'') ""OP8"", IFNULL(""U_URLPDF"",'') ""URLPDF"", IFNULL(""U_MostrarXML"",'N') ""MostrarXML"", IFNULL(""U_Safepdf"",'') ""ObtPdf"" 
                           FROM ""@VID_FEPARAM"" T0, ""OADM"" A0 ";

                ors.DoQuery(s);
                if (ors.RecordCount == 0)
                    SBO_f.SBOApp.StatusBar.SetText("No se ha ingresado URL", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                else if (((System.String)ors.Fields.Item("URL").Value).Trim() == "")
                    SBO_f.SBOApp.StatusBar.SetText("No se ha ingresado URL", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                else if ((System.String)(ors.Fields.Item("OP18").Value).ToString().Trim() == "")
                    SBO_f.SBOApp.StatusBar.SetText("No se encuentra URL para OP ejecutar DTE en Portal", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                //else if (((System.String)ors.Fields.Item("User").Value).Trim() == "")
                //   throw new Exception("No se encuentra usuario en Parametros");
                //else if (((System.String)ors.Fields.Item("Pass").Value).Trim() == "")
                //    throw new Exception("No se encuentra password en Parametros");
                else
                {
                    userED = Reg.DesEncriptar((System.String)(ors.Fields.Item("User").Value).ToString().Trim());
                    passED = Reg.DesEncriptar((System.String)(ors.Fields.Item("Pass").Value).ToString().Trim());
                    TaxIdNum = (System.String)(ors.Fields.Item("TaxIdNum").Value).ToString().Trim();
                    MostrarXML = ((System.String)ors.Fields.Item("MostrarXML").Value).Trim();

                    //validar que exista procedimentos para tipo documento
                    URL = ((System.String)ors.Fields.Item("URL").Value).Trim();
                    URLPDFConstruyeApirest = ((System.String)ors.Fields.Item("ObtPdf").Value).ToString().Trim();
                    if (bFPortal)
                    {
                        if ((System.String)(ors.Fields.Item("OP8").Value).ToString().Trim() == "")
                            throw new Exception("No se encuentra URL para OP recupera Timbre en Portal");
                        else if ((System.String)(ors.Fields.Item("URLPDF").Value).ToString().Trim() == "")
                            throw new Exception("No se encuentra URL para OP ejecutar DTE en Portal");
                    }
                    OP18 = ((System.String)ors.Fields.Item("OP18").Value).ToString().Trim();
                    OP8 = ((System.String)ors.Fields.Item("OP8").Value).ToString().Trim();
                    URLPDF = ((System.String)ors.Fields.Item("URLPDF").Value).ToString().Trim();

                    if (RunningUnderSQLServer)
                        s = @"SELECT ISNULL(U_ProcNomE,'') 'ProcNomE', ISNULL(U_ProcNomD,'') 'ProcNomD', ISNULL(U_ProcNomR,'') 'ProcNomR' 
                               FROM [@VID_FEPROCED] where ISNULL(U_Habili,'N') = 'Y' and U_TipoDoc = '{0}'";
                    else
                        s = @"SELECT IFNULL(""U_ProcNomE"",'') ""ProcNomE"", IFNULL(""U_ProcNomD"",'') ""ProcNomD"", IFNULL(""U_ProcNomR"",'') ""ProcNomR"" 
                               FROM ""@VID_FEPROCED"" where IFNULL(""U_Habili"",'N') = 'Y' and ""U_TipoDoc"" = '{0}'";

                    s = String.Format(s, TipoDocElec);
                    ors.DoQuery(s);
                    if (ors.RecordCount == 0)
                        throw new Exception("No se encuentra procedimientos para Documento electronico " + TipoDocElec);
                    else if (((System.String)ors.Fields.Item("ProcNomE").Value).Trim() == "")
                        throw new Exception("No se encuentra procedimiento Encabezado para Documento electronico " + TipoDocElec);
                    else if (((System.String)ors.Fields.Item("ProcNomD").Value).Trim() == "")
                        throw new Exception("No se encuentra procedimiento Detalle para Documento electronico " + TipoDocElec);
                    //else if ((((System.String)ors.Fields.Item("ProcNomR").Value).Trim() == "") && (TipoDocElec == "56"))
                    //    throw new Exception("No se encuentra procedimiento Referencia para Documento electronico " + TipoDocElec);
                    else
                    {
                        ProcE = ((System.String)ors.Fields.Item("ProcNomE").Value).Trim();
                        ProcD = ((System.String)ors.Fields.Item("ProcNomD").Value).Trim();
                        ProcR = ((System.String)ors.Fields.Item("ProcNomR").Value).Trim();
                    }

                    bExento = false;
                    TipoDocElec = "52";
                    bPaso = false;
                    if (bTransferencia)
                    {
                       if (ObjType == "67")
                            oTransfer = (SAPbobsCOM.StockTransfer)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer));
                       else
                           oTransfer = (SAPbobsCOM.StockTransfer)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryTransferRequest));
                        if (oTransfer.GetByKey(Convert.ToInt32(DocEntry)))
                        {

                            bPaso = true;
                            DocDate = SBO_f.DateToStr(oTransfer.DocDate);
                        }
                    }
                    else
                    {
                        if (bDevolucion)
                            oDocument = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseReturns));
                        else
                            oDocument = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes));

                        if (oDocument.GetByKey(Convert.ToInt32(DocEntry)))
                        {
                            bPaso = true;
                            DocDate = SBO_f.DateToStr(oDocument.DocDate);
                        }
                    }


                    if (!bPaso)
                        SBO_f.SBOApp.StatusBar.SetText("No se ha encontrado documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    else
                    {
                        //PARA ENCABEZADO
                        if (RunningUnderSQLServer)
                            s = @"EXEC {0} {1}, '{2}', '{3}'";//Factura
                        else
                            s = @"CALL {0} ({1}, '{2}', '{3}')";//Factura    
                        if (bTransferencia)
                            s = String.Format(s, ProcE, oTransfer.DocEntry, TipoDocElec, ObjType);
                        else
                            s = String.Format(s, ProcE, oDocument.DocEntry, TipoDocElec, ObjType);

                        ors.DoQuery(s);
                        if (ors.RecordCount == 0)
                            throw new Exception("No se encuentra datos de encabezado para Documento electronico " + TipoDocElec);

                        //para impuestos adicionales
                        if (((System.Double)ors.Fields.Item("MntImpAdic").Value) > 0)
                        {
                            if (RunningUnderSQLServer)
                                s = @"SELECT SUM (MontoImptoAdic) 'MontoImptoAdic', CodImpAdic, PorcImptoAdic
		                            FROM  VID_VW_FE_52_D
				                    WHERE DocEntry = {0}
		                            AND ObjType = '{1}'
                                    GROUP BY CodImpAdic, PorcImptoAdic";                            
                            else
                            s = @"SELECT SUM (""MontoImptoAdic"") ""MontoImptoAdic"", ""CodImpAdic"", ""PorcImptoAdic""
		                            FROM VID_VW_FE_52_D
				                    WHERE ""DocEntry"" = {0}
		                            AND ""ObjType"" = '{1}'
                                    GROUP BY ""CodImpAdic"",""PorcImptoAdic"" ";
                            if (bTransferencia)
                                s = String.Format(s, oTransfer.DocEntry, ObjType);
                            else
                                s = String.Format(s, oDocument.DocEntry, ObjType);
                            ors2.DoQuery(s);
                        }

                        miXML = null;
                        miXML = new XDocument(
                                             new XDeclaration("1.0", "utf-8", "yes"),
                            //new XComment("Lista de Alumnos"),
                                                new XElement("DTE",
                                                    new XElement("Documento")));
                        sXML = Dll.GenerarXMLStringDelivery(ref ors, ref ors2, TipoDocElec, ref miXML, "E");
                        if (sXML == "")
                            throw new Exception("Problema para generar xml Documento electronico " + TipoDocElec);


                        //PARA DETALLE
                        if (RunningUnderSQLServer)
                            s = @"exec {0} {1}, '{2}', '{3}'";//Factura
                        else
                            s = @"call {0} ({1}, '{2}', '{3}')";//Factura    
                        if (bTransferencia)
                            s = String.Format(s, ProcD, oTransfer.DocEntry, TipoDocElec, ObjType);
                        else
                            s = String.Format(s, ProcD, oDocument.DocEntry, TipoDocElec, ObjType);

                        ors.DoQuery(s);
                        if (ors.RecordCount == 0)
                            throw new Exception("No se encuentra datos de Detalle para Documento electronico (Detalle)" + TipoDocElec);
                        sXML = Dll.GenerarXMLStringDelivery(ref ors, ref ors2, TipoDocElec, ref miXML, "D");
                        if (sXML == "")
                            throw new Exception("Problema para generar xml Documento electronico (Detalle)" + TipoDocElec);


                        //PARA REFERENCIA
                        if (ProcR != "")
                        {
                            if (RunningUnderSQLServer)
                                s = @"exec {0} {1}, '{2}', '{3}'";//Factura
                            else
                                s = @"call {0} ({1}, '{2}', '{3}')";//Factura    
                            if (bTransferencia)
                                s = String.Format(s, ProcR, oTransfer.DocEntry, TipoDocElec, ObjType);
                            else
                                s = String.Format(s, ProcR, oDocument.DocEntry, TipoDocElec, ObjType);

                            ors.DoQuery(s);
                            if ((ors.RecordCount == 0) && (TipoDocElec == "56"))
                                throw new Exception("No se encuentra datos de Referencia para Documento electronico (Referencia)" + TipoDocElec);
                            if (ors.RecordCount > 0)
                            {
                                sXML = Dll.GenerarXMLStringDelivery(ref ors, ref ors2, TipoDocElec, ref miXML, "R");
                                if (sXML == "")
                                    throw new Exception("Problema para generar xml Documento electronico (Referencia)" + TipoDocElec);
                            }

                        }

                        var bImpresion = false;

                        //Cargar PDF
                        if (!bFPortal)
                        {

                            /*ºº aca preguntare por el parametro para construir o no el PDF 
                            if (bTransferencia)
                                s = Reg.PDFenString(TipoDocElecAddon, oTransfer.DocEntry.ToString(), ObjType, "", oTransfer.FolioNumber.ToString(), RunningUnderSQLServer, "CL");
                            else
                                s = Reg.PDFenString(TipoDocElecAddon, oDocument.DocEntry.ToString(), ObjType, "", oDocument.FolioNumber.ToString(), RunningUnderSQLServer, "CL");

                            if (s == "")
                                throw new Exception("No se ha creado PDF");

                            //Agrega el PDF al xml
                            xNodo = new XElement("Anexo",
                                                            new XElement("PDF", s));
                            miXML.Descendants("DTE").LastOrDefault().Add(xNodo);
                             *   * ºº */
                        }

                        //Pasar a xmlDocument
                        oXml = new XmlDocument();
                        using (var xmlReader = miXML.CreateReader())
                        {
                            oXml.Load(xmlReader);
                        }

                        //Agrega Timbre electronico
                        if (!bFPortal)
                        {
                            if (bTransferencia)
                            {
                                if (((System.String)oTransfer.UserFields.Fields.Item("U_FETimbre").Value).Trim() != "")
                                {
                                    s = oXml.InnerXml;
                                    s = s.Replace("</DTE>", ((System.String)oTransfer.UserFields.Fields.Item("U_FETimbre").Value).Trim()) + "</DTE>";
                                    oXml.LoadXml(s);
                                }
                            }
                            else
                            {
                                if (((System.String)oDocument.UserFields.Fields.Item("U_FETimbre").Value).Trim() != "")
                                {
                                    s = oXml.InnerXml;
                                    s = s.Replace("</DTE>", ((System.String)oDocument.UserFields.Fields.Item("U_FETimbre").Value).Trim()) + "</DTE>";
                                    oXml.LoadXml(s);
                                }
                            }
                        }

                        if (MostrarXML == "Y")
                            SBO_f.oLog.OutLog(oXml.InnerXml);
                        s = Reg.UpLoadDocumentByUrl(oXml, RunningUnderSQLServer, URL, userED, passED);
                        var results = JsonConvert.DeserializeObject<dynamic>(s);
                        jStatus = results.Status;
                        jCodigo = results.Codigo;
                        jDescripcion = results.Descripcion;
                        jFolio = results.Folio;

                        if (jCodigo != "00")
                        {
                            SBO_f.SBOApp.StatusBar.SetText("Error envio, " + jDescripcion, SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            if (jDescripcion.Substring(0, 19) == "Documento ya existe")
                                Status = "RR";
                            else
                            {
                                Status = "EE";
                                var sPath = System.AppDomain.CurrentDomain.BaseDirectory;
                                if (bFPortal)
                                {
                                    if (bTransferencia)
                                        sPath = sPath + "\\" + TipoDocElec + "-" + oTransfer.DocNum.ToString() + ".xml";
                                    else
                                        sPath = sPath + "\\" + TipoDocElec + "-" + oDocument.DocNum.ToString() + ".xml";
                                }
                                else
                                {
                                    if (bTransferencia)
                                        sPath = sPath + "\\" + TipoDocElec + "-" + oTransfer.FolioNumber.ToString() + ".xml";
                                    else
                                        sPath = sPath + "\\" + TipoDocElec + "-" + oDocument.FolioNumber.ToString() + ".xml";
                                }
                                oXml.Save(sPath);
                            }
                            sMessage = jDescripcion;
                            if (sMessage == "")
                                sMessage = "Error envio documento electronico a EasyDot";
                        }
                        else
                        {
                            Status = "EC";
                            sMessage = "Enviado satisfactoriamente a EasyDot";
                            SBO_f.SBOApp.StatusBar.SetText("Se ha enviado satisfactoriamente el documento electronico", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                            if (bFPortal)
                            {
                                //enviar a WS 18, rescatar timbre yield luego Enviar PDF
                                if (jFolio == "0")
                                {
                                    bImpresion = false;
                                    SBO_f.SBOApp.StatusBar.SetText("No se ha recibido folio desde el Portal", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                }
                                else
                                {
                                    //Consulta estado al portal
                                    //OP18 = @"http://portal1.easydoc.cl/Consulta/GeneracionDte.aspx?RUT={0}&amp;FOLIO={1}&amp;TIPODTE={2}&amp;OP=18";
                                    OP18 = OP18.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                                    OP18 = OP18.Replace("{1}", jFolio);
                                    OP18 = OP18.Replace("{2}", TipoDocElec);
                                    OP18 = OP18.Replace("&amp;", "&");

                                    WebRequest request = WebRequest.Create(OP18);
                                    if ((userED != "") && (passED != ""))
                                        request.Credentials = new NetworkCredential(userED, passED);
                                    request.Method = "POST";
                                    string postData = "";//** xmlDOC.InnerXml;
                                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                                    request.ContentType = "text/xml";
                                    request.ContentLength = byteArray.Length;
                                    Stream dataStream = request.GetRequestStream();
                                    dataStream.Write(byteArray, 0, byteArray.Length);
                                    dataStream.Close();
                                    WebResponse response = request.GetResponse();
                                    Console.WriteLine(((HttpWebResponse)(response)).StatusDescription);
                                    dataStream = response.GetResponseStream();
                                    StreamReader reader = new StreamReader(dataStream);
                                    string responseFromServer = reader.ReadToEnd();
                                    reader.Close();
                                    dataStream.Close();
                                    response.Close();
                                    var Responde18 = responseFromServer;
                                    request = null;
                                    response = null;
                                    dataStream = null;
                                    reader = null;
                                    GC.Collect();
                                    GC.WaitForPendingFinalizers();
                                    var FolPref = "GD";

                                    if (bTransferencia)
                                    {
                                        oTransfer.FolioPrefixString = FolPref;
                                        oTransfer.FolioNumber = Convert.ToInt32(jFolio);
                                        lRetCode = oTransfer.Update();
                                    }
                                    else
                                    {
                                        oDocument.FolioPrefixString = FolPref;
                                        oDocument.FolioNumber = Convert.ToInt32(jFolio);
                                        lRetCode = oDocument.Update();
                                    }
                                    if (lRetCode != 0)
                                    {
                                        if (RunningUnderSQLServer)
                                            s = @"UPADTE {0} SET FolioPref = '{1}', FolioNum = {2} WHERE DocEntry = {3}";
                                        else
                                            s = @"UPADTE {0} SET ""FolioPref"" = '{1}', ""FolioNum"" = {2} WHERE ""DocEntry"" = {3}";
                                        s = String.Format(s, Tabla, FolPref, jFolio, (bTransferencia ? oTransfer.DocEntry : oDocument.DocEntry));
                                        ors.DoQuery(s);
                                    }

                                    if (Responde18 != "OK")
                                    {
                                        SBO_f.SBOApp.StatusBar.SetText("No se ha logrado procesar documento en el portal, " + Responde18, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        Status = "EE";
                                        sMessage = Responde18;
                                        bImpresion = false;
                                    }
                                    else
                                    {
                                        //OP8 = @"http://portal1.easydoc.cl/Consulta/GeneracionDte.aspx?RUT={0}&amp;FOLIO={1}&amp;TIPODTE={2}&amp;OP=8";
                                        OP8 = OP8.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                                        OP8 = OP8.Replace("{1}", jFolio);
                                        OP8 = OP8.Replace("{2}", TipoDocElec);
                                        OP8 = OP8.Replace("&amp;", "&");

                                        WebRequest request8 = WebRequest.Create(OP8);
                                        if ((userED != "") && (passED != ""))
                                            request8.Credentials = new NetworkCredential(userED, passED);
                                        request8.Method = "POST";
                                        string postData8 = "";//** xmlDOC.InnerXml;
                                        byte[] byteArray8 = Encoding.UTF8.GetBytes(postData8);
                                        request8.ContentType = "text/xml";
                                        request8.ContentLength = byteArray8.Length;
                                        Stream dataStream8 = request8.GetRequestStream();
                                        dataStream8.Write(byteArray8, 0, byteArray8.Length);
                                        dataStream8.Close();
                                        WebResponse response8 = request8.GetResponse();
                                        Console.WriteLine(((HttpWebResponse)(response8)).StatusDescription);
                                        dataStream8 = response8.GetResponseStream();
                                        StreamReader reader8 = new StreamReader(dataStream8);
                                        string responseFromServer8 = reader8.ReadToEnd();
                                        reader8.Close();
                                        dataStream8.Close();
                                        response8.Close();
                                        var Response8 = responseFromServer8;

                                        if (Response8 == "")
                                        {
                                            SBO_f.SBOApp.StatusBar.SetText("No se ha logrado recuperar Timbre electronico desde el portal", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                            bImpresion = false;
                                        }
                                        else
                                        {
                                            if (RunningUnderSQLServer)
                                                s = @"UPDATE {0} SET U_FETimbre = '{1} WHERE DocEntry = {2}";
                                            else
                                                s = @"UPDATE {0} SET ""U_FETimbre"" = '{1}' WHERE ""DocEntry"" = {2}";
                                            s = String.Format(s, Tabla, Response8, (bTransferencia ? oTransfer.DocEntry : oDocument.DocEntry));
                                            ors.DoQuery(s);

                                            //URL_PDF = @"http://rest.easydoc.cl/api/Dte/SavePdf";
                                            //Cargar PDF
                                            // ºº var sPDF = Reg.PDFenString(TipoDocElecAddon, (bTransferencia ? oTransfer.DocEntry.ToString() : oDocument.DocEntry.ToString()), ObjType, "", jFolio, RunningUnderSQLServer, "CL");
                                            var sPDF = ""; //ººeliminar cuadno se agregue el parametro
                                            if (sPDF == "")
                                                throw new Exception("No se ha creado PDF");
                                            var sjson = @"""RUTEmisor"":""{0}"", " + Environment.NewLine + @"""TipoDTE"":""{1}"", " + Environment.NewLine + @"""Folio"":{2}," + Environment.NewLine + @"""Pdf"":""{3}""";
                                            sjson = String.Format(sjson, TaxIdNum.Replace("-", "").Replace(".", ""), TipoDocElec, jFolio, sPDF);
                                            sjson = "{" + Environment.NewLine + sjson + Environment.NewLine + "}";
                                            /*var sjson = @"""RUTEmisor"":""{0}"", " + @"""TipoDTE"":""{1}"", " + @"""Folio"":{2}," + @"""Pdf"":""{3}""";
                                            sjson = String.Format(sjson, TaxIdNum.Replace("-", "").Replace(".", ""), TipoDocElec, jFolio, sPDF);
                                            sjson = "{" + sjson + "}";*/
                                            s = Reg.UpLoadDocumentByUrlAPI(null, sjson, RunningUnderSQLServer, URLPDF, userED, passED, TipoDocElec + "_" + jFolio);
                                            //s = Reg.UpLoadDocumentByUrl2(null, sjson, RunningUnderSQLServer, URL_PDF, userED, passED, TipoDocElec + "_" + jFolio);
                                            var resultsAPI = JsonConvert.DeserializeObject<dynamic>(s);
                                            var jStatusAPI = resultsAPI.Status;
                                            var jDescripcionAPI = resultsAPI.Descripcion;

                                            if (jStatusAPI.Value == "OK")
                                                SBO_f.SBOApp.StatusBar.SetText("PDF enviado al portal", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                            else
                                            {
                                                SBO_f.SBOApp.StatusBar.SetText("PDF no se ha enviado al portal, " + ((System.String)jDescripcionAPI.Value).Trim(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                SBO_f.oLog.OutLog("PDF no se ha enviado al portal, Tipo Doc " + TipoDocElec + ", Folio " + jFolio + " -> " + ((System.String)jDescripcionAPI.Value).Trim());
                                            }
                                            //Guardar TED y pasar Pdf a string y luego enviarlo al portal con funcion usada en Peru

                                            //*********************falta enviar pdf y antes guardar el TED para que tenga timbre electronico
                                            bImpresion = true;
                                        }
                                    }
                                }
                            }//if (bFPortal)
                            else
                            {
                                OP18 = OP18.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                                if (bTransferencia)
                                    OP18 = OP18.Replace("{1}", oTransfer.FolioNumber.ToString());
                                else
                                    OP18 = OP18.Replace("{1}", oDocument.FolioNumber.ToString());
                                OP18 = OP18.Replace("{2}", TipoDocElec);
                                OP18 = OP18.Replace("&amp;", "&");

                                WebRequest request = WebRequest.Create(OP18);
                                if ((userED != "") && (passED != ""))
                                    request.Credentials = new NetworkCredential(userED, passED);
                                request.Method = "POST";
                                string postData = "";//** xmlDOC.InnerXml;
                                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                                request.ContentType = "text/xml";
                                request.ContentLength = byteArray.Length;
                                Stream dataStream = request.GetRequestStream();
                                dataStream.Write(byteArray, 0, byteArray.Length);
                                dataStream.Close();
                                WebResponse response = request.GetResponse();
                                Console.WriteLine(((HttpWebResponse)(response)).StatusDescription);
                                dataStream = response.GetResponseStream();
                                StreamReader reader = new StreamReader(dataStream);
                                string responseFromServer = reader.ReadToEnd();
                                reader.Close();
                                dataStream.Close();
                                response.Close();
                                var Responde18 = responseFromServer;
                                request = null;
                                response = null;
                                dataStream = null;
                                reader = null;
                                GC.Collect();
                                GC.WaitForPendingFinalizers();

                                if (Responde18 != "OK")
                                {
                                    SBO_f.SBOApp.StatusBar.SetText("No se ha logrado procesar documento en el portal, " + Responde18, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    Status = "EE";
                                    sMessage = Responde18;
                                    bImpresion = false;
                                }
                                else
                                    bImpresion = true;
                                //ººpreguntar por el parametro que indica si reporte lo construye el portal
                                // caso afirmativo: http://rest1.easydoc.cl/api/Dte/ObtenerPdf 
                                var sjson = @"""RUTEmisor"":""{0}"", " + Environment.NewLine + @"""TipoDTE"":""{1}"", " + Environment.NewLine + @"""Folio"":{2},";
                                sjson = String.Format(sjson, TaxIdNum.Replace("-", "").Replace(".", ""), TipoDocElec, jFolio);
                                sjson = "{" + Environment.NewLine + sjson + Environment.NewLine + "}";
                                s = Reg.UpLoadDocumentByUrlAPI(null, sjson, RunningUnderSQLServer, URLPDFConstruyeApirest, userED, passED, TipoDocElec + "_" + jFolio);
                                var resultsAPI = JsonConvert.DeserializeObject<dynamic>(s);
                                var jpdf = resultsAPI.Pdf;
                                var jFolioApi = resultsAPI.Folio;
                                if (jpdf.Value != null)
                                {
                                    int rest = Reg.Attachments(System.Convert.FromBase64String(jpdf.Value), Cmpny, TipoDocElec, jFolio);

                                    if (rest > 0)
                                    {
                                        if (bTransferencia)
                                        {
                                           oTransfer.AttachmentEntry = rest;
                                           lRetCode = oTransfer.Update();
                                        }
                                        else
                                        {
                                            oDocument.AttachmentEntry = rest;
                                            lRetCode = oDocument.Update();
                                        }
                                        if (lRetCode != 0)
                                        {
                                            SBO_f.SBOApp.StatusBar.SetText("No se ha logrado adjuntar documento ", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                        }
                                        else
                                        {
                                            SBO_f.SBOApp.StatusBar.SetText("Documento Adjunto ", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_None);
                                        }
                                    }

                                }
                                //ºº
                            }
                            oXml = null;
                        }

                        if (RunningUnderSQLServer)
                            s = "SELECT DocEntry, U_Status FROM [@VID_FELOG] WITH (NOLOCK) WHERE U_DocEntry = {0} AND U_ObjType = '{1}' AND U_SubType = '{2}'";
                        else
                            s = @"SELECT ""DocEntry"", ""U_Status"" FROM ""@VID_FELOG"" WHERE ""U_DocEntry"" = {0} AND ""U_ObjType"" = '{1}' AND ""U_SubType"" = '{2}' ";
                        if (bTransferencia)
                            s = String.Format(s, oTransfer.DocEntry, ObjType, SubType);
                        else
                            s = String.Format(s, oDocument.DocEntry, ObjType, SubType);
                        ors.DoQuery(s);
                        if (ors.RecordCount == 0)
                        {
                            if (bTransferencia)
                                Reg.FELOGAdd(oTransfer.DocEntry, ObjType, SubType, "", oTransfer.FolioNumber, Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", "", "", DocDate);
                            else
                                Reg.FELOGAdd(oDocument.DocEntry, ObjType, SubType, "", oDocument.FolioNumber, Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", "", "", DocDate);
                        }
                        else
                        {
                            if ((System.String)(ors.Fields.Item("U_Status").Value) != "RR")
                            {
                                SBO_f.SBOApp.StatusBar.SetText("Documento se ha enviado a EasyDot", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                if (bTransferencia)
                                    Reg.FELOGUptM((System.Int32)(ors.Fields.Item("DocEntry").Value), oTransfer.DocEntry, ObjType, SubType, "", oTransfer.FolioNumber, Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", "", "", DocDate);
                                else
                                    Reg.FELOGUptM((System.Int32)(ors.Fields.Item("DocEntry").Value), oDocument.DocEntry, ObjType, SubType, "", oDocument.FolioNumber, Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", "", "", DocDate);
                            }
                            else
                                SBO_f.SBOApp.StatusBar.SetText("Documento ya se ha enviado anteriormente a EasyDot", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                        }

                        if (Status == "EC")
                        {
                            if (bTransferencia)
                            {
                                oTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                lRetCode = oTransfer.Update();
                            }
                            else
                            {
                                oDocument.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                lRetCode = oDocument.Update();
                            }
                        }
                        else if (Status == "RR")
                        {
                            if (bTransferencia)
                            {
                                oTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                lRetCode = oTransfer.Update();
                            }
                            else
                            {
                                oDocument.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                lRetCode = oDocument.Update();
                            }
                        }
                        else
                        {
                            if (bTransferencia)
                            {
                                oTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "N";
                                lRetCode = oTransfer.Update();
                            }
                            else
                            {
                                oDocument.UserFields.Fields.Item("U_EstadoFE").Value = "N";
                                lRetCode = oDocument.Update();
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("EnviarFE " + e.Message + " ** Trace: " + e.StackTrace);
                SBO_f.SBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");  // Captura errores no manejados
            }
        }//fin EnviarFE

        //para pruebas
        private String GenerarXMLStringDeliveryx(ref SAPbobsCOM.Recordset ors, ref SAPbobsCOM.Recordset ors2, String TipoDocElec, ref XDocument miXML, String Sector)
        {
            Int32 i;
            XElement xNodo = null;

            try
            {
                if (Sector == "E")
                {
                    ors.MoveFirst();
                    xNodo = new XElement("Encabezado",
                                            new XElement("IdDoc",
                                                     new XElement("FchEmis", ((System.String)ors.Fields.Item("FchEmis").Value).Trim()),
                                                     new XElement("FchVenc", ((System.String)ors.Fields.Item("FchVenc").Value).Trim()),
                                                     new XElement("TipoDTE", ((System.String)ors.Fields.Item("TipoDTE").Value).Trim()),
                                                     new XElement("Folio", ((System.Int32)ors.Fields.Item("Folio").Value)),
                                                     //new XElement("IndServicio", ((System.String)ors.Fields.Item("IndServicio").Value).Trim()),
                                                     new XElement("IndTraslado", ((System.String)ors.Fields.Item("IndTraslado").Value).Trim()),
                                                     new XElement("TipoDespacho", ((System.String)ors.Fields.Item("TipoDespacho").Value).Trim()),
                                                     new XElement("MntBruto", ((System.Double)ors.Fields.Item("MntBruto").Value)),
                                                     new XElement("MntCancel", ((System.Double)ors.Fields.Item("MntCancel").Value)),
                                                     new XElement("SaldoInsol", ((System.Double)ors.Fields.Item("SaldoInsol").Value)),
                                                     new XElement("TpoTranCompra", ((System.String)ors.Fields.Item("TpoTranCompra").Value)),
                                                     new XElement("TpoTranVenta", ((System.String)ors.Fields.Item("TpoTranVenta").Value))
                        //new XElement("Telefono", ((System.String)ors.Fields.Item("").Value).Trim())
                                                     ),
                                            new XElement("Emisor",
                        //new XElement("CdgSIISucur", ((System.String)ors.Fields.Item("").Value).Trim()),
                                                     new XElement("CdgVendedor", ((System.String)ors.Fields.Item("CdgVendedor").Value).Trim()),
                                                     new XElement("RUTEmisor", ((System.String)ors.Fields.Item("RUTEmisor").Value).Trim()),
                                                     new XElement("RznSoc", ((System.String)ors.Fields.Item("RznSocial").Value).Trim()),
                                                     new XElement("GiroEmis", ((System.String)ors.Fields.Item("GiroEmis").Value).Trim()),
                                                     new XElement("Sucursal", ((System.String)ors.Fields.Item("Sucursal").Value).Trim()),
                                                     new XElement("Telefono", ((System.String)ors.Fields.Item("Telefono").Value).Trim()),
                                                     new XElement("CdgSIISucur", ((System.String)ors.Fields.Item("CdgSIISucur").Value).Trim()),
                                                     new XElement("Sucursal", ((System.String)ors.Fields.Item("SucursalAF").Value).Trim())
                                                    ),
                                            new XElement("Receptor",
                                                     new XElement("CiudadPostal", ((System.String)ors.Fields.Item("CiudadPostal").Value).Trim()),
                                                     new XElement("CiudadRecep", ((System.String)ors.Fields.Item("CiudadRecep").Value).Trim()),
                                                     new XElement("CmnaPostal", ((System.String)ors.Fields.Item("CmnaPostal").Value).Trim()),
                                                     new XElement("CmnaRecep", ((System.String)ors.Fields.Item("CmnaRecep").Value).Trim()),
                                                     new XElement("Contacto", ((System.String)ors.Fields.Item("Contacto").Value).Trim()),
                                                     new XElement("CorreoRecep", ((System.String)ors.Fields.Item("CorreoRecep").Value).Trim()),
                                                     new XElement("DirPostal", ((System.String)ors.Fields.Item("DirPostal").Value).Trim()),
                                                     new XElement("DirRecep", ((System.String)ors.Fields.Item("DirRecep").Value).Trim()),
                                                     new XElement("GiroRecep", ((System.String)ors.Fields.Item("GiroRecep").Value).Trim()),
                                                     new XElement("RUTRecep", ((System.String)ors.Fields.Item("RUTRecep").Value).Trim()),
                                                     new XElement("RznSocRecep", ((System.String)ors.Fields.Item("RznSocRecep").Value).Trim())
                                                    ),
                                            new XElement("Totales",
                                                     new XElement("CredEC", ((System.Int32)ors.Fields.Item("CredEC").Value)),
                                                     new XElement("IVA", ((System.Double)ors.Fields.Item("IVA").Value)),
                                                     new XElement("IVANoRet", ((System.Double)ors.Fields.Item("IVANoRet").Value)),
                                                     new XElement("IVAProp", ((System.Double)ors.Fields.Item("IVAProp").Value)),
                                                     new XElement("IVATerc", ((System.Double)ors.Fields.Item("IVATerc").Value)),
                                                     new XElement("MntBase", ((System.Double)ors.Fields.Item("MntBase").Value)),
                                                     new XElement("MntExe", ((System.Double)ors.Fields.Item("MntExe").Value)),
                                                     new XElement("MntMargenCom", ((System.Double)ors.Fields.Item("MntMargenCom").Value)),
                                                     new XElement("MntNeto", ((System.Double)ors.Fields.Item("MntNeto").Value)),
                                                     new XElement("MntTotal", ((System.Double)ors.Fields.Item("MntTotal").Value)),
                                                     new XElement("MontoNF", ((System.Double)ors.Fields.Item("MontoNF").Value)),
                                                     new XElement("MontoPeriodo", ((System.Double)ors.Fields.Item("MontoPeriodo").Value)),
                                                     new XElement("SaldoAnterior", ((System.Double)ors.Fields.Item("SaldoAnterior").Value)),
                                                     new XElement("TasaIVA", ((System.Double)ors.Fields.Item("TasaIVA").Value)),
                                                     new XElement("VlrPagar", ((System.Double)ors.Fields.Item("VlrPAgar").Value))
                                                    )
                                        );
                    miXML.Descendants("Documento").LastOrDefault().Add(xNodo);

                    //AGREGA impuestos Adicionales
                    if (((System.Double)ors.Fields.Item("MntImpAdic").Value) > 0)
                    {
                        ors2.MoveFirst();
                        while (!ors2.EoF)
                        {
                            xNodo = new XElement("ImptoReten",
                                                new XElement("TipoImp", ((System.String)ors2.Fields.Item("CodImpAdic").Value).Trim()),
                                                new XElement("TasaImp", ((System.Double)ors2.Fields.Item("PorcImptoAdic").Value)),
                                                new XElement("MontoImp", ((System.Double)ors2.Fields.Item("MontoImptoAdic").Value))
                                                );
                            miXML.Descendants("Totales").LastOrDefault().Add(xNodo);
                            ors2.MoveNext();
                        }
                    }


                    var NroLinDR = 1;
                    //AGREGA Descuento Encabezado
                    if (((System.Double)ors.Fields.Item("MntDescuento").Value) != 0)
                    {
                        xNodo = new XElement("DscRcgGlobal",
                                                    new XElement("NroLinDR", NroLinDR),
                                                    new XElement("TpoMov", "D"),
                                                    new XElement("GlosaDR", "Descuento Global"),
                                                    new XElement("TpoValor", "$"),
                                                    new XElement("ValorDR", ((System.Double)ors.Fields.Item("MntDescuento").Value))
                                            );
                        miXML.Descendants("Documento").LastOrDefault().Add(xNodo);
                        NroLinDR++;
                    }

                    //AGREGA Recargo Global
                    if (((System.Double)ors.Fields.Item("MntGlobal").Value) != 0)
                    {
                        xNodo = new XElement("DscRcgGlobal",
                                                    new XElement("NroLinDR", NroLinDR),
                                                    new XElement("TpoMov", "R"),
                                                    new XElement("GlosaDR", "Recargo Global"),
                                                    new XElement("TpoValor", "$"),
                                                    new XElement("ValorDR", ((System.Double)ors.Fields.Item("MntGlobal").Value))
                                            );
                        miXML.Descendants("Documento").LastOrDefault().Add(xNodo);
                        NroLinDR++;
                    }

                    //AGREGA COMP 
                    xNodo = new XElement("DocumentoInterno",
                                                    new XElement("COMP", ((System.Int32)ors.Fields.Item("COMP").Value)));
                    miXML.Descendants("Documento").LastOrDefault().Add(xNodo);

                    //para agregar campos EXTRA
                    var iCol = 0;
                    while (iCol < ors.Fields.Count)
                    {
                        var NomCol = ors.Fields.Item(iCol).Name;

                        if (NomCol.Contains("Extra"))
                        {
                            s = ((System.String)ors.Fields.Item(NomCol).Value).Trim();
                            if (s != "")
                            {
                                xNodo = new XElement(NomCol, ((System.String)ors.Fields.Item(NomCol).Value).Trim());
                                miXML.Descendants("DocumentoInterno").LastOrDefault().Add(xNodo);
                            }
                        }
                        iCol++;
                    }

                }//fin encabezado
                else if (Sector == "D")
                {
                    ors.MoveFirst();
                    while (!ors.EoF)
                    {
                        var result = (from nodo in miXML.Descendants("Detalle")
                                      //where nodo.Attribute("id").Value == "1234"
                                      select nodo).FirstOrDefault();

                        xNodo = new XElement("Detalle",
                                            new XElement("NroLinDet", ((System.Int32)ors.Fields.Item("NroLinDet").Value)),
                                            new XElement("DescuentoMonto", ((System.Double)ors.Fields.Item("DescuentoMonto").Value)),
                                            new XElement("DescuentoPct", ((System.Double)ors.Fields.Item("DescuentoPct").Value)),
                                            new XElement("IndExe", ((System.Int32)ors.Fields.Item("IndExe").Value)),
                                            new XElement("MontoItem", ((System.Double)ors.Fields.Item("MontoItem").Value)),
                                            new XElement("CdgItem",
                                                        new XElement("TpoCodigo", "INT1"),
                                                        new XElement("VlrCodigo", ((System.String)ors.Fields.Item("VlrCodigo").Value).Trim())
                                                        ),
                                            new XElement("NmbItem", ((System.String)ors.Fields.Item("NmbItem").Value).Trim()),
                                            new XElement("DscItem", ((System.String)ors.Fields.Item("DscItem").Value).Trim()),
                                            new XElement("PrcItem", ((System.Double)ors.Fields.Item("PrcItem").Value)),
                                            new XElement("PrcRef", ((System.Double)ors.Fields.Item("PrcRef").Value)),
                                            new XElement("QtyItem", ((System.Double)ors.Fields.Item("QtyItem").Value)),
                                            new XElement("QtyRef", ((System.Double)ors.Fields.Item("QtyRef").Value)),
                       //                     new XElement("RecargoMonto", ((System.Double)ors.Fields.Item("RecargoMonto").Value)),
                                            new XElement("RecargoPct", ((System.Double)ors.Fields.Item("RecargoPct").Value)),
                                            new XElement("UnmdItem", ((System.String)ors.Fields.Item("UnmdItem").Value).Trim()),
                                            new XElement("CodImpAdic", ((System.String)ors.Fields.Item("CodImpAdic").Value).Trim()),
                                            new XElement("RecargoMonto", ((System.Double)ors.Fields.Item("RecargoMonto").Value))
                                            );
                        //if (result == null)
                        //    miXML.Root.Add(xNodo);
                        //else
                        miXML.Descendants("Documento").LastOrDefault().Add(xNodo);
                        ors.MoveNext();
                    }
                }//fin Detalle
                else if (Sector == "R")
                {
                    ors.MoveFirst();
                    while (!ors.EoF)
                    {
                        var result = (from nodo in miXML.Descendants("Referencia")
                                      //where nodo.Attribute("id").Value == "1234"
                                      select nodo).FirstOrDefault();

                        xNodo = new XElement("Referencia",
                                            new XElement("NroLinRef", ((System.Int32)ors.Fields.Item("NroLinRef").Value)),
                                            new XElement("TpoDocRef", ((System.String)ors.Fields.Item("TpoDocRef").Value).Trim()),
                                            new XElement("FolioRef", ((System.String)ors.Fields.Item("FolioRef").Value).Trim()),
                                            new XElement("FchRef", ((System.String)ors.Fields.Item("FchRef").Value).Trim()),
                                            new XElement("CodRef", ((System.String)ors.Fields.Item("CodRef").Value).Trim()),
                                            new XElement("RazonRef", ((System.String)ors.Fields.Item("RazonRef").Value).Trim())
                                            );
                        if (result == null)
                            miXML.Root.Add(xNodo);
                        else
                            miXML.Descendants("Documento").LastOrDefault().Add(xNodo);
                        ors.MoveNext();
                    }
                }//fin Referencia


                return miXML.ToString();
            }
            catch (Exception x)
            {
                SBO_f.oLog.OutLog("Error GenerarXMLString, Sector " + Sector + " -> " + x.Message + ", TRACE " + x.StackTrace);
                return "";
            }
        }

        private Boolean ValidarDatosFE()
        {
            Boolean _result;
            SAPbouiCOM.DBDataSource oDBDSDir;
            SAPbouiCOM.DBDataSource oDBDSH;
            TFunctions Param;
            Boolean DocElec;
            String Tabla;
            Int32 i;
            SAPbouiCOM.EditText oEditText;
            SAPbouiCOM.ComboBox oComboBox;
            String TipoLinea;
            String TipoDoc = "";
            String TipoDocElec = "";
            String[] CaracteresInvalidos = { "Ñ", "°", "|", "!", @"""", "#", "$", "=", "?", "\\", "¿", "¡", "~", "´", "+", "{", "}", "[", "]", "-", ":", "%" };
            String s1;
            Int32 CantLineas;
            String ItemCode;
            String ItemCodeAnt = "";
            String TreeType;

            try
            {

                _result = true;
                if (oForm.BusinessObject.Type == "67") // Transferencia)
                {
                    oDBDSDir = oForm.DataSources.DBDataSources.Item("WTR12");
                    oDBDSH = oForm.DataSources.DBDataSources.Item("OWTR");
                }
                else if (oForm.BusinessObject.Type == "21") //Devolucion)
                {
                    oDBDSDir = oForm.DataSources.DBDataSources.Item("RPD12");
                    oDBDSH = oForm.DataSources.DBDataSources.Item("ORPD");
                }
                else if (oForm.BusinessObject.Type == "15") // Entrega
                {
                    oDBDSDir = oForm.DataSources.DBDataSources.Item("DLN12");
                    oDBDSH = oForm.DataSources.DBDataSources.Item("ODLN");
                }
                else // Solicitud de Compra
                {
                    oDBDSDir = oForm.DataSources.DBDataSources.Item("WTQ12");
                    oDBDSH = oForm.DataSources.DBDataSources.Item("OWTQ");
                }

                if (GlobalSettings.RunningUnderSQLServer)
                {
                    s = @"SELECT Count(*) Cont
                            FROM NNM1 WITH (NOLOCK)
                           WHERE (SUBSTRING(UPPER(BeginStr), 1, 1) = 'E') 
                             AND Series = '{0}'
                             --AND ObjectCode = '{1}'";
                }
                else
                {
                    s = @"SELECT Count(*) ""Cont""
                            FROM ""NNM1""
                           WHERE (SUBSTRING(UPPER(""BeginStr""), 1, 1) = 'E')
                             AND ""Series"" = '{0}'
                             --AND ""ObjectCode"" = '{1}' ";
                }
                if (oForm.BusinessObject.Type == "67") //Transferencia)
                    s = String.Format(s, (System.String)(oDBDSH.GetValue("Series", 0)), 67);
                else if (oForm.BusinessObject.Type == "21") //Devolucion)
                    s = String.Format(s, (System.String)(oDBDSH.GetValue("Series", 0)), 21);
                else if (oForm.BusinessObject.Type == "15") //Entrega)
                    s = String.Format(s, (System.String)(oDBDSH.GetValue("Series", 0)), 15);
                else //Solicitud Trasalado
                    s = String.Format(s, (System.String)(oDBDSH.GetValue("Series", 0)), 1250000001);
                oRecordSet.DoQuery(s);

                if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                    DocElec = true;
                else
                    DocElec = false;

                if (DocElec)
                {
                    if (oForm.BusinessObject.Type != "67" && oForm.BusinessObject.Type != "1250000001") //!Transferencia)
                    {
                        if ((System.String)(oDBDSDir.GetValue("CityB", 0)).Trim() == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        if (((System.String)(oDBDSDir.GetValue("CityS", 0)).Trim() == "") && (_result))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        if (((System.String)(oDBDSDir.GetValue("CountyB", 0)).Trim() == "") && (_result))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        if (((System.String)(oDBDSDir.GetValue("CountyS", 0)).Trim() == "") && (_result))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        if (((System.String)(oDBDSDir.GetValue("StreetB", 0)).Trim() == "") && (_result))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        if (((System.String)(oDBDSDir.GetValue("StreetS", 0)).Trim() == "") && (_result))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }
                    else
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"SELECT City, MailCity,County, MailCounty, Address, MailAddres
                                    FROM OCRD WITH (NOLOCK)
                                   WHERE CardCode = '{0}'";
                        }
                        else
                        {
                            s = @"SELECT ""City"", ""MailCity"", ""County"", ""MailCounty"", ""Address"", ""MailAddres""
                                FROM ""OCRD""
                               WHERE ""CardCode"" = '{0}' ";
                        }
                        s = String.Format(s, (System.String)(oDBDSH.GetValue("CardCode", 0)).Trim());
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount == 0)
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar Cliente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oRecordSet.Fields.Item("City").Value) == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oRecordSet.Fields.Item("MailCity").Value) == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oRecordSet.Fields.Item("County").Value) == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oRecordSet.Fields.Item("MailCounty").Value) == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oRecordSet.Fields.Item("Address").Value) == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oRecordSet.Fields.Item("MailAddres").Value) == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }

                    s = (System.String)(oDBDSH.GetValue("CardName", 0)).Trim();
                    if ((s == "") && (_result))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar Nombre Cliente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    //validar caracteres invalidos en el nombre del cliente
                    //se comenta segun reunion de viernes 20150320, se creo una funcion que limpia lo caracteres invalidos al momento de enviar al portal
                    //if (_result)
                    //{
                    //    foreach (String cara in CaracteresInvalidos)
                    //    {
                    //        if (s.IndexOf(cara) > 0)
                    //        {
                    //            FSBOApp.StatusBar.SetText(@"Nombre Cliente tiene caracteres prohibidos (" + cara + ")", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    //            _result = false;
                    //            break;
                    //        }
                    //    }
                    //}

                    //valida rut
                    if (_result)
                    {
                        Param = new TFunctions();
                        Param.SBO_f = FSBOf;
                        if (!Transferencia)
                        { s = Param.ValidarRut((System.String)(oDBDSH.GetValue("LicTradNum", 0))); }
                        else
                        {
                            if (GlobalSettings.RunningUnderSQLServer)
                                s = @"SELECT LicTradNum
                                        FROM OCRD WITH (NOLOCK)
                                       WHERE CardCode = '{0}'";
                            else
                                s = @"SELECT ""LicTradNum""
                                    FROM ""OCRD""
                                   WHERE ""CardCode"" = '{0}' ";
                            s = String.Format(s, (System.String)(oDBDSH.GetValue("CardCode", 0)).Trim());
                            oRecordSet.DoQuery(s);
                            if (oRecordSet.RecordCount == 0)
                                s = "No se ha encontrado cliente";
                            else if ((System.String)(oRecordSet.Fields.Item("LicTradNum").Value) == "")
                                s = "No se ha encontrado RUT del cliente";
                            else
                                s = Param.ValidarRut((System.String)(oRecordSet.Fields.Item("LicTradNum").Value));
                        }

                        if (s != "OK")
                        {
                            FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }


                    var sDocSubType = (System.String)(oDBDSH.GetValue("DocSubType", 0)).Trim();

                    if (sDocSubType == "--") TipoDocElec = "52";

                    //Valida que tenga ingreado el rut del cliente
                    if (_result)
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = "select REPLACE(ISNULL(TaxIdNum,''),'.','') TaxIdNum from OADM";
                        else
                            s = @"select REPLACE(IFNULL(""TaxIdNum"",''),'.','') ""TaxIdNum"" from ""OADM"" ";
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount > 0)
                        {
                            if ((System.String)(oRecordSet.Fields.Item("TaxIdNum").Value).ToString().Trim() == "")
                            {
                                FSBOApp.StatusBar.SetText("Debe ingresar RUT de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                return false;
                            }
                        }
                    }

                    //valida descuentos negativos en el detalle del documento
                    if (_result)
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = "select ISNULL(U_ValDescL,'Y') 'ValDescL' from [@VID_FEPARAM]";
                            s1 = "select ISNULL(U_CantLineas,0) CantLineas from [@VID_FEPROCED] where U_TipoDoc = '" + TipoDocElec + "' and U_Habili = 'Y'";
                        }
                        else
                        {
                            s = @"select IFNULL(""U_ValDescL"",'Y') ""ValDescL"" from ""@VID_FEPARAM"" ";
                            s1 = @"select IFNULL(""U_CantLineas"",0) ""CantLineas"" from ""@VID_FEPROCED"" where ""U_TipoDoc"" = '" + TipoDocElec + @"' and ""U_Habili"" = 'Y'";
                        }

                        oRecordSet.DoQuery(s1);
                        if (oRecordSet.RecordCount > 0)
                            CantLineas = (System.Int32)(oRecordSet.Fields.Item("CantLineas").Value);
                        else
                        {
                            FSBOApp.StatusBar.SetText("Debe parametrizar el maximo de lineas para documento " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }

                        oRecordSet.DoQuery(s);//consulta para descuento
                        var cantlin = 0;
                        if (oRecordSet.RecordCount > 0)
                        {
                            var ValDescL = (System.String)(oRecordSet.Fields.Item("ValDescL").Value);
                            i = 1;
                            if (ValDescL == "Y")
                            {
                                if (oForm.BusinessObject.Type == "67" || oForm.BusinessObject.Type == "1250000001") //Transferencia)
                                    mtx = (Matrix)(oForm.Items.Item("23").Specific);
                                else
                                {
                                    if ((System.String)(oDBDSH.GetValue("DocType", 0)).Trim() == "S")
                                        mtx = (Matrix)(oForm.Items.Item("39").Specific);
                                    else
                                        mtx = (Matrix)(oForm.Items.Item("38").Specific);
                                }

                                if ((mtx.RowCount - 1 > CantLineas) && (((System.String)oDBDSH.GetValue("SummryType", 0)).Trim() == "N")) //valida total de lineas solo cuando no es resumen
                                {
                                    FSBOApp.StatusBar.SetText("Cantidad de lineas supera lo permitido, parametrización FE", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    return false;
                                }

                                i = 1;
                                var sx = "OK";
                                while (i < mtx.RowCount)
                                {
                                    if (oForm.BusinessObject.Type == "67" || oForm.BusinessObject.Type == "1250000001") //(Transferencia)
                                    {
                                        cantlin++;
                                        oEditText = (EditText)(mtx.Columns.Item("12").Cells.Item(i).Specific);
                                        TipoLinea = "";
                                    }
                                    else
                                    {
                                        //contar total de lineas
                                        if ((System.String)(oDBDSH.GetValue("DocType", 0)).Trim() == "S")
                                            cantlin++;
                                        else
                                        {
                                            oEditText = (EditText)(mtx.Columns.Item("1").Cells.Item(i).Specific);
                                            ItemCode = oEditText.Value.Trim();
                                            oEditText = (EditText)(mtx.Columns.Item("39").Cells.Item(i).Specific);
                                            TreeType = oEditText.Value.Trim();
                                            if (ItemCode != "")
                                            {
                                                if (TreeType == "I")
                                                {
                                                    if (GlobalSettings.RunningUnderSQLServer)
                                                        s = @"SELECT HideComp FROM OITT WHERE Code = '{0}'";
                                                    else
                                                        s = @"SELECT ""HideComp"" FROM ""OITT"" WHERE ""Code"" = '{0}'";
                                                    s = String.Format(s, ItemCodeAnt);
                                                    oRecordSet.DoQuery(s);
                                                    if (((System.String)oRecordSet.Fields.Item("HideComp").Value).Trim() == "N")
                                                        cantlin++;
                                                }
                                                else
                                                {
                                                    if (TreeType == "S")
                                                        ItemCodeAnt = ItemCode;
                                                    cantlin++;
                                                }
                                            }
                                        }

                                        if ((System.String)(oDBDSH.GetValue("DocType", 0)).Trim() == "S")
                                        {
                                            oEditText = (EditText)(mtx.Columns.Item("6").Cells.Item(i).Specific);
                                            TipoLinea = "";
                                        }
                                        else
                                        {
                                            oEditText = (EditText)(mtx.Columns.Item("15").Cells.Item(i).Specific);
                                            oComboBox = (ComboBox)(mtx.Columns.Item("257").Cells.Item(i).Specific);
                                            TipoLinea = (System.String)(oComboBox.Value);
                                        }
                                    }

                                    if ((Convert.ToDouble(((SAPbouiCOM.EditText)(oEditText)).String.Replace(",", "."), _nf) < 0) && (TipoLinea == ""))
                                    {
                                        sx = "Descuento negativo en la linea " + Convert.ToString(i);
                                        i = mtx.RowCount;
                                    }

                                    i++;
                                }

                                if (sx != "OK")
                                {
                                    FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    _result = false;
                                }
                            }
                            //oRecordSet.MoveNext();
                        }


                        if ((cantlin > CantLineas) && (((System.String)oDBDSH.GetValue("SummryType", 0)).Trim() == "N")) //valida total de lineas solo cuando no es resumen
                        {
                            FSBOApp.StatusBar.SetText("Cantidad de lineas supera lo permitido, parametrización FE", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }
                    }


                    //valida para guais de despacho
                    if (_result)
                    {
                        if (oDBDSH.GetValue("U_Traslado", 0) == "")
                        {
                            FSBOApp.StatusBar.SetText("Guia de Despacho Electronica, debe seleccionar Indicador de Traslado", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }

                    //valida para folio Distribuido
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = "select ISNULL(U_Distrib,'N') 'Distribuido' from [@VID_FEPARAM]";
                    else
                        s = @"select IFNULL(""U_Distrib"",'N') ""Distribuido"" from ""@VID_FEPARAM"" ";
                    oRecordSet.DoQuery(s);
                    if ((oRecordSet.RecordCount > 0) && (_result))
                    {
                        if ((System.String)(oRecordSet.Fields.Item("Distribuido").Value) == "Y")
                        {

                            if (GlobalSettings.RunningUnderSQLServer)
                                s = @"select T0.DocEntry, T1.LineId, T1.U_Folio
                                        from [@VID_FEDIST] T0 WITH (NOLOCK)
	                                    join [@VID_FEDISTD] T1 WITH (NOLOCK) on T1.DocEntry = T0.DocEntry
                                       where T0.U_TipoDoc = '{0}'
                                         and T0.U_Sucursal = 'Principal'
	                                     and T1.U_Estado = 'D'
	                                     and T1.U_Folio > 0
                                       order by T1.U_Folio ASC";
                            else
                                s = @"select T0.""DocEntry"", T1.""LineId"", T1.""U_Folio""
                                        from ""@VID_FEDIST"" T0
	                                    join ""@VID_FEDISTD"" T1 on T1.""DocEntry"" = T0.""DocEntry""
                                       where T0.""U_TipoDoc"" = '{0}'
                                         and T0.""U_Sucursal"" = 'Principal'
	                                     and T1.""U_Estado"" = 'D'
	                                     and T1.""U_Folio"" > 0 
                                       order by T1.""U_Folio"" ASC ";
                            s = String.Format(s, TipoDocElec);
                            oRecordSet.DoQuery(s);
                            if (oRecordSet.RecordCount == 0)
                            {
                                FSBOApp.StatusBar.SetText("No se ha encontrado número de folio disponible para SBO", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                _result = false;
                            }
                        }
                    }
                }

                return _result;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("ValidarDatosFE " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
        }//fin ValidarDatosFE

    }//fin Class
}
