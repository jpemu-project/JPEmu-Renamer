// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
namespace JPEmuRenamer.Services;

// Manual（Español）。書式は HelpContent を参照
internal static class HelpEs
{
    public const string Play = """
# Primeros pasos
JPEmu Renamer organiza las ROM que tú mismo has extraído y te permite jugarlas al momento, con el emulador integrado (núcleo integrado) o con un emulador propio (exe).

## De la configuración al juego
- Al principio no hay sistemas. Usa "+ Añadir sistema" abajo a la izquierda para crear uno con el nombre que quieras (p. ej., Famicom, NES)
- Selecciónalo y configura su carpeta de ROM en "Ajustes del sistema"
- En la pestaña "Jugar", elige un juego
- Pulsa "▶ Iniciar". Si el núcleo aún no está instalado, se te pregunta si quieres descargarlo
- Pulsa `Esc` para volver al lanzador

> Usa ROM extraídas de software que tengas.

# Ajustes del sistema
Elige un sistema a la izquierda y abre "Ajustes del sistema" arriba a la derecha. Añade sistemas con "+ Añadir sistema". Al elegir una plantilla se rellenan las extensiones, el icono, el color y el núcleo integrado.

| Opción | Descripción |
| Nombre del sistema | Nombre que aparece en la lista |
| Icono / color | Icono de la lista de sistemas. Se admiten imágenes propias (PNG/ICO/JPG) |
| Archivo DAT | Lista de juegos usada para renombrar (formato OfflineList). Opcional si solo vas a jugar |
| Carpetas de ROM | Carpetas que contienen las ROM. Sepáralas con `;` |
| Extensiones | Extensiones que se leen (p. ej., `.nes;.zip`) |
| Método de inicio predeterminado | Núcleo integrado o emulador |
| Núcleo integrado | Núcleo usado para este sistema. Consíguelo con "Descargar" / "Actualizar" |
| Emulador / argumentos | Se usan al jugar con un emulador externo |

## Sistemas que necesitan BIOS
Famicom Disk System, Mega-CD, PC Engine CD-ROM², Saturn, Neo Geo y otros necesitan archivos BIOS. Los nombres de archivo aparecen bajo el núcleo integrado en "Ajustes del sistema". Colócalos en "Ajustes de juego" → "Avanzado" → "Carpeta BIOS" (por defecto: `system` en la carpeta de ajustes).

# Iniciar un juego
## Cómo iniciar
- Selecciona un juego y pulsa "▶ Iniciar"
- Haz doble clic en un juego
- Selecciona un juego y pulsa `Intro`

## Cambiar el método de inicio
Haz clic derecho en un juego para elegir "Iniciar con el núcleo integrado" o "Iniciar con el emulador". El método predeterminado se ajusta en "Ajustes del sistema".

## Argumentos del emulador
| Marcador | Se sustituye por |
| `{rom}` | Ruta completa de la ROM |
| `{romdir}` | Carpeta de la ROM |
| `{romname}` | Nombre del archivo ROM sin extensión |

# Controles
## Distribución de teclado predeterminada
Los nombres de los botones siguen la distribución del Super Famicom (A a la derecha, B abajo).

| Control | Tecla |
| Cruceta | `↑` `↓` `←` `→` |
| A / B | `X` / `Z` |
| X / Y | `S` / `A` |
| L / R | `Q` / `W` |
| L2 / R2 | `E` / `R` |
| Select / Start | `Mayús derecha` / `Intro` |

## Mandos
Los mandos conectados se detectan automáticamente y reciben una distribución estándar: Xbox (XInput), mandos USB genéricos (DirectInput), Switch Pro Controller, DualSense y otros. El jugador 1 usa el primer mando conectado y el jugador 2 el segundo. El stick izquierdo también funciona como cruceta. La vibración funciona en los juegos que la admiten.

## Atajos
| Acción | Tecla | Mando |
| Menú | `F1` | Botón Guide |
| Avance rápido (mantener) | `Tab` | |
| Guardar estado | `F2` | |
| Cargar estado | `F4` | |
| Ranura − / + | `F6` / `F7` | |
| Captura de pantalla | `F12` | |
| Pantalla completa sí/no | `F11` o `Alt+Intro` | |
| Pausa | `P` | |
| Reiniciar | `F10` | |
| Salir | `Esc` | |

> Todas las teclas y botones se cambian en "Ajustes de juego". En pantalla completa, `Esc` vuelve primero a la ventana.

# Menú del juego
`F1` (o el botón Guide del mando) abre el menú y pausa el juego. Usa `↑` `↓` para elegir, `Intro` (A) para aceptar y `Esc` (B) para cerrar.

| Opción | Descripción |
| Volver al juego | Cierra el menú |
| Guardar / cargar estado | En la ranura actual |
| Cambiar ranura | Pasa de una ranura a otra |
| Captura de pantalla | Guarda la pantalla del juego en PNG |
| Pantalla completa / ventana | Cambia la visualización |
| Ajustes de juego… | Cambia ajustes mientras juegas; se aplican al momento |
| Reiniciar | Reinicia el juego |
| Salir al lanzador | Termina el juego |

# Partidas guardadas y estados
## Partidas guardadas (batería)
Las partidas que guardas dentro del juego se almacenan automáticamente (cada 10 segundos aprox. y al salir) y se cargan la próxima vez.

## Guardados de estado
Guardan el instante exacto del juego. `F2` guarda, `F4` carga y `F6` / `F7` cambian de ranura (10 ranuras, de 0 a 9, por defecto).

## Ubicaciones
| Tipo | Ubicación predeterminada |
| Partidas guardadas | `saves\<sistema>` en la carpeta de ajustes |
| Guardados de estado | `states\<sistema>` en la carpeta de ajustes |
| Capturas de pantalla | `screenshots\<sistema>` en la carpeta de ajustes |

La carpeta de ajustes es `data` junto al exe, así que puedes llevar toda la carpeta en una memoria USB (`%APPDATA%\JPEmuRenamer` solo si no se puede escribir en la carpeta del exe). Las ubicaciones se cambian en "Ajustes de juego".

# Ajustes de juego
Ábrelos con el botón "Ajustes de juego" de la pestaña Jugar, con clic derecho en un juego, desde el menú del juego o desde los "Ajustes" de la aplicación.

## Todos, sistema y juego
Los ajustes se superponen en el orden "Todos → Sistema → Juego". Elige arriba el nivel que quieres editar. Un nivel superior puede sustituir el valor del inferior. Las opciones sustituidas muestran ●, y ↺ deshace la sustitución. Por ejemplo: ventana para todo y pantalla completa para un solo juego.

## Pestaña Básico
| Categoría | Descripción |
| Mando y teclado | Controles del jugador 1. Haz clic en una opción y pulsa un botón o una tecla (`Esc` cancela, `Supr` borra) |
| Ubicaciones de guardado | Carpetas de partidas, estados y capturas |
| Vídeo | Ventana/pantalla completa, tamaño de ventana (1x/2x/3x/ajustar), relación de aspecto |
| Audio | Volumen, silencio, dispositivo de salida |

## Pestaña Avanzado
| Categoría | Descripción |
| Mando y teclado | Jugadores 1–4, turbo y su velocidad, zona muerta de los sticks, atajos. Se admiten combinaciones de teclas (p. ej., `Alt+Intro`) |
| Ubicaciones de guardado | Subcarpetas (ninguna/por sistema/por juego), guardar junto a la ROM, número de ranuras, carpeta BIOS |
| Vídeo | Resolución y frecuencia en pantalla completa, escala entera, filtrado, shaders (líneas de barrido/CRT), V-Sync |
| Audio | Frecuencia de muestreo, modo de salida (WASAPI compartido/exclusivo), latencia, sincronía de audio |
| Opciones del núcleo | Opciones propias del núcleo (p. ej., paleta de NES, relación de aspecto, overclock). Aparecen tras iniciar una vez un juego con ese núcleo |

"Restablecer esta pestaña" devuelve la pestaña a los valores predeterminados del nivel seleccionado.

# Vista previa automática
En la pestaña "Jugar", selecciona un juego y espera un momento: arriba a la derecha empiezan a reproducirse imágenes del juego. El núcleo integrado se ejecuta sin sonido ni controles y muestra la demo que aparece tras la pantalla de título. El núcleo integrado del sistema debe estar instalado.

| Opción | Predeterminado |
| Inicio | Tras 0,3 segundos en la selección (no al desplazarse rápido) |
| Velocidad | 4x |
| Saltar el inicio | Los primeros 10 segundos (logotipos, etc.) |
| Reiniciar tras | 60 segundos |
| Sonido / controles | Ninguno (las partidas guardadas no se tocan) |

Cambia estos valores, o desactiva la vista previa, en "Ajustes" → "Vista previa automática".

## Capturas
- Durante una vista previa, "Guardar esta pantalla" abajo a la derecha guarda ese instante como captura
- Clic derecho en un juego → "Crear capturas de este sistema…" inicia cada ROM del sistema en segundo plano y guarda la pantalla en un momento fijo (30 segundos por defecto). Se pueden omitir los juegos que ya tienen captura
- Las capturas se muestran antes de que empiece la vista previa y en juegos cuya pantalla no se mueve
- Se guardan como `snapshots\<sistema>\<título>.png` en la carpeta de ajustes

> Iniciar un juego durante una vista previa o la creación de capturas detiene antes la vista previa.

# Descargar y actualizar núcleos
Abre "Ajustes" → "Descargar / actualizar núcleos…".

- La lista muestra para cada núcleo sus sistemas, su tamaño y su estado (no instalado / al día / con actualización)
- Selecciona núcleos con "Núcleos de mis sistemas", "No instalados", "Con actualización", etc., y pulsa "Descargar"
- "Actualizar todo" actualiza todos los núcleos que tienen actualización

> Los núcleos se descargan del sitio oficial de libretro. Un núcleo en uso por un juego no se puede actualizar.

# Solución de problemas
| Problema | Qué revisar |
| No arranca / "no se pudo cargar el juego" | Si el sistema necesita BIOS, revisa la carpeta BIOS. Otro núcleo puede funcionar |
| "OpenGL no está disponible" | El núcleo usa OpenGL (p. ej., N64). Actualiza el controlador gráfico o elige otro núcleo |
| El sonido chirría o va con retraso | Aumenta la latencia o activa la sincronía de audio en "Ajustes de juego" → "Avanzado" → "Audio" |
| Movimiento a tirones | Desactiva el V-Sync o cierra otras aplicaciones. Los núcleos exigentes como N64 necesitan un PC rápido |
| El mando no responde | Los mandos detectados aparecen arriba en "Ajustes de juego". Reconéctalo si no está. El primer mando es el jugador 1 |
| La resolución se queda cambiada | Sal del juego o pulsa `Esc` para volver a la ventana |
| La primera pulsación se ignora | Algunos juegos ignoran los controles unos segundos tras arrancar. Espera un momento |
| No hay vista previa automática | Comprueba que el núcleo integrado del sistema está instalado y que la vista previa automática está activada en "Ajustes". Se pausa cuando la aplicación está en segundo plano |
""";

    public const string Rename = """
# Cómo funciona el renombrado
La aplicación calcula el CRC32 (una huella del contenido) de cada ROM y lo compara con el archivo DAT (una lista de títulos y CRC32). Solo los archivos que coinciden se renombran con el nombre correcto del DAT, se llamen como se llamen ahora.

> Siempre revisas la lista antes de renombrar, y "Deshacer" restaura después una ejecución entera.

# Preparación
## Conseguir un archivo DAT
Usa un DAT en formato OfflineList (XML) para cada sistema. También se admiten DAT antiguos en Shift-JIS.

## Configurar el sistema
Elige el sistema a la izquierda y configura lo siguiente en "Ajustes del sistema".

| Opción | Descripción |
| Archivo DAT | El archivo DAT |
| Carpetas de ROM | Carpetas que contienen las ROM (separadas por `;`) |
| Extensiones | Extensiones que se leen (p. ej., `.nes;.zip`) |
| Tratamiento de la cabecera | Si se incluyen las cabeceras iNES/FDS en el CRC32 (normalmente "Automático") |

La lectura y la comparación empiezan automáticamente. A partir de la segunda vez se saltan los archivos sin cambios, así que es rápido.

# La pestaña Renombrar
Cambia a la pestaña "Renombrar" de arriba para ver la tabla.

| Columna | Descripción |
| ✓ | Si se renombra (solo se pueden marcar las filas "Por renombrar") |
| Estado | Resultado de la comparación (ver abajo) |
| Nombre de archivo actual | El nombre actual |
| → Nombre en el DAT | El nombre tras renombrar |
| CRC32 / tamaño / formato | Información de la ROM |

## Estados
| Estado | Significado | Se renombra |
| Correcto | El CRC32 coincide y el nombre ya es correcto | No hace falta |
| Por renombrar | El CRC32 coincide pero el nombre es distinto | Sí |
| Duplicado | Ya existe un archivo para el mismo título | No |
| No está en el DAT | El CRC32 no está en el DAT | No |
| Falta | Está en el DAT, pero no tienes el archivo | ― |

# Renombrar paso a paso
- En la pestaña "Renombrar", revisa las filas "Por renombrar" (al principio están todas marcadas)
- Desmarca las que no quieras renombrar. La casilla de la cabecera las cambia todas
- A la derecha aparecen "Antes" y "Después" de la fila seleccionada
- Pulsa "Renombrar" abajo y confirma con "Sí"
- Se muestra el número y la lista se actualiza sola

> Solo se modifican los archivos cuyo CRC32 coincide con el DAT. Un archivo se omite si ya existe uno con el nuevo nombre.

## Nombres de ROM dentro de los ZIP
Con "Ajustes" → "Renombrar también la ROM dentro de los ZIP" activado, la ROM de cada ZIP se renombra junto con el ZIP.

## Caracteres que Windows no admite
`\` `/` `:` `*` `?` `"` `<` `>` `|` se sustituyen por sus equivalentes de ancho completo.

# Deshacer
"Deshacer" al pie de la pestaña "Renombrar" restaura entera la última ejecución. Púlsalo otra vez para restaurar la anterior (hasta las últimas 50).

> "Deshacer" restaura la ejecución más reciente sea cual sea el sistema. La confirmación muestra su fecha y el número de archivos.

# Funciones útiles
| Función | Cómo se usa |
| Los que tengo / Todos / Los que faltan | Filtra la lista. "Los que faltan" muestra los juegos que aún no tienes |
| Buscar | Escribe en el cuadro de búsqueda para filtrar por título o nombre de archivo |
| Volver a comparar | Úsalo tras añadir o quitar ROM |
| Ocultar extensiones | "Ajustes" → "Ocultar las extensiones (.zip, etc.) en la lista" |
| Leer subcarpetas | "Ajustes" → "Leer también las subcarpetas" |
| Mostrar solo los nombres del DAT | Para arcade, donde renombrar los archivos impide que funcionen los emuladores. Actívalo en "Ajustes del sistema" para mantener los nombres de archivo y mostrar los nombres del DAT en la lista |

# Solución de problemas
| Problema | Qué revisar |
| Nada coincide | Comprueba que el DAT es del sistema correcto. Cambiar "Tratamiento de la cabecera" a "Incluir cabecera" o "Excluir cabecera" puede ayudar |
| "No se puede leer el archivo DAT" | Asegúrate de que es un DAT OfflineList (XML) |
| "Duplicado" | Tienes dos o más ROM del mismo título. Mueve una a otra carpeta |
| Omitido con "(ya existe)" | Ya existe un archivo con el nuevo nombre. Decide cuál conservar y ordénalos a mano |
| El nombre dentro del ZIP no cambió | Si el nombre del ZIP ya es correcto, el nombre interior se deja igual |
""";
}
