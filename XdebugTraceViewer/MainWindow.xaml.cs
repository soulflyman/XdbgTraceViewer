using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Ookii.Dialogs.Wpf;

namespace XdbgTraceViewer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// File system watcher for selected trace file folder
        /// </summary>
        private FileSystemWatcher folderWatcher;

        /// <summary>
        /// Entry point of the MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            if (RuntimeConfig.Instance.TracesFolder != Properties.Settings.Default.CustomTraceFileFolder)
            {
                RuntimeConfig.Instance.TracesFolder = Properties.Settings.Default.CustomTraceFileFolder;
            }


            RefreshTraceFileList();
            WatchTraceFolder(RuntimeConfig.Instance.TracesFolder);
        }

        /// <summary>
        /// Refresh the trace file list
        /// </summary>
        private void RefreshTraceFileList()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                var selectedTraceFileName = (TraceFileList.SelectedItem as XdebugTraces.TraceFileListItem)?.TraceFileName;

                TraceFileList.ItemsSource = new XdebugTraces().GetList();

                foreach (var traceFileListItem in TraceFileList.Items)
                {
                    if((traceFileListItem as XdebugTraces.TraceFileListItem)?.TraceFileName != selectedTraceFileName) continue;

                    TraceFileList.SelectedItem = traceFileListItem;
                }
            }));
        }

        /// <summary>
        /// Left click event in the TraceFileList element
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TraceFileList_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                ClearRecordDetails();

                var traceFileName = ((sender as ListView)?.SelectedItem as XdebugTraces.TraceFileListItem)?.TraceFileName;
                if (traceFileName == null) return;

                var traceFile = System.IO.Path.Combine(RuntimeConfig.Instance.TracesFolder, System.IO.Path.GetFileName(traceFileName));

                try
                {
                    TraceItemsTree.ItemsSource = XdebugTrace.ReadTraceFile(traceFile);
                    gbTraceFileContent.Header = "Trace File: " + traceFileName;
                }
                catch (Exception exception)
                {
                    TraceItemsTree.ItemsSource = null;
                    gbTraceFileContent.Header = "Trace File";
                    MessageBox.Show(exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }));
        }

        /// <summary>
        /// Context menu event for element 'Collapse'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TraceItemsTreeContextCollapse_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is XdebugTraceItem traceItem) traceItem.CollapseAll();
        }

        /// <summary>
        /// Context menu event for element 'Expand'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TraceItemsTreeContextExpand_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is XdebugTraceItem traceItem) traceItem.ExpandAll();
        }

        /// <summary>
        /// Selected item changed event for the TraceItemsTree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TraceItemsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                ClearRecordDetails();

                var tv = (sender as TreeView);
                if (!(tv?.SelectedItem is XdebugTraceItem traceItem)) return;

                tbFunctionName.Text = traceItem.FunctionName;
                tbExecutionTime.Text = traceItem.ExecutionTime + " seconds";
                lbParameters.ItemsSource = traceItem.Parameters;
                tbFileName.Text = traceItem.File;
                tbLineNumber.Text = traceItem.Line;

                tbReturnValue.Text = traceItem.IsReturnFormatted ? traceItem.ReturnValueFormatted : traceItem.ReturnValue;
            }));

            e.Handled = true;
        }

        /// <summary>
        /// Clear the Record details fields
        /// </summary>
        private void ClearRecordDetails()
        {
            tbFunctionName.Text = "";
            tbExecutionTime.Text = "";
            lbParameters.ItemsSource = null;
            tbFileName.Text = "";
            tbLineNumber.Text = "";
            tbReturnValue.Text = "";
        }

        /// <summary>
        /// Button btnFormatResult click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTryToFormatResult_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (!(TraceItemsTree.SelectedItem is XdebugTraceItem traceItem)) return;

                traceItem.FormatReturnValue();
                tbReturnValue.Text = traceItem.IsReturnFormatted ? traceItem.ReturnValueFormatted : traceItem.ReturnValue;
            }));
        }

        /// <summary>
        /// Button btnDisplayOriginalResult click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDisplayOriginalResultValue_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (!(TraceItemsTree?.SelectedItem is XdebugTraceItem traceItem)) return;

                traceItem.IsReturnFormatted = false;
                tbReturnValue.Text = traceItem.ReturnValue;
            }));
        }

        /// <summary>
        /// Btn 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderSelectDialog = new VistaFolderBrowserDialog
            {
                Description = "Please select a folder.", 
                UseDescriptionForTitle = true
            };
            // This applies to the Vista style dialog only, not the old dialog.

            if (!(bool) folderSelectDialog.ShowDialog(this)) return;

            RuntimeConfig.Instance.TracesFolder = folderSelectDialog.SelectedPath;
            RefreshTraceFileList();
            ChangeWatchedTraceFolder(RuntimeConfig.Instance.TracesFolder);

            Properties.Settings.Default.CustomTraceFileFolder = RuntimeConfig.Instance.TracesFolder;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Monitor the selected trace file folder for changes
        /// </summary>
        /// <param name="traceFolder"></param>
        private void WatchTraceFolder(string traceFolder)
        {
            folderWatcher = new FileSystemWatcher
            {
                Path = traceFolder,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                Filter = "*.xt",
                EnableRaisingEvents = true
            };

            folderWatcher.Changed += OnTraceFolderFileChanges;
            folderWatcher.Renamed += OnTraceFolderFileChanges;
            folderWatcher.Deleted += OnTraceFolderFileChanges;
        }

        /// <summary>
        /// Change the watched trace file folder
        /// </summary>
        /// <param name="traceFolder"></param>
        private void ChangeWatchedTraceFolder(string traceFolder)
        {
            folderWatcher.Path = traceFolder;
        }

        /// <summary>
        /// Event that runs when a chenage in the watched trace folder is detected
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTraceFolderFileChanges(object source, FileSystemEventArgs e)
        {
            RefreshTraceFileList();
        }
    }
}
