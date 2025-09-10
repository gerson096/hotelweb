
using HoteleriaGes.Models;
using HoteleriaGes.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace HoteleriaGes.Controllers
{
    public class HotelesController : Controller
    {
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

        public HotelesController(ApplicationDbContext context, IConfiguration configuration)
        {
            
            _httpClient = new HttpClient();
            _context = context;
            _configuration = configuration;
        }
        
        // Vista para seleccionar hotel existente o agregar nuevo
        public IActionResult Seleccionar()
        {
            var hoteles = _context.Hoteles.ToList();
            return View(hoteles);
        }
        public IActionResult Agregar()
        {
            var hoteles = _context.Hoteles.ToList();
            return View(hoteles);
        }
        [HttpPost]
        public IActionResult Seleccionar(int id)
        {
            HttpContext.Session.SetInt32("HotelSeleccionadoId", id);
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Populares()
        {
            // Parámetros recomendados para SerpApi
            string apiKey = _configuration["SerpApi:ApiKey"];
            string checkIn = DateTime.Now.ToString("yyyy-MM-dd");
            string checkOut = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            string url = $"https://serpapi.com/search.json?engine=google_hotels&q=Hoteles+en+Nicaragua&check_in_date={checkIn}&check_out_date={checkOut}&adults=2&currency=USD&gl=ni&hl=es&api_key={apiKey}";
            var response = await _httpClient.GetAsync(url);
            var hoteles = new List<Hotel>();
            string errorMsg = null;
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                // Procesar 'properties' y 'ads'
                if (doc.RootElement.TryGetProperty("properties", out var properties))
                {
                    foreach (var h in properties.EnumerateArray())
                    {
                        hoteles.Add(new Hotel
                        {
                            Nombre = h.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty,
                            Direccion = h.TryGetProperty("address", out var dir) ? dir.GetString() ?? string.Empty : string.Empty,
                            Rating = h.TryGetProperty("overall_rating", out var r) ? r.GetDouble() : 0,
                            Ciudad = "Nicaragua",
                            ImagenUrl = h.TryGetProperty("images", out var imgs) && imgs.GetArrayLength() > 0 && imgs[0].TryGetProperty("thumbnail", out var thumb) ? thumb.GetString() ?? string.Empty : string.Empty,
                            Descripcion = h.TryGetProperty("description", out var d) ? d.GetString() ?? string.Empty : string.Empty
                        });
                    }
                }
                if (doc.RootElement.TryGetProperty("ads", out var ads))
                {
                    foreach (var h in ads.EnumerateArray())
                    {
                        hoteles.Add(new Hotel
                        {
                            Nombre = h.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty,
                            Direccion = h.TryGetProperty("address", out var dir) ? dir.GetString() ?? string.Empty : string.Empty,
                            Rating = h.TryGetProperty("overall_rating", out var r) ? r.GetDouble() : 0,
                            Ciudad = "Nicaragua",
                            ImagenUrl = h.TryGetProperty("thumbnail", out var img) ? img.GetString() ?? string.Empty : string.Empty,
                            Descripcion = h.TryGetProperty("description", out var d) ? d.GetString() ?? string.Empty : string.Empty
                        });
                    }
                }
                if (hoteles.Count == 0)
                {
                    errorMsg = "No se encontraron hoteles disponibles en este momento.";
                }
            }
            else
            {
                errorMsg = "Error al consultar la API de hoteles. Intente más tarde.";
            }
            ViewBag.ErrorMsg = errorMsg;
            return View(hoteles);
        }

        [HttpPost]
            public async Task<IActionResult> Agregar(Hotel hotel)
            {
                if (string.IsNullOrEmpty(hotel.Nombre))
                {
                    Random rnd = new Random();
                    string[] lugares = { "Managua", "Granada", "León", "San Juan del Sur", "Matagalpa" };
                    string ciudad = lugares[rnd.Next(lugares.Length)];
                    int diasCheckIn = rnd.Next(0, 7); 
                    int diasEstadia = rnd.Next(1, 5); 
                    ModelState.AddModelError("Nombre", "El nombre del hotel es obligatorio.");
                    string apiKey = _configuration["SerpApi:ApiKey"] ?? "";
                    string checkIn = DateTime.Now.AddDays(diasCheckIn).ToString("yyyy-MM-dd");
                    string checkOut = DateTime.Now.AddDays(diasCheckIn + diasEstadia).ToString("yyyy-MM-dd");
                    string url = $"https://serpapi.com/search.json?engine=google_hotels&q=Hoteles+en+{lugares}&check_in_date={checkIn}&check_out_date={checkOut}&adults=2&currency=USD&gl=ni&hl=es&api_key={apiKey}";
                    var response = await _httpClient.GetAsync(url);
                    var hoteles = new List<Hotel>();
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("properties", out var properties))
                        {
                            foreach (var h in properties.EnumerateArray())
                            {
                                hoteles.Add(new Hotel
                                {
                                    Nombre = h.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty,
                                    Direccion = h.TryGetProperty("address", out var dir) ? dir.GetString() ?? string.Empty : string.Empty,
                                    Rating = h.TryGetProperty("overall_rating", out var r) ? r.GetDouble() : 0,
                                    Ciudad = "Nicaragua",
                                    ImagenUrl = h.TryGetProperty("images", out var imgs) && imgs.GetArrayLength() > 0 && imgs[0].TryGetProperty("thumbnail", out var thumb) ? thumb.GetString() ?? string.Empty : string.Empty,
                                    Descripcion = h.TryGetProperty("description", out var d) ? d.GetString() ?? string.Empty : string.Empty
                                });
                            }
                        }
                        if (doc.RootElement.TryGetProperty("ads", out var ads))
                        {
                            foreach (var h in ads.EnumerateArray())
                            {
                                hoteles.Add(new Hotel
                                {
                                    Nombre = h.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty,
                                    Direccion = h.TryGetProperty("address", out var dir) ? dir.GetString() ?? string.Empty : string.Empty,
                                    Rating = h.TryGetProperty("overall_rating", out var r) ? r.GetDouble() : 0,
                                    Ciudad = "Nicaragua",
                                    ImagenUrl = h.TryGetProperty("thumbnail", out var img) ? img.GetString() ?? string.Empty : string.Empty,
                                    Descripcion = h.TryGetProperty("description", out var d) ? d.GetString() ?? string.Empty : string.Empty
                                });
                            }
                        }
                    }
                    return View(hoteles);
                }
                // Guardar el hotel en la base de datos si no existe
                var existente = _context.Hoteles.FirstOrDefault(h => h.Nombre == hotel.Nombre && h.Direccion == hotel.Direccion);
                if (existente == null)
                {
                    _context.Hoteles.Add(hotel);
                    _context.SaveChanges();
                    HttpContext.Session.SetInt32("HotelSeleccionadoId", hotel.Id);
                }
                else
                {
                    // Si ya existe, seleccionarlo
                    HttpContext.Session.SetInt32("HotelSeleccionadoId", existente.Id);
                }
                return RedirectToAction("Agregar", "Habitaciones");
            }
    }
}
