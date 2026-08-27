IF OBJECT_ID (N'vwVDRBalance', N'U') IS NOT NULL
BEGIN
	EXEC sp_rename 'vwVDRBalance','vwVDRBalance.old'	
END
GO
IF OBJECT_ID (N'vwVDRBalance', N'V') IS NULL
BEGIN
	EXEC sp_sqlexec N'
	CREATE VIEW [dbo].[vwVDRBalance]
	AS
	SELECT
	  v.Id
	 ,v.VDId
	 ,vv.VoucherDate
	 ,DueDate=DATEADD(DAY,ISNULL(l.CreditPeriod,0),vv.VoucherDate)
	 ,v.ReferenceNo
	 ,v.VDRTypeID
	 ,v.OriginalRefId
	 ,v.VDRAmount
	 ,v.TransactionId
	 ,AccountId=ISNULL(v.AccountId,vd.AccountId)
	 ,v.CSID
	 ,v.CDOE
	 ,CreditDays = ISNULL(l.CreditPeriod,0)
	 ,l.CreditNatureId
	 ,PreviousPaid = ISNULL(S.Amount, 0)
	 ,Balance = (ISNULL(VDRAmount, 0) + ISNULL(S.Amount, 0))
	 ,RefType = (CASE v.VDRTypeID
		WHEN 1013 THEN ''NewRef''
		WHEN 1448 THEN ''OnAccount''
		WHEN 1449 THEN ''Advance''
		ELSE ''''
	  END)
	 ,AccountName = l.AccountName
	 ,vd.VoucherId

	FROM [dbo].[tVoucherVDR] V
	LEFT JOIN (SELECT
		Amount = SUM(VDRAmount)
	   ,RefID
	  FROM [dbo].[tVoucherVDR]
	  GROUP BY RefID) S
	  ON V.Id = S.RefID

	LEFT JOIN tVoucherVD vd on vd.Id=v.VDId
	LEFT JOIN tVouchers vv on vd.VoucherId=vv.Id
	LEFT JOIN mLedger l
	  ON ISNULL(v.AccountId,vd.AccountId) = l.Id
	WHERE (ISNULL(VDRAmount, 0) + ISNULL(S.Amount, 0)) <> 0
	AND v.VDRTypeID IN (1013, 1448, 1449)
	'
END