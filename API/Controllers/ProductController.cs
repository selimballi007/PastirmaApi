using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.ProductDTOs;
using PastirmaApi.Application.Interfaces.Services;

namespace PastirmaApi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Tüm ürünleri getirir (filtrelerle)
        /// GET /api/products?category=Electronics&isActive=true&minPrice=100&maxPrice=1000&search=laptop
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ProductDTO>>> GetProducts(
            [FromQuery] int? categoryId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? search = null,
            [FromQuery] bool isBestSeller = false,
            [FromQuery] bool isCampaign = false,
            [FromQuery] int? limit = null)
        {
            try
            {
                var filters = new ProductFiltersDTO
                {
                    CategoryId = categoryId ?? 0,
                    IsActive = isActive,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Search = search,
                    IsBestSeller = isBestSeller,
                    IsCampaign = isCampaign,
                    Limit = limit ?? 0
                };

                var products = await _productService.GetProductsAsync(filters);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ürünler alınırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir ürünü getirir
        /// GET /api/products/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id, [FromQuery] bool includeImages = true )
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id, includeImages);

                if (product == null)
                {
                    return NotFound(new { message = "Ürün bulunamadı." });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ürün alınırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Yeni ürün oluşturur
        /// POST /api/products
        /// </summary>
        ///         
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProductDTO>> CreateProduct([FromBody] CreateProductRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var product = await _productService.CreateProductAsync(request);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ürün oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Ürünü günceller
        /// PUT /api/products/{id}
        /// </summary>
        /// 
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDTO>> UpdateProduct(int id, [FromBody] UpdateProductRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var product = await _productService.UpdateProductAsync(id, request);

                if (product == null)
                {
                    return NotFound(new { message = "Ürün bulunamadı." });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ürün güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Ürünü siler
        /// DELETE /api/products/{id}
        /// </summary>
        /// 
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                var success = await _productService.DeleteProductAsync(id);

                if (!success)
                {
                    return NotFound(new { message = "Ürün bulunamadı." });
                }

                return Ok(new { message = "Ürün başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ürün silinirken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Ürün durumunu günceller (aktif/pasif)
        /// PATCH /api/products/{id}/status
        /// </summary>
        /// 
        [Authorize]
        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateProductStatus(int id, [FromBody] UpdateProductStatusRequestDTO request)
        {
            try
            {
                var success = await _productService.UpdateProductStatusAsync(id, request.IsActive);

                if (!success)
                {
                    return NotFound(new { message = "Ürün bulunamadı." });
                }

                return Ok(new { message = "Ürün durumu güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ürün durumu güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Ürün stoğunu günceller
        /// PATCH /api/products/{id}/stock
        /// </summary>
        /// 
        [Authorize]
        [HttpPatch("{id}/stock")]
        public async Task<ActionResult> UpdateProductStock(int id, [FromBody] UpdateProductsStockRequestDTO request)
        {
            try
            {
                if (request.Stock < 0)
                {
                    return BadRequest(new { message = "Stok miktarı negatif olamaz." });
                }

                var success = await _productService.UpdateProductStockAsync(id, request.Stock);

                if (!success)
                {
                    return NotFound(new { message = "Ürün bulunamadı." });
                }

                return Ok(new { message = "Stok miktarı güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Stok güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}
