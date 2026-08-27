using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exceptionless;
using Microsoft.Graph;
using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.Authentication;
using Newtonsoft.Json;

namespace TrackoApi.Core.Helpers
{
    public class OneDriveAuthenticationProvider : MsaAuthenticationProvider {
        public OneDriveAuthenticationProvider(string clientId, string returnUrl, string[] scopes) : base(clientId, returnUrl, scopes)
        {
        }

        public OneDriveAuthenticationProvider(string clientId, string returnUrl, string[] scopes, ICredentialVault credentialVault) : base(clientId, returnUrl, scopes, credentialVault)
        {
        }

        public OneDriveAuthenticationProvider(string clientId, string clientSecret, string returnUrl, string[] scopes, CredentialCache credentialCache, ICredentialVault credentialVault) : base(clientId, clientSecret, returnUrl, scopes, credentialCache, credentialVault)
        {
        }

        public OneDriveAuthenticationProvider(string clientId, string clientSecret, string returnUrl, string[] scopes, CredentialCache credentialCache) : base(clientId, clientSecret, returnUrl, scopes, credentialCache)
        {
        }
    }
    public class OneDriverHelper
    {
        private static OneDriverHelper _oneDriverHelper;
        public static OneDriverHelper Instance => _oneDriverHelper ?? new OneDriverHelper();
        public const string OAuthDesktopEndPoint = "https://login.live.com/oauth20_desktop.srf";

        public IOneDriveClient OneDriveClient { get; set; }

        protected OneDriverHelper()
        {
            BuildClient().Wait();
        }

        private async Task BuildClient()
        {
            const string msa_client_id = "ee76c3ce-e337-436d-a54c-77114b77678e";
            var offers = new string[] { "onedrive.readwrite", "wl.signin" };

            //if (OneDriveClient == null)
            //{

            //    var authProvider = new MsaAuthenticationProvider(msa_client_id, OAuthDesktopEndPoint, offers);
            //    OneDriveClient = new OneDriveClient(authProvider);
            //    await authProvider.AuthenticateUserAsync("iwltbdbackup@hotmail.com");
            //}
            var msaAuthProvider = new OneDriveAuthenticationProvider(msa_client_id, "AF49C4C036D784413EB7B6A986BD7A6A19F71E34", OAuthDesktopEndPoint, offers,/*CredentialCache*/ null, new CredentialVault(msa_client_id));
            await msaAuthProvider.RestoreMostRecentFromCacheOrAuthenticateUserAsync();
            OneDriveClient = new OneDriveClient("https://api.onedrive.com/v1.0", msaAuthProvider);
        }
        public async Task<Item> LoadFolderFromId(string id)
        {
            if (null == this.OneDriveClient) return null;
            try
            {
                return await this.OneDriveClient.Drive.Items[id].Request().GetAsync();
            }
            catch (Exception exception)
            {
                exception.ToExceptionless().Submit();
                return null;
            }
        }

        public async Task<Item> LoadFolderFromPath(string path = null)
        {
            if (null == this.OneDriveClient) return null;
            try
            {
                Item folder;
                if (path == null)
                {
                    folder = await this.OneDriveClient.Drive.Root.Request().GetAsync();
                }
                else
                {
                    folder =
                        await
                            this.OneDriveClient.Drive.Root.ItemWithPath("/" + path)
                                .Request()
                                .GetAsync();
                }
                return folder;
            }
            catch (Exception exception)
            {
                exception.ToExceptionless().Submit();
                return null;
            }
        }
        private System.IO.Stream GetFileStreamForUpload(string fullFileName)
        {
            
            try
            {
                return new System.IO.FileStream(fullFileName, System.IO.FileMode.Open);
            }
            catch (Exception ex)
            {ex.ToExceptionless().AddObject(fullFileName).Submit();
                return null;
            }
        }
        public async Task<Item> UploadFileByTargetFolderId(string localFuleFileName, string targetFolderId)
        {
            var folder = await LoadFolderFromId(targetFolderId);
            return await UploadFile(localFuleFileName, folder);
        }
        public async Task<Item> UploadFileByTargetFolderPath(string localFuleFileName, string targetFolderPath)
        {
            var folder =await LoadFolderFromPath(targetFolderPath);
            return await UploadFile(localFuleFileName, folder);
        }
        public async Task<Item> UploadFile(string localFuleFileName,Item targetFolder)
        {
            using (var stream = GetFileStreamForUpload(localFuleFileName))
            {
                Item uploadedItem=null;
                if (stream != null)
                {
                    string folderPath = targetFolder.ParentReference == null
                        ? "/drive/items/root:"
                        : targetFolder.ParentReference.Path + "/" + Uri.EscapeUriString(targetFolder.Name);
                    var uploadPath = folderPath + "/" + Uri.EscapeUriString(System.IO.Path.GetFileName(localFuleFileName));

                    try
                    {
                        uploadedItem =
                            await
                                this.OneDriveClient.ItemWithPath(uploadPath).Content.Request().PutAsync<Item>(stream);

                        
                        //MessageBox.Show("Uploaded with ID: " + uploadedItem.Id);
                    }
                    catch (Exception exception)
                    {
                        exception.ToExceptionless().AddObject(localFuleFileName).AddObject(targetFolder).Submit();
                    }
                }
                return uploadedItem;
            }
        }

        public async Task<Item> CreateFolderByTargetFolderPath(string targetPath, string newFolderName)
        {
            Item targetFolder;
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                targetFolder = await this.OneDriveClient.Drive.Root.Request().GetAsync();
            }
            else
            {
                targetFolder = await LoadFolderFromPath(targetPath);
            }
            
            if (targetFolder == null) return null;
            return await CreateFolderByTargetFolderId(targetFolder.Id, newFolderName);
        }
        public async Task<Item> CreateFolderByTargetFolderId(string targetFolderId,string newFolderName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(targetFolderId)) return null;
                
                var folderToCreate = new Item {Name = newFolderName, Folder = new Folder()};
                var newFolder =
                        await this.OneDriveClient.Drive.Items[targetFolderId].Children.Request()
                            .AddAsync(folderToCreate);
                return newFolder;
            }
            catch(Exception exception)
            {
                exception.ToExceptionless().AddObject(new
                {
                    TargetFolderId= targetFolderId,
                    NewFolderToCreate= newFolderName
                }).Submit();
                return null;
            }
        }
    }
}
