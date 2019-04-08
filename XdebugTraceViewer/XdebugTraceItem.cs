using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using XdbgTraceViewer.Annotations;

namespace XdbgTraceViewer
{
    public sealed class XdebugTraceItem : INotifyPropertyChanged
    {
        /// <summary>
        /// Stack trace level
        /// 
        /// Trace record field: 1
        /// </summary>
        public int Level { get; set; }                          // Field 1
        
        /// <summary>
        /// Name of the called function
        /// 
        /// Trace record field: 2
        /// </summary>
        public int FunctionNumber { get; set; }                 // Field 2
        
        /// <summary>
        /// Type of the trace record
        /// 
        /// Trace record field: 3
        /// '0' = Entry
        /// '1' = Exit
        /// 'R' = Return
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Time index in seconds since the first function call
        ///
        /// Trace record field: 4
        /// 
        /// Only if Type = '0' or '1'.
        /// </summary>
        public string TimeIndex { get; set; }

        /// <summary>
        /// Total memory usage of the script at this point in the stack trace
        ///
        /// Trace record field 5
        /// 
        /// Only if Type = '0' or '1'.
        /// </summary>
        public string MemoryUsage { get; set; }

        /// <summary>
        /// Name of the called function
        ///
        /// Trace record field: 6
        /// 
        /// Only if Type = '0'.
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// Type of the called function
        ///
        /// Tace record field: 7
        /// '1' = user-defined
        /// '0' = internal function 
        /// 
        /// Only if Type = '0'.
        /// </summary>
        public int FunctionType { get; set; }

        /// <summary>
        /// Name of the includ / requiere file
        ///
        /// Trace record field: 8
        /// 
        /// Only if Type = '0'.
        /// </summary>
        public string IncludeName { get; set; }

        /// <summary>
        /// Path and name of the file in which the function was called 
        ///
        /// Trace record field: 9
        /// 
        /// Only if Type = '0'.
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// Line number at which the function was called in
        ///
        /// Trace record field: 10
        /// 
        /// Only if Type = '0'.
        /// </summary>
        public string Line { get; set; }

        /// <summary>
        /// Count of function paramaeters
        ///
        /// Trace record field: 11
        /// 
        /// Only if Type = '0'.
        /// </summary>
        public int ParameterCount { get; set; }

        /// <summary>
        /// Array with all function parameters and there values
        /// 
        /// Trace record field: 12 - ...
        /// 
        /// Only if Type = '0'.
        /// </summary>
        public string[] Parameters { get; set; }

        /// <summary>
        /// All sub trace records that ascend from this record
        /// </summary>
        public ObservableCollection<XdebugTraceItem> SubElements { get; set; }

        /// <summary>
        /// Time this function needed to execute
        /// </summary>
        public string ExecutionTime { get; set; }

        /// <summary>
        /// Return value of the called function
        ///
        /// Trace record field: 6
        /// 
        /// Only if Type = 'R'.
        /// </summary>
        public string ReturnValue { get; set; }
        
        /// <summary>
        /// Formatted (intended, pretty printed, ...) ReturnValue
        /// </summary>
        public string ReturnValueFormatted { get; set; }

        /// <summary>
        /// Indicates of the ReturnValue was Formatted (intended, pretty printed, ...)
        /// </summary>
        public bool IsReturnFormatted { get; set; }

        /// <summary>
        /// Is the ThreeViewItem of this trace record expanded
        /// </summary>
        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Is this trace record a own function (FunctionType = '1') and should it be highlighted in the TreeView
        /// </summary>
        private bool highlightOwn;
        public bool HighlightOwn
        {
            get => highlightOwn;
            set
            {
                if (FunctionType == 0) return;
                
                highlightOwn= value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="traceSplit"></param>
        public XdebugTraceItem(ref string[] traceSplit)
        {
            if (traceSplit[2] == "0")
                ParseEntry(ref traceSplit);
            else if (traceSplit[2] == "1")
                ParseExit(ref traceSplit);
            else if (traceSplit[2] == "R") ParseReturn(ref traceSplit);
        }

        /// <summary>
        /// Parse a 'Entry' trace record
        /// </summary>
        /// <param name="traceSplit"></param>
        private void ParseEntry(ref string[] traceSplit)
        {
            Level = int.Parse(traceSplit[0]);
            FunctionNumber = int.Parse(traceSplit[1]);
            Type = traceSplit[2];
            TimeIndex = traceSplit[3];
            MemoryUsage = traceSplit[4];
            FunctionName = traceSplit[5];
            FunctionType = int.Parse(traceSplit[6]);
            IncludeName = traceSplit[7];
            File = traceSplit[8];
            Line = traceSplit[9];
            ParameterCount = int.Parse(traceSplit[10]);
            Parameters = new string[ParameterCount];

            for (var i = 11; i < traceSplit.Length; i++)
            {
                Parameters[i - 11] = traceSplit[i];
            }

            IsExpanded = false;
            IsReturnFormatted = false;
            HighlightOwn = true;
        }

        /// <summary>
        /// Parse a 'Exit' trace record
        /// </summary>
        /// <param name="traceSplit"></param>
        private void ParseExit(ref string[] traceSplit)
        {
            Level = int.Parse(traceSplit[0]);
            FunctionNumber = int.Parse(traceSplit[1]);
            Type = traceSplit[2];
            TimeIndex = traceSplit[3];
            MemoryUsage = traceSplit[4];

        }

        /// <summary>
        /// Parse a 'Return' trace record
        /// </summary>
        /// <param name="traceSplit"></param>
        private void ParseReturn(ref string[] traceSplit)
        {
            Level = int.Parse(traceSplit[0]);
            FunctionNumber = int.Parse(traceSplit[1]);
            Type = traceSplit[2];
            SetReturnValue(ref traceSplit);
        }

        /// <summary>
        /// Calculate the execution time
        /// </summary>
        /// <param name="traceSplit"></param>
        public void SetExecutionTime(ref string[] traceSplit)
        {
            var entryTime = decimal.Parse(TimeIndex, CultureInfo.InvariantCulture);
            var exitTime = decimal.Parse(traceSplit[3], CultureInfo.InvariantCulture);

            ExecutionTime = (exitTime - entryTime).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Set the return value of this function
        /// </summary>
        /// <param name="traceSplit"></param>
        public void SetReturnValue(ref string[] traceSplit) => ReturnValue = traceSplit[5];

        /// <summary>
        /// Expand the corresponding item of this record in the TreeView
        /// </summary>
        private void Expand() => IsExpanded = true;

        /// <summary>
        /// Collaps the corresponding item of this record in the TreeView
        /// </summary>
        private void Collaps() => IsExpanded = false;


        /// <summary>
        /// Expand the corresponding item of this record in the TreeView and all its SubElements
        /// </summary>
        public void ExpandAll()
        {
            Expand();
            if (SubElements == null) return;

            foreach (var xdebugTraceItem in SubElements)
            {
                xdebugTraceItem.ExpandAll();
            }
        }

        /// <summary>
        /// Collaps the corresponding item of this record in the TreeView and all its SubElements
        /// </summary>
        public void CollapseAll()
        {
            Collaps();
            if (SubElements == null) return;

            foreach (var xdebugTraceItem in SubElements)
            {
                xdebugTraceItem.CollapseAll();
            }
        }

        /// <summary>
        /// Try to format (intend, pretty print, ...) the ReturnValue
        /// </summary>
        public void FormatReturnValue()
        {
            if (IsReturnFormatted) return;
            if (!string.IsNullOrEmpty(ReturnValueFormatted))
            {
                IsReturnFormatted = true;
                return;
            }
            if (ReturnValue == null) return;

            ReturnValueFormatted = ReturnValue;
            
            ReturnValueFormatted = ReturnValueFormatted.Trim('\'');
            AddNewLineCharacters();

            var indentCount = 0;
            var formatedReturn = "";
            using (var reader = new StringReader(ReturnValueFormatted))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length <= 0) continue;

                    var tabs = new string('\t', indentCount);
                    
                    switch (line[0])
                    {
                        case '[':
                        case '{':
                        case '(':
                            tabs = new string('\t', indentCount);
                            formatedReturn += tabs + line + Environment.NewLine;
                            indentCount++;
                            continue;
                        case ']':
                        case '}':
                        case ')':
                            indentCount--;
                            if (indentCount < 0) indentCount = 0;
                            tabs = new string('\t', indentCount);
                            formatedReturn += tabs + line + Environment.NewLine;
                            continue;
                        default:
                            formatedReturn += tabs + line + Environment.NewLine;
                            continue;
                    }
                }
            }

            ReturnValueFormatted = formatedReturn;
            IsReturnFormatted = true;
        }

        /// <summary>
        /// Add some newline characters to the one-liner ReturnValue
        /// </summary>
        private void AddNewLineCharacters()
        {
            if (ReturnValueFormatted[0] == '#')
            {
                ReturnValueFormatted = ReturnValueFormatted.Replace("\\n", "\n");
                return;
            }

            ReturnValueFormatted = ReturnValueFormatted.Replace("; ", "; " + Environment.NewLine);
            
            ReturnValueFormatted = ReturnValueFormatted.Replace("array (", "array" + Environment.NewLine + "(" + Environment.NewLine);
            
            ReturnValueFormatted = ReturnValueFormatted.Replace(")", Environment.NewLine + ")");
            ReturnValueFormatted = ReturnValueFormatted.Replace("("+Environment.NewLine+")", "()");
            ReturnValueFormatted = ReturnValueFormatted.Replace(Environment.NewLine + ")'", ")'");
            
            ReturnValueFormatted = ReturnValueFormatted.Replace(" {", Environment.NewLine + "{" + Environment.NewLine);
            ReturnValueFormatted = ReturnValueFormatted.Replace("}", Environment.NewLine + "}");
            ReturnValueFormatted = ReturnValueFormatted.Replace("} ", "} " + Environment.NewLine);

            
            ReturnValueFormatted = ReturnValueFormatted.Replace(",", "," + Environment.NewLine);
            ReturnValueFormatted = ReturnValueFormatted.Replace(";", ";" + Environment.NewLine);

            ReturnValueFormatted = ReturnValueFormatted.Replace("}" + Environment.NewLine + ";", Environment.NewLine + "};");
            ReturnValueFormatted = ReturnValueFormatted.Replace("}" + Environment.NewLine + ",", Environment.NewLine + "},");


            ReturnValueFormatted = ReturnValueFormatted.Replace("[", Environment.NewLine + "[" + Environment.NewLine);
            ReturnValueFormatted = ReturnValueFormatted.Replace("]", Environment.NewLine + "]" + Environment.NewLine);
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
    }
}
