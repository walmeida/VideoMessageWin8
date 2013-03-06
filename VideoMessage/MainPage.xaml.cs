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
        private bool m_bAssistindo;
        private readonly String VIDEO_FILE_NAME = "video.mp4";
        private Windows.Storage.StorageFile m_recordStorageFile;

        private MediaExtensionManager extensions = new MediaExtensionManager();
        
        public MainPage()
        {
            this.InitializeComponent();
            startDeviceAndPreview();

            adicionaSuporteAoSmoothStreaming();

            mediaElement.MediaOpened += mediaElement_MediaOpened;
        }

        private void adicionaSuporteAoSmoothStreaming()
        {
            extensions.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".ism", "text/xml");
            extensions.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".ism", "application/vnd.ms-sstr+xml");
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

        private void btnPlayMensagem_Click(object sender, RoutedEventArgs e)
        {
            if (!m_bAssistindo)
            {
                mediaElement.Source = new Uri("http://ecn.channel9.msdn.com/o9/content/smf/smoothcontent/elephantsdream/Elephants_Dream_1024-h264-st-aac.ism/manifest");
                
                btnPlayMensagem.Content = "Parar Mensagem";
                m_bAssistindo = true;
            }
            else
            {
                m_bAssistindo = false;
                mediaElement.Stop();
                btnPlayMensagem.Content = "Ver Mensagem";
            }
        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "MediaElement opened";
            mediaElement.Play();
        }

        

        

        

        
    }
}
