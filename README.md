# Cachipún — Windows Forms (versión 2)

Juego de piedra, papel o tijera (el cachipún de toda la vida) en **C# con Windows Forms** y **.NET 8**, para Windows.
La compu habla en chileno, lleva estadísticas de la sesión y tiene tres niveles de dificultad.

## Qué trae

- **Dos modos de juego**
  - *Jugar contra la compu*: eliges Piedra, Papel o Tijera y ves el resultado de la batalla (TÚ vs LA COMPU).
  - *Tírame una jugada al azar*: la compu sortea una jugada por ti (no cuenta para las estadísticas).
- **Cuenta regresiva «¡Ca… chi… PÚN!»** antes de revelar la jugada (se puede desactivar).
- **Tres niveles de la compu**
  - *Fácil*: tira cualquier cosa.
  - *Normal*: 4 de cada 10 veces le gana a tu última jugada.
  - *Difícil*: aprende qué sueles tirar después de cada jugada y trata de adivinar la siguiente.
  - En todos los niveles la compu decide **antes** de enterarse de tu jugada, así que no hace trampa.
- **Resultado con la regla**: «La piedra aplasta a la tijera», «El papel envuelve a la piedra», «La tijera corta el papel».
- **Estadísticas de la sesión**: victorias, derrotas, empates, racha actual, mejor racha y tasa de éxito.
  Un empate no suma ni corta la racha.
- **Historial** de las últimas 30 jugadas, con colores.
- **Más de 110 frases chilenas** (en 29 categorías) que cambian según lo que pasó: racha de victorias, vuelta después de perder,
  derrotas seguidas, empates seguidos, si repites mucho la misma jugada, nuevo récord, hitos de 10/25/50/100 rondas, etc.
  Nunca repite la misma frase dos veces seguidas y el género y el plural siempre quedan bien («la piedra», «el papel», «dos tijeras»).
- **Pantallazo** de la ventana en PNG, guardado en el Escritorio, carpeta `PantallazosCachipun`.

## Atajos

| Tecla | Qué hace |
|---|---|
| `1` / `2` / `3` | Piedra / Papel / Tijera |
| `Ctrl+S` | Sacar pantallazo |
| `Ctrl+N` | Partir de nuevo |

## Cómo abrirlo

1. Descomprime el ZIP.
2. Abre `Cachipun.csproj` en **Visual Studio 2022** (con la carga de trabajo *Desarrollo de escritorio con .NET*).
3. Aprieta **F5**.

También se puede correr desde una terminal, en la carpeta del proyecto: `dotnet run` (requiere el SDK de .NET 8 en Windows).

## Archivos

| Archivo | Contenido |
|---|---|
| `Program.cs` | Punto de entrada. |
| `Jugada.cs` | Enums (`Jugada`, `Resultado`, `ModoJuego`, `Dificultad`) y reglas del juego, con artículos y plurales correctos. |
| `MotorCachipun.cs` | `Estadisticas` (conteos y rachas) y `MotorCachipun` (la IA de la compu). |
| `FrasesChilenas.cs` | Banco de frases con marcas como `{tu}`, `{laMia}`, `{plural}`. |
| `Controles.cs` | Colores (`Tema`), la tarjeta blanca (`Tarjeta`) y el renglón del historial. |
| `MainForm.cs` | Interfaz y flujo del juego. |

La lógica (jugadas, IA, estadísticas, frases) no depende de Windows Forms, así que se puede probar por separado.

## Cómo agregar frases

Abre `FrasesChilenas.cs`, busca la categoría y agrega una línea. Las marcas disponibles:

| Marca | Resultado de ejemplo |
|---|---|
| `{tu}` / `{Tu}` | tu piedra / Tu piedra |
| `{mi}` / `{Mi}` | mi tijera / Mi tijera (la jugada de la compu) |
| `{laTuya}` / `{LaTuya}` | la piedra / el papel |
| `{laMia}` / `{LaMia}` | la tijera / el papel (compu) |
| `{jugadaTu}` / `{jugadaMia}` | piedra / tijera (sola, sin artículo) |
| `{plural}` | piedras / papeles / tijeras |
| `{esa}` | esa piedra / ese papel |
| `{n}` | un número (racha, repeticiones…) |

## Notas

- Los emojis en Windows Forms se dibujan en blanco y negro con el color del texto (es una limitación de WinForms); por eso van pintados de morado.
- Si la ventana se hace más chica que el tamaño mínimo, aparecen barras de desplazamiento en vez de cortar los botones.
