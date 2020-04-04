IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_Diccionario' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_Diccionario
GO--
--EXEC VID_SP_FE_Diccionario '132'  --''2016-05'
CREATE PROCEDURE [dbo].[VID_SP_FE_Diccionario]
 @Periodo VARCHAR(10)
--WITH ENCRYPTION
AS
BEGIN

	SELECT REPLACE(O0.TaxIdNum,'.','') [Identificacion/RutContribuyente]
		  ,REPLACE(CONVERT(CHAR(7), F0.F_RefDate,102),'.','-') [Identificacion/PeriodoTributario]
		  ,T0.U_Clasif	[Cuenta/ClasificacionCuenta]
		  ,T1.U_CtaSAP	[Cuenta/CodigoCuenta]
		  ,T1.U_DescSAP	[Cuenta/Glosa]
		  ,REPLACE(T0.U_Cuenta,'.','')	[Cuenta/CodigoSII]
		  ,F0.Category
	  FROM [@VID_FEPLANCTA] T0
	  JOIN [@VID_FEPLANCTAD] T1 ON T1.DocEntry = T0.DocEntry
	  ,OFPR F0
	  ,OADM O0
	 WHERE ISNULL(T1.U_CtaSAP,'') <> ''
	   AND F0.AbsEntry = @Periodo

END