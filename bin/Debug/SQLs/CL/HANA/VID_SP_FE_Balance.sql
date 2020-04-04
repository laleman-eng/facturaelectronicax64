--DROP PROCEDURE VID_SP_FE_Balance

CREATE PROCEDURE VID_SP_FE_Balance
(
 IN PeriodoD VARCHAR(10),
 IN PeriodoH VARCHAR(10)
)
LANGUAGE SqlScript
AS
BEGIN
	
	SELECT REPLACE(O0."TaxIdNum",'.','') "Identificacion/RutContribuyente"
		  ,(SELECT TO_VARCHAR("F_RefDate", "yyyy-MM")FROM "OFPR" WHERE "AbsEntry" = :PeriodoH) "Identificacion/PeriodoTributario"
		  ,C0."FormatCode"	"Cuenta/CodigoCuenta"
		  ,TO_VARCHAR(ROUND(SUM(J1."Debit"),0)) "Cuenta/Debe"
		  ,TO_VARCHAR(ROUND(SUM(J1."Credit"),0)) "Cuenta/Haber"
		  ,TO_VARCHAR(ROUND(CASE WHEN SUM(J1."Debit") > SUM(J1."Credit") THEN SUM(J1."Debit") - SUM(J1."Credit") ELSE 0 END,0)) "Cuenta/SaldoDeudor"
		  ,TO_VARCHAR(ROUND(CASE WHEN SUM(J1."Credit") > SUM(J1."Debit") THEN SUM(J1."Credit") - SUM(J1."Debit") ELSE 0 END,0)) "Cuenta/SaldoAcreedor"
	  FROM "OFPR" T0
	  JOIN "OJDT" J0 ON J0."RefDate" BETWEEN T0."F_RefDate" AND T0."T_RefDate"
	  JOIN "JDT1" J1 ON J1."TransId" = J0."TransId"
	  JOIN "OACT" C0 ON C0."AcctCode" = J1."Account"
	  , "OADM" O0
	 WHERE T0."AbsEntry" >= :PeriodoD
	   AND T0."AbsEntry" <= :PeriodoH
	   AND J0."TransType" NOT IN ('-2','-3')
	   AND C0."GroupMask" <= 3
	 GROUP BY O0."TaxIdNum"
	      --,REPLACE(CONVERT(CHAR(7), T0.F_RefDate,102),'.','-')
		  ,C0."FormatCode";
END;