using System.Data;

namespace Adletec.Sonic.Benchmark.ResultWriters;

public interface IResultWriter
{
    /// <summary>
    /// Creates an implementation-specific output of the given data table.
    /// </summary>
    /// <param name="resultTable">A data table of arbitrary dimensions.</param>
    public void Write(DataTable resultTable);
}