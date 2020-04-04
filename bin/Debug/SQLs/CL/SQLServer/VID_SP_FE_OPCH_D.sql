IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_OPCH_D' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_OPCH_D
GO--

CREATE PROCEDURE VID_SP_FE_OPCH_D
(
     @DocEntry			Int
    ,@TipoDoc			VarChar(10)
    ,@ObjType			VarChar(10)
)
AS
BEGIN
	SELECT
		 ROW_NUMBER() OVER(ORDER BY T0.LineaOrden, T0.LineaOrden2)		[NroLinDet] 
		,T0.DiscSum														[DescuentoMonto]
		,T0.DiscPrcnt													[DescuentoPct]
		,T0.Indicador_Exento											[IndExe]
		,T0.LineTotal													[MontoItem]
		,T0.Dscription													[NmbItem]
		,T0.Price														[PrcItem]
		,0.0															[PrcRef]
		,T0.Quantity													[QtyItem]
		,0.0															[QtyRef]
		,ISNULL(T0.RecargoMonto,0.0)									[RecargoMonto]
		,0.0															[RecargoPct]
		,T0.DET_UNIDAD_MEDIDA											[UnmdItem]
		,T0.ItemCode													[VlrCodigo]
		,T0.Dscription_Larga											[DscItem]
		
		,T0.CodImpAdic													[CodImpAdic]
		,T0.MontoImptoAdic												[MntImpAdic]
	FROM VID_VW_FE_OPCH_D T0
	WHERE 1 = 1
		AND T0.DocEntry = @DocEntry
		AND T0.ObjType = @ObjType;
END
