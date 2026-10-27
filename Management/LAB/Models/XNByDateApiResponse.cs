using System;
using System.Collections.Generic;

namespace Management.Models
{
    // Không khai báo lại XNServiceByDateReport/XNServiceByDateRow đang có trong Models.
    public sealed class XNByDateApiResponse
    {
        public bool Success { get; set; } = true;
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TotalRows { get; set; }
        public List<XNServiceByDateGroup> DateGroups { get; set; } = new List<XNServiceByDateGroup>();
    }
}
