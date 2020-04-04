IF EXISTS(SELECT name FROM sysobjects                
      WHERE name = 'VID_SP_FE_52_R' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_52_R
GO--

CREATE PROCEDURE [dbo].[VID_SP_FE_52_R]
	 @DocEntry			Int
	,@TipoDoc			VarChar(10)
	,@ObjType			VarChar(10)
AS
BEGIN

DECLARE @Tabla AS TABLE
		(Folio_Sii Int
		,TipoDte varchar(10)
		,TpoDocRef varchar(10)
		,FolioRef varchar(18)
		,FchRef datetime
		,CodRef int
		,RazonRef varchar(90))
		
		BEGIN
		INSERT INTO @Tabla
		SELECT
			 T0.Folio_Sii						[Folio_Sii]
			,@TipoDoc							[TipoDte]
			,T0.TpoDocRef						[TpoDocRef]
			,T0.FolioRef						[FolioRef]
			,T0.FchRef							[FchRef]
			,ISNULL(T0.CodRef, '')				[CodRef]
			,ISNULL(T0.RazonRef, '')			[RazonRef]
		FROM VID_VW_FE_52_R T0 
		WHERE 1 = 1
			AND T0.DocEntry = @DocEntry
			AND T0.ObjType = @ObjType;

		INSERT INTO @Tabla
		SELECT
			 T0.Folio_Sii						[Folio_Sii]
			,@TipoDoc							[TipoDte]
			,T0.TpoDocRef						[TpoDocRef]
			,T0.FolioRef						[FolioRef]
			,T0.FchRef							[FchRef]
			,ISNULL(T0.CodRef, '')				[CodRef]
			,ISNULL(T0.RazonRef, '')			[RazonRef]
		FROM VID_VW_FE_52_R_EXTRA T0 
		WHERE 1 = 1
			AND T0.DocEntry = @DocEntry
			AND T0.ObjType = @ObjType;

		
		
	END

	--Select final para mostrar
	SELECT
		 ROW_NUMBER() OVER(ORDER BY Folio_Sii)	[NroLinRef] 
		,[TpoDocRef]							[TpoDocRef]
		,[FolioRef]								[FolioRef]
		,[FchRef]								[FchRef]
		,[CodRef]								[CodRef]		
		,[RazonRef]								[RazonRef]
	FROM @Tabla
END;
GO
