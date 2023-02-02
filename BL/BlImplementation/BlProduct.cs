﻿using DalApi;
using System.Security.Cryptography;

namespace BlImplementation;
internal class BlProduct : BlApi.IProduct
{
    DalApi.IDal? dal = DalApi.Factory.Get();
    /// <summary>
    /// Copy the list of Products from DataSource to a list of BO.ProductForList and return the list
    /// </summary>
    /// <returns></returns>
    public IEnumerable<BO.ProductForList?> GetProductForLists(Func<BO.ProductForList?, bool>? func = null)
    {
        IEnumerable<BO.ProductForList?> productsForList = dal?.Product.GetList().Select(
            p=> new BO.ProductForList
            {
            ID = (int)p?.Id!,
            Name = p?.Name,
            Price = (double)p?.Price!,
            Category = (BO.Category)p?.Category!
            }
        )!;

        //foreach (var item in dal?.Product.GetList())// dont necessery to transform with Linq to Object beacause theire is for this functon any waist of time
        //{
        //    BO.ProductForList p = new BO.ProductForList()
        //    {
        //        ID = (int)item?.Id,
        //        Name = item?.Name,
        //        Price = (double)item?.Price,
        //        Category = (BO.Category)item?.Category
        //    };
        //    productsForList.Add(p);
        //}
        if (func != null)
        {
            //Predicate<BO.ProductForList?> predicate1 = (Prod) => func(Prod);
            //var newList = productsForList.FindAll(predicate1);
            return productsForList.Where(p=>func(p));
            //return newList;
        }
        return productsForList.OrderBy(p=>p?.ID);
    }

    /// <summary>
    /// Check if the ID of the Product exist and is valid for to return the Product or not for the director
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    public BO.Product GetDirector(int productId)
    {
        if (productId < 0)
            throw new BO.IdNotValidExcpetion(" Impossible negative Id! ");
        var prod=from item in (List<DO.Products?>)dal?.Product.GetList()
                 where item?.Id == productId
                 select item;
        //DO.Products products = (DO.Products)prod.First();
        if (prod.Count()!=0)
        {
                BO.Product product = new BO.Product()
                {
                    ID = (int)prod.First()?.Id,
                    Name = prod.First()?.Name,
                    Price = (double)prod.First()?.Price,
                    Category = (BO.Category)prod.First()?.Category,
                    InStock = (int)prod.First()?.InStock
                };
                return product;
        }
        throw new BO.NoExistingItemException("Product doesn't exist!"); // Positive ID but not exist in the list of Products
    }

    /// <summary>
    /// Check if the ID of the Product exist and is valid for to return the Product or not for the client
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    public BO.ProductItem GetClient(int productId, BO.Cart cart)
    {
        if (productId < 0)
            throw new BO.IdNotValidExcpetion("Invalid ID!");
        var prod = from item in (List<DO.Products?>)dal?.Product.GetList()
                   where item?.Id == productId
                   select item;
        if (prod.Count()!=0)
        {
                int index = cart.orderItems.FindIndex(OrderItems => OrderItems.ProductID == productId);
                BO.ProductItem productItem = new BO.ProductItem()
                {
                    ID = productId,
                    Name = prod.First()?.Name,
                    Price = (double)prod.First()?.Price,
                    Category = (BO.Category)prod.First()?.Category,
                    Amount = cart.orderItems[index].Amount,
                };
                if (prod.First()?.InStock > 0)
                    productItem.InStock = true;
                else
                    productItem.InStock = false;
                return productItem;
        }
        throw new BO.NoExistingItemException("Product doesn't exist!"); // Positive ID but not exist in the list of Products
    }

    /// <summary>
    /// Test if the data of the product is right for to ad him in the list of Products in the DataSource or not
    /// </summary>
    /// <param name="product"></param>
    public void Add(BO.Product product)
    {
        if (product.ID < 0)
            throw new BO.IdNotValidExcpetion("Invalid ID!");
        if (product.Name == "")
            throw new BO.NameNotValidExcpetion("Invalid name!");
        if (product.Price < 0)
            throw new BO.PriceNotValidExcpetion("Invalid Price!");
        if (product.InStock <= 0)
            throw new BO.StockNotValidExcpetion("Invalid stock!");
        if ((bool)dal?.Product.GetList().ToList().Exists(Product => Product?.Id == product.ID))
            throw new BO.ItemAlreadyExistException("this product already exist");
        DO.Products products = new DO.Products()
        {
            Id = product.ID,
            Name = product.Name,
            Price = product.Price,
            Category = (DO.Category)product.Category!,
            InStock = product.InStock
        };
        dal?.Product.Add(products);
    }

    /// <summary>
    /// Delete a Product from the list of Products in the DataSource if its possible
    /// </summary>
    /// <param name="id"></param>
    public void Delete(int productId)
    {
        
        if ((bool)dal?.OrderItem.GetList().ToList().Exists(OrderItem=> OrderItem?.ProductId==productId))
        {
            throw new BO.DeleteItemNotValidExcpetion("Impossible to delete product!");
        }
        if ((bool)!dal?.Product.GetList().ToList().Exists(Product => Product?.Id == productId))
        {
            throw new BO.NoExistingItemException("Product mot exist");
        }
        var listOfProduct = from item in (List<BO.ProductForList?>)GetProductForLists()
                            where item?.ID == productId
                            select item;
        if (listOfProduct.Count()!=0)
        {
                dal?.Product.Delete(productId, 0);
        }
    }

    /// <summary>
    /// Update directly the product in the DataSource
    /// </summary>
    /// <param name="productId"></param>
    public void Update(BO.Product product)
    {
        if (product.ID < 0)
            throw new BO.IdNotValidExcpetion("Invalid ID!");
        if (product.Name == "")
            throw new BO.NameNotValidExcpetion("Invalid name!");
        if (product.Price < 0)
            throw new BO.PriceNotValidExcpetion("Invalid Price!");
        if (product.InStock < 0)
            throw new BO.StockNotValidExcpetion("Invalid stock!");
        var prod = from item in (List<DO.Products?>)dal?.Product.GetList()
                   where item?.Id == product.ID
                   select item;
        if (prod.Count()!=0)
        {
                DO.Products products = new DO.Products()
                {
                    Id = product.ID,
                    Name = product.Name,
                    Price = product.Price,
                    Category = (DO.Category)product.Category!,
                    InStock = product.InStock
                };
                dal?.Product.Add(products);
                dal?.Product.Update(products.Id, 0);
                return;
        }
                throw new BO.NoExistingItemException("Product doesn't exist");
    }
}