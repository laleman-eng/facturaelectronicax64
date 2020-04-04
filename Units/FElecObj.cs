using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using SAPbouiCOM;
using SAPbobsCOM; 
using VisualD.MainObjBase;
using VisualD.MenuConfFr; 
//using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.vkFormInterface;
using VisualD.MultiFunctions;
using System.Xml;
using Factura_Electronica_VK.ReImprimir;
using Factura_Electronica_VK.Invoice;
using Factura_Electronica_VK.CreditNotes;
using Factura_Electronica_VK.ConfirmacionFolio;
using Factura_Electronica_VK.PConfirmacionFolio;
using Factura_Electronica_VK.DeliveryNote;
using Factura_Electronica_VK.ImpresionMasiva;
using Factura_Electronica_VK.ConfigFE;
using Factura_Electronica_VK.CriterioImpMasiva;
using Factura_Electronica_VK.ImptoAdicional;
using Factura_Electronica_VK.SelDocImpMasivo;
using Factura_Electronica_VK.Monitor;
using Factura_Electronica_VK.Impuestos;
using Factura_Electronica_VK.IndicadoresSII;
using Factura_Electronica_VK.Sucursal;
using Factura_Electronica_VK.RegistrarCAF;
using Factura_Electronica_VK.DistribucionFolios;
using Factura_Electronica_VK.AsignarFolios;
using Factura_Electronica_VK.ResumenFolios;
using Factura_Electronica_VK.MultiplesBases;
using Factura_Electronica_VK.Functions;
using Factura_Electronica_VK.ProcedimientosFE;
using Factura_Electronica_VK.GLibro;
using Factura_Electronica_VK.GELibro;
using Factura_Electronica_VK.PurchaseInvoice;
using Factura_Electronica_VK.FoliarDocumento;
using Factura_Electronica_VK.PlanCuentaSII;
using Factura_Electronica_VK.MenuConfiguracionHANA;
using Factura_Electronica_VK.LibrosElectronicos;
using Factura_Electronica_VK.MonitorDTE;
using Factura_Electronica_VK.EnviarEstadoDTE;
using Factura_Electronica_VK.ReutilizarFolio;
using Factura_Electronica_VK.ListaBlanca;
using Factura_Electronica_VK.ListaNegra;

namespace Factura_Electronica_VK.FElecObj
{
    public class TFacturaElec : TMainObjBase //class(TMainObjBase)
    {
        String s;
        TMainObjBase mainObject = new TMainObjBase();        
        private SAPbobsCOM.Recordset oRecordSet; 

        public override void AddMenus()
        {
            base.AddMenus();
            System.Xml.XmlDocument oXMLDoc;
            //String sImagePath;
            try
            {
                //inherited addMenus;
                oXMLDoc = new System.Xml.XmlDocument();
                //try
                    //sImagePath := TMultiFunctions.ExtractFilePath(TMultiFunctions.ParamStr(0)) + '\Menus\Menu.xml';
                    //oXMLDoc.Load(sImagePath);
                    //StrAux := oXMLDoc.InnerXml;
                    //SBOApplication.LoadBatchActions(var StrAux);
                //except
                //on e: exception do
                    //SBOFunctions.oLog.OutLog('AddMenus err: ' + e.Message + ' ** Trace: ' + e.  StackTrace);
                //end;
            }
            finally
            {
                oXMLDoc = null;
            }
        } //fin AddMenus

        public override void MenuEventExt(List<object> oForms, ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
            IvkFormInterface oForm;
            base.MenuEventExt(oForms, ref pVal, ref BubbleEvent);
            try
            {
                //Inherited MenuEventExt(oForms,var pVal,var BubbleEvent);
                oForm = null;
                if (! pVal.BeforeAction)
                {
                    switch (pVal.MenuUID)
                    {
                        case "VID_FERImpFE":    
                            {
                                oForm = (IvkFormInterface)(new TReImprimir());
                                //(TReImprimir)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEConf":
                            {
                                oForm = (IvkFormInterface)(new TConfigFE());
                                //(TConfigFE)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEImptoAd":
                            {
                                oForm = (IvkFormInterface)(new TImptoAdicional());
                                //(TImptoAdicional)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEIndSII": //Menu para ingresar mapeo indicadores para los libros
                            {
                                oForm = (IvkFormInterface)(new TIndicadoresSII());
                                //(TIndicadoresSII)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FeImptoSII": //Menu para ingresar mapeo de los impuestos para los libros
                            {
                                oForm = (IvkFormInterface)(new TImpuestos());
                                //(TImpuestos)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEMonitor": //Menu para Monitor
                            {
                                oForm = (IvkFormInterface)(new TMonitor());
                                //(TMonitor)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEMonDTE": //monitor de DTE
                            {
                                oForm = (IvkFormInterface)(new TMonitorDTE());
                                break;
                            }
                        case "VID_FEEnvDTE":
                            {
                                oForm = (IvkFormInterface)(new TEnviarEstadoDTE());
                                break;
                            }
                        case "VID_FESUC": //Menu para Sucursal factura electronica
                            {
                                oForm = (IvkFormInterface)(new TSucursal());
                                //(TSucursal)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FECAF": //Menu para regsitrar CAF
                            {
                                oForm = (IvkFormInterface)(new TRegistrarCAF());
                                //(TRegistrarCAF)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEDIST": //Menu para Distribucion Folios
                            {
                                oForm = (IvkFormInterface)(new TDistribucionFolios());
                                //(TDistribucionFolios)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEASIGFOL": //Menu para Distribuir Folios
                            {
                                oForm = (IvkFormInterface)(new TAsignarFolios());
                                //(TAsignarFolios)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FERESFOL": //Menu para Resumen Estado de Folios
                            {
                                oForm = (IvkFormInterface)(new TResumenFolios());
                                //(TResumenFolios)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEMULTISOC": //Menu para Multiples bases FE
                            {
                                oForm = (IvkFormInterface)(new TMultiplesBases());
                                //(TMultiplesBases)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEPROCED": //Menu para Procedimiento FE
                            {
                                oForm = (IvkFormInterface)(new TProcedimientosFE());
                                //(TMultiplesBases)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FELISTABL": //Menu para Lista Blanca
                            {
                                oForm = (IvkFormInterface)(new TListaBlanca());
                                //(TMultiplesBases)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FELISTANE": //Menu para Lista Negra
                            {
                                oForm = (IvkFormInterface)(new TListaNegra());
                                //(TMultiplesBases)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEGLibro": //Menu para generacion Libro Ventas y Compra
                            {
                                oForm = (IvkFormInterface)(new TGLibro());
                                //(TMultiplesBases)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEGELibro": //Menu para Generacion y envioLibro Ventas y Compras
                            {
                                oForm = (IvkFormInterface)(new TGELibro());
                                //(TMultiplesBases)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEFOLIAR": //Menu para Foliar Documento Electronica
                            {
                                oForm = (IvkFormInterface)(new TFoliarDocumento());
                                break;
                            }
                        case "VID_FEREUTFOL": //Menu para Reutilizar folio rechazado
                            {
                                oForm = (IvkFormInterface)(new TReutilizarFolio());
                                break;
                            }
                        case "VID_FEPLANCTA"://Menu para Plan de cuenta SII
                            {
                                oForm = (IvkFormInterface)(new TPlanCuentaSII());
                                break;
                            }
                        case "VID_RHSQL":
                            {
                                //oForm                       := IvkFormInterface(New TCredencialesBD);
                                //TCredencialesBD(oForm).ooForms :=oForms;
                                if (GlobalSettings.RunningUnderSQLServer)
                                    oForm = (IvkFormInterface)(new TMenuConfFr());
                                else
                                    oForm = (IvkFormInterface)(new TMenuConfiguracionHANA());
                                //oForm1 := SBOApplication.Forms.ActiveForm;
                                //EditText(oForm1.Items.Item("Pw").Specific).IsPassword := true;
                                break;
                            }
                        case "VID_FELibroDiario":
                            {
                                oForm = (IvkFormInterface)(new TLibrosElectronicos());
                                TLibrosElectronicos.TipoLibro = "D";
                                break;
                            }
                        case "VID_FELibroMayor":
                            {
                                oForm = (IvkFormInterface)(new TLibrosElectronicos());
                                TLibrosElectronicos.TipoLibro = "M";
                                break;
                            }
                        case "VID_FEBalance":
                            {
                                oForm = (IvkFormInterface)(new TLibrosElectronicos());
                                TLibrosElectronicos.TipoLibro = "B";
                                break;
                            }
                        case "VID_FEDiccionario":
                            {
                                oForm = (IvkFormInterface)(new TLibrosElectronicos());
                                TLibrosElectronicos.TipoLibro = "C";
                                break;
                            }
                    }  
            
                    if (oForm != null) 
                    {
                        SAPbouiCOM.Application App = SBOApplication;
                        SAPbobsCOM.Company Cmpny = SBOCompany;
                        VisualD.SBOFunctions.CSBOFunctions SboF = SBOFunctions;
                        VisualD.GlobalVid.TGlobalVid Glob = GlobalSettings;
                        
                        if (oForm.InitForm(SBOFunctions.generateFormId(GlobalSettings.SBOSpaceName, GlobalSettings), "forms\\",ref  App,ref  Cmpny,ref SboF, ref Glob)) 
                        {   oForms.Add(oForm); }
                        else 
                        {
                            SBOApplication.Forms.Item(oForm.getFormId()).Close();
                            oForm = null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SBOApplication.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok","","");  // Captura errores no manejados
                oLog.OutLog("MenuEventExt: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        } //MenuEventExt

        public override IvkFormInterface ItemEventExt(IvkFormInterface oIvkForm, List<object> oForms, String LstFrmUID, String FormUID, ref ItemEvent pVal, ref Boolean BubbleEvent)
        {
            SAPbouiCOM.Form oForm;
            SAPbouiCOM.Form oFormParent;
            IvkFormInterface result = null;
            result = base.ItemEventExt(oIvkForm, oForms, LstFrmUID, FormUID, ref pVal, ref BubbleEvent);

            try
            {
                //inherited ItemEventExt(oIvkForm,oForms,LstFrmUID, FormUID, var pVal, var BubbleEvent);   


                result = base.ItemEventExt(oIvkForm, oForms, LstFrmUID, FormUID, ref pVal, ref BubbleEvent);

                if (result != null)
                {
                    return result;
                }
                else
                {
                    if (oIvkForm != null)
                    {
                        return oIvkForm;
                    }
                }

                // CFL Extendido (Enmascara el CFL estandar)
                if ((pVal.BeforeAction) && (pVal.EventType == BoEventTypes.et_FORM_LOAD) && (!string.IsNullOrEmpty(LstFrmUID)))
                {
                    try
                    {
                        oForm = SBOApplication.Forms.Item(LstFrmUID);
                    }
                    catch
                    {
                        oForm = null;
                    }
                }


                if ((!pVal.BeforeAction) && (pVal.FormTypeEx == "0"))
                {
                    if ((oIvkForm == null) && (GlobalSettings.UsrFldsFormActive) && (GlobalSettings.UsrFldsFormUid != "") && (pVal.EventType == BoEventTypes.et_FORM_LOAD))
                    {
                        oForm = SBOApplication.Forms.Item(pVal.FormUID);
                        oFormParent = SBOApplication.Forms.Item(GlobalSettings.UsrFldsFormUid);
                        try
                        {
                            //SBO_App.StatusBar.SetText(oFormParent.Title,BoMessageTime.bmt_Short,BoStatusBarMessageType.smt_Warning);
                            SBOFunctions.FillListUserFieldForm(GlobalSettings.ListFormsUserField, oFormParent, oForm);
                        }
                        finally
                        {
                            GlobalSettings.UsrFldsFormUid = "";
                            GlobalSettings.UsrFldsFormActive = false;
                        }
                    }
                    else
                    {
                        if ((pVal.EventType == BoEventTypes.et_FORM_ACTIVATE) || (pVal.EventType == BoEventTypes.et_COMBO_SELECT) || (pVal.EventType == BoEventTypes.et_FORM_RESIZE))
                        {
                            oForm = SBOApplication.Forms.Item(pVal.FormUID);
                            SBOFunctions.DisableListUserFieldsForm(GlobalSettings.ListFormsUserField, oForm);
                        }
                    }

                }


                if ((!pVal.BeforeAction) && (pVal.EventType == BoEventTypes.et_FORM_LOAD) && (oIvkForm == null))
                {
                    switch (pVal.FormTypeEx)
                    {
                        case "65080": //primera ventana confirmacion folio
                            {
                                result = (IvkFormInterface)(new TPConfirmacionFolio());
                                //(TPConfirmacionFolio)(result).ooForms = oForms;
                                break;
                            }
                        case "65081": //segunda ventana confirmacion folio
                            {
                                result = (IvkFormInterface)(new TConfirmacionFolio());
                                //(TConfirmacionFolio)(result).ooForms = oForms;
                                break;
                            }
                        case "65082": //Impresion Masiva
                            {
                                result = (IvkFormInterface)(new TImpresionMasiva());
                                //(TImpresionMasiva)(result).ooForms = oForms;
                                break;
                            }
                        case "184": //Criterios para impresion masiva
                            {
                                result = (IvkFormInterface)(new TCriterioImpMasiva());
                                //(TCriterioImpMasiva)(result).ooForms = oForms;
                                break;
                            }
                        case "191": //Seleccionar Documentos para impresion masiva
                            {
                                result = (IvkFormInterface)(new TSelDocImpMasivo());
                                //(TSelDocImpMasivo)(result).ooForms = oForms;
                                break;
                            }
                        case "133": //Factura
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "--";
                                TInvoice.bFolderAdd = true;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = false;
                                TInvoice.Liquidacion = true;
                                break;
                            }
                        case "65307": //Factura Exportacion
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "IX";
                                TInvoice.bFolderAdd = true;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = false;
                                TInvoice.Liquidacion = false;
                                break;
                            }
                        case "60090": //Factura + pago venta
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "--";
                                TInvoice.bFolderAdd = true;
                                TInvoice.ReservaExp = false;
                                TInvoice.ObjType = "13";
                                TInvoice.Liquidacion = true;
                                break;
                            }
                        case "60091": //Factura Reserva
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "--";
                                TInvoice.bFolderAdd = true;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = true;
                                TInvoice.Liquidacion = false;
                                break;
                            }
                        case "65302": //Factura exenta 
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "IE";
                                TInvoice.bFolderAdd = false;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = false;
                                TInvoice.Liquidacion = false;
                                break;
                            }
                        case "65300": //Factura Anticipo
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "--";
                                TInvoice.bFolderAdd = false;
                                TInvoice.ObjType = "203";
                                TInvoice.ReservaExp = false;
                                TInvoice.Liquidacion = false;
                                break;
                            }
                        case "65304": //Boleta 
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "IB";
                                TInvoice.bFolderAdd = true;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = false;
                                TInvoice.Liquidacion = false;
                                break;
                            }
                        case "65305": //Boleta Exenta
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "EB";
                                TInvoice.bFolderAdd = true;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = false;
                                TInvoice.Liquidacion = false;
                                break;
                            }
                        case "65303": //Nota de debito 
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "DN";
                                TInvoice.bFolderAdd = true;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = false;
                                TInvoice.Liquidacion = false;
                                break;
                            }
                        case "179": //Nota de Credito 
                            {
                                result = (IvkFormInterface)(new TCreditNotes());
                                //(TCreditNotes)(result).ooForms = oForms;
                                TCreditNotes.DocSubType = "--";
                                TCreditNotes.bFolderAdd = true;
                                TCreditNotes.ObjType = "14";
                                break;
                            }
                        case "140": //Entrega
                            {
                                result = (IvkFormInterface)(new TDeliveryNote());
                                //(TDeliveryNote)(result).ooForms = oForms;
                                TDeliveryNote.Transferencia = false;
                                TDeliveryNote.bFolderAdd = true;
                                TDeliveryNote.Devolucion = false;
                                break;
                            }
                        case "182": //Devolucion mercancia Compra
                            {
                                result = (IvkFormInterface)(new TDeliveryNote());
                                //(TDeliveryNote)(result).ooForms = oForms;
                                TDeliveryNote.Transferencia = false;
                                TDeliveryNote.bFolderAdd = true;
                                TDeliveryNote.Devolucion = true;
                                break;
                            }
                        case "940": //Transferencia Stock
                            {
                                result = (IvkFormInterface)(new TDeliveryNote());
                                //(TDeliveryNote)(result).ooForms = oForms;
                                TDeliveryNote.Transferencia = true;
                                TDeliveryNote.bFolderAdd = true;
                                TDeliveryNote.Devolucion = false;
                                TDeliveryNote.SolicitudTraslado = false;
                                break;
                            }
                        case "1250000940":// Solicitud de traslado de Mercancias
                            {

                                oRecordSet = (SAPbobsCOM.Recordset)(SBOCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                                if (GlobalSettings.RunningUnderSQLServer)
                                {
                                    s = @"Select isnull(U_DteSolTras,'N') dtesoltras from [@VID_FEPARAM]";
                                }
                                else
                                {
                                    s = @"Select IFNULL(""U_DteSolTras"",'N') dtesoltras ""@VID_FEPARAM"" ";   
                                }
                                
                                oRecordSet.DoQuery(s);

                                if ((System.String)(oRecordSet.Fields.Item("dtesoltras").Value) == "Y")
                                {
                                    result = (IvkFormInterface)(new TDeliveryNote());
                                    TDeliveryNote.Transferencia = true;
                                    TDeliveryNote.bFolderAdd = true;
                                    TDeliveryNote.Devolucion = false;
                                    TDeliveryNote.SolicitudTraslado = true;
                                    break;  
                                }
                                break;      
                            }
                        case "141": //Factura de compra a terceros
                            {
                                result = (IvkFormInterface)(new TPurchaseInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TPurchaseInvoice.DocSubType = "--";
                                TPurchaseInvoice.bFolderAdd = false;
                                TPurchaseInvoice.ObjType = "18";
                                break;
                            }
                        case "65301": //Factura Anticipo de compra a terceros
                            {
                                result = (IvkFormInterface)(new TPurchaseInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TPurchaseInvoice.DocSubType = "--";
                                TPurchaseInvoice.bFolderAdd = false;
                                TPurchaseInvoice.ObjType = "204";
                                break;
                            }
                        case "181": //Nota de Credito Compra
                            {
                                result = (IvkFormInterface)(new TCreditNotes());
                                //(TCreditNotes)(result).ooForms = oForms;
                                TCreditNotes.DocSubType = "--";
                                TCreditNotes.bFolderAdd = true;
                                TCreditNotes.ObjType = "19";
                                break;
                            }
                    } //fi  switch
                }


                if (result != null)
                {
                    SAPbouiCOM.Application App = SBOApplication;
                    SAPbobsCOM.Company Cmpny = SBOCompany;
                    VisualD.SBOFunctions.CSBOFunctions SboF = SBOFunctions;
                    VisualD.GlobalVid.TGlobalVid Glob = GlobalSettings;
                    if (result.InitForm(pVal.FormUID, @"forms\\", ref App, ref Cmpny, ref SboF, ref Glob))
                    {
                        oForms.Add(result);
                    }
                    else
                    {
                        SBOApplication.Forms.Item(result.getFormId()).Close();
                        result = null;
                    }
                }

                return result;
            }// fin try
            catch (Exception e)
            {
                return null;
                oLog.OutLog("ItemEventExt: " + e.Message + " ** Trace: " + e.StackTrace);
                SBOApplication.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok","","");  // Captura errores no manejados
            }
    
        } //fin ItemEventExt
    }
}
