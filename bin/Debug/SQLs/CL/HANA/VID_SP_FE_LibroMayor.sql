--DROP PROCEDURE VID_SP_FE_LibroMayor

CREATE PROCEDURE VID_SP_FE_LibroMayor
(
 	IN Periodo VARCHAR(10)
)
LANGUAGE SqlScript
AS
BEGIN

	SELECT REPLACE(A0."TaxIdNum",'.','') "Identificacion/RutContribuyente"
		  ,TO_VARCHAR(T0."F_RefDate", 'yyyy-MM') "Identificacion/PeriodoTributario/Inicial"
		  ,TO_VARCHAR(T0."F_RefDate", 'yyyy-MM') "Identificacion/PeriodoTributario/Final"
		  ,O0."FormatCode" "Cuenta/CodigoCuenta"
		  ,CAST(COUNT(*) AS VARCHAR(20)) "Cuenta/CantidadMovimientos"
		  ,TO_VARCHAR(ROUND(SUM(J1."Debit"),0)) "Cuenta/Cierre/MontosPeriodo/Debe"
		  ,TO_VARCHAR(ROUND(SUM(J1."Credit"),0)) "Cuenta/Cierre/MontosPeriodo/Haber"
		  ,TO_VARCHAR(ROUND(CASE WHEN SUM(J1."Debit") > SUM(J1."Credit") THEN SUM(J1."Debit") - SUM(J1."Credit") ELSE 0 END,0)) "Cuenta/Cierre/MontosPeriodo/SaldoDeudor"
		  ,TO_VARCHAR(ROUND(CASE WHEN SUM(J1."Credit") > SUM(J1."Debit") THEN SUM(J1."Credit") - SUM(J1."Debit") ELSE 0 END,0)) "Cuenta/Cierre/MontosPeriodo/SaldoAcreedor"
		  ,(SELECT TO_VARCHAR(ROUND(SUM(B."Debit"),0)) 
		      FROM "OJDT" A 
			  JOIN "JDT1" B ON B."TransId" = A."TransId"
			  JOIN "OFPR" C ON A."FinncPriod" = C."AbsEntry"
			 WHERE A."RefDate" <= T0."T_RefDate"
			   AND C."Category" = T0."Category"
			   AND B."Account" = O0."AcctCode"
			   AND A."TransType" NOT IN ('-2','-3')) "Cuenta/Cierre/MontosAcumulado/Debe"
		  ,(SELECT TO_VARCHAR(ROUND(SUM(B."Credit"),0)) 
		      FROM "OJDT" A 
			  JOIN "JDT1" B ON B."TransId" = A."TransId"
			  JOIN "OFPR" C ON A."FinncPriod" = C."AbsEntry"
			 WHERE A."RefDate" <= T0."T_RefDate"
			   AND C."Category" = T0."Category"
			   AND B."Account" = O0."AcctCode"
			   AND A."TransType" NOT IN ('-2','-3')) "Cuenta/Cierre/MontosAcumulado/Haber"
		  ,(SELECT TO_VARCHAR(ROUND(CASE WHEN SUM(B."Debit") > SUM(B."Credit") THEN SUM(B."Debit") - SUM(B."Credit") ELSE 0 END,0)) 
		      FROM "OJDT" A 
			  JOIN "JDT1" B ON B."TransId" = A."TransId"
			  JOIN "OFPR" C ON A."FinncPriod" = C."AbsEntry"
			 WHERE A."RefDate" <= T0."T_RefDate"
			   AND C."Category" = T0."Category"
			   AND B."Account" = O0."AcctCode"
			   AND A."TransType" NOT IN ('-2','-3')) "Cuenta/Cierre/MontosAcumulado/SaldoDeudor"
		  ,(SELECT TO_VARCHAR(ROUND(CASE WHEN SUM(B."Credit") > SUM(B."Debit") THEN SUM(B."Credit") - SUM(B."Debit") ELSE 0 END,0)) 
		      FROM "OJDT" A 
			  JOIN "JDT1" B ON B."TransId" = A."TransId"
			  JOIN "OFPR" C ON A."FinncPriod" = C."AbsEntry"
			 WHERE A."RefDate" <= T0."T_RefDate"
			   AND C."Category" = T0."Category"
			   AND B."Account" = O0."AcctCode"
			   AND A."TransType" NOT IN ('-2','-3')) "Cuenta/Cierre/MontosAcumulado/SaldoAcreedor"
	  FROM "OFPR" T0
	  JOIN "OJDT" J0 ON J0."RefDate" BETWEEN T0."F_RefDate" AND T0."T_RefDate"
	  JOIN "JDT1" J1 ON J1."TransId" = J0."TransId"
	  JOIN "OACT" O0 ON O0."AcctCode" = J1."Account"
	  , "OADM" A0
	 WHERE TO_VARCHAR(T0."AbsEntry") = :Periodo
	   AND J0."TransType" NOT IN ('-2','-3')
	 GROUP BY
		   A0."TaxIdNum"
		  ,O0."FormatCode"
		  ,T0."T_RefDate"
		  ,TO_VARCHAR(T0."F_RefDate", 'yyyy-MM')
		  ,T0."Category"
		  ,O0."AcctCode";

END;