using System.Data;
using System.IO;
using System.Linq;

namespace Jace.Benchmark.Properties.ResultWriters;

public class CsvResultWriter : IResultWriter
{
    private readonly string fileName;
    
    public CsvResultWriter(string fileName)
    {
        this.fileName = fileName;
    }
    
    public void Write(DataTable resultTable)
    {
            using var writer = new StreamWriter(fileName);

            // Write header
            writer.WriteLine(string.Join(";", resultTable.Columns.Cast<DataColumn>().Select(col => col.ColumnName)));

            // Write data
            foreach (DataRow row in resultTable.Rows)
            {
                writer.WriteLine(string.Join(";", row.ItemArray));
            }
    }
}