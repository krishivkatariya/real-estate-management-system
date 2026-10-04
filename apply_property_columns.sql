IF COL_LENGTH('dbo.Properties','Amenities') IS NULL
BEGIN
	ALTER TABLE dbo.Properties ADD Amenities NVARCHAR(2000) NULL;
END

IF COL_LENGTH('dbo.Properties','ApprovalStatus') IS NULL
BEGIN
	ALTER TABLE dbo.Properties ADD ApprovalStatus INT NOT NULL CONSTRAINT DF_Properties_ApprovalStatus DEFAULT(0) WITH VALUES;
END

IF COL_LENGTH('dbo.Properties','Balconies') IS NULL
BEGIN
	ALTER TABLE dbo.Properties ADD Balconies INT NOT NULL CONSTRAINT DF_Properties_Balconies DEFAULT(0) WITH VALUES;
END

IF COL_LENGTH('dbo.Properties','Furnishing') IS NULL
BEGIN
	ALTER TABLE dbo.Properties ADD Furnishing INT NULL;
END

IF COL_LENGTH('dbo.Properties','Pincode') IS NULL
BEGIN
	ALTER TABLE dbo.Properties ADD Pincode NVARCHAR(20) NULL;
END

IF COL_LENGTH('dbo.Properties','RejectionReason') IS NULL
BEGIN
	ALTER TABLE dbo.Properties ADD RejectionReason NVARCHAR(2000) NULL;
END

IF COL_LENGTH('dbo.Properties','SubmittedAt') IS NULL
BEGIN
	ALTER TABLE dbo.Properties ADD SubmittedAt DATETIME2 NULL CONSTRAINT DF_Properties_SubmittedAt DEFAULT(GETUTCDATE());
END
