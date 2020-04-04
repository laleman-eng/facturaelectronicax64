--DROP PROCEDURE VID_SP_FE_OPCH_R;
CREATE PROCEDURE VID_SP_FE_OPCH_R
(
     IN DocEntry		Integer
    ,IN TipoDoc			VarChar(10)
    ,IN ObjType			VarChar(10)
)
LANGUAGE SqlScript
AS
BEGIN
	SELECT
		 ROW_NUMBER() OVER(ORDER BY T0."Folio_Sii")	"NroLinRef" 
			,T0."TpoDocRef"						"TpoDocRef"
			,T0."FolioRef"						"FolioRef"
			,T0."FchRef"						"FchRef"
			,IFNULL(T0."CodRef", '')			"CodRef"
			,IFNULL(T0."RazonRef", '')			"RazonRef"
	FROM VID_VW_FE_OPCH_R T0
	WHERE 1 = 1
		AND T0."DocEntry" = :DocEntry
		AND T0."ObjType" = :ObjType;
END
