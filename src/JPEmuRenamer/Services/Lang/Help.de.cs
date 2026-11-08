// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
namespace JPEmuRenamer.Services;

// Handbuch（Deutsch）。書式は HelpContent を参照
internal static class HelpDe
{
    public const string Play = """
# Erste Schritte
JPEmu Renamer verwaltet die ROMs, die du selbst ausgelesen hast, und lässt dich sie sofort spielen – mit dem eingebauten Emulator (integrierter Core) oder mit deinem eigenen Emulator (exe).

## Von der Einrichtung zum Spiel
- Anfangs gibt es keine Systeme. Lege mit „+ System hinzufügen“ unten links ein System mit beliebigem Namen an (z. B. Famicom, NES)
- Wähle es aus und stelle in den „Systemeinstellungen“ seinen ROM-Ordner ein
- Wähle auf dem Reiter „Spielen“ ein Spiel
- Drücke „▶ Starten“. Ist der Core noch nicht installiert, wirst du gefragt, ob er heruntergeladen werden soll
- Mit `Esc` kehrst du zum Launcher zurück

> Verwende ROMs, die du aus deiner eigenen Software ausgelesen hast.

# Systemeinstellungen
Wähle links ein System und öffne oben rechts die „Systemeinstellungen“. Mit „+ System hinzufügen“ legst du weitere an. Eine Vorlage füllt Dateiendungen, Symbol, Farbe und integrierten Core aus.

| Eintrag | Beschreibung |
| Systemname | Name in der Liste |
| Symbol / Farbe | Symbol in der Systemliste. Eigene Bilder (PNG/ICO/JPG) sind möglich |
| DAT-Datei | Spieleliste zum Umbenennen (OfflineList-Format). Zum reinen Spielen nicht nötig |
| ROM-Ordner | Ordner mit den ROMs. Mehrere mit `;` trennen |
| Dateiendungen | Zu lesende Endungen (z. B. `.nes;.zip`) |
| Standard-Startart | Integrierter Core oder Emulator |
| Integrierter Core | Core für dieses System. Mit „Herunterladen“ / „Aktualisieren“ holen |
| Emulator / Argumente | Für das Spielen mit einem externen Emulator |

## Systeme, die ein BIOS brauchen
Famicom Disk System, Mega-CD, PC Engine CD-ROM², Saturn, Neo Geo und andere brauchen BIOS-Dateien. Die Dateinamen stehen unter dem integrierten Core in den „Systemeinstellungen“. Lege sie in „Spieleinstellungen“ → „Erweitert“ → „BIOS-Ordner“ ab (Standard: `system` im Einstellungsordner).

# Spiele starten
## So startest du
- Spiel wählen und „▶ Starten“ drücken
- Auf ein Spiel doppelklicken
- Spiel wählen und `Enter` drücken

## Startart wechseln
Klicke mit der rechten Maustaste auf ein Spiel und wähle „Mit integriertem Core starten“ oder „Mit Emulator (exe) starten“. Die Standard-Startart legst du in den „Systemeinstellungen“ fest.

## Argumente für den Emulator
| Platzhalter | Wird ersetzt durch |
| `{rom}` | Vollständiger ROM-Pfad |
| `{romdir}` | Ordner des ROMs |
| `{romname}` | ROM-Dateiname ohne Endung |

# Steuerung
## Standardbelegung der Tastatur
Die Tastennamen folgen der Super-Famicom-Anordnung (A rechts, B unten).

| Bedienung | Taste |
| Steuerkreuz | `↑` `↓` `←` `→` |
| A / B | `X` / `Z` |
| X / Y | `S` / `A` |
| L / R | `Q` / `W` |
| L2 / R2 | `E` / `R` |
| Select / Start | `Rechte Umschalttaste` / `Enter` |

## Controller
Angeschlossene Controller werden automatisch erkannt und erhalten eine Standardbelegung: Xbox (XInput), allgemeine USB-Pads (DirectInput), Switch Pro Controller, DualSense und weitere. Spieler 1 nutzt den zuerst angeschlossenen Controller, Spieler 2 den zweiten. Der linke Stick funktioniert auch als Steuerkreuz. Vibration funktioniert in Spielen, die sie unterstützen.

## Tastenkürzel
| Aktion | Taste | Controller |
| Menü | `F1` | Guide-Taste |
| Vorspulen (halten) | `Tab` | |
| Speicherstand sichern | `F2` | |
| Speicherstand laden | `F4` | |
| Platz − / + | `F6` / `F7` | |
| Screenshot | `F12` | |
| Vollbild ein/aus | `F11` oder `Alt+Enter` | |
| Pause | `P` | |
| Zurücksetzen | `F10` | |
| Beenden | `Esc` | |

> Alle Tasten lassen sich in den „Spieleinstellungen“ ändern. Im Vollbild kehrt `Esc` zuerst zum Fenster zurück.

# Spielmenü
`F1` (oder die Guide-Taste) öffnet das Menü und pausiert das Spiel. Mit `↑` `↓` wählen, mit `Enter` (A) bestätigen, mit `Esc` (B) schließen.

| Eintrag | Beschreibung |
| Weiterspielen | Schließt das Menü |
| Speicherstand sichern / laden | Im aktuellen Platz |
| Platz wechseln | Wechselt den Speicherplatz |
| Screenshot | Speichert das Spielbild als PNG |
| Vollbild / Fenster | Wechselt die Anzeige |
| Spieleinstellungen… | Einstellungen während des Spiels ändern; wirken sofort |
| Zurücksetzen | Startet das Spiel neu |
| Zum Launcher zurück | Beendet das Spiel |

# Spielstände und Speicherstände
## Spielstände (Batterie)
Was du im Spiel speicherst, wird automatisch gesichert (etwa alle 10 Sekunden und beim Beenden) und beim nächsten Mal geladen.

## Speicherstände
Sichern genau den aktuellen Moment. `F2` sichert, `F4` lädt, `F6` / `F7` wechseln den Platz (standardmäßig 10 Plätze, 0–9).

## Speicherorte
| Art | Standardort |
| Spielstände | `saves\<System>` im Einstellungsordner |
| Speicherstände | `states\<System>` im Einstellungsordner |
| Screenshots | `screenshots\<System>` im Einstellungsordner |

Der Einstellungsordner ist `data` neben der exe, sodass Sie den ganzen Ordner auf einem USB-Stick mitnehmen können (`%APPDATA%\JPEmuRenamer` nur, wenn der Ordner der exe nicht beschreibbar ist). Die Orte lassen sich in den „Spieleinstellungen“ ändern.

# Spieleinstellungen
Öffne sie über den Knopf „Spieleinstellungen“ auf dem Reiter Spielen, per Rechtsklick auf ein Spiel, über das Spielmenü oder über die „Einstellungen“ der App.

## Alle, System und Spiel
Einstellungen werden in der Reihenfolge „Alle → System → Spiel“ übereinandergelegt. Wähle oben die Ebene, die du ändern willst. Eine höhere Ebene kann den Wert der niedrigeren ersetzen. Ersetzte Einträge zeigen ●, und ↺ hebt das auf. Beispiel: Fenster für alles, Vollbild nur für ein Spiel.

## Reiter „Grundlagen“
| Bereich | Beschreibung |
| Controller & Tastatur | Steuerung von Spieler 1. Eintrag anklicken, dann eine Taste drücken (`Esc` bricht ab, `Entf` löscht) |
| Speicherorte | Ordner für Spielstände, Speicherstände und Screenshots |
| Video | Fenster/Vollbild, Fenstergröße (1x/2x/3x/anpassen), Seitenverhältnis |
| Audio | Lautstärke, stumm, Ausgabegerät |

## Reiter „Erweitert“
| Bereich | Beschreibung |
| Controller & Tastatur | Spieler 1–4, Turbo und Tempo, Stick-Totzone, Tastenkürzel. Tastenkombinationen (z. B. `Alt+Enter`) sind möglich |
| Speicherorte | Unterordner (keine/pro System/pro Spiel), Spielstände neben dem ROM, Anzahl der Plätze, BIOS-Ordner |
| Video | Vollbild-Auflösung und Bildrate, ganzzahlige Skalierung, Filterung, Shader (Scanlines/CRT), V-Sync |
| Audio | Abtastrate, Ausgabemodus (WASAPI geteilt/exklusiv), Latenz, Audio-Synchronkorrektur |
| Core-Optionen | Eigene Optionen des Cores (z. B. NES-Palette, Seitenverhältnis, Übertaktung). Erscheinen, nachdem einmal ein Spiel mit diesem Core gestartet wurde |

„Diesen Reiter zurücksetzen“ stellt den Reiter für die gewählte Ebene auf die Standardwerte zurück.

# Automatische Vorschau
Wähle auf dem Reiter „Spielen“ ein Spiel und warte kurz – oben rechts beginnen Spielszenen zu laufen. Der integrierte Core läuft ohne Ton und Eingabe und zeigt die Demo nach dem Titelbild. Dafür muss der integrierte Core des Systems installiert sein.

| Eintrag | Standard |
| Start | 0,3 Sekunden nach der Auswahl (nicht beim schnellen Blättern) |
| Tempo | 4x |
| Anfang überspringen | Die ersten 10 Sekunden (Logos usw.) |
| Neu starten nach | 60 Sekunden |
| Ton / Eingabe | Keine (Spielstände werden nicht verändert) |

Diese Werte änderst du – oder schaltest die Vorschau aus – unter „Einstellungen“ → „Automatische Vorschau“.

## Schnappschüsse
- Während einer Vorschau speichert „Bild speichern“ unten rechts diesen Moment als Schnappschuss
- Rechtsklick auf ein Spiel → „Schnappschüsse für dieses System erstellen…“ startet jedes ROM des Systems im Hintergrund und speichert das Bild zu einem festen Zeitpunkt (standardmäßig 30 Sekunden). Spiele mit vorhandenem Schnappschuss lassen sich überspringen
- Schnappschüsse werden gezeigt, bevor die Vorschau startet, und bei Spielen mit stehendem Bild
- Sie werden als `snapshots\<System>\<Titel>.png` im Einstellungsordner gespeichert

> Wenn du während einer Vorschau oder beim Erstellen von Schnappschüssen ein Spiel startest, wird die Vorschau zuerst angehalten.

# Cores herunterladen und aktualisieren
Öffne „Einstellungen“ → „Integrierte Cores laden / aktualisieren“.

- Die Liste zeigt für jeden Core die Systeme, die Größe und den Status (nicht installiert / aktuell / Update verfügbar)
- Wähle Cores über „Cores meiner Systeme“, „Nicht installiert“, „Mit Update“ usw. und drücke „Herunterladen“
- „Alle aktualisieren“ aktualisiert alle Cores mit Update

> Cores werden von der offiziellen libretro-Seite geladen. Ein Core, den ein laufendes Spiel nutzt, kann nicht aktualisiert werden.

# Fehlerbehebung
| Problem | Was prüfen |
| Startet nicht / „konnte das Spiel nicht laden“ | Wenn das System ein BIOS braucht, den BIOS-Ordner prüfen. Ein anderer Core kann helfen |
| „OpenGL ist nicht verfügbar“ | Der Core nutzt OpenGL (z. B. N64). Grafiktreiber aktualisieren oder anderen Core wählen |
| Ton knackt oder hinkt hinterher | In „Spieleinstellungen“ → „Erweitert“ → „Audio“ die Latenz erhöhen oder die Audio-Synchronkorrektur einschalten |
| Ruckeln | V-Sync ausschalten oder andere Apps schließen. Anspruchsvolle Cores wie N64 brauchen einen schnellen PC |
| Controller reagiert nicht | Erkannte Controller stehen oben in den „Spieleinstellungen“. Fehlt er, neu anschließen. Der erste Controller ist Spieler 1 |
| Auflösung bleibt verändert | Spiel beenden oder `Esc` drücken, um zum Fenster zurückzukehren |
| Erster Tastendruck wird ignoriert | Manche Spiele nehmen nach dem Start einige Sekunden lang keine Eingaben an. Kurz warten |
| Keine automatische Vorschau | Prüfen, ob der integrierte Core des Systems installiert und die Vorschau in den „Einstellungen“ eingeschaltet ist. Sie pausiert, solange die App im Hintergrund ist |
""";

    public const string Rename = """
# So funktioniert das Umbenennen
Die App berechnet die CRC32 (einen Fingerabdruck des Inhalts) jedes ROMs und vergleicht sie mit der DAT-Datei (einer Liste von Titeln und CRC32). Nur passende Dateien werden in den korrekten Namen aus dem DAT umbenannt – egal, wie sie gerade heißen.

> Du prüfst die Liste immer vor dem Umbenennen, und „Rückgängig“ macht einen ganzen Durchgang wieder rückgängig.

# Vorbereitung
## DAT-Datei besorgen
Verwende für jedes System ein DAT im OfflineList-Format (XML). Auch alte DATs in Shift-JIS werden unterstützt.

## System einrichten
Wähle links das System und stelle in den „Systemeinstellungen“ Folgendes ein.

| Eintrag | Beschreibung |
| DAT-Datei | Die DAT-Datei |
| ROM-Ordner | Ordner mit den ROMs (mit `;` trennen) |
| Dateiendungen | Zu lesende Endungen (z. B. `.nes;.zip`) |
| Header-Behandlung | Ob iNES/FDS-Header in die CRC32 eingehen (meist „Automatisch“) |

Lesen und Abgleichen beginnen automatisch. Ab dem zweiten Mal werden unveränderte Dateien übersprungen, daher geht es schnell.

# Der Reiter „Umbenennen“
Wechsle oben auf den Reiter „Umbenennen“, um die Tabelle zu sehen.

| Spalte | Beschreibung |
| ✓ | Ob umbenannt wird (nur Zeilen „Umbenennen“ sind wählbar) |
| Status | Ergebnis des Abgleichs (siehe unten) |
| Aktueller Dateiname | Der jetzige Name |
| → Name im DAT | Der Name nach dem Umbenennen |
| CRC32 / Größe / Format | ROM-Informationen |

## Status
| Status | Bedeutung | Wird umbenannt |
| Korrekt | CRC32 passt, Name ist schon richtig | Nicht nötig |
| Umbenennen | CRC32 passt, Name ist anders | Ja |
| Doppelt | Für denselben Titel gibt es schon eine Datei | Nein |
| Nicht im DAT | Die CRC32 steht nicht im DAT | Nein |
| Fehlend | Im DAT, aber die Datei fehlt dir | ― |

# Umbenennen Schritt für Schritt
- Prüfe auf dem Reiter „Umbenennen“ die Zeilen „Umbenennen“ (anfangs sind alle angehakt)
- Entferne den Haken bei denen, die nicht umbenannt werden sollen. Das Kästchen in der Kopfzeile schaltet alle um
- Rechts erscheinen „Vorher“ und „Nachher“ der gewählten Zeile
- Drücke unten „Umbenennen“ und bestätige mit „Ja“
- Die Anzahl wird angezeigt und die Liste aktualisiert sich selbst

> Es werden nur Dateien geändert, deren CRC32 mit dem DAT übereinstimmt. Eine Datei wird übersprungen, wenn es den neuen Namen schon gibt.

## ROM-Namen in ZIP-Dateien
Ist „Einstellungen“ → „Auch das ROM in ZIP-Dateien umbenennen“ eingeschaltet, wird das ROM in jedem ZIP zusammen mit dem ZIP umbenannt.

## Zeichen, die Windows nicht erlaubt
`\` `/` `:` `*` `?` `"` `<` `>` `|` werden durch Zeichen voller Breite ersetzt.

# Rückgängig
„Rückgängig“ unten auf dem Reiter „Umbenennen“ macht den letzten Durchgang vollständig rückgängig. Nochmals drücken macht den davor rückgängig (bis zu den letzten 50).

> „Rückgängig“ betrifft immer den neuesten Durchgang, unabhängig vom System. Die Rückfrage zeigt Datum und Anzahl der Dateien.

# Nützliche Funktionen
| Funktion | So geht's |
| Vorhanden / Alle / Fehlend | Filtert die Liste. „Fehlend“ zeigt Spiele, die du noch nicht hast |
| Suche | In das Suchfeld tippen, um nach Titel oder Dateiname zu filtern |
| Neu abgleichen | Nach dem Hinzufügen oder Entfernen von ROMs verwenden |
| Dateiendungen ausblenden | „Einstellungen“ → „Dateiendungen (.zip usw.) in der Titelliste ausblenden“ |
| Unterordner lesen | „Einstellungen“ → „Auch Unterordner lesen“ |
| Nur DAT-Namen anzeigen | Für Arcade, wo umbenannte Dateien Emulatoren stören. In den „Systemeinstellungen“ einschalten, um die Dateinamen zu behalten und in der Liste die DAT-Namen zu zeigen |

# Fehlerbehebung
| Problem | Was prüfen |
| Nichts passt | Prüfen, ob das DAT zum richtigen System gehört. „Header-Behandlung“ auf „Mit Header“ oder „Ohne Header“ zu stellen kann helfen |
| „Die DAT-Datei konnte nicht gelesen werden“ | Sicherstellen, dass es ein OfflineList-DAT (XML) ist |
| „Doppelt“ | Du hast zwei oder mehr ROMs desselben Titels. Eins in einen anderen Ordner verschieben |
| Mit „(existiert)“ übersprungen | Eine Datei mit dem neuen Namen gibt es schon. Entscheiden, welche bleibt, und von Hand aufräumen |
| Name im ZIP hat sich nicht geändert | Ist der ZIP-Name schon richtig, bleibt der innere Name unverändert |
""";
}
