IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_Bultos' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_Bultos
GO--

CREATE PROCEDURE VID_SP_FE_Bultos
(
	 @DocEntry			Int
	,@TipoDoc			VarChar(10)
	,@DYT_ID_TRASPASO	Numeric
	,@ObjType			VarChar(10)
)
AS
BEGIN
	SELECT
		 @DYT_ID_TRASPASO				[DYT_ID_TRASPASO]
		,T0.TipoBultos					[CodTpoBultos]
		,T0.TotBultos					[CantBultos]
		,''								[Marcas]
		,''								[IdContainer]
		,''								[Sello]
		,''								[EmisorSello]
	FROM VID_VW_FE_OINV_E T0
	WHERE 1 = 1
		AND T0.DocEntry = @DocEntry
		AND T0.ObjType = @ObjType;
END;
