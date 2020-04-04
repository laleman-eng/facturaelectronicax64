using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.CodeDom.Compiler;
using System.Xml;
using System.Xml.Linq;
using System.Net;
//using System.Net.Http;
using System.Configuration;
//using ServiceStack.Text;
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
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.ReportSource;
using Newtonsoft.Json;
using DLLparaXML;


namespace Factura_Electronica_VK.Invoice
{
    class TInvoice : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private String s;
        private Boolean Flag;
        private SAPbouiCOM.Matrix mtx;
        private SAPbouiCOM.StaticText oStatic;
        private SAPbouiCOM.EditText oEditText;
        private SAPbouiCOM.ComboBox oComboBox;

        private SAPbouiCOM.DataTable odt;
        private SAPbouiCOM.Grid ogrid;
        //private pe.facturamovil.User oUser_FM;
        private String JsonText;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private Boolean bMultiSoc;
        private String SerieAnterior = "";
        //
        private List<string> Lista;

        public static String DocSubType
        { get; set; }
        public static Boolean bFolderAdd
        { get; set; }
        public static String ObjType
        { get; set; }
        public static Boolean ReservaExp
        { get; set; }
        public static Boolean Liquidacion
        { get; set; }
        public VisualD.SBOFunctions.CSBOFunctions SBO_f;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            SAPbouiCOM.Item oItem;
            SAPbouiCOM.Item oItemB;


            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                //FSBOf.LoadForm(xmlPath, 'VID_Entrega.srf', Uid);
                //var sPath : String := TMultiFunctions.ExtractFilePath(TMultiFunctions.ParamStr(0));
                //sPath := sPath + "\Forms\UpdDocuments.xml";
                //var _xml : XmlDocument := new XmlDocument();
                //_xml.Load(sPath);
                //var xmlstr : String := _xml.InnerXml;
                //xmlstr := xmlstr.Replace("F_11", uid);
                //FSBOApp.LoadBatchActions(var xmlstr);
                oForm = FSBOApp.Forms.Item(uid);
                Flag = false;
                oForm.Freeze(true);

                if (GlobalSettings.RunningUnderSQLServer)
                    s = "select ISNULL(U_MultiSoc,'N') MultiSoc from [@VID_FEPARAM] where Code = '1'";
                else
                    s = @"select IFNULL(""U_MultiSoc"",'N') ""MultiSoc"" from ""@VID_FEPARAM"" where ""Code"" = '1' ";

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                    throw new Exception("Debe parametrizar el Addon Factura Electronica");
                else
                {
                    if (((System.String)oRecordSet.Fields.Item("MultiSoc").Value).Trim() == "Y")
                        bMultiSoc = true;
                    else
                        bMultiSoc = false;
                }

                if (GlobalSettings.RunningUnderSQLServer)
                {
                    try
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = "SELECT T0.U_Srvr 'Server', T0.U_Usr 'Usuario', T0.U_Pw 'Password' FROM [dbo].[@VID_MENUSU] T0";
                        else
                            s = @"SELECT T0.""U_Srvr"" ""Server"", T0.""U_Usr"" ""Usuario"", T0.""U_Pw"" ""Password"" FROM ""@VID_MENUSU"" T0";
                        oRecordSet.DoQuery(s);
                    }
                    catch //(Exception t)
                    {
                        FSBOApp.StatusBar.SetText("Los datos de acceso al servidor SQL no son validos (Gestion->Definiciones->Factura Electrónica->Configuración Conexión), guarde los datos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        FSBOApp.ActivateMenuItem("VID_RHSQL");
                        return false;
                    }

                    /*if (GlobalSettings.RunningUnderSQLServer)
                    {
                        ConexionADO = new SqlConnection("Data Source = " + FCmpny.Server + "; Initial Catalog = " + FCmpny.CompanyDB + "; User Id=" + ((System.String)oRecordSet.Fields.Item("Usuario").Value).Trim() + ";Password=" + ((System.String)oRecordSet.Fields.Item("Password").Value).Trim());

                        try
                        {
                            ConexionADO.Open();
                        }
                        catch //(Exception t)
                        {
                            FSBOApp.StatusBar.SetText("Los datos de acceso al servidor SQL no son validos (Gestion->Definiciones->Factura Electrónica->Configuración Conexión), guarde los datos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            FSBOApp.ActivateMenuItem("VID_RHSQL");
                            return false;
                        }
                        ConexionADO.Close();
                    }*/
                }

                CrearPestañas();

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
                if (ObjType == "13")
                    oComboBox.DataBind.SetBound(true, "OINV", "U_EstadoFE");
                else if (ObjType == "203")
                    oComboBox.DataBind.SetBound(true, "ODPI", "U_EstadoFE");

                //Campo con el estado SII de DTE
                oItemB = oForm.Items.Item("lblEstado");
                oItem = oForm.Items.Add("lEstadoSII", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                oItem.Left = oItemB.Left;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 1;
                oItem.Height = oItem.Height;
                oItem.LinkTo = "VID_EstadoSII";
                oStatic = (StaticText)(oForm.Items.Item("lEstadoSII").Specific);
                oStatic.Caption = "Respuesta Receptor";

                oItemB = oForm.Items.Item("VID_Estado");
                oItem = oForm.Items.Add("VID_EstSII", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                oItem.Left = oItemB.Left;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 1;
                oItem.Height = oItem.Height;
                oItem.DisplayDesc = true;
                oItem.Enabled = false;
                oComboBox = (ComboBox)(oForm.Items.Item("VID_EstSII").Specific);
                if (ObjType == "13")
                    oComboBox.DataBind.SetBound(true, "OINV", "U_EstadoSII");
                else if (ObjType == "203")
                    oComboBox.DataBind.SetBound(true, "ODPI", "U_EstadoSII");


                oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                var sSeries = (System.String)(oComboBox.Value);

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor', SUBSTRING(ISNULL(UPPER(BeginStr),''),2,LEN(ISNULL(UPPER(BeginStr),''))) 'Doc' 
                                from NNM1 where Series = {0} --AND ObjectCode = '{1}' ";
                else
                    s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"", SUBSTRING(IFNULL(UPPER(""BeginStr""),''),2,LENGTH(IFNULL(UPPER(""BeginStr""),''))) ""Doc""
                                from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' ";
                s = String.Format(s, sSeries, ObjType);
                if (sSeries != "")
                {
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount > 0)
                    {
                        var TipoDTE = ((System.String)oRecordSet.Fields.Item("Doc").Value).Trim();
                        if ((System.String)(oRecordSet.Fields.Item("Valor").Value) == "E")
                        {
                            if ((DocSubType == "IB") || (DocSubType == "EB"))
                            {
                                oForm.Items.Item("VID_Estado").Visible = false;
                                oForm.Items.Item("lblEstado").Visible = false;
                                oForm.Items.Item("VID_FEDCTO").Visible = true;
                                oForm.Items.Item("VID_EstSII").Visible = false;
                                oForm.Items.Item("lEstadoSII").Visible = false;
                                oForm.Items.Item("VID_FEAF").Visible = false;
                            }
                            else
                            {
                                oForm.Items.Item("VID_Estado").Visible = true;
                                oForm.Items.Item("lblEstado").Visible = true;
                                oForm.Items.Item("VID_EstSII").Visible = true;
                                oForm.Items.Item("lEstadoSII").Visible = true;
                                if (oForm.Mode == BoFormMode.fm_ADD_MODE)
                                {
                                    ((ComboBox)oForm.Items.Item("VID_Estado").Specific).Select("N", BoSearchKey.psk_ByValue);
                                    ((ComboBox)oForm.Items.Item("VID_EstSII").Specific).Select("P", BoSearchKey.psk_ByValue);
                                }
                                if ((DocSubType != "--") && (DocSubType != "IE"))
                                    oForm.Items.Item("VID_FEDCTO").Visible = true;
                                else if ((DocSubType == "--") && (((System.String)oRecordSet.Fields.Item("Doc").Value).Trim() == "110"))
                                    oForm.Items.Item("VID_FEDCTO").Visible = true;
                                else if (ReservaExp)
                                    oForm.Items.Item("VID_FEDCTO").Visible = false;
                                else if ((TipoDTE != "43") && (DocSubType != "IE") && (ObjType != "203"))
                                    oForm.Items.Item("VID_FEDCTO").Visible = false;

                                if ((TipoDTE != "43") && (DocSubType == "DN"))
                                    oForm.Items.Item("VID_FEAF").Visible = false;
                            }
                        }
                        else
                        {
                            oForm.Items.Item("VID_Estado").Visible = false;
                            oForm.Items.Item("lblEstado").Visible = false;
                            oForm.Items.Item("VID_EstSII").Visible = false;
                            oForm.Items.Item("lEstadoSII").Visible = false;
                            oForm.Items.Item("VID_FEAF").Visible = false;
                            if ((DocSubType != "--") && (DocSubType != "IE"))
                                oForm.Items.Item("VID_FEDCTO").Visible = false;
                            else if ((DocSubType == "--") && (((System.String)oRecordSet.Fields.Item("Doc").Value).Trim() == "110"))
                                oForm.Items.Item("VID_FEDCTO").Visible = false;
                            else if (ReservaExp)
                                oForm.Items.Item("VID_FEDCTO").Visible = false;
                            else if ((TipoDTE != "43") && (DocSubType != "IE") && (ObjType != "203"))
                                oForm.Items.Item("VID_FEDCTO").Visible = false;
                        }
                    }
                }


                Lista = new List<string>();
                // Ok Ad  Fnd Vw Rq Sec
                Lista.Add("VID_Estado   , f,  f,  f,  f, n, 1");
                Lista.Add("VID_EstSII, f,  f,  f,  f, n, 1");
                //Lista.Add('CardCode  , f,  t,  t,  f, r, 1');
                FSBOf.SetAutoManaged(oForm, Lista);

            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            oForm.Visible = true;
            oForm.Freeze(false);
            return Result;
        }//fin InitForm


        public new void MenuEvent(ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
            SAPbouiCOM.Conditions oConditions;
            SAPbouiCOM.Condition oCondition;
            String tabla;
            SAPbobsCOM.Recordset orsx = ((SAPbobsCOM.Recordset)FCmpny.GetBusinessObject(BoObjectTypes.BoRecordset));
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
                //1304 Actualizar

                if ((pVal.MenuUID != "") && (pVal.BeforeAction == false))
                {
                    //if (ObjType == "203")
                    //    DocSubType = ((System.String)oForm.DataSources.DBDataSources.Item("ODPI").GetValue("DocSubType", 0)).Trim();
                    //else
                    //    DocSubType = ((System.String)oForm.DataSources.DBDataSources.Item("OINV").GetValue("DocSubType", 0)).Trim();

                    if ((pVal.MenuUID == "1288") || (pVal.MenuUID == "1289") || (pVal.MenuUID == "1290") || (pVal.MenuUID == "1291") || (pVal.MenuUID == "1304"))
                    {
                        oForm.Freeze(true);
                        //oForm.Items.Item("VID_Estado").Enabled = false;
                        oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                        var sSeries = (System.String)(oComboBox.Value);

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor', DocSubType, SUBSTRING(ISNULL(UPPER(BeginStr),''),2,LEN(ISNULL(UPPER(BeginStr),''))) 'Doc', ObjectCode
                                        from NNM1 where Series = {0} --AND ObjectCode = '{1}' ";
                        else
                            s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"", ""DocSubType"", SUBSTRING(IFNULL(UPPER(""BeginStr""),''),2,LENGTH(IFNULL(UPPER(""BeginStr""),''))) ""Doc"", ""ObjectCode""
                                        from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' ";
                        s = String.Format(s, sSeries, oForm.BusinessObject.Type);
                        orsx.DoQuery(s);
                        if (orsx.RecordCount > 0)
                        {
                            var TipoDTE = ((System.String)orsx.Fields.Item("Doc").Value).Trim();
                            if (((System.String)orsx.Fields.Item("Valor").Value).Trim() == "E")
                            {
                                if ((((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "IB") || (((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "EB"))
                                {
                                    oForm.Items.Item("VID_Estado").Visible = false;
                                    oForm.Items.Item("lblEstado").Visible = false;
                                }
                                else
                                {
                                    oForm.Items.Item("VID_Estado").Visible = true;
                                    oForm.Items.Item("lblEstado").Visible = true;

                                    if (oForm.BusinessObject.Type == "203")
                                        tabla = "ODPI";
                                    else
                                        tabla = "OINV";
                                    if (((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "DN")
                                    {
                                        var bdraft = false;
                                        if (oForm.Title.Contains("Borrador"))
                                            bdraft = true;
                                        else if (oForm.Title.Contains("Draft"))
                                            bdraft = true;
                                        OutLog("DocEntry " + ((System.String)oForm.DataSources.DBDataSources.Item(tabla).GetValue("DocEntry", 0)).Trim() + " Tabla " + tabla);
                                        CargarReferencia(oForm.BusinessObject.Type, ((System.String)oForm.DataSources.DBDataSources.Item(tabla).GetValue("DocEntry", 0)).Trim(), bdraft);
                                    }
                                }
                            }
                            else
                            {
                                oForm.Items.Item("VID_Estado").Visible = false;
                                oForm.Items.Item("lblEstado").Visible = false;
                                //if (((System.String)orsx.Fields.Item("DocSubType").Value).Trim() != "--")
                                //{
                                //    oForm.Items.Item("VID_FEDCTO").Visible = true;
                                //    s = "112";
                                //    oForm.Items.Item(s).Click(BoCellClickType.ct_Regular);
                                //}
                            }

                            var Tab = (((System.String)orsx.Fields.Item("ObjectCode").Value).Trim() == "203" ? "ODPI" : "OINV");
                            if ((((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "--") && (((System.String)oForm.DataSources.DBDataSources.Item(Tab).GetValue("isIns", 0)).Trim() == "Y")
                                    && (((System.String)orsx.Fields.Item("Doc").Value).Trim() != "110") && ((System.String)(orsx.Fields.Item("Valor").Value) == "E"))
                            {
                                oForm.Items.Item("VID_FEDCTO").Visible = false;
                                oForm.PaneLevel = 1;
                            }
                            else if ((((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "--") && (((System.String)oForm.DataSources.DBDataSources.Item(Tab).GetValue("isIns", 0)).Trim() == "Y")
                                    && (((System.String)orsx.Fields.Item("Doc").Value).Trim() == "110") && ((System.String)(orsx.Fields.Item("Valor").Value) == "E"))
                                oForm.Items.Item("VID_FEDCTO").Visible = true;
                            else if ((((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "IX") && ((System.String)(orsx.Fields.Item("Valor").Value) == "E"))
                                oForm.Items.Item("VID_FEDCTO").Visible = true;
                            else if ((((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "IX") && ((System.String)(orsx.Fields.Item("Valor").Value) != "E"))
                            {
                                oForm.Items.Item("VID_FEDCTO").Visible = false;
                                s = "112";
                                oForm.Items.Item(s).Click(BoCellClickType.ct_Regular);
                            }
                            else if (TipoDTE == "43")
                            {
                                oForm.Items.Item("VID_FEDCTO").Visible = true;
                                s = "112";
                                oForm.Items.Item(s).Click(BoCellClickType.ct_Regular);
                            }
                        }
                        oForm.Freeze(false);
                    }

                    if ((pVal.MenuUID == "1282") || (pVal.MenuUID == "1281") || (pVal.MenuUID == "1287"))
                    {
                        oForm.Freeze(true);
                        //oForm.Items.Item("VID_Estado").Enabled = false;

                        oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                        var sSeries = (System.String)(oComboBox.Value);

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor', DocSubType, SUBSTRING(ISNULL(UPPER(BeginStr),''),2,LEN(ISNULL(UPPER(BeginStr),''))) 'Doc', ObjectCode
                                        from NNM1 where Series = {0} --AND ObjectCode = '{1}' ";
                        else
                            s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"", ""DocSubType"", SUBSTRING(IFNULL(UPPER(""BeginStr""),''),2,LENGTH(IFNULL(UPPER(""BeginStr""),''))) ""Doc"", ""ObjectCode""
                                        from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' ";
                        s = String.Format(s, sSeries, oForm.BusinessObject.Type);
                        orsx.DoQuery(s);
                        if (orsx.RecordCount > 0)
                        {
                            var TipoDTE = ((System.String)orsx.Fields.Item("Doc").Value).Trim();
                            if ((System.String)(orsx.Fields.Item("Valor").Value) == "E")
                            {
                                if ((((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "IB") || (((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "EB"))
                                {
                                    oForm.Items.Item("VID_Estado").Visible = false;
                                    oForm.Items.Item("lblEstado").Visible = false;
                                }
                                else
                                {
                                    oForm.Items.Item("VID_Estado").Visible = true;
                                    oForm.Items.Item("lblEstado").Visible = true;
                                }
                            }
                            else
                            {
                                oForm.Items.Item("VID_Estado").Visible = false;
                                oForm.Items.Item("lblEstado").Visible = false;
                            }

                            var Tab = (((System.String)orsx.Fields.Item("ObjectCode").Value).Trim() == "203" ? "ODPI" : "OINV");
                            if ((((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "--") && (((System.String)oForm.DataSources.DBDataSources.Item(Tab).GetValue("isIns", 0)).Trim() == "Y")
                                    && (((System.String)orsx.Fields.Item("Doc").Value).Trim() != "110") && ((System.String)(orsx.Fields.Item("Valor").Value) == "E"))
                                oForm.Items.Item("VID_FEDCTO").Visible = false;
                            else if ((((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "--") && (((System.String)oForm.DataSources.DBDataSources.Item(Tab).GetValue("isIns", 0)).Trim() == "Y")
                                    && (((System.String)orsx.Fields.Item("Doc").Value).Trim() == "110") && ((System.String)(orsx.Fields.Item("Valor").Value) == "E"))
                                oForm.Items.Item("VID_FEDCTO").Visible = true;
                            else if ((((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "IX") && ((System.String)(orsx.Fields.Item("Valor").Value) == "E"))
                                oForm.Items.Item("VID_FEDCTO").Visible = true;
                            else if ((((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "IX") && ((System.String)(orsx.Fields.Item("Valor").Value) != "E"))
                                oForm.Items.Item("VID_FEDCTO").Visible = false;
                            else if (TipoDTE == "43")
                            {
                                oForm.Items.Item("VID_FEDCTO").Visible = true;
                                s = "112";
                                oForm.Items.Item(s).Click(BoCellClickType.ct_Regular);
                            }

                            if ((pVal.MenuUID == "1282") || (pVal.MenuUID == "1287"))
                            {
                                if (((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "DN")
                                {
                                    ((ComboBox)oForm.Items.Item("VID_Estado").Specific).Select("N", BoSearchKey.psk_ByValue);
                                    ((ComboBox)oForm.Items.Item("VID_FEInGl").Specific).Select("0", BoSearchKey.psk_ByValue);
                                    ((ComboBox)oForm.Items.Item("VID_FEInRf").Specific).Select("00", BoSearchKey.psk_ByValue);
                                    oForm.DataSources.UserDataSources.Item("CodRef").Value = "";
                                    oForm.DataSources.UserDataSources.Item("RazRef").Value = "";
                                    ogrid = ((SAPbouiCOM.Grid)oForm.Items.Item("gridRefFE").Specific);
                                    ogrid.DataTable.Rows.Clear();
                                    ogrid.DataTable.Rows.Add();
                                    ogrid.AutoResizeColumns();
                                }
                            }
                            else if (pVal.MenuUID == "1281")
                            {
                                if (((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "DN")
                                {
                                    oForm.DataSources.UserDataSources.Item("CodRef").Value = "";
                                    oForm.DataSources.UserDataSources.Item("RazRef").Value = "";
                                    ((ComboBox)oForm.Items.Item("VID_FEInGl").Specific).Select("0", BoSearchKey.psk_ByValue);
                                    ((ComboBox)oForm.Items.Item("VID_FEInRf").Specific).Select("00", BoSearchKey.psk_ByValue);
                                    ogrid = ((SAPbouiCOM.Grid)oForm.Items.Item("gridRefFE").Specific);
                                    ogrid.DataTable.Rows.Clear();
                                    ogrid.AutoResizeColumns();
                                }
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
                oForm.Freeze(false);
            }
        }//fin MenuEvent

        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            Int32 nErr;
            String sErr;
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            SAPbouiCOM.DataTable oDataTable;
            SAPbouiCOM.IChooseFromListEvent oCFLEvento = null;
            String sValue;

            try
            {
                if ((pVal.EventType == BoEventTypes.et_CHOOSE_FROM_LIST) && (!pVal.BeforeAction) && (pVal.ItemUID == "VID_FEDocB"))
                {
                    oCFLEvento = (SAPbouiCOM.IChooseFromListEvent)(pVal);
                    oDataTable = oCFLEvento.SelectedObjects;
                    if (oDataTable != null)
                    {
                        sValue = ((System.Int32)oDataTable.GetValue("DocNum", 0)).ToString();
                        //oForm.Freeze(true);
                        oForm.Select();
                        //oForm.DataSources.UserDataSources.Item("VID_FEDocB").Value = sValue;
                        //oForm.Items.Item("VID_FEDocC").Click(BoCellClickType.ct_Regular);
                        try
                        {
                            oEditText = ((EditText)oForm.Items.Item("VID_FEDocB").Specific);
                            oEditText.Value = sValue;
                        }
                        catch { }
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if ((pVal.ItemUID == "1") && (oForm.Mode == BoFormMode.fm_ADD_MODE))
                    {
                        if (ObjType == "13")
                            s = (System.String)oForm.DataSources.DBDataSources.Item("OINV").GetValue("CANCELED", 0);
                        else if (ObjType == "203")
                            s = (System.String)oForm.DataSources.DBDataSources.Item("ODPI").GetValue("CANCELED", 0);

                        if (s == "N")
                            BubbleEvent = ValidarDatosFE();
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction))
                {
                    if (pVal.ItemUID == "VID_FEDCTO")
                        oForm.PaneLevel = 333;

                    if (pVal.ItemUID == "VID_FEAF")
                        oForm.PaneLevel = 330;
                }


                if ((pVal.EventType == BoEventTypes.et_FORM_ACTIVATE) && (!pVal.BeforeAction))
                {
                    GlobalSettings.PrevFormUID = oForm.UniqueID;
                    if (oForm.BusinessObject.Type == "203")
                        DocSubType = ((System.String)oForm.DataSources.DBDataSources.Item("ODPI").GetValue("DocSubType", 0)).Trim();
                    else
                        DocSubType = ((System.String)oForm.DataSources.DBDataSources.Item("OINV").GetValue("DocSubType", 0)).Trim();

                }

                if ((pVal.ItemUID == "VID_FEInGl") && (pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (!pVal.BeforeAction))
                {
                    oComboBox = (ComboBox)(oForm.Items.Item("VID_FEInGl").Specific);
                    var sIndGlobal = (System.String)(oComboBox.Value);
                    ogrid = ((Grid)oForm.Items.Item("gridRefFE").Specific);
                    if (sIndGlobal == "1")
                    {
                        ogrid.DataTable.Rows.Clear();
                        oForm.Items.Item("gridRefFE").Enabled = false;
                        oForm.Items.Item("VID_FEInRf").Enabled = true;
                    }
                    else
                    {
                        oForm.Items.Item("gridRefFE").Enabled = true;
                        ogrid.DataTable.Rows.Add(1);
                        oForm.Items.Item("VID_FEInRf").Enabled = false;
                    }
                }


                if ((pVal.ItemUID == "88") && (pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (pVal.BeforeAction))
                {
                    oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                    SerieAnterior = (System.String)(oComboBox.Value);
                }

                if ((pVal.ItemUID == "88") && (pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (!pVal.BeforeAction))
                {
                    oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                    var sSeries = (System.String)(oComboBox.Value);

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor', DocSubType, SUBSTRING(ISNULL(UPPER(BeginStr),''),2,LEN(ISNULL(UPPER(BeginStr),''))) 'Doc', ObjectCode, SeriesName
                                from NNM1 where Series = {0} --AND ObjectCode = '{1}' ";
                    else
                        s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"", ""DocSubType"", SUBSTRING(IFNULL(UPPER(""BeginStr""),''),2,LENGTH(IFNULL(UPPER(""BeginStr""),''))) ""Doc"", ""ObjectCode"", ""SeriesName""
                                from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' ";
                    s = String.Format(s, sSeries, oForm.BusinessObject.Type);
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount > 0)
                    {
                        var Tab = (((System.String)oRecordSet.Fields.Item("ObjectCode").Value).Trim() == "203" ? "ODPI" : "OINV");
                        var TipoDTE = ((System.String)oRecordSet.Fields.Item("Doc").Value).Trim();
                        if ((System.String)(oRecordSet.Fields.Item("Valor").Value) == "E")
                        {
                            if ((((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim() == "IB") || (((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim() == "EB"))
                            {
                                oForm.Items.Item("VID_Estado").Visible = false;
                                oForm.Items.Item("lblEstado").Visible = false;
                                oForm.Items.Item("VID_FEDCTO").Visible = true;
                                oForm.Items.Item("VID_EstSII").Visible = false;
                                oForm.Items.Item("lEstadoSII").Visible = false;
                                oForm.Items.Item("VID_FEAF").Visible = false;
                            }
                            else
                            {
                                if (TipoDTE == "43")
                                {
                                    oForm.Items.Item("VID_FEDCTO").Visible = false;
                                    oForm.Items.Item("VID_FEAF").Visible = true;
                                }
                                else if (((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim() != "IE")
                                {
                                    oForm.Items.Item("VID_FEDCTO").Visible = false;
                                    oForm.Items.Item("VID_FEAF").Visible = true;
                                }
                                else
                                    oForm.Items.Item("VID_FEAF").Visible = true;
                                oForm.Items.Item("VID_Estado").Visible = true;
                                oForm.Items.Item("lblEstado").Visible = true;
                                oForm.Items.Item("VID_EstSII").Visible = true;
                                oForm.Items.Item("lEstadoSII").Visible = true;

                                //oForm.Items.Item("VID_Estado").Enabled = false;
                                if ((((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim() != "--") && (((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim() != "IE") && (TipoDTE != "43"))
                                    oForm.Items.Item("VID_FEDCTO").Visible = true;
                                else if ((((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim() == "--") && (((System.String)oRecordSet.Fields.Item("Doc").Value).Trim() == "110")
                                        && (((System.String)oForm.DataSources.DBDataSources.Item(Tab).GetValue("isIns", 0)).Trim() == "Y"))
                                    oForm.Items.Item("VID_FEDCTO").Visible = true;
                                else if ((((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim() == "--") && (((System.String)oRecordSet.Fields.Item("Doc").Value).Trim() == "110")
                                        && (((System.String)oForm.DataSources.DBDataSources.Item(Tab).GetValue("isIns", 0)).Trim() != "Y"))
                                {
                                    if (oForm.Mode != BoFormMode.fm_FIND_MODE)
                                    {
                                        BubbleEvent = false;
                                        FSBOApp.StatusBar.SetText("No se puede usar la serie " + ((System.String)oRecordSet.Fields.Item("SeriesName").Value).Trim() + ", es solo para ser usada en Factura reserva como Exportacion Electronica", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        if (SerieAnterior != "")
                                            oComboBox.Select(SerieAnterior, BoSearchKey.psk_ByValue);
                                    }
                                }
                                else if ((((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim() == "--") && (((System.String)oForm.DataSources.DBDataSources.Item(Tab).GetValue("isIns", 0)).Trim() == "Y")
                                        && (((System.String)oRecordSet.Fields.Item("Doc").Value).Trim() != "110"))
                                    oForm.Items.Item("VID_FEDCTO").Visible = false;

                                if ((((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim() == "DN") && (TipoDTE != "43"))
                                    oForm.Items.Item("VID_FEAF").Visible = false;

                                s = "112";
                                oForm.Items.Item(s).Click(BoCellClickType.ct_Regular);
                            }
                        }
                        else
                        {
                            oForm.Items.Item("VID_Estado").Visible = false;
                            oForm.Items.Item("lblEstado").Visible = false;
                            oForm.Items.Item("VID_EstSII").Visible = false;
                            oForm.Items.Item("lEstadoSII").Visible = false;
                            if ((((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim() != "--") && (((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim() != "IE"))
                            {
                                oForm.Items.Item("VID_FEDCTO").Visible = false;
                                oForm.Items.Item("VID_FEAF").Visible = false;
                                s = "112";
                                oForm.Items.Item(s).Click(BoCellClickType.ct_Regular);
                            }
                            else if ((((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim() == "--") && (((System.String)oForm.DataSources.DBDataSources.Item(Tab).GetValue("isIns", 0)).Trim() == "Y"))
                                oForm.Items.Item("VID_FEDCTO").Visible = false;
                            else
                            {
                                oForm.Items.Item("VID_FEAF").Visible = false;
                                try
                                {
                                    oForm.Items.Item("VID_FEDCTO").Visible = false;
                                }
                                catch { }
                            }
                        }
                    }
                    else
                    {
                        oForm.Items.Item("VID_FEAF").Visible = false;
                        try
                        {
                            oForm.Items.Item("VID_FEDCTO").Visible = false;
                        }
                        catch { }
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (pVal.BeforeAction) && (pVal.ItemUID == "gridRefFE") && (pVal.ColUID == "TipoDTE"))
                {
                    var card = ((System.String)((EditText)oForm.Items.Item("4").Specific).Value).Trim();
                    if (card == "")
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar Socio de Negocio", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        BubbleEvent = false;
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_VALIDATE) && (!pVal.BeforeAction) && (pVal.ItemUID == "gridRefFE") && (pVal.ColUID == "DocFolio"))
                {
                    BubbleEvent = false;
                    ogrid = ((Grid)oForm.Items.Item("gridRefFE").Specific);
                    s = ogrid.DataTable.GetValue("DocFolio", pVal.Row).ToString();
                    oForm.Freeze(true);
                    if ((s != "") && (s != "0"))
                    {
                        if (((System.String)ogrid.DataTable.GetValue("TipoDTE", pVal.Row)).IndexOf('b') != -1)
                        {
                            FSBOApp.StatusBar.SetText("Documento de Otro Sistema", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            if (((System.String)ogrid.DataTable.GetValue("DocEntry", pVal.Row)).Trim() != "")
                                ogrid.DataTable.SetValue("DocEntry", pVal.Row, "");
                            ogrid.AutoResizeColumns();
                        }
                        else
                            BuscarDatosDoc(pVal.Row);

                        if (pVal.Row == ogrid.Rows.Count - 1)
                        {
                            ogrid.DataTable.Rows.Add(1);
                        }
                    }
                    oForm.Freeze(false);
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
            finally
            {
                ;
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
            String nMultiSoc;
            String TaxIdNum;
            String tabla = "";
            String Canceled = "";
            String GeneraT = "";
            String CAF = "";
            Int32 FolioNum;
            Int32 FDocEntry = 0;
            Int32 FLineId = -1;
            Boolean bFolioDistribuido = false;
            Boolean bFolioAsignado = false;
            String FolioPortal = "N";
            String TipoDocElect;
            String Distribuido = "N";
            String TTipoDoc = "";

            SAPbobsCOM.Documents oDocument;
            base.FormDataEvent(ref BusinessObjectInfo, ref BubbleEvent);

            try
            {
                ////pruebas
                //if ((BusinessObjectInfo.BeforeAction == false) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_UPDATE) && (BusinessObjectInfo.ActionSuccess))
                //{

                //    if (oForm.BusinessObject.Type == "13") //And (Flag = true)) then
                //    {
                //        oForm.Items.Item("VID_Estado").Enabled = false;
                //    }
                //}

                if ((BusinessObjectInfo.BeforeAction == false) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_ADD) && (BusinessObjectInfo.ActionSuccess))
                {
                    if ((oForm.BusinessObject.Type == "13") || (oForm.BusinessObject.Type == "203") || (BusinessObjectInfo.Type == "112"))//And (Flag = true)) then
                    {
                        Flag = false;
                        if (BusinessObjectInfo.Type == "13")
                        {
                            tabla = "OINV";
                            TTipoDoc = "33";
                        }
                        else if (BusinessObjectInfo.Type == "203")
                        {
                            tabla = "ODPI";
                            TTipoDoc = "33A";
                        }
                        else if (BusinessObjectInfo.Type == "112")
                        {
                            tabla = "ODRF";
                            TTipoDoc = "xx";
                        }

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = "select ISNULL(U_Distrib,'N') 'Distribuido', ISNULL(U_FPortal,'N') 'FolioPortal', ISNULL(U_MultiSoc,'N') MultiSoc, ISNULL(U_GenerarT,'N') GeneraT from [@VID_FEPARAM] WITH (NOLOCK)";
                        else
                            s = @"select IFNULL(""U_Distrib"",'N') ""Distribuido"", IFNULL(""U_FPortal"",'N') ""FolioPortal"", IFNULL(""U_MultiSoc"",'N') ""MultiSoc"", IFNULL(""U_GenerarT"",'N') ""GeneraT"" from ""@VID_FEPARAM"" ";
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount > 0)
                        {
                            GeneraT = ((System.String)oRecordSet.Fields.Item("GeneraT").Value).Trim();
                            Distribuido = ((System.String)oRecordSet.Fields.Item("Distribuido").Value).Trim();
                            FolioPortal = ((System.String)oRecordSet.Fields.Item("FolioPortal").Value).Trim();

                            if ((System.String)(oRecordSet.Fields.Item("MultiSoc").Value) == "Y")
                                bMultiSoc = true;
                            else
                                bMultiSoc = false;

                            sDocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                            if (GlobalSettings.RunningUnderSQLServer)
                                s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', 
                                                     SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'Inst', 
                                                     SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'TipoDocElect', 
                                                     T0.CANCELED, T0.isIns
                                             FROM {1} T0 WITH (NOLOCK)
                                                JOIN NNM1 T2 WITH (NOLOCK) ON T0.Series = T2.Series 
                                               WHERE T0.DocEntry = {0}";
                            else
                                s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", 
                                                     SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"", 
                                                     SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""TipoDocElect"", 
                                                     T0.""CANCELED"", T0.""isIns""
                                             FROM ""{1}"" T0
                                             JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                                            WHERE T0.""DocEntry"" = {0} ";
                            s = String.Format(s, sDocEntry, tabla);
                            oRecordSet.DoQuery(s);
                            sDocSubType = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                            TipoDocElect = (System.String)(oRecordSet.Fields.Item("TipoDocElect").Value);
                            Tipo = (System.String)(oRecordSet.Fields.Item("Tipo").Value);
                            nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);
                            Canceled = (System.String)(oRecordSet.Fields.Item("CANCELED").Value);

                            if ((sDocSubType == "--") && (((System.String)oRecordSet.Fields.Item("isIns").Value).Trim() == "Y") && (TipoDocElect == "110")) //Factura
                            {
                                TipoDocElec = "110";
                                TTipoDoc = "110";
                            }
                            else if ((TipoDocElect == "43") && (sDocSubType != "DN"))
                            {
                                TipoDocElec = "43";
                                TTipoDoc = "43";
                            }
                            else if (sDocSubType == "--") //Factura
                                TipoDocElec = "33";
                            else if (sDocSubType == "IE") //Factura Exenta
                            {
                                TipoDocElec = "34";
                                TTipoDoc = "34";
                            }
                            else if ((sDocSubType == "DN") && (TipoDocElect == "111")) //Nota Debito
                            {
                                TipoDocElec = "111";
                                TTipoDoc = "111";
                            }
                            else if ((sDocSubType == "DN") && (TipoDocElect == "43")) //Nota Debito
                            {
                                TipoDocElec = "43";
                                TTipoDoc = "43N";
                            }
                            else if ((sDocSubType == "DN") && (TipoDocElect != "111")) //Nota Debito
                            {
                                TipoDocElec = "56";
                                TTipoDoc = "56";
                            }
                            else if (sDocSubType == "IB") //Boleta
                            {
                                TipoDocElec = "39";
                                TTipoDoc = "39";
                            }
                            else if (sDocSubType == "EB") //Boleta Exenta
                            {
                                TipoDocElec = "41";
                                TTipoDoc = "41";
                            }
                            else if (sDocSubType == "IX") //Factura Exportacion
                            {
                                TipoDocElec = "110";
                                TTipoDoc = "110";
                            }


                            if ((Tipo == "E") && (Canceled == "N") && ((TipoDocElec != "43") || (sDocSubType == "DN")))
                            {
                                if ((bMultiSoc == true) && (nMultiSoc == ""))
                                    FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                else
                                {

                                    //Agregar referencia en las tablas de usuario
                                    if ((sDocSubType == "DN") && ((TipoDocElect == "111") || (TipoDocElect == "56")))
                                    {
                                        if (BusinessObjectInfo.Type == "112")
                                        {
                                            Distribuido = "N";
                                            FolioPortal = "N";
                                        }
                                        if (!GuardarReferencia(sDocEntry, tabla, false, (BusinessObjectInfo.Type == "112" ? true : false)))
                                            FSBOApp.StatusBar.SetText("No se ha guardado las referencias", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        else
                                        {
                                            ((Grid)oForm.Items.Item("gridRefFE").Specific).DataTable.Rows.Clear();
                                        }
                                    }
                                    else if (BusinessObjectInfo.Type == "112")
                                    {
                                        Distribuido = "N";
                                        FolioPortal = "N";
                                    }

                                    //Fin Agregar referencia en las tablas de usuario
                                    if ((Distribuido == "Y") && (FolioPortal == "N"))
                                    {
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

                                            if (oForm.BusinessObject.Type == "203")
                                                oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments));
                                            else
                                                oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices));


                                            if (oDocument.GetByKey(Convert.ToInt32(sDocEntry)))
                                            {
                                                if (oDocument.FolioNumber == 0)
                                                {
                                                    oDocument.FolioNumber = FolioNum;
                                                    if (TipoDocElec == "110") //Factura Exportacion
                                                        oDocument.FolioPrefixString = "FX";
                                                    else if (sDocSubType == "--") //Factura
                                                        oDocument.FolioPrefixString = "FE";
                                                    else if (sDocSubType == "IE") //Factura Exenta
                                                        oDocument.FolioPrefixString = "EE";
                                                    else if (sDocSubType == "DN") //Nota Debito
                                                        oDocument.FolioPrefixString = "ND";
                                                    else if (sDocSubType == "IB") //Boleta
                                                        oDocument.FolioPrefixString = "BE";
                                                    else if (sDocSubType == "EB") //Boleta Exenta
                                                        oDocument.FolioPrefixString = "BX";
                                                    else if (sDocSubType == "IX") //Factura Exportacion
                                                        oDocument.FolioPrefixString = "FX";

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

                                                        FSBOApp.MessageBox("*****   No se ha asignado Folio al Documento   *****", 1, "Aceptar");
                                                        FSBOApp.StatusBar.SetText("No se ha asignado Folio al Documento -> " + FCmpny.GetLastErrorDescription(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                        OutLog("No se ha asignado Folio al Documento DocEntry: " + sDocEntry + " Tipo: " + oForm.BusinessObject.Type + " - " + FCmpny.GetLastErrorDescription());
                                                    }
                                                    else
                                                    {
                                                        //ahora debo marcar que el folio fue usado y colocar los datos del documento que uso el folio
                                                        Reg = new TFunctions();
                                                        Reg.SBO_f = FSBOf;

                                                        if (GlobalSettings.RunningUnderSQLServer)
                                                            s = "update [@VID_FEDISTD] set U_Estado = 'U', U_DocEntry = {0}, U_ObjType = '{1}', U_SubType = '{2}' where DocEntry = {3} and LineId = {4}";
                                                        else
                                                            s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'U', ""U_DocEntry"" = {0}, ""U_ObjType"" = '{1}', ""U_SubType"" = '{2}' where ""DocEntry"" = {3} and ""LineId"" = {4}";
                                                        s = String.Format(s, sDocEntry, oForm.BusinessObject.Type, sDocSubType, FDocEntry, FLineId);
                                                        oRecordSet.DoQuery(s);
                                                        //lRetCode = Reg.ActEstadoFolioUpt((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value), (System.Int32)(oRecordSet.Fields.Item("LineId").Value), (System.Double)(oRecordSet.Fields.Item("U_Folio").Value), TipoDocElec, sDocEntry, "13", sDocSubType);
                                                        bFolioAsignado = true;

                                                        if (GeneraT == "Y")
                                                        {
                                                            //Colocar Timbre
                                                            XmlDocument xmlCAF = new XmlDocument();
                                                            XmlDocument xmlTimbre = new XmlDocument();
                                                            if (CAF == "")
                                                                throw new Exception("No se ha encontrado xml de CAF");
                                                            //OutLog(oRecordSet.Fields.Item("U_CAF").Value.ToString());
                                                            xmlCAF.LoadXml(CAF);
                                                            xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElec, Convert.ToString(oDocument.FolioNumber), oDocument.DocDate.ToString("yyyyMMdd"), oDocument.FederalTaxID.Replace(".", ""), oDocument.CardName, Convert.ToString(Math.Round(oDocument.DocTotal, 0)), oDocument.Lines.ItemDescription, xmlCAF, TaxIdNum);

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
                                                                        FSBOApp.StatusBar.SetText("No se ha creado Timbre en el documento - " + s + " - " + FCmpny.GetLastErrorDescription(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                                        OutLog("No se ha creado Timbre en el documento: " + sDocEntry + " Tipo: " + oForm.BusinessObject.Type + " - " + s + " - " + FCmpny.GetLastErrorDescription());
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
                                                            EnviarFE_WebService(oForm.BusinessObject.Type, oDocument, TipoDocElec, bMultiSoc, nMultiSoc, GlobalSettings.RunningUnderSQLServer, sDocSubType, TTipoDoc, (FolioPortal == "Y" ? true : false));
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                            FSBOApp.StatusBar.SetText("No se encuentra numeros disponibles para SBO", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                    }//fin if de Distribuido
                                    else if ((Distribuido == "N") & (FolioPortal == "Y"))//folio es asignado por el portal
                                    {//dejo el documento como impreso para que sap no asigne folio
                                        if (oForm.BusinessObject.Type == "203")
                                            oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments));
                                        else
                                            oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices));

                                        if (oDocument.GetByKey(Convert.ToInt32(sDocEntry)))
                                        {
                                            oDocument.Printed = PrintStatusEnum.psYes;
                                            lRetCode = oDocument.Update();
                                            if (lRetCode != 0)
                                            {
                                                if (GlobalSettings.RunningUnderSQLServer)
                                                    s = "update {0} set Printed = 'Y' where DocEntry = {1}";
                                                else
                                                    s = @"update ""{0}"" set ""Printed"" = 'Y' where ""DocEntry"" = {1}";
                                                s = String.Format(s, tabla, sDocEntry);
                                                oRecordSet.DoQuery(s);
                                                OutLog("No se actualizo campo Printed por DIAPI DocEntry: " + sDocEntry + " Tipo: " + oForm.BusinessObject.Type + " - " + FCmpny.GetLastErrorDescription());
                                            }
                                            //ahora debo marcar que el folio fue usado y colocar los datos del documento que uso el folio
                                            Reg = new TFunctions();
                                            Reg.SBO_f = FSBOf;
                                            lRetCode = 1;
                                            if (lRetCode != 0)
                                            {
                                                SBO_f = FSBOf;
                                                EnviarFE_WebService(oForm.BusinessObject.Type, oDocument, TipoDocElec, bMultiSoc, nMultiSoc, GlobalSettings.RunningUnderSQLServer, sDocSubType, TTipoDoc, (FolioPortal == "Y" ? true : false));
                                            }
                                            //--
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Canceled == "N")
                                    FSBOApp.StatusBar.SetText("Documento creado no es electronico", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            }
                            //**}
                        }
                        else
                            FSBOApp.StatusBar.SetText("Debe Parametrizar el Addon", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    }
                    else
                        Flag = true;
                }
                else if ((BusinessObjectInfo.BeforeAction == false) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_UPDATE) && (BusinessObjectInfo.ActionSuccess))
                {
                    if ((oForm.BusinessObject.Type == "13") || (oForm.BusinessObject.Type == "112"))
                    {
                        Flag = false;
                        if (oForm.BusinessObject.Type == "112")
                            tabla = "ODRF";
                        else
                            tabla = "OINV";
                        sDocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', 
                                                     SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'TipoDocElect', 
                                                     T0.CANCELED
                                             FROM {1} T0 WITH (NOLOCK)
                                                JOIN NNM1 T2 WITH (NOLOCK) ON T0.Series = T2.Series 
                                               WHERE T0.DocEntry = {0}";
                        }
                        else
                        {
                            s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"",
                                             SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""TipoDocElect"", 
                                                     T0.""CANCELED""
                                             FROM ""{1}"" T0
                                             JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                                            WHERE T0.""DocEntry"" = {0} ";
                        }
                        s = String.Format(s, sDocEntry, tabla);
                        oRecordSet.DoQuery(s);
                        sDocSubType = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                        Tipo = (System.String)(oRecordSet.Fields.Item("Tipo").Value);
                        TipoDocElect = (System.String)(oRecordSet.Fields.Item("TipoDocElect").Value);
                        Canceled = (System.String)(oRecordSet.Fields.Item("CANCELED").Value);

                        if ((Tipo == "E") && (Canceled == "N") && (TipoDocElect != "43"))
                        {
                            if (sDocSubType == "--") //Factura
                                TipoDocElec = "33";
                            else if (sDocSubType == "IE") //Factura Exenta
                                TipoDocElec = "34";
                            else if ((sDocSubType == "DN") && (TipoDocElect == "111")) //Nota Debito
                                TipoDocElec = "111";
                            else if ((sDocSubType == "DN") && (TipoDocElect != "111")) //Nota Debito
                                TipoDocElec = "56";
                            else if (sDocSubType == "IB") //Boleta
                                TipoDocElec = "39";
                            else if (sDocSubType == "EB") //Boleta Exenta
                                TipoDocElec = "41";
                            else if (sDocSubType == "IX") //Factura Exportacion
                                TipoDocElec = "110";
                            //Agregar referencia en las tablas de usuario
                            if ((sDocSubType == "DN") && ((TipoDocElect == "111") || (TipoDocElect == "56")))
                            {
                                if (!GuardarReferencia(sDocEntry, tabla, true, (oForm.BusinessObject.Type == "112" ? true : false)))
                                    FSBOApp.StatusBar.SetText("No se ha guardado las referencias", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            }
                            //Fin Agregar referencia en las tablas de usuario
                        }
                    }
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

                FSBOApp.StatusBar.SetText("FormDataEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormDataEvent: " + e.Message + " ** Trace: " + e.StackTrace);

            }
        }//fin FormDataEvent


        public new void PrintEvent(ref SAPbouiCOM.PrintEventInfo eventInfo, ref Boolean BubbleEvent)
        {
            XmlDocument _xmlDocument;
            XmlNode N;

            base.PrintEvent(ref eventInfo, ref BubbleEvent);

            oForm = FSBOApp.Forms.Item(eventInfo.FormUID);

            //OutLog("PrintEvent " + eventInfo.EventType.ToString);
            if ((eventInfo.FormUID.Length > 0) && (eventInfo.WithPrinterPreferences))
            {
                if ((eventInfo.EventType == BoEventTypes.et_PRINT) && (eventInfo.BeforeAction))
                {

                    if (GlobalSettings.RunningUnderSQLServer)
                    {
                        s = @"SELECT COUNT(*) Cont
                                FROM {1} T0 
                                JOIN NNM1 T2 ON T0.Series = T2.Series 
                               WHERE (SUBSTRING(UPPER(T2.BeginStr), 1, 1) = 'E') 
                                 AND (T0.DocEntry = {0})";
                    }
                    else
                    {
                        s = @"SELECT COUNT(*) ""Cont""
                                FROM ""{1}"" T0 
                                JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                               WHERE (SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) = 'E')
                                 AND (T0.""DocEntry"" = {0}) ";
                    }
                    if (ObjType == "13")
                        s = String.Format(s, (System.String)(oForm.DataSources.DBDataSources.Item("OINV").GetValue("DocEntry", 0)), "OINV");//, DocSubType);
                    else
                        s = String.Format(s, (System.String)(oForm.DataSources.DBDataSources.Item("ODPI").GetValue("DocEntry", 0)), "ODPI");//, DocSubType);
                    oRecordSet.DoQuery(s);
                    if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                    {
                        FSBOApp.StatusBar.SetText("Documento Electronico", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                        //BubbleEvent = false; //*****************************
                    }
                }
            }
        }//fin PrintEvent


        public new void ReportDataEvent(ref SAPbouiCOM.ReportDataInfo eventInfo, ref Boolean BubbleEvent)
        {
            base.ReportDataEvent(ref eventInfo, ref BubbleEvent);
            oForm = FSBOApp.Forms.Item(eventInfo.FormUID);

            //OutLog("ReportData " + eventInfo.EventType.ToString);
            if (eventInfo.FormUID.Length > 0) //and (eventInfo.WithPrinterPreferences) then
            {
                if ((eventInfo.EventType == BoEventTypes.et_PRINT_DATA) && (eventInfo.BeforeAction))
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                    {
                        s = @"SELECT COUNT(*) Cont
                               FROM {1} T0 
                               JOIN NNM1 T2 ON T0.Series = T2.Series 
                              WHERE (SUBSTRING(UPPER(T2.BeginStr), 1, 1) = 'E') 
                                AND (T0.DocEntry = {0})";
                    }
                    else
                    {
                        s = @"SELECT COUNT(*) ""Cont""
                               FROM ""{1}"" T0
                               JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                              WHERE (SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) = 'E')
                                AND (T0.""DocEntry"" = {0}) ";
                    }
                    if (ObjType == "13")
                        s = String.Format(s, (System.String)(oForm.DataSources.DBDataSources.Item("OINV").GetValue("DocEntry", 0)), "OINV");//, DocSubType);
                    else
                        s = String.Format(s, (System.String)(oForm.DataSources.DBDataSources.Item("ODPI").GetValue("DocEntry", 0)), "ODPI");//, DocSubType);
                    oRecordSet.DoQuery(s);
                    if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                    {
                        //FSBOApp.StatusBar.SetText("Documento Electronico", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                        //BubbleEvent = false;
                    }
                }
            }
        }//fin ReportDataEvent


        private void CrearPestañas()
        {
            SAPbouiCOM.Folder oFolder;
            SAPbouiCOM.Item oItem;
            SAPbouiCOM.Item oItemB;
            SAPbouiCOM.Matrix oMatrix;
            SAPbouiCOM.GridColumns oColumns;
            SAPbouiCOM.GridColumn oColumn;
            try
            {
                oForm.DataSources.UserDataSources.Add("VID_FEAF", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                oItem = oForm.Items.Add("VID_FEAF", SAPbouiCOM.BoFormItemTypes.it_FOLDER);

                //para SAP 882 en adelante
                oItemB = oForm.Items.Item("1320002137");
                oItem.Left = oItemB.Left + 30;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top;
                oItem.Height = oItem.Height;
                oFolder = (Folder)((oItem.Specific));
                oFolder.Caption = "Fact. Elect. Activo Fijo";
                oFolder.Pane = 330;
                oFolder.DataBind.SetBound(true, "", "VID_FEAF");
                //para SAP 882 en adelante
                oFolder.GroupWith("1320002137");

                oItemB = oForm.Items.Item("2010");
                oItem = oForm.Items.Add("lblTpCmpra", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                oItem.Left = 15;
                oItem.Width = 140;
                oItem.Top = oItemB.Top;
                oItem.Height = oItem.Height;
                oItem.FromPane = 330;
                oItem.ToPane = 330;
                oItem.LinkTo = "FETpoCmpra";
                oStatic = (StaticText)(oForm.Items.Item("lblTpCmpra").Specific);
                oStatic.Caption = "Tipo Transacción Compra";

                oItemB = oForm.Items.Item("lblTpCmpra");
                oItem = oForm.Items.Add("FETpoCmpra", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                oItem.Left = oItemB.Left + oItemB.Width + 5;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top;
                oItem.Height = oItem.Height;
                oItem.FromPane = 330;
                oItem.ToPane = 330;
                oForm.Items.Item("FETpoCmpra").DisplayDesc = true;
                oComboBox = (ComboBox)(oForm.Items.Item("FETpoCmpra").Specific);
                if (ObjType == "203")
                    oComboBox.DataBind.SetBound(true, "ODPI", "U_TpoTranCpra");
                else
                    oComboBox.DataBind.SetBound(true, "OINV", "U_TpoTranCpra");

                oItemB = oForm.Items.Item("lblTpCmpra");
                oItem = oForm.Items.Add("lblTpVta", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                oItem.Left = oItemB.Left;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 1;
                oItem.Height = oItem.Height;
                oItem.FromPane = 330;
                oItem.ToPane = 330;
                oItem.LinkTo = "FETpoVta";
                oStatic = (StaticText)(oForm.Items.Item("lblTpVta").Specific);
                oStatic.Caption = "Tipo Transacción Venta";

                oItemB = oForm.Items.Item("lblTpVta");
                oItem = oForm.Items.Add("FETpoVta", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                oItem.Left = oItemB.Left + oItemB.Width + 5;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top;
                oItem.Height = oItem.Height;
                oItem.FromPane = 330;
                oItem.ToPane = 330;
                oItem.DisplayDesc = true;
                oComboBox = (ComboBox)(oForm.Items.Item("FETpoVta").Specific);
                if (ObjType == "203")
                    oComboBox.DataBind.SetBound(true, "ODPI", "U_TpoTranVta");
                else
                    oComboBox.DataBind.SetBound(true, "OINV", "U_TpoTranVta");

                oItemB = oForm.Items.Item("lblTpVta");
                oItem = oForm.Items.Add("lblCdgSuc", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                oItem.Left = oItemB.Left;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 1;
                oItem.Height = oItem.Height;
                oItem.FromPane = 330;
                oItem.ToPane = 330;
                oItem.LinkTo = "FECdgSuc";
                oStatic = (StaticText)(oForm.Items.Item("lblCdgSuc").Specific);
                oStatic.Caption = "Código SII Sucursal";

                oItemB = oForm.Items.Item("lblCdgSuc");
                oItem = oForm.Items.Add("FECdgSuc", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                oItem.Left = oItemB.Left + oItemB.Width + 5;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top;
                oItem.Height = oItem.Height;
                oItem.FromPane = 330;
                oItem.ToPane = 330;
                oItem.DisplayDesc = true;
                oEditText = (EditText)(oForm.Items.Item("FECdgSuc").Specific);
                if (ObjType == "203")
                    oEditText.DataBind.SetBound(true, "ODPI", "U_CdgSiiSuc");
                else
                    oEditText.DataBind.SetBound(true, "OINV", "U_CdgSiiSuc");

                oItemB = oForm.Items.Item("lblCdgSuc");
                oItem = oForm.Items.Add("lblSucur", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                oItem.Left = oItemB.Left;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 1;
                oItem.Height = oItem.Height;
                oItem.FromPane = 330;
                oItem.ToPane = 330;
                oItem.LinkTo = "FESucur";
                oStatic = (StaticText)(oForm.Items.Item("lblSucur").Specific);
                oStatic.Caption = "Sucursal";

                oItemB = oForm.Items.Item("lblSucur");
                oItem = oForm.Items.Add("FESucur", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                oItem.Left = oItemB.Left + oItemB.Width + 5;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top;
                oItem.Height = oItem.Height;
                oItem.FromPane = 330;
                oItem.ToPane = 330;
                oItem.DisplayDesc = true;
                oEditText = (EditText)(oForm.Items.Item("FESucur").Specific);
                if (ObjType == "203")
                    oEditText.DataBind.SetBound(true, "ODPI", "U_FESucursal");
                else
                    oEditText.DataBind.SetBound(true, "OINV", "U_FESucursal");


                if (bFolderAdd)
                {
                    oForm.DataSources.UserDataSources.Add("VID_FEDCTO", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                    oItem = oForm.Items.Add("VID_FEDCTO", SAPbouiCOM.BoFormItemTypes.it_FOLDER);

                    //para SAP 882 en adelante
                    oItemB = oForm.Items.Item("1320002137");
                    oItem.Left = oItemB.Left + 30;
                    oItem.Width = oItemB.Width;
                    oItem.Top = oItemB.Top;
                    oItem.Height = oItem.Height;
                    oFolder = (Folder)((oItem.Specific));
                    oFolder.Caption = "Factura Electrónica";
                    oFolder.Pane = 333;
                    oFolder.DataBind.SetBound(true, "", "VID_FEDCTO");
                    //para SAP 882 en adelante
                    oFolder.GroupWith("1320002137");

                    if (Liquidacion)
                    {
                        oItemB = oForm.Items.Item("19");
                        oItem = oForm.Items.Add("lblDocB", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = 15;
                        oItem.Width = 110;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEDocB";
                        oStatic = (StaticText)(oForm.Items.Item("lblDocB").Specific);
                        oStatic.Caption = "Documento Compra";

                        oItemB = oForm.Items.Item("lblDocB");
                        oItem = oForm.Items.Add("VID_FEDocB", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oEditText = ((EditText)oForm.Items.Item("VID_FEDocB").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_VID_FEDocB");
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT Series FROM NNM1 WHERE ObjectCode = '18' AND SUBSTRING(ISNULL(BeginStr,''), 2, LEN(BeginStr)) =  '43' AND NextNumber <= ISNULL(LastNum, NextNumber) AND Locked = 'N'";
                        else
                            s = @"SELECT ""Series"" FROM ""NNM1"" WHERE ""ObjectCode"" = '18' AND SUBSTRING(IFNULL(""BeginStr"",''), 2, LENGTH(""BeginStr"")) =  '43' AND ""NextNumber"" <= IFNULL(""LastNum"", ""NextNumber"") AND ""Locked"" = 'N'";
                        oRecordSet.DoQuery(s);
                        var zz = "";
                        if (oRecordSet.RecordCount > 0)
                            zz = ((System.Int32)oRecordSet.Fields.Item("Series").Value).ToString().Trim();

                        AddChooseFromList(zz);
                        oEditText.ChooseFromListUID = "CFL0";
                        oEditText.ChooseFromListAlias = "DocNum";

                    }
                    else if ((DocSubType == "IX") || (ReservaExp))
                    {
                        oItemB = oForm.Items.Item("19");
                        oItem = oForm.Items.Add("lblCodMV", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = 15;
                        oItem.Width = 110;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FECdMV";
                        oStatic = (StaticText)(oForm.Items.Item("lblCodMV").Specific);
                        oStatic.Caption = "Codigo Modo Venta";

                        oItemB = oForm.Items.Item("lblCodMV");
                        oItem = oForm.Items.Add("VID_FECdMV", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oForm.Items.Item("VID_FECdMV").DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FECdMV").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_CodModVenta");


                        oItemB = oForm.Items.Item("lblCodMV");
                        oItem = oForm.Items.Add("lblCodCV", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FECodCV";
                        oStatic = (StaticText)(oForm.Items.Item("lblCodCV").Specific);
                        oStatic.Caption = "Codigo Clausula Venta";

                        oItemB = oForm.Items.Item("lblCodCV");
                        oItem = oForm.Items.Add("VID_FECdCV", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FECdCV").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_CodClauVenta");


                        oItemB = oForm.Items.Item("lblCodCV");
                        oItem = oForm.Items.Add("lblTotCV", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEToCV";
                        oStatic = (StaticText)(oForm.Items.Item("lblTotCV").Specific);
                        oStatic.Caption = "Total Clausula Venta";

                        oItemB = oForm.Items.Item("lblTotCV");
                        oItem = oForm.Items.Add("VID_FEToCV", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEToCV").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_TotClauVenta");


                        oItemB = oForm.Items.Item("lblTotCV");
                        oItem = oForm.Items.Add("lblCodVT", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FECdVT";
                        oStatic = (StaticText)(oForm.Items.Item("lblCodVT").Specific);
                        oStatic.Caption = "Codigo Via Transporte";

                        oItemB = oForm.Items.Item("lblCodVT");
                        oItem = oForm.Items.Add("VID_FECdVT", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oForm.Items.Item("VID_FECdVT").DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FECdVT").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_CodViaTransp");

                        oItemB = oForm.Items.Item("lblTotCV");
                        oItem = oForm.Items.Add("lblNomT", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FENomT";
                        oStatic = (StaticText)(oForm.Items.Item("lblNomT").Specific);
                        oStatic.Caption = "Nombre Transportista";

                        oItemB = oForm.Items.Item("lblNomT");
                        oItem = oForm.Items.Add("VID_FENomT", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oEditText = (EditText)(oForm.Items.Item("VID_FENomT").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_NombreTransp");

                        oItemB = oForm.Items.Item("lblNomT");
                        oItem = oForm.Items.Add("lblRutT", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FERuCT";
                        oStatic = (StaticText)(oForm.Items.Item("lblRutT").Specific);
                        oStatic.Caption = "Rut Transportista";

                        oItemB = oForm.Items.Item("lblRutT");
                        oItem = oForm.Items.Add("VID_FERuCT", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oEditText = (EditText)(oForm.Items.Item("VID_FERuCT").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_RUTCiaTransp");

                        oItemB = oForm.Items.Item("lblRutT");
                        oItem = oForm.Items.Add("lblNomCT", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FENmCT";
                        oStatic = (StaticText)(oForm.Items.Item("lblNomCT").Specific);
                        oStatic.Caption = "Nombre Cia. Transp.";

                        oItemB = oForm.Items.Item("lblNomCT");
                        oItem = oForm.Items.Add("VID_FENmCT", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oEditText = (EditText)(oForm.Items.Item("VID_FENmCT").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_NomCiaTransp");

                        oItemB = oForm.Items.Item("lblNomCT");
                        oItem = oForm.Items.Add("lblIdAT", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEIdAT";
                        oStatic = (StaticText)(oForm.Items.Item("lblIdAT").Specific);
                        oStatic.Caption = "Id Adicional Transp.";

                        oItemB = oForm.Items.Item("lblIdAT");
                        oItem = oForm.Items.Add("VID_FEIdAT", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEIdAT").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_IdAdicTransp");

                        oItemB = oForm.Items.Item("lblIdAT");
                        oItem = oForm.Items.Add("lblBooki", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEBook";
                        oStatic = (StaticText)(oForm.Items.Item("lblBooki").Specific);
                        oStatic.Caption = "Booking";

                        oItemB = oForm.Items.Item("lblBooki");
                        oItem = oForm.Items.Add("VID_FEBook", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEBook").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_Booking");

                        oItemB = oForm.Items.Item("lblBooki");
                        oItem = oForm.Items.Add("lblOpera", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEOper";
                        oStatic = (StaticText)(oForm.Items.Item("lblOpera").Specific);
                        oStatic.Caption = "Operador";

                        oItemB = oForm.Items.Item("lblOpera");
                        oItem = oForm.Items.Add("VID_FEOper", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEOper").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_Operador");

                        oItemB = oForm.Items.Item("lblOpera");
                        oItem = oForm.Items.Add("lblCPtoEm", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FECPEm";
                        oStatic = (StaticText)(oForm.Items.Item("lblCPtoEm").Specific);
                        oStatic.Caption = "Codigo Pto. Embarque";

                        oItemB = oForm.Items.Item("lblCPtoEm");
                        oItem = oForm.Items.Add("VID_FECPEm", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oForm.Items.Item("VID_FECPEm").DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FECPEm").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_CodPtoEmbarque");
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT Code, Name FROM [@VID_FEPTOEMB] ";
                        else
                            s = @"SELECT ""Code"", ""Name"" FROM ""@VID_FEPTOEMB"" ";
                        oRecordSet.DoQuery(s);
                        FSBOf.FillCombo(oComboBox, ref oRecordSet, true);

                        oItemB = oForm.Items.Item("lblCPtoEm");
                        oItem = oForm.Items.Add("lblIdAPE", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEIdPE";
                        oStatic = (StaticText)(oForm.Items.Item("lblIdAPE").Specific);
                        oStatic.Caption = "Id Adicional Pto Emb.";

                        oItemB = oForm.Items.Item("lblIdAPE");
                        oItem = oForm.Items.Add("VID_FEIdPE", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEIdPE").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_IdAdicPtoEmb");

                        oItemB = oForm.Items.Item("lblIdAPE");
                        oItem = oForm.Items.Add("lblCdPD", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FECdPD";
                        oStatic = (StaticText)(oForm.Items.Item("lblCdPD").Specific);
                        oStatic.Caption = "Codigo Pto. Desembarque";

                        oItemB = oForm.Items.Item("lblCdPD");
                        oItem = oForm.Items.Add("VID_FECdPD", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oForm.Items.Item("VID_FECdPD").DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FECdPD").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_CodPtoDesemb");
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT Code, Name FROM [@VID_FEPTOEMB] ";
                        else
                            s = @"SELECT ""Code"", ""Name"" FROM ""@VID_FEPTOEMB"" ";
                        oRecordSet.DoQuery(s);
                        FSBOf.FillCombo(oComboBox, ref oRecordSet, true);

                        oItemB = oForm.Items.Item("lblCdPD");
                        oItem = oForm.Items.Add("lblIdAPD", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEIdPD";
                        oStatic = (StaticText)(oForm.Items.Item("lblIdAPD").Specific);
                        oStatic.Caption = "Id Adicional Pto Desemb.";

                        oItemB = oForm.Items.Item("lblIdAPD");
                        oItem = oForm.Items.Add("VID_FEIdPD", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEIdPD").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_IdAdicPtoDesemb");

                        oItemB = oForm.Items.Item("lblIdAPD");
                        oItem = oForm.Items.Add("lblTara", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 2;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FETara";
                        oStatic = (StaticText)(oForm.Items.Item("lblTara").Specific);
                        oStatic.Caption = "Tara";

                        oItemB = oForm.Items.Item("lblTara");
                        oItem = oForm.Items.Add("VID_FETara", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FETara").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_Tara");

                        //mitad
                        oItemB = oForm.Items.Item("19");
                        oItem = oForm.Items.Add("lblCUMT", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = 285;// oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top;// + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FECUMT";
                        oStatic = (StaticText)(oForm.Items.Item("lblCUMT").Specific);
                        oStatic.Caption = "Cod. Un. Med. Tara";

                        oItemB = oForm.Items.Item("lblCUMT");
                        oItem = oForm.Items.Add("VID_FECUMT", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oForm.Items.Item("VID_FECUMT").DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FECUMT").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_CodUnidMedTara");

                        //PesoBruto
                        oItemB = oForm.Items.Item("lblCUMT");
                        oItem = oForm.Items.Add("lblPsBr", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEPsBr";
                        oStatic = (StaticText)(oForm.Items.Item("lblPsBr").Specific);
                        oStatic.Caption = "Peso Bruto";

                        oItemB = oForm.Items.Item("lblPsBr");
                        oItem = oForm.Items.Add("VID_FEPsBr", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEPsBr").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_PesoBruto");

                        //CodUnidPesoBruto CB
                        oItemB = oForm.Items.Item("lblPsBr");
                        oItem = oForm.Items.Add("lblCUPB", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FECUPB";
                        oStatic = (StaticText)(oForm.Items.Item("lblCUPB").Specific);
                        oStatic.Caption = "Cod. Un. Peso Bruto";

                        oItemB = oForm.Items.Item("lblCUPB");
                        oItem = oForm.Items.Add("VID_FECUPB", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oForm.Items.Item("VID_FECUPB").DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FECUPB").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_CodUnidPesoBr");

                        //PesoNeto
                        oItemB = oForm.Items.Item("lblCUPB");
                        oItem = oForm.Items.Add("lblPsNe", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEPsNe";
                        oStatic = (StaticText)(oForm.Items.Item("lblPsNe").Specific);
                        oStatic.Caption = "Peso Neto";

                        oItemB = oForm.Items.Item("lblPsNe");
                        oItem = oForm.Items.Add("VID_FEPsNe", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEPsNe").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_PesoNeto");

                        //CodUnidPesoNeto CB
                        oItemB = oForm.Items.Item("lblPsNe");
                        oItem = oForm.Items.Add("lblCUPN", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FECUPN";
                        oStatic = (StaticText)(oForm.Items.Item("lblCUPN").Specific);
                        oStatic.Caption = "Cod. Un. Peso Neto";

                        oItemB = oForm.Items.Item("lblCUPN");
                        oItem = oForm.Items.Add("VID_FECUPN", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oForm.Items.Item("VID_FECUPN").DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FECUPN").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_CodUnidPesNet");

                        //TotItems
                        oItemB = oForm.Items.Item("lblCUPN");
                        oItem = oForm.Items.Add("lblToIt", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEToIt";
                        oStatic = (StaticText)(oForm.Items.Item("lblToIt").Specific);
                        oStatic.Caption = "Total Items";

                        oItemB = oForm.Items.Item("lblToIt");
                        oItem = oForm.Items.Add("VID_FEToIt", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEToIt").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_TotItems");

                        //TotBultos
                        oItemB = oForm.Items.Item("lblToIt");
                        oItem = oForm.Items.Add("lblToBu", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEToBu";
                        oStatic = (StaticText)(oForm.Items.Item("lblToBu").Specific);
                        oStatic.Caption = "Total Bultos";

                        oItemB = oForm.Items.Item("lblToBu");
                        oItem = oForm.Items.Add("VID_FEToBu", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEToBu").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_TotBultos");

                        //CodPaisRecep CB
                        oItemB = oForm.Items.Item("lblToBu");
                        oItem = oForm.Items.Add("lblCdPR", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FECdPR";
                        oStatic = (StaticText)(oForm.Items.Item("lblCdPR").Specific);
                        oStatic.Caption = "Codigo Pais Recepción";

                        oItemB = oForm.Items.Item("lblCdPR");
                        oItem = oForm.Items.Add("VID_FECdPR", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oForm.Items.Item("VID_FECdPR").DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FECdPR").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_CodPaisRecep");
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT Code, Name FROM [@VID_FECODPAIS] ";
                        else
                            s = @"SELECT ""Code"", ""Name"" FROM ""@VID_FECODPAIS"" ";
                        oRecordSet.DoQuery(s);
                        FSBOf.FillCombo(oComboBox, ref oRecordSet, true);

                        //CodPaisDestin CB
                        oItemB = oForm.Items.Item("lblCdPR");
                        oItem = oForm.Items.Add("lblCdPaD", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FECPaD";
                        oStatic = (StaticText)(oForm.Items.Item("lblCdPaD").Specific);
                        oStatic.Caption = "Codigo Pais Destino";

                        oItemB = oForm.Items.Item("lblCdPaD");
                        oItem = oForm.Items.Add("VID_FECPaD", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oForm.Items.Item("VID_FECPaD").DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FECPaD").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_CodPaisDestin");
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT Code, Name FROM [@VID_FECODPAIS] ";
                        else
                            s = @"SELECT ""Code"", ""Name"" FROM ""@VID_FECODPAIS"" ";
                        oRecordSet.DoQuery(s);
                        FSBOf.FillCombo(oComboBox, ref oRecordSet, true);

                        //TipoMoneda
                        oItemB = oForm.Items.Item("lblCdPaD");
                        oItem = oForm.Items.Add("lblTpMn", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FETpMn";
                        oStatic = (StaticText)(oForm.Items.Item("lblTpMn").Specific);
                        oStatic.Caption = "Tipo Moneda";

                        oItemB = oForm.Items.Item("lblTpMn");
                        oItem = oForm.Items.Add("VID_FETpMn", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FETpMn").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_TipoMoneda");

                        //FmaPagExp CB
                        oItemB = oForm.Items.Item("lblTpMn");
                        oItem = oForm.Items.Add("lblFmPE", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEFmPE";
                        oStatic = (StaticText)(oForm.Items.Item("lblFmPE").Specific);
                        oStatic.Caption = "Forma Pago Exportación";

                        oItemB = oForm.Items.Item("lblFmPE");
                        oItem = oForm.Items.Add("VID_FEFmPE", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oForm.Items.Item("VID_FEFmPE").DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FEFmPE").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_FmaPagExp");

                        //TipoBultos CB
                        oItemB = oForm.Items.Item("lblFmPE");
                        oItem = oForm.Items.Add("lblTipBu", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 1;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FETiBu";
                        oStatic = (StaticText)(oForm.Items.Item("lblTipBu").Specific);
                        oStatic.Caption = "Tipo Bultos";

                        oItemB = oForm.Items.Item("lblTipBu");
                        oItem = oForm.Items.Add("VID_FETiBu", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 10;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oForm.Items.Item("VID_FETiBu").DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FETiBu").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_TipoBultos");

                    }
                    else if ((DocSubType == "IB") || (DocSubType == "EB"))//para boletas
                    {
                        oItemB = oForm.Items.Item("47");
                        oItem = oForm.Items.Add("lblIndBol", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = 50;
                        oItem.Width = 90;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEInBo";
                        oStatic = (StaticText)(oForm.Items.Item("lblIndBol").Specific);
                        oStatic.Caption = "Indicador Servicio";

                        oItemB = oForm.Items.Item("lblIndBol");
                        oItem = oForm.Items.Add("VID_FEInBo", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 80;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select C1.FldValue 'Code', C1.Descr 'Name'
                                  from CUFD C0
                                  JOIN UFD1 C1 ON C1.TableID = C0.TableID
                                              AND C1.FieldID = C0.FieldID
                                 WHERE C0.TableID = 'OINV'
                                   AND C0.AliasID = 'IndServicioBol'";
                        else
                            s = @"select C1.""FldValue"" ""Code"", C1.""Descr"" ""Name""
                                  from ""CUFD"" C0
                                  JOIN ""UFD1"" C1 ON C1.""TableID"" = C0.""TableID""
                                              AND C1.""FieldID"" = C0.""FieldID""
                                 WHERE C0.""TableID"" = 'OINV'
                                   AND C0.""AliasID"" = 'IndServicioBol' ";

                        oRecordSet.DoQuery(s);
                        FSBOf.FillCombo((ComboBox)(oForm.Items.Item("VID_FEInBo").Specific), ref oRecordSet, false);
                        oForm.Items.Item("VID_FEInBo").DisplayDesc = true;

                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FEInBo").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_IndServicioBol");
                    }
                    else
                    {
                        //cargar campos de usuarios
                        oItemB = oForm.Items.Item("2010");
                        oItem = oForm.Items.Add("lblUpRef", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = 50; //oItemB.Left;
                        oItem.Width = 110;//;oItemB.Width;
                        oItem.Top = oItemB.Top;//195
                        oItem.Height = oItem.Height;//14
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEUpRe";
                        oStatic = (StaticText)(oForm.Items.Item("lblUpRef").Specific);
                        oStatic.Caption = "Código Referencia";

                        oItemB = oForm.Items.Item("lblUpRef");
                        oItem = oForm.Items.Add("VID_FEUpRe", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 60;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oForm.Items.Item("VID_FEUpRe").DisplayDesc = true;

                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"select C1.FldValue 'Code', C1.Descr 'Name'
                                  from CUFD C0
                                  JOIN UFD1 C1 ON C1.TableID = C0.TableID
                                              AND C1.FieldID = C0.FieldID
                                 WHERE C0.TableID = '@VID_FEREF'
                                   AND C0.AliasID = 'CodRef'";
                        }
                        else
                        {
                            s = @"select C1.""FldValue"" ""Code"", C1.""Descr"" ""Name""
                                  from ""CUFD"" C0
                                  JOIN ""UFD1"" C1 ON C1.""TableID"" = C0.""TableID""
                                              AND C1.""FieldID"" = C0.""FieldID""
                                 WHERE C0.""TableID"" = '@VID_FEREF'
                                   AND C0.""AliasID"" = 'CodRef' ";
                        }
                        oRecordSet.DoQuery(s);
                        FSBOf.FillCombo((ComboBox)(oForm.Items.Item("VID_FEUpRe").Specific), ref oRecordSet, false);
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FEUpRe").Specific);
                        oForm.DataSources.UserDataSources.Add("CodRef", BoDataType.dt_SHORT_TEXT, 10);
                        oComboBox.DataBind.SetBound(true, "", "CodRef");


                        oItemB = oForm.Items.Item("lblUpRef");
                        oItem = oForm.Items.Add("lblRazRef", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FERazR";
                        oStatic = (StaticText)(oForm.Items.Item("lblRazRef").Specific);
                        oStatic.Caption = "Razón Referencia";

                        oItemB = oForm.Items.Item("lblRazRef");
                        oItem = oForm.Items.Add("VID_FERazR", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 80;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oEditText = (EditText)(oForm.Items.Item("VID_FERazR").Specific);
                        oForm.DataSources.UserDataSources.Add("RazRef", BoDataType.dt_LONG_TEXT, 254);
                        oEditText.DataBind.SetBound(true, "", "RazRef");


                        oItemB = oForm.Items.Item("lblRazRef");
                        oItem = oForm.Items.Add("lblInGl", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEInGl";
                        oStatic = (StaticText)(oForm.Items.Item("lblInGl").Specific);
                        oStatic.Caption = "Indicador Global Ref";

                        oItemB = oForm.Items.Item("lblInGl");
                        oItem = oForm.Items.Add("VID_FEInGl", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 80;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FEInGl").Specific);
                        oForm.DataSources.UserDataSources.Add("IndGlobal", BoDataType.dt_SHORT_TEXT, 1);
                        oComboBox.DataBind.SetBound(true, "", "IndGlobal");
                        oForm.Items.Item("VID_FEInGl").DisplayDesc = true;
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"select C1.FldValue 'Code', C1.Descr 'Name'
                                  from CUFD C0
                                  JOIN UFD1 C1 ON C1.TableID = C0.TableID
                                              AND C1.FieldID = C0.FieldID
                                 WHERE C0.TableID = '@VID_FEREF'
                                   AND C0.AliasID = 'IndGlobal'";
                        }
                        else
                        {
                            s = @"select C1.""FldValue"" ""Code"", C1.""Descr"" ""Name"" 
                              from ""CUFD"" C0 
                              JOIN ""UFD1"" C1 ON C1.""TableID"" = C0.""TableID"" 
                                          AND C1.""FieldID"" = C0.""FieldID"" 
                             WHERE C0.""TableID"" = '@VID_FEREF' 
                               AND C0.""AliasID"" = 'IndGlobal' ";
                        }
                        oRecordSet.DoQuery(s);
                        FSBOf.FillCombo(oComboBox, ref oRecordSet, false);
                        oComboBox.Select("0", BoSearchKey.psk_ByValue);

                        oItemB = oForm.Items.Item("VID_FEInGl");
                        oItem = oForm.Items.Add("VID_FEInRf", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FEInRf").Specific);
                        oForm.DataSources.UserDataSources.Add("IndRef", BoDataType.dt_SHORT_TEXT, 10);
                        oComboBox.DataBind.SetBound(true, "", "IndRef");
                        oForm.Items.Item("VID_FEInRf").DisplayDesc = true;
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select C1.FldValue 'Code', C1.Descr 'Name'
                                  from CUFD C0
                                  JOIN UFD1 C1 ON C1.TableID = C0.TableID
                                              AND C1.FieldID = C0.FieldID
                                 WHERE C0.TableID = '@VID_FEREF'
                                   AND C0.AliasID = 'TipoDTE'";
                        else
                            s = @"select C1.""FldValue"" ""Code"", C1.""Descr"" ""Name"" 
                              from ""CUFD"" C0 
                              JOIN ""UFD1"" C1 ON C1.""TableID"" = C0.""TableID"" 
                                          AND C1.""FieldID"" = C0.""FieldID"" 
                             WHERE C0.""TableID"" = '@VID_FEREF' 
                               AND C0.""AliasID"" = 'TipoDTE'";
                        oRecordSet.DoQuery(s);
                        FSBOf.FillCombo(oComboBox, ref oRecordSet, false);
                        oComboBox.Select("00", BoSearchKey.psk_ByValue);
                        oForm.Items.Item("VID_FEInRf").Enabled = false;


                        oItemB = oForm.Items.Item("lblRazRef");
                        oItem = oForm.Items.Add("gridRefFE", SAPbouiCOM.BoFormItemTypes.it_GRID);
                        oItem.Left = oItemB.Left;
                        oItem.Width = 440;
                        oItem.Top = oItemB.Top + 35;
                        oItem.Height = 120;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "lblRazRef";


                        ogrid = ((SAPbouiCOM.Grid)(oItem.Specific));
                        odt = oForm.DataSources.DataTables.Add("VID_FEREFD");

                        ogrid.DataTable = odt;
                        odt.Columns.Add("TipoDTE", BoFieldsType.ft_AlphaNumeric, 5);
                        odt.Columns.Add("DocEntry", BoFieldsType.ft_AlphaNumeric, 20);
                        odt.Columns.Add("DocFolio", BoFieldsType.ft_AlphaNumeric, 20);
                        odt.Columns.Add("DocDate", BoFieldsType.ft_Date);
                        odt.Columns.Add("DocTotal", BoFieldsType.ft_Sum);
                        oColumns = ogrid.Columns;

                        ((GridColumn)ogrid.Columns.Item("RowsHeader")).TitleObject.Caption = "#";

                        ogrid.Columns.Item("TipoDTE").Type = BoGridColumnType.gct_ComboBox;
                        oColumn = (GridColumn)(ogrid.Columns.Item("TipoDTE"));
                        oColumn.TitleObject.Caption = "Tipo DTE";
                        ((ComboBoxColumn)oColumn).DisplayType = BoComboDisplayType.cdt_both;
                        oColumn.Width = 90;
                        oColumn.Editable = true;
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT T1.FldValue 'Code', T1.Descr 'Name'
                                                          FROM CUFD T0
                                                          JOIN UFD1 T1 ON T1.TableID = T0.TableID
                                                                      AND T1.FieldID = T0.FieldID
                                                         WHERE T0.TableID = '@VID_FEREFD'
                                                           AND T0.AliasID = 'TipoDTE'";
                        else
                            s = @"SELECT T1.""FldValue"" ""Code"", T1.""Descr"" ""Name""
                                                          FROM ""CUFD"" T0
                                                          JOIN ""UFD1"" T1 ON T1.""TableID"" = T0.""TableID""
                                                                      AND T1.""FieldID"" = T0.""FieldID""
                                                         WHERE T0.""TableID"" = '@VID_FEREFD'
                                                           AND T0.""AliasID"" = 'TipoDTE'";
                        oRecordSet.DoQuery(s);
                        FSBOf.FillComboGrid(ogrid.Columns.Item("TipoDTE"), ref oRecordSet, false);

                        ogrid.Columns.Item("DocEntry").Type = BoGridColumnType.gct_EditText;
                        oColumn = (GridColumn)(ogrid.Columns.Item("DocEntry"));
                        oColumn.TitleObject.Caption = "Doc SBO";
                        oColumn.Width = 90;
                        oColumn.RightJustified = true;
                        oColumn.Editable = false;

                        ogrid.Columns.Item("DocFolio").Type = BoGridColumnType.gct_EditText;
                        oColumn = (GridColumn)(ogrid.Columns.Item("DocFolio"));
                        oColumn.TitleObject.Caption = "Folio";
                        oColumn.RightJustified = true;
                        oColumn.Width = 90;
                        oColumn.Editable = true;

                        ogrid.Columns.Item("DocDate").Type = BoGridColumnType.gct_EditText;
                        oColumn = (GridColumn)(ogrid.Columns.Item("DocDate"));
                        oColumn.TitleObject.Caption = "Fecha";
                        oColumn.Width = 90;
                        oColumn.Editable = true;

                        ogrid.Columns.Item("DocTotal").Type = BoGridColumnType.gct_EditText;
                        oColumn = (GridColumn)(ogrid.Columns.Item("DocTotal"));
                        oColumn.TitleObject.Caption = "Total Documento";
                        oColumn.RightJustified = true;
                        oColumn.Width = 90;
                        oColumn.Editable = false;
                        oColumn.Visible = false;

                        ogrid.AutoResizeColumns();
                        odt.Rows.Add(1);

                        if (oForm.BusinessObject.Key != "")
                        {
                            var xDocEntry = FSBOf.GetDocEntryBusinessObjectInfo(oForm.BusinessObject.Key);
                            var bdraft = false;
                            if (oForm.Title.Contains("Borrador"))
                                bdraft = true;
                            else if (oForm.Title.Contains("Draft"))
                                bdraft = true;
                            CargarReferencia("13", xDocEntry, bdraft);
                        }
                    }
                }
            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText(x.Message + ", TRACE " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CrearPestañas - " + x.Message + ", TRACE " + x.StackTrace);
            }
        }

        private void AddChooseFromList(String Series)
        {
            SAPbouiCOM.ChooseFromListCollection oCFLs;
            SAPbouiCOM.ChooseFromList oCFL;
            SAPbouiCOM.ChooseFromListCreationParams oCFLCreationParams;
            SAPbouiCOM.Conditions oCons;
            SAPbouiCOM.Condition oCon;

            oCFLs = oForm.ChooseFromLists;
            oCFLCreationParams = (ChooseFromListCreationParams)(FSBOApp.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_ChooseFromListCreationParams));

            oCFLCreationParams.MultiSelection = false;
            oCFLCreationParams.ObjectType = "18";
            oCFLCreationParams.UniqueID = "CFL0";
            oCFL = oCFLs.Add(oCFLCreationParams);


            if (Series == "")
                oCFL.SetConditions(null);
            else
            {
                oCons = oCFL.GetConditions();
                oCon = oCons.Add();
                oCon.Alias = "Series";
                oCon.Operation = BoConditionOperation.co_EQUAL;
                oCon.CondVal = Series;
                oCFL.SetConditions(oCons);
            }
        }

        private void BuscarDatosDoc(Int32 iLinea)
        {
            String TipoDTE;
            String DocDate;
            DateTime fec;
            String obj;
            String card;
            String tabla = "OINV";
            String Folio;

            oForm.Freeze(true);
            try
            {
                ogrid = ((SAPbouiCOM.Grid)oForm.Items.Item("gridRefFE").Specific);
                TipoDTE = ((System.String)ogrid.DataTable.GetValue("TipoDTE", iLinea)).Trim();
                Folio = ogrid.DataTable.GetValue("DocFolio", iLinea).ToString();
                DocDate = ((EditText)oForm.Items.Item("10").Specific).Value;
                card = ((EditText)oForm.Items.Item("4").Specific).Value;

                var doc = "";
                var bElec = false;
                var bExp = false;
                if ((TipoDTE == "34") || (TipoDTE == "32"))
                {
                    if (TipoDTE == "34")
                        bElec = true;
                    doc = "IE";
                    obj = "13";
                }
                else if ((TipoDTE == "39") || (TipoDTE == "35"))
                {
                    if (TipoDTE == "39")
                        bElec = true;
                    doc = "IB";
                    obj = "13";
                    card = "";
                }
                else if ((TipoDTE == "41") || (TipoDTE == "38"))
                {
                    if (TipoDTE == "41")
                        bElec = true;
                    doc = "EB";
                    obj = "13";
                    card = "";
                }
                else if ((TipoDTE == "56") || (TipoDTE == "55"))
                {
                    if (TipoDTE == "56")
                        bElec = true;
                    doc = "DN";
                    obj = "13";
                }
                else if ((TipoDTE == "110") || (TipoDTE == "101"))
                {
                    if (TipoDTE == "110")
                    {
                        bElec = true;
                        bExp = true;
                    }
                    doc = "IX";
                    obj = "13";
                }
                else if ((TipoDTE == "111") || (TipoDTE == "104"))
                {
                    if (TipoDTE == "111")
                    {
                        bElec = true;
                        bExp = true;
                    }
                    doc = "DN";
                    obj = "13";
                }
                else if ((TipoDTE == "112") || (TipoDTE == "106"))
                {
                    if (TipoDTE == "112")
                    {
                        bElec = true;
                        bExp = true;
                    }
                    doc = "--";
                    obj = "14";
                    tabla = "ORIN";
                }
                else if ((TipoDTE == "46") || (TipoDTE == "45"))
                {
                    if (TipoDTE == "46")
                        bElec = true;
                    doc = "--";
                    obj = "18";
                    tabla = "OPCH";
                }
                else if ((TipoDTE == "46a") || (TipoDTE == "45a"))
                {
                    if (TipoDTE == "46a")
                        bElec = true;
                    doc = "--";
                    obj = "204";
                    tabla = "ODPO";
                }
                else if (TipoDTE == "33a")
                {
                    bElec = true;
                    doc = "--";
                    obj = "203";
                    tabla = "ODPI";
                }
                else if ((TipoDTE == "33") || (TipoDTE == "30"))
                {
                    if (TipoDTE == "33")
                        bElec = true;
                    doc = "--";
                    obj = "13";
                    tabla = "OINV";
                }
                else if ((TipoDTE == "61") || (TipoDTE == "60"))
                {
                    if (TipoDTE == "61")
                        bElec = true;
                    doc = "--";
                    obj = "14";
                    tabla = "ORIN";
                }
                else
                {
                    doc = "--";
                    obj = "13";
                }

                if (GlobalSettings.RunningUnderSQLServer)
                {
                    s = @"SELECT T0.DocEntry 'DocEntry'
	                              ,T0.FolioNum 'Folio'
	                              ,T0.DocDate 'DocDate'
                                  ,T0.DocTotal
                              FROM {0} T0
                              JOIN NNM1 N1 ON N1.Series = T0.Series
							              --AND N1.ObjectCode = T0.ObjType
                             WHERE T0.DocSubType = '{2}'
                               AND ((T0.CardCode = '{1}') OR ('{1}' = ''))
                               AND ISNULL(T0.FolioNum,0) = {3}
                               AND T0.CANCELED = 'N'
                               {4}
                               {5}";
                    s = String.Format(s, tabla, card, doc, Folio, (bElec ? "AND LEFT(UPPER(ISNULL(BeginStr,'')), 1) = 'E'" : "AND LEFT(UPPER(ISNULL(BeginStr,'')), 1) <> 'E'"), (bExp ? "AND SUBSTRING(ISNULL(N1.BeginStr,''), 2, LEN(ISNULL(N1.BeginStr,''))) = '" + TipoDTE + "'" : "AND SUBSTRING(ISNULL(N1.BeginStr,''), 2, LEN(ISNULL(N1.BeginStr,''))) NOT IN ('110','111','112')"));
                }
                else
                {
                    s = @"SELECT T0.""DocEntry"" ""DocEntry""
	                              ,T0.""FolioNum"" ""Folio""
	                              ,T0.""DocDate"" ""DocDate""
                                  ,T0.""DocTotal""
                              FROM ""{0}"" T0
                              JOIN ""NNM1"" N1 ON N1.""Series"" = T0.""Series""
							              --AND N1.""ObjectCode"" = T0.""ObjType""
                             WHERE T0.""DocSubType"" = '{2}'
                               AND ((T0.""CardCode"" = '{1}') OR ('{1}' = ''))
                               AND IFNULL(T0.""FolioNum"",0) = {3}
                               AND T0.""CANCELED"" = 'N'
                               {4}
                               {5}";
                    s = String.Format(s, tabla, card, doc, Folio, (bElec ? @"AND LEFT(UPPER(IFNULL(""BeginStr"",'')), 1) = 'E'" : @"AND LEFT(UPPER(IFNULL(""BeginStr"",'')), 1) <> 'E'"), (bExp ? @"AND SUBSTRING(IFNULL(N1.""BeginStr"",''), 2, LENGTH(IFNULL(N1.""BeginStr"",''))) = '" + TipoDTE + "'" : @"AND SUBSTRING(IFNULL(N1.""BeginStr"",''), 2, LENGTH(IFNULL(N1.""BeginStr"",''))) NOT IN ('110','111','112')"));
                }
                oRecordSet.DoQuery(s);
                //fec.ToString("yyyyMMdd"), DocDate
                if (oRecordSet.RecordCount == 0)
                {
                    FSBOApp.StatusBar.SetText("No se ha encontrado documentos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    ogrid.DataTable.SetValue("DocEntry", iLinea, "0");
                    ogrid.DataTable.SetValue("DocDate", iLinea, "");
                    ogrid.DataTable.SetValue("DocTotal", iLinea, "0");
                    ogrid.DataTable.SetValue("DocFolio", iLinea, "0");
                    ogrid.AutoResizeColumns();
                }
                else
                {
                    var DocEntryRef = ((System.Int32)oRecordSet.Fields.Item("DocEntry").Value).ToString();
                    var DocDateRef = ((System.DateTime)oRecordSet.Fields.Item("DocDate").Value).ToString("yyyyMMdd");
                    var DocTotalRef = ((System.Double)oRecordSet.Fields.Item("DocTotal").Value).ToString();

                    ogrid.DataTable.SetValue("DocEntry", iLinea, DocEntryRef);
                    ogrid.DataTable.SetValue("DocDate", iLinea, DocDateRef);
                    ogrid.DataTable.SetValue("DocTotal", iLinea, DocTotalRef);
                    ogrid.AutoResizeColumns();
                }
            }
            catch (Exception w)
            {
                FSBOApp.StatusBar.SetText(w.Message + ", TRACE " + w.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("BuscarDatosDoc - " + w.Message + ", TRACE " + w.StackTrace);
            }
            finally
            {
                oForm.Freeze(false);
            }
        }

        private void CargarReferencia(String Type, String sDocEntry, Boolean bdraf)
        {
            Int32 DocEntryFE;
            try
            {
                oForm.Freeze(true);
                ogrid = (SAPbouiCOM.Grid)(oForm.Items.Item("gridRefFE").Specific);
                ogrid.DataTable.Rows.Clear();

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT DocEntry, U_CodRef, U_RazRef, ISNULL(U_IndGlobal,'0') 'U_IndGlobal', ISNULL(U_TipoDTE,'00') 'U_TipoDTE' FROM [@VID_FEREF] WHERE {2} = {0} AND U_DocSBO = '{1}'";
                else
                    s = @"SELECT ""DocEntry"", ""U_CodRef"", ""U_RazRef"", IFNULL(""U_IndGlobal"",'0') ""U_IndGlobal"", IFNULL(""U_TipoDTE"",'00') ""U_TipoDTE"" FROM ""@VID_FEREF"" WHERE ""{2}"" = {0} AND ""U_DocSBO"" = '{1}'";
                s = String.Format(s, sDocEntry, Type, (bdraf ? "U_draftKey" : "U_DocEntry"));
                //OutLog("Query linea 1545 " + s);
                oRecordSet.DoQuery(s);
                DocEntryFE = ((System.Int32)oRecordSet.Fields.Item("DocEntry").Value);

                oForm.DataSources.UserDataSources.Item("CodRef").Value = ((System.String)oRecordSet.Fields.Item("U_CodRef").Value).Trim();
                oForm.DataSources.UserDataSources.Item("RazRef").Value = ((System.String)oRecordSet.Fields.Item("U_RazRef").Value).Trim();
                oForm.DataSources.UserDataSources.Item("IndGlobal").Value = ((System.String)oRecordSet.Fields.Item("U_IndGlobal").Value).Trim();
                oForm.DataSources.UserDataSources.Item("IndRef").Value = ((System.String)oRecordSet.Fields.Item("U_TipoDTE").Value).Trim();

                var sIndGlobal = ((System.String)oForm.DataSources.UserDataSources.Item("IndGlobal").Value);
                if (sIndGlobal == "1")
                {
                    ogrid.DataTable.Rows.Clear();
                    oForm.Items.Item("gridRefFE").Enabled = false;
                    oForm.Items.Item("VID_FEInRf").Enabled = true;
                }
                else
                {
                    oForm.Items.Item("gridRefFE").Enabled = true;
                    oForm.Items.Item("VID_FEInRf").Enabled = false;

                }

                if (oForm.Items.Item("gridRefFE").Enabled)
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT U_DocEntry, U_DocDate, U_DocFolio, U_LineaRef, U_DocTotal, U_TipoDTE FROM [@VID_FEREFD] WHERE DocEntry = {0}";
                    else
                        s = @"SELECT ""U_DocEntry"", ""U_DocDate"", ""U_DocFolio"", ""U_LineaRef"", ""U_DocTotal"", ""U_TipoDTE"" FROM ""@VID_FEREFD"" WHERE ""DocEntry"" = {0}";
                    s = String.Format(s, DocEntryFE);
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount == 0)
                    {
                        ogrid.DataTable.Rows.Add(1);
                        ogrid.AutoResizeColumns();
                    }
                    else
                    {
                        var m = 0;
                        while (!oRecordSet.EoF)
                        {
                            var DocEntryRef = ((System.String)oRecordSet.Fields.Item("U_DocEntry").Value).Trim();
                            var DocDateRef = ((System.DateTime)oRecordSet.Fields.Item("U_DocDate").Value).ToString("yyyyMMdd");
                            var DocTotalRef = ((System.Double)oRecordSet.Fields.Item("U_DocTotal").Value).ToString();
                            var DocTipoDTE = ((System.String)oRecordSet.Fields.Item("U_TipoDTE").Value).Trim();
                            var DocFolio = ((System.Int32)oRecordSet.Fields.Item("U_DocFolio").Value).ToString();
                            ogrid.DataTable.Rows.Add(1);
                            ogrid.DataTable.SetValue("DocEntry", m, DocEntryRef);
                            ogrid.DataTable.SetValue("DocDate", m, DocDateRef);
                            ogrid.DataTable.SetValue("DocTotal", m, DocTotalRef);
                            ogrid.DataTable.SetValue("TipoDTE", m, DocTipoDTE);
                            ogrid.DataTable.SetValue("DocFolio", m, DocFolio);
                            oRecordSet.MoveNext();
                            m++;
                        }
                    }
                }
            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText("CargarReferencia - " + x.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CargarReferencia - " + x.Message + ", TRACE " + x.StackTrace);
            }
            finally
            {
                oForm.Freeze(false);
            }
        }


        private Boolean GuardarReferencia(String sDocEntry, String tabla, Boolean bActualizar, Boolean bdraf)
        {
            SAPbobsCOM.GeneralService oGeneralService = null;
            SAPbobsCOM.GeneralData oGeneralData = null;
            SAPbobsCOM.GeneralData oChild = null;
            SAPbobsCOM.GeneralDataCollection oChildren = null;
            SAPbobsCOM.GeneralDataParams oGeneralParams = null;
            SAPbobsCOM.CompanyService oCompanyService = null;
            Int32 EntryRef = 0;
            String ObjType;
            String DocSubType;
            String StrDummy;
            Int32 drafKey;

            try
            {
                ogrid = ((Grid)oForm.Items.Item("gridRefFE").Specific);
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT DocSubType, DocNum, ObjType, ISNULL(draftKey,0) 'draftKey' FROM {0} WHERE DocEntry = {1}";
                else
                    s = @"SELECT ""DocSubType"", ""DocNum"", ""ObjType"", IFNULL(""draftKey"",0) ""draftKey"" FROM ""{0}"" WHERE ""DocEntry"" = {1}";
                s = String.Format(s, tabla, sDocEntry);
                oRecordSet.DoQuery(s);

                ObjType = ((System.String)oRecordSet.Fields.Item("ObjType").Value).Trim();
                DocSubType = ((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim();
                drafKey = ((System.Int32)oRecordSet.Fields.Item("draftKey").Value);

                if (drafKey != -1)
                    bActualizar = true;

                oCompanyService = FCmpny.GetCompanyService();
                oGeneralService = oCompanyService.GetGeneralService("VID_FERefDocs");

                if (bActualizar)
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT DocEntry FROM [@VID_FEREF] WHERE {3} = {0} AND U_DocSBO = {1} AND U_DocSubTp = '{2}'";
                    else
                        s = @"SELECT ""DocEntry"" FROM ""@VID_FEREF"" WHERE ""{3}"" = {0} AND ""U_DocSBO"" = {1} AND ""U_DocSubTp"" = '{2}'";
                    if (drafKey != -1)
                        s = String.Format(s, drafKey, ObjType, DocSubType, "U_draftKey");
                    else
                        s = String.Format(s, sDocEntry, ObjType, DocSubType, "U_DocEntry");
                    oRecordSet.DoQuery(s);

                    if (oRecordSet.RecordCount > 0)
                    {
                        bActualizar = true;
                        EntryRef = ((System.Int32)oRecordSet.Fields.Item("DocEntry").Value);
                    }
                    else
                    {
                        if (drafKey != -1)
                        {
                            if (GlobalSettings.RunningUnderSQLServer)
                                s = @"SELECT DocEntry FROM [@VID_FEREF] WHERE U_DocEntry = {0} AND U_DocSBO = {1} AND U_DocSubTp = '{2}'";
                            else
                                s = @"SELECT ""DocEntry"" FROM ""@VID_FEREF"" WHERE ""U_DocEntry"" = {0} AND ""U_DocSBO"" = {1} AND ""U_DocSubTp"" = '{2}'";
                            s = String.Format(s, sDocEntry, ObjType, DocSubType);
                            oRecordSet.DoQuery(s);
                            if (oRecordSet.RecordCount > 0)
                            {
                                bActualizar = true;
                                EntryRef = ((System.Int32)oRecordSet.Fields.Item("DocEntry").Value);
                            }
                            else
                                bActualizar = false;
                        }
                        else
                            bActualizar = false;
                    }
                }

                if (bActualizar)
                {
                    oGeneralParams = ((SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams)));
                    oGeneralParams.SetProperty("DocEntry", EntryRef);
                    oGeneralData = oGeneralService.GetByParams(oGeneralParams);
                    StrDummy = "VID_FEREFD";
                    oChildren = oGeneralData.Child(StrDummy);
                    for (Int32 m = 1; oChildren.Count > 0; m++)
                        oChildren.Remove(0);
                    oGeneralService.Update(oGeneralData);
                    oGeneralParams = null;

                    oGeneralParams = ((SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams)));
                    oGeneralParams.SetProperty("DocEntry", EntryRef);
                    oGeneralData = oGeneralService.GetByParams(oGeneralParams);
                    // Update UDO record
                    if (bdraf)
                        oGeneralData.SetProperty("U_DocEntry", "0");
                    else
                        oGeneralData.SetProperty("U_DocEntry", sDocEntry);
                    oGeneralData.SetProperty("U_DocSBO", ObjType);
                    oGeneralData.SetProperty("U_DocSubTp", DocSubType);
                    oGeneralData.SetProperty("U_CodRef", ((System.String)oForm.DataSources.UserDataSources.Item("CodRef").Value).Trim());
                    oGeneralData.SetProperty("U_RazRef", ((System.String)oForm.DataSources.UserDataSources.Item("RazRef").Value).Trim());
                    oGeneralData.SetProperty("U_IndGlobal", ((System.String)oForm.DataSources.UserDataSources.Item("IndGlobal").Value).Trim());
                    oGeneralData.SetProperty("U_TipoDTE", ((System.String)oForm.DataSources.UserDataSources.Item("IndRef").Value).Trim());
                    if (bdraf)
                        oGeneralData.SetProperty("U_draftKey", sDocEntry);
                    StrDummy = "VID_FEREFD";
                    oChildren = oGeneralData.Child(StrDummy);

                    for (Int32 i = 0; i < ogrid.DataTable.Rows.Count; i++)
                    {
                        if ((ogrid.DataTable.GetValue("TipoDTE", i) != "00") && (ogrid.DataTable.GetValue("DocFolio", i) != "") && (ogrid.DataTable.GetValue("DocFolio", i) != "0"))
                        {
                            oChild = oChildren.Add();
                            oChild.SetProperty("U_TipoDTE", ((System.String)ogrid.DataTable.GetValue("TipoDTE", i)).Trim());
                            oChild.SetProperty("U_DocEntry", ((System.String)ogrid.DataTable.GetValue("DocEntry", i)));
                            oChild.SetProperty("U_DocFolio", ((System.String)ogrid.DataTable.GetValue("DocFolio", i)));
                            s = ((System.DateTime)ogrid.DataTable.GetValue("DocDate", i)).ToString("yyyyMMdd");
                            oChild.SetProperty("U_DocDate", FSBOf.StrToDate(((System.DateTime)ogrid.DataTable.GetValue("DocDate", i)).ToString("yyyyMMdd")));
                            oChild.SetProperty("U_DocTotal", FSBOf.StrToDouble(ogrid.DataTable.GetValue("DocTotal", i).ToString()));
                            oChild.SetProperty("U_LineaRef", i);
                        }
                    }

                    oGeneralService.Update(oGeneralData);
                    CargarReferencia(ObjType, sDocEntry, bdraf);
                }
                else
                {
                    oGeneralData = ((SAPbobsCOM.GeneralData)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData)));
                    if (bdraf)
                        oGeneralData.SetProperty("U_DocEntry", "0");//sDocEntry);
                    else
                        oGeneralData.SetProperty("U_DocEntry", sDocEntry);
                    oGeneralData.SetProperty("U_DocSBO", ObjType);
                    oGeneralData.SetProperty("U_DocSubTp", DocSubType);
                    oGeneralData.SetProperty("U_CodRef", ((System.String)oForm.DataSources.UserDataSources.Item("CodRef").Value).Trim());
                    oGeneralData.SetProperty("U_RazRef", ((System.String)oForm.DataSources.UserDataSources.Item("RazRef").Value).Trim());
                    oGeneralData.SetProperty("U_IndGlobal", ((System.String)oForm.DataSources.UserDataSources.Item("IndGlobal").Value).Trim());
                    oGeneralData.SetProperty("U_TipoDTE", ((System.String)oForm.DataSources.UserDataSources.Item("IndRef").Value).Trim());
                    if (bdraf)
                        oGeneralData.SetProperty("U_draftKey", sDocEntry);
                    //  Handle child rows
                    oChildren = oGeneralData.Child("VID_FEREFD");
                    for (Int32 i = 0; i < ogrid.DataTable.Rows.Count; i++)
                    {
                        if ((((System.String)ogrid.DataTable.GetValue("TipoDTE", i)).Trim() != "00") && (((System.String)ogrid.DataTable.GetValue("DocFolio", i)).Trim() != "") && (((System.String)ogrid.DataTable.GetValue("DocFolio", i)) != "0"))
                        {
                            oChild = oChildren.Add();
                            oChild.SetProperty("U_TipoDTE", ((System.String)ogrid.DataTable.GetValue("TipoDTE", i)).Trim());
                            oChild.SetProperty("U_DocEntry", ((System.String)ogrid.DataTable.GetValue("DocEntry", i)));
                            oChild.SetProperty("U_DocFolio", ((System.String)ogrid.DataTable.GetValue("DocFolio", i)));
                            s = ((System.DateTime)ogrid.DataTable.GetValue("DocDate", i)).ToString("yyyyMMdd");
                            oChild.SetProperty("U_DocDate", FSBOf.StrToDate(((System.DateTime)ogrid.DataTable.GetValue("DocDate", i)).ToString("yyyyMMdd")));
                            oChild.SetProperty("U_DocTotal", FSBOf.StrToDouble(((System.Double)ogrid.DataTable.GetValue("DocTotal", i)).ToString()));
                            oChild.SetProperty("U_LineaRef", i);
                        }
                    }
                    // Add the new row, including children, to database
                    oGeneralParams = oGeneralService.Add(oGeneralData);
                    //txtCode.Text = System.Convert.ToString(oGeneralParams.GetProperty("DocEntry"));
                }
                return true;
            }
            catch (Exception o)
            {
                FSBOApp.StatusBar.SetText("GuardarReferencia - " + o.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("GuardarReferencia - " + o.Message + ", TRACE " + o.StackTrace);
                return false;
            }
            finally
            {
                FSBOf._ReleaseCOMObject(oGeneralService);
                FSBOf._ReleaseCOMObject(oGeneralData);
                FSBOf._ReleaseCOMObject(oChild);
                FSBOf._ReleaseCOMObject(oChildren);
                FSBOf._ReleaseCOMObject(oGeneralParams);
                FSBOf._ReleaseCOMObject(oCompanyService);
            }
        }

        //Para webservice EasyDot Chile
        public new void EnviarFE_WebService(String sObjType, SAPbobsCOM.Documents oDocumento, String TipoDocElec, Boolean bMultiSoc, String nMultiSoc, Boolean RunningUnderSQLServer, String DocType, String TipoDocElecAddon, Boolean bFPortal)
        {
            String URL;
            XmlDocument oXml = null;
            XDocument miXML = null;
            XElement xNodo;
            String sXML = "";
            String userED = "";
            String passED = "";
            TFunctions Reg = new TFunctions();
            Reg.SBO_f = SBO_f;
            SAPbobsCOM.Company Cmpny = SBO_f.Cmpny;
            Reg.SBO_f = SBO_f;
            String Status;
            String sMessage = "";
            String jStatus = "";
            String jCodigo = "";
            String jFolio = "";
            String jDescripcion = "";
            Int32 lRetCode;
            String DocDate = "";
            String ProcE = "";
            String ProcD = "";
            String ProcR = "";
            String ProcC = "";
            String TaxIdNum = "";
            String OP18 = "";
            String OP8 = "";
            String URLPDF = "";
            String MostrarXML = "N";
            SAPbobsCOM.Recordset ors = ((SAPbobsCOM.Recordset)Cmpny.GetBusinessObject(BoObjectTypes.BoRecordset));
            SAPbobsCOM.Recordset ors2 = ((SAPbobsCOM.Recordset)Cmpny.GetBusinessObject(BoObjectTypes.BoRecordset));
            String URLPDFConstruyeApirest = "http://rest1.easydoc.cl/api/Dte/ObtenerPdf";
           //ºº

            try
            {
             TDLLparaXML Dll = new TDLLparaXML();
             Dll.SBO_f = SBO_f;
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
                if ((System.String)(ors.Fields.Item("OP18").Value).ToString().Trim() == "")
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

                    URLPDFConstruyeApirest = ((System.String)ors.Fields.Item("ObtPdf").Value).ToString().Trim();
                    //validar que exista procedimentos para tipo documento
                    URL = ((System.String)ors.Fields.Item("URL").Value).Trim();

                    DocDate = SBO_f.DateToStr(oDocumento.DocDate);

                    if (RunningUnderSQLServer)
                        s = @"SELECT ISNULL(U_ProcNomE,'') 'ProcNomE', ISNULL(U_ProcNomD,'') 'ProcNomD', ISNULL(U_ProcNomR,'') 'ProcNomR', ISNULL(U_ProcNomC,'') 'ProcNomC' FROM [@VID_FEPROCED] where ISNULL(U_Habili,'N') = 'Y' and U_TipoDoc = '{0}'";
                    else
                        s = @"SELECT IFNULL(""U_ProcNomE"",'') ""ProcNomE"", IFNULL(""U_ProcNomD"",'') ""ProcNomD"", IFNULL(""U_ProcNomR"",'') ""ProcNomR"", IFNULL(""U_ProcNomC"",'') ""ProcNomC"" FROM ""@VID_FEPROCED"" where IFNULL(""U_Habili"",'N') = 'Y' and ""U_TipoDoc"" = '{0}'";

                    s = String.Format(s, TipoDocElec);
                    ors.DoQuery(s);
                    if (ors.RecordCount == 0)
                        throw new Exception("No se encuentra procedimientos para Documento electronico " + TipoDocElec);
                    else if (((System.String)ors.Fields.Item("ProcNomE").Value).Trim() == "")
                        throw new Exception("No se encuentra procedimiento Encabezado para Documento electronico " + TipoDocElec);
                    else if (((System.String)ors.Fields.Item("ProcNomD").Value).Trim() == "")
                        throw new Exception("No se encuentra procedimiento Detalle para Documento electronico " + TipoDocElec);
                    else if ((((System.String)ors.Fields.Item("ProcNomR").Value).Trim() == "") && (TipoDocElec == "56"))
                        throw new Exception("No se encuentra procedimiento Referencia para Documento electronico " + TipoDocElec);
                    else if ((((System.String)ors.Fields.Item("ProcNomC").Value).Trim() == "") && (TipoDocElec == "43"))
                        throw new Exception("No se encuentra procedimiento Comisiones para Documento electronico " + TipoDocElec);
                    else
                    {
                        ProcE = ((System.String)ors.Fields.Item("ProcNomE").Value).Trim();
                        ProcD = ((System.String)ors.Fields.Item("ProcNomD").Value).Trim();
                        ProcR = ((System.String)ors.Fields.Item("ProcNomR").Value).Trim();
                        ProcC = ((System.String)ors.Fields.Item("ProcNomC").Value).Trim();
                    }


                    //PARA ENCABEZADO
                    if (RunningUnderSQLServer)
                        s = @"exec {0} {1}, '{2}', '{3}'";//Factura
                    else
                        s = @"call {0} ({1}, '{2}', '{3}')";//Factura    
                    s = String.Format(s, ProcE, oDocumento.DocEntry, TipoDocElec, sObjType);

                    ors.DoQuery(s);
                    if (ors.RecordCount == 0)
                        throw new Exception("No se encuentra datos de encabezado para Documento electronico " + TipoDocElec);

                    //para impuestos adicionales
                    if (TipoDocElec != "43")
                    {
                        if (((System.Double)ors.Fields.Item("MntImpAdic").Value) > 0)
                        {
                            if (RunningUnderSQLServer)
                                s = @"SELECT SUM (MontoImptoAdic) 'MontoImptoAdic', CodImpAdic, PorcImptoAdic
		                            FROM VID_VW_FE_OINV_D
				                    WHERE DocEntry = {0}
		                            AND ObjType = '{1}'
                                    GROUP BY CodImpAdic, PorcImptoAdic";
                            else
                                s = @"SELECT SUM (""MontoImptoAdic"") ""MontoImptoAdic"", ""CodImpAdic"", ""PorcImptoAdic""
		                            FROM VID_VW_FE_OINV_D
				                    WHERE ""DocEntry"" = {0}
		                            AND ""ObjType"" = '{1}'
                                    GROUP BY ""CodImpAdic"",""PorcImptoAdic"" ";
                            s = String.Format(s, oDocumento.DocEntry, sObjType);
                            ors2.DoQuery(s);
                        }
                    }

                    var e_mail = ((System.String)ors.Fields.Item("CorreoRecep").Value).Trim();

                    miXML = null;
                    if (TipoDocElec == "43")
                        miXML = new XDocument(
                                             new XDeclaration("1.0", "utf-8", "yes"),
                            //new XComment("Lista de Alumnos"),
                                                new XElement("DTE",
                                                    new XElement("Liquidacion")));
                    else
                        miXML = new XDocument(
                                             new XDeclaration("1.0", "utf-8", "yes"),
                            //new XComment("Lista de Alumnos"),
                                                new XElement("DTE",
                                                    new XElement("Documento")));

                    //para pruebas en el mismo codigo
                    //if (TipoDocElec == "43")
                    //    sXML = GenerarXMLStringLiquidacionFactura(ref ors, TipoDocElec, ref miXML, "E");
                    //else
                    //    sXML = GenerarXMLStringInvoice(ref ors, ref ors2, TipoDocElec, ref miXML, "E");
                    if (TipoDocElec == "43")
                        sXML = Dll.GenerarXMLStringLiquidacionFactura(ref ors, TipoDocElec, ref miXML, "E");
                    else
                        sXML = Dll.GenerarXMLStringInvoice(ref ors, ref ors2, TipoDocElec, ref miXML, "E");

                    if (sXML == "")
                        throw new Exception("Problema para generar xml Documento electronico " + TipoDocElec);


                    //PARA DETALLE
                    if (RunningUnderSQLServer)
                        s = @"exec {0} {1}, '{2}', '{3}'";//Factura
                    else
                        s = @"call {0} ({1}, '{2}', '{3}')";//Factura    
                    s = String.Format(s, ProcD, oDocumento.DocEntry, TipoDocElec, sObjType);

                    ors.DoQuery(s);
                    if (ors.RecordCount == 0)
                        throw new Exception("No se encuentra datos de Detalle para Documento electronico (Detalle)" + TipoDocElec);

                    //if (TipoDocElec == "43")
                    //    sXML = GenerarXMLStringLiquidacionFactura(ref ors, TipoDocElec, ref miXML, "D");
                    //else
                    //    sXML = GenerarXMLStringInvoice(ref ors, ref ors2, TipoDocElec, ref miXML, "D");
                    if (TipoDocElec == "43")
                        sXML = Dll.GenerarXMLStringLiquidacionFactura(ref ors, TipoDocElec, ref miXML, "D");
                    else
                        sXML = Dll.GenerarXMLStringInvoice(ref ors, ref ors2, TipoDocElec, ref miXML, "D");


                    if (sXML == "")
                        throw new Exception("Problema para generar xml Documento electronico (Detalle)" + TipoDocElec);


                    //PARA REFERENCIA
                    if (ProcR != "")
                    {
                        if (RunningUnderSQLServer)
                            s = @"exec {0} {1}, '{2}', '{3}'";//Factura
                        else
                            s = @"call {0} ({1}, '{2}', '{3}')";//Factura    
                        s = String.Format(s, ProcR, oDocumento.DocEntry, TipoDocElec, sObjType);

                        ors.DoQuery(s);
                        if ((ors.RecordCount == 0) && (TipoDocElec == "56"))
                            throw new Exception("No se encuentra datos de Referencia para Documento electronico (Referencia)" + TipoDocElec);
                        if (ors.RecordCount > 0)
                        {
                            //if (TipoDocElec == "43")
                            //    sXML = GenerarXMLStringLiquidacionFactura(ref ors, TipoDocElec, ref miXML, "R");
                            //else
                            //    sXML = GenerarXMLStringInvoice(ref ors, ref ors2, TipoDocElec, ref miXML, "R");
                            if (TipoDocElec == "43")
                                sXML = Dll.GenerarXMLStringLiquidacionFactura(ref ors, TipoDocElec, ref miXML, "R");
                            else
                                sXML = Dll.GenerarXMLStringInvoice(ref ors, ref ors2, TipoDocElec, ref miXML, "R");

                            if (sXML == "")
                                throw new Exception("Problema para generar xml Documento electronico (Referencia)" + TipoDocElec);
                        }

                    }

                    if (TipoDocElec == "43")
                    {
                        if (ProcC != "")
                        {
                            if (RunningUnderSQLServer)
                                s = @"exec {0} {1}, '{2}', '{3}'";//Factura
                            else
                                s = @"call {0} ({1}, '{2}', '{3}')";//Factura    
                            s = String.Format(s, ProcC, oDocumento.DocEntry, TipoDocElec, sObjType);

                            ors.DoQuery(s);
                            if (ors.RecordCount == 0)
                                throw new Exception("No se encuentra datos de Comisiones para Documento electronico " + TipoDocElec);
                            if (ors.RecordCount > 0)
                            {
                                //sXML = GenerarXMLStringLiquidacionFactura(ref ors, TipoDocElec, ref miXML, "C");
                                sXML = Dll.GenerarXMLStringLiquidacionFactura(ref ors, TipoDocElec, ref miXML, "C");
                                if (sXML == "")
                                    throw new Exception("Problema para generar xml Documento electronico (Comisiones) " + TipoDocElec);
                            }

                        }
                    }

                    var bImpresion = false;

                    if (!bFPortal)
                    {
                        //Cargar PDF
                      //  OutLog("INICIO CONSTRUCCION DE PDF");

                        /*ºº aca preguntare por el parametro para construir o no el PDF 
                        //s = Reg.PDFenString(TipoDocElecAddon, oDocumento.DocEntry.ToString(), sObjType, "", oDocumento.FolioNumber.ToString(), RunningUnderSQLServer, "CL");
                       
                        // OutLog("FIN CONSTRUCCION DE PDF");

                        if (s == "")
                            throw new Exception("No se ha creado PDF");

                        //Agrega el PDF al xml
                        xNodo = new XElement("Anexo",
                                                        new XElement("PDF", s));
                        miXML.Descendants("DTE").LastOrDefault().Add(xNodo);
                         * ºº */
                    }
                        

                    //Agrega mail para la boleta
                    if ((TipoDocElec == "39") || (TipoDocElec == "41"))
                    {
                        if (e_mail != "")
                        {
                            xNodo = new XElement("Email", e_mail);
                            miXML.Descendants("Anexo").LastOrDefault().Add(xNodo);
                        }
                    }

                    //Pasar a xmlDocument
                    oXml = new XmlDocument();
                    using (var xmlReader = miXML.CreateReader())
                    {
                        oXml.Load(xmlReader);
                    }

                    if (!bFPortal)
                    {
                        //Agrega Timbre electronico
                        if (((System.String)oDocumento.UserFields.Fields.Item("U_FETimbre").Value).Trim() != "")
                        {
                            s = oXml.InnerXml;
                            s = s.Replace("</DTE>", ((System.String)oDocumento.UserFields.Fields.Item("U_FETimbre").Value).Trim()) + "</DTE>";
                            oXml.LoadXml(s);
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
                                sPath = sPath + "\\" + TipoDocElec + "- DocNum " + oDocumento.DocNum.ToString() + ".xml";
                            else
                                sPath = sPath + "\\" + TipoDocElec + "-" + oDocumento.FolioNumber.ToString() + ".xml";
                            oXml.Save(sPath);
                        }
                        sMessage = jDescripcion;
                        if (sMessage == "")
                            sMessage = "Error envio documento electronico a EasyDot";
                    }
                    else
                    {
                        if ((TipoDocElec == "39") || (TipoDocElec == "41"))
                            Status = "RR";
                        else
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

                                var FolPref = "";
                                if (TipoDocElec == "110") //Factura Exportacion
                                    FolPref = "FX";
                                else if (DocType == "--") //Factura
                                    FolPref = "FE";
                                else if (DocType == "IE") //Factura Exenta
                                    FolPref = "EE";
                                else if ((DocType == "DN") && (TipoDocElec == "43")) //Nota Debito y Liquidicion
                                    FolPref = "LI";
                                else if (DocType == "DN") //Nota Debito
                                    FolPref = "ND";
                                else if (DocType == "IB") //Boleta
                                    FolPref = "BE";
                                else if (DocType == "EB") //Boleta Exenta
                                    FolPref = "BX";
                                else if (DocType == "IX") //Factura Exportacion
                                    FolPref = "FX";
                                oDocumento.FolioPrefixString = FolPref;
                                oDocumento.FolioNumber = Convert.ToInt32(jFolio);
                                lRetCode = oDocumento.Update();
                                if (lRetCode != 0)
                                {
                                    if (RunningUnderSQLServer)
                                        s = @"UPDATE {0} SET FolioPref = '{1}', FolioNum = {2} WHERE DocEntry = {3}";
                                    else
                                        s = @"UPDATE {0} SET ""FolioPref"" = '{1}', ""FolioNum"" = {2} WHERE ""DocEntry"" = {3}";
                                    s = String.Format(s, (sObjType == "203" ? "ODPI" : "OINV"), FolPref, jFolio, oDocumento.DocEntry);
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
                                        oDocumento.UserFields.Fields.Item("U_FETimbre").Value = Response8;
                                        lRetCode = oDocumento.Update();
                                        if (lRetCode != 0)
                                        {
                                            if (RunningUnderSQLServer)
                                                s = @"UPDATE {0} SET U_FETimbre = '{1} WHERE DocEntry = {2}";
                                            else
                                                s = @"UPDATE {0} SET ""U_FETimbre"" = '{1}' WHERE ""DocEntry"" = {2}";
                                            s = String.Format(s, (sObjType == "203" ? "ODPI" : "OINV"), Response8, oDocumento.DocEntry);
                                            ors.DoQuery(s);
                                        }

                                        //URL_PDF = @"http://rest.easydoc.cl/api/Dte/SavePdf";
                                        //Cargar PDF
                                       // ººvar sPDF = Reg.PDFenString(TipoDocElecAddon, oDocumento.DocEntry.ToString(), sObjType, "", jFolio, RunningUnderSQLServer, "CL");
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
                        }//fin if (bFPortal)
                        else
                        {
                            //var OP18 = @"http://portal1.easydoc.cl/Consulta/GeneracionDte.aspx?RUT={0}&amp;FOLIO={1}&amp;TIPODTE={2}&amp;OP=18";
                            OP18 = OP18.Replace("{0}", TaxIdNum.Replace("-", "").Replace(".", ""));
                            OP18 = OP18.Replace("{1}", oDocumento.FolioNumber.ToString());
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
                              int rest =  Reg.Attachments(System.Convert.FromBase64String(jpdf.Value), Cmpny, TipoDocElec,jFolio);

                              if (rest > 0)
                              {
                                  oDocumento.AttachmentEntry = rest;
                                  lRetCode = oDocumento.Update();
                                  if (lRetCode != 0)
                                  {
                                      SBO_f.SBOApp.StatusBar.SetText("No se ha logrado adjuntar documento " , BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                  }
                                  else
                                  {
                                      SBO_f.SBOApp.StatusBar.SetText("Documento Adjunto " , BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_None);
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
                    s = String.Format(s, oDocumento.DocEntry, sObjType, DocType);
                    ors.DoQuery(s);
                    if (ors.RecordCount == 0)
                        Reg.FELOGAdd(oDocumento.DocEntry, sObjType, DocType, "", oDocumento.FolioNumber, Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", "", "", DocDate);
                    else
                    {
                        if ((System.String)(ors.Fields.Item("U_Status").Value) != "RR")
                        {
                            SBO_f.SBOApp.StatusBar.SetText("Documento se ha enviado a EasyDot", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                            Reg.FELOGUptM((System.Int32)(ors.Fields.Item("DocEntry").Value), oDocumento.DocEntry, sObjType, DocType, "", oDocumento.FolioNumber, Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", "", "", DocDate);
                        }
                        else
                            SBO_f.SBOApp.StatusBar.SetText("Documento ya se ha enviado anteriormente a EasyDot", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    }

                    if (Status == "EC")
                    {
                        oDocumento.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                        lRetCode = oDocumento.Update();
                    }
                    else if (Status == "RR")
                    {
                        oDocumento.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                        lRetCode = oDocumento.Update();
                    }
                    else
                    {
                        oDocumento.UserFields.Fields.Item("U_EstadoFE").Value = "N";
                        lRetCode = oDocumento.Update();
                    }

                }

            }
            catch (Exception x)
            {
                SBO_f.SBOApp.StatusBar.SetText("EnviarFE_WebServiceInvoice: " + x.Message + " ** Trace: " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                SBO_f.oLog.OutLog("EnviarFE_WebServiceInvoice: " + x.Message + " ** Trace: " + x.StackTrace);
            }
            finally
            {
                SBO_f._ReleaseCOMObject(ors);
                SBO_f._ReleaseCOMObject(ors2);
                SBO_f._ReleaseCOMObject(oXml);
                SBO_f._ReleaseCOMObject(miXML);
            }
        }

        //para pruebas
        private String GenerarXMLStringInvoicex(ref SAPbobsCOM.Recordset ors, ref SAPbobsCOM.Recordset ors2, String TipoDocElec, ref XDocument miXML, String Sector)
        {
            Int32 i;
            XElement xNodo = null;

            try
            {
                if (Sector == "E")
                {
                    ors.MoveFirst();
                    var E1 = ((System.String)ors.Fields.Item("FchEmis").Value).Trim();
                    var E2 = ((System.String)ors.Fields.Item("FchVenc").Value).Trim();
                    var E3 = ((System.String)ors.Fields.Item("TipoDTE").Value).Trim();
                    var E4 = ((System.Int32)ors.Fields.Item("Folio").Value);
                    var E5 = ((System.String)ors.Fields.Item("IndServicio").Value).Trim();
                    var E6 = ((System.Double)ors.Fields.Item("MntBruto").Value);
                    var E7 = ((System.Double)ors.Fields.Item("MntCancel").Value);
                    var E8 = ((System.Double)ors.Fields.Item("SaldoInsol").Value);
                    var E9 = ((System.String)ors.Fields.Item("CdgVendedor").Value).Trim();
                    var E10 = ((System.String)ors.Fields.Item("RUTEmisor").Value).Trim();
                    var E11 = ((System.String)ors.Fields.Item("RznSocial").Value).Trim();
                    var E12 = ((System.String)ors.Fields.Item("GiroEmis").Value).Trim();
                    var E13 = ((System.String)ors.Fields.Item("Sucursal").Value).Trim();
                    var E14 = ((System.String)ors.Fields.Item("Telefono").Value).Trim();
                    var E15 = ((System.String)ors.Fields.Item("CiudadPostal").Value).Trim();
                    var E16 = ((System.String)ors.Fields.Item("CiudadRecep").Value).Trim();
                    var E17 = ((System.String)ors.Fields.Item("CmnaPostal").Value).Trim();
                    var E18 = ((System.String)ors.Fields.Item("CmnaRecep").Value).Trim();
                    var E19 = ((System.String)ors.Fields.Item("Contacto").Value).Trim();
                    var E20 = ((System.String)ors.Fields.Item("CorreoRecep").Value).Trim();
                    var E21 = ((System.String)ors.Fields.Item("DirPostal").Value).Trim();
                    var E22 = ((System.String)ors.Fields.Item("DirRecep").Value).Trim();
                    var E23 = ((System.String)ors.Fields.Item("GiroRecep").Value).Trim();
                    var E24 = ((System.String)ors.Fields.Item("RUTRecep").Value).Trim();
                    var E25 = ((System.String)ors.Fields.Item("RznSocRecep").Value).Trim();
                    var E26 = ((System.Int32)ors.Fields.Item("CredEC").Value);
                    var E27 = ((System.Double)ors.Fields.Item("IVA").Value);
                    var E28 = ((System.Double)ors.Fields.Item("IVANoRet").Value);
                    var E29 = ((System.Double)ors.Fields.Item("IVAProp").Value);
                    var E30 = ((System.Double)ors.Fields.Item("IVATerc").Value);
                    var E31 = ((System.Double)ors.Fields.Item("MntBase").Value);
                    var E32 = ((System.Double)ors.Fields.Item("MntExe").Value);
                    var E33 = ((System.Double)ors.Fields.Item("MntMargenCom").Value);
                    var E34 = ((System.Double)ors.Fields.Item("MntNeto").Value);
                    var E35 = ((System.Double)ors.Fields.Item("MntTotal").Value);
                    var E36 = ((System.Double)ors.Fields.Item("MontoNF").Value);
                    var E37 = ((System.Double)ors.Fields.Item("MontoPeriodo").Value);
                    var E38 = ((System.Double)ors.Fields.Item("SaldoAnterior").Value);
                    var E39 = ((System.Double)ors.Fields.Item("TasaIVA").Value);
                    var E40 = ((System.Double)ors.Fields.Item("VlrPagar").Value);

                    xNodo = new XElement("Encabezado",
                                            new XElement("IdDoc",
                                                     new XElement("FchEmis", ((System.String)ors.Fields.Item("FchEmis").Value).Trim()),
                                                     new XElement("FchVenc", ((System.String)ors.Fields.Item("FchVenc").Value).Trim()),
                                                     new XElement("TipoDTE", ((System.String)ors.Fields.Item("TipoDTE").Value).Trim()),
                                                     new XElement("Folio", ((System.Int32)ors.Fields.Item("Folio").Value)),
                                                     new XElement("IndServicio", ((System.String)ors.Fields.Item("IndServicio").Value).Trim()),
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
                                                     new XElement("VlrPagar", ((System.Double)ors.Fields.Item("VlrPagar").Value))
                                                    )
                                        );
                    miXML.Descendants("Documento").LastOrDefault().Add(xNodo);

                    //AGREGA impuestos Adicionales
                    if (((System.Double)ors.Fields.Item("MntImpAdic").Value) > 0)
                    {
                        ors2.MoveFirst();
                        while (!ors2.EoF)
                        {
                            var A1 = ((System.String)ors2.Fields.Item("CodImpAdic").Value).Trim();
                            var A2 = ((System.Double)ors2.Fields.Item("PorcImptoAdic").Value);
                            var A3 = ((System.Double)ors2.Fields.Item("MontoImptoAdic").Value);
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
                        var D1 = ((System.Double)ors.Fields.Item("MntDescuento").Value);
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
                        var R1 = ((System.Double)ors.Fields.Item("MntGlobal").Value);
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

                    //AGREGA Transporte
                    if ((TipoDocElec == "110") || (TipoDocElec == "111"))
                    {
                        var TM1 = ((System.String)ors.Fields.Item("TipoMoneda").Value).Trim();
                        xNodo = new XElement("TpoMoneda", ((System.String)ors.Fields.Item("TipoMoneda").Value).Trim());
                        miXML.Descendants("Totales").LastOrDefault().Add(xNodo);

                        xNodo = new XElement("OtraMoneda",
                                           new XElement("TpoMoneda", ((System.String)ors.Fields.Item("TpoMoneda").Value).Trim()),
                                           new XElement("TpoCambio", ((System.Double)ors.Fields.Item("TpoCambio").Value)),
                                           new XElement("MntExeOtrMnda", ((System.Double)ors.Fields.Item("MntExeOtrMnda").Value)),
                                           new XElement("MntTotOtrMnda", ((System.Double)ors.Fields.Item("MntTotOtrMnda").Value))
                                           );
                        miXML.Descendants("Encabezado").LastOrDefault().Add(xNodo);

                        xNodo = new XElement("Transporte",
                                            new XElement("Patente", ((System.String)ors.Fields.Item("Patente").Value).Trim()),
                                            new XElement("DirDest", ((System.String)ors.Fields.Item("DirRecep").Value).Trim()),
                                            new XElement("CmnaDest", ((System.String)ors.Fields.Item("CmnaRecep").Value).Trim()),
                                            new XElement("CiudadDest", ((System.String)ors.Fields.Item("CiudadRecep").Value).Trim()),
                                            new XElement("Aduana",
                                                        new XElement("CodModVenta", ((System.String)ors.Fields.Item("CodModVenta").Value).Trim()),
                                                        new XElement("CodClauVenta", ((System.String)ors.Fields.Item("CodClauVenta").Value).Trim()),
                                                        new XElement("MntFlete", ((System.Double)ors.Fields.Item("MntFlete").Value)),
                                                        new XElement("MntSeguro", ((System.Double)ors.Fields.Item("MntSeguro").Value))
                                                        )
                                            );
                        miXML.Descendants("Documento").LastOrDefault().Add(xNodo);


                        if (((System.String)ors.Fields.Item("FmaPagExp").Value).Trim() != "")
                        {
                            xNodo = new XElement("FmaPagExp", ((System.String)ors.Fields.Item("FmaPagExp").Value).Trim());
                            miXML.Descendants("IdDoc").LastOrDefault().Add(xNodo);
                        }

                        if (((System.String)ors.Fields.Item("CodViaTransp").Value).Trim() != "")
                        {
                            xNodo = new XElement("CodViaTransp", ((System.String)ors.Fields.Item("CodViaTransp").Value).Trim());
                            miXML.Descendants("Aduana").LastOrDefault().Add(xNodo);
                        }

                        if (((System.String)ors.Fields.Item("CodPtoEmbarque").Value).Trim() != "")
                        {
                            xNodo = new XElement("CodPtoEmbarque", ((System.String)ors.Fields.Item("CodPtoEmbarque").Value).Trim());
                            miXML.Descendants("Aduana").LastOrDefault().Add(xNodo);
                        }

                        if (((System.String)ors.Fields.Item("CodPtoDesemb").Value).Trim() != "")
                        {
                            xNodo = new XElement("CodPtoDesemb", ((System.String)ors.Fields.Item("CodPtoDesemb").Value).Trim());
                            miXML.Descendants("Aduana").LastOrDefault().Add(xNodo);
                        }

                        if (((System.String)ors.Fields.Item("CodUnidMedTara").Value).Trim() != "")
                        {
                            xNodo = new XElement("CodUnidMedTara", ((System.String)ors.Fields.Item("CodUnidMedTara").Value).Trim());
                            miXML.Descendants("Aduana").LastOrDefault().Add(xNodo);
                        }

                        if (((System.String)ors.Fields.Item("CodUnidPesoBruto").Value).Trim() != "")
                        {
                            xNodo = new XElement("CodUnidPesoBruto", ((System.String)ors.Fields.Item("CodUnidPesoBruto").Value).Trim());
                            miXML.Descendants("Aduana").LastOrDefault().Add(xNodo);
                        }

                        if (((System.String)ors.Fields.Item("CodUnidPesoNeto").Value).Trim() != "")
                        {
                            xNodo = new XElement("CodUnidPesoNeto", ((System.String)ors.Fields.Item("CodUnidPesoNeto").Value).Trim());
                            miXML.Descendants("Aduana").LastOrDefault().Add(xNodo);
                        }

                        if (((System.Int32)ors.Fields.Item("TotBultos").Value) != 0)
                        {
                            xNodo = new XElement("TotBultos", ((System.Int32)ors.Fields.Item("TotBultos").Value));
                            miXML.Descendants("Aduana").LastOrDefault().Add(xNodo);

                            /*xNodo = new XElement("TipoBultos", 
                                                new XElement("", ((System.String)ors.Fields.Item("").Value).Trim()),
                                                new XElement("", ((System.String)ors.Fields.Item("").Value).Trim()),
                                                new XElement("", ((System.String)ors.Fields.Item("").Value).Trim())
                                                );
                            miXML.Descendants("Aduana").LastOrDefault().Add(xNodo);*/
                        }

                        if (((System.String)ors.Fields.Item("CodPaisRecep").Value).Trim() != "")
                        {
                            xNodo = new XElement("CodPaisRecep", ((System.String)ors.Fields.Item("CodPaisRecep").Value).Trim());
                            miXML.Descendants("Aduana").LastOrDefault().Add(xNodo);
                        }

                        if (((System.String)ors.Fields.Item("CodPaisDestin").Value).Trim() != "")
                        {
                            xNodo = new XElement("CodPaisDestin", ((System.String)ors.Fields.Item("CodPaisDestin").Value).Trim());
                            miXML.Descendants("Aduana").LastOrDefault().Add(xNodo);
                        }

                        //AGREGA Exportacion Flete
                        if (((System.Double)ors.Fields.Item("MntFlete").Value) != 0)
                        {
                            xNodo = new XElement("DscRcgGlobal",
                                                        new XElement("NroLinDR", NroLinDR),
                                                        new XElement("TpoMov", "R"),
                                                        new XElement("GlosaDR", "Recargo Flete"),
                                                        new XElement("TpoValor", "$"),
                                                        new XElement("ValorDR", ((System.Double)ors.Fields.Item("MntFlete").Value))
                                                );
                            miXML.Descendants("Documento").LastOrDefault().Add(xNodo);
                            NroLinDR++;
                        }

                        //AGREGA Exportacion Seguro
                        if (((System.Double)ors.Fields.Item("MntSeguro").Value) != 0)
                        {
                            xNodo = new XElement("DscRcgGlobal",
                                                        new XElement("NroLinDR", NroLinDR),
                                                        new XElement("TpoMov", "R"),
                                                        new XElement("GlosaDR", "Recargo Seguro"),
                                                        new XElement("TpoValor", "$"),
                                                        new XElement("ValorDR", ((System.Double)ors.Fields.Item("MntSeguro").Value))
                                                );
                            miXML.Descendants("Documento").LastOrDefault().Add(xNodo);
                            NroLinDR++;
                        }

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

                        var NroLinDet = ((System.Int32)ors.Fields.Item("NroLinDet").Value);
                        var DescuentoMonto = ((System.Double)ors.Fields.Item("DescuentoMonto").Value);
                        var DescuentoPct = ((System.Double)ors.Fields.Item("DescuentoPct").Value);
                        var IndExe = ((System.Int32)ors.Fields.Item("IndExe").Value);
                        var MontoItem = ((System.Double)ors.Fields.Item("MontoItem").Value);
                        var VlrCodigo = ((System.String)ors.Fields.Item("VlrCodigo").Value).Trim();
                        var NmbItem = ((System.String)ors.Fields.Item("NmbItem").Value).Trim();
                        var DscItem = ((System.String)ors.Fields.Item("DscItem").Value).Trim();
                        var PrcItem = ((System.Double)ors.Fields.Item("PrcItem").Value);
                        var PrcRef = ((System.Double)ors.Fields.Item("PrcRef").Value);
                        var QtyItem = ((System.Double)ors.Fields.Item("QtyItem").Value);
                        var QtyRef = ((System.Double)ors.Fields.Item("QtyRef").Value);
                        var RecargoPct = ((System.Double)ors.Fields.Item("RecargoPct").Value);
                        var UnmdItem = ((System.String)ors.Fields.Item("UnmdItem").Value).Trim();
                        var CodImpAdic = ((System.String)ors.Fields.Item("CodImpAdic").Value).Trim();
                        var RecargoMonto = ((System.Double)ors.Fields.Item("RecargoMonto").Value);


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
                            //new XElement("RecargoMonto", ((System.Double)ors.Fields.Item("RecargoMonto").Value)),
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
        //para pruebas
        private String GenerarXMLStringLiquidacionFacturax(ref SAPbobsCOM.Recordset ors, String TipoDocElec, ref XDocument miXML, String Sector)
        {
            Int32 i;
            XElement xNodo = null;

            try
            {
                if (Sector == "E")
                {
                    ors.MoveFirst();
                    var FchEmis = ((System.String)ors.Fields.Item("FchEmis").Value).Trim();
                    var FchVenc = ((System.String)ors.Fields.Item("FchVenc").Value).Trim();
                    var TipoDTE = ((System.String)ors.Fields.Item("TipoDTE").Value).Trim();
                    var Folio = ((System.Int32)ors.Fields.Item("Folio").Value);
                    var CdgVendedor = ((System.String)ors.Fields.Item("CdgVendedor").Value).Trim();
                    var RUTEmisor = ((System.String)ors.Fields.Item("RUTEmisor").Value).Trim();
                    var RznSoc = ((System.String)ors.Fields.Item("RznSocial").Value).Trim();
                    var GiroEmis = ((System.String)ors.Fields.Item("GiroEmis").Value).Trim();
                    var Sucursal = ((System.String)ors.Fields.Item("Sucursal").Value).Trim();
                    var Telefono = ((System.String)ors.Fields.Item("Telefono").Value).Trim();
                    var CiudadPostal = ((System.String)ors.Fields.Item("CiudadPostal").Value).Trim();
                    var CiudadRecep = ((System.String)ors.Fields.Item("CiudadRecep").Value).Trim();
                    var CmnaPostal = ((System.String)ors.Fields.Item("CmnaPostal").Value).Trim();
                    var CmnaRecep = ((System.String)ors.Fields.Item("CmnaRecep").Value).Trim();
                    var Contacto = ((System.String)ors.Fields.Item("Contacto").Value).Trim();
                    var CorreoRecep = ((System.String)ors.Fields.Item("CorreoRecep").Value).Trim();
                    var DirPostal = ((System.String)ors.Fields.Item("DirPostal").Value).Trim();
                    var DirRecep = ((System.String)ors.Fields.Item("DirRecep").Value).Trim();
                    var GiroRecep = ((System.String)ors.Fields.Item("GiroRecep").Value).Trim();
                    var RUTRecep = ((System.String)ors.Fields.Item("RUTRecep").Value).Trim();
                    var RznSocRecep = ((System.String)ors.Fields.Item("RznSocRecep").Value).Trim();
                    var IVA = ((System.Double)ors.Fields.Item("IVA").Value);
                    var MntExe = ((System.Double)ors.Fields.Item("MntExe").Value);
                    var MntNeto = ((System.Double)ors.Fields.Item("MntNeto").Value);
                    var MntTotal = ((System.Double)ors.Fields.Item("MntTotal").Value);
                    var TasaIVA = ((System.Double)ors.Fields.Item("TasaIVA").Value);

                    xNodo = new XElement("Encabezado",
                                            new XElement("IdDoc",
                                                     new XElement("FchEmis", ((System.String)ors.Fields.Item("FchEmis").Value).Trim()),
                                                     new XElement("FchVenc", ((System.String)ors.Fields.Item("FchVenc").Value).Trim()),
                                                     new XElement("TipoDTE", ((System.String)ors.Fields.Item("TipoDTE").Value).Trim()),
                                                     new XElement("Folio", ((System.Int32)ors.Fields.Item("Folio").Value))
                                                     ),
                                            new XElement("Emisor",
                        //new XElement("CdgSIISucur", ((System.String)ors.Fields.Item("").Value).Trim()),
                                                     new XElement("CdgVendedor", ((System.String)ors.Fields.Item("CdgVendedor").Value).Trim()),
                                                     new XElement("RUTEmisor", ((System.String)ors.Fields.Item("RUTEmisor").Value).Trim()),
                                                     new XElement("RznSoc", ((System.String)ors.Fields.Item("RznSocial").Value).Trim()),
                                                     new XElement("GiroEmis", ((System.String)ors.Fields.Item("GiroEmis").Value).Trim()),
                                                     new XElement("Sucursal", ((System.String)ors.Fields.Item("Sucursal").Value).Trim()),
                                                     new XElement("Telefono", ((System.String)ors.Fields.Item("Telefono").Value).Trim())
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
                                                     new XElement("IVA", ((System.Double)ors.Fields.Item("IVA").Value)),
                                                     new XElement("MntExe", ((System.Double)ors.Fields.Item("MntExe").Value)),
                                                     new XElement("MntNeto", ((System.Double)ors.Fields.Item("MntNeto").Value)),
                                                     new XElement("MntTotal", ((System.Double)ors.Fields.Item("MntTotal").Value)),
                                                     new XElement("TasaIVA", ((System.Double)ors.Fields.Item("TasaIVA").Value)),
                                                     new XElement("Comisiones",
                                                         new XElement("ValComNeto", ((System.Double)ors.Fields.Item("ValComNeto").Value)),
                                                         new XElement("ValComExe", ((System.Double)ors.Fields.Item("ValComExe").Value)),
                                                         new XElement("ValComIVA", ((System.Double)ors.Fields.Item("ValComIVA").Value))
                                                         )
                                                    )
                                        );
                    miXML.Descendants("Liquidacion").LastOrDefault().Add(xNodo);

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
                        miXML.Descendants("Liquidacion").LastOrDefault().Add(xNodo);
                        NroLinDR++;
                    }

                    //AGREGA COMP 
                    xNodo = new XElement("DocumentoInterno",
                                                    new XElement("COMP", ((System.Int32)ors.Fields.Item("COMP").Value)));
                    miXML.Descendants("Liquidacion").LastOrDefault().Add(xNodo);

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
                                            new XElement("QtyItem", ((System.Double)ors.Fields.Item("QtyItem").Value)),
                                            new XElement("RecargoMonto", ((System.Double)ors.Fields.Item("RecargoMonto").Value)),
                                            new XElement("RecargoPct", ((System.Double)ors.Fields.Item("RecargoPct").Value)),
                                            new XElement("TpoDocLiq", ((System.String)ors.Fields.Item("TpoDocLiq").Value).Trim())
                                            );
                        //if (result == null)
                        //    miXML.Root.Add(xNodo);
                        //else
                        miXML.Descendants("Liquidacion").LastOrDefault().Add(xNodo);
                        ors.MoveNext();
                    }
                }//fin Detalle
                else if (Sector == "R")//Referencia
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
                            miXML.Descendants("Liquidacion").LastOrDefault().Add(xNodo);
                        ors.MoveNext();
                    }
                }//fin Referencia
                else if (Sector == "C")//Comisiones
                {
                    ors.MoveFirst();
                    while (!ors.EoF)
                    {
                        var result = (from nodo in miXML.Descendants("Comisiones")
                                      //where nodo.Attribute("id").Value == "1234"
                                      select nodo).FirstOrDefault();
                        xNodo = new XElement("Comisiones",
                                            new XElement("NroLinCom", ((System.Int32)ors.Fields.Item("NroLinCom").Value)),
                                            new XElement("TipoMovim", ((System.String)ors.Fields.Item("TipoMovim").Value).Trim()),
                                            new XElement("Glosa", ((System.String)ors.Fields.Item("Glosa").Value).Trim()),
                                            new XElement("ValComNeto", ((System.Double)ors.Fields.Item("ValComNeto").Value)),
                                            new XElement("ValComExe", ((System.Double)ors.Fields.Item("ValComExe").Value)),
                                            new XElement("ValComIVA", ((System.Double)ors.Fields.Item("ValComIVA").Value))
                                            );
                        if (result == null)
                            miXML.Root.Add(xNodo);
                        else
                            miXML.Descendants("Liquidacion").LastOrDefault().Add(xNodo);
                        ors.MoveNext();
                    }
                }//fin Comisiones


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
            String TipoLinea = "";
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
                if (ObjType == "203")
                {
                    oDBDSDir = oForm.DataSources.DBDataSources.Item("DPI12");
                    oDBDSH = oForm.DataSources.DBDataSources.Item("ODPI");
                }
                else
                {
                    oDBDSDir = oForm.DataSources.DBDataSources.Item("INV12");
                    oDBDSH = oForm.DataSources.DBDataSources.Item("OINV");
                }

                if (GlobalSettings.RunningUnderSQLServer)
                {
                    s = @"select SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'Inst', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'TipoDocElect'
                                             FROM NNM1 T2 WITH (NOLOCK)
                                               WHERE Series = {0}
                                                 --AND ObjectCode = '{1}'";
                }
                else
                {
                    s = @"select SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""TipoDocElect""
                                             FROM ""NNM1"" T2 
                                            WHERE ""Series"" = {0}
                                              --AND ""ObjectCode"" = '{1}' ";
                }

                s = String.Format(s, (System.String)(oDBDSH.GetValue("Series", 0)), ObjType);
                oRecordSet.DoQuery(s);

                if ((System.String)(oRecordSet.Fields.Item("Tipo").Value) == "E")
                    DocElec = true;
                else
                    DocElec = false;

                if (DocElec)
                {

                    var sDocSubType = (System.String)(oDBDSH.GetValue("DocSubType", 0)).Trim();
                    var DocS = ((System.String)oRecordSet.Fields.Item("TipoDocElect").Value).Trim();

                    if ((sDocSubType == "--") && ((((System.String)oDBDSH.GetValue("isIns", 0)).Trim() == "Y") && (DocS != "110"))) //Factura
                        TipoDocElec = "33";
                    else if ((sDocSubType == "--") && (((System.String)oDBDSH.GetValue("isIns", 0)).Trim() == "Y") && (DocS == "110")) //Factura Exportacion
                        TipoDocElec = "110";
                    else if (DocS == "43")
                        TipoDocElec = "43";
                    else if (sDocSubType == "--") //Factura
                        TipoDocElec = "33";
                    else if (sDocSubType == "IE") //Factura Exenta
                        TipoDocElec = "34";
                    else if ((sDocSubType == "DN") && ((System.String)(oRecordSet.Fields.Item("TipoDocElect").Value) != "111")) //Nota Debito
                        TipoDocElec = "56";
                    else if ((sDocSubType == "DN") && ((System.String)(oRecordSet.Fields.Item("TipoDocElect").Value) == "111")) //Nota Debito Exportacion
                        TipoDocElec = "111";
                    else if (sDocSubType == "IB") //Boleta
                        TipoDocElec = "39";
                    else if (sDocSubType == "EB") //Boleta Exenta
                        TipoDocElec = "41";
                    else if (sDocSubType == "IX") //Factura Exportacion
                        TipoDocElec = "110";


                    if (((System.String)(oDBDSDir.GetValue("CityB", 0)).Trim() == "") && (TipoDocElec != "39") && (TipoDocElec != "41"))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    if (((System.String)(oDBDSDir.GetValue("CityS", 0)).Trim() == "") && (_result) && (TipoDocElec != "39") && (TipoDocElec != "41"))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    if (((System.String)(oDBDSDir.GetValue("CountyB", 0)).Trim() == "") && (_result) && (TipoDocElec != "39") && (TipoDocElec != "41"))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    if (((System.String)(oDBDSDir.GetValue("CountyS", 0)).Trim() == "") && (_result) && (TipoDocElec != "39") && (TipoDocElec != "41"))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    if (((System.String)(oDBDSDir.GetValue("StreetB", 0)).Trim() == "") && (_result) && (TipoDocElec != "39") && (TipoDocElec != "41"))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    if (((System.String)(oDBDSDir.GetValue("StreetS", 0)).Trim() == "") && (_result) && (TipoDocElec != "39") && (TipoDocElec != "41"))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    s = (System.String)(oDBDSH.GetValue("CardName", 0)).Trim();
                    if ((s == "") && (_result))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar Nombre Cliente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    if ((TipoDocElec == "110") || (TipoDocElec == "111"))
                    {
                        var CardCode = (System.String)(oDBDSH.GetValue("CardCode", 0)).Trim();
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"SELECT COUNT(*) 'Conta' FROM OCRD WHERE CardCode = '{0}' AND U_FE_Export = 'Y'";

                        else
                            s = @"SELECT COUNT(*) ""Conta"" FROM ""OCRD"" WHERE ""CardCode"" = '{0}' AND ""U_FE_Export"" = 'Y' ";
                        s = String.Format(s, CardCode);
                        oRecordSet.DoQuery(s);
                        if (((System.Int32)oRecordSet.Fields.Item("Conta").Value) == 0)
                        {
                            FSBOApp.StatusBar.SetText("Cliente no puede generar Factura Electronica de Exportacion, revisar Maestro Socio Negocio campo Cliente Exportacion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
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
                    if (((TipoDocElec == "110") || (TipoDocElec == "111")) && (_result))
                    {
                        s = (System.String)(oDBDSH.GetValue("U_CodModVenta", 0)).Trim();
                        if (s == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe seleccionar Codigo Modo Venta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }

                    //valida rut
                    if ((_result) && ((TipoDocElec != "110") && (TipoDocElec != "111")))
                    {
                        Param = new TFunctions();
                        Param.SBO_f = FSBOf;
                        s = Param.ValidarRut((System.String)(oDBDSH.GetValue("LicTradNum", 0)));
                        if (s != "OK")
                        {
                            FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }


                    //incorporar validar campo U_IndServicioBol para boleta
                    if ((_result) && ((TipoDocElec == "39") || (TipoDocElec == "41")))
                    {
                        if ((System.String)(oDBDSH.GetValue("U_IndServicioBol", 0)).Trim() == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe seleccionar Indicador de servicio para Boleta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }

                    //Valida que tenga ingreado el rut del cliente
                    if ((_result) && (bMultiSoc == false))
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


                    //valida descuentos negativos en el detalle del documento, caracteres especiales y descripcion de articulo
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
                        {
                            CantLineas = (System.Int32)(oRecordSet.Fields.Item("CantLineas").Value);

                        }
                        else
                        {
                            FSBOApp.StatusBar.SetText("Debe parametrizar el maximo de lineas para documento " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }

                        oRecordSet.DoQuery(s);//consulta para descuento
                        if (oRecordSet.RecordCount > 0)
                        {
                            oComboBox = (ComboBox)(oForm.Items.Item("3").Specific);
                            TipoDoc = oComboBox.Selected.Value.Trim();
                            if (TipoDoc == "S")
                                mtx = (Matrix)(oForm.Items.Item("39").Specific);
                            else
                                mtx = (Matrix)(oForm.Items.Item("38").Specific);

                            var ValDescL = (System.String)(oRecordSet.Fields.Item("ValDescL").Value);
                            i = 1;
                            var cantlin = 0;
                            while (i < mtx.RowCount)
                            {
                                if (TipoDoc == "S") //System.String(oDBDSH.GetValue("DocType",0)).Trim()
                                {
                                    TipoLinea = "";
                                }
                                else
                                {
                                    oComboBox = (ComboBox)(mtx.Columns.Item("257").Cells.Item(i).Specific);
                                    TipoLinea = (System.String)(oComboBox.Selected.Value);
                                }

                                if (ValDescL == "Y")
                                {
                                    if (TipoDoc == "S") //System.String(oDBDSH.GetValue("DocType",0)).Trim()
                                        oEditText = (EditText)(mtx.Columns.Item("6").Cells.Item(i).Specific);
                                    else
                                        oEditText = (EditText)(mtx.Columns.Item("15").Cells.Item(i).Specific);

                                    if ((Convert.ToDouble(((SAPbouiCOM.EditText)(oEditText)).String.Replace(",", "."), _nf) < 0) && (TipoLinea == ""))
                                    {
                                        s = "Descuento negativo en la linea " + Convert.ToString(i);
                                        FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        _result = false;
                                        i = mtx.RowCount;
                                    }
                                }

                                if ((TipoDocElec == "43") && (sDocSubType == "DN"))//para liquidacion de factura
                                {
                                    if (((ComboBox)mtx.Columns.Item("U_TipoDTELF").Cells.Item(i).Specific).Selected == null)
                                    {
                                        s = "Debe seleccionar tipo documento referencia, linea " + Convert.ToString(i);
                                        FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        _result = false;
                                        i = mtx.RowCount;
                                    }
                                    else
                                    {
                                        var com = ((ComboBox)mtx.Columns.Item("U_TipoDTELF").Cells.Item(i).Specific).Selected.Value;
                                        com = com.Trim();
                                        if ((com == "") || (com == "00"))
                                        {
                                            s = "Debe seleccionar tipo documento referencia, linea " + Convert.ToString(i);
                                            FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                            _result = false;
                                            i = mtx.RowCount;
                                        }
                                        else if (com != "99")
                                        {
                                            oEditText = ((EditText)mtx.Columns.Item("U_FolioLiqF").Cells.Item(i).Specific);
                                            if ((((SAPbouiCOM.EditText)oEditText).Value.ToString() == "") || (((SAPbouiCOM.EditText)oEditText).Value.ToString() == "0"))
                                            {
                                                s = "Debe ingresar numero folio de referencia, linea " + Convert.ToString(i);
                                                FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                _result = false;
                                                i = mtx.RowCount;
                                            }
                                        }
                                    }
                                }

                                if (_result)
                                {
                                    if (TipoDoc == "S")
                                        oEditText = (EditText)(mtx.Columns.Item("1").Cells.Item(i).Specific);
                                    else
                                        oEditText = (EditText)(mtx.Columns.Item("3").Cells.Item(i).Specific);

                                    s = oEditText.Value;
                                    if ((s == "") && (TipoLinea == ""))
                                    {
                                        FSBOApp.StatusBar.SetText("Debe ingresar descripción en la linea " + Convert.ToString(i), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        _result = false;
                                        i = mtx.RowCount;
                                    }

                                    //contar total de lineas
                                    if (TipoDoc == "S")
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


                                    //se comenta segun reunion de viernes 20150320, se creo una funcion que limpia lo caracteres invalidos al momento de enviar al portal
                                    //if (_result)
                                    //{
                                    //    foreach (String cara in CaracteresInvalidos)
                                    //    {
                                    //        if (s.IndexOf(cara) > 0)
                                    //        {
                                    //            FSBOApp.StatusBar.SetText(@"Descripción tiene caracteres prohibidos (" + cara + "), linea " + i.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    //            _result = false;
                                    //            break;
                                    //        }
                                    //    }
                                    //}
                                }

                                i++;
                            }
                            if ((cantlin > CantLineas) && (((System.String)oDBDSH.GetValue("SummryType", 0)).Trim() == "N")) //valida total de lineas solo cuando no es resumen
                            {
                                FSBOApp.StatusBar.SetText("Cantidad de lineas supera lo permitido, parametrización FE", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                return false;
                            }
                            //oRecordSet.MoveNext(); //no se necesita ya que siempre trae un puro registro
                        }
                    }


                    //Para Facturas Exentas, validar que usen impuesto Exento
                    if ((TipoDocElec == "34") || (TipoDocElec == "110") || (TipoDocElec == "111"))
                    {
                        if (Convert.ToDouble((System.String)(oDBDSH.GetValue("VatSum", 0)).Trim().Replace(",", "."), _nf) != 0)
                        {
                            FSBOApp.StatusBar.SetText("Existe lineas con impuesto, el documento que esta generando es exento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }
                    }

                    //valida para nota debito
                    if ((_result) && ((System.String)(oDBDSH.GetValue("DocSubType", 0)).Trim() == "DN") && (TipoDocElec == "56"))
                    {
                        if (((System.String)oForm.DataSources.UserDataSources.Item("CodRef").Value).Trim() == "2")
                        {
                            FSBOApp.StatusBar.SetText("Corrige texto documento no es permitido para Nota de Debito", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if (((System.String)oForm.DataSources.UserDataSources.Item("CodRef").Value).Trim() == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe seleccionar Código Referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else
                        {
                            if ((((System.String)oForm.DataSources.UserDataSources.Item("IndGlobal").Value).Trim() == "1") && (((System.String)oForm.DataSources.UserDataSources.Item("IndRef").Value).Trim() == "00"))
                            {
                                FSBOApp.StatusBar.SetText("Debe seleccinar tipo documento referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                _result = false;
                            }
                            else if (((System.String)oForm.DataSources.UserDataSources.Item("IndGlobal").Value).Trim() == "1")
                                FSBOApp.StatusBar.SetText("Seleccionado Docto Sin Referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            else
                            {
                                ogrid = ((Grid)oForm.Items.Item("gridRefFE").Specific);
                                var CantDoc = 0;

                                for (Int32 iLin = 0; iLin < ogrid.DataTable.Rows.Count; iLin++)
                                {
                                    if ((((System.String)ogrid.DataTable.GetValue("DocFolio", iLin)) != "0") && (((System.String)ogrid.DataTable.GetValue("TipoDTE", iLin)).Trim() != "00") && (((System.String)ogrid.DataTable.GetValue("DocFolio", iLin)) != ""))
                                    {
                                        s = ((System.String)ogrid.DataTable.GetValue("TipoDTE", iLin)).Trim();
                                        if (s.IndexOf("b") == -1)
                                        {
                                            if ((((System.String)ogrid.DataTable.GetValue("TipoDTE", iLin)).Trim() == "61") || (((System.String)ogrid.DataTable.GetValue("TipoDTE", iLin)).Trim() == "60") || (((System.String)ogrid.DataTable.GetValue("TipoDTE", iLin)).Trim() == "112"))
                                                Tabla = "ORIN";
                                            else
                                                Tabla = "OINV";

                                            if (GlobalSettings.RunningUnderSQLServer)
                                            {
                                                s = @"SELECT COUNT(*) 'Cont'
                                                    FROM {0} T1 WITH (NOLOCK)
                                                    JOIN NNM1 T2 WITH (NOLOCK) ON T1.Series = T2.Series
                                                   WHERE ISNULL(T1.FolioNum, -1) = {1}
                                                     AND CASE 
                                                           WHEN '{2}' = '33'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                            	                           WHEN '{2}' = '39'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'IB'
                            	                           WHEN '{2}' = '41'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'BE'
                            	                           WHEN '{2}' = '110' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'IX'
                            	                           WHEN '{2}' = '34'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'IE'
                            	                           WHEN '{2}' = '56'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'DN'
                                                           WHEN '{2}' = '61'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                                                           WHEN '{2}' = '111' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'DN'
                                                           WHEN '{2}' = '112' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                                                           WHEN '{2}' = '30'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                                                           WHEN '{2}' = '32'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'IE'
                                                           WHEN '{2}' = '55'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'DN'
                                                           WHEN '{2}' = '60'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                                                           WHEN '{2}' = '101' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'IX'
                                                           WHEN '{2}' = '104' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'DN'
                                                           WHEN '{2}' = '106' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                            	                           Else '-1'
                                                         END = T1.DocSubType";
                                            }
                                            else
                                            {
                                                s = @"SELECT COUNT(*) ""Cont""
                                                    FROM ""{0}"" T1
                                                    JOIN ""NNM1"" T2 ON T1.""Series"" = T2.""Series""
                                                   WHERE IFNULL(T1.""FolioNum"", -1) = {1}
                                                     AND CASE 
                                                           WHEN '{2}' = '33'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--' 
                            	                           WHEN '{2}' = '39'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'IB'
                            	                           WHEN '{2}' = '41'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'BE' 
                            	                           WHEN '{2}' = '110' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'IX'
                            	                           WHEN '{2}' = '34'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'IE'
                            	                           WHEN '{2}' = '56'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'DN'
                                                           WHEN '{2}' = '61'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                                                           WHEN '{2}' = '111' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'DN'
                                                           WHEN '{2}' = '112' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                                                           WHEN '{2}' = '30'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                                                           WHEN '{2}' = '32'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'IE'
                                                           WHEN '{2}' = '55'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'DN'
                                                           WHEN '{2}' = '60'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                                                           WHEN '{2}' = '101' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'IX'
                                                           WHEN '{2}' = '104' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'DN'
                                                           WHEN '{2}' = '106' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                            	                           Else '-1' 
                                                         END = T1.""DocSubType"" ";
                                            }
                                            s = String.Format(s, Tabla, ((System.String)ogrid.DataTable.GetValue("DocFolio", iLin)).Trim(), ((System.String)ogrid.DataTable.GetValue("TipoDTE", iLin)).Trim());
                                            oRecordSet.DoQuery(s);

                                            if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                                            {
                                                _result = true;
                                                CantDoc++;
                                            }
                                            else
                                            {
                                                FSBOApp.StatusBar.SetText("No se ha encontrado documento de referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                _result = false;
                                                break;
                                            }
                                        }//fin if dif b
                                        else
                                        {
                                            s = ogrid.DataTable.GetValue("DocFolio", iLin).ToString();
                                            var sc = "";
                                            if (ogrid.DataTable.GetValue("DocDate", iLin) != null)
                                                sc = ((System.DateTime)ogrid.DataTable.GetValue("DocDate", iLin)).ToString("yyyyMMdd");
                                            if ((sc == "") && ((s != "0") && (s != "")))
                                            {
                                                FSBOApp.StatusBar.SetText("Debe ingresar Fecha, linea " + (iLin + 1).ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                _result = false;
                                                break;
                                            }
                                            else if ((sc != "") && ((s == "0") || (s == "")))
                                            {
                                                FSBOApp.StatusBar.SetText("Debe ingresar Folio, linea " + (iLin + 1).ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                _result = false;
                                                break;
                                            }

                                            if ((sc != "") && (s != "") && (s != "0"))
                                                CantDoc++;
                                        }
                                    }
                                }//fin for

                                if ((CantDoc == 0) && (_result))
                                {
                                    FSBOApp.StatusBar.SetText("Debe ingresar documento de referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    _result = false;
                                }
                                else if ((CantDoc > 1) && (((System.String)oForm.DataSources.UserDataSources.Item("CodRef").Value).Trim() != "3"))
                                {
                                    FSBOApp.StatusBar.SetText("Nota de credito solo debe tener una Factura de referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    _result = false;
                                }
                            }
                        }
                    }

                    //valida para folio Distribuido
                    if ((TipoDocElec == "43") && (((System.String)oDBDSH.GetValue("DocSubType", 0)).Trim() != "DN"))
                    {
                        s = ((System.String)((EditText)oForm.Items.Item("VID_FEDocB").Specific).Value).Trim();
                        if (s == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar Factura compra que representa comision de liquidacion factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }
                    else
                    {
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
                                {
                                    s = @"select T0.DocEntry, T1.LineId, T1.U_Folio
                                        from [@VID_FEDIST] T0 WITH (NOLOCK)
	                                    join [@VID_FEDISTD] T1 WITH (NOLOCK) on T1.DocEntry = T0.DocEntry
                                       where T0.U_TipoDoc = '{0}'
                                         and T0.U_Sucursal = 'Principal'
	                                     and T1.U_Estado = 'D'
	                                     and T1.U_Folio > 0
                                       order by T1.U_Folio ASC";
                                }
                                else
                                {
                                    s = @"select T0.""DocEntry"", T1.""LineId"", T1.""U_Folio""
                                        from ""@VID_FEDIST"" T0
	                                    join ""@VID_FEDISTD"" T1 on T1.""DocEntry"" = T0.""DocEntry""
                                       where T0.""U_TipoDoc"" = '{0}'
                                         and T0.""U_Sucursal"" = 'Principal'
	                                     and T1.""U_Estado"" = 'D'
	                                     and T1.""U_Folio"" > 0
                                       order by T1.""U_Folio"" ASC ";
                                }
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
                }

                return _result;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("ValidarDatosFE " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
        }


    }//fin Class
}
