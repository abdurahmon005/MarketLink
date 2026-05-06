using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Product;
using MarketLink.Application.Models.Rating;
using Microsoft.AspNetCore.Http;

namespace MarketLink.Application.Service
{
    public interface ICompanyProductService
    {
        /// <summary>Yangi mahsulot qo'shish</summary>
        Task<(bool Success, string Message, ProductResponse? Data)> CreateAsync(
            int companyId, CreateProductRequest request, CancellationToken ct = default);

        /// <summary>O'z mahsulotlari ro'yxati (sahifalash bilan)</summary>
        Task<PagedResult<ProductResponse>> GetMyProductsAsync(
            int companyId, int page, int pageSize, CancellationToken ct = default);

        /// <summary>Bitta mahsulotni olish (ownership tekshiruvi bilan)</summary>
        Task<ProductResponse?> GetByIdAsync(int productId, int companyId, CancellationToken ct = default);

        /// <summary>Mahsulotni tahrirlash</summary>
        Task<(bool Success, string Message)> UpdateAsync(
            int productId, int companyId, UpdateProductRequest request, CancellationToken ct = default);

        /// <summary>Mahsulotni o'chirish (faol buyurtmalarda bo'lmasa)</summary>
        Task<(bool Success, string Message)> DeleteAsync(
            int productId, int companyId, CancellationToken ct = default);

        /// <summary>Mahsulot rasmini yangilash</summary>
        Task<(bool Success, string Message, string? ImageUrl)> UpdateImageAsync(
            int productId, int companyId, IFormFile image, CancellationToken ct = default);

        /// <summary>Qoldiq monitoringi</summary>
        Task<List<ProductStockResponse>> GetStockAsync(int companyId, CancellationToken ct = default);

        /// <summary>Mahsulot reytinglari</summary>
        Task<PagedResult<ProductRatingResponse>> GetRatingsAsync(
            int companyId, int? productId, int page, int pageSize, CancellationToken ct = default);

        /// <summary>Bitta mahsulotning o'rtacha reytingi</summary>
        Task<ProductAverageRatingResponse?> GetAverageRatingAsync(
            int productId, int companyId, CancellationToken ct = default);
    }
}
