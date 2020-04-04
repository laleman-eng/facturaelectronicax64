--DROP VIEW VID_VW_FE_NotaCredito_E;
CREATE VIEW VID_VW_FE_NotaCredito_E
AS
	SELECT
		 T0."FolioNum"																														"FolioNum"
		,CASE WHEN LTRIM(RTRIM(RIGHT(N1."BeginStr", 3))) = '112' THEN '55555555-5'
															   ELSE REPLACE(T0."LicTradNum", '.', '')
		 END																																"LicTradNum"
		,TO_VARCHAR(T0."TaxDate", 'yyyy-MM-dd')																								"DocDate"
		,TO_VARCHAR(T0."DocDueDate", 'yyyy-MM-dd')																							"DocDueDate"
		,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN CASE WHEN T0."CurSource" = 'L' THEN TO_VARCHAR(ROUND(T0."DocTotal",4))
														   WHEN T0."CurSource" = 'S' THEN TO_VARCHAR(ROUND(T0."DocTotalSy",4))
														   WHEN T0."CurSource" = 'C' THEN TO_VARCHAR(ROUND(T0."DocTotalFC",4))
																			  END
			  ELSE ROUND(T0."DocTotal", 0)
		 END																																"DocTotal"
		,ROUND(CASE WHEN RIGHT(N1."BeginStr", 3) =  '112' THEN 0
														ELSE  IFNULL((SELECT SUM("TaxSum")
																		FROM "RIN4"
																	   WHERE 1 = 1
																	     AND "ExpnsCode" = -1
																		 AND "DocEntry" = T0."DocEntry"
																		 AND "StaCode"  = 'IVA'), 0.0)
			   END, 0)																														"Total_Impuesto"
		,CASE WHEN RIGHT(N1."BeginStr", 3) =  '112' THEN 0.0
												  ELSE IFNULL((SELECT SUM(ROUND("LineTotal",0))
																		FROM "RIN1"
																	   WHERE 1 = 1
																		 AND "DocEntry" = T0."DocEntry"
																		 AND "TaxCode" = 'IVA'), 0.0)

		 END																																"Total_Afecto"
		,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN TO_VARCHAR(ROUND(IFNULL((SELECT SUM(CASE WHEN ORIN."CurSource" = 'L' THEN "BaseSum"
																						WHEN ORIN."CurSource" = 'S' THEN "BaseSumSys"
																						WHEN ORIN."CurSource" = 'C' THEN "BaseSumFrg"
																				   END)
																		  FROM "ORIN"
																		  JOIN "RIN4" ON ORIN."DocEntry" = RIN4."DocEntry"
																		 WHERE 1 = 1
																		   AND ORIN."DocEntry" = T0."DocEntry"
																		   AND RIN4."StaCode" IN ('IVA_EXE')), 0),4))	
												 ELSE ROUND(IFNULL((SELECT SUM("BaseSum")
																	  FROM "RIN4"
																	 WHERE 1 = 1
																	   AND "DocEntry" = T0."DocEntry"
																	   AND "StaCode" IN ('IVA_EXE')), 0),0)
		 END																																"Total_Exento"
		,''																																	"Codigo_Retencion"
		,0																																	"Tasa_Retencion"
		,0																																	"Total_Retencion"
		,VID_FN_FE_LimpiaCaracteres(LEFT(T0."CardName", 100))																				"CardName"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(T12."StreetB", '') || ' ' || IFNULL(T12."StreetNoB", ''), 60))								"StreetB"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(T12."CityB", ''), 15))																		"CityB"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(T12."CountyB", ''), 20))																	"CountyB"
		,LEFT(IFNULL(C0."E_Mail", ''), 80)																									"E_Mail"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(CAST(C0."Notes" AS VARCHAR(40)), 'Sin Giro Definido'), 40))									"Giro"
		,CASE WHEN T0."VatSum" > 0 THEN CASE WHEN T0."VatPercent" <> 0 THEN T0."VatPercent" ELSE (SELECT "Rate" FROM "OSTC" WHERE "Code" = 'IVA') END
		      ELSE 0.0
		 END																																"VatPercent"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(T12."StreetS", '') || ' ' || IFNULL(T12."StreetNoS", ''), 60))								"StreetS"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(T12."CountyS", ''), 20))																	"CountyS"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(T12."CityS", ''), 15))																		"CityS"
		,LEFT(IFNULL(V0."SlpName", ''), 100)																								"SlpName"
		,LEFT(IFNULL(C0."Phone1", ''), 30)																									"Phone1"
		,IFNULL(T0."U_VK_Patente", '')																										"Patente"
		,CASE RIGHT(N1."BeginStr", 3)
			WHEN '112' THEN CASE T6."ISOCurrCod"
								WHEN 'VEB' THEN 'BOLIVAR'
								WHEN 'BOB' THEN 'BOLIVIANO'
								WHEN 'DKK' THEN 'CORONA_DIN'
								WHEN 'NOK' THEN 'CORONA_NOR'
								WHEN 'SEK' THEN 'CORONA_SC'
								WHEN 'BRL' THEN 'CRUZEIRO_REAL'
								WHEN 'AED' THEN 'DIRHAM'
								WHEN 'AUD' THEN 'DOLAR_AUST'
								WHEN 'CAD' THEN 'DOLAR_CAN'
								WHEN 'HKD' THEN 'DOLAR_HK'
								WHEN 'NZD' THEN 'DOLAR_NZ'
								WHEN 'SGD' THEN 'DOLAR_SIN'
								WHEN 'TWD' THEN 'DOLAR_TAI'
								WHEN 'USD' THEN 'DOLAR_USA'
								WHEN 'EUR' THEN 'EURO'
								WHEN 'PYG' THEN 'GUARANI'
								WHEN 'GBP' THEN 'LIBRA_EST'
								WHEN 'PEN' THEN 'NUEVO_SOL'
								WHEN 'ARS' THEN 'PESO'
								WHEN 'CLP' THEN 'PESO_CL'
								WHEN 'COP' THEN 'PESO_COL'
								WHEN 'MXN' THEN 'PESO_MEX'
								WHEN 'UYU' THEN 'PESO_URUG'
								WHEN 'ZAR' THEN 'RAND'
								WHEN 'CNY' THEN 'RENMINBI'
								WHEN 'INR' THEN 'RUPIA'
								WHEN 'JPY' THEN 'YEN'
								ELSE 'DOLAR_USA'
							END
			ELSE T0."DocCur"
		 END																																"DocCur"
		,CASE WHEN RIGHT(N1."BeginStr",3) = '112' THEN CASE T0."CurSource" 
															WHEN 'S' THEN (SELECT ROUND(ORTT."Rate",4) 
																			 FROM ORTT 
																			WHERE ORTT."Currency" = (SELECT "SysCurrncy" 
																									   FROM "OADM") 
																			  AND ORTT."RateDate" = T0."TaxDate") 
															WHEN 'L' THEN 0.0 
															WHEN 'C' THEN ROUND(T0."DocRate",4) 
															ELSE 0.0 
														END 
			  ELSE ROUND(T0."DocRate",4) 
		 END																																"DocRate"
		,LEFT(IFNULL(U0."U_NAME", ''), 30)																									"Usuario"
		,IFNULL(T0."Comments", '')																											"Comments"
		,IFNULL(ROUND(T0."DiscSum", 0), 0)																									"DiscSum"
		,T5."PymntGroup"																													"Condicion_Pago"
		,IFNULL(T0."U_FETimbre", '')																										"XMLTED"
		,T0."ObjType"																														"ObjType"
		,T0."DocEntry"																														"DocEntry"
		,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN TO_VARCHAR(IFNULL(T0."U_CodModVenta", 0)) ELSE '0' END								"CodModVenta"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN TO_VARCHAR(IFNULL(T0."U_CodClauVenta", 0)) ELSE '0' END								"CodClauVenta"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_TotClauVenta", 0)	  ELSE 0 END										"TotClauVenta"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN TO_VARCHAR(IFNULL(T0."U_CodViaTransp", 0)) ELSE '0' END								"CodViaTransp"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_NombreTransp", '')	  ELSE '' END										"NombreTransp"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_RUTCiaTransp", '')	  ELSE '' END										"RUTCiaTransp"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_NomCiaTransp", '')	  ELSE '' END										"NomCiaTransp"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_IdAdicTransp", '')	  ELSE '' END										"IdAdicTransp"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_Booking", '')		  ELSE '' END										"Booking"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_Operador", '')		  ELSE '' END										"Operador"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN TO_VARCHAR(IFNULL(T0."U_CodPtoEmbarque", 0)) ELSE '0' END							"CodPtoEmbarque"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_IdAdicPtoEmb", '')	  ELSE '' END										"IdAdicPtoEmb"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN TO_VARCHAR(IFNULL(T0."U_CodPtoDesemb", 0)) ELSE '0' END								"CodPtoDesemb"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_IdAdicPtoDesemb", '') ELSE '' END										"IdAdicPtoDesemb"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_Tara", 0)			  ELSE 0 END										"Tara"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN TO_VARCHAR(IFNULL(T0."U_CodUnidMedTara", 0)) ELSE '0' END							"CodUnidMedTara"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_PesoBruto", 0)		  ELSE 0 END										"PesoBruto"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN TO_VARCHAR(IFNULL(T0."U_CodUnidPesoBr", 0)) ELSE '0' END							"CodUnidPesoBruto"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_PesoNeto", 0)		  ELSE 0 END										"PesoNeto"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN TO_VARCHAR(IFNULL(T0."U_CodUnidPesNet", 0)) ELSE '0' END							"CodUnidPesoNeto"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_TotItems", 0)		  ELSE 0 END										"TotItems"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN TO_INT(IFNULL(T0."U_TotBultos", 0))	  ELSE 0 END									"TotBultos"
		,CASE WHEN RIGHT(N1."BeginStr", 3) = '112'
					THEN CASE WHEN A0."MainCurncy" IN ('CLP','$') THEN T0."DocTotal" ELSE T0."DocTotalFC" END
					ELSE 0.0 
		 END																																"OtraMoneda"
		
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL((SELECT GA0."LineTotal"
                                                       FROM "RIN3" GA0
                                                       JOIN "OEXD" EX0 ON EX0."ExpnsCode" = GA0."ExpnsCode"
                                                      WHERE GA0."DocEntry" = T0."DocEntry"
                                                        AND EX0."ExpnsName" = 'Flete'), 0.0)	ELSE 0.0 END								"MntFlete"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL((SELECT GA0."LineTotal"
                                                       FROM "RIN3" GA0
                                                       JOIN "OEXD" EX0 ON EX0."ExpnsCode" = GA0."ExpnsCode"
                                                      WHERE GA0."DocEntry" = T0."DocEntry"
                                                        AND EX0."ExpnsName" = 'Seguro'), 0.0)	ELSE 0.0 END								"MntSeguro"
		,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL((SELECT GA0."LineTotal"
                                                       FROM "RIN3" GA0
                                                       JOIN "OEXD" EX0 ON EX0."ExpnsCode" = GA0."ExpnsCode"
                                                      WHERE GA0."DocEntry" = T0."DocEntry"
                                                        AND EX0."ExpnsName" = 'Global'), 0.0)	ELSE 0.0 END								"MntGlobal"
		
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_CodPaisRecep", '')	  ELSE '' END										"CodPaisRecep"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_CodPaisDestin", '')	  ELSE '' END									"CodPaisDestin"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_TipoMoneda", '')	  ELSE '' END										"TipoMoneda"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN TO_VARCHAR(IFNULL(T0."U_FmaPagExp", 0)) ELSE '0' END								"FmaPagExp"
        ,CASE WHEN RIGHT(N1."BeginStr", 3) = '112' THEN IFNULL(T0."U_TipoBultos", 0)		  ELSE 0 END									"TipoBultos"
		,CAST(T0."ObjType" || CAST(T0."DocEntry" AS varchar) AS INT )																		"COMP"
		,IFNULL(A0."Phone1",'')																												"TelefenoRecep"
		,REPLACE(REPLACE(IFNULL(A0."TaxIdNum",''),'-',''),'.','')																			"TaxIdNum"
		,IFNULL(A0."CompnyName",'')																											"RazonSocial"
		,IFNULL(A1."GlblLocNum",'')																											"GiroEmis"
		,'Central'																															"Sucursal"
		,IFNULL(C1."Name",'')																												"Contacto"
		,IFNULL((SELECT SUM(CASE WHEN A3."TaxCode" = 'IVA' THEN A3."LineTotal" * -1
							 ELSE 0.0 END)
				   FROM "ORIN" A0
				   JOIN "RIN3" A3 ON A3."DocEntry" = A0."DocEntry"
				   JOIN "OEXD" A1 ON A1."ExpnsCode" = A3."ExpnsCode"
		           WHERE 1 = 1
					AND A0."DocEntry" = T0."DocEntry"
					AND A0."ObjType"  = T0."ObjType"),0.0)																					"ValComNeto"
		,IFNULL((SELECT SUM(CASE WHEN A3."TaxCode" = 'IVA_EXE' THEN A3."LineTotal" * -1
							ELSE 0.0 END)										
				  FROM "ORIN" A0
				  JOIN "RIN3" A3 ON A3."DocEntry" = A0."DocEntry"
				  JOIN "OEXD" A1 ON A1."ExpnsCode" = A3."ExpnsCode"
				 WHERE 1 = 1
					AND A0."DocEntry" = T0."DocEntry"
					AND A0."ObjType"  = T0."ObjType"),0.0)																					"ValComExe"
		,IFNULL((SELECT SUM(A3."VatSum"	*-1)
				   FROM "ORIN" A0
				   JOIN "RIN3" A3 ON A3."DocEntry" = A0."DocEntry"
				   JOIN "OEXD" A1 ON A1."ExpnsCode" = A3."ExpnsCode"
				  WHERE 1 = 1
					AND A0."DocEntry" = T0."DocEntry"
					AND A0."ObjType"  = T0."ObjType"),0.0)																					"ValComIVA"
		,IFNULL(T0."U_TpoTranCpra",'')																										"TpoTranCompra"
		,IFNULL(T0."U_TpoTranVta",'')																										"TpoTranVenta"
		,IFNULL(CAST(T0."U_CdgSiiSuc" AS VARCHAR(9)),'0')																					"CdgSIISucur"
		,IFNULL(T0."U_FESucursal",'')																										"SucursalAF"
		,'PESOS_CL'																															"TpoMoneda"--Para Otra Moneda
		,IFNULL(T5."U_FmaPago",'2')																											"FmaPago"
		,TO_VARCHAR(T0."DocDueDate", 'yyyy-MM-dd')																							"FchPago"
		,T0."DocTotal"																														"MntPago"
		,T5."PymntGroup"																													"GlosaPagos"
	FROM	  "ORIN"	T0
		 JOIN "RIN12" T12	ON T12."DocEntry"	= T0."DocEntry"
		 JOIN "OCRD"	C0	ON C0."CardCode"	= T0."CardCode"
		 JOIN "OUSR"	U0	ON U0."INTERNAL_K"	= T0."UserSign"
		 JOIN "NNM1"	N1	ON N1."Series"		= T0."Series"
		 			   AND N1."ObjectCode"		= T0."ObjType"
	LEFT JOIN "OCTG"	T5	ON T5."GroupNum"	= T0."GroupNum"
	LEFT JOIN "OSLP"	V0	ON V0."SlpCode"		= T0."SlpCode"
	LEFT JOIN "OCRN"	T6	ON T6."CurrCode"	= T0."DocCur"
	LEFT JOIN "OCPR"  	C1  ON C1."CntctCode"	= T0."CntctCode"
	,"OADM" A0, "ADM1" A1
	WHERE 1 = 1
--		AND IFNULL(T0."FolioNum", 0) <> 0
		AND UPPER(LEFT(N1."BeginStr", 1)) = 'E'
		
	UNION ALL

	SELECT
		 T0."FolioNum"																														"FolioNum"
		,REPLACE(T0."LicTradNum", '.', '')																									"LicTradNum"
		,TO_VARCHAR(T0."TaxDate", 'yyyy-MM-dd')																								"DocDate"
		,TO_VARCHAR(T0."DocDueDate", 'yyyy-MM-dd')																							"DocDueDate"
		,ROUND(T0."DocTotal", 0)																											"DocTotal"
		,ROUND(T0."VatSum" - IFNULL((SELECT SUM("TaxSum")
								     FROM "RPC4"
								    WHERE 1 = 1
									  AND "ExpnsCode" = -1
									  AND "DocEntry" = T0."DocEntry"
									  AND "StaCode" NOT IN ('IVA', 'IVA_EXE')), 0.0), 0.0)													"Total_Impuesto"
		,ROUND((T0."DocTotal" - T0."VatSum"), 0.0)																							"Total_Afecto"
		,ROUND(IFNULL((SELECT SUM("BaseSum")
						 FROM "RPC4"
						WHERE 1 = 1
						  AND "DocEntry" = T0."DocEntry"
						  AND "StaCode" IN ('IVA_Exe')), 0),0)																				"Total_Exento"
		,IFNULL(LEFT(F0."U_CodImpto", 3), '')																								"Codigo_Retencion"
		,IFNULL(T6."Rate", 0)																												"Tasa_Retencion"
		,IFNULL(T6."WTAmnt", 0)																												"Total_Retencion"
		,VID_FN_FE_LimpiaCaracteres(LEFT(T0."CardName", 100))																				"CardName"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(T12."StreetB", '') || ' ' || IFNULL(T12."StreetNoB", ''), 60))								"StreetB"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(T12."CityB", ''), 15))																		"CityB"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(T12."CountyB", ''), 20))																	"CountyB"
		,LEFT(IFNULL(C0."E_Mail", ''), 80)																									"E_Mail"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(CAST(C0."Notes" AS VARCHAR(40)), 'Sin Giro Definido'), 40))									"Giro"
		,CASE WHEN T0."VatSum" > 0 THEN CASE WHEN T0."VatPercent" <> 0 THEN T0."VatPercent" ELSE (SELECT "Rate" FROM "OSTC" WHERE "Code" = 'IVA') END
		      ELSE 0.0
		 END																																"VatPercent"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(T12."StreetS", '') || ' ' || IFNULL(T12."StreetNoS", ''), 60))								"StreetS"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(T12."CountyS", ''), 20))																	"CountyS"
		,VID_FN_FE_LimpiaCaracteres(LEFT(IFNULL(T12."CityS", ''), 15))																		"CityS"
		,LEFT(IFNULL(V0."SlpName", ''), 100)																								"SlpName"
		,LEFT(IFNULL(C0."Phone1", ''), 30)																									"Phone1"
		,IFNULL(T0."U_VK_Patente", '')																										"Patente"
		,T0."DocCur"																														"DocCur"
		,ROUND(T0."DocRate", 4)																												"DocRate"
		,LEFT(IFNULL(U0."U_NAME", ''), 30)																									"Usuario"
		,IFNULL(T0."Comments", '')																											"Comments"
		,IFNULL(ROUND(T0."DiscSum", 0), 0)																									"DiscSum"
		,T5."PymntGroup"																													"Condicion_Pago"
		,IFNULL(T0."U_FETimbre", '')																										"XMLTED"
		,T0."ObjType"																														"ObjType"
		,T0."DocEntry"																														"DocEntry"
		,'0'																																"CodModVenta"
        ,'0'																																"CodClauVenta"
        ,0																																	"TotClauVenta"
        ,'0'																																"CodViaTransp"
        ,''																																	"NombreTransp"
        ,''																																	"RUTCiaTransp"
        ,''																																	"NomCiaTransp"
        ,''																																	"IdAdicTransp"
        ,''																																	"Booking"
        ,''																																	"Operador"
        ,'0'																																"CodPtoEmbarque"
        ,''																																	"IdAdicPtoEmb"
        ,'0'																																"CodPtoDesemb"
        ,''																																	"IdAdicPtoDesemb"
        ,0																																	"Tara"
        ,'0'																																"CodUnidMedTara"
        ,0																																	"PesoBruto"
        ,'0'																																"CodUnidPesoBruto"
        ,0																																	"PesoNeto"
        ,'0'																																"CodUnidPesoNeto"
        ,0																																	"TotItems"
        ,0																																	"TotBultos"
		,0.0																																"OtraMoneda"
        ,0																																	"MntFlete"
        ,0																																	"MntSeguro"
        ,0																																	"MntGlobal"
        ,''																																	"CodPaisRecep"
        ,''																																	"CodPaisDestin"
        ,''																																	"TipoMoneda"
        ,'0'																																"FmaPagExp"
        ,0																																	"TipoBultos"
		,CAST(T0."ObjType" || CAST(T0."DocEntry" AS varchar) AS INT )																		"COMP"
		,IFNULL(A0."Phone1",'')																												"TelefenoRecep"
		,REPLACE(REPLACE(IFNULL(A0."TaxIdNum",''),'-',''),'.','')																			"TaxIdNum"
		,IFNULL(A0."CompnyName",'')																											"RazonSocial"
		,IFNULL(A1."GlblLocNum",'')																											"GiroEmis"
		,'Central'																															"Sucursal"
		,IFNULL(C1."Name",'')																												"Contacto"
		,0.0																																"ValComNeto"
		,0.0																																"ValComExe"
		,0.0																																"ValComIVA"
		,IFNULL(T0."U_TpoTranCpra",'')																										"TpoTranCompra"
		,IFNULL(T0."U_TpoTranVta",'')																										"TpoTranVenta"
		,IFNULL(CAST(T0."U_CdgSiiSuc" AS VARCHAR(9)),'0')																					"CdgSIISucur"
		,IFNULL(T0."U_FESucursal",'')																										"SucursalAF"
		,'PESOS_CL'																															"TpoMoneda"--Para Otra Moneda
		,IFNULL(T5."U_FmaPago",'2')																											"FmaPago"
		,''																																	"FchPago"
		,0																																	"MntPago"
		,''																																	"GlosaPagos"
	FROM	  "ORPC"				T0
		 JOIN "RPC12"				T12	ON T12."DocEntry"	= T0."DocEntry"
		 JOIN "OCRD"				C0	ON C0."CardCode"	= T0."CardCode"
		 JOIN "OUSR"				U0	ON U0."INTERNAL_K"	= T0."UserSign"
		 JOIN "NNM1"				N1	ON N1."Series"		= T0."Series"
								   AND N1."ObjectCode"		= T0."ObjType"
	LEFT JOIN "OCTG"				T5	ON T5."GroupNum"	= T0."GroupNum"
	LEFT JOIN "OSLP"				V0	ON V0."SlpCode"		= T0."SlpCode"
	LEFT JOIN "RPC5"				T6	ON T6."AbsEntry"	= T0."DocEntry"
	LEFT JOIN "@VID_FEIMPADIC"	F0	ON F0."Code"			= T6."WTCode"
	LEFT JOIN "OCPR"  				C1  ON C1."CntctCode"	= T0."CntctCode"
	,"OADM" A0, "ADM1" A1
	WHERE 1 = 1
--		AND IFNULL(T0."FolioNum", 0) <> 0
		AND UPPER(LEFT(N1."BeginStr", 1)) = 'E';