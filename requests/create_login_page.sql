SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON

GO
CREATE TABLE [dbo].[Logins] (
    [loginId] [bigint] IDENTITY(1,1) NOT NULL,
    [login] [nvarchar](80) NOT NULL,
    [password] [nvarchar](60) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
    [updated_at] [datetime2](7) NOT NULL,
	[websiteId] [bigint] NULL,
	[userId] [bigint] NULL,
)

GO
ALTER TABLE [dbo].[Logins] ADD  CONSTRAINT [PK_Keys_logins] PRIMARY KEY CLUSTERED 
(
	[loginId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_Keys_websiteId_logins] ON [dbo].[Logins]
(
	[websiteId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_Keys_userId_logins] ON [dbo].[Logins]
(
	[userId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Logins]  WITH CHECK ADD  CONSTRAINT [FK_Keys_Websites_websiteId_logins] FOREIGN KEY([websiteId])
REFERENCES [dbo].[Websites] ([websiteId])

GO
ALTER TABLE [dbo].[Logins] CHECK CONSTRAINT [FK_Keys_Websites_websiteId_logins]

GO
ALTER TABLE [dbo].[Logins]  WITH CHECK ADD  CONSTRAINT [FK_Keys_Users_userId_logins] FOREIGN KEY([userId])
REFERENCES [dbo].[Users] ([userId])

GO
ALTER TABLE [dbo].[Logins] CHECK CONSTRAINT [FK_Keys_Users_userId_logins]