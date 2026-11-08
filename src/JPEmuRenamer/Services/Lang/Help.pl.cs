// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
namespace JPEmuRenamer.Services;

// Instrukcja（Polski）。書式は HelpContent を参照
internal static class HelpPl
{
    public const string Play = """
# Pierwsze kroki
JPEmu Renamer porządkuje ROM-y, które samodzielnie zgrałeś, i pozwala od razu w nie grać – we wbudowanym emulatorze (wbudowany rdzeń) albo we własnym emulatorze (exe).

## Od konfiguracji do gry
- Na początku nie ma żadnych systemów. Przyciskiem „+ Dodaj system” w lewym dolnym rogu utwórz system o dowolnej nazwie (np. Famicom, NES, Pegasus)
- Zaznacz go i ustaw jego folder ROM w „Ustawieniach systemu”
- Na karcie „Graj” wybierz grę
- Naciśnij „▶ Uruchom”. Jeśli rdzeń nie jest jeszcze zainstalowany, pojawi się pytanie o pobranie
- Klawisz `Esc` wraca do launchera

> Używaj ROM-ów zgranych z oprogramowania, które posiadasz.

# Ustawienia systemu
Wybierz system po lewej i otwórz „Ustawienia systemu” w prawym górnym rogu. Kolejne systemy dodasz przyciskiem „+ Dodaj system”. Wybranie szablonu uzupełnia rozszerzenia, ikonę, kolor i wbudowany rdzeń.

| Pozycja | Opis |
| Nazwa systemu | Nazwa na liście |
| Ikona / kolor | Ikona na liście systemów. Można użyć własnych obrazów (PNG/ICO/JPG) |
| Plik DAT | Lista gier do zmiany nazw (format OfflineList). Niepotrzebna, jeśli tylko grasz |
| Foldery ROM | Foldery z ROM-ami. Kilka oddziel `;` |
| Rozszerzenia | Odczytywane rozszerzenia (np. `.nes;.zip`) |
| Domyślny sposób uruchamiania | Wbudowany rdzeń lub emulator |
| Wbudowany rdzeń | Rdzeń dla tego systemu. Pobierz go przyciskiem „Pobierz” / „Aktualizuj” |
| Emulator / argumenty | Używane przy grze w zewnętrznym emulatorze |

## Systemy wymagające BIOS-u
Famicom Disk System, Mega-CD, PC Engine CD-ROM², Saturn, Neo Geo i inne wymagają plików BIOS. Nazwy plików są podane pod wbudowanym rdzeniem w „Ustawieniach systemu”. Umieść je w „Ustawienia gry” → „Zaawansowane” → „Folder BIOS” (domyślnie `system` w folderze ustawień).

# Uruchamianie gier
## Jak uruchomić
- Wybierz grę i naciśnij „▶ Uruchom”
- Kliknij grę dwukrotnie
- Wybierz grę i naciśnij `Enter`

## Zmiana sposobu uruchamiania
Kliknij grę prawym przyciskiem i wybierz „Uruchom we wbudowanym rdzeniu” albo „Uruchom w emulatorze (exe)”. Domyślny sposób ustawisz w „Ustawieniach systemu”.

## Argumenty emulatora
| Symbol | Zastępowany przez |
| `{rom}` | Pełna ścieżka ROM-u |
| `{romdir}` | Folder ROM-u |
| `{romname}` | Nazwa pliku ROM bez rozszerzenia |

# Sterowanie
## Domyślny układ klawiatury
Nazwy przycisków odpowiadają układowi Super Famicom (A po prawej, B na dole).

| Sterowanie | Klawisz |
| Krzyżak | `↑` `↓` `←` `→` |
| A / B | `X` / `Z` |
| X / Y | `S` / `A` |
| L / R | `Q` / `W` |
| L2 / R2 | `E` / `R` |
| Select / Start | `Prawy Shift` / `Enter` |

## Kontrolery
Podłączone kontrolery są wykrywane automatycznie i dostają standardowy układ: Xbox (XInput), zwykłe pady USB (DirectInput), Switch Pro Controller, DualSense i inne. Gracz 1 używa pierwszego podłączonego kontrolera, gracz 2 drugiego. Lewa gałka działa też jak krzyżak. Wibracje działają w grach, które je obsługują.

## Skróty
| Czynność | Klawisz | Kontroler |
| Menu | `F1` | Przycisk Guide |
| Przewijanie (przytrzymaj) | `Tab` | |
| Zapisz stan | `F2` | |
| Wczytaj stan | `F4` | |
| Slot − / + | `F6` / `F7` | |
| Zrzut ekranu | `F12` | |
| Pełny ekran wł./wył. | `F11` lub `Alt+Enter` | |
| Pauza | `P` | |
| Reset | `F10` | |
| Wyjście | `Esc` | |

> Wszystkie klawisze i przyciski zmienisz w „Ustawieniach gry”. W trybie pełnoekranowym `Esc` najpierw wraca do okna.

# Menu w grze
`F1` (lub przycisk Guide) otwiera menu i wstrzymuje grę. `↑` `↓` – wybór, `Enter` (A) – zatwierdzenie, `Esc` (B) – zamknięcie.

| Pozycja | Opis |
| Wróć do gry | Zamyka menu |
| Zapisz / wczytaj stan | W bieżącym slocie |
| Zmień slot | Przełącza sloty |
| Zrzut ekranu | Zapisuje obraz gry jako PNG |
| Pełny ekran / okno | Przełącza tryb wyświetlania |
| Ustawienia gry… | Zmiana ustawień podczas gry; działa od razu |
| Reset | Uruchamia grę od nowa |
| Wyjdź do launchera | Kończy grę |

# Zapisy gry i zapisy stanu
## Zapisy gry (bateria)
To, co zapiszesz w samej grze, jest zapisywane automatycznie (mniej więcej co 10 sekund i przy wyjściu) i wczytywane następnym razem.

## Zapisy stanu
Zapisują dokładny moment gry. `F2` zapisuje, `F4` wczytuje, `F6` / `F7` zmieniają slot (domyślnie 10 slotów, 0–9).

## Lokalizacje
| Rodzaj | Domyślna lokalizacja |
| Zapisy gry | `saves\<system>` w folderze ustawień |
| Zapisy stanu | `states\<system>` w folderze ustawień |
| Zrzuty ekranu | `screenshots\<system>` w folderze ustawień |

Folder ustawień to `data` obok pliku exe, więc cały folder można nosić na pendrivie USB (`%APPDATA%\JPEmuRenamer` tylko wtedy, gdy do folderu exe nie da się zapisywać). Lokalizacje zmienisz w „Ustawieniach gry”.

# Ustawienia gry
Otworzysz je przyciskiem „Ustawienia gry” na karcie Graj, prawym kliknięciem na grze, z menu w grze lub z „Ustawień” aplikacji.

## Wszystkie, system i gra
Ustawienia nakładają się w kolejności „Wszystkie → System → Gra”. U góry wybierz poziom do edycji. Wyższy poziom może zastąpić wartość niższego. Zastąpione pozycje mają ●, a ↺ cofa zastąpienie. Przykład: okno dla wszystkiego, pełny ekran tylko dla jednej gry.

## Karta „Podstawowe”
| Kategoria | Opis |
| Kontroler i klawiatura | Sterowanie gracza 1. Kliknij pozycję i naciśnij przycisk lub klawisz (`Esc` anuluje, `Delete` czyści) |
| Miejsca zapisu | Foldery zapisów gry, zapisów stanu i zrzutów ekranu |
| Obraz | Okno/pełny ekran, rozmiar okna (1x/2x/3x/dopasuj), proporcje |
| Dźwięk | Głośność, wyciszenie, urządzenie wyjściowe |

## Karta „Zaawansowane”
| Kategoria | Opis |
| Kontroler i klawiatura | Gracze 1–4, turbo i jego szybkość, martwa strefa gałek, skróty. Obsługiwane są kombinacje klawiszy (np. `Alt+Enter`) |
| Miejsca zapisu | Podfoldery (brak/dla systemu/dla gry), zapisy obok ROM-u, liczba slotów, folder BIOS |
| Obraz | Rozdzielczość i odświeżanie pełnego ekranu, skalowanie całkowite, filtrowanie, shadery (linie skanowania/CRT), V-Sync |
| Dźwięk | Częstotliwość próbkowania, tryb wyjścia (WASAPI współdzielony/wyłączny), opóźnienie, korekta synchronizacji |
| Opcje rdzenia | Własne opcje rdzenia (np. paleta NES, proporcje, podkręcanie). Pojawiają się po jednorazowym uruchomieniu gry w tym rdzeniu |

„Przywróć tę kartę” przywraca domyślne wartości karty na wybranym poziomie.

# Automatyczny podgląd
Na karcie „Graj” wybierz grę i chwilę poczekaj – w prawym górnym rogu zacznie się odtwarzać nagranie z gry. Wbudowany rdzeń działa bez dźwięku i sterowania i pokazuje demo po ekranie tytułowym. Wbudowany rdzeń systemu musi być zainstalowany.

| Pozycja | Domyślnie |
| Start | 0,3 s po wybraniu (nie podczas szybkiego przewijania) |
| Prędkość | 4x |
| Pominięcie początku | Pierwsze 10 sekund (logo itp.) |
| Zacznij od nowa po | 60 sekundach |
| Dźwięk / sterowanie | Brak (zapisy gry nie są zmieniane) |

Te wartości zmienisz – albo wyłączysz podgląd – w „Ustawienia” → „Automatyczny podgląd”.

## Zrzuty
- Podczas podglądu przycisk „Zapisz ten ekran” w prawym dolnym rogu zapisuje ten moment jako zrzut
- Prawy klik na grze → „Utwórz zrzuty dla tego systemu…” uruchamia w tle każdy ROM systemu i zapisuje ekran w ustalonym momencie (domyślnie po 30 sekundach). Gry, które mają już zrzut, można pominąć
- Zrzuty są pokazywane przed startem podglądu i w grach, w których obraz się nie zmienia
- Są zapisywane jako `snapshots\<system>\<tytuł>.png` w folderze ustawień

> Uruchomienie gry podczas podglądu lub tworzenia zrzutów najpierw zatrzymuje podgląd.

# Pobieranie i aktualizacja rdzeni
Otwórz „Ustawienia” → „Pobierz / aktualizuj wbudowane rdzenie”.

- Lista pokazuje dla każdego rdzenia systemy, rozmiar i stan (niezainstalowany / aktualny / dostępna aktualizacja)
- Zaznacz rdzenie przyciskami „Rdzenie moich systemów”, „Niezainstalowane”, „Z aktualizacją” itp. i naciśnij „Pobierz”
- „Zaktualizuj wszystkie” aktualizuje wszystkie rdzenie z aktualizacją

> Rdzenie są pobierane z oficjalnej strony libretro. Rdzenia używanego przez uruchomioną grę nie można zaktualizować.

# Rozwiązywanie problemów
| Problem | Co sprawdzić |
| Nie uruchamia się / „nie mógł wczytać gry” | Jeśli system wymaga BIOS-u, sprawdź folder BIOS. Może pomóc inny rdzeń |
| „OpenGL jest niedostępny” | Rdzeń używa OpenGL (np. N64). Zaktualizuj sterownik karty graficznej lub wybierz inny rdzeń |
| Dźwięk trzeszczy lub się opóźnia | W „Ustawienia gry” → „Zaawansowane” → „Dźwięk” zwiększ opóźnienie lub włącz korektę synchronizacji |
| Obraz się przycina | Wyłącz V-Sync lub zamknij inne programy. Wymagające rdzenie, jak N64, potrzebują szybkiego komputera |
| Kontroler nie reaguje | Wykryte kontrolery są wymienione u góry „Ustawień gry”. Jeśli go brak, podłącz ponownie. Pierwszy kontroler to gracz 1 |
| Rozdzielczość się nie przywróciła | Wyjdź z gry lub naciśnij `Esc`, aby wrócić do okna |
| Pierwsze naciśnięcie jest ignorowane | Niektóre gry przez kilka sekund po starcie nie przyjmują sterowania. Poczekaj chwilę |
| Brak automatycznego podglądu | Sprawdź, czy wbudowany rdzeń systemu jest zainstalowany i czy podgląd jest włączony w „Ustawieniach”. Wstrzymuje się, gdy aplikacja jest w tle |
""";

    public const string Rename = """
# Jak działa zmiana nazw
Aplikacja oblicza CRC32 (odcisk zawartości) każdego ROM-u i porównuje go z plikiem DAT (listą tytułów i CRC32). Tylko pasujące pliki dostają poprawną nazwę z DAT – niezależnie od tego, jak nazywają się teraz.

> Przed zmianą nazw zawsze sprawdzasz listę, a „Cofnij” przywraca potem całą operację.

# Przygotowanie
## Plik DAT
Dla każdego systemu użyj DAT w formacie OfflineList (XML). Obsługiwane są też stare pliki DAT w Shift-JIS.

## Konfiguracja systemu
Wybierz system po lewej i w „Ustawieniach systemu” ustaw poniższe pozycje.

| Pozycja | Opis |
| Plik DAT | Plik DAT |
| Foldery ROM | Foldery z ROM-ami (oddziel `;`) |
| Rozszerzenia | Odczytywane rozszerzenia (np. `.nes;.zip`) |
| Obsługa nagłówka | Czy nagłówki iNES/FDS wliczać do CRC32 (zwykle „Automatycznie”) |

Odczyt i porównanie zaczynają się automatycznie. Od drugiego razu niezmienione pliki są pomijane, więc idzie szybko.

# Karta „Zmiana nazw”
Przejdź na kartę „Zmiana nazw” u góry, aby zobaczyć tabelę.

| Kolumna | Opis |
| ✓ | Czy zmienić nazwę (zaznaczać można tylko wiersze „Do zmiany”) |
| Stan | Wynik porównania (patrz niżej) |
| Obecna nazwa pliku | Aktualna nazwa |
| → Nazwa w DAT | Nazwa po zmianie |
| CRC32 / rozmiar / format | Informacje o ROM-ie |

## Stany
| Stan | Znaczenie | Zmiana nazwy |
| Zgodny | CRC32 pasuje, nazwa jest już poprawna | Niepotrzebna |
| Do zmiany | CRC32 pasuje, ale nazwa jest inna | Tak |
| Duplikat | Plik dla tego tytułu już istnieje | Nie |
| Brak w DAT | Tego CRC32 nie ma w DAT | Nie |
| Brakuje | Jest w DAT, ale nie masz pliku | ― |

# Zmiana nazw krok po kroku
- Na karcie „Zmiana nazw” sprawdź wiersze „Do zmiany” (na początku wszystkie są zaznaczone)
- Odznacz te, których nazw nie chcesz zmieniać. Pole w nagłówku przełącza wszystkie
- Po prawej widać „Przed” i „Po” dla wybranego wiersza
- Naciśnij „Zmień nazwy” na dole i potwierdź przyciskiem „Tak”
- Pojawi się liczba plików, a lista odświeży się sama

> Zmieniane są tylko pliki, których CRC32 zgadza się z DAT. Plik jest pomijany, jeśli plik o nowej nazwie już istnieje.

## Nazwy ROM-ów w plikach ZIP
Gdy włączone jest „Ustawienia” → „Zmieniaj też nazwę ROM-u wewnątrz ZIP”, ROM w każdym ZIP-ie dostaje nową nazwę razem z ZIP-em.

## Znaki niedozwolone w Windows
`\` `/` `:` `*` `?` `"` `<` `>` `|` są zastępowane odpowiednikami o pełnej szerokości.

# Cofanie
„Cofnij” na dole karty „Zmiana nazw” przywraca w całości ostatnią operację. Naciśnij ponownie, aby przywrócić wcześniejszą (do 50 ostatnich).

> „Cofnij” zawsze dotyczy najnowszej operacji, niezależnie od systemu. Potwierdzenie pokazuje jej datę i liczbę plików.

# Przydatne funkcje
| Funkcja | Jak użyć |
| Posiadane / Wszystkie / Brakujące | Filtruje listę. „Brakujące” pokazuje gry, których jeszcze nie masz |
| Wyszukiwanie | Wpisz tekst w polu wyszukiwania, aby filtrować po tytule lub nazwie pliku |
| Porównaj ponownie | Użyj po dodaniu lub usunięciu ROM-ów |
| Ukrywanie rozszerzeń | „Ustawienia” → „Ukryj rozszerzenia plików (.zip itp.) na liście tytułów” |
| Odczyt podfolderów | „Ustawienia” → „Odczytuj też podfoldery” |
| Tylko nazwy z DAT | Dla automatów, gdzie zmiana nazw plików psuje działanie emulatorów. Włącz w „Ustawieniach systemu”, aby zachować nazwy plików i pokazywać na liście nazwy z DAT |

# Rozwiązywanie problemów
| Problem | Co sprawdzić |
| Nic nie pasuje | Sprawdź, czy DAT jest od właściwego systemu. Może pomóc zmiana „Obsługi nagłówka” na „Z nagłówkiem” lub „Bez nagłówka” |
| „Nie udało się odczytać pliku DAT” | Upewnij się, że to DAT OfflineList (XML) |
| „Duplikat” | Masz dwa lub więcej ROM-ów tego samego tytułu. Przenieś jeden do innego folderu |
| Pominięto z dopiskiem „(istnieje)” | Plik o nowej nazwie już istnieje. Zdecyduj, który zostawić, i uporządkuj ręcznie |
| Nazwa wewnątrz ZIP się nie zmieniła | Jeśli nazwa samego ZIP-a jest już poprawna, nazwa wewnętrzna zostaje bez zmian |
""";
}
