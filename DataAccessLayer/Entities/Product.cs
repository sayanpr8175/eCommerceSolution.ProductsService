using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities;

public class Product
{
    [Key]
    public Guid ProductID { get; set; }
    public string ProductName { get; set; }
    public string Category { get; set; }
    public double? UnitPrice { get; set; }
    public int? QuantityInStock { get; set; }
}
