using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;
using VisualD.Main;
using VisualD.MultiFunctions;
using VisualD.MainObjBase;
using System.Threading;
using System.Windows.Forms;
using Factura_Electronica_VK.FElecObj;
using System.Diagnostics;
using Factura_Electronica_VK.ReImprimir;
using Factura_Electronica_VK.Invoice;
using Factura_Electronica_VK.CreditNotes;
using Factura_Electronica_VK.ConfirmacionFolio;
using Factura_Electronica_VK.PConfirmacionFolio;
using Factura_Electronica_VK.DeliveryNote;
using Factura_Electronica_VK.ImpresionMasiva;
using Factura_Electronica_VK.ConfigFE;
using Factura_Electronica_VK.Impuestos;
using Factura_Electronica_VK.Monitor;
using Factura_Electronica_VK.ResumenFolios;
using Factura_Electronica_VK.IndicadoresSII;
using Factura_Electronica_VK.MultiplesBases;
using Factura_Electronica_VK.Functions;
using Factura_Electronica_VK.GLibro;
using Factura_Electronica_VK.GELibro;
using Factura_Electronica_VK.Libros;
using Factura_Electronica_VK.PurchaseInvoice;
using Factura_Electronica_VK.FoliarDocumento;
using System.Xml;
using System.IO;

namespace Factura_Electronica_VK
{

    public partial class Form1 : System.Windows.Forms.Form
    {

        TMainClassExt MainClass = new TMainClassExt();
        public Form1()
        {
            InitializeComponent();
            MainClass.MainObj.Add(new TFacturaElec());
            MainClass.Init();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ShowInTaskbar = false;
            Hide();
        }

    }

    public class TMainClassExt : TMainClass //class (TMainClass)
    {
        public TMainClassExt()
            : base()
        {
        }

        private void CloseSplash()
        {
            //  if SplashScreen.Visible then SplashScreen.Close(); 
        }

        public override void SetFiltros()
        {
            SAPbouiCOM.EventFilters oFilters;
            SAPbouiCOM.EventFilter oFilter;

            /*base.SetFiltros();
            
            oFilters = SBOApplication.GetFilter();

            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_FORM_LOAD);
            //oFilter.AddEx('504');
            oFilter.AddEx("VID_RImpFE");
            oFilter.AddEx("strCnn");
            oFilter.AddEx("65081");
            oFilter.AddEx("65080");
            oFilter.AddEx("VID_FEMonitor");
            oFilter.AddEx("179"); //Nota de Credito
            oFilter.AddEx("140"); //Guia de despacho
            oFilter.AddEx("65303"); //Nota de debito
            oFilter.AddEx("940"); //transferencia stock
            oFilter.AddEx("182"); //devolucion mercancia Compra
            oFilter.AddEx("65082"); //Impresion Masiva
            oFilter.AddEx("65302"); //Factura Exenta
            oFilter.AddEx("60090"); //Factura + pago venta 
            oFilter.AddEx("60091"); //Factura Reserva
            oFilter.AddEx("65300"); //Factura Anticipo
            oFilter.AddEx("65305"); //boleta exenta
            oFilter.AddEx("65304"); //boleta
            oFilter.AddEx("65307"); //Factura Exportacion
            oFilter.AddEx("133"); //Factura venta
            oFilter.AddEx("141"); //Factura de Compra
            oFilter.AddEx("181"); //Nota de Credito de Compra
            oFilter.AddEx("65301"); //Factura Anticipo de Compra
            oFilter.AddEx("VID_FEIMPADIC");//Impuesto Adicional
            oFilter.AddEx("VID_FEDOCE"); //Indicadores SII
            oFilter.AddEx("VID_FEIMPTO"); //Impuestos
            oFilter.AddEx("VID_FESUC"); //Sucursales
            oFilter.AddEx("VID_FECAF"); //Registrar CAF
            oFilter.AddEx("VID_FEDIST"); //Distribuir Folios
            oFilter.AddEx("VID_FEASIGFOL"); //Asignacion de folios
            oFilter.AddEx("VID_FERESFOL"); //Resumen Folios
            oFilter.AddEx("VID_FEMULTISOC"); //Multiples Sociedades
            oFilter.AddEx("VID_FEPROCED"); //Procedimientos FE
            oFilter.AddEx("VID_GLIBRO");//Generacion Libro Venta y compra
            oFilter.AddEx("VID_GELIBRO");//Generacion y envio Libro Venta y Compra
            oFilter.AddEx("VID_FELIBROS");//Libros
            oFilter.AddEx("FM_IVA");//Configuracion Impuestos PE
            oFilter.AddEx("FM_NOTES");//Tipo de notas PE
            oFilter.AddEx("FM_UMISO");//Unidad de medida ISO
            oFilter.AddEx("VID_FEFOLIAR");//Foliar Documento electronico
            oFilter.AddEx("4873");//Formulario ejecutar reportes crystal

            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_MENU_CLICK);
            //oFilter.AddEx('VID_FCaja');

            //oFilter := SBOFunctions.GetFilterByEventType(oFilters ,SAPbouiCOM.BoEventTypes.et_RIGHT_CLICK);
            //oFilter.AddEx('VID_FCaja');
            //oFilter.AddEx('VID_REPAC');

            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD);
            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_DOUBLE_CLICK);
            oFilter.AddEx("VID_FEMonitor");

            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE);
            oFilter.AddEx("65082"); //Impresion Masiva
            //oFilter.AddEx('65080');

            //oFilter := SBOFunctions.GetFilterByEventType(oFilters ,SAPbouiCOM.BoEventTypes.et_PRINT);
            //oFilter.AddEx('133'); //factura venta
            //oFilter.AddEx('65302'); //factura exenta
            //oFilter.AddEx('65304'); //boleta
            //oFilter.AddEx('65305'); //boleta exenta
            //oFilter.AddEx('65303'); //nota de debito
            //oFilter.AddEx('179'); //nota de credito
            //oFilter.AddEx('140'); //entrega
            //oFilter.AddEx('940'); //transferencia stock

            //oFilter := SBOFunctions.GetFilterByEventType(oFilters ,SAPbouiCOM.BoEventTypes.et_PRINT_DATA);
            //oFilter.AddEx('133'); //factura venta
            //oFilter.AddEx('65302'); //factura exenta
            //oFilter.AddEx('65304'); //boleta
            //oFilter.AddEx('65305'); //boleta exenta
            //oFilter.AddEx('65303'); //nota de debito
            //oFilter.AddEx('179'); //nota de credito
            //oFilter.AddEx('140'); //entrega
            //oFilter.AddEx('940'); //transferencia stock

            //oFilter := SBOFunctions.GetFilterByEventType(oFilters ,SAPbouiCOM.BoEventTypes.et_FORM_RESIZE);
            //oFilter := SBOFunctions.GetFilterByEventType(oFilters ,SAPbouiCOM.BoEventTypes.et_PRINT_LAYOUT_KEY);

            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_FORM_ACTIVATE);
            oFilter.AddEx("179"); //Nota de Credito
            oFilter.AddEx("140"); //Guia de despacho
            oFilter.AddEx("65303"); //Nota de debito
            oFilter.AddEx("940"); //transferencia stock
            oFilter.AddEx("141"); //Factura de Compra
            oFilter.AddEx("181"); //Nota de Credito de Compra
            oFilter.AddEx("65301"); //Factura Anticipo de Compra
            oFilter.AddEx("182"); //devolucion mercancia Compra
            oFilter.AddEx("60090"); //Factura + pago venta 
            oFilter.AddEx("60091"); //Factura Reserva
            oFilter.AddEx("65305"); //boleta exenta
            oFilter.AddEx("65300"); //Factura Anticipo
            oFilter.AddEx("65307"); //Factura Exportacion
            oFilter.AddEx("4873");//Formulario ejecutar reportes crystal
            //oFilter.AddEx('0');//form user fields

            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_COMBO_SELECT);
            //oFilter.AddEx('0');//form user fields
            oFilter.AddEx("VID_FEIMPADIC");//Impuesto Adicional
            oFilter.AddEx("VID_FEDIST"); //Distribuir Folios
            oFilter.AddEx("strCnn");//Parametrizacion FE
            oFilter.AddEx("179"); //Nota de Credito
            oFilter.AddEx("181"); //Nota de Credito de Compra
            oFilter.AddEx("140"); //Guia de despacho
            oFilter.AddEx("65303"); //Nota de debito
            oFilter.AddEx("940"); //transferencia stock
            oFilter.AddEx("182"); //devolucion mercancia Compra
            oFilter.AddEx("133"); //Factura de Venta
            oFilter.AddEx("60090"); //Factura + pago venta
            oFilter.AddEx("60091"); //Factura Reserva
            oFilter.AddEx("65307"); //Factura Exportacion
            oFilter.AddEx("65305"); //boleta exenta
            oFilter.AddEx("FM_IVA");//Configuracion Impuestos PE
            oFilter.AddEx("141"); //Factura de Compra
            oFilter.AddEx("VID_FEFOLIAR");//Foliar Documento electronico
            oFilter.AddEx("65302"); //Factura Exenta
            oFilter.AddEx("65301"); //Factura anticipo compra
            oFilter.AddEx("65300"); //Factura Anticipo
            oFilter.AddEx("VID_FEIMPTO"); //Impuestos
            oFilter.AddEx("141"); //Factura de Compra

            //oFilter := SBOFunctions.GetFilterByEventType(oFilters ,SAPbouiCOM.BoEventTypes.et_CLICK);

            //oFilter := oFilters.Add(SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD);

            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED);
            oFilter.AddEx("strCnn");
            oFilter.AddEx("VID_ReImprimir");
            oFilter.AddEx("65081");
            oFilter.AddEx("65080");
            oFilter.AddEx("VID_FEMonitor");
            oFilter.AddEx("179"); //Nota de Credito
            oFilter.AddEx("140"); //Guia de despacho
            oFilter.AddEx("65303"); //Nota de debito
            oFilter.AddEx("940"); //transferencia stock
            oFilter.AddEx("182"); //devolucion mercancia Compra
            oFilter.AddEx("65082"); //Impresion Masiva
            oFilter.AddEx("133"); //Factura de Venta
            oFilter.AddEx("60090"); //Factura + pago venta
            oFilter.AddEx("60091"); //Factura Reserva
            oFilter.AddEx("65300"); //Factura Anticipo
            oFilter.AddEx("65305"); //boleta exenta
            oFilter.AddEx("65304"); //boleta
            oFilter.AddEx("65302"); //Factura exenta
            oFilter.AddEx("65307"); //Factura Exportacion
            oFilter.AddEx("141"); //Factura de Compra
            oFilter.AddEx("181"); //Nota de Credito de Compra
            oFilter.AddEx("65301"); //Factura Anticipo de Compra
            oFilter.AddEx("VID_FEDOCE"); //Indicadores SII
            oFilter.AddEx("VID_FEIMPTO"); //Impuestos
            oFilter.AddEx("VID_FESUC"); //Sucursales
            oFilter.AddEx("VID_FECAF"); //Registrar CAF
            oFilter.AddEx("VID_FEDIST"); //Distribuir Folios
            oFilter.AddEx("VID_FEASIGFOL"); //Asignacion de folios
            oFilter.AddEx("VID_FERESFOL"); //Resumen Folios
            oFilter.AddEx("VID_FEMULTISOC"); //Multiples Sociedades
            oFilter.AddEx("VID_FEPROCED"); //Procedimientos FE
            oFilter.AddEx("VID_FEIMPADIC");//Impuesto Adicional
            oFilter.AddEx("VID_GLIBRO");//Generacion Libro Venta y compra
            oFilter.AddEx("VID_GELIBRO");//Generacion y envio Libro Venta y Compra
            oFilter.AddEx("VID_FELIBROS");//Libros
            oFilter.AddEx("FM_IVA");//Configuracion Impuestos PE
            oFilter.AddEx("FM_NOTES");//Tipo de notas PE
            oFilter.AddEx("FM_UMISO");//Unidad de medida ISO
            oFilter.AddEx("VID_FEFOLIAR");//Foliar Documento electronico

            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_VALIDATE);
            oFilter.AddEx("VID_FEMonitor");
            oFilter.AddEx("VID_FECAF"); //Registrar CAF
            oFilter.AddEx("VID_FEASIGFOL"); //Asignacion de folios
            oFilter.AddEx("4873");//Formulario ejecutar reportes crystal

            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST);
            oFilter.AddEx("VID_FEASIGFOL"); //Asignacion de folios
            oFilter.AddEx("VID_FEDOCE"); //Indicadores SII
            oFilter.AddEx("VID_FEIMPTO"); //Impuestos

            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_FORM_CLOSE);
            oFilter.AddEx("VID_FEMonitor");


            //oFilter := SBOFunctions.GetFilterByEventType(oFilters ,SAPbouiCOM.BoEventTypes.et_FORM_DATA_LOAD);
            //oFilter.AddEx('VID_FCaja');

            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE);
            oFilter.AddEx("65081");
            oFilter.AddEx("4873");//Formulario ejecutar reportes crystal

            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD);
            oFilter.AddEx("179"); //Nota de Credito
            oFilter.AddEx("140"); //Guia de despacho
            oFilter.AddEx("65303"); //Nota de debito
            oFilter.AddEx("940"); //transferencia stock
            oFilter.AddEx("182"); //devolucion mercancia Compra
            oFilter.AddEx("65302"); //Factura Exenta
            oFilter.AddEx("133"); //Factura venta
            oFilter.AddEx("60090"); //Factura + pago venta 
            oFilter.AddEx("60091"); //Factura Reserva
            oFilter.AddEx("65300"); //Factura Anticipo
            oFilter.AddEx("65305"); //boleta exenta
            oFilter.AddEx("65304"); //boleta
            oFilter.AddEx("65307"); //Factura Exportacion
            oFilter.AddEx("141"); //Factura de Compra
            oFilter.AddEx("181"); //Nota de Credito de Compra
            oFilter.AddEx("65301"); //Factura Anticipo de Compra
            oFilter.AddEx("4873");//Formulario ejecutar reportes crystal
            //oFilter := oFilters.Add(SAPbouiCOM.BoEventTypes.et_KEY_DOWN);

            oFilter = SBOFunctions.GetFilterByEventType(oFilters, SAPbouiCOM.BoEventTypes.et_MATRIX_LINK_PRESSED);
            oFilter.AddEx("VID_FEMonitor");

            SBOApplication.SetFilter(oFilters);*/

        }


        public override void initApp()
        {
            String XlsFile;
            String s;
            base.initApp();
            String Local;
            Boolean bCargar = false;

            try
            {
                // compilacion SQL HANA
                GlobalSettings.SBO_f = SBOFunctions;
                GlobalSettings.SBOMeta = SBOMetaData;
                //
                

                //oLog.LogFile = "C:\\Visualk\\xxx.log";
                oLog.LogFile = System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\VD.log";
                SAPbobsCOM.Recordset oRecordSet;
                oRecordSet = (SAPbobsCOM.Recordset)(SBOCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                try
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = "SELECT SUPERUSER FROM OUSR WHERE USER_CODE = '{0}'";
                    else
                        s = @"SELECT ""SUPERUSER"" FROM ""OUSR"" WHERE ""USER_CODE"" = '{0}'";
                    s = String.Format(s, SBOCompany.UserName.Trim());
                    oRecordSet.DoQuery(s);
                    if (((System.String)oRecordSet.Fields.Item("SUPERUSER").Value).Trim() == "Y")
                        bCargar = true;

                }
                catch
                {
                    if (bCargar == false)
                        SBOApplication.MessageBox("El addon se esta iniciando por primera vez, debe inicar con un super usuario");
                }
                finally
                {
                    SBOFunctions._ReleaseCOMObject(oRecordSet);
                    oRecordSet = null;
                }


                //if (SBOCompany.UserName == "manager")
                if (bCargar)
                {
                    XlsFile = System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\Docs\\UDFFELEC.xls";
                    if (!SBOFunctions.ValidEstructSHA1(XlsFile))
                    {
                        oLog.OutLog("InitApp: Estructura de datos 1 - Facturación Electronica");
                        SBOApplication.StatusBar.SetText("Inicializando AddOn Factura Electronica(1).", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        if (!SBOMetaData.SyncTablasUdos("1.1", XlsFile))
                        {
                            SBOFunctions.DeleteSHA1FromTable("EDAG.xls");
                            oLog.OutLog("InitApp: sincronización de Estructura de datos fallo");
                            CloseSplash();
                            SBOApplication.MessageBox("Estructura de datos con problemas, consulte a soporte...", 1, "Ok", "", "");
                            Halt(0);
                        }
                    }

                    XlsFile = System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\Docs\\UDFFELECCL.xls";
                    if (!SBOFunctions.ValidEstructSHA1(XlsFile))
                    {
                        oLog.OutLog("InitApp: Estructura de datos 2 - Facturación Electronica CL");
                        SBOApplication.StatusBar.SetText("Inicializando AddOn Factura Electronica(2).", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        if (!SBOMetaData.SyncTablasUdos("1.1", XlsFile))
                        {
                            SBOFunctions.DeleteSHA1FromTable("UDFFELECCL.xls");
                            oLog.OutLog("InitApp: sincronización de Estructura de datos fallo");
                            SBOApplication.MessageBox("Estructura de datos con problemas, consulte a soporte...", 1, "Ok", "", "");
                        }
                    }


                    XlsFile = System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\Docs\\UDFSAP.xls";
                    if (!SBOFunctions.ValidEstructSHA1(XlsFile))
                    {
                        oLog.OutLog("InitApp: Estructura de datos 3 - Facturación Electronica CL");
                        SBOApplication.StatusBar.SetText("Inicializando AddOn Factura Electronica(3).", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        if (!SBOMetaData.SyncTablasUdos("1.1", XlsFile))
                        {
                            SBOFunctions.DeleteSHA1FromTable("UDFSAP.xls");
                            oLog.OutLog("InitApp: sincronización de Estructura de datos fallo");
                            SBOApplication.MessageBox("Estructura de datos con problemas, consulte a soporte...", 1, "Ok", "", "");
                        }
                    }
                }

                oLog.DebugLvl = 20;
                MainObj[0].GlobalSettings = GlobalSettings;
                MainObj[0].SBOApplication = SBOApplication;
                MainObj[0].SBOCompany = SBOCompany;
                MainObj[0].oLog = oLog;
                MainObj[0].SBOFunctions = SBOFunctions;


                //SetFiltros();


                MainObj[0].AddMenus();

                InitOK = true;
                oLog.OutLog("App SBO in C# - Init!");
                SBOApplication.StatusBar.SetText("Aplicación Inicializada.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);


                if (1 == 1) //(SBOFunctions.AccessStr.Substring(10,1) = 'P') then 
                {
                    GlobalSettings.SBO_f = SBOFunctions;
                    MainObj[0].GlobalSettings = GlobalSettings;
                    MainObj[0].SBOApplication = SBOApplication;
                    MainObj[0].SBOCompany = SBOCompany;
                    MainObj[0].oLog = oLog;
                    MainObj[0].SBOFunctions = SBOFunctions;
                }

                SetFiltros();

                if (1 == 1) // (SBOFunctions.AccessStr.Substring(10,1) = 'P') then
                {
                    //MainObj[0].AddMenus();
                    SAPbouiCOM.Menus oMenus = null;
                    SAPbouiCOM.MenuItem oMenuItem = null;


                    System.Xml.XmlDocument oXmlDoc = null;
                    oXmlDoc = new System.Xml.XmlDocument();

                    oXmlDoc.Load(System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\Menus\\RemoveMenu.xml");


                    string sXML = oXmlDoc.InnerXml.ToString();
                    SBOApplication.LoadBatchActions(ref sXML);

                    oXmlDoc.Load(System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\Menus\\Menu.xml");

                    sXML = oXmlDoc.InnerXml.ToString();
                    SBOApplication.LoadBatchActions(ref sXML);

                    //GlobalSettings.AddMenusRepAux(False);
                    //GlobalSettings.AddMenusRepAux(True);
                }

                InitOK = true;
                oLog.OutLog("C# - Shine your crazy diamond!");
                SBOApplication.StatusBar.SetText("Aplicación Inicializada.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                oLog.OutLog("Error iniApp, " + ex.Message + ", TRACE " + ex.StackTrace);
            }
            finally
            {
                CloseSplash();
            }
        }
    }
}
