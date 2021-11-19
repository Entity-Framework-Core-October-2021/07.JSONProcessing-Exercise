using System;
using System.Collections.Generic;
using System.Text;

namespace ProductShop.Dtos.Output
{
    public class ProductsOutputDto
    {
        public int Count { get; set; }

        public List<ProductOutputDto> Products { get; set; }
    }
}
