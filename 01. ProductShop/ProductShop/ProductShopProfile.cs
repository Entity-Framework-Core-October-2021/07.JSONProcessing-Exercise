using AutoMapper;
using ProductShop.Dtos.Input;
using ProductShop.Dtos.Output;
using ProductShop.Models;
using System.Linq;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            CreateMap<UserInputDto, User>();
            CreateMap<ProductInputDto, Product>();
            CreateMap<CategoryInputDto, Category>();
            CreateMap<CategoryAndProductInputDto, CategoryProduct>();

            CreateMap<Product, ProductOutputDto>()
                .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => $"{src.Seller.FirstName} {src.Seller.LastName}"));

            CreateMap<User, UserSoldProductOutputDto>()
                .ForMember(dest => dest.SoldProducts, opt => opt.MapFrom(src => src.ProductsSold));

            CreateMap<Product, SoldProductOutputDto>()
                .ForMember(dest => dest.BuyerFirstName, opt => opt.MapFrom(src => src.Buyer.FirstName))
                .ForMember(dest => dest.BuyerLastName, opt => opt.MapFrom(src => src.Buyer.LastName));

            CreateMap<Category, CategoryProductsOutputDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ProductsCount, opt => opt.MapFrom(src => src.CategoryProducts.Count))
                .ForMember(dest => dest.AveragePrice, opt => opt.MapFrom(src => $"{(src.CategoryProducts.Sum(cp => cp.Product.Price) / src.CategoryProducts.Count):F2}"))
                .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => $"{src.CategoryProducts.Sum(c => c.Product.Price)}"));
        }
    }
}
