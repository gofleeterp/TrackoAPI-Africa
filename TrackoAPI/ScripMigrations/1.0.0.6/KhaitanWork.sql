IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'GeographicPoint' AND is_computed=0
          AND Object_ID = Object_ID(N'dbo.mVehicleMaster'))
BEGIN
    EXEC sp_sqlexec 'ALTER TABLE dbo.mVehicleMaster DROP COLUMN GeographicPoint';
	EXEC sp_sqlexec 'ALTER TABLE dbo.mVehicleMaster ADD GeographicPoint AS ([geography]::Point([Latitude],[Longitude],(4326)))';
	EXEC sp_sqlexec N'CREATE SPATIAL INDEX idx_CityMaster_GeographicPoint ON dbo.mCityMaster(GeographyPoint) USING GEOGRAPHY_AUTO_GRID persisted'
	EXEC sp_sqlexec N'CREATE SPATIAL INDEX idx_mVehicleMaster_GeographicPoint ON dbo.mVehicleMaster(GeographicPoint) USING GEOGRAPHY_AUTO_GRID'
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.triggers where name='trg_Update_GPSInfo_VehicleMaster')
BEGIN

EXEC sp_sqlexec N'-- =============================================
-- Author:		Mukesh Rebari
-- Create date: 2019-09-09
-- Description:	Update GP Info on Vehicle Master
-- =============================================
CREATE TRIGGER dbo.trg_Update_GPSInfo_VehicleMaster 
   ON  dbo.tGPSStatusLog 
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	UPDATE vm SET vm.GPSLocation=i.GPSLocation,vm.Latitude=i.Latitude,vm.Longitude=i.Longitude,vm.GPSTime=i.GPSTime,vm.GPSId=i.Id
	FROM inserted i JOIN mVehicleMaster vm ON i.VehicleNo=vm.VehicleRegNo
END'
END
GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'GPSGeofenceEventLog')
BEGIN
EXEC sp_sqlexec N'
CREATE TABLE [dbo].[GPSGeofenceEventLog](
	[VehicleId] [bigint] NULL,
	[VehicleNo] [nvarchar](200) NOT NULL,
	[GeoFenceId] [bigint] NOT NULL,
	[InTime] [datetime] NOT NULL,
	[OutTime] [datetime] NULL,
	[TimeInMinutes]  AS (datediff(minute,[InTime],isnull([OutTime],getdate()))),
	[EventId] [uniqueidentifier] NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_GPSGeofenceEventLog] PRIMARY KEY CLUSTERED 
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[GPSGeofenceEventLog] ADD  CONSTRAINT [DF_GPSGeofenceEventLog_EventId]  DEFAULT (newsequentialid()) FOR [EventId]
GO

ALTER TABLE [dbo].[GPSGeofenceEventLog] ADD  CONSTRAINT [DF_GPSGeofenceEventLog_UpdatedAt]  DEFAULT (getdate()) FOR [UpdatedAt]
GO

CREATE NONCLUSTERED INDEX [IX_GPSGeofenceEventLog] ON [dbo].[GPSGeofenceEventLog]
(
	[VehicleNo] ASC,
	[GeoFenceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


CREATE NONCLUSTERED INDEX [IX_GPSGeofenceEventLog_VehicleNo] ON [dbo].[GPSGeofenceEventLog]
(
	[VehicleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
'
END

GO
EXEC sp_sqlexec N'
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER Proc [dbo].[Proc_GLB_CheckForGeoFenceEvent]
AS
BEGIN
	UPDATE ge SET ge.OutTime=vm.GPSTime,ge.UpdatedAt=GETDATE()
	FROM GPSGeofenceEventLog ge
	JOIN mVehicleMaster vm ON ge.VehicleNo=vm.VehicleRegNo AND ge.OutTime IS NULL
	LEFT JOIN mCityMaster cm ON ge.GeoFenceId=cm.Id AND vm.GeographicPoint.STWithin(cm.GeographyPoint)=1
	WHERE cm.Id IS NULL

	UPDATE GPSGeofenceEventLog SET UpdatedAt=GETDATE()
	where OutTime IS NULL

	INSERT INTO GPSGeofenceEventLog(VehicleId,VehicleNo,GeoFenceId,InTime)
	SELECT VehicleId=vm.Id,VehicleNo=vm.VehicleRegNo,GeoFenceId=g.CityId,InTime=vm.GPSTime
	FROM mVehicleMaster vm 
	JOIN (SELECT CityName,CityId=Id,GeographyPoint 
			FROM mCityMaster 
			WHERE GeographyPoint IS NOT NULL) as g ON vm.GeographicPoint.STWithin(g.GeographyPoint)=1
	LEFT JOIN GPSGeofenceEventLog ge ON ge.VehicleNo=vm.VehicleRegNo AND ge.GeoFenceId=g.CityId and ge.OutTime IS NULL
	WHERE ge.EventId IS NULL
END/****** Object:  StoredProcedure [dbo].[Proc_GLB_CheckForGeoFenceEvent]    Script Date: 11-09-2019 17:25:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[Proc_GLB_CheckForGeoFenceEvent]
AS
BEGIN
	UPDATE ge SET ge.OutTime=vm.GPSTime,ge.UpdatedAt=GETDATE()
	FROM GPSGeofenceEventLog ge
	JOIN mVehicleMaster vm ON ge.VehicleNo=vm.VehicleRegNo AND ge.OutTime IS NULL
	LEFT JOIN mCityMaster cm ON ge.GeoFenceId=cm.Id AND vm.GeographicPoint.STWithin(cm.GeographyPoint)=1
	WHERE cm.Id IS NULL

	UPDATE GPSGeofenceEventLog SET UpdatedAt=GETDATE()
	where OutTime IS NULL

	INSERT INTO GPSGeofenceEventLog(VehicleId,VehicleNo,GeoFenceId,InTime)
	SELECT VehicleId=vm.Id,VehicleNo=vm.VehicleRegNo,GeoFenceId=g.CityId,InTime=vm.GPSTime
	FROM mVehicleMaster vm 
	JOIN (SELECT CityName,CityId=Id,GeographyPoint 
			FROM mCityMaster 
			WHERE GeographyPoint IS NOT NULL) as g ON vm.GeographicPoint.STWithin(g.GeographyPoint)=1
	LEFT JOIN GPSGeofenceEventLog ge ON ge.VehicleNo=vm.VehicleRegNo AND ge.GeoFenceId=g.CityId and ge.OutTime IS NULL
	WHERE ge.EventId IS NULL
END'