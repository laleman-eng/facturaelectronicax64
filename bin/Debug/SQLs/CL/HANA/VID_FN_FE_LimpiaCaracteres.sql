--DROP FUNCTION VID_FN_FE_LimpiaCaracteres;
CREATE FUNCTION VID_FN_FE_LimpiaCaracteres (texto VARCHAR(254))
RETURNS resultado VARCHAR(254)
LANGUAGE SQLSCRIPT
AS
BEGIN
		
	resultado := REPLACE(:texto, 'Ñ', 'N');
	resultado := REPLACE(:resultado, 'ñ', 'n');
	resultado := REPLACE(:resultado, 'º', '');
	resultado := REPLACE(:resultado, '\\', '');
	resultado := REPLACE(:resultado, '"', '');
	resultado := REPLACE(:resultado, '!', '');
	resultado := REPLACE(:resultado, '|', '');
	resultado := REPLACE(:resultado, '·', '');
	resultado := REPLACE(:resultado, '#', '');
	resultado := REPLACE(:resultado, '$', '');
	resultado := REPLACE(:resultado, '=', '');
	resultado := REPLACE(:resultado, '?', '');
	resultado := REPLACE(:resultado, '¿', '');
	resultado := REPLACE(:resultado, '¡', '');
	resultado := REPLACE(:resultado, '~', '');
	resultado := REPLACE(:resultado, '{', '');
	resultado := REPLACE(:resultado, '}', '');
	resultado := REPLACE(:resultado, '[', '');
	resultado := REPLACE(:resultado, ']', '');
	resultado := REPLACE(:resultado, '%', '');
	resultado := REPLACE(:resultado, '&', '');
	resultado := REPLACE(:resultado, '-', '');
	resultado := REPLACE(:resultado, ':', '');
	resultado := REPLACE(:resultado, ';', '');
	resultado := REPLACE(:resultado, '`', '');
	resultado := REPLACE(:resultado, '^', '');

END;


