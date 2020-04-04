IF EXISTS(SELECT name FROM sysobjects                
      WHERE name = 'VID_VW_FE_OINV_R' AND type = 'V')
   DROP VIEW VID_VW_FE_OINV_R
GO--                                                 

CREATE VIEW [dbo].[VID_VW_FE_OINV_R]
AS
	-- Nota de dÃ©bito electrÃ³nica basada en documento fuera de SAP
	SELECT
		 T0.FolioNum												[Folio_Sii]
		,REPLACE(REPLACE(T2.U_TipoDTE, 'b', ''),'a','')				[TpoDocRef]
		,CAST(T2.U_DocFolio	AS VARCHAR)								[FolioRef]
		,REPLACE(CONVERT(CHAR(10),T2.U_DocDate, 102),'.','-')		[FchRef]
		,T1.U_CodRef												[CodRef]
		,ISNULL(T1.U_RazRef, '')									[RazonRef]
		,T0.DocEntry												[DocEntry]
		,T0.ObjType													[ObjType]
		,ISNULL(T1.U_IndGlobal,'0')									[IndGlobal]
	FROM OINV T0
	JOIN [@VID_FEREF] T1 ON T1.U_DocEntry = T0.DocEntry
	                    AND T1.U_DocSBO = T0.ObjType
	JOIN [@VID_FEREFD] T2 ON T2.DocEntry = T1.DocEntry
	JOIN NNM1 N0 ON N0.Series		= T0.Series
	, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND ISNULL(T2.U_DocFolio, '0') <> '0'
	
	UNION

	-- Nota de Debito Electronica con Ind Global 1
	SELECT
		 T0.FolioNum												[Folio_Sii]
		,T1.U_TipoDTE												[TpoDocRef]
		,''															[FolioRef]
		,REPLACE(CONVERT(CHAR(10),T0.DocDate, 102),'.','-')			[FchRef]
		,T1.U_CodRef												[CodRef]
		,ISNULL(T1.U_RazRef, '')									[RazonRef]
		,T0.DocEntry												[DocEntry]
		,T0.ObjType													[ObjType]
		,ISNULL(T1.U_IndGlobal,'0')									[IndGlobal]
	FROM OINV T0
	JOIN [@VID_FEREF] T1 ON T1.U_DocEntry = T0.DocEntry
	                    AND T1.U_DocSBO = T0.ObjType
	JOIN NNM1 N0 ON N0.Series		= T0.Series
	, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND ISNULL(T1.U_IndGlobal, '0') = '1'
		AND (SELECT COUNT(*)
		       FROM [@VID_FEREFD]
			  WHERE DocEntry = T1.DocEntry) = 0

	UNION
	
	-- Factura electrÃ³nica y Factura exenta electrÃ³nica basada en GuÃ­as manuales y electrÃ³nicas
	SELECT
		 T0.FolioNum												[Folio_Sii]
		,CASE SUBSTRING(UPPER(ISNULL(T2.BeginStr, '')), 1, 1)
			WHEN 'E' THEN '52'
			ELSE '50'
		 END														[TpoDocRef]
		,CAST(G0.FolioNum AS VARCHAR)								[FolioRef]
		,REPLACE(CONVERT(CHAR(10),G0.TaxDate, 102),'.','-')			[FchRef]
		,'0'														[CodRef]
		,''															[RazonRef]
		,T0.DocEntry												[DocEntry]
		,T0.ObjType													[ObjType]
		,'0'														[IndGlobal]
	FROM OINV T0
	JOIN INV1 T1 ON T1.DocEntry	= T0.DocEntry
	JOIN ODLN G0 ON G0.DocEntry	= T1.BaseEntry
				AND G0.ObjType	= T1.BaseType
	JOIN NNM1 T2 ON T2.Series	= G0.Series
	, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND ISNULL(G0.FolioNum, 0) <> 0
		AND ISNULL(T1.BaseType, 0) = 15
	GROUP BY
		 T0.FolioNum
		,G0.FolioNum
		,G0.TaxDate
		,T2.BeginStr
		,T0.DocEntry
		,T0.ObjType
		
	UNION
	
	-- Factura electrÃ³nica y Factura exenta electrÃ³nica con Orden de Compra
	SELECT
		 T0.FolioNum												[Folio_Sii]
		,'801'														[TpoDocRef]
		,LEFT(T0.NumAtCard, 18)										[FolioRef]
		,REPLACE(CONVERT(CHAR(10),T0.TaxDate, 102),'.','-')			[FchRef]
		,'0'														[CodRef]
		,''															[RazonRef]
		,T0.DocEntry												[DocEntry]
		,T0.ObjType													[ObjType]
		,'0'														[IndGlobal]
	FROM OINV T0
	JOIN NNM1 N0 ON T0.Series = N0.Series
	, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND ISNULL(T0.NumAtCard, '') <> ''
		AND T0.DocSubType IN ('--', 'IE')
		AND UPPER(LEFT(N0.BeginStr, 1)) = 'E'
	
	UNION
	
	--------------------------------------------------------------------------------------------
	
	-- Anticipo de clientes
	-- Factura electrÃ³nica y Factura exenta electrÃ³nica basada en GuÃ­as manuales y electrÃ³nicas
	SELECT
		 T0.FolioNum												[Folio_Sii]
		,CASE SUBSTRING(UPPER(ISNULL(T2.BeginStr, '')), 1, 1)
			WHEN 'E' THEN '52'
			ELSE '50'
		 END														[TpoDocRef]
		,CAST(G0.FolioNum AS VARCHAR)								[FolioRef]
		,REPLACE(CONVERT(CHAR(10),G0.TaxDate, 102),'.','-')			[FchRef]
		,'0'														[CodRef]
		,''															[RazonRef]
		,T0.DocEntry												[DocEntry]
		,T0.ObjType													[ObjType]
		,'0'														[IndGlobal]
	FROM ODPI T0
	JOIN DPI1 T1 ON T1.DocEntry	= T0.DocEntry
	JOIN ODLN G0 ON G0.DocEntry	= T1.BaseEntry
				AND G0.ObjType	= T1.BaseType
	JOIN NNM1 T2 ON T2.Series	= G0.Series
	, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND ISNULL(G0.FolioNum, 0) <> 0
		AND ISNULL(T1.BaseType, 0) = 15
		AND UPPER(LEFT(T2.BeginStr, 1)) = 'E'
	GROUP BY
		 T0.FolioNum
		,G0.FolioNum
		,G0.TaxDate
		,T2.BeginStr
		,T0.DocEntry
		,T0.ObjType
		
	UNION
	
	-- Factura electrÃ³nica con Orden de Compra
	SELECT
		 T0.FolioNum												[Folio_Sii]
		,'801'														[TpoDocRef]
		,LEFT(T0.NumAtCard, 18)										[FolioRef]
		,REPLACE(CONVERT(CHAR(10),T0.TaxDate, 102),'.','-')			[FchRef]
		,'0'														[CodRef]
		,''															[RazonRef]
		,T0.DocEntry												[DocEntry]
		,T0.ObjType													[ObjType]
		,'0'														[IndGlobal]
	FROM ODPI T0
	JOIN NNM1 N0 ON N0.Series = T0.Series
	, [@VID_FEPARAM] PA0
	WHERE 1 = 1
		AND ((ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_Distrib,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) = 0 AND ISNULL(PA0.U_FPortal,'N') = 'Y') OR (ISNULL(T0.FolioNum, 0) <> 0 AND ISNULL(PA0.U_FPortal,'N') = 'N' AND ISNULL(PA0.U_Distrib,'N') = 'N'))
		AND ISNULL(T0.NumAtCard, '') <> ''
		AND T0.DocSubType = '--'
		AND UPPER(LEFT(N0.BeginStr, 1)) = 'E'
GO
