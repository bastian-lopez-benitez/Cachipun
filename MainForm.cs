using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Cachipun;

/// <summary>
/// Ventana principal. Aquí solo hay interfaz y "pegamento": la lógica del juego vive en
/// MotorCachipun / Estadisticas y las frases en FrasesChilenas.
///
/// La interfaz está armada con TableLayoutPanel y Dock (nada de posiciones fijas en pixeles
/// dentro de las tarjetas), así los botones no se cortan al cambiar el tamaño de la ventana.
/// </summary>
public sealed class MainForm : Form
{
    // ------------------------------------------------------------------ lógica
    private readonly Random azar = new Random();
    private readonly MotorCachipun motor;
    private readonly FrasesChilenas frases;
    private readonly Estadisticas estadisticas = new Estadisticas();

    // ------------------------------------------------------------------ estado de la partida
    private bool jugando;                 // true mientras corre la cuenta regresiva
    private Jugada jugadaPendiente;       // la jugada elegida (o sorteada) que se revela al final de la cuenta
    private int pasoCuenta;
    private Jugada? ultimaTuya;
    private int repeticiones;             // cuántas veces seguidas has tirado la misma jugada
    private int tiradasAzar;
    private int pasoDestello;
    private Color colorDestello;

    private const int PasosDestello = 12;
    private static readonly string[] PalabrasCuenta = { "¡Ca…", "chi…", "¡PÚN!" };

    // ------------------------------------------------------------------ controles
    private readonly Label lblCaptionTu = new Label();
    private readonly Label lblCaptionCompu = new Label();
    private readonly Label lblIconoTu = new Label();
    private readonly Label lblIconoCompu = new Label();
    private readonly Label lblNombreTu = new Label();
    private readonly Label lblNombreCompu = new Label();
    private readonly Label lblIndicacion = new Label();

    private readonly Label lblTitulo = new Label();
    private readonly Label lblRegla = new Label();
    private readonly Label lblDetalle = new Label();
    private readonly Panel panelResultado = new Panel();
    private readonly Panel acentoResultado = new Panel();
    private TableLayoutPanel contenidoResultado = null!;

    private readonly Label lblVictorias = new Label();
    private readonly Label lblDerrotas = new Label();
    private readonly Label lblEmpates = new Label();
    private readonly Label lblRacha = new Label();
    private readonly Label lblMejorRacha = new Label();
    private readonly Label lblTasa = new Label();
    private readonly Label lblRonda = new Label();

    private readonly RadioButton rbCompu = new RadioButton();
    private readonly RadioButton rbAzar = new RadioButton();
    private readonly Button btnPiedra = new Button();
    private readonly Button btnPapel = new Button();
    private readonly Button btnTijera = new Button();
    private readonly Button btnAzar = new Button();
    private TableLayoutPanel panelJugadas = null!;
    private readonly Panel panelAzar = new Panel();

    private readonly ComboBox cmbNivel = new ComboBox();
    private readonly CheckBox chkComentar = new CheckBox();
    private readonly CheckBox chkCuenta = new CheckBox();
    private readonly Button btnReiniciar = new Button();
    private readonly ListBox lstHistorial = new ListBox();

    private readonly Label lblEstado = new Label();
    private readonly Button btnPantallazo = new Button();
    private readonly ToolTip pistas = new ToolTip { AutoPopDelay = 5000, InitialDelay = 350, ReshowDelay = 150 };

    // Los dos tamaños del emoji: en la cuenta regresiva el puño "late" (chico, grande, chico...).
    private readonly Font fuenteIcono = new Font("Segoe UI Emoji", 30F);
    private readonly Font fuenteIconoChica = new Font("Segoe UI Emoji", 22F);

    private readonly System.Windows.Forms.Timer timerCuenta = new System.Windows.Forms.Timer { Interval = 380 };
    private readonly System.Windows.Forms.Timer timerDestello = new System.Windows.Forms.Timer { Interval = 30 };

    // ==================================================================== arranque

    public MainForm()
    {
        motor = new MotorCachipun(azar);
        frases = new FrasesChilenas(azar);

        Text = "Cachipún — ¡échate una partida!";
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 10F);
        ClientSize = new Size(1140, 724);
        MinimumSize = new Size(900, 560);
        BackColor = Tema.Fondo;
        ForeColor = Tema.Tinta;
        DoubleBuffered = true;
        KeyPreview = true;

        ConstruirInterfaz();
        MostrarPanelInicial();
        ActualizarEstadisticas();

        timerCuenta.Tick += AlTickCuenta;
        timerDestello.Tick += AlTickDestello;
        KeyDown += AlPresionarTecla;
        FormClosed += (_, _) =>
        {
            timerCuenta.Stop();
            timerDestello.Stop();
            timerCuenta.Dispose();
            timerDestello.Dispose();
            fuenteIcono.Dispose();
            fuenteIconoChica.Dispose();
        };
    }

    // ==================================================================== construcción de la interfaz

    private void ConstruirInterfaz()
    {
        SuspendLayout();

        var raiz = Rejilla(1, 3, Tema.Fondo);
        raiz.Padding = new Padding(20, 10, 20, 6);
        Col(raiz, SizeType.Percent, 100);
        Fila(raiz, SizeType.Absolute, 68);
        Fila(raiz, SizeType.Percent, 100);
        Fila(raiz, SizeType.Absolute, 30);
        raiz.Controls.Add(ConstruirEncabezado(), 0, 0);
        raiz.Controls.Add(ConstruirCuerpo(), 0, 1);
        raiz.Controls.Add(ConstruirPie(), 0, 2);
        Controls.Add(raiz);

        ResumeLayout(false);
        PerformLayout();
    }

    private Control ConstruirEncabezado()
    {
        var cab = Rejilla(3, 1, Tema.Fondo);
        Col(cab, SizeType.Absolute, 66);
        Col(cab, SizeType.Percent, 100);
        Col(cab, SizeType.Absolute, 200);
        Fila(cab, SizeType.Percent, 100);

        var logo = new Label
        {
            Text = "✊",
            Font = new Font("Segoe UI Emoji", 22F),
            ForeColor = Tema.Morado,
            BackColor = Tema.MoradoClaro,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Size = new Size(54, 54),
            Anchor = AnchorStyles.Left,
            Margin = Padding.Empty
        };
        cab.Controls.Add(logo, 0, 0);

        var textos = Rejilla(1, 2, Tema.Fondo);
        Col(textos, SizeType.Percent, 100);
        Fila(textos, SizeType.Absolute, 38);
        Fila(textos, SizeType.Percent, 100);
        textos.Controls.Add(Etiqueta("Cachipún", 21, FontStyle.Bold, Tema.Tinta), 0, 0);
        textos.Controls.Add(
            Etiqueta("Una partidita como en el recreo. ¿O vai a arrancar apenas te gane? 😏",
                     9.5f, FontStyle.Regular, Tema.Suave, ContentAlignment.TopLeft), 0, 1);
        cab.Controls.Add(textos, 1, 0);

        var chip = new Label
        {
            Text = "●  ¡YA, PARTAMOS!",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Tema.Verde,
            BackColor = Color.FromArgb(231, 247, 238),
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Size = new Size(180, 32),
            Anchor = AnchorStyles.Right,
            Margin = Padding.Empty
        };
        cab.Controls.Add(chip, 2, 0);
        return cab;
    }

    private Control ConstruirCuerpo()
    {
        // Si la ventana queda más chica que este tamaño mínimo, aparecen barras de desplazamiento
        // en vez de cortar los controles.
        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            AutoScrollMinSize = new Size(900, 610),
            BackColor = Tema.Fondo,
            Margin = Padding.Empty
        };

        var cuerpo = Rejilla(2, 1, Tema.Fondo);
        Col(cuerpo, SizeType.Percent, 68);
        Col(cuerpo, SizeType.Percent, 32);
        Fila(cuerpo, SizeType.Percent, 100);
        cuerpo.Controls.Add(ConstruirColumnaJuego(), 0, 0);
        cuerpo.Controls.Add(ConstruirColumnaLateral(), 1, 0);

        scroll.Controls.Add(cuerpo);
        return scroll;
    }

    // ------------------------------------------------------------------ columna izquierda: juego + estadísticas

    private Control ConstruirColumnaJuego()
    {
        var col = Rejilla(1, 2, Tema.Fondo);
        col.Margin = new Padding(0, 0, 14, 0);
        Col(col, SizeType.Percent, 100);
        Fila(col, SizeType.Percent, 100);
        Fila(col, SizeType.Absolute, 132);
        col.Controls.Add(ConstruirTarjetaJuego(), 0, 0);
        col.Controls.Add(ConstruirTarjetaEstadisticas(), 0, 1);
        return col;
    }

    private Control ConstruirTarjetaJuego()
    {
        var tarjeta = new Tarjeta { Margin = new Padding(0, 0, 0, 12) };

        var f = Rejilla(1, 6, Tema.FondoTarjeta);
        Col(f, SizeType.Percent, 100);
        Fila(f, SizeType.Absolute, 58);      // título
        Fila(f, SizeType.Absolute, 44);      // selector de modo
        Fila(f, SizeType.Percent, 100);      // duelo (se estira)
        Fila(f, SizeType.Absolute, 108);     // resultado
        Fila(f, SizeType.Absolute, 22);      // indicación
        Fila(f, SizeType.Absolute, 92);      // botones

        var titulo = Rejilla(1, 2, Tema.FondoTarjeta);
        Col(titulo, SizeType.Percent, 100);
        Fila(titulo, SizeType.Absolute, 32);
        Fila(titulo, SizeType.Percent, 100);
        titulo.Controls.Add(Etiqueta("¡Ya po, no te achunchís! ¿Qué vai a tirar?", 15, FontStyle.Bold, Tema.Tinta), 0, 0);
        titulo.Controls.Add(
            Etiqueta("Escoge nomás. Después no vale decir que fue suerte 😄",
                     9.5f, FontStyle.Regular, Tema.Suave, ContentAlignment.TopLeft), 0, 1);
        f.Controls.Add(titulo, 0, 0);

        f.Controls.Add(ConstruirSelectorDeModo(), 0, 1);
        f.Controls.Add(ConstruirDuelo(), 0, 2);
        f.Controls.Add(ConstruirResultado(), 0, 3);

        Estilizar(lblIndicacion, 8.5f, FontStyle.Bold, Tema.Suave, ContentAlignment.BottomLeft);
        lblIndicacion.Text = "¿QUÉ TIRAI?";
        f.Controls.Add(lblIndicacion, 0, 4);

        f.Controls.Add(ConstruirJugadas(), 0, 5);

        tarjeta.Controls.Add(f);
        return tarjeta;
    }

    private Control ConstruirSelectorDeModo()
    {
        var modo = Rejilla(2, 1, Tema.FondoTarjeta);
        Col(modo, SizeType.Percent, 50);
        Col(modo, SizeType.Percent, 50);
        Fila(modo, SizeType.Percent, 100);

        PrepararBotonDeModo(rbCompu, "🎮  Jugar contra la compu", new Padding(0, 4, 5, 4));
        PrepararBotonDeModo(rbAzar, "🎲  Tírame una jugada al azar", new Padding(5, 4, 0, 4));
        modo.Controls.Add(rbCompu, 0, 0);
        modo.Controls.Add(rbAzar, 1, 0);

        rbCompu.Checked = true;
        rbCompu.FlatAppearance.BorderColor = Tema.Morado;

        // Con escuchar a rbAzar basta: cada vez que cambia el modo, rbAzar cambia (se marca o se desmarca).
        rbAzar.CheckedChanged += (_, _) => AlCambiarModo();
        return modo;
    }

    private void PrepararBotonDeModo(RadioButton rb, string texto, Padding margen)
    {
        rb.Appearance = Appearance.Button;
        rb.Text = texto;
        rb.Dock = DockStyle.Fill;
        rb.Margin = margen;
        rb.TextAlign = ContentAlignment.MiddleCenter;
        rb.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        rb.ForeColor = Tema.Tinta;
        rb.BackColor = Color.White;
        rb.FlatStyle = FlatStyle.Flat;
        rb.FlatAppearance.BorderColor = Tema.Borde;
        rb.FlatAppearance.CheckedBackColor = Tema.MoradoClaro;
        rb.FlatAppearance.MouseOverBackColor = Tema.MoradoClaro;
        rb.Cursor = Cursors.Hand;
        rb.UseVisualStyleBackColor = false;
        rb.CheckedChanged += (_, _) =>
            rb.FlatAppearance.BorderColor = rb.Checked ? Tema.Morado : Tema.Borde;
    }

    private Control ConstruirDuelo()
    {
        var duelo = Rejilla(3, 1, Tema.FondoDuelo);
        duelo.Margin = new Padding(0, 4, 0, 4);
        Col(duelo, SizeType.Percent, 42);
        Col(duelo, SizeType.Percent, 16);
        Col(duelo, SizeType.Percent, 42);
        Fila(duelo, SizeType.Percent, 100);

        duelo.Controls.Add(LadoDelDuelo(lblCaptionTu, lblIconoTu, lblNombreTu, "TÚ"), 0, 0);
        duelo.Controls.Add(Etiqueta("VS", 13, FontStyle.Bold, Tema.Morado, ContentAlignment.MiddleCenter), 1, 0);
        duelo.Controls.Add(LadoDelDuelo(lblCaptionCompu, lblIconoCompu, lblNombreCompu, "LA COMPU"), 2, 0);
        return duelo;
    }

    private Control LadoDelDuelo(Label caption, Label icono, Label nombre, string textoCaption)
    {
        var lado = Rejilla(1, 3, Tema.FondoDuelo);
        Col(lado, SizeType.Percent, 100);
        Fila(lado, SizeType.Absolute, 24);
        Fila(lado, SizeType.Percent, 100);
        Fila(lado, SizeType.Absolute, 28);

        Estilizar(caption, 8.5f, FontStyle.Bold, Tema.Suave, ContentAlignment.MiddleCenter);
        caption.Text = textoCaption;
        Estilizar(icono, 30f, FontStyle.Regular, Tema.Morado, ContentAlignment.MiddleCenter, "Segoe UI Emoji");
        icono.Font = fuenteIcono;
        Estilizar(nombre, 10f, FontStyle.Bold, Tema.Tinta, ContentAlignment.MiddleCenter);

        lado.Controls.Add(caption, 0, 0);
        lado.Controls.Add(icono, 0, 1);
        lado.Controls.Add(nombre, 0, 2);
        return lado;
    }

    private Control ConstruirResultado()
    {
        panelResultado.Dock = DockStyle.Fill;
        panelResultado.BackColor = Tema.Neutro;
        panelResultado.Margin = new Padding(0, 6, 0, 6);

        contenidoResultado = Rejilla(1, 3, Tema.Neutro);
        Col(contenidoResultado, SizeType.Percent, 100);
        Fila(contenidoResultado, SizeType.Absolute, 36);
        Fila(contenidoResultado, SizeType.Absolute, 22);
        Fila(contenidoResultado, SizeType.Percent, 100);

        Estilizar(lblTitulo, 16f, FontStyle.Bold, Tema.Tinta, ContentAlignment.MiddleCenter);
        Estilizar(lblRegla, 9.5f, FontStyle.Bold, Tema.Morado, ContentAlignment.MiddleCenter);
        Estilizar(lblDetalle, 9.5f, FontStyle.Regular, Tema.Suave, ContentAlignment.TopCenter);
        contenidoResultado.Controls.Add(lblTitulo, 0, 0);
        contenidoResultado.Controls.Add(lblRegla, 0, 1);
        contenidoResultado.Controls.Add(lblDetalle, 0, 2);

        acentoResultado.Dock = DockStyle.Left;
        acentoResultado.Width = 6;
        acentoResultado.BackColor = Tema.Morado;

        // Primero el que llena (Fill) y después el de la orilla (Left), así el orden de "dock" queda bien.
        panelResultado.Controls.Add(contenidoResultado);
        panelResultado.Controls.Add(acentoResultado);
        return panelResultado;
    }

    private Control ConstruirJugadas()
    {
        var contenedor = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Tema.FondoTarjeta,
            Margin = Padding.Empty
        };

        // Modo "contra la compu": los tres botones.
        panelJugadas = Rejilla(3, 1, Tema.FondoTarjeta);
        for (int i = 0; i < 3; i++) Col(panelJugadas, SizeType.Percent, 33.333f);
        Fila(panelJugadas, SizeType.Percent, 100);
        PrepararBotonDeJugada(btnPiedra, Jugada.Piedra);
        PrepararBotonDeJugada(btnPapel, Jugada.Papel);
        PrepararBotonDeJugada(btnTijera, Jugada.Tijera);
        panelJugadas.Controls.Add(btnPiedra, 0, 0);
        panelJugadas.Controls.Add(btnPapel, 1, 0);
        panelJugadas.Controls.Add(btnTijera, 2, 0);

        // Modo "al azar": un solo botón grande.
        panelAzar.Dock = DockStyle.Fill;
        panelAzar.BackColor = Tema.FondoTarjeta;
        panelAzar.Margin = Padding.Empty;
        panelAzar.Visible = false;

        btnAzar.Text = "🎲  ¡Tírame una jugada!";
        btnAzar.Dock = DockStyle.Fill;
        btnAzar.Margin = new Padding(4);
        btnAzar.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        btnAzar.BackColor = Tema.MoradoClaro;
        btnAzar.ForeColor = Tema.Morado;
        btnAzar.FlatStyle = FlatStyle.Flat;
        btnAzar.FlatAppearance.BorderColor = Color.FromArgb(190, 178, 255);
        btnAzar.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 223, 255);
        btnAzar.FlatAppearance.MouseDownBackColor = Color.FromArgb(215, 206, 255);
        btnAzar.Cursor = Cursors.Hand;
        btnAzar.UseVisualStyleBackColor = false;
        btnAzar.Click += (_, _) => PedirJugadaAlAzar();
        pistas.SetToolTip(btnAzar, "La compu sortea una jugada por ti (no cuenta para las estadísticas).");
        panelAzar.Controls.Add(btnAzar);

        contenedor.Controls.Add(panelJugadas);
        contenedor.Controls.Add(panelAzar);
        return contenedor;
    }

    private void PrepararBotonDeJugada(Button b, Jugada jugada)
    {
        b.Dock = DockStyle.Fill;
        b.Margin = new Padding(4);
        b.Text = jugada.Emoji() + "  " + jugada.NombreTitulo() + "\n" + jugada.ReglaCorta();
        b.TextAlign = ContentAlignment.MiddleCenter;
        b.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        b.BackColor = Color.White;
        b.ForeColor = Tema.Tinta;
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderColor = Tema.Borde;
        b.FlatAppearance.MouseOverBackColor = Tema.MoradoClaro;
        b.FlatAppearance.MouseDownBackColor = Color.FromArgb(224, 217, 255);
        b.Cursor = Cursors.Hand;
        b.UseVisualStyleBackColor = false;
        b.Click += (_, _) => Tirar(jugada);

        int atajo = (int)jugada + 1;
        pistas.SetToolTip(b, jugada.NombreTitulo() + " (tecla " + atajo + "): " + jugada.ReglaCorta() + ".");
    }

    private Control ConstruirTarjetaEstadisticas()
    {
        var tarjeta = new Tarjeta();

        var f = Rejilla(1, 2, Tema.FondoTarjeta);
        Col(f, SizeType.Percent, 100);
        Fila(f, SizeType.Absolute, 28);
        Fila(f, SizeType.Percent, 100);

        var cabecera = Rejilla(2, 1, Tema.FondoTarjeta);
        Col(cabecera, SizeType.Percent, 100);
        Col(cabecera, SizeType.Absolute, 130);
        Fila(cabecera, SizeType.Percent, 100);
        cabecera.Controls.Add(Etiqueta("Así vamos, po", 13, FontStyle.Bold, Tema.Tinta), 0, 0);
        Estilizar(lblRonda, 9.5f, FontStyle.Bold, Tema.Morado, ContentAlignment.MiddleRight);
        cabecera.Controls.Add(lblRonda, 1, 0);
        f.Controls.Add(cabecera, 0, 0);

        var fichas = Rejilla(6, 1, Tema.FondoTarjeta);
        for (int i = 0; i < 6; i++) Col(fichas, SizeType.Percent, 100f / 6f);
        Fila(fichas, SizeType.Percent, 100);
        fichas.Controls.Add(Ficha("VICTORIAS", lblVictorias, Tema.Verde, Tema.FondoFicha, false), 0, 0);
        fichas.Controls.Add(Ficha("DERROTAS", lblDerrotas, Tema.Rojo, Tema.FondoFicha, false), 1, 0);
        fichas.Controls.Add(Ficha("EMPATES", lblEmpates, Tema.Dorado, Tema.FondoFicha, false), 2, 0);
        fichas.Controls.Add(Ficha("RACHA ACTUAL", lblRacha, Tema.Dorado, Tema.FondoRacha, false), 3, 0);
        fichas.Controls.Add(Ficha("MEJOR RACHA", lblMejorRacha, Tema.Dorado, Tema.FondoRacha, false), 4, 0);
        fichas.Controls.Add(Ficha("TASA DE ÉXITO", lblTasa, Tema.Morado, Tema.FondoFicha, true), 5, 0);
        f.Controls.Add(fichas, 0, 1);

        tarjeta.Controls.Add(f);
        return tarjeta;
    }

    private Control Ficha(string titulo, Label valor, Color colorValor, Color fondo, bool ultima)
    {
        var ficha = Rejilla(1, 2, fondo);
        ficha.Margin = new Padding(0, 0, ultima ? 0 : 8, 0);
        Col(ficha, SizeType.Percent, 100);
        Fila(ficha, SizeType.Absolute, 24);
        Fila(ficha, SizeType.Percent, 100);

        ficha.Controls.Add(Etiqueta(titulo, 7.5f, FontStyle.Bold, Tema.Suave, ContentAlignment.BottomCenter), 0, 0);
        Estilizar(valor, 20f, FontStyle.Bold, colorValor, ContentAlignment.MiddleCenter);
        valor.Text = "0";
        ficha.Controls.Add(valor, 0, 1);
        return ficha;
    }

    // ------------------------------------------------------------------ columna derecha: ajustes + historial

    private Control ConstruirColumnaLateral()
    {
        var col = Rejilla(1, 2, Tema.Fondo);
        Col(col, SizeType.Percent, 100);
        Fila(col, SizeType.Absolute, 336);
        Fila(col, SizeType.Percent, 100);
        col.Controls.Add(ConstruirTarjetaAjustes(), 0, 0);
        col.Controls.Add(ConstruirTarjetaHistorial(), 0, 1);
        return col;
    }

    private Control ConstruirTarjetaAjustes()
    {
        var tarjeta = new Tarjeta { Margin = new Padding(0, 0, 0, 12) };

        var f = Rejilla(1, 7, Tema.FondoTarjeta);
        Col(f, SizeType.Percent, 100);
        Fila(f, SizeType.Absolute, 30);     // título
        Fila(f, SizeType.Absolute, 22);     // "nivel de la compu"
        Fila(f, SizeType.Absolute, 36);     // combo
        Fila(f, SizeType.Absolute, 30);     // check comentar
        Fila(f, SizeType.Absolute, 30);     // check cuenta regresiva
        Fila(f, SizeType.Absolute, 44);     // botón partir de nuevo
        Fila(f, SizeType.Percent, 100);     // reglas

        f.Controls.Add(Etiqueta("Arma tu partida", 13, FontStyle.Bold, Tema.Tinta), 0, 0);
        f.Controls.Add(Etiqueta("NIVEL DE LA COMPU", 8.5f, FontStyle.Bold, Tema.Suave, ContentAlignment.BottomLeft), 0, 1);

        cmbNivel.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbNivel.Items.AddRange(new object[]
        {
            "Fácil (tira cualquier cosa)",
            "Normal (mira tu última jugada)",
            "Difícil (te tiene cachao)"
        });
        cmbNivel.SelectedIndex = 0;
        cmbNivel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        cmbNivel.Margin = new Padding(0, 2, 0, 2);
        cmbNivel.BackColor = Color.White;
        cmbNivel.ForeColor = Tema.Tinta;
        cmbNivel.SelectedIndexChanged += (_, _) => AlCambiarNivel();
        f.Controls.Add(cmbNivel, 0, 2);

        PrepararCasilla(chkComentar, "Que la compu comente cada jugada");
        PrepararCasilla(chkCuenta, "Cuenta regresiva ¡Ca-chi-pún!");
        f.Controls.Add(chkComentar, 0, 3);
        f.Controls.Add(chkCuenta, 0, 4);

        btnReiniciar.Text = "↻  Partir de nuevo";
        btnReiniciar.Dock = DockStyle.Fill;
        btnReiniciar.Margin = new Padding(0, 6, 0, 2);
        btnReiniciar.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnReiniciar.BackColor = Color.FromArgb(245, 243, 255);
        btnReiniciar.ForeColor = Tema.Morado;
        btnReiniciar.FlatStyle = FlatStyle.Flat;
        btnReiniciar.FlatAppearance.BorderColor = Color.FromArgb(224, 217, 255);
        btnReiniciar.Cursor = Cursors.Hand;
        btnReiniciar.UseVisualStyleBackColor = false;
        btnReiniciar.Click += (_, _) => PartirDeNuevo();
        pistas.SetToolTip(btnReiniciar, "Deja todo en cero (atajo: Ctrl+N).");
        f.Controls.Add(btnReiniciar, 0, 5);

        var reglas = Etiqueta(
            "La piedra aplasta a la tijera.\n" +
            "El papel envuelve a la piedra.\n" +
            "La tijera corta el papel.\n\n" +
            "¿Tiran lo mismo? Empate, po: no suma ni resta, ni te corta la racha.",
            9f, FontStyle.Regular, Tema.Suave, ContentAlignment.TopLeft);
        reglas.Margin = new Padding(0, 8, 0, 0);
        f.Controls.Add(reglas, 0, 6);

        tarjeta.Controls.Add(f);
        return tarjeta;
    }

    private static void PrepararCasilla(CheckBox casilla, string texto)
    {
        casilla.Text = texto;
        casilla.Checked = true;
        casilla.AutoSize = false;
        casilla.Dock = DockStyle.Fill;
        casilla.Margin = Padding.Empty;
        casilla.Font = new Font("Segoe UI", 9.5F);
        casilla.ForeColor = Tema.Tinta;
        casilla.BackColor = Color.Transparent;
    }

    private Control ConstruirTarjetaHistorial()
    {
        var tarjeta = new Tarjeta();

        var f = Rejilla(1, 2, Tema.FondoTarjeta);
        Col(f, SizeType.Percent, 100);
        Fila(f, SizeType.Absolute, 30);
        Fila(f, SizeType.Percent, 100);
        f.Controls.Add(Etiqueta("Las últimas jugadas", 13, FontStyle.Bold, Tema.Tinta), 0, 0);

        lstHistorial.Dock = DockStyle.Fill;
        lstHistorial.Margin = Padding.Empty;
        lstHistorial.BorderStyle = BorderStyle.None;
        lstHistorial.BackColor = Color.FromArgb(250, 250, 253);
        lstHistorial.Font = new Font("Segoe UI", 9F);
        lstHistorial.IntegralHeight = false;
        lstHistorial.SelectionMode = SelectionMode.None;
        lstHistorial.DrawMode = DrawMode.OwnerDrawFixed;
        lstHistorial.ItemHeight = lstHistorial.Font.Height + 8;
        lstHistorial.DrawItem += DibujarRenglonDelHistorial;
        f.Controls.Add(lstHistorial, 0, 1);

        tarjeta.Controls.Add(f);
        return tarjeta;
    }

    private Control ConstruirPie()
    {
        var pie = Rejilla(2, 1, Tema.Fondo);
        Col(pie, SizeType.Percent, 100);
        Col(pie, SizeType.Absolute, 230);
        Fila(pie, SizeType.Percent, 100);

        Estilizar(lblEstado, 9.5f, FontStyle.Regular, Tema.Suave, ContentAlignment.MiddleLeft);
        lblEstado.Text = frases.Elegir(Categoria.EstadoBienvenida);
        pie.Controls.Add(lblEstado, 0, 0);

        btnPantallazo.Text = "📷  Sacar pantallazo (Ctrl+S)";
        btnPantallazo.Dock = DockStyle.Fill;
        btnPantallazo.Margin = new Padding(0, 2, 0, 2);
        btnPantallazo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnPantallazo.BackColor = Color.White;
        btnPantallazo.ForeColor = Tema.Morado;
        btnPantallazo.FlatStyle = FlatStyle.Flat;
        btnPantallazo.FlatAppearance.BorderColor = Tema.Borde;
        btnPantallazo.Cursor = Cursors.Hand;
        btnPantallazo.UseVisualStyleBackColor = false;
        btnPantallazo.Click += (_, _) => SacarPantallazo();
        pie.Controls.Add(btnPantallazo, 1, 0);
        return pie;
    }

    // ------------------------------------------------------------------ ayudas para armar controles

    private static TableLayoutPanel Rejilla(int columnas, int filas, Color fondo)
    {
        return new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = columnas,
            RowCount = filas,
            BackColor = fondo,
            Margin = Padding.Empty
        };
    }

    private static void Col(TableLayoutPanel t, SizeType tipo, float valor)
    {
        t.ColumnStyles.Add(new ColumnStyle(tipo, valor));
    }

    private static void Fila(TableLayoutPanel t, SizeType tipo, float valor)
    {
        t.RowStyles.Add(new RowStyle(tipo, valor));
    }

    private static Label Estilizar(Label l, float tamano, FontStyle estilo, Color color,
                                   ContentAlignment alineacion = ContentAlignment.MiddleLeft,
                                   string fuente = "Segoe UI")
    {
        l.AutoSize = false;
        l.Dock = DockStyle.Fill;
        l.Margin = Padding.Empty;
        l.Font = new Font(fuente, tamano, estilo);
        l.ForeColor = color;
        l.BackColor = Color.Transparent;
        l.TextAlign = alineacion;
        return l;
    }

    private static Label Etiqueta(string texto, float tamano, FontStyle estilo, Color color,
                                  ContentAlignment alineacion = ContentAlignment.MiddleLeft)
    {
        Label l = Estilizar(new Label(), tamano, estilo, color, alineacion);
        l.Text = texto;
        return l;
    }

    // ==================================================================== jugar

    private Dificultad NivelActual()
    {
        int i = cmbNivel.SelectedIndex;
        return i < 0 ? Dificultad.Facil : (Dificultad)i;
    }

    private bool ModoAzar
    {
        get { return rbAzar.Checked; }
    }

    private void Tirar(Jugada tuya)
    {
        if (jugando || ModoAzar) return;

        jugadaPendiente = tuya;
        Bloquear(true);
        ComenzarCuentaORevelar();
    }

    private void PedirJugadaAlAzar()
    {
        if (jugando || !ModoAzar) return;

        jugadaPendiente = motor.JugadaAlAzar();
        Bloquear(true);
        ComenzarCuentaORevelar();
    }

    private void ComenzarCuentaORevelar()
    {
        if (chkCuenta.Checked)
        {
            pasoCuenta = 0;
            MostrarPasoDeLaCuenta();
            timerCuenta.Start();
        }
        else
        {
            Revelar();
        }
    }

    private void AlTickCuenta(object? sender, EventArgs e)
    {
        pasoCuenta++;

        if (pasoCuenta >= PalabrasCuenta.Length)
        {
            timerCuenta.Stop();
            Revelar();
            return;
        }

        MostrarPasoDeLaCuenta();
    }

    /// <summary>"¡Ca…" "chi…" "¡PÚN!": los puños suben y bajan (o giran los emojis, en modo azar).</summary>
    private void MostrarPasoDeLaCuenta()
    {
        lblTitulo.Text = PalabrasCuenta[pasoCuenta];
        lblTitulo.ForeColor = Tema.Morado;
        lblRegla.Text = string.Empty;
        lblDetalle.Text = string.Empty;
        acentoResultado.BackColor = Tema.Morado;
        contenidoResultado.BackColor = Tema.Neutro;

        if (ModoAzar)
        {
            lblIconoTu.Text = JugadaExtensiones.Todas[azar.Next(3)].Emoji();
            lblIconoCompu.Text = "🎲";
        }
        else
        {
            lblIconoTu.Text = Jugada.Piedra.Emoji();
            lblIconoCompu.Text = Jugada.Piedra.Emoji();
        }

        lblNombreTu.Text = "…";
        lblNombreCompu.Text = "…";

        // El puño "late": chico en "¡Ca…" y "¡PÚN!", grande en "chi…".
        Font fuente = pasoCuenta % 2 == 0 ? fuenteIconoChica : fuenteIcono;
        lblIconoTu.Font = fuente;
        lblIconoCompu.Font = fuente;
    }

    private void Revelar()
    {
        lblIconoTu.Font = fuenteIcono;
        lblIconoCompu.Font = fuenteIcono;

        if (ModoAzar)
            RevelarJugadaAlAzar();
        else
            RevelarRondaContraLaCompu();

        Bloquear(false);
    }

    private void RevelarRondaContraLaCompu()
    {
        Jugada tuya = jugadaPendiente;
        Resultado? anterior = estadisticas.Anterior;

        // La compu decide ANTES de registrar tu jugada de esta ronda: así no hace trampa.
        Jugada compu = motor.ElegirJugadaCompu(NivelActual());
        motor.RegistrarJugadaDelJugador(tuya);

        Resultado resultado = tuya.ContraOtra(compu);
        estadisticas.Registrar(resultado);

        repeticiones = ultimaTuya == tuya ? repeticiones + 1 : 1;
        ultimaTuya = tuya;

        lblIconoTu.Text = tuya.Emoji();
        lblIconoCompu.Text = compu.Emoji();
        lblNombreTu.Text = tuya.NombreTitulo();
        lblNombreCompu.Text = compu.NombreTitulo();

        Color color = Tema.Suave;
        string titulo = string.Empty;
        string regla = string.Empty;
        string detalle = string.Empty;
        string estado = string.Empty;
        int racha = estadisticas.RachaActual;

        switch (resultado)
        {
            case Resultado.Ganaste:
                color = Tema.Verde;
                titulo = frases.Elegir(Categoria.TituloGana);
                regla = tuya.Regla();
                if (racha >= 3)
                {
                    detalle = frases.Elegir(Categoria.DetalleGanaRacha, tuya, compu, racha);
                    estado = frases.Elegir(Categoria.EstadoGanaRacha, tuya, compu, racha);
                }
                else
                {
                    detalle = anterior == Resultado.Perdiste
                        ? frases.Elegir(Categoria.DetalleGanaVuelta, tuya, compu)
                        : frases.Elegir(Categoria.DetalleGana, tuya, compu);
                    estado = frases.Elegir(Categoria.EstadoGana);
                }
                break;

            case Resultado.Perdiste:
                color = Tema.Rojo;
                titulo = frases.Elegir(Categoria.TituloPierde);
                regla = compu.Regla();
                if (estadisticas.RachaDerrotas >= 3)
                {
                    detalle = frases.Elegir(Categoria.DetallePierdeRacha, tuya, compu, estadisticas.RachaDerrotas);
                    estado = frases.Elegir(Categoria.EstadoPierdeRacha);
                }
                else
                {
                    detalle = anterior == Resultado.Ganaste
                        ? frases.Elegir(Categoria.DetallePierdeCortaRacha, tuya, compu)
                        : frases.Elegir(Categoria.DetallePierde, tuya, compu);
                    estado = frases.Elegir(Categoria.EstadoPierde);
                }
                break;

            default:
                color = Tema.Dorado;
                titulo = frases.Elegir(Categoria.TituloEmpate);
                regla = "Los dos tiramos " + tuya.Nombre();
                detalle = estadisticas.EmpatesSeguidos >= 2
                    ? frases.Elegir(Categoria.DetalleEmpateSeguido, tuya, compu, estadisticas.EmpatesSeguidos)
                    : frases.Elegir(Categoria.DetalleEmpate, tuya, compu);
                estado = frases.Elegir(Categoria.EstadoEmpate);
                break;
        }

        // Mensajes especiales para la barra de abajo: récord > hito de rondas > "repites mucho la misma".
        if (estadisticas.BatioRecord)
        {
            estado = frases.Elegir(Categoria.EstadoRecord, n: estadisticas.MejorRacha);
        }
        else
        {
            string? hito = frases.Hito(estadisticas.Rondas);
            if (hito != null)
                estado = hito;
            else if (repeticiones >= 2 && azar.NextDouble() < 0.40)
                estado = frases.Elegir(Categoria.EstadoRepite, tuya, compu, repeticiones);
        }

        MostrarResultado(titulo, regla, detalle, color);
        SetEstado(estado, color);
        AgregarAlHistorial(
            estadisticas.Rondas.ToString("00") + "  " + tuya.NombreTitulo() + " vs " + compu.NombreTitulo() + "  ·  " + resultado,
            color);
        ActualizarEstadisticas();
    }

    private void RevelarJugadaAlAzar()
    {
        Jugada sugerida = jugadaPendiente;
        tiradasAzar++;

        lblIconoTu.Text = sugerida.Emoji();
        lblNombreTu.Text = sugerida.NombreTitulo();
        lblIconoCompu.Text = "🎲";
        lblNombreCompu.Text = "Sin rival, solo suerte";

        MostrarResultado(
            frases.Elegir(Categoria.TituloAzar, sugerida),
            "Jugada al azar n.º " + tiradasAzar,
            frases.Elegir(Categoria.DetalleAzar, sugerida),
            Tema.Morado);

        SetEstado(frases.Elegir(Categoria.EstadoAzar), Tema.Suave);
        AgregarAlHistorial("Az  " + sugerida.NombreTitulo() + "  ·  al azar", Tema.Suave);
    }

    private void MostrarResultado(string titulo, string regla, string detalle, Color color)
    {
        lblTitulo.Text = titulo;
        lblTitulo.ForeColor = color;
        lblRegla.Text = regla;
        lblDetalle.Text = chkComentar.Checked ? detalle : string.Empty;
        acentoResultado.BackColor = color;
        IniciarDestello(color);
    }

    private void Bloquear(bool bloquear)
    {
        jugando = bloquear;
        btnPiedra.Enabled = !bloquear;
        btnPapel.Enabled = !bloquear;
        btnTijera.Enabled = !bloquear;
        btnAzar.Enabled = !bloquear;
        rbCompu.Enabled = !bloquear;
        rbAzar.Enabled = !bloquear;
        btnReiniciar.Enabled = !bloquear;
    }

    // ==================================================================== animación del resultado

    /// <summary>El panel del resultado parte con un tinte del color del resultado y se va aclarando.</summary>
    private void IniciarDestello(Color color)
    {
        colorDestello = Tema.Mezclar(color, Color.White, 0.80);
        pasoDestello = 0;
        contenidoResultado.BackColor = colorDestello;
        timerDestello.Stop();
        timerDestello.Start();
    }

    private void AlTickDestello(object? sender, EventArgs e)
    {
        pasoDestello++;

        if (pasoDestello >= PasosDestello)
        {
            timerDestello.Stop();
            contenidoResultado.BackColor = Tema.Neutro;
            return;
        }

        contenidoResultado.BackColor = Tema.Mezclar(colorDestello, Tema.Neutro, pasoDestello / (double)PasosDestello);
    }

    // ==================================================================== estadísticas, historial y mensajes

    private void ActualizarEstadisticas()
    {
        lblVictorias.Text = estadisticas.Victorias.ToString();
        lblDerrotas.Text = estadisticas.Derrotas.ToString();
        lblEmpates.Text = estadisticas.Empates.ToString();
        lblRacha.Text = estadisticas.RachaActual.ToString();
        lblMejorRacha.Text = estadisticas.MejorRacha.ToString();
        lblTasa.Text = estadisticas.TasaExito.ToString("0.0") + "%";
        lblRonda.Text = "Ronda " + estadisticas.Rondas;
    }

    private void AgregarAlHistorial(string texto, Color color)
    {
        lstHistorial.Items.Insert(0, new RegistroHistorial(texto, color));

        while (lstHistorial.Items.Count > 30)
            lstHistorial.Items.RemoveAt(lstHistorial.Items.Count - 1);
    }

    private void DibujarRenglonDelHistorial(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        e.DrawBackground();

        if (lstHistorial.Items[e.Index] is RegistroHistorial registro)
        {
            TextRenderer.DrawText(
                e.Graphics,
                registro.Texto,
                lstHistorial.Font,
                e.Bounds,
                registro.Color,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    private void SetEstado(string texto, Color color)
    {
        lblEstado.Text = texto;
        lblEstado.ForeColor = color;
    }

    private void MostrarPanelInicial()
    {
        bool azarActivo = ModoAzar;

        lblIconoTu.Text = "❔";
        lblNombreTu.Text = azarActivo ? "Tu jugada del día" : "Aún no tirai nada";
        lblIconoCompu.Text = azarActivo ? "🎲" : "🤖";
        lblNombreCompu.Text = azarActivo ? "Sin rival, solo suerte" : "Esperando tu jugada";
        lblCaptionCompu.Text = azarActivo ? "EL AZAR" : "LA COMPU";
        lblIndicacion.Text = azarActivo ? "¿NO SABÍ QUÉ TIRAR?" : "¿QUÉ TIRAI?";

        lblTitulo.Text = "¡Ya, a jugar!";
        lblTitulo.ForeColor = Tema.Tinta;
        lblRegla.Text = azarActivo ? "Aprieta el botón y la compu sortea por ti" : "Piedra, papel o tijera… ¡cachipún!";
        lblDetalle.Text = azarActivo
            ? "No cuenta para las estadísticas: es solo para sacarte de un apuro."
            : "Escoge una jugada abajo (o aprieta 1, 2 o 3) y vemos quién manda.";
        acentoResultado.BackColor = Tema.Morado;
        contenidoResultado.BackColor = Tema.Neutro;
    }

    // ==================================================================== ajustes, atajos y pantallazo

    private void AlCambiarModo()
    {
        if (jugando) return;

        panelJugadas.Visible = !ModoAzar;
        panelAzar.Visible = ModoAzar;
        cmbNivel.Enabled = !ModoAzar;

        MostrarPanelInicial();
        SetEstado(frases.Elegir(ModoAzar ? Categoria.EstadoModoAzar : Categoria.EstadoModoCompu), Tema.Suave);
    }

    private void AlCambiarNivel()
    {
        string categoria;
        switch (NivelActual())
        {
            case Dificultad.Facil: categoria = Categoria.EstadoNivelFacil; break;
            case Dificultad.Normal: categoria = Categoria.EstadoNivelNormal; break;
            default: categoria = Categoria.EstadoNivelDificil; break;
        }

        SetEstado(frases.Elegir(categoria), Tema.Suave);
    }

    private void PartirDeNuevo()
    {
        if (jugando) return;

        estadisticas.Reiniciar();
        motor.Reiniciar();
        frases.Reiniciar();
        ultimaTuya = null;
        repeticiones = 0;
        tiradasAzar = 0;

        timerDestello.Stop();
        lstHistorial.Items.Clear();
        MostrarPanelInicial();
        ActualizarEstadisticas();
        SetEstado(frases.Elegir(Categoria.EstadoReset), Tema.Suave);
    }

    private void AlPresionarTecla(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.S)
        {
            SacarPantallazo();
            e.SuppressKeyPress = true;
            return;
        }

        if (e.Control && e.KeyCode == Keys.N)
        {
            PartirDeNuevo();
            e.SuppressKeyPress = true;
            return;
        }

        if (e.Control || e.Alt || ModoAzar) return;

        if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1) Tirar(Jugada.Piedra);
        else if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2) Tirar(Jugada.Papel);
        else if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3) Tirar(Jugada.Tijera);
        else return;

        e.SuppressKeyPress = true;
    }

    /// <summary>
    /// Guarda en PNG lo que se ve en la ventana. Se usa CopyFromScreen (y no DrawToBitmap) para que
    /// salga igual que en pantalla, sin la barra de título cortando el borde de abajo.
    /// </summary>
    private void SacarPantallazo()
    {
        try
        {
            string carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "PantallazosCachipun");
            Directory.CreateDirectory(carpeta);

            string archivo = Path.Combine(carpeta, "cachipun_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
            Point origen = PointToScreen(Point.Empty);

            using (var imagen = new Bitmap(ClientSize.Width, ClientSize.Height))
            {
                using (var g = Graphics.FromImage(imagen))
                {
                    g.CopyFromScreen(origen, Point.Empty, ClientSize);
                }

                imagen.Save(archivo, System.Drawing.Imaging.ImageFormat.Png);
            }

            SetEstado(frases.Elegir(Categoria.EstadoPantallazo), Tema.Verde);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Pucha, no pude guardar el pantallazo: " + ex.Message,
                "Pantallazo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
