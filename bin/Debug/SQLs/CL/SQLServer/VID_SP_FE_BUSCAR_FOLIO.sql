IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_BUSCAR_FOLIO' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_BUSCAR_FOLIO
GO--

CREATE PROCEDURE VID_SP_FE_BUSCAR_FOLIO
(
	@TipoDoc VarChar(10)
)
AS
BEGIN
	BEGIN TRANSACTION;
		
		DECLARE
			 @DocEntry	Int
			,@LineId	Int
			,@Folio		Int
			,@CAF		VarChar(5000)
			,@TaxIdNum	VarChar(30)
		
		SELECT TOP (1)
			 @DocEntry	= T0.DocEntry
			,@LineId	= T1.LineId
			,@Folio		= T1.U_Folio
			,@CAF		= T2.U_CAF
			,@TaxIdNum	= REPLACE(A0.TaxIdNum, '.', '')
		FROM [@VID_FEDIST]	T0 WITH (NOLOCK)
		JOIN [@VID_FEDISTD]	T1 WITH (ROWLOCK) ON T1.DocEntry	= T0.DocEntry
		JOIN [@VID_FECAF]	T2 WITH (NOLOCK)  ON T2.Code		= T0.U_RangoF
		,OADM A0
		WHERE 1 = 1
			AND T0.U_TipoDoc = @TipoDoc
			AND T0.U_Sucursal = 'Principal'
			AND T1.U_Estado = 'D'
			AND T1.U_Folio > 0
		ORDER BY
			 T1.U_Folio ASC
		
		UPDATE [@VID_FEDISTD]
		SET U_Estado = 'R'
		WHERE 1 = 1
			AND DocEntry = @DocEntry
			AND LineId = @LineId
	
	COMMIT TRANSACTION;
	
	SELECT
		 @DocEntry	[DocEntry]
		,@LineId	[LineId]
		,@Folio		[Folio]
		,@CAF		[CAF]
		,@TaxIdNum	[TaxIdNum]
END;
