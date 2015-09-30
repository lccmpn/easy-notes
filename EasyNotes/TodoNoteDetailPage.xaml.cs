using EasyNotes.Common;
using System;
using System.Diagnostics;
using EasyNotes.ViewModel;
using EasyNotes.Database;
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
using EasyNotes.Data.Model;
using EasyNotes.Utility;
using Windows.UI.Popups;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace EasyNotes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TodoNoteDetailPage : Page
    {
        private NavigationHelper navigationHelper;
        private TodoNoteDetailViewModel viewModel;
        private readonly string deletionAlertMessage;
        private readonly string deletionConfirm;
        private readonly string deletionCancel;

        public TodoNoteDetailPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.deletionCancel = AppResourcesLoader.LoadStringResource(StringResources.RESOURCES, "No");
            this.deletionConfirm = AppResourcesLoader.LoadStringResource(StringResources.RESOURCES, "Yes");
            this.deletionAlertMessage = AppResourcesLoader.LoadStringResource(StringResources.RESOURCES, "NoteDeletionAlertMessage");
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
            long noteID = (long)e.NavigationParameter;
            viewModel = new TodoNoteDetailViewModel(DataManager.TodoNoteData.GetNoteById(noteID));

            // TODO Perfect data context association. Anyway, it works.
            this.DataContext = viewModel.TodoNote;
            TodoNotesList.DataContext = viewModel.TodoNote.TodoEntries;
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
        private async void SaveNoteBarButton_Click(object sender, RoutedEventArgs e)
        {
            //string title = TitleTextBox.Text;
            //string content = ContentTextBox.Text;
            if (viewModel.TodoNote.TodoEntries.Count == 0)
            {
                string alertMessage = AppResourcesLoader.LoadStringResource(StringResources.ERRORS, "EmptyNoteAlert");
                MessageDialog msgbox = new MessageDialog(alertMessage);
                await msgbox.ShowAsync();
                return;
            }
            if (string.IsNullOrEmpty(viewModel.TodoNote.Title))
            {
                viewModel.TodoNote.Title = AppResourcesLoader.LoadStringResource(StringResources.RESOURCES, "DefaultNoteTitle");
            }
            for (int i = viewModel.TodoNote.TodoEntries.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(viewModel.TodoNote.TodoEntries[i].Content))
                {
                    viewModel.TodoNote.TodoEntries.Remove(viewModel.TodoNote.TodoEntries[i]);
                }
            }

            DataManager.TodoNoteData.UpdateNote(viewModel.TodoNote.ID, viewModel.TodoNote.Title, viewModel.TodoNote.TodoEntries);
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.TodoNote.AddEntry("", false);
            
            //ItemCollection items = TodoNotesList.Items;
            //DataTemplate data = items.ElementAt(items.Count) as DataTemplate;

            // Scroll the new item into view.
            TodoNotesList.ScrollIntoView(viewModel.TodoNote.TodoEntries.ElementAt(viewModel.TodoNote.TodoEntries.Count - 1), ScrollIntoViewAlignment.Leading);
        }

        private async void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog(deletionAlertMessage);
            messageDialog.Commands.Add(new UICommand(deletionConfirm, new UICommandInvokedHandler(this.CommandInvokedHandler)));
            messageDialog.Commands.Add(new UICommand(deletionCancel, new UICommandInvokedHandler(this.CommandInvokedHandler)));
            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            if (command.Label.Equals(deletionConfirm))
            {
                DataManager.TodoNoteData.DeleteNote(this.viewModel.TodoNote.ID);
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
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
    }
}
