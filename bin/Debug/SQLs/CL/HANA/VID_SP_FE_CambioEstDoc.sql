DROP PROCEDURE VID_SP_FE_CambioEstDoc;

CREATE PROCEDURE VID_SP_FE_CambioEstDoc
(
    IN Folio integer,
	IN TipoDoc  Varchar(10), 
	IN DYT_ID_TRASPASO Numeric
)
LANGUAGE SQLSCRIPT
AS
BEGIN
	
SELECT 'UPDATE dbo.Faet_Erp_Encabezado_Doc set DYT_ESTADO_TRASPASO=1 from dbo.Faet_Erp_Encabezado_Doc with (nolock) WHERE CAB_COD_TP_FACTURA = ' || :TipoDoc || ' AND CAB_FOL_DOCTO_INT = ' || CAST(:Folio AS VARCHAR(20)) || ' AND DYT_ID_TRASPASO = ' || CAST(:DYT_ID_TRASPASO AS VARCHAR(20)) "Texto"
  FROM DUMMY;
END;