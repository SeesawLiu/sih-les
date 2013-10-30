alter table LE_FlowDetSnapShot add ExtraDmdSource varchar(256)
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_FlowDetExtraDmdSourceSnapshot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_FlowDetExtraDmdSourceSnapshot]
GO

GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[LE_FlowDetExtraDmdSourceSnapshot](
	[FlowDetSnapShotId] [int] NOT NULL,
	[Location] [varchar](50) NOT NULL,
 CONSTRAINT [PK_LE_FlowDetExtraDmdSourceSnapshot] PRIMARY KEY CLUSTERED 
(
	[FlowDetSnapShotId] ASC,
	[Location] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

alter table LOG_OrderTraceDet alter column ReqTime datetime null
go


