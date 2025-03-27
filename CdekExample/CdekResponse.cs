using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CdekExample;

public class CdekResponse
{
    public dynamic Data { get; set; }
    public IEnumerable<(string name, string value)> Headers { get; set; }
}