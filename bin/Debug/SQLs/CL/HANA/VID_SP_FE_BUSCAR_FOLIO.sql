DROP PROCEDURE VID_SP_FE_BUSCAR_FOLIO;

CREATE PROCEDURE VID_SP_FE_BUSCAR_FOLIO
(
	IN TipoDoc VarChar(10)
)
LANGUAGE SqlScript
AS
	DocEntry	Integer;
	LineId		Integer;
	Folio		Integer;
	CAF			VarChar(5000);
	TaxIdNum	VarChar(30);
BEGIN
	SELECT TOP 1
		 W0."DocEntry"
		,W0."LineId"
		,W0."U_Folio"
		,W0."U_CAF"
		,W0."TaxIdNum"
	INTO
		 DocEntry
		,LineId
		,Folio
		,CAF
		,TaxIdNum
	FROM (
 		 SELECT T0."DocEntry"
			   ,T1."LineId"
			   ,T1."U_Folio"
			   ,TO_VARCHAR(T2."U_CAF") "U_CAF"
			   ,REPLACE(IFNULL(A0."TaxIdNum",''), '.', '') "TaxIdNum"
		   FROM "@VID_FEDIST"	T0
		   JOIN "@VID_FEDISTD"	T1 ON T1."DocEntry"	= T0."DocEntry"
		   JOIN "@VID_FECAF"	T2 ON T2."Code"		= T0."U_RangoF"
		  ,OADM A0
		  WHERE 1 = 1
			AND T0."U_TipoDoc" = :TipoDoc
			AND T0."U_Sucursal" = 'Principal'
			AND T1."U_Estado" = 'D'
			AND T1."U_Folio" > 0
		  UNION 
		 SELECT 0
		       ,0
		       ,0
		       ,CAST('' AS VARCHAR(5000))
		       ,CAST('ZZZ' AS VARCHAR(30)) 
		   FROM DUMMY) W0
	ORDER BY
		  W0."TaxIdNum" ASC,W0."U_Folio" ASC;

	
	IF :TaxIdNum <> 'ZZZ' THEN
		UPDATE "@VID_FEDISTD"
		SET "U_Estado" = 'R'
		WHERE 1 = 1
			AND "DocEntry" = :DocEntry
			AND "LineId" = :LineId;
	END IF;
	
	SELECT
		 :DocEntry	"DocEntry"
		,:LineId	"LineId"
		,:Folio		"Folio"
		,:CAF		"CAF"
		,:TaxIdNum	"TaxIdNum"
	FROM DUMMY;
END
