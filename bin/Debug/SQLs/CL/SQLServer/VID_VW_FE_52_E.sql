IF EXISTS(SELECT name FROM sysobjects                
      WHERE name = 'VID_VW_FE_52_E' AND type = 'V')
   DROP VIEW VID_VW_FE_52_E
GO--                                                 

CREATE VIEW [dbo].[VID_VW_FE_52_E]
AS
	SELECT
		 T0.FolioNum																														[FolioNum]
		,REPLACE(C0.LicTradNum, '.', '')																									[LicTradNum]
		,REPLACE(CONVERT(CHAR(10), T0.TaxDate, 102),'.','-')																				[DocDate]
		,REPLACE(CONVERT(CHAR(10), T0.DocDueDate, 102),'.','-')																				[DocDueDate]
		,ISNULL(T0.U_TipDespacho, '0')																										[TipoDespacho]
		,ISNULL(T0.U_Traslado, '0')																											[IndTraslado]
		,ROUND(T0.DocTotal, 0.0)																											[DocTotal]
		,ROUND(ISNULL((SELECT SUM(TaxSum)
						 FROM WTR4
						WHERE 1 = 1
						  AND DocEntry = T0.DocEntry
						  AND StaCode  = 'IVA'), 0.0), 0)																					[Total_Impuesto]
		,ROUND(T0.DocTotal - T0.VatSum, 0)																									[Total_Afecto]
		,ROUND(ISNULL((SELECT SUM(BaseSum)
						 FROM WTR4
						WHERE 1 = 1
						  AND DocEntry = T0.DocEntry
						  AND StaCode IN ('IVA_Exe')), 0),0)																				[Total_Exento]
		,''																																	[Codigo_Retencion]
		,0																																	[Tasa_Retencion]
		,0																																	[Total_Retencion]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(T0.CardName, 100))																				[CardName]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(B0.Street, '') + ' ' + ISNULL(B0.StreetNo, ''), 60))									[StreetB]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(B0.City, ''), 15))																		[CityB]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(B0.County, ''), 20))																	[CountyB]
		,LEFT(ISNULL(C0.E_Mail, ''), 80)																									[E_Mail]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(CAST(C0.Notes AS varchar(40)), 'Sin Giro Definido'), 40))								[Giro]
		,CASE WHEN T0.VatSum > 0 THEN CASE WHEN T0.VatPercent <> 0 THEN T0.VatPercent ELSE (SELECT Rate FROM OSTC WHERE Code = 'IVA') END
		      ELSE 0.0
		 END																																[VatPercent]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(S0.Street, '') + ' ' + ISNULL(B0.StreetNo, ''), 60))									[StreetS]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(S0.County, ''), 20))																	[CountyS]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(S0.City, ''), 15))																		[CityS]
		,LEFT(ISNULL(V0.SlpName, ''), 100)																									[SlpName]
		,LEFT(ISNULL(C0.Phone1, ''), 30)																									[Phone1]
		,T0.DocCur																															[DocCur]
		,ROUND(T0.DocRate, 4)																												[DocRate]
		,LEFT(ISNULL(U0.U_NAME, ''), 30)																									[Usuario]
		,ISNULL(T0.Comments, '')																											[Comments]
		,ROUND(T0.DiscSum + T0.DpmAmnt, 0)																									[DiscSum]
		,''																																	[Condicion_Pago]
		,ISNULL(T0.U_FETimbre, '')																											[XMLTED]
		,T0.ObjType																															[ObjType]
		,T0.DocEntry																														[DocEntry]
		,CAST(T0.ObjType + CAST(T0.DocEntry AS varchar) AS INT )																			[COMP]
		,ISNULL(A0.Phone1,'')																												[TelefenoRecep]
		,REPLACE(REPLACE(ISNULL(A0.TaxIdNum,''),'-',''),'.','')																				[TaxIdNum]
		,ISNULL(A0.CompnyName,'')																											[RazonSocial]
		,ISNULL(A1.GlblLocNum,'')																											[GiroEmis]
		,'Central'																															[Sucursal]
		,CAST('' AS VARCHAR(100))																											[Contacto]
		,0.0																																[MntGlobal]
		,ISNULL(T0.U_TpoTranCpra,'')																										[TpoTranCompra]
		,ISNULL(T0.U_TpoTranVta,'')																											[TpoTranVenta]
		,ISNULL(CAST(T0.U_CdgSiiSuc AS VARCHAR(9)),'0')																						[CdgSIISucur]
		,ISNULL(T0.U_FESucursal,'')																											[SucursalAF]	
		,''																																	[FchPago]
		,0																																	[MntPago] 
		,''																																	[GlosaPagos]
	FROM	  OWTR T0
			 JOIN OCRD C0 ON C0.CardCode	= T0.CardCode
			 JOIN OUSR U0 ON U0.INTERNAL_K	= T0.UserSign
			 JOIN OWHS D0 ON D0.WhsCode		= T0.Filler
			 JOIN OWHS H0 ON H0.WhsCode		= T0.ToWhsCode
		 	 JOIN NNM1 N1 ON N1.Series		= T0.Series
		 				 AND N1.ObjectCode	= T0.ObjType
		LEFT JOIN OSLP V0 ON V0.SlpCode		= T0.SlpCode
		LEFT JOIN CRD1 B0 ON B0.CardCode	= C0.CardCode
						 AND B0.Address		= C0.BillToDef
						 AND B0.AdresType	= 'B'
		LEFT JOIN CRD1 S0 ON S0.CardCode	= C0.CardCode
						 AND S0.Address		= C0.ShipToDef
						 AND S0.AdresType	= 'S'
		, OADM A0, ADM1 A1, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND UPPER(LEFT(N1.BeginStr, 1)) = 'E'
	
	UNION ALL

	SELECT
		 T0.FolioNum																														[FolioNum]
		,REPLACE(T0.LicTradNum, '.', '')																									[LicTradNum]
		,REPLACE(CONVERT(CHAR(10), T0.TaxDate, 102),'.','-')																				[DocDate]
		,REPLACE(CONVERT(CHAR(10), T0.DocDueDate, 102),'.','-')																				[DocDueDate]
		,ISNULL(T0.U_TipDespacho, '0')																										[TipoDespacho]
		,ISNULL(T0.U_Traslado, '0')																											[IndTraslado]
		,ROUND(T0.DocTotal, 0)																												[DocTotal]
		,ROUND(ISNULL((SELECT SUM(TaxSum)
						 FROM RPD4
						WHERE 1 = 1
						  AND DocEntry = T0.DocEntry
						  AND StaCode  = 'IVA'), 0.0), 0)																					[Total_Impuesto]
		,ROUND(T0.DocTotal - T0.VatSum, 0)																									[Total_Afecto]
		,ROUND(ISNULL((SELECT SUM(BaseSum)
						 FROM RPD4
						WHERE 1 = 1
						  AND DocEntry = T0.DocEntry
						  AND StaCode IN ('IVA_EXE')), 0.0),0)																				[Total_Exento]
		,''																																	[Codigo_Retencion]
		,0																																	[Tasa_Retencion]
		,0																																	[Total_Retencion]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(T0.CardName, 100))																				[CardName]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.StreetB, '') + ' ' + ISNULL(T12.StreetNoB, ''), 60))								[StreetB]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CityB, ''), 15))																	[CityB]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CountyB, ''), 20))																	[CountyB]
		,LEFT(ISNULL(C0.E_Mail, ''), 80)																									[E_Mail]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(CAST(C0.Notes AS VARCHAR(40)), 'Sin Giro Definido'), 40))								[Giro]
		,CASE WHEN T0.VatSum > 0 THEN CASE WHEN T0.VatPercent <> 0 THEN T0.VatPercent ELSE (SELECT Rate FROM OSTC WHERE Code = 'IVA') END
		      ELSE 0.0
		 END																																[VatPercent]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.StreetS, '') + ' ' + ISNULL(T12.StreetNoS, ''), 60))								[StreetS]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CountyS, ''), 20))																	[CountyS]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CityS, ''), 15))																	[CityS]
		,LEFT(ISNULL(V0.SlpName, ''), 100)																									[SlpName]
		,LEFT(ISNULL(C0.Phone1, ''), 30)																									[Phone1]
		,T0.DocCur																															[DocCur]
		,ROUND(T0.DocRate, 4)																												[DocRate]
		,LEFT(ISNULL(U0.U_NAME, ''), 30)																									[Usuario]
		,ISNULL(T0.Comments, '')																											[Comments]
		,ROUND(T0.DiscSum + T0.DpmAmnt, 0)																									[DiscSum]
		,ISNULL(T5.PymntGroup, '')																											[Condicion_Pago]
		,ISNULL(T0.U_FETimbre, '')																											[XMLTED]
		,T0.ObjType																															[ObjType]
		,T0.DocEntry																														[DocEntry]
		,CAST(T0.ObjType + CAST(T0.DocEntry AS varchar) AS INT )																			[COMP]
		,ISNULL(A0.Phone1,'')																												[TelefenoRecep]
		,REPLACE(REPLACE(ISNULL(A0.TaxIdNum,''),'-',''),'.','')																				[TaxIdNum]
		,ISNULL(A0.CompnyName,'')																											[RazonSocial]
		,ISNULL(A1.GlblLocNum,'')																											[GiroEmis]
		,'Central'																															[Sucursal]
		,ISNULL(C1.Name,'')																													[Contacto]
		,0.0																																[MntGlobal]	
		,ISNULL(T0.U_TpoTranCpra,'')																										[TpoTranCompra]
		,ISNULL(T0.U_TpoTranVta,'')																											[TpoTranVenta]
		,ISNULL(CAST(T0.U_CdgSiiSuc AS VARCHAR(9)),'0')																						[CdgSIISucur]
		,ISNULL(T0.U_FESucursal,'')																											[SucursalAF]		
		,''																																	[FchPago]
		,0																																	[MntPago] 
		,''																																	[GlosaPagos]
	FROM	  ORPD	T0	
		 JOIN RPD12	T12	ON T12.DocEntry		= T0.DocEntry
		 JOIN OCRD	C0	ON C0.CardCode		= T0.CardCode
		 JOIN OUSR	U0	ON U0.INTERNAL_K	= T0.UserSign
		 JOIN NNM1	N1	ON N1.Series		= T0.Series
		 			   AND N1.ObjectCode	= T0.ObjType
	LEFT JOIN OSLP	V0	ON V0.SlpCode		= T0.SlpCode
	LEFT JOIN OCTG	T5	ON T5.GroupNum		= T0.GroupNum
	LEFT JOIN OCPR  C1  ON C1.CntctCode		= T0.CntctCode
	, OADM A0, ADM1 A1, [@VID_FEPARAM] PA0
	WHERE 1 = 1
	  AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
	  AND UPPER(LEFT(N1.BeginStr, 1)) = 'E'
	  
	  
	  
	 UNION ALL

	SELECT
		 T0.FolioNum																														[FolioNum]
		,REPLACE(T0.LicTradNum, '.', '')																									[LicTradNum]
		,CONVERT(CHAR(10), T0.TaxDate, 103)																									[DocDate]
		,CONVERT(CHAR(10), T0.DocDueDate, 103)																								[DocDueDate]
		,ISNULL(T0.U_TipDespacho, '0')																										[CAB_TIPO_DESPACHO]
		,ISNULL(T0.U_Traslado, '0')																											[CAB_IND_TRASLADO]
		,ROUND(T0.DocTotal, 0)																												[DocTotal]
		,ROUND(ISNULL((SELECT SUM(TaxSum)
						 FROM DLN4
						WHERE 1 = 1
						  AND DocEntry = T0.DocEntry
						  AND StaCode  = 'IVA'), 0.0), 0)																					[Total_Impuesto]
		,ROUND(T0.DocTotal - T0.VatSum, 0)																									[Total_Afecto]
		,ROUND(ISNULL((SELECT SUM(BaseSum)
						 FROM DLN4
						WHERE 1 = 1
						  AND DocEntry = T0.DocEntry
						  AND StaCode IN ('IVA_EXE')), 0.0),0)																				[Total_Exento]
		,''																																	[Codigo_Retencion]
		,0																																	[Tasa_Retencion]
		,0																																	[Total_Retencion]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(T0.CardName, 100))																				[CardName]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.StreetB, '') + ' ' + ISNULL(T12.StreetNoB, ''), 60))								[StreetB]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CityB, ''), 15))																	[CityB]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CountyB, ''), 20))																	[CountyB]
		,LEFT(ISNULL(C0.E_Mail, ''), 80)																									[E_Mail]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(CAST(C0.Notes AS VARCHAR(40)), 'Sin Giro Definido'), 40))								[Giro]
		,CASE WHEN T0.VatSum > 0 THEN CASE WHEN T0.VatPercent <> 0 THEN T0.VatPercent ELSE (SELECT Rate FROM OSTC WHERE Code = 'IVA') END
		      ELSE 0.0
		 END																																[VatPercent]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.StreetS, '') + ' ' + ISNULL(T12.StreetNoS, ''), 60))								[StreetS]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CountyS, ''), 20))																	[CountyS]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CityS, ''), 15))																	[CityS]
		,LEFT(ISNULL(V0.SlpName, ''), 100)																									[SlpName]
		,LEFT(ISNULL(C0.Phone1, ''), 30)																									[Phone1]
		,T0.DocCur																															[DocCur]
		,ROUND(T0.DocRate, 4)																												[DocRate]
		,LEFT(ISNULL(U0.U_NAME, ''), 30)																									[Usuario]
		,ISNULL(T0.Comments, '')																											[Comments]
		,ROUND(T0.DiscSum + T0.DpmAmnt, 0)																									[DiscSum]
		,ISNULL(T5.PymntGroup, '')																											[Condicion_Pago]
		,ISNULL(T0.U_FETimbre, '')																											[XMLTED]
		,T0.ObjType																															[ObjType]
		,T0.DocEntry																														[DocEntry]
		,CAST(T0.ObjType + CAST(T0.DocEntry AS varchar) AS INT )																			[COMP]
		,ISNULL(A0.Phone1,'')																												[TelefenoRecep]
		,REPLACE(REPLACE(ISNULL(A0.TaxIdNum,''),'-',''),'.','')																				[TaxIdNum]
		,ISNULL(A0.CompnyName,'')																											[RazonSocial]
		,ISNULL(A1.GlblLocNum,'')																											[GiroEmis]
		,'Central'																															[Sucursal]
		,ISNULL(C1.Name,'')																													[Contacto]
		,0.0																																[MntGlobal]	
		,ISNULL(T0.U_TpoTranCpra,'')																										[TpoTranCompra]
		,ISNULL(T0.U_TpoTranVta,'')																											[TpoTranVenta]
		,ISNULL(CAST(T0.U_CdgSiiSuc AS VARCHAR(9)),'0')																						[CdgSIISucur]
		,ISNULL(T0.U_FESucursal,'')																											[SucursalAF]
		,REPLACE(CONVERT(CHAR(10), T0.DocDueDate, 102),'.','-')																				[FchPago]
		,T0.DocTotal																														[MntPago] 
		,T5.PymntGroup																														[GlosaPagos]
	FROM	  ODLN	T0
			 JOIN DLN12	T12	ON T12.DocEntry		= T0.DocEntry
			 JOIN OCRD	C0	ON C0.CardCode		= T0.CardCode
			 JOIN OUSR	U0	ON U0.INTERNAL_K	= T0.UserSign
		 	 JOIN NNM1	N1	ON N1.Series		= T0.Series
		 				   AND N1.ObjectCode	= T0.ObjType
		LEFT JOIN OSLP	V0	ON V0.SlpCode		= T0.SlpCode
		LEFT JOIN OCTG	T5	ON T5.GroupNum		= T0.GroupNum
		LEFT JOIN OCPR  C1  ON C1.CntctCode		= T0.CntctCode
	, OADM A0, ADM1 A1, [@VID_FEPARAM] PA0
	WHERE 1 = 1
	  AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
	  AND UPPER(LEFT(N1.BeginStr, 1)) = 'E'
GO
