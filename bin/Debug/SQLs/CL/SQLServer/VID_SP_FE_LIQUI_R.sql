IF EXISTS(SELECT name FROM sysobjects                
      WHERE name = 'VID_SP_FE_LIQUI_R' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_LIQUI_R
GO--

CREATE  PROCEDURE [dbo].VID_SP_FE_LIQUI_R
(
     @DocEntry			Int
    ,@TipoDoc			VarChar(10)
    ,@ObjType			VarChar(10)
)
AS
BEGIN
	DECLARE @Tabla AS TABLE
		(Folio_Sii Int
		,TipoDte varchar(10)
		,TpoDocRef varchar(10)
		,FolioRef varchar(18)
		,FchRef VARCHAR(10)
		,CodRef VARCHAR(10)
		,RazonRef varchar(90))
	
			INSERT INTO @Tabla
			SELECT
				 T0.FolioNum																		[Folio_Sii]
				,@TipoDoc																			[TipoDte]
				,T1.U_TipoDTELF																		[TpoDocRef]
				,CAST(T1.U_FolioLiqF AS VARCHAR(20))												[FolioRef]
				,REPLACE(CONVERT(CHAR(10), T0.TaxDate, 102),'.','-')								[FchRef]
				,''																					[CodRef]
				,'Referencia Liquidacion'															[RazonRef]
			FROM ORIN T0
			JOIN RIN1 T1 ON T1.DocEntry = T0.DocEntry
			WHERE 1 = 1
				AND ISNULL(T1.U_TipoDTELF, '00') <> '00' 
				AND ISNULL(T1.U_TipoDTELF, '00') <> '99' 
				AND T0.DocEntry = @DocEntry
				AND T0.ObjType  = @ObjType
			UNION
			SELECT
				 T0.FolioNum																		[Folio_Sii]
				,@TipoDoc																			[TipoDte]
				,T1.U_TipoDTELF																		[TpoDocRef]
				,CAST(T1.U_FolioLiqF AS VARCHAR(20))												[FolioRef]
				,REPLACE(CONVERT(CHAR(10), T0.TaxDate, 102),'.','-')								[FchRef]
				,''																					[CodRef]
				,'Referencia Liquidacion'															[RazonRef]
			FROM OINV T0
			JOIN INV1 T1 ON T1.DocEntry = T0.DocEntry
			WHERE 1 = 1
				AND ISNULL(T1.U_TipoDTELF, '00') <> '00' 
				AND ISNULL(T1.U_TipoDTELF, '00') <> '99' 
				AND T0.DocEntry = @DocEntry
				AND T0.ObjType  = @ObjType;
	

	--Select final para mostrar
	SELECT
		 ROW_NUMBER() OVER(ORDER BY Folio_Sii)	[NroLinRef] 
		,[TpoDocRef]							[TpoDocRef]
		,[FolioRef]								[FolioRef]
		,[FchRef]								[FchRef]
		,[CodRef]								[CodRef]		
		,[RazonRef]								[RazonRef]
	FROM @Tabla
	
END
GO
