// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
namespace JPEmuRenamer.Services;

// Manuel（Français）。書式は HelpContent を参照
internal static class HelpFr
{
    public const string Play = """
# Prise en main
JPEmu Renamer gère les ROM que vous avez extraites vous-même et vous permet d'y jouer immédiatement, avec l'émulateur intégré (cœur intégré) ou un émulateur que vous fournissez (exe).

## De la configuration au jeu
- Au départ, il n'y a aucun système. Utilisez « + Ajouter un système » en bas à gauche pour en créer un avec le nom de votre choix (ex. : Famicom, NES)
- Sélectionnez-le et indiquez son dossier de ROM dans « Paramètres du système »
- Dans l'onglet « Jouer », choisissez un jeu
- Appuyez sur « ▶ Lancer ». Si le cœur n'est pas encore installé, l'application propose de le télécharger
- Appuyez sur `Échap` pour revenir au lanceur

> Utilisez des ROM extraites de logiciels que vous possédez.

# Paramètres du système
Choisissez un système à gauche et ouvrez « Paramètres du système » en haut à droite. Ajoutez des systèmes avec « + Ajouter un système ». Choisir un modèle remplit les extensions, l'icône, la couleur et le cœur intégré.

| Élément | Description |
| Nom du système | Nom affiché dans la liste |
| Icône / couleur | Icône dans la liste des systèmes. Les images personnalisées (PNG/ICO/JPG) sont acceptées |
| Fichier DAT | Liste de jeux utilisée pour renommer (format OfflineList). Facultatif si vous voulez seulement jouer |
| Dossiers de ROM | Dossiers contenant les ROM. Séparez-les par `;` |
| Extensions | Extensions à lire (ex. : `.nes;.zip`) |
| Méthode de lancement par défaut | Cœur intégré ou émulateur |
| Cœur intégré | Cœur utilisé pour ce système. Obtenez-le avec « Télécharger » / « Mettre à jour » |
| Émulateur / arguments | Utilisés pour jouer avec un émulateur externe |

## Systèmes nécessitant un BIOS
Le Famicom Disk System, le Mega-CD, la PC Engine CD-ROM², la Saturn, la Neo Geo et d'autres nécessitent des fichiers BIOS. Leurs noms sont indiqués sous le cœur intégré dans « Paramètres du système ». Placez-les dans « Paramètres de jeu » → « Avancé » → « Dossier BIOS » (par défaut : `system` dans le dossier des paramètres).

# Lancer un jeu
## Comment lancer
- Sélectionnez un jeu et appuyez sur « ▶ Lancer »
- Double-cliquez sur un jeu
- Sélectionnez un jeu et appuyez sur `Entrée`

## Changer de méthode de lancement
Faites un clic droit sur un jeu pour choisir « Lancer avec le cœur intégré » ou « Lancer avec l'émulateur ». La méthode par défaut se règle dans « Paramètres du système ».

## Arguments de l'émulateur
| Code | Remplacé par |
| `{rom}` | Chemin complet de la ROM |
| `{romdir}` | Dossier de la ROM |
| `{romname}` | Nom du fichier ROM sans extension |

# Commandes
## Disposition par défaut du clavier
Les noms des boutons suivent la disposition Super Famicom (A à droite, B en bas).

| Commande | Touche |
| Croix directionnelle | `↑` `↓` `←` `→` |
| A / B | `X` / `Z` |
| X / Y | `S` / `A` |
| L / R | `Q` / `W` |
| L2 / R2 | `E` / `R` |
| Select / Start | `Maj droite` / `Entrée` |

## Manettes
Les manettes connectées sont détectées automatiquement et reçoivent une disposition standard : Xbox (XInput), USB génériques (DirectInput), manette Switch Pro, DualSense, etc. Le joueur 1 utilise la première manette connectée, le joueur 2 la deuxième. Le stick gauche fonctionne aussi comme croix directionnelle. Les vibrations fonctionnent dans les jeux qui les gèrent.

## Raccourcis
| Action | Touche | Manette |
| Menu | `F1` | Bouton Guide |
| Avance rapide (maintenir) | `Tab` | |
| Sauvegarder l'état | `F2` | |
| Charger l'état | `F4` | |
| Emplacement − / + | `F6` / `F7` | |
| Capture d'écran | `F12` | |
| Basculer en plein écran | `F11` ou `Alt+Entrée` | |
| Pause | `P` | |
| Réinitialiser | `F10` | |
| Quitter | `Échap` | |

> Toutes les touches et tous les boutons se modifient dans « Paramètres de jeu ». En plein écran, `Échap` revient d'abord en fenêtre.

# Menu en jeu
`F1` (ou le bouton Guide de la manette) ouvre le menu et met le jeu en pause. Utilisez `↑` `↓` pour choisir, `Entrée` (A) pour valider et `Échap` (B) pour fermer.

| Élément | Description |
| Reprendre | Ferme le menu |
| Sauvegarder / charger l'état | Dans l'emplacement actuel |
| Changer d'emplacement | Passe d'un emplacement à l'autre |
| Capture d'écran | Enregistre l'écran du jeu en PNG |
| Plein écran / fenêtre | Change l'affichage |
| Paramètres de jeu… | Modifiez les réglages en jouant ; ils s'appliquent tout de suite |
| Réinitialiser | Réinitialise le jeu |
| Quitter vers le lanceur | Termine le jeu |

# Sauvegardes et sauvegardes d'état
## Sauvegardes (batterie)
Les sauvegardes faites dans le jeu sont enregistrées automatiquement (environ toutes les 10 secondes et à la fermeture) et rechargées la fois suivante.

## Sauvegardes d'état
Enregistrent l'instant exact du jeu. `F2` sauvegarde, `F4` charge, `F6` / `F7` changent d'emplacement (10 emplacements, de 0 à 9, par défaut).

## Emplacements
| Type | Emplacement par défaut |
| Sauvegardes | `saves\<système>` dans le dossier des paramètres |
| Sauvegardes d'état | `states\<système>` dans le dossier des paramètres |
| Captures d'écran | `screenshots\<système>` dans le dossier des paramètres |

Le dossier des paramètres est `%APPDATA%\JPEmuRenamer` (ou `data` à côté de l'exe si `portable.txt` s'y trouve). Les emplacements se modifient dans « Paramètres de jeu ».

# Paramètres de jeu
Ouvrez-les avec le bouton « Paramètres de jeu » de l'onglet Jouer, le clic droit sur un jeu, le menu en jeu ou les « Paramètres » de l'application.

## Tous, système et jeu
Les réglages s'empilent dans l'ordre « Tous → Système → Jeu ». Choisissez en haut le niveau à modifier. Un niveau supérieur peut remplacer la valeur du niveau inférieur. Les réglages remplacés affichent ●, et ↺ annule le remplacement. Par exemple : fenêtre pour tout, plein écran pour un seul jeu.

## Onglet Essentiel
| Catégorie | Description |
| Manette et clavier | Commandes du joueur 1. Cliquez sur un élément, puis appuyez sur un bouton ou une touche (`Échap` annule, `Suppr` efface) |
| Emplacements de sauvegarde | Dossiers des sauvegardes, sauvegardes d'état et captures |
| Vidéo | Fenêtre/plein écran, taille de la fenêtre (1x/2x/3x/adapter), format d'image |
| Audio | Volume, muet, périphérique de sortie |

## Onglet Avancé
| Catégorie | Description |
| Manette et clavier | Joueurs 1 à 4, turbo et vitesse du turbo, zone morte des sticks, raccourcis. Les combinaisons de touches (ex. : `Alt+Entrée`) sont possibles |
| Emplacements de sauvegarde | Sous-dossiers (aucun/par système/par jeu), sauvegardes à côté de la ROM, nombre d'emplacements, dossier BIOS |
| Vidéo | Résolution et fréquence en plein écran, mise à l'échelle entière, filtrage, shaders (lignes de balayage/CRT), synchro verticale |
| Audio | Fréquence d'échantillonnage, mode de sortie (WASAPI partagé/exclusif), latence, correction de synchro audio |
| Options du cœur | Options propres au cœur (ex. : palette NES, format d'image, overclocking). Elles apparaissent après avoir lancé une fois un jeu avec ce cœur |

« Réinitialiser cet onglet » rétablit cet onglet pour le niveau sélectionné.

# Aperçu automatique
Dans l'onglet « Jouer », sélectionnez un jeu et attendez un instant : des images du jeu défilent en haut à droite. Le cœur intégré tourne sans son ni commande et montre la démo qui démarre après l'écran titre. Le cœur intégré du système doit être installé.

| Élément | Valeur par défaut |
| Démarrage | Après 0,3 seconde sur la sélection (pas pendant un défilement rapide) |
| Vitesse | 4x |
| Passer le début | Les 10 premières secondes (logos, etc.) |
| Recommencer après | 60 secondes |
| Son / commandes | Aucun (les sauvegardes ne sont pas touchées) |

Modifiez ces valeurs, ou désactivez l'aperçu, dans « Paramètres » → « Aperçu automatique ».

## Captures
- Pendant un aperçu, « Enregistrer cet écran » en bas à droite enregistre l'instant comme capture
- Clic droit sur un jeu → « Créer les captures de ce système… » lance chaque ROM du système en arrière-plan et enregistre l'écran à un moment donné (30 secondes par défaut). Les jeux qui ont déjà une capture peuvent être ignorés
- Les captures s'affichent avant le début de l'aperçu et pour les jeux dont l'écran ne bouge pas
- Elles sont enregistrées sous `snapshots\<système>\<titre>.png` dans le dossier des paramètres

> Lancer un jeu pendant un aperçu ou des captures groupées arrête d'abord l'aperçu.

# Télécharger et mettre à jour les cœurs
Ouvrez « Paramètres » → « Télécharger / mettre à jour les cœurs… ».

- La liste indique pour chaque cœur ses systèmes, sa taille et son état (non installé / à jour / mise à jour disponible)
- Sélectionnez les cœurs avec « Cœurs de mes systèmes », « Non installés », « Avec mise à jour », etc., puis appuyez sur « Télécharger »
- « Tout mettre à jour » met à jour tous les cœurs qui ont une mise à jour

> Les cœurs sont téléchargés depuis le site officiel de libretro. Un cœur utilisé par un jeu en cours ne peut pas être mis à jour.

# Dépannage
| Problème | À vérifier |
| Démarrage impossible / « n'a pas pu charger le jeu » | Si le système nécessite un BIOS, vérifiez le dossier BIOS. Un autre cœur peut fonctionner |
| « OpenGL n'est pas disponible » | Le cœur utilise OpenGL (ex. : N64). Mettez à jour le pilote graphique ou choisissez un autre cœur |
| Le son grésille ou est en retard | Augmentez la latence ou activez la correction de synchro audio dans « Paramètres de jeu » → « Avancé » → « Audio » |
| Mouvements saccadés | Désactivez la synchro verticale ou fermez d'autres applications. Les cœurs exigeants comme la N64 demandent un PC rapide |
| La manette ne répond pas | Les manettes détectées sont listées en haut de « Paramètres de jeu ». Reconnectez-la si elle n'y est pas. La première manette est le joueur 1 |
| La résolution reste modifiée | Quittez le jeu ou appuyez sur `Échap` pour revenir en fenêtre |
| La première pression est ignorée | Certains jeux ignorent les commandes quelques secondes après le démarrage. Patientez un peu |
| Pas d'aperçu automatique | Vérifiez que le cœur intégré du système est installé et que l'aperçu automatique est activé dans « Paramètres ». Il se met en pause quand l'application est en arrière-plan |
""";

    public const string Rename = """
# Principe du renommage
L'application calcule le CRC32 (une empreinte du contenu) de chaque ROM et le compare au fichier DAT (une liste de titres et de CRC32). Seuls les fichiers correspondants sont renommés avec le nom correct du DAT, quel que soit leur nom actuel.

> Vous vérifiez toujours la liste avant de renommer, et « Annuler » rétablit ensuite toute une exécution.

# Préparation
## Obtenir un fichier DAT
Utilisez un DAT au format OfflineList (XML) pour chaque système. Les anciens DAT en Shift-JIS sont pris en charge.

## Configurer le système
Choisissez le système à gauche et réglez les éléments suivants dans « Paramètres du système ».

| Élément | Description |
| Fichier DAT | Le fichier DAT |
| Dossiers de ROM | Dossiers contenant les ROM (séparés par `;`) |
| Extensions | Extensions à lire (ex. : `.nes;.zip`) |
| Gestion de l'en-tête | Inclure ou non les en-têtes iNES/FDS dans le CRC32 (en général « Auto ») |

La lecture et la comparaison démarrent automatiquement. Les fichiers inchangés sont ignorés à partir de la deuxième fois, c'est donc rapide.

# L'onglet Renommer
Passez à l'onglet « Renommer » en haut pour afficher le tableau.

| Colonne | Description |
| ✓ | Renommer ou non (seules les lignes « À renommer » sont sélectionnables) |
| État | Résultat de la comparaison (voir ci-dessous) |
| Nom de fichier actuel | Le nom actuel |
| → Nom dans le DAT | Le nom après renommage |
| CRC32 / taille / format | Informations sur la ROM |

## États
| État | Signification | Renommé |
| Correct | Le CRC32 correspond et le nom est déjà bon | Inutile |
| À renommer | Le CRC32 correspond mais le nom diffère | Oui |
| Doublon | Un fichier pour le même titre existe déjà | Non |
| Absent du DAT | Le CRC32 n'est pas dans le DAT | Non |
| Manquant | Dans le DAT, mais vous n'avez pas le fichier | ― |

# Renommer étape par étape
- Dans l'onglet « Renommer », vérifiez les lignes « À renommer » (toutes cochées au départ)
- Décochez celles à ne pas renommer. La case de l'en-tête coche ou décoche tout
- La partie droite affiche « Avant » et « Après » pour la ligne sélectionnée
- Appuyez sur « Renommer » en bas et confirmez avec « Oui »
- Le nombre est affiché et la liste se met à jour automatiquement

> Seuls les fichiers dont le CRC32 correspond au DAT sont modifiés. Un fichier est ignoré si un fichier portant le nouveau nom existe déjà.

## Noms des ROM dans les ZIP
Avec « Paramètres » → « Renommer aussi la ROM dans les fichiers ZIP » activé, la ROM contenue dans chaque ZIP est renommée en même temps que le ZIP.

## Caractères interdits par Windows
`\` `/` `:` `*` `?` `"` `<` `>` `|` sont remplacés par des équivalents pleine chasse.

# Annuler
« Annuler » en bas de l'onglet « Renommer » rétablit entièrement la dernière exécution. Appuyez de nouveau pour rétablir la précédente (jusqu'aux 50 dernières).

> « Annuler » rétablit l'exécution la plus récente, quel que soit le système. La confirmation indique sa date et le nombre de fichiers.

# Fonctions pratiques
| Fonction | Utilisation |
| Possédés / Tous / Manquants | Filtre la liste. « Manquants » montre les jeux que vous n'avez pas encore |
| Recherche | Tapez dans le champ de recherche pour filtrer par titre ou nom de fichier |
| Recomparer | À utiliser après avoir ajouté ou retiré des ROM |
| Masquer les extensions | « Paramètres » → « Masquer les extensions dans la liste » |
| Lire les sous-dossiers | « Paramètres » → « Lire aussi les sous-dossiers » |
| Afficher seulement les noms du DAT | Pour l'arcade, où renommer les fichiers empêche les émulateurs de fonctionner. Activez-le dans « Paramètres du système » pour garder les noms de fichier et afficher les noms du DAT dans la liste |

# Dépannage
| Problème | À vérifier |
| Rien ne correspond | Vérifiez que le DAT correspond au bon système. Passer « Gestion de l'en-tête » sur « Inclure » ou « Exclure » peut aider |
| « Impossible de lire le fichier DAT » | Vérifiez qu'il s'agit d'un DAT OfflineList (XML) |
| « Doublon » | Vous avez au moins deux ROM pour le même titre. Déplacez-en une dans un autre dossier |
| Ignoré avec « (existe déjà) » | Un fichier portant le nouveau nom existe déjà. Choisissez lequel garder et triez-les à la main |
| Le nom dans le ZIP n'a pas changé | Si le nom du ZIP est déjà correct, le nom interne reste tel quel |
""";
}
