using EasyNotes.Common;
using EasyNotes.Data.Model;
using EasyNotes.Database;
using EasyNotes.Utility;
using Windows.Phone.UI.Input;
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
using EasyNotes.Data.Database;
using EasyNotes.Data;
using EasyNotes.Data.NoteManager;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace EasyNotes
{
    public  enum NoteType { SimpleNote, TodoNote, PhotoNote };

    public sealed partial class PivotPage : Page
    {
        List<INoteManager> noteManagers = new List<INoteManager>();
        private NoteType selectedPivot;
        private Dictionary<int, ListView> listViews = new Dictionary<int, ListView>();
        private readonly NavigationHelper navigationHelper;
        private ObservableDictionary viewModels = new ObservableDictionary();
        private const int DELETE_APP_BAR_BUTTON_POSITION = 0;
        private readonly string SIMPLE_NOTE = NoteType.SimpleNote.ToString();
        private readonly string TODO_NOTE = NoteType.TodoNote.ToString();
        private readonly string PHOTO_NOTE = NoteType.PhotoNote.ToString();

        public PivotPage()
        {
            this.InitializeComponent();
            this.selectedPivot = NoteType.SimpleNote;
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.listViews[(int)NoteType.SimpleNote] = SimpleNotesList;
            this.listViews[(int)NoteType.TodoNote] = ToDoNotesList;
            this.listViews[(int)NoteType.PhotoNote] = PhotoNotesList;
            noteManagers.Add(new SimpleNoteManager());
            noteManagers.Add(new TodoNoteManager());
            noteManagers.Add(new PhotoNoteManager());
        }

        private string ConvertPivotIndexToString(int pivotIndex)
        {
            if(pivotIndex == (int)NoteType.SimpleNote){
                return NoteType.SimpleNote.ToString();
            }
            if(pivotIndex == (int)NoteType.TodoNote){
                return NoteType.TodoNote.ToString();
            }
            if (pivotIndex == (int)NoteType.PhotoNote)
            {
                return NoteType.PhotoNote.ToString();
            }
            else
            {
                throw new Exception("Pivot not found");
            }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            if (frame == null)
            {
                return;
            }
            if (SimpleNotesList.SelectionMode.Equals(ListViewSelectionMode.Multiple))
            {
                UnsetMultipleSelection();
                e.Handled = true;
                return;
            }
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
            FirstPivot_Loaded(sender, new RoutedEventArgs());
        }

        private void FirstPivot_Loaded(object sender, RoutedEventArgs e)
        {
            viewModels[SIMPLE_NOTE] = noteManagers[(int)NoteType.SimpleNote].GetAllNotes();
            SimpleNotesList.DataContext = viewModels[SIMPLE_NOTE];
            this.selectedPivot = NoteType.SimpleNote;
        }

        /// <summary>
        /// Loads the content for the second pivot item when it is scrolled into view.
        /// </summary>
        private void SecondPivot_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Second list loaded");
            viewModels[TODO_NOTE] = noteManagers[(int)NoteType.TodoNote].GetAllNotes();
            ToDoNotesList.DataContext = viewModels[TODO_NOTE];
            this.selectedPivot = NoteType.TodoNote;
            
            
        }

        /// <summary>
        /// Loads the content for the third pivot item when it is scrolled into view.
        /// </summary>
        private void ThirdPivot_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Third list loaded");
            viewModels[PHOTO_NOTE] = noteManagers[(int)NoteType.PhotoNote].GetAllNotes();
            PhotoNotesList.DataContext = viewModels[PHOTO_NOTE];
            this.selectedPivot = NoteType.PhotoNote;
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
            Type type = null;
            switch (this.pivot.SelectedIndex)
            {
                case (int)NoteType.SimpleNote:
                    type = typeof(EditSimpleNote);
                    break;
                case (int)NoteType.TodoNote:
                    type = typeof(EditTodoNotePage);
                    break;
                case (int)NoteType.PhotoNote:
                    type = typeof(PhotoPreviewPage);
                    break;
            }
            if (type == null || !Frame.Navigate(type))
            {
                throw new Exception(AppResourcesLoader.LoadStringResource(StringResources.ERRORS, "NavigationFailedExceptionMessage"));
            }
            if(IsMultipleSelectionenable()){
                UnsetMultipleSelection();
            }
            //// Scroll the new item into view.

        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            
                long itemId = ((BaseNote)e.ClickedItem).Id;
                Type type = null;
                switch (this.pivot.SelectedIndex)
                {
                    case (int)NoteType.SimpleNote:
                        type = typeof(EditSimpleNote);
                        break;
                    case (int)NoteType.TodoNote:
                        type = typeof(EditTodoNotePage);
                        break;
                    case (int)NoteType.PhotoNote:
                        type = typeof(EditPhotoNotePage);
                        break;
                }
                if (type == null || !Frame.Navigate(type, itemId))
                {
                    throw new Exception(AppResourcesLoader.LoadStringResource(StringResources.ERRORS, "NavigationFailedExceptionMessage"));
                }
            
            //else
            //{
            //    Debug.WriteLine(SimpleNotesList.SelectedItems.);
            //}
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
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        private void MultipleSelectionAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsMultipleSelectionenable())
            {
                SetMultipleSelection();
            }
            else
            {
                UnsetMultipleSelection();
            }
        }

        private void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {   
            ListView list = listViews[this.pivot.SelectedIndex];
            Debug.WriteLine("pivot " + this.pivot.SelectedIndex);
            if (list.SelectedItems.Count > 0)
            {
                foreach (BaseNote note in list.SelectedItems.Reverse())
                {
                    noteManagers[this.pivot.SelectedIndex].DeleteNote(note.Id);
                    ObservableCollection<BaseNote> notes = (ObservableCollection<BaseNote>)viewModels[ConvertPivotIndexToString(pivot.SelectedIndex)];
                    notes.Remove(note);
                }
                UnsetMultipleSelection();
            }
        }

        private void SetMultipleSelection()
        {
            ListView list = listViews[this.pivot.SelectedIndex];
            if (((ObservableCollection<BaseNote>)viewModels[ConvertPivotIndexToString(pivot.SelectedIndex)]).Count > 0 && !IsMultipleSelectionenable())
            {
                Debug.WriteLine("Setting multiple");
                list.SelectionMode = ListViewSelectionMode.Multiple;
                list.IsItemClickEnabled = false;
                AddDeleteButtonToAppBar();
            }
        }

        private void UnsetMultipleSelection()
        {
            ListView list = listViews[this.pivot.SelectedIndex];
            list.SelectionMode = ListViewSelectionMode.None;
            list.IsItemClickEnabled = true;
            if (CommandBar != null)
            {
                AppBarButton b = CommandBar.PrimaryCommands[DELETE_APP_BAR_BUTTON_POSITION] as AppBarButton;
                b.Click -= DeleteAppBarButton_Click;
                // Remove AppBarButton.
                CommandBar.PrimaryCommands.RemoveAt(DELETE_APP_BAR_BUTTON_POSITION);
                // Remove AppBarSeparator.
                CommandBar.PrimaryCommands.RemoveAt(DELETE_APP_BAR_BUTTON_POSITION);
            }
        }

        private bool IsMultipleSelectionenable()
        {
            ListView list = listViews[this.pivot.SelectedIndex];
            return list.SelectionMode == ListViewSelectionMode.Multiple;
        }

        private void AddDeleteButtonToAppBar()
        {
            // Add separator.
            CommandBar.PrimaryCommands.Insert(DELETE_APP_BAR_BUTTON_POSITION, new AppBarSeparator());
            // Create delete button.
            AppBarButton deleteButton = new AppBarButton();
            deleteButton.Icon = new SymbolIcon(Symbol.Delete);
            deleteButton.Label = AppResourcesLoader.LoadStringResource(StringResources.RESOURCES, "DeleteAppBarButtonLabel");
            deleteButton.Click += DeleteAppBarButton_Click;
            // Add delete button.
            CommandBar.PrimaryCommands.Insert(DELETE_APP_BAR_BUTTON_POSITION, deleteButton);
        }



    }
}