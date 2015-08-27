﻿using EasyNotes.Common;
using EasyNotes.DataModel;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace EasyNotes
{
    public sealed partial class PivotPage : Page
    {
        enum PivotItem { SimpleNote, TodoNote, Other};
        private readonly NavigationHelper navigationHelper;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private ObservableCollection<AbstractNote> notes = new ObservableCollection<AbstractNote> ();

        public PivotPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
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
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            notes = DataManager.GetAllNotes();
            SimpleNotesList.DataContext = notes;
        }

        /// <summary>
        /// Loads the content for the second pivot item when it is scrolled into view.
        /// </summary>
        private void SecondPivot_Loaded(object sender, RoutedEventArgs e)
        {   
            ToDoNotesList.DataContext = notes;
        }

        /// <summary>
        /// Loads the content for the third pivot item when it is scrolled into view.
        /// </summary>
        private void ThirdPivot_Loaded(object sender, RoutedEventArgs e)
        {
           //PhotoNotesList.ItemsSource = DataManager.GetAllNotes();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        /// <summary>
        /// Adds an item to the list when the app bar button is clicked.
        /// </summary>
        private void AddNoteBarButton_Click(object sender, RoutedEventArgs e)
        {
            System.Type type = null;
            switch (this.pivot.SelectedIndex)
            {
                case (int)PivotItem.SimpleNote:
                    type = typeof(AddSimpleNote);
                    break;
                case (int)PivotItem.TodoNote:
                    break;
                case (int)PivotItem.Other:
                    break;
            }
            if (type == null || !Frame.Navigate(type))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
            //string groupName = this.pivot.SelectedIndex == 0 ? AllNotes : AllTodos;
            //var group = this.DefaultViewModel[groupName] as SampleDataGroup;
            //var nextItemId = group.Items.Count + 1;
            //var newItem = new SampleDataItem(
            //    string.Format(CultureInfo.InvariantCulture, "Group-{0}-Item-{1}", this.pivot.SelectedIndex + 1, nextItemId),
            //    string.Format(CultureInfo.CurrentCulture, this.resourceLoader.GetString("NewItemTitle"), nextItemId),
            //    string.Empty,
            //    string.Empty,
            //    this.resourceLoader.GetString("NewItemDescription"),
            //    string.Empty);

            //group.Items.Add(newItem);

            //// Scroll the new item into view.
            //var container = this.pivot.ContainerFromIndex(this.pivot.SelectedIndex) as ContentControl;
            //var listView = container.ContentTemplateRoot as ListView;
            //listView.ScrollIntoView(newItem, ScrollIntoViewAlignment.Leading);
        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            long itemId = ((AbstractNote)e.ClickedItem).ID;
            Type type = null;
            if (e.ClickedItem.GetType() == typeof(SimpleNote))
            {
                type = typeof(SimpleNoteDetailPage);
            }
            if (type == null || !Frame.Navigate(type, itemId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
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
