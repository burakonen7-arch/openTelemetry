# Product API — .NET 10 ve OpenTelemetry laboratuvarı

Controller tabanlı, EF Core ve PostgreSQL kullanan küçük bir ürün kataloğu.

## Mimari

- **Domain/Entities**: Product entity'si; dış katmanlara bağımlılığı yok.
- **Application/Products**: servis, DTO, repository portu ve iş akışı telemetrisi. EF Core'a bağımlı değil.
- **Infrastructure/Persistence**: DbContext, PostgreSQL repository ve migration'lar.
- **Api/Controllers**: HTTP sözleşmesi, doğrulama, dependency injection ve OpenTelemetry yapılandırması.

Bağımlılıklar içeriye doğrudur: API → Infrastructure → Application → Domain. Servis Application'daki IProductRepository portuyla çalışır. API composition root'tur. Küçük CRUD için ayrıca genel repository veya MediatR eklenmedi.

## Yerel çalıştırma

Docker ve Docker Compose gereklidir:

```sh
docker compose up --build -d
python3 scripts/smoke.py
```

Compose PostgreSQL'i bekler, ayrı migration servisiyle şemayı oluşturur, sonra API'yi başlatır. `postgres-data` volume'u veriyi korur. Örnek parola yalnızca yerel geliştirme içindir; gerçek ortamlarda secret yönetimi kullanın. API kimlik doğrulaması içermeyen bir eğitim uygulamasıdır.

API portu 8080, Aspire Dashboard portu 18888, OTLP gRPC portu 4317'dir. Portlar sadece loopback arayüzüne bağlanır. Dashboard anonim erişimi yalnızca bu yerel laboratuvar içindir. OpenAPI belgesi Development ortamında `/openapi/v1.json` adresindedir.

.NET SDK ile doğrudan çalıştırmak için PostgreSQL ve OTLP alıcısını başlatıp:

```sh
export ConnectionStrings__Products='Host=127.0.0.1;Database=products;Username=products;Password=local-development-only'
export OTEL_EXPORTER_OTLP_ENDPOINT=http://127.0.0.1:4317
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://127.0.0.1:8080
dotnet run --project src/Product.Api -- --migrate
dotnet run --project src/Product.Api
```

Migration uygulaması ayrı bir adımdır; her API başlangıcında otomatik çalışmaz.

## İstekler

```sh
curl -i -X POST http://127.0.0.1:8080/api/products \
  -H 'Content-Type: application/json' \
  -d '{"name":"Keyboard","price":99.95,"stock":5}'
curl 'http://127.0.0.1:8080/api/products?page=1&pageSize=20'
```

`GET /api/products/{id}`, `PUT /api/products/{id}`, `DELETE /api/products/{id}` diğer uçlardır. PUT, POST ile aynı gövdeyi kullanır. Başarılı oluşturma 201 ve Location, silme 204; eksik ürün 404; geçersiz giriş 400 döndürür. Sayfa boyutu en fazla 100'dür. Para alanı PostgreSQL'de numeric(12,2), fiyat pozitif, stok sıfır veya pozitif olmalıdır. Hatalar ProblemDetails biçimindedir. İptal token'ı veritabanına kadar iletilir.

## OpenTelemetry öğrenme adımları

1. POST isteği gönderin. Dashboard Traces ekranında `product-api` servisini bulun: HTTP server span'i → `products.create` özel span'i → Npgsql SQL span'leri. Span süresiyle SQL süresini karşılaştırın.
2. `product.persisted` event'ini ve `product.id` etiketini inceleyin. GET ile `product.found=false` durumunu gözleyin. 404 beklenen bir sonuçtur; otomatik olarak uygulama hatası sayılmaz.
3. Logs ekranında `Product ... created` kaydını aynı TraceId/SpanId ile ilişkilendirin. EF sorgu loglarını izlerle karşılaştırın. SQL parametre değerleri için sensitive-data logging açılmamıştır.
4. Metrics ekranında `products.created` sayacını izleyin. Birkaç ürün oluşturun; artışı görün. ASP.NET Core istek süresi ve .NET runtime metriklerini inceleyin. Metriklerin varsayılan gönderim aralığı yaklaşık 60 saniyedir.
5. Negatif fiyat veya boş ad gönderin: 400 döner; servis çağrılmadığı için `products.create` span'i oluşmaz. HTTP span'i yine vardır.
6. PostgreSQL'i durdurup bir GET gönderin: 500 ProblemDetails, hata logu ve başarısız SQL span'ini karşılaştırın; ardından PostgreSQL'i yeniden başlatın.

Özel ActivitySource/Meter adı `Product.Catalog`'dur. `AddSource`/`AddMeter` kaldırılırsa özel telemetri dışarı aktarılmaz. `OTEL_EXPORTER_OTLP_ENDPOINT` ve `OTEL_EXPORTER_OTLP_PROTOCOL` ile başka bir collector kullanabilirsiniz. Varsayılan protokol gRPC; HTTP/protobuf için protokolü `http/protobuf` ve endpoint'i alıcının HTTP portuna ayarlayın. W3C traceparent başlığı gelen iz bağlamını taşır. Ürün ID'si sadece span etiketidir; yüksek kardinaliteli ürün ID'leri metrik etiketi yapılmamıştır.

Bu ilk sürüm dağıtık birden fazla servis yerine HTTP → servis → PostgreSQL iz zincirine odaklanır. Dashboard verileri kalıcı bir üretim gözlemlenebilirlik deposu değildir.

## Doğrulama

```sh
dotnet build Product.slnx
python3 scripts/smoke.py
```

Smoke testi gerçek çalışan API/PostgreSQL üzerinde CRUD, pagination ve hata durumlarını denetler; kendi oluşturduğu ürünü siler. `scripts/smoke.py` varsayılan olarak 8080 portunu, isteğe bağlı ilk argümanla başka API adresini kullanır.

Bu bulut ortamında .NET 10.0.402 ile derleme ve PostgreSQL 17 üzerinde migration/CRUD doğrulandı. OTLP alıcısına iz, metrik ve log gönderimi ayrıca kontrol edildi. Compose PostgreSQL 18 hedefler; Docker imaj indirmeleri ortamda 403 döndüğünden Compose ve Dashboard arayüzü burada doğrulanamadı.
