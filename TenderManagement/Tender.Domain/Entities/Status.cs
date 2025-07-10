using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tender.Domain.Entities;

public class Status : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Scope { get; private set; } = default!;
    public int SortOrder { get; private set; }

    internal Status() { }

    public Status(string name, string scope, int sortOrder)
    {
        Name = name;
        Scope = scope;
        SortOrder = sortOrder;
    }
}