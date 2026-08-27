--Rename the stored procedure.
IF OBJECT_ID (N'Proc_OfflineData_Desktop', N'P') IS NULL
BEGIN
	EXEC sp_rename 'Proc_OfflineData_Mobile','Proc_OfflineData_Desktop';
	UPDATE mReportProcedure SET spName='[dbo].[Proc_OfflineData_Desktop]@parameter1,@parameter2' WHERE Id=533
END
GO
IF(NOT EXISTS(SELECT 1 FROM dbo.mReportProcedure AS m WHERE m.Id=535))
BEGIN
	Insert Into dbo.mReportProcedure([Id],[spName],[ReportId],[Count],[Columns],[PrintFormatDSId],[IsCUD],[IsJson],[Relations])
	Values (535,N'[dbo].[Proc_OfflineData_Mobile]@parameter1,@parameter2',1576,0,NULL,NULL,0,0,NULL)
END
GO
IF OBJECT_ID (N'Proc_OfflineData_Mobile', N'P') IS NULL
BEGIN
EXEC sp_sqlexec N'/****** Object:  StoredProcedure [dbo].[Proc_OfflineData_Mobile]    Script Date: 11-06-2019 12:54:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[Proc_OfflineData_Mobile]
@parameter1 bigint=0,--UseId
@parameter2 int=0--TypeId Zero for all
AS
BEGIN

/*Table=Offices*/
SELECT o.Id,o.OfficeName,o.OfficeAbbr,o.CityId,o.AddressId,o.PrintingAddress,o.StateId,StateName=s.[Name],o.DefaultCashAccountId,o.GSTNatureId,o.GSTNo,StateCode=s.Abbr
FROM mOfficeMaster o left join mGenericMaster s on o.StateId=s.Id
WHERE o.StatusId=0

/*Table1=Cities*/
SELECT c.Id,CityName,CityAbbr,StateId,ControllingOfficeId,Latitude,Longitude,c.ParentCityId,c.PostalCode
FROM mCityMaster as c where c.StatusId=0

/*Table2=GenericMasters*/
SELECT g.Id,g.[Name],Abbreviation=g.Abbr,g.FormId,g.ConstantId,g.Ref1Id,g.Ref2Id,g.TaxServiceTypeId
FROM mGenericMaster g where g.StatusId=0
/*Table3=ConstantTypes*/
SELECT Id,ConstantTypeAbbr,ConstantTypeName,c.ConstantTypeDesc
FROM mConstantType c where c.IsDepricated=0
/*Table4=ConstantValues*/
SELECT Id,ConstantAbbr,ConstantName,ConstantTypeId,c.ConstantRemarks,c.Ref1,c.Ref2,c.Ref3,c.Ref4,c.Visiblity
FROM mConstantValue c WHERE c.IsDepricated=0
/*Table5=ViewFields*/
SELECT Id,FieldType,DefaultGroupId,VoucherTypeId,DefaultRoleId,DefaultLedgerId,ViewId,Remark,Label,LabelToolTip,IsRequired,Watermark,BookTypeId,ShowInVTG,ControlId
FROM mViewField v where v.StatusId=0
/*Table6=DTSStatusMaps*/
SELECT d.Id,d.CurrentStatusId,d.NextStatusId,NextStatu=s.[Name],d.IsReserved
FROM mDTSStatusMap d left join mDTSStatus s on d.NextStatusId=s.Id
WHERE d.StatusId=0
/*Table7=Ledgers*/
SELECT l.Id,Code=l.AccountAbbr,l.AccountName,l.AccountRoleId,l.BookingAcName,l.FleetAcName,AccountGroupId=l.GroupId,l.ContractId,l.IsTaxApplicable,l.TaxTypeId,l.ServiceTypeId,l.StateId
FROM mLedger l left join mAccountGroup g on l.GroupId=g.Id
WHERE l.IsDefaulter=0 AND l.StatusId=0

/*Table8=LedgerRoles*/
SELECT Id,LedgerId,RoleId,IsDefault
FROM mLedgerRole lr
/*Table9=RouteMasters*/
SELECT r.Id,r.Name,r.Abbr,FromPlaceId,ToPlaceId,TransitKm,TransitHours,IsReturnRoute,NatureId,GoogleKm,ReviewDate
FROM mRouteMaster r 
WHERE r.StatusId=0

/*Table10=Rules*/
SELECT Id,RuleKey,Description,RuleNature,ValidationDefination,AssignmentDefination,FailedMessage,SuccessMessage,TerminateOnError,ReturnOnSuccess,IsActive,CreatedDate=CDOE,ModifiedDate=MDOE,ExecutionOrder
FROM mRule where IsActive=1

/*Table11=HireVehicles*/
SELECT h.Id,VehicleNo,h.Owner,h.VehicleNo,h.GPSVendorId,h.GPSAlias,HVPartyName=l.AccountName
FROM mHireVehicle h LEFT JOIN mLedger l on h.HirePartyId=l.Id
where h.IsBlackListed=0
/*Table12=Vehicles*/
SELECT v.Id,VehicleNo,OwnerId=v.OwnerPartyId,IsAttached=v.IsHireVehicle,IsGPSAttached=v.IsGPSAttached,v.GPSVendorId,VehicleOwner=o.AccountName,VehicleTypeId,VehicleType=vt.Name,LoadedAvg=v.LoadedMileage,EmptyAvg=v.EmptyMileage
FROM mVehicleMaster v left join mLedger o on v.OwnerPartyId=o.Id
left join mGenericMaster vt on vt.Id=v.VehicleTypeId

/*Table13=ApiConfigurations*/
SELECT [Key]=Id,[Value]=[ConfigValue],Options,IsApiConfig=CAST(0 as bit)
FROM mClientConfiguration
UNION ALL 
SELECT [Key]=Id,[Value],Options,IsApiConfig=CAST(1 as bit)
FROM ApiConfigurations

/*Table14=VoucherTypeLedgerMappings*/
SELECT Id,Exclude,GroupId,[Include],LedgerRoleId,MaxAmount,MinAmount,TypeId,ViewId,VoucherTypeId
FROM tVTGMapping
/*Table15=MaterialGroups*/
SELECT Id,[Name],Code
FROM mMaterialGroup m
/*Table16=Materials*/
SELECT m.Id,m.Breadth,m.DeliveryLocationId,DeliveryLocationName=dl.Name,m.Height,m.[Length],m.[Weight],m.MaterialGroupId,MaterialName=m.Name,m.PartyId,m.PerDayConsumption,m.PkgUnitId,PkgUnitName=pum.UnitName,UnitName=um.UnitName,m.QtyPerPkg,m.UnitId,WarehouseLocation=wl.Name,m.WarehouseLocationId
FROM mMaterialMaster m LEFT JOIN mGenericMaster dl on m.DeliveryLocationId=dl.Id
LEFT JOIN mUnitMaster um on m.UnitId=um.Id
LEFT JOIN mUnitMaster pum on m.PkgUnitId=pum.Id
LEFT JOIN mGenericMaster wl on m.WarehouseLocationId=wl.Id
WHERE m.StatusId=0

/*Table17=Contracts*/
SELECT cc.Id,cc.[Name]
FROM mRateContract cc

/*Table18=LoadTypes*/
SELECT Id,[Name],Code,RateCriteriaId
FROM mLoadType l
/*Table19=TaxMasters*/
SELECT Id,Code,Description,TaxTypeId
FROM mTaxServiceType t where t.StatusId=0
/*Table20=FileUploadNatures*/
SELECT Id,TypeId,Name,AllowedExtensions,Code,MaxFileSize,MaxFilesPerRecord
FROM mFileUploadNature fl where [Status]=0
/*Table21=GPSEndPoints*/
SELECT Id,AcceptEncoding,[Authorization],ContentEncoding,ContentType,IsParameterInArray,Method,ParameterMapping,ParameterTemplate,ResultMapping,ServiceTypeId,SuccessCode,Url,VendorId
FROM mGpsEndPoint gps
/*Table22=DueTypes*/
SELECT dm.Id,dm.Name,dm.Abbr,dm.DueTypeId,DueType=dt.ConstantName,dm.DueAccountId,DueAccount=da.AccountName,dm.PayableAccountId,PayableAccount=pa.AccountName
FROM mDueMaster dm LEFT JOIN mLedger da on dm.DueAccountId=da.Id
LEFT JOIN mLedger pa on dm.PayableAccountId=pa.Id
LEFT JOIN mConstantValue dt on dm.DueTypeId=dt.Id
where dm.StatusId=0
END'
END
