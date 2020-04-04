--IF EXISTS (SELECT name FROM dbo.sysobjects WHERE type = 'U' AND NAME = 'VID_FEASIGFOLSUC')
--	DROP table [dbo].[VID_FEASIGFOLSUC]
--GO--

CREATE TABLE [dbo].[VID_FEASIGFOLSUC](
	[DocEntry] [numeric](18, 0) NOT NULL,
	[TipoDoc] [varchar](10) NOT NULL,
	[CAFDesde] [int] NOT NULL,
	[CAFHasta] [int] NOT NULL,
	[CAFFecha] [datetime] NOT NULL,
	[Sucursal] [nvarchar](30) NOT NULL,
	[Desde] [int] NOT NULL,
	[Hasta] [int] NOT NULL,
	[CantAsig] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [KVID_FEASIGFOLSUC_PR] PRIMARY KEY CLUSTERED 
(
	[DocEntry] ASC,
	[TipoDoc] ASC,
	[CAFDesde] ASC,
	[CAFHasta] ASC,
	[Sucursal] ASC,
	[Desde] ASC,
	[Hasta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


