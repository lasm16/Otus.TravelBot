CREATE TABLE [dbo].[Users] (
    [Id]         BIGINT         NOT NULL,
    [NickName]   NVARCHAR (MAX) NULL,
    [Type]       INT            NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);

