IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_LIBRO_COMPRAS' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_LIBRO_COMPRAS
GO--

CREATE PROCEDURE VID_SP_FE_LIBRO_COMPRAS
(
	 @FechaD VarChar(8)
	,@FechaH VarChar(8)
)
--WITH ENCRYPTION 
AS
BEGIN


/*********************FACTURAS Y NOTAS DE DEBITO*********************/
SELECT CASE WHEN IND.Code = 'AFE' AND T0.DocSubType = '--' THEN '33'
            WHEN IND.Code = 'AFM' AND T0.DocSubType = '--' THEN '30'
			WHEN IND.Code = 'AFE' AND T0.DocSubType = 'DM' THEN '56'
            WHEN IND.Code = 'AFM' AND T0.DocSubType = 'DM' THEN '55'
            ELSE ISNULL(IND.Code, '')
       END																												"SII"
      ,T0.TaxDate																										"FECHA"
	  ,ISNULL(T0.FolioNum, 0)																							"NUMFACT"
	  ,T0.CardName																										"RAZSOC2"
	  ,REPLACE(ISNULL(T0.LicTradNum, ''), '.', '')																		"RUTFACT"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code IN ('IVA', 'IVAND') 
							AND T4.TaxSum > 0 THEN T4.BaseSum
											  ELSE 0
					  END)+ISNULL(GA.NETO, 0), 0) AS BIGINT)															"NETO"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IVA' THEN T4.TaxSum
	  											 ELSE 0
	                  END)+ISNULL(GA.IVA, 0), 0) AS BIGINT)																"TOTIVA"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code IN ('IVA', 'IVAND')
							AND T4.TaxSum > 0 THEN T4.BaseSum
											  ELSE 0
		              END)+ISNULL(GA.NETO, 0), 0) AS BIGINT)															"AFECTO"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IVAEXE'
							 OR T4.TaxSum = 0 THEN T4.BaseSum
											  ELSE 0
		              END)+ISNULL(GA.EXENTO, 0), 0) AS BIGINT)															"EXENTO"
	  ,CAST(ROUND(SUM(CASE WHEN T1.TaxOnly = 'Y'
						   THEN T1.LineTotal
						   ELSE 0
					  END)+T0.DocTotal-T0.RoundDif, 0) AS BIGINT)														"TOTAL"
      ,CAST(ROUND(SUM(CASE WHEN RET.Code = 'RETT' THEN T5.WTAmnt
												  ELSE 0
		              END), 0) AS BIGINT)																				"IVARetTotal"
	  ,CASE WHEN CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IVAND' THEN T4.TaxSum
															 ELSE 0                               
								END), 0) AS BIGINT) > 0 THEN ISNULL(T0.U_CodIVANoRec,0)
			ELSE 0
	   END	                 																							"IVANoRec_CodIVANoRec"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IVAND' THEN T4.TaxSum
												   ELSE 0                               
		              END), 0) AS BIGINT)																				"IVANoRec_MntIVANoRec"
	  ,CAST(ROUND(SUM(CASE WHEN IND.Code IN ('AFE', 'AFM')    
	                        AND IMP.Code = 'IVA' THEN T4.BaseSum
												 ELSE 0
	                  END), 0) AS BIGINT)																				"MntActivoFijo"
      ,CAST(ROUND(SUM(CASE WHEN IND.Code IN ('AFE', 'AFM') 
	                        AND IMP.Code = 'IVA' THEN T4.TaxSum
												 ELSE 0
		              END), 0) AS BIGINT)																				"MntIVAActivoFijo"
	  ,CAST(ROUND(SUM(CASE WHEN RET.Code IN ('RETP', 'RETH') THEN T5.WTAmnt
															 ELSE 0
		              END), 0) AS BIGINT)																				"IVARetParcial"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'DIESEL' 
		                   THEN T4.TaxSum
		                   ELSE 0
		              END), 0) AS BIGINT)																				"ivaDiesel"
	  ,0																												"IVAUsoComun"
FROM	  OPCH					T0
	 JOIN PCH1					T1 ON T1.DocEntry	   = T0.DocEntry
LEFT JOIN PCH4					T4 ON T4.DocEntry	   = T0.DocEntry
								  AND T4.LineNum	   = T1.LineNum
								  AND T4.RelateType   <> 3
LEFT JOIN PCH5					T5 ON T5.AbsEntry	   = T0.DocEntry
	 JOIN NNM1					N0 ON T0.Series	       = N0.Series
LEFT JOIN "@VID_FEIMPTOD"	   IMP ON IMP.U_CodeImp    = T4.StaCode
LEFT JOIN "@VID_FEDOCED"	   IND ON T0.Indicator     = IND.U_Indicato
LEFT JOIN "@VID_FEIMPTOD"	   RET ON T5.WTCode        = RET.U_CodeImp
OUTER APPLY (SELECT SUM(CASE WHEN T3.VatSum = 0 THEN T3.LineTotal
												ELSE 0
						END)																				"EXENTO"
				   ,SUM(CASE WHEN T3.VatSum > 0 THEN T3.LineTotal
												ELSE 0
						END)																				"NETO"
				   ,SUM(CASE WHEN T3.VatSum > 0 THEN T3.VatSum
												ELSE 0
						END)																				"IVA"
				   ,T3.DocEntry
			   FROM PCH3 T3
			  WHERE T3.DocEntry = T0.DocEntry
			  GROUP BY T3.DocEntry) GA
WHERE 1 = 1
  AND T0.DocSubType IN ('--', 'DM')
  AND ISNULL(IND.Code, '') NOT IN  ('NL') 
  AND ISNULL(T0.FolioNum, 0) <> 0
  AND T0.DocDate BETWEEN @FechaD AND @FechaH
GROUP BY IND.Code
		,T0.DocSubType
		,T0.TaxDate
		,T0.FolioNum
		,T0.CardName
		,T0.LicTradNum
		,T0.DocTotal
		,T0.RoundDif
		,T0.U_CodIVANoRec
		,T0.DocEntry
		,GA.NETO
		,GA.EXENTO
		,GA.IVA
			  
UNION ALL


/*********************NOTAS DE CREDITO*********************/
SELECT CASE WHEN IND.Code = 'AFE' THEN '61'
            WHEN IND.Code = 'AFM' THEN '60'
            ELSE ISNULL(IND.Code, '')
       END																												"SII"
      ,T0.TaxDate																										"FECHA"
	  ,ISNULL(T0.FolioNum, 0)																							"NUMFACT"
	  ,T0.CardName																										"RAZSOC2"
	  ,REPLACE(ISNULL(T0.LicTradNum, ''), '.', '')																		"RUTFACT"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code IN ('IVA', 'IVAND') 
							AND T4.TaxSum > 0 THEN T4.BaseSum
											  ELSE 0
					  END)+ISNULL(GA.NETO, 0), 0) AS BIGINT)															"NETO"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IVA' THEN T4.TaxSum
	  											 ELSE 0
	                  END)+ISNULL(GA.IVA, 0), 0) AS BIGINT)																"TOTIVA"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code IN ('IVA', 'IVAND')
							AND T4.TaxSum > 0 THEN T4.BaseSum
											  ELSE 0
		              END)+ISNULL(GA.NETO, 0), 0) AS BIGINT)															"AFECTO"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IVAEXE'
							 OR T4.TaxSum = 0 THEN T4.BaseSum
											  ELSE 0
		              END)+ISNULL(GA.EXENTO, 0), 0) AS BIGINT)															"EXENTO"
	  ,CAST(ROUND(SUM(CASE WHEN T1.TaxOnly = 'Y'
						   THEN T1.LineTotal
						   ELSE 0
					  END)+T0.DocTotal-T0.RoundDif, 0) AS BIGINT)														"TOTAL"
      ,CAST(ROUND(SUM(CASE WHEN RET.Code = 'RETT' THEN T5.WTAmnt
												  ELSE 0
		              END), 0) AS BIGINT)																				"IVARetTotal"
	  ,CASE WHEN CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IVAND' THEN T4.TaxSum
															 ELSE 0                               
								END), 0) AS BIGINT) > 0 THEN ISNULL(T0.U_CodIVANoRec,0)
			ELSE 0
	   END	                 																							"IVANoRec_CodIVANoRec"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IVAND' THEN T4.TaxSum
												   ELSE 0                               
		              END), 0) AS BIGINT)																				"IVANoRec_MntIVANoRec"
	  ,CAST(ROUND(SUM(CASE WHEN IND.Code IN ('AFE', 'AFM')    
	                        AND IMP.Code = 'IVA' THEN T4.BaseSum
												 ELSE 0
	                  END), 0) AS BIGINT)																				"MntActivoFijo"
      ,CAST(ROUND(SUM(CASE WHEN IND.Code IN ('AFE', 'AFM') 
	                        AND IMP.Code = 'IVA' THEN T4.TaxSum
												 ELSE 0
		              END), 0) AS BIGINT)																				"MntIVAActivoFijo"
	  ,CAST(ROUND(SUM(CASE WHEN RET.Code IN ('RETP', 'RETH') THEN T5.WTAmnt
															 ELSE 0
		              END), 0) AS BIGINT)																				"IVARetParcial"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'DIESEL' 
		                   THEN T4.TaxSum
		                   ELSE 0
		              END), 0) AS BIGINT)																				"ivaDiesel"
	  ,0																												"IVAUsoComun"
FROM	  ORPC					T0
	 JOIN RPC1					T1 ON T1.DocEntry	   = T0.DocEntry
LEFT JOIN RPC4					T4 ON T4.DocEntry	   = T0.DocEntry
								  AND T4.LineNum	   = T1.LineNum
LEFT JOIN RPC5					T5 ON T5.AbsEntry	   = T0.DocEntry
	 JOIN NNM1					N0 ON T0.Series	       = N0.Series
LEFT JOIN "@VID_FEIMPTOD"	   IMP ON IMP.U_CodeImp    = T4.StaCode
LEFT JOIN "@VID_FEDOCED"	   IND ON T0.Indicator     = IND.U_Indicato
LEFT JOIN "@VID_FEIMPTOD"	   RET ON T5.WTCode        = RET.U_CodeImp
OUTER APPLY (SELECT SUM(CASE WHEN T3.VatSum = 0 THEN T3.LineTotal
												ELSE 0
						END)																				"EXENTO"
				   ,SUM(CASE WHEN T3.VatSum > 0 THEN T3.LineTotal
												ELSE 0
						END)																				"NETO"
				   ,SUM(CASE WHEN T3.VatSum > 0 THEN T3.VatSum
												ELSE 0
						END)																				"IVA"
				   ,T3.DocEntry
			   FROM RPC3 T3
			  WHERE T3.DocEntry = T0.DocEntry
			  GROUP BY T3.DocEntry) GA
WHERE 1 = 1
  AND ISNULL(IND.Code, '') NOT IN  ('NL') 
  AND ISNULL(T0.FolioNum, 0) <> 0
  AND T0.DocDate BETWEEN @FechaD AND @FechaH
GROUP BY IND.Code
		,T0.DocSubType
		,T0.TaxDate
		,T0.FolioNum
		,T0.CardName
		,T0.LicTradNum
		,T0.DocTotal
		,T0.RoundDif
		,T0.U_CodIVANoRec
		,T0.DocEntry
		,GA.NETO
		,GA.EXENTO
		,GA.IVA


END

