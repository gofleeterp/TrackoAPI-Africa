IF(EXISTS(SELECT 1 FROM dbo.mConstantType WHERE Id in(122,124,125,126,127)))
BEGIN
DELETE FROM dbo.mConstantType WHERE Id in(122,124,125,126,127)
UPDATE dbo.mConstantValue SET ConstantAbbr='Alert',ConstantName='Alert' WHERE Id=1507
UPDATE dbo.mConstantValue SET ConstantAbbr='ApiJob',ConstantName='ApiJob' WHERE Id=1508
UPDATE dbo.mConstantValue SET ConstantAbbr='SqlJob',ConstantName='SqlJob' WHERE Id=1509
DELETE FROM dbo.mConstantValue WHERE Id in(1510,1511)
UPDATE mJob SET JobNatureId=1507 WHERE JobNatureId IS NULL
END
GO
IF(NOT EXISTS(SELECT 1 FROM dbo.mConstantValue WHERE Id=1510))
BEGIN
INSERT INTO dbo.mConstantValue(Id,ConstantAbbr,ConstantName,ConstantRemarks,ConstantTypeId,[Visiblity],[IsDepricated])
SELECT 1510,'Half Month','Half Month','IntervalType (1-15 or 16-31)',121,1,0
END
ELSE IF(NOT EXISTS(SELECT 1 FROM dbo.mConstantValue WHERE Id=1510 AND ConstantTypeId=121))
BEGIN
UPDATE dbo.mConstantValue SET ConstantAbbr='Half Month',ConstantName='Half Month',ConstantRemarks='IntervalType (1-15 or 16-31)',ConstantTypeId=121 WHERE Id=1510
END