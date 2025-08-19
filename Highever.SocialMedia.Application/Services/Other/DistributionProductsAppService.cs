using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain;
using Highever.SocialMedia.Domain.Repository;
using Highever.SocialMedia.MongoDB;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application
{
    public class DistributionProductsAppService : IDistributionProductsAppService
    {
        private readonly IMongoRepository<DistributionProducts> _repository;

        public DistributionProductsAppService(IMongoRepository<DistributionProducts> repository)
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
            await _repository.InsertAsync(product);
            return product.Id;
        }
        public async Task<int> CreateAsync(DistributionProducts input)
        {
            await _repository.InsertAsync(input);
            return 1;
        }
        /// <summary>
        /// 获取产品列表
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<List<DistributionProducts>> GetQueryListAsync(Expression<Func<DistributionProducts, bool>> predicate)
        {
            var data = await _repository.FindAsync(predicate);
            return data.ToList();
        }

        /// <summary>
        /// 根据ID获取产品
        /// </summary>
        /// <param name="id">产品ID</param>
        /// <returns>产品DTO</returns>
        public async Task<DistributionProductDto> GetByIdAsync(int id)
        {
            var product = await _repository.FindOneAsync(t => t.Id == id);

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
