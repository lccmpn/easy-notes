using EasyNotes.Common;
using EasyNotes.Data.Model;
using System.ComponentModel;
using EasyNotes.Data.NoteManager;
using EasyNotes.Utility;
using EasyNotes.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using EasyNotes.Enums;
using Windows.UI.Popups;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace EasyNotes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditPhotoNotePage : Page
    {
        private NavigationHelper navigationHelper;
        private EditPhotoNoteViewModel viewModel = new EditPhotoNoteViewModel();
        PhotoNoteManager photoNoteManager = new PhotoNoteManager();
        private string photoPath;
        private PageAction action;

        //private MediaCapture capturePhotoManager;
        //private bool isPreviewing;

        public EditPhotoNotePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        //public ObservableDictionary DefaultViewModel
        //{
        //    get { return this.viewModel; }
        //}

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (e.NavigationParameter != null)
            {
                Debug.WriteLine("Id passed");
                long noteID = (long)e.NavigationParameter;
                viewModel = new EditPhotoNoteViewModel((PhotoNote)photoNoteManager.GetNoteById(noteID));
                Debug.WriteLine(viewModel.ToString());
                action = PageAction.Update;
                if (viewModel.IsNotificationDateVisible)
                {
                    SetNotificationSchedulingVisibile(true);
                }
            }
            else
            {
                if (e.PageState != null)
                {
                    Debug.WriteLine("Restoring state");
                    EditPhotoNoteViewModel editPhotoNoteViewModel = (EditPhotoNoteViewModel)e.PageState["ViewModel"];
                    if (editPhotoNoteViewModel != null)
                    {
                        viewModel = editPhotoNoteViewModel;
                    }
                }
                else
                {
                    Debug.WriteLine("Creating new note");
                    viewModel = new EditPhotoNoteViewModel();
                }
            }
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            Object value = localSettings.Values[PhotoNoteManager.PHOTO_PATH_KEY];
            if (value != null)
            {
                //if (!string.IsNullOrWhiteSpace(viewModel.PhotoPath))
                //{
                //    photoNoteManager.DeleteImageFromLocalFolder(viewModel.PhotoPath);
                //}
                Debug.WriteLine(value);

                viewModel.PhotoPath = (string)value;
                localSettings.Values.Remove(PhotoNoteManager.PHOTO_PATH_KEY);
            }
            this.DataContext = viewModel;

        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // Save the ViewModel variable in the page's State dictionary.
            e.PageState["ViewModel"] = viewModel;
        }

        private void TakePhotoAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(viewModel.PhotoPath))
            {
                if (!Frame.Navigate(typeof(PhotoPreviewPage), PageAction.Update))
                {
                    throw new Exception(AppResourcesLoader.LoadStringResource(StringResources.ERRORS, "NavigationFailedExceptionMessage"));
                }
            }
            else
            {
                if (!Frame.Navigate(typeof(PhotoPreviewPage)))
                {
                    throw new Exception(AppResourcesLoader.LoadStringResource(StringResources.ERRORS, "NavigationFailedExceptionMessage"));
                }
            }

        }

        private async void SaveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(viewModel.Title))
            {
                viewModel.Title = AppResourcesLoader.LoadStringResource(StringResources.RESOURCES, "DefaultNoteTitle");
            }
            if (RememberNoteGrid.Visibility == Visibility.Visible)
            {
                DateTime date = (DateTime)RememberingDatePicker.Date.Date;
                TimeSpan time = (TimeSpan)RememberingTimePicker.Time;
                DateTimeOffset dateTime = date + time;
                if (dateTime < DateTime.Now)
                {
                    // TODO put this string in string resources
                    MessageDialog msgbox = new MessageDialog("Date and hour must be in the future.");
                    await msgbox.ShowAsync();
                    return;
                }
                if (action == PageAction.Create)
                {
                    photoNoteManager.AddNoteAndNotification(viewModel.Title, viewModel.PhotoPath, viewModel.Content, viewModel.Title, viewModel.NotificationDate.Date + viewModel.NotificationTime);
                }
                else
                {
                    photoNoteManager.UpdateNoteAndNotification(viewModel.ID, viewModel.Title, viewModel.PhotoPath, viewModel.Content, viewModel.Title, viewModel.NotificationDate.Date + viewModel.NotificationTime);
                }
            }
            else
            {
                if (action == PageAction.Create)
                {
                    photoNoteManager.AddNote(viewModel.Title, viewModel.PhotoPath, viewModel.Content);
                }
                else
                {
                    photoNoteManager.UpdateNote(viewModel.ID, viewModel.Title, viewModel.PhotoPath, viewModel.Content);
                }
            }
            if (!Frame.Navigate(typeof(PivotPage)))
            {
                throw new Exception(AppResourcesLoader.LoadStringResource(StringResources.ERRORS, "NavigationFailedExceptionMessage"));
            }
            //if(Frame.CanGoBack){
            //    Frame.GoBack();
            //}
        }

        private void CalendarAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SetNotificationSchedulingVisibile(true);
        }

        private void CancelSchedulingAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SetNotificationSchedulingVisibile(false);
        }

        private void SetNotificationSchedulingVisibile(bool visible)
        {
            if (visible)
            {
                CancelSchedulingAppBarButton.Visibility = Visibility.Visible;
                CalendarAppBarButton.Visibility = Visibility.Collapsed;
                RememberNoteGrid.Visibility = Visibility.Visible;
            }
            else
            {
                CancelSchedulingAppBarButton.Visibility = Visibility.Collapsed;
                CalendarAppBarButton.Visibility = Visibility.Visible;
                RememberNoteGrid.Visibility = Visibility.Collapsed;
            }
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void TakenImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(PhotoDetailPage), viewModel.PhotoPath))
            {
                throw new Exception(AppResourcesLoader.LoadStringResource(StringResources.ERRORS, "NavigationFailedExceptionMessage"));
            }
        }

    }
}
