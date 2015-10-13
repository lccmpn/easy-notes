using EasyNotes.Data.NoteManager;
using EasyNotes.Enums;
using EasyNotes.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace EasyNotes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PhotoPreviewPage : Page
    {
        private MediaCapture capturePhotoManager;
        private bool isPreviewing;
        private string photoTakenPath;
        private PageAction action;

        public PhotoPreviewPage()
        {
            this.InitializeComponent();
            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                action = PageAction.Update;
            }
            InitializePreview();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
        }

        //private async void RotateImage()
        //{
        //    string captureFileName = string.Empty;
        //    ImageEncodingProperties format = ImageEncodingProperties.CreateJpeg();
        //    //rotate and save the image
        //    using (var imageStream = new InMemoryRandomAccessStream())
        //    {
        //        //generate stream from MediaCapture
        //        await captureManager.CapturePhotoToStreamAsync(format, imageStream);

        //        //create decoder and encoder
        //        BitmapDecoder dec = await BitmapDecoder.CreateAsync(imageStream);
        //        BitmapEncoder enc = await BitmapEncoder.CreateForTranscodingAsync(imageStream, dec);

        //        //roate the image
        //        enc.BitmapTransform.Rotation = BitmapRotation.Clockwise90Degrees;

        //        //write changes to the image stream
        //        await enc.FlushAsync();

        //        //save the image
        //        StorageFolder folder = KnownFolders.SavedPictures;
        //        StorageFile capturefile = await folder.CreateFileAsync("photo_" + DateTime.Now.Ticks.ToString() + ".jpg", CreationCollisionOption.ReplaceExisting);
        //        captureFileName = capturefile.Name;

        //        //store stream in file
        //        using (var fileStream = await capturefile.OpenStreamForWriteAsync())
        //        {
        //            try
        //            {
        //                //because of using statement stream will be closed automatically after copying finished
        //                await RandomAccessStream.CopyAsync(imageStream, fileStream.AsOutputStream());
        //            }
        //            catch
        //            {

        //            }
        //        }
        //    }
        //}

        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Get the desired camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            // If there is no device mounted on the desired panel, return the first device found
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }

        private async void InitializePreview()
        {
            var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Back);
            if (cameraDevice == null)
            {
                Debug.WriteLine("No camera device found!");
                return;
            }
            capturePhotoManager = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };
            capturePhotoManager = new MediaCapture();
            await capturePhotoManager.InitializeAsync(settings);
            capturePhotoManager.SetPreviewRotation(VideoRotation.Clockwise90Degrees);
            PhotoPreview.Source = null;
            TakenImage.Source = null;
            PhotoPreview.Visibility = Visibility.Visible;
            TakenImage.Visibility = Visibility.Collapsed;
            StartPreview();
        }

        private async void StartPreview()
        {
            PhotoPreview.Source = capturePhotoManager;
            await capturePhotoManager.StartPreviewAsync();
            isPreviewing = true;
        }

        private async void StopPreview()
        {
            isPreviewing = false;
            await capturePhotoManager.StopPreviewAsync();
        }

        private async void CapturePhoto()
        {
            if (capturePhotoManager != null)
            {
                if (isPreviewing == true)
                {
                    ImageEncodingProperties format = ImageEncodingProperties.CreateJpeg();
                    StorageFile capturefile = await ApplicationData.Current.LocalFolder.CreateFileAsync("photo_" + DateTime.Now.Ticks.ToString(), CreationCollisionOption.ReplaceExisting);
                    await capturePhotoManager.CapturePhotoToStorageFileAsync(format, capturefile);
                    BitmapImage photoTaken = new BitmapImage(new Uri(capturefile.Path));
                    photoTakenPath = capturefile.Path;
                    PhotoPreview.Source = null;
                    PhotoPreview.Visibility = Visibility.Collapsed;
                    TakenImage.Source = photoTaken;
                    TakenImage.Visibility = Visibility.Visible;
                    StopPreview();
                }
                else
                {
                    DeletePhoto(photoTakenPath);
                    InitializePreview();
                }
            }
        }

        private async void DeletePhoto(string path)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(path);
            await file.DeleteAsync();
        }

        private void SaveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            capturePhotoManager.Dispose();
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[PhotoNoteManager.PHOTO_PATH_KEY] = photoTakenPath;
            if (action == PageAction.Update)
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            }
            else
            {
                if (!Frame.Navigate(typeof(EditPhotoNotePage)))
                {
                    throw new Exception(AppResourcesLoader.LoadStringResource(StringResources.ERRORS, "NavigationFailedExceptionMessage"));
                }
            }
        }

        private void CapturePhotoAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAppBarButton.Visibility = Visibility.Visible;
            CaptureNewPhotoAppBarButton.Visibility = Visibility.Visible;
            CapturePhotoAppBarButton.Visibility = Visibility.Collapsed;
            CapturePhoto();
        }

        private void CaptureNewPhotoAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAppBarButton.Visibility = Visibility.Collapsed;
            CaptureNewPhotoAppBarButton.Visibility = Visibility.Collapsed;
            CapturePhotoAppBarButton.Visibility = Visibility.Visible;
            CapturePhoto();
        }
    }
}