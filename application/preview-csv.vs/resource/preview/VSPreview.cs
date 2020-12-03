
using Csv;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace resource.preview
{
    internal class VSPreview : cartridge.AnyPreview
    {
        protected override void _Execute(atom.Trace context, string url)
        {
            if (File.Exists(url))
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
                    var a_Context1 = File.ReadAllText(url);
                    {
                        var a_Context2 = CsvReader.ReadFromText(a_Context1, a_Context);
                        {
                            context.
                                SetState(NAME.STATE.HEADER).
                                Send(NAME.SOURCE.PREVIEW, NAME.TYPE.FOLDER, 1, "[[Info]]");
                            {
                                context.Send(NAME.SOURCE.PREVIEW, NAME.TYPE.VARIABLE, 2, "[[File Name]]", url);
                                context.Send(NAME.SOURCE.PREVIEW, NAME.TYPE.VARIABLE, 2, "[[File Size]]", a_Context1.Length.ToString());
                                context.Send(NAME.SOURCE.PREVIEW, NAME.TYPE.VARIABLE, 2, "[[Row Count]]", __GetRowCount(a_Context2));
                            }
                        }
                        if (a_Context2 != null)
                        {
                            __Execute(a_Context2, 1, context, url, GetProperty(NAME.PROPERTY.PREVIEW_TABLE_SIZE));
                        }
                    }
                }
            }
            else
            {
                context.
                    SendError(1, "[[File not found]]");
            }
        }

        private static void __Execute(IEnumerable<ICsvLine> node, int level, atom.Trace context, string url, int limit)
        {
            var a_Index = 0;
            foreach (var a_Context in node)
            {
                if (GetState() == STATE.CANCEL)
                {
                    context.
                        SendWarning(level, NAME.WARNING.TERMINATED);
                    return;
                }
                else
                {
                    a_Index++;
                }
                if (a_Index > limit)
                {
                    context.
                        SendWarning(level, NAME.WARNING.DATA_SKIPPED);
                    return;
                }
                else
                {
                    __Execute(a_Context, level, context, a_Index, url);
                }
            }
        }

        private static void __Execute(ICsvLine node, int level, atom.Trace context, int index, string url)
        {
            if ((index == 1) && __IsCaption(node))
            {
                context.
                    SetComment("<[[Header]]>").
                    SetCommentHint("<[[Row type]]>").
                    SetState(NAME.STATE.HEADER);
            }
            else
            {
                context.
                    SetComment("[" + index.ToString("D4") + "]").
                    SetCommentHint("[[[Row number]]]");
            }
            {
                context.
                    SetUrl(url).
                    SetUrlLine(node.Index).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.TABLE, level, "ROW");
            }
            foreach (var a_Context in node.Values)
            {
                context.
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.TABLE, level + 1, __GetName(a_Context));
            }
        }

        private static bool __IsCaption(ICsvLine node)
        {
            var a_Result = true;
            foreach (var a_Context in node.Values)
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

        private static string __GetName(string value)
        {
            var a_Result = GetCleanString(value);
            if (string.IsNullOrWhiteSpace(value))
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

        private static string __GetRowCount(IEnumerable<ICsvLine> context)
        {
            var a_Context1 = 0;
            var a_Context2 = 0;
            foreach (var a_Context in context)
            {
                a_Context1++;
                a_Context2 = Math.Max(a_Context2, a_Context.ColumnCount);
            }
            return a_Context1.ToString();
        }
    };
}
