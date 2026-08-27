using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace TrackoAPI.TSVS
{
    public class TSVSClient
    {
        readonly string _uri;
        readonly string _personalAccessToken;
        readonly string _project;
        /// <summary>
        /// Constructor. values to match organization. 
        /// </summary>
        public TSVSClient()
        {
            _uri = "https://iwlt.visualstudio.com";
            _personalAccessToken = "b6xjhe4s3fvffwuzq4s47ruzdzez6znctnusmuhk7uaqnvw22zdq";
            _project = "GOF.ClientUI";
        }

        public async Task<WorkItem> CreateWorkItem(IDictionary<string, object> fields, string type = "Bug", string projectName = null)
        {
            Uri uri = new Uri(_uri);
            string personalAccessToken = _personalAccessToken;
            string project = string.IsNullOrWhiteSpace(projectName) ? _project : projectName;
            VssBasicCredential credentials = new VssBasicCredential("", personalAccessToken);
            JsonPatchDocument patchDocument = new JsonPatchDocument();
            foreach (var field in fields)
            {
                if (string.IsNullOrWhiteSpace(field.Key)) continue;
                patchDocument.Add(
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = $"/fields/{field.Key}",
                        Value = field.Value?.ToString()
                    }
                );
            }
            VssConnection connection = new VssConnection(uri, credentials);
            WorkItemTrackingHttpClient workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>();

            try
            {
                WorkItem result = await workItemTrackingHttpClient.CreateWorkItemAsync(patchDocument, project, type);
#if DEBUG
                Debug.WriteLine("Bug Successfully Created: Bug #{0}", result.Id);
#endif
                return result;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("Error creating bug: {0}", ex.InnerException.Message);
#endif
                return null;
            }
        }
        public async Task<List<WorkItem>> GetWorkItems(string query,List<string> fields=null, string projectName = null)
        {
            Uri uri = new Uri(_uri);
            string personalAccessToken = _personalAccessToken;
            string project = string.IsNullOrWhiteSpace(projectName) ? _project : projectName;
            VssBasicCredential credentials = new VssBasicCredential("", personalAccessToken);
            Wiql wiql = new Wiql()
            {
                Query = query
            };

            try
            {
                //create instance of work item tracking http client
                using (WorkItemTrackingHttpClient workItemTrackingHttpClient = new WorkItemTrackingHttpClient(uri, credentials))
                {
                    //execute the query to get the list of work items in the results
                    WorkItemQueryResult workItemQueryResult = workItemTrackingHttpClient.QueryByWiqlAsync(wiql).Result;

                    //some error handling                
                    if (workItemQueryResult.WorkItems.Count() != 0)
                    {
                        //need to get the list of our work item ids and put them into an array
                        List<int> list = new List<int>();
                        foreach (var item in workItemQueryResult.WorkItems)
                        {
                            list.Add(item.Id);
                        }
                        int[] arr = list.ToArray();

                        

                        //get work items for the ids found in query
                        var workItems = workItemTrackingHttpClient.GetWorkItemsAsync(arr, fields, workItemQueryResult.AsOf).Result;

                        Debug.WriteLine("Query Results: {0} items found", workItems.Count);

                        //loop though work items and write to console
#if DEBUG
                        foreach (var workItem in workItems)
                        {
                            Debug.WriteLine("{0}          {1}                     {2}", workItem.Id, workItem.Fields["System.Title"], workItem.Fields["System.State"]);
                        }
#endif
                        return workItems;
                    }

                    return new List<WorkItem>();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("Error creating bug: {0}", ex.InnerException.Message);
#endif
                throw;
            }
        }
    }

    public class WorkItemQuery
    {
        [Required,MinLength(10)]
        public string Query { get; set; }
        public List<string> Fields { get; set; }
    }
}