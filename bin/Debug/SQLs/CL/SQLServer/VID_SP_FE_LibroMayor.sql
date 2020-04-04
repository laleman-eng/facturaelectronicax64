IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_LibroMayor' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_LibroMayor
GO--
--EXEC VID_SP_FE_LibroMayor '132'  --'2016-12'
CREATE PROCEDURE [dbo].[VID_SP_FE_LibroMayor]
 @Periodo VARCHAR(10)
--WITH ENCRYPTION
AS
BEGIN
	SELECT REPLACE(A0.TaxIdNum,'.','') [Identificacion/RutContribuyente]
		  ,REPLACE(CONVERT(CHAR(7), T0.F_RefDate,102),'.','-') [Identificacion/PeriodoTributario/Inicial]
		  ,REPLACE(CONVERT(CHAR(7), T0.F_RefDate,102),'.','-') [Identificacion/PeriodoTributario/Final]
		  ,O0.FormatCode [Cuenta/CodigoCuenta]
		  ,CAST(COUNT(*) AS VARCHAR(20)) [Cuenta/CantidadMovimientos]
		  ,LTRIM(STR(SUM(J1.Debit),18,0)) [Cuenta/Cierre/MontosPeriodo/Debe]
		  ,LTRIM(STR(SUM(J1.Credit),18,0)) [Cuenta/Cierre/MontosPeriodo/Haber]
		  ,LTRIM(STR(CASE WHEN SUM(J1.Debit) > SUM(J1.Credit) THEN SUM(J1.Debit) - SUM(J1.Credit) ELSE 0 END,18,0)) [Cuenta/Cierre/MontosPeriodo/SaldoDeudor]
		  ,LTRIM(STR(CASE WHEN SUM(J1.Credit) > SUM(J1.Debit) THEN SUM(J1.Credit) - SUM(J1.Debit) ELSE 0 END,18,0)) [Cuenta/Cierre/MontosPeriodo/SaldoAcreedor]
		  ,(SELECT LTRIM(STR(SUM(B.Debit),18,0)) 
		      FROM OJDT A 
			  JOIN JDT1 B ON B.TransId = A.TransId 
			  JOIN OFPR C ON A.FinncPriod = C.AbsEntry 
			 WHERE A.RefDate <= T0.T_RefDate 
			   AND C.Category = T0.Category 
			   AND B.Account = O0.AcctCode
			   AND A.TransType NOT IN ('-2','-3')) [Cuenta/Cierre/MontosAcumulado/Debe]
		  ,(SELECT LTRIM(STR(SUM(B.Credit),18,0)) 
		      FROM OJDT A 
			  JOIN JDT1 B ON B.TransId = A.TransId 
			  JOIN OFPR C ON A.FinncPriod = C.AbsEntry 
			 WHERE A.RefDate <= T0.T_RefDate 
			   AND C.Category = T0.Category 
			   AND B.Account = O0.AcctCode
			   AND A.TransType NOT IN ('-2','-3')) [Cuenta/Cierre/MontosAcumulado/Haber]
		  ,(SELECT LTRIM(STR(CASE WHEN SUM(B.Debit) > SUM(B.Credit) THEN SUM(B.Debit) - SUM(B.Credit) ELSE 0 END,18,0)) 
		      FROM OJDT A 
			  JOIN JDT1 B ON B.TransId = A.TransId 
			  JOIN OFPR C ON A.FinncPriod = C.AbsEntry 
			 WHERE A.RefDate <= T0.T_RefDate 
			   AND C.Category = T0.Category 
			   AND B.Account = O0.AcctCode
			   AND A.TransType NOT IN ('-2','-3')) [Cuenta/Cierre/MontosAcumulado/SaldoDeudor]
		  ,(SELECT LTRIM(STR(CASE WHEN SUM(B.Credit) > SUM(B.Debit) THEN SUM(B.Credit) - SUM(B.Debit) ELSE 0 END,18,0)) 
		      FROM OJDT A 
			  JOIN JDT1 B ON B.TransId = A.TransId 
			  JOIN OFPR C ON A.FinncPriod = C.AbsEntry 
			 WHERE A.RefDate <= T0.T_RefDate 
			   AND C.Category = T0.Category 
			   AND B.Account = O0.AcctCode
			   AND A.TransType NOT IN ('-2','-3')) [Cuenta/Cierre/MontosAcumulado/SaldoAcreedor]
	  FROM OFPR T0
	  JOIN OJDT J0 ON J0.RefDate BETWEEN T0.F_RefDate AND T0.T_RefDate
	  JOIN JDT1 J1 ON J1.TransId = J0.TransId
	  JOIN OACT O0 ON O0.AcctCode = J1.Account
	  , OADM A0
	 WHERE T0.AbsEntry = @Periodo
	   AND J0.TransType NOT IN ('-2','-3')
	 GROUP BY
		   A0.TaxIdNum
		  ,O0.FormatCode
		  ,T0.T_RefDate
		  ,REPLACE(CONVERT(CHAR(7), T0.F_RefDate,102),'.','-')
		  ,T0.Category
		  ,O0.AcctCode

		  --select * from JDT1 where RefDate between '20160101' and '20161231' and Account = '1-1-040-10-000' and TransType not in('-2','-3')
		  --select Sum(Credit) from JDT1 where RefDate between '20160101' and '20161231' and Account = '1-1-040-10-000'and TransType not in('-2','-3')
END