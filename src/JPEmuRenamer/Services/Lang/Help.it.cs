// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
namespace JPEmuRenamer.Services;

// Manuale（Italiano）。書式は HelpContent を参照
internal static class HelpIt
{
    public const string Play = """
# Per iniziare
JPEmu Renamer gestisce le ROM che hai estratto tu stesso e ti permette di giocarle subito, con l'emulatore integrato (core integrato) o con un emulatore tuo (exe).

## Dalla configurazione al gioco
- All'inizio non ci sono sistemi. Usa "+ Aggiungi sistema" in basso a sinistra per crearne uno con il nome che preferisci (es. Famicom, NES)
- Selezionalo e imposta la sua cartella ROM in "Impostazioni sistema"
- Nella scheda "Gioca", scegli un gioco
- Premi "▶ Avvia". Se il core non è ancora installato, ti viene chiesto se scaricarlo
- Premi `Esc` per tornare al launcher

> Usa ROM estratte da software che possiedi.

# Impostazioni sistema
Scegli un sistema a sinistra e apri "Impostazioni sistema" in alto a destra. Aggiungi sistemi con "+ Aggiungi sistema". Scegliendo un modello vengono compilati estensioni, icona, colore e core integrato.

| Voce | Descrizione |
| Nome del sistema | Nome mostrato nell'elenco |
| Icona / colore | Icona nell'elenco dei sistemi. Sono supportate immagini personalizzate (PNG/ICO/JPG) |
| File DAT | Elenco dei giochi usato per rinominare (formato OfflineList). Facoltativo se vuoi solo giocare |
| Cartelle ROM | Cartelle che contengono le ROM. Separale con `;` |
| Estensioni | Estensioni da leggere (es. `.nes;.zip`) |
| Metodo di avvio predefinito | Core integrato o emulatore |
| Core integrato | Core usato per questo sistema. Ottienilo con "Scarica" / "Aggiorna" |
| Emulatore / argomenti | Usati per giocare con un emulatore esterno |

## Sistemi che richiedono un BIOS
Famicom Disk System, Mega-CD, PC Engine CD-ROM², Saturn, Neo Geo e altri richiedono file BIOS. I nomi dei file sono indicati sotto il core integrato in "Impostazioni sistema". Mettili in "Impostazioni di gioco" → "Avanzate" → "Cartella BIOS" (predefinita: `system` nella cartella delle impostazioni).

# Avviare un gioco
## Come avviare
- Seleziona un gioco e premi "▶ Avvia"
- Fai doppio clic su un gioco
- Seleziona un gioco e premi `Invio`

## Cambiare metodo di avvio
Fai clic destro su un gioco per scegliere "Avvia con il core integrato" o "Avvia con l'emulatore". Il metodo predefinito si imposta in "Impostazioni sistema".

## Argomenti dell'emulatore
| Segnaposto | Sostituito con |
| `{rom}` | Percorso completo della ROM |
| `{romdir}` | Cartella della ROM |
| `{romname}` | Nome del file ROM senza estensione |

# Comandi
## Disposizione predefinita della tastiera
I nomi dei pulsanti seguono la disposizione del Super Famicom (A a destra, B in basso).

| Comando | Tasto |
| Croce direzionale | `↑` `↓` `←` `→` |
| A / B | `X` / `Z` |
| X / Y | `S` / `A` |
| L / R | `Q` / `W` |
| L2 / R2 | `E` / `R` |
| Select / Start | `Maiusc destro` / `Invio` |

## Controller
I controller collegati vengono rilevati automaticamente e ricevono una disposizione standard: Xbox (XInput), USB generici (DirectInput), controller Switch Pro, DualSense e altri. Il giocatore 1 usa il primo controller collegato, il giocatore 2 il secondo. Anche la levetta sinistra funziona come croce direzionale. La vibrazione funziona nei giochi che la supportano.

## Scorciatoie
| Azione | Tasto | Controller |
| Menu | `F1` | Tasto Guide |
| Avanti veloce (tenere premuto) | `Tab` | |
| Salva stato | `F2` | |
| Carica stato | `F4` | |
| Slot − / + | `F6` / `F7` | |
| Screenshot | `F12` | |
| Schermo intero sì/no | `F11` o `Alt+Invio` | |
| Pausa | `P` | |
| Reset | `F10` | |
| Esci | `Esc` | |

> Tutti i tasti e i pulsanti si cambiano in "Impostazioni di gioco". A schermo intero, `Esc` torna prima alla finestra.

# Menu di gioco
`F1` (o il tasto Guide del controller) apre il menu e mette in pausa il gioco. Usa `↑` `↓` per scegliere, `Invio` (A) per confermare ed `Esc` (B) per chiudere.

| Voce | Descrizione |
| Riprendi | Chiude il menu |
| Salva / carica stato | Nello slot attuale |
| Cambia slot | Passa da uno slot all'altro |
| Screenshot | Salva la schermata del gioco in PNG |
| Schermo intero / finestra | Cambia la visualizzazione |
| Impostazioni di gioco… | Cambia le impostazioni mentre giochi; si applicano subito |
| Reset | Riavvia il gioco |
| Esci al launcher | Termina il gioco |

# Salvataggi e salvataggi di stato
## Salvataggi (batteria)
I salvataggi fatti nel gioco vengono memorizzati automaticamente (circa ogni 10 secondi e all'uscita) e caricati la volta successiva.

## Salvataggi di stato
Salvano l'istante esatto del gioco. `F2` salva, `F4` carica, `F6` / `F7` cambiano slot (10 slot, da 0 a 9, per impostazione predefinita).

## Percorsi
| Tipo | Percorso predefinito |
| Salvataggi | `saves\<sistema>` nella cartella delle impostazioni |
| Salvataggi di stato | `states\<sistema>` nella cartella delle impostazioni |
| Screenshot | `screenshots\<sistema>` nella cartella delle impostazioni |

La cartella delle impostazioni è `%APPDATA%\JPEmuRenamer` (oppure `data` accanto all'exe se c'è `portable.txt`). I percorsi si cambiano in "Impostazioni di gioco".

# Impostazioni di gioco
Aprile dal pulsante "Impostazioni di gioco" della scheda Gioca, dal clic destro su un gioco, dal menu di gioco o dalle "Impostazioni" dell'app.

## Tutti, sistema e gioco
Le impostazioni si sovrappongono nell'ordine "Tutti → Sistema → Gioco". Scegli in alto il livello da modificare. Un livello superiore può sostituire il valore di quello inferiore. Le voci sostituite mostrano ●, e ↺ annulla la sostituzione. Ad esempio: finestra per tutto, schermo intero per un solo gioco.

## Scheda Base
| Categoria | Descrizione |
| Controller e tastiera | Comandi del giocatore 1. Fai clic su una voce, poi premi un pulsante o un tasto (`Esc` annulla, `Canc` cancella) |
| Percorsi di salvataggio | Cartelle di salvataggi, salvataggi di stato e screenshot |
| Video | Finestra/schermo intero, dimensione della finestra (1x/2x/3x/adatta), proporzioni |
| Audio | Volume, muto, dispositivo di uscita |

## Scheda Avanzate
| Categoria | Descrizione |
| Controller e tastiera | Giocatori 1–4, turbo e sua velocità, zona morta delle levette, scorciatoie. Sono supportate combinazioni di tasti (es. `Alt+Invio`) |
| Percorsi di salvataggio | Sottocartelle (nessuna/per sistema/per gioco), salvataggi accanto alla ROM, numero di slot, cartella BIOS |
| Video | Risoluzione e frequenza a schermo intero, scala intera, filtro, shader (linee di scansione/CRT), V-Sync |
| Audio | Frequenza di campionamento, modalità di uscita (WASAPI condiviso/esclusivo), latenza, correzione sincronia audio |
| Opzioni del core | Opzioni del core (es. palette NES, proporzioni, overclock). Compaiono dopo aver avviato una volta un gioco con quel core |

"Ripristina questa scheda" riporta la scheda ai valori predefiniti per il livello selezionato.

# Anteprima automatica
Nella scheda "Gioca", seleziona un gioco e aspetta un attimo: in alto a destra parte un filmato del gioco. Il core integrato gira senza audio né comandi e mostra la demo che parte dopo la schermata del titolo. Il core integrato del sistema deve essere installato.

| Voce | Predefinito |
| Avvio | Dopo 0,3 secondi sulla selezione (non durante lo scorrimento veloce) |
| Velocità | 4x |
| Salta l'inizio | I primi 10 secondi (loghi ecc.) |
| Ricomincia dopo | 60 secondi |
| Audio / comandi | Nessuno (i salvataggi non vengono toccati) |

Cambia questi valori, o disattiva l'anteprima, in "Impostazioni" → "Anteprima automatica".

## Istantanee
- Durante un'anteprima, "Salva questa schermata" in basso a destra salva quell'istante come istantanea
- Clic destro su un gioco → "Crea istantanee per questo sistema…" avvia ogni ROM del sistema in background e salva la schermata a un momento stabilito (30 secondi per impostazione predefinita). I giochi che hanno già un'istantanea possono essere saltati
- Le istantanee vengono mostrate prima che parta l'anteprima e per i giochi con schermata ferma
- Vengono salvate come `snapshots\<sistema>\<titolo>.png` nella cartella delle impostazioni

> Avviare un gioco durante un'anteprima o la creazione di istantanee ferma prima l'anteprima.

# Scaricare e aggiornare i core
Apri "Impostazioni" → "Scarica / aggiorna i core…".

- L'elenco mostra per ogni core i sistemi, la dimensione e lo stato (non installato / aggiornato / aggiornamento disponibile)
- Seleziona i core con "Core dei miei sistemi", "Non installati", "Con aggiornamenti" e così via, poi premi "Scarica"
- "Aggiorna tutto" aggiorna tutti i core che hanno un aggiornamento

> I core vengono scaricati dal sito ufficiale di libretro. Un core usato da un gioco in corso non può essere aggiornato.

# Risoluzione dei problemi
| Problema | Cosa controllare |
| Non si avvia / "non è riuscito a caricare il gioco" | Se il sistema richiede un BIOS, controlla la cartella BIOS. Un altro core potrebbe funzionare |
| "OpenGL non è disponibile" | Il core usa OpenGL (es. N64). Aggiorna il driver grafico o scegli un altro core |
| L'audio gracchia o è in ritardo | Aumenta la latenza o attiva la correzione sincronia audio in "Impostazioni di gioco" → "Avanzate" → "Audio" |
| Movimento a scatti | Disattiva il V-Sync o chiudi altre app. I core impegnativi come N64 richiedono un PC veloce |
| Il controller non risponde | I controller rilevati sono elencati in alto in "Impostazioni di gioco". Ricollegalo se manca. Il primo controller è il giocatore 1 |
| La risoluzione resta cambiata | Esci dal gioco o premi `Esc` per tornare alla finestra |
| La prima pressione viene ignorata | Alcuni giochi ignorano i comandi per qualche secondo dopo l'avvio. Aspetta un attimo |
| Nessuna anteprima automatica | Controlla che il core integrato del sistema sia installato e che l'anteprima automatica sia attiva in "Impostazioni". Si mette in pausa quando l'app è in background |
""";

    public const string Rename = """
# Come funziona la rinomina
L'app calcola il CRC32 (un'impronta del contenuto) di ogni ROM e lo confronta con il file DAT (un elenco di titoli e CRC32). Solo i file corrispondenti vengono rinominati con il nome corretto del DAT, comunque si chiamino ora.

> Controlli sempre l'elenco prima di rinominare, e "Annulla" ripristina poi un'intera esecuzione.

# Preparazione
## Procurarsi un file DAT
Usa un DAT in formato OfflineList (XML) per ogni sistema. Sono supportati anche i vecchi DAT in Shift-JIS.

## Configurare il sistema
Scegli il sistema a sinistra e imposta quanto segue in "Impostazioni sistema".

| Voce | Descrizione |
| File DAT | Il file DAT |
| Cartelle ROM | Cartelle che contengono le ROM (separate da `;`) |
| Estensioni | Estensioni da leggere (es. `.nes;.zip`) |
| Gestione dell'header | Se includere gli header iNES/FDS nel CRC32 (di solito "Auto") |

Lettura e confronto partono automaticamente. Dalla seconda volta i file invariati vengono saltati, quindi è veloce.

# La scheda Rinomina
Passa alla scheda "Rinomina" in alto per vedere la tabella.

| Colonna | Descrizione |
| ✓ | Se rinominare (selezionabili solo le righe "Da rinominare") |
| Stato | Risultato del confronto (vedi sotto) |
| Nome file attuale | Il nome attuale |
| → Nome nel DAT | Il nome dopo la rinomina |
| CRC32 / dimensione / formato | Informazioni sulla ROM |

## Stati
| Stato | Significato | Rinominato |
| Corretto | Il CRC32 corrisponde e il nome è già giusto | Non serve |
| Da rinominare | Il CRC32 corrisponde ma il nome è diverso | Sì |
| Duplicato | Esiste già un file per lo stesso titolo | No |
| Non nel DAT | Il CRC32 non è nel DAT | No |
| Mancante | Nel DAT, ma non hai il file | ― |

# Rinominare passo per passo
- Nella scheda "Rinomina", controlla le righe "Da rinominare" (all'inizio sono tutte spuntate)
- Togli la spunta da quelle da non rinominare. La casella dell'intestazione le cambia tutte
- A destra compaiono "Prima" e "Dopo" per la riga selezionata
- Premi "Rinomina" in basso e conferma con "Sì"
- Viene mostrato il numero e l'elenco si aggiorna da solo

> Vengono modificati solo i file il cui CRC32 corrisponde al DAT. Un file viene saltato se esiste già un file con il nuovo nome.

## Nomi delle ROM nei file ZIP
Con "Impostazioni" → "Rinomina anche la ROM nei file ZIP" attivo, la ROM dentro ogni ZIP viene rinominata insieme allo ZIP.

## Caratteri non ammessi da Windows
`\` `/` `:` `*` `?` `"` `<` `>` `|` vengono sostituiti da equivalenti a larghezza piena.

# Annulla
"Annulla" in fondo alla scheda "Rinomina" ripristina per intero l'ultima esecuzione. Premilo di nuovo per ripristinare quella precedente (fino alle ultime 50).

> "Annulla" ripristina l'esecuzione più recente indipendentemente dal sistema. La conferma ne mostra data e numero di file.

# Funzioni utili
| Funzione | Come si usa |
| Posseduti / Tutti / Mancanti | Filtra l'elenco. "Mancanti" mostra i giochi che non hai ancora |
| Cerca | Scrivi nella casella di ricerca per filtrare per titolo o nome file |
| Riconfronta | Da usare dopo aver aggiunto o rimosso ROM |
| Nascondi estensioni | "Impostazioni" → "Nascondi le estensioni nell'elenco" |
| Leggi sottocartelle | "Impostazioni" → "Leggi anche le sottocartelle" |
| Mostra solo i nomi del DAT | Per l'arcade, dove rinominare i file impedisce agli emulatori di funzionare. Attivalo in "Impostazioni sistema" per mantenere i nomi file e mostrare i nomi del DAT nell'elenco |

# Risoluzione dei problemi
| Problema | Cosa controllare |
| Nessuna corrispondenza | Controlla che il DAT sia del sistema giusto. Cambiare "Gestione dell'header" in "Includi" o "Escludi" può aiutare |
| "Impossibile leggere il file DAT" | Assicurati che sia un DAT OfflineList (XML) |
| "Duplicato" | Hai due o più ROM per lo stesso titolo. Sposta una in un'altra cartella |
| Saltato con "(esiste già)" | Esiste già un file con il nuovo nome. Decidi quale tenere e sistemali a mano |
| Il nome dentro lo ZIP non è cambiato | Se il nome dello ZIP è già corretto, il nome interno resta invariato |
""";
}
