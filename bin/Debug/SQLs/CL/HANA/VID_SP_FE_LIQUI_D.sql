--DROP PROCEDURE VID_SP_FE_LIQUI_D;
CREATE PROCEDURE VID_SP_FE_LIQUI_D
(
     IN DocEntry		Integer
    ,IN TipoDoc			VarChar(10)
    ,IN ObjType			VarChar(10)
)
LANGUAGE SqlScript
AS
BEGIN	

	IF :ObjType = '14' THEN
		SELECT
			 ROW_NUMBER() OVER(ORDER BY T0."LineaOrden", T0."LineaOrden2")							"NroLinDet"
			,T0."DiscSum"																			"DescuentoMonto"
			,T0."DiscPrcnt"																			"DescuentoPct"
			,T0."Indicador_Exento"																	"IndExe"
			,T0."LineTotal"																			"MontoItem"
			,T0."Dscription"																		"NmbItem"
			,T0."Price"																				"PrcItem"
			,T0."Quantity"																			"QtyItem"
			,IFNULL(T0."RecargoMonto",0.0)															"RecargoMonto"
			,0.0																					"RecargoPct"
			--,T0."DET_UNIDAD_MEDIDA"																"UnmdItem"
			,T0."TpoDocLiq"																			"VlrCodigo"
			,T0."Dscription_Larga"																	"DscItem"
			
			,T0."TpoDocLiq"																			"TpoDocLiq"
			,IFNULL(T1."DET_EXTRA1", '')															"Extra1"
			,IFNULL(T1."DET_EXTRA2", '')															"Extra2"
			,IFNULL(T1."DET_EXTRA3", '')															"Extra3"
			,IFNULL(T1."DET_EXTRA4", '')															"Extra4"
			,IFNULL(T1."DET_EXTRA5", '')															"Extra5"
		FROM	  VID_VW_FE_NotaCredito_D		T0
		LEFT JOIN VID_VW_FE_NotaCredito_D_EXTRA T1 ON T0."DocEntry"    = T1."DocEntry"
												  AND T0."ObjType"     = T1."ObjType"
												  AND T0."LineaOrden"  = T1."LineaOrden"
												  AND T0."LineaOrden2" = T1."LineaOrden2"
		WHERE 1 = 1
			AND T0."DocEntry" = :DocEntry
			AND T0."ObjType" = :ObjType;
	ELSE IF :ObjType = '13' THEN
		SELECT
			 ROW_NUMBER() OVER(ORDER BY T0."LineaOrden", T0."LineaOrden2")							"NroLinDet"
			,T0."DiscSum"																			"DescuentoMonto"
			,T0."DiscPrcnt"																			"DescuentoPct"
			,T0."Indicador_Exento"																	"IndExe"
			,T0."LineTotal" * -1																	"MontoItem"
			,T0."Dscription"																		"NmbItem"
			,0.0																					"PrcItem"
			,T0."Quantity"																			"QtyItem"
			,IFNULL(T0."RecargoMonto",0.0)															"RecargoMonto"
			,0.0																					"RecargoPct"
			--,T0."DET_UNIDAD_MEDIDA"																"UnmdItem"
			,T0."TpoDocLiq"																			"VlrCodigo"
			,T0."Dscription_Larga"																	"DscItem"
			
			,T0."TpoDocLiq"																			"TpoDocLiq"
			,IFNULL(T1."DET_EXTRA1", '')															"Extra1"
			,IFNULL(T1."DET_EXTRA2", '')															"Extra2"
			,IFNULL(T1."DET_EXTRA3", '')															"Extra3"
			,IFNULL(T1."DET_EXTRA4", '')															"Extra4"
			,IFNULL(T1."DET_EXTRA5", '')															"Extra5"
		FROM	  VID_VW_FE_OINV_D		T0
		LEFT JOIN VID_VW_FE_OINV_D_EXTRA T1 ON T0."DocEntry"    = T1."DocEntry"
												  AND T0."ObjType"     = T1."ObjType"
												  AND T0."LineaOrden"  = T1."LineaOrden"
												  AND T0."LineaOrden2" = T1."LineaOrden2"
		WHERE 1 = 1
			AND T0."DocEntry" = :DocEntry
			AND T0."ObjType" = :ObjType;
	END IF;
END;
