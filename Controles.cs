using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cachipun;

/// <summary>Colores de la app, todos en un solo lugar para cambiarlos fácil.</summary>
internal static class Tema
{
    public static readonly Color Fondo = Color.FromArgb(247, 248, 252);
    public static readonly Color FondoTarjeta = Color.White;
    public static readonly Color FondoDuelo = Color.FromArgb(248, 247, 255);
    public static readonly Color FondoFicha = Color.FromArgb(249, 250, 253);
    public static readonly Color FondoRacha = Color.FromArgb(255, 247, 226);
    public static readonly Color Neutro = Color.FromArgb(245, 246, 250);
    public static readonly Color Borde = Color.FromArgb(226, 228, 238);
    public static readonly Color Tinta = Color.FromArgb(39, 43, 61);
    public static readonly Color Suave = Color.FromArgb(112, 119, 139);
    public static readonly Color Morado = Color.FromArgb(112, 86, 235);
    public static readonly Color MoradoClaro = Color.FromArgb(241, 237, 255);
    public static readonly Color Verde = Color.FromArgb(44, 154, 95);
    public static readonly Color Rojo = Color.FromArgb(219, 77, 91);
    public static readonly Color Dorado = Color.FromArgb(194, 139, 28);

    /// <summary>Mezcla dos colores: t = 0 devuelve "a", t = 1 devuelve "b".</summary>
    public static Color Mezclar(Color a, Color b, double t)
    {
        t = Math.Max(0, Math.Min(1, t));
        return Color.FromArgb(
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }
}

/// <summary>
/// Panel blanco con borde fino y un poco de espacio interior: la "tarjeta" que usa toda la interfaz.
/// Se estira solo para llenar la celda donde se pone.
/// </summary>
internal sealed class Tarjeta : Panel
{
    public Tarjeta()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Tema.FondoTarjeta;
        Padding = new Padding(16, 12, 16, 12);
        Margin = new Padding(0);
        Dock = DockStyle.Fill;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using (var lapiz = new Pen(Tema.Borde))
        {
            e.Graphics.DrawRectangle(lapiz, 0, 0, Width - 1, Height - 1);
        }
    }
}

/// <summary>Un renglón del historial: el texto y el color con que se dibuja.</summary>
internal sealed class RegistroHistorial
{
    public RegistroHistorial(string texto, Color color)
    {
        Texto = texto;
        Color = color;
    }

    public string Texto { get; }
    public Color Color { get; }

    public override string ToString()
    {
        return Texto;
    }
}
