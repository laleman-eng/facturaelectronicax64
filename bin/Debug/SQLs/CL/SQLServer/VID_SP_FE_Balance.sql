IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_Balance' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_Balance
GO--
--EXEC VID_SP_FE_Diccionario '132'  --''2016-05'
CREATE PROCEDURE [dbo].[VID_SP_FE_Balance]
 @PeriodoD VARCHAR(10),
 @PeriodoH VARCHAR(10)
--WITH ENCRYPTION
AS
BEGIN
	
	SELECT REPLACE(O0.TaxIdNum,'.','') [Identificacion/RutContribuyente]
		  ,(SELECT REPLACE(CONVERT(CHAR(7), F_RefDate,102),'.','-') FROM OFPR WHERE AbsEntry = @PeriodoH) [Identificacion/PeriodoTributario]
		  ,C0.FormatCode	[Cuenta/CodigoCuenta]
		  ,LTRIM(STR(SUM(J1.Debit),18,0)) [Cuenta/Debe]
		  ,LTRIM(STR(SUM(J1.Credit),18,0)) [Cuenta/Haber]
		  ,LTRIM(STR(CASE WHEN SUM(J1.Debit) > SUM(J1.Credit) THEN SUM(J1.Debit) - SUM(J1.Credit) ELSE 0 END,18,0)) [Cuenta/SaldoDeudor]
		  ,LTRIM(STR(CASE WHEN SUM(J1.Credit) > SUM(J1.Debit) THEN SUM(J1.Credit) - SUM(J1.Debit) ELSE 0 END,18,0)) [Cuenta/SaldoAcreedor]
	  FROM OFPR T0
	  JOIN OJDT J0 ON J0.RefDate BETWEEN T0.F_RefDate AND T0.T_RefDate
	  JOIN JDT1 J1 ON J1.TransId = J0.TransId
	  JOIN OACT C0 ON C0.AcctCode = J1.Account
	  , OADM O0
	 WHERE T0.AbsEntry >= @PeriodoD
	   AND T0.AbsEntry <= @PeriodoH
	   AND J0.TransType NOT IN ('-2','-3')
	   AND C0.GroupMask <= 3
	 GROUP BY O0.TaxIdNum
	      --,REPLACE(CONVERT(CHAR(7), T0.F_RefDate,102),'.','-')
		  ,C0.FormatCode
END