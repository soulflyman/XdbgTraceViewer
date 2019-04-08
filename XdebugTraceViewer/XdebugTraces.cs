using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace XdbgTraceViewer
{
    public class XdebugTraces
    {
        /// <summary>
        /// List of trace files
        /// </summary>
        private readonly string[] traceFiles;

        /// <summary>
        /// Constructor - Search the RuntimeConfig.TraceFolder for trace files
        /// </summary>
        public XdebugTraces()
        {
            traceFiles = Directory.GetFiles(RuntimeConfig.Instance.TracesFolder, "*.xt");
        }

        /// <summary>
        /// Returns a list of all found trace files in the RuntimeConfig.TracesFolder
        /// </summary>
        /// <returns></returns>
        public List<TraceFileListItem> GetList()
        {
            var traceFileListItems = new List<TraceFileListItem>();
            
            foreach (var traceFilePath in traceFiles)
            {
                var fi = new FileInfo(traceFilePath);

                var listItem = new TraceFileListItem
                {
                    TraceFileName = fi.Name,
                    TraceFileDate = fi.CreationTime.ToString(CultureInfo.InvariantCulture),
                    TraceFileSize = fi.Length / 1024 + " KB"
                };

                traceFileListItems.Add(listItem);
            }

            return traceFileListItems;
        }

        /// <summary>
        /// Trace file list entry
        /// </summary>
        public class TraceFileListItem
        {
            /// <summary>
            /// Trace file name
            /// </summary>
            public string TraceFileName { get; set; }
            
            /// <summary>
            /// Trace file cration date and time
            /// </summary>
            public string TraceFileDate { get; set; }

            /// <summary>
            /// Trace file size in KB
            /// </summary>
            public string TraceFileSize { get; set; }
        }
    }
}
