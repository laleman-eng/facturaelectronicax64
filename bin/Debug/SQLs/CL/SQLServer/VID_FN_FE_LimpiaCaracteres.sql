IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_FN_FE_LimpiaCaracteres' AND type = 'FN')
   DROP FUNCTION VID_FN_FE_LimpiaCaracteres
GO--

CREATE FUNCTION VID_FN_FE_LimpiaCaracteres (@texto VARCHAR(MAX))
RETURNS VARCHAR(MAX)
AS
BEGIN
	DECLARE
		 @TextoFinal	VarChar(MAX)
		,@wcount		Int
		,@index			Int
		,@len			Int
		,@char			VarChar(1)
	
	DECLARE
		 @ListofIDs		Table(IDs VarChar(100));
	
	INSERT INTO @ListofIDs
	VALUES ('°'),('\'),('"'),('!'),('|'),('·'),('#'),('$'),('='),('?'),('¿'),('¡'),('~'),('{'),('}'),('['),(']'),('%'),('&'),('-'),(':'),(';'),('`'),('^'),('´'),('€'),('¬');
	
	SET @TextoFinal	= ''
	SET @wcount		= 0 
	SET @index		= 1 
	SET @len		= LEN(@texto)

	WHILE @index <= @len 
	BEGIN 
		SET @char = SUBSTRING(@texto, @index, 1) 
		IF NOT EXISTS(SELECT IDs FROM @ListofIDs WHERE IDs = @char)
			SET @TextoFinal = @TextoFinal + @char
			
		SET @index = @index + 1 
	END

	SET @TextoFinal = REPLACE(REPLACE(@TextoFinal, 'Ñ', 'N'), 'ñ', 'n')

	RETURN REPLACE(@TextoFinal, char(39), '')
END
