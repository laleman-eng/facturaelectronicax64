--DROP VIEW VID_VW_FE_OPCH_R;
CREATE VIEW VID_VW_FE_OPCH_R
AS
	-- Factura de compra electrónica basada en Orden de compra pasando por Guía
	SELECT
		 T0."FolioNum"									"Folio_Sii"
		,'801'											"TpoDocRef"
		,TO_VARCHAR(ROUND(O0."DocNum",0))				"FolioRef"
		,TO_VARCHAR(O0."TaxDate",'yyyy-MM-dd')			"FchRef"
		,'0'											"CodRef"
		,''												"RazonRef"
		,T0."DocEntry"									"DocEntry"
		,T0."ObjType"									"ObjType"
	FROM OPCH T0
	JOIN PCH1 T1 ON T1."DocEntry"	= T0."DocEntry"
	JOIN PDN1 G1 ON G1."DocEntry"	= T1."BaseEntry"
				AND G1."ObjType"	= T1."BaseType"
				AND G1."LineNum"	= T1."BaseLine"
	JOIN OPDN G0 ON G0."DocEntry"	= G1."DocEntry"
	JOIN POR1 O1 ON O1."DocEntry"	= G1."BaseEntry"
				AND O1."ObjType"	= G1."BaseType"
				AND O1."LineNum"	= G1."BaseLine"
	JOIN OPOR O0 ON O0."DocEntry"	= O1."DocEntry"
	JOIN NNM1 N0 ON G0."Series"		= N0."Series"
	WHERE 1 = 1
		--AND IFNULL(T0."FolioNum", 0) <> 0
		AND IFNULL(G1."BaseType", 0) = 22
	GROUP BY
		 T0."FolioNum"
		,TO_VARCHAR(ROUND(O0."DocNum",0))
		,O0."TaxDate"
		,T0."DocEntry"
		,T0."ObjType"
		
	UNION ALL
	
	-- Factura de compra electrónica basada en Orden de compra sin Guía
	SELECT
		 T0."FolioNum"									"Folio_Sii"
		,'801'											"TpoDocRef"
		,TO_VARCHAR(ROUND(O0."DocNum",0))				"FolioRef"
		,TO_VARCHAR(O0."TaxDate",'yyyy-MM-dd')			"FchRef"
		,'0'											"CodRef"
		,''												"RazonRef"
		,T0."DocEntry"									"DocEntry"
		,T0."ObjType"									"ObjType"
	FROM OPCH T0
	JOIN PCH1 T1 ON T1."DocEntry"	= T0."DocEntry"
	JOIN OPOR O0 ON O0."DocEntry"	= T1."BaseEntry"
	WHERE 1 = 1
		--AND IFNULL(T0."FolioNum", 0) <> 0
		AND IFNULL(T1."BaseType", 0) = 22
	GROUP BY
		 T0."FolioNum"
		,TO_VARCHAR(ROUND(O0."DocNum",0))
		,O0."TaxDate"
		,T0."DocEntry"
		,T0."ObjType"
	
	UNION ALL
	
	--------------------------------------------------------------------------------------------
	
	-- Anticipo de proveedores
	-- Factura de compra electrónica basada en Orden de compra pasando por Guía
	SELECT
		 T0."FolioNum"									"Folio_Sii"
		,'801'											"TpoDocRef"
		,TO_VARCHAR(ROUND(O0."DocNum",0))				"FolioRef"
		,TO_VARCHAR(O0."TaxDate",'yyyy-MM-dd')			"FchRef"
		,'0'											"CodRef"
		,''												"RazonRef"
		,T0."DocEntry"									"DocEntry"
		,T0."ObjType"									"ObjType"
	FROM ODPO T0
	JOIN DPO1 T1 ON T1."DocEntry"	= T0."DocEntry"
	JOIN PDN1 G1 ON G1."DocEntry"	= T1."BaseEntry"
				AND G1."ObjType"	= T1."BaseType"
				AND G1."LineNum"	= T1."BaseLine"
	JOIN OPDN G0 ON G0."DocEntry"	= G1."DocEntry"
	JOIN POR1 O1 ON O1."DocEntry"	= G1."BaseEntry"
				AND O1."ObjType"	= G1."BaseType"
				AND O1."LineNum"	= G1."BaseLine"
	JOIN OPOR O0 ON O0."DocEntry"	= O1."DocEntry"
	JOIN NNM1 N0 ON G0."Series"		= N0."Series"
	WHERE 1 = 1
		--AND IFNULL(T0."FolioNum", 0) <> 0
		AND IFNULL(G1."BaseType", 0) = 22
	GROUP BY
		 T0."FolioNum"
		,TO_VARCHAR(ROUND(O0."DocNum",0))
		,O0."TaxDate"
		,T0."DocEntry"
		,T0."ObjType"
		
	UNION ALL
	
	-- Factura de compra electrónica basada en Orden de compra sin Guía
	SELECT
		 T0."FolioNum"									"Folio_Sii"
		,'801'											"TpoDocRef"
		,TO_VARCHAR(ROUND(O0."DocNum",0))				"FolioRef"
		,TO_VARCHAR(O0."TaxDate",'yyyy-MM-dd')			"FchRef"
		,'0'											"CodRef"
		,''												"RazonRef"
		,T0."DocEntry"									"DocEntry"
		,T0."ObjType"									"ObjType"
	FROM ODPO T0
	JOIN DPO1 T1 ON T1."DocEntry"	= T0."DocEntry"
	JOIN OPOR O0 ON O0."DocEntry"	= T1."BaseEntry"
	WHERE 1 = 1
		--AND IFNULL(T0."FolioNum", 0) <> 0
		AND IFNULL(T1."BaseType", 0) = 22
	GROUP BY
		 T0."FolioNum"
		,TO_VARCHAR(ROUND(O0."DocNum",0))
		,O0."TaxDate"
		,T0."DocEntry"
		,T0."ObjType"