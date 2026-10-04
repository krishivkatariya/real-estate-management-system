IF OBJECT_ID('dbo.BookingRequests','U') IS NULL
BEGIN
	CREATE TABLE dbo.BookingRequests(
		Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		BuyerId NVARCHAR(450) NOT NULL,
		PropertyId INT NOT NULL,
		SellerId NVARCHAR(450) NOT NULL,
		PreferredDate DATETIME2 NOT NULL,
		PreferredTime NVARCHAR(50) NOT NULL,
		Message NVARCHAR(2000) NULL,
		Status INT NOT NULL DEFAULT(0),
		SellerResponse NVARCHAR(2000) NULL,
		CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
		RespondedAt DATETIME2 NULL
	);

	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BookingRequests_AspNetUsers_Buyer')
	BEGIN
		ALTER TABLE dbo.BookingRequests
		ADD CONSTRAINT FK_BookingRequests_AspNetUsers_Buyer FOREIGN KEY (BuyerId) REFERENCES dbo.AspNetUsers(Id);
	END

	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BookingRequests_AspNetUsers_Seller')
	BEGIN
		ALTER TABLE dbo.BookingRequests
		ADD CONSTRAINT FK_BookingRequests_AspNetUsers_Seller FOREIGN KEY (SellerId) REFERENCES dbo.AspNetUsers(Id);
	END

	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BookingRequests_Properties_Property')
	BEGIN
		ALTER TABLE dbo.BookingRequests
		ADD CONSTRAINT FK_BookingRequests_Properties_Property FOREIGN KEY (PropertyId) REFERENCES dbo.Properties(Id);
	END

	IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BookingRequests_Property_Buyer')
	BEGIN
		CREATE INDEX IX_BookingRequests_Property_Buyer ON dbo.BookingRequests(PropertyId, BuyerId);
	END
END
GO
