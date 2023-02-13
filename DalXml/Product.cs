﻿using DAL;
using DalApi;
using DO;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Data.Common;
using System.Reflection.Metadata.Ecma335;
using System.Linq;

namespace Dal;
internal class Product : IProduct
{
    XElement ProductRoot;

    public string ProductPath;
    public Product()
    {
        string localPath;
        string str = Assembly.GetExecutingAssembly().Location;
        localPath = Path.GetDirectoryName(str);
        localPath = Path.GetDirectoryName(localPath);
        //localPath = Path.GetDirectoryName(localPath);

        localPath += @"\xml";
        string extProductPath = localPath + @"\ProductXml.xml";
        if (!File.Exists(extProductPath))
        {
            HelpXml.CreateFiles(extProductPath);
        }
        else
        {
            HelpXml.LoadData(extProductPath);
        }
        ProductPath = extProductPath;
    }


    public void Add(DO.Products t)
    {
        XElement id = new XElement("Id", t.Id);
        XElement name = new XElement("Name", t.Name);
        XElement price = new XElement("Price", t.Price);
        XElement category = new XElement("Category", t.Category);
        XElement instok = new XElement("InStock", t.InStock);

        ProductRoot.Add(new XElement("Products",id,name,price,category,instok));
        ProductRoot.Save(ProductPath);
    }

    public void Delete(int Id1, int Id2)
    {
        ProductRoot = XElement.Load(ProductPath);
        try
        {
            XElement product = (from item in ProductRoot.Elements()
                                where int.Parse(item.Element("Id")!.Value) == Id1
                                select item).FirstOrDefault()!;
            product.Remove();
            ProductRoot.Save(ProductPath);
        }
        catch
        {
            throw new Exception("Impossible to delete the product");
        }
    }

    public DO.Products Get(int Id1, int Id2)
    {
        ProductRoot = XElement.Load(ProductPath);
        DO.Products product=new Products();
        try
        {
            ProductRoot = XElement.Load(ProductPath);

            // LINQ to XML to get the list of products. The type of products is IEnumerable<XElement>?
            var products = from item in ProductRoot.Elements()
                           select item;


            List<DO.Products?> p = new List<DO.Products?>();

            // Allow us to cast from IEnumerable<XElement>? to List<Products> that is also an IEnumerable<Products?> 
            foreach (var item1 in products)
            {
                product.Id = int.Parse(item1.Element("Id")!.Value);
                product.Name = item1.Element("Name")!.Value;
                product.Price = double.Parse(item1.Element("Price").Value);
                product.Category = (Category)Enum.Parse(typeof(Category), item1.Element("Category").Value);
                product.InStock = int.Parse(item1.Element("InStock").Value);
                p.Add(product);
                // Reinitialize the product for next iteration 
                product = new Products();
            }
            var prod = from item1 in p
                       where item1?.Id == Id1
                       select item1;
            DO.Products products1=new Products();
            products1 = (DO.Products)prod.First()!;
            return products1;
        }
        catch
        {
            throw new Exception("Product doen't exists!");
        }
    }

    // A finir
    public DO.Products? GetItem(Func<DO.Products?, bool>? predicate)
    {
        //HelpXml.LoadData(ProductPath);
        //DO.Products? product;
        //Predicate<Products?> predicate1 = prod => predicate!(prod);
        //try
        //{
        //    product = (from item in ProductRoot.Elements() // Voir comment utiliser le predicate
        //               select new DO.Products()
        //               {
        //                   Id = int.Parse(item.Element("id")!.Value),
        //                   Name = item.Element("name")!.Value,
        //                   Price = double.Parse(item.Element("price")!.Value),
        //                   Category = (Category)int.Parse(item.Element("category")!.Value),
        //                   InStock = int.Parse(item.Element("inStock")!.Value)
        //               }).FirstOrDefault();
        //    return (DO.Products)product!;
        //}
        //catch
        //{
        //    throw new Exception("Product doen't exists!");
        //}
        throw new Exception();
    }

    // Voir comment faire avec predicate
    public IEnumerable<DO.Products?> GetList(Func<DO.Products?, bool>? predicate = null)
    {
        // Load the file in the root
        ProductRoot = XElement.Load(ProductPath);

        // LINQ to XML to get the list of products. The type of products is IEnumerable<XElement>?
        var products = from item in ProductRoot.Elements()
                       select item;


        List<DO.Products?> p = new List<DO.Products?>();
        DO.Products product=new Products();

        // Allow us to cast from IEnumerable<XElement>? to List<Products> that is also an IEnumerable<Products?> 
        foreach (var item1 in products)
        {
            product.Id = int.Parse(item1.Element("Id")!.Value);
            product.Name = item1.Element("Name")!.Value;
            product.Price = double.Parse(item1.Element("Price").Value);
            product.Category = (Category)Enum.Parse(typeof(Category), item1.Element("Category").Value);
            product.InStock = int.Parse(item1.Element("InStock").Value);
            p.Add(product);
            // Reinitialize the product for next iteration 
            product = new Products();
        }
        if (predicate != null)
        {
            p = (List<Products?>)p.Where(predicate);
        }
        return p;
    }


    public void Update(int Id1, int Id2)
    {
        // Load the file in the root
        ProductRoot = XElement.Load(ProductPath);
        //We get the target product
        XElement product = (from item in ProductRoot.Elements()
                            where int.Parse(item.Element("Id")!.Value) == Id1
                            select item).FirstOrDefault()!;
        // We get the new product
        XElement product1 = (from item in ProductRoot.Elements()
                             where int.Parse(item.Element("Id")!.Value) == Id1
                             select item).Last();
        // We update the target product with the values that the admin entered in the PL
        foreach (var item in ProductRoot.Elements())
        {
            if (item == product)
            {
              product.Element("Price").SetValue(double.Parse(product1.Element("Price").Value));
              product.Element("InStock").SetValue(int.Parse(product1.Element("InStock").Value));
              product.Element("Category").SetValue((Category)Enum.Parse(typeof(Category),product1.Element("Category").Value));
              product.Element("Name").SetValue(product1.Element("Name").Value);
            }
        }
        product1.Remove();
        ProductRoot.Save(ProductPath);
    }

    /// <summary>
    /// Saving when used with LINQ
    /// </summary>
    /// <param name="products"></param>
    public void SaveProductList(IEnumerable<DO.Products?> products)
    {
        ProductRoot = new XElement("products",
            from item in products
            select new XElement("product",
            new XElement("id", item?.Id),
            new XElement("name", item?.Name),
            new XElement("price", item?.Price),
            new XElement("category", item?.Category),
            new XElement("inStock", item?.InStock)
            )
            
        );
        ProductRoot.Save(ProductPath);
    }
}