--DROP FUNCTION VID_FN_FE_LimpiaCaracteres;
CREATE FUNCTION VID_FN_FE_LimpiaCaracteres (texto VARCHAR(254))
RETURNS resultado VARCHAR(254)
LANGUAGE SQLSCRIPT
AS
BEGIN
		
	resultado := REPLACE(:texto, '�', 'N');
	resultado := REPLACE(:resultado, '�', 'n');
	resultado := REPLACE(:resultado, '�', '');
	resultado := REPLACE(:resultado, '\\', '');
	resultado := REPLACE(:resultado, '"', '');
	resultado := REPLACE(:resultado, '!', '');
	resultado := REPLACE(:resultado, '|', '');
	resultado := REPLACE(:resultado, '�', '');
	resultado := REPLACE(:resultado, '#', '');
	resultado := REPLACE(:resultado, '$', '');
	resultado := REPLACE(:resultado, '=', '');
	resultado := REPLACE(:resultado, '?', '');
	resultado := REPLACE(:resultado, '�', '');
	resultado := REPLACE(:resultado, '�', '');
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


