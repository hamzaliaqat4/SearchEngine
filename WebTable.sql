CREATE TABLE [dbo].[Web] (
    [URL]   VARCHAR (150)  NOT NULL,
    [ID]    INT            NOT NULL,
    [Title] VARCHAR (100)  NOT NULL,
    [Page]  VARCHAR (8000) NOT NULL,
    PRIMARY KEY CLUSTERED ([URL] ASC)
);

