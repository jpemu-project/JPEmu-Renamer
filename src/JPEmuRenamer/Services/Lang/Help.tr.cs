// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
namespace JPEmuRenamer.Services;

// Kılavuz（Türkçe）。書式は HelpContent を参照
internal static class HelpTr
{
    public const string Play = """
# Başlarken
JPEmu Renamer, kendi aldığınız ROM'ları düzenler ve onları hemen oynamanızı sağlar – yerleşik emülatörle (yerleşik çekirdek) ya da kendi emülatörünüzle (exe).

## Kurulumdan oyuna
- Başta hiç sistem yoktur. Sol alttaki "+ Sistem ekle" ile istediğiniz adda bir sistem oluşturun (ör. Famicom, NES)
- Onu seçin ve "Sistem ayarları"ndan ROM klasörünü ayarlayın
- "Oyna" sekmesinde bir oyun seçin
- "▶ Başlat"a basın. Çekirdek henüz yüklü değilse indirilip indirilmeyeceği sorulur
- Başlatıcıya dönmek için `Esc`'ye basın

> Sahip olduğunuz yazılımlardan aldığınız ROM'ları kullanın.

# Sistem ayarları
Soldan bir sistem seçin ve sağ üstteki "Sistem ayarları"nı açın. "+ Sistem ekle" ile yeni sistem ekleyebilirsiniz. Bir şablon seçmek uzantıları, simgeyi, rengi ve yerleşik çekirdeği doldurur.

| Öğe | Açıklama |
| Sistem adı | Listede görünen ad |
| Simge / renk | Sistem listesindeki simge. Kendi resimleriniz (PNG/ICO/JPG) de kullanılabilir |
| DAT dosyası | Yeniden adlandırma için oyun listesi (OfflineList biçimi). Yalnızca oynayacaksanız gerekmez |
| ROM klasörleri | ROM'ların bulunduğu klasörler. Birden çoksa `;` ile ayırın |
| Uzantılar | Okunacak uzantılar (ör. `.nes;.zip`) |
| Varsayılan başlatma yöntemi | Yerleşik çekirdek veya emülatör |
| Yerleşik çekirdek | Bu sistemin çekirdeği. "İndir" / "Güncelle" ile edinin |
| Emülatör / argümanlar | Harici bir emülatörle oynarken kullanılır |

## BIOS gerektiren sistemler
Famicom Disk System, Mega-CD, PC Engine CD-ROM², Saturn, Neo Geo ve diğerleri BIOS dosyaları gerektirir. Dosya adları "Sistem ayarları"nda yerleşik çekirdeğin altında gösterilir. Bunları "Oyun ayarları" → "Gelişmiş" → "BIOS klasörü"ne koyun (varsayılan: ayarlar klasöründeki `system`).

# Oyun başlatma
## Nasıl başlatılır
- Bir oyun seçip "▶ Başlat"a basın
- Bir oyuna çift tıklayın
- Bir oyun seçip `Enter`'a basın

## Başlatma yöntemini değiştirme
Bir oyuna sağ tıklayıp "Yerleşik çekirdekle başlat" veya "Emülatörle (exe) başlat"ı seçin. Varsayılan yöntem "Sistem ayarları"ndan belirlenir.

## Emülatör argümanları
| Yer tutucu | Şununla değiştirilir |
| `{rom}` | ROM'un tam yolu |
| `{romdir}` | ROM'un klasörü |
| `{romname}` | Uzantısız ROM dosya adı |

# Kontroller
## Varsayılan klavye düzeni
Düğme adları Super Famicom düzenine göredir (A sağda, B altta).

| Kontrol | Tuş |
| Yön tuşları | `↑` `↓` `←` `→` |
| A / B | `X` / `Z` |
| X / Y | `S` / `A` |
| L / R | `Q` / `W` |
| L2 / R2 | `E` / `R` |
| Select / Start | `Sağ Shift` / `Enter` |

## Kumandalar
Bağlı kumandalar otomatik algılanır ve standart düzen atanır: Xbox (XInput), genel USB kumandalar (DirectInput), Switch Pro Controller, DualSense ve diğerleri. 1. oyuncu ilk bağlanan kumandayı, 2. oyuncu ikincisini kullanır. Sol çubuk da yön tuşu gibi çalışır. Titreşim, destekleyen oyunlarda çalışır.

## Kısayollar
| İşlem | Tuş | Kumanda |
| Menü | `F1` | Guide düğmesi |
| Hızlı ileri (basılı tut) | `Tab` | |
| Durumu kaydet | `F2` | |
| Durumu yükle | `F4` | |
| Yuva − / + | `F6` / `F7` | |
| Ekran görüntüsü | `F12` | |
| Tam ekran aç/kapat | `F11` veya `Alt+Enter` | |
| Duraklat | `P` | |
| Sıfırla | `F10` | |
| Çık | `Esc` | |

> Tüm tuşlar "Oyun ayarları"ndan değiştirilebilir. Tam ekrandayken `Esc` önce pencereye döner.

# Oyun içi menü
`F1` (veya kumandanın Guide düğmesi) menüyü açar ve oyunu duraklatır. `↑` `↓` ile seçin, `Enter` (A) ile onaylayın, `Esc` (B) ile kapatın.

| Öğe | Açıklama |
| Oyuna dön | Menüyü kapatır |
| Durumu kaydet / yükle | Geçerli yuvada |
| Yuvayı değiştir | Yuvalar arasında geçiş yapar |
| Ekran görüntüsü | Oyun ekranını PNG olarak kaydeder |
| Tam ekran / pencere | Görünümü değiştirir |
| Oyun ayarları… | Oynarken ayarları değiştirir; hemen uygulanır |
| Sıfırla | Oyunu yeniden başlatır |
| Başlatıcıya dön | Oyunu kapatır |

# Oyun kayıtları ve durum kayıtları
## Oyun kayıtları (pil)
Oyun içinde yaptığınız kayıtlar otomatik saklanır (yaklaşık 10 saniyede bir ve çıkışta) ve bir dahaki sefere yüklenir.

## Durum kayıtları
Oyunun tam o anını kaydeder. `F2` kaydeder, `F4` yükler, `F6` / `F7` yuvayı değiştirir (varsayılan olarak 10 yuva, 0–9).

## Konumlar
| Tür | Varsayılan konum |
| Oyun kayıtları | Ayarlar klasöründe `saves\<sistem>` |
| Durum kayıtları | Ayarlar klasöründe `states\<sistem>` |
| Ekran görüntüleri | Ayarlar klasöründe `screenshots\<sistem>` |

Ayarlar klasörü exe'nin yanındaki `data`'dır; klasörün tamamını USB bellekte taşıyabilirsiniz (`%APPDATA%\JPEmuRenamer` yalnızca exe'nin klasörüne yazılamıyorsa kullanılır). Konumlar "Oyun ayarları"ndan değiştirilebilir.

# Oyun ayarları
Oyna sekmesindeki "Oyun ayarları" düğmesinden, bir oyuna sağ tıklayarak, oyun içi menüden veya uygulamanın "Ayarlar"ından açabilirsiniz.

## Tümü, sistem ve oyun
Ayarlar "Tümü → Sistem → Oyun" sırasıyla üst üste biner. Üstten düzenlemek istediğiniz düzeyi seçin. Üst düzey alttakinin değerini geçersiz kılabilir. Geçersiz kılınan öğelerde ● görünür, ↺ bunu geri alır. Örnek: her şey için pencere, yalnızca bir oyun için tam ekran.

## "Temel" sekmesi
| Bölüm | Açıklama |
| Kumanda ve klavye | 1. oyuncunun kontrolleri. Bir öğeye tıklayıp bir düğmeye veya tuşa basın (`Esc` iptal, `Delete` temizler) |
| Kayıt konumları | Oyun kayıtları, durum kayıtları ve ekran görüntüleri klasörleri |
| Görüntü | Pencere/tam ekran, pencere boyutu (1x/2x/3x/sığdır), en-boy oranı |
| Ses | Ses düzeyi, sessiz, çıkış aygıtı |

## "Gelişmiş" sekmesi
| Bölüm | Açıklama |
| Kumanda ve klavye | 1–4. oyuncular, turbo ve hızı, çubuk ölü bölgesi, kısayollar. Tuş kombinasyonları (ör. `Alt+Enter`) desteklenir |
| Kayıt konumları | Alt klasörler (yok/sisteme göre/oyuna göre), kayıtları ROM'un yanına koyma, yuva sayısı, BIOS klasörü |
| Görüntü | Tam ekran çözünürlüğü ve yenileme hızı, tam sayı ölçekleme, filtreleme, gölgelendirici (tarama çizgileri/CRT), V-Sync |
| Ses | Örnekleme hızı, çıkış modu (WASAPI paylaşımlı/özel), gecikme, ses senkron düzeltmesi |
| Çekirdek seçenekleri | Çekirdeğin kendi seçenekleri (ör. NES paleti, en-boy oranı, hız aşırtma). O çekirdekle bir kez oyun başlattıktan sonra görünür |

"Bu sekmeyi sıfırla", sekmeyi seçili düzey için varsayılan değerlere döndürür.

# Otomatik önizleme
"Oyna" sekmesinde bir oyun seçip biraz bekleyin; sağ üstte oyun görüntüsü oynamaya başlar. Yerleşik çekirdek ses ve girdi olmadan çalışır ve başlık ekranından sonraki demoyu gösterir. Sistemin yerleşik çekirdeğinin yüklü olması gerekir.

| Öğe | Varsayılan |
| Başlangıç | Seçimden 0,3 saniye sonra (hızlı kaydırırken başlamaz) |
| Hız | 4x |
| Başlangıcı atla | İlk 10 saniye (logolar vb.) |
| Yeniden başlama | 60 saniye sonra |
| Ses / girdi | Yok (oyun kayıtlarına dokunulmaz) |

Bu değerleri değiştirmek veya önizlemeyi kapatmak için "Ayarlar" → "Otomatik önizleme"yi kullanın.

## Anlık görüntüler
- Önizleme sırasında sağ alttaki "Bu ekranı kaydet", o anı anlık görüntü olarak kaydeder
- Bir oyuna sağ tıklayıp → "Bu sistem için anlık görüntüler oluştur…", sistemdeki her ROM'u arka planda başlatır ve ekranı belirli bir anda (varsayılan 30 saniye) kaydeder. Anlık görüntüsü olan oyunlar atlanabilir
- Anlık görüntüler önizleme başlamadan önce ve ekranı değişmeyen oyunlarda gösterilir
- Ayarlar klasöründe `snapshots\<sistem>\<başlık>.png` olarak kaydedilir

> Önizleme veya anlık görüntü oluşturma sırasında bir oyun başlatmak önce önizlemeyi durdurur.

# Çekirdekleri indirme ve güncelleme
"Ayarlar" → "Yerleşik çekirdekleri indir / güncelle"yi açın.

- Liste her çekirdek için sistemleri, boyutu ve durumu (yüklü değil / güncel / güncelleme var) gösterir
- "Sistemlerimin çekirdekleri", "Yüklü olmayanlar", "Güncellemesi olanlar" gibi düğmelerle çekirdekleri seçip "İndir"e basın
- "Tümünü güncelle", güncellemesi olan tüm çekirdekleri günceller

> Çekirdekler resmî libretro sitesinden indirilir. Çalışan bir oyunun kullandığı çekirdek güncellenemez.

# Sorun giderme
| Sorun | Kontrol edilecekler |
| Başlamıyor / "oyunu yükleyemedi" | Sistem BIOS gerektiriyorsa BIOS klasörünü kontrol edin. Başka bir çekirdek işe yarayabilir |
| "OpenGL kullanılamıyor" | Çekirdek OpenGL kullanıyor (ör. N64). Ekran kartı sürücüsünü güncelleyin veya başka çekirdek seçin |
| Ses cızırdıyor veya gecikiyor | "Oyun ayarları" → "Gelişmiş" → "Ses"te gecikmeyi artırın veya ses senkron düzeltmesini açın |
| Görüntü takılıyor | V-Sync'i kapatın veya diğer uygulamaları kapatın. N64 gibi ağır çekirdekler hızlı bir bilgisayar ister |
| Kumanda tepki vermiyor | Algılanan kumandalar "Oyun ayarları"nın üstünde listelenir. Yoksa yeniden bağlayın. İlk kumanda 1. oyuncudur |
| Çözünürlük geri dönmüyor | Oyundan çıkın veya pencereye dönmek için `Esc`'ye basın |
| İlk basış algılanmıyor | Bazı oyunlar açıldıktan sonra birkaç saniye girdi almaz. Biraz bekleyin |
| Otomatik önizleme yok | Sistemin yerleşik çekirdeğinin yüklü ve "Ayarlar"da otomatik önizlemenin açık olduğunu kontrol edin. Uygulama arka plandayken duraklar |
""";

    public const string Rename = """
# Yeniden adlandırma nasıl çalışır
Uygulama her ROM'un CRC32'sini (içeriğin parmak izi) hesaplar ve DAT dosyasıyla (başlık ve CRC32 listesi) karşılaştırır. Yalnızca eşleşen dosyalar, şu anki adları ne olursa olsun, DAT'taki doğru adla yeniden adlandırılır.

> Yeniden adlandırmadan önce listeyi her zaman kontrol edersiniz; "Geri al" da sonradan tüm işlemi geri alır.

# Hazırlık
## DAT dosyası edinme
Her sistem için OfflineList biçiminde (XML) bir DAT kullanın. Eski Shift-JIS DAT'ları da desteklenir.

## Sistemi ayarlama
Soldan sistemi seçin ve "Sistem ayarları"nda şunları ayarlayın.

| Öğe | Açıklama |
| DAT dosyası | DAT dosyası |
| ROM klasörleri | ROM'ların bulunduğu klasörler (`;` ile ayırın) |
| Uzantılar | Okunacak uzantılar (ör. `.nes;.zip`) |
| Başlık işleme | iNES/FDS başlıklarının CRC32'ye dahil edilip edilmeyeceği (genellikle "Otomatik") |

Okuma ve karşılaştırma otomatik başlar. İkinci seferden itibaren değişmeyen dosyalar atlandığı için hızlıdır.

# "Yeniden adlandır" sekmesi
Tabloyu görmek için üstteki "Yeniden adlandır" sekmesine geçin.

| Sütun | Açıklama |
| ✓ | Yeniden adlandırılıp adlandırılmayacağı (yalnızca "Adlandırılacak" satırları seçilebilir) |
| Durum | Karşılaştırma sonucu (aşağıya bakın) |
| Geçerli dosya adı | Şimdiki ad |
| → DAT'taki ad | Yeniden adlandırma sonrası ad |
| CRC32 / boyut / biçim | ROM bilgisi |

## Durumlar
| Durum | Anlamı | Yeniden adlandırılır |
| Doğru | CRC32 eşleşiyor, ad zaten doğru | Gerekmez |
| Adlandırılacak | CRC32 eşleşiyor ama ad farklı | Evet |
| Yinelenen | Aynı başlık için zaten bir dosya var | Hayır |
| DAT'ta yok | Bu CRC32 DAT'ta yok | Hayır |
| Eksik | DAT'ta var ama dosya sizde yok | ― |

# Adım adım yeniden adlandırma
- "Yeniden adlandır" sekmesinde "Adlandırılacak" satırlarını kontrol edin (başta hepsi işaretlidir)
- Yeniden adlandırmak istemediklerinizin işaretini kaldırın. Başlıktaki kutu hepsini değiştirir
- Sağda seçili satırın "Önce" ve "Sonra" hâli görünür
- Alttaki "Yeniden adlandır"a basın ve "Evet" ile onaylayın
- Sayı gösterilir ve liste kendiliğinden yenilenir

> Yalnızca CRC32'si DAT ile eşleşen dosyalar değiştirilir. Yeni addaki dosya zaten varsa o dosya atlanır.

## ZIP içindeki ROM adları
"Ayarlar" → "ZIP içindeki ROM'u da yeniden adlandır" açıksa, her ZIP'in içindeki ROM da ZIP ile birlikte yeniden adlandırılır.

## Windows'un izin vermediği karakterler
`\` `/` `:` `*` `?` `"` `<` `>` `|` tam genişlikli karşılıklarıyla değiştirilir.

# Geri alma
"Yeniden adlandır" sekmesinin altındaki "Geri al", son işlemi tümüyle geri alır. Bir öncekini geri almak için yeniden basın (son 50 işleme kadar).

> "Geri al" sistemden bağımsız olarak her zaman en yeni işlemi geri alır. Onay penceresi tarihini ve dosya sayısını gösterir.

# Kullanışlı özellikler
| Özellik | Nasıl kullanılır |
| Sahip olunan / Tümü / Eksik | Listeyi filtreler. "Eksik", henüz sahip olmadığınız oyunları gösterir |
| Arama | Başlığa veya dosya adına göre filtrelemek için arama kutusuna yazın |
| Yeniden karşılaştır | ROM ekledikten veya kaldırdıktan sonra kullanın |
| Uzantıları gizle | "Ayarlar" → "Oyun listesinde dosya uzantılarını (.zip vb.) gizle" |
| Alt klasörleri oku | "Ayarlar" → "Alt klasörleri de oku" |
| Yalnızca DAT adlarını göster | Dosya adını değiştirmenin emülatörleri bozduğu atari salonu oyunları için. "Sistem ayarları"nda açarak dosya adlarını koruyup listede DAT adlarını gösterin |

# Sorun giderme
| Sorun | Kontrol edilecekler |
| Hiçbiri eşleşmiyor | DAT'ın doğru sisteme ait olduğunu kontrol edin. "Başlık işleme"yi "Başlıkla" veya "Başlıksız" yapmak yardımcı olabilir |
| "DAT dosyası okunamadı" | OfflineList DAT'ı (XML) olduğundan emin olun |
| "Yinelenen" | Aynı başlıktan iki veya daha fazla ROM'unuz var. Birini başka bir klasöre taşıyın |
| "(zaten var)" ile atlandı | Yeni addaki dosya zaten var. Hangisini tutacağınıza karar verip elle düzenleyin |
| ZIP içindeki ad değişmedi | ZIP'in kendi adı zaten doğruysa içteki ad değiştirilmez |
""";
}
