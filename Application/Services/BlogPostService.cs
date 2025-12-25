using Microsoft.EntityFrameworkCore;
using PastirmaApi.Application.DTOs.BlogPostDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Entities;
using PastirmaApi.Core.Exceptions;
using PastirmaApi.Infrastructure.Data;

namespace PastirmaApi.Application.Services
{
    public class BlogPostService : IBlogPostService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BlogPostService> _logger;

        public BlogPostService(ApplicationDbContext context, ILogger<BlogPostService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BlogPostDTO> CreatePostAsync(CreateBlogPostDTO dto, int userId)
        {
            try
            {
                var post = new BlogPost
                {
                    Title = dto.Title,
                    Content = dto.Content,
                    Excerpt = dto.Excerpt,
                    ImageUrl = dto.ImageUrl,
                    CategoryId = dto.CategoryId,
                    AuthorId = userId,
                    PublishedDate = dto.PublishedDate,
                    IsFeatured = dto.IsFeatured,
                    ReadTime = CalculateReadTime(dto.Content),
                    ViewCount = 0
                };

                _context.BlogPosts.Add(post);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Blog post created: {PostId}, Title: {Title}", post.Id, post.Title);

                return await GetPostByIdAsync(post.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog post: {Title}", dto.Title);
                throw;
            }
        }

        public async Task<BlogPostDTO> UpdatePostAsync(int id, UpdateBlogPostDTO dto)
        {
            try
            {
                var post = await _context.BlogPosts.FindAsync(id);
                if (post == null)
                {
                    throw new NotFoundException("Blog yazısı bulunamadı.");
                }

                post.Title = dto.Title;
                post.Content = dto.Content;
                post.Excerpt = dto.Excerpt;
                post.ImageUrl = dto.ImageUrl;
                post.CategoryId = dto.CategoryId;
                post.PublishedDate = dto.PublishedDate;
                post.IsFeatured = dto.IsFeatured;
                post.ReadTime = CalculateReadTime(dto.Content);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Blog post updated: {PostId}, Title: {Title}", id, dto.Title);

                return await GetPostByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog post: {PostId}", id);
                throw;
            }
        }

        public async Task<bool> DeletePostAsync(int id)
        {
            try
            {
                var post = await _context.BlogPosts.FindAsync(id);
                if (post == null)
                {
                    throw new NotFoundException("Blog yazısı bulunamadı.");
                }

                _context.BlogPosts.Remove(post);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Blog post deleted: {PostId}, Title: {Title}", id, post.Title);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog post: {PostId}", id);
                throw;
            }
        }

        public async Task<BlogPostDTO> GetPostByIdAsync(int id, bool incrementViewCount = false)
        {
            var post = await _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                throw new NotFoundException("Blog yazısı bulunamadı.");
            }

            if (incrementViewCount)
            {
                post.ViewCount++;
                await _context.SaveChangesAsync();
            }

            return MapToFullDto(post);
        }

        public async Task<List<BlogPostListDTO>> GetAllPostsAsync(bool includeInactive = false)
        {
            var query = _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive);
            }

            var posts = await query
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return posts.Select(MapToListDto).ToList();
        }

        public async Task<List<BlogPostListDTO>> GetPublishedPostsAsync()
        {
            var posts = await _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Where(p => p.IsActive && p.PublishedDate != null && p.PublishedDate <= DateTime.UtcNow)
                .OrderByDescending(p => p.PublishedDate)
                .ToListAsync();

            return posts.Select(MapToListDto).ToList();
        }

        public async Task<List<BlogPostListDTO>> GetFeaturedPostsAsync()
        {
            var posts = await _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Where(p => p.IsActive && p.IsFeatured && p.PublishedDate != null && p.PublishedDate <= DateTime.UtcNow)
                .OrderByDescending(p => p.PublishedDate)
                .Take(3)
                .ToListAsync();

            return posts.Select(MapToListDto).ToList();
        }

        public async Task<bool> TogglePostStatusAsync(int id)
        {
            try
            {
                var post = await _context.BlogPosts.FindAsync(id);
                if (post == null)
                {
                    throw new NotFoundException("Blog yazısı bulunamadı.");
                }

                post.IsActive = !post.IsActive;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Blog post status toggled: {PostId}, IsActive: {IsActive}", id, post.IsActive);

                return post.IsActive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling blog post status: {PostId}", id);
                throw;
            }
        }

        public async Task<bool> ToggleFeaturedAsync(int id)
        {
            try
            {
                var post = await _context.BlogPosts.FindAsync(id);
                if (post == null)
                {
                    throw new NotFoundException("Blog yazısı bulunamadı.");
                }

                post.IsFeatured = !post.IsFeatured;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Blog post featured status toggled: {PostId}, IsFeatured: {IsFeatured}", id, post.IsFeatured);

                return post.IsFeatured;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling blog post featured status: {PostId}", id);
                throw;
            }
        }

        // Private helper methods
        private string CalculateReadTime(string content)
        {
            // Remove HTML tags for word count
            var plainText = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", string.Empty);
            var wordCount = plainText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            var minutes = Math.Max(1, wordCount / 200); // 200 words per minute
            return $"{minutes} min";
        }

        private BlogPostDTO MapToFullDto(BlogPost post)
        {
            return new BlogPostDTO
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Excerpt = post.Excerpt,
                ImageUrl = post.ImageUrl,
                CategoryId = post.CategoryId,
                CategoryName = post.Category?.Name ?? "",
                AuthorId = post.AuthorId,
                AuthorName = post.Author?.UserName ?? "",
                PublishedDate = post.PublishedDate,
                IsActive = post.IsActive,
                IsFeatured = post.IsFeatured,
                ViewCount = post.ViewCount,
                ReadTime = post.ReadTime,
                CreatedAt = post.CreatedDate,
                UpdatedAt = post.UpdatedDate
            };
        }

        private BlogPostListDTO MapToListDto(BlogPost post)
        {
            return new BlogPostListDTO
            {
                Id = post.Id,
                Title = post.Title,
                Excerpt = post.Excerpt,
                ImageUrl = post.ImageUrl,
                CategoryId = post.CategoryId,
                CategoryName = post.Category?.Name ?? "",
                AuthorName = post.Author?.UserName ?? "",
                PublishedDate = post.PublishedDate,
                IsActive = post.IsActive,
                IsFeatured = post.IsFeatured,
                ViewCount = post.ViewCount,
                ReadTime = post.ReadTime,
                CreatedAt = post.CreatedDate
            };
        }
    }
}
