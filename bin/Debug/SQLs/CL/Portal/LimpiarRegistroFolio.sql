IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_LimpiarRegistroFolio' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_LimpiarRegistroFolio
GO--

CREATE PROCEDURE VID_SP_FE_LimpiarRegistroFolio
  @TipoDoc  Varchar(10), @Folio_Sii numeric
--WITH ENCRYPTION
AS
BEGIN
	DECLARE @idBoletaENC INT

	IF EXISTS (SELECT Cab_Fol_Docto_Int 
	             FROM FaeT_ErrorInteraAuxDyt WITH (NOLOCK)
			    WHERE Cab_Fol_Docto_Int = @Folio_Sii 
				  AND Cab_Cod_Tp_Factura = @TipoDoc)
	BEGIN
		
		if (@TipoDoc in ('39','41')) --para boletas
		BEGIN
			SET @idBoletaENC = (SELECT IdBoletaENC FROM faet_BOL_ENC WHERE Folio = @Folio_Sii AND TipoDTE = @TipoDoc)
			DELETE faet_BOL_DET WITH (ROWLOCK) WHERE IdBoletaENC = @idBoletaENC
			DELETE FAET_BOL_REF WITH (ROWLOCK) WHERE IdBoletaENC = @idBoletaENC
			DELETE FAET_BOL_DRGLOBAL WITH (ROWLOCK) WHERE IdBoletaENC = @idBoletaENC
			DELETE FAET_BOL_SUBTOT WITH (ROWLOCK) WHERE IdBoletaENC = @idBoletaENC
			DELETE faet_BOL_ENC WITH (ROWLOCK) WHERE IdBoletaENC = @idBoletaENC
		END
		ELSE
		BEGIN
			DELETE Faet_Erp_Encabezado_Doc WITH (ROWLOCK) WHERE CAB_FOL_DOCTO_INT = @Folio_Sii AND CAB_COD_TP_FACTURA = @TipoDoc
			DELETE Faet_Erp_Detalle_Doc WITH (ROWLOCK) WHERE CAB_FOL_DOCTO_INT = @Folio_Sii AND CAB_COD_TP_FACTURA = @TipoDoc
			DELETE Faet_Erp_Referencia_Doc WITH (ROWLOCK) WHERE CAB_FOL_DOCTO_INT = @Folio_Sii AND CAB_COD_TP_FACTURA = @TipoDoc
		END
		DELETE Faet_InterAux_Dte WITH (ROWLOCK) WHERE CAB_FOL_DOCTO_INT = @Folio_Sii AND CAB_COD_TP_FACTURA = @TipoDoc
		DELETE ENCABEZADO_DOC WITH (ROWLOCK) WHERE FOLIO_SII = @Folio_Sii AND TIPODTE = @TipoDoc
		DELETE DETALLE_DOC WITH (ROWLOCK) WHERE FOLIO_SII = @Folio_Sii AND TIPODTE = @TipoDoc
		DELETE FaeT_ReferenciaDoc WITH (ROWLOCK) WHERE FOLIO_SII = @Folio_Sii AND TIPODTE = @TipoDoc
		DELETE FaeT_ErrorInteraAuxDyt WITH (ROWLOCK) WHERE Cab_Fol_Docto_Int = @Folio_Sii AND CAB_COD_TP_FACTURA = @TipoDoc
		DELETE DTE_XML WITH (ROWLOCK) WHERE Folio = @Folio_Sii AND TipoDTE = @TipoDoc
	END
	ELSE
	BEGIN
	    IF EXISTS (SELECT CAB_FOL_DOCTO_INT
		             FROM Faet_Erp_Encabezado_Doc WITH (NOLOCK)
                    WHERE CAB_FOL_DOCTO_INT = @Folio_Sii
					  AND CAB_COD_TP_FACTURA = @TipoDoc)
		BEGIN
			IF ((SELECT DYT_ESTADO_TRASPASO 
				   FROM Faet_Erp_Encabezado_Doc WITH (NOLOCK)
				  WHERE CAB_FOL_DOCTO_INT = @Folio_Sii
					AND CAB_COD_TP_FACTURA = @TipoDoc) = 0)
			BEGIN
				DELETE [Faet_Erp_Encabezado_Doc] WITH (ROWLOCK) WHERE CAB_FOL_DOCTO_INT = @Folio_Sii AND CAB_COD_TP_FACTURA = @TipoDoc
				DELETE [Faet_Erp_Detalle_Doc] WITH (ROWLOCK) WHERE CAB_FOL_DOCTO_INT = @Folio_Sii AND CAB_COD_TP_FACTURA = @TipoDoc
				DELETE [Faet_Erp_Referencia_Doc] WITH (ROWLOCK) WHERE CAB_FOL_DOCTO_INT = @Folio_Sii AND CAB_COD_TP_FACTURA = @TipoDoc
				DELETE DTE_XML WITH (ROWLOCK) WHERE Folio = @Folio_Sii AND TipoDTE = @TipoDoc
			END
		END
		--Para Boletas
		IF EXISTS (SELECT Folio
		             FROM FAET_BOL_ENC WITH (NOLOCK)
                    WHERE Folio = @Folio_Sii
					  AND TipoDTE = @TipoDoc)
		BEGIN
			IF ((SELECT Estado
				   FROM Faet_BOL_ENC WITH (NOLOCK)
				  WHERE Folio = @Folio_Sii
					AND TipoDTE = @TipoDoc) = 0)
			BEGIN
				SET @idBoletaENC = (SELECT IdBoletaENC FROM faet_BOL_ENC WHERE Folio = @Folio_Sii AND TipoDTE = @TipoDoc)
				DELETE faet_BOL_DET WITH (ROWLOCK) WHERE IdBoletaENC = @idBoletaENC
				DELETE FAET_BOL_REF WITH (ROWLOCK) WHERE IdBoletaENC = @idBoletaENC
				DELETE FAET_BOL_DRGLOBAL WITH (ROWLOCK) WHERE IdBoletaENC = @idBoletaENC
				DELETE FAET_BOL_SUBTOT WITH (ROWLOCK) WHERE IdBoletaENC = @idBoletaENC
				DELETE faet_BOL_ENC WITH (ROWLOCK) WHERE IdBoletaENC = @idBoletaENC
			END
		END
	END

END