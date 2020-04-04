DROP PROCEDURE VID_SP_FE_Bultos;

CREATE PROCEDURE VID_SP_FE_Bultos
(
	 IN DocEntry		Integer
	,IN TipoDoc			VarChar(10)
	,IN DYT_ID_TRASPASO	Float
	,IN ObjType			VarChar(10)
)
LANGUAGE SqlScript
AS
BEGIN
	SELECT
		 :DYT_ID_TRASPASO				"DYT_ID_TRASPASO"
		,T0."TipoBultos"				"CodTpoBultos"
		,T0."TotBultos"			"CantBultos"
		,''								"Marcas"
		,''								"IdContainer"
		,''								"Sello"
		,''								"EmisorSello"
	FROM VID_VW_FE_OINV_E T0
	WHERE 1 = 1
		AND T0."DocEntry" = :DocEntry
		AND T0."ObjType" = :ObjType;
END;
