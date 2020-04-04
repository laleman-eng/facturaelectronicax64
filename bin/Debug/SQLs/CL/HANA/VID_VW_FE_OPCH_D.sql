--DROP VIEW VID_VW_FE_OPCH_D;
CREATE VIEW VID_VW_FE_OPCH_D
AS
	SELECT
		 T0."FolioNum"																			"FolioNum"
		,CASE
			WHEN T1."VatSum" = 0.0 THEN 1
			ELSE 2
		 END																					"Indicador_Exento"
		,CASE T0."DocType"
			WHEN 'S' THEN 'Servicio'
			ELSE LEFT(T1."ItemCode", 50)
		 END																					"ItemCode"
		,LEFT(T1."Dscription", 80)																"Dscription"
		,LEFT(T1."Dscription", 250)																"Dscription_Larga"
		,CASE
			WHEN T0."DocType" = 'S' THEN 1
			ELSE T1."Quantity"
		 END																					"Quantity"
		,CASE IFNULL(T1."Currency", 'CLP')
			WHEN 'CLP' THEN T1."PriceBefDi"
			ELSE ROUND(T1."PriceBefDi" * T1."Rate", 4)
		 END																					"Price"
		,CASE WHEN T1."DiscPrcnt" >= 0.0 THEN T1."DiscPrcnt" ELSE 0.0 END						"DiscPrcnt"
		,CASE 
			WHEN IFNULL(T1."DiscPrcnt", 0.0) <= 0.0 THEN 0.0
			ELSE ROUND((CASE
							WHEN IFNULL(T1."Currency", 'CLP') = 'CLP' THEN T1."PriceBefDi"
							ELSE ROUND(T1."PriceBefDi" * T1."Rate", 2)
						END) * CASE
									WHEN T0."DocType" = 'S' THEN 1
									ELSE T1."Quantity"
							   END, 0) - T1."LineTotal"
		 END																					"DiscSum"
		,CASE 
			WHEN IFNULL(T1."DiscPrcnt", 0.0) >= 0.0 THEN 0.0
			ELSE T1."LineTotal" - ROUND((CASE
											WHEN IFNULL(T1."Currency", 'CLP') = 'CLP' THEN T1."PriceBefDi"
											ELSE ROUND(T1."PriceBefDi" * T1."Rate", 2)
										END) * CASE
													WHEN T0."DocType" = 'S' THEN 1
													ELSE T1."Quantity"
											   END, 0)
		 END																					"RecargoMonto"
		,ROUND(T1."LineTotal", 0)																"LineTotal"
		,IFNULL(T4."U_CodImpto", '')															"CodImpAdic"
		,LEFT(IFNULL(T2."U_NAME", ''), 30)														"Usuario"
		,IFNULL(T3."WTAmnt", 0.0)																"MontoImptoAdic"
		,IFNULL(T4."U_Porc",0.0)																"PorcImptoAdic"
		,T1."Rate"																				"Rate"
		,T1."Currency"																			"Currency"
		,T1."VisOrder"																			"LineaOrden"
		,1																						"LineaOrden2"
		,T0."ObjType"																			"ObjType"
		,T0."DocEntry"																			"DocEntry"
		,LEFT(IFNULL(T1."unitMsr", ''), 4)														"DET_UNIDAD_MEDIDA"
	FROM	  OPCH			   T0
		 JOIN PCH1			   T1 ON T1."DocEntry"	 = T0."DocEntry"
		 JOIN OUSR			   T2 ON T2."INTERNAL_K" = T0."UserSign"
		 JOIN NNM1			   N0 ON N0."Series"	 = T0."Series"
								 AND N0."ObjectCode" = T0."ObjType"
	LEFT JOIN PCH5			   T3 ON T3."AbsEntry"	 = T0."DocEntry"
	LEFT JOIN "@VID_FEIMPADIC" T4 ON T4."Code"		 = T3."WTCode"
	WHERE 1 = 1
		--AND IFNULL(T0."FolioNum", 0) <> 0
		AND UPPER(LEFT(N0."BeginStr", 1)) = 'E'
		
	UNION ALL
	
	SELECT
		 T0."FolioNum"																			"FolioNum"
		,2																						"Indicador_Exento"
		,'Texto'																				"ItemCode"
		,LEFT(T10."LineText", 80)																"Dscription"
		,LEFT(T10."LineText", 250)																"Dscription_Larga"
		,1.0																					"Quantity"
		,0.0																					"Price"
		,0.0																					"DiscPrcnt"
		,0.0																					"DiscSum"
		,0.0																					"RecargoMonto"
		,0.0																					"LineTotal"
		,''																						"CodImpAdic"
		,LEFT(IFNULL(U0."U_NAME", ''), 30)														"Usuario"
		,0.0																					"MontoImptoAdic"
		,0.0																					"PorcImptoAdic"
		,0.0																					"Rate"
		,''																						"Currency"
		,T10."AftLineNum"																		"LineaOrden"
		,2																						"LineaOrden2"
		,T0."ObjType"																			"ObjType"
		,T0."DocEntry"																			"DocEntry"
		,''																						"DET_UNIDAD_MEDIDA"
	FROM OPCH	T0
	JOIN PCH10	T10	ON T10."DocEntry"	= T0."DocEntry"
	JOIN OUSR	U0	ON U0."INTERNAL_K"	= T0."UserSign"
	JOIN NNM1	N0	ON N0."Series"	 	= T0."Series"
				   AND N0."ObjectCode"	= T0."ObjType"
	WHERE 1 = 1
		--AND IFNULL(T0."FolioNum", 0) <> 0
		AND UPPER(LEFT(N0."BeginStr", 1)) = 'E'
	
	UNION ALL
	
	SELECT
		 T0."FolioNum"																			"FolioNum"
		,CASE
			WHEN T1."VatSum" = 0 THEN 1
			ELSE 2
		 END																					"Indicador_Exento"
		,CASE T0."DocType"
			WHEN 'S' THEN 'Servicio'
			ELSE LEFT(T1."ItemCode", 50)
		 END																					"ItemCode"
		,LEFT(T1."Dscription", 80)																"Dscription"
		,LEFT(T1."Dscription", 250)																"Dscription_Larga"
		,CASE
			WHEN T0."DocType" = 'S' THEN 1
			ELSE T1."Quantity"
		 END																					"Quantity"
		,CASE IFNULL(T1."Currency", 'CLP')
			WHEN 'CLP' THEN T1."PriceBefDi"
			ELSE ROUND(T1."PriceBefDi" * T1."Rate", 4)
		 END																					"Price"
		,CASE WHEN T1."DiscPrcnt" >= 0.0 THEN T1."DiscPrcnt" ELSE 0.0 END						"DiscPrcnt"
		,CASE 
			WHEN IFNULL(T1."DiscPrcnt", 0.0) <= 0.0 THEN 0.0
			ELSE ROUND((CASE
							WHEN IFNULL(T1."Currency", 'CLP') = 'CLP' THEN T1."PriceBefDi"
							ELSE ROUND(T1."PriceBefDi" * T1."Rate", 2)
						END) * CASE
									WHEN T0."DocType" = 'S' THEN 1
									ELSE T1."Quantity"
							   END, 0) - T1."LineTotal"
		 END																					"DiscSum"
		,CASE 
			WHEN IFNULL(T1."DiscPrcnt", 0.0) >= 0.0 THEN 0.0
			ELSE T1."LineTotal" - ROUND((CASE
											WHEN IFNULL(T1."Currency", 'CLP') = 'CLP' THEN T1."PriceBefDi"
											ELSE ROUND(T1."PriceBefDi" * T1."Rate", 2)
										END) * CASE
													WHEN T0."DocType" = 'S' THEN 1
													ELSE T1."Quantity"
											   END, 0)
		 END																					"RecargoMonto"
		,ROUND(T1."LineTotal", 0)																"LineTotal"
		,IFNULL(T4."U_CodImpto", '')															"CodImpAdic"
		,LEFT(IFNULL(T2."U_NAME", ''), 30)														"Usuario"
		,IFNULL(T3."WTAmnt", 0.0)																"MontoImptoAdic"
		,IFNULL(T4."U_Porc",0.0)																"PorcImptoAdic"
		,T1."Rate"																				"Rate"
		,T1."Currency"																			"Currency"
		,T1."VisOrder"																			"LineaOrden"
		,1																						"LineaOrden2"
		,T0."ObjType"																			"ObjType"
		,T0."DocEntry"																			"DocEntry"
		,LEFT(IFNULL(T1."unitMsr", ''), 4)														"DET_UNIDAD_MEDIDA"
	FROM	  ODPO			   T0
	JOIN	  DPO1			   T1 ON T1."DocEntry"	 = T0."DocEntry"
	JOIN	  OUSR			   T2 ON T2."INTERNAL_K" = T0."UserSign"
		 JOIN NNM1			   N0 ON N0."Series"	 = T0."Series"
								 AND N0."ObjectCode" = T0."ObjType"
	LEFT JOIN DPO5			   T3 ON T3."AbsEntry"	 = T0."DocEntry"
	LEFT JOIN "@VID_FEIMPADIC" T4 ON T4."Code"		 = T3."WTCode"
	WHERE 1 = 1
		--AND IFNULL(T0."FolioNum", 0) <> 0
		AND UPPER(LEFT(N0."BeginStr", 1)) = 'E'
		
	UNION ALL
	
	SELECT
		 T0."FolioNum"																			"FolioNum"
		,2																						"Indicador_Exento"
		,'Texto'																				"ItemCode"
		,LEFT(T10."LineText", 80)																"Dscription"
		,LEFT(T10."LineText", 250)																"Dscription_Larga"
		,1.0																					"Quantity"
		,0.0																					"Price"
		,0.0																					"DiscPrcnt"
		,0.0																					"DiscSum"
		,0.0																					"RecargoMonto"
		,0.0																					"LineTotal"
		,''																						"CodImpAdic"
		,LEFT(IFNULL(U0."U_NAME", ''), 30)														"Usuario"
		,0.0																					"MontoImptoAdic"
		,0.0																					"PorcImptoAdic"
		,0.0																					"Rate"
		,''																						"Currency"
		,T10."AftLineNum"																		"LineaOrden"
		,2																						"LineaOrden2"
		,T0."ObjType"																			"ObjType"
		,T0."DocEntry"																			"DocEntry"
		,''																						"DET_UNIDAD_MEDIDA"
	FROM ODPO	T0
	JOIN DPO10	T10	ON T10."DocEntry"	= T0."DocEntry"
	JOIN OUSR	U0	ON U0."INTERNAL_K"	= T0."UserSign"
	JOIN NNM1	N0	ON N0."Series"	 	= T0."Series"
				   AND N0."ObjectCode"	= T0."ObjType"
	WHERE 1 = 1
		--AND IFNULL(T0."FolioNum", 0) <> 0
		AND UPPER(LEFT(N0."BeginStr", 1)) = 'E'
