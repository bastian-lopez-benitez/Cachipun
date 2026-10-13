using System;
using System.Collections.Generic;
using System.Linq;

namespace Cachipun;

/// <summary>
/// Nombres de las categorías de frases. Usar estas constantes (y no strings sueltos)
/// evita errores de tipeo.
/// </summary>
public static class Categoria
{
    // Títulos grandes del resultado
    public const string TituloGana = "titulo.gana";
    public const string TituloPierde = "titulo.pierde";
    public const string TituloEmpate = "titulo.empate";
    public const string TituloAzar = "titulo.azar";

    // Comentario de la compu bajo el resultado
    public const string DetalleGana = "detalle.gana";
    public const string DetalleGanaRacha = "detalle.gana.racha";
    public const string DetalleGanaVuelta = "detalle.gana.vuelta";
    public const string DetallePierde = "detalle.pierde";
    public const string DetallePierdeRacha = "detalle.pierde.racha";
    public const string DetallePierdeCortaRacha = "detalle.pierde.cortaracha";
    public const string DetalleEmpate = "detalle.empate";
    public const string DetalleEmpateSeguido = "detalle.empate.seguido";
    public const string DetalleAzar = "detalle.azar";

    // Mensajes de la barra de abajo
    public const string EstadoBienvenida = "estado.bienvenida";
    public const string EstadoGana = "estado.gana";
    public const string EstadoGanaRacha = "estado.gana.racha";
    public const string EstadoPierde = "estado.pierde";
    public const string EstadoPierdeRacha = "estado.pierde.racha";
    public const string EstadoEmpate = "estado.empate";
    public const string EstadoRepite = "estado.repite";
    public const string EstadoRecord = "estado.record";
    public const string EstadoReset = "estado.reset";
    public const string EstadoModoCompu = "estado.modo.compu";
    public const string EstadoModoAzar = "estado.modo.azar";
    public const string EstadoAzar = "estado.azar";
    public const string EstadoNivelFacil = "estado.nivel.facil";
    public const string EstadoNivelNormal = "estado.nivel.normal";
    public const string EstadoNivelDificil = "estado.nivel.dificil";
    public const string EstadoPantallazo = "estado.pantallazo";
}

/// <summary>
/// Banco de frases chilenas. Cada frase es una plantilla con "marcas" entre llaves que se
/// reemplazan según las jugadas de la ronda, así el género y el plural siempre quedan bien:
///
///   {tu} / {Tu}          → "tu piedra" / "Tu piedra"        (tu jugada)
///   {mi} / {Mi}          → "mi tijera" / "Mi tijera"        (jugada de la compu)
///   {laTuya} / {LaTuya}  → "la piedra" / "el papel"         (tu jugada con artículo)
///   {laMia} / {LaMia}    → "la tijera" / "el papel"         (jugada de la compu con artículo)
///   {jugadaTu}           → "piedra"                         (tu jugada, sola)
///   {jugadaMia}          → "tijera"                         (jugada de la compu, sola)
///   {plural}             → "piedras" / "papeles" / "tijeras"
///   {esa}                → "esa piedra" / "ese papel"
///   {n}                  → un número (racha, repeticiones, etc.)
///
/// Las frases evitan adjetivos con género sobre quien juega ("cómodo", "cachado"), para que
/// funcionen igual si juega un hombre o una mujer.
/// </summary>
public sealed class FrasesChilenas
{
    private readonly Random azar;
    private readonly Dictionary<string, string> ultimaPorCategoria = new Dictionary<string, string>();

    public FrasesChilenas(Random azar)
    {
        this.azar = azar;
    }

    /// <summary>Todas las plantillas (se deja público para poder revisarlas en pruebas).</summary>
    public static IReadOnlyDictionary<string, string[]> Plantillas
    {
        get { return Banco; }
    }

    /// <summary>
    /// Elige una frase de la categoría, sin repetir la misma dos veces seguidas,
    /// y reemplaza las marcas con las jugadas indicadas.
    /// </summary>
    public string Elegir(string categoria, Jugada? tuya = null, Jugada? compu = null, int n = 0)
    {
        if (!Banco.TryGetValue(categoria, out string[]? opciones) || opciones.Length == 0)
            return string.Empty;

        string plantilla = opciones[0];

        if (opciones.Length > 1)
        {
            ultimaPorCategoria.TryGetValue(categoria, out string? anterior);
            string[] candidatas = anterior == null
                ? opciones
                : opciones.Where(o => o != anterior).ToArray();

            if (candidatas.Length == 0) candidatas = opciones;

            plantilla = candidatas[azar.Next(candidatas.Length)];
            ultimaPorCategoria[categoria] = plantilla;
        }

        return Armar(plantilla, tuya, compu, n);
    }

    /// <summary>Frase especial cuando se llega a cierta cantidad de rondas (null si no toca ninguna).</summary>
    public string? Hito(int rondas)
    {
        return Hitos.TryGetValue(rondas, out string? frase) ? frase : null;
    }

    public void Reiniciar()
    {
        ultimaPorCategoria.Clear();
    }

    private static string Armar(string plantilla, Jugada? tuya, Jugada? compu, int n)
    {
        string r = plantilla;

        if (tuya is Jugada t)
        {
            r = r.Replace("{Tu}", "Tu " + t.Nombre())
                 .Replace("{tu}", "tu " + t.Nombre())
                 .Replace("{LaTuya}", Mayuscula(t.ConArticulo()))
                 .Replace("{laTuya}", t.ConArticulo())
                 .Replace("{jugadaTu}", t.Nombre())
                 .Replace("{plural}", t.Plural())
                 .Replace("{esa}", t.Esa());
        }

        if (compu is Jugada c)
        {
            r = r.Replace("{Mi}", "Mi " + c.Nombre())
                 .Replace("{mi}", "mi " + c.Nombre())
                 .Replace("{LaMia}", Mayuscula(c.ConArticulo()))
                 .Replace("{laMia}", c.ConArticulo())
                 .Replace("{jugadaMia}", c.Nombre());
        }

        return r.Replace("{n}", n.ToString());
    }

    private static string Mayuscula(string texto)
    {
        return char.ToUpper(texto[0]) + texto.Substring(1);
    }

    private static readonly Dictionary<int, string> Hitos = new Dictionary<int, string>
    {
        [10] = "¡Ya van 10 rondas! Esto se está poniendo serio, ¿cachai?",
        [25] = "25 rondas... ¿no teníai tareas pendientes, po?",
        [50] = "50 rondas. Ya somos casi amigos.",
        [100] = "100 rondas. Oficialmente, leyenda del cachipún."
    };

    private static readonly Dictionary<string, string[]> Banco = new Dictionary<string, string[]>
    {
        // ------------------------------------------------------------------ TÍTULOS
        [Categoria.TituloGana] = new[]
        {
            "¡Ganaste!",
            "¡Punto pa' ti!",
            "¡Esa estuvo filete!",
            "¡Bacán, ganaste!",
            "¡Te la mandaste!",
            "¡Buena, esa fue tuya!"
        },
        [Categoria.TituloPierde] = new[]
        {
            "¡Perdiste!",
            "¡Ahí te pillé!",
            "Esta fue pa' la compu",
            "¡Chuta, perdiste!",
            "¡Punto pa' mí!",
            "Se la llevó la compu"
        },
        [Categoria.TituloEmpate] = new[]
        {
            "¡Empate!",
            "¡Quedamos a mano!",
            "¡Pensamos lo mismo!",
            "¡Empate, po!",
            "¡Parejito!"
        },
        [Categoria.TituloAzar] = new[]
        {
            "¡Te tocó {laTuya}!",
            "¡Sale {laTuya}!",
            "¡Va {laTuya}!",
            "¡Tírate {laTuya}!"
        },

        // ------------------------------------------------------------------ DETALLE: GANAS
        [Categoria.DetalleGana] = new[]
        {
            "{Tu} le ganó a {mi}. Bien ahí, no hay na que decir.",
            "Pucha, me pillaste: {tu} pudo con {mi}.",
            "Esa te salió buena: {laTuya} le gana a {laMia}. Y a mí me dolió un poquito, ¿cachai?",
            "Ya, punto pa' ti. {Tu} se impuso sobre {mi} sin drama.",
            "Chuta, justo tiré {jugadaMia}. Se nota que cachaste por dónde iba la cosa.",
            "Bacán: {tu} dejó a {mi} fuera de combate. Voy a tener que ponerme las pilas.",
            "No me la esperaba: {tu} le pasó por encima a {mi}."
        },
        [Categoria.DetalleGanaRacha] = new[]
        {
            "¡Van {n} seguidas! {Tu} le ganó a {mi} y ya me tenís sudando.",
            "Estai on fire: {n} al hilo. {Tu} le pasó por encima a {mi}.",
            "Ya po, ¿hasta cuándo? {n} seguidas y {mi} no pudo con {tu}.",
            "{n} victorias al hilo... Me tinca que hoy no es mi día.",
            "Esto ya es sospechoso: {n} seguidas. ¿Me estai cachando las jugadas o qué?"
        },
        [Categoria.DetalleGanaVuelta] = new[]
        {
            "¡Ahí está la vuelta! {Tu} le ganó a {mi}. Eso es no achuncharse.",
            "Te desquitaste: {laTuya} le gana a {laMia}.",
            "Así me gusta: caíste y te paraste. {Tu} venció a {mi}.",
            "Revancha cobrada: {tu} le pasó la aplanadora a {mi}."
        },

        // ------------------------------------------------------------------ DETALLE: PIERDES
        [Categoria.DetallePierde] = new[]
        {
            "{Mi} le ganó a {tu}. Pucha, esta fue mía.",
            "Ahí te pillé: tiré {jugadaMia} y tú {jugadaTu}. Mala pata.",
            "{Mi} pudo más que {tu}. Tranqui, que es solo una ronda.",
            "Chuta, no era esa. {Mi} le pasó por encima a {tu}.",
            "¡Qué penca! {Mi} se llevó el punto contra {tu}.",
            "No hay caso: {laMia} le gana a {laTuya}. Tírate otra, po."
        },
        [Categoria.DetallePierdeRacha] = new[]
        {
            "Van {n} pa' mí... Tranqui, cambia el chip y tírate otra cosa.",
            "¿{n} seguidas? Cámbiate de jugada, que ya te caché el estilo.",
            "Esto ya no es suerte, es estrategia, jaja. Ya, ya, no te piquís.",
            "{n} derrotas al hilo. Respira hondo y tírate algo distinto."
        },
        [Categoria.DetallePierdeCortaRacha] = new[]
        {
            "Ahí se cortó la racha: {mi} le ganó a {tu}. Igual venías súper bien.",
            "¡Te corté la racha! {Mi} frenó a {tu}.",
            "Bueno, no se puede ganar siempre: {laMia} le gana a {laTuya}."
        },

        // ------------------------------------------------------------------ DETALLE: EMPATES
        [Categoria.DetalleEmpate] = new[]
        {
            "Los dos tiramos {jugadaTu}. ¿Nos leímos la mente o qué?",
            "Misma jugada: {jugadaTu}. Esto está súper parejo.",
            "Nos copiamos: los dos tiramos {jugadaTu}. Desempate al tiro, po.",
            "{Tu} contra {mi}: exactamente lo mismo. Nadie gana, nadie pierde.",
            "¡Pensamos lo mismo! {LaTuya} por los dos lados.",
            "Empate con {laTuya}. Esto se está poniendo bueno, ¿cachai?"
        },
        [Categoria.DetalleEmpateSeguido] = new[]
        {
            "{n} empates seguidos... Esto ya es telepatía.",
            "Otra vez empate. ¿Estamos conectados o qué?",
            "Ya van {n} empates al hilo. Alguien tiene que cambiar de jugada, po."
        },

        // ------------------------------------------------------------------ DETALLE: MODO AZAR
        [Categoria.DetalleAzar] = new[]
        {
            "Si no te convence, pídeme otra, po.",
            "Mi consejo: tírate {laTuya}. Si sale mal, no me culpís, ¿ya?",
            "El destino habló: {laTuya}. Yo soy solo el mensajero.",
            "Va {laTuya}. ¡Y no vale cambiar después!"
        },

        // ------------------------------------------------------------------ BARRA DE ABAJO
        [Categoria.EstadoBienvenida] = new[]
        {
            "Acomódate nomás, que venimos a pasarlo chancho. ¡Ya, partamos!",
            "Una partidita rápida, ¿ya? Escoge tu jugada y partimos.",
            "Atajos: 1, 2 y 3 para jugar. Ahora, ¡a echarse un cachipún!"
        },
        [Categoria.EstadoGana] = new[]
        {
            "Bacán. A ver si te dura la racha.",
            "Punto pa' ti; ya me dieron ganas de revancha.",
            "Va bien la cosa, sigue así nomás.",
            "Disfrútalo nomás, ¿cachai?",
            "Se ganó el punto, ahora no te durmai."
        },
        [Categoria.EstadoGanaRacha] = new[]
        {
            "Estai imparable. ¿Quién te para?",
            "Ya po, dame una oportunidad, que me estai dejando en vergüenza.",
            "No te agrandís todavía... aunque lo estai haciendo súper.",
            "Racha de {n}. Me tinca que tengo que cambiar la estrategia."
        },
        [Categoria.EstadoPierde] = new[]
        {
            "Tranqui, es una ronda nomás. ¿Revancha?",
            "No pasa na; prueba otra jugada.",
            "Me llevé este punto, pero la partida sigue abierta.",
            "Ya, sacúdete y vamos de nuevo.",
            "Así es el cachipún: hoy tú, mañana yo."
        },
        [Categoria.EstadoPierdeRacha] = new[]
        {
            "Ya, respiremos. Cambia la jugada y veamos qué pasa.",
            "No te piquís; a veces toca remontar. ¿Otra?",
            "Si querís un respiro, bájale el nivel a la compu ahí a la derecha.",
            "Ponte las pilas, que todavía se puede dar vuelta."
        },
        [Categoria.EstadoEmpate] = new[]
        {
            "Ya, otra más; esta ronda no cuenta pa' ninguno.",
            "Estamos leyendo la misma mente, parece.",
            "Ni pa' ti ni pa' mí. La próxima sí que sí.",
            "Seguimos igualados, queda harto juego."
        },
        [Categoria.EstadoRepite] = new[]
        {
            "Van {n} {plural} seguidas... ya te estoy cachando el estilo 👀",
            "Otra vez {laTuya}. ¿Es tu jugada favorita o me estai tendiendo una trampa?",
            "Te gusta {laTuya}, ¿ah? Voy a tener que andar con ojo.",
            "{n} {plural} al hilo. ¿Y si probamos con otra cosa?"
        },
        [Categoria.EstadoRecord] = new[]
        {
            "¡Nueva mejor racha: {n}! Anótalo en la pizarra.",
            "¡Récord! {n} seguidas, tu mejor racha hasta ahora.",
            "¡{n} al hilo! Nunca habías llegado tan lejos."
        },
        [Categoria.EstadoReset] = new[]
        {
            "Partida nueva, po. ¡Ya, aperra sin miedo!",
            "Todo en cero. Borrón y cuenta nueva.",
            "Partimos de cero. Esta vez gano yo... o tú, ¿quién sabe?"
        },
        [Categoria.EstadoModoCompu] = new[]
        {
            "Modo compu: yo juego y tú tirái. ¡A ver quién manda!",
            "Contra la compu otra vez. ¡Ya, aperra!"
        },
        [Categoria.EstadoModoAzar] = new[]
        {
            "Modo azar: yo te digo qué tirar y tú lo tirái nomás.",
            "Modo azar: confía en el destino... o en mí."
        },
        [Categoria.EstadoAzar] = new[]
        {
            "Cuando querís otra, aprieta el botón de nuevo.",
            "Anda con confianza, que el azar no se equivoca... ¿o sí?",
            "Tírala nomás: la suerte ya está echada."
        },
        [Categoria.EstadoNivelFacil] = new[]
        {
            "Nivel fácil: tiro lo primero que se me ocurra. Relax.",
            "Modo fácil: ni yo sé qué voy a tirar."
        },
        [Categoria.EstadoNivelNormal] = new[]
        {
            "Nivel normal: ojo que me voy a fijar en tu última jugada.",
            "Normal: ya empiezo a mirarte de reojo."
        },
        [Categoria.EstadoNivelDificil] = new[]
        {
            "Nivel difícil: ya te estoy cachando los hábitos. Ahora sí que sí.",
            "Difícil: me aprendo tus mañas... y no las suelto."
        },
        [Categoria.EstadoPantallazo] = new[]
        {
            "¡Listoco! Pantallazo guardado en el Escritorio, carpeta PantallazosCachipun.",
            "Pantallazo listo, en el Escritorio. Ahora a presumir con los amigos."
        }
    };
}
