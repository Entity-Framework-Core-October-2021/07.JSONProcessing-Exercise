using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProductShop.Data;
using ProductShop.Dtos.Input;
using ProductShop.Dtos.Output;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        private static IMapper mapper;
        private static string ResultDirectoryPath = "../../../Datasets/Results";
        public static void Main(string[] args)
        {
            ProductShopContext context = new ProductShopContext();

            //ResetDatabase(context);

            ////01. Import Users - AutoMapper
            //string usersJsonAsString = File.ReadAllText("../../../Datasets/users.json");
            //Console.WriteLine(ImportUsers(context, usersJsonAsString));

            ////02. Import Products
            //string productsJsonAsString = File.ReadAllText("../../../Datasets/products.json");
            //Console.WriteLine(ImportProducts(context, productsJsonAsString));

            ////03. Import Categories
            //string categoriesJsonAsString = File.ReadAllText("../../../Datasets/categories.json");
            //Console.WriteLine(ImportCategories(context, categoriesJsonAsString));

            ////04. Import Categories and Products
            //string categoriesAndProductsJsonAsString = File.ReadAllText("../../../Datasets/categories-products.json");
            //Console.WriteLine(ImportCategoryProducts(context, categoriesAndProductsJsonAsString));

            ////05. Export Products In Range
            //string json = GetProductsInRange(context);

            //Console.WriteLine(json);

            //if (!Directory.Exists(ResultDirectoryPath))
            //{
            //    Directory.CreateDirectory(ResultDirectoryPath);
            //}

            //File.WriteAllText(ResultDirectoryPath + "/products-in-range.json", json);


            //06. Export Sold Products
            //Console.WriteLine(GetSoldProducts(context));

            //07. Export Categories By Products Count
            //Console.WriteLine(GetCategoriesByProductsCount(context));

            //08. Export Users and Products
            Console.WriteLine(GetUsersWithProducts(context));
        }



        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            IEnumerable<UserInputDto> users = JsonConvert.DeserializeObject<IEnumerable<UserInputDto>>(inputJson);

            InitializeMapper();

            IEnumerable<User> mappedUsers = mapper.Map<IEnumerable<User>>(users);

            context.Users.AddRange(mappedUsers);

            context.SaveChanges();

            return $"Successfully imported {mappedUsers.Count()}";
        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            IEnumerable<ProductInputDto> products = JsonConvert.DeserializeObject<IEnumerable<ProductInputDto>>(inputJson);
            InitializeMapper();

            IEnumerable<Product> mappedProducts = mapper.Map<IEnumerable<Product>>(products);
            context.Products.AddRange(mappedProducts);
            context.SaveChanges();

            return $"Successfully imported {mappedProducts.Count()}";
        }

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            IEnumerable<CategoryInputDto> categories =
                JsonConvert.DeserializeObject<IEnumerable<CategoryInputDto>>(inputJson)
                .Where(x => !string.IsNullOrEmpty(x.Name));

            InitializeMapper();

            IEnumerable<Category> mappedCategories = mapper.Map<IEnumerable<Category>>(categories);

            context.Categories.AddRange(mappedCategories);

            context.SaveChanges();

            return $"Successfully imported {mappedCategories.Count()}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            IEnumerable<CategoryAndProductInputDto> categoriesAndProducts = JsonConvert.DeserializeObject<IEnumerable<CategoryAndProductInputDto>>(inputJson);

            InitializeMapper();

            IEnumerable<CategoryProduct> mappedCategoriesAndProducts = mapper.Map<IEnumerable<CategoryProduct>>(categoriesAndProducts);

            context.CategoryProducts.AddRange(mappedCategoriesAndProducts);
            context.SaveChanges();

            return $"Successfully imported {mappedCategoriesAndProducts.Count()}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            //with select
            //var products = context.Products
            //                             .Include(x=> x.Seller)
            //                             .Where(x => x.Price >= 500 && x.Price <= 1000)
            //                             .OrderBy(x => x.Price)
            //                             .Select(x => new ProductOutputDto
            //                             {
            //                                 Name = x.Name,
            //                                 Price = x.Price,
            //                                 Seller = $"{x.Seller.FirstName} {x.Seller.LastName}"
            //                             })
            //                             .ToList();

            //AutoMapper
            InitializeMapper();

            //var products = context.Products
            //                            .Include(x=> x.Seller)
            //                            .Where(x => x.Price >= 500 && x.Price <= 1000)
            //                            .OrderBy(x => x.Price)
            //                            .Select(x => new ProductOutputDto
            //                            {
            //                                Name = x.Name,
            //                                Price = x.Price,
            //                                Seller = $"{x.Seller.FirstName} {x.Seller.LastName}"
            //                            })
            //                            .ToList();

            //var mappedResult = mapper.Map<IEnumerable<ProductOutputDto>>(products);

            //ProjectTo
            var products = context
                .Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .OrderBy(x => x.Price)
                .ProjectTo<ProductOutputDto>(mapper.ConfigurationProvider) // <--- mapper profile configurations
                .ToList();

            JsonSerializerSettings jsonSettings = JsonSettings();

            string productsAsJson = JsonConvert.SerializeObject(products, jsonSettings);

            return productsAsJson;
        }


        public static string GetSoldProducts(ProductShopContext context)
        {
            InitializeMapper();

            var usersWithSoldProducts = context.Users
                                     .Include(p => p.ProductsSold)
                                     .Where(x => x.ProductsSold.Any(y => y.Buyer != null))
                                     .OrderBy(x => x.LastName)
                                     .ThenBy(x => x.FirstName)
                                     .Select(x => new UserSoldProductOutputDto
                                     {
                                         FirstName = x.FirstName,
                                         LastName = x.LastName,
                                         SoldProducts = x.ProductsSold.Select(p => new SoldProductOutputDto
                                         {
                                             Name = p.Name,
                                             Price = p.Price,
                                             BuyerFirstName = p.Buyer.FirstName,
                                             BuyerLastName = p.Buyer.LastName
                                         })
                                                        .ToList()
                                     })
                                     .ToList();

            JsonSerializerSettings jsonSettings = JsonSettings();

            string usersWithSoldProductsAsJson = JsonConvert.SerializeObject(usersWithSoldProducts, jsonSettings);

            return usersWithSoldProductsAsJson;
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            InitializeMapper();

            var result = context
              .Categories
              .OrderByDescending(x => x.CategoryProducts.Count)
              .ProjectTo<CategoryProductsOutputDto>(mapper.ConfigurationProvider)
              .ToList();

            JsonSerializerSettings jsonSettings = JsonSettings();

            string resultsAsJson = JsonConvert.SerializeObject(result, jsonSettings);

            return resultsAsJson;
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            InitializeMapper();

            var users = context.Users
                  .Include(x => x.ProductsSold)
                  .ToList()
                  .Where(x => x.ProductsSold.Any(b => b.Buyer != null))
                  .Select(x => new UserProductsOutputDto
                  {
                      FirstName = x.FirstName,
                      LastName = x.LastName,
                      Age = x.Age,
                      SoldProducts = new ProductsOutputDto
                      {
                          Count = x.ProductsSold
                          .Where(p => p.Buyer != null)
                          .Count(),

                          Products = x.ProductsSold
                          .Where(p => p.Buyer != null)
                          .Select(p => new ProductOutputDto
                          {
                              Name = p.Name,
                              Price = p.Price,
                          })
                          .ToList()
                      }
                  })
                  .OrderByDescending(x => x.SoldProducts.Count)
                  .ToList();

            var result = new UsersWithSoldProductsOutputDto
            {
                UsersCount = users.Count(),
                Users = users
            };

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = contractResolver,
                NullValueHandling = NullValueHandling.Ignore
            };
            string resultAsJson = JsonConvert
               .SerializeObject(result,
               jsonSettings);

            return resultAsJson;
        }

        private static void InitializeMapper()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });
            mapper = new Mapper(mapperConfiguration);
        }
        private static JsonSerializerSettings JsonSettings()
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = contractResolver
            };
            return jsonSettings;
        }

        private static void ResetDatabase(ProductShopContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }

    }
}