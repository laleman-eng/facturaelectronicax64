--DROP PROCEDURE VID_SP_FE_LIQUI_E;
CREATE PROCEDURE VID_SP_FE_LIQUI_E
(
     IN DocEntry		Integer
    ,IN TipoDoc			VarChar(10)
    ,IN ObjType			VarChar(10)
)
LANGUAGE SqlScript
AS
BEGIN	

	IF :ObjType = '14' THEN
		SELECT --IdDoc
			T0."DocDate"														"FchEmis"
			,T0."DocDueDate"													"FchVenc"
			,:TipoDoc															"TipoDTE"
			,T0."FolioNum"														"Folio"
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
			,T0."Total_Impuesto"												"IVA"
			,T0."Total_Exento"													"MntExe"
			,T0."Total_Afecto"													"MntNeto"
			,T0."DocTotal"														"MntTotal"
			,T0."VatPercent"													"TasaIVA"
			,T0."COMP"															"COMP"
			,T0."DiscSum"														"MntDescuento"
			--,IFNULL(T0."OtraMoneda",0.0)											"OtraMoneda"
			--,CAST(IFNULL(T0."DocRate",0.0) AS FLOAT)								"TpoCambio"
			--Campos Extras
			,T1."CAB_EXTRA1"													"Extra1"
			,T1."CAB_EXTRA2"													"Extra2"
			,T0."ValComNeto"													"ValComNeto"
			,T0."ValComExe"														"ValComExe"
			,T0."ValComIVA"														"ValComIVA"

			,T0."FchPago"														"FchPago"
			,T0."MntPago"														"MntPago"
			,T0."GlosaPagos"													"GlosaPagos"
		FROM VID_VW_FE_NotaCredito_E	   T0
		JOIN VID_VW_FE_NotaCredito_E_EXTRA T1 ON T0."DocEntry" = T1."DocEntry"
											 AND T0."ObjType"  = T1."ObjType"
		WHERE 1 = 1
			AND T0."DocEntry" = :DocEntry
			AND T0."ObjType" = :ObjType;
	ELSE
		SELECT --IdDoc
			T0."DocDate"														"FchEmis"
			,T0."DocDueDate"													"FchVenc"
			,:TipoDoc															"TipoDTE"
			,T0."FolioNum"														"Folio"
			,T0."FmaPago"														"FmaPago"
			--Emisor
			,T0."SlpName"														"CdgVendedor"
			,T0."TaxIdNum"														"RUTEmisor"
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
			,T0."LicTradNum"													"RUTRecep"
			,T0."CardName"														"RznSocRecep"
			--Totales
			,T0."Total_Impuesto" * -1											"IVA"
			,T0."Total_Exento" * -1												"MntExe"
			,T0."Total_Afecto" * -1												"MntNeto"
			,T0."DocTotal" * -1													"MntTotal"
			,T0."VatPercent"													"TasaIVA"
			,T0."COMP"															"COMP"
			,T0."DiscSum" * -1													"MntDescuento"
			--,IFNULL(T0."OtraMoneda",0.0)											"OtraMoneda"
			--,CAST(IFNULL(T0."DocRate",0.0) AS FLOAT)								"TpoCambio"
			--Campos Extras
			,T1."CAB_EXTRA1"													"Extra1"
			,T1."CAB_EXTRA2"													"Extra2"
			,T0."ValComNeto"													"ValComNeto"
			,T0."ValComExe"														"ValComExe"
			,T0."ValComIVA"														"ValComIVA"

			,T0."FchPago"														"FchPago"
			,T0."MntPago"														"MntPago"
			,T0."GlosaPagos"													"GlosaPagos"
		FROM VID_VW_FE_OINV_E	   T0
		JOIN VID_VW_FE_OINV_E_EXTRA T1 ON T0."DocEntry" = T1."DocEntry"
											 AND T0."ObjType"  = T1."ObjType"
		WHERE 1 = 1
			AND T0."DocEntry" = :DocEntry
			AND T0."ObjType" = :ObjType;
	END IF;
END;
