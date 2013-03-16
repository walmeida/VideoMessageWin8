using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Media.Imaging;
using System.Text;
using Windows.Media;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Windows.Storage.Streams;
using System.Runtime.Serialization;
using Microsoft.WindowsAzure.MobileServices;
using modelo;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace VideoMessage
{

    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : VideoMessage.Common.LayoutAwarePage
    {
        private MediaCapture m_mediaCaptureMgr;
        private bool m_bRecording;
        private readonly String VIDEO_FILE_NAME = "video.mp4";
        private Windows.Storage.StorageFile m_recordStorageFile;
        String accessToken;
        String accessPolicyId;
        String assetId;
        String mediaProcessorId;
        String jobId;
        String pathUpload;
        String downloadAccessPolicyId;
        String pathDownload;
        String urlAtualDaApi;
        String urlInicialDaApi = "https://media.windows.net/API/";
        HttpClientHandler handler;
        HttpClient httpClient;


        private MediaExtensionManager extensions = new MediaExtensionManager();
        
        public MainPage()
        {
            this.InitializeComponent();
            startDeviceAndPreview();

            adicionaSuporteAoSmoothStreaming();
        }

        private void adicionaSuporteAoSmoothStreaming()
        {
            extensions.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".ism", "text/xml");
            extensions.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".ism", "application/vnd.ms-sstr+xml");
        }

        private void inicializaElementosHttp()
        {
            handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=verbose"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            urlAtualDaApi = "";
        }

        private async void startDeviceAndPreview()
        {
            try
            {
                m_mediaCaptureMgr = new MediaCapture();
                await m_mediaCaptureMgr.InitializeAsync();
                //m_mediaCaptureMgr.RecordLimitationExceeded += new Windows.Media.Capture.RecordLimitationExceededEventHandler(RecordLimitationExceeded); ;
                //m_mediaCaptureMgr.Failed += new Windows.Media.Capture.MediaCaptureFailedEventHandler(Failed); ;

                previewCanvas1.Visibility = Windows.UI.Xaml.Visibility.Visible;
                previewElement1.Source = m_mediaCaptureMgr;
                await m_mediaCaptureMgr.StartPreviewAsync();

                m_bRecording = false;
                btnStartStopRecord.IsEnabled = true;

            }
            catch (Exception exception)
            {
                
            }
        }

        internal async void btnStartStopRecord_Click(Object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                String fileName;
                btnStartStopRecord.IsEnabled = false;

                if (!m_bRecording)
                {

                    fileName = VIDEO_FILE_NAME;

                    m_recordStorageFile = await Windows.Storage.KnownFolders.VideosLibrary.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);

                    MediaEncodingProfile recordProfile = null;
                    recordProfile = MediaEncodingProfile.CreateMp4(Windows.Media.MediaProperties.VideoEncodingQuality.Auto);

                    await m_mediaCaptureMgr.StartRecordToStorageFileAsync(recordProfile, m_recordStorageFile);
                    m_bRecording = true;
                    btnStartStopRecord.Content = "Parar";
                    btnStartStopRecord.IsEnabled = true;

                }
                else
                {
                    //ShowStatusMessage("Stopping Record");

                    await m_mediaCaptureMgr.StopRecordAsync();

                    m_bRecording = false;
                    btnStartStopRecord.IsEnabled = true;
                    btnStartStopRecord.Content = "Gravar";

                    btnSelectFriend.IsEnabled = true;

                    /*if (!m_bSuspended)
                    {
                        var stream = await m_recordStorageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);

                        ShowStatusMessage("Record file opened");
                        ShowStatusMessage(this.m_recordStorageFile.Path);
                        playbackElement1.AutoPlay = true;
                        playbackElement1.SetSource(stream, this.m_recordStorageFile.FileType);
                        playbackElement1.Play();


                    }*/

                }
            }
            catch (Exception ex)
            {
                
            }

        }

        private async void PickAContactButton_Click(object sender, RoutedEventArgs e)
        {
            
            var contactPicker = new Windows.ApplicationModel.Contacts.ContactPicker();
            contactPicker.CommitButtonText = "Selecionar";
            Windows.ApplicationModel.Contacts.ContactInformation contact = await contactPicker.PickSingleContactAsync();

            if (contact != null)
            {
                //OutputFields.Visibility = Visibility.Visible;
                //OutputEmpty.Visibility = Visibility.Collapsed;

                OutputName.Text = contact.Name;
                AppendContactFieldValues(OutputEmailHeader, OutputEmails, contact.Emails);

                Windows.Storage.Streams.IRandomAccessStreamWithContentType stream = await contact.GetThumbnailAsync();
                if (stream != null && stream.Size > 0)
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.SetSource(stream);
                    OutputThumbnail.Source = bitmap;
                }
                else
                {
                    OutputThumbnail.Source = null;
                }

                btnSend.IsEnabled = true;
            }
            else
            {
                OutputFields.Visibility = Visibility.Collapsed;
                OutputThumbnail.Source = null;
            }
            
        }

        private void AppendContactFieldValues<T>(TextBlock header, TextBlock content, IReadOnlyCollection<T> fields)
        {
            if (fields.Count > 0)
            {
                StringBuilder output = new StringBuilder();
                foreach (Windows.ApplicationModel.Contacts.IContactField field in fields)
                {
                    output.AppendFormat("{0} ({1})\n", field.Value, field.Category);
                }
                header.Visibility = Visibility.Visible;
                content.Visibility = Visibility.Visible;
                content.Text = output.ToString();
            }
            else
            {
                header.Visibility = Visibility.Collapsed;
                content.Visibility = Visibility.Collapsed;
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSend.Content = "Enviando...";
                btnSend.IsEnabled = false;
                HttpClient httpClient = new HttpClient();
                FormUrlEncodedContent form = new FormUrlEncodedContent(new System.Collections.Generic.Dictionary<string, string> { { "grant_type", "client_credentials" }, { "client_id", "videomessagems" }, { "client_secret", "+yaQ3dn0uZ/8wHHFYtAkVp9XiabClBHd5IGJwf2g2io=" }, { "scope", "urn:WindowsAzureMediaServices" } });
                HttpResponseMessage response = await httpClient.PostAsync("https://wamsprodglobal001acs.accesscontrol.windows.net/v2/OAuth2-13", form);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    String responseBodyAsText = await response.Content.ReadAsStringAsync();
                    JObject jsonObj = JObject.Parse(responseBodyAsText);

                    accessToken = (String) jsonObj["access_token"];
                    inicializaElementosHttp();
                    criarAssetCall();
                }
            }
            catch (HttpRequestException hre)
            {
                
            }
            catch (TaskCanceledException)
            {
                
            }
        }

        private async void criarAssetCall()
        {
            String strContent = "{'Name': 'NovoAssetVideoMessage'}";
            
            HttpRequestMessage req = criaRequest(HttpMethod.Post ,urlInicialDaApi + "Assets", strContent);
            HttpResponseMessage response = await httpClient.SendAsync(req);

            if (response.StatusCode != HttpStatusCode.MovedPermanently)
            {
                //Erro
            }
            
            urlAtualDaApi = response.Headers.Location.ToString();
            String urlFinal = urlAtualDaApi + "Assets";
            req = criaRequest(HttpMethod.Post, urlFinal, strContent);
            response = await httpClient.SendAsync(req);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                //Sucesso
                String responseBodyAsText = await response.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(responseBodyAsText);

                //POG Nervosa para obter o id
                assetId = Uri.UnescapeDataString(((String)jsonObj["d"]["__metadata"]["id"]).Replace(urlFinal + "('", "").Replace("')", ""));
                criaAccessPolicy();
            }
            else
            {
                //Erro
            }
        }

        private async void criaAccessPolicy()
        {
            String strContent = "{'Name': 'NewUploadPolicy', 'DurationInMinutes' : '300', 'Permissions' : 2 }";
            String urlFinal = urlAtualDaApi + "AccessPolicies";
            HttpRequestMessage req = criaRequest(HttpMethod.Post, urlFinal, strContent);
            HttpResponseMessage response = await httpClient.SendAsync(req);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                //Sucesso
                String responseBodyAsText = await response.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(responseBodyAsText);

                //POG Nervosa para obter o id
                accessPolicyId = Uri.UnescapeDataString(((String)jsonObj["d"]["__metadata"]["id"]).Replace(urlFinal + "('", "").Replace("')", ""));
                getUrlUpload();
            }
        }

        private async void getUrlUpload()
        {
            String strContent = "{'AccessPolicyId': '" + accessPolicyId + "', 'AssetId' : '" + assetId + "', 'StartTime' : '" + String.Format("{0:M/d/yyyy h:mm:ss tt}",DateTime.Now.AddMinutes(-5)) + "', 'Type' : 1 }";
            String urlFinal = urlAtualDaApi + "Locators";
            HttpRequestMessage req = criaRequest(HttpMethod.Post, urlFinal, strContent);
            HttpResponseMessage response = await httpClient.SendAsync(req);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                //Sucesso
                String responseBodyAsText = await response.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(responseBodyAsText);

                //POG Nervosa para obter o id
                String strBrutaPath = (String) jsonObj["d"]["Path"];
                String[] arrPath = strBrutaPath.Split('?');
                pathUpload = arrPath[0] + "/video.mp4?" + arrPath[1];
                uploadVideo();
            }
        }

        private async void uploadVideo()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            HttpClient httpClient = new HttpClient(handler);
            
            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await m_recordStorageFile.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }

            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, pathUpload);
            req.Content =  new ByteArrayContent(fileBytes);
            req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            req.Content.Headers.Add("x-ms-version", "2011-08-18");
            req.Content.Headers.Add("x-ms-date", String.Format("{0:yyyy-MM-dd}",DateTime.UtcNow));
            req.Content.Headers.Add("x-ms-blob-type", "BlockBlob");
                        
            HttpResponseMessage response = await httpClient.SendAsync(req);

            String responseBodyAsText = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Created)
            {
                //Sucesso
                //criaDownloadAccessPolicy();
                //encondingJob();
                generateFileEntity();
            }
        }

        private async void criaDownloadAccessPolicy()
        {
            String strContent = "{'Name': 'DownloadPolicy', 'DurationInMinutes' : '300', 'Permissions' : 1 }";
            String urlFinal = urlAtualDaApi + "AccessPolicies";
            HttpRequestMessage req = criaRequest(HttpMethod.Post, urlFinal, strContent);
            HttpResponseMessage response = await httpClient.SendAsync(req);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                //Sucesso
                String responseBodyAsText = await response.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(responseBodyAsText);

                //POG Nervosa para obter o id
                downloadAccessPolicyId = Uri.UnescapeDataString(((String)jsonObj["d"]["__metadata"]["id"]).Replace(urlFinal + "('", "").Replace("')", ""));

                getUrlDownload();
            }
        }

        private async void getUrlDownload()
        {
            String strContent = "{'AccessPolicyId': '" + downloadAccessPolicyId + "', 'AssetId' : '" + assetId + "', 'StartTime' : '" + String.Format("{0:M/d/yyyy h:mm:ss tt}", DateTime.Now.AddMinutes(-5)) + "', 'Type' : 2 }";
            String urlFinal = urlAtualDaApi + "Locators";
            HttpRequestMessage req = criaRequest(HttpMethod.Post, urlFinal, strContent);
            HttpResponseMessage response = await httpClient.SendAsync(req);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                //Sucesso
                String responseBodyAsText = await response.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(responseBodyAsText);

                //POG Nervosa para obter o id
                String strBrutaPath = (String)jsonObj["d"]["Path"];
                //String[] arrPath = strBrutaPath.Split('?');
                pathDownload = strBrutaPath + "video.ism/manifest";
                btnSend.Content = "Video Enviado";
                btnPlayMensagem.IsEnabled = true;
                //downloadVideo();

                var nome = await Windows.System.UserProfile.UserInformation.GetDisplayNameAsync();
                var todoItem = new TodoItem { Text = "Nova mensagem de " + nome, Destinatario = App.dest};
                InsertTodoItem(todoItem);

            }
        }

        private async void generateFileEntity()
        {
            String strContent = "";
            String urlFinal = urlAtualDaApi + "CreateFileInfos?assetid='" + Uri.EscapeDataString(assetId) + "'";
            //HttpRequestMessage req = criaRequest(HttpMethod.Get, urlFinal, strContent);
            //req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=verbose");
            //req.Content.Headers.Add("DataServiceVersion", "3.0");
            //req.Content.Headers.Add("MaxDataServiceVersion", "3.0");
            //req.Content.Headers.Add("x-ms-version", "2.0");

            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, urlFinal);
            //req.Content = new StringContent(
            //req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=verbose");
            //req.Content.Headers.Add("DataServiceVersion", "3.0");
            //req.Content.Headers.Add("MaxDataServiceVersion", "3.0");
            //req.Content.Headers.Add("x-ms-version", "2.0");
            req.Headers.Add("DataServiceVersion", "3.0");
            req.Headers.Add("MaxDataServiceVersion", "3.0");
            req.Headers.Add("x-ms-version", "2.0");

            HttpResponseMessage response = await httpClient.SendAsync(req);

            String responseBodyAsText = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                //Sucesso
                encondingJob();
            }
        }

        private async void encondingJob()
        {
            String strContent = "{'Name' : 'NewTestJob', 'InputMediaAssets' : [{'__metadata' : {'uri' : \"https://media.windows.net/api/Assets('" + assetId +"')\"}}],  'Tasks' : [{'Configuration' : 'H264 Smooth Streaming SD 4x3', 'MediaProcessorId' : 'nb:mpid:UUID:A2F9AFE9-7146-4882-A5F7-DA4A85E06A93',  'TaskBody' : '<?xml version=\"1.0\" encoding=\"utf-8\"?><taskBody><inputAsset>JobInputAsset(0)</inputAsset><outputAsset>JobOutputAsset(0)</outputAsset></taskBody>'}]}";
            //String strContent = "{'Name' : 'NewTestJob', 'InputMediaAssets' : [{'__metadata' : {'uri' : \"https://media.windows.net/api/Assets('" + assetId + "')\"}}]  }";
            String urlFinal = urlAtualDaApi + "Jobs";
            HttpRequestMessage req = criaRequest(HttpMethod.Post, urlFinal, strContent);
            HttpResponseMessage response = await httpClient.SendAsync(req);

            String responseBodyAsText = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Created)
            {
                //Sucesso
                //responseBodyAsText = await response.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(responseBodyAsText);

                //POG Nervosa para obter o id
                jobId = ((String)jsonObj["d"]["__metadata"]["id"]).Replace(urlFinal + "('", "").Replace("')", "");
                criaDownloadAccessPolicy();
                
            }
        }

        private HttpRequestMessage criaRequest(HttpMethod verb, String url, String strContent)
        {
            HttpRequestMessage req = new HttpRequestMessage(verb, url);
            req.Content = new StringContent(strContent);
            req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=verbose");
            req.Content.Headers.Add("DataServiceVersion", "3.0");
            req.Content.Headers.Add("MaxDataServiceVersion", "3.0");
            req.Content.Headers.Add("x-ms-version", "2.0");
            return req;
        }




        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }


        private IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>();

        private async void InsertTodoItem(TodoItem todoItem)
        {
            // This code inserts a new TodoItem into the database. When the operation completes
            // and Mobile Services has assigned an Id, the item is added to the CollectionView
            await todoTable.InsertAsync(todoItem);
            
        }

        private async void btnPlayMensagem_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MensagensContato), m_recordStorageFile);
        }
        
    }
}
