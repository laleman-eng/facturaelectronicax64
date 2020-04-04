using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO;
using System.CodeDom.Compiler;
using System.Xml;
using System.Text;
using Factura_Electronica_VK.Functions;
using VisualD.untLog;
using System.Data;
using FactRemota;
using pe.facturamovil;
using ServiceStack.Text;
using System.Net.Http;
using System.Configuration;
using VisualD.SBOGeneralService;

namespace Factura_Electronica_VK.CreditNotes
{
    public class TCreditNotes : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private String s;
        private Boolean Flag;
        private SAPbouiCOM.EditText oEditText;
        private SAPbouiCOM.ComboBox oComboBox;
        private SAPbouiCOM.StaticText oStatic;
        //private SAPbouiCOM.DBDataSource oDBVID_FEREF;
        private SAPbouiCOM.DBDataSource oDBVID_FEREFD;
        private List<string> Lista;
        //por Peru
        private String Localidad;
        private String FacturadorPE;
        private String RUC;
        //private pe.facturamovil.User oUser_FM;
        private Int32 LoginCount_FM;
        private String CCEmail_FM;
        private String Email_FM;
        private pe.facturamovil.Note oNote_FM;
        private String JsonText;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String SeparadorM = "";
        private SqlConnection ConexionADO;
        private String Val90;

        private SAPbouiCOM.Matrix mtx;
        public VisualD.SBOFunctions.CSBOFunctions SBO_f;
        public static String DocSubType
        { get; set; }
        public static Boolean bFolderAdd
        { get; set; }
        public static String ObjType
        { get; set; }


        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            SAPbouiCOM.Folder oFolder;
            SAPbouiCOM.Item oItem;
            SAPbouiCOM.Item oItemB;
            SAPbouiCOM.StaticText oStaticText;
            SAPbouiCOM.Matrix oMatrix;
            SAPbouiCOM.Columns oColumns;
            SAPbouiCOM.Column oColumn;
            SAPbouiCOM.DataTable oDataTable;
            String sSeries;
            //result  := inherited InitForm(uid, xmlPath,var application,var company,var sboFunctions,var _GlobalSettings );
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                //FSBOf.LoadForm(xmlPath, 'VID_Entrega.srf', Uid);
                oForm = FSBOApp.Forms.Item(uid);
                Flag = false;
                oForm.Freeze(true);

                if (GlobalSettings.RunningUnderSQLServer)
                    s = "select ISNULL(U_Localidad,'CL') Localidad, ISNULL(U_FacturaP,'E') FacturadorP, ISNULL(U_Val90, 'N') 'Val90' from [@VID_FEPARAM] where Code = '1'";
                else
                    s = @"select IFNULL(""U_Localidad"",'CL') ""Localidad"", IFNULL(""U_FacturaP"",'E') ""FacturadorP"", IFNULL(""U_Val90"", 'N') ""Val90"" from ""@VID_FEPARAM"" where ""Code"" = '1' ";

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                    throw new Exception("Debe parametrizar el Addon Factura Electronica");
                else
                {
                    Localidad = ((System.String)oRecordSet.Fields.Item("Localidad").Value).Trim();
                    FacturadorPE = ((System.String)oRecordSet.Fields.Item("FacturadorP").Value).Trim();
                    Val90 = ((System.String)oRecordSet.Fields.Item("Val90").Value).Trim();
                }

                if (Localidad == "CL")
                {

                    if (bFolderAdd)
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select ThousSep from OADM";
                        else
                            s = @"select ""ThousSep"" from ""OADM"" ";
                        oRecordSet.DoQuery(s);
                        SeparadorM = ((System.String)oRecordSet.Fields.Item("ThousSep").Value).Trim();

                        //oDBVID_FEREF = oForm.DataSources.DBDataSources.Add("@VID_FEREF");
                        oDBVID_FEREFD = oForm.DataSources.DBDataSources.Add("@VID_FEREFD");

                        oForm.DataSources.UserDataSources.Add("VID_FEDCTO", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                        oItem = (SAPbouiCOM.Item)(oForm.Items.Add("VID_FEDCTO", SAPbouiCOM.BoFormItemTypes.it_FOLDER));
                        oItemB = oForm.Items.Item("1320002137");

                        oItem.Left = oItemB.Left + 30;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oFolder = (Folder)(oItem.Specific);
                        oFolder.Caption = "Factura Electrónica";
                        oFolder.Pane = 333;
                        oFolder.DataBind.SetBound(true, "", "VID_FEDCTO");
                        oFolder.GroupWith("1320002137");


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
                        oStaticText = (StaticText)(oForm.Items.Item("lblUpRef").Specific);
                        oStaticText.Caption = "Código Referencia";

                        oItemB = oForm.Items.Item("lblUpRef");
                        oItem = oForm.Items.Add("VID_FEUpRe", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
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
                        //if (ObjType == "19")
                        //    oComboBox.DataBind.SetBound(true, "ORPC", "U_UpRef");
                        //else
                        oForm.DataSources.UserDataSources.Add("CodRef", BoDataType.dt_SHORT_TEXT, 10);
                        oComboBox.DataBind.SetBound(true, "", "CodRef");
                        oForm.Items.Item("VID_FEUpRe").DisplayDesc = true;


                        oItemB = oForm.Items.Item("lblUpRef");
                        oItem = oForm.Items.Add("lblRazRef", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FERazR";
                        oStaticText = (StaticText)(oForm.Items.Item("lblRazRef").Specific);
                        oStaticText.Caption = "Razón Referencia";

                        oItemB = oForm.Items.Item("lblRazRef");
                        oItem = oForm.Items.Add("VID_FERazR", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 170;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oEditText = (EditText)(oForm.Items.Item("VID_FERazR").Specific);
                        //if (ObjType == "19")
                        //    oEditText.DataBind.SetBound(true, "ORPC", "U_RazonRef");
                        //else
                        oForm.DataSources.UserDataSources.Add("RazRef", BoDataType.dt_LONG_TEXT, 254);
                        oEditText.DataBind.SetBound(true, "", "RazRef");


                        oItemB = oForm.Items.Item("lblRazRef");
                        oItem = oForm.Items.Add("lbl90", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;//14
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FE90";
                        oStaticText = (StaticText)(oForm.Items.Item("lbl90").Specific);
                        oStaticText.Caption = "IVA + 90 dias";

                        oItemB = oForm.Items.Item("lbl90");
                        oItem = oForm.Items.Add("VID_FE90", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = oItemB.Width + 60;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;

                        /*if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"select C1.FldValue 'Code', C1.Descr 'Name'
                                  from CUFD C0
                                  JOIN UFD1 C1 ON C1.TableID = C0.TableID
                                              AND C1.FieldID = C0.FieldID
                                 WHERE C0.TableID = 'ORIN'
                                   AND C0.AliasID = 'UpRef'";
                        }
                        else
                        {
                            s = @"select C1.""FldValue"" ""Code"", C1.""Descr"" ""Name"" 
                              from ""CUFD"" C0 
                              JOIN ""UFD1"" C1 ON C1.""TableID"" = C0.""TableID"" 
                                          AND C1.""FieldID"" = C0.""FieldID"" 
                             WHERE C0.""TableID"" = 'ORIN' 
                               AND C0.""AliasID"" = 'UpRef' ";
                        }
                        oRecordSet.DoQuery(s);
                        FSBOf.FillCombo((ComboBox)(oForm.Items.Item("VID_FEUpRe").Specific), ref oRecordSet, false);*/
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FE90").Specific);
                        //if (ObjType == "19")
                        //    oComboBox.DataBind.SetBound(true, "ORPC", "U_UpRef");
                        //else
                        //oForm.DataSources.UserDataSources.Add("CodRef", BoDataType.dt_SHORT_TEXT, 10);
                        if (oForm.BusinessObject.Type == "14")
                            oComboBox.DataBind.SetBound(true, "ORIN", "U_90_dias");
                        else
                            oComboBox.DataBind.SetBound(true, "ORPC", "U_90_dias");
                        oForm.Items.Item("VID_FE90").DisplayDesc = true;


                        oItemB = oForm.Items.Item("lbl90");
                        oItem = oForm.Items.Add("mtxRefFE", SAPbouiCOM.BoFormItemTypes.it_MATRIX);
                        oItem.Left = oItemB.Left;
                        oItem.Width = 440;
                        oItem.Top = oItemB.Top + 15;
                        oItem.Height = 120;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "lblRazRef";

                        oMatrix = ((SAPbouiCOM.Matrix)(oItem.Specific));
                        oColumns = oMatrix.Columns;

                        oColumn = oColumns.Add("#", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oColumn.TitleObject.Caption = "#";
                        oColumn.Width = 30;
                        oColumn.Editable = false;

                        oColumn = oColumns.Add("TipoDTE", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oColumn.TitleObject.Caption = "Tipo DTE";
                        oColumn.DisplayDesc = true;
                        oColumn.Width = 90;
                        oColumn.Editable = true;
                        oColumn.DataBind.SetBound(true, "@VID_FEREFD", "U_TipoDTE");

                        oColumn = oColumns.Add("DocEntry", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oColumn.TitleObject.Caption = "Doc SBO";
                        oColumn.Width = 90;
                        oColumn.RightJustified = true;
                        oColumn.Editable = false;
                        oColumn.DataBind.SetBound(true, "@VID_FEREFD", "U_DocEntry");

                        oColumn = oColumns.Add("Folio", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oColumn.TitleObject.Caption = "Folio";
                        oColumn.RightJustified = true;
                        oColumn.Width = 90;
                        oColumn.Editable = true;
                        oColumn.DataBind.SetBound(true, "@VID_FEREFD", "U_DocFolio");

                        oColumn = oColumns.Add("Fecha", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oColumn.TitleObject.Caption = "Fecha";
                        oColumn.Width = 90;
                        oColumn.Editable = true;
                        oColumn.DataBind.SetBound(true, "@VID_FEREFD", "U_DocDate");

                        //                        if (GlobalSettings.RunningUnderSQLServer)
                        //                            s = @"SELECT T1.FldValue 'Code', T1.Descr 'Name'
                        //                                  FROM CUFD T0
                        //                                  JOIN UFD1 T1 ON T1.TableID = T0.TableID
                        //                                              AND T1.FieldID = T0.FieldID
                        //                                 WHERE T0.TableID = 'ORIN'
                        //                                   AND T0.AliasID = 'TipoRef'";
                        //                        else
                        //                            s = @"SELECT T1.""FldValue"" 'Code', T1.""Descr"" 'Name'
                        //                                  FROM ""CUFD"" T0
                        //                                  JOIN ""UFD1"" T1 ON T1.""TableID"" = T0.""TableID""
                        //                                              AND T1.""FieldID"" = T0.""FieldID""
                        //                                 WHERE T0.""TableID"" = 'ORIN'
                        //                                   AND T0.""AliasID"" = 'TipoRef'";
                        //                        oRecordSet.DoQuery(s);
                        //                        FSBOf.FillComboMtx(oMatrix.Columns.Item("TipoDTE"), ref oRecordSet, true);

                        oColumn = oColumns.Add("DocTotal", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oColumn.TitleObject.Caption = "Total Documento";
                        oColumn.RightJustified = true;
                        oColumn.Width = 90;
                        oColumn.Editable = false;
                        oColumn.Visible = false;
                        oColumn.DataBind.SetBound(true, "@VID_FEREFD", "U_DocTotal");

                        oMatrix.AddRow(1, 1);
                        oMatrix.AutoResizeColumns();

                    }


                    //Campo con el estado de DTE
                    oItemB = oForm.Items.Item("84");
                    oItem = oForm.Items.Add("lblEstado", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                    oItem.Left = oItemB.Left;
                    oItem.Width = oItemB.Width;
                    oItem.Top = oItemB.Top + oItemB.Height + 5;
                    oItem.Height = oItem.Height;
                    oItem.LinkTo = "VID_FEEstado";
                    oStatic = (StaticText)(oForm.Items.Item("lblEstado").Specific);
                    oStatic.Caption = "Estado Doc. Electronico";

                    oItemB = oForm.Items.Item("208");
                    oItem = oForm.Items.Add("VID_Estado", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                    oItem.Left = oItemB.Left;
                    oItem.Width = oItemB.Width + 30;
                    oItem.Top = oItemB.Top + oItemB.Height + 5;
                    oItem.Height = oItem.Height;
                    oItem.DisplayDesc = true;
                    oItem.Enabled = false;
                    oComboBox = (ComboBox)(oForm.Items.Item("VID_Estado").Specific);
                    if (ObjType == "19")
                        oComboBox.DataBind.SetBound(true, "ORPC", "U_EstadoFE");
                    else
                        oComboBox.DataBind.SetBound(true, "ORIN", "U_EstadoFE");


                    oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                    sSeries = (System.String)(oComboBox.Value);

                    if (GlobalSettings.RunningUnderSQLServer)
                    { s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor' from NNM1 where Series = {0} --AND ObjectCode = '{1}'"; }
                    else
                    { s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"" from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' "; }
                    s = String.Format(s, sSeries, oForm.BusinessObject.Type);
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
                                oForm.Items.Item("VID_FEDCTO").Visible = true;
                            }
                            else
                            {
                                oForm.Items.Item("VID_Estado").Visible = false;
                                oForm.Items.Item("lblEstado").Visible = false;
                                oForm.Items.Item("VID_FEDCTO").Visible = false;
                            }
                        }
                    }

                    Lista = new List<string>();
                    // Ok Ad  Fnd Vw Rq Sec
                    Lista.Add("VID_Estado  , f,  f,  f,  f, n, 1");
                    //Lista.Add('CardCode  , f,  t,  t,  f, r, 1');
                    FSBOf.SetAutoManaged(ref oForm, Lista);
                }
                else if ((Localidad == "PE") && (ObjType == "14"))
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = "select ISNULL(TaxIdNum,'') TaxIdNum from OADM ";
                    else
                        s = @"select IFNULL(""TaxIdNum"",'') ""TaxIdNum"" from ""OADM"" ";

                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount == 0)
                        throw new Exception("Debe ingresar RUC de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1");
                    else
                        RUC = ((System.String)oRecordSet.Fields.Item("TaxIdNum").Value).Trim();

                    if (FacturadorPE == "F")
                    {
                        try
                        {
                            LoginCount_FM = 0;
                            //oUser_FM = new pe.facturamovil.User();
                            if (GlobalSettings.oUser_FM.token == null)
                            {
                                if (GlobalSettings.RunningUnderSQLServer)
                                    oRecordSet.DoQuery("SELECT U_User,U_Pwd,U_CCEmail FROM [@VID_FEPARAM] WHERE Code = '1'");
                                else
                                    oRecordSet.DoQuery(@"SELECT ""U_User"", ""U_Pwd"", ""U_CCEmail"" FROM ""@VID_FEPARAM"" WHERE ""Code"" = '1'");
                                GlobalSettings.oUser_FM = FacturaMovilGlobal.processor.Authenticate(((System.String)oRecordSet.Fields.Item("U_User").Value).Trim(), ((System.String)oRecordSet.Fields.Item("U_Pwd").Value).Trim());
                                FacturaMovilGlobal.userConnected = GlobalSettings.oUser_FM;
                                var ii = 0;
                                var bExistePE = false;

                                if (GlobalSettings.oUser_FM.companies.Find(c => c.code.Trim() == RUC.Trim()) != null)
                                {
                                    FacturaMovilGlobal.selectedCompany = GlobalSettings.oUser_FM.companies.Single(c => c.code.Trim() == RUC.Trim());
                                    bExistePE = true;
                                    ii = GlobalSettings.oUser_FM.companies.Count;
                                }

                                if (!bExistePE)
                                    throw new Exception("No se ha encontrado el RUC " + RUC + "en la conexion de Factura Movil");

                                CCEmail_FM = ((System.String)oRecordSet.Fields.Item("U_CCEmail").Value).Trim();
                            }
                        }
                        catch (Exception ex)
                        {
                            FSBOApp.StatusBar.SetText("No se pudo establecer conexion con el servidor Factura Movil : " + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            OutLog("No se pudo establecer conexion con el servidor Factura Movil - User: " + ((System.String)oRecordSet.Fields.Item("U_User").Value).Trim() + " Pass: " + ((System.String)oRecordSet.Fields.Item("U_Pwd").Value).Trim() + " - " + ex.Message);
                        }
                    }
                    else if (FacturadorPE == "E")
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

                        if (GlobalSettings.RunningUnderSQLServer)
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
                        }
                    }

                    //colocar folder con los campos necesarios en FE PERU
                    oForm.DataSources.UserDataSources.Add("VID_FEDCTO", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                    oItem = oForm.Items.Add("VID_FEDCTO", SAPbouiCOM.BoFormItemTypes.it_FOLDER);

                    if (DocSubType == "--")
                    {
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

                        //cargar campos de usuarios
                        oItemB = oForm.Items.Item("40");
                        oItem = oForm.Items.Add("lblMDTD", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = 50; //oItemB.Left;
                        oItem.Width = 125;//;oItemB.Width;
                        oItem.Top = oItemB.Top + 15;//195
                        oItem.Height = oItem.Height;//14
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEMDTD";
                        oStatic = (StaticText)(oForm.Items.Item("lblMDTD").Specific);
                        oStatic.Caption = "Tipo de Documento";

                        oItemB = oForm.Items.Item("lblMDTD");
                        oItem = oForm.Items.Add("VID_FEMDTD", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = 60;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItemB.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEMDTD").Specific);
                        oEditText.DataBind.SetBound(true, "ORIN", "U_BPP_MDTD");

                        //--
                        oItemB = oForm.Items.Item("lblMDTD");
                        oItem = oForm.Items.Add("lblMDSD", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEMDSD";
                        oStatic = (StaticText)(oForm.Items.Item("lblMDSD").Specific);
                        oStatic.Caption = "Serie del documento";

                        oItemB = oForm.Items.Item("lblMDSD");
                        oItem = oForm.Items.Add("VID_FEMDSD", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = 90; // oItemB.Width;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEMDSD").Specific);
                        oEditText.DataBind.SetBound(true, "ORIN", "U_BPP_MDSD");

                        //--
                        oItemB = oForm.Items.Item("lblMDSD");
                        oItem = oForm.Items.Add("lblMDCD", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEMDCD";
                        oStatic = (StaticText)(oForm.Items.Item("lblMDCD").Specific);
                        oStatic.Caption = "Correlativo del documento";

                        oItemB = oForm.Items.Item("lblMDCD");
                        oItem = oForm.Items.Add("VID_FEMDCD", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = 90; // oItemB.Width;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEMDCD").Specific);
                        oEditText.DataBind.SetBound(true, "ORIN", "U_BPP_MDCD");


                        //--
                        oItemB = oForm.Items.Item("lblMDCD");
                        oItem = oForm.Items.Add("lblMDTN", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEMDTN";
                        oStatic = (StaticText)(oForm.Items.Item("lblMDTN").Specific);
                        oStatic.Caption = "Tipo de operacion";

                        oItemB = oForm.Items.Item("lblMDTN");
                        oItem = oForm.Items.Add("VID_FEMDTN", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = 140; // oItemB.Width;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FEMDTN").Specific);
                        oComboBox.DataBind.SetBound(true, "ORIN", "U_BPP_MDTN");

                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"select Code 'Code', Name 'Name'
                                    from [@FM_NOTES] 
                                   ORDER BY Code ";
                        }
                        else
                        {
                            s = @"select ""Code"" ""Code"", ""Name"" ""Name""
                                    from ""@FM_NOTES""
                                   ORDER BY ""Code"" ";
                        }
                        oRecordSet.DoQuery(s);
                        FSBOf.FillCombo((ComboBox)(oForm.Items.Item("VID_FEMDTN").Specific), ref oRecordSet, false);

                        //--
                        oItemB = oForm.Items.Item("VID_FEMDTD");
                        oItem = oForm.Items.Add("lblFE", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left + oItemB.Width + 100;
                        oItem.Width = oItemB.Width + 60;
                        oItem.Top = oItemB.Top - oItemB.Height - 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "lblMDCD";
                        oItem.Visible = false;
                        oStatic = (StaticText)(oForm.Items.Item("lblFE").Specific);
                        oStatic.Caption = "Datos documento origen";
                        oForm.Items.Item("lblFE").Visible = false;


                        //--
                        oItemB = oForm.Items.Item("lblFE");
                        oItem = oForm.Items.Add("lblMDTO", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEMDTO";
                        oStatic = (StaticText)(oForm.Items.Item("lblMDTO").Specific);
                        oStatic.Caption = "Tipo de Docto. origen";

                        oItemB = oForm.Items.Item("lblMDTO");
                        oItem = oForm.Items.Add("VID_FEMDTO", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = 90;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEMDTO").Specific);
                        oEditText.DataBind.SetBound(true, "ORIN", "U_BPP_MDTO");

                        //--
                        oItemB = oForm.Items.Item("lblMDTO");
                        oItem = oForm.Items.Add("lblMDSO", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEMDSO";
                        oStatic = (StaticText)(oForm.Items.Item("lblMDSO").Specific);
                        oStatic.Caption = "Serie documento origen";

                        oItemB = oForm.Items.Item("lblMDSO");
                        oItem = oForm.Items.Add("VID_FEMDSO", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = 90;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEMDSO").Specific);
                        oEditText.DataBind.SetBound(true, "ORIN", "U_BPP_MDSO");

                        //--
                        oItemB = oForm.Items.Item("lblMDSO");
                        oItem = oForm.Items.Add("lblMDCO", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEMDCO";
                        oStatic = (StaticText)(oForm.Items.Item("lblMDCO").Specific);
                        oStatic.Caption = "Correlativo docto. origen";

                        oItemB = oForm.Items.Item("lblMDCO");
                        oItem = oForm.Items.Add("VID_FEMDCO", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = 90;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEMDCO").Specific);
                        oEditText.DataBind.SetBound(true, "ORIN", "U_BPP_MDCO");
                    }
                }
            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            finally
            {
                oForm.Visible = true;
                oForm.Freeze(false);
            }
            return Result;

        }//fin InitForm

        public new void MenuEvent(ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
            SAPbouiCOM.Conditions oConditions;
            SAPbouiCOM.Condition oCondition;
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
                    if (oForm.Mode == BoFormMode.fm_ADD_MODE)
                        oForm.Items.Item("VID_FE90").Enabled = true;
                    else
                        oForm.Items.Item("VID_FE90").Enabled = true; //** false;

                    if ((pVal.MenuUID == "1288") || (pVal.MenuUID == "1289") || (pVal.MenuUID == "1290") || (pVal.MenuUID == "1291") || (pVal.MenuUID == "1304"))
                    {
                        oForm.Freeze(true);
                        //oForm.Items.Item("VID_Estado").Enabled = false;

                        if (Localidad == "CL")
                        {
                            oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                            var sSeries = (System.String)(oComboBox.Value);

                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor' from NNM1 where Series = {0} --AND ObjectCode = '{1}' "; }
                            else
                            { s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"" from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' "; }
                            s = String.Format(s, sSeries, oForm.BusinessObject.Type);
                            oRecordSet.DoQuery(s);
                            if (oRecordSet.RecordCount > 0)
                            {
                                if ((System.String)(oRecordSet.Fields.Item("Valor").Value) == "E")
                                {
                                    oForm.Items.Item("VID_Estado").Visible = true;
                                    oForm.Items.Item("lblEstado").Visible = true;
                                    oForm.Items.Item("VID_FEDCTO").Visible = true;
                                    if (oForm.BusinessObject.Type == "14")
                                        CargarReferencia(oForm.BusinessObject.Type, ((System.String)oForm.DataSources.DBDataSources.Item("ORIN").GetValue("DocEntry", 0)).Trim());
                                    else
                                        CargarReferencia(oForm.BusinessObject.Type, ((System.String)oForm.DataSources.DBDataSources.Item("ORPC").GetValue("DocEntry", 0)).Trim());
                                }
                                else
                                {
                                    oForm.Items.Item("VID_Estado").Visible = false;
                                    oForm.Items.Item("lblEstado").Visible = false;
                                }
                            }
                        }
                        oForm.Freeze(false);
                    }

                    if ((pVal.MenuUID == "1282") || (pVal.MenuUID == "1281") || (pVal.MenuUID == "1287"))
                    {
                        oForm.Freeze(true);
                        //oForm.Items.Item("VID_Estado").Enabled = false;
                        if (Localidad == "CL")
                        {
                            oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                            var sSeries = (System.String)(oComboBox.Value);

                            if (GlobalSettings.RunningUnderSQLServer)
                            { s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor' from NNM1 where Series = {0} --AND ObjectCode = '{1}' "; }
                            else
                            { s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"" from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' "; }
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
                                    var oMatrix = ((SAPbouiCOM.Matrix)oForm.Items.Item("mtxRefFE").Specific);
                                    oConditions = (SAPbouiCOM.Conditions)FSBOApp.CreateObject(BoCreatableObjectType.cot_Conditions);

                                    oCondition = oConditions.Add();
                                    oCondition.Alias = "DocEntry";
                                    oCondition.Operation = BoConditionOperation.co_EQUAL;
                                    oCondition.CondVal = "0";

                                    //oDBVID_FEREF.Query(oConditions);
                                    oDBVID_FEREFD.Query(oConditions);
                                    oForm.DataSources.UserDataSources.Item("CodRef").Value = "";
                                    oForm.DataSources.UserDataSources.Item("RazRef").Value = "";
                                    oMatrix.LoadFromDataSource();
                                    oMatrix.AddRow(1, 1);
                                    oMatrix.FlushToDataSource();
                                    oMatrix.AutoResizeColumns();
                                }
                                else if (pVal.MenuUID == "1281")
                                {
                                    oForm.DataSources.UserDataSources.Item("CodRef").Value = "";
                                    oForm.DataSources.UserDataSources.Item("RazRef").Value = "";
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
            }
        }//fin MenuEvent

        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            Int32 nErr;
            String sErr;
            SAPbouiCOM.Grid oGrid;
            SAPbouiCOM.IChooseFromListEvent oCFLEvento = null;
            SAPbouiCOM.DataTable oDataTableD;
            //inherited FormEvent(FormUID,Var pVal,Var BubbleEvent);
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if ((pVal.ItemUID == "1") && (oForm.Mode == BoFormMode.fm_ADD_MODE))
                    {
                        if (Localidad == "CL")
                            BubbleEvent = ValidarDatosFE();
                        else if (Localidad == "PE")
                            BubbleEvent = ValidarDatosFE_PE();
                    }
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

                if ((pVal.ItemUID == "88") && (pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (!pVal.BeforeAction) && (Localidad == "CL"))
                {
                    oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                    var sSeries = (System.String)(oComboBox.Value);

                    if (GlobalSettings.RunningUnderSQLServer)
                    { s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor' from NNM1 where Series = {0} --AND ObjectCode = '{1}' "; }
                    else
                    { s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"" from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' "; }
                    s = String.Format(s, sSeries, oForm.BusinessObject.Type);
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount > 0)
                    {
                        if ((System.String)(oRecordSet.Fields.Item("Valor").Value) == "E")
                        {
                            oForm.Items.Item("VID_Estado").Visible = true;
                            oForm.Items.Item("lblEstado").Visible = true;
                            oForm.Items.Item("VID_FEDCTO").Visible = true;
                        }
                        else
                        {
                            oForm.Items.Item("VID_Estado").Visible = false;
                            oForm.Items.Item("lblEstado").Visible = false;
                            oForm.Items.Item("VID_FEDCTO").Visible = false;
                            s = "112";
                            oForm.Items.Item(s).Click(BoCellClickType.ct_Regular);
                        }
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (pVal.BeforeAction) && (pVal.ItemUID == "mtxRefFE") && (pVal.ColUID == "TipoDTE"))
                {
                    var oMatrix = ((Matrix)oForm.Items.Item("mtxRefFE").Specific);
                    var card = ((System.String)((EditText)oForm.Items.Item("4").Specific).Value).Trim();
                    if (card == "")
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar Socio de Negocio", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        BubbleEvent = false;
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (!pVal.BeforeAction) && (pVal.ItemUID == "mtxRefFE") && (pVal.ColUID == "TipoDTE"))
                {
                    var oMatrix = ((Matrix)oForm.Items.Item("mtxRefFE").Specific);
                    oForm.Freeze(true);
                    if (((ComboBox)oMatrix.Columns.Item("TipoDTE").Cells.Item(pVal.Row).Specific).Selected.Value == "00")
                    {
                        oMatrix.FlushToDataSource();
                        ((EditText)oMatrix.Columns.Item("DocEntry").Cells.Item(pVal.Row).Specific).Value = "";
                        ((EditText)oMatrix.Columns.Item("Folio").Cells.Item(pVal.Row).Specific).Value = "";
                        ((EditText)oMatrix.Columns.Item("Fecha").Cells.Item(pVal.Row).Specific).Value = "";
                    }
                    else
                    {
                        if (((System.String)((ComboBox)oMatrix.Columns.Item("TipoDTE").Cells.Item(pVal.Row).Specific).Selected.Value).IndexOf('b') != -1)
                            oMatrix.Columns.Item("Fecha").Editable = true;
                        else
                            oMatrix.Columns.Item("Fecha").Editable = true;
                    }
                    oMatrix.FlushToDataSource();
                    oMatrix.AutoResizeColumns();
                    oForm.Freeze(false);

                }

                if ((pVal.EventType == BoEventTypes.et_VALIDATE) && (!pVal.BeforeAction) && (pVal.ItemUID == "mtxRefFE") && (pVal.ColUID == "Folio"))
                {
                    var oMatrix = ((Matrix)oForm.Items.Item("mtxRefFE").Specific);
                    oMatrix.FlushToDataSource();
                    s = oDBVID_FEREFD.GetValue("U_DocFolio", pVal.Row - 1).ToString();
                    if ((s != "") && (s != "0"))
                    {
                        if (((EditText)oMatrix.Columns.Item("Folio").Cells.Item(pVal.Row).Specific).Value != "0")
                        {
                            if (((System.String)((ComboBox)oMatrix.Columns.Item("TipoDTE").Cells.Item(pVal.Row).Specific).Selected.Value).IndexOf('b') != -1)
                            {
                                FSBOApp.StatusBar.SetText("Documento de Otro Sistema", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                if (((EditText)oMatrix.Columns.Item("DocEntry").Cells.Item(pVal.Row).Specific).Value != "")
                                    ((EditText)oMatrix.Columns.Item("DocEntry").Cells.Item(pVal.Row).Specific).Value = "";
                                oMatrix.FlushToDataSource();
                            }
                            else
                                BuscarDatosDoc(pVal.Row - 1);

                            if (((Matrix)oForm.Items.Item("mtxRefFE").Specific).RowCount == pVal.Row)
                            {
                                oForm.Freeze(true);
                                ((Matrix)oForm.Items.Item("mtxRefFE").Specific).AddRow(1, pVal.Row);
                                ((EditText)oMatrix.Columns.Item("DocEntry").Cells.Item(pVal.Row + 1).Specific).Value = "";
                                ((EditText)oMatrix.Columns.Item("Folio").Cells.Item(pVal.Row + 1).Specific).Value = "";
                                ((EditText)oMatrix.Columns.Item("Fecha").Cells.Item(pVal.Row + 1).Specific).Value = "";
                                ((ComboBox)oMatrix.Columns.Item("TipoDTE").Cells.Item(pVal.Row + 1).Specific).Select("00", BoSearchKey.psk_ByValue);

                                ((Matrix)oForm.Items.Item("mtxRefFE").Specific).FlushToDataSource();
                                oMatrix.AutoResizeColumns();
                                oForm.Freeze(false);
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
            String TaxIdNum;
            String Canceled = "";
            String GeneraT = "";
            String CAF = "";
            Int32 FolioNum;
            Int32 FDocEntry = 0;
            Int32 FLineId = -1;
            String tabla;
            Boolean bFolioDistribuido = false;
            String TipoElect;
            Boolean bFolioAsignado = false;
            SAPbobsCOM.Documents oDocument;
            String Distribuido = "N";
            //inherited FormDataEvent(var BusinessObjectInfo,var BubbleEvent);
            base.FormDataEvent(ref BusinessObjectInfo, ref BubbleEvent);

            try
            {
                if ((BusinessObjectInfo.BeforeAction == false) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_ADD) && (BusinessObjectInfo.ActionSuccess))
                {
                    if (Localidad == "CL")
                    {
                        var Obj = new[] { "14", "19" }; // int[] 
                        //if (oForm.BusinessObject.Type in ["14"]) //And (Flag = true)) then
                        if (Obj.Contains(oForm.BusinessObject.Type))
                        {
                            if (ObjType == "19")
                                tabla = "ORPC";
                            else
                                tabla = "ORIN";

                            Flag = false;
                            if (GlobalSettings.RunningUnderSQLServer)
                                s = @"select ISNULL(U_Distrib,'N') 'Distribuido', ISNULL(U_MultiSoc,'N') MultiSoc, ISNULL(U_GenerarT,'N') GeneraT from [@VID_FEPARAM] WITH (NOLOCK)";
                            else
                                s = @"select IFNULL(""U_Distrib"",'N') ""Distribuido"", IFNULL(""U_MultiSoc"",'N') ""MultiSoc"", IFNULL(""U_GenerarT"",'N') ""GeneraT"" from ""@VID_FEPARAM"" ";
                            oRecordSet.DoQuery(s);
                            if (oRecordSet.RecordCount > 0)
                            {
                                GeneraT = ((System.String)oRecordSet.Fields.Item("GeneraT").Value).Trim();
                                Distribuido = ((System.String)oRecordSet.Fields.Item("Distribuido").Value).Trim();

                                if ((System.String)(oRecordSet.Fields.Item("MultiSoc").Value) == "Y")
                                    bMultiSoc = true;
                                else
                                    bMultiSoc = false;


                                sDocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                                if (GlobalSettings.RunningUnderSQLServer)
                                {
                                    s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'Inst', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'TipoDocElect', T0.CANCELED
                                             FROM {1} T0 WITH (NOLOCK)
                                                JOIN NNM1 T2 WITH (NOLOCK) ON T0.Series = T2.Series 
                                               WHERE T0.DocEntry = {0}";
                                }
                                else
                                {
                                    s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""TipoDocElect"", T0.""CANCELED""
                                             FROM ""{1}"" T0
                                             JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                                            WHERE T0.""DocEntry"" = {0} ";
                                }
                                s = String.Format(s, sDocEntry, tabla);
                                oRecordSet.DoQuery(s);
                                sDocSubType = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                                Tipo = (System.String)(oRecordSet.Fields.Item("Tipo").Value);
                                TipoElect = (System.String)(oRecordSet.Fields.Item("TipoDocElect").Value);
                                Canceled = (System.String)(oRecordSet.Fields.Item("CANCELED").Value);

                                if ((Tipo == "E") && (Canceled == "N"))
                                {
                                    //Agregar referencia en las tablas de usuario
                                    if (!GuardarReferencia(sDocEntry, tabla, false))
                                        FSBOApp.StatusBar.SetText("No se ha guardado las referencias", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    else
                                    {
                                        ((Matrix)oForm.Items.Item("mtxRefFE").Specific).Clear();
                                    }
                                    //Fin Agregar referencia en las tablas de usuario
                                    if (Distribuido == "Y")
                                    {
                                        if (ObjType == "19")
                                            oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseCreditNotes));
                                        else
                                            oDocument = (SAPbobsCOM.Documents)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes));


                                        if ((bMultiSoc == true) && (nMultiSoc == ""))
                                            FSBOApp.StatusBar.SetText("Se encuentra parametrizado para Multiples Sociedades pero no se encuentra parametrizada la serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                        else
                                        {
                                            if (TipoElect == "112")
                                                TipoDocElec = "112";
                                            else //if (sDocSubType == "--") //Nota Credito
                                                TipoDocElec = "61";

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

                                                if (oDocument.GetByKey(Convert.ToInt32(sDocEntry)))
                                                {
                                                    if (oDocument.FolioNumber == 0)
                                                    {
                                                        oDocument.FolioNumber = FolioNum;
                                                        oDocument.FolioPrefixString = "NC";
                                                        oDocument.Printed = PrintStatusEnum.psYes;

                                                        lRetCode = oDocument.Update();
                                                        if (lRetCode != 0)
                                                        {
                                                            bFolioAsignado = false;
                                                            FSBOApp.StatusBar.SetText("No se ha asignado Folio al Documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                            OutLog("No se ha asignado Folio al Documento DocEntry: " + sDocEntry + " Tipo: " + oForm.BusinessObject.Type + " - " + FCmpny.GetLastErrorDescription());
                                                        }
                                                        else
                                                        {
                                                            //ahora debo marcar que el folio fue usado y colocar los datos del documento que uso el folio
                                                            Reg = new TFunctions();
                                                            Reg.SBO_f = FSBOf;
                                                            //s = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value));
                                                            //s = Convert.ToString((System.Int32)(oRecordSet.Fields.Item("LineId").Value));
                                                            //s = Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_Folio").Value));

                                                            if (GlobalSettings.RunningUnderSQLServer)
                                                                s = "update [@VID_FEDISTD] set U_Estado = 'U', U_DocEntry = {0}, U_ObjType = '{1}', U_SubType = '{2}' where DocEntry = {3} and LineId = {4}";
                                                            else
                                                                s = @"update ""@VID_FEDISTD"" set ""U_Estado"" = 'U', ""U_DocEntry"" = {0}, ""U_ObjType"" = '{1}', ""U_SubType"" = '{2}' where ""DocEntry"" = {3} and ""LineId"" = {4}";
                                                            s = String.Format(s, sDocEntry, ObjType, sDocSubType, FDocEntry, FLineId);
                                                            oRecordSet.DoQuery(s);
                                                            bFolioAsignado = true;

                                                            //lRetCode = Reg.ActEstadoFolioUpt((System.Int32)(oRecordSet.Fields.Item("DocEntry").Value), (System.Int32)(oRecordSet.Fields.Item("LineId").Value), (System.Double)(oRecordSet.Fields.Item("U_Folio").Value), TipoDocElec, sDocEntry, "14", sDocSubType);

                                                            if (GeneraT == "Y")
                                                            {
                                                                //Colocar Timbre
                                                                XmlDocument xmlCAF = new XmlDocument();
                                                                XmlDocument xmlTimbre = new XmlDocument();
                                                                if (CAF == "")
                                                                    throw new Exception("No se ha encontrado xml de CAF");
                                                                //OutLog(CAF);
                                                                xmlCAF.LoadXml(CAF);
                                                                xmlTimbre = TimbreSII.EmitirTimbre(TipoDocElec, Convert.ToString(oDocument.FolioNumber), oDocument.DocDate.ToString("yyyyMMdd"), oDocument.FederalTaxID, oDocument.CardName, Convert.ToString(Math.Round(oDocument.DocTotal, 0)), oDocument.Lines.ItemDescription, xmlCAF, TaxIdNum);

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
                                                                            FSBOApp.StatusBar.SetText("No se ha creado Timbre en el documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
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
                                                                EnviarFE(sDocEntry, sDocSubType, bMultiSoc, nMultiSoc, GlobalSettings.GLOB_EncryptSQL, GlobalSettings.RunningUnderSQLServer, ObjType, TipoDocElec);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            { FSBOApp.StatusBar.SetText("No se encuentra folios disponibles para SBO", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                                        }
                                    }
                                }
                                else
                                {
                                    if (Canceled == "N")
                                        FSBOApp.StatusBar.SetText("Documento creado no es electronico", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                }
                            }
                            else
                            { FSBOApp.StatusBar.SetText("Debe Parametrizar el Addon", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning); }
                        }
                        else
                        { Flag = true; }
                    }
                    else if (Localidad == "PE")
                    {
                        sDocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'Inst'
                                                 ,ISNULL(T0.U_BPP_MDTD,'') BPP_MDTD, ISNULL(T0.U_BPP_MDSD,'') BPP_MDSD, ISNULL(T0.U_BPP_MDCD,'') BPP_MDCD, T0.CANCELED
                                             FROM ORIN T0 WITH (NOLOCK)
                                                JOIN NNM1 T2 WITH (NOLOCK) ON T0.Series = T2.Series 
                                               WHERE T0.DocEntry = {0}";
                        }
                        else
                        {
                            s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                                                 ,IFNULL(T0.""U_BPP_MDTD"",'') ""BPP_MDTD"", IFNULL(T0.""U_BPP_MDSD"",'') ""BPP_MDSD"", IFNULL(T0.""U_BPP_MDCD"",'') ""BPP_MDCD"", T0.""CANCELED""
                                             FROM ""ORIN"" T0
                                             JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                                            WHERE T0.""DocEntry"" = {0} ";
                        }
                        s = String.Format(s, sDocEntry);
                        oRecordSet.DoQuery(s);
                        sDocSubType = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                        Canceled = (System.String)(oRecordSet.Fields.Item("CANCELED").Value);

                        if (Canceled == "N")
                        {
                            if (((System.String)oRecordSet.Fields.Item("BPP_MDTD").Value).Trim() == "")
                                FSBOApp.StatusBar.SetText("No se encuentra ingresado tipo de documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            else if (((System.String)oRecordSet.Fields.Item("BPP_MDSD").Value).Trim() == "")
                                FSBOApp.StatusBar.SetText("No se encuentra ingresado serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            else if (((System.String)oRecordSet.Fields.Item("BPP_MDCD").Value).Trim() == "")
                                FSBOApp.StatusBar.SetText("No se encuentra ingresado correlativo del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            else
                            {
                                SBO_f = FSBOf;
                                if (FacturadorPE == "F") //Factura Movil
                                {
                                    TipoDocElec = "07";
                                    EnviarCN_PE(sDocEntry, GlobalSettings.RunningUnderSQLServer, ((System.String)oRecordSet.Fields.Item("BPP_MDSD").Value).Trim(), ((System.String)oRecordSet.Fields.Item("BPP_MDCD").Value).Trim(), TipoDocElec, oForm.BusinessObject.Type, sDocSubType, RUC, ref GlobalSettings.oUser_FM);
                                }
                                else //FacturadoPE == E //EasyDot
                                {
                                    TipoDocElec = "07";
                                    EnviarCN_PE_ED(sDocEntry, GlobalSettings.RunningUnderSQLServer, ((System.String)oRecordSet.Fields.Item("BPP_MDSD").Value).Trim(), ((System.String)oRecordSet.Fields.Item("BPP_MDCD").Value).Trim(), TipoDocElec, oForm.BusinessObject.Type, sDocSubType, RUC, ConexionADO, Localidad, TipoDocElec);
                                }
                            }
                        }
                        //--
                    }
                }
                else if ((BusinessObjectInfo.BeforeAction == false) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_UPDATE) && (BusinessObjectInfo.ActionSuccess))
                {
                    if (Localidad == "CL")
                    {
                        var Obj = new[] { "14", "19" }; // int[] 
                        //if (oForm.BusinessObject.Type in ["14"]) //And (Flag = true)) then
                        if (Obj.Contains(oForm.BusinessObject.Type))
                        {
                            if (ObjType == "19")
                                tabla = "ORPC";
                            else
                                tabla = "ORIN";

                            Flag = false;
                            sDocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                            if (GlobalSettings.RunningUnderSQLServer)
                            {
                                s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'Inst', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'TipoDocElect', T0.CANCELED
                                             FROM {1} T0 WITH (NOLOCK)
                                                JOIN NNM1 T2 WITH (NOLOCK) ON T0.Series = T2.Series 
                                               WHERE T0.DocEntry = {0}";
                            }
                            else
                            {
                                s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""TipoDocElect"", T0.""CANCELED""
                                             FROM ""{1}"" T0
                                             JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                                            WHERE T0.""DocEntry"" = {0} ";
                            }
                            s = String.Format(s, sDocEntry, tabla);
                            oRecordSet.DoQuery(s);
                            sDocSubType = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                            Tipo = (System.String)(oRecordSet.Fields.Item("Tipo").Value);
                            TipoElect = (System.String)(oRecordSet.Fields.Item("TipoDocElect").Value);
                            Canceled = (System.String)(oRecordSet.Fields.Item("CANCELED").Value);

                            if ((Tipo == "E") && (Canceled == "N"))
                            {
                                //Agregar referencia en las tablas de usuario
                                if (!GuardarReferencia(sDocEntry, tabla, true))
                                    FSBOApp.StatusBar.SetText("No se ha guardado las referencias", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            }
                        }
                    }//fin localidad CL
                }//fin eventype
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

                FSBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormDataEvent " + e.Message + " ** Trace: " + e.StackTrace);
            }

        }//fin FormDataEvent


        public new void PrintEvent(ref SAPbouiCOM.PrintEventInfo eventInfo, ref Boolean BubbleEvent)
        {
            String tabla;
            //XmlDocument _xmlDocument;
            //XmlNode N;
            //inherited PrintEvent(var eventInfo,var BubbleEvent);
            base.PrintEvent(ref eventInfo, ref BubbleEvent);
            oForm = FSBOApp.Forms.Item(eventInfo.FormUID);

            if ((eventInfo.FormUID.Length > 0) && (eventInfo.WithPrinterPreferences))
            {
                if ((eventInfo.EventType == BoEventTypes.et_PRINT) && (eventInfo.BeforeAction))
                {
                    if (Localidad == "CL")
                    {
                        if (ObjType == "19")
                            tabla = "ORPC";
                        else
                            tabla = "ORIN";

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

                        if (ObjType == "19")
                            s = String.Format(s, (System.String)(oForm.DataSources.DBDataSources.Item("ORPC").GetValue("DocEntry", 0)), tabla);//, DocSubType);
                        else
                            s = String.Format(s, (System.String)(oForm.DataSources.DBDataSources.Item("ORIN").GetValue("DocEntry", 0)), tabla);//, DocSubType);

                        oRecordSet.DoQuery(s);
                        if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                        {
                            FSBOApp.StatusBar.SetText("Documento Electronico", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                            //BubbleEvent := false;
                        }
                    }
                    //OutLog(s);
                    //end;
                }
            }
        }//fin PrintEvent

        public new void ReportDataEvent(ref SAPbouiCOM.ReportDataInfo eventInfo, ref Boolean BubbleEvent)
        {
            String tabla;
            base.ReportDataEvent(ref eventInfo, ref BubbleEvent);
            //inherited ReportDataEvent(var eventInfo,var BubbleEvent);
            oForm = FSBOApp.Forms.Item(eventInfo.FormUID);

            //OutLog("ReportData " + eventInfo.EventType.ToString);
            if (eventInfo.FormUID.Length > 0) //and (eventInfo.WithPrinterPreferences) then
            {
                if ((eventInfo.EventType == BoEventTypes.et_PRINT_DATA) && (eventInfo.BeforeAction))
                {
                    if (Localidad == "CL")
                    {
                        if (ObjType == "19")
                            tabla = "ORPC";
                        else
                            tabla = "ORIN";

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

                        if (ObjType == "19")
                            s = String.Format(s, (System.String)(oForm.DataSources.DBDataSources.Item("ORPC").GetValue("DocEntry", 0)), tabla);//, DocSubType);
                        else
                            s = String.Format(s, (System.String)(oForm.DataSources.DBDataSources.Item("ORIN").GetValue("DocEntry", 0)), tabla);//, DocSubType);

                        oRecordSet.DoQuery(s);
                        if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                        {
                            //FSBOApp.StatusBar.SetText("Documento Electronico", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                            //BubbleEvent := false;
                        }
                    }
                }
            }
        }//fin ReportDataEvent


        private void BuscarDatosDoc(Int32 iLinea)
        {
            SAPbouiCOM.Matrix oMatrix;
            String TipoDTE;
            String DocDate;
            DateTime fec;
            String obj;
            String card;
            String tabla = "OINV";
            String Folio;
            String NoIVA = "0";
            String sVal90;

            oForm.Freeze(true);
            try
            {
                oMatrix = ((SAPbouiCOM.Matrix)oForm.Items.Item("mtxRefFE").Specific);
                TipoDTE = ((System.String)oDBVID_FEREFD.GetValue("U_TipoDTE", iLinea)).Trim();
                Folio = oDBVID_FEREFD.GetValue("U_DocFolio", iLinea).ToString();
                DocDate = ((EditText)oForm.Items.Item("10").Specific).Value;
                fec = FSBOf.StrToDate(DocDate);

                card = ((EditText)oForm.Items.Item("4").Specific).Value;
                NoIVA = ((System.String)((ComboBox)oForm.Items.Item("VID_FE90").Specific).Selected.Value).Trim();
                if (NoIVA == "1")
                    fec = fec.AddMonths(-36);
                else
                    fec = fec.AddMonths(-2);

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
                else
                {
                    doc = "--";
                    obj = "13";
                }

                if (Val90 == "Y")
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        sVal90 = @" AND CONVERT(CHAR(6), T0.DocDate, 112) BETWEEN '{0}' AND '{1}'";
                    else
                        sVal90 = @" AND TO_VARCHAR(T0.""DocDate"", 'yyyyMM') BETWEEN '{0}' AND '{1}'";
                    sVal90 = String.Format(sVal90, fec.ToString("yyyyMM"), DocDate.Substring(0, 6));
                }
                else
                    sVal90 = "";

                if (GlobalSettings.RunningUnderSQLServer)
                {
                    s = @"SELECT T0.DocEntry 'DocEntry'
	                              ,T0.FolioNum 'Folio'
	                              ,T0.DocDate 'DocDate'
                                  ,T0.DocTotal
                                  ,O0.CurOnRight
                              FROM {0} T0
                              JOIN NNM1 N1 ON N1.Series = T0.Series
							              --AND N1.ObjectCode = T0.ObjType
                                  ,OADM O0
                             WHERE T0.DocSubType = '{2}'
                               AND ((T0.CardCode = '{1}') OR ('{1}' = ''))
                               AND ISNULL(T0.FolioNum,0) = {3}
                               AND T0.CANCELED = 'N'
                               {6}
                               {4}
                               {5}";
                    s = String.Format(s, tabla, card, doc, Folio, (bElec ? "AND LEFT(UPPER(ISNULL(BeginStr,'')), 1) = 'E'" : "AND LEFT(UPPER(ISNULL(BeginStr,'')), 1) <> 'E'")
                                     , (bExp ? "AND SUBSTRING(ISNULL(N1.BeginStr,''), 2, LEN(ISNULL(N1.BeginStr,''))) = '" + TipoDTE + "'" : "AND SUBSTRING(ISNULL(N1.BeginStr,''), 2, LEN(ISNULL(N1.BeginStr,''))) NOT IN ('110','111')")
                                     , sVal90);
                }
                else
                {
                    s = @"SELECT T0.""DocEntry"" ""DocEntry""
	                              ,T0.""FolioNum"" ""Folio""
	                              ,T0.""DocDate"" ""DocDate""
                                  ,T0.""DocTotal""
                                  ,O0.""CurOnRight""
                              FROM ""{0}"" T0
                              JOIN ""NNM1"" N1 ON N1.""Series"" = T0.""Series""
							              --AND N1.""ObjectCode"" = T0.""ObjType""
                                  ,""OADM"" O0
                             WHERE T0.""DocSubType"" = '{2}'
                               AND ((T0.""CardCode"" = '{1}') OR ('{1}' = ''))
                               AND IFNULL(T0.""FolioNum"",0) = {3}
                               AND T0.""CANCELED"" = 'N'
                               {6}
                               {4}
                               {5}";
                    s = String.Format(s, tabla, card, doc, Folio, (bElec ? @"AND LEFT(UPPER(IFNULL(""BeginStr"",'')), 1) = 'E'" : @"AND LEFT(UPPER(IFNULL(""BeginStr"",'')), 1) <> 'E'")
                                     , (bExp ? @"AND SUBSTRING(IFNULL(N1.""BeginStr"",''), 2, LENGTH(IFNULL(N1.""BeginStr"",''))) = '" + TipoDTE + "'" : @"AND SUBSTRING(IFNULL(N1.""BeginStr"",''), 2, LENGTH(IFNULL(N1.""BeginStr"",''))) NOT IN ('110','111')")
                                     , sVal90);
                }
                oRecordSet.DoQuery(s);
                //fec.ToString("yyyyMMdd"), DocDate
                if (oRecordSet.RecordCount == 0)
                {
                    FSBOApp.StatusBar.SetText("No se ha encontrado documentos o es superior a 90 dias", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    ((EditText)oMatrix.Columns.Item("Folio").Cells.Item(iLinea + 1).Specific).Value = "0";
                    ((EditText)oMatrix.Columns.Item("DocEntry").Cells.Item(iLinea + 1).Specific).Value = "0";
                    ((EditText)oMatrix.Columns.Item("Fecha").Cells.Item(iLinea + 1).Specific).Value = "";
                    ((EditText)oMatrix.Columns.Item("DocTotal").Cells.Item(iLinea + 1).Specific).Value = "0";
                    oMatrix.FlushToDataSource();
                    oMatrix.AutoResizeColumns();
                }
                else
                {
                    var DocEntryRef = ((System.Int32)oRecordSet.Fields.Item("DocEntry").Value).ToString();
                    var DocDateRef = ((System.DateTime)oRecordSet.Fields.Item("DocDate").Value).ToString("yyyyMMdd");
                    var DocTotalRef = ((System.Double)oRecordSet.Fields.Item("DocTotal").Value).ToString();

                    ((EditText)oMatrix.Columns.Item("DocEntry").Cells.Item(iLinea + 1).Specific).Value = DocEntryRef;
                    ((EditText)oMatrix.Columns.Item("Fecha").Cells.Item(iLinea + 1).Specific).Value = DocDateRef;
                    ((EditText)oMatrix.Columns.Item("DocTotal").Cells.Item(iLinea + 1).Specific).Value = DocTotalRef;
                    //oDBVID_FEREFD.SetValue("U_DocEntry", iLinea, s);
                    //oDBVID_FEREFD.SetValue("U_DocDate", iLinea, ((System.DateTime)oRecordSet.Fields.Item("DocDate").Value).ToString("yyyyMMdd"));
                    //oMatrix.LoadFromDataSource();
                    oMatrix.FlushToDataSource();
                    oMatrix.AutoResizeColumns();
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

        private void CargarReferencia(String Type, String sDocEntry)
        {
            SAPbouiCOM.Matrix oMatrix;
            SAPbouiCOM.Conditions oConditions;
            SAPbouiCOM.Condition oCondition;
            Int32 DocEntryFE;
            try
            {
                oMatrix = (SAPbouiCOM.Matrix)(oForm.Items.Item("mtxRefFE").Specific);
                oMatrix.Clear();

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT DocEntry, U_CodRef, U_RazRef FROM [@VID_FEREF] WHERE U_DocEntry = {0} AND U_DocSBO = '{1}'";
                else
                    s = @"SELECT ""DocEntry"", ""U_CodRef"", ""U_RazRef"" FROM ""@VID_FEREF"" WHERE ""U_DocEntry"" = {0} AND ""U_DocSBO"" = '{1}'";
                s = String.Format(s, sDocEntry, Type);
                //OutLog("Query linea 1545 " + s);
                oRecordSet.DoQuery(s);
                DocEntryFE = ((System.Int32)oRecordSet.Fields.Item("DocEntry").Value);

                oConditions = (SAPbouiCOM.Conditions)FSBOApp.CreateObject(BoCreatableObjectType.cot_Conditions);

                oCondition = oConditions.Add();
                oCondition.Alias = "DocEntry";
                oCondition.Operation = BoConditionOperation.co_EQUAL;
                oCondition.CondVal = DocEntryFE.ToString();

                //oDBVID_FEREF.Query(oConditions);
                oForm.DataSources.UserDataSources.Item("CodRef").Value = ((System.String)oRecordSet.Fields.Item("U_CodRef").Value).Trim();
                oForm.DataSources.UserDataSources.Item("RazRef").Value = ((System.String)oRecordSet.Fields.Item("U_RazRef").Value).Trim();
                oDBVID_FEREFD.Query(oConditions);

                oMatrix.LoadFromDataSource();
                oMatrix.AutoResizeColumns();

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT DocEntry FROM [@VID_FEREFD] WHERE DocEntry = {0}";
                else
                    s = @"SELECT ""DocEntry"" FROM ""@VID_FEREFD"" WHERE ""DocEntry"" = {0}";
                s = String.Format(s, DocEntryFE);
                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                {
                    oForm.Freeze(true);
                    ((Matrix)oForm.Items.Item("mtxRefFE").Specific).AddRow(1, 1);
                    ((EditText)oMatrix.Columns.Item("DocEntry").Cells.Item(1).Specific).Value = "";
                    ((EditText)oMatrix.Columns.Item("Folio").Cells.Item(1).Specific).Value = "";
                    ((EditText)oMatrix.Columns.Item("Fecha").Cells.Item(1).Specific).Value = "";
                    ((ComboBox)oMatrix.Columns.Item("TipoDTE").Cells.Item(1).Specific).Select("00", BoSearchKey.psk_ByValue);

                    ((Matrix)oForm.Items.Item("mtxRefFE").Specific).FlushToDataSource();
                    oMatrix.AutoResizeColumns();
                    oForm.Freeze(false);
                }
            }
            catch (Exception x)
            {
                FSBOApp.StatusBar.SetText("CargarReferencia - " + x.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CargarReferencia - " + x.Message + ", TRACE " + x.StackTrace);
            }
        }


        private Boolean GuardarReferencia(String sDocEntry, String tabla, Boolean bActualizar)
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

            try
            {
                ((Matrix)oForm.Items.Item("mtxRefFE").Specific).FlushToDataSource();
                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT DocSubType, DocNum, ObjType FROM {0} WHERE DocEntry = {1}";
                else
                    s = @"SELECT ""DocSubType"", ""DocNum"", ""ObjType"" FROM ""{0}"" WHERE ""DocEntry"" = {1}";
                s = String.Format(s, tabla, sDocEntry);
                oRecordSet.DoQuery(s);

                ObjType = ((System.String)oRecordSet.Fields.Item("ObjType").Value).Trim();
                DocSubType = ((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim();

                oCompanyService = FCmpny.GetCompanyService();
                oGeneralService = oCompanyService.GetGeneralService("VID_FERefDocs");

                if (bActualizar)
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
                    oGeneralData.SetProperty("U_DocEntry", sDocEntry);
                    oGeneralData.SetProperty("U_DocSBO", ObjType);
                    oGeneralData.SetProperty("U_DocSubTp", DocSubType);
                    oGeneralData.SetProperty("U_CodRef", ((System.String)oForm.DataSources.UserDataSources.Item("CodRef").Value).Trim());
                    oGeneralData.SetProperty("U_RazRef", ((System.String)oForm.DataSources.UserDataSources.Item("RazRef").Value).Trim());

                    StrDummy = "VID_FEREFD";
                    oChildren = oGeneralData.Child(StrDummy);
                    
                    for (Int32 i = 0; i < oDBVID_FEREFD.Size; i++)
                    {
                        if ((oDBVID_FEREFD.GetValue("U_TipoDTE", i) != "00") && (oDBVID_FEREFD.GetValue("U_DocFolio", i) != "") && (oDBVID_FEREFD.GetValue("U_DocFolio", i) != "0"))
                        {
                            oChild = oChildren.Add();
                            oChild.SetProperty("U_TipoDTE", oDBVID_FEREFD.GetValue("U_TipoDTE", i).Trim());
                            oChild.SetProperty("U_DocEntry", oDBVID_FEREFD.GetValue("U_DocEntry", i));
                            oChild.SetProperty("U_DocFolio", oDBVID_FEREFD.GetValue("U_DocFolio", i));
                            s = oDBVID_FEREFD.GetValue("U_DocDate", i);
                            oChild.SetProperty("U_DocDate", FSBOf.StrToDate(oDBVID_FEREFD.GetValue("U_DocDate", i)));
                            oChild.SetProperty("U_DocTotal", FSBOf.StrToDouble(oDBVID_FEREFD.GetValue("U_DocTotal", i)));
                            oChild.SetProperty("U_LineaRef", i);
                        }
                    }

                    oGeneralService.Update(oGeneralData);
                    CargarReferencia(ObjType, sDocEntry);
                }
                else
                {
                    oGeneralData = ((SAPbobsCOM.GeneralData)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData)));
                    oGeneralData.SetProperty("U_DocEntry", sDocEntry);
                    oGeneralData.SetProperty("U_DocSBO", ObjType);
                    oGeneralData.SetProperty("U_DocSubTp", DocSubType);
                    oGeneralData.SetProperty("U_CodRef", ((System.String)oForm.DataSources.UserDataSources.Item("CodRef").Value).Trim());
                    oGeneralData.SetProperty("U_RazRef", ((System.String)oForm.DataSources.UserDataSources.Item("RazRef").Value).Trim());
                    //  Handle child rows
                    oChildren = oGeneralData.Child("VID_FEREFD");
                    for (Int32 i = 0; i < oDBVID_FEREFD.Size; i++)
                    {
                        if ((oDBVID_FEREFD.GetValue("U_TipoDTE", i) != "00") && (oDBVID_FEREFD.GetValue("U_DocFolio", i) != "") && (oDBVID_FEREFD.GetValue("U_DocFolio", i) != "0"))
                        {
                            oChild = oChildren.Add();
                            oChild.SetProperty("U_TipoDTE", oDBVID_FEREFD.GetValue("U_TipoDTE", i).Trim());
                            oChild.SetProperty("U_DocEntry", oDBVID_FEREFD.GetValue("U_DocEntry", i));
                            oChild.SetProperty("U_DocFolio", oDBVID_FEREFD.GetValue("U_DocFolio", i));
                            s = oDBVID_FEREFD.GetValue("U_DocDate", i);
                            oChild.SetProperty("U_DocDate", FSBOf.StrToDate(oDBVID_FEREFD.GetValue("U_DocDate", i)));
                            oChild.SetProperty("U_DocTotal", FSBOf.StrToDouble(oDBVID_FEREFD.GetValue("U_DocTotal", i)));
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

        public void EnviarFE(String DocEntry, String SubType, Boolean bMultiSoc, String nMultiSoc, String GLOB_EncryptSQL, Boolean RunningUnderSQLServer, String sObjType, String TipoDocElec)
        {
            Boolean DocElec;
            SAPbobsCOM.Documents oDocument;
            TFunctions Param;
            String sCnn;
            Boolean bExento = false;
            SAPbobsCOM.Recordset ors;
            SAPbobsCOM.Company Cmpny;
            String sUser;
            String sPass;
            String tabla;

            try
            {
                if (sObjType == "19")
                    tabla = "ORPC";
                else
                    tabla = "ORIN";

                Cmpny = SBO_f.Cmpny;
                if (RunningUnderSQLServer)
                {
                    s = @"SELECT Count(*) Cont
                            FROM {2} T0 WITH (NOLOCK)
                            JOIN NNM1 T2 WITH (NOLOCK) ON T0.Series = T2.Series 
                           WHERE (SUBSTRING(UPPER(T2.BeginStr), 1, 1) = 'E') 
                            AND (T0.DocEntry = {0}) 
                            AND (T0.DocSubType = '{1}')";
                }
                else
                {
                    s = @"SELECT Count(*) ""Cont"" 
                            FROM ""{2}"" T0
                            JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                           WHERE (SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) = 'E')
                            AND (T0.""DocEntry"" = {0}) 
                            AND (T0.""DocSubType"" = '{1}') ";
                }
                s = String.Format(s, DocEntry, SubType, tabla);

                ors = (SAPbobsCOM.Recordset)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                ors.DoQuery(s);

                if ((System.Int32)(ors.Fields.Item("Cont").Value) > 0)
                { DocElec = true; }
                else
                { DocElec = false; }

                if (DocElec)
                {
                    bExento = false;

                    if (sObjType == "19")
                        oDocument = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseCreditNotes));
                    else
                        oDocument = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes));

                    if (oDocument.GetByKey(Convert.ToInt32(DocEntry)))
                    {
                        if (bMultiSoc == true)
                        {
                            if (RunningUnderSQLServer)
                            {
                                s = @"select U_Servidor, U_Base, U_Usuario, U_Password
                                      from [@VID_FEMULTISOC] WITH (NOLOCK)
                                     where DocEntry = {0}";
                            }
                            else
                            {
                                s = @"select ""U_Servidor"", ""U_Base"", ""U_Usuario"", ""U_Password""
                                      from ""@VID_FEMULTISOC""
                                     where ""DocEntry"" = {0} ";
                            }
                            s = String.Format(s, nMultiSoc);
                        }
                        else
                        {
                            if (RunningUnderSQLServer)
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
                        ors.DoQuery(s);
                        if (ors.RecordCount > 0)
                        {
                            Param = new TFunctions();
                            Param.SBO_f = SBO_f;
                            if (bMultiSoc)
                            {
                                sUser = (System.String)(ors.Fields.Item("U_Usuario").Value).ToString().Trim();
                                sPass = (System.String)(ors.Fields.Item("U_Password").Value).ToString().Trim();
                            }
                            else
                            {
                                sUser = Param.DesEncriptar((System.String)(ors.Fields.Item("U_Usuario").Value).ToString().Trim());
                                sPass = Param.DesEncriptar((System.String)(ors.Fields.Item("U_Password").Value).ToString().Trim());
                            }

                            sCnn = Param.sConexion((System.String)(ors.Fields.Item("U_Servidor").Value), (System.String)(ors.Fields.Item("U_Base").Value), sUser, sPass);
                            if (sCnn.Substring(0, 1) != "E")
                            {
                                //validar que exista procedimentos para tipo documento
                                if (RunningUnderSQLServer)
                                { s = "select ISNULL(U_ProcNomE,'') 'ProcNomE', ISNULL(U_ProcNomD,'') 'ProcNomD', ISNULL(U_ProcNomR,'') 'ProcNomR', ISNULL(U_ProcNomL,'') 'ProcNomL', ISNULL(U_ProcNomS,'') 'ProcNomS' from [@VID_FEPROCED] where ISNULL(U_Habili,'N') = 'Y' and U_TipoDoc = '{0}'"; }
                                else
                                { s = @"select IFNULL(""U_ProcNomE"",'') ""ProcNomE"", IFNULL(""U_ProcNomD"",'') ""ProcNomD"", IFNULL(""U_ProcNomR"",'') ""ProcNomR"", IFNULL(""U_ProcNomL"",'') ""ProcNomL"", IFNULL(""U_ProcNomS"",'') ""ProcNomS"" from ""@VID_FEPROCED"" where IFNULL(""U_Habili"",'N') = 'Y' and ""U_TipoDoc"" = '{0}'"; }

                                s = String.Format(s, TipoDocElec);
                                ors.DoQuery(s);
                                if (ors.RecordCount == 0)
                                {
                                    SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimientos para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                }
                                else
                                {
                                    if ((System.String)(ors.Fields.Item("ProcNomE").Value).ToString().Trim() == "")
                                        SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de encabezado para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    else if ((System.String)(ors.Fields.Item("ProcNomD").Value).ToString().Trim() == "")
                                        SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de detalle para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    else if (((System.String)(ors.Fields.Item("ProcNomR").Value).ToString().Trim() == "") && (TipoDocElec == "56"))
                                        SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de referencia para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    else
                                    {
                                        Enviar(sCnn, false, bExento, oDocument, null, TipoDocElec, bMultiSoc, nMultiSoc, (System.String)(ors.Fields.Item("ProcNomE").Value).ToString().Trim(), (System.String)(ors.Fields.Item("ProcNomD").Value).ToString().Trim(), (System.String)(ors.Fields.Item("ProcNomR").Value).ToString().Trim(), (System.String)(ors.Fields.Item("ProcNomL").Value).ToString().Trim(), (System.String)(ors.Fields.Item("ProcNomS").Value).ToString().Trim(), RunningUnderSQLServer, sObjType);
                                    }
                                }
                            }
                            else
                            { SBO_f.SBOApp.StatusBar.SetText("Faltan datos Conexion. " + sCnn.Substring(1, sCnn.Length - 1), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
                        }
                        else
                        { SBO_f.SBOApp.StatusBar.SetText("Debe ingresar datos de conexion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error); }
                    }
                }
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("EnviarFE " + e.Message + " ** Trace: " + e.StackTrace);
                SBO_f.SBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");  // Captura errores no manejados
            }
        }//fin EnviarFE


        private void Enviar(String sCnn, Boolean bTransfer, Boolean bExento, SAPbobsCOM.Documents oDocumento, SAPbobsCOM.Documents oTransfer, String TipoDocElec, Boolean bMultiSoc, String nMultiSoc, String ProcNomE, String ProcNomD, String ProcNomR, String ProcNomL, String ProcNomS, Boolean RunningUnderSQLServer, String sObjType)
        {
            String sPath;
            Boolean bImpresionOk = false;
            SqlConnection ConexionADO;
            SAPbobsCOM.Recordset orsLocal;
            String Arch = "";
            SAPbobsCOM.Company Cmpny;
            Int32 lRetCode;
            String DocSubType;
            Int32 DocEntry, FolioNum;
            String Status, sMessage, TipoDoc;
            TFunctions Reg;
            Int32 iCol;
            Boolean bDocExiste = false;
            Int32 iCont;
            String tabla;
            String DocDate;
            //SAPbouiCOM.DBDataSource oDBDSHeader;
            //SAPbouiCOM.DBDataSource oDBDSD;

            try
            {
                Cmpny = SBO_f.Cmpny;

                orsLocal = (SAPbobsCOM.Recordset)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                //Nota de Credito Electronica
                DocSubType = "--";


                String NomCol;
                Int32 iFila;
                SqlCommand SqlComan;
                SqlParameter oParameter;
                SqlDataAdapter Adapter;
                DataSet DataSet;
                Int32 dyt_id_traspaso = 0;
                SqlCommand cmd;
                if (RunningUnderSQLServer)
                    s = "exec " + ProcNomE + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "', '" + sObjType + "'";
                else
                    s = "CALL " + ProcNomE + " (" + oDocumento.DocEntry + ", '" + TipoDocElec + "', '" + sObjType + "')";

                orsLocal.DoQuery(s);
                if (orsLocal.RecordCount > 0)
                {
                    ConexionADO = new SqlConnection(sCnn);
                    if (ConexionADO.State == ConnectionState.Closed) ConexionADO.Open();
                    while (!orsLocal.EoF)
                    {
                        //revisa si documento se encuentra en el portal con estado 0 o en las tablas de error, si es asi eliminas los registros
                        cmd = new SqlCommand();
                        cmd.CommandTimeout = 0;
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = ConexionADO;
                        s = "exec VID_SP_FE_LimpiarRegistroFolio '{0}', {1}";
                        s = String.Format(s, TipoDocElec, oDocumento.FolioNumber);
                        cmd.CommandText = s;
                        cmd.ExecuteNonQuery();
                        //fin limpia registros
                        s = "select count(*) from faet_erp_encabezado_doc where cab_FOL_DOCTO_INT = {0} and CAB_COD_TP_FACTURA = '{1}' ";//and DYT_ESTADO_TRASPASO < 1 ";
                        s = String.Format(s, oDocumento.FolioNumber, TipoDocElec);
                        cmd.CommandText = s;
                        iCont = Convert.ToInt32(cmd.ExecuteScalar());
                        if (iCont > 0)
                        {
                            SBO_f.SBOApp.StatusBar.SetText("Documento ya se encuentra en el Portal  " + TipoDocElec + " - " + oDocumento.FolioNumber.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            bDocExiste = true;
                            break;
                        }
                        iCol = 0;
                        //ProcExecE = "exec FaeSrv_Erp_TrataEncabezado_Doc ";
                        SqlComan = new SqlCommand("FaeSrv_Erp_TrataEncabezado_Doc", ConexionADO);
                        oParameter = new SqlParameter();
                        SqlComan.CommandType = CommandType.StoredProcedure;
                        while (iCol < orsLocal.Fields.Count)
                        {
                            NomCol = "@" + orsLocal.Fields.Item(iCol).Name;
                            s = orsLocal.Fields.Item(iCol).Type.ToString();
                            oParameter = SqlComan.Parameters.AddWithValue(NomCol, orsLocal.Fields.Item(iCol).Value.ToString());
                            //SBO_f.oLog.OutLog("Parametro  " + NomCol + " - Valor " + orsLocal.Fields.Item(iCol).Value);
                            iCol++;
                        }

                        Adapter = new SqlDataAdapter(SqlComan);
                        DataSet = new DataSet(SqlComan.CommandText);
                        Adapter.Fill(DataSet);
                        dyt_id_traspaso = Convert.ToInt32(DataSet.Tables[0].Rows[0][1].ToString());
                        SBO_f.oLog.OutLog("DYT_ID_TRASPASO " + dyt_id_traspaso.ToString());


                        orsLocal.MoveNext();
                    }

                    if (!bDocExiste)
                    {
                        //buscar detalle
                        if ((ProcNomD != "") && (dyt_id_traspaso != 0))
                        {
                            if (RunningUnderSQLServer)
                            { s = "exec " + ProcNomD + " " + oDocumento.DocEntry + ", '" + TipoDocElec + "', " + dyt_id_traspaso.ToString() + ", '" + sObjType + "'"; }
                            else
                            { s = "CALL " + ProcNomD + " (" + oDocumento.DocEntry + ", '" + TipoDocElec + "', " + dyt_id_traspaso.ToString() + ", '" + sObjType + "')"; }
                            orsLocal.DoQuery(s);
                            iFila = 0;
                            if (orsLocal.RecordCount > 0)
                            {
                                while (!orsLocal.EoF)
                                {
                                    iCol = 0;
                                    SqlComan = new SqlCommand("FaeSrv_Erp_TrataDetalle_Doc", ConexionADO);
                                    oParameter = new SqlParameter();
                                    SqlComan.CommandType = CommandType.StoredProcedure;
                                    while (iCol < orsLocal.Fields.Count)
                                    {
                                        NomCol = "@" + orsLocal.Fields.Item(iCol).Name;
                                        s = orsLocal.Fields.Item(iCol).Type.ToString();
                                        oParameter = SqlComan.Parameters.AddWithValue(NomCol, orsLocal.Fields.Item(iCol).Value);
                                        //SBO_f.oLog.OutLog("Parametro  " + NomCol + " - Valor " + orsLocal.Fields.Item(iCol).Value);
                                        iCol++;
                                    }

                                    //mandar ejecucion al portal
                                    Adapter = new SqlDataAdapter(SqlComan);
                                    DataSet = new DataSet(SqlComan.CommandText);
                                    Adapter.Fill(DataSet);
                                    iFila++;
                                    orsLocal.MoveNext();
                                }

                                //busca lotes
                                if ((ProcNomL != "") && (dyt_id_traspaso != 0))
                                {
                                    if (RunningUnderSQLServer)
                                    { s = "exec " + ProcNomL + " " + oDocumento.DocEntry + ", '" + TipoDocElec + "', " + dyt_id_traspaso.ToString() + ", '" + sObjType + "'"; }
                                    else
                                    { s = "CALL " + ProcNomL + " (" + oDocumento.DocEntry + ", '" + TipoDocElec + "', " + dyt_id_traspaso.ToString() + ", '" + sObjType + "')"; }
                                    orsLocal.DoQuery(s);
                                    iFila = 0;
                                    if (orsLocal.RecordCount > 0)
                                    {
                                        while (!orsLocal.EoF)
                                        {
                                            iCol = 0;
                                            SqlComan = new SqlCommand("VID_SP_LOTES", ConexionADO);
                                            oParameter = new SqlParameter();
                                            SqlComan.CommandType = CommandType.StoredProcedure;
                                            while (iCol < orsLocal.Fields.Count)
                                            {
                                                NomCol = "@" + orsLocal.Fields.Item(iCol).Name;
                                                s = orsLocal.Fields.Item(iCol).Type.ToString();
                                                oParameter = SqlComan.Parameters.AddWithValue(NomCol, orsLocal.Fields.Item(iCol).Value);
                                                //SBO_f.oLog.OutLog("Parametro  " + NomCol + " - Valor " + orsLocal.Fields.Item(iCol).Value);
                                                iCol++;
                                            }

                                            //mandar ejecucion al portal
                                            Adapter = new SqlDataAdapter(SqlComan);
                                            DataSet = new DataSet(SqlComan.CommandText);
                                            Adapter.Fill(DataSet);

                                            iFila++;
                                            orsLocal.MoveNext();
                                        }
                                    }
                                }

                                //busca Series
                                if ((ProcNomS != "") && (dyt_id_traspaso != 0))
                                {
                                    if (RunningUnderSQLServer)
                                    { s = "exec " + ProcNomS + " " + oDocumento.DocEntry + ", '" + TipoDocElec + "', " + dyt_id_traspaso.ToString() + ", '" + sObjType + "'"; }
                                    else
                                    { s = "CALL " + ProcNomS + " (" + oDocumento.DocEntry + ", '" + TipoDocElec + "', " + dyt_id_traspaso.ToString() + ", '" + sObjType + "')"; }
                                    orsLocal.DoQuery(s);
                                    iFila = 0;
                                    if (orsLocal.RecordCount > 0)
                                    {
                                        while (!orsLocal.EoF)
                                        {
                                            iCol = 0;
                                            SqlComan = new SqlCommand("VID_SP_SERIES", ConexionADO);
                                            oParameter = new SqlParameter();
                                            SqlComan.CommandType = CommandType.StoredProcedure;
                                            while (iCol < orsLocal.Fields.Count)
                                            {
                                                NomCol = "@" + orsLocal.Fields.Item(iCol).Name;
                                                s = orsLocal.Fields.Item(iCol).Type.ToString();
                                                oParameter = SqlComan.Parameters.AddWithValue(NomCol, orsLocal.Fields.Item(iCol).Value);
                                                //SBO_f.oLog.OutLog("Parametro  " + NomCol + " - Valor " + orsLocal.Fields.Item(iCol).Value);
                                                iCol++;
                                            }

                                            //mandar ejecucion al portal
                                            Adapter = new SqlDataAdapter(SqlComan);
                                            DataSet = new DataSet(SqlComan.CommandText);
                                            Adapter.Fill(DataSet);

                                            iFila++;
                                            orsLocal.MoveNext();
                                        }
                                    }
                                }


                                //buscar Referencia
                                if (ProcNomR != "")
                                {
                                    if (RunningUnderSQLServer)
                                    { s = "exec " + ProcNomR + " " + oDocumento.DocEntry + ", '" + TipoDocElec + "', " + dyt_id_traspaso.ToString() + ", '" + sObjType + "'"; }
                                    else
                                    { s = "CALL " + ProcNomR + " (" + oDocumento.DocEntry + ", '" + TipoDocElec + "', " + dyt_id_traspaso.ToString() + ", '" + sObjType + "')"; }
                                    orsLocal.DoQuery(s);
                                    iFila = 0;
                                    if (orsLocal.RecordCount > 0)
                                    {
                                        while (!orsLocal.EoF)
                                        {
                                            iCol = 0;
                                            SqlComan = new SqlCommand("FaeSrv_Erp_TrataReferenciaDoc", ConexionADO);
                                            oParameter = new SqlParameter();
                                            SqlComan.CommandType = CommandType.StoredProcedure;
                                            while (iCol < orsLocal.Fields.Count)
                                            {
                                                NomCol = "@" + orsLocal.Fields.Item(iCol).Name;
                                                s = orsLocal.Fields.Item(iCol).Type.ToString();
                                                oParameter = SqlComan.Parameters.AddWithValue(NomCol, orsLocal.Fields.Item(iCol).Value);
                                                //SBO_f.oLog.OutLog("Parametro  " + NomCol + " - Valor " + orsLocal.Fields.Item(iCol).Value);
                                                iCol++;
                                            }

                                            //mandar ejecucion al portal
                                            Adapter = new SqlDataAdapter(SqlComan);
                                            DataSet = new DataSet(SqlComan.CommandText);
                                            Adapter.Fill(DataSet);

                                            iFila++;
                                            orsLocal.MoveNext();
                                        }
                                        bImpresionOk = true;
                                    }
                                    else
                                    {
                                        SBO_f.SBOApp.StatusBar.SetText("No se encuentra Datos en Referencia", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        bImpresionOk = false;
                                    }
                                }
                                else
                                {
                                    SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento en Referencia", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    bImpresionOk = false;
                                }

                                if (bImpresionOk)
                                { //cambiar estado del documento en el portal para que sea considerado en los servicios del portal
                                    //forma antigua ahora se hara por procedimiento de almacenado del portal
                                    /*if (RunningUnderSQLServer)
                                        s = "exec VID_SP_FE_CambioEstDoc " + oDocumento.FolioNumber + ", '" + TipoDocElec + "', " + dyt_id_traspaso.ToString();
                                    else
                                        s = "CALL VID_SP_FE_CambioEstDoc (" + oDocumento.FolioNumber + ", '" + TipoDocElec + "', " + dyt_id_traspaso.ToString() + ")";
                                    orsLocal.DoQuery(s);
                                    while (!orsLocal.EoF)
                                    {
                                        //SBO_f.oLog.OutLog("CambiaEstado*****");

                                        //mandar ejecucion al portal
                                        s = (System.String)(orsLocal.Fields.Item(0).Value);
                                        if (s != "")
                                        {
                                            cmd = new SqlCommand(s);
                                            cmd.Connection = ConexionADO;
                                            cmd.ExecuteNonQuery();
                                        }
                                        orsLocal.MoveNext();
                                    }*/
                                    SqlComan = new SqlCommand("FaeSrv_Erp_CambioEstDocv2", ConexionADO);
                                    oParameter = new SqlParameter();
                                    SqlComan.CommandType = CommandType.StoredProcedure;
                                    oParameter = SqlComan.Parameters.AddWithValue("@DYT_ID_TRASPASO", dyt_id_traspaso.ToString());
                                    oParameter = SqlComan.Parameters.AddWithValue("@DYT_USUARIO_ACTUALIZA", SBO_f.Cmpny.UserName);
                                    //mandar ejecucion al portal
                                    Adapter = new SqlDataAdapter(SqlComan);
                                    DataSet = new DataSet(SqlComan.CommandText);
                                    Adapter.Fill(DataSet);
                                }
                                bImpresionOk = true;
                            }
                            else
                            {
                                SBO_f.SBOApp.StatusBar.SetText("No se encuentra Datos en detalle", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                bImpresionOk = false;
                            }
                        }
                    }
                    if (ConexionADO.State == ConnectionState.Open) ConexionADO.Close();
                    //bImpresionOk = true;
                }
                else
                {
                    SBO_f.SBOApp.StatusBar.SetText("No se encuentra Datos en encabezado", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    bImpresionOk = false;
                }

                DocDate = SBO_f.DateToStr(oDocumento.DocDate);
                //ejecutar query para pasar los datos
                if (!bDocExiste)
                {
                    if (!bImpresionOk)
                    {
                        DocEntry = oDocumento.DocEntry;
                        FolioNum = oDocumento.FolioNumber;
                        sObjType = oDocumento.DocObjectCodeEx;
                        Status = "EE";
                        sMessage = "Error traspaso al portal";
                        TipoDoc = TipoDocElec;
                        SBO_f.SBOApp.MessageBox("Error durante impresión");
                        SBO_f.SBOApp.StatusBar.SetText("Error imprimiendo documento electrónico (1)", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        //if (FCmpny.InTransaction) then
                        //FCmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                        //exit
                    }
                    else
                    {
                        //actualiza campos de documento
                        DocEntry = oDocumento.DocEntry;
                        FolioNum = oDocumento.FolioNumber;
                        sObjType = oDocumento.DocObjectCodeEx;
                        Status = "EC";
                        sMessage = "Enviado satisfactoriamente";
                        TipoDoc = TipoDocElec;

                        if (oDocumento.Indicator == "")
                        {
                            if (RunningUnderSQLServer)
                            {
                                s = @"select TOP 1 U_Indicato
                                      from [@VID_FEDOCE]  T0 WITH (NOLOCK)
                                      join [@VID_FEDOCED] T1 WITH (NOLOCK) on T1.Code = T0.Code
                                     where T0.Code = '{0}'
                                       and isnull(T1.U_Indicato,'') <> ''";
                            }
                            else
                            {
                                s = @"select TOP 1 ""U_Indicato""
                                      from ""@VID_FEDOCE""  T0
                                      join ""@VID_FEDOCED"" T1 on T1.""Code"" = T0.""Code""
                                     where T0.""Code"" = '{0}'
                                       and IFNULL(T1.""U_Indicato"",'') <> '' ";
                            }
                            s = String.Format(s, TipoDocElec);
                            orsLocal.DoQuery(s);
                            if (orsLocal.RecordCount > 0)
                            {
                                if ((System.String)(orsLocal.Fields.Item("U_Indicato").Value) != "")
                                { oDocumento.Indicator = (System.String)(orsLocal.Fields.Item("U_Indicato").Value); }
                            }
                        }
                        oDocumento.UserFields.Fields.Item("U_EstadoFE").Value = "P";

                        lRetCode = oDocumento.Update();
                        //if (lRetCode <> 0) then
                        //begin
                        //s := "UPDATE ORIN SET U_SIIpref = '{0}', U_SIInum = '{1}' WHERE DocEntry = {3}";
                        //s := String.Format(s, oDocumento.FolioPrefixString, Convert.ToString(oDocumento.FolioNumber), oDocumento.DocEntry);
                        //orsLocal.DoQuery(s);
                        //end;
                        SBO_f.SBOApp.StatusBar.SetText("Enviado exitosamente al portal Factura Electronica", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    }

                    if (RunningUnderSQLServer)
                    { s = @"select DocEntry, U_Status from [@VID_FELOG] WITH (NOLOCK) where U_DocEntry = {0} and U_ObjType = '{1}' and U_SubType = '{2}'"; }
                    else
                    { s = @"select ""DocEntry"", ""U_Status"" from ""@VID_FELOG"" where ""U_DocEntry"" = {0} and ""U_ObjType"" = '{1}' and ""U_SubType"" = '{2}' "; }

                    s = String.Format(s, DocEntry, sObjType, DocSubType);
                    orsLocal.DoQuery(s);
                    Reg = new TFunctions();
                    Reg.SBO_f = SBO_f;
                    if (orsLocal.RecordCount == 0)
                        Reg.FELOGAdd(DocEntry, sObjType, DocSubType, "", FolioNum, Status, sMessage, TipoDoc, SBO_f.SBOApp.Company.UserName, "", "", "", DocDate);
                    else
                    {
                        if ((System.String)(orsLocal.Fields.Item("U_Status").Value) != "RR")
                            Reg.FELOGUptM((System.Int32)(orsLocal.Fields.Item("DocEntry").Value), DocEntry, sObjType, DocSubType, "", FolioNum, Status, sMessage, TipoDoc, SBO_f.SBOApp.Company.UserName, "", "", "", DocDate);
                        else
                            SBO_f.SBOApp.StatusBar.SetText("Documento ya se encuentra Recibido por SII", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    }
                }
            }
            catch (Exception ex)
            {
                SBO_f.oLog.OutLog("Enviar " + ex.Message + " ** Trace: " + ex.StackTrace);
                SBO_f.SBOApp.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);  // Captura errores no manejados
            }

        }//fin Enviar

        //Peru
        public new void EnviarCN_PE(String DocEntry, Boolean RunningUnderSQLServer, String SeriePE, String FolioNum, String TipoDocElec, String ObjType, String DocSubType, String lRUC, ref pe.facturamovil.User oUserFM)
        {
            SAPbobsCOM.Recordset orsLocal;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.Documents oDocumento;
            Boolean bImpresionOk;
            String Status;
            String sMessage;
            Int32 lRetCode;
            TFunctions Reg;
            String ProcNomE;
            String ProcNomD;
            String ProcNomR;
            String externalFolio;
            String Email;
            String Id = "0";
            String Validation = "";
            String DocDate;

            try
            {
                bImpresionOk = true;
                Cmpny = SBO_f.Cmpny;
                JsonText = "";
                orsLocal = (SAPbobsCOM.Recordset)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                oDocumento = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes));
                sMessage = "";

                //validar que exista procedimentos para tipo documento
                if (RunningUnderSQLServer)
                { s = "select ISNULL(U_ProcNomE,'') 'ProcNomE', ISNULL(U_ProcNomD,'') 'ProcNomD', ISNULL(U_ProcNomR,'') 'ProcNomR' from [@VID_FEPROCED] where ISNULL(U_Habili,'N') = 'Y' and U_TipoDocPE = '{0}'"; }
                else
                { s = @"select IFNULL(""U_ProcNomE"",'') ""ProcNomE"", IFNULL(""U_ProcNomD"",'') ""ProcNomD"", IFNULL(""U_ProcNomR"",'') ""ProcNomR"" from ""@VID_FEPROCED"" where IFNULL(""U_Habili"",'N') = 'Y' and ""U_TipoDocPE"" = '{0}'"; }

                s = String.Format(s, TipoDocElec);
                orsLocal.DoQuery(s);
                if (orsLocal.RecordCount == 0)
                {
                    //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimientos para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    throw new Exception("No se encuentra procedimientos para Documento electronico " + TipoDocElec);
                }
                else
                {
                    if ((System.String)(orsLocal.Fields.Item("ProcNomE").Value).ToString().Trim() == "")
                        //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de encabezado para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        throw new Exception("No se encuentra procedimiento de encabezado para Documento electronico " + TipoDocElec);
                    else if ((System.String)(orsLocal.Fields.Item("ProcNomD").Value).ToString().Trim() == "")
                        //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de detalle para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        throw new Exception("No se encuentra procedimiento de detalle para Documento electronico " + TipoDocElec);
                    else if ((System.String)(orsLocal.Fields.Item("ProcNomR").Value).ToString().Trim() == "")
                        //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de referencia para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        throw new Exception("No se encuentra procedimiento de referencia para Documento electronico " + TipoDocElec);

                    ProcNomE = (System.String)(orsLocal.Fields.Item("ProcNomE").Value).ToString().Trim();
                    ProcNomD = (System.String)(orsLocal.Fields.Item("ProcNomD").Value).ToString().Trim();
                    ProcNomR = (System.String)(orsLocal.Fields.Item("ProcNomR").Value).ToString().Trim();
                }


                if ((oDocumento.GetByKey(Convert.ToInt32(DocEntry))) && (bImpresionOk))
                {
                    if (RunningUnderSQLServer)
                        s = "exec " + ProcNomE + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
                    else
                        s = "CALL " + ProcNomE + "  (" + oDocumento.DocEntry + ", '" + TipoDocElec + "')";
                    //consulta por encabezado
                    orsLocal.DoQuery(s);
                    if (orsLocal.RecordCount > 0)
                    {
                        oNote_FM = new pe.facturamovil.Note();
                        oNote_FM.currency = ((System.String)orsLocal.Fields.Item("currency").Value).Trim();
                        oNote_FM.date = ((System.DateTime)orsLocal.Fields.Item("date").Value);

                        oNote_FM.series = ((System.String)orsLocal.Fields.Item("series").Value).Trim();
                        externalFolio = ((System.String)orsLocal.Fields.Item("externalFolio").Value).Trim();
                        oNote_FM.externalFolio = externalFolio;

                        var oNoteType = new pe.facturamovil.NoteType();
                        oNoteType.code = ((System.String)orsLocal.Fields.Item("noteType").Value).Trim();
                        oNoteType.isCredit = true;
                        oNote_FM.noteType = oNoteType;

                        var oClient = new pe.facturamovil.Client();
                        oClient.code = ((System.String)orsLocal.Fields.Item("code").Value).Trim();
                        oClient.name = ((System.String)orsLocal.Fields.Item("name").Value).Trim();
                        oClient.address = ((System.String)orsLocal.Fields.Item("address").Value).Trim();
                        var oDistrict = new pe.facturamovil.Municipality();
                        oDistrict.code = ((System.String)orsLocal.Fields.Item("municipality").Value).Trim();
                        oClient.municipality = oDistrict;
                        oClient.contact = ((System.String)orsLocal.Fields.Item("contact").Value).Trim();
                        oClient.phone = ((System.String)orsLocal.Fields.Item("phone").Value).Trim();

                        if (((System.String)orsLocal.Fields.Item("identityDocumentType").Value).Trim() != "")
                        {
                            var oid = new pe.facturamovil.IdentityDocumentType();
                            oid.code = ((System.String)orsLocal.Fields.Item("identityDocumentType").Value);
                            oClient.identityDocumentType = oid;
                        }

                        Email = ((System.String)orsLocal.Fields.Item("email").Value).Trim();
                        oClient.email = Email;
                        oNote_FM.client = oClient;
                        oNote_FM.expirationDate = ((System.DateTime)orsLocal.Fields.Item("expirationDate").Value);


                        try
                        {
                            var oAditional = new pe.facturamovil.AdditionalPrintInformation();
                            if (((System.String)orsLocal.Fields.Item("certificateNumber").Value).Trim() == "")
                                oAditional.certificateNumber = null;
                            else
                                oAditional.certificateNumber = ((System.String)orsLocal.Fields.Item("certificateNumber").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("contactP").Value).Trim() == "")
                                oAditional.contact = null;
                            else
                                oAditional.contact = ((System.String)orsLocal.Fields.Item("contactP").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("gloss").Value).Trim() == "")
                                oAditional.gloss = null;
                            else
                                oAditional.gloss = ((System.String)orsLocal.Fields.Item("gloss").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("project").Value).Trim() == "")
                                oAditional.project = null;
                            else
                                oAditional.project = ((System.String)orsLocal.Fields.Item("project").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("reference").Value).Trim() == "")
                                oAditional.reference = null;
                            else
                                oAditional.reference = ((System.String)orsLocal.Fields.Item("reference").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("account").Value).Trim() == "")
                                oAditional.account = null;
                            else
                                oAditional.account = ((System.String)orsLocal.Fields.Item("account").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("estimateNumber").Value).Trim() == "")
                                oAditional.estimateNumber = null;
                            else
                                oAditional.estimateNumber = ((System.String)orsLocal.Fields.Item("estimateNumber").Value).Trim();

                            oNote_FM.additionalPrintInformation = oAditional;
                        }
                        catch (Exception er)
                        {
                            SBO_f.oLog.OutLog("Error additionalPrintInformation - " + er.Message);
                        }

                        //DETALLE
                        if (RunningUnderSQLServer)
                            s = "exec " + ProcNomD + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
                        else
                            s = "CALL " + ProcNomD + "  (" + oDocumento.DocEntry + ", '" + TipoDocElec + "')";
                        //consulta por detalle
                        orsLocal.DoQuery(s);
                        if (orsLocal.RecordCount > 0)
                        {
                            oNote_FM.details = new List<pe.facturamovil.Detail>();
                            while (!orsLocal.EoF)
                            {
                                var oProduct = new pe.facturamovil.Product();
                                var oService = new pe.facturamovil.Service();

                                if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "I")
                                {

                                    oProduct.code = ((System.String)orsLocal.Fields.Item("code").Value).Trim();
                                    oProduct.name = ((System.String)orsLocal.Fields.Item("name").Value).Trim();

                                    var oUM = new pe.facturamovil.Unit();
                                    oUM.code = ((System.String)orsLocal.Fields.Item("unit").Value).Trim();
                                    oProduct.unit = oUM;
                                    oProduct.price = ((System.Double)orsLocal.Fields.Item("price").Value);

                                    var oIGV = new pe.facturamovil.ExemptType();
                                    oIGV.code = ((System.String)orsLocal.Fields.Item("exemptType").Value).Trim();
                                    oProduct.exemptType = oIGV;
                                }
                                else if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "S")
                                {
                                    oService.name = ((System.String)orsLocal.Fields.Item("name").Value).Trim();

                                    var oUM = new pe.facturamovil.Unit();
                                    oUM.code = ((System.String)orsLocal.Fields.Item("unit").Value).Trim();
                                    oService.unit = oUM;
                                    oService.price = ((System.Double)orsLocal.Fields.Item("price").Value);
                                    var oIGV = new pe.facturamovil.ExemptType();
                                    oIGV.code = ((System.String)orsLocal.Fields.Item("exemptType").Value).Trim();
                                    oService.exemptType = oIGV;
                                }

                                var oDetail = new pe.facturamovil.Detail();

                                oDetail.position = ((System.Int32)orsLocal.Fields.Item("idLine").Value);

                                if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "I")
                                {
                                    oDetail.product = oProduct;
                                    oDetail.quantity = float.Parse(((System.Double)orsLocal.Fields.Item("quantity").Value).ToString().Trim());
                                    oDetail.description = ((System.String)orsLocal.Fields.Item("description").Value).Trim();
                                }
                                else if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "S")
                                {
                                    oDetail.service = oService;
                                    oDetail.description = ((System.String)orsLocal.Fields.Item("description").Value).Trim();
                                    oDetail.quantity = float.Parse(((System.Double)orsLocal.Fields.Item("quantity").Value).ToString().Trim());
                                }

                                oDetail.longDescription = ((System.String)orsLocal.Fields.Item("longDescription").Value).Trim();

                                oNote_FM.details.Add(oDetail);

                                orsLocal.MoveNext();
                            }//fin agregar detalle a la cabecera


                            //REFERENCIAS
                            if (RunningUnderSQLServer)
                                s = "exec " + ProcNomR + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
                            else
                                s = "CALL " + ProcNomR + "  (" + oDocumento.DocEntry + ", '" + TipoDocElec + "')";
                            //consulta por referencia
                            orsLocal.DoQuery(s);
                            if (orsLocal.RecordCount > 0)
                            {
                                oNote_FM.references = new List<pe.facturamovil.Reference>();
                                while (!orsLocal.EoF)
                                {
                                    var oReference = new pe.facturamovil.Reference();
                                    oReference.position = ((System.Int32)orsLocal.Fields.Item("position").Value);
                                    var oDocType = new pe.facturamovil.DocumentType();
                                    oDocType.code = ((System.String)orsLocal.Fields.Item("documentType").Value).Trim();
                                    oReference.documentType = oDocType;
                                    oReference.referencedFolio = ((System.String)orsLocal.Fields.Item("referencedFolio").Value).Trim();
                                    oReference.date = ((System.DateTime)orsLocal.Fields.Item("date").Value);
                                    oReference.description = ((System.String)orsLocal.Fields.Item("description").Value).Trim();

                                    oNote_FM.references.Add(oReference);
                                    orsLocal.MoveNext();
                                }
                            }

                            //termina de cargar documento
                            JsonText = FacturaMovilGlobal.processor.getNoteJson(oNote_FM);
                            //oRecordSet.DoQuery("UPDATE [@OFMP] SET U_JSON='" + JsonText + "' WHERE DOCENTRY=1");

                            if (FacturaMovilGlobal.userConnected == null)
                            {
                                try
                                {
                                    LoginCount_FM = 0;
                                    //oUser_FM = new pe.facturamovil.User();
                                    if (oUserFM.token == null)
                                    {
                                        if (RunningUnderSQLServer)
                                            orsLocal.DoQuery("SELECT U_User,U_Pwd,U_CCEmail FROM [@VID_FEPARAM] WHERE Code = '1'");
                                        else
                                            orsLocal.DoQuery(@"SELECT ""U_User"", ""U_Pwd"", ""U_CCEmail"" FROM ""@VID_FEPARAM"" WHERE ""Code"" = '1'");
                                        oUserFM = FacturaMovilGlobal.processor.Authenticate(((System.String)orsLocal.Fields.Item("U_User").Value).Trim(), ((System.String)orsLocal.Fields.Item("U_Pwd").Value).Trim());
                                        FacturaMovilGlobal.userConnected = oUserFM;

                                        var ii = 0;
                                        var bExistePE = false;

                                        if (oUserFM.companies.Find(c => c.code.Trim() == lRUC.Trim()) != null)
                                        {
                                            FacturaMovilGlobal.selectedCompany = oUserFM.companies.Single(c => c.code.Trim() == lRUC.Trim());
                                            bExistePE = true;
                                            ii = oUserFM.companies.Count;
                                        }

                                        if (!bExistePE)
                                            throw new Exception("No se ha encontrado el RUC " + lRUC + "en la conexion de Factura Movil");

                                        CCEmail_FM = ((System.String)orsLocal.Fields.Item("U_CCEmail").Value).Trim();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //if (SBO_f.SBOApp.MessageBox("No se pudo establecer conexion con el servidor. Desea Continuar?", 2, "Si", "No") == 2)
                                    //{
                                    //    throw new Exception("Motivos de error en conexion : " + ex.Message);
                                    //}
                                    bImpresionOk = false;
                                    sMessage = "Motivos de error en conexion : " + ex.Message;
                                }
                            }

                            try
                            {
                                if (bImpresionOk)
                                {
                                    FacturaMovilGlobal.processor.sendNote(FacturaMovilGlobal.selectedCompany, oNote_FM, FacturaMovilGlobal.userConnected.token);
                                    Id = oNote_FM.id.ToString();
                                    Validation = oNote_FM.validation;
                                    //orsLocal.DoQuery("UPDATE OINV SET U_FM_MDFE='Y' WHERE NUMATCARD='" + NumAtCard + "' AND DOCSUBTYPE='--'")
                                    SBO_f.SBOApp.StatusBar.SetText("Nota de Credito emitida con exito.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    FacturaMovilGlobal.processor.showDocument(oNote_FM);

                                    if (Email != "")
                                    {
                                        SBO_f.SBOApp.StatusBar.SetText("Enviando documento via email. Porfavor Espere...", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        FacturaMovilGlobal.processor.sendEmail(FacturaMovilGlobal.selectedCompany, oNote_FM, Email, CCEmail_FM, FacturaMovilGlobal.userConnected.token);
                                        SBO_f.SBOApp.StatusBar.SetText("Nota de Credito emitida y enviada al cliente electronicamente con exito. Numero SUNAT : " + externalFolio, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                    else
                                        SBO_f.SBOApp.StatusBar.SetText("Factura emitida electronicamente con exito. Asegurese de enviar el documento al cliente. Numero SUNAT : " + externalFolio, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                }
                            }
                            catch (Exception ex)
                            {
                                SBO_f.SBOApp.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                SBO_f.oLog.OutLog("EnviarFE_PE " + ex.Message + " ** Trace: " + ex.StackTrace);
                                bImpresionOk = false;
                                sMessage = ex.Message;
                            }
                        }
                        else
                        {
                            SBO_f.SBOApp.StatusBar.SetText("No se encuentra Datos en detalle", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            bImpresionOk = false;
                        }

                    }
                    else
                    {
                        SBO_f.SBOApp.StatusBar.SetText("No se encuentra Datos en encabezado", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        bImpresionOk = false;
                    }

                }
                else
                {
                    SBO_f.SBOApp.StatusBar.SetText("Error - No se ha encontrado el documento", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    bImpresionOk = false;
                }

                DocDate = SBO_f.DateToStr(oDocumento.DocDate);
                if (!bImpresionOk)
                {
                    //SBO_f.SBOApp.MessageBox("Error envio documento electronico ");
                    if (sMessage != "")
                        SBO_f.SBOApp.StatusBar.SetText(sMessage, SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    else
                        SBO_f.SBOApp.StatusBar.SetText("Error envio documento electrónico (1)", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    ObjType = "14";
                    Status = "EE";
                    if (sMessage == "")
                        sMessage = "Error envio documento electronico a Factura Movil";
                }
                else
                {
                    Status = "EC";
                    ObjType = "14";
                    sMessage = "Enviado satisfactoriamente a Factura Movil";
                    SBO_f.SBOApp.StatusBar.SetText("Se ha enviado satisfactoriamente el documento electronico a Factura Movil", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    //oDocumento.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                    //actualizo campo de impresion para que no aparezca formulario solicitando folio
                    oDocumento.Printed = PrintStatusEnum.psYes;
                    lRetCode = oDocumento.Update();
                    if (lRetCode != 0)
                    {
                        s = SBO_f.Cmpny.GetLastErrorDescription();
                        SBO_f.SBOApp.StatusBar.SetText("Error actualizar documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        SBO_f.oLog.OutLog("Error actualizar Nota credito - " + s);
                    }
                }

                if (RunningUnderSQLServer)
                { s = "SELECT DocEntry, U_Status FROM [@VID_FELOG] WITH (NOLOCK) WHERE U_DocEntry = {0} AND U_ObjType = '{1}' AND U_SubType = '{2}'"; }
                else
                { s = @"SELECT ""DocEntry"", ""U_Status"" FROM ""@VID_FELOG"" WHERE ""U_DocEntry"" = {0} AND ""U_ObjType"" = '{1}' AND ""U_SubType"" = '{2}' "; }
                s = String.Format(s, DocEntry, ObjType, DocSubType);
                orsLocal.DoQuery(s);
                Reg = new TFunctions();
                Reg.SBO_f = SBO_f;

                if (sMessage.Length > 254)
                    sMessage = sMessage.Substring(0, 253);

                if (orsLocal.RecordCount == 0)
                    Reg.FELOGAdd(Int32.Parse(DocEntry), ObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, JsonText, Id, Validation, DocDate);
                else
                {
                    if ((System.String)(orsLocal.Fields.Item("U_Status").Value) != "RR")
                        Reg.FELOGUptM((System.Int32)(orsLocal.Fields.Item("DocEntry").Value), Int32.Parse(DocEntry), ObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, JsonText, Id, Validation, DocDate);
                    else
                        SBO_f.SBOApp.StatusBar.SetText("Documento ya se encuentra en Factura Movil y SUNAT", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }

            }
            catch (Exception e)
            {
                SBO_f.SBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                SBO_f.oLog.OutLog("EnviarFE_PE " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }


        //Para PEru EasyDot
        public new void EnviarCN_PE_ED(String DocEntry, Boolean RunningUnderSQLServer, String SeriePE, String FolioNum, String TipoDocElec, String sObjType, String DocSubType, String lRUC, SqlConnection MConexionADO, String Local, String TipoDocElecAddon)
        {
            String URL;
            String Procedimiento;
            XmlDocument oXml = null;
            String userED;
            String passED;
            TFunctions Reg = new TFunctions();
            SAPbobsCOM.Company Cmpny = SBO_f.Cmpny;
            Reg.SBO_f = SBO_f;
            String Status;
            String sMessage = "";
            Int32 lRetCode;
            String DocDate = "";
            SqlCommand comando1 = new SqlCommand();
            System.Data.DataTable rTable;
            SqlDataAdapter adapter;
            SAPbobsCOM.Recordset ors = ((SAPbobsCOM.Recordset)Cmpny.GetBusinessObject(BoObjectTypes.BoRecordset));
            try
            {
                if (ConexionADO == null)
                    ConexionADO = MConexionADO;

                if (RunningUnderSQLServer)
                    s = @"SELECT U_URLEasyDot 'URL', ISNULL(U_UserED,'') 'User', ISNULL(U_PwdED,'') 'Pass' FROM [@VID_FEPARAM]";
                else
                    s = @"SELECT ""U_URLEasyDot"" ""URL"", IFNULL(""U_UserED"",'') ""User"", IFNULL(""U_PwdED"",'') ""Pass"" FROM ""@VID_FEPARAM"" ";

                ors.DoQuery(s);
                if (ors.RecordCount == 0)
                    SBO_f.SBOApp.StatusBar.SetText("No se ha ingresado URL", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                else if (((System.String)ors.Fields.Item("URL").Value).Trim() == "")
                    SBO_f.SBOApp.StatusBar.SetText("No se ha ingresado URL", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                else if (((System.String)ors.Fields.Item("User").Value).Trim() == "")
                    throw new Exception("No se encuentra usuario en Parametros");
                else if (((System.String)ors.Fields.Item("Pass").Value).Trim() == "")
                    throw new Exception("No se encuentra password en Parametros");
                else
                {
                    userED = Reg.DesEncriptar((System.String)(ors.Fields.Item("User").Value).ToString().Trim());
                    passED = Reg.DesEncriptar((System.String)(ors.Fields.Item("Pass").Value).ToString().Trim());

                    URL = ((System.String)ors.Fields.Item("URL").Value).Trim() + "/SendDocument.ashx";
                    //validar que exista procedimentos para tipo documento
                    if (RunningUnderSQLServer)
                        s = "select ISNULL(U_ProcNomE,'') 'ProcNomE' FROM [@VID_FEPROCED] where ISNULL(U_Habili,'N') = 'Y' and U_TipoDocPE = '{0}'";
                    else
                        s = @"select IFNULL(""U_ProcNomE"",'') ""ProcNomE"" FROM ""@VID_FEPROCED"" where IFNULL(""U_Habili"",'N') = 'Y' and ""U_TipoDocPE"" = '{0}'";

                    s = String.Format(s, TipoDocElec);
                    ors.DoQuery(s);
                    if (ors.RecordCount == 0)
                        throw new Exception("No se encuentra procedimientos para Documento electronico " + TipoDocElec);
                    else
                    {
                        Procedimiento = ((System.String)ors.Fields.Item("ProcNomE").Value).Trim();
                        if (RunningUnderSQLServer)
                            s = @"exec {0} {1}, '{2}', '{3}'";//Nota de Credito
                        else
                            s = @"call {0} ({1}, '{2}', '{3}')";//Nota de Credito
                        s = String.Format(s, Procedimiento, DocEntry, TipoDocElec, sObjType);
                        if (ConexionADO.State == ConnectionState.Closed)
                            ConexionADO.Open();

                        comando1.Connection = ConexionADO;
                        comando1.CommandText = s;
                        rTable = new System.Data.DataTable();
                        adapter = new SqlDataAdapter(comando1);
                        adapter.Fill(rTable);
                        if (rTable.Rows.Count > 0)
                        {
                            var i = 0;
                            foreach (DataRow row in rTable.Rows)
                            {
                                if (i == 0)
                                    s = row[0].ToString().Trim();
                                else
                                    s += row[0].ToString().Trim();
                                i++;
                            }
                        }

                        if (ConexionADO.State == ConnectionState.Open)
                            ConexionADO.Close();

                        if (((System.String)ors.Fields.Item(0).Value).Trim() == "")
                            throw new Exception("No se encuentra datos para Documento electronico " + TipoDocElec);
                        else
                        {
                            var bImpresion = false;
                            oXml = new XmlDocument();
                            oXml.LoadXml(s);

                            //obtiene string de pdf
                            s = Reg.PDFenString(TipoDocElecAddon, DocEntry, sObjType, SeriePE, FolioNum, RunningUnderSQLServer, Local);

                            if (s == "")
                                throw new Exception("No se ha creado PDF");

                            //Agrega el PDF al xml
                            XmlNode node;
                            if (oXml.SelectSingleNode("//CamposExtras") == null)
                                node = oXml.CreateNode(XmlNodeType.Element, "CamposExtras", null);
                            else
                                node = oXml.SelectSingleNode("//CamposExtras");

                            XmlNode nodePDF = oXml.CreateElement("PDF");
                            nodePDF.InnerText = s;
                            node.AppendChild(nodePDF);
                            oXml.DocumentElement.AppendChild(node);

                            s = Reg.UpLoadDocumentByUrl(oXml, RunningUnderSQLServer, URL, userED, passED);
                            //SBO_f.SBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);

                            oXml.LoadXml(s);
                            //var Configuracion = oXml.GetElementsByTagName("Error");
                            var lista = ((XmlElement)oXml.GetElementsByTagName("Error")[0]).GetElementsByTagName("ErrorText");
                            var ErrorText = lista[0].InnerText;
                            lista = ((XmlElement)oXml.GetElementsByTagName("Error")[0]).GetElementsByTagName("ErrorCode");
                            var ErrorCode = lista[0].InnerText;

                            if (ErrorCode != "0")
                            {
                                SBO_f.SBOApp.StatusBar.SetText("Error envio documento electrónico (1)", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                //sObjType = "13";
                                if (ErrorCode == "-103")
                                    Status = "RR";
                                else
                                    Status = "EE";
                                sMessage = ErrorText;
                                if (sMessage == "")
                                    sMessage = "Error envio documento electronico a EasyDot";
                            }
                            else
                            {
                                Status = "RR";
                                //sObjType = "13";
                                sMessage = "Enviado satisfactoriamente a EasyDot y Aceptado";
                                SBO_f.SBOApp.StatusBar.SetText("Se ha enviado satisfactoriamente el documento electronico", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                var oDocumento = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices));
                                if (oDocumento.GetByKey(Convert.ToInt32(DocEntry)))
                                {
                                    DocDate = SBO_f.DateToStr(oDocumento.DocDate);
                                    oDocumento.Printed = PrintStatusEnum.psYes;
                                    lRetCode = oDocumento.Update();
                                    if (lRetCode != 0)
                                    {
                                        s = SBO_f.Cmpny.GetLastErrorDescription();
                                        SBO_f.SBOApp.StatusBar.SetText("Error actualizar documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                        sMessage = "Error actualizar documento - " + s;
                                        //SBO_f.oLog.OutLog("Error actualizar Nota debito - " + s);
                                    }
                                    else
                                        bImpresion = true;
                                }
                                else
                                {
                                    sMessage = "No se ha encontrado documento al actualizar Impresion";
                                    bImpresion = false;
                                }
                            }
                            oXml = null;

                            if (RunningUnderSQLServer)
                                s = "SELECT DocEntry, U_Status FROM [@VID_FELOG] WITH (NOLOCK) WHERE U_DocEntry = {0} AND U_ObjType = '{1}' AND U_SubType = '{2}'";
                            else
                                s = @"SELECT ""DocEntry"", ""U_Status"" FROM ""@VID_FELOG"" WHERE ""U_DocEntry"" = {0} AND ""U_ObjType"" = '{1}' AND ""U_SubType"" = '{2}' ";
                            s = String.Format(s, DocEntry, sObjType, DocSubType);
                            ors.DoQuery(s);
                            Reg = new TFunctions();
                            Reg.SBO_f = SBO_f;
                            if (ors.RecordCount == 0)
                                Reg.FELOGAdd(Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", ErrorCode, ErrorText, DocDate);
                            else
                            {
                                if ((System.String)(ors.Fields.Item("U_Status").Value) != "RR")
                                {
                                    SBO_f.SBOApp.StatusBar.SetText("Documento se ha enviado a EasyDot", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                    Reg.FELOGUptM((System.Int32)(ors.Fields.Item("DocEntry").Value), Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", ErrorCode, ErrorText, DocDate);
                                }
                                else
                                    SBO_f.SBOApp.StatusBar.SetText("Documento ya se ha enviado anteriormente a EasyDot y se encuentra en Sunat", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                            }
                        }
                    }
                }

            }
            catch (Exception x)
            {
                SBO_f.SBOApp.StatusBar.SetText("EnviarCN_PE_ED: " + x.Message + " ** Trace: " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                SBO_f.oLog.OutLog("EnviarCN_PE_ED: " + x.Message + " ** Trace: " + x.StackTrace);
            }
            finally
            {
                SBO_f._ReleaseCOMObject(ors);
                SBO_f._ReleaseCOMObject(oXml);
                if (ConexionADO.State == ConnectionState.Open)
                    ConexionADO.Close();
            }
        }


        private Boolean ValidarDatosFE()
        {
            Boolean _result;
            SAPbouiCOM.DBDataSource oDBDSDir;
            SAPbouiCOM.DBDataSource oDBDSH;
            SAPbouiCOM.Matrix oMatrixRef;
            TFunctions Param;
            Boolean DocElec;
            Int32 c, i;
            String BaseEntry;
            SAPbouiCOM.Matrix oMatrix;
            Boolean PedirRefCab;
            String Tabla;
            SAPbouiCOM.EditText oEditText;
            SAPbouiCOM.EditText oEditTextB;
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
            String TipoDocElect;
            String TipoDocRef;
            Boolean bMultiSoc;
            String nMultiSoc;

            try
            {
                _result = true;
                if (ObjType == "19")
                {
                    oDBDSDir = oForm.DataSources.DBDataSources.Item("RPC12");
                    oDBDSH = oForm.DataSources.DBDataSources.Item("ORPC");
                }
                else
                {
                    oDBDSDir = oForm.DataSources.DBDataSources.Item("RIN12");
                    oDBDSH = oForm.DataSources.DBDataSources.Item("ORIN");
                }

                //TipoDocRef = ((System.String)oDBDSH.GetValue("U_TipoRef", 0)).Trim();
                TipoDocRef = ((System.String)oDBVID_FEREFD.GetValue("U_TipoDTE", 0)).Trim();

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select ISNULL(T0.U_MultiSoc,'N') MultiSoc, ISNULL(T0.U_GenerarT,'N') GeneraT from [@VID_FEPARAM] T0";
                else
                    s = @"select IFNULL(T0.""U_MultiSoc"",'N') ""MultiSoc"", IFNULL(T0.""U_GenerarT"",'N') ""GeneraT"" from ""@VID_FEPARAM"" T0 ";
                oRecordSet.DoQuery(s);
                if ((System.String)(oRecordSet.Fields.Item("MultiSoc").Value) == "Y")
                    bMultiSoc = true;
                else
                    bMultiSoc = false;


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

                s = String.Format(s, (System.String)(oDBDSH.GetValue("Series", 0)).Trim(), ObjType);
                oRecordSet.DoQuery(s);

                nMultiSoc = (System.String)(oRecordSet.Fields.Item("Inst").Value);

                if ((System.String)(oRecordSet.Fields.Item("Tipo").Value) == "E")
                    DocElec = true;
                else
                    DocElec = false;

                if (DocElec)
                {
                    if ((System.String)(oRecordSet.Fields.Item("TipoDocElect").Value) == "112")
                        TipoDocElect = "112";
                    else
                        TipoDocElect = "61";


                    if ((System.String)(oDBDSDir.GetValue("CityB", 0)).Trim() == "")
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    if (((System.String)(oDBDSDir.GetValue("CityS", 0)).Trim() == "") && (_result) && (TipoDocRef != "39") && (TipoDocRef != "41"))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    if (((System.String)(oDBDSDir.GetValue("CountyB", 0)).Trim() == "") && (_result))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    if (((System.String)(oDBDSDir.GetValue("CountyS", 0)).Trim() == "") && (_result) && (TipoDocRef != "39") && (TipoDocRef != "41"))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    if (((System.String)(oDBDSDir.GetValue("StreetB", 0)).Trim() == "") && (_result))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    if (((System.String)(oDBDSDir.GetValue("StreetS", 0)).Trim() == "") && (_result) && (TipoDocRef != "39") && (TipoDocRef != "41"))
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

                    //valida rut
                    if ((_result) && (TipoDocElect != "112"))
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

                    var sDocSubType = (System.String)(oDBDSH.GetValue("DocSubType", 0)).Trim();


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
                            if (!bMultiSoc)
                            {
                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = @"select REPLACE(REPLACE(A0.TaxIdNum,'.',''),'-','') TaxIdNum
                                            from OADM A0";
                                else
                                    s = @"select REPLACE(REPLACE(A0.""TaxIdNum"",'.',''),'-','') ""TaxIdNum""
                                            from ""OADM"" A0";
                            }
                            else
                            {
                                if (GlobalSettings.RunningUnderSQLServer)
                                    s = @"SELECT REPLACE(REPLACE(U_RUT,'.',''),'-','') TaxIdNum FROM [@VID_FEMULTISOC] WHERE DocEntry = {0}";
                                else
                                    s = @"SELECT REPLACE(REPLACE(""U_RUT"",'.',''),'-','') ""TaxIdNum"" FROM ""@VID_FEMULTISOC"" WHERE ""DocEntry"" = {0}";
                                s = String.Format(s, nMultiSoc);
                            }
                            oRecordSet.DoQuery(s);
                            var TaxIdNum = (System.String)(oRecordSet.Fields.Item("TaxIdNum").Value).ToString().Trim();
                            if (TaxIdNum == "")
                            {
                                if (!bMultiSoc)
                                    throw new Exception("Debe ingresar RUT de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1");
                                else
                                    throw new Exception("Debe ingresar RUT de Emisor, Gestión -> Definiciones -> Facturación Electrónica -> Multiples bases FE");
                            }
                        }

                    }

                    //valida descuentos negativos en el detalle del documento
                    if (_result)
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = "select ISNULL(U_ValDescL,'Y') 'ValDescL' from [@VID_FEPARAM]";
                            s1 = "select ISNULL(U_CantLineas,0) CantLineas from [@VID_FEPROCED] where U_TipoDoc = '" + TipoDocElect + "' and U_Habili = 'Y'";
                        }
                        else
                        {
                            s = @"select IFNULL(""U_ValDescL"",'Y') ""ValDescL"" from ""@VID_FEPARAM"" ";
                            s1 = @"select IFNULL(""U_CantLineas"",0) ""CantLineas"" from ""@VID_FEPROCED"" where ""U_TipoDoc"" = '" + TipoDocElect + @"' and ""U_Habili"" = 'Y'";
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
                            { mtx = (Matrix)(oForm.Items.Item("39").Specific); }
                            else
                            { mtx = (Matrix)(oForm.Items.Item("38").Specific); }

                            var ValDescL = (System.String)(oRecordSet.Fields.Item("ValDescL").Value);
                            i = 1;
                            var cantlin = 0;

                            i = 1;
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
                                    {
                                        oEditText = (EditText)(mtx.Columns.Item("6").Cells.Item(i).Specific);
                                    }
                                    else
                                    {
                                        oEditText = (EditText)(mtx.Columns.Item("15").Cells.Item(i).Specific);
                                    }

                                    if ((Convert.ToDouble(((SAPbouiCOM.EditText)(oEditText)).String.Replace(",", "."), _nf) < 0) && (TipoLinea == ""))
                                    {
                                        s = "Descuento negativo en la linea " + Convert.ToString(i);
                                        FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        _result = false;
                                        i = mtx.RowCount;
                                    }
                                }

                                if (_result)
                                {
                                    if (TipoDoc == "S")
                                    { oEditText = (EditText)(mtx.Columns.Item("1").Cells.Item(i).Specific); }
                                    else
                                    { oEditText = (EditText)(mtx.Columns.Item("3").Cells.Item(i).Specific); }
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
                                }

                                i++;
                            }
                            if ((cantlin > CantLineas) && (((System.String)oDBDSH.GetValue("SummryType", 0)).Trim() == "N")) //valida total de lineas solo cuando no es resumen
                            {
                                FSBOApp.StatusBar.SetText("Cantidad de lineas supera lo permitido, parametrización FE", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                return false;
                            }
                            //oRecordSet.MoveNext();
                        }
                    }


                    //valida para nota credito
                    if (_result)
                    {
                        if (((System.String)oForm.DataSources.UserDataSources.Item("CodRef").Value).Trim() == "")//**
                        {
                            FSBOApp.StatusBar.SetText("Debe seleccionar Código Referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else
                        {
                            c = 0;
                            if (oDBDSH.GetValue("DocType", 0) == "S")
                            {
                                oMatrix = (Matrix)(oForm.Items.Item("39").Specific);
                                oEditText = (EditText)(oMatrix.Columns.Item("25").Cells.Item(1).Specific);
                            }
                            else
                            {
                                oMatrix = (Matrix)(oForm.Items.Item("38").Specific);
                                oEditText = (EditText)(oMatrix.Columns.Item("45").Cells.Item(1).Specific);
                            }

                            BaseEntry = oEditText.Value;
                            PedirRefCab = false;

                            i = 1;
                            while (i <= oMatrix.RowCount - 1)
                            {
                                if (oDBDSH.GetValue("DocType", 0) != "S")
                                {
                                    //para articulo
                                    oComboBox = (ComboBox)(oMatrix.Columns.Item("257").Cells.Item(i).Specific);
                                    if (oComboBox.Value == "")
                                    {
                                        oEditText = (EditText)(oMatrix.Columns.Item("45").Cells.Item(i).Specific);//BaseEntry
                                        oEditTextB = (EditText)(oMatrix.Columns.Item("43").Cells.Item(i).Specific); //basetype

                                        if (BaseEntry != oEditText.Value)
                                            c = c + 1;

                                        if ((oEditText.Value == "") || ((((oEditTextB.Value != "13") && (oEditTextB.Value != "203")) && (ObjType == "14")) || (((oEditTextB.Value != "18") && (oEditTextB.Value != "204")) && (ObjType == "19"))))
                                            PedirRefCab = true;
                                    }
                                }
                                else
                                {
                                    //para servicio
                                    oEditText = (EditText)(oMatrix.Columns.Item("25").Cells.Item(i).Specific);
                                    oEditTextB = (EditText)(oMatrix.Columns.Item("23").Cells.Item(i).Specific);  //baseType

                                    if (BaseEntry != oEditText.Value)
                                        c = c + 1;

                                    if ((oEditText.Value == "") || ((((oEditTextB.Value != "13") && (oEditTextB.Value != "203")) && (ObjType == "14")) || (((oEditTextB.Value != "18") && (oEditTextB.Value != "204")) && (ObjType == "19"))))
                                        PedirRefCab = true;
                                }

                                i++;
                            }

                            if ((c > 0) && (((System.String)oForm.DataSources.UserDataSources.Item("CodRef").Value).Trim() != "3"))
                            {
                                FSBOApp.StatusBar.SetText("Nota de credito solo debe tener una Factura de referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                _result = false;
                            }
                            else
                            {
                                if (PedirRefCab)
                                {
                                    if (1 == 1)
                                    {
                                        var bDocTotal = false;
                                        oMatrixRef = ((Matrix)oForm.Items.Item("mtxRefFE").Specific);
                                        oMatrixRef.FlushToDataSource();
                                        var CantDoc = 0;
                                        var TotalDoc = 0.0;
                                        for (Int32 iLin = 0; iLin < oMatrixRef.RowCount; iLin++)
                                        {
                                            if ((((System.String)oDBVID_FEREFD.GetValue("U_DocFolio", iLin)) != "0") && (((System.String)oDBVID_FEREFD.GetValue("U_TipoDTE", iLin)).Trim() != "00") && (((System.String)oDBVID_FEREFD.GetValue("U_DocFolio", iLin)) != ""))
                                            {
                                                TotalDoc = TotalDoc + FSBOf.StrToDouble(((System.String)oDBVID_FEREFD.GetValue("U_DocTotal", iLin)));
                                                s = (System.String)(oDBVID_FEREFD.GetValue("U_TipoDTE", iLin)).Trim();
                                                if (s.IndexOf("b") == -1)
                                                {
                                                    bDocTotal = true;
                                                    if (ObjType == "19")
                                                    {
                                                        if (((System.String)oDBVID_FEREFD.GetValue("U_TipoDTE", iLin)).Trim() == "46a")
                                                            Tabla = "ODPO";
                                                        else
                                                            Tabla = "OPCH";

                                                        if (GlobalSettings.RunningUnderSQLServer)
                                                            s = @"SELECT COUNT(*) 'Cont'
                                                                FROM {0} T1 WITH (NOLOCK)
                                                                JOIN NNM1 T2 WITH (NOLOCK) ON T1.Series = T2.Series
                                                               WHERE ISNULL(T1.FolioNum, -1) = {1}
                                                                 AND CASE 
                                                                       WHEN '{2}' = '46'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                                                                       WHEN '{2}' = '46a'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                            	                                       WHEN '{2}' = '45'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                                                                       Else '-1'
                                                                     END = T1.DocSubType";
                                                        else
                                                            s = @"SELECT COUNT(*) ""Cont""
                                                            FROM ""{0}"" T1
                                                            JOIN ""NNM1"" T2 ON T1.""Series"" = T2.""Series""
                                                           WHERE IFNULL(T1.""FolioNum"", -1) = {1}
                                                             AND CASE
                                                                   WHEN '{2}' = '46'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                                                                   WHEN '{2}' = '46a'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                            	                                   WHEN '{2}' = '45'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                                                                   Else '-1'
                                                                 END = T1.""DocSubType"" ";
                                                        s = String.Format(s, Tabla, ((System.String)oDBVID_FEREFD.GetValue("U_DocFolio", iLin)), ((System.String)oDBVID_FEREFD.GetValue("U_TipoDTE", iLin)).Trim(), bMultiSoc == true ? "Y" : "N", nMultiSoc);
                                                        oRecordSet.DoQuery(s);
                                                        if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                                                        {
                                                            _result = true;
                                                            CantDoc++;
                                                        }
                                                        else
                                                        {
                                                            FSBOApp.StatusBar.SetText("No se ha encontrado documento de referencia,", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                                            _result = false;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (((System.String)oDBVID_FEREFD.GetValue("U_TipoDTE", iLin)).Trim() == "33a")
                                                            Tabla = "ODPI";
                                                        else
                                                            Tabla = "OINV";

                                                        if (GlobalSettings.RunningUnderSQLServer)
                                                            s = @"SELECT COUNT(*) 'Cont'
                                                                FROM {0} T1 WITH (NOLOCK)
                                                                JOIN NNM1 T2 WITH (NOLOCK) ON T1.Series = T2.Series
                                                               WHERE ISNULL(T1.FolioNum, -1) = {1}
                                                                 AND CASE 
                                                                       WHEN '{2}' = '33'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                                                                       WHEN '{2}' = '33a'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                            	                                       WHEN '{2}' = '39'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') AND ('{3}' = 'N') THEN 'IB' --para no multibase
				                                                       WHEN '{2}' = '41'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') AND ('{3}' = 'N') THEN 'EB' --para no multibase
				                                                       WHEN '{2}' = '39'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') AND ('{3}' = 'Y') AND SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) = '{4}' THEN 'IB' --
                                                                       WHEN '{2}' = '41'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') AND ('{3}' = 'Y') AND SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) = '{4}' THEN 'EB' --
                            	                                       WHEN '{2}' = '110' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'IX'
                            	                                       WHEN '{2}' = '34'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'IE'
                            	                                       WHEN '{2}' = '56'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'DN'
                                                                       WHEN '{2}' = '61'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                                                                       WHEN '{2}' = '111' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN 'DN'
                                                                       WHEN '{2}' = '112' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) = 'E') THEN '--'
                                                                       WHEN '{2}' = '30'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                                                                       WHEN '{2}' = '32'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'IE'
                                                                       WHEN '{2}' = '35'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'IB'
                            	                                       WHEN '{2}' = '38'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'BE'
                                                                       WHEN '{2}' = '55'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'DN'
                                                                       WHEN '{2}' = '60'  AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                                                                       WHEN '{2}' = '101' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'IX'
                                                                       WHEN '{2}' = '104' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN 'DN'
                                                                       WHEN '{2}' = '106' AND (SUBSTRING(UPPER(ISNULL(T2.BeginStr,'')), 1, 1) <> 'E') THEN '--'
                            	                                       Else '-1'
                                                                     END = T1.DocSubType";
                                                        else
                                                            s = @"SELECT COUNT(*) ""Cont""
                                                                FROM ""{0}"" T1
                                                                JOIN ""NNM1"" T2 ON T1.""Series"" = T2.""Series""
                                                               WHERE IFNULL(T1.""FolioNum"", -1) = {1}
                                                                 AND CASE
                                                                       WHEN '{2}' = '33'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                                                                       WHEN '{2}' = '33a' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                            	                                       WHEN '{2}' = '39'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') AND ('{3}' = 'N') THEN 'IB' --para no multibase
				                                                       WHEN '{2}' = '41'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') AND ('{3}' = 'N') THEN 'EB' --para no multibase
				                                                       WHEN '{2}' = '39'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') AND ('{3}' = 'Y') AND SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) = '{4}' THEN 'IB' --
                                                                       WHEN '{2}' = '41'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') AND ('{3}' = 'Y') AND SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) = '{4}' THEN 'EB' --
                            	                                       WHEN '{2}' = '110' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'IX'
                            	                                       WHEN '{2}' = '34'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'IE'
                            	                                       WHEN '{2}' = '56'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'DN'
                                                                       WHEN '{2}' = '61'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                                                                       WHEN '{2}' = '111' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN 'DN'
                                                                       WHEN '{2}' = '112' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) = 'E') THEN '--'
                                                                       WHEN '{2}' = '30'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                                                                       WHEN '{2}' = '32'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'IE'
                                                                       WHEN '{2}' = '35'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'IB'
                            	                                       WHEN '{2}' = '38'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'BE'
                                                                       WHEN '{2}' = '55'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'DN'
                                                                       WHEN '{2}' = '60'  AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                                                                       WHEN '{2}' = '101' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'IX'
                                                                       WHEN '{2}' = '104' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN 'DN'
                                                                       WHEN '{2}' = '106' AND (SUBSTRING(UPPER(IFNULL(T2.""BeginStr"",'')), 1, 1) <> 'E') THEN '--'
                            	                                       Else '-1'
                                                                     END = T1.""DocSubType"" ";
                                                        s = String.Format(s, Tabla, ((System.String)oDBVID_FEREFD.GetValue("U_DocFolio", iLin)), ((System.String)oDBVID_FEREFD.GetValue("U_TipoDTE", iLin)).Trim(), bMultiSoc == true ? "Y" : "N", nMultiSoc);
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
                                                    }
                                                }
                                                else
                                                {
                                                    s = oDBVID_FEREFD.GetValue("U_DocFolio", iLin).ToString();
                                                    var sc = "";
                                                    if (oDBVID_FEREFD.GetValue("U_DocDate", iLin) != null)
                                                        sc = ((System.String)oDBVID_FEREFD.GetValue("U_DocDate", iLin));
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
                                        else if ((CantDoc > 1) && (((System.String)oForm.DataSources.UserDataSources.Item("CodRef").Value).Trim() == "1"))
                                        {
                                            FSBOApp.StatusBar.SetText("Nota de credito solo debe tener una Factura de referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                            _result = false;
                                        }
                                        else if ((FSBOf.StrToDouble(oDBDSH.GetValue("DocTotal", 0)) > TotalDoc) && (((System.String)oForm.DataSources.UserDataSources.Item("CodRef").Value).Trim() != "3") && (bDocTotal))
                                        {
                                            FSBOApp.StatusBar.SetText("Total del documento Nota de Crédito no puede ser mayor al total de las facturas de venta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                            _result = false;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //valida para folio Distribuido
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select ISNULL(U_Distrib,'N') 'Distribuido' from [@VID_FEPARAM]";
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

                            s = String.Format(s, TipoDocElect);
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
        }//fin ValdarDatosFE

        private Boolean ValidarDatosFE_PE()
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


            try
            {
                _result = true;
                oDBDSDir = oForm.DataSources.DBDataSources.Item("RIN12");
                oDBDSH = oForm.DataSources.DBDataSources.Item("ORIN");

                var sDocSubType = (System.String)(oDBDSH.GetValue("DocSubType", 0)).Trim();

                if (sDocSubType == "--") //Nota de Credito
                    TipoDocElec = "07";


                if ((TipoDocElec == "07"))
                {
                    if ((System.String)(oDBDSDir.GetValue("CityB", 0)).Trim() == "")
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    //if (((System.String)(oDBDSDir.GetValue("CityS", 0)).Trim() == "") && (_result))
                    //{
                    //    FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    //    _result = false;
                    //}

                    if (((System.String)(oDBDSDir.GetValue("BlockB", 0)).Trim() == "") && (_result))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    //if (((System.String)(oDBDSDir.GetValue("CountyS", 0)).Trim() == "") && (_result))
                    //{
                    //    FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    //    _result = false;
                    //}

                    if (((System.String)(oDBDSDir.GetValue("StreetB", 0)).Trim() == "") && (_result))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    //if (((System.String)(oDBDSDir.GetValue("StreetS", 0)).Trim() == "") && (_result))
                    //{
                    //    FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    //    _result = false;
                    //}

                    s = (System.String)(oDBDSH.GetValue("CardName", 0)).Trim();
                    if ((s == "") && (_result))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar Nombre Cliente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    ////valida RUC
                    //se deja comentado, por problemas en la validacion de un cliente, Jimmy colocara una validacion en el TN 20151204
                    //if (_result)
                    //{
                    //    Param = new TFunctions();
                    //    Param.SBO_f = FSBOf;
                    //    s = Param.ValidarRuc((System.String)(oDBDSH.GetValue("LicTradNum", 0)));
                    //    if (s != "OK")
                    //    {
                    //        FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    //        _result = false;
                    //    }
                    //}

                    if (_result)
                    {

                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            //s = "select ISNULL(U_ValDescL,'Y') 'ValDescL' from [@VID_FEPARAM]";
                            s1 = "select ISNULL(U_CantLineas,0) CantLineas from [@VID_FEPROCED] where U_TipoDocPE = '" + TipoDocElec + "' and U_Habili = 'Y'";
                        }

                        else
                        {
                            //s = @"select IFNULL(""U_ValDescL"",'Y') ""ValDescL"" from ""@VID_FEPARAM"" ";
                            s1 = @"select IFNULL(""U_CantLineas"",0) ""CantLineas"" from ""@VID_FEPROCED"" where ""U_TipoDocPE"" = '" + TipoDocElec + @"' and ""U_Habili"" = 'Y'";
                        }

                        oRecordSet.DoQuery(s1);
                        if (oRecordSet.RecordCount > 0)
                            CantLineas = (System.Int32)(oRecordSet.Fields.Item("CantLineas").Value);
                        else
                        {
                            FSBOApp.StatusBar.SetText("Debe parametrizar el maximo de lineas para documento " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }

                        oComboBox = (ComboBox)(oForm.Items.Item("3").Specific);
                        TipoDoc = oComboBox.Selected.Value.Trim();
                        if (TipoDoc == "S")
                            mtx = (Matrix)(oForm.Items.Item("39").Specific);
                        else
                            mtx = (Matrix)(oForm.Items.Item("38").Specific);


                        if ((mtx.RowCount - 1 > CantLineas) && (((System.String)oDBDSH.GetValue("SummryType", 0)).Trim() == "N")) //valida total de lineas solo cuando no es resumen
                        {
                            FSBOApp.StatusBar.SetText("Cantidad de lineas supera lo permitido, parametrización FE", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }

                        i = 1;
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

                            //if ((System.String)(oRecordSet.Fields.Item("ValDescL").Value) == "Y")
                            //{
                            //    if (TipoDoc == "S") //System.String(oDBDSH.GetValue("DocType",0)).Trim()
                            //    {
                            //        oEditText = (EditText)(mtx.Columns.Item("6").Cells.Item(i).Specific);
                            //    }
                            //    else
                            //    {
                            //        oEditText = (EditText)(mtx.Columns.Item("15").Cells.Item(i).Specific);
                            //    }

                            //    if ((Convert.ToDouble((System.String)(oEditText.Value)) < 0) && (TipoLinea == ""))
                            //    {
                            //        s = "Descuento negativo en la linea " + Convert.ToString(i);
                            //        FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            //        _result = false;
                            //        i = mtx.RowCount;
                            //    }
                            //}

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

                            }

                            i++;
                        }


                        //validacion solo para nota de debito
                        if (TipoDocElec == "07")
                        {
                            //Validacion tipo de operacion
                            if ((System.String)(oDBDSH.GetValue("U_BPP_MDTN", 0)).Trim() == "")
                            {
                                FSBOApp.StatusBar.SetText("Debe ingresar tipo de operacion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                return false;
                            }
                            else
                            {
                                if (GlobalSettings.RunningUnderSQLServer)
                                {
                                    s = @"select U_TypeCode
                                            from [@FM_NOTES] 
                                           where Code = '{0}' ";
                                }
                                else
                                {
                                    s = @"select ""U_TypeCode""
                                            from ""@FM_NOTES""
                                           where ""Code"" = '{0}' ";
                                }
                                s = String.Format(s, (System.String)(oDBDSH.GetValue("U_BPP_MDTN", 0)).Trim());
                                oRecordSet.DoQuery(s);
                                if (oRecordSet.RecordCount == 0)
                                {
                                    FSBOApp.StatusBar.SetText("No se encuentra tipo de operacion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    return false;
                                }
                                //else if (((System.String)(oRecordSet.Fields.Item("Distribuido").Value)).Trim() != "02")
                                else if (((System.String)(oDBDSH.GetValue("U_BPP_MDTN", 0)).Trim() == "11") || ((System.String)(oDBDSH.GetValue("U_BPP_MDTN", 0)).Trim() == "10") || ((System.String)(oDBDSH.GetValue("U_BPP_MDTN", 0)).Trim() == "04"))
                                {
                                    FSBOApp.StatusBar.SetText("Debe seleccionar tipo de operacion valida por FM", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    return false;
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
                OutLog("ValidarDatosFE_PE " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }

        }

    }//fin class
}
