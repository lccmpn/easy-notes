using EasyNotes.Common;
using EasyNotes.Data.Model;
using EasyNotes.Database;
using EasyNotes.Utility;
using System;
using Windows.UI.Popups;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using EasyNotes.ViewModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace EasyNotes
{
    public sealed partial class SimpleNoteDetailPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly string deletionAlertMessage;
        private readonly string deletionConfirm;
        private readonly string deletionCancel;
        private SimpleNoteDetailViewModel viewModel;

        public SimpleNoteDetailPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.deletionCancel = AppResourcesLoader.LoadStringResource(StringResources.RESOURCES, "No");
            this.deletionConfirm = AppResourcesLoader.LoadStringResource(StringResources.RESOURCES, "Yes");
            this.deletionAlertMessage = AppResourcesLoader.LoadStringResource(StringResources.RESOURCES, "NoteDeletionAlertMessage");
            viewModel = new SimpleNoteDetailViewModel();
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            SimpleNote note = (SimpleNote)DataManager.SimpleNoteData.GetNoteById((long)e.NavigationParameter);
            viewModel.SimpleNote = note;
            this.DataContext = viewModel.SimpleNote;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
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
                DataManager.SimpleNoteData.DeleteNote(this.viewModel.SimpleNote.ID);
                if(Frame.CanGoBack){
                    Frame.GoBack();
                }
            }
        }

        private async void SaveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(viewModel.SimpleNote.Content))
            {
                string alertMessage = AppResourcesLoader.LoadStringResource(StringResources.ERRORS, "EmptyNoteAlert");
                MessageDialog msgbox = new MessageDialog(alertMessage);
                await msgbox.ShowAsync();
                return;
            }
            if (string.IsNullOrEmpty(viewModel.SimpleNote.Title))
            {
                viewModel.SimpleNote.Title = AppResourcesLoader.LoadStringResource(StringResources.RESOURCES, "DefaultNoteTitle");
            }
            DataManager.SimpleNoteData.UpdateNote(viewModel.SimpleNote.ID, viewModel.SimpleNote.Title, viewModel.SimpleNote.Content);
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

    }
}