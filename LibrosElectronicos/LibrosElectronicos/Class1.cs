using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using SAPbouiCOM;

namespace LibrosElectronicosXml
{
    public class LEXml
    {
        private static String s;
        private static System.Globalization.CultureInfo _nf = ((CultureInfo)CultureInfo.InvariantCulture.Clone());

        public static String CrearXMLDiccionario(String Periodo, SAPbouiCOM.DataTable odt, SAPbouiCOM.Application FSBOApp)
        {
            String NomCol;
            try
            {
                s = @"<LceCoCierre version=""1.0""><LceDiccionario version=""1.0""><DocumentoDiccionario ID=""DICC{0}"">";
                s = String.Format(s, Periodo.Replace("-", "."));

                NomCol = odt.Columns.Item(0).Name;
                var Col = NomCol.Split('/');
                s = s + @"<{0}><{1}>{2}</{1}>";
                s = String.Format(s, Col[0], Col[1], ((System.String)odt.GetValue(0, 0)).Trim());

                NomCol = odt.Columns.Item(1).Name;
                Col = NomCol.Split('/');
                s = s + @"<{1}>{2}</{1}></{0}>";
                s = String.Format(s, Col[0], Col[1], ((System.String)odt.GetValue(1, 0)).Trim());

                for (Int32 i = 0; i < odt.Rows.Count; i++)
                {
                    NomCol = odt.Columns.Item(2).Name;
                    Col = NomCol.Split('/');
                    var sFin = Col[0];
                    s = s + @"<{0}>";
                    s = String.Format(s, sFin);

                    for (Int32 c = 2; c < odt.Columns.Count-1; c++)
                    {
                        NomCol = odt.Columns.Item(c).Name;
                        Col = NomCol.Split('/');
                        s = s + @"<{0}>{1}</{0}>";
                        s = String.Format(s, Col[1], ((System.String)odt.GetValue(c, i)).Trim());
                    }

                    s = s + @"</{0}>";
                    s = String.Format(s, sFin);
                }

                s = s + @"</DocumentoDiccionario></LceDiccionario></LceCoCierre>";

                return s;
            }
            catch (Exception z)
            {
                FSBOApp.StatusBar.SetText(z.Message + " ** Trace: " + z.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                //OutLog("CrearXMLDiccionario: " + z.Message + " ** Trace: " + z.StackTrace);
                return "";
            }
        }

        public static String CrearXMLLibroDiarioMayor(String Periodo, SAPbouiCOM.DataTable odt, SAPbouiCOM.DataTable odt2, SAPbouiCOM.Application FSBOApp)
        {
            String NomCol;
            String NomCol2;
            Int32 i;
            Int32 c = 0;
            Boolean bIden;
            String[] Col2;
            try
            {
                s = odt.GetAsXML();
                s = @"<LceCoCierre version=""1.0""><LceDiarioRes version=""1.0""><DocumentoDiarioRes ID=""DIARIO_RES_{0}"">";
                s = String.Format(s, Periodo.Replace(".", "-"));

                NomCol = odt.Columns.Item(0).Name;
                var Col = NomCol.Split('/');
                s = s + @"<{0}><{1}>{2}</{1}>";
                s = String.Format(s, Col[0], Col[1], ((System.String)odt.GetValue(0, 0)).Trim());

                NomCol = odt.Columns.Item(1).Name;
                Col = NomCol.Split('/');
                s = s + @"<{0}><{1}>{2}</{1}>";
                s = String.Format(s, Col[1], Col[2], ((System.String)odt.GetValue(1, 0)).Trim());

                NomCol = odt.Columns.Item(2).Name;
                Col = NomCol.Split('/');
                s = s + @"<{0}>{1}</{0}></{2}></{3}>";
                //[Identificacion/PeriodoTributario/Final]
                s = String.Format(s, Col[2], ((System.String)odt.GetValue(2, 0)).Trim(), Col[1], Col[0]);

                //RegistroDiario
                for (i = 0; i < odt.Rows.Count; i++)
                {
                    NomCol = odt.Columns.Item(3).Name;
                    Col = NomCol.Split('/');
                    var sFin = Col[0];
                    s = s + @"<{0}>";
                    s = String.Format(s, sFin);

                    for (c = 3; c < odt.Columns.Count; c++)
                    {
                        NomCol = odt.Columns.Item(c).Name;
                        Col = NomCol.Split('/');
                        if (Col[0] != sFin)
                            break;
                        s = s + @"<{0}>{1}</{0}>";
                        s = String.Format(s, Col[1], ((System.String)odt.GetValue(c, i)).Trim());
                    }
                    s = s + @"</{0}>";
                    s = String.Format(s, sFin);
                }
                //Cierre
                for (Int32 x = 0; x == 0; x++)
                {
                    NomCol = odt.Columns.Item(c).Name;
                    Col = NomCol.Split('/');
                    var sFin = Col[0];
                    s = s + @"<{0}>";
                    s = String.Format(s, sFin);

                    for (Int32 z = c; z < odt.Columns.Count; z++)
                    {
                        NomCol = odt.Columns.Item(z).Name;
                        Col = NomCol.Split('/');
                        if (Col[0] != sFin)
                            break;
                        s = s + @"<{0}>{1}</{0}>";
                        s = String.Format(s, Col[1], ((System.String)odt.GetValue(z, x)).Trim());
                    }
                    s = s + @"</{0}>";
                    s = String.Format(s, sFin);
                }
                s = s + @"</DocumentoDiarioRes></LceDiarioRes>"; //</LceCoCierre>";

                //Inicia Libro Mayor
                s = s + @"<LceMayorRes version=""1.0""><DocumentoMayorRes ID=""MAYOR_RES_{0}"">";
                s = String.Format(s, Periodo.Replace(".", "-"));

                //Identificacion
                NomCol = odt2.Columns.Item(0).Name;
                Col = NomCol.Split('/');
                s = s + @"<{0}><{1}>{2}</{1}>";
                s = String.Format(s, Col[0], Col[1], ((System.String)odt2.GetValue(0, 0)).Trim());

                NomCol = odt2.Columns.Item(1).Name;
                Col = NomCol.Split('/');
                s = s + @"<{0}><{1}>{2}</{1}>";
                s = String.Format(s, Col[1], Col[2], ((System.String)odt2.GetValue(1, 0)).Trim());

                NomCol = odt2.Columns.Item(2).Name;
                Col = NomCol.Split('/');
                s = s + @"<{0}>{1}</{0}></{2}></{3}>";
                //[Identificacion/PeriodoTributario/Final]
                s = String.Format(s, Col[2], ((System.String)odt2.GetValue(2, 0)).Trim(), Col[1], Col[0]);

                //Cuenta
                for (i = 0; i < odt2.Rows.Count; i++)
                {
                    NomCol = odt2.Columns.Item(3).Name;
                    Col = NomCol.Split('/');
                    var sFin = Col[0];
                    s = s + @"<{0}>";
                    s = String.Format(s, sFin);

                    for (c = 3; c < odt2.Columns.Count; c++)
                    {
                        NomCol = odt2.Columns.Item(c).Name;
                        Col = NomCol.Split('/');
                        if (Col[0] != sFin)
                            break;

                        if (Col.Count() == 2)
                        {
                            s = s + @"<{0}>{1}</{0}>";
                            s = String.Format(s, Col[1], ((System.String)odt2.GetValue(c, i)).Trim());
                        }
                        else
                        {
                            var ff = Col[1];
                            s = s + @"<{0}>";
                            s = String.Format(s, ff);//coloca Inicio Cierre

                            //MontosPeriodo - Debe
                            s = s + @"<{0}><{1}>{2}</{1}>";
                            s = String.Format(s, Col[2], Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca Debe
                            c++;

                            //MontosPeriodo - Haber
                            NomCol = odt2.Columns.Item(c).Name;
                            Col = NomCol.Split('/');
                            s = s + @"<{0}>{1}</{0}>";
                            s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca Haber
                            c++;

                            //MontosPeriodo - Deudor
                            if (((System.String)odt2.GetValue(c, i)).Trim() != "0")
                            {
                                NomCol = odt2.Columns.Item(c).Name;
                                Col = NomCol.Split('/');
                                s = s + @"<{0}>{1}</{0}>";
                                s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca Deudor
                            }
                            c++;

                            //MontosPeriodo - Acreedor
                            if (((System.String)odt2.GetValue(c, i)).Trim() != "0")
                            {
                                NomCol = odt2.Columns.Item(c).Name;
                                Col = NomCol.Split('/');
                                s = s + @"<{0}>{1}</{0}>";
                                s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca Acreedor
                            }
                            s = s + @"</{0}>"; //Coloca Final MontosPeriodo
                            s = String.Format(s, Col[2]);
                            c++;

                            //**Agrega Montos Acumulados
                            NomCol = odt2.Columns.Item(c).Name;
                            Col = NomCol.Split('/');
                            s = s + @"<{0}>";
                            s = String.Format(s, Col[2]);//coloca Inicio MontosAcumulado

                            //MontosAcumulado - Debe
                            s = s + @"<{0}>{1}</{0}>";
                            s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca Debe
                            c++;

                            //MontosAcumulado - Haber
                            NomCol = odt2.Columns.Item(c).Name;
                            Col = NomCol.Split('/');
                            s = s + @"<{0}>{1}</{0}>";
                            s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca Haber
                            c++;

                            if (((System.String)odt2.GetValue(c, i)).Trim() != "0")
                            {
                                NomCol = odt2.Columns.Item(c).Name;
                                Col = NomCol.Split('/');
                                s = s + @"<{0}>{1}</{0}>";
                                s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca SaldoDeudor
                            }
                            c++;

                            if (((System.String)odt2.GetValue(c, i)).Trim() != "0")
                            {
                                NomCol = odt2.Columns.Item(c).Name;
                                Col = NomCol.Split('/');
                                s = s + @"<{0}>{1}</{0}>";
                                s = String.Format(s, Col[3], ((System.String)odt2.GetValue(c, i)).Trim());//coloca SaldoAcreedor
                            }
                            s = s + @"</{0}>"; //Coloca Final MontosAcumulado
                            s = String.Format(s, Col[2]);

                            c++;

                            s = s + @"</{0}>"; //Coloca Final Cierre
                            s = String.Format(s, ff);
                        }
                    }
                    s = s + @"</{0}>";
                    s = String.Format(s, sFin); //Cierra Cuenta
                }
                //[Cuenta/CodigoCuenta]
                //[Cuenta/Cierre/MontosPeriodo/Deudor]
                s = s + @"</DocumentoMayorRes></LceMayorRes></LceCoCierre>";


                return s;
            }
            catch (Exception z)
            {
                FSBOApp.StatusBar.SetText(z.Message + " ** Trace: " + z.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                //OutLog("CrearXMLDiccionario: " + z.Message + " ** Trace: " + z.StackTrace);
                return "";
            }
        }
    }
}
