using Library_web.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;



namespace Library_web.Controllers
{

    public class AuthorController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _baseUrl = "https://localhost:7178/api/Authors";

        public AuthorController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // ------------------- GET: Liệt kê tất cả tác giả -------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");

            // ✅ Kiểm tra đăng nhập
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "⚠️ Vui lòng đăng nhập trước khi truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            // ✅ Gắn token JWT vào Header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // ✅ Gọi API lấy danh sách tác giả
            var response = await client.GetAsync($"{_baseUrl}/get-all-author");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "❌ Không thể tải danh sách tác giả!";
                return View(new List<authorDTO>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var authors = JsonSerializer.Deserialize<List<authorDTO>>(json, _jsonOptions)
                           ?? new List<authorDTO>();

            return View(authors);
        }


        // ------------------- GET: Chi tiết tác giả -------------------
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_baseUrl}/get-author-by-id/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "❌ Không tìm thấy tác giả!";
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var author = JsonSerializer.Deserialize<authorDTO>(json, _jsonOptions);
            return View(author);
        }

        // ------------------- GET: Trang thêm tác giả -------------------
        [HttpGet]
        public IActionResult Add() => View();

        // ------------------- POST: Gửi form thêm tác giả -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(authorDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(model);
            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

            // 🟢 Sửa đúng endpoint theo Swagger (add-authors)
            var response = await client.PostAsync($"{_baseUrl}/add-authors", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "✅ Thêm tác giả thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "❌ Lỗi khi thêm tác giả!";
            return View(model);
        }

        // ------------------- GET: Trang chỉnh sửa tác giả -------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_baseUrl}/get-author-by-id/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "❌ Không tìm thấy tác giả!";
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var author = JsonSerializer.Deserialize<authorDTO>(json, _jsonOptions);
            return View(author);
        }

        // ------------------- POST: Cập nhật tác giả -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, authorDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(model);
            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{_baseUrl}/update-author-by-id/{id}", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "✅ Cập nhật tác giả thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "❌ Cập nhật thất bại!";
            return View(model);
        }

        // ------------------- POST: Xóa tác giả -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"{_baseUrl}/delete-author-by-id/{id}");

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "✅ Đã xóa tác giả!";
            else
                TempData["ErrorMessage"] = "❌ Xóa thất bại!";

            return RedirectToAction(nameof(Index));
        }
    }
}