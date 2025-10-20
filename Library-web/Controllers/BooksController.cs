using Library_web.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers; 
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Library_web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IHttpContextAccessor httpContextAccessor;
        public BooksController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            this.httpClientFactory = httpClientFactory;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index([FromQuery] string filterOn = null, string filterQuery = null, string sortBy = null, bool isAscending = true)
        {
            var token = httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "⚠️ Vui lòng đăng nhập trước!";
                return RedirectToAction("Login", "Account"); // Đổi "Account" thành tên Controller đăng nhập của bạn
            }

            List<BookDTO> response = new List<BookDTO>(); // tạo đối tượng với model Book
            try
            {
                //lấy dữ liệu books from API
                var client = httpClientFactory.CreateClient(); //khởi tạo Client
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var httpResponseMess = await client.GetAsync("https://localhost:7178/api/Books/get-all-books?filterOn=" + filterOn + "&filterQuery=" + filterQuery + "&sortBy=" + sortBy + "&isAscending=" + isAscending);
                httpResponseMess.EnsureSuccessStatusCode(); // kiểm tra mã trạng thái trả về 200
                response.AddRange(await httpResponseMess.Content.ReadFromJsonAsync<IEnumerable<BookDTO>>());
                // đổi kiểu dữ liệu từ Json sang mảng đối tượng BookDTO
            }
            catch (Exception ex)
            {
                return View("Error");
            }
            return View(response); //truyền dữ liệu sang View thông qua biến response
        }

        [HttpGet]
        public async Task<IActionResult> addBook()
        {
            var token = httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "⚠️ Vui lòng đăng nhập trước!";
                return View();
            }

            try
            {
                var client = httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Lấy danh sách tác giả
                List<authorDTO> authors = new List<authorDTO>();
                var httpResponseAuthors = await client.GetAsync("https://localhost:7178/api/Authors/get-all-authors");
                httpResponseAuthors.EnsureSuccessStatusCode();
                authors.AddRange(await httpResponseAuthors.Content.ReadFromJsonAsync<IEnumerable<authorDTO>>());
                ViewBag.listAuthor = authors;

                // Lấy danh sách nhà xuất bản
                List<publisherDTO> publishers = new List<publisherDTO>();
                var httpResponsePublishers = await client.GetAsync("https://localhost:7178/api/Publishers/get-all-publishers");
                httpResponsePublishers.EnsureSuccessStatusCode();
                publishers.AddRange(await httpResponsePublishers.Content.ReadFromJsonAsync<IEnumerable<publisherDTO>>());
                ViewBag.listPublisher = publishers;
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            // Trả về view hiển thị form thêm sách
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> addBook(addBookDTO addBookDTO)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                var httpRequestMess = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://localhost:7178/api/Books/add-book"),  // Thay URL
                    Content = new StringContent(JsonSerializer.Serialize(addBookDTO), Encoding.UTF8, MediaTypeNames.Application.Json)
                };
                //Console.WriteLine(JsonSerializer(addBookDTO));
                var httpResponseMess = await client.SendAsync(httpRequestMess);
                httpResponseMess.EnsureSuccessStatusCode();
                var response = await httpRequestMess.Content.ReadFromJsonAsync<addBookDTO>();
                if (response != null)
                {
                    return RedirectToAction("Index", "Books");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View();
        }

        public async Task<IActionResult> listBook(int id)
        {
            BookDTO response = new BookDTO();
            try
            {
                //lay du lieu books from API
                var client = httpClientFactory.CreateClient();
                var httpResponseMess = await client.GetAsync("https://localhost:7178/api/Books/get-book-by-id/" + id);
                httpResponseMess.EnsureSuccessStatusCode();
                var stringReponseBody = await httpResponseMess.Content.ReadAsStringAsync();
                response = await httpResponseMess.Content.ReadFromJsonAsync<BookDTO>();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View(response);
        }

        [HttpGet]
        public async Task<IActionResult> editBook(int id)
        {
            BookDTO responseBook = new BookDTO();
            var client = httpClientFactory.CreateClient();
            var httpResponseMess = await client.GetAsync("https://localhost:7178/api/Books/get-book-by-id/" + id);
            httpResponseMess.EnsureSuccessStatusCode();
            responseBook = await httpResponseMess.Content.ReadFromJsonAsync<BookDTO>();
            ViewBag.Book = responseBook;

            List<authorDTO> responseAu = new List<authorDTO>();
            var httpResponseAu = await client.GetAsync("https://localhost:7178/api/Authors/get-all-authors");
            httpResponseAu.EnsureSuccessStatusCode();
            responseAu.AddRange(await httpResponseAu.Content.ReadFromJsonAsync<IEnumerable<authorDTO>>());
            ViewBag.listAuthor = responseAu;

            List<publisherDTO> responsePu = new List<publisherDTO>();
            var httpResponsePu = await client.GetAsync("https://localhost:7178/api/Publishers/get-all-publishers");
            httpResponsePu.EnsureSuccessStatusCode();
            responsePu.AddRange(await httpResponsePu.Content.ReadFromJsonAsync<IEnumerable<publisherDTO>>());
            ViewBag.listPublisher = responsePu;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> editBook([FromRoute] int id, editBookDTO bookDTO)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                var httpRequestMess = new HttpRequestMessage()
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri("https://localhost:7178/api/Books/update-book-by-id/" + id),
                    Content = new StringContent(JsonSerializer.Serialize(bookDTO), Encoding.UTF8, MediaTypeNames.Application.Json)
                };

                var httpResponseMess = await client.SendAsync(httpRequestMess);
                httpResponseMess.EnsureSuccessStatusCode();
                var response = await httpRequestMess.Content.ReadFromJsonAsync<addBookDTO>();
                if (response != null)
                {
                    return RedirectToAction("Index", "Books");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> delBook([FromRoute] int id)
        {
            try
            {
                //lay du lieu books from API
                var client = httpClientFactory.CreateClient();
                var httpResponseMess = await client.DeleteAsync("https://localhost:7178/api/Books/delete-book-by-id/" + id);
                httpResponseMess.EnsureSuccessStatusCode();
                return RedirectToAction("Index", "Books");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View("Index");
        }
    }
}
