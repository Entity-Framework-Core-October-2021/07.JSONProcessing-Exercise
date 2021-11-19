using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductShop.Dtos.Output
{
    public class UserSoldProductOutputDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public ICollection<SoldProductOutputDto> SoldProducts { get; set; }
    }
}
