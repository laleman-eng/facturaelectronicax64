--DROP PROCEDURE VID_SP_FE_LibroDiario

CREATE PROCEDURE VID_SP_FE_LibroDiario
(
 	IN Periodo VARCHAR(10)
)
LANGUAGE SqlScript
AS
BEGIN

	SELECT REPLACE(A0."TaxIdNum",'.','') "Identificacion/RutContribuyente"
		  ,TO_VARCHAR(T0."F_RefDate", 'yyyy-MM') "Identificacion/PeriodoTributario/Inicial"
		  ,TO_VARCHAR(T0."F_RefDate", 'yyyy-MM') "Identificacion/PeriodoTributario/Final"
		  ,TO_VARCHAR(J0."RefDate", 'yyyy-MM-dd')	"RegistroDiario/FechaContable"
		  ,CAST(SUM(CASE WHEN J1."Line_ID" = 0 THEN 1 ELSE 0 END) AS VARCHAR(20))	"RegistroDiario/CantidadComprobantes"
		  ,CAST(COUNT(*) AS VARCHAR(20)) "RegistroDiario/CantidadMovimientos"
		  ,TO_VARCHAR(ROUND(SUM(J1."Debit"),0)) "RegistroDiario/SumaValorComprobante"
		  ,(SELECT CAST(COUNT(*) AS VARCHAR(20)) FROM "OJDT" WHERE "RefDate" BETWEEN T0."F_RefDate" AND T0."T_RefDate") "Cierre/CantidadComprobantes"
		  ,(SELECT CAST(COUNT(*) AS VARCHAR(20)) FROM "OJDT" A JOIN "JDT1" B ON B."TransId" = A."TransId" WHERE A."RefDate" BETWEEN T0."F_RefDate" AND T0."T_RefDate") "Cierre/CantidadMovimientos"
		  ,(SELECT TO_VARCHAR(ROUND(SUM(B."Debit"),0)) FROM "OJDT" A JOIN "JDT1" B ON B."TransId" = A."TransId" WHERE A."RefDate" BETWEEN T0."F_RefDate" AND T0."T_RefDate") "Cierre/SumaValorComprobante"
		  ,(SELECT TO_VARCHAR(ROUND(SUM(B."Debit"),0)) FROM "OJDT" A JOIN "JDT1" B ON B."TransId" = A."TransId" JOIN "OFPR" C ON A."FinncPriod" = C."AbsEntry" WHERE A."RefDate" <= T0."T_RefDate" AND C."Category" = T0."Category") "Cierre/ValorAcumulado"
	  FROM "OFPR" T0
	  JOIN "OJDT" J0 ON J0."RefDate" BETWEEN T0."F_RefDate" AND T0."T_RefDate"
	  JOIN "JDT1" J1 ON J1."TransId" = J0."TransId"
	  , "OADM" A0
	 WHERE TO_VARCHAR(T0."AbsEntry") = :Periodo
	   AND J0."TransType" NOT IN ('-2','-3')
	 GROUP BY
		   A0."TaxIdNum"
		  ,TO_VARCHAR(J0."RefDate", 'yyyy-MM-dd')
		  ,T0."F_RefDate"
		  ,T0."T_RefDate"
		  ,T0."Category";
END;