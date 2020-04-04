DROP PROCEDURE VID_SP_FE_LIBRO_COMPRAS;

CREATE PROCEDURE VID_SP_FE_LIBRO_COMPRAS
(
	 IN FechaD	VarChar(8)
	,IN FechaH	VarChar(8)
)
LANGUAGE SqlScript
AS
BEGIN
	-- Facturas y Notas de Débito
	docs1 = SELECT
				 CASE
				 	WHEN IND."Code" = 'AFE' AND T0."DocSubType" = '--' THEN '33'
				 	WHEN IND."Code" = 'AFM' AND T0."DocSubType" = '--' THEN '30'
				 	WHEN IND."Code" = 'AFE' AND T0."DocSubType" = 'DM' THEN '56'
				 	WHEN IND."Code" = 'AFM' AND T0."DocSubType" = 'DM' THEN '55'
					ELSE IFNULL(IND."Code", 'INDICADOR NO VALIDO')
				 END																							"SII"
				,TO_CHAR(T0."TaxDate",'DD/MM/YYYY')																"FECHA"
				,IFNULL(T0."FolioNum", 0)																		"NUMFACT"
				,T0."CardName"																					"RAZSOC2"
				,REPLACE(IFNULL(T0."LicTradNum", ''), '.', '')													"RUTFACT"
				,SUM(CASE			
						WHEN IMP."Code" IN ('IVA', 'IVAND') THEN T4."BaseSum"			
						ELSE 0			
					 END)																						"TOTNETO"
				,SUM(CASE			
						WHEN IMP."Code" = 'IVA' THEN T4."TaxSum"			
						ELSE 0			
					 END)																						"TOTIVA"
				,SUM(CASE			
						WHEN IMP."Code" IN ('IVA', 'IVAND') THEN T4."BaseSum"			
						ELSE 0			
					 END)																						"AFECTO"
				,SUM(CASE			
						WHEN IMP."Code" = 'IVAEXE' THEN T4."BaseSum"			
						ELSE 0			
					 END)																						"EXENTO"
				,SUM(CASE			
						WHEN T1."TaxOnly" = 'Y' THEN T1."LineTotal"			
						ELSE 0			
					 END) + T0."DocTotal"																		"TOTAL"
				,SUM(CASE			
						WHEN RET."Code" = 'RETT' THEN T5."WTAmnt"			
						ELSE 0			
					 END)																						"IVARetTotal"
				,IFNULL(T0."U_CodIVANoRec", 0)																	"IVANoRec_CodIVANoRec"
				,SUM(CASE			
						WHEN IMP."Code" = 'IVAND' THEN T4."TaxSum"			
						ELSE 0			
					 END)																						"IVANoRec_MntIVANoRec"
				,SUM(CASE			
						WHEN IND."Code" IN ('AFE', 'AFM') AND IMP."Code" = 'IVA' THEN T4."BaseSum"			
						ELSE 0			
					 END)																						"MntActivoFijo"
				,SUM(CASE	
						WHEN IND."Code" IN ('AFE', 'AFM') AND IMP."Code" IN ('IVA', 'IVAND') THEN T4."TaxSum"
						ELSE 0	
					 END)																						"MntIVAActivoFijo"
				,SUM(CASE
						WHEN RET."Code" IN ('RETP', 'RETH') THEN T5."WTAmnt"
						ELSE 0
					 END)																						"IVARetParcial"
				,SUM(CASE
						WHEN IMP."Code" = 'DIESEL' THEN T4."TaxSum"
						ELSE 0
					 END)																						"ivaDiesel"
				,SUM(CASE
						WHEN IND."Code" IN ('AFE', 'AFM') AND IMP."Code" = 'IVAEXE' THEN T4."BaseSum"
						ELSE 0
					 END)																						"ExentoActivoFijo"
				,0																								"IVAUsoComun"
			FROM	  OPCH			  T0
				 JOIN PCH1			  T1	ON T1."DocEntry"	= T0."DocEntry"
			LEFT JOIN PCH4			  T4	ON T4."DocEntry"	= T0."DocEntry"
										   AND T4."LineNum"		= T1."LineNum"
			LEFT JOIN PCH5			  T5	ON T5."AbsEntry"	= T0."DocEntry"
			LEFT JOIN "@VID_FEDOCED"  IND	ON IND."U_Indicato"	= T0."Indicator"
			LEFT JOIN "@VID_FEIMPTOD" IMP	ON IMP."U_CodeImp"	= T4."StaCode"
			LEFT JOIN "@VID_FEIMPTOD" RET	ON RET."U_CodeImp"	= T5."WTCode"
				 JOIN NNM1			  N0	ON N0."Series"		= T0."Series"
			,OADM A0
			WHERE 1 = 1
				AND T0."DocSubType" IN ('--', 'DM')
				AND IFNULL(IND."Code", '') NOT IN ('NL', 'NTL')
				AND T0."DocDate" BETWEEN :FechaD AND :FechaH
			GROUP BY
				 IND."Code"
				,T0."DocSubType"
				,T0."DocDate"
				,T0."TaxDate"
				,T0."FolioNum"
				,T0."CardName"
				,T0."LicTradNum"
				,T0."U_CodIVANoRec"
				,T0."DocTotal";

	-- Notas de Crédito
	docs2 = SELECT
				 CASE
				 	WHEN IND."Code" = 'AFE' THEN '61'
				 	WHEN IND."Code" = 'AFM' THEN '60'
					ELSE IFNULL(IND."Code", 'INDICADOR NO VALIDO')
				 END																							"SII"
				,TO_CHAR(T0."TaxDate",'DD/MM/YYYY')																"FECHA"
				,IFNULL(T0."FolioNum", 0)																		"NUMFACT"
				,T0."CardName"																					"RAZSOC2"
				,REPLACE(IFNULL(T0."LicTradNum", ''), '.', '')													"RUTFACT"
				,SUM(CASE			
						WHEN IMP."Code" IN ('IVA', 'IVAND') THEN T4."BaseSum"			
						ELSE 0			
					 END)																						"TOTNETO"
				,SUM(CASE			
						WHEN IMP."Code" = 'IVA' THEN T4."TaxSum"			
						ELSE 0			
					 END)																						"TOTIVA"
				,SUM(CASE			
						WHEN IMP."Code" IN ('IVA', 'IVAND') THEN T4."BaseSum"			
						ELSE 0			
					 END)																						"AFECTO"
				,SUM(CASE			
						WHEN IMP."Code" = 'IVAEXE' THEN T4."BaseSum"			
						ELSE 0			
					 END)																						"EXENTO"
				,SUM(CASE			
						WHEN T1."TaxOnly" = 'Y' THEN T1."LineTotal"			
						ELSE 0			
					 END) + T0."DocTotal"																		"TOTAL"
				,SUM(CASE			
						WHEN RET."Code" = 'RETT' THEN T5."WTAmnt"			
						ELSE 0			
					 END)																						"IVARetTotal"
				,IFNULL(T0."U_CodIVANoRec", 0)																	"IVANoRec_CodIVANoRec"
				,SUM(CASE			
						WHEN IMP."Code" = 'IVAND' THEN T4."TaxSum"			
						ELSE 0			
					 END)																						"IVANoRec_MntIVANoRec"
				,SUM(CASE			
						WHEN IND."Code" IN ('AFE', 'AFM') AND IMP."Code" = 'IVA' THEN T4."BaseSum"			
						ELSE 0			
					 END)																						"MntActivoFijo"
				,SUM(CASE	
						WHEN IND."Code" IN ('AFE', 'AFM') AND IMP."Code" IN ('IVA', 'IVAND') THEN T4."TaxSum"
						ELSE 0	
					 END)																						"MntIVAActivoFijo"
				,SUM(CASE
						WHEN RET."Code" IN ('RETP', 'RETH') THEN T5."WTAmnt"
						ELSE 0
					 END)																						"IVARetParcial"
				,SUM(CASE
						WHEN IMP."Code" = 'DIESEL' THEN T4."TaxSum"
						ELSE 0
					 END)																						"ivaDiesel"
				,SUM(CASE
						WHEN IND."Code" IN ('AFE', 'AFM') AND IMP."Code" = 'IVAEXE' THEN T4."BaseSum"
						ELSE 0
					 END)																						"ExentoActivoFijo"
				,0																								"IVAUsoComun"
			FROM	  ORPC			  T0
				 JOIN RPC1			  T1	ON T1."DocEntry"	= T0."DocEntry"
			LEFT JOIN RPC4			  T4	ON T4."DocEntry"	= T0."DocEntry"
										   AND T4."LineNum"		= T1."LineNum"
			LEFT JOIN RPC5			  T5	ON T5."AbsEntry"	= T0."DocEntry"
			LEFT JOIN "@VID_FEDOCED"  IND	ON IND."U_Indicato"	= T0."Indicator"
			LEFT JOIN "@VID_FEIMPTOD" IMP	ON IMP."U_CodeImp"	= T4."StaCode"
			LEFT JOIN "@VID_FEIMPTOD" RET	ON RET."U_CodeImp"	= T5."WTCode"
				 JOIN NNM1			  N0	ON N0."Series"		= T0."Series"
			,OADM A0
			WHERE 1 = 1
				AND T0."DocSubType" IN ('--', 'DM')
				AND IFNULL(IND."Code", '') NOT IN ('NL', 'NTL')
				AND T0."DocDate" BETWEEN :FechaD AND :FechaH
			GROUP BY
				 IND."Code"
				,T0."DocSubType"
				,T0."DocDate"
				,T0."TaxDate"
				,T0."FolioNum"
				,T0."CardName"
				,T0."LicTradNum"
				,T0."U_CodIVANoRec"
				,T0."DocTotal";
				
	v_out = CE_UNION_ALL(:docs1, :docs2);
	
	SELECT
		 "SII"
		,"FECHA"
		,"NUMFACT"
		,"RAZSOC2"
		,"RUTFACT"
		,"TOTNETO"
		,"TOTIVA"
		,"AFECTO"
		,"EXENTO"
		,"TOTAL"
		,"IVARetTotal"
		,"IVANoRec_CodIVANoRec"
		,"IVANoRec_MntIVANoRec"
		,"MntActivoFijo"
		,"MntIVAActivoFijo"
		,"IVARetParcial"
		,"ivaDiesel"
		,"ExentoActivoFijo"
		,"IVAUsoComun"
	FROM :v_out
	ORDER BY
		 "SII"
		,"FECHA"
		,"NUMFACT";
END;
