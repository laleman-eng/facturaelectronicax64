--DROP VIEW VID_VW_FE_OPCH_E;
CREATE VIEW VID_VW_FE_OPCH_E
AS
	SELECT
		 T0."FolioNum"																					"FolioNum"
		,REPLACE(T0."LicTradNum", '.', '')																"LicTradNum"
		,TO_CHAR(T0."TaxDate",'YYYY-MM-DD')																"DocDate"
		,TO_CHAR(T0."DocDueDate",'YYYY-MM-DD')															"DocDueDate"
		,ROUND(T0."DocTotal", 0)																		"DocTotal"
		,ROUND(T0."VatSum" - IFNULL((SELECT                                                             
										SUM("TaxSum")                                                   
									 FROM PCH4                                                          
									 WHERE 1 = 1                                                        
										 AND "DocEntry" = T0."DocEntry"                                 
										 AND "StaCode" NOT IN ('IVA', 'IVA_EXE')), 0), 0)				"Total_Impuesto"
		,ROUND(T0."DocTotal", 0)																		"Total_Afecto"
		,CASE                                                                                           
			WHEN T0."DocSubType" IN ('IE', 'EB') THEN ROUND(T0."DocTotal", 0)                           
			ELSE 0                                                                                      
		 END																							"Total_Exento"
		,IFNULL(LEFT(F0."U_CodImpto", 3), '')															"Codigo_Retencion"
		,IFNULL(T6."Rate", 0)																			"Tasa_Retencion"
		,IFNULL(T6."WTAmnt", 0)																			"Total_Retencion"
		,LEFT(T0."CardName", 100)																		"CardName"
		,LEFT(IFNULL(T12."StreetB", '') || ' ' || IFNULL(T12."StreetNoB", ''), 60)						"StreetB"
		,LEFT(IFNULL(T12."CityB", ''), 15)																"CityB"
		,LEFT(IFNULL(T12."CountyB", ''), 20)															"CountyB"
		,LEFT(IFNULL(C0."E_Mail", ''), 80)																"E_Mail"
		,LEFT(IFNULL(C0."Notes", 'Sin Giro Definido'), 40)												"Giro"
		,CASE WHEN T0."VatSum" > 0 THEN CASE WHEN T0."VatPercent" <> 0 THEN T0."VatPercent" ELSE (SELECT "Rate" FROM "OSTC" WHERE "Code" = 'IVA') END
		      ELSE 0.0
		 END																							"VatPercent"
		,LEFT(IFNULL(T12."StreetS", '') || ' ' || IFNULL(T12."StreetNoS", ''), 60)						"StreetS"
		,LEFT(IFNULL(T12."CountyS", ''), 20)															"CountyS"
		,LEFT(IFNULL(T12."CityS", ''), 15)																"CityS"
		,LEFT(IFNULL(V0."SlpName", ''), 100)															"SlpName"
		,LEFT(IFNULL(C0."Phone1", ''), 30)																"Phone1"
		,IFNULL(T0."U_VK_Patente", '')																	"Patente"
		,T0."DocCur"																					"DocCur"
		,T0."DocRate"																					"DocRate"
		,LEFT(IFNULL(U0."U_NAME", ''), 30)																"Usuario"
		,IFNULL(T0."Comments", '')																		"Comments"
		,IFNULL(ROUND(T0."DiscSum", 0), 0)																"DiscSum"
		,T5."PymntGroup"																				"Condicion_Pago"
		,IFNULL(T0."U_FETimbre", '')																	"XMLTED"
		,T0."ObjType"																					"ObjType"
		,T0."DocEntry"																					"DocEntry"
		,0																								"CodModalidadVenta"
		,0																								"TipoBultos"
		,0																								"CantidadBultos"
		,'0'																							"FormaPagoExp"
		,REPLACE(REPLACE(IFNULL(A0."TaxIdNum",''),'-',''),'.','')										"TaxIdNum"
		,IFNULL(A0."CompnyName",'')																		"RazonSocial"
		,IFNULL(A1."GlblLocNum",'')																		"GiroEmis"
		,'Central'																						"Sucursal"
		,IFNULL(C1."Name",'')																			"Contacto"
		,IFNULL(A0."Phone1",'')																			"TelefenoRecep"
		,CAST(T0."ObjType" || CAST(T0."DocEntry" AS varchar) AS INT)									"COMP"
		,CASE WHEN T0."DocSubType" = 'IX' THEN IFNULL((SELECT GA0."LineTotal"
                                                            FROM "PCH3" GA0
                                                            JOIN "OEXD" EX0 ON EX0."ExpnsCode" = GA0."ExpnsCode"
                                                           WHERE GA0."DocEntry" = T0."DocEntry"
                                                             AND EX0."ExpnsName" = 'Global'), 0.0)	ELSE 0.0 END	"MntGlobal"
	    ,IFNULL(T5."U_FmaPago",'2')																		"FmaPago"
		,''																								"FchPago"
		,0																								"MntPago"
		,''																								"GlosaPagos"
	FROM	  "OPCH"				T0
		 JOIN "PCH12"	 			T12	ON T12."DocEntry"	= T0."DocEntry"
		 JOIN "OCRD"				C0	ON C0."CardCode"	= T0."CardCode"
		 JOIN "OUSR"				U0	ON U0."INTERNAL_K"	= T0."UserSign"
		 JOIN "NNM1"				N1	ON N1."Series"		= T0."Series"
		 			   			   AND N1."ObjectCode"	= T0."ObjType"
	LEFT JOIN "OCTG"				T5	ON T5."GroupNum"	= T0."GroupNum"
	LEFT JOIN "OSLP"				V0	ON V0."SlpCode"		= T0."SlpCode"
	LEFT JOIN "PCH5"				T6	ON T6."AbsEntry"	= T0."DocEntry"
	LEFT JOIN "@VID_FEIMPADIC"		F0	ON F0."Code"		= T6."WTCode"
	LEFT JOIN "OCPR"  C1  ON C1."CntctCode"		= T0."CntctCode"
	,"OADM" A0, "ADM1" A1
	WHERE 1 = 1
		--AND IFNULL(T0."FolioNum", 0) <> 0
		AND UPPER(LEFT(N1."BeginStr", 1)) = 'E'
		
	UNION ALL

	SELECT
		 T0."FolioNum"																					"FolioNum"
		,REPLACE(T0."LicTradNum", '.', '')																"LicTradNum"
		,TO_CHAR(T0."TaxDate",'YYYY-MM-DD')																"DocDate"
		,TO_CHAR(T0."DocDueDate",'YYYY-MM-DD')															"DocDueDate"
		,CASE                                                                                           
			WHEN T0."DocSubType" NOT IN ('IE', 'EB') THEN ROUND((T0."DocTotal" - T0."VatSum"), 0)       
			ELSE 0                                                                                      
		 END																							"DocTotal"
		,ROUND(T0."VatSum" - IFNULL((SELECT                                                             
										SUM("TaxSum")                                                   
									 FROM DPO4                                                          
									 WHERE 1 = 1                                                        
										 AND "DocEntry" = T0."DocEntry"                                 
										 AND "StaCode" NOT IN ('IVA', 'IVA_EXE')), 0), 0)				"Total_Impuesto"
		,CASE
			WHEN T0."DocSubType" NOT IN ('IE', 'EB') THEN ROUND((T0."DocTotal" - T0."VatSum"), 0)
			ELSE 0
		 END																							"Total_Afecto"
		,CASE
			WHEN T0."DocSubType" IN ('IE', 'EB') THEN ROUND(T0."DocTotal", 0)
			ELSE 0
		 END																							"Total_Exento"
		,IFNULL(LEFT(F0."U_CodImpto", 3), '')															"Codigo_Retencion"
		,IFNULL(T6."Rate", 0)																			"Tasa_Retencion"
		,IFNULL(T6."WTAmnt", 0)																			"Total_Retencion"
		,LEFT(T0."CardName", 100)																		"CardName"
		,LEFT(IFNULL(T12."StreetB", '') || ' ' || IFNULL(T12."StreetNoB", ''), 60)						"StreetB"
		,LEFT(IFNULL(T12."CityB", ''), 15)																"CityB"
		,LEFT(IFNULL(T12."CountyB", ''), 20)															"CountyB"
		,LEFT(IFNULL(C0."E_Mail", ''), 80)																"E_Mail"
		,LEFT(IFNULL(C0."Notes", 'Sin Giro Definido'), 40)												"Giro"
		,CASE WHEN T0."VatSum" > 0 THEN CASE WHEN T0."VatPercent" <> 0 THEN T0."VatPercent" ELSE (SELECT "Rate" FROM "OSTC" WHERE "Code" = 'IVA') END
		      ELSE 0.0
		 END																							"VatPercent"
		,LEFT(IFNULL(T12."StreetS", '') || ' ' || IFNULL(T12."StreetNoS", ''), 60)						"StreetS"
		,LEFT(IFNULL(T12."CountyS", ''), 20)															"CountyS"
		,LEFT(IFNULL(T12."CityS", ''), 15)																"CityS"
		,LEFT(IFNULL(V0."SlpName", ''), 100)															"SlpName"
		,LEFT(IFNULL(C0."Phone1", ''), 30)																"Phone1"
		,IFNULL(T0."U_VK_Patente", '')																	"Patente"
		,T0."DocCur"																					"DocCur"
		,T0."DocRate"																					"DocRate"
		,LEFT(IFNULL(U0."U_NAME", ''), 30)																"Usuario"
		,IFNULL(T0."Comments", '')																		"Comments"
		,IFNULL(ROUND(T0."DiscSum", 0), 0)																"DiscSum"
		,T5."PymntGroup"																				"Condicion_Pago"
		,IFNULL(T0."U_FETimbre", '')																	"XMLTED"
		,T0."ObjType"																					"ObjType"
		,T0."DocEntry"																					"DocEntry"
		,0																								"CodModalidadVenta"
		,0																								"TipoBultos"
		,0																								"CantidadBultos"
		,'0'																							"FormaPagoExp"
		,REPLACE(REPLACE(IFNULL(A0."TaxIdNum",''),'-',''),'.','')										"TaxIdNum"
		,IFNULL(A0."CompnyName",'')																		"RazonSocial"
		,IFNULL(A1."GlblLocNum",'')																		"GiroEmis"
		,'Central'																						"Sucursal"
		,IFNULL(C1."Name",'')																			"Contacto"
		,IFNULL(A0."Phone1",'')																			"TelefenoRecep"
		,CAST(T0."ObjType" || CAST(T0."DocEntry" AS varchar) AS INT)									"COMP"
		,CASE WHEN T0."DocSubType" = 'IX' THEN IFNULL((SELECT GA0."LineTotal"
                                                            FROM "DPO3" GA0
                                                            JOIN "OEXD" EX0 ON EX0."ExpnsCode" = GA0."ExpnsCode"
                                                           WHERE GA0."DocEntry" = T0."DocEntry"
                                                             AND EX0."ExpnsName" = 'Global'), 0.0)	ELSE 0.0 END	"MntGlobal"
		,IFNULL(T5."U_FmaPago",'2')																		"FmaPago"
		,''																								"FchPago"
		,0																								"MntPago"
		,''																								"GlosaPagos"
	FROM	  "ODPO"				T0
		 JOIN "DPO12"	 			T12	ON T12."DocEntry"	= T0."DocEntry"
		 JOIN "OCRD"				C0	ON C0."CardCode"	= T0."CardCode"
		 JOIN "OUSR"				U0	ON U0."INTERNAL_K"	= T0."UserSign"
		 JOIN "NNM1"				N1	ON N1."Series"		= T0."Series"
		 						   AND N1."ObjectCode"	= T0."ObjType"
	LEFT JOIN "OCTG"				T5	ON T5."GroupNum"	= T0."GroupNum"
	LEFT JOIN "OSLP"				V0	ON V0."SlpCode"		= T0."SlpCode"
	LEFT JOIN "DPO5"				T6	ON T6."AbsEntry"	= T0."DocEntry"
	LEFT JOIN "@VID_FEIMPADIC"	F0	ON F0."Code"		= T6."WTCode"
	LEFT JOIN "OCPR" 				C1  ON C1."CntctCode"		= T0."CntctCode"
	,"OADM" A0, "ADM1" A1
	WHERE 1 = 1
		--AND IFNULL(T0."FolioNum", 0) <> 0
		AND UPPER(LEFT(N1."BeginStr", 1)) = 'E'
