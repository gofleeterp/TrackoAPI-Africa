using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RestSharp;

namespace GenerateMigrationScript
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(this.textBox1.Text)) return;
                //var filterPath = "$.result";
                //var jt = JToken.Parse(this.textBox1.Text);
                //JToken acme = jt.SelectToken(filterPath);
                //this.textBox1.Text = acme.ToString();
                var method = (RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), "POST");
                var client = new RestClient();
                client.Timeout = -1;
                var request = new RestRequest("https://gsp.adaequare.com/enriched/ewb/ewayapi?action=EXTENDVALIDITY", method);
                request.AddHeader("gstin", "06AADFA1447L1ZL");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("username", "autocarrie_API_gof");
                request.AddHeader("requestid", "3D80A44788-4183-5855A1-F5EF3B5B1");
                request.AddHeader("password", "gofleet@iwlt");
                request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzY29wZSI6WyJnc3AiXSwiZXhwIjoxNjM5MDc0NjAwLCJhdXRob3JpdGllcyI6WyJST0xFX1BST0RfRV9BUElfRUkiLCJST0xFX1BST0RfRV9BUElfRVdCIl0sImp0aSI6Ijc2ZmM4ZTIzLTRmZGItNGZkMC05YjE4LWI3Y2U2MGI5MjhkZSIsImNsaWVudF9pZCI6Ijc3OTIxNUJFRTA4RjQyN0U5RDBGMkU4QjM1MTc3NzBCIn0.WO_W7PfcTl8Phb9KnWdLxRNkvX-9m2ktCWPfRiowW9U");
                var body = @"{      ""ewbNo"": 871193512294,      ""vehicleNo"": ""NL01AA8122"",      ""fromPlace"": ""JALPAIGURI"",      ""fromState"": ""18"",      ""remainingDistance"": 0,      ""transDocNo"": ""SAU1003102"",      ""transDocDate"": ""04/12/2021"",      ""transMode"": ""1"",      ""fromPincode"": 781001,      ""consignmentStatus"": ""M"",      ""extnRsnCode"": 2,      ""extnRemarks"": ""Extend"",      ""transitType"": """"  }";
                var newbody = "{\r\n    \"ewbNo\": 871193512294,\r\n    \"vehicleNo\": \"NL01AA8122\",\r\n    \"fromPlace\": \"JALPAIGURI\",\r\n    \"fromState\": \"19\",\r\n    \"remainingDistance\": 0,\r\n    \"transDocNo\": \"SAU1003102\",\r\n    \"transDocDate\": \"04/12/2021\",\r\n    \"transMode\": \"1\",\r\n    \"fromPincode\": 781001,\r\n    \"consignmentStatus\": \"M\",\r\n    \"extnRsnCode\": 2,\r\n    \"extnRemarks\": \"Extend\",\r\n    \"transitType\": \"\"\r\n}";
                request.AddParameter("application/json", newbody, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                this.textBox1.Text=response.Content;
            }
            catch
            {
                //Ignore
            }
        }
        
    }
}
