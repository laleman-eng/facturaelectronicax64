--DROP PROCEDURE VID_SP_FE_LIQUI_R;
CREATE PROCEDURE VID_SP_FE_LIQUI_R
(
     IN DocEntry		Integer
    ,IN TipoDoc			VarChar(10)
    ,IN ObjType			VarChar(10)
)
LANGUAGE SqlScript
AS
BEGIN		
	
	docs1 = SELECT
			 T0."FolioNum"																		"Folio_Sii"
			,:TipoDoc																			"TipoDte"
			,T1."U_TipoDTELF"																	"TpoDocRef"
			,CAST(T1."U_FolioLiqF" AS VARCHAR(20))												"FolioRef"
			,TO_VARCHAR(T0."TaxDate", 'yyyy-MM-dd')												"FchRef"
			,''																					"CodRef"
			,'Referencia Liquidacion'															"RazonRef"
		FROM "ORIN" T0
		JOIN "RIN1" T1 ON T1."DocEntry" = T0."DocEntry"
		WHERE 1 = 1
		    AND IFNULL(T1."U_TipoDTELF", '00') <> '00' 
			AND IFNULL(T1."U_TipoDTELF", '00') <> '99' 
			AND T0."DocEntry" = :DocEntry
			AND T0."ObjType"  = :ObjType;
	
	docs2 = SELECT
			 T0."FolioNum"																		"Folio_Sii"
			,:TipoDoc																			"TipoDte"
			,T1."U_TipoDTELF"																	"TpoDocRef"
			,CAST(T1."U_FolioLiqF" AS VARCHAR(20))												"FolioRef"
			,TO_VARCHAR(T0."TaxDate", 'yyyy-MM-dd')												"FchRef"
			,''																					"CodRef"
			,'Referencia Liquidacion'															"RazonRef"
		FROM "OINV" T0
		JOIN "INV1" T1 ON T1."DocEntry" = T0."DocEntry"
		WHERE 1 = 1
		    AND IFNULL(T1."U_TipoDTELF", '00') <> '00' 
			AND IFNULL(T1."U_TipoDTELF", '00') <> '99' 
			AND T0."DocEntry" = :DocEntry
			AND T0."ObjType"  = :ObjType;
			
	v_out = CE_UNION_ALL(:docs1, :docs2);

	--Select final para mostrar
	SELECT
		 ROW_NUMBER() OVER(ORDER BY "Folio_Sii")"NroLinRef" 
		,"TpoDocRef"							"TpoDocRef"
		,"FolioRef"								"FolioRef"
		,"FchRef"								"FchRef"
		,"CodRef"								"CodRef"	
		,"RazonRef"								"RazonRef"
	FROM :v_out;
	
END;
