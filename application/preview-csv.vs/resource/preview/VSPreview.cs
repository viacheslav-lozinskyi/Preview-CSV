using Csv;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace resource.preview
{
    internal class VSPreview : extension.AnyPreview
    {
        protected override void _Execute(atom.Trace context, int level, string url, string file)
        {
            var a_Context = new CsvOptions
            {
                RowsToSkip = 0,
                SkipRow = (row, idx) => string.IsNullOrEmpty(row) || row[0] == '#',
                Separator = '\0',
                TrimData = true,
                Comparer = null,
                HeaderMode = HeaderMode.HeaderAbsent,
                ValidateColumnCount = false,
                ReturnEmptyForMissingColumn = true,
                Aliases = null,
                AllowNewLineInEnclosedFieldValues = false,
                AllowBackSlashToEscapeQuote = true,
                AllowSingleQuoteToEncloseFieldValues = true,
                NewLine = Environment.NewLine
            };
            {
                var a_Context1 = File.ReadAllText(file);
                {
                    var a_Context2 = CsvReader.ReadFromText(a_Context1, a_Context);
                    {
                        context.Send(NAME.SOURCE.PREVIEW, NAME.EVENT.HEADER, level, "[[[Info]]]");
                        {
                            context.Send(NAME.SOURCE.PREVIEW, NAME.EVENT.PARAMETER, level + 1, "[[[File Name]]]", url);
                            context.Send(NAME.SOURCE.PREVIEW, NAME.EVENT.PARAMETER, level + 1, "[[[File Size]]]", a_Context1.Length.ToString());
                            context.Send(NAME.SOURCE.PREVIEW, NAME.EVENT.PARAMETER, level + 1, "[[[Row Count]]]", __GetRowCount(a_Context2));
                        }
                    }
                    if (a_Context2 != null)
                    {
                        __Execute(context, level, a_Context2, file, GetProperty(NAME.PROPERTY.PREVIEW_TABLE_SIZE, true));
                    }
                }
            }
        }

        private static void __Execute(atom.Trace context, int level, IEnumerable<ICsvLine> data, string file, int limit)
        {
            var a_Index = 0;
            foreach (var a_Context in data)
            {
                if (GetState() == NAME.STATE.CANCEL)
                {
                    return;
                }
                else
                {
                    a_Index++;
                }
                if (a_Index > limit)
                {
                    context.
                        Send(NAME.SOURCE.PREVIEW, NAME.EVENT.WARNING, level, "...");
                    return;
                }
                else
                {
                    __Execute(context, level, a_Context, a_Index, file);
                }
            }
        }

        private static void __Execute(atom.Trace context, int level, ICsvLine data, int index, string file)
        {
            if ((index == 1) && __IsCaption(data))
            {
                context.
                    SetControl(NAME.CONTROL.TABLE).
                    SetUrl(file, data.Index, 0).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.HEADER, level, "HEADER");
            }
            else
            {
                context.
                    SetControl(NAME.CONTROL.TABLE).
                    SetUrl(file, data.Index, 0).
                    SetCount(Math.Max(data.Values.Length, 50)).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.CONTROL, level, "ROW");
            }
            foreach (var a_Context in data.Values)
            {
                context.
                    SetControl(NAME.CONTROL.TABLE).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.CONTROL, level + 1, __GetName(a_Context));
            }
        }

        private static bool __IsCaption(ICsvLine data)
        {
            var a_Result = true;
            foreach (var a_Context in data.Values)
            {
                var a_Context1 = __GetName(a_Context);
                var a_Context2 = 0.0;
                if (string.IsNullOrEmpty(a_Context1) || double.TryParse(a_Context1, NumberStyles.Any, CultureInfo.InvariantCulture, out a_Context2))
                {
                    return false;
                }
            }
            return a_Result;
        }

        private static string __GetName(string data)
        {
            var a_Result = GetFinalText(data);
            if (string.IsNullOrWhiteSpace(data))
            {
                return "";
            }
            if (a_Result.Length >= 2)
            {
                var a_Index = a_Result.Length - 1;
                if ((a_Result[0] == '\"') && (a_Result[a_Index] == '\"'))
                {
                    a_Result = a_Result.Substring(1, a_Index - 1);
                }
                if ((a_Result[0] == '\'') && (a_Result[a_Index] == '\''))
                {
                    a_Result = a_Result.Substring(1, a_Index - 1);
                }
            }
            return a_Result.Trim();
        }

        private static string __GetRowCount(IEnumerable<ICsvLine> data)
        {
            var a_Context1 = 0;
            var a_Context2 = 0;
            foreach (var a_Context in data)
            {
                a_Context1++;
                a_Context2 = Math.Max(a_Context2, a_Context.ColumnCount);
            }
            return a_Context1.ToString();
        }
    };
}
