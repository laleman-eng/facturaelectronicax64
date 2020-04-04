IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_LIBRO_VENTAS' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_LIBRO_VENTAS
GO-- 

CREATE PROCEDURE VID_SP_FE_LIBRO_VENTAS
(
	 @FechaD VarChar(8)
	,@FechaH VarChar(8)
)
--WITH ENCRYPTION
AS
BEGIN


/**FACTURAS, FACTURA EXENTA Y NOTAS DE DEBITO*/
SELECT CASE WHEN ISNULL(LEFT(UPPER(N0.BeginStr), 1),'') = 'E'
            THEN CASE WHEN T0.DocSubType = 'DN' AND ISNULL(N0.BeginStr,'')     LIKE '%111%' THEN '111'
					  WHEN T0.DocSubType = 'DN' AND ISNULL(N0.BeginStr,'') NOT LIKE '%111%' THEN '56'
                      WHEN T0.DocSubType = '--' THEN '33'
                      WHEN T0.DocSubType = 'IE' THEN '34'
                      WHEN T0.DocSubType = 'IX' THEN '110'
                      WHEN T0.DocSubType = 'EB' THEN '41'
                      WHEN T0.DocSubType = 'IB' THEN '39'                      
                 END
            ELSE CASE WHEN T0.DocSubType = 'DN' THEN '55'
                      WHEN T0.DocSubType = '--' THEN '30'
                      WHEN T0.DocSubType = 'IE' THEN '32'
                      WHEN T0.DocSubType = 'IX' THEN '101'
                      WHEN T0.DocSubType = 'EB' THEN '38'
                      WHEN T0.DocSubType = 'IB' THEN '35'
                 END
       END																										"SII"
      ,T0.DocDate																								"FECHA"
	  ,ISNULL(T0.FolioNum, 0)																					"NUMFACT"
	  ,T0.CardName																								"RAZSOC2"
	  ,CASE WHEN T0.DocSubType = 'IX'				  THEN '55555555-5'
			WHEN T0.DocSubType = 'DN'
			  AND ISNULL(N0.BeginStr,'') LIKE '%111%' THEN '55555555-5'
													  ELSE REPLACE(ISNULL(T0.LicTradNum, ''), '.', '')    
	   END																										"RUTFACT"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code IN ('IVA', 'IVAND') THEN T4.BaseSum
															 ELSE 0
					  END), 0) AS BIGINT)																		"TOTNETO"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IVA' THEN T4.TaxSum
	  											 ELSE 0
					  END) , 0) AS BIGINT)																		"TOTIVA"
      ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IMP1' THEN T4.TaxSum
	  											  ELSE 0
					  END) , 0) AS BIGINT)																		"IMP1"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IMP2' THEN T4.TaxSum
	  											  ELSE 0
					  END) , 0) AS BIGINT)																		"IMP2"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code IN ('IVA', 'IVAND') THEN T4.BaseSum
															 ELSE 0
					  END), 0)  AS BIGINT)																		"AFECTO"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IVAEXE' THEN T4.BaseSum
													ELSE 0
					  END), 0)	AS BIGINT)																		"EXENTO"
	  ,CAST(ROUND(T0.DocTotal, 0) AS BIGINT)																	"TOTAL" 
	  ,0																										"IVAFueraPlazo" 
	  ,CASE WHEN T0.FolioNum = ISNULL(ANLD."OINVFolioNum", 0) THEN '1'
			WHEN T0.FolioNum = (   SELECT U_NUMERO 
		                                       FROM [@znul1] 
										      WHERE U_NUMERO = T0.FolioNum 
										        AND U_ORIG_DOC = 'V'
												AND U_TIPO = (CASE WHEN T0.DocSubType = 'DN' THEN '3'
																   WHEN T0.DocSubType = '--' THEN '1'
																   WHEN T0.DocSubType = 'IE' THEN '4'
																   WHEN T0.DocSubType = 'IX' THEN '5'
																   WHEN T0.DocSubType = 'EB' THEN '7'
																   WHEN T0.DocSubType = 'IB' THEN '6'
																   ELSE ''
															  END) 
									     )
															  THEN '1'
															  ELSE '0'
	   END																										"ANULADO"
FROM	  OINV			   T0
     JOIN INV1			   T1 ON T1.DocEntry   = T0.DocEntry
LEFT JOIN INV4			   T4 ON T4.DocEntry   = T0.DocEntry
							 AND T4.LineNum    = T1.LineNum
	 JOIN NNM1			   N0 ON T0.Series	   = N0.Series
LEFT JOIN [@VID_FEIMPTOD] IMP ON IMP.U_CodeImp = T4.StaCode
LEFT JOIN [@VID_FEDOCED]  IND ON T0.Indicator  = IND.U_Indicato
OUTER APPLY (	SELECT OINV.DocEntry													"OINVDocEntry"
					  ,OINV.FolioNum													"OINVFolioNum"
					  ,OINV.DocSubType													"OINVDocSubType"
				FROM OINV
				JOIN INV1			   ON OINV.DocEntry = INV1.DocEntry
				JOIN RIN1			   ON INV1.TargetType = '14'
									  AND INV1.TrgetEntry = RIN1.DocEntry
									  AND INV1.LineNum    = RIN1.BaseLine
				JOIN ORIN			   ON RIN1.DocEntry   = ORIN.DocEntry
				JOIN "@VID_FEREF" RENC ON ORIN.DocEntry   = RENC.U_DocEntry
									  AND ORIN.ObjType	  = RENC.U_DocSBO
									  AND ORIN.DocSubType = RENC.U_DocSubTp
				WHERE 1 = 1
				  AND OINV.DocEntry = T0.DocEntry
				  AND RENC.U_CodRef = '1'  
				GROUP BY OINV.DocEntry
						,OINV.FolioNum
						,OINV.DocSubType	  
				UNION

				SELECT OINV.DocEntry													"OINVDocEntry"
					  ,OINV.FolioNum													"OINVFolioNum"
					  ,OINV.DocSubType													"OINVDocSubType"
				FROM OINV
				JOIN NNM1 ON OINV.Series = NNM1.Series
				JOIN "@VID_FEREFD" RDNC ON OINV.DocEntry = RDNC.U_DocEntry
									   AND OINV.FolioNum = RDNC.U_DocFolio
									   AND OINV.DocTotal = RDNC.U_DocTotal
									   AND CASE WHEN ISNULL(LEFT(UPPER(NNM1.BeginStr), 1),'') = 'E'
												THEN CASE WHEN OINV.DocSubType = 'DN' THEN '56'
														  WHEN OINV.DocSubType = '--' THEN '33'
														  WHEN OINV.DocSubType = 'IE' THEN '34'
														  WHEN OINV.DocSubType = 'IX' THEN '101'
														  WHEN OINV.DocSubType = 'EB' THEN '41'
														  WHEN OINV.DocSubType = 'IB' THEN '39'                      
													 END
												ELSE CASE WHEN OINV.DocSubType = 'DN' THEN '55'
														  WHEN OINV.DocSubType = '--' THEN '30'
														  WHEN OINV.DocSubType = 'IE' THEN '32'
														  WHEN OINV.DocSubType = 'IX' THEN '101'
														  WHEN OINV.DocSubType = 'EB' THEN '38'
														  WHEN OINV.DocSubType = 'IB' THEN '35'
													 END
										   END = U_TipoDTE
				JOIN "@VID_FEREF"  RENC ON RDNC.DocEntry   = RENC.DocEntry
				JOIN ORIN			    ON ORIN.DocEntry   = RENC.U_DocEntry
									   AND ORIN.ObjType	   = RENC.U_DocSBO
									   AND ORIN.DocSubType = RENC.U_DocSubTp
				WHERE 1 = 1
				  AND OINV.DocEntry = T0.DocEntry
				  AND RENC.U_CodRef = '1'  
				GROUP BY OINV.DocEntry
						,OINV.FolioNum
						,OINV.DocSubType)  AS ANLD
WHERE 1 = 1
  AND T0.DocDate BETWEEN @FechaD AND @FechaH
  AND ISNULL(T0.FolioNum, 0) <> 0
  AND T0.DocSubType IN ('--', 'DN', 'IE', 'IX', 'EB', 'IB')
  AND ISNULL(IND.Code, '') <> 'NL'
GROUP BY N0.BeginStr
		,T0.DocSubType
		,T0.DocDate
		,T0.FolioNum
		,T0.CardName
		,T0.LicTradNum
		,T0.DocTotal
		,ANLD.OINVFolioNum
      
   
UNION ALL

/**ANTICIPO ELECTRONICO*/
SELECT CASE WHEN ISNULL(LEFT(UPPER(N0.BeginStr), 1),'') = 'E'
            THEN '33'
            ELSE ''
       END																										"SII"
      ,T0.DocDate																								"FECHA"
	  ,ISNULL(T0.FolioNum, 0)																					"NUMFACT"
	  ,T0.CardName																								"RAZSOC2"
	  ,CASE WHEN T0.DocSubType = 'IX'				  THEN '55555555-5'
			WHEN T0.DocSubType = 'DN'
			  AND ISNULL(N0.BeginStr,'') LIKE '%111%' THEN '55555555-5'
													  ELSE REPLACE(ISNULL(T0.LicTradNum, ''), '.', '')    
	   END																										"RUTFACT"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code IN ('IVA', 'IVAND') THEN T4.BaseSum
															 ELSE 0
					  END), 0) AS BIGINT)																		"TOTNETO"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IVA' THEN T4.TaxSum
	  											 ELSE 0
					  END) , 0) AS BIGINT)																		"TOTIVA"
      ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IMP1' THEN T4.TaxSum
	  											  ELSE 0
					  END) , 0) AS BIGINT)																		"IMP1"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IMP2' THEN T4.TaxSum
	  											  ELSE 0
					  END) , 0) AS BIGINT)																		"IMP2"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code IN ('IVA', 'IVAND') THEN T4.BaseSum
															 ELSE 0
					  END), 0)  AS BIGINT)																		"AFECTO"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IVAEXE' THEN T4.BaseSum
													ELSE 0
					  END), 0)	AS BIGINT)																		"EXENTO"
	  ,CAST(ROUND(T0.DocTotal, 0) AS BIGINT)																	"TOTAL" 
	  ,0																										"IVAFueraPlazo" 
	  ,CASE WHEN T0.FolioNum = ISNULL(ANLD."OINVFolioNum", 0) THEN '1'
															  ELSE '0'
	   END																										"ANULADO"
FROM	  ODPI			   T0
     JOIN DPI1			   T1 ON T1.DocEntry   = T0.DocEntry
LEFT JOIN DPI4			   T4 ON T4.DocEntry   = T0.DocEntry
							 AND T4.LineNum    = T1.LineNum
	 JOIN NNM1			   N0 ON T0.Series	   = N0.Series
LEFT JOIN [@VID_FEIMPTOD] IMP ON IMP.U_CodeImp = T4.StaCode
LEFT JOIN [@VID_FEDOCED]  IND ON T0.Indicator  = IND.U_Indicato
OUTER APPLY (	SELECT ODPI.DocEntry													"OINVDocEntry"
					  ,ODPI.FolioNum													"OINVFolioNum"
					  ,ODPI.DocSubType													"OINVDocSubType"
				FROM ODPI
				JOIN DPI1			   ON ODPI.DocEntry = DPI1.DocEntry
				JOIN RIN1			   ON DPI1.TargetType = '14'
									  AND DPI1.TrgetEntry = RIN1.DocEntry
									  AND DPI1.LineNum    = RIN1.BaseLine
				JOIN ORIN			   ON RIN1.DocEntry   = ORIN.DocEntry
				JOIN "@VID_FEREF" RENC ON ORIN.DocEntry   = RENC.U_DocEntry
									  AND ORIN.ObjType	  = RENC.U_DocSBO
									  AND ORIN.DocSubType = RENC.U_DocSubTp
				WHERE 1 = 1
				  AND ODPI.DocEntry = T0.DocEntry
				  AND RENC.U_CodRef = '1'  
				GROUP BY ODPI.DocEntry
						,ODPI.FolioNum
						,ODPI.DocSubType	  
				UNION
				
				SELECT ODPI.DocEntry													"OINVDocEntry"
					  ,ODPI.FolioNum													"OINVFolioNum"
					  ,ODPI.DocSubType													"OINVDocSubType"
				FROM ODPI
				JOIN NNM1 ON ODPI.Series = NNM1.Series
				JOIN "@VID_FEREFD" RDNC ON ODPI.DocEntry = RDNC.U_DocEntry
									   AND ODPI.FolioNum = RDNC.U_DocFolio
									   AND ODPI.DocTotal = RDNC.U_DocTotal
									   AND CASE WHEN ISNULL(LEFT(UPPER(NNM1.BeginStr), 1),'') = 'E'
												THEN '33'
												ELSE '-1'
										   END = RTRIM(LTRIM(REPLACE(UPPER(U_TipoDTE), 'A', '')))
				JOIN "@VID_FEREF"  RENC ON RDNC.DocEntry   = RENC.DocEntry
				JOIN ORIN			    ON ORIN.DocEntry   = RENC.U_DocEntry
									   AND ORIN.ObjType	   = RENC.U_DocSBO
									   AND ORIN.DocSubType = RENC.U_DocSubTp
				WHERE 1 = 1
				  AND ODPI.DocEntry = T0.DocEntry
				  AND RENC.U_CodRef = '1'  
				GROUP BY ODPI.DocEntry
						,ODPI.FolioNum
						,ODPI.DocSubType)  AS ANLD
WHERE 1 = 1
  --AND T0.DocDate BETWEEN @FechaD AND @FechaH
  AND ISNULL(T0.FolioNum, 0) <> 0
  AND ISNULL(LEFT(UPPER(N0.BeginStr), 1),'') = 'E'
  AND ISNULL(IND.Code, '') <> 'NL'
GROUP BY N0.BeginStr
		,T0.DocSubType
		,T0.DocDate
		,T0.FolioNum
		,T0.CardName
		,T0.LicTradNum
		,T0.DocTotal
		,ANLD.OINVFolioNum

UNION ALL

SELECT CASE WHEN ISNULL(LEFT(UPPER(N0.BeginStr), 1),'') = 'E'
			 AND ISNULL(N0.BeginStr,'') NOT LIKE '%112%' THEN '61'
			WHEN ISNULL(LEFT(UPPER(N0.BeginStr), 1),'') = 'E'
			 AND ISNULL(N0.BeginStr,'')     LIKE '%112%' THEN '112'
														 ELSE '60'

       END																										"SII"
      ,T0.TaxDate																								"FECHA"
	  ,ISNULL(T0.FolioNum, 0)																					"NUMFACT"
	  ,T0.CardName																								"RAZSOC2"
	  ,CASE WHEN ISNULL(LEFT(UPPER(N0.BeginStr), 1),'') = 'E'
			 AND ISNULL(N0.BeginStr,'') LIKE '%112%' THEN '55555555-5'
													 ELSE REPLACE(ISNULL(T0.LicTradNum, ''), '.', '')    
	   END																										"RUTFACT"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code IN ('IVA', 'IVAND') THEN T4.BaseSum
															 ELSE 0
					  END), 0) AS BIGINT)																		"TOTNETO"
	  ,CAST(ROUND(SUM(CASE WHEN ISNULL(R0.U_CodRef, '0') IN ('1', '3')
							AND ISNULL(IVFP.FolioNc, 0) <> 0
							AND DATEADD(MONTH, 3, IVFP.FechaRef) < T0.DocDate
                     THEN 0
                     ELSE CASE WHEN IMP.Code ='IVA' 
	                           THEN T4.TaxSum
	  		                   ELSE 0
	  	                  END
					 END), 0) AS BIGINT)																		"TOTIVA"
      ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IMP1' THEN T4.TaxSum
	  											  ELSE 0
					  END) , 0) AS BIGINT)																		"IMP1"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IMP2' THEN T4.TaxSum
	  											  ELSE 0
					  END) , 0) AS BIGINT)																		"IMP2"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code IN ('IVA', 'IVAND') THEN T4.BaseSum
															 ELSE 0
					  END), 0)  AS BIGINT)																		"AFECTO"
	  ,CAST(ROUND(SUM(CASE WHEN IMP.Code = 'IVAEXE' THEN T4.BaseSum
													ELSE 0
					  END), 0)	AS BIGINT)																		"EXENTO"
	  ,CAST(ROUND(T0.DocTotal, 0) AS BIGINT)																	"TOTAL" 
	  ,CAST(ROUND(SUM(CASE WHEN ISNULL(R0.U_CodRef, '0') IN ('1', '3')
							AND ISNULL(IVFP.FolioNc, 0) <> 0
							AND DATEADD(MONTH, 3, IVFP.FechaRef) < T0.DocDate
						   THEN CASE WHEN IMP.Code IN ('IVA', 'IVAND') 
									 THEN T4.TaxSum
	  								 ELSE 0
								END
						   ELSE 0
					  END), 0) AS BIGINT)																		"IVAFueraPlazo"
	  ,CASE WHEN T0.FolioNum = ISNULL(ANLD."ORINFolioNum", 0) THEN '1'
			WHEN T0.FolioNum = (   SELECT U_NUMERO 
		                                  FROM [@znul1] 
										  WHERE U_NUMERO = T0.FolioNum 
										    AND U_ORIG_DOC = 'V'
										    AND U_TIPO = (CASE WHEN T0.DocSubType = '--' THEN '2'
															   ELSE ''
														  END) 
									  )
															  THEN '1'
															  ELSE '0'
	   END																										"ANULADO" 
FROM	  ORIN			   T0
     JOIN RIN1			   T1 ON T1.DocEntry   = T0.DocEntry
LEFT JOIN RIN4			   T4 ON T4.DocEntry   = T0.DocEntry
							 AND T4.LineNum    = T1.LineNum
	 JOIN NNM1			   N0 ON T0.Series	   = N0.Series
LEFT JOIN "@VID_FEIMPTOD" IMP ON IMP.U_CodeImp = T4.StaCode
LEFT JOIN "@VID_FEDOCED"  IND ON T0.Indicator  = IND.U_Indicato
LEFT JOIN "@VID_FEREF"	   R0 ON T0.DocEntry   = R0.U_DocEntry
							 AND T0.ObjType	   = R0.U_DocSBO
							 AND T0.DocSubType = R0.U_DocSubTp
OUTER APPLY (	SELECT ORIN.DocEntry													"ORINDocEntry"
					  ,ORIN.FolioNum													"ORINFolioNum"
					  ,ORIN.DocSubType													"ORINDocSubType"
				FROM ORIN
				JOIN NNM1 ON ORIN.Series = NNM1.Series
				JOIN "@VID_FEREFD" RDNC ON ORIN.DocEntry = RDNC.U_DocEntry
									   AND ORIN.FolioNum = RDNC.U_DocFolio
									   AND ORIN.DocTotal = RDNC.U_DocTotal
									   AND CASE WHEN ISNULL(LEFT(UPPER(NNM1.BeginStr), 1),'') = 'E'
												 AND ISNULL(NNM1.BeginStr,'') NOT LIKE '%112%' THEN '61'
												WHEN ISNULL(LEFT(UPPER(NNM1.BeginStr), 1),'') = 'E'
												 AND ISNULL(NNM1.BeginStr,'')     LIKE '%112%' THEN '112'
																							   ELSE '60'
										   END = U_TipoDTE
				JOIN "@VID_FEREF"  RENC ON RDNC.DocEntry   = RENC.DocEntry
				JOIN OINV			    ON OINV.DocEntry   = RENC.U_DocEntry
									   AND OINV.ObjType	   = RENC.U_DocSBO
									   AND OINV.DocSubType = RENC.U_DocSubTp
				WHERE 1 = 1
				  AND ORIN.DocEntry = T0.DocEntry
				  AND RENC.U_CodRef = '1'  
				GROUP BY ORIN.DocEntry
						,ORIN.FolioNum
						,ORIN.DocSubType)  AS ANLD
  OUTER APPLY ( 				/****************REFERENCIA BASE FACTURA********************/
				SELECT CASE WHEN ISNULL(LEFT(NNM1.BeginStr, 1),'') = 'e'
							THEN CASE WHEN RIGHT(NNM1.BeginStr, 3) <> '112'  THEN '61'
									  WHEN RIGHT(NNM1.BeginStr, 3) =  '112'  THEN '112'
								 END
							ELSE '60'
					   END																					"TipoNc"
					  ,ORIN.FolioNum																		"FolioNc"
					  ,ORIN.DocDate																			"FechaNc"
					  ,CASE WHEN ISNULL(LEFT(NNM2.BeginStr, 1),'') = 'e'
							THEN CASE WHEN OINV.DocSubType = 'DN' AND RIGHT(NNM2.BeginStr, 3) <> '111' THEN '56'
									  WHEN OINV.DocSubType = 'DN' AND RIGHT(NNM2.BeginStr, 3) =  '111' THEN '111'
									  WHEN OINV.DocSubType = '--' THEN '33'
									  WHEN OINV.DocSubType = 'IE' THEN '34'
									  WHEN OINV.DocSubType = 'IX' THEN '110'
									  WHEN OINV.DocSubType = 'EB' THEN '41'
									  WHEN OINV.DocSubType = 'IB' THEN '39'                      
								 END
							ELSE CASE WHEN OINV.DocSubType = 'DN' THEN '55'
									  WHEN OINV.DocSubType = '--' THEN '30'
									  WHEN OINV.DocSubType = 'IE' THEN '32'
									  WHEN OINV.DocSubType = 'IX' THEN '101'
									  WHEN OINV.DocSubType = 'EB' THEN '38'
									  WHEN OINV.DocSubType = 'IB' THEN '35'
								 END
					   END																					"TipoRef"
					  ,OINV.DocDate																			"FechaRef"
				FROM ORIN ORIN
				JOIN "@VID_FEREF" RENC  ON ORIN.DocEntry   = RENC.U_DocEntry
									   AND ORIN.ObjType	   = RENC.U_DocSBO
									   AND ORIN.DocSubType = RENC.U_DocSubTp
				JOIN NNM1		  NNM1  ON ORIN.Series     = NNM1.Series
				JOIN RIN1		  RIN1  ON ORIN.DocEntry   = RIN1.DocEntry
				JOIN INV1		  INV1  ON RIN1.BaseEntry  = INV1.DocEntry
									   AND RIN1.BaseLine   = INV1.LineNum
									   AND RIN1.BaseType   = '13'
				JOIN OINV		  OINV  ON INV1.DocEntry   = OINV.DocEntry
				JOIN NNM1		  NNM2  ON OINV.Series     = NNM2.Series
				WHERE 1 = 1
				  AND ORIN.DocEntry = T0.DocEntry
				  AND ISNULL(ORIN.FolioNum, 0) <> 0
				  AND ISNULL(OINV.FolioNum, 0) <> 0
				  AND RENC.U_CodRef IN ('1', '3')
				GROUP BY ORIN.FolioNum
						,ORIN.DocDate
						,NNM1.BeginStr
						,NNM2.BeginStr
						,OINV.DocSubType
						,OINV.DocDate
			
				UNION
				
				/****************REFERENCIA CDU NOTA CREDITO********************/
				SELECT CASE WHEN ISNULL(LEFT(NNM1.BeginStr, 1),'') = 'e'
							THEN CASE WHEN RIGHT(NNM1.BeginStr, 3) <> '112'  THEN '61'
									  WHEN RIGHT(NNM1.BeginStr, 3) =  '112'  THEN '112'
								 END
							ELSE '60'
					   END																					"TipoNc"
					  ,ORIN.FolioNum																		"FolioNc"
					  ,ORIN.DocDate																			"FechaNc"
					  ,U_TipoDTE																			"TipoRef"
					  ,MIN(OINV.DocDate)																	"FechaRef"
				FROM ORIN		   ORIN
				JOIN NNM1		   NNM1  ON ORIN.Series     = NNM1.Series
				JOIN "@VID_FEREF"  RENC  ON ORIN.DocEntry   = RENC.U_DocEntry
									    AND ORIN.ObjType	= RENC.U_DocSBO
									    AND ORIN.DocSubType = RENC.U_DocSubTp
				JOIN "@VID_FEREFD" RDNC  ON RENC.DocEntry   = RDNC.DocEntry
				JOIN OINV		   OINV  ON OINV.DocEntry   = RDNC.U_DocEntry
									    AND OINV.FolioNum   = RDNC.U_DocFolio
									    AND OINV.DocTotal   = RDNC.U_DocTotal
				JOIN NNM1		    NNM2 ON OINV.Series     = NNM2.Series 
				WHERE 1 = 1
				 AND ORIN.DocEntry = T0.DocEntry
				 AND RENC.U_CodRef IN ('1', '3')
				 AND CASE WHEN ISNULL(LEFT(UPPER(NNM2.BeginStr), 1),'') = 'E'
							THEN CASE WHEN OINV.DocSubType = 'DN' AND ISNULL(NNM2.BeginStr,'')     LIKE '%111%' THEN '111'
									  WHEN OINV.DocSubType = 'DN' AND ISNULL(NNM2.BeginStr,'') NOT LIKE '%111%' THEN '56'
									  WHEN OINV.DocSubType = '--' THEN '33'
									  WHEN OINV.DocSubType = 'IE' THEN '34'
									  WHEN OINV.DocSubType = 'IX' THEN '110'
									  WHEN OINV.DocSubType = 'EB' THEN '41'
									  WHEN OINV.DocSubType = 'IB' THEN '39'                      
								 END
							ELSE CASE WHEN OINV.DocSubType = 'DN' THEN '55'
									  WHEN OINV.DocSubType = '--' THEN '30'
									  WHEN OINV.DocSubType = 'IE' THEN '32'
									  WHEN OINV.DocSubType = 'IX' THEN '101'
									  WHEN OINV.DocSubType = 'EB' THEN '38'
									  WHEN OINV.DocSubType = 'IB' THEN '35'
								 END
					   END = U_TipoDTE
				GROUP BY ORIN.FolioNum
						,NNM1.BeginStr
						,ORIN.DocDate
						,RDNC.U_TipoDTE
						
				UNION
				
				/****************REFERENCIA BASE ANTICIPO********************/
				SELECT CASE WHEN ISNULL(LEFT(NNM1.BeginStr, 1),'') = 'e'
							THEN CASE WHEN RIGHT(NNM1.BeginStr, 3) <> '112'  THEN '61'
									  WHEN RIGHT(NNM1.BeginStr, 3) =  '112'  THEN '112'
								 END
							ELSE '60'
					   END																					"TipoNc"
					  ,ORIN.FolioNum																		"FolioNc"
					  ,ORIN.DocDate																			"FechaNc"
					  ,CASE WHEN ISNULL(LEFT(NNM3.BeginStr, 1),'') = 'e' THEN '33'
																		 ELSE '-1'
					   END																					"TipoRef"
					  ,ODPI.DocDate																			"FechaRef"
				FROM ORIN ORIN
				JOIN "@VID_FEREF" RENC  ON ORIN.DocEntry   = RENC.U_DocEntry
									   AND ORIN.ObjType	   = RENC.U_DocSBO
									   AND ORIN.DocSubType = RENC.U_DocSubTp
				JOIN NNM1		  NNM1  ON ORIN.Series     = NNM1.Series
				JOIN RIN1		  RIN1  ON ORIN.DocEntry   = RIN1.DocEntry
				JOIN DPI1		  DPI1  ON RIN1.BaseEntry  = DPI1.DocEntry
									   AND RIN1.BaseLine   = DPI1.LineNum
									   AND RIN1.BaseType   = '203'
				JOIN ODPI		  ODPI  ON DPI1.DocEntry   = ODPI.DocEntry
				JOIN NNM1		  NNM3  ON ODPI.Series     = NNM3.Series
				WHERE 1 = 1
				  AND ORIN.DocEntry = T0.DocEntry
				  AND ISNULL(ORIN.FolioNum, 0) <> 0
				  AND ISNULL(ODPI.FolioNum, 0) <> 0
				  AND RENC.U_CodRef IN ('1', '3')
				  AND ISNULL(LEFT(NNM3.BeginStr, 1),'') = 'e'
				GROUP BY ORIN.FolioNum
						,ORIN.DocDate
						,NNM1.BeginStr
						,NNM3.BeginStr
						,ODPI.DocSubType
						,ODPI.DocDate
			
				UNION

				/****************REFERENCIA CDU ANTICIPO********************/
				SELECT CASE WHEN ISNULL(LEFT(NNM1.BeginStr, 1),'') = 'e'
							THEN CASE WHEN RIGHT(NNM1.BeginStr, 3) <> '112'  THEN '61'
									  WHEN RIGHT(NNM1.BeginStr, 3) =  '112'  THEN '112'
								 END
							ELSE '60'
					   END																					"TipoNc"
					  ,ORIN.FolioNum																		"FolioNc"
					  ,ORIN.DocDate																			"FechaNc"
					  ,REPLACE(U_TipoDTE, 'a', '')															"TipoRef"
					  ,MIN(ODPI.DocDate)																	"FechaRef"
				FROM ORIN		   ORIN
				JOIN NNM1		   NNM1  ON ORIN.Series     = NNM1.Series
				JOIN "@VID_FEREF"  RENC  ON ORIN.DocEntry   = RENC.U_DocEntry
									    AND ORIN.ObjType	= RENC.U_DocSBO
									    AND ORIN.DocSubType = RENC.U_DocSubTp
				JOIN "@VID_FEREFD" RDNC  ON RENC.DocEntry   = RDNC.DocEntry
				JOIN ODPI		   ODPI  ON ODPI.DocEntry   = RDNC.U_DocEntry
									    AND ODPI.FolioNum   = RDNC.U_DocFolio
									    AND ODPI.DocTotal   = RDNC.U_DocTotal
				JOIN NNM1		    NNM3 ON ODPI.Series     = NNM3.Series 
				WHERE 1 = 1
				 AND ORIN.DocEntry = T0.DocEntry
				 AND RENC.U_CodRef IN ('1', '3')
				 AND RDNC.U_TipoDTE = '33a'
				 AND CASE WHEN ISNULL(LEFT(UPPER(NNM3.BeginStr), 1),'') = 'E' THEN '33'
																			  ELSE ''
					   END = REPLACE(U_TipoDTE, 'a', '')
				GROUP BY ORIN.FolioNum
						,NNM1.BeginStr
						,ORIN.DocDate
						,RDNC.U_TipoDTE) AS IVFP
WHERE 1 = 1
  AND T0.DocDate BETWEEN @FechaD AND @FechaH
  AND ISNULL(T0.FolioNum, 0) <> 0
  AND ISNULL(IND.Code, '') <> 'NL'
GROUP BY N0.BeginStr
		,T0.DocSubType
		,T0.TaxDate
		,T0.FolioNum
		,T0.CardName
		,T0.LicTradNum
		,T0.DocTotal
		,ANLD.ORINFolioNum

END