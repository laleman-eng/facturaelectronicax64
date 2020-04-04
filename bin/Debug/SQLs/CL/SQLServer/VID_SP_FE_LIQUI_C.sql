IF EXISTS(SELECT name FROM sysobjects                
      WHERE name = 'VID_SP_FE_LIQUI_C' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_LIQUI_C
GO--

CREATE  PROCEDURE [dbo].VID_SP_FE_LIQUI_C
(
     @DocEntry			Int
    ,@TipoDoc			VarChar(10)
    ,@ObjType			VarChar(10)
)
AS
BEGIN
	DECLARE @Tabla AS TABLE
		(TipoMovim varchar(10)
		,Glosa varchar(60)
		,ValComNeto Decimal(18,0)
		,ValComExe Decimal(18,0)
		,ValComIVA Decimal(18,0)
		,ExpnsCode Int)
		
		
		INSERT INTO @Tabla
		SELECT
			 CASE WHEN C1.ExpnsName = 'LIF Comisiones' THEN 'C'
			      ELSE 'O' END																	[TipoMovim]
			,LEFT(ISNULL(T3.Comments, C1."ExpnsName"),60)										[Glosa]
			,CASE WHEN T3.TaxCode = 'IVA' THEN T3.LineTotal * -1
			      ELSE 0.0 END																	[ValComNeto]
			,CASE WHEN T3.TaxCode = 'IVA_EXE' THEN T3.LineTotal * -1
			      ELSE 0.0 END																	[ValComExe]
			,T3.VatSum	*-1																		[ValComIVA]
			,T3.ExpnsCode
		FROM ORIN T0
		JOIN RIN3 T3 ON T3.DocEntry = T0.DocEntry
		JOIN OEXD C1 ON C1.ExpnsCode = T3.ExpnsCode
		WHERE 1 = 1
			AND T0.DocEntry = @DocEntry
			AND T0.ObjType  = @ObjType
		UNION
		SELECT
			 CASE WHEN C1.ExpnsName = 'LIF Comisiones' THEN 'C'
			      ELSE 'O' END																	[TipoMovim]
			,LEFT(ISNULL(T3.Comments, C1."ExpnsName"),60)										[Glosa]
			,CASE WHEN T3.TaxCode = 'IVA' THEN T3.LineTotal
			      ELSE 0.0 END																	[ValComNeto]
			,CASE WHEN T3.TaxCode = 'IVA_EXE' THEN T3.LineTotal
			      ELSE 0.0 END																	[ValComExe]
			,T3.VatSum																			[ValComIVA]
			,T3.ExpnsCode
		FROM OINV T0
		JOIN INV3 T3 ON T3.DocEntry = T0.DocEntry
		JOIN OEXD C1 ON C1.ExpnsCode = T3.ExpnsCode
		WHERE 1 = 1
			AND T0.DocEntry = @DocEntry
			AND T0.ObjType  = @ObjType;

	--Select final para mostrar
	SELECT
		 ROW_NUMBER() OVER(ORDER BY ExpnsCode)	[NroLinCom]
		,TipoMovim								[TipoMovim]
		,Glosa									[Glosa]
		,ValComNeto								[ValComNeto]
		,ValComExe								[ValComExe]
		,ValComIVA								[ValComIVA]
	FROM @Tabla
	
END
GO