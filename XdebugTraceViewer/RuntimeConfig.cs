using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using XdbgTraceViewer.Annotations;

namespace XdbgTraceViewer
{
    public sealed class RuntimeConfig : INotifyPropertyChanged
    {
        /// <summary>
        /// Lazy loaded singleton instance of the RuntimeConfig
        /// </summary>
        private static readonly Lazy<RuntimeConfig> lazy = new Lazy<RuntimeConfig>(() => new RuntimeConfig());

        /// <summary>
        /// Singleton instance of the RuntimeConfig
        /// </summary>
        public static RuntimeConfig Instance
        {
            get { return lazy.Value; }
        }

        /// <summary>
        /// Property change event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Property changed event
        /// </summary>
        /// <param name="propertyName"></param>
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string traceFolder = @"C:\";
        /// <summary>
        /// Holds the selected trace file folder path
        /// </summary>
        public string TracesFolder
        {
            get => traceFolder;
            set
            {
                if (traceFolder == value) return;
                traceFolder = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Hidden constructor
        /// </summary>
        private RuntimeConfig()
        {

        }
    }
}
