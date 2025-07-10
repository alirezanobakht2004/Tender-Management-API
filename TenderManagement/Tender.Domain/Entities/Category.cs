using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tender.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;

    internal Category() { }

    public Category(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
