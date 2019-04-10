using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace XdbgTraceViewer
{
    public static class XdebugTrace
    {
        /// <summary>
        /// Stream reader for the selected trace file
        /// </summary>
        private static StreamReader traceFile;

        /// <summary>
        /// Parse the first line of the tace file
        /// </summary>
        /// <param name="traceFilePath"></param>
        /// <returns></returns>
        public static ObservableCollection<XdebugTraceItem> ReadTraceFile(string traceFilePath)
        {
            traceFile = File.OpenText(traceFilePath);
            var traces = new ObservableCollection<XdebugTraceItem>();

            XdebugTraceItem firstTraceItem = null;
            var firstValidRecordNotFound = true;
            while (firstValidRecordNotFound)
            {
                var recordLine = traceFile.ReadLine();
                var traceRecord = SplitRecord(ref recordLine);
                if (traceRecord == null) return traces;
                if(traceRecord.Length == 0) continue;

                firstTraceItem = new XdebugTraceItem(ref traceRecord) {IsExpanded = true};
                firstValidRecordNotFound = false;
            }
            
            traces.Add(firstTraceItem);

            ParseRemainingFileContent(ref traces);

            traceFile.Close();

            return traces;
        }

        /// <summary>
        /// Split one trace record up in its single fields
        /// </summary>
        /// <param name="traceRecord">one single line of the trace file</param>
        /// <returns></returns>
        private static string[] SplitRecord(ref string traceRecord)
        {
            if(traceRecord == null) return new string[0];

            var traceSplit = traceRecord.Split('\t');

            if (traceSplit.Length < 3 ||
                traceSplit[0] == "" ||
                traceSplit[0] == "TRACE START" ||
                traceSplit[0] == "TRACE END")
            {

                traceSplit = new string[0];
            }

            return traceSplit;
        }

        /// <summary>
        /// Parse all remaining lines of the trace file
        /// </summary>
        /// <param name="traces"></param>
        /// <returns></returns>
        private static string[] ParseRemainingFileContent(ref ObservableCollection<XdebugTraceItem> traces)
        {
            var record = new string[0];
            var doLoop = true;
            while (doLoop)
            {
                if (record.Length <= 0)
                {
                    string recordLine;
                    doLoop = (recordLine = traceFile.ReadLine()) != null;
                    if (!doLoop) return new string[0];
                    
                    record = SplitRecord(ref recordLine);
                    if (record.Length == 0) break;
                }
                
                var traceCounter = traces.Count;

                if (int.Parse(record[0]) < traces[traceCounter - 1].Level) return record;

                var traceItem = new XdebugTraceItem(ref record);

                if (traceItem.Level == traces[traceCounter - 1].Level)
                {
                    switch (record[2])
                    {
                        case "1":
                            traces[traceCounter - 1].SetExecutionTime(ref record);
                            record = new string[0];
                            continue;
                        case "R":
                            traces[traceCounter - 1].SetReturnValue(ref record);
                            record = new string[0];
                            continue;
                    }

                    traces.Add(traceItem);
                    record = new string[0];
                    continue;
                }

                var newSubElement = new ObservableCollection<XdebugTraceItem> {traceItem};
                record = ParseRemainingFileContent(ref newSubElement);

                traces[traceCounter - 1].SubElements = newSubElement;
            }

            return new string[0];
        }
    }
}
