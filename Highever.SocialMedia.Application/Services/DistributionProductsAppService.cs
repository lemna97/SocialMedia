using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain;
using Highever.SocialMedia.SqlSugar;
using NPOI.SS.Formula.Functions;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application
{
    public class DistributionProductsAppService : IDistributionProductsAppService
    {
        public readonly ISqlSugarRepository<DistributionProducts> _repository;

        public DistributionProductsAppService(ISqlSugarRepository<DistributionProducts> repository)
        {
            _repository = repository;
        }
        public async Task<int> CreateAsync(DistributionProductDto input)
        {
            var product = new DistributionProducts(
                input.Sku,
                input.Platform,
                input.SelfPickupPrice,
                input.FreeShippingPrice,
                input.StockQuantity
            );
            await _repository.InsertEntityAsync(product);
            return product.Id;
        }
        public async Task<int> CreateAsync(DistributionProducts input)
        { 
          return  await _repository.InsertEntityAsync(input); 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<List<DistributionProducts>> GetQueryListAsync(Expression<Func<DistributionProducts, bool>>? predicate = null)
        {
            return await _repository.QueryListAsync(predicate: predicate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<DistributionProductDto> GetByIdAsync(int id)
        {
            var product = await _repository.QuerySingleAsync(t => t.Id == id);

            if (product == null)
            {
                return new DistributionProductDto();
            }
            return new DistributionProductDto
            {
                Id = product.Id,
                Sku = product.Sku,
                Platform = product.Platform,
                SelfPickupPrice = product.SelfPickupPrice,
                FreeShippingPrice = product.FreeShippingPrice,
                StockQuantity = product.StockQuantity,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
