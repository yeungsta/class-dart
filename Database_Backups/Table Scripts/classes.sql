USE [DB_51435_menudart]
GO

/****** Object:  Table [dbo].[Classes_classdart]    Script Date: 08/16/2013 15:34:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Classes_classdart](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Template] [nvarchar](max) NULL,
	[Instructor] [nvarchar](max) NULL,
	[Url] [nvarchar](max) NULL,
	[Announcements] [xml] NULL,
	[Assignments] [xml] NULL,
	[Subscribers] [xml] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

