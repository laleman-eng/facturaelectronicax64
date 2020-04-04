--DROP PROCEDURE VID_SP_FE_LIQUI_C;
CREATE PROCEDURE VID_SP_FE_LIQUI_C
(
     IN DocEntry		Integer
    ,IN TipoDoc			VarChar(10)
    ,IN ObjType			VarChar(10)
)
LANGUAGE SqlScript
AS
BEGIN	

	docs1 = SELECT
			 CASE WHEN C1."ExpnsName" = 'LIF Comisiones' THEN 'C'
			      ELSE 'O' END																	"TipoMovim"
			,LEFT(IFNULL(T3."Comments", C1."ExpnsName"),60)										"Glosa"
			,CASE WHEN T3."TaxCode" = 'IVA' THEN T3."LineTotal" * -1
			      ELSE 0.0 END																	"ValComNeto"
			,CASE WHEN T3."TaxCode" = 'IVA_EXE' THEN T3."LineTotal" * -1
			      ELSE 0.0 END																	"ValComExe"
			,T3."VatSum"	*-1																	"ValComIVA"
			,T3."ExpnsCode"
		FROM "ORIN" T0
		JOIN "RIN3" T3 ON T3."DocEntry" = T0."DocEntry"
		JOIN "OEXD" C1 ON C1."ExpnsCode" = T3."ExpnsCode"
		WHERE 1 = 1
			AND T0."DocEntry" = :DocEntry
			AND T0."ObjType"  = :ObjType;
			
	docs2 = SELECT
			 CASE WHEN C1."ExpnsName" = 'LIF Comisiones' THEN 'C'
			      ELSE 'O' END																	"TipoMovim"
			,LEFT(IFNULL(T3."Comments", C1."ExpnsName"),60)										"Glosa"
			,CASE WHEN T3."TaxCode" = 'IVA' THEN T3."LineTotal"
			      ELSE 0.0 END																	"ValComNeto"
			,CASE WHEN T3."TaxCode" = 'IVA_EXE' THEN T3."LineTotal"
			      ELSE 0.0 END																	"ValComExe"
			,T3."VatSum"																		"ValComIVA"
			,T3."ExpnsCode"
		FROM "OINV" T0
		JOIN "INV3" T3 ON T3."DocEntry" = T0."DocEntry"
		JOIN "OEXD" C1 ON C1."ExpnsCode" = T3."ExpnsCode"
		WHERE 1 = 1
			AND T0."DocEntry" = :DocEntry
			AND T0."ObjType"  = :ObjType;

	v_out = CE_UNION_ALL(:docs1, :docs2);
	
	--Select final para mostrar
	SELECT
		 ROW_NUMBER() OVER(ORDER BY "ExpnsCode")"NroLinCom"
		,"TipoMovim"							"TipoMovim"
		,"Glosa"								"Glosa"
		,"ValComNeto"							"ValComNeto"
		,"ValComExe"							"ValComExe"
		,"ValComIVA"							"ValComIVA"
	FROM :v_out;
	
END;