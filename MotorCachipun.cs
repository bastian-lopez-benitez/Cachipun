using System;
using System.Collections.Generic;
using System.Linq;

namespace Cachipun;

/// <summary>
/// Lleva la cuenta de la partida: victorias, derrotas, empates y rachas.
/// Regla de la casa: un empate no suma ni corta la racha.
/// </summary>
public sealed class Estadisticas
{
    public int Victorias { get; private set; }
    public int Derrotas { get; private set; }
    public int Empates { get; private set; }

    /// <summary>Victorias seguidas ahora mismo.</summary>
    public int RachaActual { get; private set; }

    /// <summary>La racha de victorias más larga de la sesión.</summary>
    public int MejorRacha { get; private set; }

    /// <summary>Derrotas seguidas ahora mismo.</summary>
    public int RachaDerrotas { get; private set; }

    public int EmpatesSeguidos { get; private set; }

    /// <summary>Resultado de la ronda anterior (null si todavía no se juega ninguna).</summary>
    public Resultado? Anterior { get; private set; }

    /// <summary>Se pone en true cuando la última ronda batió la mejor racha (desde 3 victorias seguidas).</summary>
    public bool BatioRecord { get; private set; }

    public int Rondas
    {
        get { return Victorias + Derrotas + Empates; }
    }

    /// <summary>Porcentaje de rondas ganadas (0 a 100). Los empates cuentan como ronda jugada.</summary>
    public double TasaExito
    {
        get { return Rondas == 0 ? 0 : Victorias * 100.0 / Rondas; }
    }

    public void Registrar(Resultado resultado)
    {
        BatioRecord = false;

        switch (resultado)
        {
            case Resultado.Ganaste:
                Victorias++;
                RachaActual++;
                RachaDerrotas = 0;
                EmpatesSeguidos = 0;
                BatioRecord = RachaActual > MejorRacha && RachaActual >= 3;
                MejorRacha = Math.Max(MejorRacha, RachaActual);
                break;

            case Resultado.Perdiste:
                Derrotas++;
                RachaDerrotas++;
                RachaActual = 0;
                EmpatesSeguidos = 0;
                break;

            default:
                Empates++;
                EmpatesSeguidos++;
                break;
        }

        Anterior = resultado;
    }

    public void Reiniciar()
    {
        Victorias = Derrotas = Empates = 0;
        RachaActual = MejorRacha = RachaDerrotas = EmpatesSeguidos = 0;
        Anterior = null;
        BatioRecord = false;
    }
}

/// <summary>
/// El "cerebro" de la compu. No sabe nada de ventanas ni botones, solo de jugadas.
///
/// Importante: la compu decide ANTES de enterarse de tu jugada de esta ronda. Por eso
/// <see cref="ElegirJugadaCompu"/> no recibe tu jugada, y después hay que llamar a
/// <see cref="RegistrarJugadaDelJugador"/>. Así no hace trampa.
/// </summary>
public sealed class MotorCachipun
{
    private readonly Random azar;
    private readonly int[] frecuencia = new int[3];              // cuántas veces has tirado cada jugada
    private readonly int[,] transiciones = new int[3, 3];        // [jugada anterior, jugada siguiente]
    private Jugada? ultima;

    public MotorCachipun(Random? azar = null)
    {
        this.azar = azar ?? new Random();
    }

    public Jugada JugadaAlAzar()
    {
        return (Jugada)azar.Next(3);
    }

    public Jugada ElegirJugadaCompu(Dificultad nivel)
    {
        switch (nivel)
        {
            case Dificultad.Facil:
                return JugadaAlAzar();

            case Dificultad.Normal:
                // 4 de cada 10 veces le gana a tu última jugada; el resto, al azar.
                if (ultima is Jugada anterior && azar.NextDouble() < 0.40)
                    return anterior.QuienLeGana();
                return JugadaAlAzar();

            default:
                // Difícil: intenta adivinar qué vas a tirar y le gana (7 de cada 10 veces).
                Jugada? prediccion = PredecirJugadaDelJugador();
                if (prediccion is Jugada esperada && azar.NextDouble() < 0.70)
                    return esperada.QuienLeGana();
                return JugadaAlAzar();
        }
    }

    public void RegistrarJugadaDelJugador(Jugada jugada)
    {
        if (ultima is Jugada anterior)
            transiciones[(int)anterior, (int)jugada]++;

        frecuencia[(int)jugada]++;
        ultima = jugada;
    }

    public void Reiniciar()
    {
        Array.Clear(frecuencia, 0, frecuencia.Length);
        Array.Clear(transiciones, 0, transiciones.Length);
        ultima = null;
    }

    /// <summary>
    /// Primero mira qué sueles tirar DESPUÉS de tu última jugada (por ejemplo, "después de piedra
    /// casi siempre tira papel"). Si todavía hay pocos datos, usa la jugada que más has tirado.
    /// </summary>
    private Jugada? PredecirJugadaDelJugador()
    {
        if (ultima is Jugada anterior)
        {
            int[] fila =
            {
                transiciones[(int)anterior, 0],
                transiciones[(int)anterior, 1],
                transiciones[(int)anterior, 2]
            };

            if (fila.Sum() >= 3)
                return (Jugada)IndiceMayor(fila);
        }

        if (frecuencia.Sum() >= 2)
            return (Jugada)IndiceMayor(frecuencia);

        return null;
    }

    /// <summary>Índice del valor más grande; si hay empate entre varios, elige uno al azar.</summary>
    private int IndiceMayor(int[] valores)
    {
        int maximo = valores.Max();
        var empatados = new List<int>();

        for (int i = 0; i < valores.Length; i++)
        {
            if (valores[i] == maximo) empatados.Add(i);
        }

        return empatados[azar.Next(empatados.Count)];
    }
}
