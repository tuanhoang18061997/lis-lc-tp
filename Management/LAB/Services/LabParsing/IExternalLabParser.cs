using Management.Models;
using System.Collections.Generic;

namespace Management.Services.LabParsing
{
    public interface IExternalLabParser
    {
        string Name { get; }            // Tên vendor/layout
        List<ExternalLabItem> Parse(string pdfText);
    }
}
