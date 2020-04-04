IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_Boleta' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_Boleta
GO--
--EXEC VID_SP_FE_Boleta 260, '39', '13'
CREATE PROCEDURE [dbo].[VID_SP_FE_Boleta]
 @DocEntry Int ,@TipoDoc  Varchar(10), @ObjType VarChar(10)
--WITH ENCRYPTION
AS
BEGIN

		SELECT @TipoDoc					[Encabezado/IdDoc/TipoDTE]
		      ,T0.FolioNum				[Encabezado/IdDoc/Folio]
			  ,T0.DocDateWS				[Encabezado/IdDoc/FchEmis]
			  ,T0.IndServicio			[Encabezado/IdDoc/IndServicio]
			  ,T0.DocDueDateWS			[Encabezado/IdDoc/FchVenc]
			   
		      ,T3.U_RUT					[Encabezado/Emisor/RUTEmisor]
			  ,T3.U_Sociedad			[Encabezado/Emisor/RznSocEmisor]
			  ,'Educación'				[Encabezado/Emisor/GiroEmisor]
			  ,CASE T3.U_RUT
				 WHEN '78205980-7' THEN ''
				 WHEN '78205970-K' THEN ''
				 WHEN '71652000-5' THEN ''
				 ELSE ''
			   END						[Encabezado/Emisor/DirOrigen]
			  ,CASE T3.U_RUT
				 WHEN '78205980-7' THEN 'Santiago'
				 WHEN '78205970-K' THEN 'Santiago'
				 WHEN '71652000-5' THEN 'Santiago'
				 ELSE ''
			   END						[Encabezado/Emisor/CmnaOrigen]
			  ,CASE T3.U_RUT
				 WHEN '78205980-7' THEN 'Santiago'
				 WHEN '78205970-K' THEN 'Santiago'
				 WHEN '71652000-5' THEN 'Santiago'
				 ELSE ''
			   END						[Encabezado/Emisor/CiudadOrigen]

			  ,T0.LicTradNum			[Encabezado/Receptor/RUTRecep]
			  ,T0.CardName				[Encabezado/Receptor/RznSocRecep]
			  ,T0.StreetB				[Encabezado/Receptor/DirRecep]
			  ,T0.CountyB				[Encabezado/Receptor/CmnaRecep]
			  ,T0.CityB					[Encabezado/Receptor/CiudadRecep]

			  ,ROUND(T0.DocTotal - T0.Total_Impuesto - T0.DiscSum, 0) [Encabezado/Totales/MntNeto]
			  ,T0.Total_Impuesto		[Encabezado/Totales/IVA]
			  ,T0.DocTotal				[Encabezado/Totales/MntTotal]
			  --,T0.DocTotal				[Encabezado/Totales/TotalPeriodo]
			  --,T0.DocTotal				[Encabezado/Totales/VlrPagar]

			  ,(SELECT CAST(ROW_NUMBER() OVER(ORDER BY T0.LineaOrden, T0.LineaOrden2) AS INT)	[NroLinDet]
					  ,T0.ItemCode			[VlrCodigo]
					  ,T0.Dscription		[NmbItem]
					  ,T0.Dscription_Larga	[DscItem]
			          ,T0.Quantity			[QtyItem]
					  ,T0.Price				[PrcItem]
					  ,T0.DiscPrcnt			[DescuentoPct]
					  ,T0.DiscSum			[DescuentoMonto]
					  ,T0.LineTotal			[MontoItem]
				 FROM VID_VW_FE_OINV_D T0
				WHERE T0.DocEntry = @DocEntry
				  AND T0.ObjType = @ObjType
				ORDER BY  T0.[LineaOrden], T0.[LineaOrden2]
				FOR XML PATH('Detalle'),TYPE)
			  ,(SELECT 1						[NroLinDR]
					  ,'D'						[TpoMov]
					  ,'Descuento Encabezado'	[GlosaDR]
					  ,'$'						[TpoValor]
					  ,X0.DiscSum				[ValorDR]
					  ,1						[IndExeDR]
				 FROM VID_VW_FE_OINV_E X0
				WHERE X0.DocEntry = @DocEntry
				  AND X0.ObjType = @ObjType
				  AND X0.DiscSum > 0
				FOR XML PATH('DscRcgGlobal'),TYPE)
			  ,ISNULL(T0.E_Mail,'')		[Anexo/Email]
			  --,1						[DscRcgGlobal/NroLinDR]
			  --,'D'						[DscRcgGlobal/TpoMov]
			  --,'Descuento Encabezado'	[DscRcgGlobal/GlosaDR]
			  --,'$'						[DscRcgGlobal/TpoValor]
			  --,T0.DiscSum				[DscRcgGlobal/ValorDR]
			  --,1						[DscRcgGlobal/IndExeDR]
		  FROM VID_VW_FE_OINV_E T0
		  JOIN NNM1 T2 ON T2.Series = T0.Series
		  JOIN [@VID_FEMULTISOC] T3 ON T3.DocEntry =  SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr))
		      ,OADM A0, ADM1 A1
		 WHERE T0.DocEntry = @DocEntry
		   AND T0.ObjType = @ObjType
		FOR XML path ('Documento')
		select * from VID_VW_FE_OINV_E
		
		select T2.Series, T0.U_RUT,T0.DocEntry, T2.ObjectCode, T2.DocSubType, T0.U_Sociedad
  from [@VID_FEMULTISOC] T0 
  JOIN NNM1 T2 ON T0.DocEntry =  SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr))

END