using EasyNotes.Common;
using System;
using EasyNotes.ViewModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using EasyNotes.Utility;
using Windows.UI.Popups;
using EasyNotes.Database;
using System.Diagnostics;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using EasyNotes.Data.Model;
using EasyNotes.Enums;
using EasyNotes.Data.NoteManager;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace EasyNotes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditTodoNotePage : Page
    {
        private PageAction state;
        private NavigationHelper navigationHelper;
        private EditTodoNoteViewModel viewModel;
        private TodoNoteManager todoNoteDataHelper = new TodoNoteManager();
        private const string  EMPTY_STRING = "";

        public EditTodoNotePage()
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
                long noteID = (long)e.NavigationParameter;
                viewModel = new EditTodoNoteViewModel((TodoNote)todoNoteDataHelper.GetNoteById(noteID));
                if (viewModel.IsNotificationDateVisible)
                {
                    SetNotificationSchedulingVisibile(true);
                }
                state = PageAction.Update;

            }
            else
            {
                viewModel = new EditTodoNoteViewModel();
                viewModel.AddEntry(EMPTY_STRING, false);
                state = PageAction.Create;
            }
            this.DataContext = viewModel;
            TodoNotesList.DataContext = viewModel.TodoEntries;
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
        }

        private void ContentTextBox_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async void SaveNoteBarButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO check if all TodoEntries are empty
            if (viewModel.TodoEntries.Count == 0)
            {
                string alertMessage = AppResourcesLoader.LoadStringResource(StringResources.ERRORS, "EmptyNoteAlert");
                MessageDialog msgbox = new MessageDialog(alertMessage);
                await msgbox.ShowAsync();
                return;
            }
            if (string.IsNullOrEmpty(viewModel.Title))
            {
                viewModel.Title = AppResourcesLoader.LoadStringResource(StringResources.RESOURCES, "DefaultNoteTitle");
            }
            for (int i = viewModel.TodoEntries.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(viewModel.TodoEntries[i].Content))
                {
                    viewModel.TodoEntries.Remove(viewModel.TodoEntries[i]);
                }
            }
            if (RememberNoteGrid.Visibility == Visibility.Visible)
            {
                DateTimeOffset date = (DateTimeOffset)RememberingDatePicker.Date.Date;
                TimeSpan time = (TimeSpan)RememberingTimePicker.Time;
                date = date.Add(time);
                Debug.WriteLine(date);
                if (date < DateTime.Now)
                {
                    // TODO put this string in string resources
                    MessageDialog msgbox = new MessageDialog("Date and hour must be in the future.");
                    await msgbox.ShowAsync();
                    return;
                }
                if (state == PageAction.Create)
                {
                    todoNoteDataHelper.AddNoteAndNotification(viewModel.Title, viewModel.TodoEntries, viewModel.Title, viewModel.NotificationDate.Date + viewModel.NotificationTime);
                }
                else
                {
                    todoNoteDataHelper.UpdateNoteAndNotification(viewModel.Id, viewModel.Title, viewModel.TodoEntries, viewModel.Title, viewModel.NotificationDate.Date + viewModel.NotificationTime);
                }
            }
            else
            {
                if (state == PageAction.Create)
                {
                    todoNoteDataHelper.AddNote(viewModel.Title, viewModel.TodoEntries);
                }
                else
                {
                    todoNoteDataHelper.UpdateNote(viewModel.Id, viewModel.Title, viewModel.TodoEntries);
                }
            }
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(viewModel.TodoEntries.Count - 1);
            viewModel.AddEntry(EMPTY_STRING, false);
            TodoNotesList.UpdateLayout();
            TodoNotesList.ScrollIntoView(TodoNotesList.Items[viewModel.TodoEntries.Count - 1]);
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

        //private void SelectRememberingTime_Click(object sender, RoutedEventArgs e)
        //{
        //    FrameworkElement senderElement = sender as FrameworkElement;
        //    FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
        //    flyoutBase.ShowAt(senderElement);
        //}

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

        private void CalendarAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SetNotificationSchedulingVisibile(true);
        }

        private void CancelSchedulingAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SetNotificationSchedulingVisibile(false);
        }


    }
}
