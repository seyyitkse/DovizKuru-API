# 💱 DovizKuru API

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API-5C2D91?style=for-the-badge&logo=dotnet)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=for-the-badge&logo=swagger)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)

</div>

Basit, hızlı ve frontend dostu bir döviz kuru API’si.
Canlı kur verisini alır, kısa süreli cache uygular, dönüşüm yapar ve desteklenen para birimlerini döner.

---

## ✨ Özellikler

- 🌍 Çoklu para birimi desteği (USD, EUR, TRY, GBP vb.)
- 🔁 Kur dönüşümü (`from` → `to`)
- ⚡ 5 dakikalık bellek içi cache ile hızlı yanıt
- 🛡️ Harici servis erişilemezse fallback kurlar ile devam
- 📘 Swagger / OpenAPI desteği
- 🔐 Frontend için CORS ayarı (`localhost:5173`, `localhost:3000`)
- 🐳 Docker ile çalıştırma desteği

---

## 🧰 Teknolojiler

- ASP.NET Core Web API (.NET 8)
- HttpClient
- System.Text.Json
- Swagger / Swashbuckle

---

## 🚀 Hızlı Başlangıç

### 1) Projeyi çalıştır

```bash
dotnet restore
dotnet run --project DovizKuru-API/DovizKuru-API.csproj
```

Varsayılan geliştirme adresleri:

- https://localhost:7035
- http://localhost:5094

Swagger UI:

- https://localhost:7035/swagger
- http://localhost:5094/swagger

### 2) Docker ile çalıştır

```bash
docker build -t dovizkuru-api -f DovizKuru-API/Dockerfile .
docker run -p 8080:8080 -e ASPNETCORE_URLS=http://+:8080 dovizkuru-api
```

Docker sonrası API:

- http://localhost:8080

---

## 📌 API Detayları

Base route:

```text
/api/ExchangeRate
```

> Not: Route, controller adı üzerinden üretildiği için `ExchangeRate` kullanılır.

### 1) Tüm kurları getir

**Endpoint**

```http
GET /api/ExchangeRate/rates?baseCurrency=TRY
```

**Query parametreleri**

- `baseCurrency` (opsiyonel, varsayılan: `TRY`)

**Örnek istek**

```bash
curl "https://localhost:7035/api/ExchangeRate/rates?baseCurrency=USD"
```

**Örnek başarılı yanıt (200)**

```json
{
  "base": "USD",
  "date": "2026-01-01T12:34:56.000Z",
  "rates": {
    "TRY": 32.5,
    "EUR": 0.92,
    "GBP": 0.79,
    "JPY": 154
  },
  "currencies": [
    {
      "code": "USD",
      "name": "US Dollar",
      "nameTR": "Amerikan Doları",
      "symbol": "$",
      "flag": "🇺🇸"
    }
  ]
}
```

**Hata yanıtı (500)**

```text
Kurlar alınırken bir hata oluştu
```

---

### 2) Döviz dönüştür (POST)

**Endpoint**

```http
POST /api/ExchangeRate/convert
Content-Type: application/json
```

**Request body**

```json
{
  "amount": 100,
  "fromCurrency": "USD",
  "toCurrency": "TRY"
}
```

**Örnek başarılı yanıt (200)**

```json
{
  "amount": 100,
  "fromCurrency": "USD",
  "toCurrency": "TRY",
  "result": 3250.0,
  "rate": 32.5,
  "timestamp": "2026-01-01T12:35:30.000Z"
}
```

**Olası hatalar**

- `400 Bad Request`: `Miktar 0'dan büyük olmalıdır`
- `400 Bad Request`: `Geçersiz para birimi`
- `500 Internal Server Error`: `Dönüşüm yapılırken bir hata oluştu`

---

### 3) Döviz dönüştür (GET)

**Endpoint**

```http
GET /api/ExchangeRate/convert?amount=100&from=USD&to=TRY
```

Yanıt formatı, `POST /convert` ile aynıdır.

---

### 4) Desteklenen para birimleri

**Endpoint**

```http
GET /api/ExchangeRate/currencies
```

**Örnek başarılı yanıt (200)**

```json
[
  {
    "code": "TRY",
    "name": "Turkish Lira",
    "nameTR": "Türk Lirası",
    "symbol": "₺",
    "flag": "🇹🇷"
  },
  {
    "code": "USD",
    "name": "US Dollar",
    "nameTR": "Amerikan Doları",
    "symbol": "$",
    "flag": "🇺🇸"
  }
]
```

---

## 🧠 Çalışma Mantığı

1. API, canlı kuru dış servisten çeker.
2. Sonuçları 5 dakika cache’ler.
3. Dönüşümde formül: `rate = toRate / fromRate`, `result = amount * rate`.
4. Harici servis hatasında:
   - Cache varsa onu kullanır.
   - Cache yoksa fallback kurları kullanır.

---

## 🌐 CORS

`Program.cs` içinde izin verilen origin’ler:

- http://localhost:5173
- http://localhost:3000

Frontend farklı port/domain’deyse CORS politikasını güncelleyin.

---

## 🧪 Test Senaryoları

- `GET /rates?baseCurrency=TRY` ile tüm kurları TRY bazlı çek
- `POST /convert` ile `USD -> EUR` dönüşümü yap
- `GET /convert` ile query tabanlı dönüşüm dene
- `GET /currencies` ile dropdown için para birimleri al

---

## 🗂️ Proje Yapısı

```text
DovizKuru-API/
├─ Controllers/
│  ├─ ExchangeRateController.cs
│  └─ WeatherForecastController.cs
├─ Models/
│  └─ CurrencyRate.cs
├─ Services/
│  ├─ IExchangeRateService.cs
│  └─ ExchangeRateService.cs
├─ Properties/
│  └─ launchSettings.json
├─ Program.cs
└─ Dockerfile
```

> `WeatherForecast` dosyaları .NET şablonundan gelir; döviz API akışının parçası değildir.

---

## 🔗 İlgili Proje

- Frontend: https://github.com/Nursenacmk12/DovizKuru-Frontend

---

## 🤝 Katkı

Geliştirme önerileri ve katkılar için pull request açabilirsiniz.

---

<div align="center">

Made with ❤️ for currency apps

</div>
