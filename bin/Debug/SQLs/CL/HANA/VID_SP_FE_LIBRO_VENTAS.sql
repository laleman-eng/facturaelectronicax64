DROP PROCEDURE VID_SP_FE_LIBRO_VENTAS;

CREATE PROCEDURE VID_SP_FE_LIBRO_VENTAS
(
	 IN FechaD	VarChar(8)
	,IN FechaH	VarChar(8)
)
LANGUAGE SqlScript
AS
BEGIN
	-- Facturas y Notas de débito
	docs1 = SELECT
				 CASE
				 	WHEN UPPER(IFNULL(LEFT(N0."BeginStr", 1), '')) = 'E'
					 	THEN CASE
		 						WHEN T0."DocSubType" = 'DN' THEN '56'
			 					WHEN T0."DocSubType" = '--' THEN '33'
			 					WHEN T0."DocSubType" = 'IE' THEN '34'
		 						WHEN T0."DocSubType" = 'IX' THEN '110'
		 						WHEN T0."DocSubType" = 'EB' THEN '41'
		 						WHEN T0."DocSubType" = 'IB' THEN '39'
		 					 END
					 	ELSE CASE
					 			WHEN T0."DocSubType" = 'DN' THEN '55'
					 			WHEN T0."DocSubType" = '--' THEN '30'
					 			WHEN T0."DocSubType" = 'IE' THEN '32'
					 			WHEN T0."DocSubType" = 'IX' THEN '101'
					 			WHEN T0."DocSubType" = 'EB' THEN '38'
					 			WHEN T0."DocSubType" = 'IB' THEN '35'
					 		 END
				 END																														"SII"
				,TO_CHAR(T0."TaxDate",'DD/MM/YYYY')																							"FECHA"
				,T0."DocEntry"																												"COMP"
				,IFNULL(T0."FolioNum", 0)																									"NUMFACT"
				,T0."CardName"																												"RAZSOC2"
				,REPLACE(IFNULL(T0."LicTradNum", ''), '.', '')																				"RUTFACT"
				,SUM(CASE							
						WHEN IMP."Code" = 'IVA' THEN T4."TaxSum"							
						ELSE 0							
					 END)																													"TOTIVA"
				,SUM(CASE																	
						WHEN IMP."Code" IN ('IVA', 'IVAND') THEN T4."BaseSum"																
						ELSE 0																	
					 END)																													"AFECTO"
				,SUM(CASE																	
						WHEN IMP."Code" = 'IVAEXE' THEN T4."BaseSum"																	
						ELSE 0																	
					 END)																													"EXENTO"
				,T0."DocTotal"																												"TOTAL"
				,SUM(CASE 
						WHEN RET."Code" = 'RETT' THEN T5."WTAmnt"
						ELSE 0
					 END)																													"IVARetTotal"
				,IFNULL(T0."U_CodIVANoRec", '0')																							"IVANoRec_CodIVANoRec"
				,SUM(CASE
						WHEN IMP."Code" = 'IVAND' THEN T4."TaxSum"
						ELSE 0                               
					 END)																													"IVANoRec_MntIVANoRec"
				,0																															"IVAFueraPlazo"
				,SUM(CASE 
						WHEN IND."Code" = 'AF' AND IMP."Code" = 'IVA' THEN T4."BaseSum"
						ELSE 0
					 END)																													"MntActivoFijo"
				,SUM(CASE
						WHEN IND."Code" = 'AF' AND IMP."Code" IN ('IVA', 'IVAND') THEN T4."TaxSum"
						ELSE 0
					 END)																													"MntIVAActivoFijo"
				,SUM(CASE 
						WHEN RET."Code" IN ('RETP', 'RETH') THEN T5."WTAmnt"
						ELSE 0
					 END)																													"IVARetParcial"
				,SUM(CASE 
						WHEN IMP."Code" = 'DIESEL' THEN T4."TaxSum"
						ELSE 0
					 END)																													"ivaDiesel"
				,SUM(CASE 
						WHEN IND."Code" = 'AF' AND IMP."Code" = 'IVAEXE' THEN T4."BaseSum"
						ELSE 0
					 END)																													"ExentoActivoFijo"
				,0																															"IVAUsoComun"
				,CASE							
					WHEN N0."BeginStr" IN ('e33', 'e34')							
						THEN CASE							
								 WHEN T0."FolioNum" IN (SELECT							
															"U_folioRef"							
														FROM ORIN							
														WHERE 1 = 1							
															AND "U_UpRef" = 1							
															AND "U_folioRef" = T0."FolioNum") THEN '1'							
								 ELSE '0'							
							 END							
					ELSE CASE							
							WHEN T0."FolioNum" = (SELECT							
							 						  "U_NUMERO"							
												  FROM "@ZNUL1"							
												  WHERE 1 = 1							
													  AND "U_NUMERO" = T0."FolioNum"							
												  	  AND "U_ORIG_DOC" = 'V'							
												  	  AND "U_TIPO" = (CASE							
												  						 WHEN T0."DocSubType" = 'DN' THEN '3'							
												  						 WHEN T0."DocSubType" = '--' THEN '1'							
																		 WHEN T0."DocSubType" = 'IE' THEN '4'							
																		 WHEN T0."DocSubType" = 'IX' THEN '5'							
																		 WHEN T0."DocSubType" = 'EB' THEN '7'							
																		 WHEN T0."DocSubType" = 'IB' THEN '6'							
																		 ELSE ''							
																	  END))	THEN '1'							
							ELSE '0'							
 						 END							
				 END																														"ANULADO"
			FROM	  OINV			  T0
				 JOIN INV1			  T1	ON T1."DocEntry"	= T0."DocEntry"
			LEFT JOIN INV4			  T4	ON T4."DocEntry"	= T0."DocEntry"
										   AND T4."LineNum"		= T1."LineNum"
			LEFT JOIN INV5			  T5	ON T5."AbsEntry"	= T0."DocEntry"
			LEFT JOIN "@VID_FEDOCED"  IND	ON IND."U_Indicato"	= T0."Indicator"
			LEFT JOIN "@VID_FEIMPTOD" IMP	ON IMP."U_CodeImp"	= T4."StaCode"
			LEFT JOIN "@VID_FEIMPTOD" RET	ON RET."U_CodeImp"	= T5."WTCode"
				 JOIN NNM1			  N0	ON N0."Series"		= T0."Series"
			,OADM A0
			WHERE 1 = 1
				AND T0."DocSubType" IN ('--', 'DN', 'IE', 'IX', 'EB', 'IB')
				AND IFNULL(IND."Code", '') NOT IN ('NL', 'NTL')
				AND T0."DocDate" BETWEEN :FechaD AND :FechaH
			GROUP BY
				 N0."BeginStr"
				,T0."DocSubType"
				,T0."DocEntry"
				,T0."DocDate"
				,T0."TaxDate"
				,T0."FolioNum"
				,T0."CardName"
				,T0."LicTradNum"
				,T0."U_CodIVANoRec"
				,T0."DocTotal";
				
	docs2 = SELECT
				 CASE
				 	WHEN UPPER(IFNULL(LEFT(N0."BeginStr", 1), '')) = 'E'
						THEN CASE
			 					WHEN T0."DocSubType" = '--' THEN '61'
			 				 END
					 	ELSE CASE
					 			WHEN T0."DocSubType" = '--' THEN '60'
					 		 END
				 END																														"SII"
				,TO_CHAR(T0."TaxDate",'DD/MM/YYYY')																							"FECHA"
				,T0."DocEntry"																												"COMP"
				,IFNULL(T0."FolioNum", 0)																									"NUMFACT"
				,T0."CardName"																												"RAZSOC2"
				,REPLACE(IFNULL(T0."LicTradNum", ''), '.', '')																				"RUTFACT"
				,SUM(CASE
						WHEN T0."U_UpRef" = '1' AND IFNULL(I0."FolioNum", 0) <> 0 AND DAYS_BETWEEN(T0."TaxDate", I0."TaxDate") >= 90 THEN 0
						ELSE CASE
								WHEN IMP."Code" = 'IVA' THEN T4."TaxSum"
								ELSE 0
							 END
					 END)																													"TOTIVA"
				,SUM(CASE										
						WHEN IMP."Code" IN ('IVA', 'IVAND') THEN T4."BaseSum"										
						ELSE 0										
					 END)																													"AFECTO"
				,SUM(CASE																
						WHEN IMP."Code" = 'IVAEXE' THEN T4."BaseSum"																
						ELSE 0																
					 END)																													"EXENTO"
				,T0."DocTotal"																												"TOTAL"
				,SUM(CASE 
						WHEN RET."Code" = 'RETT' THEN T5."WTAmnt"
						ELSE 0
					 END)																													"IVARetTotal"
				,IFNULL(T0."U_CodIVANoRec", '0')																							"IVANoRec_CodIVANoRec"
				,SUM(CASE
						WHEN IMP."Code" = 'IVAND' THEN T4."TaxSum"
						ELSE 0                               
					 END)																													"IVANoRec_MntIVANoRec"
				,SUM(CASE
						WHEN T0."U_UpRef" = '1' AND IFNULL(I0."FolioNum", 0) <> 0 AND DAYS_BETWEEN(T0."TaxDate", I0."TaxDate") >= 90
							THEN CASE
									WHEN IMP."Code" = 'IVA' THEN T4."TaxSum"
									ELSE 0
								 END
						ELSE 0
					 END)"IVAFueraPlazo"
				,SUM(CASE 
						WHEN IND."Code" = 'AF' AND IMP."Code" = 'IVA' THEN T4."BaseSum"
						ELSE 0
					 END)																													"MntActivoFijo"
				,SUM(CASE
						WHEN IND."Code" = 'AF' AND IMP."Code" IN ('IVA', 'IVAND') THEN T4."TaxSum"
						ELSE 0
					 END)																													"MntIVAActivoFijo"
				,SUM(CASE 
						WHEN RET."Code" IN ('RETP', 'RETH') THEN T5."WTAmnt"
						ELSE 0
					 END)																													"IVARetParcial"
				,SUM(CASE 
						WHEN IMP."Code" = 'DIESEL' THEN T4."TaxSum"
						ELSE 0
					 END)																													"ivaDiesel"
				,SUM(CASE 
						WHEN IND."Code" = 'AF' AND IMP."Code" = 'IVAEXE' THEN T4."BaseSum"
						ELSE 0
					 END)																													"ExentoActivoFijo"
				,0																															"IVAUsoComun"
				,CASE
					WHEN N0."BeginStr" = 'e61'
						THEN CASE
								 WHEN T0."FolioNum" IN (SELECT
															"U_folioRef"
														FROM OINV
														WHERE 1 = 1
															AND "U_UpRef" = '1'
															AND "U_folioRef" = T0."FolioNum") THEN '1'
								 ELSE '0'
							 END
					ELSE CASE
							WHEN T0."FolioNum" = (SELECT
							 						  "U_NUMERO"
												  FROM "@ZNUL1"
												  WHERE 1 = 1
													  AND "U_NUMERO" = T0."FolioNum"
												  	  AND "U_ORIG_DOC" = 'V'
												  	  AND "U_TIPO" = (CASE
												  						 WHEN T0."DocSubType" = '--' THEN '2'
																		 ELSE ''
																	  END))	THEN '1'
							ELSE '0'
 						 END
				 END																														"ANULADO"
			FROM	  ORIN			  T0
				 JOIN RIN1			  T1	ON T1."DocEntry"	= T0."DocEntry"
			LEFT JOIN RIN4			  T4	ON T4."DocEntry"	= T0."DocEntry"
										   AND T4."LineNum"		= T1."LineNum"
			LEFT JOIN RIN5			  T5	ON T5."AbsEntry"	= T0."DocEntry"
			LEFT JOIN "@VID_FEDOCED"  IND	ON IND."U_Indicato"	= T0."Indicator"
			LEFT JOIN "@VID_FEIMPTOD" IMP	ON IMP."U_CodeImp"	= T4."StaCode"
			LEFT JOIN "@VID_FEIMPTOD" RET	ON RET."U_CodeImp"	= T5."WTCode"
				 JOIN NNM1			  N0	ON N0."Series"		= T0."Series"
			/* Inicio referencia al documento base */
			LEFT JOIN INV1			  I1	ON I1."TrgetEntry"	= T1."DocEntry"
										   AND I1."LineNum"		= T1."LineNum"
										   AND I1."TargetType"	= T0."ObjType"
			LEFT JOIN OINV			  I0	ON I1."DocEntry"	= I0."DocEntry"
			/* Fin referencia al documento base*/
			,OADM A0
			WHERE 1 = 1
				AND IFNULL(IND."Code", '') NOT IN ('NL', 'NTL')
				AND T0."DocDate" BETWEEN :FechaD AND :FechaH
			GROUP BY
				 N0."BeginStr"
				,T0."DocSubType"
				,T0."DocEntry"
				,T0."DocDate"
				,T0."TaxDate"
				,T0."FolioNum"
				,T0."CardName"
				,T0."LicTradNum"
				,T0."U_CodIVANoRec"
				,T0."DocTotal";
	
	v_out = CE_UNION_ALL(:docs1, :docs2);
	
	SELECT
		 T0."SII"
		,T0."FECHA"
		,T0."COMP"
		,T0."NUMFACT"
		,T0."RAZSOC2"
		,T0."RUTFACT"
		,T0."TOTIVA"
		,T0."AFECTO"
		,T0."EXENTO"
		,T0."TOTAL"
		,T0."IVARetTotal"
		,T0."IVANoRec_CodIVANoRec"
		,T0."IVANoRec_MntIVANoRec"
        ,T0."MntActivoFijo"
        ,T0."MntIVAActivoFijo"
        ,T0."IVARetParcial"
        ,T0."ivaDiesel"
        ,T0."ExentoActivoFijo"
        ,T0."IVAUsoComun"
		,T0."ANULADO"
	FROM :v_out T0
	ORDER BY
		 T0."SII"
		,T0."FECHA"
		,T0."NUMFACT";
END;    
