
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NET105.Entities;
using NET105.Interfaces;
using Z.PagedList;

namespace NET105.Repository
{
    public class ProductRepository : IProduct
    {
        private readonly ShopContext context;
        private readonly IUploadHelper uploadHelper;

        public ProductRepository(ShopContext context, IUploadHelper uploadHelper)
        {
            this.context = context;
            this.uploadHelper = uploadHelper;
        }

        public async Task<bool?> CreateAsync(Product product)
        {
            product.ProductId = Guid.NewGuid();
            try
            {
                bool? upload = await uploadHelper.UploadFileAsync(product.Upload);
                if (upload is null) return null;
                else if (upload == true)
                {
                    product.ImageUrl = product.Upload.FileName;
                    await context.Products.AddAsync(product);
                    var result = await context.SaveChangesAsync();
                    return result > 0;

                }

                return false;

            }
            catch
            {

                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var product = await FindProductAsync(id);
                uploadHelper.DeleteFile(product.ImageUrl);
                context.Products.Remove(product);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool?> EditAsync(Guid id, Product product, bool update = true)
        {


            try
            {
                if (update)
                {
                    bool? upload = await uploadHelper.UploadFileAsync(product.Upload);
                    if (upload is null) return null;
                    else if (upload == true)
                    {
                        // xóa file cũ đi 
                        uploadHelper.DeleteFile(product.ImageUrl);
                        // cập nhật url file mới
                        product.ImageUrl = product.Upload.FileName;

                    }
                }

                context.Update(product);
                var result = await context.SaveChangesAsync();
                return true;

  
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.ProductId))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<Product> FindProductAsync(Guid? id)
        {
            if (id is null) return null;
            return await context.Products.FindAsync(id);
        }

        public async Task<Product> GetProductAsync(Guid? id)
        {
            if (id == null)
            {
                return null;
            }

            var product = await context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            return product;
        }

        public IQueryable<Product> GetProductsAsync()
        {
            IQueryable<Product> product = context.Products.Include(p => p.Category);

            return product;
        }

        public IQueryable<Product> GetProductsWithIdCgr(int idCategory)
        {
            var products = context.Products.Where(p => p.CategoryId == idCategory);

            return products;
        }

        public SelectList GetSelectListCategory(int CategoryId = 0)
        {
            if (CategoryId == 0)
                return new SelectList(context.Categories, "CategoryId", "Name");

            return new SelectList(context.Categories, "CategoryId", "Name", CategoryId);
        }

        public IQueryable<Product> ProductOder(IQueryable<Product> products, int? type)
        {

            switch(type)
            {

                case 1 : 
                products = products.OrderByDescending(x=> x.Price);    
                break;

                case 2 : 
                products = products.OrderBy(x=> x.Price);    
                break;

                
                case 3 : 
                products = products.OrderByDescending(x=> x.Name);    
                break;

                case 4 : 
                products = products.OrderBy(x=> x.Name);    
                break;

            }
            return products;

        }

        private bool ProductExists(Guid id)
        {
            return context.Products.Any(e => e.ProductId == id);
        }

    }
}