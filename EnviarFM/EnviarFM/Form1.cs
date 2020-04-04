using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Xml;
using System.IO;
using System.Net;
using ServiceStack.Text;
using System.Net.Http;
using System.Net.NetworkInformation;
using EnviarFM.Functions;
using pe.facturamovil;
using SAPbobsCOM;

namespace EnviarFM
{
    public partial class Form1 : Form
    {
        public SAPbobsCOM.CompanyClass oCompany;
        public SAPbobsCOM.Recordset oRecordSet;
        public CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        public String RUC;
        public Int32 LoginCount_FM;
        public String CCEmail_FM;
        public String Email_FM;
        public pe.facturamovil.Invoice oInvoice_FM;
        public pe.facturamovil.Ticket oTicket_FM;
        public pe.facturamovil.Note oNote_FM;
        public pe.facturamovil.User oUser_FM;
        public String JsonText;
        public String s;
        public TFunctions Func = new TFunctions();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*TipoDoc.DisplayMember = "Text";
            TipoDoc.ValueMember = "Value";

            TipoDoc.Items.Add(new { Text = "01", Value = "Factura" });
            TipoDoc.Items.Add(new { Text = "01A", Value = "Factura de Anticipo" });
            TipoDoc.Items.Add(new { Text = "03", Value = "Boleta Venta" });
            TipoDoc.Items.Add(new { Text = "07", Value = "Nota de Credito" });
            TipoDoc.Items.Add(new { Text = "08", Value = "Nota de Debito" });
            TipoDoc.Items.Add(new { Text = "09", Value = "Guia de Remision Remitente" });
            TipoDoc.Items.Add(new { Text = "12", Value = "Ticket de Maquina Registradora" });
            TipoDoc.Items.Add(new { Text = "31", Value = "Guia Remision Transportista" });
            TipoDoc.SelectedIndex = 0;*/

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Boolean bSeProcesa = true;
            String sMessage;
            String UserFM = "";
            String PassFM = "";
            String ServerFM = "";
            String Serie;
            String Folio;
            String sTipoDoc;
            String TipoDocElect = "";
            String Tabla = "";
            String sDocSubType = "";
            String sDocEntry;

            
            try
            {
                if (ConectarSAP())
                {
                    if (oRecordSet == null)
                        oRecordSet = ((SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                    sTipoDoc = TipoDoc.SelectedItem.ToString();
                    var Doc = sTipoDoc.Split('-');
                    sTipoDoc = Doc[0].Trim();
                    Serie = txSerie.Text;
                    Folio = txFolio.Text;

                    if (Serie == "")
                    {
                        MessageBox.Show("Debe ingresar Serie");
                        return;
                    }
                    if (Folio == "")
                    {
                        MessageBox.Show("Debe ingresar Folio");
                        return;
                    }
                    if (sTipoDoc == "")
                    {
                        MessageBox.Show("Debe ingresar Tipo Documento");
                        return;
                    }

                    sTipoDoc = sTipoDoc.Trim();
                    Serie = Serie.Trim();
                    Folio = Folio.Trim();

                    //Conexion a Factura Movil
                    LoginCount_FM = 0;
                    oUser_FM = new pe.facturamovil.User();
                    if (oUser_FM.token == null)
                    {
                        RUC = Func.DatosConfig("FacturaMovil", "RUC");
                        UserFM = Func.DatosConfig("FacturaMovil", "user");
                        PassFM = Func.DatosConfig("FacturaMovil", "pass");
                        ServerFM = Func.DatosConfig("FacturaMovil", "server");
                        Func.AddLog(UserFM + " - " + PassFM);
                        oUser_FM = FacturaMovilGlobal.processor.Authenticate(UserFM, PassFM);
                        FacturaMovilGlobal.userConnected = oUser_FM;
                        var ii = 0;
                        var bExistePE = false;
                        try
                        {
                            if (oUser_FM.companies.Find(c => c.code.Trim() == RUC.Trim()) != null)
                            {
                                FacturaMovilGlobal.selectedCompany = oUser_FM.companies.Single(c => c.code.Trim() == RUC.Trim());
                                bExistePE = true;
                                ii = oUser_FM.companies.Count;
                            }
                        }
                        catch (Exception v)
                        {
                            Func.AddLog("Error al buscar empresa en FM");
                            bExistePE = false;
                            bSeProcesa = false;
                        }

                        if (!bExistePE)
                            throw new Exception("No se ha encontrado el RUC " + RUC + " en la conexion de Factura Movil");

                        CCEmail_FM = Func.DatosConfig("FacturaMovil", "mail");
                        //bSeProcesa = true;
                    }

                    //se empieza a procesar por que ya esta conectado
                    if (bSeProcesa)
                    {
                        if (sTipoDoc == "01") //Factura venta
                        {
                            Tabla = "OINV";
                            sDocSubType = "--";
                            TipoDocElect = sTipoDoc;
                        }
                        else if (sTipoDoc == "01A") //Factura anticipo
                        {
                            Tabla = "ODPI";
                            sDocSubType = "--";
                            TipoDocElect = "01";
                        }
                        else if (sTipoDoc == "08") //Nota de Debito
                        {
                            Tabla = "OINV";
                            sDocSubType = "DN";
                            TipoDocElect = sTipoDoc;
                        }
                        else if (sTipoDoc == "03") //Boleta
                        {
                            Tabla = "OINV";
                            sDocSubType = "IB";
                            TipoDocElect = sTipoDoc;
                        }
                        else if (sTipoDoc == "07") //nota de credito
                        {
                            Tabla = "ORIN";
                            sDocSubType = "--";
                            TipoDocElect = sTipoDoc;
                        }

                        s = @"SELECT CAST(T0.DocEntry AS VARCHAR(20)) 'DocEntry', T0.DocSubType, SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) Inst
                            FROM {0} T0 
                            JOIN NNM1 T2 ON T0.Series = T2.Series 
                           WHERE (T0.U_BPP_MDCD = '{1}')
                             AND T0.U_BPP_MDSD = '{3}'
                             --AND SUBSTRING(UPPER(T2.BeginStr), 1, 1) = 'E'
                             AND T0.DocSubType = '{2}'
                           ORDER BY T0.DocEntry DESC";

                        s = String.Format(s, Tabla, Folio, sDocSubType, Serie);
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount == 0)
                            MessageBox.Show("No se ha encontrado el documento " + Serie + "-" + Folio);
                        else
                        {
                            sDocEntry = (System.String)(oRecordSet.Fields.Item("DocEntry").Value);
                            s = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);

                            //if (sTipo in ['33','34','39','41','56'])
                            if (sTipoDoc == "01")
                                EnviarFE_PE(sDocEntry, Serie, Folio, TipoDocElect, "13", sDocSubType, RUC, ref oUser_FM);
                            else if (sTipoDoc == "01A")
                                EnviarFE_PE(sDocEntry, Serie, Folio, TipoDocElect, "203", sDocSubType, RUC, ref oUser_FM);
                            else if (sTipoDoc == "03")
                                ;// EnviarBE_PE(sDocEntry, Serie, Folio, TipoDocElect, "13", sDocSubType, RUC, ref oUser_FM);
                            if (sTipoDoc == "08")
                                EnviarDN_PE(sDocEntry, Serie, Folio, TipoDocElect, "13", sDocSubType, RUC, ref oUser_FM);
                            else if (sTipoDoc == "07")
                                EnviarCN_PE(sDocEntry, Serie, Folio, TipoDocElect, "14", sDocSubType, RUC, ref oUser_FM);
                        }
                    }
                }
            }
            catch (Exception x)
            {
                Func.AddLog("Se ha producido un error, " + x.Message + ", TRACE " + x.StackTrace);
                MessageBox.Show("Se ha producido un error, " + x.Message);
            }
        }



        private Boolean ConectarSAP()
        {
            XmlDocument xDoc;
            XmlNodeList Configuracion;
            XmlNodeList lista;
            Int32 lRetCode;
            String sErrMsg;
            String sPath = Path.GetDirectoryName(this.GetType().Assembly.Location);

            try
            {
                if (oCompany == null)
                    oCompany = new SAPbobsCOM.CompanyClass();
                else
                    oCompany.Disconnect();

                xDoc = new XmlDocument();

                xDoc.Load(sPath + "\\Config.xml");

                Configuracion = xDoc.GetElementsByTagName("Configuracion");
                lista = ((XmlElement)Configuracion[0]).GetElementsByTagName("ServidorSAP");

                foreach (XmlElement nodo in lista)
                {
                    var i = 0;
                    var nServidor = nodo.GetElementsByTagName("Servidor");
                    var nLicencia = nodo.GetElementsByTagName("ServLicencia");
                    var nUserSAP = nodo.GetElementsByTagName("UsuarioSAP");
                    var nPassSAP = nodo.GetElementsByTagName("PasswordSAP");
                    var nSQL = nodo.GetElementsByTagName("SQL");
                    var nUserSQL = nodo.GetElementsByTagName("UsuarioSQL");
                    var nPassSQL = nodo.GetElementsByTagName("PasswordSQL");
                    var nBaseSAP = nodo.GetElementsByTagName("BaseSAP");

                    oCompany.Server = (System.String)(nServidor[i].InnerText);
                    oCompany.LicenseServer = (System.String)(nLicencia[i].InnerText);
                    oCompany.DbUserName = (System.String)(nUserSQL[i].InnerText);
                    oCompany.DbPassword = (System.String)(nPassSQL[i].InnerText);

                    if ((System.String)(nSQL[i].InnerText) == "2008")
                        oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008;
                    else if ((System.String)(nSQL[i].InnerText) == "2012")
                        oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012;

                    oCompany.UseTrusted = false;
                    oCompany.CompanyDB = (System.String)(nBaseSAP[i].InnerText);
                    oCompany.UserName = (System.String)(nUserSAP[i].InnerText);
                    oCompany.Password = (System.String)(nPassSAP[i].InnerText);

                    //            Func.AddLog(oCompany.Server);
                    //            Func.AddLog(oCompany.LicenseServer);
                    //            Func.AddLog(oCompany.DbUserName);
                    //            Func.AddLog(oCompany.DbPassword);
                    //            Func.AddLog(oCompany.CompanyDB);
                    //            Func.AddLog(oCompany.UserName);
                    //            Func.AddLog(oCompany.Password);


                    lRetCode = oCompany.Connect();

                    if (lRetCode != 0)
                    {
                        sErrMsg = oCompany.GetLastErrorDescription();
                        Func.AddLog("Error de conexión base SAP, " + sErrMsg);
                        MessageBox.Show("Error de conexión base SAP, " + sErrMsg);
                        return false;
                    }
                    else
                        return true;
                }

                return true;
            }
            catch (Exception m)
            {
                Func.AddLog("No se ha conectado a SAP, " + m.Message + ", TRACE " + m.StackTrace);
                MessageBox.Show("No se ha conectado a SAP, " + m.Message);
                return false;
            }
        }

        private void EnviarFE_PE(String DocEntry, String SeriePE, String FolioNum, String TipoDocElec, String sObjType, String DocSubType, String lRUC, ref pe.facturamovil.User oUserFM)
        {
            SAPbobsCOM.Recordset orsLocal;
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
            String IncPay;
            String DocDate;

            try
            {
                bImpresionOk = true;
                JsonText = "";
                orsLocal = (SAPbobsCOM.Recordset)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                if (sObjType == "203")
                    oDocumento = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments));
                else
                    oDocumento = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices));

                sMessage = "";

                s = @"select ISNULL(U_IncPay,'N') IncPay from [@VID_FEPARAM] where Code = '1'";
                orsLocal.DoQuery(s);
                IncPay = ((System.String)orsLocal.Fields.Item("IncPay").Value).Trim();

                //validar que exista procedimentos para tipo documento
                s = @"select ISNULL(U_ProcNomE,'') 'ProcNomE', ISNULL(U_ProcNomD,'') 'ProcNomD', ISNULL(U_ProcNomR,'') 'ProcNomR' from [@VID_FEPROCED] where ISNULL(U_Habili,'N') = 'Y' and U_TipoDocPE = '{0}'";

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
                    s = "exec " + ProcNomE + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "', '" + sObjType + "'";

                    //consulta por encabezado
                    orsLocal.DoQuery(s);
                    if (orsLocal.RecordCount > 0)
                    {
                        oInvoice_FM = new pe.facturamovil.Invoice();
                        oInvoice_FM.currency = ((System.String)orsLocal.Fields.Item("currency").Value).Trim();
                        oInvoice_FM.date = ((System.DateTime)orsLocal.Fields.Item("date").Value);
                        oInvoice_FM.series = ((System.String)orsLocal.Fields.Item("series").Value).Trim();
                        externalFolio = ((System.String)orsLocal.Fields.Item("externalFolio").Value).Trim();
                        oInvoice_FM.externalFolio = externalFolio;
                        if (((System.String)orsLocal.Fields.Item("sellerCode").Value).Trim() != "-1")
                            oInvoice_FM.sellerCode = ((System.String)orsLocal.Fields.Item("sellerCode").Value).Trim();

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
                        oInvoice_FM.client = oClient;
                        oInvoice_FM.expirationDate = ((System.DateTime)orsLocal.Fields.Item("expirationDate").Value);

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

                            oInvoice_FM.additionalPrintInformation = oAditional;
                        }
                        catch (Exception er)
                        {
                            throw new Exception("Error additionalPrintInformation - " + er.Message);
                        }

                        if (IncPay == "Y")
                        {
                            //PAYMENT
                            oInvoice_FM.payments = new List<pe.facturamovil.Payment>();
                            var oPayment = new pe.facturamovil.Payment();
                            oPayment.position = 1;
                            oPayment.date = ((System.DateTime)orsLocal.Fields.Item("datePayment").Value);
                            oPayment.amount = ((System.Double)orsLocal.Fields.Item("amountPayment").Value);
                            oPayment.description = ((System.String)orsLocal.Fields.Item("descriptionPayment").Value).Trim();

                            oInvoice_FM.payments.Add(oPayment);
                        }


                        //DETALLE
                        s = "exec " + ProcNomD + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "', '" + sObjType + "'";
                        //consulta por detalle
                        orsLocal.DoQuery(s);
                        if (orsLocal.RecordCount > 0)
                        {
                            oInvoice_FM.details = new List<pe.facturamovil.Detail>();
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

                                oInvoice_FM.details.Add(oDetail);

                                orsLocal.MoveNext();
                            }//fin agregar detalle a la cabecera


                            //REFERENCIAS
                            s = "exec " + ProcNomR + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "', '" + sObjType + "'";
                            //consulta por referencia
                            orsLocal.DoQuery(s);
                            if (orsLocal.RecordCount > 0)
                            {
                                oInvoice_FM.references = new List<pe.facturamovil.Reference>();
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

                                    oInvoice_FM.references.Add(oReference);
                                    orsLocal.MoveNext();
                                }
                            }

                            //termina de cargar documento
                            JsonText = FacturaMovilGlobal.processor.getInvoiceJson(oInvoice_FM);
                            //oRecordSet.DoQuery("UPDATE [@OFMP] SET U_JSON='" + JsonText + "' WHERE DOCENTRY=1");
                            Func.AddLog(JsonText);

                            if (FacturaMovilGlobal.userConnected == null)
                            {
                                try
                                {
                                    LoginCount_FM = 0;
                                    //oUser_FM = new pe.facturamovil.User();
                                    if (oUserFM.token == null)
                                    {
                                        //GlobalSettings.oUser_FM = new pe.facturamovil.User();
                                        orsLocal.DoQuery("SELECT U_User,U_Pwd,U_CCEmail FROM [@VID_FEPARAM] WHERE Code = '1'");
                                        //SBO_f.oLog.OutLog("usuario : '" + ((System.String)orsLocal.Fields.Item("U_User").Value).Trim() + "'");
                                        //SBO_f.oLog.OutLog("password: '" + ((System.String)orsLocal.Fields.Item("U_Pwd").Value).Trim() + "'");
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
                                    //throw new Exception("Motivos de error en conexion : " + ex.Message);
                                    bImpresionOk = false;
                                    sMessage = "Motivos de error en conexion : " + ex.Message;
                                }
                            }

                            try
                            {
                                if (bImpresionOk)
                                {
                                    FacturaMovilGlobal.processor.sendInvoice(FacturaMovilGlobal.selectedCompany, oInvoice_FM, FacturaMovilGlobal.userConnected.token);
                                    Id = oInvoice_FM.id.ToString();
                                    Validation = oInvoice_FM.validation;

                                    //orsLocal.DoQuery("UPDATE OINV SET U_FM_MDFE='Y' WHERE NUMATCARD='" + NumAtCard + "' AND DOCSUBTYPE='--'")
                                    MessageBox.Show("Factura emitida con exito - Serie " + SeriePE + " Folio " + FolioNum);
                                    FacturaMovilGlobal.processor.showDocument(oInvoice_FM);

                                    if (Email != "")
                                    {
                                        Func.AddLog("Enviando documento via email. Porfavor Espere...");
                                        MessageBox.Show("Enviando documento via email. Porfavor Espere...");
                                        FacturaMovilGlobal.processor.sendEmail(FacturaMovilGlobal.selectedCompany, oInvoice_FM, Email, CCEmail_FM, FacturaMovilGlobal.userConnected.token);
                                        Func.AddLog("Factura emitida y enviada al cliente electronicamente con exito. Numero SUNAT : " + externalFolio);
                                        MessageBox.Show("Factura emitida y enviada al cliente electronicamente con exito. Numero SUNAT : " + externalFolio);
                                    }
                                    else
                                    {
                                        Func.AddLog("Factura emitida electronicamente con exito. Asegurese de enviar el documento al cliente. Numero SUNAT : " + externalFolio);
                                        MessageBox.Show("Factura emitida electronicamente con exito. Asegurese de enviar el documento al cliente. Numero SUNAT : " + externalFolio);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                                Func.AddLog("EnviarFE_PE " + ex.Message + " ** Trace: " + ex.StackTrace);
                                bImpresionOk = false;
                                sMessage = ex.Message;
                            }
                        }
                        else
                        {
                            MessageBox.Show("No se encuentra Datos en detalle - Serie " + SeriePE + " Folio " + FolioNum);
                            bImpresionOk = false;
                        }

                    }
                    else
                    {
                        MessageBox.Show("No se encuentra Datos en encabezado - Serie " + SeriePE + " Folio " + FolioNum);
                        Func.AddLog("No se encuentra Datos en encabezado - Serie " + SeriePE + " Folio " + FolioNum);
                        bImpresionOk = false;
                    }
                }
                else
                {
                    MessageBox.Show("Error - No se ha encontrado el documento - Serie " + SeriePE + " Folio " + FolioNum);
                    Func.AddLog("Error - No se ha encontrado el documento - Serie " + SeriePE + " Folio " + FolioNum);
                    bImpresionOk = false;
                }

                DocDate = oDocumento.DocDate.ToString("yyyyMMdd");

                if (!bImpresionOk)
                {
                    MessageBox.Show("Error envio documento electrónico (1) Serie " + SeriePE + " Folio " + FolioNum + " - " + sMessage);
                    Func.AddLog("Error envio documento electrónico (1) Serie " + SeriePE + " Folio " + FolioNum + " - " + sMessage);
                    //sObjType = "13";
                    Status = "EE";
                    if (sMessage == "")
                        sMessage = "Error envio documento electronico a Factura Movil";
                }
                else
                {
                    Status = "EC";
                    //sObjType = "13";
                    sMessage = "Enviado satisfactoriamente";
                    //SBO_f.SBOApp.StatusBar.SetText("Se ha enviado satisfactoriamente el documento electronico a Factura Movil", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    //oDocumento.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                    //actualizo campo de impresion para que no aparezca formulario solicitando folio
                    oDocumento.Printed = SAPbobsCOM.PrintStatusEnum.psYes;
                    lRetCode = oDocumento.Update();
                    if (lRetCode != 0)
                    {
                        s = oCompany.GetLastErrorDescription();
                        Func.AddLog("Error actualizar documento Serie " + SeriePE + " FolioNum " + FolioNum + " - " + s);
                        MessageBox.Show("Error actualizar documento Serie " + SeriePE + " FolioNum " + FolioNum + " - " + s);
                    }
                }

                if (sMessage.Length > 254)
                    sMessage = sMessage.Substring(0, 253);

                s = "SELECT DocEntry, U_Status FROM [@VID_FELOG] WITH (NOLOCK) WHERE U_DocEntry = {0} AND U_ObjType = '{1}' AND U_SubType = '{2}'";
                s = String.Format(s, DocEntry, sObjType, DocSubType);
                orsLocal.DoQuery(s);
                if (orsLocal.RecordCount == 0)
                    FELOGAdd(Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, oCompany.UserName, JsonText, Id, Validation, DocDate);
                else
                {
                    if ((System.String)(orsLocal.Fields.Item("U_Status").Value) != "RR")
                        FELOGUptM((System.Int32)(orsLocal.Fields.Item("DocEntry").Value), Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, oCompany.UserName, JsonText, Id, Validation, DocDate);
                    else
                    {
                        Func.AddLog("Documento ya se encuentra en Factura Movil y en SUNAT - serie " + SeriePE + " folio " + FolioNum);
                        MessageBox.Show("Documento ya se encuentra en Factura Movil y en SUNAT");
                    }
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Func.AddLog("EnviarFE_PE  Serie " + SeriePE + " Folio" + FolioNum + " - " + e.Message + " ** Trace: " + e.StackTrace);
            }

        }

        //Nota de Debito
        public void EnviarDN_PE(String DocEntry, String SeriePE, String FolioNum, String TipoDocElec, String sObjType, String DocSubType, String lRUC, ref pe.facturamovil.User oUserFM)
        {
            SAPbobsCOM.Recordset orsLocal;
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
                JsonText = "";
                orsLocal = (SAPbobsCOM.Recordset)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                oDocumento = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices));
                sMessage = "";

                //validar que exista procedimentos para tipo documento
                s = "select ISNULL(U_ProcNomE,'') 'ProcNomE', ISNULL(U_ProcNomD,'') 'ProcNomD', ISNULL(U_ProcNomR,'') 'ProcNomR' from [@VID_FEPROCED] where ISNULL(U_Habili,'N') = 'Y' and U_TipoDocPE = '{0}'";

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
                    s = "exec " + ProcNomE + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
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
                        oNoteType.isCredit = false;
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
                            throw new Exception("Serie " + SeriePE + " Folio " + FolioNum + " - Error additionalPrintInformation - " + er.Message);
                        }

                        //DETALLE
                        s = "exec " + ProcNomD + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
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
                            s = "exec " + ProcNomR + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
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
                            Func.AddLog(JsonText);

                            if (FacturaMovilGlobal.userConnected == null)
                            {
                                try
                                {

                                    LoginCount_FM = 0;
                                    //oUser_FM = new pe.facturamovil.User();
                                    if (oUserFM.token == null)
                                    {
                                        //oUserFM = new pe.facturamovil.User();
                                        orsLocal.DoQuery("SELECT U_User,U_Pwd,U_CCEmail FROM [@VID_FEPARAM] WHERE Code = '1'");
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
                                    Func.AddLog("Nota de Debito emitida con exito. Serie " + SeriePE + " Folio " + FolioNum);
                                    MessageBox.Show("Nota de Debito emitida con exito. Serie " + SeriePE + " Folio " + FolioNum);
                                    FacturaMovilGlobal.processor.showDocument(oNote_FM);


                                    if (Email != "")
                                    {
                                        Func.AddLog(" Serie " + SeriePE + " Folio " + FolioNum + " - Enviando documento via email. Porfavor Espere...");
                                        MessageBox.Show(" Serie " + SeriePE + " Folio " + FolioNum + " - Enviando documento via email. Porfavor Espere...");
                                        FacturaMovilGlobal.processor.sendEmail(FacturaMovilGlobal.selectedCompany, oNote_FM, Email, CCEmail_FM, FacturaMovilGlobal.userConnected.token);
                                        Func.AddLog("Nota de Debito emitida y enviada al cliente electronicamente con exito. Numero SUNAT : " + externalFolio);
                                        MessageBox.Show("Nota de Debito emitida y enviada al cliente electronicamente con exito. Numero SUNAT : " + externalFolio);
                                    }
                                    else
                                    {
                                        Func.AddLog("Factura emitida electronicamente con exito. Asegurese de enviar el documento al cliente. Numero SUNAT : " + externalFolio);
                                        MessageBox.Show("Factura emitida electronicamente con exito. Asegurese de enviar el documento al cliente. Numero SUNAT : " + externalFolio);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Serie " + SeriePE + " Folio " + FolioNum + " - " + ex.Message);
                                Func.AddLog("EnviarND_PE Serie " + SeriePE + " Folio " + FolioNum + " - " + ex.Message + " ** Trace: " + ex.StackTrace);
                                bImpresionOk = false;
                                sMessage = ex.Message;
                            }
                        }
                        else
                        {
                            Func.AddLog("No se encuentra Datos en detalle Serie " + SeriePE + " Folio " + FolioNum);
                            MessageBox.Show("No se encuentra Datos en detalle Serie " + SeriePE + " Folio " + FolioNum);
                            bImpresionOk = false;
                        }

                    }
                    else
                    {
                        Func.AddLog("No se encuentra Datos en encabezado Serie " + SeriePE + " Folio " + FolioNum);
                        MessageBox.Show("No se encuentra Datos en encabezado Serie " + SeriePE + " Folio " + FolioNum);
                        bImpresionOk = false;
                    }
                }
                else
                {
                    Func.AddLog("Error - No se ha encontrado el documento Serie " + SeriePE + " Folio " + FolioNum);
                    MessageBox.Show("Error - No se ha encontrado el documento Serie " + SeriePE + " Folio " + FolioNum);
                    bImpresionOk = false;
                }

                DocDate = oDocumento.DocDate.ToString("yyyyMMdd");

                if (!bImpresionOk)
                {
                    //SBO_f.SBOApp.MessageBox("Error envio documento electronico ");
                    Func.AddLog("Error envio documento electrónico (1) Serie " + SeriePE + " Folio " + FolioNum);
                    MessageBox.Show("Error envio documento electrónico (1) Serie " + SeriePE + " Folio " + FolioNum);
                    sObjType = "13";
                    Status = "EE";
                    if (sMessage == "")
                        sMessage = "Error envio documento electronico a Factura Movil";
                }
                else
                {
                    Status = "EC";
                    sObjType = "13";
                    sMessage = "Enviado satisfactoriamente a Factura Movil";
                    //SBO_f.SBOApp.StatusBar.SetText("Se ha enviado satisfactoriamente el documento electronico", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    //oDocumento.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                    //actualizo campo de impresion para que no aparezca formulario solicitando folio
                    oDocumento.Printed = SAPbobsCOM.PrintStatusEnum.psYes;
                    lRetCode = oDocumento.Update();
                    if (lRetCode != 0)
                    {
                        s = oCompany.GetLastErrorDescription();
                        MessageBox.Show("Error actualizar documento - " + s);
                        Func.AddLog("Serie " + SeriePE + " folio " + FolioNum+ " - Error actualizar documento - " + s);
                    }
                }

                if (sMessage.Length > 254)
                    sMessage = sMessage.Substring(0, 253);

                s = "SELECT DocEntry, U_Status FROM [@VID_FELOG] WITH (NOLOCK) WHERE U_DocEntry = {0} AND U_ObjType = '{1}' AND U_SubType = '{2}'";
                s = String.Format(s, DocEntry, sObjType, DocSubType);
                orsLocal.DoQuery(s);
                if (orsLocal.RecordCount == 0)
                    FELOGAdd(Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, oCompany.UserName, JsonText, Id, Validation, DocDate);
                else
                {
                    if ((System.String)(orsLocal.Fields.Item("U_Status").Value) != "RR")
                        FELOGUptM((System.Int32)(orsLocal.Fields.Item("DocEntry").Value), Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, oCompany.UserName, JsonText, Id, Validation, DocDate);
                    else
                    {
                        Func.AddLog("Documento ya se encuentra en Factura Movil y en SUNAT - Serie " + SeriePE + " folio " + FolioNum);
                        MessageBox.Show("Documento ya se encuentra en Factura Movil y en SUNAT - Serie " + SeriePE + " folio " + FolioNum);
                    }
                }

            }
            catch (Exception e)
            {
                Func.AddLog("Serie" + SeriePE + " folio " + FolioNum + " - EnviarDN_PE " + e.Message + " ** Trace: " + e.StackTrace);
                MessageBox.Show(e.Message);
            }
        }

        //Nota de Credito
        public void EnviarCN_PE(String DocEntry, String SeriePE, String FolioNum, String TipoDocElec, String ObjType, String DocSubType, String lRUC, ref pe.facturamovil.User oUserFM)
        {
            SAPbobsCOM.Recordset orsLocal;
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
                JsonText = "";
                orsLocal = (SAPbobsCOM.Recordset)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                oDocumento = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes));
                sMessage = "";

                //validar que exista procedimentos para tipo documento
                s = "select ISNULL(U_ProcNomE,'') 'ProcNomE', ISNULL(U_ProcNomD,'') 'ProcNomD', ISNULL(U_ProcNomR,'') 'ProcNomR' from [@VID_FEPROCED] where ISNULL(U_Habili,'N') = 'Y' and U_TipoDocPE = '{0}'";

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
                    s = "exec " + ProcNomE + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
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
                            throw new Exception("Serie " + SeriePE + " folio " + FolioNum + " - Error additionalPrintInformation - " + er.Message);
                        }

                        //DETALLE
                        s = "exec " + ProcNomD + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
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
                            s = "exec " + ProcNomR + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
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
                            Func.AddLog(JsonText);

                            if (FacturaMovilGlobal.userConnected == null)
                            {
                                try
                                {
                                    LoginCount_FM = 0;
                                    //oUser_FM = new pe.facturamovil.User();
                                    if (oUserFM.token == null)
                                    {
                                        orsLocal.DoQuery("SELECT U_User,U_Pwd,U_CCEmail FROM [@VID_FEPARAM] WHERE Code = '1'");
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
                                    Func.AddLog("Nota de Credito emitida con exito. Serie " + SeriePE + " folio " + FolioNum);
                                    MessageBox.Show("Nota de Credito emitida con exito. Serie " + SeriePE + " folio " + FolioNum);
                                    FacturaMovilGlobal.processor.showDocument(oNote_FM);

                                    if (Email != "")
                                    {
                                        Func.AddLog("Serie " + SeriePE + " folio" + FolioNum + " - Enviando documento via email. Porfavor Espere...");
                                        MessageBox.Show("Serie " + SeriePE + " folio" + FolioNum + " - Enviando documento via email. Porfavor Espere...");
                                        FacturaMovilGlobal.processor.sendEmail(FacturaMovilGlobal.selectedCompany, oNote_FM, Email, CCEmail_FM, FacturaMovilGlobal.userConnected.token);
                                        Func.AddLog("Nota de Credito emitida y enviada al cliente electronicamente con exito. Numero SUNAT : " + externalFolio);
                                        MessageBox.Show("Nota de Credito emitida y enviada al cliente electronicamente con exito. Numero SUNAT : " + externalFolio);
                                    }
                                    else
                                    {
                                        Func.AddLog("Factura emitida electronicamente con exito. Asegurese de enviar el documento al cliente. Numero SUNAT : " + externalFolio);
                                        MessageBox.Show("Factura emitida electronicamente con exito. Asegurese de enviar el documento al cliente. Numero SUNAT : " + externalFolio);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                                Func.AddLog("EnviarFE_PE " + ex.Message + " ** Trace: " + ex.StackTrace);
                                bImpresionOk = false;
                                sMessage = ex.Message;
                            }
                        }
                        else
                        {
                            Func.AddLog("No se encuentra Datos en detalle. Serie " + SeriePE + " folio " + FolioNum);
                            MessageBox.Show("No se encuentra Datos en detalle. Serie " + SeriePE + " folio " + FolioNum);
                            bImpresionOk = false;
                        }

                    }
                    else
                    {
                        Func.AddLog("No se encuentra Datos en encabezado. Serie " + SeriePE + " folio " + FolioNum);
                        MessageBox.Show("No se encuentra Datos en encabezado. Serie " + SeriePE + " folio " + FolioNum);
                        bImpresionOk = false;
                    }

                }
                else
                {
                    Func.AddLog("Error - No se ha encontrado el documento. Serie " + SeriePE + " folio " + FolioNum);
                    MessageBox.Show("Error - No se ha encontrado el documento. Serie " + SeriePE + " folio " + FolioNum);
                    bImpresionOk = false;
                }

                DocDate = oDocumento.DocDate.ToString("yyyyMMdd");
                if (!bImpresionOk)
                {
                    //SBO_f.SBOApp.MessageBox("Error envio documento electronico ");
                    if (sMessage != "")
                        MessageBox.Show(sMessage);
                    else
                        MessageBox.Show("Error envio documento electrónico (1)");
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
                    //SBO_f.SBOApp.StatusBar.SetText("Se ha enviado satisfactoriamente el documento electronico a Factura Movil", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    //oDocumento.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                    //actualizo campo de impresion para que no aparezca formulario solicitando folio
                    oDocumento.Printed = SAPbobsCOM.PrintStatusEnum.psYes;
                    lRetCode = oDocumento.Update();
                    if (lRetCode != 0)
                    {
                        s = oCompany.GetLastErrorDescription();
                        MessageBox.Show("Error actualizar documento - " + s);
                        Func.AddLog("Serie" + SeriePE + " folio " + FolioNum + " - Error actualizar Nota credito - " + s);
                    }
                }


                if (sMessage.Length > 254)
                    sMessage = sMessage.Substring(0, 253);

                s = "SELECT DocEntry, U_Status FROM [@VID_FELOG] WITH (NOLOCK) WHERE U_DocEntry = {0} AND U_ObjType = '{1}' AND U_SubType = '{2}'";
                s = String.Format(s, DocEntry, ObjType, DocSubType);
                orsLocal.DoQuery(s);
                if (orsLocal.RecordCount == 0)
                    FELOGAdd(Int32.Parse(DocEntry), ObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, oCompany.UserName, JsonText, Id, Validation, DocDate);
                else
                {
                    if ((System.String)(orsLocal.Fields.Item("U_Status").Value) != "RR")
                        FELOGUptM((System.Int32)(orsLocal.Fields.Item("DocEntry").Value), Int32.Parse(DocEntry), ObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, oCompany.UserName, JsonText, Id, Validation, DocDate);
                    else
                    {
                        Func.AddLog("Documento ya se encuentra en Factura Movil y SUNAT. Serie " + SeriePE + " folio " + FolioNum);
                        MessageBox.Show("Documento ya se encuentra en Factura Movil y SUNAT. Serie " + SeriePE + " folio " + FolioNum);
                    }
                }

            }
            catch (Exception e)
            {
                Func.AddLog("Serie " + SeriePE + " folio " + FolioNum + " - EnviarFE_PE " + e.Message + " ** Trace: " + e.StackTrace);
                MessageBox.Show(e.Message);
            }
        }



        public Int32 FELOGAdd(Int32 DocEntry, String ObjType, String SubType, String SeriePE, Int32 FolioNum, String Status, String sMessage, String TipoDoc, String UserCode, String JsonText, String Id, String Validation, String sDocDate)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            SAPbobsCOM.GeneralDataCollection oFELOGLines = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            SAPbobsCOM.CompanyService CmpnyService;
            DateTime mydate;
            CmpnyService = oCompany.GetCompanyService();

            try
            {
                //Get GeneralService (oCmpSrv is the CompanyService)
                oFELOG = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FELOG"));

                //Create data for new row in main UDO
                oFELOGData = (SAPbobsCOM.GeneralData)(oFELOG.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oFELOGData.SetProperty("U_DocEntry", DocEntry);
                oFELOGData.SetProperty("U_ObjType", ObjType);
                oFELOGData.SetProperty("U_FolioNum", FolioNum);
                oFELOGData.SetProperty("U_SubType", SubType);
                oFELOGData.SetProperty("U_Status", Status);
                oFELOGData.SetProperty("U_Message", sMessage);
                oFELOGData.SetProperty("U_TipoDoc", TipoDoc);
                oFELOGData.SetProperty("U_UserCode", UserCode);
                /*if (sDocDate != "")
                {
                    mydate = DateTime.ParseExact(sDocDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                    oFELOGData.SetProperty("U_DocDate", mydate);
                }*/

                if (JsonText != null)
                    oFELOGData.SetProperty("U_Json", JsonText);
                
                if (SeriePE != null)
                    oFELOGData.SetProperty("U_SeriePE", SeriePE);
                
                if (Id != null)
                    oFELOGData.SetProperty("U_Id", Id);
                
                if (Validation != null)
                    oFELOGData.SetProperty("U_Validation", Validation);

                oFELOGParameter = oFELOG.Add(oFELOGData);
                return (System.Int32)(oFELOGParameter.GetProperty("DocEntry"));
            }
            catch (Exception e)
            {
                Func.AddLog("Agregar DocEntry: " + DocEntry.ToString() + " ObjType: " + ObjType + " SubType: " + SubType + "Error insertar datos en FELOG: " + e.Message + " ** Trace: " + e.StackTrace);
                MessageBox.Show("Agregar DocEntry: " + DocEntry.ToString() + " ObjType: " + ObjType + " SubType: " + SubType + "Error insertar datos en FELOG: " + e.Message);
                return 0;
            }
            finally
            {
                oFELOG = null;
                oFELOGData = null;
                oFELOGLines = null;
                oFELOGParameter = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }//fin FELOGAdd

        public Int32 FELOGUptM(Int32 DocEntry, Int32 DocEntryDoc, String ObjType, String SubType, String SeriePE, Int32 FolioNum, String Status, String sMessage, String TipoDoc, String UserCode, String JsonText, String Id, String Validation, String sDocDate)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            SAPbobsCOM.GeneralDataCollection oFELOGLines = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            String StrDummy;
            SAPbobsCOM.CompanyService CmpnyService;
            DateTime mydate;
            CmpnyService = oCompany.GetCompanyService();

            try
            {
                oFELOG = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FELOG"));
                oFELOGParameter = (SAPbobsCOM.GeneralDataParams)(oFELOG.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = Convert.ToString(DocEntry);
                oFELOGParameter.SetProperty("DocEntry", StrDummy);
                oFELOGData = oFELOG.GetByParams(oFELOGParameter);
                oFELOGData.SetProperty("U_DocEntry", Convert.ToString(DocEntryDoc));
                oFELOGData.SetProperty("U_FolioNum", Convert.ToString(FolioNum));
                oFELOGData.SetProperty("U_Status", Status);
                oFELOGData.SetProperty("U_Message", sMessage);
                oFELOGData.SetProperty("U_TipoDoc", TipoDoc);
                oFELOGData.SetProperty("U_UserCode", UserCode);
                oFELOGData.SetProperty("U_Json", JsonText);
                oFELOGData.SetProperty("U_SeriePE", SeriePE);
                oFELOGData.SetProperty("U_Id", Id);
                oFELOGData.SetProperty("U_Validation", Validation);
                /*if (sDocDate != "")
                {
                    mydate = DateTime.ParseExact(sDocDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                    oFELOGData.SetProperty("U_DocDate", mydate);
                }*/
                
                oFELOG.Update(oFELOGData);
                return (System.Int32)(oFELOGData.GetProperty("DocEntry"));
            }
            catch (Exception e)
            {
                Func.AddLog("Actualizar DocEntry: " + DocEntry.ToString() + " ObjType: " + ObjType + " SubType: " + SubType + "Error insertar datos en FELOG: " + e.Message + " ** Trace: " + e.StackTrace);
                MessageBox.Show("Actualizar DocEntry: " + DocEntry.ToString() + " ObjType: " + ObjType + " SubType: " + SubType + "Error insertar datos en FELOG: " + e.Message);
                return 0;
            }
            finally
            {
                oFELOG = null;
                oFELOGData = null;
                oFELOGLines = null;
                oFELOGParameter = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }//fin FELOGUptM
    }
}
