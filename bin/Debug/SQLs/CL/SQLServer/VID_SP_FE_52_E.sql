IF EXISTS(SELECT name FROM sysobjects                
      WHERE name = 'VID_SP_FE_52_E' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_52_E
GO--

CREATE PROCEDURE [dbo].[VID_SP_FE_52_E]
(
     @DocEntry	Int
    ,@TipoDoc	VarChar(10)
    ,@ObjType	VarChar(10)
)
AS
BEGIN
	SELECT --IdDoc
		T0.DocDate															[FchEmis]
		,T0.DocDueDate														[FchVenc]
		,@TipoDoc															[TipoDTE]
		,T0.FolioNum														[Folio]
		,'0'																[IndServicio]
		,0.0																[MntBruto]
		,0.0																[MntCancel]
		,0.0																[SaldoInsol]
		,T0.IndTraslado														[IndTraslado]
		,T0.TipoDespacho													[TipoDespacho]
		--Emisor
		,T0.SlpName															[CdgVendedor]
		,UPPER(T0.TaxIdNum)													[RUTEmisor]
		,T0.RazonSocial														[RznSocial]
		,T0.GiroEmis														[GiroEmis]
		,T0.Sucursal														[Sucursal]
		,T0.TelefenoRecep													[Telefono]
		--Receptor
		,T0.CityB															[CiudadPostal]
		,T0.CityS															[CiudadRecep]
		,T0.CountyB															[CmnaPostal]
		,T0.CountyS															[CmnaRecep]
		,T0.Contacto														[Contacto]
		,T0.E_Mail															[CorreoRecep]
		,T0.StreetB															[DirPostal]
		,T0.StreetS															[DirRecep]
		,T0.Giro															[GiroRecep]
		,UPPER(T0.LicTradNum)												[RUTRecep]
		,T0.CardName														[RznSocRecep]
		--Totales
		,0																	[CredEC]
		,T0.Total_Impuesto													[IVA]
		,0.0																[IVANoRet]
		,0.0																[IVAProp]
		,0.0																[IVATerc]
		,0.0																[MntBase]
		,T0.Total_Exento													[MntExe]
		,0.0																[MntMargenCom]
		,T0.Total_Afecto													[MntNeto]
		,T0.DocTotal														[MntTotal]
		,0.0																[MontoNF]
		,0.0																[MontoPeriodo]
		,0.0																[SaldoAnterior]
		,T0.VatPercent														[TasaIVA]
		,0.0																[VlrPagar]
		,T0.COMP															[COMP]
		,T0.DiscSum															[MntDescuento]
		,T0.MntGlobal														[MntGlobal]
		,ISNULL((SELECT SUM(MontoImptoAdic) 
		           FROM VID_VW_FE_52_D
				  WHERE DocEntry = T0.DocEntry
		            AND ObjType = T0.ObjType),0.0)							[MntImpAdic]
		--Activo Fijo
		,T0.TpoTranCompra													[TpoTranCompra]
		,T0.TpoTranVenta													[TpoTranVenta]
		,T0.CdgSIISucur														[CdgSIISucur]
		,T0.SucursalAF														[SucursalAF]

		,T0.FchPago															[FchPago]
		,T0.MntPago															[MntPago]
		,T0.GlosaPagos														[GlosaPagos]

		--Campos Extras
		,T1.CAB_EXTRA1														[Extra1]
		,T1.CAB_EXTRA2														[Extra2]
	FROM VID_VW_FE_52_E		  T0
	JOIN VID_VW_FE_52_E_EXTRA T1 ON T0.DocEntry = T1.DocEntry
								AND T0.ObjType  = T1.ObjType
	WHERE 1 = 1
		AND T0.DocEntry = @DocEntry
		AND T0.ObjType = @ObjType;
END
GO
