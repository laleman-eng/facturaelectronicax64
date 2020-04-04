IF EXISTS(SELECT name FROM sysobjects                
      WHERE name = 'VID_VW_FE_NotaCredito_D' AND type = 'V')
   DROP VIEW VID_VW_FE_NotaCredito_D
GO--                                                 

CREATE VIEW [dbo].[VID_VW_FE_NotaCredito_D]
AS
	SELECT
		 T0.FolioNum																			[FolioNum]
		,CASE
			WHEN T1.VatSum = 0.0 THEN 1
			ELSE 2
		 END																					[Indicador_Exento]
		,CASE T0.DocType
			WHEN 'S' THEN 'Servicio'
			ELSE LEFT(T1.ItemCode, 50)
		 END																					[ItemCode]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(T1.Dscription, 80))								[Dscription]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(T1.Dscription, 250))								[Dscription_Larga]
		,LEFT(ISNULL(T1.unitMsr, ''),4)															[DET_UNIDAD_MEDIDA]
		,CASE
			WHEN T0.DocType = 'S' THEN 1
			ELSE T1.Quantity
		 END																					[Quantity]
		,CASE WHEN RIGHT(N0.BeginStr, 3) = '112' THEN LTRIM(STR(T1.PriceBefDi,18,4))
												 ELSE CASE ISNULL(T1.Currency, 'CLP') WHEN 'CLP' THEN T1.PriceBefDi
																					  WHEN '$'   THEN T1.PriceBefDi
																								 ELSE ROUND(T1.PriceBefDi * T1.Rate, 4)
													  END										  
		 END																					[Price]
		,CASE WHEN T1.DiscPrcnt >= 0.0 THEN T1.DiscPrcnt ELSE 0.0 END							[DiscPrcnt]
		,CASE 
			WHEN ISNULL(T1.DiscPrcnt, 0.0) <= 0.0 THEN 0.0
				   ELSE CASE WHEN RIGHT(N0.BeginStr, 3) = '112' THEN LTRIM(STR(T1.PriceBefDi * CASE WHEN T0.DocType = 'S' THEN 1
																														  ELSE T1.Quantity
																							   END - CASE T0.CurSource WHEN 'L' THEN ROUND(T1.LineTotal, 0)
																													   WHEN 'S' THEN T1.TotalSumSy
																													   WHEN 'C' THEN T1.TotalFrgn
																									 END,18,4))
																ELSE ROUND((CASE WHEN ISNULL(T1.Currency, 'CLP') IN ('CLP', '$') THEN T1.PriceBefDi
																																 ELSE ROUND(T1.PriceBefDi * T1.Rate, 2)
																			END) * CASE WHEN T0.DocType = 'S' THEN 1
																											  ELSE T1.Quantity
																				   END, 0) - T1.LineTotal
				   END
																
		 END																					[DiscSum]
		,CASE 
			WHEN ISNULL(T1.DiscPrcnt, 0.0) >= 0.0 THEN 0.0
				   ELSE CASE WHEN RIGHT(N0.BeginStr, 3) = '112' THEN LTRIM(STR(T1.PriceBefDi * CASE WHEN T0.DocType = 'S' THEN 1
																														  ELSE T1.Quantity
																							   END - CASE T0.CurSource WHEN 'L' THEN ROUND(T1.LineTotal, 0)
																													   WHEN 'S' THEN T1.TotalSumSy
																													   WHEN 'C' THEN T1.TotalFrgn
																									 END,18,4))
																ELSE T1.LineTotal - ROUND((CASE WHEN ISNULL(T1.Currency, 'CLP') IN ('CLP', '$') THEN T1.PriceBefDi
																																				 ELSE ROUND(T1.PriceBefDi * T1.Rate, 2)
																							END) * CASE WHEN T0.DocType = 'S' THEN 1
																															  ELSE T1.Quantity
																								   END, 0)
				   END
																
		 END																					[RecargoMonto]
		,CASE WHEN RIGHT(N0.BeginStr, 3) = '112' THEN CASE T0.CurSource WHEN 'L' THEN ROUND(T1.LineTotal, 0)
																	    WHEN 'S' THEN T1.TotalSumSy
																	    WHEN 'C' THEN T1.TotalFrgn
													  END
			  ELSE ROUND(T1.LineTotal, 0)	
		 END																					[LineTotal]
		,ISNULL(F0.U_CodImpto, '')																[CodImpAdic]
		,LEFT(ISNULL(U0.U_NAME, ''), 30)														[Usuario]
		,ISNULL(T4.TaxSum, 0.0)																	[MontoImptoAdic]
		,ISNULL(F0.U_Porc,0.0)																	[PorcImptoAdic]
		,T1.Rate																				[Rate]
		,T1.Currency																			[Currency]
		,T1.VisOrder																			[LineaOrden]
		,1																						[LineaOrden2]
		,T0.ObjType																				[ObjType]
		,T0.DocEntry																			[DocEntry]
		,T1.U_TipoDTELF																			[TpoDocLiq]
		,CAST(T1.U_FolioLiqF AS VARCHAR(20))													[FolioRefLF]
	FROM	  ORIN			   T0
		 JOIN RIN1			   T1 ON T1.DocEntry	= T0.DocEntry
		 JOIN OUSR			   U0 ON U0.INTERNAL_K	= T0.UserSign
		 JOIN NNM1			   N0 ON N0.Series		= T0.Series
								 AND N0.ObjectCode	= T0.ObjType
	LEFT JOIN RIN4			   T4 ON T4.DocEntry	= T0.DocEntry
								 AND T4.LineNum		= T1.LineNum
								 AND T4.StaCode NOT IN ('IVA', 'IVA_EXE')
	LEFT JOIN [@VID_FEIMPADIC] F0 ON F0.Code		= T4.StaCode
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
		,''																						[DET_UNIDAD_MEDIDA]
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
		,''																						[TpoDocLiq]
		,''																						[FolioRefLF]
	FROM ORIN  T0
	JOIN RIN10 T10 ON T10.DocEntry	= T0.DocEntry
	JOIN OUSR  U0  ON U0.INTERNAL_K	= T0.UserSign
	JOIN NNM1  N0  ON N0.Series		= T0.Series
				  AND N0.ObjectCode	= T0.ObjType
	, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND UPPER(LEFT(N0.BeginStr, 1)) = 'E'
	
	UNION ALL
	
	SELECT
		 T0.FolioNum																			[FolioNum]
		,CASE
			WHEN T1.VatSum = 0.0 THEN 1
			ELSE 2
		 END																					[Indicador_Exento]
		,CASE T0.DocType
			WHEN 'S' THEN 'Servicio'
			ELSE LEFT(T1.ItemCode, 50)
		 END																					[ItemCode]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(T1.Dscription, 80))								[Dscription]
		,dbo.VID_FN_FE_LimpiaCaracteres(LEFT(T1.Dscription, 250))								[Dscription_Larga]
		,LEFT(ISNULL(T1.unitMsr, ''),4)															[DET_UNIDAD_MEDIDA]
		,CASE
			WHEN T0.DocType = 'S' THEN 1
			ELSE T1.Quantity
		 END																					[Quantity]
		,CASE ISNULL(T1.Currency, 'CLP')
			WHEN 'CLP' THEN T1.PriceBefDi
			WHEN '$'   THEN T1.PriceBefDi
			ELSE ROUND(T1.PriceBefDi * T1.Rate, 4)
		 END																					[Price]
		,CASE WHEN T1.DiscPrcnt >= 0.0 THEN T1.DiscPrcnt ELSE 0.0 END							[DiscPrcnt]
		,CASE 
			WHEN ISNULL(T1.DiscPrcnt, 0.0) <= 0.0 THEN 0.0
			ELSE ROUND((CASE
							WHEN ISNULL(T1.Currency, 'CLP') IN ('CLP', '$') THEN T1.PriceBefDi
							ELSE ROUND(T1.PriceBefDi * T1.Rate, 2)
						END) * CASE
									WHEN T0.DocType = 'S' THEN 1
									ELSE T1.Quantity
							   END, 0) - T1.LineTotal
		 END																					[DiscSum]
		,CASE 
			WHEN ISNULL(T1.DiscPrcnt, 0.0) >= 0.0 THEN 0.0
			ELSE T1.LineTotal - ROUND((CASE
											WHEN ISNULL(T1.Currency, 'CLP') IN ('CLP', '$') THEN T1.PriceBefDi
											ELSE ROUND(T1.PriceBefDi * T1.Rate, 2)
										END) * CASE
													WHEN T0.DocType = 'S' THEN 1
													ELSE T1.Quantity
											   END, 0)
		 END																					[RecargoMonto]
		,ROUND(T1.LineTotal, 0)																	[LineTotal]
		,ISNULL(F0.U_CodImpto, '')																[CodImpAdic]
		,LEFT(ISNULL(U0.U_NAME, ''), 30)														[Usuario]
		,ISNULL(T5.WTAmnt, 0.0)																	[MontoImptoAdic]
		,ISNULL(F0.U_Porc,0.0)																	[PorcImptoAdic]
		,T1.Rate																				[Rate]
		,T1.Currency																			[Currency]
		,T1.VisOrder																			[LineaOrden]
		,1																						[LineaOrden2]
		,T0.ObjType																				[ObjType]
		,T0.DocEntry																			[DocEntry]
		,''																						[TpoDocLiq]
		,''																						[FolioRefLF]
	FROM	  ORPC			   T0
	JOIN	  RPC1			   T1 ON T1.DocEntry	= T0.DocEntry
	JOIN	  OUSR			   U0 ON U0.INTERNAL_K	= T0.UserSign
	JOIN	  NNM1			   N0 ON N0.Series		= T0.Series
								 AND N0.ObjectCode	= T0.ObjType
	LEFT JOIN RPC5			   T5 ON T5.AbsEntry	= T0.DocEntry
	LEFT JOIN [@VID_FEIMPADIC] F0 ON F0.Code		= T5.WTCode
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
		,''																						[DET_UNIDAD_MEDIDA]
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
		,''																						[TpoDocLiq]
		,''																						[FolioRefLF]
	FROM ORPC  T0
	JOIN RPC10 T10 ON T10.DocEntry	= T0.DocEntry
	JOIN OUSR  U0  ON U0.INTERNAL_K	= T0.UserSign
	JOIN NNM1  N0  ON N0.Series		= T0.Series
				  AND N0.ObjectCode	= T0.ObjType
	, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND UPPER(LEFT(N0.BeginStr, 1)) = 'E'
GO
