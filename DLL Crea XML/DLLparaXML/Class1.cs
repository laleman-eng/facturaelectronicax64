using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using SAPbobsCOM;
using VisualD.SBOFunctions;

namespace DLLparaXML
{
    public class TDLLparaXML
    {
        public VisualD.SBOFunctions.CSBOFunctions SBO_f;
        private String s;


        public String GenerarXMLStringInvoice(ref SAPbobsCOM.Recordset ors, ref SAPbobsCOM.Recordset ors2, String TipoDocElec, ref XDocument miXML, String Sector)
        {
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
                                                     new XElement("TpoTranVenta", ((System.String)ors.Fields.Item("TpoTranVenta").Value)),
                                                     new XElement("FmaPago", ((System.String)ors.Fields.Item("FmaPago").Value))
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


                    if ((TipoDocElec != "39") && (TipoDocElec != "41"))
                    {
                        xNodo = new XElement("MntPagos",
                                                new XElement("FchPago", ((System.String)ors.Fields.Item("FchPago").Value).Trim()),
                                                new XElement("MntPago", ((System.Double)ors.Fields.Item("MntPago").Value)),
                                                new XElement("GlosaPagos", ((System.String)ors.Fields.Item("GlosaPagos").Value).Trim())
                                                );
                        miXML.Descendants("IdDoc").LastOrDefault().Add(xNodo);
                    }
                    
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

                        if ((((System.String)ors.Fields.Item("IndGlobal").Value).Trim() == "1") && (TipoDocElec == "56"))
                            xNodo = new XElement("Referencia",
                                                new XElement("NroLinRef", ((System.Int32)ors.Fields.Item("NroLinRef").Value)),
                                                new XElement("IndGlobal", ((System.String)ors.Fields.Item("IndGlobal").Value).Trim()),
                                                new XElement("TpoDocRef", ((System.String)ors.Fields.Item("TpoDocRef").Value).Trim()),
                                                new XElement("FolioRef", ((System.String)ors.Fields.Item("FolioRef").Value).Trim()),
                                                new XElement("FchRef", ((System.String)ors.Fields.Item("FchRef").Value).Trim()),
                                                new XElement("CodRef", ((System.String)ors.Fields.Item("CodRef").Value).Trim()),
                                                new XElement("RazonRef", ((System.String)ors.Fields.Item("RazonRef").Value).Trim())
                                                );
                        else
                            xNodo = new XElement("Referencia",
                                                new XElement("NroLinRef", ((System.Int32)ors.Fields.Item("NroLinRef").Value)),
                                                new XElement("TpoDocRef", ((System.String)ors.Fields.Item("TpoDocRef").Value).Trim()),
                                                new XElement("FolioRef", ((System.String)ors.Fields.Item("FolioRef").Value).Trim()),
                                                new XElement("FchRef", ((System.String)ors.Fields.Item("FchRef").Value).Trim()),
                                                new XElement("CodRef", ((System.String)ors.Fields.Item("CodRef").Value).Trim()),
                                                new XElement("RazonRef", ((System.String)ors.Fields.Item("RazonRef").Value).Trim())
                                                );
                        //if (result == null)
                        //    miXML.Root.Add(xNodo);
                        //else
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

        public String GenerarXMLStringLiquidacionFactura(ref SAPbobsCOM.Recordset ors, String TipoDocElec, ref XDocument miXML, String Sector)
        {
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
                                                     new XElement("Folio", ((System.Int32)ors.Fields.Item("Folio").Value)),
                                                     new XElement("FmaPago", ((System.String)ors.Fields.Item("FmaPago").Value))
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

                    xNodo = new XElement("MntPagos",
                                                new XElement("FchPago", ((System.String)ors.Fields.Item("FchPago").Value).Trim()),
                                                new XElement("MntPago", ((System.Double)ors.Fields.Item("MntPago").Value)),
                                                new XElement("GlosaPagos", ((System.String)ors.Fields.Item("GlosaPagos").Value).Trim())
                                                );
                    miXML.Descendants("IdDoc").LastOrDefault().Add(xNodo);

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
                        //if (result == null)
                        //    miXML.Root.Add(xNodo);
                        //else
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

        public String GenerarXMLStringNotaCredito(ref SAPbobsCOM.Recordset ors, ref SAPbobsCOM.Recordset ors2, String TipoDocElec, ref XDocument miXML, String Sector)
        {
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
                                                     new XElement("MntBruto", ((System.Double)ors.Fields.Item("MntBruto").Value)),
                                                     new XElement("MntCancel", ((System.Double)ors.Fields.Item("MntCancel").Value)),
                                                     new XElement("SaldoInsol", ((System.Double)ors.Fields.Item("SaldoInsol").Value)),
                                                     new XElement("TpoTranCompra", ((System.String)ors.Fields.Item("TpoTranCompra").Value)),
                                                     new XElement("TpoTranVenta", ((System.String)ors.Fields.Item("TpoTranVenta").Value)),
                                                     new XElement("FmaPago", ((System.String)ors.Fields.Item("FmaPago").Value))
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

                    xNodo = new XElement("MntPagos",
                                                new XElement("FchPago", ((System.String)ors.Fields.Item("FchPago").Value).Trim()),
                                                new XElement("MntPago", ((System.Double)ors.Fields.Item("MntPago").Value)),
                                                new XElement("GlosaPagos", ((System.String)ors.Fields.Item("GlosaPagos").Value).Trim())
                                                );
                    miXML.Descendants("IdDoc").LastOrDefault().Add(xNodo);

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

                    //AGREGA Transporte
                    if (TipoDocElec == "112")
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

                        if ((((System.String)ors.Fields.Item("IndGlobal").Value).Trim() == "1") && (TipoDocElec == "61"))
                            xNodo = new XElement("Referencia",
                                                new XElement("NroLinRef", ((System.Int32)ors.Fields.Item("NroLinRef").Value)),
                                                new XElement("IndGlobal", ((System.String)ors.Fields.Item("IndGlobal").Value).Trim()),
                                                new XElement("TpoDocRef", ((System.String)ors.Fields.Item("TpoDocRef").Value).Trim()),
                                                new XElement("FolioRef", ((System.String)ors.Fields.Item("FolioRef").Value).Trim()),
                                                new XElement("FchRef", ((System.String)ors.Fields.Item("FchRef").Value).Trim()),
                                                new XElement("CodRef", ((System.String)ors.Fields.Item("CodRef").Value).Trim()),
                                                new XElement("RazonRef", ((System.String)ors.Fields.Item("RazonRef").Value).Trim())
                                                );
                        else
                            xNodo = new XElement("Referencia",
                                                new XElement("NroLinRef", ((System.Int32)ors.Fields.Item("NroLinRef").Value)),
                                                new XElement("TpoDocRef", ((System.String)ors.Fields.Item("TpoDocRef").Value).Trim()),
                                                new XElement("FolioRef", ((System.String)ors.Fields.Item("FolioRef").Value).Trim()),
                                                new XElement("FchRef", ((System.String)ors.Fields.Item("FchRef").Value).Trim()),
                                                new XElement("CodRef", ((System.String)ors.Fields.Item("CodRef").Value).Trim()),
                                                new XElement("RazonRef", ((System.String)ors.Fields.Item("RazonRef").Value).Trim())
                                                );
                        //if (result == null)
                        //    miXML.Root.Add(xNodo);
                        //else
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

        public String GenerarXMLStringLiquidacionFacturaNC(ref SAPbobsCOM.Recordset ors, String TipoDocElec, ref XDocument miXML, String Sector)
        {
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
                                                     new XElement("Folio", ((System.Int32)ors.Fields.Item("Folio").Value)),
                                                     new XElement("FmaPago", ((System.String)ors.Fields.Item("FmaPago").Value))
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

                    xNodo = new XElement("MntPagos",
                                                new XElement("FchPago", ((System.String)ors.Fields.Item("FchPago").Value).Trim()),
                                                new XElement("MntPago", ((System.Double)ors.Fields.Item("MntPago").Value)),
                                                new XElement("GlosaPagos", ((System.String)ors.Fields.Item("GlosaPagos").Value).Trim())
                                                );
                    miXML.Descendants("IdDoc").LastOrDefault().Add(xNodo);

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
                        //if (result == null)
                        //    miXML.Root.Add(xNodo);
                        //else
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

        public String GenerarXMLStringDelivery(ref SAPbobsCOM.Recordset ors, ref SAPbobsCOM.Recordset ors2, String TipoDocElec, ref XDocument miXML, String Sector)
        {
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

//##Comenta AS                   //xNodo = new XElement("MntPagos",
                    //                                            new XElement("FchPago", ((System.String)ors.Fields.Item("FchPago").Value).Trim()),
                    //                                            new XElement("MntPago", ((System.Double)ors.Fields.Item("MntPago").Value)),
                    //                                            new XElement("GlosaPagos", ((System.String)ors.Fields.Item("GlosaPagos").Value).Trim())
                    //                                            );
                    //miXML.Descendants("IdDoc").LastOrDefault().Add(xNodo);

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
                      //                      new XElement("RecargoMonto", ((System.Double)ors.Fields.Item("RecargoMonto").Value)),
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
                        //if (result == null)
                        //    miXML.Root.Add(xNodo);
                        //else
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

        public String GenerarXMLStringPurchase(ref SAPbobsCOM.Recordset ors, ref SAPbobsCOM.Recordset ors2, String TipoDocElec, ref XDocument miXML, String Sector)
        {
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
                                                     new XElement("IndServicio", ((System.String)ors.Fields.Item("IndServicio").Value).Trim()),
                                                     new XElement("MntBruto", ((System.Double)ors.Fields.Item("MntBruto").Value)),
                                                     new XElement("MntCancel", ((System.Double)ors.Fields.Item("MntCancel").Value)),
                                                     new XElement("SaldoInsol", ((System.Double)ors.Fields.Item("SaldoInsol").Value)),
                                                     new XElement("FmaPago", ((System.String)ors.Fields.Item("FmaPago").Value))
                        //new XElement("Telefono", ((System.String)ors.Fields.Item("").Value).Trim())
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

                    xNodo = new XElement("MntPagos",
                                                new XElement("FchPago", ((System.String)ors.Fields.Item("FchPago").Value).Trim()),
                                                new XElement("MntPago", ((System.Double)ors.Fields.Item("MntPago").Value)),
                                                new XElement("GlosaPagos", ((System.String)ors.Fields.Item("GlosaPagos").Value).Trim())
                                                );
                    miXML.Descendants("IdDoc").LastOrDefault().Add(xNodo);

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
                        //if (result == null)
                        //    miXML.Root.Add(xNodo);
                        //else
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

    }
}
