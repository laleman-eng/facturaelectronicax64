IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_CambioEstDoc' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_CambioEstDoc
GO--

CREATE PROCEDURE VID_SP_FE_CambioEstDoc
	 @Folio				Int
	,@TipoDoc			VarChar(10)
	,@DYT_ID_TRASPASO	Numeric
--WITH ENCRYPTION
AS
BEGIN
	SELECT
		'UPDATE dbo.Faet_Erp_Encabezado_Doc
		 SET DYT_ESTADO_TRASPASO = 1
		 FROM dbo.Faet_Erp_Encabezado_Doc WITH (nolock)
		 WHERE CAB_COD_TP_FACTURA = ' + @TipoDoc + '
		 AND CAB_FOL_DOCTO_INT = ' + CAST(@Folio AS VARCHAR(20)) + '
		 AND DYT_ID_TRASPASO = ' + CAST(@DYT_ID_TRASPASO AS VARCHAR(20))	[Texto]
END
