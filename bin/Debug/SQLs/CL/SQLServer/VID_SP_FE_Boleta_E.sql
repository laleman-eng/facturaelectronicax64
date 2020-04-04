IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_Boleta_E' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_Boleta_E
GO--
CREATE PROCEDURE [dbo].[VID_SP_FE_Boleta_E]
 @DocEntry Int ,@TipoDoc  Varchar(10), @ObjType VarChar(10)
AS
BEGIN
	SELECT --IdDoc
		T0.DocDate															[FchEmis]
		,T0.DocDueDate														[FchVenc]
		,@TipoDoc															[TipoDTE]
		,T0.FolioNum														[Folio]
		,CAST(T0.IndServicio AS VARCHAR)									[IndServicio]
		,0.0																[MntBruto]
		,0.0																[MntCancel]
		,0.0																[SaldoInsol]
		,T0.FmaPago															[FmaPago]
		--Activo Fijo
		,T0.TpoTranCompra													[TpoTranCompra]
		,T0.TpoTranVenta													[TpoTranVenta]
		,T0.CdgSIISucur														[CdgSIISucur]
		,T0.SucursalAF														[SucursalAF]
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
		,ISNULL(T0.OtraMoneda,0.0)											[OtraMoneda]
		,ISNULL((SELECT SUM(MontoImptoAdic) 
		           FROM VID_VW_FE_OINV_D
				  WHERE DocEntry = T0.DocEntry
		            AND ObjType = T0.ObjType),0.0)							[MntImpAdic]
		,T0.MntGlobal														[MntGlobal]
	FROM VID_VW_FE_OINV_E		T0
	WHERE 1 = 1
		AND T0.DocEntry = @DocEntry
		AND T0.ObjType = @ObjType;

END