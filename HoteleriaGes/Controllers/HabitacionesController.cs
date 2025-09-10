using HoteleriaGes.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace HoteleriaGes.Controllers
{
        public class HabitacionesController : Controller
        {
            // ...existing code...
            // Acción para editar habitación (GET)
            [HttpGet]
            public IActionResult Editar(int id)
            {
                var rol = HttpContext.Session.GetString("Rol");
                if (rol != "admin")
                    return RedirectToAction("Disponibles");

                Habitacion habitacion = null;
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT * FROM Habitaciones WHERE id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        habitacion = new Habitacion
                        {
                            Id = reader.GetInt32("id"),
                            Numero = reader.IsDBNull(reader.GetOrdinal("numero")) ? string.Empty : reader.GetString("numero"),
                            Tipo = reader.IsDBNull(reader.GetOrdinal("tipo")) ? string.Empty : reader.GetString("tipo"),
                            Precio = reader.IsDBNull(reader.GetOrdinal("precio")) ? 0 : reader.GetDecimal("precio"),
                            Estado = reader.IsDBNull(reader.GetOrdinal("estado")) ? string.Empty : reader.GetString("estado")
                        };
                    }
                }
                if (habitacion == null)
                    return RedirectToAction("Disponibles");
                return View(habitacion);
            }

            // Acción para editar habitación (POST)
            [HttpPost]
            public IActionResult Editar(Habitacion habitacion)
            {
                var rol = HttpContext.Session.GetString("Rol");
                if (rol != "admin")
                    return RedirectToAction("Disponibles");
                if (!ModelState.IsValid)
                    return View(habitacion);

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("UPDATE Habitaciones SET numero=@numero, tipo=@tipo, precio=@precio, estado=@estado WHERE id=@id", conn);
                    cmd.Parameters.AddWithValue("@numero", habitacion.Numero);
                    cmd.Parameters.AddWithValue("@tipo", habitacion.Tipo);
                    cmd.Parameters.AddWithValue("@precio", habitacion.Precio);
                    cmd.Parameters.AddWithValue("@estado", habitacion.Estado);
                    cmd.Parameters.AddWithValue("@id", habitacion.Id);
                    try
                    {
                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                            return RedirectToAction("Disponibles");
                        else
                        {
                            ViewBag.Error = "No se pudo actualizar la habitación.";
                            return View(habitacion);
                        }
                    }
                    catch (MySqlException ex)
                    {
                        ViewBag.Error = $"Error: {ex.Message}";
                        return View(habitacion);
                    }
                }
            }

            // Acción para borrar habitación
            [HttpPost]
            public IActionResult Borrar(int id)
            {
                var rol = HttpContext.Session.GetString("Rol");
                if (rol != "admin")
                    return RedirectToAction("Disponibles");

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("DELETE FROM Habitaciones WHERE id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                            return RedirectToAction("Disponibles");
                        else
                        {
                            ViewBag.Error = "No se pudo borrar la habitación.";
                            return RedirectToAction("Disponibles");
                        }
                    }
                    catch (MySqlException ex)
                    {
                        ViewBag.Error = $"Error: {ex.Message}";
                        return RedirectToAction("Disponibles");
                    }
                }
            }
                   private readonly string connectionString = "server=localhost;database=hoteleriaweb;user=root;password=;";

        public IActionResult Disponibles()
        {
            var habitaciones = new List<Habitacion>();
            var hoteles = new Dictionary<int, string>();
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // Traer habitaciones y nombre del hotel
                var cmd = new MySqlCommand("SELECT h.*, ht.Nombre as HotelNombre FROM Habitaciones h LEFT JOIN Hoteles ht ON h.HotelId = ht.Id WHERE h.estado='disponible'", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var habitacion = new Habitacion
                    {
                        Id = reader.GetInt32("id"),
                        Numero = reader.GetString("numero"),
                        Tipo = reader.GetString("tipo"),
                        Precio = reader.GetDecimal("precio"),
                        Estado = reader.GetString("estado"),
                        // Se asume que tienes HotelId en el modelo
                    };
                    habitaciones.Add(habitacion);
                    int hotelId = reader.IsDBNull(reader.GetOrdinal("HotelId")) ? 0 : reader.GetInt32("HotelId");
                    string hotelNombre = reader.IsDBNull(reader.GetOrdinal("HotelNombre")) ? "" : reader.GetString("HotelNombre");
                    hoteles[habitacion.Id] = hotelNombre;
                }
            }
            ViewBag.HotelNombres = hoteles;
            return View(habitaciones);
        }

        [HttpGet]
            public IActionResult Agregar()
            {
                var rol = HttpContext.Session.GetString("Rol");
                if (rol != "admin")
                {
                    return RedirectToAction("Disponibles");
                }
                // Obtener el hotel seleccionado de la sesión
                int? hotelId = HttpContext.Session.GetInt32("HotelSeleccionadoId");
                HoteleriaGes.Models.Hotel hotel = null;
                if (hotelId.HasValue)
                {
                    using (var conn = new MySql.Data.MySqlClient.MySqlConnection(connectionString))
                    {
                        conn.Open();
                        var cmd = new MySql.Data.MySqlClient.MySqlCommand("SELECT * FROM Hoteles WHERE Id=@id", conn);
                        cmd.Parameters.AddWithValue("@id", hotelId.Value);
                        var reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            hotel = new HoteleriaGes.Models.Hotel
                            {
                                Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32("Id"),
                                Nombre = reader.IsDBNull(reader.GetOrdinal("Nombre")) ? string.Empty : reader.GetString("Nombre"),
                                Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? string.Empty : reader.GetString("Direccion"),
                                Rating = reader.IsDBNull(reader.GetOrdinal("Rating")) ? 0 : reader.GetDouble("Rating"),
                                Ciudad = reader.IsDBNull(reader.GetOrdinal("Ciudad")) ? string.Empty : reader.GetString("Ciudad"),
                                ImagenUrl = reader.IsDBNull(reader.GetOrdinal("ImagenUrl")) ? string.Empty : reader.GetString("ImagenUrl"),
                                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? string.Empty : reader.GetString("Descripcion")
                            };
                        }
                    }
                }
                ViewBag.Hotel = hotel;
                return View();
            }

        [HttpPost]
        public IActionResult Agregar(Habitacion habitacion)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "admin")
            {
                return RedirectToAction("Disponibles");
            }
            if (!ModelState.IsValid)
                return View(habitacion);

            // Obtener el hotel seleccionado de la sesión
            int? hotelId = HttpContext.Session.GetInt32("HotelSeleccionadoId");
            if (!hotelId.HasValue)
            {
                ViewBag.Error = "No hay hotel seleccionado. Selecciona un hotel antes de agregar habitaciones.";
                return View(habitacion);
            }

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("INSERT INTO Habitaciones (numero, tipo, precio, estado, HotelId) VALUES (@numero, @tipo, @precio, @estado, @hotelId)", conn);
                cmd.Parameters.AddWithValue("@numero", habitacion.Numero);
                cmd.Parameters.AddWithValue("@tipo", habitacion.Tipo);
                cmd.Parameters.AddWithValue("@precio", habitacion.Precio);
                cmd.Parameters.AddWithValue("@estado", habitacion.Estado);
                cmd.Parameters.AddWithValue("@hotelId", hotelId.Value);
                try
                {
                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                        return RedirectToAction("Disponibles");
                    else
                    {
                        ViewBag.Error = "No se pudo guardar la habitación.";
                        return View(habitacion);
                    }
                }
                catch (MySqlException ex)
                {
                    ViewBag.Error = $"Error: {ex.Message}";
                    return View(habitacion);
                }
            }
        }
    }
}
