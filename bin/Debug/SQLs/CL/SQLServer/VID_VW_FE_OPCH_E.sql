IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_VW_FE_OPCH_E' AND type = 'V')
   DROP VIEW VID_VW_FE_OPCH_E
GO--

CREATE VIEW VID_VW_FE_OPCH_E
AS
	SELECT
		 T0.FolioNum																							[FolioNum]
		,REPLACE(T0.LicTradNum, '.', '')																		[LicTradNum]
		,REPLACE(CONVERT(CHAR(10), T0.TaxDate, 102),'.','-')													[DocDate]
		,REPLACE(CONVERT(CHAR(10), T0.DocDueDate, 102),'.','-')													[DocDueDate]
		,ROUND(T0.DocTotal, 0)																					[DocTotal]
		,ROUND(T0.VatSum - ISNULL((SELECT                                                               		
									  SUM(TaxSum)                                                       		
								   FROM PCH4                                                            		
								   WHERE 1 = 1                                                          		
									  AND DocEntry = T0.DocEntry                                        		
									  AND StaCode NOT IN ('IVA', 'IVA_EXE')), 0), 0)							[Total_Impuesto]
		,ROUND(T0.DocTotal, 0)																					[Total_Afecto]
		,CASE                                                                                           		
			WHEN T0.DocSubType IN ('IE', 'EB') THEN ROUND(T0.DocTotal, 0)                               		
			ELSE 0                                                                                      		
		 END																									[Total_Exento]
		,ISNULL(LEFT(F0.U_CodImpto, 3), '')																		[Codigo_Retencion]
		,ISNULL(T6.Rate, 0)																						[Tasa_Retencion]
		,ISNULL(ROUND(T6.WTAmnt, 0), 0)																			[Total_Retencion]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(T0.CardName, 100))													[CardName]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.StreetB, '') + ' ' + ISNULL(T12.StreetNoB, ''), 60))	[StreetB]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CityB, ''), 15))										[CityB]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CountyB, ''), 20))										[CountyB]
		,LEFT(ISNULL(C0.E_Mail, ''), 80)																		[E_Mail]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(C0.Notes, 'Sin Giro Definido'), 40))						[Giro]
		,CASE WHEN T0.VatSum > 0 THEN CASE WHEN T0.VatPercent <> 0 THEN T0.VatPercent ELSE (SELECT Rate FROM OSTC WHERE Code = 'IVA') END
		      ELSE 0
		 END																									[VatPercent]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.StreetS, '') + ' ' + ISNULL(T12.StreetNoS, ''), 60))	[StreetS]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CountyS, ''), 20))										[CountyS]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CityS, ''), 15))										[CityS]
		,LEFT(ISNULL(V0.SlpName, ''), 100)																		[SlpName]
		,LEFT(ISNULL(C0.Phone1, ''), 30)																		[Phone1]
		,ISNULL(T0.U_VK_Patente, '')																			[Patente]
		,T0.DocCur																								[DocCur]
		,ROUND(T0.DocRate, 4)																					[DocRate]
		,LEFT(ISNULL(U0.U_NAME, ''), 30)																		[Usuario]
		,ISNULL(T0.Comments, '')																				[Comments]
		,ISNULL(ROUND(T0.DiscSum, 0), 0)																		[DiscSum]
		,T5.PymntGroup																							[Condicion_Pago]
		,ISNULL(T0.U_FETimbre, '')																				[XMLTED]
		,T0.ObjType																								[ObjType]
		,T0.DocEntry																							[DocEntry]
		,0																										[CodModalidadVenta]
		,0																										[TipoBultos]
		,0																										[CantidadBultos]
		,'0'																									[FormaPagoExp]
		,REPLACE(REPLACE(ISNULL(A0.TaxIdNum,''),'-',''),'.','')													[TaxIdNum]
		,ISNULL(A0.CompnyName,'')																				[RazonSocial]
		,ISNULL(A1.GlblLocNum,'')																				[GiroEmis]
		,'Central'																								[Sucursal]
		,ISNULL(C1.Name,'')																						[Contacto]
		,ISNULL(A0.Phone1,'')																					[TelefenoRecep]
		,CAST(T0.ObjType + CAST(T0.DocEntry AS varchar) AS INT )												[COMP]
		,CASE WHEN T0.DocSubType = 'IX' THEN ISNULL((SELECT GA0.LineTotal
                                                       FROM PCH3 GA0
                                                       JOIN OEXD EX0 ON EX0.ExpnsCode = GA0.ExpnsCode
                                                      WHERE GA0.DocEntry = T0.DocEntry
                                                        AND EX0.ExpnsName = 'Global'), 0.0)	ELSE 0.0 END		[MntGlobal]
	    ,ISNULL(T5.U_FmaPago,'2')																				[FmaPago]
		,''																										[FchPago]
		,0																										[MntPago] 
		,''																										[GlosaPagos]
	FROM	  OPCH				T0
		 JOIN PCH12 			T12	ON T12.DocEntry		= T0.DocEntry
		 JOIN OCRD				C0	ON C0.CardCode		= T0.CardCode
		 JOIN OUSR				U0	ON U0.INTERNAL_K	= T0.UserSign
		 JOIN NNM1				N1	ON N1.Series		= T0.Series
		 			   			   AND N1.ObjectCode	= T0.ObjType
	LEFT JOIN OCTG				T5	ON T5.GroupNum		= T0.GroupNum
	LEFT JOIN OSLP				V0	ON V0.SlpCode		= T0.SlpCode
	LEFT JOIN PCH5				T6	ON T6.AbsEntry		= T0.DocEntry
	LEFT JOIN [@VID_FEIMPADIC]	F0	ON F0.Code			= T6.WTCode
	LEFT JOIN OCPR  C1  ON C1.CntctCode		= T0.CntctCode
	,OADM A0, ADM1 A1, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		--AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND UPPER(LEFT(N1.BeginStr, 1)) = 'E'
		
	UNION ALL

	SELECT
		 T0.FolioNum																							[FolioNum]
		,REPLACE(T0.LicTradNum, '.', '')																		[LicTradNum]
		,REPLACE(CONVERT(CHAR(10), T0.TaxDate, 102),'.','-')															[DocDate]   
		,REPLACE(CONVERT(CHAR(10), T0.DocDueDate, 102),'.','-')															[DocDueDate]
		,CASE                                                                                           		
			WHEN T0.DocSubType NOT IN ('IE', 'EB') THEN ROUND((T0.DocTotal - T0.VatSum), 0)       		
			ELSE 0                                                                                      		
		 END																									[DocTotal]
		,ROUND(T0.VatSum - ISNULL((SELECT                                                             		
									  SUM(TaxSum)                                                   		
								   FROM DPO4                                                          		
								   WHERE 1 = 1                                                        		
									  AND DocEntry = T0.DocEntry                                 		
									  AND StaCode NOT IN ('IVA', 'IVA_EXE')), 0), 0)							[Total_Impuesto]
		,CASE                                                                                           			
			WHEN T0.DocSubType NOT IN ('IE', 'EB') THEN ROUND((T0.DocTotal - T0.VatSum), 0)       		    	
			ELSE 0                                                                                      			
		 END																									[Total_Afecto]
		,CASE                                                                                           			
			WHEN T0.DocSubType IN ('IE', 'EB') THEN ROUND(T0.DocTotal, 0)                           			
			ELSE 0                                                                                      			
		 END																									[Total_Exento]
		,ISNULL(LEFT(F0.U_CodImpto, 3), '')																		[Codigo_Retencion]
		,ISNULL(T6.Rate, 0)																						[Tasa_Retencion]
		,ISNULL(ROUND(T6.WTAmnt, 0), 0)																			[Total_Retencion]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(T0.CardName, 100))													[CardName]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.StreetB, '') + ' ' + ISNULL(T12.StreetNoB, ''), 60))	[StreetB]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CityB, ''), 15))										[CityB]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CountyB, ''), 20))										[CountyB]
		,LEFT(ISNULL(C0.E_Mail, ''), 80)																		[E_Mail]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(C0.Notes, 'Sin Giro Definido'), 40))						[Giro]
		,CASE WHEN T0.VatSum > 0 THEN CASE WHEN T0.VatPercent <> 0 THEN T0.VatPercent ELSE (SELECT Rate FROM OSTC WHERE Code = 'IVA') END
		      ELSE 0
		 END																									[VatPercent]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.StreetS, '') + ' ' + ISNULL(T12.StreetNoS, ''), 60))	[StreetS]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CountyS, ''), 20))										[CountyS]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(ISNULL(T12.CityS, ''), 15))										[CityS]
		,LEFT(ISNULL(V0.SlpName, ''), 100)																		[SlpName]
		,LEFT(ISNULL(C0.Phone1, ''), 30)																		[Phone1]
		,ISNULL(T0.U_VK_Patente, '')																			[Patente]
		,T0.DocCur																								[DocCur]
		,ROUND(T0.DocRate, 4)																					[DocRate]
		,LEFT(ISNULL(U0.U_NAME, ''), 30)																		[Usuario]
		,ISNULL(T0.Comments, '')																				[Comments]
		,ISNULL(ROUND(T0.DiscSum, 0), 0)																		[DiscSum]
		,T5.PymntGroup																							[Condicion_Pago]
		,ISNULL(T0.U_FETimbre, '')																				[XMLTED]
		,T0.ObjType																								[ObjType]
		,T0.DocEntry																							[DocEntry]
		,0																										[CodModalidadVenta]
		,0																										[TipoBultos]
		,0																										[CantidadBultos]
		,'0'																									[FormaPagoExp]
		,REPLACE(REPLACE(ISNULL(A0.TaxIdNum,''),'-',''),'.','')													[TaxIdNum]
		,ISNULL(A0.CompnyName,'')																				[RazonSocial]
		,ISNULL(A1.GlblLocNum,'')																				[GiroEmis]
		,'Central'																								[Sucursal]
		,ISNULL(C1.Name,'')																						[Contacto]
		,ISNULL(A0.Phone1,'')																					[TelefenoRecep]
		,CAST(T0.ObjType + CAST(T0.DocEntry AS varchar) AS INT )												[COMP]
		,CASE WHEN T0.DocSubType = 'IX' THEN ISNULL((SELECT GA0.LineTotal
                                                       FROM DPO3 GA0
                                                       JOIN OEXD EX0 ON EX0.ExpnsCode = GA0.ExpnsCode
                                                      WHERE GA0.DocEntry = T0.DocEntry
                                                        AND EX0.ExpnsName = 'Global'), 0.0)	ELSE 0.0 END		[MntGlobal]
	    ,ISNULL(T5.U_FmaPago,'2')																				[FmaPago]
		,''																										[FchPago]
		,0																										[MntPago] 
		,''																										[GlosaPagos]
	FROM	  ODPO				T0
		 JOIN DPO12 			T12	ON T12.DocEntry		= T0.DocEntry
		 JOIN OCRD				C0	ON C0.CardCode		= T0.CardCode
		 JOIN OUSR				U0	ON U0.INTERNAL_K	= T0.UserSign
		 JOIN NNM1				N1	ON N1.Series		= T0.Series
		 						   AND N1.ObjectCode	= T0.ObjType
	LEFT JOIN OCTG				T5	ON T5.GroupNum		= T0.GroupNum
	LEFT JOIN OSLP				V0	ON V0.SlpCode		= T0.SlpCode
	LEFT JOIN DPO5				T6	ON T6.AbsEntry		= T0.DocEntry
	LEFT JOIN [@VID_FEIMPADIC]	F0	ON F0.Code			= T6.WTCode
	LEFT JOIN OCPR  C1  ON C1.CntctCode		= T0.CntctCode
	,OADM A0, ADM1 A1, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		--AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND UPPER(LEFT(N1.BeginStr, 1)) = 'E'
