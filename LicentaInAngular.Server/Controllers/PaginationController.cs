using LicentaInAngular.Server.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaginationController : ControllerBase
    {
        private readonly ProductService _productService;

        public PaginationController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProductsPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await GetProductsPaginatedResponse(page, pageSize);
        }

        [HttpGet("products/search")]
        public async Task<IActionResult> SearchProductsPaginated([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await SearchProductsPaginatedResponse(searchTerm, page, pageSize);
        }

        [HttpGet("products/category")]
        public async Task<IActionResult> GetProductsByCategoryPaginated([FromQuery] string category, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await GetProductsByCategoryPaginatedResponse(category, page, pageSize);
        }

        [HttpGet("products/user")]
        public async Task<IActionResult> GetProductsByUserPaginated([FromQuery] int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await GetProductsByUserPaginatedResponse(userId, page, pageSize);
        }

        [HttpGet("products/with-discount")]
        public async Task<IActionResult> GetProductsWithDiscountPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await GetProductsWithDiscountPaginatedResponse(page, pageSize);
        }

        [HttpGet("products/offered")]
        public async Task<IActionResult> GetOfferedProductsPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await GetOfferedProductsPaginatedResponse(page, pageSize);
        }

        private async Task<IActionResult> GetProductsPaginatedResponse(int page, int pageSize)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new
                {
                    message = "Page and pageSize must be greater than 0."
                });
            }

            if (pageSize > 100)
            {
                return BadRequest(new
                {
                    message = "Maximum page size is 100."
                });
            }

            var allProducts = await _productService.GetAll();
            var totalCount = allProducts.Count();

            var products = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PaginationResponse<object>
            {
                Data = products,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize),
                HasNextPage = page < (int)System.Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = page > 1
            };

            return Ok(response);
        }

        private async Task<IActionResult> SearchProductsPaginatedResponse(string searchTerm, int page, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new
                {
                    message = "Search term cannot be empty."
                });
            }

            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new
                {
                    message = "Page and pageSize must be greater than 0."
                });
            }

            if (pageSize > 100)
            {
                return BadRequest(new
                {
                    message = "Maximum page size is 100."
                });
            }

            var allProducts = await _productService.SearchByName(searchTerm);
            var totalCount = allProducts.Count();

            var products = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PaginationResponse<object>
            {
                Data = products,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize),
                HasNextPage = page < (int)System.Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = page > 1
            };

            return Ok(response);
        }

        private async Task<IActionResult> GetProductsByCategoryPaginatedResponse(string category, int page, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return BadRequest(new
                {
                    message = "Category cannot be empty."
                });
            }

            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new
                {
                    message = "Page and pageSize must be greater than 0."
                });
            }

            if (pageSize > 100)
            {
                return BadRequest(new
                {
                    message = "Maximum page size is 100."
                });
            }

            var allProducts = await _productService.GetByCategory(category);
            var totalCount = allProducts.Count();

            if (totalCount == 0)
            {
                return NotFound(new
                {
                    message = "No products found in this category."
                });
            }

            var products = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PaginationResponse<object>
            {
                Data = products,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize),
                HasNextPage = page < (int)System.Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = page > 1
            };

            return Ok(response);
        }

        private async Task<IActionResult> GetProductsByUserPaginatedResponse(int userId, int page, int pageSize)
        {
            if (userId < 1)
            {
                return BadRequest(new
                {
                    message = "User ID must be greater than 0."
                });
            }

            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new
                {
                    message = "Page and pageSize must be greater than 0."
                });
            }

            if (pageSize > 100)
            {
                return BadRequest(new
                {
                    message = "Maximum page size is 100."
                });
            }

            var allProducts = await _productService.GetByUserId(userId);
            var totalCount = allProducts.Count();

            if (totalCount == 0)
            {
                return NotFound(new
                {
                    message = "No products found for this user."
                });
            }

            var products = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PaginationResponse<object>
            {
                Data = products,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize),
                HasNextPage = page < (int)System.Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = page > 1
            };

            return Ok(response);
        }

        private async Task<IActionResult> GetProductsWithDiscountPaginatedResponse(int page, int pageSize)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new
                {
                    message = "Page and pageSize must be greater than 0."
                });
            }

            if (pageSize > 100)
            {
                return BadRequest(new
                {
                    message = "Maximum page size is 100."
                });
            }

            var allProducts = await _productService.GetProduseCuReducereResponse();
            var okResult = allProducts as OkObjectResult;

            if (okResult == null)
            {
                return allProducts;
            }

            var productsList = (okResult.Value as IEnumerable<dynamic>)?.ToList() ?? new List<dynamic>();
            var totalCount = productsList.Count;

            if (totalCount == 0)
            {
                return NotFound(new
                {
                    message = "No products with discount found."
                });
            }

            var products = productsList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PaginationResponse<dynamic>
            {
                Data = products,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize),
                HasNextPage = page < (int)System.Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = page > 1
            };

            return Ok(response);
        }

        private async Task<IActionResult> GetOfferedProductsPaginatedResponse(int page, int pageSize)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new
                {
                    message = "Page and pageSize must be greater than 0."
                });
            }

            if (pageSize > 100)
            {
                return BadRequest(new
                {
                    message = "Maximum page size is 100."
                });
            }

            var offeredProducts = await _productService.GetOfferedProductsAsync();
            var totalCount = offeredProducts.Count();

            if (totalCount == 0)
            {
                return NotFound(new
                {
                    message = "No offered products found."
                });
            }

            var products = offeredProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PaginationResponse<object>
            {
                Data = products,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize),
                HasNextPage = page < (int)System.Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = page > 1
            };

            return Ok(response);
        }
    }

    // DTOs
    public class PaginationResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public class PaginationRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public string Category { get; set; }
        public int? UserId { get; set; }
    }
}