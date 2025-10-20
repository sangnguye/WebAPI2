using Library_web.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;



namespace Library_web.Controllers
{

    public class PublisherController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _baseUrl = "https://localhost:7178/api/Publishers";

        public PublisherController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // ------------------- GET: Liệt kê tất cả -------------------
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

            // ✅ Gắn JWT vào Header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // ✅ Gọi API
            var response = await client.GetAsync($"{_baseUrl}/get-all-publisher");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "❌ Không thể tải danh sách nhà xuất bản!";
                return View(new List<publisherDTO>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var publishers = JsonSerializer.Deserialize<List<publisherDTO>>(json, _jsonOptions)
                              ?? new List<publisherDTO>();

            return View(publishers);
        }


        // ------------------- GET: Chi tiết nhà xuất bản -------------------
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_baseUrl}/get-publisher-by-id/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "❌ Không tìm thấy nhà xuất bản!";
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var publisher = JsonSerializer.Deserialize<publisherDTO>(json, _jsonOptions);
            return View(publisher);
        }

        // ------------------- GET: Trang thêm -------------------
        [HttpGet]
        public IActionResult Add() => View();

        // ------------------- POST: Thêm -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(publisherDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_baseUrl}/add-publisher", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "✅ Thêm nhà xuất bản thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "❌ Lỗi khi thêm nhà xuất bản!";
            return View(model);
        }

        // ------------------- GET: Trang chỉnh sửa -------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_baseUrl}/get-publisher-by-id/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "❌ Không tìm thấy nhà xuất bản!";
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var publisher = JsonSerializer.Deserialize<publisherDTO>(json, _jsonOptions);
            return View(publisher);
        }

        // ------------------- POST: Cập nhật -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, publisherDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{_baseUrl}/update-publisher-by-id/{id}", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "✅ Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "❌ Cập nhật thất bại!";
            return View(model);
        }

        // ------------------- POST: Xóa -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"{_baseUrl}/delete-publisher-by-id/{id}");

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "✅ Đã xóa nhà xuất bản!";
            else
                TempData["ErrorMessage"] = "❌ Xóa thất bại!";

            return RedirectToAction(nameof(Index));
        }
    }
}