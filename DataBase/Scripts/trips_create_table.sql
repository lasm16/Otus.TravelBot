CREATE TABLE [dbo].[Trips] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [City]        NVARCHAR (MAX)   NULL,
    [DateStart]   DATETIME2 (7)    NOT NULL,
    [DateEnd]     DATETIME2 (7)    NOT NULL,
    [Description] NVARCHAR (MAX)   NULL,
    [Photo]       NVARCHAR (MAX)   NULL,
    [TripStatusId]    INT              NOT NULL,
    [Status]      INT              NOT NULL,
    [UserId]      BIGINT           NOT NULL,
    CONSTRAINT [PK_Trips] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Trips_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Trips_UserId]
    ON [dbo].[Trips]([UserId] ASC);

