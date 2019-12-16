using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using System.Linq;
using NLog;
using NorthwindConsole.Models;

namespace NorthwindConsole
{
    class MainClass
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static void Main(string[] args)
        {
            logger.Info("Program started");
            try
            {
                string choice;
                do
                {
                    //Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine("Welcome to the Nowrthwind Traders Application \n");
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Add Category");
                    Console.WriteLine("3) Display Active Products by Specified Category");
                    Console.WriteLine("4) Display All Categories and Their Relative Active Products");
                    Console.WriteLine("5) Edit Category Name");
                    Console.WriteLine("6) Delete Category");


                    // C Selction 
                    Console.WriteLine("7) Display All Products"); // Name only: all, active only or discontinued only
                    Console.WriteLine("8) Add New Product");
                    Console.WriteLine("9) Edit Product Information");
                    Console.WriteLine("10) Display a Specific Product"); // All fields for selected product

                    // A Selction
                    Console.WriteLine("11) Delete Product");

                    // Extra 
                    Console.WriteLine("12) Inventory Managment");

                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");

                    if (choice == "1")
                    {
                        var db = new NorthwindContext();
                        var query = db.Categories.OrderBy(p => p.CategoryName);
                        const int FieldWidthRightAligned = 20;

                        Console.WriteLine($"{query.Count()} records returned\n");
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName,FieldWidthRightAligned}: \t {item.Description}");
                        }

                        Console.ReadLine();

                    }
                    else if (choice == "2")
                    {
                        Category category = new Category();
                        Console.WriteLine("Enter Category Name:");
                        category.CategoryName = Console.ReadLine();
                        Console.WriteLine("Enter the Category Description:");
                        category.Description = Console.ReadLine();

                        // save category to db
                        var db = new NorthwindContext();
                        //db.AddCategory(category);
                        db.Categories.Add(category);
                        db.SaveChanges();

                        foreach (var validationResult in db.GetValidationErrors())
                        {
                            foreach (var error in validationResult.ValidationErrors)
                            {
                                logger.Error(
                                    "Entity Property: {0}, Error {1}",
                                    error.PropertyName,
                                    error.ErrorMessage);
                            }
                        }


                        ValidationContext context = new ValidationContext(category, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(category, context, results, true);
                        if (isValid)
                        {
                            var dbb = new NorthwindContext();
                            // check for unique name
                            if (dbb.Categories.Any(c => c.CategoryName == category.CategoryName))
                            {

                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                // TODO: save category to db
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "3") // All active products by category 
                    {
                        var db = new NorthwindContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.ForegroundColor = ConsoleColor.DarkCyan;

                        Console.WriteLine("Select the category whose products you want to display:");
                        Console.WriteLine("NOTE: Active Products Only\n");
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}: {item.Description}");
                        }

                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Category category = db.Categories.FirstOrDefault(c => c.CategoryId == id);
                        //Product product = db.Products.FirstOrDefault(p => p.CategoryId == id && p.Discontinued == false);

                        //Console.WriteLine($"{category.CategoryName} - {category.Description}");


                        var catProdQuery = from p in db.Products.AsEnumerable()
                                           join c in db.Categories.AsEnumerable() on p.CategoryId equals c.CategoryId
                                           where (p.Discontinued == false && p.CategoryId == id)
                                           select p;

                        Console.WriteLine("Selected Category: " + category.CategoryName + "\n");
                        foreach (Product p in catProdQuery)

                        {
                            Console.WriteLine(p.ProductName);
                        }

                        Console.ReadLine();

                        Console.ResetColor();

                    }
                    else if (choice == "4")
                    {
                        var db = new NorthwindContext();
                        //var query = db.Categories.Include("Products").OrderBy(c => c.CategoryId);
                        //var query = from c in db.Categories.AsEnumerable()
                        //            join p in db.Products.AsEnumerable() on c.CategoryId equals p.CategoryId
                        //            where (p.Discontinued == false)
                        //            select c;

                        var catProdQuery = from p in db.Products.AsEnumerable()
                                           join c in db.Categories.AsEnumerable() on p.CategoryId equals c.CategoryId
                                           where (p.Discontinued == false)
                                           orderby c.CategoryName
                                           select new { c.CategoryName, p.ProductName };


                        const int FieldWidthRightAligned = 15;
                        //const int FieldWidthLeftAligned = 10;

                        Console.WriteLine("|    Category      |   |   Product     |");
                        Console.WriteLine();
                        foreach (var item in catProdQuery)
                        {

                            Console.WriteLine($"{item.CategoryName,FieldWidthRightAligned}: \t{item.ProductName}");
                            //foreach(var ProductName in catProdQuery)
                            //    Console.WriteLine($" \t \t{ProductName}");


                        }
                        Console.ReadLine();


                        //    foreach (var item in query)
                        //    {
                        //        Console.WriteLine($"{item.CategoryName}");
                        //        foreach (Product p in item.Products)
                        //        {
                        //            Console.WriteLine($"\t \t{p.ProductName}");
                        //        }

                        //    }
                    }


                    else if (choice == "5")
                    {
                        var db = new NorthwindContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category ID you want to edit:");
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Category category = db.Categories.FirstOrDefault(c => c.CategoryId == id);

                        Console.WriteLine("Enter the new category name:");
                        var name = Console.ReadLine();
                        logger.Info($"Category Name {name} entered");

                        category.CategoryName = name;
                        db.SaveChanges();
                        logger.Info($"Category Name {name} updated");

                    }


                    else if (choice == "6")
                    {
                        var db = new NorthwindContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category ID you want to delete:\n");

                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");


                        try
                        {
                            Product product = db.Products.FirstOrDefault(p => p.CategoryId == id);
                            db.Products.Remove(product);
                            db.SaveChanges();
                        }
                        catch

                        {
                            Console.WriteLine("No products associated with this category.");
                        }


                        Category category = db.Categories.FirstOrDefault(c => c.CategoryId == id);

                        db.Categories.Remove(category);
                        db.SaveChanges();
                        logger.Info($"Category Id {id} deleted");
                    }


                    else if (choice == "7") // Display all products (specify active or discontinued)
                    {
                        string subMenu;
                        do
                        {
                            Console.WriteLine("Which products did you want to view?\n");
                            Console.WriteLine();
                            Console.WriteLine("1) Active");
                            Console.WriteLine("2) Discontinued");
                            Console.WriteLine("\"q\" to quit");

                            var db = new NorthwindContext();
                            subMenu = Console.ReadLine();



                            if (subMenu == "1")
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;

                                var activeProd = from p in db.Products.AsEnumerable()
                                                     //join c in db.Categories.AsEnumerable() on p.CategoryId equals c.CategoryId
                                                     //join s in db.Suppliers.AsEnumerable() on p.SupplierId equals s.SupplierId
                                                 where (p.Discontinued == false)
                                                 select p;

                                Console.WriteLine("--------------------");
                                Console.WriteLine("Active Products: ");
                                Console.WriteLine("--------------------\n");
                                foreach (var item in activeProd)
                                {
                                    Console.WriteLine($"{item.ProductName} ");
                                }

                                Console.ResetColor();
                            }

                            else if (subMenu == "2")
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;

                                var disProd = from p in db.Products.AsEnumerable()
                                                  //join c in db.Categories.AsEnumerable() on p.CategoryId equals c.CategoryId
                                                  //join s in db.Suppliers.AsEnumerable() on p.SupplierId equals s.SupplierId
                                              where (p.Discontinued == true)
                                              select p;

                                Console.WriteLine("--------------------------");
                                Console.WriteLine("Discontinued Products: ");
                                Console.WriteLine("--------------------------\n");
                                foreach (var item in disProd)
                                {
                                    Console.WriteLine($"{item.ProductName} ");
                                }

                                Console.ResetColor();
                            }



                            Console.ReadLine();

                        } while (subMenu.ToLower() != "q");

                    }

                    // Add new product
                    else if (choice == "8")
                    {
                        var db = new NorthwindContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Which category does the new product belong to?:");
                        Console.WriteLine("Enter the ID of the category");
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }

                        int categoryid = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {categoryid} selected");



                        Console.Write("Enter name of product: ");
                        var productName = Console.ReadLine();

                        Console.WriteLine("Enter quantity per unit: ");
                        var quantityPerUnit = Console.ReadLine();

                        Console.WriteLine("Enter unit price: ");
                        double unitPrice = Convert.ToDouble(Console.ReadLine());

                        Console.WriteLine("Enter units in stock: ");
                        int unitsInStock = int.Parse(Console.ReadLine());


                        Console.WriteLine("Enter units on order: ");
                        int unitsOnOrder = int.Parse(Console.ReadLine());

                        Console.WriteLine("Enter reorder level: ");
                        int reorderLevel = int.Parse(Console.ReadLine());

                        //Console.WriteLine("Is this an Active Product?");
                        bool discontinued = false;

                        Console.WriteLine("Here is a list of suppliers");




                        var spdb = new NorthwindContext();
                        var suplierquery = db.Suppliers.OrderBy(s => s.SupplierId);


                        foreach (var suplieritem in suplierquery)
                        {
                            Console.WriteLine($"{suplieritem.SupplierId}) {suplieritem.CompanyName}");
                        }



                        Console.WriteLine("Enter the Supplier ID of the Product: ");
                        int supplierID = int.Parse(Console.ReadLine());


                        var product = new Product
                        {
                            ProductName = productName,
                            QuantityPerUnit = quantityPerUnit,
                            UnitPrice = Convert.ToDecimal(unitPrice),
                            UnitsInStock = Convert.ToInt16(unitsInStock),
                            UnitsOnOrder = Convert.ToInt16(unitsOnOrder),
                            ReorderLevel = Convert.ToInt16(reorderLevel),
                            Discontinued = discontinued,
                            CategoryId = categoryid,
                            SupplierId = supplierID


                        };

                        db.AddProduct(product);
                        logger.Info("Product Added - {productName}", productName);
                        db.SaveChanges();

                    }


                    else if (choice == "9")
                    {
                        var db = new NorthwindContext();
                        var query = db.Products.OrderBy(p => p.ProductID);


                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.ProductID}) {item.ProductName}");
                        }
                        Console.WriteLine();
                        Console.WriteLine("Select the ID of the product you'd like to edit: ");

                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"ProductId {id} selected");

                        //Console.WriteLine("What field would you like to edit?");

                        string subMenu;
                        do
                        {
                            var selectProd = from p in db.Products.AsEnumerable()
                                             join c in db.Categories.AsEnumerable() on p.CategoryId equals c.CategoryId
                                             join s in db.Suppliers.AsEnumerable() on p.SupplierId equals s.SupplierId
                                             where (p.ProductID == Convert.ToInt32(id))
                                             select p;


                            foreach (var item in selectProd)
                            {
                                Console.WriteLine($"ID: {item.ProductID} \nName: {item.ProductName} \nQuantity per unit: {item.QuantityPerUnit} \nUnit Price: {item.UnitPrice} \n" +
                                $"Units in stock: {item.UnitsInStock} \nUnits on order: {item.UnitsOnOrder} \nReorder level: {item.ReorderLevel} \n" +
                                $"Discontinued: {item.Discontinued} \nCategory: {item.Category.CategoryName} \nSupplier: {item.Supplier.CompanyName}");
                            }

                            //var Pdb = new BlogContext();
                            //var Pquery = db.Posts.OrderBy(p => p.PostId, blogID = Convert.ToInt32(BlogID));

                            Console.WriteLine();

                            Console.WriteLine("What field would you like to edit?\n");
                            Console.WriteLine();
                            Console.WriteLine("1) Product Name");
                            Console.WriteLine("2) Quantity Per Unit");
                            Console.WriteLine("3) Unit Price");
                            Console.WriteLine("4) Units In Stock");
                            Console.WriteLine("5) Units On Order");
                            Console.WriteLine("6) Reorder Level");
                            Console.WriteLine("7) Change Status (Active/ Discontinued)");
                            Console.WriteLine("8) Category");
                            Console.WriteLine("9) Supplier");
                            Console.WriteLine("\"q\" to quit");
                            subMenu = Console.ReadLine();
                            Console.Clear();
                            logger.Info($"Option {subMenu} selected");



                            // Edit Product Name
                            if (subMenu == "1")
                            {


                                Product product = db.Products.FirstOrDefault(c => c.ProductID == id);

                                Console.WriteLine("Enter the new Product name:");
                                var name = Console.ReadLine();
                                logger.Info($"Product Name {name} entered");

                                product.ProductName = name;
                                db.SaveChanges();
                                logger.Info($"Category Name {name} updated");



                            }

                            // Edit QPU
                            else if (subMenu == "2")
                            {

                                Product product = db.Products.FirstOrDefault(c => c.ProductID == id);

                                Console.WriteLine("Enter the new Quantity per unit value: ");
                                var qpu = Console.ReadLine();
                                logger.Info($"Product Quantity per unit {qpu} entered");

                                product.QuantityPerUnit = qpu;
                                db.SaveChanges();
                                logger.Info($"Quantity per unit {qpu} updated");

                            }

                            // Edit Unit Price
                            else if (subMenu == "3")
                            {
                                Product product = db.Products.FirstOrDefault(c => c.ProductID == id);

                                Console.WriteLine("Enter the new Unit Price: ");
                                decimal unitPrice = Convert.ToDecimal(Console.ReadLine());
                                logger.Info($"Unit Price {unitPrice} entered");

                                product.UnitPrice = Convert.ToDecimal(unitPrice);
                                db.SaveChanges();
                                logger.Info($"Unit Price {unitPrice} updated");


                            }


                            // Edit UIS
                            else if (subMenu == "4")
                            {
                                Product product = db.Products.FirstOrDefault(c => c.ProductID == id);

                                Console.WriteLine("Enter the new inventory value: ");
                                int unitsInStock = Convert.ToInt16(Console.ReadLine());
                                logger.Info($"Units in stock {unitsInStock} entered");

                                product.UnitsInStock = Convert.ToInt16(unitsInStock);
                                db.SaveChanges();
                                logger.Info($"Units in stock {unitsInStock} updated");


                            }


                            // Edit UOO
                            else if (subMenu == "5")
                            {

                                Product product = db.Products.FirstOrDefault(c => c.ProductID == id);

                                Console.WriteLine("Enter the change to Units on Order: ");
                                int unitsOnOrder = Convert.ToInt16(Console.ReadLine());
                                logger.Info($"Units on order {unitsOnOrder} entered");

                                product.UnitsOnOrder = Convert.ToInt16(unitsOnOrder);
                                db.SaveChanges();
                                logger.Info($"Units on order {unitsOnOrder} updated");

                            }

                            // Edit ROL

                            else if (subMenu == "6")
                            {

                                Product product = db.Products.FirstOrDefault(c => c.ProductID == id);

                                Console.WriteLine("Enter the new Reorder Level: ");
                                int reorderLevel = Convert.ToInt16(Console.ReadLine());
                                logger.Info($"Reorder Level {reorderLevel} entered");

                                product.ReorderLevel = Convert.ToInt16(reorderLevel);
                                db.SaveChanges();
                                logger.Info($"Reorder Level {reorderLevel} updated");

                            }


                            //Edit DCNT
                            else if (subMenu == "7")
                            {

                                Product product = db.Products.FirstOrDefault(c => c.ProductID == id);

                                Console.WriteLine("Change Discontinued Status: ");
                                bool discontinued = Convert.ToBoolean(Console.ReadLine());
                                logger.Info($"Discontinued {discontinued} entered");

                                product.Discontinued = discontinued;
                                db.SaveChanges();
                                logger.Info($"Discontinued {discontinued} updated");

                            }


                            // Edit Category ID
                            else if (subMenu == "8")
                            {

                                Product product = db.Products.FirstOrDefault(c => c.ProductID == id);

                                Console.WriteLine("Change Category ID: ");
                                int categoryID = Convert.ToInt32(Console.ReadLine());
                                logger.Info($"Category ID {categoryID} entered");

                                product.CategoryId = categoryID;
                                db.SaveChanges();
                                logger.Info($"Category ID {categoryID} updated");



                            }


                            //Edit Supplier ID

                            else if (subMenu == "9")
                            {

                                Product product = db.Products.FirstOrDefault(c => c.ProductID == id);

                                Console.WriteLine("Change Supplier ID: ");
                                int supplierID = Convert.ToInt32(Console.ReadLine());
                                logger.Info($"Supplier ID {supplierID} entered");

                                product.SupplierId = supplierID;
                                db.SaveChanges();
                                logger.Info($"Suppler ID {supplierID} updated");


                            }





                        } while (subMenu.ToLower() != "q");



                    }

                    else if (choice == "10")
                    {


                        var db = new NorthwindContext();
                        var query = db.Products.OrderBy(p => p.ProductID);


                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.ProductID}) {item.ProductName}");
                        }
                        Console.WriteLine();
                        Console.WriteLine("Select the ID of the product you'd like to expand: ");

                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"ProductId {id} selected");

                        //Console.WriteLine("What field would you like to edit?");



                        var selectProd = from p in db.Products.AsEnumerable()
                                         join c in db.Categories.AsEnumerable() on p.CategoryId equals c.CategoryId
                                         join s in db.Suppliers.AsEnumerable() on p.SupplierId equals s.SupplierId
                                         where (p.ProductID == Convert.ToInt32(id))
                                         select p;


                        foreach (var item in selectProd)
                        {
                            Console.WriteLine($"You've Selected: {item.ProductName}\n");
                            Console.WriteLine($"ID: {item.ProductID} \nName: {item.ProductName} \nQuantity per unit: {item.QuantityPerUnit} \nUnit Price: {item.UnitPrice} \n" +
                            $"Units in stock: {item.UnitsInStock} \nUnits on order: {item.UnitsOnOrder} \nReorder level: {item.ReorderLevel} \n" +
                            $"Discontinued: {item.Discontinued} \nCategory: {item.Category.CategoryName} \nSupplier: {item.Supplier.CompanyName}");
                        }



                        Console.ReadLine();





                    }



                    else if (choice == "11")
                    {

                        var db = new NorthwindContext();
                        var query = db.Products.OrderBy(p => p.ProductID);

                        Console.WriteLine("Select the ID of the product you want to delete:");
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.ProductID}) {item.ProductName}");
                        }
                        Console.WriteLine();
                        Console.WriteLine("Select the ID of the product you want to delete:");

                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"Product {id} selected");
                        Product product = db.Products.FirstOrDefault(p => p.ProductID == id);

                        db.Products.Remove(product);
                        db.SaveChanges();
                        logger.Info($" The Product: {product.ProductName} deleted");


                    }



                    else if (choice == "12")
                    {
                        Console.WriteLine("Inventory Managment Reports\n");
                        Console.WriteLine("Select the report you'd like to view: \n");

                        Console.WriteLine("1) Priority Reorder");
                        Console.WriteLine("2) Category PI");
                        Console.WriteLine("\"q\" Return to Main Menu");
                        Console.WriteLine();



                        string invMenu;
                        do
                        {


                            invMenu = Console.ReadLine();
                            Console.Clear();
                            logger.Info($"Option {invMenu} selected");



                            if (invMenu == "1")
                            {

                                var db = new NorthwindContext();

                                var reInvQuery = from p in db.Products.AsEnumerable()
                                                     //join c in db.Categories.AsEnumerable() on p.CategoryId equals c.CategoryId
                                                 where (p.Discontinued == false && p.UnitsInStock <= p.ReorderLevel && p.UnitsInStock + p.UnitsOnOrder <= p.ReorderLevel)
                                                 orderby p.ProductID
                                                 select p;

                                foreach (var item in reInvQuery)
                                {

                                    Console.WriteLine($"Product: {item.ProductID}, \"{item.ProductName}\"");
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Reorder Level: {item.ReorderLevel}");
                                    Console.WriteLine($"Units in Stock: {item.UnitsInStock}");
                                    Console.WriteLine($"Units on Order: {item.UnitsOnOrder}");
                                    Console.ResetColor();
                                    //Console.WriteLine();
                                    //foreach(var ProductName in catProdQuery)
                                    //    Console.WriteLine($" \t \t{ProductName}");


                                }



                            }

                            else if (invMenu == "2")
                            {
                                var db = new NorthwindContext();

                                var catGroupQry = from p in db.Products.AsEnumerable()
                                                  join c in db.Categories.AsEnumerable() on p.CategoryId equals c.CategoryId
                                                  group p by c.CategoryName into g
                                                  // where (p.Discontinued)

                                                  select new {g.Key, ProductCount = g.Count() };

                                //foreach (var line in db.Products.GroupBy(info => info.CategoryId)
                                //        .Select(group => new 
                                //        {
                                //            CategoryID = group.Key,
                                //            Count = group.Count()
                                //        })
                                //        .Join(db.Categories => db.Products.CategoryID  )
                                //        .OrderBy(x => x.CategoryID))
                                //            {
                                //                Console.WriteLine("{0} {1}", line.CategoryID, line.Count);
                                //            }

                                foreach (var catCount in catGroupQry)
                                {
                                    Console.WriteLine($"{catCount.Key}");
                                    Console.WriteLine($"{catCount.ProductCount}\n");

                                }
                            }

                        } while (invMenu.ToLower() != "q") ;

                    }



                    //Console.WriteLine();

                } while (choice.ToLower() != "q");
            }
            catch (DbEntityValidationException e)
            {
                logger.Error(e.Message);
                foreach (var eve in e.EntityValidationErrors)
                {
                    logger.Error("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        logger.Error("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }
    }
}