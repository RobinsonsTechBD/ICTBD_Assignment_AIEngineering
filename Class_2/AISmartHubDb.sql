CREATE DATABASE AISmartHubDb;
GO

USE AISmartHubDb;
GO

CREATE TABLE AIInteractions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    InteractionType VARCHAR(50) NOT NULL, -- 'TextToText', 'SpeechToText', 'TextToSpeech'
    InputData NVARCHAR(MAX) NOT NULL,      -- The original text or audio file metadata
    OutputData NVARCHAR(MAX) NOT NULL,     -- The translated text or generated file path
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO