using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.Models.Shared
{
    public enum AclType
    {
        Entity = 0,
        Form = 1,
        Field = 2,
        Report = 3,
        Action = 4,
        Office = 5,
        GenericMaster = 6,
        UserDefinedReport = 7,
        UserControl = 8,
        MobileView = 9,
        WebView = 10,
        AccontRole = 11,
        AccountGroup = 12,
        Vehicle = 13
    }
    public enum MasterStatus
    {
        Active = 0,
        Suspended = 1,
        Deleted = 2,
        Default=0,
        BlackListed=3
    }

    public enum DrCr
    {
        Debit=1,
        Credit=2,
        Default=1
    }

    public enum ReportParameterType
    {
        AutoComplete = 1,
        ListBox = 2,
        Integer = 3,
        Decimal = 4,
        String = 5,
        DateTime = 6,
        Boolean=7
    }

    public enum VTSTriplogDate
    {
        SentforLoadingDate = 1,
        ReportingAtLoadingPointDate=2,
        LoadingStartDate = 3,//Critical
        LoadingEndDate = 4,
        ReachDestinationDate = 5,
        UnloadingStartDate = 6,
        UnloadingEndDate = 7,//Critical
    }
    public enum FinanceStatus
    {
        NA = 0,
        DirectImport = 1,
        ApprovalRequired = 2
    }

    public enum JobResult
    {
        Success=0,
        Failed=1,
        Running=2,
        ReadyForProcess=3,
        Pending=4
    }

    public enum DocType
    {
        None=0,
        Word=1,
        Excel=2,
        PPT=3,
        PDF=4,
        InlineHtml=4
    }

    public enum NotificationType
    {
        None=0,
        Email=1,
        SMS=2,
        WebHook=3,
        WhatsApp=4,
        VoiceMessage=5,
        Broadcast=6,
        BackgroundJob=7
    }
    public enum PurchaseType
    {
        FreeThreshold = 0,
        Paid=1,
        Trial=2
    }
    public enum LogType
    {
        None = 0,
        ErrorOnly = 1,
        All = 2,
        AllButNot404 = 3,
        AllButNot401 = 4,
        NotAllBut401 = 5,
        ErrorExcept404 = 6
    }
    public enum ApplicationCategory
    {
        JavaScript = 0,
        NativeConfidential = 1,
        NativeMobileApp = 2
    }
}
