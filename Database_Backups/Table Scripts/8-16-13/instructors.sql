USE [DB_51435_menudart]
GO

/****** Object:  Table [dbo].[Instructors_classdart]    Script Date: 06/11/2013 22:18:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Instructors_classdart](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[User] [nvarchar](max) NOT NULL,
	[Prefix] [nvarchar](max) NULL,
	[FirstName] [nvarchar](max) NULL,
	[MiddleInitial] [nvarchar](max) NULL,
	[LastName] [nvarchar](max) NULL,
	[Bio] [nvarchar](max) NULL,
	[Phone] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[Facebook] [nvarchar](max) NULL,
	[Twitter] [nvarchar](max) NULL,
	[LinkedIn] [nvarchar](max) NULL,
	[Website] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

