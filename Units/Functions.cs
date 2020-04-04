using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using VisualD.MainObjBase;
using VisualD.MenuConfFr; 
using VisualD.GlobalVid;
using VisualD.SBOFunctions; 
using VisualD.vkBaseForm;
using VisualD.vkFormInterface;
using VisualD.MultiFunctions;
using VisualD.SBOGeneralService;
using System.Xml;
using System.IO;
using System.Web;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.ReportSource;

namespace Factura_Electronica_VK.Functions
{
    class TFunctions
    {
        private String s;
        //private SAPbobsCOM.Recordset oRecordSet;
        public VisualD.SBOFunctions.CSBOFunctions SBO_f;

        public String sConexion(String Servidor, String Base, String Usuario, String Password)
        {
            Boolean Valida = true;
            String Texto = "";

            try
            {
                if (Servidor == "")
                {
                    Texto = "Debe ingresar Servidor";
                    Valida = false;
                }

                if ((Usuario == "") && (Valida))
                {
                    Texto = "Debe ingresar Usuario";
                    Valida = false;
                }

                if ((Password == "") && (Valida))
                {
                    Texto = "Debe ingresar Password";
                    Valida = false;
                }

                if ((Base == "") && (Valida))
                {
                    Texto = "Debe ingresar Base de datos FE";
                    Valida = false;
                }

                if (Valida)
                {
                    s = "Password={0};Persist Security Info=True;User ID={1};Initial Catalog={2};Data Source={3}";
                    Texto = String.Format(s, Password, Usuario, Base, Servidor);
                    return Texto;
                }
                else
                {
                    return Texto;
                }
            }
            catch(Exception e)
            {
                SBO_f.oLog.OutLog("sConexion : " + e.Message + " ** Trace: " + e.StackTrace);
                return "EsConexion : " + e.Message + " ** Trace: " + e.StackTrace;
            }
        }


        public String ValidarRut(String rut_Verificar)
        {
            String rutLimpio;
            Int32 rut;
            String digitoVerificador;
            String rutsindigito;
            String RutDigito;
            Int32 Acumulador = 0;
            Int32 Digito;
            Int32 Multiplo;
            Int32 Contador = 2;
            String retorno = "";

            try
            {
                rut_Verificar = rut_Verificar.Trim();
                rutLimpio = rut_Verificar.Replace(".", "");
                rutLimpio = rutLimpio.Replace("-", "");
                rutLimpio = rutLimpio.Replace(" ", "");
                rutLimpio = rutLimpio.Substring(0, rutLimpio.Length - 1);

                digitoVerificador = rut_Verificar.Substring(rut_Verificar.Length - 1, 1);
                rutsindigito = rutLimpio; //rut_Verificar.Substring(0, rut_Verificar.Length - 1);

                rut = Convert.ToInt32(rutsindigito);
                while (rut != 0)
                {
                    Multiplo = (rut % 10) * Contador;
                    Acumulador = Acumulador + Multiplo;
                    rut = rut / 10;
                    Contador = Contador + 1;
                    if (Contador == 8)
                    {
                        Contador = 2;
                    }
                }

                Digito = 11 - (Acumulador % 11);
                RutDigito = Digito.ToString().Trim();
                if (Digito == 10) RutDigito = "K";

                if (Digito == 11) RutDigito = "0";


                if (RutDigito == digitoVerificador.ToUpper())
                {
                    retorno = "OK";
                }
                else
                {
                    retorno = "RUT invalido";
                }

                return retorno;
            }
            catch (Exception e)
            {
                return "Error validar RUT (2), " + e.Message; 
            }
        }//fin ValidarRut


        public Boolean ParamAdd(SAPbouiCOM.DBDataSource oDBDSHeader)
        {
            SAPbobsCOM.GeneralService oParamAdd = null;
            SAPbobsCOM.GeneralData oParamAddData = null;
            SAPbobsCOM.GeneralDataCollection oParamAddLines = null;
            SAPbobsCOM.GeneralDataParams oParamAddParameter = null;
            TSBOGeneralService oGen = null;
            //String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;
            //Int32 i;

            Cmpny = SBO_f.Cmpny;
            try
            {
                CmpnyService = Cmpny.GetCompanyService();
                oGen = new TSBOGeneralService();

                oGen.SBO_f = SBO_f;
                oParamAdd = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEPARAM")); //
                oParamAddData = (SAPbobsCOM.GeneralData)(oParamAdd.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oParamAddData);

                //Cmpny.StartTransaction();
                oParamAddParameter = oParamAdd.Add(oParamAddData);
                return true;
                //Result := System.int32(oRequerParameter.GetProperty('DocEntry')).ToString;
                //Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("ParamAdd: " + e.Message);
                //if (Cmpny.InTransaction) Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                return false;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oParamAdd);
                SBO_f._ReleaseCOMObject(oParamAddData);
                SBO_f._ReleaseCOMObject(oParamAddLines);
                SBO_f._ReleaseCOMObject(oParamAddParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }

        }//fin ParamAdd


        public Boolean ParamUpd(SAPbouiCOM.DBDataSource oDBDSHeader)
        {
            SAPbobsCOM.GeneralService oGeneralService = null;
            SAPbobsCOM.GeneralData oGeneralData = null;
            SAPbobsCOM.GeneralDataCollection oGeneralDataCollection = null;
            SAPbobsCOM.GeneralDataParams oGeneralDataParams = null;
            TSBOGeneralService oGen;
            String StrDummy;
 
            oGen = new TSBOGeneralService();
            oGen.SBO_f = SBO_f;

            try
            {
                oGeneralService = (SAPbobsCOM.GeneralService)(SBO_f.Cmpny.GetCompanyService().GetGeneralService("VID_FEPARAM"));
                oGeneralDataParams = (SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = "1";
                oGeneralDataParams.SetProperty("Code", StrDummy);
                oGeneralData = oGeneralService.GetByParams(oGeneralDataParams);

                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGeneralData);
     
                SBO_f.Cmpny.StartTransaction();
                oGeneralService.Update(oGeneralData);
                SBO_f.Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                return true;
            }
            catch//(Exception e)
            {
                return false;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGeneralService);
                SBO_f._ReleaseCOMObject(oGeneralData);
                SBO_f._ReleaseCOMObject(oGeneralDataCollection);
                SBO_f._ReleaseCOMObject(oGeneralDataParams);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin ParamUpd


        public Int32 FELOGAdd(Int32 DocEntry, String ObjType, String SubType, String SeriePE, Int32 FolioNum, String Status, String sMessage, String TipoDoc, String UserCode, String JsonText, String Id, String Validation, String sDocDate)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            SAPbobsCOM.GeneralDataCollection oFELOGLines = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            TSBOGeneralService oGen;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;

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
                if (sDocDate != "")
                    oFELOGData.SetProperty("U_DocDate", SBO_f.StrToDate(sDocDate));

                if (JsonText != null)
                    oFELOGData.SetProperty("U_Json", JsonText);
                
                if (SeriePE != null)
                    oFELOGData.SetProperty("U_SeriePE", SeriePE);
                
                if (Id != null)
                    oFELOGData.SetProperty("U_Id", Id);
                
                if (Validation != null)
                    oFELOGData.SetProperty("U_Validation", Validation);

                //Add the new row, including children, to database
                //oGeneralParams := oGeneralService.Add(oGeneralData);


                ////---
                ////oFELOG     := GeneralService(CmpnyService.GetGeneralService('VID_FELOG'));
                ////oFELOGData := GeneralData(oFELOG.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData));
                ////oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oFELOGData);

                //Cmpny.StartTransaction();
                oFELOGParameter = oFELOG.Add(oFELOGData);
                return (System.Int32)(oFELOGParameter.GetProperty("DocEntry"));
            
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("Insertar datos en FELOG: " + e.Message + " ** Trace: " + e.StackTrace);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oFELOG);
                SBO_f._ReleaseCOMObject(oFELOGData);
                SBO_f._ReleaseCOMObject(oFELOGLines);
                SBO_f._ReleaseCOMObject(oFELOGParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }

        }//fin FELOGAdd


        public Int32 FELOGUpt(SAPbouiCOM.DBDataSource oDBDSHeader, SAPbouiCOM.DBDataSource oDBDSD)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            SAPbobsCOM.GeneralDataCollection oFELOGLines = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            TSBOGeneralService oGen;
            String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;
                oFELOG = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FELOG"));
                oFELOGParameter = (SAPbobsCOM.GeneralDataParams)(oFELOG.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = Convert.ToString(oDBDSHeader.GetValue("DocEntry",0));
                oFELOGParameter.SetProperty("DocEntry", StrDummy);
                oFELOGData = oFELOG.GetByParams(oFELOGParameter);

                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oFELOGData);

                if (oDBDSD != null)
                {
                    if (oDBDSD.Size > 0)
                    {
                        StrDummy = "VID_FELOGD";
                        oFELOGLines = oFELOGData.Child(StrDummy);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSD, oFELOGData, oFELOGLines);
                    }
                }

                //Cmpny.StartTransaction();
                oFELOG.Update(oFELOGData);
                return Convert.ToInt32(oDBDSHeader.GetValue("DocEntry",0));
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("Actualizar tabla FELOG: " + e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oFELOG);
                SBO_f._ReleaseCOMObject(oFELOGData);
                SBO_f._ReleaseCOMObject(oFELOGLines);
                SBO_f._ReleaseCOMObject(oFELOGParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin FELOGUpt


        public Int32 FELOGUptM(Int32 DocEntry, Int32 DocEntryDoc, String ObjType, String SubType, String SeriePE, Int32 FolioNum, String Status, String sMessage, String TipoDoc, String UserCode, String JsonText, String Id, String Validation, String sDocDate)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            SAPbobsCOM.GeneralDataCollection oFELOGLines = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            TSBOGeneralService oGen;
            String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;

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
                if (sDocDate != "")
                    oFELOGData.SetProperty("U_DocDate", SBO_f.StrToDate(sDocDate));

                //oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oFELOGData);

                //Cmpny.StartTransaction();
                oFELOG.Update(oFELOGData);
                //Result :=Convert.ToInt32(TMultiFunctions.Trim(System.String(oFELOGData.GetProperty('DocEntry'))));
                return (System.Int32)(oFELOGData.GetProperty("DocEntry"));
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("Actualizar tabla FELOG: " + e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                    //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oFELOG);
                SBO_f._ReleaseCOMObject(oFELOGData);
                SBO_f._ReleaseCOMObject(oFELOGLines);
                SBO_f._ReleaseCOMObject(oFELOGParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin FELOGUptM


        public Boolean ImpAdicAdd(SAPbouiCOM.DBDataSource oDBDSHeader)
        {
            SAPbobsCOM.GeneralService oImpAdicAdd = null;
            SAPbobsCOM.GeneralData oImpAdicAddData = null;
            SAPbobsCOM.GeneralDataCollection oImpAdicAddLines = null;
            SAPbobsCOM.GeneralDataParams oImpAdicAddParameter = null;
            TSBOGeneralService oGen;
            //String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;
            //Int32 i; 

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;  
                oImpAdicAdd = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEIMPADIC")); //
                oImpAdicAddData = (SAPbobsCOM.GeneralData)(oImpAdicAdd.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oImpAdicAddData);

                Cmpny.StartTransaction();
                oImpAdicAddParameter = oImpAdicAdd.Add(oImpAdicAddData);
                //Result := System.int32(oRequerParameter.GetProperty('DocEntry')).ToString;
                Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                return true;
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("ImpAdicAdd : " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
                //FSBOApp.StatusBar.SetText('AddDatos : ' + e.Message + ' ** Trace: ' + e.StackTrace , BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oImpAdicAdd);
                SBO_f._ReleaseCOMObject(oImpAdicAddData);
                SBO_f._ReleaseCOMObject(oImpAdicAddLines);
                SBO_f._ReleaseCOMObject(oImpAdicAddParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin ImpAdicAdd


        public Boolean ImpAdicDel(ref SAPbobsCOM.Recordset oRecordSet)
        {
            SAPbobsCOM.GeneralService oGeneralService = null;
            SAPbobsCOM.GeneralData oGeneralData = null;
            SAPbobsCOM.GeneralDataCollection oGeneralDataCollection = null;
            SAPbobsCOM.GeneralDataParams oGeneralDataParams = null;
            TSBOGeneralService oGen;
            String StrDummy;
            Int32 i;

            oGen = new TSBOGeneralService();
            try
            {
                oGen.SBO_f = SBO_f;
                oGeneralService = (SAPbobsCOM.GeneralService)(SBO_f.Cmpny.GetCompanyService().GetGeneralService("VID_FEIMPADIC"));
                oGeneralDataParams = (SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));

                i = 1;
                while (! oRecordSet.EoF)
                {
                    StrDummy = (System.String)(oRecordSet.Fields.Item("Code").Value);
                    oGeneralDataParams.SetProperty("Code", StrDummy);
                    oGeneralService.Delete(oGeneralDataParams);
                    oRecordSet.MoveNext();
                }
      
                return true;
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("ImpAdicDel " + e.Message);
                return false;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGeneralService);
                SBO_f._ReleaseCOMObject(oGeneralData);
                SBO_f._ReleaseCOMObject(oGeneralDataCollection);
                SBO_f._ReleaseCOMObject(oGeneralDataParams);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin ImpAdicDel


        public System.Data.DataTable GetDataTable(String prmSQL, SqlConnection sqlCnn)
        {
            Boolean bEstadoConexionModificado = false;
            SqlDataAdapter cmd;
            System.Data.DataTable resultDataTable;

            try
            {
                if (sqlCnn.State == ConnectionState.Closed)
                {
                    sqlCnn.Open();
                    bEstadoConexionModificado = true;
                }
    
                cmd = new  SqlDataAdapter(prmSQL, sqlCnn);
                resultDataTable = new System.Data.DataTable();
                cmd.SelectCommand.CommandTimeout = 0;  
                cmd.Fill(resultDataTable);
                return resultDataTable;
            }
            catch //(Exception e)
            {
                return null;
            }
            finally
            {
                if (bEstadoConexionModificado)
                {
                    if (sqlCnn.State == ConnectionState.Open) sqlCnn.Close();
                }
            }
        }//fin GetDataTable


        public Boolean CAFAdd(SAPbouiCOM.DBDataSource oDBDSHeader)
        {
            SAPbobsCOM.GeneralService oGSAdd = null;
            SAPbobsCOM.GeneralData oGDAddData = null;
            SAPbobsCOM.GeneralDataCollection oGDCAddLines = null;
            SAPbobsCOM.GeneralDataParams oGDPAddParameter = null;
            TSBOGeneralService oGen;
            //String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;
            //Int32 i; 

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;  
                oGSAdd = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FECAF")); //
                oGDAddData = (SAPbobsCOM.GeneralData)(oGSAdd.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGDAddData);

                Cmpny.StartTransaction();
                oGDPAddParameter = oGSAdd.Add(oGDAddData);
                //Result := System.int32(oRequerParameter.GetProperty('DocEntry')).ToString;
                Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                return true;
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("AddDatos : " + e.Message + " ** Trace: " + e.StackTrace);
                if (Cmpny.InTransaction)
                    Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                return false;
                //FSBOApp.StatusBar.SetText('AddDatos : ' + e.Message + ' ** Trace: ' + e.StackTrace , BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGSAdd);
                SBO_f._ReleaseCOMObject(oGDAddData);
                SBO_f._ReleaseCOMObject(oGDCAddLines);
                SBO_f._ReleaseCOMObject(oGDPAddParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin CADAdd


        public Boolean CAFUpd(SAPbouiCOM.DBDataSource oDBDSHeader)
        {
            SAPbobsCOM.GeneralService oGeneralService = null;
            SAPbobsCOM.GeneralData oGeneralData = null;
            SAPbobsCOM.GeneralDataCollection oGeneralDataCollection = null;
            SAPbobsCOM.GeneralDataParams oGeneralDataParams = null;
            TSBOGeneralService oGen;
            String StrDummy;

            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;
                oGeneralService = (SAPbobsCOM.GeneralService)(SBO_f.Cmpny.GetCompanyService().GetGeneralService("VID_FECAF"));
                oGeneralDataParams = (SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = (System.String)(oDBDSHeader.GetValue("Code", 0).Trim());
                oGeneralDataParams.SetProperty("Code", StrDummy);
                oGeneralData = oGeneralService.GetByParams(oGeneralDataParams);

                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGeneralData);
     
                //SBO_f.Cmpny.StartTransaction;
                oGeneralService.Update(oGeneralData);
                return true;

                //SBO_f.Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch //(Exception e)
            {
                return false;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGeneralService);
                SBO_f._ReleaseCOMObject(oGeneralData);
                SBO_f._ReleaseCOMObject(oGeneralDataCollection);
                SBO_f._ReleaseCOMObject(oGeneralDataParams);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin CAFUpd


        public Boolean FESUCAdd(SAPbouiCOM.DBDataSource oDBDSHeader)
        {
            SAPbobsCOM.GeneralService oGSAdd = null;
            SAPbobsCOM.GeneralData oGDAddData = null;
            SAPbobsCOM.GeneralDataCollection oGDCAddLines = null;
            SAPbobsCOM.GeneralDataParams oGDPAddParameter = null;
            TSBOGeneralService oGen;
            //String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;
            //Int32 i;

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;
                oGSAdd = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FESUC")); //
                oGDAddData = (SAPbobsCOM.GeneralData)(oGSAdd.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGDAddData);

                Cmpny.StartTransaction();
                oGDPAddParameter = oGSAdd.Add(oGDAddData);
                //Result := System.int32(oRequerParameter.GetProperty('DocEntry')).ToString;
                Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                return true;
            }
            catch //(Exception e)
            {
                //SBO_f.oLog.OutLog('AddDatos : ' + e.Message + ' ** Trace: ' + e.StackTrace);
                return false;
                //FSBOApp.StatusBar.SetText('AddDatos : ' + e.Message + ' ** Trace: ' + e.StackTrace , BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGSAdd);
                SBO_f._ReleaseCOMObject(oGDAddData);
                SBO_f._ReleaseCOMObject(oGDCAddLines);
                SBO_f._ReleaseCOMObject(oGDPAddParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin FESUCAdd


        public Boolean FESUCUpd(SAPbouiCOM.DBDataSource oDBDSHeader)
        {
            SAPbobsCOM.GeneralService oGeneralService = null;
            SAPbobsCOM.GeneralData oGeneralData = null;
            SAPbobsCOM.GeneralDataCollection oGeneralDataCollection = null;
            SAPbobsCOM.GeneralDataParams oGeneralDataParams = null;
            TSBOGeneralService oGen;
            String StrDummy;
            
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;
                oGeneralService = (SAPbobsCOM.GeneralService)(SBO_f.Cmpny.GetCompanyService().GetGeneralService("VID_FESUC"));
                oGeneralDataParams = (SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = (System.String)(oDBDSHeader.GetValue("Code", 0).Trim());
                oGeneralDataParams.SetProperty("Code", StrDummy);
                oGeneralData = oGeneralService.GetByParams(oGeneralDataParams);

                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGeneralData);
     
                SBO_f.Cmpny.StartTransaction();
                oGeneralService.Update(oGeneralData);

                SBO_f.Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                return true;
            }
            catch //(Exception e)
            {
                if (SBO_f.Cmpny.InTransaction) SBO_f.Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                return false;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGeneralService);
                SBO_f._ReleaseCOMObject(oGeneralData);
                SBO_f._ReleaseCOMObject(oGeneralDataCollection);
                SBO_f._ReleaseCOMObject(oGeneralDataParams);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin FESUCUpd


        public Int32 FEDistAdd(SAPbouiCOM.DBDataSource oDBDSHeader, SAPbouiCOM.DBDataSource oDBDSD)
        {
            SAPbobsCOM.GeneralService oGS = null;
            SAPbobsCOM.GeneralData oGData = null;
            SAPbobsCOM.GeneralDataCollection oGLines = null;
            SAPbobsCOM.GeneralDataParams oGParameter = null;
            TSBOGeneralService oGen;
            String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;
            //Int32 i;
 
            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;
                oGS = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEDIST"));
                oGData = (SAPbobsCOM.GeneralData)(oGS.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGData);

                if (oDBDSD != null)
                {
                    StrDummy = "VID_FEDISTD";
                    oGLines = oGData.Child(StrDummy);
                    oGen.SetNewDataSourceLines_InUDO(oDBDSD, oGData, oGLines);
                }

                //Cmpny.StartTransaction();
                oGParameter = oGS.Add(oGData);
                return (System.Int32)(oGParameter.GetProperty("DocEntry"));
            
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("Insertar Folios a Distribuir: " + e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                    //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGS);
                SBO_f._ReleaseCOMObject(oGData);
                SBO_f._ReleaseCOMObject(oGLines);
                SBO_f._ReleaseCOMObject(oGParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin FEDistAdd


        public Int32 FEDistUpt(SAPbouiCOM.DBDataSource oDBDSHeader, SAPbouiCOM.DBDataSource oDBDSD)
        {
            SAPbobsCOM.GeneralService oGS = null;
            SAPbobsCOM.GeneralData oGData = null;
            SAPbobsCOM.GeneralDataCollection oGLines = null;
            SAPbobsCOM.GeneralDataParams oGParameter = null;
            TSBOGeneralService oGen;
            String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;
      
                oGS = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEDIST"));
                oGParameter = (SAPbobsCOM.GeneralDataParams)(oGS.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = Convert.ToString(oDBDSHeader.GetValue("DocEntry",0));
                oGParameter.SetProperty("DocEntry", StrDummy);
                oGData = oGS.GetByParams(oGParameter);

                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGData);

                if (oDBDSD != null)
                {
                    StrDummy = "VID_FEDISTD";
                    oGLines = oGData.Child(StrDummy);
                    oGen.SetNewDataSourceLines_InUDO(oDBDSD, oGData, oGLines);
                }

                //Cmpny.StartTransaction();
                oGS.Update(oGData);
                return Convert.ToInt32(oDBDSHeader.GetValue("DocEntry",0));
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("Actualizar Distribuir Folios: " + e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                    //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGS);
                SBO_f._ReleaseCOMObject(oGData);
                SBO_f._ReleaseCOMObject(oGLines);
                SBO_f._ReleaseCOMObject(oGParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }

        }//fin FEDistUpt


        public Int32 ActEstadoFolioUpt(Int32 DocEntry, Int32 LineId, Double Folio, String TipoDoc, String DocEntryDoc, String ObjType, String SubType)
        {
            SAPbobsCOM.GeneralService oGS = null;
            SAPbobsCOM.GeneralData oGData = null;
            SAPbobsCOM.GeneralDataCollection oGLines = null;
            SAPbobsCOM.GeneralDataParams oGParameter = null;
            TSBOGeneralService oGen;
            String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;
      
                oGS = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEDIST"));
                oGParameter = (SAPbobsCOM.GeneralDataParams)(oGS.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                //StrDummy = TMultiFunctions.IntToStr(DocEntry);
                StrDummy = DocEntry.ToString();
                oGParameter.SetProperty("DocEntry", StrDummy);
                oGData = oGS.GetByParams(oGParameter);

                StrDummy = "VID_FEDISTD";
                oGLines = oGData.Child(StrDummy);
                oGLines.Item(LineId-1).SetProperty("U_Estado", "U");
                if (DocEntryDoc != "")
                {   oGLines.Item(LineId-1).SetProperty("U_DocEntry", DocEntryDoc); }
                if (ObjType != "")
                {   oGLines.Item(LineId-1).SetProperty("U_ObjType", ObjType); }
                if (SubType != "")
                {   oGLines.Item(LineId-1).SetProperty("U_SubType", SubType); }


                //Cmpny.StartTransaction();
                oGS.Update(oGData);
                return (System.Int32)(oGParameter.GetProperty("DocEntry"));
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("Actualizar Estado de Folio: " + TipoDoc +"-"+ Convert.ToString(Folio) +", "+ e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                    //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGS);
                SBO_f._ReleaseCOMObject(oGData);
                SBO_f._ReleaseCOMObject(oGLines);
                SBO_f._ReleaseCOMObject(oGParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }

        }//fin ActEstadoFolioUpt


        public String sNuevoDocEntryLargo(String sprmTabla, Boolean RunningUnderSQLServer)
        {
            SAPbobsCOM.Recordset oRecordSet = (SAPbobsCOM.Recordset)(SBO_f.Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            String sSQl;

            try
            {
                if (RunningUnderSQLServer)
                {   sSQl = @"SELECT CASE WHEN MAX(DocEntry) IS NULL THEN RIGHT(REPLICATE('0',8)+'1',8) ELSE RIGHT(REPLICATE('0',8)+Cast(MAX(DocEntry)+1 as varchar(20)),8) end AS iDocEntry 
                                     FROM [{0}]"; }
                else
                {   sSQl = @"SELECT CASE WHEN COUNT(*) = 0 THEN RIGHT('00000000'+'1',8) ELSE RIGHT('00000000'+Cast(MAX(""DocEntry"")+1 as varchar(20)),8) end AS ""iDocEntry"" 
                               FROM ""{0}"" "; }
                sSQl = String.Format(sSQl, sprmTabla);
                oRecordSet.DoQuery (sSQl);
                return oRecordSet.Fields.Item("iDocEntry").Value.ToString();
            }
            catch //(Exception e)
            {
                return null;
            }
        }//fin sNuevoDocEntryLargo


        public Int32 FEAsigAdd(SAPbouiCOM.DBDataSource oDBDSHeader, SAPbouiCOM.DBDataSource oDBDSD)
        {
            SAPbobsCOM.GeneralService oGS = null;
            SAPbobsCOM.GeneralData oGData = null;
            SAPbobsCOM.GeneralDataCollection oGLines = null;
            SAPbobsCOM.GeneralDataParams oGParameter = null;
            TSBOGeneralService oGen;  
            String StrDummy;   
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;
            //Int32 i; 

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;  
                oGS = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEASIGFOL"));
                oGData = (SAPbobsCOM.GeneralData)(oGS.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGData);

                if (oDBDSD != null)
                {
                    StrDummy = "VID_FEASIGFOLD";
                    oGLines = oGData.Child(StrDummy);
                    oGen.SetNewDataSourceLines_InUDO(oDBDSD, oGData, oGLines);
                }

                //Cmpny.StartTransaction();
                oGParameter = oGS.Add(oGData);
                return (System.Int32)(oGParameter.GetProperty("DocEntry"));
            
      //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch(Exception e)
            {
                SBO_f.oLog.OutLog("Insertar Asignar Folios: "+ e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                    //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGS);
                SBO_f._ReleaseCOMObject(oGData);
                SBO_f._ReleaseCOMObject(oGLines);
                SBO_f._ReleaseCOMObject(oGParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin FEAsigAdd


        public Int32 FEAsigUpt(SAPbouiCOM.DBDataSource oDBDSHeader, SAPbouiCOM.DBDataSource oDBDSD)
        {
            SAPbobsCOM.GeneralService oGS = null;
            SAPbobsCOM.GeneralData oGData = null;
            SAPbobsCOM.GeneralDataCollection oGLines = null;
            SAPbobsCOM.GeneralDataParams oGParameter = null;
            TSBOGeneralService oGen;
            String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;
      
                oGS = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEASIGFOL"));
                oGParameter = (SAPbobsCOM.GeneralDataParams)(oGS.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = Convert.ToString(oDBDSHeader.GetValue("DocEntry",0));
                oGParameter.SetProperty("DocEntry", StrDummy);
                oGData = oGS.GetByParams(oGParameter);

                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGData);

                if (oDBDSD != null)
                {
                    StrDummy = "VID_FEASIGFOLD";
                    oGLines = oGData.Child(StrDummy);
                    oGen.SetNewDataSourceLines_InUDO(oDBDSD, oGData, oGLines);
                }

                //Cmpny.StartTransaction();
                oGS.Update(oGData);
                return Convert.ToInt32(oDBDSHeader.GetValue("DocEntry",0));
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("Actualizar Asignar Folios: "+ e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                    //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGS);
                SBO_f._ReleaseCOMObject(oGData);
                SBO_f._ReleaseCOMObject(oGLines);
                SBO_f._ReleaseCOMObject(oGParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin FEAsigUpt

        public Int32 FEMultiSocUpt(String DocEntry, String Servidor, String RUT, String Base, String Usuario, String Password, String Sociedad, String Habilitada)
        {
            SAPbobsCOM.GeneralService oFE = null;
            SAPbobsCOM.GeneralData oFEData = null;
            SAPbobsCOM.GeneralDataCollection oFELines = null;
            SAPbobsCOM.GeneralDataParams oFEParameter = null;
            TSBOGeneralService oGen;
            String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;

                oFE = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEMULTISOC"));
                oFEParameter = (SAPbobsCOM.GeneralDataParams)(oFE.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = DocEntry.Trim();
                oFEParameter.SetProperty("DocEntry", StrDummy);
                oFEData = oFE.GetByParams(oFEParameter);
                oFEData.SetProperty("U_Servidor", Servidor.Trim());
                oFEData.SetProperty("U_RUT", RUT.Trim());
                oFEData.SetProperty("U_Base", Base.Trim());
                oFEData.SetProperty("U_Usuario", Usuario.Trim());
                oFEData.SetProperty("U_Password", Password.Trim());
                oFEData.SetProperty("U_Sociedad", Sociedad.Trim());
                oFEData.SetProperty("U_Habilitada", Habilitada.Trim());

                //oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oFELOGData);

                //Cmpny.StartTransaction();
                oFE.Update(oFEData);
                //Result :=Convert.ToInt32(TMultiFunctions.Trim(System.String(oFELOGData.GetProperty('DocEntry'))));
                return (System.Int32)(oFEData.GetProperty("DocEntry"));
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("Actualizar tabla Multiples Sociedades: "+ e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                    //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oFE);
                SBO_f._ReleaseCOMObject(oFEData);
                SBO_f._ReleaseCOMObject(oFELines);
                SBO_f._ReleaseCOMObject(oFEParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin FEMultiSocUpt


        public Int32 FEMultiSocAdd(String Servidor, String RUT, String Base, String Usuario, String Password, String Sociedad, String Habilitada)
        {
            SAPbobsCOM.GeneralService oFE = null;
            SAPbobsCOM.GeneralData oFEData = null;
            SAPbobsCOM.GeneralDataCollection oFELines = null;
            SAPbobsCOM.GeneralDataParams oFEParameter = null;
            TSBOGeneralService oGen;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;  

                //Get GeneralService (oCmpSrv is the CompanyService)
                oFE = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEMULTISOC"));

                //Create data for new row in main UDO
                oFEData = (SAPbobsCOM.GeneralData)(oFE.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oFEData.SetProperty("U_Servidor", Servidor.Trim());
                oFEData.SetProperty("U_Base", Base.Trim());
                oFEData.SetProperty("U_RUT", RUT.Trim());
                oFEData.SetProperty("U_Usuario", Usuario.Trim());
                oFEData.SetProperty("U_Password", Password.Trim());
                oFEData.SetProperty("U_Sociedad", Sociedad.Trim());
                oFEData.SetProperty("U_Habilitada", Habilitada.Trim());

                //Add the new row, including children, to database
                //oGeneralParams := oGeneralService.Add(oGeneralData);

                //Cmpny.StartTransaction();
                oFEParameter = oFE.Add(oFEData);
                return (System.Int32)(oFEParameter.GetProperty("DocEntry"));
            
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("Insertar datos en VID_FEMULTISOC: "+ e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                    //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oFE);
                SBO_f._ReleaseCOMObject(oFEData);
                SBO_f._ReleaseCOMObject(oFELines);
                SBO_f._ReleaseCOMObject(oFEParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin FEMultiSocAdd


        public String Encriptar(String _cadenaAencriptar)
        {
            System.String sresult;
            System.Byte[] encryted;

            sresult = System.String.Empty;
            encryted = System.Text.Encoding.Unicode.GetBytes(_cadenaAencriptar);
            sresult = Convert.ToBase64String(encryted);
            return  sresult;
        }//fin Encriptar

        
        public String DesEncriptar(String _cadenaAdesencriptar)
        {
            String sresult;
            System.Byte[] decryted;

            sresult = System.String.Empty;
            decryted = Convert.FromBase64String(_cadenaAdesencriptar);
            //result = System       .Text.Encoding.Unicode.GetString(decryted, 0, decryted.ToArray().Length);
            sresult = System.Text.Encoding.Unicode.GetString(decryted);
            return  sresult;
        }


        public Int32 FEPROCAdd(SAPbouiCOM.DBDataSource oDBDSHeader)
        {
            SAPbobsCOM.GeneralService oGS = null;
            SAPbobsCOM.GeneralData oGData = null;
            SAPbobsCOM.GeneralDataCollection oGLines = null;
            SAPbobsCOM.GeneralDataParams oGParameter = null;
            TSBOGeneralService oGen;
            String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;
            //Int32 i;
 
            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;
                oGS = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEPROCED"));
                oGData = (SAPbobsCOM.GeneralData)(oGS.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGData);


                //Cmpny.StartTransaction();
                oGParameter = oGS.Add(oGData);
                return (System.Int32)(oGParameter.GetProperty("DocEntry"));
            
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("Insertar Procedimientos FE: " + e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                    //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGS);
                SBO_f._ReleaseCOMObject(oGData);
                SBO_f._ReleaseCOMObject(oGLines);
                SBO_f._ReleaseCOMObject(oGParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin FEPROCAdd


        public Int32 FEPROCUpt(SAPbouiCOM.DBDataSource oDBDSHeader)
        {
            SAPbobsCOM.GeneralService oGS = null;
            SAPbobsCOM.GeneralData oGData = null;
            SAPbobsCOM.GeneralDataCollection oGLines = null;
            SAPbobsCOM.GeneralDataParams oGParameter = null;
            TSBOGeneralService oGen;
            String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;

                oGS = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FEPROCED"));
                oGParameter = (SAPbobsCOM.GeneralDataParams)(oGS.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = oDBDSHeader.GetValue("DocEntry",0).ToString();
                oGParameter.SetProperty("DocEntry", StrDummy);
                oGData = oGS.GetByParams(oGParameter);

                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGData);

                //Cmpny.StartTransaction();
                oGS.Update(oGData);
                return Convert.ToInt32(oDBDSHeader.GetValue("DocEntry",0).ToString());
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("Actualizar Procedimientos FE: " + e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                    //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGS);
                SBO_f._ReleaseCOMObject(oGData);
                SBO_f._ReleaseCOMObject(oGLines);
                SBO_f._ReleaseCOMObject(oGParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }

        }//fin FEPROCUpt


        public Boolean PEImpAdd(SAPbouiCOM.DBDataSource oDBDSHeader)
        {
            SAPbobsCOM.GeneralService oImpAdicAdd = null;
            SAPbobsCOM.GeneralData oImpAdicAddData = null;
            SAPbobsCOM.GeneralDataCollection oImpAdicAddLines = null;
            SAPbobsCOM.GeneralDataParams oImpAdicAddParameter = null;
            TSBOGeneralService oGen;
            //String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;
            //Int32 i; 

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;
                oImpAdicAdd = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("FM_IVA")); //
                oImpAdicAddData = (SAPbobsCOM.GeneralData)(oImpAdicAdd.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oImpAdicAddData);

                //Cmpny.StartTransaction();
                oImpAdicAddParameter = oImpAdicAdd.Add(oImpAdicAddData);
                //Result := System.int32(oRequerParameter.GetProperty('DocEntry')).ToString;
                //Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                return true;
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("PEImpAdd : " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
                //FSBOApp.StatusBar.SetText('AddDatos : ' + e.Message + ' ** Trace: ' + e.StackTrace , BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oImpAdicAdd);
                SBO_f._ReleaseCOMObject(oImpAdicAddData);
                SBO_f._ReleaseCOMObject(oImpAdicAddLines);
                SBO_f._ReleaseCOMObject(oImpAdicAddParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin PEImpAdd


        public Boolean PEImpDel(ref SAPbobsCOM.Recordset oRecordSet)
        {
            SAPbobsCOM.GeneralService oGeneralService = null;
            SAPbobsCOM.GeneralData oGeneralData = null;
            SAPbobsCOM.GeneralDataCollection oGeneralDataCollection = null;
            SAPbobsCOM.GeneralDataParams oGeneralDataParams = null;
            TSBOGeneralService oGen;
            String StrDummy;
            Int32 i;

            oGen = new TSBOGeneralService();
            try
            {
                oGen.SBO_f = SBO_f;
                oGeneralService = (SAPbobsCOM.GeneralService)(SBO_f.Cmpny.GetCompanyService().GetGeneralService("FM_IVA"));
                oGeneralDataParams = (SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));

                i = 1;
                while (!oRecordSet.EoF)
                {
                    StrDummy = (System.String)(oRecordSet.Fields.Item("Code").Value);
                    oGeneralDataParams.SetProperty("Code", StrDummy);
                    oGeneralService.Delete(oGeneralDataParams);
                    oRecordSet.MoveNext();
                }

                return true;
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("PEImpDel " + e.Message);
                return false;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGeneralService);
                SBO_f._ReleaseCOMObject(oGeneralData);
                SBO_f._ReleaseCOMObject(oGeneralDataCollection);
                SBO_f._ReleaseCOMObject(oGeneralDataParams);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin PEImpDel


        public Boolean PENotesAdd(SAPbouiCOM.DBDataSource oDBDSHeader)
        {
            SAPbobsCOM.GeneralService oImpAdicAdd = null;
            SAPbobsCOM.GeneralData oImpAdicAddData = null;
            SAPbobsCOM.GeneralDataCollection oImpAdicAddLines = null;
            SAPbobsCOM.GeneralDataParams oImpAdicAddParameter = null;
            TSBOGeneralService oGen;
            //String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;
            //Int32 i; 

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;
                oImpAdicAdd = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("FM_NOTES")); //
                oImpAdicAddData = (SAPbobsCOM.GeneralData)(oImpAdicAdd.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oImpAdicAddData);

                Cmpny.StartTransaction();
                oImpAdicAddParameter = oImpAdicAdd.Add(oImpAdicAddData);
                //Result := System.int32(oRequerParameter.GetProperty('DocEntry')).ToString;
                Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                return true;
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("PENotesAdd : " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
                //FSBOApp.StatusBar.SetText('AddDatos : ' + e.Message + ' ** Trace: ' + e.StackTrace , BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oImpAdicAdd);
                SBO_f._ReleaseCOMObject(oImpAdicAddData);
                SBO_f._ReleaseCOMObject(oImpAdicAddLines);
                SBO_f._ReleaseCOMObject(oImpAdicAddParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin PENotesAdd


        public Boolean PENotesDel(ref SAPbobsCOM.Recordset oRecordSet)
        {
            SAPbobsCOM.GeneralService oGeneralService = null;
            SAPbobsCOM.GeneralData oGeneralData = null;
            SAPbobsCOM.GeneralDataCollection oGeneralDataCollection = null;
            SAPbobsCOM.GeneralDataParams oGeneralDataParams = null;
            TSBOGeneralService oGen;
            String StrDummy;
            Int32 i;

            oGen = new TSBOGeneralService();
            try
            {
                oGen.SBO_f = SBO_f;
                oGeneralService = (SAPbobsCOM.GeneralService)(SBO_f.Cmpny.GetCompanyService().GetGeneralService("FM_NOTES"));
                oGeneralDataParams = (SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));

                i = 1;
                while (!oRecordSet.EoF)
                {
                    StrDummy = (System.String)(oRecordSet.Fields.Item("Code").Value);
                    oGeneralDataParams.SetProperty("Code", StrDummy);
                    oGeneralService.Delete(oGeneralDataParams);
                    oRecordSet.MoveNext();
                }

                return true;
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("PENotesDel " + e.Message);
                return false;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGeneralService);
                SBO_f._ReleaseCOMObject(oGeneralData);
                SBO_f._ReleaseCOMObject(oGeneralDataCollection);
                SBO_f._ReleaseCOMObject(oGeneralDataParams);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin PENotesDel


        public Boolean PEUMISOAdd(SAPbouiCOM.DBDataSource oDBDSHeader)
        {
            SAPbobsCOM.GeneralService oImpAdicAdd = null;
            SAPbobsCOM.GeneralData oImpAdicAddData = null;
            SAPbobsCOM.GeneralDataCollection oImpAdicAddLines = null;
            SAPbobsCOM.GeneralDataParams oImpAdicAddParameter = null;
            TSBOGeneralService oGen;
            //String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;
            //Int32 i; 

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;
                oImpAdicAdd = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("FM_UMISO")); //
                oImpAdicAddData = (SAPbobsCOM.GeneralData)(oImpAdicAdd.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oImpAdicAddData);

                Cmpny.StartTransaction();
                oImpAdicAddParameter = oImpAdicAdd.Add(oImpAdicAddData);
                //Result := System.int32(oRequerParameter.GetProperty('DocEntry')).ToString;
                Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                return true;
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("PEUMISOAdd : " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
                //FSBOApp.StatusBar.SetText('AddDatos : ' + e.Message + ' ** Trace: ' + e.StackTrace , BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oImpAdicAdd);
                SBO_f._ReleaseCOMObject(oImpAdicAddData);
                SBO_f._ReleaseCOMObject(oImpAdicAddLines);
                SBO_f._ReleaseCOMObject(oImpAdicAddParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin PEUMISOAdd


        public Boolean PEUMISODel(ref SAPbobsCOM.Recordset oRecordSet)
        {
            SAPbobsCOM.GeneralService oGeneralService = null;
            SAPbobsCOM.GeneralData oGeneralData = null;
            SAPbobsCOM.GeneralDataCollection oGeneralDataCollection = null;
            SAPbobsCOM.GeneralDataParams oGeneralDataParams = null;
            TSBOGeneralService oGen;
            String StrDummy;
            Int32 i;

            oGen = new TSBOGeneralService();
            try
            {
                oGen.SBO_f = SBO_f;
                oGeneralService = (SAPbobsCOM.GeneralService)(SBO_f.Cmpny.GetCompanyService().GetGeneralService("FM_UMISO"));
                oGeneralDataParams = (SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));

                i = 1;
                while (!oRecordSet.EoF)
                {
                    StrDummy = (System.String)(oRecordSet.Fields.Item("Code").Value);
                    oGeneralDataParams.SetProperty("Code", StrDummy);
                    oGeneralService.Delete(oGeneralDataParams);
                    oRecordSet.MoveNext();
                }

                return true;
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("PEUMISODel " + e.Message);
                return false;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGeneralService);
                SBO_f._ReleaseCOMObject(oGeneralData);
                SBO_f._ReleaseCOMObject(oGeneralDataCollection);
                SBO_f._ReleaseCOMObject(oGeneralDataParams);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin PEUMISODel

        public String UpLoadDocumentByUrl(XmlDocument xmlDOC, Boolean RunningUnderSQLServer, String URL, String user, String pass) 
        {
            //string url = “http://portalPE.asydoc.cl/SendDocument.ashx";
            try
            {
                WebRequest request = WebRequest.Create(URL);
                //**request.Credentials = new NetworkCredential(user, pass);
                request.Method = "POST";
                string postData = xmlDOC.InnerXml;
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
                return responseFromServer;
            }
            catch (Exception ex) 
            {
                SBO_f.oLog.OutLog("Error UpLoadDocumentByUrl " + ex.Message);   
                return "Error " + ex.Message;
            }
        
        }

        public String UpLoadDocumentByUrlAPI(XmlDocument xmlDOC, String json, Boolean RunningUnderSQLServer, String URL, String user, String pass, String NombreArchivo)
        {
            string postData;
            String oPath = System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\xml";
            String archivo;
            //string url = “http://portalPE.asydoc.cl/SendDocument.ashx";
            try
            {
                if (json == null)
                    archivo = oPath + "\\" + NombreArchivo + ".xml";
                else
                    archivo = oPath + "\\" + NombreArchivo + ".json";

                if (File.Exists(archivo))
                    File.Delete(archivo);

                if (json == null)
                    xmlDOC.Save(archivo);
                else
                {
                    //fijamos dondevamos a crear el archivo 
                    StreamWriter escrito = File.CreateText(archivo); // en el 
                    String contenido = json;
                    escrito.Write(contenido.ToString());
                    escrito.Flush();
                    escrito.Close();
                }

                WebRequest request = WebRequest.Create(URL);
                //**request.Credentials = new NetworkCredential(user, pass);
                request.Method = "POST";
                if (json == null)
                    postData = xmlDOC.InnerXml;
                else
                    postData = json;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                if (json == null)
                    request.ContentType = "text/xml";
                else
                    request.ContentType = "application/json";
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

                return responseFromServer;

            }
            catch (Exception ex)
            {
                SBO_f.oLog.OutLog("Error UpLoadDocumentByUrlAPI " + ex.Message);
                return "Error " + ex.Message;
            }

        }

        public Int32 Attachments(byte[] pdf, SAPbobsCOM.Company Cmpny,string tipo, string folio)
        {
            int result = -1;
            int lRetCode;
            string fileName = string.Concat("document","-",tipo,"-",folio,".pdf");
            string path = System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\PDF";
            string str = string.Concat(path,"\\",fileName);
            File.WriteAllBytes(str, pdf);

            SAPbobsCOM.Attachments2 oAtt = (SAPbobsCOM.Attachments2)Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oAttachments2);
            oAtt.Lines.SetCurrentLine(oAtt.Lines.Count - 1);
            oAtt.Lines.Add();
            oAtt.Lines.SourcePath = path; //System.IO.Path.GetDirectoryName(fileName);  
            oAtt.Lines.FileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            oAtt.Lines.FileExtension = System.IO.Path.GetExtension(fileName).Substring(1);
            oAtt.Lines.Override = SAPbobsCOM.BoYesNoEnum.tYES;

            lRetCode = oAtt.Add();

            if (lRetCode != 0)
            {
                string description = Cmpny.GetLastErrorDescription();
                int code = Cmpny.GetLastErrorCode();
                SBO_f.SBOApp.StatusBar.SetText("No se ha logrado crear objeto adjunto "  +  Cmpny.GetLastErrorCode() +" " + Cmpny.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            else
            {
                result = int.Parse(SBO_f.Cmpny.GetNewObjectKey());
                if (File.Exists(str))
                    File.Delete(str);
            }
            return result;
        }


        public String UpLoadDocumentByUrl2(XmlDocument xmlDOC, String json, Boolean RunningUnderSQLServer, String URL, String user, String pass, String NombreArchivo)
        {
            string actionUrl = URL;
            string paramString = "";
            Stream paramFileStream = null;
            byte[] paramFileBytes = null;
            Stream respuesta = null;
            String oPath = System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\xml";
            String archivo;

            try
            {
                if (json == null)
                    archivo = oPath + "\\" + NombreArchivo + ".xml";
                else
                    archivo = oPath + "\\" + NombreArchivo + ".json";

                if (File.Exists(archivo))
                    File.Delete(archivo);

                if (json == null)
                    xmlDOC.Save(archivo);
                else
                {
                    //fijamos dondevamos a crear el archivo 
                    StreamWriter escrito = File.CreateText(archivo); // en el 
                    String contenido = json;
                    escrito.Write(contenido.ToString());
                    escrito.Flush();
                    escrito.Close();
                }
                MemoryStream stream1 = new MemoryStream(File.ReadAllBytes(archivo));

                HttpContent fileStreamContent = new StreamContent(stream1);
                HttpClient client = new HttpClient();
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(fileStreamContent, "file1", "file1");
                    var response = client.PostAsync(actionUrl, formData).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("");
                    }
                    respuesta = response.Content.ReadAsStreamAsync().Result;
                }
                string resp = "";
                using (var stream = new MemoryStream())
                {
                    byte[] buffer = new byte[2048];
                    int bytesRead;
                    while ((bytesRead = respuesta.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        stream.Write(buffer, 0, bytesRead);
                    }
                    byte[] result = stream.ToArray();
                    resp = System.Text.Encoding.Default.GetString(result);
                }

                return resp;

            }
            catch (Exception x)
            {
                SBO_f.oLog.OutLog("Error UpLoadDocumentByUrl2 " + x.Message);
                return "Error " + x.Message;
            }
        }

        public Int32 AddDataSourceInt1(String Header, SAPbouiCOM.DBDataSource oDBDSHeader, String Line1, SAPbouiCOM.DBDataSource oDBDSLine1, String Line2, SAPbouiCOM.DBDataSource oDBDSLine2, String Line3, SAPbouiCOM.DBDataSource oDBDSLine3)
        {
            SAPbobsCOM.GeneralService oGeneralServiceAdd = null;
            SAPbobsCOM.GeneralData oGeneralDataAdd = null;
            SAPbobsCOM.GeneralDataCollection oGeneralCollection = null;
            SAPbobsCOM.GeneralDataParams oGeneralDataParameter = null;
            TSBOGeneralService oGen = null;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            try
            {
                Cmpny = SBO_f.Cmpny;
                CmpnyService = Cmpny.GetCompanyService();
                oGen = new TSBOGeneralService();

                oGen.SBO_f = SBO_f;
                oGeneralServiceAdd = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService(Header));
                oGeneralDataAdd = (SAPbobsCOM.GeneralData)(oGeneralServiceAdd.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGeneralDataAdd);

                if (oDBDSLine1 != null)
                {
                    if (oDBDSLine1.Size > 0)
                    {
                        oGeneralCollection = oGeneralDataAdd.Child(Line1);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine1, oGeneralDataAdd, oGeneralCollection);
                    }
                }

                if (oDBDSLine2 != null)
                {
                    if (oDBDSLine2.Size > 0)
                    {
                        oGeneralCollection = oGeneralDataAdd.Child(Line2);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine2, oGeneralDataAdd, oGeneralCollection);
                    }
                }

                if (oDBDSLine3 != null)
                {
                    if (oDBDSLine3.Size > 0)
                    {
                        oGeneralCollection = oGeneralDataAdd.Child(Line3);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine3, oGeneralDataAdd, oGeneralCollection);
                    }
                }

                //Cmpny.StartTransaction();
                oGeneralDataParameter = oGeneralServiceAdd.Add(oGeneralDataAdd);
                //Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                return (System.Int32)(oGeneralDataParameter.GetProperty("DocEntry"));
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("AddDataSource1: Error-> " + e.Message + " ** Trace: " + e.StackTrace);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGeneralServiceAdd);
                SBO_f._ReleaseCOMObject(oGeneralDataAdd);
                SBO_f._ReleaseCOMObject(oGeneralCollection);
                SBO_f._ReleaseCOMObject(oGeneralDataParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }


        public Int32 UpdDataSourceInt1(String Header, SAPbouiCOM.DBDataSource oDBDSHeader, String Line1, SAPbouiCOM.DBDataSource oDBDSLine1, String Line2, SAPbouiCOM.DBDataSource oDBDSLine2, String Line3, SAPbouiCOM.DBDataSource oDBDSLine3)
        {
            SAPbobsCOM.GeneralService oGeneralService = null;
            SAPbobsCOM.GeneralData oGeneralData = null;
            SAPbobsCOM.GeneralDataCollection oGeneralDataCollection = null;
            SAPbobsCOM.GeneralDataParams oGeneralDataParams = null;
            TSBOGeneralService oGen = null;
            String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;
            Int32 DocEntry;

            oGen = new TSBOGeneralService();
            try
            {
                oGen.SBO_f = SBO_f;
                Cmpny = SBO_f.Cmpny;
                CmpnyService = Cmpny.GetCompanyService();
                oGeneralService = (SAPbobsCOM.GeneralService)(SBO_f.Cmpny.GetCompanyService().GetGeneralService(Header));
                oGeneralDataParams = (SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = "DocEntry";
                DocEntry = Convert.ToInt32(((System.String)oDBDSHeader.GetValue("DocEntry", 0)));
                oGeneralDataParams.SetProperty(StrDummy, DocEntry);
                oGeneralData = oGeneralService.GetByParams(oGeneralDataParams);

                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGeneralData);

                if (oDBDSLine1 != null)
                {
                    if (oDBDSLine1.Size > 0)
                    {
                        oGeneralDataCollection = oGeneralData.Child(Line1);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine1, oGeneralData, oGeneralDataCollection);
                    }
                }

                if (oDBDSLine2 != null)
                {
                    if (oDBDSLine2.Size > 0)
                    {
                        oGeneralDataCollection = oGeneralData.Child(Line2);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine2, oGeneralData, oGeneralDataCollection);
                    }
                }

                if (oDBDSLine3 != null)
                {
                    if (oDBDSLine3.Size > 0)
                    {
                        oGeneralDataCollection = oGeneralData.Child(Line3);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine3, oGeneralData, oGeneralDataCollection);
                    }
                }

                //SBO_f.Cmpny.StartTransaction();
                oGeneralService.Update(oGeneralData);

                //SBO_f.Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                return Convert.ToInt32(((System.String)oDBDSHeader.GetValue("DocEntry", 0)));
            }
            catch
            {
                //if (SBO_f.Cmpny.InTransaction)
                //    SBO_f.Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGeneralService);
                SBO_f._ReleaseCOMObject(oGeneralData);
                SBO_f._ReleaseCOMObject(oGeneralDataCollection);
                SBO_f._ReleaseCOMObject(oGeneralDataParams);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }

        public Boolean AddDataSource1(String Header, SAPbouiCOM.DBDataSource oDBDSHeader, String Line1, SAPbouiCOM.DBDataSource oDBDSLine1, String Line2, SAPbouiCOM.DBDataSource oDBDSLine2, String Line3, SAPbouiCOM.DBDataSource oDBDSLine3, String Line4, SAPbouiCOM.DBDataSource oDBDSLine4)
        {
            SAPbobsCOM.GeneralService oGeneralServiceAdd = null;
            SAPbobsCOM.GeneralData oGeneralDataAdd = null;
            SAPbobsCOM.GeneralDataCollection oGeneralCollection = null;
            SAPbobsCOM.GeneralDataParams oGeneralDataParameter = null;
            TSBOGeneralService oGen = null;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            try
            {
                Cmpny = SBO_f.Cmpny;
                CmpnyService = Cmpny.GetCompanyService();
                oGen = new TSBOGeneralService();

                oGen.SBO_f = SBO_f;
                oGeneralServiceAdd = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService(Header));
                oGeneralDataAdd = (SAPbobsCOM.GeneralData)(oGeneralServiceAdd.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGeneralDataAdd);

                if (oDBDSLine1 != null)
                {
                    if (oDBDSLine1.Size > 0)
                    {
                        oGeneralCollection = oGeneralDataAdd.Child(Line1);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine1, oGeneralDataAdd, oGeneralCollection);
                    }
                }

                if (oDBDSLine2 != null)
                {
                    if (oDBDSLine2.Size > 0)
                    {
                        oGeneralCollection = oGeneralDataAdd.Child(Line2);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine2, oGeneralDataAdd, oGeneralCollection);
                    }
                }

                if (oDBDSLine3 != null)
                {
                    if (oDBDSLine3.Size > 0)
                    {
                        oGeneralCollection = oGeneralDataAdd.Child(Line3);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine3, oGeneralDataAdd, oGeneralCollection);
                    }
                }

                if (oDBDSLine4 != null)
                {
                    if (oDBDSLine4.Size > 0)
                    {
                        oGeneralCollection = oGeneralDataAdd.Child(Line4);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine4, oGeneralDataAdd, oGeneralCollection);
                    }
                }

                //Cmpny.StartTransaction();
                oGeneralDataParameter = oGeneralServiceAdd.Add(oGeneralDataAdd);
                //Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                return true;
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("AddDataSource1: Error-> " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGeneralServiceAdd);
                SBO_f._ReleaseCOMObject(oGeneralDataAdd);
                SBO_f._ReleaseCOMObject(oGeneralCollection);
                SBO_f._ReleaseCOMObject(oGeneralDataParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }


        public Boolean UpdDataSource1(String Tipo, String Code, String Header, SAPbouiCOM.DBDataSource oDBDSHeader, String Line1, SAPbouiCOM.DBDataSource oDBDSLine1, String Line2, SAPbouiCOM.DBDataSource oDBDSLine2, String Line3, SAPbouiCOM.DBDataSource oDBDSLine3, String Line4, SAPbouiCOM.DBDataSource oDBDSLine4)
        {
            SAPbobsCOM.GeneralService oGeneralService = null;
            SAPbobsCOM.GeneralData oGeneralData = null;
            SAPbobsCOM.GeneralDataCollection oGeneralDataCollection = null;
            SAPbobsCOM.GeneralDataParams oGeneralDataParams = null;
            TSBOGeneralService oGen = null;
            String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            oGen = new TSBOGeneralService();
            try
            {
                oGen.SBO_f = SBO_f;
                Cmpny = SBO_f.Cmpny;
                CmpnyService = Cmpny.GetCompanyService();
                oGeneralService = (SAPbobsCOM.GeneralService)(SBO_f.Cmpny.GetCompanyService().GetGeneralService(Header));
                oGeneralDataParams = (SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                if (Tipo == "M")
                    StrDummy = "Code";
                else
                    StrDummy = "DocEntry";
                oGeneralDataParams.SetProperty(StrDummy, Code);
                oGeneralData = oGeneralService.GetByParams(oGeneralDataParams);

                oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oGeneralData);

                if (oDBDSLine1 != null)
                {
                    if (oDBDSLine1.Size > 0)
                    {
                        oGeneralDataCollection = oGeneralData.Child(Line1);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine1, oGeneralData, oGeneralDataCollection);
                    }
                }

                if (oDBDSLine2 != null)
                {
                    if (oDBDSLine2.Size > 0)
                    {
                        oGeneralDataCollection = oGeneralData.Child(Line2);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine2, oGeneralData, oGeneralDataCollection);
                    }
                }

                if (oDBDSLine3 != null)
                {
                    if (oDBDSLine3.Size > 0)
                    {
                        oGeneralDataCollection = oGeneralData.Child(Line3);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine3, oGeneralData, oGeneralDataCollection);
                    }
                }

                if (oDBDSLine4 != null)
                {
                    if (oDBDSLine4.Size > 0)
                    {
                        oGeneralDataCollection = oGeneralData.Child(Line4);
                        oGen.SetNewDataSourceLines_InUDO(oDBDSLine4, oGeneralData, oGeneralDataCollection);
                    }
                }

                //SBO_f.Cmpny.StartTransaction();
                oGeneralService.Update(oGeneralData);

                //Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                return true;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGeneralService);
                SBO_f._ReleaseCOMObject(oGeneralData);
                SBO_f._ReleaseCOMObject(oGeneralDataCollection);
                SBO_f._ReleaseCOMObject(oGeneralDataParams);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }


        public String PDFenString(String TipoDocElectAddon, String DocEntry, String ObjType, String Serie, String Folio, Boolean RunningUnderSQLServer, String Localidad)
        {
            
            TableLogOnInfo logOnInfo;
            //CrystalDecisions.CrystalReports.Engine.Table tabla;
            
            ConnectionInfo connection = new ConnectionInfo();
            String oPath;
            String sNombreArchivo = "";
            String Pass = "";
            String PDFString;
            System.IO.Stream oStream;
            Boolean Seguir = false;
            String Tipo = "";
            string str1;
            SAPbobsCOM.Recordset orsL = ((SAPbobsCOM.Recordset)SBO_f.Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            ReportDocument rpt = new ReportDocument(); 

            try
            {
                
                //ReportDocument rpt = new ReportDocument(); //******
                oPath = System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0));
                //sNombreArchivo = oPath + "\\Reports\\" + Localidad + "\\Reporte " + TipoDocElect + ".rpt";
                sNombreArchivo = oPath + "\\Reports\\" + Localidad + "\\" + TipoDocElectAddon + "_" + SBO_f.Cmpny.CompanyDB + ".rpt";
                if (!File.Exists(sNombreArchivo))// || (TipoDocElectAddon == "111") || (TipoDocElectAddon == "112"))
                {
                    Seguir = RescatarRPT(TipoDocElectAddon, sNombreArchivo);
                }
                else
                    Seguir = true;

                if (Seguir)
                {
                    rpt.Load(sNombreArchivo);
                    rpt.Refresh();

                    
                    //Lb.EncryptFile("rsaPublicKey.txt")
                    try
                    {
                        if (RunningUnderSQLServer)
                            s = "SELECT ISNULL(U_Usr,'') 'Usuario', ISNULL(U_Pw,'') 'Pass' FROM [@VID_MENUSU] ";
                        else
                            s = @"SELECT IFNULL(""U_Usr"",'') ""Usuario"", IFNULL(""U_Pw"",'') ""Pass"", IFNULL(""U_Srvr"", '') ""Servidor"" FROM ""@VID_MENUSU"" ";
                        orsL.DoQuery(s);
                        if (orsL.RecordCount == 0)
                        {
                            if (RunningUnderSQLServer)
                                s = @"Se debe ingresar password de SQL en Parametros";
                            else

                                s = @"Se debe ingresar password de SYSTEM en Parametros";
                            SBO_f.SBOApp.StatusBar.SetText(s, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return "";
                        }
                        
                        if (RunningUnderSQLServer)
                        {
                            connection.ServerName = SBO_f.Cmpny.Server.ToString().Trim();
                            connection.DatabaseName = SBO_f.Cmpny.CompanyDB.ToString().Trim();
                            connection.IntegratedSecurity = false;
                            Pass = ((System.String)orsL.Fields.Item("Pass").Value).Trim();
                            //Pass = Param.DesEncriptar(Pass);
                            connection.Password = Pass;
                            connection.UserID = ((System.String)orsL.Fields.Item("Usuario").Value).Trim();
                            connection.Type = ConnectionInfoType.SQL;
                        }
                        else
                        {//DRIVER={B1CRHPROXY32};SERVERNODE=hanab1:30015;DATABASE=ZZZ_SBO_HIJUELAS
                            //DRIVER={B1CRHPROXY32};SERVERNODE=hanab1:30015;DATABASE=ZZZ_SBO_HIJUELAS
                            String strConnection = "DRIVER= {B1CRHPROXY32};UID=" + ((System.String)orsL.Fields.Item("Usuario").Value).Trim(); //string de conexion contruido de las variables
                            strConnection += ";PWD=" + ((System.String)orsL.Fields.Item("Pass").Value).Trim() + ";SERVERNODE=" + ((System.String)orsL.Fields.Item("Servidor").Value).Trim();
                            strConnection += ";DATABASE=" + SBO_f.Cmpny.CompanyDB.ToString().Trim() + ";";
                            /*String strConnection = "DRIVER= {B1CRHPROXY32};UID=SYSTEM"; //string de conexion contruido de las variables
                            strConnection += ";PWD=SAPB1Admin;SERVERNODE=hanab1:30015";
                            strConnection += ";DATABASE=SBO_SYNTHEON;";*/
                            NameValuePairs2 logonProps2 = rpt.DataSourceConnections[0].LogonProperties;
                            logonProps2.Set("Provider", "B1CRHPROXY32");
                            logonProps2.Set("Server Type", "B1CRHPROXY32");
                            logonProps2.Set("Connection String", strConnection);
                            logonProps2.Set("Locale Identifier", "1033");
                            rpt.DataSourceConnections[0].SetLogonProperties(logonProps2);
                            rpt.DataSourceConnections[0].SetConnection(((System.String)orsL.Fields.Item("Servidor").Value).Trim(), SBO_f.Cmpny.CompanyDB.ToString().Trim(), ((System.String)orsL.Fields.Item("Usuario").Value).Trim(), ((System.String)orsL.Fields.Item("Pass").Value).Trim());
                            //rpt.DataSourceConnections[0].SetConnection("hanab1:30015", "SBO_SYNTHEON", "SYSTEM", "SAPB1Admin");
                        }

                    }
                    catch (Exception ex)
                    {
                        SBO_f.oLog.OutLog(ex.Message + ", TRACE " + ex.StackTrace + ", file " + sNombreArchivo);
                    }


                    foreach (CrystalDecisions.CrystalReports.Engine.Table tabla in rpt.Database.Tables)
                    {
                        logOnInfo = tabla.LogOnInfo;
                        logOnInfo.ConnectionInfo = connection;
                        tabla.ApplyLogOnInfo(logOnInfo);
                    }

                    if (rpt.Subreports.Count > 0)
                    {
                        foreach (CrystalDecisions.CrystalReports.Engine.Table tabla in rpt.Subreports[0].Database.Tables)
                        {
                            logOnInfo = tabla.LogOnInfo;
                            logOnInfo.ConnectionInfo = connection;
                            tabla.ApplyLogOnInfo(logOnInfo);
                        }
                    }

                    rpt.VerifyDatabase();

                    //rpt.SetParameterValue("ObjectId@", ObjType);
                    rpt.SetParameterValue("DocKey@", DocEntry);
                    //rpt.PrintToPrinter(1, false, 1, 1);
                    //rpt.ExportToDisk(ExportFormatType.PortableDocFormat, "C:\\Paso\\prueba" + Folio + ".pdf");

                    oStream = rpt.ExportToStream(ExportFormatType.PortableDocFormat);

                    rpt.Close();

                    byte[] b1 = new byte[oStream.Length];
                    oStream.Seek(0, System.IO.SeekOrigin.Begin);
                    oStream.Read(b1, 0, Convert.ToInt32(oStream.Length));

                    str1 = Convert.ToBase64String(b1);

                    /*oStream.Position = 0;
                    using (StreamReader reader = new StreamReader(oStream, Encoding.UTF8))
                    {
                        PDFString = reader.ReadToEnd();
                    }*/

                }
                else
                {
                    SBO_f.SBOApp.StatusBar.SetText("Problema al rescatar layout en cristal", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    str1 = "";
                }

                return str1;
            }
            catch (Exception e)
            {
                //oSBOApplication.MessageBox(e.Message + ", TRACE " + e.StackTrace);
                SBO_f.oLog.OutLog(e.Message + ", TRACE " + e.StackTrace + ", file " + sNombreArchivo);
                return "";
            }
            finally
            {
                rpt.Dispose();
                connection = null;
                rpt.Dispose();
                rpt = null;
                SBO_f._ReleaseCOMObject(orsL);
                SBO_f._ReleaseCOMObject(rpt);
            }
        }


        public Boolean RescatarRPT(String TipoDocElectAddon, String sNombreArchivo)
        {
            String Tipo = "";
            SAPbobsCOM.ReportLayoutsService oLayoutService = (SAPbobsCOM.ReportLayoutsService)SBO_f.Cmpny.GetCompanyService().GetBusinessService(SAPbobsCOM.ServiceTypes.ReportLayoutsService);
            SAPbobsCOM.ReportParams oReportParams = (SAPbobsCOM.ReportParams)oLayoutService.GetDataInterface(SAPbobsCOM.ReportLayoutsServiceDataInterfaces.rlsdiReportParams);
            SAPbobsCOM.BlobParams oBlobParams = (SAPbobsCOM.BlobParams)SBO_f.Cmpny.GetCompanyService().GetDataInterface(SAPbobsCOM.CompanyServiceDataInterfaces.csdiBlobParams);

            try
            {
                //if ((TipoDocElectAddon == "111") || (TipoDocElectAddon == "112"))
                //{
                //    if (File.Exists(sNombreArchivo))
                //        File.Delete(sNombreArchivo);
                //}

                if (TipoDocElectAddon == "33")
                    Tipo = "INV2";
                else if (TipoDocElectAddon == "33A")
                    Tipo = "DPI2";
                else if (TipoDocElectAddon == "34")
                    Tipo = "IEX2";
                else if (TipoDocElectAddon == "39")
                    Tipo = "INB2";
                else if (TipoDocElectAddon == "41")
                    Tipo = "IEB2";
                else if (TipoDocElectAddon == "46")
                    Tipo = "PCH2";
                else if (TipoDocElectAddon == "46A")
                    Tipo = "DPO2";
                else if (TipoDocElectAddon == "52")
                    Tipo = "DLN2";
                else if (TipoDocElectAddon == "52T")
                    Tipo = "WTR1";
                else if (TipoDocElectAddon == "52D")
                    Tipo = "RPD2";
                else if (TipoDocElectAddon == "56")
                    Tipo = "IDN2";
                else if (TipoDocElectAddon == "61")
                    Tipo = "RIN2";
                else if (TipoDocElectAddon == "61C")
                    Tipo = "RPC2";
                else if (TipoDocElectAddon == "110")
                    Tipo = "INE2";
                else if (TipoDocElectAddon == "111")
                    Tipo = "IDN2";
                else if (TipoDocElectAddon == "112")
                    Tipo = "RIN2";
                else if (TipoDocElectAddon == "01")
                    Tipo = "INV2";
                else if (TipoDocElectAddon == "01A")
                    Tipo = "DPI2";
                else if (TipoDocElectAddon == "03")
                    Tipo = "INB2";
                else if (TipoDocElectAddon == "07")
                    Tipo = "RIN2";
                else if (TipoDocElectAddon == "08")
                    Tipo = "IDN2";

                oReportParams.ReportCode = Tipo;//defined in db table "RTYP"
                //oReportParams.CardCode = "296";//business partner 
                var oReport = oLayoutService.GetDefaultReport(oReportParams);
                oBlobParams.Table = "RDOC";
                oBlobParams.Field = "Template";
                oBlobParams.FileName = sNombreArchivo;//@"C:\Paso\salesorder.rpt";
                SAPbobsCOM.BlobTableKeySegment oKeySegment = oBlobParams.BlobTableKeySegments.Add();
                oKeySegment.Name = "DocCode";
                oKeySegment.Value = oReport.LayoutCode;
                SBO_f.Cmpny.GetCompanyService().SaveBlobToFile(oBlobParams);

                return true;
            }
            catch (Exception z)
            {
                SBO_f.oLog.OutLog(z.Message + ", TRACE " + z.StackTrace + ", file " + sNombreArchivo);
                return false;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oLayoutService);
                SBO_f._ReleaseCOMObject(oReportParams);
                SBO_f._ReleaseCOMObject(oBlobParams);
            }

        }


        public String Base64Encode(String plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }


        public Boolean DelDataSource(String Tipo, String Header, String sCode, Int32 DocEntry)
        {
            SAPbobsCOM.GeneralService oGeneralService = null;
            SAPbobsCOM.GeneralData oGeneralData = null;
            SAPbobsCOM.GeneralDataCollection oGeneralDataCollection = null;
            SAPbobsCOM.GeneralDataParams oGeneralDataParams = null;
            TSBOGeneralService oGen = null;
            String StrDummy;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            oGen = new TSBOGeneralService();
            try
            {
                oGen.SBO_f = SBO_f;
                Cmpny = SBO_f.Cmpny;
                CmpnyService = Cmpny.GetCompanyService();
                oGeneralService = (SAPbobsCOM.GeneralService)(SBO_f.Cmpny.GetCompanyService().GetGeneralService(Header));
                oGeneralDataParams = (SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                if (Tipo == "M")
                {
                    StrDummy = "Code";
                    oGeneralDataParams.SetProperty(StrDummy, sCode);
                }
                else
                {
                    StrDummy = "DocEntry";
                    oGeneralDataParams.SetProperty(StrDummy, DocEntry);
                }

                //SBO_f.Cmpny.StartTransaction();
                oGeneralService.Delete(oGeneralDataParams);
                //SBO_f.Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                return true;
            }
            catch (Exception x)
            {
                SBO_f.oLog.OutLog(x.Message);
                //if (SBO_f.Cmpny.InTransaction)
                //    SBO_f.Cmpny.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                return false;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oGeneralService);
                SBO_f._ReleaseCOMObject(oGeneralData);
                SBO_f._ReleaseCOMObject(oGeneralDataCollection);
                SBO_f._ReleaseCOMObject(oGeneralDataParams);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }


        public Boolean EjecutarSQLOleDb(String prmSQL, String StringConexion)
        {
            System.Data.OleDb.OleDbConnection conn1;
            System.Data.OleDb.OleDbCommand cmd;

            conn1 = new System.Data.OleDb.OleDbConnection(StringConexion);
            try
            {

                conn1.Open();
                cmd = new System.Data.OleDb.OleDbCommand(prmSQL, conn1);
                cmd.ExecuteNonQuery();

                return true;
            }
            catch
            {
                //FSBOApp.MessageBox(e.Message + ' ** Trace: ' + e.StackTrace,1,'Ok','','');
                //OutLog('Actualizando variables HROne: ' + e.Message + ' ** Trace: ' + e.StackTrace);
                return false;
            }
            finally
            {
                if (conn1.State == ConnectionState.Open)
                    conn1.Close();
            }
        }

        public Int32 FERechazosAdd(String TipoDoc, Int32 Folio, Int32 DocEntryO, String SubTypeO, String ObjTypeO, Int32 DocEntryD, String SubTypeD, String ObjTypeD)//insertar datos en tabla rechazados
        {
            SAPbobsCOM.GeneralService oFE = null;
            SAPbobsCOM.GeneralData oFEData = null;
            SAPbobsCOM.GeneralDataCollection oFELines = null;
            SAPbobsCOM.GeneralDataParams oFEParameter = null;
            TSBOGeneralService oGen;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.CompanyService CmpnyService;

            Cmpny = SBO_f.Cmpny;
            CmpnyService = Cmpny.GetCompanyService();
            oGen = new TSBOGeneralService();

            try
            {
                oGen.SBO_f = SBO_f;

                //Get GeneralService (oCmpSrv is the CompanyService)
                oFE = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FERECHAZO"));

                //Create data for new row in main UDO
                oFEData = (SAPbobsCOM.GeneralData)(oFE.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oFEData.SetProperty("U_TipoDoc", TipoDoc.Trim());
                oFEData.SetProperty("U_Folio", Folio);
                oFEData.SetProperty("U_DocEntryO", DocEntryO);
                oFEData.SetProperty("U_SubTypeO", SubTypeO.Trim());
                oFEData.SetProperty("U_ObjTypeO", ObjTypeO.Trim());
                oFEData.SetProperty("U_DocEntryD", DocEntryD);
                oFEData.SetProperty("U_SubTypeD", SubTypeD.Trim());
                oFEData.SetProperty("U_ObjTypeD", ObjTypeD.Trim());
                //oFEData.SetProperty("U_", Sociedad.Trim());

                //Add the new row, including children, to database
                //oGeneralParams := oGeneralService.Add(oGeneralData);

                //Cmpny.StartTransaction();
                oFEParameter = oFE.Add(oFEData);
                return (System.Int32)(oFEParameter.GetProperty("DocEntry"));

                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                SBO_f.oLog.OutLog("Insertar datos en VID_FERECHAZO: " + e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                SBO_f._ReleaseCOMObject(oFE);
                SBO_f._ReleaseCOMObject(oFEData);
                SBO_f._ReleaseCOMObject(oFELines);
                SBO_f._ReleaseCOMObject(oFEParameter);
                SBO_f._ReleaseCOMObject(oGen);
            }
        }//fin insertar datos en tabla rechazados
    }//fin Class
}
