using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Jace.Benchmark.Properties.ResultWriters;

public class ConsoleResultWriter : IResultWriter
{
    public void Write(DataTable resultTable)
    {
    var columnCount = resultTable.Columns.Count;

    // Calculate column widths
    var maxLengths = new int[columnCount];
    for (var i = 0; i < columnCount; i++)
    {
        maxLengths[i] = Math.Max(resultTable.Columns[i].ColumnName.Length, resultTable.AsEnumerable().Max(row => row[i].ToString().Length));
    }

    // Line length is the sum of all column widths + two spaces and one separator for each column + 1 separator at the end
    var totalLineLength = maxLengths.Sum() + columnCount * 3 + 1;

    // Separator line
    var separatorLine = new string('-', totalLineLength);

    // Header
    Console.WriteLine();
    Console.WriteLine(separatorLine);
    Console.WriteLine(GetFormattedRow(resultTable.Columns.Cast<DataColumn>().Select(col => col.ColumnName), maxLengths));
    Console.WriteLine(separatorLine);

    // Data
    foreach (DataRow row in resultTable.Rows)
    {
        Console.WriteLine(GetFormattedRow(row.ItemArray, maxLengths));
    }

    Console.WriteLine(separatorLine);
    Console.WriteLine();
    }
    
private static string GetFormattedRow(IEnumerable<object> values, int[] maxLengths)
{
    var formattedValues = new List<string>();
    for (var i = 0; i < maxLengths.Length; i++)
    {
        var formattedValue = values.ElementAt(i).ToString();

        // If the value is numeric, format with decimal separator and pad left; otherwise, pad right
        if (IsNumeric(formattedValue))
        {
            var numericValue = double.Parse(formattedValue, CultureInfo.InvariantCulture);
            formattedValue = numericValue.ToString("N0", CultureInfo.CurrentCulture).PadLeft(maxLengths[i]);
        }
        else
        {
            formattedValue = formattedValue.PadRight(maxLengths[i]);
        }

        formattedValues.Add(formattedValue);
    }
    return "| " + string.Join(" | ", formattedValues) + " |";
}

private static bool IsNumeric(string value)
{
    return double.TryParse(value, out _);
}

}