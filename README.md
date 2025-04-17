# MintBlockChain Bot

**MintBlockChain Bot**, blockchain tabanlı tarayıcı oyunu **MintForest** ile etkileşim kurmak için geliştirdiğim bir konsol uygulamasıdır. Bu proje, tamamen kişisel bir tutkuyla yapılan bir hobi çalışmasıdır. MintForest’te birden fazla hesabı yöneterek otomatik giriş, günlük ödülleri toplama, enerji yönetimi ve diğer oyunculardan puan çalma (steal) gibi görevleri gerçekleştiriyor.

## Proje Hakkında

JSON tabanlı bir ayar dosyası üzerinden hesap bilgilerini okuyarak her hesap için ayrı görevler yürütür. Discord webhook’ları ile başarı ve hata bildirimleri gönderir, lider tablosunu tarayarak stratejik avantaj sağlar. Oyunun dinamiklerine özel optimizasyonlar ile güncel tutmaya çalıştım. Elimden geldiğince projeyi olabildiğince sağlam ve verimli hale getirdim. Kişisel bir proje olsa da, blockchain ve otomasyonla ilgilenenler için örnek olabilir.

## Özellikler

- **Çoklu Hesap Desteği:** Birden fazla MintForest hesabını aynı anda yönetebilir.
- **Otomatik Görevler:** Oyuna giriş, günlük ödülleri toplama, enerji yönetimi ve puan toplama işlemlerini otomatikleştirir.
- **Stratejik Puan Çalma (Steal):** Lider tablosunu tarayarak günlük ödüllerini henüz toplamamış oyuncuları tespit eder ve optimize edilmiş zamanlamayla (14:35 tarama, 15:00 çalma) puan çalma işlemlerini gerçekleştirir.
- **Verimli Lider Tablosu Tarama:** Tüm hesaplar arasında iş birliği yaparak lider tablosunu multithread bir şekilde tarar, böylece tarama işlemi daha hızlı ve kaynak dostu olur.
- **Brute Force Modu:** Gerektiğinde rastgele kullanıcılar üzerinden ek çalma denemeleri yapar (opsiyonel).
- **Discord Entegrasyonu:** İşlem ve hata bildirimlerini Discord webhook’ları üzerinden gönderir.
- **Proxy Desteği:** Her hesap için ayrı proxy ayarları kullanılabilir.
- **JSON Konfigürasyonu:** Ayarlar, kolayca düzenlenebilen bir JSON dosyasında saklanır.
- **Lider Tablosu Tarama:** Oyunun lider tablosunu tarar ve stratejik çalma fırsatlarını belirler.
- **Blockchain Entegrasyonu:** MintForest’ün blockchain kontratlarıyla (MintChain ağı) etkileşim kurar, ödülleri toplamak için kontrat işlemlerini simüle eder ve gerçekleştirir.

## Teknik Detaylar

- **Dil ve Teknoloji:** C# ile yazılmış bir konsol uygulaması, Nethereum kütüphanesi ile blockchain işlemleri gerçekleştiriyor.
- **Proje Yapısı:**
  - **`MintBlockChainBotConsoleUI`:** Botun ana mantığını ve konsol arayüzünü içerir. `Bot.cs`, oyunun otomatikleştirme mantığını (günlük toplama, puan çalma, enerji yönetimi) yöneten ana sınıftır. `Program.cs`, çoklu hesapları paralel olarak başlatır.
  - **`MintBlockChainWrapper`:** MintForest oyunuyla etkileşim kuran özel bir kütüphane. `MintForest.cs`, oyunun HTTP endpoint’leri ve blockchain kontratlarıyla iletişim kurar. `Authorization.cs`, kimlik doğrulama için bearer token üretimini ve önbelleklemesini yönetir.
  - **Yardımcı Sınıflar:** `HttpHelper` (HTTP istek yönetimi), `JsonFileManager` (JSON ayar dosyası okuma), `CacheManager` (token önbellekleme), `DiscordWebHookManager` (Discord bildirimleri) gibi modüler sınıflar, projenin yeniden kullanılabilirliğini artırır.
- **Blockchain İşlemleri:** MintChain ağı (`rpc.mintchain.io`) üzerinden kontratlarla (`0x12906892...`) etkileşim kurar. `SimulateContractAction` ve `PerformContractAction` metodları, işlemleri önce simüle eder, ardından yürütür.
- **Kimlik Doğrulama:** Özel anahtarlarla nonce oluşturur, mesaj imzalar ve MintForest’ün login endpoint’inden bearer token alır. Token’lar önbelleğe alınarak performans optimize edilir.
- **Eşzamanlı İşlemler:** Her hesap için ayrı bir görev (task) oluşturarak hesapları eşzamanlı yönetir.
- **Hata Yönetimi:** Giriş hataları, işlem başarısızlıkları ve beklenmedik durumlar konsola yazılır, Discord üzerinden raporlanır ve “Unauthorized” gibi durumlarda otomatik yeniden giriş denenir.


## Yasal Sorumluluk Reddi ve Uyarı

Bu proje, **kişisel bir hobi** çalışması olup yalnızca eğitim ve öğrenme amaçlı geliştirilmiştir. MintBlockChain Bot, **MintForest** oyununun kullanım koşullarına uygun olmayan şekillerde kullanılmamalıdır. Botun kullanımı tamamen kullanıcının sorumluluğundadır ve herhangi bir yasal, finansal veya başka türlü sorumluluk geliştiriciye ait değildir. Kullanmadan önce MintForest’ün kullanım koşullarını ve ilgili tüm yasal düzenlemeleri dikkatlice inceleyin. Bu proje genel dağıtım için optimize edilmemiştir ve teknik destek veya düzenli güncellemeler sunulmamaktadır. Blockchain tabanlı oyunlar ve otomasyonla ilgilenenler için yalnızca bir fikir kıvılcımı olmayı amaçlar!

## İletişim

Sorularınız veya önerileriniz için GitHub üzerinden bana ulaşabilirsiniz. Blockchain tabanlı oyunlar, otomasyon veya benzer projelerle ilgileniyorsanız, bu botun kodu size ilham verebilir!
