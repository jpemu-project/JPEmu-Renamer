// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
namespace JPEmuRenamer.Services;

// Manual（Português）。書式は HelpContent を参照
internal static class HelpPt
{
    public const string Play = """
# Primeiros passos
O JPEmu Renamer organiza as ROMs que você mesmo extraiu e permite jogá-las na hora, com o emulador integrado (núcleo integrado) ou com um emulador seu (exe).

## Da configuração ao jogo
- No início não há sistemas. Use "+ Adicionar sistema" no canto inferior esquerdo para criar um com o nome que quiser (ex.: Famicom, NES)
- Selecione-o e defina a pasta de ROMs em "Configurações do sistema"
- Na aba "Jogar", escolha um jogo
- Clique em "▶ Iniciar". Se o núcleo ainda não estiver instalado, você será perguntado se quer baixá-lo
- Pressione `Esc` para voltar ao iniciador

> Use ROMs extraídas de software que você possui.

# Configurações do sistema
Escolha um sistema à esquerda e abra "Configurações do sistema" no canto superior direito. Adicione sistemas com "+ Adicionar sistema". Escolher um modelo preenche extensões, ícone, cor e núcleo integrado.

| Item | Descrição |
| Nome do sistema | Nome exibido na lista |
| Ícone / cor | Ícone na lista de sistemas. Imagens próprias (PNG/ICO/JPG) são aceitas |
| Arquivo DAT | Lista de jogos usada para renomear (formato OfflineList). Opcional se você só vai jogar |
| Pastas de ROMs | Pastas que contêm as ROMs. Separe com `;` |
| Extensões | Extensões lidas (ex.: `.nes;.zip`) |
| Método de início padrão | Núcleo integrado ou emulador |
| Núcleo integrado | Núcleo usado neste sistema. Obtenha com "Baixar" / "Atualizar" |
| Emulador / argumentos | Usados ao jogar com um emulador externo |

## Sistemas que precisam de BIOS
Famicom Disk System, Mega-CD, PC Engine CD-ROM², Saturn, Neo Geo e outros precisam de arquivos BIOS. Os nomes dos arquivos aparecem abaixo do núcleo integrado em "Configurações do sistema". Coloque-os em "Configurações de jogo" → "Avançado" → "Pasta BIOS" (padrão: `system` na pasta de configurações).

# Iniciar um jogo
## Como iniciar
- Selecione um jogo e clique em "▶ Iniciar"
- Clique duas vezes em um jogo
- Selecione um jogo e pressione `Enter`

## Trocar o método de início
Clique com o botão direito em um jogo para escolher "Iniciar com o núcleo integrado" ou "Iniciar com o emulador". O método padrão é definido em "Configurações do sistema".

## Argumentos do emulador
| Marcador | Substituído por |
| `{rom}` | Caminho completo da ROM |
| `{romdir}` | Pasta da ROM |
| `{romname}` | Nome do arquivo ROM sem extensão |

# Controles
## Layout de teclado padrão
Os nomes dos botões seguem o layout do Super Famicom (A à direita, B embaixo).

| Comando | Tecla |
| Direcional | `↑` `↓` `←` `→` |
| A / B | `X` / `Z` |
| X / Y | `S` / `A` |
| L / R | `Q` / `W` |
| L2 / R2 | `E` / `R` |
| Select / Start | `Shift direito` / `Enter` |

## Controles
Controles conectados são detectados automaticamente e recebem um layout padrão: Xbox (XInput), controles USB genéricos (DirectInput), Switch Pro Controller, DualSense e outros. O jogador 1 usa o primeiro controle conectado e o jogador 2, o segundo. O analógico esquerdo também funciona como direcional. A vibração funciona nos jogos que a suportam.

## Atalhos
| Ação | Tecla | Controle |
| Menu | `F1` | Botão Guide |
| Avanço rápido (segurar) | `Tab` | |
| Salvar estado | `F2` | |
| Carregar estado | `F4` | |
| Espaço − / + | `F6` / `F7` | |
| Captura de tela | `F12` | |
| Tela cheia sim/não | `F11` ou `Alt+Enter` | |
| Pausa | `P` | |
| Reiniciar | `F10` | |
| Sair | `Esc` | |

> Todas as teclas e botões podem ser alterados em "Configurações de jogo". Em tela cheia, `Esc` volta primeiro para a janela.

# Menu do jogo
`F1` (ou o botão Guide do controle) abre o menu e pausa o jogo. Use `↑` `↓` para escolher, `Enter` (A) para confirmar e `Esc` (B) para fechar.

| Item | Descrição |
| Voltar ao jogo | Fecha o menu |
| Salvar / carregar estado | No espaço atual |
| Trocar espaço | Alterna entre os espaços |
| Captura de tela | Salva a tela do jogo em PNG |
| Tela cheia / janela | Alterna a exibição |
| Configurações de jogo… | Altera configurações durante o jogo; aplicadas na hora |
| Reiniciar | Reinicia o jogo |
| Sair para o iniciador | Encerra o jogo |

# Jogos salvos e estados
## Jogos salvos (bateria)
O que você salva dentro do jogo é gravado automaticamente (a cada 10 segundos aprox. e ao sair) e carregado da próxima vez.

## Estados salvos
Salvam o momento exato do jogo. `F2` salva, `F4` carrega e `F6` / `F7` trocam de espaço (10 espaços, de 0 a 9, por padrão).

## Locais
| Tipo | Local padrão |
| Jogos salvos | `saves\<sistema>` na pasta de configurações |
| Estados salvos | `states\<sistema>` na pasta de configurações |
| Capturas de tela | `screenshots\<sistema>` na pasta de configurações |

A pasta de configurações é `%APPDATA%\JPEmuRenamer` (ou `data` ao lado do exe se existir `portable.txt`). Os locais podem ser alterados em "Configurações de jogo".

# Configurações de jogo
Abra pelo botão "Configurações de jogo" da aba Jogar, clicando com o botão direito em um jogo, pelo menu do jogo ou pelas "Configurações" do aplicativo.

## Todos, sistema e jogo
As configurações se sobrepõem na ordem "Todos → Sistema → Jogo". Escolha no topo o nível que quer editar. Um nível superior pode substituir o valor do inferior. Itens substituídos mostram ●, e ↺ desfaz a substituição. Por exemplo: janela para tudo e tela cheia para um único jogo.

## Aba Básico
| Categoria | Descrição |
| Controle e teclado | Comandos do jogador 1. Clique em um item e pressione um botão ou tecla (`Esc` cancela, `Delete` apaga) |
| Locais de salvamento | Pastas de jogos salvos, estados e capturas |
| Vídeo | Janela/tela cheia, tamanho da janela (1x/2x/3x/ajustar), proporção |
| Áudio | Volume, mudo, dispositivo de saída |

## Aba Avançado
| Categoria | Descrição |
| Controle e teclado | Jogadores 1–4, turbo e sua velocidade, zona morta dos analógicos, atalhos. Combinações de teclas (ex.: `Alt+Enter`) são aceitas |
| Locais de salvamento | Subpastas (nenhuma/por sistema/por jogo), salvar ao lado da ROM, número de espaços, pasta BIOS |
| Vídeo | Resolução e taxa em tela cheia, escala inteira, filtragem, shaders (scanlines/CRT), V-Sync |
| Áudio | Taxa de amostragem, modo de saída (WASAPI compartilhado/exclusivo), latência, sincronia de áudio |
| Opções do núcleo | Opções próprias do núcleo (ex.: paleta do NES, proporção, overclock). Aparecem depois de iniciar um jogo com esse núcleo uma vez |

"Restaurar esta aba" volta a aba aos valores padrão do nível selecionado.

# Prévia automática
Na aba "Jogar", selecione um jogo e espere um pouco: imagens do jogo começam a passar no canto superior direito. O núcleo integrado roda sem som nem controles e mostra a demonstração que aparece após a tela de título. O núcleo integrado do sistema precisa estar instalado.

| Item | Padrão |
| Início | Após 0,3 segundo na seleção (não ao rolar rápido) |
| Velocidade | 4x |
| Pular o início | Os primeiros 10 segundos (logotipos etc.) |
| Recomeçar após | 60 segundos |
| Som / controles | Nenhum (os jogos salvos não são alterados) |

Altere esses valores, ou desative a prévia, em "Configurações" → "Prévia automática".

## Capturas
- Durante uma prévia, "Salvar esta tela" no canto inferior direito salva aquele instante como captura
- Clique direito em um jogo → "Criar capturas deste sistema…" inicia cada ROM do sistema em segundo plano e salva a tela em um momento fixo (30 segundos por padrão). Jogos que já têm captura podem ser pulados
- As capturas aparecem antes de a prévia começar e em jogos cuja tela não se move
- São salvas como `snapshots\<sistema>\<título>.png` na pasta de configurações

> Iniciar um jogo durante uma prévia ou a criação de capturas interrompe antes a prévia.

# Baixar e atualizar núcleos
Abra "Configurações" → "Baixar / atualizar núcleos…".

- A lista mostra, para cada núcleo, os sistemas, o tamanho e o status (não instalado / atualizado / com atualização)
- Selecione núcleos com "Núcleos dos meus sistemas", "Não instalados", "Com atualização" etc. e clique em "Baixar"
- "Atualizar tudo" atualiza todos os núcleos que têm atualização

> Os núcleos são baixados do site oficial do libretro. Um núcleo em uso por um jogo não pode ser atualizado.

# Solução de problemas
| Problema | O que verificar |
| Não inicia / "não foi possível carregar o jogo" | Se o sistema precisa de BIOS, verifique a pasta BIOS. Outro núcleo pode funcionar |
| "OpenGL não está disponível" | O núcleo usa OpenGL (ex.: N64). Atualize o driver de vídeo ou escolha outro núcleo |
| O som chia ou atrasa | Aumente a latência ou ative a sincronia de áudio em "Configurações de jogo" → "Avançado" → "Áudio" |
| Movimento travado | Desative o V-Sync ou feche outros aplicativos. Núcleos pesados como N64 precisam de um PC rápido |
| O controle não responde | Os controles detectados aparecem no topo de "Configurações de jogo". Reconecte se não estiver lá. O primeiro controle é o jogador 1 |
| A resolução continua alterada | Saia do jogo ou pressione `Esc` para voltar à janela |
| O primeiro toque é ignorado | Alguns jogos ignoram comandos por alguns segundos após iniciar. Espere um pouco |
| Sem prévia automática | Verifique se o núcleo integrado do sistema está instalado e se a prévia automática está ativada em "Configurações". Ela pausa quando o aplicativo está em segundo plano |
""";

    public const string Rename = """
# Como funciona a renomeação
O aplicativo calcula o CRC32 (uma impressão digital do conteúdo) de cada ROM e o compara com o arquivo DAT (uma lista de títulos e CRC32). Só os arquivos que coincidem são renomeados para o nome correto do DAT, seja qual for o nome atual.

> Você sempre revisa a lista antes de renomear, e "Desfazer" restaura depois uma execução inteira.

# Preparação
## Obter um arquivo DAT
Use um DAT no formato OfflineList (XML) para cada sistema. DATs antigos em Shift-JIS também são aceitos.

## Configurar o sistema
Escolha o sistema à esquerda e defina o seguinte em "Configurações do sistema".

| Item | Descrição |
| Arquivo DAT | O arquivo DAT |
| Pastas de ROMs | Pastas que contêm as ROMs (separadas por `;`) |
| Extensões | Extensões lidas (ex.: `.nes;.zip`) |
| Tratamento do cabeçalho | Se os cabeçalhos iNES/FDS entram no CRC32 (normalmente "Automático") |

A leitura e a comparação começam automaticamente. A partir da segunda vez, arquivos sem alteração são pulados, então é rápido.

# A aba Renomear
Mude para a aba "Renomear" no topo para ver a tabela.

| Coluna | Descrição |
| ✓ | Se será renomeado (só as linhas "A renomear" podem ser marcadas) |
| Status | Resultado da comparação (veja abaixo) |
| Nome do arquivo atual | O nome atual |
| → Nome no DAT | O nome após renomear |
| CRC32 / tamanho / formato | Informações da ROM |

## Status
| Status | Significado | Renomeado |
| Correto | O CRC32 coincide e o nome já está certo | Não precisa |
| A renomear | O CRC32 coincide, mas o nome é diferente | Sim |
| Duplicado | Já existe um arquivo para o mesmo título | Não |
| Fora do DAT | O CRC32 não está no DAT | Não |
| Falta | Está no DAT, mas você não tem o arquivo | ― |

# Renomear passo a passo
- Na aba "Renomear", confira as linhas "A renomear" (no início, todas vêm marcadas)
- Desmarque as que não quer renomear. A caixa do cabeçalho alterna todas
- À direita aparecem "Antes" e "Depois" da linha selecionada
- Clique em "Renomear" embaixo e confirme com "Sim"
- O número é exibido e a lista se atualiza sozinha

> Só são alterados os arquivos cujo CRC32 coincide com o DAT. Um arquivo é pulado se já existir outro com o novo nome.

## Nomes de ROM dentro de ZIP
Com "Configurações" → "Renomear também a ROM dentro dos arquivos ZIP" ativado, a ROM de cada ZIP é renomeada junto com o ZIP.

## Caracteres que o Windows não aceita
`\` `/` `:` `*` `?` `"` `<` `>` `|` são substituídos por equivalentes de largura total.

# Desfazer
"Desfazer" no rodapé da aba "Renomear" restaura por inteiro a última execução. Clique de novo para restaurar a anterior (até as últimas 50).

> "Desfazer" restaura a execução mais recente, independentemente do sistema. A confirmação mostra a data e o número de arquivos.

# Recursos úteis
| Recurso | Como usar |
| Que tenho / Todos / Que faltam | Filtra a lista. "Que faltam" mostra os jogos que você ainda não tem |
| Pesquisar | Digite na caixa de pesquisa para filtrar por título ou nome de arquivo |
| Comparar de novo | Use depois de adicionar ou remover ROMs |
| Ocultar extensões | "Configurações" → "Ocultar as extensões (.zip etc.) na lista" |
| Ler subpastas | "Configurações" → "Ler também as subpastas" |
| Mostrar só os nomes do DAT | Para arcade, onde renomear arquivos impede os emuladores de funcionar. Ative em "Configurações do sistema" para manter os nomes dos arquivos e mostrar os nomes do DAT na lista |

# Solução de problemas
| Problema | O que verificar |
| Nada coincide | Verifique se o DAT é do sistema certo. Trocar "Tratamento do cabeçalho" para "Incluir cabeçalho" ou "Excluir cabeçalho" pode ajudar |
| "Não foi possível ler o arquivo DAT" | Confirme que é um DAT OfflineList (XML) |
| "Duplicado" | Você tem duas ou mais ROMs do mesmo título. Mova uma para outra pasta |
| Pulado com "(já existe)" | Já existe um arquivo com o novo nome. Decida qual manter e organize à mão |
| O nome dentro do ZIP não mudou | Se o nome do ZIP já está correto, o nome interno fica como está |
""";
}
