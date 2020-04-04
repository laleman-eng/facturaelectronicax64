IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_LOTES' AND type = 'P')
	DROP PROCEDURE [dbo].[VID_SP_LOTES]
GO--

CREATE PROCEDURE [dbo].[VID_SP_LOTES](
    @DYT_ID_TRASPASO_ENC numeric(18, 0),
	@CAB_COD_TP_FACTURA char(3),
	@CAB_FOL_DOCTO_INT numeric(18, 0),
	@DET_NUM_LINEA_DET numeric(18, 0),
	@LOTE nvarchar(30),
	@CANTIDAD NUMERIC(18,6),
	@ITEMCODE nvarchar(20),
	@DUEDATE varchar(10),
	@VALOR1  varchar(250),
	@VALOR2  varchar(250),
	@VALOR3  varchar(250),
	@VALOR4  varchar(250),
	@VALOR5  varchar(250),
	@VALOR6  varchar(250),
	@VALOR7  varchar(250)
)      
AS 
BEGIN   
	Declare    
	   @Ws_Mensaje char(200)    
	    
	   Select @Ws_Mensaje = ' '    
	    
	set dateformat dmy    
	  
	If Not Exists(Select 1 
				  From VID_FELOTES
				  Where DYT_ID_TRASPASO_ENC = @DYT_ID_TRASPASO_ENC
					And CAB_COD_TP_FACTURA = @CAB_COD_TP_FACTURA
					And CAB_FOL_DOCTO_INT = @CAB_FOL_DOCTO_INT
					And DET_NUM_LINEA_DET = @DET_NUM_LINEA_DET
					And LOTE = @LOTE
					And CANTIDAD = @CANTIDAD
					And ITEMCODE = @ITEMCODE)
	   Begin    
	  INSERT INTO  VID_FELOTES     
		   ( DYT_ID_TRASPASO_ENC
			,CAB_COD_TP_FACTURA
			,CAB_FOL_DOCTO_INT
			,DET_NUM_LINEA_DET
			,LOTE
			,ITEMCODE
			,CANTIDAD
			,DUEDATE
			,VALOR1
			,VALOR2
			,VALOR3
			,VALOR4
			,VALOR5
			,VALOR6
			,VALOR7
		   ) VALUES (
		     @DYT_ID_TRASPASO_ENC
			,@CAB_COD_TP_FACTURA
			,@CAB_FOL_DOCTO_INT
			,@DET_NUM_LINEA_DET
			,@LOTE
			,@ITEMCODE
			,@CANTIDAD
			,@DUEDATE
			,@VALOR1
			,@VALOR2
			,@VALOR3
			,@VALOR4
			,@VALOR5
			,@VALOR6
			,@VALOR7
		   )       
	 
		 --Select @DYT_ID_TRASPASO = @@IDENTITY  
		 SET @Ws_Mensaje = 'INSERT'   
	   End    
	       
	Else    
	   Begin    
		  UPDATE  VID_FELOTES
			 SET  LOTE   = @LOTE     
			 , CANTIDAD = @CANTIDAD
			 , ITEMCODE = @ITEMCODE
			 , DUEDATE  = @DUEDATE
			 , VALOR1   = @Valor1
			 , VALOR2   = @Valor2
			 , VALOR3   = @Valor3
			 , VALOR4   = @Valor4
			 , VALOR5   = @Valor5
			 , VALOR6   = @Valor6
			 , VALOR7   = @Valor7
		   Where DYT_ID_TRASPASO_ENC   = @DYT_ID_TRASPASO_ENC    
		     And CAB_COD_TP_FACTURA    = @CAB_COD_TP_FACTURA     
		     And CAB_FOL_DOCTO_INT     = @CAB_FOL_DOCTO_INT 
			 And DET_NUM_LINEA_DET     = @DET_NUM_LINEA_DET   
	   SET @Ws_Mensaje = 'UPDATE'  
	 End    
	    
	    
	 Select @Ws_Mensaje
END