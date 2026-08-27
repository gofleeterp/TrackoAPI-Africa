using Repository.DatabaseCLR.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TinyJson;
using TrackoApi.Service.Global;

using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace GenerateMigrationScript
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.btnGenCommand.Click += BtnGenCommand_Click;
            this.GenMig.CheckStateChanged += GenMig_CheckStateChanged;
        }

        private void GenMig_CheckStateChanged(object sender, EventArgs e)
        {
            if (this.GenMig.Checked){
                this.txtConnectionString.Text = "Server=103.205.127.190,20561;Database=unicorn;User ID=iwltsa;Password=Meru@gofleet#;Trusted_Connection=False;Encrypt=False;Connection Timeout=30;";
            }
            else
            {
                this.txtConnectionString.Text = "";
            }
        }

        private async void BtnGenCommand_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.GenMig.Checked)
                {
                    if (!string.IsNullOrWhiteSpace(this.txtConnectionString.Text))
                    {
                        this.txtSqlScript.Text = TrackoApi.Data.GenerateMigrationScript.GenerateSqlScript(this.txtConnectionString.Text);
                    }
                }
                else if (this.btnBajajCall.Checked)
                {
                    var client = new BajajClientService();
                    var res = await client.GetTransportersDataOutAsync();
                    if(res!=null){
                        using (ExcelPackage pck = new ExcelPackage())
                        {
                            var wb = pck.Workbook;
                            if (res.ZSD_DTAG_STG != null)
                            {
                                ToExcelWorkSheet(res.ZSD_DTAG_STG, wb);
                            }
                            if (res.ZSD_LR_DET_STG != null)
                            {
                                ToExcelWorkSheet(res.ZSD_LR_DET_STG, wb);
                            }
                            if (res.ZSD_LR_HDR_DTL != null)
                            {
                                ToExcelWorkSheet(res.ZSD_LR_HDR_DTL, wb);
                            }
                            if (res.ZSD_DAM_DTL_STG != null)
                            {
                                ToExcelWorkSheet(res.ZSD_DAM_DTL_STG, wb);
                            }
                            if (res.ZSD_DEL_DET_STG != null)
                            {
                                ToExcelWorkSheet(res.ZSD_DEL_DET_STG, wb);
                            }
                            if (res.ZSD_FRT_DTL_STG != null)
                            {
                                ToExcelWorkSheet(res.ZSD_FRT_DTL_STG, wb);
                            }
                            if (res.ZSD_SRT_DTL_STG != null)
                            {
                                ToExcelWorkSheet(res.ZSD_SRT_DTL_STG, wb);
                            }
                            var filepath=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,$"Bajaj_Data_{DateTime.Now:dd_MMM_yy_HH_mm_ss}.xlsx");
                            pck.SaveAs(new FileInfo(filepath));
                            txtSqlScript.Text = filepath;
                        }
                    }
                }
                else
                {
                    var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "GoFleetCLR");
                    //if (string.IsNullOrWhiteSpace(txtConnectionString.Text))
                    //{
                    //    MessageBox.Show("Connection String is required");
                    //    return;
                    //}
                    txtSqlScript.Text = @"DECLARE @Command VARCHAR(MAX) = 'EXEC sp_changedbowner ''<<LoginName>>''
ALTER DATABASE[<< DatabaseName >>] SET trustworthy ON' 

SELECT @Command = REPLACE(REPLACE(@Command
            , '<<DatabaseName>>', SD.Name)
            , '<<LoginName>>', SL.Name)
FROM master..sysdatabases SD
JOIN master..syslogins SL ON  SD.SID = SL.SID
WHERE SD.Name = DB_NAME()

PRINT @Command
EXEC(@Command)";
                    txtSqlScript.Text += SqlClrLoader.GetDropAssemblyAndFunctionsStatement(assembly);
                    txtSqlScript.Text += $"{SqlClrLoader.GetCreateAssemblyAndFunctionsStatement(assembly, PermissionSet.Unsafe)}";
                }
            }catch (Exception ex)
            {
                txtSqlScript.Text = ex.GetBaseException().Message;
            }
            
        }

        private void ToExcelWorkSheet<T>(T[] list,ExcelWorkbook book)
        {
            ExcelWorksheet ws =book.Worksheets.Add(typeof(T).GetTypeInfo().Name.Replace("TransportersResponseData",""));
            using (var drange = ws.Cells["A1"].LoadFromCollection(list, true, TableStyles.Light13))
            {
                //var tbl = ws.Tables.Add(drange, "report_data_range");
                //tbl.ShowFilter = true;
                int colNumber = 1;
                foreach (PropertyInfo pi in typeof(T).GetProperties())
                {
                    if (pi.PropertyType == typeof(DateTime?) || pi.PropertyType == typeof(DateTime))
                    {
                        ws.Column(colNumber).Style.Numberformat.Format = "dd-MMM-yyyy HH:mm";
                    }
                    colNumber++;
                }
                drange.Calculate();
                drange.AutoFitColumns();
            }
        }

        private void btnGenCommand_Click_1(object sender, EventArgs e)
        {

        }
    }
}
