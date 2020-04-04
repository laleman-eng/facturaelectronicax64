--DROP VIEW VID_VW_FE_NotaCredito_R;
CREATE VIEW VID_VW_FE_NotaCredito_R
AS
	-- Nota de crÃ©dito electrÃ³nica basada en Factura
	SELECT
		 T0."FolioNum"																								"Folio_Sii"
		,CASE
			WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' 
			     AND RIGHT(IFNULL(N1."BeginStr", ''), 3) = '110' AND O0."isIns" = 'Y' THEN '110'
			WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '33'
			WHEN O0."DocSubType" = 'IB' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '39'
			WHEN O0."DocSubType" = 'BE' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '41'
			WHEN O0."DocSubType" = 'IX' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '110'
			WHEN O0."DocSubType" = 'IE' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '34'
			WHEN O0."DocSubType" = 'DN' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '56'
			WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) <> 'E' THEN '30'
			WHEN O0."DocSubType" = 'IX' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) <> 'E' THEN '101'
			WHEN O0."DocSubType" = 'IE' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) <> 'E' THEN '32'
			WHEN O0."DocSubType" = 'DN' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) <> 'E' THEN '55'
			ELSE '-1'
		 END																										"TpoDocRef"
		,TO_VARCHAR(O0."FolioNum")																					"FolioRef"
		,TO_CHAR(O0."TaxDate",'yyyy-MM-dd')																			"FchRef"
		,R0."U_CodRef"																								"CodRef"
		,IFNULL(R0."U_RazRef", '')																					"RazonRef"
		,IFNULL(R0."U_IndGlobal",'0')																				"IndGlobal"
		,T0."ObjType"
		,T0."DocEntry"
	FROM "ORIN" T0
	JOIN "RIN1" T1 ON T1."DocEntry"		= T0."DocEntry"
	JOIN "OINV" O0 ON T1."BaseEntry"	= O0."DocEntry"
	JOIN "NNM1" N0 ON T0."Series"		= N0."Series"
	JOIN "NNM1" N1 ON N1."Series"		= O0."Series"
	LEFT JOIN "@VID_FEREF" R0 ON R0."U_DocEntry" = T0."DocEntry"
	                         AND R0."U_DocSBO" = T0."ObjType"
	WHERE 1 = 1
		AND T1."BaseType" = '13'
		AND CASE
				WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '33'
				WHEN O0."DocSubType" = 'IB' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '39'
				WHEN O0."DocSubType" = 'BE' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '41'
				WHEN O0."DocSubType" = 'IX' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '110'
				WHEN O0."DocSubType" = 'IE' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '34'
				WHEN O0."DocSubType" = 'DN' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '56'
				WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) <> 'E' THEN '30'
				WHEN O0."DocSubType" = 'IX' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) <> 'E' THEN '101'
				WHEN O0."DocSubType" = 'IE' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) <> 'E' THEN '32'
				WHEN O0."DocSubType" = 'DN' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) <> 'E' THEN '55'
				ELSE '-1'
			END <> '-1'
		AND UPPER(LEFT(N0."BeginStr", 1)) = 'E'
	GROUP BY
		 T0."FolioNum"
		,O0."FolioNum"
		,O0."TaxDate"
		,R0."U_CodRef"
		,R0."U_RazRef"
		,O0."DocSubType"
		,N0."BeginStr"
		,T0."ObjType"
		,T0."DocEntry"
		,O0."isIns"
		,N1."BeginStr"
		,IFNULL(R0."U_IndGlobal",'0')
	
	UNION ALL
	
	-- Nota de crÃ©dito electrÃ³nica basada en Factura de anticipo electrÃ³nica
	SELECT
		 T0."FolioNum"																								"Folio_Sii"
		,CASE
			WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '33'
			ELSE '-1'
		 END																										"TpoDocRef"
		,TO_VARCHAR(O0."FolioNum")																					"FolioRef"
		,TO_CHAR(O0."TaxDate",'yyyy-MM-dd')																			"FchRef"
		,R0."U_CodRef"																								"CodRef"
		,IFNULL(R0."U_RazRef", '')																					"RazonRef"
		,IFNULL(R0."U_IndGlobal",'0')																				"IndGlobal"
		,T0."ObjType"
		,T0."DocEntry"
	FROM "ORIN" T0
	JOIN "RIN1" T1 ON T1."DocEntry"		= T0."DocEntry"
	JOIN "ODPI" O0 ON T1."BaseEntry"	= O0."DocEntry"
	JOIN "NNM1" N0 ON T0."Series"		= N0."Series"
	LEFT JOIN "@VID_FEREF" R0 ON R0."U_DocEntry" = T0."DocEntry"
	                         AND R0."U_DocSBO" = T0."ObjType"
	WHERE 1 = 1
		AND T1."BaseType" = '203'
		AND CASE
				WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '33'
				ELSE '-1'
			END <> '-1'
		AND UPPER(LEFT(N0."BeginStr", 1)) = 'E'
	GROUP BY
		 T0."FolioNum"
		,O0."FolioNum"
		,O0."TaxDate"
		,R0."U_CodRef"
		,R0."U_RazRef"
		,O0."DocSubType"
		,N0."BeginStr"
		,T0."ObjType"
		,T0."DocEntry"
		,IFNULL(R0."U_IndGlobal",'0')
	
	UNION ALL
	
	-- Nota de crÃ©dito electrÃ³nica basada en documentos fuera de SAP
	SELECT
		 T0."FolioNum"																								"Folio_Sii"
		,REPLACE(REPLACE(T2."U_TipoDTE", 'b', ''),'a','')															"TpoDocRef"
		,TO_VARCHAR(T2."U_DocFolio")																				"FolioRef"
		,TO_CHAR(T2."U_DocDate",'yyyy-MM-dd')																		"FchRef"
		,T1."U_CodRef"																								"CodRef"
		,IFNULL(T1."U_RazRef", '')																					"RazonRef"
		,IFNULL(T1."U_IndGlobal",'0')																				"IndGlobal"
		,T0."ObjType"
		,T0."DocEntry"
	FROM "ORIN" T0
	JOIN "@VID_FEREF" T1 ON T1."U_DocEntry" = T0."DocEntry"
	                    AND T1."U_DocSBO" = T0."ObjType"
	JOIN "@VID_FEREFD" T2 ON T2."DocEntry" = T1."DocEntry"
	JOIN "NNM1" N0 ON T0."Series" = N0."Series"
	WHERE 1 = 1
		--AND IFNULL(T0."FolioNum", -1) <> 0
		AND (RIGHT(T2."U_TipoDTE",1) = 'b' OR RIGHT(T2."U_TipoDTE",1) = 'a')
		AND IFNULL(T2."U_DocFolio", 0) <> 0
	
	UNION ALL
	
	-- Nota de crÃ©dito electrÃ³nica con IndGlobal
	SELECT
		 T0."FolioNum"																		"Folio_Sii"
		,T1."U_TipoDTE"																		"TpoDocRef"
		,'0'																										[FolioRef]
		,TO_CHAR(T0."DocDate", 'yyyy-MM-dd')												"FchRef"
		,T1."U_CodRef"																		"CodRef"
		,IFNULL(T1."U_RazRef", '')															"RazonRef"
		,IFNULL(T1."U_IndGlobal",'0')														"IndGlobal"
		,T0."ObjType"
		,T0."DocEntry"
	FROM "ORIN" T0
	JOIN "@VID_FEREF" T1 ON T1."U_DocEntry" = T0."DocEntry"
	                    AND T1."U_DocSBO" = T0."ObjType"
	JOIN "NNM1" N0 ON T0."Series" = N0."Series"
	, "@VID_FEPARAM" PA0
	WHERE 1 = 1
		AND ((IFNULL(T0."FolioNum", 0) <> 0 AND IFNULL(PA0."U_Distrib",'N') = 'Y') OR (IFNULL(T0."FolioNum", 0) = 0 AND IFNULL(PA0."U_FPortal",'N') = 'Y') OR (IFNULL(T0."FolioNum", 0) <> 0 AND IFNULL(PA0."U_FPortal",'N') = 'N' AND IFNULL(PA0."U_Distrib",'N') = 'N'))
		AND IFNULL(T1."U_IndGlobal", '0') = '1'
		AND (SELECT COUNT(*)
		       FROM "@VID_FEREFD"
			  WHERE "DocEntry" = T1."DocEntry") = 0

	UNION
	
	-- Para documentos de proveedores
	-- Nota de crÃ©dito electrÃ³nica basada en Factura de compra electrÃ³nica o manual
	SELECT
		 T0."FolioNum"																								"Folio_Sii"
		,CASE
			WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '46'
			WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) <> 'E' THEN '45'
			ELSE '-1'
		 END																										"TpoDocRef"
		,TO_VARCHAR(O0."FolioNum")																					"FolioRef"
		,TO_CHAR(O0."TaxDate",'yyyy-MM-dd')																			"FchRef"
		,R0."U_CodRef"																								"CodRef"
		,IFNULL(R0."U_RazRef", '')																					"RazonRef"
		,IFNULL(R0."U_IndGlobal",'0')																				"IndGlobal"
		,T0."ObjType"
		,T0."DocEntry"
	FROM "ORPC" T0
	JOIN "RPC1" T1 ON T1."DocEntry"		= T0."DocEntry"
	JOIN "OPCH" O0 ON T1."BaseEntry"	= O0."DocEntry"
	JOIN "NNM1" N0 ON T0."Series"		= N0."Series"
	LEFT JOIN "@VID_FEREF" R0 ON R0."U_DocEntry" = T0."DocEntry"
	                         AND R0."U_DocSBO" = T0."ObjType"
	WHERE 1 = 1
		AND T1."BaseType" = '18'
		AND CASE
				WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '46'
				WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) <> 'E' THEN '45'
				ELSE '-1'
			END <> '-1'
		AND UPPER(LEFT(N0."BeginStr", 1)) = 'E'
	GROUP BY
		 T0."FolioNum"
		,O0."FolioNum"
		,O0."TaxDate"
		,R0."U_CodRef"
		,R0."U_RazRef"
		,O0."DocSubType"
		,N0."BeginStr"
		,T0."ObjType"
		,T0."DocEntry"
		,IFNULL(R0."U_IndGlobal",'0')
	
	UNION ALL
	
	-- Nota de crÃ©dito electrÃ³nica basada en Factura de compra por anticipo electrÃ³nica o manual
	SELECT
		 T0."FolioNum"																								"Folio_Sii"
		,CASE
			WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '46'
			WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) <> 'E' THEN '45'
			ELSE '-1'
		 END																										"TpoDocRef"
		,TO_VARCHAR(O0."FolioNum")																					"FolioRef"
		,TO_CHAR(O0."TaxDate",'yyyy-MM-dd')																			"FchRef"
		,R0."U_CodRef"																								"CodRef"
		,IFNULL(R0."U_RazRef", '')																					"RazonRef"
		,IFNULL(R0."U_IndGlobal",'0')																				"IndGlobal"
		,T0."ObjType"
		,T0."DocEntry"
	FROM "ORPC" T0
	JOIN "RPC1" T1 ON T1."DocEntry"		= T0."DocEntry"
	JOIN "ODPO" O0 ON T1."BaseEntry"	= O0."DocEntry"
	JOIN "NNM1" N0 ON T0."Series"		= N0."Series"
	LEFT JOIN "@VID_FEREF" R0 ON R0."U_DocEntry" = T0."DocEntry"
	                         AND R0."U_DocSBO" = T0."ObjType"
	WHERE 1 = 1
		AND T1."BaseType" = '204'
		AND CASE
				WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) = 'E' THEN '46'
				WHEN O0."DocSubType" = '--' AND SUBSTRING(UPPER(IFNULL(N0."BeginStr", '')), 1, 1) <> 'E' THEN '45'
				ELSE '-1'
			END <> '-1'
		AND UPPER(LEFT(N0."BeginStr", 1)) = 'E'
	GROUP BY
		 T0."FolioNum"
		,O0."FolioNum"
		,O0."TaxDate"
		,R0."U_CodRef"
		,R0."U_RazRef"
		,O0."DocSubType"
		,N0."BeginStr"
		,T0."ObjType"
		,T0."DocEntry"
		,IFNULL(R0."U_IndGlobal",'0')
	
	UNION ALL
	
	-- Nota de crÃ©dito electrÃ³nica basada en documentos fuera de SAP
	SELECT
		 T0."FolioNum"																								"Folio_Sii"
		,REPLACE(REPLACE(T2."U_TipoDTE", 'b', ''),'a','')															"TpoDocRef"
		,TO_VARCHAR(T2."U_DocFolio")																				"FolioRef"
		,TO_CHAR(T2."U_DocDate",'yyyy-MM-dd')																		"FchRef"
		,T1."U_CodRef"																								"CodRef"
		,IFNULL(T1."U_RazRef", '')																					"RazonRef"
		,IFNULL(T1."U_IndGlobal",'0')																				"IndGlobal"
		,T0."ObjType"
		,T0."DocEntry"
	FROM "ORPC" T0
	JOIN "@VID_FEREF" T1 ON T1."U_DocEntry" = T0."DocEntry"
	                    AND T1."U_DocSBO" = T0."ObjType"
	JOIN "@VID_FEREFD" T2 ON T2."DocEntry" = T1."DocEntry"
	JOIN "NNM1" N0 ON T0."Series" = N0."Series"
	WHERE 1 = 1
		--AND IFNULL(T0."FolioNum", 0) <> 0
		AND (RIGHT(T2."U_TipoDTE",1) = 'b' OR RIGHT(T2."U_TipoDTE",1) = 'a')
		AND IFNULL(T2."U_DocFolio", 0) <> 0
	
	UNION

	-- Nota de crÃ©dito electrÃ³nica con IndGlobal
	SELECT
		 T0."FolioNum"																		"Folio_Sii"
		,T1."U_TipoDTE"																		"TpoDocRef"
		,'0'																				"FolioRef"
		,TO_CHAR(T0."DocDate", 'yyyy-MM-dd')												"FchRef"
		,T1."U_CodRef"																		"CodRef"
		,IFNULL(T1."U_RazRef", '')															"RazonRef"
		,IFNULL(T1."U_IndGlobal",'0')														"IndGlobal"
		,T0."ObjType"
		,T0."DocEntry"
	FROM "ORPC" T0
	JOIN "@VID_FEREF" T1 ON T1."U_DocEntry" = T0."DocEntry"
	                    AND T1."U_DocSBO" = T0."ObjType"
	JOIN "NNM1" N0 ON T0."Series" = N0."Series"
	, "@VID_FEPARAM" PA0
	WHERE 1 = 1
		AND ((IFNULL(T0."FolioNum", 0) <> 0 AND IFNULL(PA0."U_Distrib",'N') = 'Y') OR (IFNULL(T0."FolioNum", 0) = 0 AND IFNULL(PA0."U_FPortal",'N') = 'Y') OR (IFNULL(T0."FolioNum", 0) <> 0 AND IFNULL(PA0."U_FPortal",'N') = 'N' AND IFNULL(PA0."U_Distrib",'N') = 'N'))
		AND IFNULL(T1."U_IndGlobal", '0') = '1'
		AND (SELECT COUNT(*)
		       FROM "@VID_FEREFD"
			  WHERE "DocEntry" = T1."DocEntry") = 0;
		
	