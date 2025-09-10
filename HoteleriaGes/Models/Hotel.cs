using System.ComponentModel.DataAnnotations;

namespace HoteleriaGes.Models
{
    public class Hotel
    {
    [Key]
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Direccion { get; set; }
    public double Rating { get; set; }
    public string? Ciudad { get; set; }
    public string? ImagenUrl { get; set; }
    public string? Descripcion { get; set; }
    }
}
