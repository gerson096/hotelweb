using HoteleriaGes.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace HoteleriaGes.Controllers
{
    public class ReservasController : Controller
    {
        private readonly string connectionString = "server=localhost;database=hoteleriaweb;user=root;password=;";

        [HttpGet]
        public IActionResult Crear()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "cliente")
            {
                return RedirectToAction("Login", "Auth");
            }
            // Traer hoteles guardados por el admin
            var hoteles = new List<Hotel>();
            int? hotelId = null;
            if (Request.Query.ContainsKey("hotelId"))
            {
                if (int.TryParse(Request.Query["hotelId"], out int hid))
                {
                    hotelId = hid;
                    HttpContext.Session.SetInt32("HotelSeleccionadoId", hid);
                }
            }
            else
            {
                hotelId = HttpContext.Session.GetInt32("HotelSeleccionadoId");
            }
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmdHoteles = new MySqlCommand("SELECT * FROM Hoteles", conn);
                var readerHoteles = cmdHoteles.ExecuteReader();
                while (readerHoteles.Read())
                {
                    hoteles.Add(new Hotel
                    {
                        Id = readerHoteles.IsDBNull(readerHoteles.GetOrdinal("Id")) ? 0 : readerHoteles.GetInt32("Id"),
                        Nombre = readerHoteles.IsDBNull(readerHoteles.GetOrdinal("Nombre")) ? string.Empty : readerHoteles.GetString("Nombre"),
                        Direccion = readerHoteles.IsDBNull(readerHoteles.GetOrdinal("Direccion")) ? string.Empty : readerHoteles.GetString("Direccion"),
                        Rating = readerHoteles.IsDBNull(readerHoteles.GetOrdinal("Rating")) ? 0 : readerHoteles.GetDouble("Rating"),
                        Ciudad = readerHoteles.IsDBNull(readerHoteles.GetOrdinal("Ciudad")) ? string.Empty : readerHoteles.GetString("Ciudad"),
                        ImagenUrl = readerHoteles.IsDBNull(readerHoteles.GetOrdinal("ImagenUrl")) ? string.Empty : readerHoteles.GetString("ImagenUrl"),
                        Descripcion = readerHoteles.IsDBNull(readerHoteles.GetOrdinal("Descripcion")) ? string.Empty : readerHoteles.GetString("Descripcion")
                    });
                }
                readerHoteles.Close();

                var habitaciones = new List<Habitacion>();
                if (hotelId.HasValue)
                {
                    var cmdHab = new MySqlCommand("SELECT * FROM Habitaciones WHERE estado='disponible' AND HotelId=@hotelId", conn);
                    cmdHab.Parameters.AddWithValue("@hotelId", hotelId.Value);
                    var readerHab = cmdHab.ExecuteReader();
                    while (readerHab.Read())
                    {
                        habitaciones.Add(new Habitacion
                        {
                            Id = readerHab.GetInt32("id"),
                            Numero = readerHab.GetString("numero"),
                            Tipo = readerHab.GetString("tipo"),
                            Precio = readerHab.GetDecimal("precio"),
                            Estado = readerHab.GetString("estado")
                        });
                    }
                    readerHab.Close();
                }
                ViewBag.Hoteles = hoteles;
                ViewBag.HotelSeleccionadoId = hotelId;
                if (hotelId.HasValue)
                {
                    var hotelSel = hoteles.FirstOrDefault(h => h.Id == hotelId.Value);
                    ViewBag.HotelSeleccionado = hotelSel;
                }
                return View(habitaciones);
            }
        }

    [HttpPost]
    public IActionResult Seleccionar(int habitacionId, int dias)
        {
            // Obtener el usuario actual (correo)
            var correo = HttpContext.Session.GetString("Usuario");
            if (string.IsNullOrEmpty(correo))
                return RedirectToAction("Login", "Auth");

            int clienteId = 0;
            int reservaId = 0;
            decimal monto = 0;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // Buscar el cliente por correo
                var cmdCliente = new MySqlCommand("SELECT id FROM Clientes WHERE correo=@correo", conn);
                cmdCliente.Parameters.AddWithValue("@correo", correo);
                var reader = cmdCliente.ExecuteReader();
                if (reader.Read())
                {
                    clienteId = reader.GetInt32("id");
                }
                reader.Close();

                if (clienteId == 0)
                {
                    // Si no existe, crear el cliente
                    var cmdInsert = new MySqlCommand("INSERT INTO Clientes (nombre, correo) VALUES (@nombre, @correo); SELECT LAST_INSERT_ID();", conn);
                    cmdInsert.Parameters.AddWithValue("@nombre", correo);
                    cmdInsert.Parameters.AddWithValue("@correo", correo);
                    clienteId = Convert.ToInt32(cmdInsert.ExecuteScalar());
                }

                // Obtener el precio de la habitación
                var cmdPrecio = new MySqlCommand("SELECT precio FROM Habitaciones WHERE id=@id", conn);
                cmdPrecio.Parameters.AddWithValue("@id", habitacionId);
                monto = Convert.ToDecimal(cmdPrecio.ExecuteScalar());

                // Crear la reserva
                var cmdReserva = new MySqlCommand("INSERT INTO Reservas (cliente_id, habitacion_id, fecha_entrada, fecha_salida, estado) VALUES (@cliente_id, @habitacion_id, @fecha_entrada, @fecha_salida, @estado); SELECT LAST_INSERT_ID();", conn);
                cmdReserva.Parameters.AddWithValue("@cliente_id", clienteId);
                cmdReserva.Parameters.AddWithValue("@habitacion_id", habitacionId);
                cmdReserva.Parameters.AddWithValue("@fecha_entrada", DateTime.Now.Date);
                cmdReserva.Parameters.AddWithValue("@fecha_salida", DateTime.Now.Date.AddDays(dias));
                cmdReserva.Parameters.AddWithValue("@estado", "activa");
                reservaId = Convert.ToInt32(cmdReserva.ExecuteScalar());

                // Actualizar estado de la habitación
                var cmdUpdate = new MySqlCommand("UPDATE Habitaciones SET estado='ocupada' WHERE id=@id", conn);
                cmdUpdate.Parameters.AddWithValue("@id", habitacionId);
                cmdUpdate.ExecuteNonQuery();
            }
            // Redirigir a la página de pago
            return RedirectToAction("Realizar", "Pagos", new { reservaId = reservaId, monto = monto });
        }
    }
}
