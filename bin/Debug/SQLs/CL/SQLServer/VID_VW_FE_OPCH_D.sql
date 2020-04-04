IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_VW_FE_OPCH_D' AND type = 'V')
   DROP VIEW VID_VW_FE_OPCH_D
GO--

CREATE VIEW VID_VW_FE_OPCH_D
AS
	SELECT
		 T0.FolioNum																			[FolioNum]
		,CASE
			WHEN T1.VatSum = 0 THEN 1
			ELSE 2
		 END																					[Indicador_Exento]
		,CASE T0.DocType
			WHEN 'S' THEN 'Servicio'
			ELSE LEFT(T1.ItemCode, 50)
		 END																					[ItemCode]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(T1.Dscription, 80))								[Dscription]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(T1.Dscription, 250))								[Dscription_Larga]
		,CASE
			WHEN T0.DocType = 'S' THEN 1
			ELSE T1.Quantity
		 END																					[Quantity]
		,CASE ISNULL(T1.Currency, 'CLP')
			WHEN 'CLP' THEN T1.PriceBefDi
			ELSE ROUND(T1.PriceBefDi * T1.Rate, 4)
		 END																					[Price]
		,CASE WHEN T1.DiscPrcnt	>= 0.0 THEN T1.DiscPrcnt ELSE 0.0 END							[DiscPrcnt]
		,CASE 
			WHEN ISNULL(T1.DiscPrcnt, 0.0) <= 0.0 THEN 0.0
			ELSE ROUND((CASE
							WHEN ISNULL(T1.Currency, 'CLP') = 'CLP' THEN T1.PriceBefDi
							ELSE ROUND(T1.PriceBefDi * T1.Rate, 2)
						END) * CASE
									WHEN T0.DocType = 'S' THEN 1
									ELSE T1.Quantity
							   END, 0) - T1.LineTotal
		 END																					[DiscSum]
		,CASE 
			WHEN ISNULL(T1.DiscPrcnt, 0.0) >= 0.0 THEN 0.0
			ELSE T1.LineTotal - ROUND((CASE
											WHEN ISNULL(T1.Currency, 'CLP') = 'CLP' THEN T1.PriceBefDi
											ELSE ROUND(T1.PriceBefDi * T1.Rate, 2)
										END) * CASE
													WHEN T0.DocType = 'S' THEN 1
													ELSE T1.Quantity
											   END, 0)
		 END																					[RecargoMonto]
		,ROUND(T1.LineTotal, 0)																	[LineTotal]
		,ISNULL(T4.U_CodImpto, '')																[CodImpAdic]
		,LEFT(ISNULL(T2.U_NAME, ''), 30)														[Usuario]
		,ISNULL(T3.WTAmnt, 0)																	[MontoImptoAdic]
		,ISNULL(T4.U_Porc,0.0)																	[PorcImptoAdic]
		,T1.Rate																				[Rate]
		,T1.Currency																			[Currency]
		,T1.VisOrder																			[LineaOrden]
		,1																						[LineaOrden2]
		,T0.ObjType																				[ObjType]
		,T0.DocEntry																			[DocEntry]
		,LEFT(ISNULL(T1.unitMsr, ''), 4)														[DET_UNIDAD_MEDIDA]
	FROM	  OPCH			   T0
		 JOIN PCH1			   T1 ON T1.DocEntry	= T0.DocEntry
		 JOIN OUSR			   T2 ON T2.INTERNAL_K	= T0.UserSign
		 JOIN NNM1			   N0 ON N0.Series	 	= T0.Series
								 AND N0.ObjectCode 	= T0.ObjType
	LEFT JOIN PCH5			   T3 ON T3.AbsEntry	= T0.DocEntry
	LEFT JOIN [@VID_FEIMPADIC] T4 ON T4.Code		= T3.WTCode
	, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND UPPER(LEFT(N0.BeginStr, 1)) = 'E'
		
	UNION ALL
	
	SELECT
		 T0.FolioNum																			[FolioNum]
		,2																						[Indicador_Exento]
		,'Texto'																				[ItemCode]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(CAST(T10.LineText AS VARCHAR(MAX)), 80))			[Dscription]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(CAST(T10.LineText AS VARCHAR(MAX)), 250))			[Dscription_Larga]
		,1.0																					[Quantity]
		,0.0																					[Price]
		,0.0																					[DiscPrcnt]
		,0.0																					[DiscSum]
		,0.0																					[RecargoMonto]
		,0.0																					[LineTotal]
		,''																						[CodImpAdic]
		,LEFT(ISNULL(U0.U_NAME, ''), 30)														[Usuario]
		,0.0																					[MontoImptoAdic]
		,0.0																					[PorcImptoAdic]
		,0.0																					[Rate]
		,''																						[Currency]
		,T10.AftLineNum																			[LineaOrden]
		,2																						[LineaOrden2]
		,T0.ObjType																				[ObjType]
		,T0.DocEntry																			[DocEntry]
		,''																						[DET_UNIDAD_MEDIDA]
	FROM OPCH	T0
	JOIN PCH10	T10	ON T10.DocEntry		= T0.DocEntry
	JOIN OUSR	U0	ON U0.INTERNAL_K	= T0.UserSign
	JOIN NNM1	N0	ON N0.Series	 	= T0.Series
				   AND N0.ObjectCode	= T0.ObjType
	, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND UPPER(LEFT(N0.BeginStr, 1)) = 'E'
	
	UNION ALL
	
	SELECT
		 T0.FolioNum																			[FolioNum]
		,CASE
			WHEN T1.VatSum = 0 THEN 1
			ELSE 2
		 END																					[Indicador_Exento]
		,CASE T0.DocType
			WHEN 'S' THEN 'Servicio'
			ELSE LEFT(T1.ItemCode, 50)
		 END																					[ItemCode]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(T1.Dscription, 80))								[Dscription]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(T1.Dscription, 250))								[Dscription_Larga]
		,CASE
			WHEN T0.DocType = 'S' THEN 1
			ELSE T1.Quantity
		 END																					[Quantity]
		,CASE ISNULL(T1.Currency, 'CLP')
			WHEN 'CLP' THEN T1.PriceBefDi
			ELSE ROUND(T1.PriceBefDi * T1.Rate, 4)
		 END																					[Price]
		,CASE WHEN T1.DiscPrcnt >= 0 THEN T1.DiscPrcnt ELSE 0 END								[DiscPrcnt]
		,CASE 
			WHEN ISNULL(T1.DiscPrcnt, 0.0) <= 0.0 THEN 0.0
			ELSE ROUND((CASE
							WHEN ISNULL(T1.Currency, 'CLP') = 'CLP' THEN T1.PriceBefDi
							ELSE ROUND(T1.PriceBefDi * T1.Rate, 2)
						END) * CASE
									WHEN T0.DocType = 'S' THEN 1
									ELSE T1.Quantity
							   END, 0) - T1.LineTotal
		 END																					[DiscSum]
		,CASE 
			WHEN ISNULL(T1.DiscPrcnt, 0.0) >= 0.0 THEN 0.0
			ELSE T1.LineTotal - ROUND((CASE
											WHEN ISNULL(T1.Currency, 'CLP') = 'CLP' THEN T1.PriceBefDi
											ELSE ROUND(T1.PriceBefDi * T1.Rate, 2)
										END) * CASE
													WHEN T0.DocType = 'S' THEN 1
													ELSE T1.Quantity
											   END, 0)
		 END																					[RecargoMonto]
		,ROUND(T1.LineTotal, 0)																	[LineTotal]
		,ISNULL(T4.U_CodImpto, '')																[CodImpAdic]
		,LEFT(ISNULL(T2.U_NAME, ''), 30)														[Usuario]
		,ISNULL(T3.WTAmnt, 0.0)																	[MontoImptoAdic]
		,ISNULL(T4.U_Porc,0.0)																	[PorcImptoAdic]
		,T1.Rate																				[Rate]
		,T1.Currency																			[Currency]
		,T1.VisOrder																			[LineaOrden]
		,1																						[LineaOrden2]
		,T0.ObjType																				[ObjType]
		,T0.DocEntry																			[DocEntry]
		,LEFT(ISNULL(T1.unitMsr, ''), 4)														[DET_UNIDAD_MEDIDA]
	FROM	  ODPO			   T0
	JOIN	  DPO1			   T1 ON T1.DocEntry	= T0.DocEntry
	JOIN	  OUSR			   T2 ON T2.INTERNAL_K	= T0.UserSign
		 JOIN NNM1			   N0 ON N0.Series		= T0.Series
								 AND N0.ObjectCode	= T0.ObjType
	LEFT JOIN DPO5			   T3 ON T3.AbsEntry	= T0.DocEntry
	LEFT JOIN [@VID_FEIMPADIC] T4 ON T4.Code		= T3.WTCode
	, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND UPPER(LEFT(N0.BeginStr, 1)) = 'E'
		
	UNION ALL
	
	SELECT
		 T0.FolioNum																			[FolioNum]
		,2																						[Indicador_Exento]
		,'Texto'																				[ItemCode]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(CAST(T10.LineText AS VARCHAR(MAX)), 80))			[Dscription]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(CAST(T10.LineText AS VARCHAR(MAX)), 250))			[Dscription_Larga]
		,1.0																					[Quantity]
		,0.0																					[Price]
		,0.0																					[DiscPrcnt]
		,0.0																					[DiscSum]
		,0.0																					[RecargoMonto]
		,0.0																					[LineTotal]
		,''																						[CodImpAdic]
		,LEFT(ISNULL(U0.U_NAME, ''), 30)														[Usuario]
		,0.0																					[MontoImptoAdic]
		,0.0																					[PorcImptoAdic]
		,0.0																					[Rate]
		,''																						[Currency]
		,T10.AftLineNum																			[LineaOrden]
		,2																						[LineaOrden2]
		,T0.ObjType																				[ObjType]
		,T0.DocEntry																			[DocEntry]
		,''																						[DET_UNIDAD_MEDIDA]
	FROM ODPO	T0
	JOIN DPO10	T10	ON T10.DocEntry		= T0.DocEntry
	JOIN OUSR	U0	ON U0.INTERNAL_K	= T0.UserSign
	JOIN NNM1	N0	ON N0.Series	 	= T0.Series
				   AND N0.ObjectCode	= T0.ObjType
	, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND UPPER(LEFT(N0.BeginStr, 1)) = 'E'
