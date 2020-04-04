IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_OPCH_R' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_OPCH_R
GO--

CREATE PROCEDURE VID_SP_FE_OPCH_R
(
     @DocEntry			Int
    ,@TipoDoc			VarChar(10)
	,@ObjType			VarChar(10)
)
AS
BEGIN
	SELECT
			ROW_NUMBER() OVER(ORDER BY Folio_Sii)	[NroLinRef] 
			,T0.TpoDocRef						[TpoDocRef]
			,T0.FolioRef						[FolioRef]
			,T0.FchRef							[FchRef]
			,ISNULL(T0.CodRef, '')				[CodRef]
			,ISNULL(T0.RazonRef, '')			[RazonRef]
		
	FROM VID_VW_FE_OPCH_R T0
	WHERE 1 = 1
		AND T0.DocEntry = @DocEntry
		AND T0.ObjType = @ObjType;
END
