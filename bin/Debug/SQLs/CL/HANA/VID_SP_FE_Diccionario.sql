--DROP PROCEDURE VID_SP_FE_Diccionario

CREATE PROCEDURE VID_SP_FE_Diccionario
(
 	IN Periodo VARCHAR(10)
)
LANGUAGE SqlScript
AS
BEGIN

	SELECT REPLACE(O0."TaxIdNum",'.','') "Identificacion/RutContribuyente"
		  ,TO_VARCHAR(F0."F_RefDate", 'yyyy-MM') "Identificacion/PeriodoTributario"
		  ,T0."U_Clasif"	"Cuenta/ClasificacionCuenta"
		  ,T1."U_CtaSAP"	"Cuenta/CodigoCuenta"
		  ,T1."U_DescSAP"	"Cuenta/Glosa"
		  ,REPLACE(T0."U_Cuenta",'.','')	"Cuenta/CodigoSII"
	  FROM "@VID_FEPLANCTA" T0
	  JOIN "@VID_FEPLANCTAD" T1 ON T1."DocEntry" = T0."DocEntry"
	  ,"OFPR" F0
	  ,"OADM" O0
	 WHERE IFNULL(T1."U_CtaSAP",'') <> ''
	   AND TO_VARCHAR(F0."AbsEntry") = :Periodo;
END;