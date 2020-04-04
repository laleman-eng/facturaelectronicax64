--DROP PROCEDURE VID_SP_FE_NotaCredito_E;
CREATE PROCEDURE VID_SP_FE_NotaCredito_E
(
     IN DocEntry	Integer
    ,IN TipoDoc		VarChar(10)
    ,IN ObjType		VarChar(10)
)
LANGUAGE SqlScript
AS
BEGIN
	SELECT
		T0."DocDate"														"FchEmis"
		,T0."DocDueDate"													"FchVenc"
		,:TipoDoc															"TipoDTE"
		,T0."FolioNum"														"Folio"
		,'0'																"IndServicio"
		,0.0																"MntBruto"
		,0.0																"MntCancel"
		,0.0																"SaldoInsol"
		,T0."FmaPago"														"FmaPago"
		--Emisor
		,T0."SlpName"														"CdgVendedor"
		,UPPER(T0."TaxIdNum")												"RUTEmisor"
		,T0."RazonSocial"													"RznSocial"
		,T0."GiroEmis"														"GiroEmis"
		,T0."Sucursal"														"Sucursal"
		,T0."TelefenoRecep"													"Telefono"
		--Receptor
		,T0."CityB"															"CiudadPostal"
		,T0."CityS"															"CiudadRecep"
		,T0."CountyB"														"CmnaPostal"
		,T0."CountyS"														"CmnaRecep"
		,T0."Contacto"														"Contacto"
		,T0."E_Mail"														"CorreoRecep"
		,T0."StreetB"														"DirPostal"
		,T0."StreetS"														"DirRecep"
		,T0."Giro"															"GiroRecep"
		,UPPER(T0."LicTradNum")												"RUTRecep"
		,T0."CardName"														"RznSocRecep"
		--Totales
		,0																	"CredEC"
		,T0."Total_Impuesto"												"IVA"
		,0.0																"IVANoRet"
		,0.0																"IVAProp"
		,0.0																"IVATerc"
		,0.0																"MntBase"
		,T0."Total_Exento"													"MntExe"
		,0.0																"MntMargenCom"
		,T0."Total_Afecto"													"MntNeto"
		,T0."DocTotal"														"MntTotal"
		,0.0																"MontoNF"
		,0.0																"MontoPeriodo"
		,0.0																"SaldoAnterior"
		,T0."VatPercent"													"TasaIVA"
		,0.0																"VlrPagar"
		,T0."COMP"															"COMP"
		,T0."DiscSum"														"MntDescuento"
		,IFNULL((SELECT SUM("MontoImptoAdic") 
		           FROM VID_VW_FE_NotaCredito_D
				  WHERE "DocEntry" = T0."DocEntry"
		            AND "ObjType" = T0."ObjType"),0.0)						"MntImpAdic"
		,T0."DocCur"														"TipoMoneda"--En Totales
		--Exportacion
		,T0."MntFlete"														"MntFlete"
		,T0."MntSeguro"														"MntSeguro"
		,T0."MntGlobal"														"MntGlobal"
		,T0."Patente"														"Patente"
		,T0."CodClauVenta"													"CodClauVenta"
		,T0."CodModVenta"													"CodModVenta"
		--,T0."TipoMoneda"													"TipoMoneda"
		,T0."FmaPagExp"														"FmaPagExp"
		,T0."CodViaTransp"													"CodViaTransp"
		,T0."CodPtoEmbarque"												"CodPtoEmbarque"
		,T0."CodPtoDesemb"													"CodPtoDesemb"
		,T0."CodUnidMedTara"												"CodUnidMedTara"
		,T0."CodUnidPesoBruto"												"CodUnidPesoBruto"
		,T0."CodUnidPesoNeto"												"CodUnidPesoNeto"
		,T0."TotBultos"														"TotBultos"
		,T0."CodPaisRecep"													"CodPaisRecep"
        ,T0."CodPaisDestin"													"CodPaisDestin"
		--Otra Moneda Exportacion
		,T0."TpoMoneda"														"TpoMoneda"
		,CAST(IFNULL(T0."DocRate", 0.0) AS FLOAT)							"TpoCambio"
		,IFNULL(T0."OtraMoneda",0.0)										"MntExeOtrMnda"
		,IFNULL(T0."OtraMoneda",0.0)										"MntTotOtrMnda"
		--Activo Fijo
		,T0."TpoTranCompra"													"TpoTranCompra"
		,T0."TpoTranVenta"													"TpoTranVenta"
		,T0."CdgSIISucur"													"CdgSIISucur"
		,T0."SucursalAF"													"SucursalAF"

		,T0."FchPago"														"FchPago"
		,T0."MntPago"														"MntPago"
		,T0."GlosaPagos"													"GlosaPagos"

		--Campos Extras
		,T1."CAB_EXTRA1"													"Extra1"
		,T1."CAB_EXTRA2"													"Extra2"
	FROM VID_VW_FE_NotaCredito_E	   T0
	JOIN VID_VW_FE_NotaCredito_E_EXTRA T1 ON T0."DocEntry" = T1."DocEntry"
										 AND T0."ObjType"  = T1."ObjType"
	WHERE 1 = 1
		AND T0."DocEntry" = :DocEntry
		AND T0."ObjType" = :ObjType;
END;
