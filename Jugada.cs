using System;

namespace Cachipun;

/// <summary>
/// Las tres jugadas. El orden importa: cada jugada le gana a la que va antes en la lista,
/// y Piedra (la primera) le gana a Tijera (la última). Papel > Piedra > Tijera > Papel.
/// </summary>
public enum Jugada
{
    Piedra = 0,
    Papel = 1,
    Tijera = 2
}

/// <summary>Cómo terminó una ronda, mirado desde el jugador.</summary>
public enum Resultado
{
    Ganaste,
    Perdiste,
    Empate
}

/// <summary>Los dos modos de la app: jugar contra la compu o pedir una jugada al azar.</summary>
public enum ModoJuego
{
    ContraLaCompu,
    Azar
}

/// <summary>Niveles de la compu. El número coincide con la posición en el ComboBox.</summary>
public enum Dificultad
{
    Facil = 0,
    Normal = 1,
    Dificil = 2
}

/// <summary>
/// Métodos de ayuda para las jugadas: nombres con su artículo correcto (la piedra, el papel,
/// la tijera), plurales, emojis y las reglas del juego. Las frases chilenas usan esto para no
/// decir cosas como "el piedra" o "dos papel seguidas".
/// </summary>
public static class JugadaExtensiones
{
    public static readonly Jugada[] Todas = { Jugada.Piedra, Jugada.Papel, Jugada.Tijera };

    /// <summary>"piedra", "papel", "tijera" (en minúscula, para meter en frases).</summary>
    public static string Nombre(this Jugada j)
    {
        switch (j)
        {
            case Jugada.Piedra: return "piedra";
            case Jugada.Papel: return "papel";
            default: return "tijera";
        }
    }

    /// <summary>"Piedra", "Papel", "Tijera" (para títulos y botones).</summary>
    public static string NombreTitulo(this Jugada j)
    {
        string n = j.Nombre();
        return char.ToUpper(n[0]) + n.Substring(1);
    }

    /// <summary>"la" para piedra y tijera, "el" para papel.</summary>
    public static string Articulo(this Jugada j)
    {
        return j == Jugada.Papel ? "el" : "la";
    }

    /// <summary>"la piedra", "el papel", "la tijera".</summary>
    public static string ConArticulo(this Jugada j)
    {
        return j.Articulo() + " " + j.Nombre();
    }

    /// <summary>"piedras", "papeles", "tijeras".</summary>
    public static string Plural(this Jugada j)
    {
        switch (j)
        {
            case Jugada.Piedra: return "piedras";
            case Jugada.Papel: return "papeles";
            default: return "tijeras";
        }
    }

    /// <summary>"esa piedra", "ese papel", "esa tijera".</summary>
    public static string Esa(this Jugada j)
    {
        return j == Jugada.Papel ? "ese papel" : "esa " + j.Nombre();
    }

    /// <summary>Emoji de la mano. En WinForms salen en blanco y negro y toman el color del texto.</summary>
    public static string Emoji(this Jugada j)
    {
        switch (j)
        {
            case Jugada.Piedra: return "✊";
            case Jugada.Papel: return "✋";
            default: return "✌";
        }
    }

    /// <summary>La jugada que le gana a esta (Piedra → Papel, Papel → Tijera, Tijera → Piedra).</summary>
    public static Jugada QuienLeGana(this Jugada j)
    {
        return (Jugada)(((int)j + 1) % 3);
    }

    /// <summary>La jugada a la que esta le gana (Piedra → Tijera, Papel → Piedra, Tijera → Papel).</summary>
    public static Jugada AQuienLeGana(this Jugada j)
    {
        return (Jugada)(((int)j + 2) % 3);
    }

    /// <summary>Resultado de tirar "tuya" contra "otra", mirado desde quien tiró "tuya".</summary>
    public static Resultado ContraOtra(this Jugada tuya, Jugada otra)
    {
        if (tuya == otra) return Resultado.Empate;
        return otra == tuya.AQuienLeGana() ? Resultado.Ganaste : Resultado.Perdiste;
    }

    /// <summary>La regla completa de la jugada ganadora, como en la web de referencia ("Papel cubre a Piedra").</summary>
    public static string Regla(this Jugada ganadora)
    {
        switch (ganadora)
        {
            case Jugada.Piedra: return "La piedra aplasta a la tijera";
            case Jugada.Papel: return "El papel envuelve a la piedra";
            default: return "La tijera corta el papel";
        }
    }

    /// <summary>Versión corta para los botones.</summary>
    public static string ReglaCorta(this Jugada j)
    {
        switch (j)
        {
            case Jugada.Piedra: return "le gana a la tijera";
            case Jugada.Papel: return "le gana a la piedra";
            default: return "le gana al papel";
        }
    }
}
